using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardsManager : MonoBehaviour
{
  
    [Header("Skills")]
    public List<Transform> CardSlots = new List<Transform>();
    public Transform SlotsHolder;

    public List<Card> SkillCards = new List<Card>();
    public List<SkillItem> AllSkills = new List<SkillItem>();
    public List<SkillItem> BoardSkills = new List<SkillItem>();

    public SkillController ActiveSkill;

    private void Start()
    {
        CreateSkills();
        InitBoardSkills();
    }

    public void CreateSkills()
    {
        foreach (var c in SkillCards)
        {
            GameObject newSkill = Instantiate(c.SkillPrefab, SlotsHolder);
            newSkill.SetActive(false);

            AllSkills.Add(new SkillItem(c, newSkill.GetComponent<SkillController>()));
        }
    }

    public void InitBoardSkills()
    {
        BoardSkills = GetRandomSkills(5);

        for (int i = 0; i < BoardSkills.Count; i++)
        {
            var skillItem = BoardSkills[i];

            skillItem.skillController.transform.SetParent(CardSlots[i].transform);
            skillItem.skillController.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            skillItem.skillController.gameObject.SetActive(true);
            skillItem.skillController.InitSkill(skillItem.card);
        }
    }

    public List<SkillItem> GetRandomSkills(int count)
    {
        // Ensure we don't request more skills than available
        int takeCount = Mathf.Min(count, AllSkills.Count);

        // Fisher-Yates shuffle
        List<SkillItem> shuffledList = new List<SkillItem>(AllSkills);
        int n = shuffledList.Count;
        for (int i = 0; i < n - 1; i++)
        {
            int r = Random.Range(i, n);
            (shuffledList[i], shuffledList[r]) = (shuffledList[r], shuffledList[i]); // Swap
        }

        // Get selected skills and remove them from AllSkills
        List<SkillItem> selectedSkills = shuffledList.GetRange(0, takeCount);
        foreach (var skill in selectedSkills)
        {
            AllSkills.Remove(skill);
        }

        return selectedSkills;
    }

}

[System.Serializable]
public class SkillItem
{
    public Card card;
    public SkillController skillController;

    public SkillItem(Card _card, SkillController _skillController)
    {
        card = _card;
        skillController = _skillController;
    }
}

