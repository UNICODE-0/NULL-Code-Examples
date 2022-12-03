using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class CodeEditor : MonoBehaviour
{
    [SerializeField] private TMP_InputField _TargetInput;
    [SerializeField] private TMP_Text _InputText;
    [SerializeField] private TMP_Text _TextMask;
    [SerializeField] private Keyword[] _Keywords;

    private List<(string DefaultWord, string FormattedKeyword)> _FormattedKeywords = new List<(string, string)>();

    public string inputText
    {
        get { return _TargetInput.text; }
    }
    public Vector2 CaretStartOffsetMin { get; private set; }
    public Vector2 CaretStartOffsetMax { get; private set; }
    public TMP_Text tmpText
    {
        get { return _InputText; }
    }
    public TMP_InputField inputField
    {
        get { return _TargetInput; }
    }
    public int caretPosition
    {
        get { return _TargetInput.caretPosition; }
    }

    private void Awake() 
    {
        IDEManager.codeEditor = this;
    }
    private void Start() 
    {
        string KeyWordText;
        string ColorKeyWordText;

        foreach (Keyword Keyword in _Keywords)
        {
            KeyWordText = Keyword.formatedWord;
            ColorKeyWordText = $"<color={Keyword.hexColor}>{Keyword.word}</color>";

            if(Keyword.spaceNeeded)
            {
                KeyWordText += " ";
                ColorKeyWordText += " ";
            }

            if(Keyword.CompleteMatch) KeyWordText = String.Format(@"\b{0}\b", KeyWordText);

            _FormattedKeywords.Add((KeyWordText, ColorKeyWordText));
        }
    }
    public void ChangeSourceString(string ToString)
    {
        _TargetInput.text = ToString;
    }
    public void OnTextChanged()
    {
        if(IDEManager.inputController.isPaste) return;

        // if(_LastTextLength < _TargetInput.text.Length && Regex.IsMatch(_TargetInput.text[caretPosition - 1].ToString(), "[ЁёА-я]"))
        // _TargetInput.text = _TargetInput.text.Remove(caretPosition - 1,1);

        string InputString = _TargetInput.text;

        if(Settings.syntaxHighlight)
        {
            foreach (var Keyword in _FormattedKeywords)
            {
                InputString = Regex.Replace(InputString, Keyword.DefaultWord, Keyword.FormattedKeyword);
            }
        }
        
        _TextMask.text = InputString;
    }
}

[Serializable]
class Keyword
{
    [SerializeField] private string _Word;
    public string formatedWord
    {
        get 
        { 
            if(Regex.IsMatch(_Word, "^[a-zA-Z]*$")) return _Word;
            else if(_Word.Length == 1) return @"\" + _Word;
            else
            {
                Debug.LogError($"Wrong keyword text: {_Word}");
                return _Word;
            }
        }
    }
    public string word
    {
        get { return _Word; }
    }
    
    [SerializeField] private string _HexColor;
    public string hexColor
    {
        get { return _HexColor; }
    }
    [SerializeField] private bool _SpaceNeeded = false;
    public bool spaceNeeded
    {
        get { return _SpaceNeeded; }
    }
    [SerializeField] private bool _СompleteMatch = false;
    public bool CompleteMatch
    {
        get { return _СompleteMatch; }
    }
    public Keyword(Keyword CopyKeyword)
    {
        _Word = CopyKeyword.word;
        _HexColor = CopyKeyword.hexColor;
        _SpaceNeeded = CopyKeyword.spaceNeeded;
    }
    public Keyword(string Word, string HexColor, bool SpaceNeeded = false, bool СompleteMatch = false)
    {
        _Word = Word;
        _HexColor = HexColor;
        _SpaceNeeded = SpaceNeeded;
        _СompleteMatch = СompleteMatch;
    }
}
