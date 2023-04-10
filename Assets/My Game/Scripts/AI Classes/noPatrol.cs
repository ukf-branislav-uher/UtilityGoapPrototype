using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class noPatrol : noAction
{
    private List<Transform> waypoints;

    public noPatrol(
        string name,
        float preDuration,
        float postDuration,
        float travelDuration,
        float constantDurationPenalty,
        float minDistanceToActivateTarget,
        string targetTag,
        AIPlanner planner,
        GameObject npc,
        List<AIDict> initialPreconditions,
        List<AIDict> initialEffects,
        NavMeshAgent agent,
        List<Transform> waypoints) : base(name, preDuration, postDuration, travelDuration, constantDurationPenalty, minDistanceToActivateTarget, targetTag, planner,
        npc, initialPreconditions, initialEffects, agent)
    {
        this.waypoints = waypoints;
    }
    
    public noPatrol() : base() {}

    public override GameObject FindNearestTarget(GameObject @from)
    {
        target = waypoints[Planner.currentWaypointIndex].gameObject;
        return target;
    }

    public override void CalculateCostNI()
    {
        base.CalculateCostNI();
        totalCost = 0.2f;
    }

    public override void OnDestinationArrival()
    {
        base.OnDestinationArrival();
        Planner.currentWaypointIndex = (Planner.currentWaypointIndex + 1) % waypoints.Count;
    }
}
