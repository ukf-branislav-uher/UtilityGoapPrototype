using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class weaponControl : MonoBehaviour
{
    public float fireratePerSec = 2;
    public int maxAmmoPool;
    public int maxMagAmmo;
    public int damage = 10;
    public float cirticalMultiplier = 2;
    [SerializeField] private int curAmmoPool;
    [SerializeField] private int curMagAmmo;

    public LayerMask hittable;
    public GameObject cameraGO;
    public GameObject bulletEffect;
    private bool isFiring;
    private bool isReloading;

    public Animator anim;

    public const string MOV_ANIM = "MovementSpeed";
    public const string SHOOT_ANIM = "isShooting";
    public const string RELOAD_ANIM = "isReloading";

    private void Start()
    {
        curMagAmmo = maxMagAmmo;
        curAmmoPool = maxAmmoPool;
    }

    private void FixedUpdate()
    {
        if (Input.GetButton("Fire1") && curMagAmmo > 0 && !isFiring && !isReloading)
        {
            StartCoroutine(fireContinuously());
        }

        if (Input.GetButton("Reload") && !isReloading && curMagAmmo < maxMagAmmo && curAmmoPool > 0)
        {
            Reload();
        }
    }

    private void Reload()
    {
        isReloading = true;
        anim.SetBool(RELOAD_ANIM, true);

        var kolkoAmmoChceme = maxMagAmmo - curMagAmmo;
        curAmmoPool -= kolkoAmmoChceme;
        var kolkoMusimeZobrat = 0;
        if (curAmmoPool < 0)
        {
            kolkoMusimeZobrat = curAmmoPool;
            curAmmoPool = 0;
        }
        curMagAmmo += kolkoAmmoChceme + kolkoMusimeZobrat;
    }

    public void StopReload()
    {
        isReloading = false;
        anim.SetBool(RELOAD_ANIM, false);
    }

    private IEnumerator fireContinuously()
    {
        isFiring = true;
        anim.SetBool(SHOOT_ANIM, true);
        fire();
        HearingControl.Instance.OnSoundEmmited(gameObject, transform.position, ESounds.Shooting, 1);
        yield return new WaitForSeconds(1 / fireratePerSec);
        isFiring = false;
        anim.SetBool(SHOOT_ANIM, false);
    }

    private void fire()
    {
        curMagAmmo--;
        RaycastHit hit;
        
        if (Physics.Raycast(cameraGO.transform.position, cameraGO.transform.forward, out hit,  Mathf.Infinity, hittable))
        {
            Debug.DrawLine(cameraGO.transform.position, hit.point);
            var particle = Instantiate(bulletEffect, hit.point, Quaternion.LookRotation(hit.normal))
                .GetComponent<ParticleSystem>();
            particle.Play();
            Destroy(particle.gameObject, particle.main.duration);
        }

        if (hit.collider.tag == "Head")
        {
            hit.collider.GetComponentInParent<HealthControl>().LoseHealth((int) Math.Floor(damage * cirticalMultiplier));
        } else if (hit.collider.tag == "Target")
        {
            hit.collider.GetComponentInParent<HealthControl>().LoseHealth(damage);
        }
    }
}