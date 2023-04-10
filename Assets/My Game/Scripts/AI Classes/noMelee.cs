using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class noMelee : noAction
{
    private GameObject player;
    private Animator anim;
    public noMelee(
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
        GameObject player,
        Animator anim) : base(name, preDuration, postDuration, travelDuration, constantDurationPenalty, minDistanceToActivateTarget, targetTag,
        planner, npc, initialPreconditions, initialEffects, agent)
    {
        this.player = player;
        this.anim = anim;
    }

    public override void Perform()
    {
        base.Perform();
        Vector3 directionToTarget = (player.transform.position - npc.transform.position).normalized;
        float distanceToTarget = Vector3.Distance(npc.transform.position, player.transform.position);
        RaycastHit hit;
        if (Physics.Raycast(npc.transform.position, directionToTarget, out hit, distanceToTarget))
        {
            if (hit.transform.gameObject.tag == "Target")
            {
                player.GetComponent<CameraShake>().ShakeIt();
            }
        }
    }
}
