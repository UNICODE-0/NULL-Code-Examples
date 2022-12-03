using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotManager : MonoBehaviour
{
    public static RobotMovement movement { get; set; }

    public static void ResetAll()
    {
        movement.ResetBehaviour();
    }
    private void Start() 
    {
        if(movement is null) Debug.LogError("RobotManager is not initialized.");
    }
}
