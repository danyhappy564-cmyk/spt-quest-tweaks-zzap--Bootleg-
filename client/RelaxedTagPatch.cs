using System.Collections.Generic;
using EFT;
using HarmonyLib;

namespace QuestTweaksLive
{
    /// <summary>
    /// Appends "[퀘스트 완화됨: ...]" to relaxed objectives at display time.
    /// Every text lookup in the game (id.Localized() -> LocalizedValue -> TryGetLocalization) ends here,
    /// so this works even if the locale the game loaded at login lacks the server-side tag
    /// (e.g. another translation mod overwrote it).
    /// </summary>
    [HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.TryGetLocalization))]
    internal static class RelaxedTagPatch
    {
        private const string TagLocale = "kr";
        private const string Marker = "[퀘스트 완화됨";

        // objective id -> "부위 무관 · 목표 5→3"; replaced as a whole, never mutated
        public static volatile Dictionary<string, string> Tags = new Dictionary<string, string>();

        // TextMeshPro rich-text color for the tag, e.g. "#FF4040"; empty = no color
        public static volatile string ColorHex = "#FF4040";

        [HarmonyPostfix]
        private static void Postfix(string id, string locale, ref string localizedValue, bool __result)
        {
            if (!__result || locale != TagLocale || id == null || string.IsNullOrEmpty(localizedValue))
            {
                return;
            }

            var tags = Tags;
            if (tags.Count == 0 || !tags.TryGetValue(id, out var label))
            {
                return;
            }

            // the server may already have put a plain tag at the end of the locale text: replace it
            var serverTag = localizedValue.IndexOf(" " + Marker, System.StringComparison.Ordinal);
            if (serverTag >= 0)
            {
                localizedValue = localizedValue.Substring(0, serverTag);
            }
            else if (localizedValue.Contains(Marker))
            {
                return;
            }

            var tag = Marker + ": " + label + "]";
            var color = ColorHex;
            localizedValue = string.IsNullOrEmpty(color)
                ? localizedValue + " " + tag
                : localizedValue + " <color=" + color + ">" + tag + "</color>";
        }
    }
}
