using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextList : MonoBehaviour
{
    [SerializeField] TMP_Text[] _TextItems;
    [SerializeField] Button _PreviousButton;
    [SerializeField] Button _NextButton;

    private int _CurrentItem = 0;
    private void Awake() 
    {
        SetButtonsActiveState();
    }
    public void NextItem()
    {
        _TextItems[_CurrentItem].gameObject.SetActive(false);
        _TextItems[++_CurrentItem].gameObject.SetActive(true);
        SetButtonsActiveState();
    }
    public void PreviousItem()
    {
        _TextItems[_CurrentItem].gameObject.SetActive(false);
        _TextItems[--_CurrentItem].gameObject.SetActive(true);
        SetButtonsActiveState();
    }

    private void SetButtonsActiveState()
    {
        if(_CurrentItem - 1 < 0)
        {
            _PreviousButton.interactable = false;
        } else if(_CurrentItem + 1 >= _TextItems.Length)
        {
            _NextButton.interactable = false;
        } else
        {
            _NextButton.interactable = true;
            _PreviousButton.interactable = true;
        }
    }
}
