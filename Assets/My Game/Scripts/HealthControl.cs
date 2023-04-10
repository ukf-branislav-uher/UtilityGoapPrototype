using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class HealthControl : MonoBehaviour
{
    public int MaxHP = 100;
    [SerializeField] private int _hp;

    private Animator anim;

    private Collider[] _colliders;

    private void Start()
    {
        _hp = MaxHP;
        _colliders = transform.GetComponentsInChildren<Collider>();
        anim = GetComponentInChildren<Animator>();
    }

    public void LoseHealth(int amount)
    {
        _hp = Math.Max(0, _hp - amount); // aby sme nesli pod 0

        if (_hp == 0) Die();
    }

    public void GainHealth(int amount)
    {
        _hp = Math.Min(MaxHP, _hp + amount); // aby sme nesli nad maximalne HP
    }

    public void Die()
    {
        GetComponent<NavMeshAgent>().isStopped = true;
        anim.SetTrigger("die");
        Destroy(gameObject, 3);
    }
}