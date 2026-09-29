using System;
using System.Collections.Generic;
using System.Reflection;
using RuntimeInspectorNamespace;
using UnityEngine;
using UnityEngine.UI;

[RuntimeInspectorCustomEditor(typeof(ScenarioData))]
public class ScenarioDataRuntimeInspectorEditor : IRuntimeInspectorCustomEditor
{
    private const string StageButtonsRootName = "__stage_move_buttons";
    private const string StageAddButtonsRootName = "__stage_add_buttons";
    private const string SkillAddButtonsRootName = "__skill_add_buttons";
    private const string StageIdLabelName = "__stage_id_label";

    private static readonly HashSet<string> LockedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        nameof(ScenarioData.id).ToLowerInvariant(),
        nameof(ScenarioData.contentPackId).ToLowerInvariant()
    };

    private static Font _runtimeFont;
    private ObjectField _parent;

    public void GenerateElements(ObjectField parent)
    {
        _parent = parent;
        parent.CreateDrawersForVariables();
        ApplyLocks();
        ApplySkillsControls();
        ApplyStageControls();
    }

    public void Refresh()
    {
        ApplyLocks();
        ApplySkillsControls();
        ApplyStageControls();
    }

    private void ApplySkillsControls()
    {
        if (_parent?.Value is not ScenarioData scenario)
            return;

        scenario.skills ??= new List<SkillData>();

        var rootDrawArea = GetDrawArea(_parent);
        if (rootDrawArea == null)
            return;

        var skillsField = FindRootField(rootDrawArea, nameof(ScenarioData.skills));
        if (skillsField is not ExpandableInspectorField skillsExpandable)
            return;

        if (!skillsExpandable.IsExpanded)
            skillsExpandable.IsExpanded = true;

        var skillsDrawArea = GetDrawArea(skillsExpandable);
        if (skillsDrawArea == null)
            return;

        for (int i = 0; i < skillsDrawArea.childCount; i++)
        {
            var row = skillsDrawArea.GetChild(i);
            var rowField = row.GetComponent<InspectorField>();
            if (rowField == null || !IsStageElementRow(rowField.NameRaw))
                continue;

            if (row.TryGetComponent<ExpandableInspectorField>(out var rowExpandable) && !rowExpandable.IsExpanded)
                rowExpandable.IsExpanded = true;
        }

        LockListSizeInput(skillsExpandable, skillsDrawArea);
        ApplySkillsAddButtons(skillsDrawArea);
    }

    public void Cleanup()
    {
        _parent = null;
    }

    private void ApplyLocks()
    {
        if (_parent == null)
            return;

        var drawArea = GetDrawArea(_parent);
        if (drawArea == null)
            return;

        for (int i = 0; i < drawArea.childCount; i++)
        {
            var child = drawArea.GetChild(i);
            var inspectorField = child.GetComponent<InspectorField>();
            if (inspectorField == null)
                continue;

            if (!IsLockedField(inspectorField.NameRaw))
                continue;

            var selectables = child.GetComponentsInChildren<Selectable>(true);
            for (int j = 0; j < selectables.Length; j++)
                selectables[j].interactable = false;
        }
    }

    private void ApplyStageControls()
    {
        if (_parent?.Value is not ScenarioData scenario || scenario.stages == null)
            return;

        EnsureConcreteStageInstances(scenario);

        var rootDrawArea = GetDrawArea(_parent);
        if (rootDrawArea == null)
            return;

        var stagesField = FindRootField(rootDrawArea, nameof(ScenarioData.stages));

        if (stagesField is not ExpandableInspectorField stagesExpandable)
            return;

        if (!stagesExpandable.IsExpanded)
            stagesExpandable.IsExpanded = true;

        var stagesDrawArea = GetDrawArea(stagesExpandable);
        if (stagesDrawArea == null)
            return;

        LockListSizeInput(stagesExpandable, stagesDrawArea);

        int stageIndex = 0;
        for (int i = 0; i < stagesDrawArea.childCount; i++)
        {
            var row = stagesDrawArea.GetChild(i) as RectTransform;
            if (row == null)
                continue;

            var rowField = row.GetComponent<InspectorField>();
            if (rowField == null || !IsStageElementRow(rowField.NameRaw))
                continue;

            if (row.TryGetComponent<ExpandableInspectorField>(out var rowExpandable) && !rowExpandable.IsExpanded)
                rowExpandable.IsExpanded = true;

            ApplyUnlockSkillValidation(rowExpandable, scenario, stageIndex);

            var buttonsRoot = GetOrCreateButtonsRoot(row);

            var idText = GetOrCreateStageIdLabel(buttonsRoot);
            idText.text = GetStageMetaLabel(scenario, stageIndex);

            var upButton = GetOrCreateOverlayButton(buttonsRoot, "move_up", "↑", 164f, 24f);
            ConfigureMoveButton(upButton, stageIndex > 0, stageIndex, -1);

            var downButton = GetOrCreateOverlayButton(buttonsRoot, "move_down", "↓", 190f, 24f);
            ConfigureMoveButton(downButton, stageIndex < scenario.stages.Count - 1, stageIndex, +1);

            var deleteButton = GetOrCreateOverlayButton(buttonsRoot, "delete", "✕", 216f, 24f);
            ConfigureDeleteButton(deleteButton, scenario.stages.Count > 0, stageIndex);

            stageIndex++;
        }

        ApplyStageAddButtons(stagesDrawArea);
    }

    private static InspectorField FindRootField(RectTransform rootDrawArea, string fieldName)
    {
        for (int i = 0; i < rootDrawArea.childCount; i++)
        {
            var field = rootDrawArea.GetChild(i).GetComponent<InspectorField>();
            if (field != null && IsFieldName(field.NameRaw, fieldName))
                return field;
        }

        return null;
    }

    private void LockListSizeInput(ExpandableInspectorField listExpandable, RectTransform listDrawArea)
    {
        var boundInputs = listExpandable.GetComponentsInChildren<BoundInputField>(true);
        for (int i = 0; i < boundInputs.Length; i++)
        {
            var boundInput = boundInputs[i];
            if (boundInput == null || boundInput.BackingField == null)
                continue;

            if (boundInput.transform.IsChildOf(listDrawArea))
                continue;

            boundInput.BackingField.interactable = false;
        }
    }

    private void ApplySkillsAddButtons(RectTransform skillsDrawArea)
    {
        var row = GetOrCreateAddButtonsRow(skillsDrawArea, SkillAddButtonsRootName);

        var skillButton = GetOrCreateRowButton(row, "add_skill", "Add Skill", 96f);
        ConfigureAddButton(skillButton, AddSkill);

        row.SetAsLastSibling();
    }

    private void ApplyStageAddButtons(RectTransform stagesDrawArea)
    {
        var row = GetOrCreateAddButtonsRow(stagesDrawArea, StageAddButtonsRootName);

        var dialogueButton = GetOrCreateRowButton(row, "add_dialogue", "Add Dialogue", 110f);
        ConfigureAddButton(dialogueButton, () => AddStage(new ScenarioStageDialogueData()));

        var decisionButton = GetOrCreateRowButton(row, "add_decision", "Add Decision", 110f);
        ConfigureAddButton(decisionButton, () => AddStage(new ScenarioStageDecisionData()));

        var questionButton = GetOrCreateRowButton(row, "add_question", "Add Question", 110f);
        ConfigureAddButton(questionButton, () => AddStage(new ScenarioStageQuestionData()));

        var unlockButton = GetOrCreateRowButton(row, "add_unlock", "Add Skill Unlock", 130f);
        ConfigureAddButton(unlockButton, () => AddStage(new ScenarioStageUnlockSkillData()));

        var mapButton = GetOrCreateRowButton(row, "add_map", "Add Map Interaction", 150f);
        ConfigureAddButton(mapButton, () => AddStage(new ScenarioStageMapInteractionData()));

        row.SetAsLastSibling();
    }

    private RectTransform GetOrCreateAddButtonsRow(RectTransform drawArea, string rowName)
    {
        var existing = drawArea.Find(rowName) as RectTransform;
        if (existing != null)
            return existing;

        var go = new GameObject(rowName, typeof(RectTransform), typeof(LayoutElement), typeof(HorizontalLayoutGroup));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(drawArea, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(0f, 24f);

        var layoutElement = go.GetComponent<LayoutElement>();
        layoutElement.minHeight = 24f;
        layoutElement.preferredHeight = 24f;
        layoutElement.flexibleWidth = 1f;

        var layout = go.GetComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleRight;
        layout.childControlHeight = true;
        layout.childControlWidth = false;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        layout.spacing = 4f;
        layout.padding = new RectOffset(0, 4, 2, 2);

        return rect;
    }

    private Button GetOrCreateRowButton(RectTransform parent, string objectName, string label, float width)
    {
        Button button;
        var existing = parent.Find(objectName);

        if (existing != null && existing.TryGetComponent<Button>(out var existingButton))
        {
            button = existingButton;
        }
        else
        {
            var buttonGo = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            button = buttonGo.GetComponent<Button>();

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.SetParent(buttonGo.transform, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textGo.GetComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = GetRuntimeFont();
            text.raycastTarget = false;
        }

        var buttonRect = button.GetComponent<RectTransform>();
        buttonRect.SetParent(parent, false);
        buttonRect.sizeDelta = new Vector2(width, 18f);

        var layoutElement = button.GetComponent<LayoutElement>();
        layoutElement.minWidth = width;
        layoutElement.preferredWidth = width;
        layoutElement.flexibleWidth = 0f;
        layoutElement.minHeight = 18f;
        layoutElement.preferredHeight = 18f;

        var image = button.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.18f);

        var labelText = button.GetComponentInChildren<Text>(true);
        if (labelText != null)
            labelText.text = label;

        return button;
    }

    private RectTransform GetOrCreateButtonsRoot(RectTransform row)
    {
        var existing = row.Find(StageButtonsRootName) as RectTransform;
        RectTransform rect;
        LayoutElement layoutElement;

        if (existing != null)
        {
            rect = existing;
            layoutElement = existing.GetComponent<LayoutElement>();
            if (layoutElement == null)
                layoutElement = existing.gameObject.AddComponent<LayoutElement>();
        }
        else
        {
            var go = new GameObject(StageButtonsRootName, typeof(RectTransform), typeof(LayoutElement));
            rect = go.GetComponent<RectTransform>();
            rect.SetParent(row, false);
            layoutElement = go.GetComponent<LayoutElement>();
        }

        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.sizeDelta = new Vector2(244f, 20f);
        rect.anchoredPosition = new Vector2(-4f, -2f);
        layoutElement.ignoreLayout = true;

        return rect;
    }

    private Text GetOrCreateStageIdLabel(RectTransform parent)
    {
        var existing = parent.Find(StageIdLabelName);
        Text text;

        if (existing != null && existing.TryGetComponent<Text>(out var existingText))
        {
            text = existingText;
        }
        else
        {
            var textGo = new GameObject(StageIdLabelName, typeof(RectTransform), typeof(Text));
            text = textGo.GetComponent<Text>();
            text.font = GetRuntimeFont();
            text.alignment = TextAnchor.MiddleLeft;
            text.color = new Color(1f, 1f, 1f, 0.85f);
            text.raycastTarget = false;
        }

        var rect = text.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.sizeDelta = new Vector2(160f, 18f);
        rect.anchoredPosition = new Vector2(0f, 0f);

        return text;
    }

    private static string GetStageMetaLabel(ScenarioData scenario, int index)
    {
        if (scenario == null || scenario.stages == null || index < 0 || index >= scenario.stages.Count)
            return "Unknown | ------";

        var stage = scenario.stages[index];
        if (stage == null)
            return "Unknown | ------";

        var id = string.IsNullOrWhiteSpace(stage.id) ? "------" : stage.id;
        var type = stage.stageType switch
        {
            ScenarioStageData.STAGE_TYPE.DIALOGUE => "Dialogue",
            ScenarioStageData.STAGE_TYPE.DECISION => "Decision",
            ScenarioStageData.STAGE_TYPE.QUESTION => "Question",
            ScenarioStageData.STAGE_TYPE.UNLOCK_SKILL => "Skill Unlock",
            ScenarioStageData.STAGE_TYPE.MAP_INTERACTION => "Map Interaction",
            _ => "Unknown"
        };

        return $"{type} | {id}";
    }

    private Button GetOrCreateOverlayButton(RectTransform parent, string objectName, string label, float x, float width)
    {
        Button button;
        var existing = parent.Find(objectName);

        if (existing != null && existing.TryGetComponent<Button>(out var existingButton))
        {
            button = existingButton;
        }
        else
        {
            var buttonGo = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            button = buttonGo.GetComponent<Button>();

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.SetParent(buttonGo.transform, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textGo.GetComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = GetRuntimeFont();
            text.raycastTarget = false;
        }

        var buttonRect = button.GetComponent<RectTransform>();
        buttonRect.SetParent(parent, false);
        buttonRect.anchorMin = new Vector2(0f, 0.5f);
        buttonRect.anchorMax = new Vector2(0f, 0.5f);
        buttonRect.pivot = new Vector2(0f, 0.5f);
        buttonRect.sizeDelta = new Vector2(width, 18f);
        buttonRect.anchoredPosition = new Vector2(x, 0f);

        var image = button.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.18f);

        var labelText = button.GetComponentInChildren<Text>(true);
        if (labelText != null)
            labelText.text = label;

        return button;
    }

    private static bool IsStageElementRow(string nameRaw)
    {
        if (string.IsNullOrWhiteSpace(nameRaw))
            return false;

        nameRaw = nameRaw.Trim();
        return nameRaw.EndsWith(":");
    }

    private void AddStage(ScenarioStageData stage)
    {
        if (_parent?.Value is not ScenarioData scenario || scenario.stages == null || stage == null)
            return;

        if (string.IsNullOrWhiteSpace(stage.id))
            stage.id = GenerateShortStageId(scenario);

        scenario.stages.Add(stage);
        _parent.Inspector.Refresh();
    }

    private void AddSkill()
    {
        if (_parent?.Value is not ScenarioData scenario)
            return;

        scenario.skills ??= new List<SkillData>();

        scenario.skills.Add(new SkillData
        {
            id = GenerateShortSkillId(scenario)
        });

        _parent.Inspector.Refresh();
    }

    private static void EnsureConcreteStageInstances(ScenarioData scenario)
    {
        for (int i = 0; i < scenario.stages.Count; i++)
        {
            if (scenario.stages[i] == null)
            {
                scenario.stages[i] = new ScenarioStageDialogueData
                {
                    id = GenerateShortStageId(scenario)
                };
            }
            else if (string.IsNullOrWhiteSpace(scenario.stages[i].id))
            {
                scenario.stages[i].id = GenerateShortStageId(scenario);
            }
        }
    }

    private static string GenerateShortStageId(ScenarioData scenario)
    {
        const int maxAttempts = 32;
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (scenario?.stages != null)
        {
            for (int i = 0; i < scenario.stages.Count; i++)
            {
                var id = scenario.stages[i]?.id;
                if (!string.IsNullOrWhiteSpace(id))
                    used.Add(id);
            }
        }

        for (int i = 0; i < maxAttempts; i++)
        {
            var candidate = Guid.NewGuid().ToString("N").Substring(0, 6);
            if (!used.Contains(candidate))
                return candidate;
        }

        return Guid.NewGuid().ToString("N").Substring(0, 6);
    }

    private static string GenerateShortSkillId(ScenarioData scenario)
    {
        const string prefix = "skill_";
        var usedNumbers = new HashSet<int>();

        if (scenario?.skills != null)
        {
            for (int i = 0; i < scenario.skills.Count; i++)
            {
                var id = scenario.skills[i]?.id;
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                id = id.Trim();
                if (!id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                var numberPart = id.Substring(prefix.Length);
                if (int.TryParse(numberPart, out var parsedNumber) && parsedNumber > 0)
                    usedNumbers.Add(parsedNumber);
            }
        }

        var nextNumber = 1;
        while (usedNumbers.Contains(nextNumber))
            nextNumber++;

        return $"{prefix}{nextNumber:00}";
    }

    private void ApplyUnlockSkillValidation(ExpandableInspectorField rowExpandable, ScenarioData scenario, int stageIndex)
    {
        if (rowExpandable == null || scenario?.stages == null || stageIndex < 0 || stageIndex >= scenario.stages.Count)
            return;

        var rowDrawArea = GetDrawArea(rowExpandable);
        if (rowDrawArea == null)
            return;

        var unlockStage = scenario.stages[stageIndex] as ScenarioStageUnlockSkillData;
        var isUnlockStage = unlockStage != null;
        for (int i = 0; i < rowDrawArea.childCount; i++)
        {
            var field = rowDrawArea.GetChild(i).GetComponent<InspectorField>();
            if (field == null || !IsFieldName(field.NameRaw, nameof(ScenarioStageUnlockSkillData.skillId)))
                continue;

            var input = field.GetComponentInChildren<BoundInputField>(true);
            if (input?.BackingField != null)
                input.BackingField.interactable = true;

            SetInputValidationState(input, isUnlockStage && IsKnownSkillId(scenario, unlockStage.skillId));
        }
    }

    private static bool IsKnownSkillId(ScenarioData scenario, string skillId)
    {
        if (scenario?.skills == null || string.IsNullOrWhiteSpace(skillId))
            return false;

        for (int i = 0; i < scenario.skills.Count; i++)
        {
            var knownId = scenario.skills[i]?.id;
            if (!string.IsNullOrWhiteSpace(knownId) && string.Equals(knownId, skillId, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static void SetInputValidationState(BoundInputField input, bool isValid)
    {
        if (input == null)
            return;

        var background = input.GetComponent<Image>();
        if (background == null)
            return;

        if (input.Skin != null)
            background.color = isValid ? input.Skin.InputFieldNormalBackgroundColor : input.Skin.InputFieldInvalidBackgroundColor;
        else
            background.color = isValid ? Color.white : new Color(1f, 0.45f, 0.45f, 1f);
    }

    private void ConfigureMoveButton(Button button, bool interactable, int index, int delta)
    {
        button.interactable = interactable;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => MoveStage(index, index + delta));
    }

    private void ConfigureDeleteButton(Button button, bool interactable, int index)
    {
        button.interactable = interactable;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => DeleteStage(index));
    }

    private static void ConfigureAddButton(Button button, Action onClick)
    {
        button.interactable = true;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
    }

    private static Font GetRuntimeFont()
    {
        if (_runtimeFont != null)
            return _runtimeFont;

        _runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (_runtimeFont == null)
            _runtimeFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        return _runtimeFont;
    }

    private void MoveStage(int from, int to)
    {
        if (_parent?.Value is not ScenarioData scenario || scenario.stages == null)
            return;

        if (from < 0 || from >= scenario.stages.Count || to < 0 || to >= scenario.stages.Count)
            return;

        var tmp = scenario.stages[from];
        scenario.stages[from] = scenario.stages[to];
        scenario.stages[to] = tmp;

        _parent.Inspector.Refresh();
    }

    private void DeleteStage(int index)
    {
        if (_parent?.Value is not ScenarioData scenario || scenario.stages == null)
            return;

        if (index < 0 || index >= scenario.stages.Count)
            return;

        scenario.stages.RemoveAt(index);
        _parent.Inspector.Refresh();
    }

    private static RectTransform GetDrawArea(ExpandableInspectorField field)
    {
        var drawAreaField = typeof(ExpandableInspectorField).GetField("drawArea", BindingFlags.Instance | BindingFlags.NonPublic);
        return drawAreaField?.GetValue(field) as RectTransform;
    }

    private static bool IsLockedField(string displayedName)
    {
        if (string.IsNullOrWhiteSpace(displayedName))
            return false;

        var normalized = NormalizeName(displayedName);
        return LockedFields.Contains(normalized);
    }

    private static bool IsFieldName(string displayedName, string expectedFieldName)
    {
        if (string.IsNullOrWhiteSpace(displayedName))
            return false;

        return string.Equals(NormalizeName(displayedName), NormalizeName(expectedFieldName), StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeName(string name)
    {
        return name
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .ToLowerInvariant();
    }
}