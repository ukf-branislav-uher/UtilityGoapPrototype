using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class noAwarenessControl : MonoBehaviour
{
    // vyuzijeme vision a hearing, aby sme kontinualne ziskavali informacie o hracovom pohybe. 
    // hearing vzdy resultne v Aware
    // vision vzdy resultne v Aware a LoS
    // Chase sa spusti, ked sme aware, ale nemame LoS
    // tento manager iba setuje Planneru spravne parametre, aby ten vedel, ze moze spustat niektore ciele

    public AIPlanner Planner;

    public HashSet<Transform> previousTargets = new HashSet<Transform>();

    public float awareTime = 5;

    public GameObject player;
    
    public Dictionary<Vision, HashSet<Transform>> visuals = new Dictionary<Vision, HashSet<Transform>>();

    public void ReportSawSomething(Vision vision, HashSet<Transform> targets)
    {
        if (targets.Count == 0)
        {
            visuals.Remove(vision);
        }
        else if (!visuals.ContainsKey(vision))
        {
            visuals.Add(vision, targets);
        }

        if (visuals.Count == 0)
        {
            Planner.WorldState.Remove("los");
        }
        else if(visuals.ContainsKey(vision))
        {
            StopCoroutine("DecayAwareness");
            Planner.WorldState.Add("los");
            Planner.lastKnownPos = targets.ToList()[0].position;
            StartCoroutine("DecayAwareness");
        }
    }

    public void ReportHeardSound(GameObject source, Vector3 location, ESounds type, float intensity)
    {
        StopCoroutine("DecayAwareness");
        Planner.lastKnownPos = source.transform.position;
        StartCoroutine("DecayAwareness");
    }

    IEnumerator DecayAwareness()
    {
        yield return new WaitForSeconds(awareTime);
        Planner.lastKnownPos = Vector3.negativeInfinity;
    }

    private void Start()
    {
        Planner = GetComponent<AIPlanner>();
        player = GameObject.FindGameObjectWithTag("Player");
    }
}
