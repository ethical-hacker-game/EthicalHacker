using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SkillManager;

public class SkillsUI_Skill : MonoBehaviour
{
    [SerializeField] private Color lockedColorButton;
    [SerializeField] private Color activatedColorButton;
    [Space]
    [SerializeField] private ButtonImproved buttonUnlock;
    [SerializeField] private TextMeshProUGUI descriptionTMP;

    private SkillData skillData;

    public void SetupSkillButton(SkillData skillData, SkillStatus skillStatus)
    {
        this.skillData = skillData;

        descriptionTMP.text = $"<b>{skillData.name?.ToUpper()}</b>\n{skillData.description}";

        if (skillStatus != SkillStatus.Active)
            descriptionTMP.text += $"\n<i>Cost: {skillData.cost}</i>";

        ColorBlock btnColors;

        switch (skillStatus)
        {
            case SkillStatus.Locked:
                buttonUnlock.ButtonComponent.interactable = false;
                buttonUnlock.ButtonText = "Locked";
                btnColors = buttonUnlock.ButtonComponent.colors;
                btnColors.disabledColor = lockedColorButton;
                buttonUnlock.ButtonComponent.colors = btnColors;
                break;
            case SkillStatus.Available:
                buttonUnlock.ButtonComponent.interactable = true;
                buttonUnlock.ButtonText = "Unlock";
                break;
            case SkillStatus.Active:
                buttonUnlock.ButtonComponent.interactable = false;
                buttonUnlock.ButtonText = "<b>ACTIVE</b>";
                btnColors = buttonUnlock.ButtonComponent.colors;
                btnColors.disabledColor = activatedColorButton;
                buttonUnlock.ButtonComponent.colors = btnColors;
                break;
            default:
                break;
        }
    }

    public void ClickUnlock()
    {
        SetupSkillButton(skillData, SkillStatus.Active);
        SkillManager.Instance.ActivatedSkills.Add(skillData.id);
        ApplySkillCost();
    }

    private void ApplySkillCost()
    {
        if (skillData == null || SaveLoad.CurrentSaveInfo == null)
            return;

        SaveLoad.CurrentSaveInfo.statMoney -= skillData.cost;
        SaveLoad.NotifyStatsChanged();
    }
}
