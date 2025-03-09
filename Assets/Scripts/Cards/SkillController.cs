using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SkillController : MonoBehaviour
{
    [Header("References")]
    public Card card;
    public Skill skillData;
    public PlayerController player;
    public CardView cardView => GetComponent<CardView>();
    public DraggableUI draggableUI => GetComponent<DraggableUI>();
    public enum SkillState { Idle, Cooldown, Ready, Cast }

    public SkillState currentState = SkillState.Idle;

    private CanvasGroup canvasGroup;

    private float cooldownTimer = 0f;

    public SlotIndex slotIndex;


    public virtual void SetUp()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        currentState = SkillState.Idle;

        if (draggableUI != null)
            draggableUI.onSwipe += Activate;

    }

    public virtual void Update()
    {
        if (canvasGroup != null && draggableUI != null)
        {
           draggableUI.enabled = canvasGroup.interactable = (currentState == SkillState.Ready || currentState == SkillState.Cast);
        }
      
    }
    public void InitSkill(Card _card, PlayerController _player)
    {
        card = _card;
        player = _player;
        skillData = card.skill;
        if(cardView.isActiveAndEnabled)
        {
            cardView.Init(card);
        }
        SetUp();
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
            if (cardView.isActiveAndEnabled)
            {
                cardView.UpdateUI(cooldownTimer, cooldownTimer / skillData.cooldownTime);
            }
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

    public void Deactivate()
    {
        currentState = SkillState.Idle;
        StartCooldown();
    }

    public virtual void ManageHit()
    {
        if (player.Target == null)
        {
            Debug.LogError("Target is null!");
            return;
        }

        if (TargetIsInRange())
        {
            player.Target.GetComponent<HealthManager>()?.TakeDamage((int)skillData.Stats.attackDamage);
        }
        else
        {
            Debug.Log("Target not in range");
        }
    }

  
    public bool TargetIsInRange()
    {
        if (player.Target == null)
        {
            Debug.LogError("Target is null!");
            return false;
        }

        float distance = Vector3.Distance(player.Target.position, player.transform.position);
        float range = Mathf.Max(0, skillData.Stats.rangeType.Range); // Ensures non-negative range
        //Debug.Log($"Player Position: {player.transform.position}, Target Position: {player.Target.position}");
        //Debug.Log($"Distance to target: {Vector3.Distance(player.Target.position, player.transform.position)}");

        //Debug.Log($"Distance to target: {distance}");
        //Debug.Log($"Skill range: {range}");
        //Debug.Log($"Is target in range? {distance <= range}");

        return distance <= range;
    }

}
