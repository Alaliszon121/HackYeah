using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueNode : Node 
{ 
    private string _id; 
    public string ID => _id;
    public void SetID(string newId) { if (!string.IsNullOrEmpty(newId)) _id = newId; }
    public string Speaker; 
    public string DialogueText; 
    public string FunctionName; 

    private INodeIdValidator _idValidator; 

    protected TextField _idField;
    protected TextField _speakerField;
    protected TextField _dialogueField;
    protected TextField _functionField;

    public DialogueNode() { 
        SetID(Guid.NewGuid().ToString()); 

        title = "Dialogue"; 
        titleContainer.style.backgroundColor = new StyleColor(new Color(0.2f, 0.45f, 0.85f)); 

        _idField = new TextField("ID:") { value = UIHelpers.WrapText(ID, 40), multiline = true }; 
        _idField.RegisterValueChangedCallback(evt => 
        { 
            string newID = evt.newValue.Trim().Replace("\n", " "); 
            if (!string.IsNullOrEmpty(newID) && (_idValidator == null || !_idValidator.IsIdUsed(newID) || newID == ID)) 
            { 
                SetID(newID); 
                _idField.SetValueWithoutNotify(UIHelpers.WrapText(ID, 40)); 
            } 
            else _idField.SetValueWithoutNotify(UIHelpers.WrapText(ID, 40));
        }); 

        mainContainer.Add(_idField); 

        _speakerField = new TextField("Speaker:") { value = Speaker }; 
        _speakerField.RegisterValueChangedCallback(evt => Speaker = evt.newValue); 
        mainContainer.Add(_speakerField); 

        _dialogueField = new TextField("Dialogue:") { value = DialogueText, multiline = true }; 
        _dialogueField.RegisterValueChangedCallback(evt => 
        { 
            DialogueText = evt.newValue; 
            _dialogueField.SetValueWithoutNotify(UIHelpers.WrapText(DialogueText, 40)); 
        }); 
        mainContainer.Add(_dialogueField); 

        _functionField = new TextField("Function (optional):") { value = FunctionName }; 
        _functionField.RegisterValueChangedCallback(evt => FunctionName = evt.newValue); 
        mainContainer.Add(_functionField); 

        Port input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float)); 
        input.portName = "In"; 
        inputContainer.Add(input); 

        Port output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float)); 
        output.portName = ""; 
        outputContainer.Add(output); 

        RefreshExpandedState(); 
        RefreshPorts(); 
    }

    public void SetIdValidator(INodeIdValidator validator) => _idValidator = validator; 

    public virtual void LoadData(string speaker, string dialogueText, string functionName, string id = null) 
    { 
        if (!string.IsNullOrEmpty(id)) SetID(id); 
        Speaker = speaker; 
        DialogueText = dialogueText; 
        FunctionName = functionName; 

        if (_idField != null) _idField.SetValueWithoutNotify(UIHelpers.WrapText(ID, 40)); 
        if (_speakerField != null) _speakerField.SetValueWithoutNotify(Speaker); 
        if (_dialogueField != null) _dialogueField.SetValueWithoutNotify(DialogueText); 
        if (_functionField != null) _functionField.SetValueWithoutNotify(FunctionName); 
    } 

    public List<Port> GetOutputPorts() { return GetPortsInContainer(outputContainer); } 

    public List<Port> GetInputPorts() { return GetPortsInContainer(inputContainer); } 

    protected List<Port> GetPortsInContainer(VisualElement container) 
    { 
        List<Port> found = new(); 
        Stack<VisualElement> stack = new(); 

        stack.Push(container); 

        while (stack.Count > 0) 
        { 
            VisualElement ve = stack.Pop(); 
            foreach (VisualElement child in ve.Children()) 
            { 
                if (child is Port p) found.Add(p); 
                else if (child is VisualElement veChild) stack.Push(veChild); 
            } 
        } 
        return found; 
    } 
}