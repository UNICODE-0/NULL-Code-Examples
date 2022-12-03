using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsLock : MonoBehaviour
{
    [SerializeField] private int _TargetFrameRate = 100;
    private void Awake() 
    {
        Application.targetFrameRate = _TargetFrameRate;
    }
}
