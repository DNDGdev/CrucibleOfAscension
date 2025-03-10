using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card Game/Card")]
public class Card : ScriptableObject
{
    public Skill skill;
    public GameObject SkillPrefab;
}

[System.Serializable]
public struct Skill
{
    public Sprite skillIcon;
    public string skillName;
    public string skillAnimation;
    public string info;
    public SkillType skillType;
    public SophisticationTier tier;
    public CombatStats Stats;
    public float cooldownTime;
    public float duration;
    public float effectValue; // Used for damage, healing, shield value, etc.
    public EffectItem effectItem;
    public DamageItem damageItem;
}

public enum SkillType
{
    SingleTargetAttack,    // Targets an enemy and deals damage
    HealthRestoration,     // Heals the player's hero
    Protection,            // Provides a shield or blocks attacks
    MovementEnhancement,   // Improves mobility
    MovementDebilitation   // Hinders enemy movement
}

public enum SophisticationTier
{
    Common,
    Uncommon
}

public enum SkillIndex
{
    basicAttack,
    slot1,
    slot2,
    slot3,
    slot4
}

[System.Serializable]
public struct EffectItem
{
    public GameObject SkillObject;
    public Vector3 SpawnPos;
    public ParticleSystem particle;
}
