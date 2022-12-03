using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

[RequireComponent(typeof(RectTransform))]
public class LineCounter : MonoBehaviour
{
    [SerializeField] private RectTransform TargetRectTransform;
    [SerializeField] private TMP_InputField _Input;

    private RectTransform _RecTf;
    private int _LastLineCount = 0;
    private void Awake() 
    {
        IDEManager.lineCounter = this;
        _RecTf = GetComponent<RectTransform>();
    }
    private void Start() 
    {
        ChangeLineCount();
        _LastLineCount = IDEManager.codeEditor.tmpText.GetTextInfo(IDEManager.codeEditor.tmpText.text).lineCount;
    }
    private void Update()
    {
        _RecTf.offsetMin = new Vector2(0,TargetRectTransform.offsetMin.y);
        _RecTf.offsetMax = new Vector2(0,TargetRectTransform.offsetMax.y);
    }
    public void ChangeLineCount()
    {
        if(IDEManager.inputController.isPaste) return;

        TMP_Text TmpText = IDEManager.codeEditor.tmpText;
        TMP_TextInfo TmpTextInfo = TmpText.GetTextInfo(TmpText.text);
        
        int CurrentLineCount = TmpTextInfo.lineCount;

        if(_LastLineCount != CurrentLineCount)
        {
            TMP_LineInfo[] TmpLinesInfo = TmpTextInfo.lineInfo;
            string TempText = " 1\n";
            int VisibleLinesCount = 1;
            foreach (var TMPLineInfo in TmpLinesInfo)
            {
                int LastLineCharIndex = TMPLineInfo.lastCharacterIndex;

                if(LastLineCharIndex < TmpTextInfo.characterCount && TmpText.text[LastLineCharIndex] == '\n')
                {
                    VisibleLinesCount++;
                    if(VisibleLinesCount > TmpTextInfo.lineCount) break;
                    TempText += $"{VisibleLinesCount}\n";
                } 
                else TempText += '\n';
            }
            
            _Input.text = TempText;
            _LastLineCount = CurrentLineCount;
        }
    }
}
