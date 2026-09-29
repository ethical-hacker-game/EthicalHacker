using TMPro;
using UnityEngine;

public class StoryUI_StoryElement : MonoBehaviour
{
    [SerializeField] private Transform elementsParent;
    [SerializeField] private GameObject elementVisiblePrefab;
    [SerializeField] private GameObject elementHiddenPrefab;

    public void SetupStoryElement(int allElementsCount, int visibleElementIndex, string elementText)
    {
        for (int i = elementsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(elementsParent.GetChild(i).gameObject);
        }

        for (int i = 0; i < allElementsCount; i++)
        {
            if(i == visibleElementIndex)
            {
                GameObject newElement = Instantiate(elementVisiblePrefab, elementsParent);
                newElement.GetComponentInChildren<TextMeshProUGUI>().SetText(elementText);
            }
            else
            {
                Instantiate(elementHiddenPrefab, elementsParent);
            }
        }
    }
}
