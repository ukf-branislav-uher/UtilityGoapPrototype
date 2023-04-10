using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class noAction : noNode
{
    public List<AIDict> initialEffects = new List<AIDict>();
    public HashSet<string> effects = new HashSet<string>();

    public string targetTag = "";

    public float minDistanceToActivateTarget = 1;
    
    public float preDuration;
    public float postDuration;
    public float travelDuration;
    public float constantDurationPenalty;
    
    public GameObject target = null;

    public NavMeshAgent agent;
    
    public noAction(
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
        NavMeshAgent agent) : base(name, planner, npc, initialPreconditions)
    {
        this.agent = agent;
        this.preDuration = preDuration;
        this.postDuration = postDuration;
        this.travelDuration = travelDuration;
        this.constantDurationPenalty = constantDurationPenalty;
        this.minDistanceToActivateTarget = minDistanceToActivateTarget;
        this.targetTag = targetTag;
        this.initialEffects = initialEffects;
        
        foreach (var effect in initialEffects)
        {
            effects.Add(effect.key);
        }
    }
    
    public noAction() {}
    
    public virtual GameObject FindNearestTarget(GameObject from)
    {
        var possibleTargets = new GameObject[] {};
        if (targetTag != "") possibleTargets = GameObject.FindGameObjectsWithTag(targetTag); // optimalizacia: vykonat raz v starte a cachenut
        target = from;

        float minDistance = float.MaxValue;
        foreach (var possibleTarget in possibleTargets)
        {
            if (possibleTarget == null)
                continue;
            float distanceToCurrentPossibleTarget = CalculateMinDistanceTo(from.transform.position, possibleTarget.transform.position);
            if (distanceToCurrentPossibleTarget < minDistance)
            {
                target = possibleTarget;
                minDistance = distanceToCurrentPossibleTarget;
            }
        }

        return target;
    }
    
    private float CalculateMinDistanceTo(Vector3 start, Vector3 destination)
    {
        float distance = 0f;
        NavMeshPath pathToCurrentPossibleTarget = new NavMeshPath();
        NavMesh.CalculatePath(start, destination, agent.areaMask, pathToCurrentPossibleTarget);
        if (pathToCurrentPossibleTarget.status == NavMeshPathStatus.PathComplete)
        {
            for (var i = 1; i < pathToCurrentPossibleTarget.corners.Length; i++)
            {
                distance += Vector3.Distance(pathToCurrentPossibleTarget.corners[i - 1], pathToCurrentPossibleTarget.corners[i]);
            }
        }
        else
        {
            distance = float.MaxValue;
        }

        return distance;
    }
    
    public virtual void StartAction()
    {
        isRunning = true;
    }

    public virtual void FinishAction()
    {
        isRunning = false;
        Planner.FinishActionInvoked();
        
        agent.destination = npc.transform.position;
    }

    public bool isRunning;
    public bool isAborted;

    public virtual void OnDestinationArrival()
    {
        
    }

    public virtual void Perform()
    {
        int delay = (int) postDuration * 1000;
        Task.Delay(delay).ContinueWith(t => FinishAction());
    }

    public virtual void CalculateCostNI() // NI znamena nepouzitelne inde - treba volat presne tam kde sa to vola
    {
        if (children?.Count > 1)
        {
            return;
        }

        /*
         * ak sme v liste, najdi target listu a vzdialenost k targetu
         * ak sme v inej akcii, najdi najblizsi target od pozicie childu a vzdialenost
         */

        GameObject from;
        if (children?.Count == 0)
        {
            from = npc; // nastavi GO ako target, ak sa nenajde nic ine
        }
        else
        {
            from = children?.ToList()[0].target;
        }
        
        FindNearestTarget(from);
        travelDuration = Vector3.Distance(from.transform.position, target.transform.position) / agent.speed;

        totalCost = postDuration + preDuration + travelDuration + constantDurationPenalty;
    }

    public float ReturnCost()
    {
        return postDuration + preDuration + travelDuration + constantDurationPenalty;
    }
    
    public virtual void Update(){}
}
