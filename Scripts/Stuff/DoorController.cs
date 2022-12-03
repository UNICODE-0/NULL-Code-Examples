using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour, IRestartable
{
    [SerializeField] protected MeshRenderer _MeshRnd;
    [SerializeField] private Color _OpenColor;
    [SerializeField] protected Color _CloseColor;
    [SerializeField] private bool _DefaultState = false;
    [SerializeField] private MonoBehaviour _OpenTarget;

    private IOpenable _OpenableObject;
    private void Awake()
    {
        LevelManager.restartableObjects.Add(this);

        if(_MeshRnd.materials.Length < 2)
        {
            Debug.LogError("DoorComtroller doesn't have the right amount of materials.");
        }
        if(_OpenTarget is not IOpenable)
        {
            Debug.LogError("OpenTarget must implement the IOpenable interface.");
        } else _OpenableObject = _OpenTarget as IOpenable;
    }
    private void Start() 
    {
        SetDefaultColor();
    }
    protected void SetDefaultColor()
    {
        if(_DefaultState) _MeshRnd.materials[1].color = _OpenColor;
        else _MeshRnd.materials[1].color = _CloseColor;
    }
    public virtual void OpenDoor()
    {
        _MeshRnd.materials[1].color = _OpenColor;
        _OpenableObject.Open();
    }
    public virtual void CloseDoor()
    {
        _MeshRnd.materials[1].color = _CloseColor;
        _OpenableObject.Close();
    }

    public virtual void SetInitialState()
    {
        SetDefaultColor();
    }
}
