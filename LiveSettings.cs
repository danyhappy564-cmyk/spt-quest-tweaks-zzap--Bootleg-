using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Common.Models.Logging;
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
}

[Injectable(InjectionType.Singleton)]
public class LiveSettingsService(
    ISptLogger<LiveSettingsService> logger,
    Config config,
    QuestTweaksService questTweaks
)
{
    private readonly object _lock = new();

    public LiveSettingsResponse Get()
    {
        lock (_lock)
        {
            return new LiveSettingsResponse { Ok = true, Settings = Current(), Tags = questTweaks.GetTagLabels() };
        }
    }

    public LiveSettingsResponse Set(LiveSettings request)
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
            logger.Info("[QuestTweaks] settings changed from F12 menu, quest tweaks re-applied");

            return new LiveSettingsResponse
            {
                Ok = true,
                Message = message,
                Settings = Current(),
                LocaleChanges = localeChanges,
                Tags = questTweaks.GetTagLabels()
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
                ValueTask.FromResult(jsonUtil.Serialize(service.Get())!)
        ),
        new RouteAction<LiveSettings>(
            "/sgtlaggy-questtweaks/settings/set",
            (url, info, sessionId, output, cancellationToken) =>
                ValueTask.FromResult(jsonUtil.Serialize(service.Set(info))!)
        )
    ]
);
