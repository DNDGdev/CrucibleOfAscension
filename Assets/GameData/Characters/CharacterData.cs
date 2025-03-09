using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/Character")]
public class CharacterData : ScriptableObject
{
    public CharacterType characterType;
    public string characterName;
    public Sprite characterIcon;
    public Stats stats;
}

[System.Serializable]
public struct Stats
{
    public CombatStats combatStats;
    public AttrebiuteRates attrebiuteRates;
}

[System.Serializable]
public struct CombatStats
{
    [Header("Stats")]
    public int maxHealth;
    public float attackDamage;
    public float evasionChance;
    public float resilienceChance; // Chance to resist card effects
    public RangeType rangeType;
}

[System.Serializable]
public struct AttrebiuteRates
{
    [Header("Attrebiutes")]
    public int Haste;
    public float Range;
    public float Might;
    public float Vitality;
    public float resilience; // Chance to resist card effects
}

[System.Serializable]
public enum CharacterType
{
    BladeMaster,
    Hunter,
    Warden
}

[System.Serializable]
public enum AttackType
{
    Melee,
    Range
}

[System.Serializable]
public struct RangeType
{
    public AttackType type;
    public float Range;
}
