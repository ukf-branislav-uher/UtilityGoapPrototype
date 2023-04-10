using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum DoorState
{
    Closed,
    Open,
    Animating
};

[RequireComponent(typeof(NavMeshObstacle))]
public class DoorControl : MonoBehaviour
{ 
    private Animator anim;
    void Start()
    {
        anim = transform.parent.GetComponent<Animator>();
        anim.SetBool("Open", true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            anim.SetBool("Open", !anim.GetBool("Open"));
        }
    }
}
