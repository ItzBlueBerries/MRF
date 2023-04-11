using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Persist;
using MelonLoader;
using UnityEngine;

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

    [HarmonyPatch(typeof(PediaDirector), "Awake")]
    public static class PediaDirectorAwake
    {
        public static bool IsTesting = false;
        public static void Prefix(PediaDirector __instance)
        {
            if (!IsTesting) return;
            var pediaEntryCategory = __instance.entryCategories.items.ToArray().First(x => x.name == "Slimes");
            var pediaEntry = pediaEntryCategory.items.ToArray().First();
            var identifiablePediaEntry = ScriptableObject.CreateInstance<IdentifiablePediaEntry>();
            var gastropodTest = Resources.FindObjectsOfTypeAll<IdentifiableType>().FirstOrDefault(x => x.name == "BrineGastropod");
            identifiablePediaEntry.hideFlags |= HideFlags.HideAndDontSave;
            identifiablePediaEntry.name = "Mud";
            identifiablePediaEntry.identifiableType = gastropodTest;
            identifiablePediaEntry.template = pediaEntry.template;
            identifiablePediaEntry.title = gastropodTest.LocalizedName;
            identifiablePediaEntry.description = gastropodTest.LocalizedName;
            identifiablePediaEntry.isUnlockedInitially = false;
            identifiablePediaEntry.actionButtonLabel = pediaEntry.actionButtonLabel;
            identifiablePediaEntry.infoButtonLabel = pediaEntry.infoButtonLabel;
            
            pediaEntryCategory.items.Add(identifiablePediaEntry);
            __instance.identDict.Add(gastropodTest, identifiablePediaEntry);
        }
    }
    
    
    [HarmonyLib.HarmonyPatch(typeof(SavedGame))]
    internal static class SavedGamePushPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SavedGame.Push), typeof(GameModel))]
        public static void PushGameModel(SavedGame __instance)
        {
            IdentifiableTypePersistenceIdReverseLookupTable identifiableTypePersistenceIdReverseLookupTable = __instance.persistenceIdToIdentifiableType;

            var actorDataV01s = __instance.gameState.actors.ToArray().Where(x => identifiableTypePersistenceIdReverseLookupTable.GetIdentifiableType(x.typeId) is null);
            foreach (var actorDataV01 in actorDataV01s)
            {
                MelonLogger.Warning($"Actor is null, removing actor: {actorDataV01.typeId}");
                __instance.gameState.actors.Remove(actorDataV01);
            }
            var pediaStrings = __instance.gameState.pedia.unlockedIds.ToArray().Where(pediaUnlockedId => !__instance.pediaEntryLookup.ContainsKey(pediaUnlockedId));
            foreach (var pediaString in pediaStrings)
            {
                MelonLogger.Warning($"PediaId is null, removing pedia named {pediaString}");
                __instance.gameState.pedia.unlockedIds.Remove(pediaString);

            }

            List<LandPlotV02> landPlotV02s = new List<LandPlotV02>();
            foreach (var landPlotV02 in __instance.gameState.ranch.plots)
            {
                if (!Enum.IsDefined(landPlotV02.typeId))
                {
                    landPlotV02s.Add(landPlotV02);
                    continue;
                }
                var upgrades = new List<LandPlot.Upgrade>();
                foreach (var upgrade in landPlotV02.upgrades)
                {
                    if (!Enum.IsDefined(upgrade))
                    {
                        upgrades.Add(upgrade);
                    }
                }
                upgrades.ForEach(x => landPlotV02.upgrades.Remove(x) );
            }
            //var landPlotV02s = __instance.gameState.ranch.plots.ToArray().Where(x => !Enum.IsDefined(x.typeId));
            foreach (var landPlotV02 in landPlotV02s)
            {
                MelonLogger.Warning($"Plot TypeId is NONE, removing plot: {landPlotV02.typeId}");
                __instance.gameState.ranch.plots.Remove(landPlotV02);
            }

        }
    }
}