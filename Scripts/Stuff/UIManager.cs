using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Image _BlackScreen;
    [SerializeField] private Button _RestartButton;
    [SerializeField] private Button _ExecuteButton;
    [SerializeField] private Button _CompileButton;
    [SerializeField] private TMP_Text _CompileButtontText;
    [SerializeField] private GameObject _MenuPanel;
    [SerializeField] private GameObject _SettingsPanel;
    [SerializeField] private TMP_Text _MemoryUsedText;
    [Space]
    [Range(0,1)]
    [SerializeField] private float _CompileButtonDisableAlpha = 0.55f;

    private const float RED_COLOR_OVERFLOW = 1.5f; // Multiplier

    private float _CompileButtonDefultAlpha;
    private float _BlackScreenTransparency;

    public float blackScreenTransparency
    {
        get
        { 
            return _BlackScreenTransparency; 
        }
        set 
        { 
            _BlackScreenTransparency = value;
            _BlackScreen.color = new Color(0.05f, 0.05f, 0.05f, value);
        }
    }
    public static UIManager instance { get; set; }
    private void Awake() 
    {
        instance = this;
    }
    private void OnEnable() 
    {
        Interpreter.compileEnd += SetMemoryUsed;
    }
    private void OnDisable() 
    {
        Interpreter.compileEnd -= SetMemoryUsed;
    }
    private void Start() 
    {
        _CompileButtonDefultAlpha = _CompileButtontText.color.a;

        SetMemoryUsed(0);
    }
    private void Update() 
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(_MenuPanel.activeSelf) CloseMenuPanel();
            else OpenMenuPanel();
        }
    }
    private void SetMemoryUsed(int MemoryUsed)
    {
        string MemoryUsedString = "";
        if(MemoryUsed > LevelManager.avalibleMemory * RED_COLOR_OVERFLOW)
        {
            MemoryUsedString = $"Memory: <color=#CC2222>{MemoryUsed}/{LevelManager.avalibleMemory}</color>";
        } else if (MemoryUsed > LevelManager.avalibleMemory)
        {
            MemoryUsedString = $"Memory: <color=#DE8300>{MemoryUsed}/{LevelManager.avalibleMemory}</color>";
        }else
        {
            MemoryUsedString = $"Memory: {MemoryUsed}/{LevelManager.avalibleMemory}";
        }
        _MemoryUsedText.text = MemoryUsedString;
    }
    private void SetCompileButtonDisableState()
    {
        _CompileButton.interactable = false;
        Color ButtonCol = _CompileButtontText.color;
        _CompileButtontText.color = new Color(ButtonCol.r,ButtonCol.g,ButtonCol.b, _CompileButtonDisableAlpha);
    }
    private void SetCompileButtonDefaultState()
    {
        _CompileButton.interactable = true;
        Color ButtonCol = _CompileButtontText.color;
        _CompileButtontText.color = new Color(ButtonCol.r,ButtonCol.g,ButtonCol.b, _CompileButtonDefultAlpha);
    }
    public void SetExecuteUIState()
    {
        _ExecuteButton.interactable = false;
        SetCompileButtonDisableState();
    }
    public void SetCompileUIState()
    {
        _RestartButton.interactable = false;
        _ExecuteButton.interactable = false;
        SetCompileButtonDisableState();
    }
    public void SetLevelEndUIState()
    {
        _RestartButton.interactable = false;
        _ExecuteButton.interactable = false;
        SetCompileButtonDisableState();
    }
    public void SetDefaultUIState()
    {
        _RestartButton.interactable = true;
        _ExecuteButton.interactable = true;
        SetCompileButtonDefaultState();
    }
    public void OpenMenuPanel()
    {
        _MenuPanel.SetActive(true);
        LevelManager.freeCam.DisableFreeCam();
        IDEManager.IDECtrl.DisableHotKeys();
    }
    public void OpenSettingsPanel()
    {
        if(_SettingsPanel.activeSelf) _SettingsPanel.SetActive(false);
        else _SettingsPanel.SetActive(true);
    }
    public void CloseMenuPanel()
    {
        _MenuPanel.SetActive(false);
        _SettingsPanel.SetActive(false);
        LevelManager.freeCam.EnableFreeCam();
        IDEManager.IDECtrl.EnableHotKeys();
    }
    public void ExitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
