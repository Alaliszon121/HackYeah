using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class DialogueGraphWindow : EditorWindow
{
    private DialogueGraphView _graphView;

    [MenuItem("Window/Dialogue Graph")]
    public static void OpenDialogueGraph()
    {
        DialogueGraphWindow window = GetWindow<DialogueGraphWindow>();
        window.titleContent = new GUIContent("Dialogue Graph");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
    }

    private void OnDisable()
    {
        if (_graphView != null)
            rootVisualElement.Remove(_graphView);
    }

    private void ConstructGraphView()
    {
        _graphView = new DialogueGraphView { name = "Dialogue Graph" };
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    private void GenerateToolbar()
    {
        VisualElement toolbar = new();

        toolbar.style.flexDirection = FlexDirection.Row;
        toolbar.style.justifyContent = Justify.FlexStart;
        toolbar.style.alignItems = Align.Center;
        toolbar.style.marginBottom = 6;
        toolbar.style.paddingTop = 2;
        toolbar.style.paddingBottom = 2;
        toolbar.style.paddingLeft = 4;

        static VisualElement CreateGroup(string label, VisualElement content)
        {
            VisualElement group = new();

            group.style.flexDirection = FlexDirection.Column;
            group.style.alignItems = Align.FlexStart;
            group.style.marginRight = 10;
            group.style.paddingLeft = 6;
            group.style.paddingRight = 6;
            group.style.paddingTop = 4;
            group.style.paddingBottom = 4;

            group.style.borderTopWidth = 1;
            group.style.borderBottomWidth = 1;
            group.style.borderLeftWidth = 1;
            group.style.borderRightWidth = 1;
            group.style.borderTopColor = new Color(0.4f, 0.4f, 0.4f);
            group.style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f);
            group.style.borderLeftColor = new Color(0.4f, 0.4f, 0.4f);
            group.style.borderRightColor = new Color(0.4f, 0.4f, 0.4f);
            group.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);

            Label groupLabel = new(label);

            groupLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            groupLabel.style.fontSize = 11;
            groupLabel.style.marginBottom = 2;

            group.Add(groupLabel);
            group.Add(content);

            return group;
        }

        Button twineButton = new(() =>
        {
            string path = EditorUtility.OpenFilePanel("Import Twine (.twee)", "Assets", "twee");
            if (!string.IsNullOrEmpty(path)) TwineImporter.ImportToGraph(path, _graphView);
        })
        { text = "Import Twine" };

        toolbar.Add(CreateGroup("Twine", twineButton));

        DropdownField jsonDropdown = new("", new List<string> { "Select", "Export", "Import" }, 0);
        jsonDropdown.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue == "Export")
            {
                string path = EditorUtility.SaveFilePanel("Export JSON", "Assets", "dialogues", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    string tempPath = "Assets/__TempDialogue.asset";
                    GraphSaveUtility.GetInstance(_graphView).SaveGraph(tempPath);
                    DialogueContainer container = AssetDatabase.LoadAssetAtPath<DialogueContainer>(tempPath);
                    DialogueJsonUtility.ExportToJson(container, path);
                }
            }
            else if (evt.newValue == "Import")
            {
                string path = EditorUtility.OpenFilePanel("Import JSON", "Assets", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    DialogueContainer container = DialogueJsonUtility.ImportFromJson(path);
                    if (container != null)
                    {
                        string assetPath = "Assets/ImportedDialogue.asset";
                        AssetDatabase.CreateAsset(container, assetPath);
                        AssetDatabase.SaveAssets();
                        GraphSaveUtility.GetInstance(_graphView).LoadGraph(assetPath);
                    }
                }
            }
            jsonDropdown.value = "Select";
        });

        DropdownField assetDropdown = new("", new List<string> { "Select", "Save", "Load" }, 0);
        assetDropdown.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue == "Save")
            {
                string path = EditorUtility.SaveFilePanelInProject("Save Dialogue Graph", "NewDialogueGraph", "asset", "");
                if (!string.IsNullOrEmpty(path))
                    GraphSaveUtility.GetInstance(_graphView).SaveGraph(path);
            }
            else if (evt.newValue == "Load")
            {
                string path = EditorUtility.OpenFilePanel("Load Dialogue Graph", "Assets", "asset");
                if (!string.IsNullOrEmpty(path))
                {
                    if (!path.StartsWith("Assets") && path.Contains(Application.dataPath))
                        path = "Assets" + path[Application.dataPath.Length..];
                    GraphSaveUtility.GetInstance(_graphView).LoadGraph(path);
                }
            }
            assetDropdown.value = "Select";
        });

        VisualElement nodesGroup = new();
        nodesGroup.style.flexDirection = FlexDirection.Row;

        Button dialogueNodeButton = new(() => _graphView.CreateDialogueNode())
        {
            text = "Add Dialogue Node"
        };
        Button branchNodeButton = new(() => _graphView.CreateBranchNode())
        {
            text = "Add Branch Node"
        };

        nodesGroup.Add(dialogueNodeButton);
        nodesGroup.Add(branchNodeButton);

        toolbar.Add(CreateGroup("JSON", jsonDropdown));
        toolbar.Add(CreateGroup(".asset", assetDropdown));
        toolbar.Add(CreateGroup("Nodes", nodesGroup));

        rootVisualElement.Add(toolbar);
    }

    private void RequestDataOperation(bool save)
    {
        string path;
        if (save) path = EditorUtility.SaveFilePanelInProject("Save Dialogue Graph", "NewDialogueGraph", "asset", "");
        else path = EditorUtility.OpenFilePanel("Load Dialogue Graph", "Assets", "asset");

        if (string.IsNullOrEmpty(path)) return;

        if (!path.StartsWith("Assets") && path.Contains(Application.dataPath))
        {
            path = "Assets" + path[Application.dataPath.Length..];
        }

        GraphSaveUtility util = GraphSaveUtility.GetInstance(_graphView);

        if (save) util.SaveGraph(path);
        else util.LoadGraph(path);
    }
}
