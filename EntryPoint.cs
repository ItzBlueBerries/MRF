using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: MelonInfo(typeof(MRF.EntryPoint), "MRF", "1.0.3", "Mod Contributors")]
namespace MRF
{
    public class EntryPoint : MelonMod
    {
        public static MelonLogger.Instance modLogger = new MelonLogger.Instance("MRF"); 

        public override void OnInitializeMelon() { }

    }
}