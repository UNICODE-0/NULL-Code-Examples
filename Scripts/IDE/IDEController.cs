using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IDEController : MonoBehaviour
{
    [SerializeField] private Image _ParseProgressBar;
    [SerializeField] private GameObject _UIParent;
    [SerializeField] private bool _DisableUIOnLoad = true;

    private bool _HotKeysDisabled = false;

    public delegate void OnEnableUI();
    public static event OnEnableUI onEnableUI;
    public delegate void OnDisableUI();
    public static event OnDisableUI onDisableUI;

    private void Awake() 
    {
        IDEManager.IDECtrl = this;
    }
    private void Start() 
    {
        if(_DisableUIOnLoad) _UIParent.SetActive(false);    
    }
    private void FixedUpdate() 
    {
        if(IDEManager.parser.isParsing)
        {
            _ParseProgressBar.fillAmount = IDEManager.parser.parseStatus;
        } else _ParseProgressBar.fillAmount = 0;
    }
    private void Update() 
    {
        // Hotkeys
        if(_HotKeysDisabled) return;

        if(Input.GetKeyDown(KeyCode.F1))
        {
            if(_UIParent.activeSelf)
            {
                DisableUI();
            } 
            else
            {
                EnableUI();
            } 
        }
    }
    public void DisableHotKeys()
    {
        _HotKeysDisabled = true;
    }
    public void EnableHotKeys()
    {
        _HotKeysDisabled = false;
    }
    public void DisableUI()
    {
        onDisableUI();
        _UIParent.SetActive(false);
    }
    public void EnableUI()
    {
        onEnableUI();
        _UIParent.SetActive(true);
    }
}
