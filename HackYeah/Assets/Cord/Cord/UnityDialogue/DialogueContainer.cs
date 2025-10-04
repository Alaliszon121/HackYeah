using System.Collections.Generic;
using UnityEngine;

[System.Serializable] 
public class DialogueNodeData 
{ 
    public string ID; 
    public string Speaker; 
    public string DialogueText; 
    public string FunctionName; 
    public Vector2 Position; 
    public bool IsBranch; 
    public List<string> Choices = new(); 
}

[System.Serializable] 
public class DialogueLinkData 
{ 
    public string SourceNodeID; 
    public int SourcePortIndex; 
    public string TargetNodeID; 
    public int TargetPortIndex; 
}

[CreateAssetMenu(fileName = "DialogueContainer", menuName = "Scriptable Objects/DialogueContainer")] 
public class DialogueContainer : ScriptableObject 
{ 
    public List<DialogueNodeData> nodes = new(); 
    public List<DialogueLinkData> links = new(); 
}