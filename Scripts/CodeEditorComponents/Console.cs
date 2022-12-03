using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Console : MonoBehaviour
{
    [SerializeField] private TMP_InputField _CodeEditorOutputField;
    [SerializeField] private TMP_InputField _InGameOutputField;

    private string _CodeEditorOutputString;
    private string _InGameOutputString;
    
    private void Awake() 
    {
        IDEManager.console = this;
    }
    private void Update() 
    {
        _CodeEditorOutputField.text = _CodeEditorOutputString;
        _InGameOutputField.text = _InGameOutputString;
    }

    public void Print(string Text, bool ClerBeforePrint = false, PrintSource Source = PrintSource.Both)
    {
        if(ClerBeforePrint) Clear(Source);

        switch (Source)
        {
            case PrintSource.CodeEditor:
            _CodeEditorOutputString += Text;
            break;
            case PrintSource.InGameUI:
            _InGameOutputString += Text;
            break;
            case PrintSource.Both:
            _CodeEditorOutputString += Text;
            _InGameOutputString += Text;
            break;
            default:
            Debug.LogError("Unknown print source.");
            break;
        }
    }
    public void EPrint(string Text, bool ClerBeforePrint = false, PrintSource Source = PrintSource.Both)
    {
        if(ClerBeforePrint) Clear(Source);

        string Out = "<color=#FF514E>"+Text+"</color>";
        switch (Source)
        {
            case PrintSource.CodeEditor:
            _CodeEditorOutputString += Out;
            break;
            case PrintSource.InGameUI:
            _InGameOutputString += Out;
            break;
            case PrintSource.Both:
            _CodeEditorOutputString += Out;
            _InGameOutputString += Out;
            break;
            default:
            Debug.LogError("Unknown print source.");
            break;
        }
    }
    public void Clear(PrintSource Source = PrintSource.Both)
    {
        switch (Source)
        {
            case PrintSource.CodeEditor:
            _CodeEditorOutputString = "";
            break;
            case PrintSource.InGameUI:
            _InGameOutputString = "";
            break;
            case PrintSource.Both:
            _CodeEditorOutputString = "";
            _InGameOutputString = "";
            break;
            default:
            Debug.LogError("Unknown print source.");
            break;
        }
    }
}

public enum PrintSource
{
    CodeEditor,
    InGameUI,
    Both
}