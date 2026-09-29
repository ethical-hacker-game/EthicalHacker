using UnityEngine;

public class SkillsUI : UIScreen
{
    [SerializeField] private Transform skillsParent;
    [SerializeField] private SkillsUI_Skill skillButtonPrefab;

    public void ShowGameUI()
    {
        UIManager.Instance.ShowScreen("GameUI");
    }

    private void OnEnable()
    {
        if (Application.isPlaying == false)
            return;

        PrepareSkillButtons();
    }

    private void PrepareSkillButtons()
    {
        for (int i = skillsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(skillsParent.GetChild(i).gameObject);
        }

        for (int i = 0; i < SkillManager.Instance.Skills.Count; i++)
        {
            SkillData skillData = SkillManager.Instance.Skills[i];
            SkillsUI_Skill newButton = Instantiate(skillButtonPrefab, skillsParent);
            newButton.SetupSkillButton(skillData, SkillManager.GetSkillStatus(skillData.id));
        }
    }
}
