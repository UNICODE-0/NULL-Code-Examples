using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuideController : MonoBehaviour
{
    [SerializeField] GameObject _GuideSectionText;
    [SerializeField] GameObject _FunctionSectionText;
    [SerializeField] private Button _GuideSectionButton;
    [SerializeField] private Button _FunctionSectionButton;
    [SerializeField] private Color _GuideButtonDefaultColor;

    private ColorBlock _DefaultGuideButtonColorBlock;
    private ColorBlock SelectedButtonColorBlock;
    bool _IsBeforeFirstPress = true;
    private void Awake() 
    {
        _DefaultGuideButtonColorBlock = _GuideSectionButton.colors;

        SelectedButtonColorBlock = _GuideSectionButton.colors;
        SelectedButtonColorBlock.normalColor = _GuideButtonDefaultColor;
    }
    private void OnEnable() 
    {
        _IsBeforeFirstPress = true;

        if(_GuideSectionText.activeSelf) _GuideSectionButton.colors = SelectedButtonColorBlock;
        else _FunctionSectionButton.colors = SelectedButtonColorBlock;
    }
    private void ResetButtonsColor()
    {
        _GuideSectionButton.colors = _DefaultGuideButtonColorBlock;
        _FunctionSectionButton.colors = _DefaultGuideButtonColorBlock;
        _IsBeforeFirstPress = false;
    }
    public void OpenGuideSection()
    {   
        if(_IsBeforeFirstPress) ResetButtonsColor();

        _FunctionSectionText.SetActive(false);
        _GuideSectionText.SetActive(true);
    }
    public void OpenFunctionSection()
    {
        if(_IsBeforeFirstPress) ResetButtonsColor();

        _GuideSectionText.SetActive(false);
        _FunctionSectionText.SetActive(true);
    }
    public void OpenGuidePanel()
    {
        if(gameObject.activeSelf) gameObject.SetActive(false);
        else gameObject.SetActive(true);
    }
}
