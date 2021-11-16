using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int index;
    public int damage;
    public int speed;
    public float range;

    public virtual void WeaponSetup() { }

    public void destroyWeapon()
    {
        Destroy(this);
    }
}
