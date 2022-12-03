using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallTimerButton : WallButton
{
    [SerializeField] private float _Timer = 15;

    public const float TIMER_STEP = 0.1F;
    
    private Coroutine _TimerCoroutine;
    public override void Push()
    {
        base.Push();
        StartTime(_Timer);
    }
    private void StartTime(float Time)
    {
        _TimerCoroutine = StartCoroutine(Timer(Time));
    }
    IEnumerator Timer(float Time)
    {
        float CurrentTime = 0;
        while (CurrentTime < Time)
        {
            yield return new WaitForSeconds(TIMER_STEP);
            CurrentTime += TIMER_STEP;
        }
        Reset();
    }
    public override void SetInitialState()
    {
        base.SetInitialState();
        if(_TimerCoroutine is not null) StopCoroutine(_TimerCoroutine);
    }
}
