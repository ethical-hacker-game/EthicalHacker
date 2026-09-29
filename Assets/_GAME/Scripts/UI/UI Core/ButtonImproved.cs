using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonImproved : MonoBehaviour
{
    private Button button;
    private TextMeshProUGUI tmp;

    public Button ButtonComponent
    {
        get
        {
            if (button == null)
                button = GetComponent<Button>();

            return button;
        }
    }
    public UnityEvent OnClick
    {
        get
        {
            if (button == null)
                button = GetComponent<Button>();

            return button.onClick;
        }
    }

    public string ButtonText
    {
        get
        {
            if(tmp == null)
                tmp = GetComponentInChildren<TextMeshProUGUI>();
            
            return tmp.text;
        }

        set
        {
            if (tmp == null)
                tmp = GetComponentInChildren<TextMeshProUGUI>();

            tmp.SetText(value);
        }
    }

    public ContentSizeFitter ContentSizeFitter
    {
        get
        {
            return GetComponent<ContentSizeFitter>();
        }
    }

}
