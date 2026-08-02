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
    [JsonPropertyName("revealAllQuestObjectives")]
    public bool RevealAllQuestObjectives { get; set; }

    [JsonPropertyName("revealUnknownRewards")]
    public bool RevealUnknownRewards { get; set; }

    [JsonPropertyName("removeTimeGates")]
    public bool RemoveTimeGates { get; set; }

    [JsonPropertyName("removeConditions")]
    public required ConditionsConfig RemoveConditions { get; set; }

    [JsonPropertyName("handoverItemCount")]
    public int HandoverItemCount { get; set; }

    [JsonPropertyName("handoverItemPercent")]
    public int HandoverItemPercent { get; set; }

    [JsonPropertyName("eliminationCount")]
    public int EliminationCount { get; set; }

    [JsonPropertyName("eliminationPercent")]
    public int EliminationPercent { get; set; }

    [JsonPropertyName("affectRepeatables")]
    public bool AffectRepeatables { get; set; }

    [JsonPropertyName("exemptQuests")]
    public required HashSet<MongoId> ExemptQuests { get; set; }

    [JsonPropertyName("onlyQuests")]
    public required HashSet<MongoId> OnlyQuests { get; set; }

    [JsonPropertyName("questOverrides")]
    public required Dictionary<MongoId, ConditionsConfig> QuestOverrides { get; set; }

    [JsonPropertyName("lightkeeperOnlyRequireLevel")]
    public int LightkeeperOnlyRequireLevel { get; set; }

    [JsonPropertyName("tarkovShooterM10")]
    public bool TarkovShooterM10 { get; set; }

    [JsonPropertyName("collectorPrerequisiteBackport")]
    public bool CollectorPrerequisiteBackport { get; set; }
}

public record ConditionsConfig
{
    [JsonPropertyName("target")]
    public bool? Target { get; set; }

    [JsonPropertyName("weapon")]
    public bool? Weapon { get; set; }

    [JsonPropertyName("weaponMods")]
    public bool? WeaponMods { get; set; }

    [JsonPropertyName("selfGear")]
    public bool? SelfGear { get; set; }

    [JsonPropertyName("enemyGear")]
    public bool? EnemyGear { get; set; }

    [JsonPropertyName("selfHealthEffect")]
    public bool? SelfHealthEffect { get; set; }

    [JsonPropertyName("enemyHealthEffect")]
    public bool? EnemyHealthEffect { get; set; }

    [JsonPropertyName("bodyPart")]
    public bool? BodyPart { get; set; }

    [JsonPropertyName("distance")]
    public bool? Distance { get; set; }

    [JsonPropertyName("time")]
    public bool? Time { get; set; }

    [JsonPropertyName("map")]
    public bool? Map { get; set; }

    [JsonPropertyName("zone")]
    public bool? Zone { get; set; }

    [JsonPropertyName("findInRaid")]
    public bool? FindInRaid { get; set; }

    public bool AnyEnabled
    {
        get => (Target ?? false)
               || (Weapon ?? false)
               || (WeaponMods ?? false)
               || (SelfGear ?? false)
               || (EnemyGear ?? false)
               || (SelfHealthEffect ?? false)
               || (EnemyHealthEffect ?? false)
               || (BodyPart ?? false)
               || (Distance ?? false)
               || (Time ?? false)
               || (Map ?? false)
               || (Zone ?? false)
               || (FindInRaid ?? false);
    }
}

public class ConfigRegistration : IOnDIConstruct
{
    public static async Task OnDIConstructAsync(
        IServiceCollection serviceCollection,
        CancellationToken cancellationToken
    )
    {
        var modDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        var jsonSerializerOptions = new JsonSerializerOptions()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            Converters = { new StringToMongoIdConverter() }
        };

        var configJson = await File.ReadAllTextAsync(Path.Join(modDir, "config.json"), cancellationToken);
        var config = JsonSerializer.Deserialize<Config>(configJson, jsonSerializerOptions)!;

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
