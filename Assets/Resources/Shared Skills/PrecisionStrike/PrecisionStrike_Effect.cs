using UnityEngine;

public class PrecisionStrike_Effect : MonoBehaviour
{
    float additionalDmg;
    void OnEnable()
    {
        Debug.Log("PrecisionStrike_Effect");
        PlayerController player = GetComponentInParent<PlayerController>();
        BladeMaster_BasicAttack basicAttack = (BladeMaster_BasicAttack)player.cardsManager.BasicAttack.skillController;
        basicAttack.onAttack += DeactivateBuff;

        float dmg = player.GetDamage();
        additionalDmg = dmg / 2;

        player.AddDamage(additionalDmg);
    }

    public void DeactivateBuff()
    {
        Invoke(nameof(Deactivating), 0.5f);
    }

    private void Deactivating()
    {
        PlayerController player = GetComponentInParent<PlayerController>();
        BladeMaster_BasicAttack basicAttack = (BladeMaster_BasicAttack)player.cardsManager.BasicAttack.skillController;
        basicAttack.onAttack -= DeactivateBuff;

        player.ReduceDamage(additionalDmg);

        Destroy(gameObject);
    }
}
