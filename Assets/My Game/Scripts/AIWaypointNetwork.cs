using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PathDisplayMode {None, Connections, Paths }
public class AIWaypointNetwork : MonoBehaviour
{
    [HideInInspector]
    public PathDisplayMode PathDisplayMode = PathDisplayMode.Connections;
    [HideInInspector]
    public int UIStart;
    [HideInInspector]
    public int UIEnd;
    public List<Transform> Waypoints = new List<Transform>();
}

