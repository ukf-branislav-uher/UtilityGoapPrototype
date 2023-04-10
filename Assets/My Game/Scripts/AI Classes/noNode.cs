using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public enum EConsideration
{
    Time,
    Ammo,
    Grenades,
    Aware,
    LoS,
}
[System.Serializable]
public struct  Consideration
{
    public EConsideration originalValue;
    public AnimationCurve curve;
    public float minValue;
    public float maxValue;
}
public class noNode
{
    public string name = "MyNode";
    public HashSet<noAction> children = new HashSet<noAction>();
    public HashSet<noNode> parents = new HashSet<noNode>();
    
    public float totalCost = 0;
    public float pathCost = int.MaxValue;
    public noNode previousOnPath;
    
    protected AIPlanner Planner;
    public GameObject npc;

    public List<AIDict> initialPreconditions = new List<AIDict>();
    public HashSet<string> preconditions = new HashSet<string>();

    public List<Consideration> Considerations;

    public noNode(string name, AIPlanner planner, GameObject npc, List<AIDict> initialPreconditions, List<Consideration> considerations = null)
    {
        this.name = name;
        this.Planner = planner;
        this.npc = npc;
        this.initialPreconditions = initialPreconditions;
        this.Considerations = considerations == null ? new List<Consideration>() : considerations;
        
        foreach (var precondition in initialPreconditions)
        {
            preconditions.Add(precondition.key);
        }
    }
    
    public noNode() {}
    
    public void CalculateUtilityAsCost(float timeToComplete, bool completable, HashSet<string> worldState)
    {
        float score = 1;
        foreach (var c in Considerations)
        {
            score *= GetConsiderationValue(c, timeToComplete, completable, worldState);
        }

        totalCost = score + score*(1-score)*(1-1/Considerations.Count);
    }

    public float GetConsiderationValue(Consideration c, float timeToComplete, bool completable, HashSet<string> worldState)
    {
        float value = 0;
        if (c.originalValue == EConsideration.Time)
            value = timeToComplete;
        if (c.originalValue == EConsideration.Ammo)
        {
            value = npc.GetComponent<ResourcesControl>().currentAmmoInMag;
        }
        if (c.originalValue == EConsideration.Grenades)
        {
            value = npc.GetComponent<ResourcesControl>().currentGrenades;
        }
        if (c.originalValue == EConsideration.Aware)
            value = worldState.Contains("aware") ? 1 : 0;
        if (c.originalValue == EConsideration.LoS)
            value = worldState.Contains("los") ? 1 : 0;

        value = Mathf.Clamp(value, c.minValue, c.maxValue);
        value = (value - c.minValue) / (c.maxValue - c.minValue);
        return completable ? c.curve.Evaluate(value) : 0;
    }
    
    public void ResetBeforeGeneratingGraph()
    {
        children = new HashSet<noAction>();
        parents = new HashSet<noNode>();
    }
    
    public void ResetBeforeSearchingGraph()
    {
        pathCost = int.MaxValue;
        previousOnPath = null;
    }
}
