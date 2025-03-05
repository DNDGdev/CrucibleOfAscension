using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
    [SerializeField] private CharacterData characterData;
    public Stats playerStats;

    
    private void InitializeCharacter(CharacterData _characterData)
    {
        playerStats = _characterData.stats;
    }

    private void Start()
    {
        
    }

    public void GetHit(DamageItem damageItem)
    {
        throw new System.NotImplementedException();
    }
}
