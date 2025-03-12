using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HealthManager : MonoBehaviour ,IDamageable
{
    public PlayerController player => GetComponent<PlayerController>();
    public GameObject PopUp;
    public Transform spawnPoint;
    public DamageableItem damageableItem;
    public HitState hitState;

    [Header("Health Settings")]
    [SerializeField] private int currentHealth;

    [Header("UI Reference")]
    public Image healthBar; // Assign this in the Inspector

    public System.Action OnDeath; // Event for when health reaches zero

    private void Start()
    {
        if (damageableItem == DamageableItem.Player)
            currentHealth = player.playerStats.combatStats.maxHealth;

        hitState = HitState.Hittable;
        UpdateHealthBar();
    }

    public void GetHit(DamageItem damageItem)
    {
        switch (hitState)
        {
            case HitState.Hittable:
                Instantiate(damageItem.HitEffect, transform.position, Quaternion.identity);
                TakeDamage((int)damageItem.DamageAmount);
                break;
            case HitState.Blocked:
                ShowTextPopup("Blocked", Color.white);
                hitState = HitState.Hittable;
                break;
            case HitState.Invulnerable:
                ShowTextPopup("Invulnerable", Color.cyan);

                break;
            default:
                break;
        }
      
    }

    public int excessDamage = 0; // Store excess damage

    public void TakeDamage(int damage)
    {
        Debug.Log($"Took damage: {damage}");

        // Calculate excess damage
        excessDamage = Mathf.Max(0, damage - currentHealth);

        // Reduce health
        currentHealth = Mathf.Max(0, currentHealth - damage);

        // Update UI
        UpdateHealthBar();
        ShowDamagePopup(damage);

        // Handle death
        if (currentHealth == 0)
        {
            Die();
        }
    }

    private void ShowDamagePopup(int damage)
    {
        GameObject popUp = Instantiate(PopUp, spawnPoint.position, Quaternion.identity, spawnPoint);
        TextMeshProUGUI txt = popUp.GetComponent<TextMeshProUGUI>();
        txt.text = damage.ToString();
        txt.color = (damageableItem == DamageableItem.shield) ? Color.blue : Color.red;
        popUp.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        // Fade out TMP text and destroy after completion
        txt.DOFade(0, 0.5f).OnComplete(() => Destroy(popUp));
    }

    private void ShowTextPopup(string _txt ,Color color )
    {
        GameObject popUp = Instantiate(PopUp, spawnPoint.position, Quaternion.identity, spawnPoint);
        TextMeshProUGUI txt = popUp.GetComponent<TextMeshProUGUI>();
        txt.text = _txt;
        txt.color = color;
        popUp.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        // Fade out TMP text and destroy after completion
        txt.DOFade(0, 0.5f).OnComplete(() => Destroy(popUp));
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > player.playerStats.combatStats.maxHealth) currentHealth = player.playerStats.combatStats.maxHealth;

        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)currentHealth / player.playerStats.combatStats.maxHealth;
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has died!");
        OnDeath?.Invoke(); // Notify other scripts (like respawn system)

        if(damageableItem == DamageableItem.shield)
        {
            if(excessDamage >0)
            {
                PlayerController _player = GetComponentInParent<PlayerController>();
                _player.healthManager?.TakeDamage(excessDamage);
            }
            Destroy(gameObject);
        }
    }
}

public enum DamageableItem
{
    Player,
    shield,
    obstacle
}

public enum HitState
{
    Hittable,      // The player can be hit and take damage.
    Blocked,       // The player blocked the attack and takes no damage.
    Invulnerable   // The player cannot be hit or damaged.
}
