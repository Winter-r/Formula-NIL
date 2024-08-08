using System.Collections.Generic;
using UnityEngine;

public class WaypointContainer : MonoBehaviour
{
    public List<Transform> waypoints = new();

    private void Awake()
    {
        foreach (Transform waypoint in gameObject.GetComponentsInChildren<Transform>())
        {
            if (waypoint != this.transform)
            {
                waypoints.Add(waypoint);
            }
        }
    }
}
