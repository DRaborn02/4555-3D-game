using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Items/Weapon")]
public class Weapon : Item
{
    public enum WeaponType { LightMelee, HeavyMelee, Ranged }

    public WeaponType type;
    public float damage;
    public float secondaryDamage;
    public float swingSpeed;
    public float cooldown;
    public float secondaryCooldown;    
    public float projectileSpeed;
    public GameObject projectilePrefab;
}