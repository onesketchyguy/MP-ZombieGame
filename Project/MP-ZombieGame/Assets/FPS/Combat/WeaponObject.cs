using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Weapon
{
    public GameObject weaponPrefab;
    public int damage;
    public uint clipSize;
    public uint ammo;

    // FIXME: add sounds
}

[CreateAssetMenu(fileName="Weapon object", menuName = "FPS/Weapon")]
public class WeaponObject : ScriptableObject
{
    public Weapon data;
    public IdentifiableObject idObj = null;
}