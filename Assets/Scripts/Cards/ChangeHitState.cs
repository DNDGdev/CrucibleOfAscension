using UnityEngine;

public class ChangeHitState : SkillController
{
    public HitState hitState;


    public override void Activate()
    {
        Debug.Log("reach activate");
        if (currentState != SkillState.Ready) return;

        currentState = SkillState.Cast;
        cardView.OnActivate();
        if (card.skill.effectItem.SkillObject != null)
        {
            GameObject newEffect = Instantiate(card.skill.effectItem.SkillObject, player.effectsHolder);
        }

        SetHitState();
        Debug.Log("Skill Activated!");
        // Simulating skill effect duration
        if (skillData.duration > 0)
        {
            Invoke(nameof(Deactivate), skillData.duration);
        }

        else
        {
            Deactivate();
        }
    }
    public void SetHitState()
    {
        player.healthManager.hitState = hitState;
    }
}
