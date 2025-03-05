using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SkillController : MonoBehaviour
{
    [Header("References")]
    public Card card;
    public Skill skillData;
    public CardView cardView => GetComponent<CardView>();
    public DraggableUI draggableUI => GetComponent<DraggableUI>();
    public enum SkillState { Idle, Cooldown, Ready, Cast }

    public SkillState currentState = SkillState.Idle;

    private CanvasGroup canvasGroup;

    private float cooldownTimer = 0f;

    public SlotIndex slotIndex;

    public bool autoStart;
    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        currentState = SkillState.Idle;

        if (draggableUI != null)
            draggableUI.onSwipe += Activate;

        if(autoStart)
        {
            InitSkill(card);
        }
    }

    private void Update()
    {
        if (canvasGroup != null && draggableUI != null)
        {
           draggableUI.enabled = canvasGroup.interactable = (currentState == SkillState.Ready || currentState == SkillState.Cast);
        }
    }
    public void InitSkill(Card _card)
    {
        card = _card;
        skillData = card.skill;
        cardView.Init(card);

        StartCooldown();
    }

    private void StartCooldown()
    {
        Debug.Log("startCooldown");
        currentState = SkillState.Cooldown;
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        cooldownTimer = skillData.cooldownTime;
        while (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            cardView.UpdateUI(cooldownTimer, cooldownTimer / skillData.cooldownTime);
            yield return null;
        }

        cardView.SkillReady();

        currentState = SkillState.Ready;
    }

    public virtual void Activate()
    {
        Debug.Log("reach activate");
        if (currentState != SkillState.Ready) return;

        currentState = SkillState.Cast;
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

    private void Deactivate()
    {
        currentState = SkillState.Idle;
        //StartCooldown();
    }
}
