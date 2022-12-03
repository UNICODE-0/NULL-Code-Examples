using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class TimerDoorController : DoorController
{
    [SerializeField] private TMP_Text _TimerText;
    [SerializeField] private float _Time;

    private Coroutine _TimerCoroutine;
    private void Start() 
    {
        SetDefaultColor();
        _TimerText.gameObject.SetActive(false);
    }
    public override void OpenDoor()
    {
        base.OpenDoor();
        StartTimer(_Time);
    }
    private void StartTimer(float Time)
    {
        _TimerCoroutine = StartCoroutine(Timer(Time));
    }
    public override void SetInitialState()
    {
        base.SetInitialState();
        if(_TimerCoroutine is not null)
        {
            StopCoroutine(_TimerCoroutine);
            _TimerText.gameObject.SetActive(false);
        }
    }
    IEnumerator Timer(float Time)
    {
        float CurrentTime = Time;
        float TimerStep = WallTimerButton.TIMER_STEP;
        
        _TimerText.text = Time.ToString();
        _TimerText.gameObject.SetActive(true);

        while (CurrentTime > 0)
        {
            yield return new WaitForSeconds(TimerStep);
            CurrentTime -= TimerStep;
            _TimerText.text = Math.Round(CurrentTime,1).ToString();
        }

        _TimerText.text = "0";
        _TimerText.gameObject.SetActive(false);
    }
}
