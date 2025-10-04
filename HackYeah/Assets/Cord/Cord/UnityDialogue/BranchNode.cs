using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class BranchNode : DialogueNode 
{ 
    public List<string> Choices = new(); 
    private int _choiceCounter = 1; 
    public BranchNode() : base() 
    { 
        title = "Branch"; 
        titleContainer.style.backgroundColor = new StyleColor(new Color(0.55f, 0.25f, 0.70f)); 
        
        outputContainer.Clear(); 

        Button addButton = new(() => { Choices.Add("Choice " + _choiceCounter++); RebuildOutputPorts(); }) { text = "Add Choice" }; 
        mainContainer.Add(addButton); 

        RefreshExpandedState(); 
        RefreshPorts(); 
    } 

    public void AddChoice(string choiceDialogue) 
    { 
        Choices.Add(choiceDialogue); 
        RebuildOutputPorts(); 
    } 

    public void RebuildOutputPorts() 
    { 
        outputContainer.Clear(); 
        for (int i = 0; i < Choices.Count; i++) 
        { 
            int index = i; 
            string choice = Choices[index]; 

            Port choicePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float)); 
            choicePort.portName = ""; 
            
            TextField textField = new() { value = UIHelpers.WrapText(choice, 40), multiline = false, style = { width = 200 } }; 
            textField.RegisterValueChangedCallback(evt => 
            { 
                Choices[index] = evt.newValue; 
                textField.SetValueWithoutNotify(UIHelpers.WrapText(evt.newValue, 40)); 
            }); 
            
            VisualElement row = new(); 
            row.style.flexDirection = FlexDirection.Row; 
            row.Add(choicePort); 
            row.Add(textField); 
            
            outputContainer.Add(row); 
        } 
        RefreshExpandedState(); 
        RefreshPorts(); 
    } 

    public void ClearChoices() { 
        Choices.Clear(); 
        RebuildOutputPorts(); 
    } 
}
