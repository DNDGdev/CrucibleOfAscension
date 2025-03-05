using System.Collections;
using UnityEngine;

public interface IDamageable
{
    public void GetHit(DamageItem damageItem);

}

[System.Serializable]
public struct DamageItem
{
    public float DamageAmount;
    public float HealAmount;
    public Vector3 KnockBackAmount;
    public CrowdControl crowdControl;
    public GameObject HitEffect;
    public string HitAnimation;
}

public enum CrowdControl
{
    None,
    Stun,
    Freeze,
    Burn
}
