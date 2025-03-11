using System.Collections;
using UnityEngine;

public class HealthRestoration : SkillController
{
    public float HealthAmount;

    public override void Update()
    {
        base.Update();
    }

    public override void Activate()
    {
        Debug.Log("reach activate");
        if (currentState != SkillState.Ready) return;

        currentState = SkillState.Cast;
        cardView.OnActivate();
        GameObject newEffect = Instantiate(card.skill.effectItem.SkillObject, player.effectsHolder);
        player.RestoreHealth(HealthAmount);
        Debug.Log("Skill Activated!");
        if (skillData.duration > 0)
        {
            Invoke(nameof(Deactivate), skillData.duration);
        }

    }

}
