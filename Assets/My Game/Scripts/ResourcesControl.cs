using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesControl : MonoBehaviour
{
    public int maxAmmoInMag = 10;
    public int currentAmmoInMag = 0;
    public int maxReloads = 3;
    public int reloadsLeft = 0;
    // public int shotsPerBurst = 1;
    public int maxGrenades = 1;
    public int currentGrenades = 0;

    private void Start()
    {
        Replenish();
    }

    public bool Shoot()
    {
        if (currentAmmoInMag > 0)
        {
            currentAmmoInMag--;
            return true;
        }
        return false;
    }

    public bool Reload()
    {
        if (reloadsLeft > 0)
        {
            reloadsLeft--;
            currentAmmoInMag = maxAmmoInMag;
            return true;
        }

        return false;
    }

    public bool Throw()
    {
        if (currentGrenades > 0)
        {
            currentGrenades--;
            return true;
        }

        return false;
    }

    public void Replenish()
    {
        reloadsLeft = maxReloads;
        currentAmmoInMag = maxAmmoInMag;
        currentGrenades = maxGrenades;
    }
}
