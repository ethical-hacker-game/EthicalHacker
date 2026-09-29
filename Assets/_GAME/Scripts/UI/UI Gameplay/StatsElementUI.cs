using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatsElementUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyLabel;
    [SerializeField] private TextMeshProUGUI honorLabel;
    [SerializeField] private TextMeshProUGUI legendLabel;
    [SerializeField] private TextMeshProUGUI progressLabel;
    [SerializeField] private List<HonorDescriptorRange> honorDescriptors = new List<HonorDescriptorRange>
    {
        new HonorDescriptorRange { minInclusive = int.MinValue, maxInclusive = -50, label = "Outlaw" },
        new HonorDescriptorRange { minInclusive = -49, maxInclusive = -10, label = "Rogue" },
        new HonorDescriptorRange { minInclusive = -9, maxInclusive = 9, label = "Drifter" },
        new HonorDescriptorRange { minInclusive = 10, maxInclusive = 49, label = "Citizen" },
        new HonorDescriptorRange { minInclusive = 50, maxInclusive = int.MaxValue, label = "Paragon" }
    };

    [Serializable]
    private class HonorDescriptorRange
    {
        public int minInclusive;
        public int maxInclusive;
        public string label;
    }

    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            SaveLoad.StatsChanged += Refresh;
            Refresh();
        }
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
        {
            SaveLoad.StatsChanged -= Refresh;
        }
    }

    private void Refresh()
    {
        SaveInfo saveInfo = SaveLoad.CurrentSaveInfo;

        int money = saveInfo != null ? saveInfo.statMoney : 0;
        int honor = saveInfo != null ? saveInfo.statHonor : 0;
        int legend = saveInfo != null ? saveInfo.statLegend : 0;

        string honorDescriptor = ResolveHonorDescriptor(honor);

        if (moneyLabel != null)
            moneyLabel.text = $"Money: <b>{money}</b>";

        if (honorLabel != null)
            honorLabel.text = string.IsNullOrEmpty(honorDescriptor)
                ? $"Honor: <b>{honor}</b>"
                : $"Honor: <b>{honor}</b> (<b>{honorDescriptor}</b>)";

        if (legendLabel != null)
            legendLabel.text = $"Legend: <b>{legend}</b>";

        if (progressLabel != null)
            progressLabel.text = $"{saveInfo?.ProgressPercent ?? 0}%";
    }

    private string ResolveHonorDescriptor(int honorValue)
    {
        if (honorDescriptors == null || honorDescriptors.Count == 0)
            return string.Empty;

        for (int i = 0; i < honorDescriptors.Count; i++)
        {
            HonorDescriptorRange range = honorDescriptors[i];
            if (honorValue >= range.minInclusive && honorValue <= range.maxInclusive)
                return range.label ?? string.Empty;
        }

        return string.Empty;
    }
}
