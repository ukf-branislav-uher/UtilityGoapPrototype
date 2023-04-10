using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class noShoot : noAction
{
    public ResourcesControl resources;
    
    public GameObject bulletEffect;

    public GameObject player;

    public float hitProb;

    public noShoot(
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
        GameObject bulletEffect,
        float hitProb) : base(name, preDuration, postDuration, travelDuration, constantDurationPenalty, minDistanceToActivateTarget, targetTag,
        planner, npc, initialPreconditions, initialEffects, agent)
    {
        this.resources = resources;
        this.bulletEffect = bulletEffect;
        this.player = player;
        this.hitProb = hitProb;
    }
    
    public override void Perform()
    {
        base.Perform();
        if (resources.Shoot())
        {
            npc.transform.LookAt(player.transform.position);
            // instanciuj particle efekt
            var particle = GameObject.Instantiate(bulletEffect, npc.transform.position + npc.transform.forward + (npc.transform.up * 1.6f) + (npc.transform.right * 0.2f), npc.transform.rotation).GetComponent<ParticleSystem>();
            particle.Play();
            GameObject.Destroy(particle.gameObject, 1);
            // raycast na hraca, ak trafime, tak zobrat hracovi HP + sreenshake
            float randomHitChance = Random.value;
            if(randomHitChance > hitProb)
            {
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
    }
}
