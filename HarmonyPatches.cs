using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Script.Util.Extensions;
using Il2CppInterop.Runtime;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Il2CppMonomiPark.SlimeRancher.Persist;
using Il2CppMonomiPark.SlimeRancher.UI.Localization;
using Il2CppMonomiPark.SlimeRancher.Weather;
using Il2CppMonomiPark.SlimeRancher.World;
using Il2CppSystem.IO;
using MelonLoader;
using UnityEngine;
using static MRF.EntryPoint;

namespace MRF;

internal class HarmonyPatches
{



    [HarmonyPatch]
    internal static class Il2cppDetourMethodPatcherReportExceptionPatch
    {
        public static MethodInfo TargetMethod() => AccessTools.Method(((IEnumerable<Type>) AccessTools.AllAssemblies().FirstOrDefault<Assembly>((Func<Assembly, bool>) (x => x.GetName().Name.Equals("Il2CppInterop.HarmonySupport"))).GetTypes()).FirstOrDefault<Type>((Func<Type, bool>) (x => x.Name == "Il2CppDetourMethodPatcher")), "ReportException");
        public static bool Prefix(System.Exception ex)
        {
            MelonLogger.Error("During invoking native->managed trampoline", ex);
            return false;
        }
    }



    [HarmonyPatch(typeof(SavedGame))]
    internal static class SavedGamePushPatch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch(nameof(SavedGame.Push), typeof(GameModel))]
        public static void PushGameModel(SavedGame __instance)
        {
            IdentifiableTypePersistenceIdReverseLookupTable identifiableTypePersistenceIdReverseLookupTable = __instance.persistenceIdToIdentifiableType;

            // ACTOR
            var actorDataV01s = __instance.gameState.Actors.ToArray().Where(x => identifiableTypePersistenceIdReverseLookupTable.GetIdentifiableType(x.TypeId) is null);
            foreach (var actorDataV01 in actorDataV01s)
            {
                modLogger.Warning($"Actor is null, removing actor: {actorDataV01.TypeId}");
                __instance.gameState.Actors.Remove(actorDataV01);
            }

            // PEDIA
            var pediaStrings = __instance.gameState.Pedia.UnlockedIds.ToArray().Where(pediaUnlockedId => !__instance.pediaEntryLookup.ContainsKey(pediaUnlockedId));
            foreach (var pediaString in pediaStrings)
            {
                modLogger.Warning($"PediaId is null, removing pedia named {pediaString}");
                __instance.gameState.Pedia.UnlockedIds.Remove(pediaString);
            }

            // LAND PLOT
            List<LandPlotV02> landPlotV02s = new List<LandPlotV02>();
            foreach (var landPlotV02 in __instance.gameState.Ranch.Plots)
            {
                if (!Enum.IsDefined(landPlotV02.TypeId))
                {
                    landPlotV02s.Add(landPlotV02);
                    continue;
                }
                var upgrades = new List<LandPlot.Upgrade>();
                foreach (var upgrade in landPlotV02.Upgrades)
                {
                    if (!Enum.IsDefined(upgrade))
                    {
                        upgrades.Add(upgrade);
                    }
                }
                upgrades.ForEach(x => landPlotV02.Upgrades.Remove(x));
            }
            //var landPlotV02s = __instance.gameState.ranch.plots.ToArray().Where(x => !Enum.IsDefined(x.typeId));
            foreach (var landPlotV02 in landPlotV02s)
            {
                modLogger.Warning($"Plot TypeId is NONE, removing plot: {landPlotV02.TypeId}");
                __instance.gameState.Ranch.Plots.Remove(landPlotV02);
            }
                // WEATHER
                PersistenceIdReverseLookupTable<IWeatherState> idToStateTable = __instance._weatherStateTranslation.ReverseLookupTable;
                PersistenceIdReverseLookupTable<WeatherPatternDefinition> idToPatternTable = __instance._weatherPatternTranslation.ReverseLookupTable;
                PersistenceIdLookupTable<IWeatherState> stateLookup = __instance._weatherStateTranslation.InstanceLookupTable;
                PersistenceIdLookupTable<WeatherPatternDefinition> patternLookup = __instance._weatherPatternTranslation.InstanceLookupTable;
                WeatherV01 weatherV01 = __instance.gameState.Weather;
                List<string> stateTable = new List<string>();
                if (idToStateTable._indexTable != null)
                {
                    stateTable = idToStateTable._indexTable.ToList();
                }
                List<string> patternTable = new List<string>();
                if (idToPatternTable._indexTable != null)
                {
                    patternTable = idToPatternTable._indexTable.ToList();
                }

                var stateDict = __instance._weatherStateTranslation.RawLookupDictionary;

                if (idToStateTable._indexTable != null)
                {
                    var stateIndexes = idToStateTable._indexTable.Where(index => !stateDict.ContainsKey(index));
                    foreach (var index in stateIndexes)
                    {
                        modLogger.Warning($"Weather State is unavailable, removing weather state named {index}");
                        stateTable.Remove(index);
                    }
                }
                var patternDict = __instance._weatherPatternTranslation.RawLookupDictionary;
                if (idToPatternTable._indexTable != null)
                {
                    var patternIndexes = idToPatternTable._indexTable.Where(index => !patternDict.ContainsKey(index));
                    foreach (var index in patternIndexes)
                    {
                        modLogger.Warning($"Weather Pattern is unavailable, removing weather pattern named {index}");
                        patternTable.Remove(index);
                    }
                }

                foreach (var entry in weatherV01.Entries)
                {
                    var entryStateIds = entry.StateCompletionTimeIDs.ToArray().Where(id => !stateLookup._reverseIndex.ContainsValue(id));
                    foreach (var id in entryStateIds)
                        entry.StateCompletionTimeIDs.Remove(id);

                    var entryPatternIds = entry.PatternCompletionTimeIDs.ToArray().Where(id => !patternLookup._reverseIndex.ContainsValue(id));
                    foreach (var id in entryPatternIds)
                        entry.PatternCompletionTimeIDs.Remove(id);

                    var forecastEntries = entry.Forecast.ToArray().Where(id => !stateLookup._reverseIndex.ContainsValue(id.StateID) || !patternLookup._reverseIndex.ContainsValue(id.PatternID));
                    foreach (var forecastEntry in forecastEntries)
                        entry.Forecast.Remove(forecastEntry);
                }

                idToStateTable._indexTable = stateTable.ToArray();
                idToPatternTable._indexTable = patternTable.ToArray();
                __instance.gameState.WeatherIndex.StateIndexTable = stateTable.ToArray();
                __instance.gameState.WeatherIndex.PatternIndexTable = patternTable.ToArray();
         
        }



    }
}