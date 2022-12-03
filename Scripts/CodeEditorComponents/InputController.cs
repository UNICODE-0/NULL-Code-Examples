using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private const int MAX_UNDO_STEPS = 100;

    private List<string> _UndoStrings = new List<string>();
    string _PreviousInputString = "";
    private Coroutine _PasteCommandCoroutine;
    private bool _CompileHotKeyState = true;

    public bool isPaste { get; set; }
    private void Awake() 
    {
        IDEManager.inputController = this;    
    }
    private void OnEnable() 
    {
        _PasteCommandCoroutine = StartCoroutine(CheckPasteCommand());
    }
    private void OnDisable() 
    {
        StopCoroutine(_PasteCommandCoroutine);
    }
    public void DisableCompileHotKey()
    {
        _CompileHotKeyState = false;
    }
    public void EnableCompileHotKey()
    {
        _CompileHotKeyState = true;
    }
    private void Update()
    {
        // if (Input.GetKey(KeyCode.PageUp) || Input.GetKey(KeyCode.PageDown) || Input.GetKeyUp(KeyCode.PageUp) || Input.GetKeyUp(KeyCode.PageDown))
        // {
        //     IDEManager.codeEditor.caretRectTransform.offsetMax = IDEManager.codeEditor.CaretStartOffsetMax;
        //     IDEManager.codeEditor.caretRectTransform.offsetMin = IDEManager.codeEditor.CaretStartOffsetMin;

        //     IDEManager.textMask.inputRectTransform.offsetMax = IDEManager.textMask.startOffsetMax;
        //     IDEManager.textMask.inputRectTransform.offsetMin = IDEManager.textMask.startOffsetMin;

        //     IDEManager.codeEditor.inputField.caretPosition = 0;
        // }
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S) && _CompileHotKeyState)
        {
            IDEManager.interpreter.CompileAsync();
        }
        
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
        {
            Undo();
        }
    }
    private void Undo()
    {
        if(_UndoStrings.Count > 0)
        {
            IDEManager.codeEditor.ChangeSourceString(_UndoStrings[_UndoStrings.Count - 1]);
            _UndoStrings.RemoveAt(_UndoStrings.Count - 1);
        }
    }
    IEnumerator CheckPasteCommand()
    {
        while(true)
        {
            yield return new WaitForEndOfFrame();
            if(isPaste)
            {
                isPaste = false;
                ReplaceCarriadeReturn();

                WriteUndo();
                
                IDEManager.codeEditor.OnTextChanged();
                IDEManager.lineCounter.ChangeLineCount();
            }
        }
    }
    private void ReplaceCarriadeReturn()
    {
        IDEManager.codeEditor.ChangeSourceString(IDEManager.codeEditor.inputText.Replace("\r", ""));
    }
    private void ReplacaeCyrillic()
    {
        string InputString = IDEManager.codeEditor.inputText;
        for (int i = 0; i < InputString.Length; i++)
        {
            if(Regex.IsMatch(InputString[i].ToString(), "[ЁёА-я]")) InputString = InputString.Remove(i,1);
        }
        IDEManager.codeEditor.ChangeSourceString(InputString);
    }
    public void OnTextChanged()
    {
        if(isPaste) return;
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && (Input.GetKey(KeyCode.V) || Input.GetKeyUp(KeyCode.V)))
        {
            isPaste = true;
            return;
        }

        WriteUndo();
    }
    private void WriteUndo()
    {
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
        {
            _PreviousInputString = IDEManager.codeEditor.inputText;
        } else
        {
            if(_UndoStrings.Count >= MAX_UNDO_STEPS) _UndoStrings.RemoveAt(0);
            _UndoStrings.Add(_PreviousInputString);

            _PreviousInputString = IDEManager.codeEditor.inputText;
        }
    }
}
