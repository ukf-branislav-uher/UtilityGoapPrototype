using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct  AIDict
{
    public string key;
    public int val;
}
public class AINode : MonoBehaviour
{
    public string name = "MyNode";
    public HashSet<AIAction> children = new HashSet<AIAction>();
    public HashSet<AINode> parents = new HashSet<AINode>();

    public List<Consideration> Considerations = new List<Consideration>();
    
    // atributy pre Dijsktrov algoritmus
    public float totalCost = 0;
    [HideInInspector] public float pathCost = int.MaxValue;
    [HideInInspector] public AINode previousOnPath;
    
    protected AIPlanner Planner;

    public List<AIDict> initialPreconditions = new List<AIDict>();
    public Dictionary<string, int> preconditions = new Dictionary<string, int>();

    public void Awake()
    {
        name = gameObject.name;
        Planner = gameObject.GetComponentInParent<AIPlanner>();
        //unity editor nevie zobrazit slovniky/mapy, preto mame zoznam vlastnych struktur
        foreach (var precondition in initialPreconditions)
        {
            preconditions.Add(precondition.key, precondition.val);
        }
    }
    
    public void CalculateUtilityAsCost(float timeToComplete)
    {
        // NIKDY NEVOLAT Z AKCII
        // Debug.Log("calling utility");
        totalCost = timeToComplete;
        // tu vezmeme vsetky considerations vrtatane casu na vykonanie ciela, kazdemu vypocitame hodnotu z krivky a zlucime
    }
    
    public void ResetBeforeGeneratingGraph()
    {
        children = new HashSet<AIAction>();
        // TODO: vyhodit vsetky deti
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }
    
    public void ResetBeforeSearchingGraph()
    {
        pathCost = int.MaxValue;
        previousOnPath = null;
    }

    // musime zmenit Equals a GetHashCode aby sme vedeli pouzit HashSety
    public override bool Equals(object obj)
    {
        try
        {
            if (obj is AINode)
            {
                AINode other = obj as AINode;
                return GetHashCode() == other.GetHashCode();
            }

            if (obj == null) return false;
            
            return base.Equals(obj);
        }
        catch (NullReferenceException e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public override int GetHashCode()
    {
        return name.GetHashCode();
    }
}
