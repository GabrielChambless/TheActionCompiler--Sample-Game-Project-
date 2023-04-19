using System;
using UnityEngine;

public class MenuMonitorController : MonoBehaviour
{
    public static Action MatchMenuMonitor;

    private void OnEnable()
    {
        MatchMenuMonitor += MatchCameraPositionAndRotation;
    }

    private void OnDisable()
    {
        MatchMenuMonitor -= MatchCameraPositionAndRotation;
    }

    private void MatchCameraPositionAndRotation()
    {
        transform.position = new Vector3(transform.position.x, 17.3f, transform.position.z);
    }
}
