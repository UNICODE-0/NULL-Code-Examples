using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ScrollBarFix : MonoBehaviour
{
    [SerializeField] private Scrollbar _ScrollBar;
    [SerializeField] private TMP_InputField _InputField;
    [SerializeField] private float _ScrollSensitivity = 10f;
    void Start()
    {
        _InputField.verticalScrollbar = _ScrollBar;
        _InputField.scrollSensitivity = _ScrollSensitivity;
        _InputField.ForceLabelUpdate();
    }
}
