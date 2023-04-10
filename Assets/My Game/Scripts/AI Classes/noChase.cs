using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class noChase : noAction
{
    public Transform targetParent;

    public noChase(
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

    public override void StartAction()
    {
        targetParent = GameObject.Find("Targets").transform;
        base.StartAction();
    }

    public override GameObject FindNearestTarget(GameObject @from)
    {
        target = from;
        if(Planner.lastKnownPos.magnitude != Vector3.negativeInfinity.magnitude)
        {
            var targetGO = new GameObject();
            targetGO.transform.parent = targetParent;
            targetGO.transform.position = new Vector3(Planner.lastKnownPos.x, 1, Planner.lastKnownPos.z);
            GameObject.Destroy(targetGO, 15);
            target = targetGO;
        }
        return target;
    }
    
    private bool isLookingAround = false;
    public override void Perform()
    {
        base.Perform();
        
        if (!isLookingAround)
        {
            isLookingAround = true;
        }
    }
    
    public override void Update()
    {
        agent.SetDestination(Planner.lastKnownPos);
    }
}
