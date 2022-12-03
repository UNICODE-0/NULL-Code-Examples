using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameAudioController : MenuAudioController
{
    [SerializeField] private AudioSource _RobotMovementSound;
    [SerializeField] private AudioSource _ElevatorDoorsSound;
    private void Awake() 
    {
        LevelManager.audioController = this;
    }
    public void PlayElevatorDoorsSound()
    {
        _ElevatorDoorsSound.Play();
    }
    public void PlayRobotMovementSound()
    {
        _RobotMovementSound.Play();
    }
    public void StopRobotMovementSound()
    {
        _RobotMovementSound.Stop();
    }
}
