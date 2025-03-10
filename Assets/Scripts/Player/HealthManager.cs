using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HealthManager : MonoBehaviour
{
    public PlayerController player => GetComponent<PlayerController>();
    public GameObject PopUp;
    public Transform spawnPoint;

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
        Debug.Log("took dammage" + damage);
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthBar();
        GameObject popUp =  Instantiate(PopUp, spawnPoint.position, Quaternion.identity, spawnPoint);
        TextMeshProUGUI txt = popUp.GetComponent<TextMeshProUGUI>();
        txt.text = damage.ToString();
        popUp.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        // Fade out TMP text and destroy after completion
        txt.DOFade(0, 0.5f).OnComplete(() => Destroy(txt.gameObject));
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
