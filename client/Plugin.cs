using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using BepInEx;
using BepInEx.Configuration;
using EFT;
using HarmonyLib;
using Newtonsoft.Json.Linq;

namespace QuestTweaksLive
{
    /// <summary>
    /// F12 (BepInEx ConfigurationManager) front-end for the Quest Tweaks server mod.
    /// The server's config.json is the source of truth: on start the plugin pulls it, and every
    /// change made in F12 is pushed back, saved to config.json and re-applied without a restart.
    /// </summary>
    [BepInPlugin(Guid, "Quest Tweaks Live (F12)", "1.2.0")]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "com.zzap.questtweaks.live";
        private const string TagLocale = "kr";
        private const double PushDelaySeconds = 0.8;
        private const double RetrySeconds = 10;

        private readonly List<BoolSetting> _bools = new List<BoolSetting>();
        private readonly List<IntSetting> _ints = new List<IntSetting>();
        private readonly ConcurrentQueue<Action> _mainThread = new ConcurrentQueue<Action>();
        private ConfigEntry<string> _status;
        private ConfigEntry<string> _tagColor;

        private bool _synced;
        private bool _dirty;
        private bool _busy;
        private bool _suppress;
        private DateTime _lastChange;
        private DateTime _nextSyncAttempt;

        private void Awake()
        {
            BindAll();
            Config.SettingChanged += OnSettingChanged;

            try
            {
                new Harmony(Guid).PatchAll(typeof(RelaxedTagPatch));
            }
            catch (Exception ex)
            {
                Logger.LogError($"relaxed-tag patch failed, tags only come from the server locale: {ex}");
            }
            SetStatus("서버 연결 대기 중");
        }

        private void Update()
        {
            while (_mainThread.TryDequeue(out var action))
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
            }

            if (_busy)
            {
                return;
            }

