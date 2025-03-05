using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    public Image img;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private Image cooldownImage;
    public void Init(Card _card)
    {
        img.sprite = _card.skill.skillIcon;
    }

    public void UpdateUI(float cooldownTimer, float amount)
    {
        cooldownText.text = Mathf.Ceil(cooldownTimer).ToString();
        cooldownImage.fillAmount = amount; 
    }

    public void SkillReady()
    {
        cooldownText.text = " ";
        cooldownImage.fillAmount = 0;
    }
}
