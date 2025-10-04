using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class DialogueJsonUtility
{
    public static void ExportToJson(DialogueContainer container, string path)
    {
        DialogueJsonRoot root = new();

        Dictionary<string, List<DialogueLinkData>> linksBySource = container.links.GroupBy(l => l.SourceNodeID)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (DialogueNodeData node in container.nodes)
        {
            DialogueJsonDialog dialog = new()
            {
                id = node.ID,
                speaker = node.Speaker,
                text = node.DialogueText,
                callback = node.FunctionName
            };

            if (node.IsBranch)
            {
                List<DialogueLinkData> nodeLinks = linksBySource.ContainsKey(node.ID) ? linksBySource[node.ID] : new List<DialogueLinkData>();

                for (int i = 0; i < node.Choices.Count; i++)
                {
                    DialogueJsonChoice choice = new()
                    {
                        text = node.Choices[i],
                        callback = ""
                    };

                    DialogueLinkData link = nodeLinks.FirstOrDefault(l => l.SourcePortIndex == i);
                    if (link != null) choice.nextId = link.TargetNodeID;

                    dialog.choices.Add(choice);
                }
            }
            else
            {
                if (linksBySource.ContainsKey(node.ID))
                {
                    DialogueLinkData firstLink = linksBySource[node.ID].FirstOrDefault();
                    if (firstLink != null) dialog.nextId = firstLink.TargetNodeID;
                }
            }
            root.dialogs.Add(dialog);
        }

        string json = JsonUtility.ToJson(root, true);

        File.WriteAllText(path, json);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Export Complete", $"Exported JSON to:\n{path}", "OK");
    }

    public static DialogueContainer ImportFromJson(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("JSON file not found: " + path);
            return null;
        }

        string json = File.ReadAllText(path);
        DialogueJsonRoot root = JsonUtility.FromJson<DialogueJsonRoot>(json);

        DialogueContainer container = ScriptableObject.CreateInstance<DialogueContainer>();

        foreach (DialogueJsonDialog dialog in root.dialogs)
        {
            DialogueNodeData nodeData = new()
            {
                ID = dialog.id,
                Speaker = dialog.speaker,
                DialogueText = dialog.text,
                FunctionName = dialog.callback,
                IsBranch = dialog.choices != null && dialog.choices.Count > 0,
                Choices = dialog.choices?.Select(c => c.text).ToList() ?? new List<string>()
            };
            container.nodes.Add(nodeData);
        }

        foreach (DialogueJsonDialog dialog in root.dialogs)
        {
            if (!string.IsNullOrEmpty(dialog.nextId))
            {
                container.links.Add(new DialogueLinkData
                {
                    SourceNodeID = dialog.id,
                    TargetNodeID = dialog.nextId,
                    SourcePortIndex = 0,
                    TargetPortIndex = 0
                });
            }

            if (dialog.choices != null)
            {
                for (int i = 0; i < dialog.choices.Count; i++)
                {
                    DialogueJsonChoice choice = dialog.choices[i];
                    if (!string.IsNullOrEmpty(choice.nextId))
                    {
                        container.links.Add(new DialogueLinkData
                        {
                            SourceNodeID = dialog.id,
                            TargetNodeID = choice.nextId,
                            SourcePortIndex = i,
                            TargetPortIndex = 0
                        });
                    }
                }
            }
        }
        AutoLayoutNodes(container);

        return container;
    }

    private static void AutoLayoutNodes(DialogueContainer container)
    {
        Dictionary<string, List<string>> childrenMap = container.links.GroupBy(l => l.SourceNodeID).ToDictionary(g => g.Key, g => g.Select(l => l.TargetNodeID).ToList());

        HashSet<string> allTargets = container.links.Select(l => l.TargetNodeID).ToHashSet();
        List<DialogueNodeData> roots = container.nodes.Where(n => !allTargets.Contains(n.ID)).ToList();

        if (roots.Count == 0) roots = container.nodes.Take(1).ToList();

        HashSet<string> visited = new();

        float startX = 100f, startY = 100f;

        foreach (DialogueNodeData root in roots)
        {
            LayoutNodeRecursive(root, container, childrenMap, visited, startX, startY, 0);
            startY += 400f;
        }
    }

    private static float LayoutNodeRecursive(DialogueNodeData node, DialogueContainer container, Dictionary<string, List<string>> childrenMap, HashSet<string> visited, float x, float y, int depth)
    {
        if (visited.Contains(node.ID)) return y;

        visited.Add(node.ID);

        node.Position = new Vector2(x, y);

        if (!childrenMap.ContainsKey(node.ID)) return y;

        List<string> children = childrenMap[node.ID];
        float currentY = y;

        for (int i = 0; i < children.Count; i++)
        {
            DialogueNodeData child = container.nodes.FirstOrDefault(n => n.ID == children[i]);
            if (child == null) continue;

            float childX = x + 350f;
            float childY = currentY + (i * 250f);

            currentY = LayoutNodeRecursive(child, container, childrenMap, visited, childX, childY, depth + 1);
        }

        return currentY;
    }
}
