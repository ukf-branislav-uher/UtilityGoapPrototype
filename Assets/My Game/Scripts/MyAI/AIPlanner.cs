using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class AIPlanner : MonoBehaviour
{
    public List<AIAction> possibleActionsObjs = new List<AIAction>();
    public List<AINode> possibleGoalsObjs = new List<AINode>();
    
    public List<noAction> possibleActions = new List<noAction>();
    public List<noNode> possibleGoals = new List<noNode>();
    
    [HideInInspector] public int currentWaypointIndex = 0;

    public noNode currentGoal;
    public noAction currentAction;

    private List<noAction> usedActions;
    
    public NavMeshAgent agent;
    public float planningCooldown = 1;

    private float positionDestinationYDelta = 0;
    private Vector3 positionDestinationOffsetVector;
    
    private bool actionInvoked = false;
    private noNode lastGoal = null;

    public GameObject player;
    public Vector3 lastKnownPos = Vector3.negativeInfinity;
    public Vector3 tempLKP = Vector3.negativeInfinity;

    public float lastKnownPosDist = float.MaxValue;

    public HashSet<string> WorldState = new HashSet<string>();

    private ResourcesControl resources;
    
    private Animator anim;

    public GameObject enemyParticleSystem;

    public float hitProb = 0.5f;

    public GameObject grenadePrefab;

    public GameObject chaser = null;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 24;
        agent = GetComponent<NavMeshAgent>();
        resources = GetComponent<ResourcesControl>();

        positionDestinationYDelta = transform.position.y - agent.destination.y;
        positionDestinationOffsetVector = new Vector3(0, positionDestinationYDelta, 0);
        
        anim = GetComponentInChildren<Animator>();

        #region Create goals and actions as parent GOs for possibleGoals and possibleActions

        var transformCached = transform;
        var positionCached = transformCached.position;
        var goalsObj = new GameObject("goals");
        goalsObj.transform.parent = transformCached;
        goalsObj.transform.position = positionCached;
        var actionsObj = new GameObject("actions");
        actionsObj.transform.parent = transformCached;
        actionsObj.transform.position = positionCached;
        usedActions = new List<noAction>();

        #endregion

        #region Instantiate goals and actions into their parent objects

        foreach (var goal in possibleGoalsObjs)
            possibleGoals.Add(InitializeGoalByNameAINode(goal));
        
        foreach (var action in possibleActionsObjs)
        {
            possibleActions.Add(InitializeActionByNameAIAction(action));
        }
        
        #endregion
    }

    private void SetAllAnimationsFalse()
    {
        anim.SetBool("jumping", false);
        anim.SetBool("attacking", false);
        anim.SetBool("walking", false);
    }
    
    public noNode InitializeGoalByNameAINode(AINode goal)
    {
        return new noNode(goal.name, this, gameObject, goal.initialPreconditions, goal.Considerations);
    }

    public noAction InitializeActionByNameAIAction(AIAction action)
    {
        if(action.gameObject.name == "Patrol")
            return new noPatrol(action.gameObject.name, action.preDuration, action.postDuration, action.travelDuration, action.constantDurationPenalty, 
                action.minDistanceToActivateTarget, action.targetTag, this, gameObject, action.initialPreconditions, action.initialEffects, agent, 
                transform.Find("Network").GetComponent<AIWaypointNetwork>().Waypoints);
        if(action.gameObject.name == "Chase")
            return new noChase(action.gameObject.name, action.preDuration, action.postDuration, action.travelDuration, action.constantDurationPenalty, 
                action.minDistanceToActivateTarget, action.targetTag, this, gameObject, action.initialPreconditions, action.initialEffects, agent);
        if(action.gameObject.name == "Shoot")
            return new noShoot(action.gameObject.name, action.preDuration, action.postDuration, action.travelDuration, action.constantDurationPenalty, 
                action.minDistanceToActivateTarget, action.targetTag, this, gameObject, action.initialPreconditions, action.initialEffects, agent,
                resources, player, enemyParticleSystem, hitProb);
        if(action.gameObject.name == "Melee")
            return new noMelee(action.gameObject.name, action.preDuration, action.postDuration, action.travelDuration, action.constantDurationPenalty, 
                action.minDistanceToActivateTarget, action.targetTag, this, gameObject, action.initialPreconditions, action.initialEffects, agent,
                player, anim);
        if(action.gameObject.name == "Reload")
            return new noReload(action.gameObject.name, action.preDuration, action.postDuration, action.travelDuration, action.constantDurationPenalty, 
                action.minDistanceToActivateTarget, action.targetTag, this, gameObject, action.initialPreconditions, action.initialEffects, agent);
        if(action.gameObject.name == "Replenish")
            return new noReplenish(action.gameObject.name, action.preDuration, action.postDuration, action.travelDuration, action.constantDurationPenalty, 
                action.minDistanceToActivateTarget, action.targetTag, this, gameObject, action.initialPreconditions, action.initialEffects, agent);
        if(action.gameObject.name == "Throw")
            return new noThrow(action.gameObject.name, action.preDuration, action.postDuration, action.travelDuration, action.constantDurationPenalty, 
                action.minDistanceToActivateTarget, action.targetTag, this, gameObject, action.initialPreconditions, action.initialEffects, agent, 
                resources, player, grenadePrefab);
            
        return new noAction(action.gameObject.name, action.preDuration, action.postDuration, action.travelDuration, action.constantDurationPenalty, 
            action.minDistanceToActivateTarget, action.targetTag, this, gameObject, action.initialPreconditions, action.initialEffects, agent);
    }

    public noAction CopyActionByNameNoAction(noAction action)
    {
        if(action.name == "Patrol")
            return new noPatrol(action.name, action.preDuration, action.postDuration, action.travelDuration, action.constantDurationPenalty, 
                action.minDistanceToActivateTarget, action.targetTag, this, gameObject, action.initialPreconditions, action.initialEffects, agent, 
                transform.Find("Network").GetComponent<AIWaypointNetwork>().Waypoints);
        if(action.name == "Chase")
            return new noChase(action.name, action.preDuration, action.postDuration, action.travelDuration, action.constantDurationPenalty, 
                action.minDistanceToActivateTarget, action.targetTag, this, gameObject, action.initialPreconditions, action.initialEffects, agent);
        if(action.name == "Shoot")
            return new noShoot(action.name, action.preDuration, action.postDuration, action.travelDuration, action.constantDurationPenalty, 
                action.minDistanceToActivateTarget, action.targetTag, this, gameObject, action.initialPreconditions, action.initialEffects, agent,
                resources, player, enemyParticleSystem, hitProb);
        if(action.name == "Melee")
            return new noMelee(action.name, action.preDuration, action.postDuration, action.travelDuration, action.constantDurationPenalty, 
                action.minDistanceToActivateTarget, action.targetTag, this, gameObject, action.initialPreconditions, action.initialEffects, agent,
                player, anim);
        if(action.name == "Reload")
            return new noReload(action.name, action.preDuration, action.postDuration, action.travelDuration, action.constantDurationPenalty, 
                action.minDistanceToActivateTarget, action.targetTag, this, gameObject, action.initialPreconditions, action.initialEffects, agent);
        if(action.name == "Replenish")
            return new noReplenish(action.name, action.preDuration, action.postDuration, action.travelDuration, action.constantDurationPenalty, 
                action.minDistanceToActivateTarget, action.targetTag, this, gameObject, action.initialPreconditions, action.initialEffects, agent);
        if (action.name == "Throw")
            return new noThrow(action.name, action.preDuration, action.postDuration, action.travelDuration, action.constantDurationPenalty,
                action.minDistanceToActivateTarget, action.targetTag, this, gameObject, action.initialPreconditions, action.initialEffects, agent,
                resources, player, grenadePrefab);

        return new noAction(action.name, action.preDuration, action.postDuration, action.travelDuration, action.constantDurationPenalty, 
            action.minDistanceToActivateTarget, action.targetTag, this, gameObject, action.initialPreconditions, action.initialEffects, agent);
    }

    private void Start()
    {
        Replan(false, false);
    }

    public void PrintGraph(noNode root)
    {
        bool notDoneYet = true;

        string aaa = "";

        HashSet<noNode> currentLevel = new HashSet<noNode>();
        currentLevel.Add(root);
        int depth = 0;

        while (notDoneYet)
        {
            HashSet<noNode> nextLevel = new HashSet<noNode>();
            foreach (var n in currentLevel)
            {
                aaa += " Depth " + depth + ", " + n.name + ", " + n.children.Count + " children:";
                n.children.ToList<noNode>().ForEach(x => aaa += x.name + ", ");
                aaa += "\n";

                nextLevel.UnionWith(n.children);
            }

            currentLevel = nextLevel;
            notDoneYet = depth++ <= 50 || nextLevel.Count != 0;
        }

        // Debug.Log(aaa);
    }

    public noNode GenerateActionGraph(
        noNode goal, //root
        List<noAction> possibleActions,
        ref List<noNode> allUsedNodes,
        ref List<noNode> leaves,
        HashSet<string> worldState)
    {
        List<noAction> usableActions = new List<noAction>();
        goal.ResetBeforeGeneratingGraph();
        
        foreach (var action in possibleActions) // resetuj a skopiruj kazdy possible akciu
        {
            if (chaser == gameObject)
            {
                usableActions.Add(action);
            }
            else
            {
                if (action.name.Contains("Chase"))
                {
                    noChase actionCopy = CopyActionByNameNoAction(action) as noChase;
                    actionCopy.minDistanceToActivateTarget = 10;
                    usableActions.Add(actionCopy);
                }
                else 
                if (action.name.Contains("Throw"))
                {
                    // npc ktore nie je chaser nebude hadzat granaty
                }
                else
                    usableActions.Add(action);
            }
            action.ResetBeforeGeneratingGraph();
        }

        HashSet<noNode> currentDepthNodes = new HashSet<noNode>();
        currentDepthNodes.Add(goal);
        bool allAvailableNodesProcessed = false;
        while (!allAvailableNodesProcessed)
        {
            HashSet<noNode> nextDepthNodes = new HashSet<noNode>();
            foreach (var currentNode in currentDepthNodes)
            {
                foreach (var precondition in currentNode.preconditions)
                {
                    IEnumerable<noAction> newChildrenEnumerable = usableActions.Where(a => a.effects.Contains(precondition));
                    HashSet<noAction> newChildrenSet = new HashSet<noAction>(from a in newChildrenEnumerable select a);
                    currentNode.children.UnionWith(newChildrenSet); // currentNode rozsirime children o novo najdene
                }
                foreach (var child in currentNode.children) // novo najdenym pridame currentNode ako parenta
                {
                    child.parents.Add(currentNode);
                }
                IEnumerable<noNode> leafChildrenEnumerable = currentNode.children.Where(n => n.preconditions.Count == 0 || worldState.Contains(n.preconditions.ToList()[0]));
                HashSet<noNode> currentNodeLeaves = new HashSet<noNode>(from n in leafChildrenEnumerable select n);
                leaves.AddRange(currentNodeLeaves);
                leaves = leaves.Distinct().ToList(); // malo by len zabezpecit, aby sa listy nevkladali viackrat

                HashSet<noNode> notLeafChildrenSet = new HashSet<noNode>(currentNode.children);
                notLeafChildrenSet.ExceptWith(currentNodeLeaves);
                nextDepthNodes.UnionWith(notLeafChildrenSet);
                if (!allUsedNodes.Contains(currentNode))
                    allUsedNodes.Add(currentNode);
            }
            currentDepthNodes = nextDepthNodes;
            allAvailableNodesProcessed = currentDepthNodes.Count == 0;
        }

        allUsedNodes.AddRange(leaves);
        return goal;
    }
    
    public void AlterGraphAndCalculateCosts(
        noNode root, // miesto porovnavania s referenciou goalu by asi uplne stacilo checkovat, ci to nie je nie-akction
        ref List<noNode> allUsedNodes,
        List<noNode> leaves
    )
    {
        var startingAgentPos = transform.position - positionDestinationOffsetVector; // zaciatocna pozicia NPC na navmeshi
        var previousAgentPos = startingAgentPos;

        HashSet<noNode> currentDepthNodes = new HashSet<noNode>(leaves);
        while (currentDepthNodes.Count > 0)
        {
            HashSet<noNode> nextDepthNodes = new HashSet<noNode>();
            foreach (var currentNode in currentDepthNodes)
            {
                nextDepthNodes.UnionWith(currentNode.parents);
                nextDepthNodes.ExceptWith(new HashSet<noNode>() {root});

                if(currentNode.children.Count > 1)
                {
                    var tempChildrenSet = new HashSet<noAction>(currentNode.children).ToList();
                    for (int i = 0; i < tempChildrenSet.Count; i++)
                    {
                        noAction currentNodeCopy;
                        if (i != 0)
                        {
                            var cAction = currentNode as noAction;
                            currentNodeCopy = CopyActionByNameNoAction(cAction);
                            allUsedNodes.Add(currentNodeCopy);
                            currentNodeCopy.parents = currentNode.parents;
                            foreach (var parent in currentNodeCopy.parents)
                                parent.children.UnionWith(new HashSet<noAction>(){currentNodeCopy});
                            currentNodeCopy.name = currentNodeCopy.name + i;
                        }
                        else currentNodeCopy = currentNode as noAction;
                        
                        currentNodeCopy.children = new HashSet<noAction>();
                        currentNodeCopy.children.Add(tempChildrenSet[i]);
                        currentNodeCopy.children.ToList()[0].parents.ExceptWith(new HashSet<noNode>() {currentNode});
                        currentNodeCopy.children.ToList()[0].parents.UnionWith(new HashSet<noNode>() {currentNodeCopy});
                        
                        currentNodeCopy.CalculateCostNI(); // TODO: vyhodit offset vector
                    }
                }
                else // TODO: nepotrebny else, da sa spravit vramci foru ^
                {
                    (currentNode as noAction).CalculateCostNI();
                }
            }
            currentDepthNodes = nextDepthNodes;
        }
    }

    public noAction FindBestActionSequence(noNode root, List<noNode> allUsedNodes, List<noNode> leaves)
    {
        HashSet<noNode> nodesToVisit = new HashSet<noNode>(allUsedNodes);
        
        foreach (var node in nodesToVisit)
        {
            node.ResetBeforeSearchingGraph();
        }

        noNode currentlyVisitedNode = root;
        currentlyVisitedNode.pathCost = 0;

        while (nodesToVisit.Count != 0)
        {
            foreach (var child in currentlyVisitedNode.children)
            {
                float potentialNewPathCost = child.totalCost + currentlyVisitedNode.pathCost;
                if (potentialNewPathCost < child.pathCost)
                {
                    child.pathCost = potentialNewPathCost;
                    child.previousOnPath = currentlyVisitedNode;
                }
            }

            nodesToVisit.Remove(currentlyVisitedNode);

            noNode cheapestNode = null;
            float minCost = int.MaxValue;
            foreach (var node in nodesToVisit)
            {
                if (node.pathCost < minCost)
                {
                    cheapestNode = node;
                    minCost = cheapestNode.pathCost;
                }
            }

            currentlyVisitedNode = cheapestNode;
        }

        noNode cheapestLeaf = new noAction();
        cheapestLeaf.name = "NEVER USED";
        cheapestLeaf.pathCost = int.MaxValue;
        foreach (var leaf in leaves)
        {
            if (leaf.pathCost < cheapestLeaf.pathCost)
                cheapestLeaf = leaf;
        }

        if (cheapestLeaf.pathCost == int.MaxValue)
            return null;

        List<noAction> bestActionSequence = new List<noAction>();
        noAction currentNode = cheapestLeaf as noAction;
        int iterCnt = 0;
        while (iterCnt++ < 30)
        {
            bestActionSequence.Add(currentNode);
            if (currentNode.previousOnPath != root)
                currentNode = currentNode.previousOnPath as noAction;
            else break;
        }

        currentNode.previousOnPath = null;
   
        return cheapestLeaf as noAction;
    }

    private void Update()
    {
        if (lastKnownPos.magnitude != Vector3.negativeInfinity.magnitude)
        {
            lastKnownPosDist = Vector3.Distance(transform.position, lastKnownPos);
        }
        else
        {
            lastKnownPosDist = float.MaxValue;
        }
    }

    private void LateUpdate()
    {
        ProcessCurrentAction();
    }

    public IEnumerator ReplanWithCooldown()
    {
        yield return new WaitForSeconds(planningCooldown);
        Replan();
    }

    public bool testLeaf = false; 
    public HashSet<string> CreateWorldState()
    {
        ChooseChaser();
        
        var worldState = new HashSet<string>(WorldState);
        
        if (resources.currentAmmoInMag > 0) worldState.Add("reloaded");

        if (resources.reloadsLeft > 0) worldState.Add("replenished");
        
        if (resources.currentGrenades > 0) worldState.Add("hasgrenade");

        if (lastKnownPos.magnitude != Vector3.negativeInfinity.magnitude) worldState.Add("aware");

        if ((currentGoal?.name.Contains("Attack") == true || currentGoal?.name.Contains("Resources") == true ) && currentAction?.name.Contains("Replenish") == true) worldState.Add("los");
        
        foreach (var ws in worldState)
        {
            // Debug.Log(ws);
        }
        return worldState;
    }

    private void ChooseChaser()
    {
        // ziskaj vsetkych npc a vyber ten s najmensiu lkpd
        var allNpcs = GameObject.FindGameObjectsWithTag("AI");
        float minDist = allNpcs[0].GetComponent<AIPlanner>().lastKnownPosDist;
        GameObject bestChaser = allNpcs[0];
        foreach (var potentialChaser in allNpcs)
        {
            var potentialDist = potentialChaser.GetComponent<AIPlanner>().lastKnownPosDist;
            if (potentialDist < minDist)
            {
                minDist = potentialDist;
                bestChaser = potentialChaser;
            }
        }
    
        chaser = bestChaser;
    }
    
    public void Replan(bool logGraph = false, bool logPath = false)
    {
        StopCoroutine("ReplanWithCooldown");
        var worldState = CreateWorldState();
        int bestGoalIndex = 0;
        List<noAction> cheapestLeaves = new List<noAction>();
        int i;
        for (i = 0; i < possibleGoals.Count; i++)
        {
            possibleGoals[i].totalCost = 100;

            bool completable = true;
            List<noNode> allUsedNodes = new List<noNode>();
            List<noNode> leaves = new List<noNode>();
            noNode root = GenerateActionGraph(possibleGoals[i], possibleActions, ref allUsedNodes, ref leaves, worldState);
            if (logGraph) PrintGraph(root);
            AlterGraphAndCalculateCosts(root, ref allUsedNodes, leaves);
            PrintGraph(root);
            cheapestLeaves.Add(FindBestActionSequence(root, allUsedNodes, leaves));

            if (leaves.Count == 0)
            {
                // nenasli sme cestu grafom, abort
                completable = false;
            }

            noNode startingNode = cheapestLeaves[i];
            float totalGoalTime = 0;
            while (startingNode != null)
            {
                totalGoalTime += startingNode.totalCost;
                startingNode = startingNode.previousOnPath;
            }
            possibleGoals[i].CalculateUtilityAsCost(totalGoalTime, completable, worldState);
            
            if (possibleGoals[i].totalCost > possibleGoals[bestGoalIndex].totalCost) bestGoalIndex = i;
        }

        if (lastGoal?.name != possibleGoals[bestGoalIndex]?.name || currentAction == null)
        {
            currentGoal = possibleGoals[bestGoalIndex];

            if (currentGoal.name.Contains("Chase"))
            {
                if (chaser == null)
                {
                    chaser = gameObject;
                }
            }

            if (currentGoal?.name.Contains("Attack") == true && currentAction?.name.Contains("Replenish") == true)
            {
                tempLKP = new Vector3(lastKnownPos.x, lastKnownPos.y, lastKnownPos.z);
                lastKnownPos = Vector3.negativeInfinity;
            }
            
            currentAction?.FinishAction();
            currentAction = cheapestLeaves[bestGoalIndex];
            currentAction?.StartAction();
        }
        

        lastGoal = currentGoal;
        StartCoroutine("ReplanWithCooldown");
        
        if (logPath)
        {
            string cheapestPath = "";
            noAction currentAction = this.currentAction;
            while (currentAction != null)
            {
                cheapestPath += currentAction.name + " > ";
                currentAction = currentAction.previousOnPath as noAction;
            }

            Debug.Log(cheapestPath);
        }
    }

    private void ResolveCurrentAction()
    {
        if (tempLKP.magnitude != Vector3.negativeInfinity.magnitude)
        {
            lastKnownPos = tempLKP;
            tempLKP = Vector3.negativeInfinity;
        }
        currentAction?.Perform();
    }

    private void ProcessCurrentAction()
    {
        currentAction?.Update();
        if (!actionInvoked)
        {
            if (currentAction == null)
            {
                Replan(false, false);
            }

            if (currentAction == null)
                return; // ak po nastaveni je currentAction stale null, skipneme tento frame a dufame, ze na dalsom frame uz budeme mat akciu

            // AKCIA DOBEHLA? zacni dalsiu alebo nastav null a returnni
            if (!currentAction.isRunning)
            {
                if (currentAction?.previousOnPath != null)
                {
                    currentAction = currentAction?.previousOnPath as noAction;
                    currentAction?.StartAction();
                }
                else
                {
                    currentAction = null;
                    return;
                }
            }

            if (currentAction?.target == null) // len pre isotu, nevykona sa, ak sme target naplnili uz pocas planovania
            {
                var target = currentAction?.FindNearestTarget(gameObject);
                if (currentAction?.targetTag != "" && target == null)
                {
                    currentAction = null;
                    return;
                }

                if (target != null && currentAction != null)
                {
                    currentAction.target = target;
                }
            }

            if (currentAction != null && currentAction.target != null) // Ak po nastaveni mame target ale nemame destinaciu/stojime
            {
                agent.SetDestination(currentAction.target.transform.position);
                agent.stoppingDistance = currentAction.minDistanceToActivateTarget;
            }

            if (currentAction != null &&
                (currentAction.target == null || // ak je target este stale null, akciu mozno vykonat kdekolvek (napr. hadzanie granatu, strelba...)
                 Vector3.Distance(transform.position, currentAction.target.transform.position) // sucastou tejto podmienky byvalo currentAction.agent.hasPath, ale uz netusim preco
                 < currentAction?.minDistanceToActivateTarget))
            {
                actionInvoked = true;
                currentAction.OnDestinationArrival();
                Invoke("ResolveCurrentAction", currentAction.preDuration);
            }
        }
    }

    public void FinishActionInvoked()
    {
        actionInvoked = false;
        currentAction = currentAction.previousOnPath as noAction;
        currentAction?.StartAction();
    }
}
