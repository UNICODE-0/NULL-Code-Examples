using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAnimation : MonoBehaviour
{
    [SerializeField] private Transform _RotationAnchor;
    [SerializeField] private float _RotationSpeed = 10;
    private void Update() 
    {
        transform.RotateAround(_RotationAnchor.position, Vector3.up, _RotationSpeed * Time.deltaTime);
    }
}
