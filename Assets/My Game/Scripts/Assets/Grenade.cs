using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float range;
    public ParticleSystem explosion;
    public GameObject player;
    public GameObject[] enemies;
    private void Awake()
    {
        enemies = GameObject.FindGameObjectsWithTag("AI");
        player = GameObject.FindWithTag("Player");
    }

    /*
     * Tato metoda pochadza v znacnej miere zo zdroja: https://forum.unity.com/threads/making-an-object-land-directly-on-a-position-using-velocity.887023/
     */
    public void Throw(Vector3 target, GameObject npc)
    {
        var rigid = GetComponent<Rigidbody>();
        
        float gravity = Physics.gravity.magnitude;
        // Selected angle in radians
        float angle = 50 * Mathf.Deg2Rad;
 
        // Positions of this object and the target on the same plane
        Vector3 planarTarget = new Vector3(target.x, 0, target.z);
        Vector3 planarPosition = new Vector3(transform.position.x, 0, transform.position.z);
 
        // Planar distance between objects
        float distance = Vector3.Distance(planarTarget, planarPosition);
        // Distance along the y axis between objects
        float yOffset = transform.position.y - target.y;
 
        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));
 
        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

        var velocityUpward = initialVelocity * Mathf.Sin(angle);
        var velocityForward = initialVelocity * Mathf.Cos(angle);
        Vector3 finalVelocity = npc.transform.rotation * velocity;
        
        rigid.velocity = finalVelocity;
   
        StartCoroutine(Countdown());
    }

    public void Explode()
    {
        var particle = Instantiate(explosion, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        particle.Play();

        var distanceToTarget = Vector3.Distance(player.transform.position, transform.position);
        if (distanceToTarget <= range)
        {
            Vector3 directionToTarget = (player.transform.position - transform.position).normalized;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToTarget, out hit, distanceToTarget))
            {
                if (hit.transform.gameObject.tag == "Target")
                {
                    // player.GetComponent<HealthControl>().LoseHealth(1);
                    player.GetComponent<CameraShake>().ShakeIt();
                }
            }
        }

        foreach (var enemy in enemies)
        {
            var distanceToEnemy = Vector3.Distance(enemy.transform.position, transform.position);
            Vector3 directionToTarget = (enemy.transform.position - transform.position).normalized;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToTarget, out hit, distanceToEnemy))
            {
                if (hit.transform.gameObject.tag == "Target")
                {
                    enemy.GetComponent<HealthControl>().LoseHealth(90);
                }
            }
        }
        Destroy(particle.gameObject, 0.2f);
        Destroy(gameObject, 0.2f);
    }

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(3);
        Explode();
    }
}

