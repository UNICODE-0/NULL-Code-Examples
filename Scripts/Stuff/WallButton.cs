using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallButton : MonoBehaviour, IRestartable
{
    [SerializeField] private Animator _Animator;
    [SerializeField] private DoorController _DoorController;

    private bool _IsPushed = false;

    private void OnTriggerEnter(Collider other) 
    {
        if(_IsPushed) return;

        if(other.tag == "Robot")
        {
            Push();
            _IsPushed = true;
        } 
    }

    private void Awake() 
    {
        LevelManager.restartableObjects.Add(this);
    }
    public virtual void Push()
    {
        _DoorController.OpenDoor();
        _Animator.SetTrigger("Down");
    }

    public void Reset()
    {
        _DoorController.CloseDoor();
        _Animator.SetTrigger("Up");
    }

    public virtual void SetInitialState()
    {
        if(_IsPushed) _Animator.SetTrigger("Up");
        _IsPushed = false;
    }
}
