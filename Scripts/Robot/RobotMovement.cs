using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RobotMovement : MonoBehaviour, IRestartable
{
    [SerializeField] private GameObject _CenterOfMass;
    [SerializeField] private Transform _RaycastOrigin;
    [SerializeField] private float _RaycastDistance;
    [SerializeField] private LayerMask _RaycastLayer;

    private const float MIN_SLOPE = 0.5f; // Scalar value

    private Vector3 _StartPosition;
    private Quaternion _StartRotation;

    private Rigidbody _Rgbd;

    private float _CurrentYRotattion;
    private Vector3 _CurrentPosition;

    private bool _IsRotatingAroundY = false;
    private float _TargetYRot = 0;

    private Vector3 _RightRaycast;
    private Vector3 _LeftRaycast;
    private Vector3 _ForwardRaycast;
    private Vector3 _BackwardRaycast;

    private bool _IsMoving = false;
    private float _MovementSpeed = 0f;
    private void Awake() 
    {
        RobotManager.movement = this;
        LevelManager.restartableObjects.Add(this);
    }
    private void Start()
    {
        _Rgbd = GetComponent<Rigidbody>();
        _Rgbd.centerOfMass = _CenterOfMass.transform.localPosition;

        _StartPosition = transform.position;
        _StartRotation = transform.rotation;
    }
    private void FixedUpdate()
    {
        _CurrentYRotattion = transform.rotation.eulerAngles.y;
        _CurrentPosition = transform.position;

        RaycastHit hit; 
        Ray ray = new Ray(_RaycastOrigin.position, _RaycastOrigin.right);
        if(Physics.Raycast(ray, out hit, _RaycastDistance, _RaycastLayer)) _RightRaycast = hit.point;
        ray.direction = -_RaycastOrigin.right;
        if(Physics.Raycast(ray, out hit, _RaycastDistance, _RaycastLayer)) _LeftRaycast = hit.point;
        ray.direction = _RaycastOrigin.forward;
        if(Physics.Raycast(ray, out hit, _RaycastDistance, _RaycastLayer)) _ForwardRaycast = hit.point;
        ray.direction = -_RaycastOrigin.forward;
        if(Physics.Raycast(ray, out hit, _RaycastDistance, _RaycastLayer)) _BackwardRaycast = hit.point;

        bool IsNotFell = Vector3.Dot(transform.up,Vector3.up) > MIN_SLOPE;

        if(_IsMoving)
        {
            if(IsNotFell) 
            {
                _Rgbd.MovePosition(transform.position + transform.forward * Time.deltaTime * _MovementSpeed);
            }

            _IsMoving = false;
            _MovementSpeed = 0;
        } 

        if(_IsRotatingAroundY)
        {
            if(IsNotFell) 
            {
                Quaternion TargetQuat = Quaternion.Euler(0, _TargetYRot * Time.fixedDeltaTime, 0);
                _Rgbd.MoveRotation(transform.rotation * TargetQuat);
            }

            _IsRotatingAroundY = false;
            _TargetYRot = 0;
        }
    }
    public void Move(float Speed)
    {
        _IsMoving = true;
        _MovementSpeed += Speed;

        _MovementSpeed = Mathf.Clamp(_MovementSpeed,-2,2); 
    }
    public void RotateAroundY(float Deg)
    {
        _IsRotatingAroundY = true;
        _TargetYRot += Deg;

        _TargetYRot = Mathf.Clamp(_TargetYRot,-100,100); 

    }
    public float GetYRotation()
    {
        return _CurrentYRotattion;
    }
    public float Raycast(int Side, int Axis)
    {
        float GetPosFromAxis(Vector3 Vec, int VecAxis)
        {
            switch (VecAxis)
            {
                case 1:
                return Vec.x;
                case 2:
                return Vec.y;
                case 3:
                return Vec.z;
                default:
                return Vec.z;
            }
        }

        switch (Side)
        {
            case 1:
            return GetPosFromAxis(_ForwardRaycast,Axis);
            case 2:
            return GetPosFromAxis(_BackwardRaycast,Axis);
            case 3:
            return GetPosFromAxis(_RightRaycast,Axis);
            case 4:
            return GetPosFromAxis(_LeftRaycast,Axis);
            default:
            return GetPosFromAxis(_LeftRaycast,Axis);
        }
    }
    public float GetPosition(int Axis)
    {
        switch (Axis)
        {
            case 1:
            return _CurrentPosition.x;
            case 2:
            return _CurrentPosition.y;
            case 3:
            return _CurrentPosition.z;
            default:
            return _CurrentPosition.z;
        }
    }
    public void ResetBehaviour()
    {
        _MovementSpeed = 0f;
        _TargetYRot = 0f;

        _IsMoving = false;
        _IsRotatingAroundY = false;
    }

    public void SetInitialState()
    {
        transform.position = _StartPosition;
        transform.rotation = _StartRotation;

        _Rgbd.velocity = Vector3.zero;
        _Rgbd.angularVelocity = Vector3.zero;
    }
}
