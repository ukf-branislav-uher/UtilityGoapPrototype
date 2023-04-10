using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AIAction : AINode
{
    public List<AIDict>
        initialEffects = new List<AIDict>(); // TO DO: spravit z toho zoznam stringov, lebo value sa nikde nepouziva (ak sa nevyuzije vo world state that is)

    public Dictionary<string, int> effects = new Dictionary<string, int>();

    public string targetTag = "";

    public float minDistanceToActivateTarget = 1;

    private float _score;
    
    public float preDuration; //dlzka vykonavania akcie
    public float postDuration;
    public float travelDuration;
    public float constantDurationPenalty;

    public float score
    {
        get { return _score; }
        set { this._score = Mathf.Clamp01(value); }
    }

    // public ScoringCriteria[] criteria; // kriteria podla ktorych ohodnocujeme akcie

    public GameObject target = null;

    public NavMeshAgent agent;
    // public string targetLayer;
    protected void Awake()
    {
        base.Awake();
        //unity editor nevie zobrazit slovniky/mapy, preto mame zoznam vlastnych struktur
        foreach (var effect in initialEffects)
        {
            effects.Add(effect.key, effect.val);
        }

        agent = GetComponentInParent<NavMeshAgent>();
    }

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

        // if (target != null)
        //     agent.destination = target.transform.position;
        //
        // return target != null;
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

    public void FinishAction()
    {
        isRunning = false;
        Planner.FinishActionInvoked();
    }

    public bool isRunning;
    public bool isAborted;

    public virtual void Perform()
    {
        // FinishAction();
        Invoke("FinishAction", postDuration);
        // Debug.Log("performing " + name);
    } // vrati false ak abortnute

    public virtual void CalculateCostNI() // NI znamena neouzitelne inde - toto treba volat presne tam kde sa to vola, je to len pre prehladnost
    {
        if (children?.Count > 1)
        {
            Debug.Log("PRIVELA CHILDOV, VOLAT AZ PO UPRAVENI GRAFU!!!");
            return;
        }

        /*
         * ak sme v liste, najdi target listu a vzdialenost k targetu
         * ak sme v inej akcii, najdi najblizsi target od pozicie childu a vzdialenost
         */

        GameObject from;
        if (children?.Count == 0)
        {
            from = gameObject; // nastavi GO ako target, ak sa nenajde nic ine
            // } else if (targetTag == "" && children?.ToList()[0]?.children?.Count == 0)
        }
        else
        {
            from = children?.ToList()[0].target;
        }
        
        FindNearestTarget(from);
        travelDuration = Vector3.Distance(from.transform.position, target.transform.position) / agent.speed;

        totalCost = postDuration + preDuration + travelDuration + constantDurationPenalty;
        // vychadza zo vzdialenosti medzi targetom this a predosleho objektu (childu). Ak target je null, asi mu chceme nastavit seba, aby sme nemuseli viac riesit

        // vezmeme vzdialenost, vypocitame z agentovho speedu cas, pridame k ostatnym casom + cost = celkova cena
    }

    public float ReturnCost()
    {
        return postDuration + preDuration + travelDuration + constantDurationPenalty;
    }
    
    /* public bool IsAchievableGiven(Dictionary<string, int> conditions)
     {
         foreach (KeyValuePair<string, int> p in preconditionsDic)
         {
             if (!conditions.ContainsKey(p.Key))
                 return false;
         }
    
         return true;
     }*/
}
