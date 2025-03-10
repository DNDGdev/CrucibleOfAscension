using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
    [SerializeField] private CharacterData characterData;
    public Stats playerStats;
    public Animator animator;
    public Transform effectsHolder;
    public CardsManager cardsManager => GetComponent<CardsManager>();

    public HealthManager healthManager => GetComponent<HealthManager>();

    [Header("Combat")]
    public Transform rayPos;
    public LayerMask hitableLayer;
    public float rayDistance;
    public Transform Target;
    public Outline targetOutline;
    private void InitializeCharacter(CharacterData _characterData)
    {
        playerStats = _characterData.stats;
        rayDistance = _characterData.stats.combatStats.rangeType.Range;
    }

    private void Awake()
    {
        InitializeCharacter(characterData);
    }

    public void GetHit(DamageItem damageItem)
    {
        Instantiate(damageItem.HitEffect, transform.position , Quaternion.identity);
        healthManager.TakeDamage((int)damageItem.DamageAmount);
    }

    public void TriggerManageHit()
    {
        if (cardsManager.ActiveSkill != null)
            cardsManager.ActiveSkill.ManageHit();
    }


    public void AddDamage(float amount)
    {
        cardsManager.BasicAttack.skillController.skillData.damageItem.DamageAmount += amount;
    }

    public void ReduceDamage(float amount)
    {
        cardsManager.BasicAttack.skillController.skillData.damageItem.DamageAmount -= amount;
    }

    public float GetDamage()
    {
        return cardsManager.BasicAttack.skillController.skillData.damageItem.DamageAmount;
    }

    void Update()
    {

        // Define the direction (forward from the object's perspective)
        Vector3 direction = transform.forward;

        // Perform the raycast
        if (Physics.Raycast(rayPos.position, direction, out RaycastHit hit, rayDistance, hitableLayer))
        {
            if(Target == null)
            {
                Debug.Log("Hit: " + hit.collider.name);
                Target = hit.transform;
                Target.GetComponent<Outline>().enabled = true;
            }
        }
        else
        {
            if(Target != null)
            {
                Target.GetComponent<Outline>().enabled = false;
                Target = null;
            }
        }

        // Draw the ray in the Scene view (for debugging)
        Debug.DrawRay(rayPos.position, direction * rayDistance, Color.red);
    }
   
}
