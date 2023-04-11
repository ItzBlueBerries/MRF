using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: MelonInfo(typeof(MRF.EntryPoint), "MRF", "1.0.0", "Mod Contributors")]
namespace MRF
{
    public class EntryPoint : MelonMod
    {
        public override void OnInitializeMelon() { }
        private static void Spawn(string name)
        {
            var instancePlayer = SRSingleton<SceneContext>.Instance.Player;
            SRBehaviour.InstantiateActor(
                Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(x => x.name.Equals(name)),
                SRSingleton<SceneContext>.Instance.RegionRegistry.CurrentSceneGroup,
                instancePlayer.transform.position, instancePlayer.transform.rotation);
        }

        public static void SpawnGastroPodForTest() => Spawn("brineGastropod");
    }
}