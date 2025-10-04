using System;
using System.Collections.Generic;

[Serializable]
public class DialogueJsonRoot
{
    public List<DialogueJsonDialog> dialogs = new();
}

[Serializable]
public class DialogueJsonDialog
{
    public string id;
    public string speaker;
    public string text;
    public string nextId;
    public string callback;
    public List<DialogueJsonChoice> choices = new();
}

[Serializable]
public class DialogueJsonChoice
{
    public string text;
    public string nextId;
    public string callback;
}
