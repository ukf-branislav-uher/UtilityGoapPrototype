using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class noThrow : noChase
{
    private GameObject grenadePrefab;
    public GameObject player;
    public ResourcesControl resources;
    
    public noThrow(
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
        ResourcesControl resources,
        GameObject player,
        GameObject grenadePrefab) : base(name, preDuration, postDuration, travelDuration, constantDurationPenalty, minDistanceToActivateTarget, targetTag,
        planner, npc, initialPreconditions, initialEffects, agent)
    {
        this.resources = resources;
        this.player = player;
        this.grenadePrefab = grenadePrefab;
    }
    
    public override void Perform()
    {
        base.Perform();
        if(resources.Throw())
        {
            npc.transform.LookAt(Planner.lastKnownPos);
            var grenade = GameObject.Instantiate(grenadePrefab, npc.transform.position + new Vector3(0f, 3f), Quaternion.identity).GetComponent<Grenade>();
            grenade.Throw(Planner.lastKnownPos, npc);
        }
    }

    public override void Update()
    {}
}
