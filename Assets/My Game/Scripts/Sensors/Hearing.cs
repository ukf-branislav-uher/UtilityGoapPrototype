using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hearing : MonoBehaviour
{
    public float hearingRange;
    private noAwarenessControl thisAI;
    
    void Start()
    {
        thisAI = GetComponent<noAwarenessControl>();
        HearingControl.Instance.Register(this);
    }

    public void OnSoundHeard(GameObject source, Vector3 location, ESounds type, float intensity)
    {
        if(Vector3.Distance(location, thisAI.gameObject.transform.position) > hearingRange)
            return;

        thisAI.ReportHeardSound(source, location, type, intensity);
    }
    
    private void OnDestroy()
    {
        if(HearingControl.Instance != null)
            HearingControl.Instance.Deregister(this);
    }
}
