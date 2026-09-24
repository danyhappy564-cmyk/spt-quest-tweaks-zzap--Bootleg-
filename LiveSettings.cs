using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;


namespace sgtlaggyQuestTweaks;

/// <summary>
/// The part of config.json that the client plugin (F12 menu) can edit live.
/// exemptQuests / onlyQuests / questOverrides stay file-only.
/// </summary>
public record LiveSettings : IRequestData
{
    [JsonPropertyName("QualityOfLife")]
    public QualityOfLifeConfig? QualityOfLife { get; set; }

    [JsonPropertyName("GlobalConditions")]
    public GlobalConditionsConfig? GlobalConditions { get; set; }

    [JsonPropertyName("SpecialCases")]
    public SpecialCasesConfig? SpecialCases { get; set; }
}

public record LiveSettingsResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = "";

    [JsonPropertyName("settings")]
    public LiveSettings? Settings { get; set; }

    // Locale entries ("kr") that changed; the client merges them into its loaded locale.
    [JsonPropertyName("localeChanges")]
    public Dictionary<string, string>? LocaleChanges { get; set; }

    // Objective id -> relaxed labels; the client appends "[퀘스트 완화됨: ...]" when displaying.
    [JsonPropertyName("tags")]
    public Dictionary<string, string>? Tags { get; set; }

    // Changes made to already received quests in the profile by this call (game restart needed to see them).
    [JsonPropertyName("profileFixes")]
    public int ProfileFixes { get; set; }
}

[Injectable(InjectionType.Singleton)]
public class LiveSettingsService(
    ISptLogger<LiveSettingsService> logger,
    Config config,
    QuestTweaksService questTweaks
)
{
    private readonly object _lock = new();

    public LiveSettingsResponse Get(MongoId sessionId)
    {
        lock (_lock)
        {
            // a mod that loads after us may have added quests; include them
            if (questTweaks.HasNewQuests())
            {
                questTweaks.Apply();
                logger.Info("[QuestTweaks] quests added by other mods detected, quest tweaks re-applied");
            }

            var fixes = questTweaks.FixProfile(sessionId);
            return new LiveSettingsResponse { Ok = true, Settings = Current(), ProfileFixes = fixes, Tags = questTweaks.GetTagLabels(sessionId) };
        }
    }

    public LiveSettingsResponse Tags(MongoId sessionId)
    {
        lock (_lock)
        {
            var fixes = questTweaks.FixProfile(sessionId);
            return new LiveSettingsResponse { Ok = true, ProfileFixes = fixes, Tags = questTweaks.GetTagLabels(sessionId) };
        }
    }

    public LiveSettingsResponse Set(LiveSettings request, MongoId sessionId)
    {
        lock (_lock)
        {
            if (request.QualityOfLife is not null)
            {
                config.QualityOfLife = request.QualityOfLife;
            }
            if (request.GlobalConditions is not null)
            {
                config.GlobalConditions = request.GlobalConditions;
            }
            if (request.SpecialCases is not null)
            {
                config.SpecialCases = request.SpecialCases;
            }

            var message = "적용 완료";
            try
            {
                ConfigRegistration.Save(config);
            }
            catch (Exception ex)
            {
                message = $"적용 완료 (config.json 저장 실패: {ex.Message})";
                logger.Error($"[QuestTweaks] failed to save config.json: {ex}");
            }

            var localeChanges = questTweaks.Apply();
            var fixes = questTweaks.FixProfile(sessionId);
            logger.Info("[QuestTweaks] settings changed from F12 menu, quest tweaks re-applied");

            return new LiveSettingsResponse
            {
                Ok = true,
                Message = message,
                Settings = Current(),
                LocaleChanges = localeChanges,
                ProfileFixes = fixes,
                Tags = questTweaks.GetTagLabels(sessionId)
            };
        }
    }

    private LiveSettings Current() => new()
    {
        QualityOfLife = config.QualityOfLife,
        GlobalConditions = config.GlobalConditions,
        SpecialCases = config.SpecialCases
    };
}

[Injectable]
public class LiveSettingsRouter(JsonUtil jsonUtil, LiveSettingsService service) : StaticRouter(
    jsonUtil,
    [
        new RouteAction<EmptyRequestData>(
            "/sgtlaggy-questtweaks/settings/get",
            (url, info, sessionId, output, cancellationToken) =>
                ValueTask.FromResult(jsonUtil.Serialize(service.Get(sessionId))!)
        ),
        new RouteAction<EmptyRequestData>(
            "/sgtlaggy-questtweaks/tags",
            (url, info, sessionId, output, cancellationToken) =>
                ValueTask.FromResult(jsonUtil.Serialize(service.Tags(sessionId))!)
        ),
        new RouteAction<LiveSettings>(
            "/sgtlaggy-questtweaks/settings/set",
            (url, info, sessionId, output, cancellationToken) =>
                ValueTask.FromResult(jsonUtil.Serialize(service.Set(info, sessionId))!)
        )
    ]
);
