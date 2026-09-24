using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT;
using EFT.Quests;
using HarmonyLib;

namespace QuestTweaksLive
{
    /// <summary>
    /// Appends "[퀘스트 완화됨: ...]" to relaxed objectives at display time, in two places:
    /// 1. LocalizationManager.TryGetLocalization — every "id".Localized() lookup ends here, so normal
    ///    quest objectives are tagged even if the locale loaded at login lacks the server-side tag.
    /// 2. Condition.FormattedDescription overrides — repeatable (daily/weekly) objectives use
    ///    DynamicLocale, i.e. the game builds their text itself without looking up the objective id.
    /// </summary>
    internal static class RelaxedTagPatch
    {
        private const string TagLocale = "kr";
        private const string Marker = "[퀘스트 완화됨";

        // objective id -> "부위 무관 · 목표 5→3"; replaced as a whole, never mutated
        public static volatile Dictionary<string, string> Tags = new Dictionary<string, string>();

        // TextMeshPro rich-text color for the tag, e.g. "#FF4040"; empty = no color
        public static volatile string ColorHex = "#FF4040";

        // put the tag on its own line under the objective text
        public static volatile bool NewLine = true;

        // TextMeshPro size of the tag in percent; 100 = same as the objective text
        public static volatile int SizePercent = 90;

        // horizontal position of the tag line (only when NewLine is on); null = left, the default
        public static volatile string Align;

        public static void Apply(Harmony harmony)
        {
            harmony.Patch(
                AccessTools.Method(typeof(LocalizationManager), nameof(LocalizationManager.TryGetLocalization)),
                postfix: new HarmonyMethod(typeof(RelaxedTagPatch), nameof(LocalizationPostfix)));

            var descriptionPostfix = new HarmonyMethod(typeof(RelaxedTagPatch), nameof(DescriptionPostfix));
            foreach (var type in ConditionTypes())
            {
                var getter = AccessTools.DeclaredPropertyGetter(type, nameof(Condition.FormattedDescription));
                if (getter == null || getter.IsAbstract || getter.GetMethodBody() == null)
                {
                    continue;
                }
                harmony.Patch(getter, postfix: descriptionPostfix);
            }
        }

        private static IEnumerable<Type> ConditionTypes()
        {
            Type[] types;
            try
            {
                types = typeof(Condition).Assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray();
            }
            return types.Where(t => typeof(Condition).IsAssignableFrom(t) && !t.ContainsGenericParameters);
        }

        private static void LocalizationPostfix(string id, string locale, ref string localizedValue, bool __result)
        {
            if (!__result || locale != TagLocale || id == null)
            {
                return;
            }
            localizedValue = Decorate(id, localizedValue);
        }

        private static void DescriptionPostfix(Condition __instance, ref string __result)
        {
            if (__instance == null || !IsKorean())
            {
                return;
            }
            __result = Decorate(__instance.id.ToString(), __result);
        }

        private static bool IsKorean()
        {
            try
            {
                return LocalizationManager.Instance.Culture == TagLocale;
            }
            catch
            {
                return false;
            }
        }

        private static string Decorate(string id, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var tags = Tags;
            if (tags.Count == 0 || !tags.TryGetValue(id, out var label))
            {
                return text;
            }

            // the server may already have put a plain tag at the end of the locale text: replace it
            var serverTag = text.IndexOf(" " + Marker, StringComparison.Ordinal);
            if (serverTag >= 0)
            {
                text = text.Substring(0, serverTag);
            }
            else if (text.Contains(Marker))
            {
                return text; // already decorated (e.g. base getter went through the locale patch)
            }

            var tag = Marker + ": " + label + "]";
            var color = ColorHex;
            if (!string.IsNullOrEmpty(color))
            {
                tag = "<color=" + color + ">" + tag + "</color>";
            }
            var size = SizePercent;
            if (size != 100)
            {
                tag = "<size=" + size + "%>" + tag + "</size>";
            }
            if (!NewLine)
            {
                return text + " " + tag;
            }

            // <align> applies to the line it starts on, so only the tag line moves
            var align = Align;
            return string.IsNullOrEmpty(align)
                ? text + "\n" + tag
                : text + "\n<align=" + align + ">" + tag + "</align>";
        }
    }
}
