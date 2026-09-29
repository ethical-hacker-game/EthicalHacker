using UnityEngine;
using System.Collections.Generic;

public class StoryUI : UIScreen
{
    [SerializeField] private Transform storyElementsParent;
    [SerializeField] private GameObject storyElement;

    private List<SaveInfo.DecisionInfo> Decisions => SaveLoad.CurrentSaveInfo?.decisions;

    public void ShowGameUI()
    {
        UIManager.Instance.ShowScreen("GameUI");
    }

    private void OnEnable()
    {
        if (Application.isPlaying == false)
            return;

        PrepareStoryElements();
    }

    private void PrepareStoryElements()
    {
        for (int i = storyElementsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(storyElementsParent.GetChild(i).gameObject);
        }

        if (Decisions == null || Decisions?.Count == 0)
        {
            StoryUI_StoryElement newElement = Instantiate(storyElement, storyElementsParent).GetComponent<StoryUI_StoryElement>();
            newElement.SetupStoryElement(1, 0, "No decisions made yet.");
            return;
        }

        for (int i = 0; i < Decisions.Count; i++)
        {
            SaveInfo.DecisionInfo d = Decisions[i];
            StoryUI_StoryElement newElement = Instantiate(storyElement, storyElementsParent).GetComponent<StoryUI_StoryElement>();
            newElement.SetupStoryElement(d.optionsCount, d.optionIndex, d.FullText);
        }
    }
}
