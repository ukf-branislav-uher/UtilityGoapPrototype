using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : AIAction
{
    private List<Transform> waypoints;
    protected int currentWaypointIndex = 0;
    
    public Patrol() : base() {}

    private void Awake()
    {
        base.Awake();
        waypoints = transform.parent.parent.Find("Network").GetComponent<AIWaypointNetwork>().Waypoints;
    }

    private GameObject GetNextWaypoint()
    {
        var originalAction = this.transform.parent.parent.Find("actions").Find("Patrol(Clone)").GetComponent<Patrol>();
        this.currentWaypointIndex = originalAction.currentWaypointIndex;
        originalAction.currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        return waypoints[currentWaypointIndex].gameObject;
    }

    public override GameObject FindNearestTarget(GameObject @from)
    {
        return GetNextWaypoint();
    }

    public override void CalculateCostNI()
    {
        // Debug.Log("total Patrol cost: " + totalCost);
        // base.CalculateCostNI();
        // totalCost = 10;
        // totalCost += Vector3.Distance(waypoints[currentWaypointIndex].transform.position, target.transform.position) / agent.speed;
    }

    public override void Perform()
    {
        base.Perform();
        Debug.Log("performing Patrol, index " + currentWaypointIndex);
        Debug.Log(target);
        
        // this.target = GetNextWaypoint();
        
    }
}
