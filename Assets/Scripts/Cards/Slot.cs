using UnityEngine;

public class Slot : MonoBehaviour
{
    public SkillController currentSkill;
    public SlotIndex slotIndex;
}

public enum SlotIndex
{
    basicAttack,
    slot1,
    slot2,
    slot3,
    slot4,
    slot5
}