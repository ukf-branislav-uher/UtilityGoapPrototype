using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Vision : MonoBehaviour
{
    /*
     * Detectable targets musia mat child objekt s tagom Target a colliderom, na zaklade tohto objektu riesime vision
     */

    private noAwarenessControl thisAI;
    
    public float visionDistance;
    [Range(0, 360)] public float visionAngle;
    public LayerMask allowedHits;

    public HashSet<Transform> visibleTargets = new HashSet<Transform>();

    public float visionRepeatRate = 1f;
    
    private void removeVisibleTarget(Transform target)
    {
        if (visibleTargets.Contains(target.transform))
        {
            visibleTargets.Remove(target.transform);
        }
    }
    
    void FindVisibleTargets()
    {
        var possibleTargets = GameObject.FindGameObjectsWithTag("Player"); // TODO: cache this

        foreach (var target in possibleTargets)
        {
            if (!visibleTargets.Contains(target.transform))
            {
                visibleTargets.Add(target.transform);
            }
            
            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            if (distanceToTarget > visionDistance)
            {
                removeVisibleTarget(target.transform);
                continue;
            }

            if (Vector3.Angle(transform.forward, directionToTarget) > visionAngle / 2)
            {
                removeVisibleTarget(target.transform);
                continue;
            }

            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToTarget, out hit, distanceToTarget, allowedHits))
            {
                if (hit.transform.gameObject.tag != "Target")
                {
                    removeVisibleTarget(target.transform);
                    continue;
                }
            }
        }

        thisAI.ReportSawSomething(this, visibleTargets);
    }

    public (Vector3, Vector3) DirFromAngle()
    {
        var angleA = -visionAngle / 2 + transform.eulerAngles.y;
        var angleB = visionAngle / 2 + transform.eulerAngles.y;

        float xA = Mathf.Sin(angleA * Mathf.Deg2Rad); //vymenene sin a cos lebo unity to ma o 90 stupnov posunute
        float zA = Mathf.Cos(angleA * Mathf.Deg2Rad);

        float xB = Mathf.Sin(angleB * Mathf.Deg2Rad);
        float zB = Mathf.Cos(angleB * Mathf.Deg2Rad);

        var viewAngleA = new Vector3(xA, 0, zA);
        var viewAngleB = new Vector3(xB, 0, zB);

        return (viewAngleA, viewAngleB);
    }

    void Start()
    {
        InvokeRepeating("FindVisibleTargets", 0, visionRepeatRate);

        // thisAI = GetComponent<NavAgentExample>();
        thisAI = GetComponent<noAwarenessControl>();
    }
}