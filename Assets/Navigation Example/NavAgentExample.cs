using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;

// [RequireComponent(typeof(NavMeshAgent), typeof(AwarenessControl))]
public class NavAgentExample : MonoBehaviour
{
    // TODO : 
    //  nastudovat behavior trees na nasledovne:
    //  ak npc nema zbran, vyhodnotit, ci ju potrebuje a zobrat, ked uvidi
    //  ak npc uvidi hraca, strielat ak ma zbran, prenasledovat, ak nema
    //  ak npc pocuje hraca, prist priblizne k danemu miestu a hladat
    //  ak npc nenajde hraca, vratit sa na patrol route

    private Animator anim;

    private NavMeshAgent _agent;

    private TextMesh headText;

    public bool disablePatrol;
    public AIWaypointNetwork Network = null;
    public int CurrentIndex = -1;

    public AnimationCurve JumpCurve = new AnimationCurve();

    public AwarenessControl awarenessController;

    private void Start()
    {
        awarenessController = GetComponent<AwarenessControl>();
        anim = GetComponentInChildren<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        headText = transform.Find("Text").GetComponent<TextMesh>();

        if (Network == null) return;

        SetNextDestination(false);
        // if (Network.Waypoints[CurrentIndex] != null)
        // {
        //     _agent.destination = Network.Waypoints[CurrentIndex].position;
        // }
    }

    private void SetAllAnimationsFalse()
    {
        anim.SetBool("jumping", false);
        anim.SetBool("attacking", false);
        anim.SetBool("walking", false);
        // anim.SetBool("dying", false);
    }

    private void Update()
    {
        // if ( Vector3.Distance(_agent.gameObject.transform.position, Network.Waypoints[CurrentIndex].position) < CloseEnough)
        // {
        //     CurrentIndex = (CurrentIndex + 1) % Network.Waypoints.Count;
        //     _agent.destination = Network.Waypoints[CurrentIndex].position;
        // }

        ResolveAnimations();


        if ((!_agent.hasPath && !_agent.pathPending) || _agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            SetNextDestination(true);
        }
        else if (_agent.isPathStale)
        {
            SetNextDestination(false);
        }
    }

    private void ResolveAnimations()
    {
        if (!anim.GetBool("walking") && _agent.velocity != Vector3.zero)
        {
            anim.SetBool("walking", true);
            // anim.speed = 4; <- miesto tohto radsej proste spomalit animaciu samotnu
        }

        if (anim.GetBool("walking") && _agent.velocity == Vector3.zero)
        {
            anim.SetBool("walking", false);
            // anim.speed = 1;
        }

        if (_agent.isOnOffMeshLink && !anim.GetBool("jumping"))
        {
            StartCoroutine(Jump(1));
        }
    }

    IEnumerator Jump(float duration)
    {
        SetAllAnimationsFalse();
        anim.SetBool("jumping", true);
        OffMeshLinkData data = _agent.currentOffMeshLinkData;
        Vector3 startPos = transform.position;
        Vector3 endPos = data.endPos +
                         (_agent.baseOffset *
                          Vector3.up); // ak je offset medzi agentovym transformom a agentom samotnym, chceme ho zaratat
        float time = 0;

        while (time < duration)
        {
            float t = time / duration;
            float typeOffset = data.linkType == OffMeshLinkType.LinkTypeJumpAcross ? 1 : 3;
            _agent.transform.position =
                Vector3.Lerp(startPos, endPos, t) + (JumpCurve.Evaluate(t) * typeOffset * Vector3.up);
            // // Debug.Log(JumpCurve.Evaluate(t) + "-" + t);
            time += Time.deltaTime;
            yield return null;
        }

        _agent.CompleteOffMeshLink();
        anim.SetBool("jumping", false);
    }

    private void SetNextDestination(bool increment) // parameter je na resetovanie cesty ak ju treba prepocitat
    {
        if (disablePatrol) return;
        if (!Network) return;

        int incStep = increment ? 1 : 0;
        Transform nextWaypointTransform = null;

        int nextWpIndex = (CurrentIndex + incStep >= Network.Waypoints.Count) ? 0 : CurrentIndex + incStep;
        nextWaypointTransform = Network.Waypoints[nextWpIndex];

        if (nextWaypointTransform != null)
        {
            CurrentIndex = nextWpIndex;
            _agent.destination = nextWaypointTransform.position;
        }
        else
        {
            CurrentIndex++;
        }
    }
    
    public void ReportHeardSound(GameObject source, Vector3 location, ESounds type, float intensity)
    {
        awarenessController.ReportHeardSound(source, location, type, intensity);
        // Debug.Log("Sound heard");
    }

    public void ReportSawSomething(HashSet<Transform> locations)
    {
        foreach (var loc in locations)
            awarenessController.ReportSawSomething(loc);
        // Debug.Log("Something seen");
    }

    public void OnSuspicious()
    {
        headText.text = "I hear you";
        Debug.Log(headText.text);
    }

    public void OnDetected(GameObject target)
    {
        headText.text = "I see you " + target.gameObject.name;
        Debug.Log(headText.text);
    }

    public void OnFullyDetected(GameObject target)
    {
        headText.text = "Charge! " + target.gameObject.name;
        Debug.Log(headText.text);
    }

    public void OnLostDetect(GameObject target)
    {
        headText.text = "Where are you " + target.gameObject.name;
        Debug.Log(headText.text);
    }

    public void OnLostSuspicion()
    {
        headText.text = "Where did you go";
        Debug.Log(headText.text);
    }

    public void OnFullyLost()
    {
        headText.text = "Must be nothing";
        Debug.Log(headText.text);
    }
}