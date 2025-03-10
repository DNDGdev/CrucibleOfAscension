using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    public Image img;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private Image cooldownImage;
    public CanvasGroup canvasGroup => GetComponent<CanvasGroup>(); // Assign the CanvasGroup in Inspector

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

    public void OnActivate()
    {
        canvasGroup.alpha = 1; // Start fully transparent
        canvasGroup.DOFade(0, 0.3f).OnComplete(() =>
        {
            gameObject.SetActive(false);
        }); // Fade in over 0.3 seconds
    }

    public void OnReset()
    {
        canvasGroup.alpha = 0; // Start fully transparent
        canvasGroup.DOFade(1, 0.3f); // Fade in over 0.3 seconds
    }
}
