using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationElementUI : MonoBehaviour
{
    public static NotificationElementUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI notificationTMP;

    public void ShowNotification(string content)
    {
        gameObject.SetActive(true);
        notificationTMP.SetText(content);
    }

    public void HideNotification()
    {
        gameObject.SetActive(false);
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        HideNotification();
    }
}
