using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphView : GraphView, INodeIdValidator 
{ 
    private List<Node> _copyBuffer = new(); 
    private List<Edge> _copiedEdges = new(); 

    public DialogueGraphView() { 
        style.flexGrow = 1; 

        this.AddManipulator(new ContentZoomer()); 
        this.AddManipulator(new ContentDragger()); 
        this.AddManipulator(new SelectionDragger()); 
        this.AddManipulator(new RectangleSelector()); 

        GridBackground grid = new(); Insert(0, grid); 
        grid.StretchToParentSize(); 

        RegisterCallback<KeyDownEvent>(OnKeyDown); 
    } 

    private void OnKeyDown(KeyDownEvent evt) 
    { 
        if (evt.ctrlKey && evt.keyCode == KeyCode.C) 
        { 
            CopySelection(); 
            evt.StopImmediatePropagation(); 
        } 
        else if (evt.ctrlKey && evt.keyCode == KeyCode.V) 
        { 
            PasteSelection(); 
            evt.StopImmediatePropagation(); 
        } 
    } 

    public DialogueNode CreateDialogueNode(Vector2 pos = default) 
    { 
        DialogueNode node = new() { title = "Dialogue Node" }; 
        node.SetIdValidator(this); 
        AddElement(node); 
        node.SetPosition(new Rect(pos == default ? Vector2.zero : pos, new Vector2(250, 150))); 

        return node; 
    } 

    public BranchNode CreateBranchNode(Vector2 pos = default) 
    { 
        BranchNode node = new() { title = "Branch Node" }; 
        node.SetIdValidator(this); 
        AddElement(node); 
        node.SetPosition(new Rect(pos == default ? Vector2.zero : pos, new Vector2(300, 200))); 
        
        return node; 
    } 

    public bool IsIdUsed(string id) { return nodes.ToList().OfType<DialogueNode>().Any(n => n.ID == id); } 

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) 
    { 
        List<Port> portsList = new(); 

        foreach (Port p in ports) 
        { 
            if (p == startPort) continue; 
            if (p.node == startPort.node) continue; 
            portsList.Add(p); 
        } 

        return portsList; 
    } 

    private void CopySelection() { 
        _copyBuffer.Clear(); 
        _copiedEdges.Clear(); 
        _copyBuffer = selection.OfType<Node>().Where(n => n is DialogueNode || n is BranchNode).ToList(); 

        if (_copyBuffer.Count == 0) 
        { 
            Debug.LogWarning("No nodes selected to copy."); 
            return; 
        } 

        _copiedEdges = edges.ToList().Where(e => 
        { 
            return e.output?.node is Node outNode && e.input?.node is Node inNode && _copyBuffer.Contains(outNode) && _copyBuffer.Contains(inNode); 
        }).ToList(); 

        Debug.Log($"Copied {_copyBuffer.Count} node(s) and {_copiedEdges.Count} connection(s)."); 
    } 

    private void PasteSelection() 
    { 
        if (_copyBuffer.Count == 0) 
        { 
            Debug.LogWarning("Nothing copied to paste."); 
            return; 
        } 
        ClearSelection(); 
        Vector2 offset = new(50, 50); 
        Dictionary<Node, Node> cloneMap = new(); 
        foreach (Node original in _copyBuffer) 
        { 
            Node clone; 

            if (original is BranchNode originalBranch) 
            { 
                BranchNode branchClone = CreateBranchNode(original.GetPosition().position + offset); 
                branchClone.Speaker = originalBranch.Speaker; 
                branchClone.DialogueText = originalBranch.DialogueText; 
                branchClone.FunctionName = originalBranch.FunctionName; 
                branchClone.Choices = new List<string>(originalBranch.Choices); 
                branchClone.SetID(Guid.NewGuid().ToString()); clone = branchClone; 
            } 
            else if (original is DialogueNode originalDialogue) 
            { 
                DialogueNode dialogueClone = CreateDialogueNode(original.GetPosition().position + offset); 
                dialogueClone.Speaker = originalDialogue.Speaker; 
                dialogueClone.DialogueText = originalDialogue.DialogueText; 
                dialogueClone.FunctionName = originalDialogue.FunctionName; 
                dialogueClone.SetID(Guid.NewGuid().ToString()); clone = dialogueClone; 
            } 
            else continue; 
            
            cloneMap[original] = clone; 
            AddToSelection(clone); 
        } 
        
        foreach (Edge edge in _copiedEdges) 
        { 
            if (edge.output?.node is not Node oldOutNode || edge.input?.node is not Node oldInNode) continue; 

            if (cloneMap.ContainsKey(oldOutNode) && cloneMap.ContainsKey(oldInNode)) 
            { 
                Node newOutNode = cloneMap[oldOutNode]; 
                Node newInNode = cloneMap[oldInNode]; 

                List<Port> oldOutPorts = oldOutNode.outputContainer.Children().OfType<Port>().ToList(); 
                int oldOutPortIndex = oldOutPorts.IndexOf(edge.output); 

                List<Port> newOutPorts = newOutNode.outputContainer.Children().OfType<Port>().ToList(); 
                Port newOutPort = newOutPorts.ElementAtOrDefault(oldOutPortIndex); 

                List<Port> newInPorts = newInNode.inputContainer.Children().OfType<Port>().ToList(); 

                Port newInPort = newInPorts.FirstOrDefault(); 

                if (newOutPort != null && newInPort != null) 
                { 
                    Edge newEdge = newOutPort.ConnectTo(newInPort); 
                    AddElement(newEdge); 
                } 
            } 
        } 
        Debug.Log($"Pasted {cloneMap.Count} node(s) and {_copiedEdges.Count} edge(s)."); 
    } 
}