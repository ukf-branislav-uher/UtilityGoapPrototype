using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weaponManager : MonoBehaviour
{
    public weaponControl currentWpn;

    public void StopReload()
    {
        currentWpn.StopReload();
    }
}
