using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ESounds
{
    Footsteps,
    Jumping,
    Shooting,
}
public class HearingControl : MonoBehaviour
{
    // existuje, pretoze ked vydame zvuk, treba notifikovat listenerov. Singleton na scene
    public static HearingControl Instance { get; private set; } = null;

    public HashSet<Hearing> listeners { get; private set; } = new HashSet<Hearing>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Register(Hearing sensor)
    {
        listeners.Add(sensor);
    }

    public void Deregister(Hearing sensor)
    {
        listeners.Remove(sensor);
    }

    public void OnSoundEmmited(GameObject source, Vector3 location, ESounds type, float intensity)
    {
        foreach (var listener in listeners)
        {
            listener.OnSoundHeard(source, location, type, intensity);
        }
    }
}
