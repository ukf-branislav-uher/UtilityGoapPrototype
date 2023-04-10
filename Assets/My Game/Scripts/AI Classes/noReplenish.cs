using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class noReplenish : noAction
{
    public noReplenish(
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
        NavMeshAgent agent) : base(name, preDuration, postDuration, travelDuration, constantDurationPenalty, minDistanceToActivateTarget, targetTag,
        planner, npc, initialPreconditions, initialEffects, agent)
    {}

    public override void Perform()
    {
        base.Perform();
        npc.GetComponent<ResourcesControl>().Replenish();
    }
}
