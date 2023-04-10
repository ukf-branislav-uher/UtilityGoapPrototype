using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class TrackedTarget
{
    public Vector3 RawPosition;

    public float LastSensedTime = -1f;

    public float Awareness; 
    // 0     = not aware (will be culled); 
    // 0-1   = rough idea (no set location); 
    // 1-2   = likely target (location)
    // 2     = fully detected

    public bool UpdateAwareness(Vector3 position, float awarenessAddition, float minAwareness)
    {
        var oldAwareness = Awareness;
        
        RawPosition = position;
        LastSensedTime = Time.time;
        Awareness = Mathf.Clamp(Mathf.Max(Awareness, minAwareness) + awarenessAddition, 0f, 2f);
        
        if (oldAwareness < 1f && Awareness >= 1f)
            return true;
        if (oldAwareness <= 0f && Awareness >= 0f)
            return true;

        return false;
    }

    public bool DecayAwareness(float decayTime, float amount)
    {
        // detected too recently - no change
        if (Time.time - LastSensedTime < decayTime)
            return false;

        var oldAwareness = Awareness;

        Awareness -= amount;
        
        if (oldAwareness >= 1f && Awareness < 1f)
            return true;
        return Awareness <= 0f;
    }
}

[RequireComponent(typeof(NavAgentExample))]
public class AwarenessControl : MonoBehaviour
{
    [SerializeField] AnimationCurve VisionSensitivity;
    [SerializeField] float VisionMinimumAwareness = 1f;
    [SerializeField] float VisionAwarenessBuildRate = 20f;

    [SerializeField] float HearingMinimumAwareness = 0f;
    [SerializeField] float HearingAwarenessBuildRate = 0.5f;

    [SerializeField] float AwarenessDecayDelay = 0.1f;
    [SerializeField] float AwarenessDecayRate = 0.1f;

    Dictionary<GameObject, TrackedTarget> Targets = new Dictionary<GameObject, TrackedTarget>();
    NavAgentExample LinkedAI;

    void Start()
    {
        LinkedAI = GetComponent<NavAgentExample>();
    }

    void Update()
    {
        List<GameObject> toCleanup = new List<GameObject>();
        foreach (var targetGO in Targets.Keys)
        {
            if (Targets[targetGO].DecayAwareness(AwarenessDecayDelay, AwarenessDecayRate * Time.deltaTime))
            {
                Debug.Log("Awns decreased: " + Targets[targetGO].Awareness);
                if (Targets[targetGO].Awareness <= 0f)
                {
                    LinkedAI.OnFullyLost();
                    toCleanup.Add(targetGO);
                }
                else
                {
                    if (Targets[targetGO].Awareness <= 1f)
                        LinkedAI.OnLostSuspicion();
                }
            }
        }

        // cleanup targets that are no longer detected
        foreach (var target in toCleanup)
            Targets.Remove(target);
    }

    void UpdateAwareness(GameObject targetGO, Transform target, Vector3 position, float awarenessAddition, float minimumAwareness)
    {
        // not in targets
        if (!Targets.ContainsKey(targetGO))
            Targets[targetGO] = new TrackedTarget();

        // update target awareness
        if (Targets[targetGO].UpdateAwareness(position, awarenessAddition, minimumAwareness))
        {
            if (Targets[targetGO].Awareness >= 2f)
                LinkedAI.OnFullyDetected(targetGO);
            else if (Targets[targetGO].Awareness >= 0f)
                LinkedAI.OnSuspicious();
            Debug.Log("Awns gon up:" + Targets[targetGO].Awareness);
        }
    }

    public void ReportSawSomething(Transform location)
    {
        var awareness = VisionAwarenessBuildRate * Time.deltaTime;

        // passujeme parenta, pretoze povodny transform patri DetectionCollideru, ktory vzdy davame ako level 1 child
        UpdateAwareness(location.gameObject, location, location.position, awareness, VisionMinimumAwareness);
    }

    public void ReportHeardSound(GameObject source, Vector3 location, ESounds type, float intensity)
    {
        var awareness = intensity * HearingAwarenessBuildRate * Time.deltaTime;

        UpdateAwareness(source, null, location, awareness, HearingMinimumAwareness);
    }
}