            var now = DateTime.UtcNow;
            if (!_synced)
            {
                if (now >= _nextSyncAttempt)
                {
                    FetchFromServer();
                }
            }
            else if (_dirty && (now - _lastChange).TotalSeconds >= PushDelaySeconds)
            {
                PushToServer();
            }
        }

        private void OnSettingChanged(object sender, SettingChangedEventArgs e)
        {
            if (_suppress || e.ChangedSetting == _status || e.ChangedSetting == _tagColor)
            {
                return;
            }

            _dirty = true;
            _lastChange = DateTime.UtcNow;
            if (_synced)
            {
                SetStatus("변경 감지, 곧 서버에 적용합니다...");
            }
        }

        private void FetchFromServer()
        {
            _busy = true;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    var response = ServerApi.Post(ServerApi.GetRoute, "{}");
                    _mainThread.Enqueue(() =>
                    {
                        ApplyServerSettings(response["settings"]);
                        ApplyTags(response["tags"] as JObject);
                        _synced = true;
                        _dirty = false;
                        _busy = false;
                        SetStatus($"서버와 동기화됨 ({DateTime.Now:HH:mm:ss})");
                    });
                }
                catch (Exception ex)
                {
                    _mainThread.Enqueue(() =>
                    {
                        _busy = false;
                        _nextSyncAttempt = DateTime.UtcNow.AddSeconds(RetrySeconds);
                        SetStatus($"서버 연결 실패, {RetrySeconds}초 후 재시도 ({ex.Message})");
                        Logger.LogWarning($"settings fetch failed: {ex.Message}");
                    });
                }
            });
        }

        private void PushToServer()
        {
            _busy = true;
            _dirty = false;
            var body = BuildSettingsJson().ToString(Newtonsoft.Json.Formatting.None);
            SetStatus("서버에 적용 중...");

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    var response = ServerApi.Post(ServerApi.SetRoute, body);
                    _mainThread.Enqueue(() =>
                    {
                        _busy = false;
                        if (!_dirty)
                        {
                            ApplyServerSettings(response["settings"]);
                        }

                        ApplyTags(response["tags"] as JObject);
                        var updated = MergeLocaleChangesSafe(response["localeChanges"] as JObject);
                        var message = (string)response["message"] ?? "적용 완료";
                        SetStatus($"{message} ({DateTime.Now:HH:mm:ss}, 문구 {updated}개 갱신)");
                        Logger.LogInfo($"settings applied: {message}, {updated} locale entries merged");
                    });
                }
                catch (Exception ex)
                {
                    _mainThread.Enqueue(() =>
                    {
                        _busy = false;
                        _dirty = true;
                        _lastChange = DateTime.UtcNow.AddSeconds(RetrySeconds);
                        SetStatus($"적용 실패, {RetrySeconds}초 후 재시도 ({ex.Message})");
                        Logger.LogWarning($"settings push failed: {ex.Message}");
                    });
                }
            });
        }

        private JObject BuildSettingsJson()
        {
            var root = new JObject();
            foreach (var setting in _bools)
            {
                Group(root, setting.Group)[setting.Key] = setting.Entry.Value;
            }
            foreach (var setting in _ints)
            {
                Group(root, setting.Group)[setting.Key] = setting.Entry.Value;
            }
            return root;
        }

        private static JObject Group(JObject root, string name)
        {
            if (!(root[name] is JObject group))
            {
                group = new JObject();
                root[name] = group;
            }
            return group;
        }

        private void ApplyServerSettings(JToken settings)
        {
            if (settings == null || settings.Type != JTokenType.Object)
            {
                return;
            }

            _suppress = true;
            try
            {
                foreach (var setting in _bools)
                {
                    var value = settings[setting.Group]?[setting.Key];
                    if (value != null && value.Type == JTokenType.Boolean)
                    {
                        setting.Entry.Value = value.Value<bool>();
                    }
                }
                foreach (var setting in _ints)
                {
                    var value = settings[setting.Group]?[setting.Key];
                    if (value != null && (value.Type == JTokenType.Integer || value.Type == JTokenType.Float))
                    {
                        setting.Entry.Value = value.Value<int>();
                    }
                }
            }
            finally
            {
                _suppress = false;
            }
        }

        private void ApplyTagColor()
        {
            var value = (_tagColor.Value ?? "").Trim();
            if (value.Length > 0 && !System.Text.RegularExpressions.Regex.IsMatch(value, "^#([0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$"))
            {
                Logger.LogWarning($"invalid tag color '{value}', using no color");
                value = "";
            }
            RelaxedTagPatch.ColorHex = value;
        }

        private void ApplyTags(JObject tags)
        {
            var map = new Dictionary<string, string>();
            if (tags != null)
            {
                foreach (var property in tags.Properties())
                {
                    map[property.Name] = (string)property.Value;
                }
            }

            RelaxedTagPatch.Tags = map;
            Logger.LogInfo($"relaxed-quest tags received: {map.Count}");
        }

        private int MergeLocaleChangesSafe(JObject changes)
        {
            if (changes == null || changes.Count == 0)
            {
                return 0;
            }

            var entries = new Dictionary<string, string>();
            foreach (var property in changes.Properties())
            {
                entries[property.Name] = (string)property.Value;
            }

            try
            {
                return MergeLocaleChanges(entries);
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"could not update the loaded locale, restart the game to see new text: {ex.Message}");
                return 0;
            }
        }

        // Kept separate so a changed game API fails inside the caller's try/catch.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int MergeLocaleChanges(Dictionary<string, string> entries)
        {
            var manager = LocalizationManager.Instance;
            if (!manager.ContainsCulture(TagLocale))
            {
                return 0;
            }

            manager.UpdateLocales(TagLocale, entries);
            return entries.Count;
        }

        private void SetStatus(string text)
        {
            if (_status == null)
            {
                return;
            }

            _suppress = true;
            _status.Value = text;
            _suppress = false;
        }

        private void BindAll()
        {
            const string status = "0. 상태";
            const string display = "1. 표시";
            const string remove = "2. 조건 제거";
            const string counts = "3. 수량 조정";
            const string qol = "4. 편의 기능";
            const string special = "5. 특수 케이스";

            _status = Config.Bind(status, "서버 연결 상태", "",
                new ConfigDescription("서버 모드와의 연결/적용 결과. 직접 수정하지 않는다.", null,
                    new ConfigurationManagerAttributes { ReadOnly = true, HideDefaultButton = true, Order = 100 }));

            var order = 100;
            Bool(display, "완화 표시 붙이기", "QualityOfLife", "showRelaxedTag", true,
                "완화된 퀘스트 목표 문구 뒤에 [퀘스트 완화됨: 부위 무관 · 목표 5→3] 같은 표시를 붙인다. 한국어 클라이언트 전용.", order--);

            // client-only: not sent to the server
            _tagColor = Config.Bind(display, "완화 표시 색상", "#FF4040",
                new ConfigDescription("완화 표시 글자 색. #RRGGBB 형식 (예: #FF4040 빨강, #FFD040 노랑). 비우면 색 없음.", null,
                    new ConfigurationManagerAttributes { Order = order-- }));
            ApplyTagColor();
            _tagColor.SettingChanged += (sender, args) => ApplyTagColor();

            order = 100;
            Bool(remove, "대상 제한 해제", "GlobalConditions", "removeTarget", false, "PMC/스캐브/보스 등 사살 대상 제한을 없앤다.", order--);
            Bool(remove, "무기 제한 해제", "GlobalConditions", "removeWeapon", false, "특정 무기/구경 제한을 없앤다.", order--);
            Bool(remove, "부착물 제한 해제", "GlobalConditions", "removeWeaponMods", false, "소음기/스코프 등 부착물 조건을 없앤다.", order--);
            Bool(remove, "부위 제한 해제", "GlobalConditions", "removeBodyPart", false, "헤드샷 등 명중 부위 제한을 없앤다.", order--);
            Bool(remove, "거리 제한 해제", "GlobalConditions", "removeDistance", false, "사살 거리 제한을 없앤다.", order--);
            Bool(remove, "시간대 제한 해제", "GlobalConditions", "removeTime", false, "낮/밤 시간대 제한을 없앤다.", order--);
            Bool(remove, "맵 제한 해제", "GlobalConditions", "removeMap", false, "특정 맵에서만 진행되는 제한을 없앤다.", order--);
            Bool(remove, "구역 제한 해제", "GlobalConditions", "removeZone", false, "특정 구역 제한을 없앤다. 맵 제한은 유지하면 구역이 맵 전체로 넓어진다.", order--);
            Bool(remove, "착용 장비 제한 해제", "GlobalConditions", "removeSelfGear", false, "특정 장비 착용 조건을 없앤다.", order--);
            Bool(remove, "적 장비 제한 해제", "GlobalConditions", "removeEnemyGear", false, "적이 특정 장비를 착용해야 하는 조건을 없앤다.", order--);
            Bool(remove, "본인 상태 제한 해제", "GlobalConditions", "removeSelfHealthEffect", false, "탈수/뇌진탕 등 본인 상태 조건을 없앤다.", order--);
            Bool(remove, "적 상태 제한 해제", "GlobalConditions", "removeEnemyHealthEffect", false, "적의 상태 조건을 없앤다.", order--);
            Bool(remove, "FIR 조건 해제", "GlobalConditions", "removeFindInRaid", false, "납품 아이템의 인레이드 획득(FIR) 조건을 없앤다.", order--);
            Bool(remove, "반복 퀘스트에도 적용", "GlobalConditions", "affectRepeatables", true, "일일/주간 퀘스트에도 대상/무기/부위/거리/FIR 해제를 적용한다. 이미 받은 반복 퀘스트는 바뀌지 않는다.", order--);

            order = 100;
            Int(counts, "사살 수 비율(%)", "GlobalConditions", "eliminationPercent", -1, -1, 200,
                "-1 = 변경 안 함. 예) 60이면 5명 → 3명 (최소 1).", order--);
            Int(counts, "사살 수 고정값", "GlobalConditions", "eliminationCount", -1, -1, 999,
                "-1 = 변경 안 함. 0 이상이면 비율보다 우선 적용.", order--);
            Int(counts, "납품 수 비율(%)", "GlobalConditions", "handoverItemPercent", -1, -1, 200,
                "-1 = 변경 안 함. 퀘스트 아이템/열쇠에는 적용되지 않는다.", order--);
            Int(counts, "납품 수 고정값", "GlobalConditions", "handoverItemCount", -1, -1, 999,
                "-1 = 변경 안 함. 0 이상이면 비율보다 우선 적용.", order--);

            order = 100;
            Bool(qol, "숨겨진 목표 전부 공개", "QualityOfLife", "revealAllQuestObjectives", false,
                "주의: 목표를 순서와 다르게 완료할 수 있고, 끈 뒤에도 진행 중인 퀘스트에는 공개 상태가 남는다.", order--);
            Bool(qol, "알 수 없는 보상 공개", "QualityOfLife", "revealUnknownRewards", false, "\"알 수 없는 보상\"을 실제 아이템으로 보여준다.", order--);
            Bool(qol, "퀘스트 대기 시간 제거", "QualityOfLife", "removeTimeGates", false, "건스미스 등 일부 퀘스트 사이의 대기 시간을 없앤다.", order--);

            order = 100;
            Int(special, "등대지기 레벨 조건", "SpecialCases", "lightkeeperOnlyRequireLevel", 0, 0, 79,
                "0 = 끔. 0보다 크면 Network Provider - Part 1 시작 조건을 이 레벨 하나로 바꾼다.", order--);
            Bool(special, "타르코프 슈터에 TRG M10 허용", "SpecialCases", "tarkovShooterM10", false, "The Tarkov Shooter 시리즈에서 SAKO TRG M10 사용을 허용한다.", order--);
            Bool(special, "컬렉터 선행 조건 백포트", "SpecialCases", "collectorPrerequisiteBackport", false, "EFT 1.1.0.0의 완화된 컬렉터 선행 조건을 적용한다.", order--);
        }

        private void Bool(string section, string name, string group, string key, bool defaultValue, string description, int order)
        {
            var entry = Config.Bind(section, name, defaultValue,
                new ConfigDescription(description, null, new ConfigurationManagerAttributes { Order = order }));
            _bools.Add(new BoolSetting(group, key, entry));
        }

        private void Int(string section, string name, string group, string key, int defaultValue, int min, int max, string description, int order)
        {
            var entry = Config.Bind(section, name, defaultValue,
                new ConfigDescription(description, new AcceptableValueRange<int>(min, max),
                    new ConfigurationManagerAttributes { Order = order }));
            _ints.Add(new IntSetting(group, key, entry));
        }

        private sealed class BoolSetting
        {
            public readonly string Group;
            public readonly string Key;
            public readonly ConfigEntry<bool> Entry;

            public BoolSetting(string group, string key, ConfigEntry<bool> entry)
            {
                Group = group;
                Key = key;
                Entry = entry;
            }
        }

        private sealed class IntSetting
        {
            public readonly string Group;
            public readonly string Key;
            public readonly ConfigEntry<int> Entry;

            public IntSetting(string group, string key, ConfigEntry<int> entry)
            {
                Group = group;
                Key = key;
                Entry = entry;
            }
        }
    }

    // Read by BepInEx ConfigurationManager through reflection (field names matter).
    internal sealed class ConfigurationManagerAttributes
    {
        public int? Order;
        public bool? ReadOnly;
        public bool? HideDefaultButton;
    }
}
