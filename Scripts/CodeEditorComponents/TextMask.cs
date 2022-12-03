using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class TextMask : MonoBehaviour
{
    [SerializeField] private RectTransform _TargetRectTransform;
    private RectTransform _RecTf;

    public Vector2 startOffsetMin { get; private set; }
    public Vector2 startOffsetMax { get; private set; }
    public RectTransform inputRectTransform
    {
        get { return _TargetRectTransform; }
    }
    public RectTransform maskRectTransform
    {
        get { return _TargetRectTransform; }
    }
    
    private void Awake() 
    {
        _RecTf = GetComponent<RectTransform>();
        
        startOffsetMin = _TargetRectTransform.offsetMin;
        startOffsetMax = _TargetRectTransform.offsetMax;
        IDEManager.textMask = this;
    }

    private void Update() 
    {
        _RecTf.offsetMin = new Vector2(_TargetRectTransform.offsetMin.x, _TargetRectTransform.offsetMin.y);
        _RecTf.offsetMax = new Vector2(_TargetRectTransform.offsetMax.x, _TargetRectTransform.offsetMax.y);
    }
}
