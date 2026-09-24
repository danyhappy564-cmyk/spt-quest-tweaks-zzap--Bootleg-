using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Utils.Json.Converters;
using SPTarkov.Server.Web.Models.Configs;
using SPTarkov.Server.Web.Services;


namespace sgtlaggyQuestTweaks;

public record Config
{
    [JsonPropertyName("QualityOfLife")]
    public required QualityOfLifeConfig QualityOfLife { get; set; }

    [JsonPropertyName("GlobalConditions")]
    public required GlobalConditionsConfig GlobalConditions { get; set; }

    [JsonPropertyName("SpecialCases")]
    public required SpecialCasesConfig SpecialCases { get; set; }

    [JsonPropertyName("exemptQuests")]
    public required HashSet<MongoId> ExemptQuests { get; set; }

    [JsonPropertyName("onlyQuests")]
    public required HashSet<MongoId> OnlyQuests { get; set; }

    [JsonPropertyName("questOverrides")]
    public required Dictionary<MongoId, ConditionsConfig> QuestOverrides { get; set; }
}

public record QualityOfLifeConfig
{
    [JsonPropertyName("revealAllQuestObjectives")]
    public bool RevealAllQuestObjectives { get; set; }

    [JsonPropertyName("revealUnknownRewards")]
    public bool RevealUnknownRewards { get; set; }

    [JsonPropertyName("removeTimeGates")]
    public bool RemoveTimeGates { get; set; }

    // Append "[퀘스트 완화됨: ...]" to the Korean text of every objective that was actually relaxed.
    [JsonPropertyName("showRelaxedTag")]
    public bool ShowRelaxedTag { get; set; } = true;

    // Also fix copies of relaxed values already stored in the profile (running wait timers,
    // generated dailies/weeklies, stored Locked status). Irreversible, so off by default.
    [JsonPropertyName("applyToExistingProgress")]
    public bool ApplyToExistingProgress { get; set; }
}

public record ConditionsConfig
{
    [JsonPropertyName("removeTarget")]
    public bool? RemoveTarget { get; set; }

    [JsonPropertyName("removeWeapon")]
    public bool? RemoveWeapon { get; set; }

    [JsonPropertyName("removeWeaponMods")]
    public bool? RemoveWeaponMods { get; set; }

    [JsonPropertyName("removeSelfGear")]
    public bool? RemoveSelfGear { get; set; }

    [JsonPropertyName("removeEnemyGear")]
    public bool? RemoveEnemyGear { get; set; }

    [JsonPropertyName("removeSelfHealthEffect")]
    public bool? RemoveSelfHealthEffect { get; set; }

    [JsonPropertyName("removeEnemyHealthEffect")]
    public bool? RemoveEnemyHealthEffect { get; set; }

    [JsonPropertyName("removeBodyPart")]
    public bool? RemoveBodyPart { get; set; }

    [JsonPropertyName("removeDistance")]
    public bool? RemoveDistance { get; set; }

    [JsonPropertyName("removeTime")]
    public bool? RemoveTime { get; set; }

    [JsonPropertyName("removeMap")]
    public bool? RemoveMap { get; set; }

    [JsonPropertyName("removeZone")]
    public bool? RemoveZone { get; set; }

    [JsonPropertyName("removeFindInRaid")]
    public bool? RemoveFindInRaid { get; set; }

    [JsonPropertyName("handoverItemCount")]
    public int? HandoverItemCount { get; set; }

    [JsonPropertyName("handoverItemPercent")]
    public int? HandoverItemPercent { get; set; }

    [JsonPropertyName("eliminationCount")]
    public int? EliminationCount { get; set; }

    [JsonPropertyName("eliminationPercent")]
    public int? EliminationPercent { get; set; }

    [JsonIgnore]
    public bool AnyChanged
    {
        get => (RemoveTarget ?? false)
               || (RemoveWeapon ?? false)
               || (RemoveWeaponMods ?? false)
               || (RemoveSelfGear ?? false)
               || (RemoveEnemyGear ?? false)
               || (RemoveSelfHealthEffect ?? false)
               || (RemoveEnemyHealthEffect ?? false)
               || (RemoveBodyPart ?? false)
               || (RemoveDistance ?? false)
               || (RemoveTime ?? false)
               || (RemoveMap ?? false)
               || (RemoveZone ?? false)
               || (RemoveFindInRaid ?? false)
               || (HandoverItemCount >= 0)
               || (HandoverItemPercent >= 0)
               || (EliminationCount >= 0)
               || (EliminationPercent >= 0);
    }
}

public record GlobalConditionsConfig : ConditionsConfig
{
    [JsonPropertyName("affectRepeatables")]
    public bool AffectRepeatables { get; set; }
}

public record SpecialCasesConfig
{
    [JsonPropertyName("lightkeeperOnlyRequireLevel")]
    public int LightkeeperOnlyRequireLevel { get; set; }

    [JsonPropertyName("tarkovShooterM10")]
    public bool TarkovShooterM10 { get; set; }

    [JsonPropertyName("collectorPrerequisiteBackport")]
    public bool CollectorPrerequisiteBackport { get; set; }
}

public class ConfigRegistration : IOnDIConstruct
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new StringToMongoIdConverter() }
    };

    public static string ConfigPath =>
        Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config.json");

    public static void Save(Config config)
    {
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(config, JsonOptions) + Environment.NewLine);
    }

    public static async Task OnDIConstructAsync(
        IServiceCollection serviceCollection,
        CancellationToken cancellationToken
    )
    {
        var configJson = await File.ReadAllTextAsync(ConfigPath, cancellationToken);
        var config = JsonSerializer.Deserialize<Config>(configJson, JsonOptions)!;

        serviceCollection.AddSingleton(config);
    }
}

[Injectable(InjectionType = InjectionType.Singleton)]
public class ConfigEditorProvider(Config config, ModHelper modHelper) : IConfigEditorConfigProvider
{
    public IEnumerable<ConfigEditorConfigRegistration> GetConfigs()
    {
        var metadata = new ModMetadata();
        var modDir = modHelper.GetAbsolutePathToModFolder();
        yield return ConfigEditorConfigRegistration.Create(
            metadata.ModGuid,
            metadata.Name,
            config,
            Path.Combine(modDir, "config.json")
        );
    }
}
