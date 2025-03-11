using UnityEngine;

public class PrecisionStrike : SkillController
{
    public override void Update()
    {
        base.Update();
    }

    public override void Activate()
    {
        Debug.Log("reach activate");
        if (currentState != SkillState.Ready) return;

        currentState = SkillState.Cast;
        Debug.Log("Skill Activated!");
        // Simulating skill effect duration
        player.cardsManager.ActiveSkill = this;
        player.animator.SetTrigger(skillData.skillAnimation);
        GameObject newEffect = Instantiate(card.skill.effectItem.SkillObject, player.effectsHolder);

        if (audioSrc != null)
            audioSrc.Play();

        if (skillData.duration > 0)
        {
            Invoke(nameof(Deactivate), skillData.duration);
        }
    }
}
