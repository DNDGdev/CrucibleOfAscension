using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public PlayerController player => GetComponent<PlayerController>();

    [Header("Health Settings")]
    private int currentHealth;

    [Header("UI Reference")]
    public Image healthBar; // Assign this in the Inspector

    public System.Action OnDeath; // Event for when health reaches zero

    public void InitHealthBar()
    {

    }

    private void Start()
    {
        currentHealth = player.playerStats.combatStats.maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthBar();

        if (currentHealth == 0)
        {
            Die();
        }
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
    }
}
