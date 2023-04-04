using HarmonyLib;
using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Persist;
using MelonLoader;
using UnityEngine.Yoga;

internal class HarmonyPatches
{
    [HarmonyPatch(typeof(YogaConstants), nameof(YogaConstants.IsUndefined), typeof(float))]
    public static class YogaConstantsIsUndefinedPatch
    {

        public static bool Prefix(float value, ref bool __result)
        {
            if (SavedGamePushActorDataPatch.pushActor)
            {
                SavedGamePushActorDataPatch.pushActor = false;
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SavedGame), nameof(SavedGame.PushActorData))]
    public static class SavedGamePushActorDataPatch
    {
        public static bool pushActor = false;
        public static void Prefix(SavedGame __instance, GameModel gameModel, ActorDataV01 actorData, WorldV03 world)
        {

            IdentifiableTypePersistenceIdReverseLookupTable identifiableTypePersistenceIdReverseLookupTable = __instance.persistenceIdToIdentifiableType;
            int typeId = actorData.typeId;
            IdentifiableType identifiableType = identifiableTypePersistenceIdReverseLookupTable.GetIdentifiableType(typeId);
            if (identifiableType == null)
            {
                pushActor = true;
                MelonLogger.Msg("identType is null");

            }

            //MelonLogger.Msg(identifiableType.ToString());
        }
    }
}
