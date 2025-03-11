using System;
using UnityEngine;
using UnityEngine.UI;

public class BladeMaster_BasicAttack : SkillController
{
    public Button TriggerButton;
    public Action onAttack;

    public override void Update()
    {
        base.Update();
        //if (player.Target != null)
        //{
        //    Debug.Log(TargetIsInRange());
        //    player.targetOutline.enabled = TargetIsInRange();
        //}
    }
    public override void SetUp()
    {
        base.SetUp();
        TriggerButton.onClick.AddListener(Activate);
        GameObject newEffect = Instantiate(card.skill.effectItem.SkillObject, player.effectsHolder);
        skillData.effectItem.particle = newEffect.GetComponent<ParticleSystem>();
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
        skillData.effectItem.particle.Play();
        onAttack?.Invoke();

        if (audioSrc != null)
            audioSrc.Play();

        ResetCard();

    }

    public override void ResetCard()
    {
        currentState = SkillState.Idle;
        StartCooldown();
    }
}
