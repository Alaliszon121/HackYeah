using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TwineImporter
{
    private DialogueGraphView _graphView;
    private Dictionary<string, DialogueNode> _createdNodes = new();
    private Vector2 _startPos = new(100, 100);

    private static readonly HashSet<string> ReservedHeaders = new()
    {
        "StoryTitle",
        "StorySubtitle",
        "StoryAuthor",
        "StoryMenu",
        "StorySettings",
        "StoryIncludes",
        "StoryData",
        "UserStylesheet",
        "UserScript",
        "stylesheet",
        "script"
    };

    public TwineImporter(DialogueGraphView graphView)
    {
        _graphView = graphView;
    }

    public static void ImportToGraph(string path, DialogueGraphView graphView)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"TwineImporter: File not found: {path}");
            return;
        }

        string twineText = File.ReadAllText(path);
        TwineImporter importer = new(graphView);
        importer.Import(twineText);
    }

    public void Import(string twineText)
    {
        twineText = Regex.Replace(twineText, @"<!--[\s\S]*?-->", "", RegexOptions.Multiline);

        List<ParsedPassage> passages = ParsePassages(twineText);

        _createdNodes.Clear();
        _startPos = new Vector2(100, 100);

        foreach (ParsedPassage passage in passages)
            BuildPassageGraph(passage);

        ConnectChoices(passages);
    }

    private List<ParsedPassage> ParsePassages(string text)
    {
        List<ParsedPassage> result = new();

        string[] parts = Regex.Split(text, @"(?m)^\s*::\s*");

        foreach (string rawPart in parts)
        {
            string part = rawPart.Trim();
            if (string.IsNullOrEmpty(part)) continue;

            int firstNewline = part.IndexOf('\n');
            string headerRaw;
            string bodyRaw;
            if (firstNewline >= 0)
            {
                headerRaw = part.Substring(0, firstNewline).Trim();
                bodyRaw = part[(firstNewline + 1)..];
            }
            else
            {
                headerRaw = part.Trim();
                bodyRaw = "";
            }

            headerRaw = Regex.Replace(headerRaw, @"\s*\{[\s\S]*\}\s*$", "").Trim();

            if (headerRaw.StartsWith("\"") && headerRaw.EndsWith("\"") && headerRaw.Length >= 2) headerRaw = headerRaw[1..^1];
            if (ReservedHeaders.Contains(headerRaw)) continue;
            if (string.IsNullOrEmpty(headerRaw)) continue;

            List<string> lines = new();

            if (!string.IsNullOrEmpty(bodyRaw))
            {
                string[] rawLines = bodyRaw.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                foreach (string rl in rawLines)
                {
                    string ln = rl.Trim();

                    if (string.IsNullOrEmpty(ln)) continue;

                    ln = Regex.Replace(ln, @"\{[\s\S]*?\}", "").Trim();

                    if (!string.IsNullOrEmpty(ln)) lines.Add(ln);
                }
            }

            ParsedPassage passage = new()
            {
                Title = headerRaw,
                Lines = lines
            };

            result.Add(passage);
        }

        return result;
    }

    private void BuildPassageGraph(ParsedPassage passage)
    {
        DialogueNode lastNode = null;
        Vector2 pos = _startPos;

        if (passage.Lines.Count == 0)
        {
            DialogueNode only = _graphView.CreateDialogueNode(pos);
            only.LoadData("", passage.Title, "", passage.Title);
            _createdNodes[only.ID] = only;
            _startPos += new Vector2(0, 250);
            return;
        }

        for (int i = 0; i < passage.Lines.Count; i++)
        {
            string line = passage.Lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            bool hasChoice = Regex.IsMatch(line, @"\[\[.*?\]\]");

            if (hasChoice)
            {
                BranchNode branch = _graphView.CreateBranchNode(pos);

                string branchText = lastNode != null ? lastNode.DialogueText : passage.Title;
                string branchId = passage.Title + "_branch";

                branch.LoadData("", branchText, "", branchId);
                branch.ClearChoices();

                foreach (string choiceLine in passage.Lines)
                {
                    foreach (Match m in Regex.Matches(choiceLine, @"\[\[(.*?)\]\]"))
                    {
                        string inner = m.Groups[1].Value.Trim();

                        string choiceText = inner, nextId = inner;

                        int arrowPos = IndexOfUnnested(inner, "->");
                        if (arrowPos >= 0)
                        {
                            choiceText = inner.Substring(0, arrowPos).Trim();
                            nextId = inner[(arrowPos + 2)..].Trim();
                        }
                        else
                        {
                            int arrowPos2 = IndexOfUnnested(inner, "<-");
                            if (arrowPos2 >= 0)
                            {
                                nextId = inner.Substring(0, arrowPos2).Trim();
                                choiceText = inner[(arrowPos2 + 2)..].Trim();
                            }
                        }

                        choiceText = StripSurroundingQuotes(choiceText);
                        nextId = StripSurroundingQuotes(nextId);

                        branch.AddChoice(choiceText);

                        passage.Choices.Add(new ParsedChoice
                        {
                            ChoiceText = choiceText,
                            NextID = nextId,
                            Node = branch,
                            PortIndex = branch.Choices.Count - 1
                        });
                    }
                }

                _createdNodes[branch.ID] = branch;

                if (lastNode != null)
                {
                    Port outPort = lastNode.GetOutputPorts().Count > 0 ? lastNode.GetOutputPorts()[0] : null;
                    Port inPort = branch.GetInputPorts().Count > 0 ? branch.GetInputPorts()[0] : null;
                    if (outPort != null && inPort != null)
                        _graphView.AddElement(outPort.ConnectTo(inPort));
                }

                break;
            }
            else
            {
                string[] parts = line.Split(new[] { ':' }, 2);
                string speaker = parts.Length > 1 ? parts[0].Trim() : "";
                string dialogue = parts.Length > 1 ? parts[1].Trim() : line.Trim();

                dialogue = Regex.Replace(dialogue, @"\{[\s\S]*?\}", "").Trim();

                DialogueNode node = _graphView.CreateDialogueNode(pos);

                if (i == 0)
                {
                    node.LoadData("", passage.Title, "", passage.Title);
                }
                else
                {
                    string id = passage.Title + "_line" + i;
                    node.LoadData(speaker, dialogue, "", id);
                }

                _createdNodes[node.ID] = node;

                if (lastNode != null)
                {
                    Port outPort = lastNode.GetOutputPorts().Count > 0 ? lastNode.GetOutputPorts()[0] : null;
                    Port inPort = node.GetInputPorts().Count > 0 ? node.GetInputPorts()[0] : null;

                    if (outPort != null && inPort != null) _graphView.AddElement(outPort.ConnectTo(inPort));
                }

                lastNode = node;
                pos += new Vector2(350, 0);
            }
        }

        _startPos += new Vector2(0, 250);
    }

    private void ConnectChoices(List<ParsedPassage> passages)
    {
        foreach (ParsedPassage passage in passages)
        {
            foreach (ParsedChoice parsedChoice in passage.Choices)
            {
                if (string.IsNullOrEmpty(parsedChoice.NextID)) continue;

                if (_createdNodes.TryGetValue(parsedChoice.NextID, out DialogueNode target))
                {
                    List<Port> outPorts = parsedChoice.Node.GetOutputPorts();
                    if (parsedChoice.PortIndex >= 0 && parsedChoice.PortIndex < outPorts.Count)
                    {
                        Port outPort = outPorts[parsedChoice.PortIndex];
                        Port inPort = target.GetInputPorts().Count > 0 ? target.GetInputPorts()[0] : null;

                        if (outPort != null && inPort != null) _graphView.AddElement(outPort.ConnectTo(inPort));
                    }
                }
            }
        }
    }

    private static string StripSurroundingQuotes(string s)
    {
        s = s.Trim();
        if (s.Length >= 2 && ((s.StartsWith("\"") && s.EndsWith("\"")) || (s.StartsWith("'") && s.EndsWith("'")))) return s[1..^1].Trim();

        return s;
    }

    private static int IndexOfUnnested(string text, string token)
    {
        int depth = 0;

        for (int i = 0; i <= text.Length - token.Length; i++)
        {
            char c = text[i];
            if (c == '(' || c == '[' || c == '{') depth++;
            else if (c == ')' || c == ']' || c == '}') depth = Math.Max(0, depth - 1);

            if (depth == 0 && text.Substring(i, token.Length) == token) return i;
        }

        return -1;
    }

    private class ParsedPassage
    {
        public string Title;
        public List<string> Lines = new();
        public List<ParsedChoice> Choices = new();
    }

    private class ParsedChoice
    {
        public string ChoiceText;
        public string NextID;
        public BranchNode Node;
        public int PortIndex;
    }
}
