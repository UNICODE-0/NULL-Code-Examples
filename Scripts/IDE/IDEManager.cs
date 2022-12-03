using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDEManager : MonoBehaviour
{
    public static CodeEditor codeEditor { get; set; }
    public static Interpreter interpreter { get; set; }
    public static LanguageParser parser { get; set; }
    public static Console console { get; set; }
    public static InputController inputController { get; set; }
    public static LineCounter lineCounter { get; set; }
    public static TextMask textMask { get; set; }
    public static IDEController IDECtrl { get; set; }
    public static Inclusions inclusionsInfo { get; set; }
    private void Start() 
    {
        if(
        codeEditor is null ||
        interpreter is null ||
        parser is null ||
        console is null ||
        inputController is null ||
        lineCounter is null ||
        textMask is null ||
        IDECtrl is null || 
        inclusionsInfo is null) Debug.LogError("IDEManager is not initialized.");
    }

}
