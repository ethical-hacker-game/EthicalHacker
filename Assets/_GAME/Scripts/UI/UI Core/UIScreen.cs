using UnityEngine;

public class UIScreen : MonoBehaviour
{
    [SerializeField] private bool isDefaultScreen;
    [SerializeField] private bool dontHideOtherUIs;

    public string ScreenName => gameObject.name;
    public bool IsDefaultScreen => isDefaultScreen;
    public bool DontHideOtherUIs => dontHideOtherUIs;
}
