using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GraphSaveUtility
{
    private DialogueGraphView _targetGraphView;
    private DialogueContainer _containerCache;

    public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
    {
        return new GraphSaveUtility { _targetGraphView = targetGraphView };
    }

    public void SaveGraph(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        if (AssetDatabase.LoadAssetAtPath<DialogueContainer>(path) != null)
            AssetDatabase.DeleteAsset(path);

        DialogueContainer container = ScriptableObject.CreateInstance<DialogueContainer>();

        foreach (DialogueNode node in _targetGraphView.nodes.ToList().Cast<DialogueNode>())
        {
            DialogueNodeData nodeData = new()
            {
                ID = node.ID,
                Speaker = node.Speaker,
                DialogueText = node.DialogueText,
                FunctionName = node.FunctionName,
                Position = node.GetPosition().position,
                IsBranch = node is BranchNode
            };

            if (node is BranchNode branch)
            {
                nodeData.Choices = new List<string>(branch.Choices);
            }

            container.nodes.Add(nodeData);
        }

        foreach (Edge edge in _targetGraphView.edges.ToList())
        {
            if (edge.output == null || edge.input == null) continue;
            if (edge.output.node is not DialogueNode outputNode) continue;
            if (edge.input.node is not DialogueNode inputNode) continue;

            List<Port> outPorts = outputNode.GetOutputPorts();
            List<Port> inPorts = inputNode.GetInputPorts();

            int outIndex = outPorts.IndexOf(edge.output);
            int inIndex = inPorts.IndexOf(edge.input);

            DialogueLinkData link = new()
            {
                SourceNodeID = outputNode.ID,
                SourcePortIndex = Mathf.Max(0, outIndex),
                TargetNodeID = inputNode.ID,
                TargetPortIndex = Mathf.Max(0, inIndex)
            };

            container.links.Add(link);
        }

        AssetDatabase.CreateAsset(container, path);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Saved", "Dialogue graph saved to:\n" + path, "OK");
    }

    public void LoadGraph(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        _containerCache = AssetDatabase.LoadAssetAtPath<DialogueContainer>(path);
        if (_containerCache == null)
        {
            EditorUtility.DisplayDialog("File Not Found", "Target dialogue graph does not exist!", "OK");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
    }

    private void ClearGraph()
    {
        List<GraphElement> toRemove = _targetGraphView.graphElements.ToList();
        foreach (GraphElement e in toRemove)
            _targetGraphView.RemoveElement(e);
    }

    private void CreateNodes()
    {
        List<DialogueNodeData> nodeDatas = _containerCache.nodes.ToList();

        foreach (DialogueNodeData nd in nodeDatas)
        {
            if (nd.IsBranch)
            {
                BranchNode branch = _targetGraphView.CreateBranchNode(nd.Position);
                branch.SetID(nd.ID);
                branch.LoadData(nd.Speaker, nd.DialogueText, nd.FunctionName, nd.ID);

                branch.ClearChoices();
                foreach (string choiceText in nd.Choices ?? new List<string>())
                    branch.AddChoice(choiceText);
            }
            else
            {
                DialogueNode node = _targetGraphView.CreateDialogueNode(nd.Position);
                node.SetID(nd.ID);
                node.LoadData(nd.Speaker, nd.DialogueText, nd.FunctionName, nd.ID);
            }
        }
    }

    private void ConnectNodes()
    {
        Dictionary<string, DialogueNode> nodes = _targetGraphView.nodes.ToList().Cast<DialogueNode>().ToDictionary(n => n.ID, n => n);

        foreach (DialogueLinkData link in _containerCache.links)
        {
            if (!nodes.ContainsKey(link.SourceNodeID) || !nodes.ContainsKey(link.TargetNodeID))
                continue;

            DialogueNode sourceNode = nodes[link.SourceNodeID];
            DialogueNode targetNode = nodes[link.TargetNodeID];

            List<Port> outPorts = sourceNode.GetOutputPorts();
            List<Port> inPorts = targetNode.GetInputPorts();

            int outIndex = Mathf.Clamp(link.SourcePortIndex, 0, Mathf.Max(0, outPorts.Count - 1));
            int inIndex = Mathf.Clamp(link.TargetPortIndex, 0, Mathf.Max(0, inPorts.Count - 1));

            if (outPorts.Count == 0 || inPorts.Count == 0) continue;

            Port outPort = outPorts[outIndex];
            Port inPort = inPorts[inIndex];

            Edge edge = outPort.ConnectTo(inPort);
            _targetGraphView.AddElement(edge);
        }
    }
}
