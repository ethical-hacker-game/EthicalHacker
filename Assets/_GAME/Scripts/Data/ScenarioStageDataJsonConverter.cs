using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public sealed class ScenarioStageDataJsonConverter : JsonConverter<ScenarioStageData>
{
    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, ScenarioStageData value, JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }

    public override ScenarioStageData ReadJson(JsonReader reader, Type objectType, ScenarioStageData existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        JObject obj = JObject.Load(reader);
        ScenarioStageData.STAGE_TYPE stageType = ResolveStageType(obj);
        ScenarioStageData stage = CreateByType(stageType);

        using var objReader = obj.CreateReader();
        serializer.Populate(objReader, stage);

        return stage;
    }

    private static ScenarioStageData.STAGE_TYPE ResolveStageType(JObject obj)
    {
        JToken stageTypeToken = obj["stageType"];

        if (stageTypeToken != null)
        {
            if (stageTypeToken.Type == JTokenType.Integer)
                return (ScenarioStageData.STAGE_TYPE)stageTypeToken.Value<int>();

            if (stageTypeToken.Type == JTokenType.String &&
                Enum.TryParse(stageTypeToken.Value<string>(), true, out ScenarioStageData.STAGE_TYPE parsedType))
            {
                return parsedType;
            }
        }

        if (obj["options"] != null || obj["decisionText"] != null)
            return ScenarioStageData.STAGE_TYPE.DECISION;

        if (obj["questionText"] != null || obj["answers"] != null)
            return ScenarioStageData.STAGE_TYPE.QUESTION;

        if (obj["skillId"] != null)
            return ScenarioStageData.STAGE_TYPE.UNLOCK_SKILL;

        if (obj["latitude"] != null || obj["longitude"] != null || obj["locationId"] != null)
            return ScenarioStageData.STAGE_TYPE.MAP_INTERACTION;

        return ScenarioStageData.STAGE_TYPE.DIALOGUE;
    }

    private static ScenarioStageData CreateByType(ScenarioStageData.STAGE_TYPE stageType)
    {
        return stageType switch
        {
            ScenarioStageData.STAGE_TYPE.DIALOGUE => new ScenarioStageDialogueData(),
            ScenarioStageData.STAGE_TYPE.DECISION => new ScenarioStageDecisionData(),
            ScenarioStageData.STAGE_TYPE.QUESTION => new ScenarioStageQuestionData(),
            ScenarioStageData.STAGE_TYPE.UNLOCK_SKILL => new ScenarioStageUnlockSkillData(),
            ScenarioStageData.STAGE_TYPE.MAP_INTERACTION => new ScenarioStageMapInteractionData(),
            _ => throw new JsonSerializationException($"Unsupported stage type: {stageType}")
        };
    }
}
