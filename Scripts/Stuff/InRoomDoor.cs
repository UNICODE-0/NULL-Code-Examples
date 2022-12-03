using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InRoomDoor : MonoBehaviour, IOpenable, IRestartable
{
    [SerializeField] private float _DoorsSpeed = 1f;
    [SerializeField] private bool _DefaultState = true;
    [Space]
    [SerializeField] private Transform _LeftDoor;
    [SerializeField] private Transform _LeftDoorOpenTarget;
    [SerializeField] private Transform _LeftDoorCloseTarget;
    [SerializeField] private Transform _RightDoor;
    [SerializeField] private Transform _RightDoorOpenTarget;
    [SerializeField] private Transform _RightDoorCloseTarget;
    
    private bool _IsDoorSpaceClear = true;
    private Coroutine _DoorOpenCoroutine;
    private Coroutine _DoorCloseCoroutine;

    public DoorState State { get; set; }
    private void OnTriggerStay(Collider other) 
    {
        if(other.tag == "Robot")
        {
            _IsDoorSpaceClear = false;
        }
    }
    private void OnTriggerExit(Collider other) 
    {
        if(other.tag == "Robot")
        {
            _IsDoorSpaceClear = true;
        }
    }
    private void Awake() 
    {
        LevelManager.restartableObjects.Add(this);

        SetDefaultDoorState();
    }

    private void Start() 
    {
        SetDoorsState(_DefaultState);
    }
    private void SetDefaultDoorState()
    {
        if(_DefaultState) State = DoorState.Open;
        else State = DoorState.Close;
    }
    public void SetInitialState()
    {
        if(_DoorCloseCoroutine is not null) StopCoroutine(_DoorCloseCoroutine);
        if(_DoorOpenCoroutine is not null) StopCoroutine(_DoorOpenCoroutine);

        SetDefaultDoorState();
        SetDoorsState(_DefaultState);
    }
    private void SetDoorsState(bool State)
    {
        if(State)
        {
            _LeftDoor.position = _LeftDoorOpenTarget.position;
            _RightDoor.position = _RightDoorOpenTarget.position;
        } else
        {
            _LeftDoor.position = _LeftDoorCloseTarget.position;
            _RightDoor.position = _RightDoorCloseTarget.position;
        }
    }
    public void Open()
    {
        if(State == DoorState.Close)
        {
            State = DoorState.Open;
            _DoorOpenCoroutine = StartCoroutine(OpenDoors());
        }
    }
    public void Close()
    {
        if(State == DoorState.Open)
        {
            State = DoorState.Close;
            _DoorCloseCoroutine = StartCoroutine(CloseDoors());
        }
    }
    private IEnumerator CloseDoors()
    {
        while (Vector3.Distance(_LeftDoor.position, _LeftDoorCloseTarget.position) > 0 && Vector3.Distance(_RightDoor.position, _RightDoorCloseTarget.position) > 0)
        {
            yield return new WaitForFixedUpdate();
            if(_IsDoorSpaceClear)
            {
                _LeftDoor.position = Vector3.MoveTowards(_LeftDoor.position, _LeftDoorCloseTarget.position, _DoorsSpeed * Time.deltaTime);
                _RightDoor.position = Vector3.MoveTowards(_RightDoor.position, _RightDoorCloseTarget.position, _DoorsSpeed * Time.deltaTime);
            }
        }
    }
    private IEnumerator OpenDoors()
    {
        while (Vector3.Distance(_LeftDoor.position, _LeftDoorOpenTarget.position) > 0 && Vector3.Distance(_RightDoor.position, _RightDoorOpenTarget.position) > 0)
        {
            yield return new WaitForFixedUpdate();
            _LeftDoor.position = Vector3.MoveTowards(_LeftDoor.position, _LeftDoorOpenTarget.position, _DoorsSpeed * Time.deltaTime);
            _RightDoor.position = Vector3.MoveTowards(_RightDoor.position, _RightDoorOpenTarget.position, _DoorsSpeed * Time.deltaTime);
        }
    }
    
    public enum DoorState
    {
        Open,
        Close
    }
}
