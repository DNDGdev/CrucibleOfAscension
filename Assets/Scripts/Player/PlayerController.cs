using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
    [SerializeField] private CharacterData characterData;
    public Stats playerStats;
    public Animator animator;
    public Transform effectsHolder;
    public CardsManager cardsManager => GetComponent<CardsManager>();

    [Header("Combat")]
    public Transform Target;
    public Outline targetOutline;
    private void InitializeCharacter(CharacterData _characterData)
    {
        playerStats = _characterData.stats;
    }

    private void Awake()
    {
        InitializeCharacter(characterData);
    }

    public void GetHit(DamageItem damageItem)
    {
        throw new System.NotImplementedException();
    }

    public void TriggerManageHit()
    {
        if (cardsManager.ActiveSkill != null)
            cardsManager.ActiveSkill.ManageHit();
    }
}
