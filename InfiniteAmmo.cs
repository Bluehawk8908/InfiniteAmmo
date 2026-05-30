using System.Collections;
using UnityEngine;
using MelonLoader;
using HarmonyLib;
using GHPC.Player;
using GHPC.State;
using GHPC.Weapons;
using GHPC.Vehicle;
using InfiniteAmmo;

[assembly: MelonInfo(typeof(InfiniteAmmoClass), "Infinite Ammo", "1.0.0", "Bluehawk")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace InfiniteAmmo
{
    public class InfAmmo : MonoBehaviour
    {
        void Awake()
        {
            enabled = false;
        }
    }
    public class InfiniteAmmoClass : MelonMod
    {
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "MainMenu2_Scene" || sceneName == "t64_menu" || sceneName == "MainMenu2-1_Scene")
            {
                return;
            }

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Chainguns), GameStatePriority.Medium);
        }

        public IEnumerator Chainguns(GameState _)
        {
            Vehicle[] list = GameObject.FindObjectsByType<Vehicle>(FindObjectsSortMode.None);
            foreach (Vehicle vic in list)
            {

                if (vic.gameObject.GetComponent<InfAmmo>() != null) { continue; }
                if (vic.UniqueName == "M2BRADLEY" || vic.UniqueName == "M2BRADLEY(ALT)" ||
                    vic.UniqueName == "BMP2_SA" || vic.UniqueName == "BMP2" ||
                    vic.UniqueName == "MARDERA1"|| vic.UniqueName == "MARDERA1PLUS" || vic.UniqueName == "MARDER1A2" || vic.UniqueName == "MARDERA1_NO_ATGM")
                {                                       
                    LoadoutManager.RackLoadout rackLoadout = vic.transform.GetComponent<LoadoutManager>().RackLoadouts[0];                    
                    GHPC.Weapons.AmmoRack rack = rackLoadout.Rack;
                    rack.AddInvisibleClip(rackLoadout.Rack.ClipTypes[0]);
                    rack.AddInvisibleClip(rackLoadout.Rack.ClipTypes[1]);
                    rackLoadout.AmmoCounts[0] = 2;
                    rackLoadout.AmmoCounts[1] = 2;
                    rack.ClipCapacity = 4;
                    vic.gameObject.AddComponent<InfAmmo>();
                }  
            }
            yield break;
        }
    }
    

    [HarmonyPatch(typeof(GHPC.Weapons.AmmoFeed), "FinishClipReload")]
    public static class Replenish
    {
        private static void Postfix(GHPC.Weapons.AmmoFeed __instance)
        {            
            PlayerInput pi = GameObject.Find("_APP_GHPC_").GetComponent<PlayerInput>();
            var _loadoutManager = AccessTools.FieldRefAccess<AmmoFeed, LoadoutManager>("_loadoutManager");
            LoadoutManager lm = _loadoutManager(__instance);
            Vehicle reloadingUnit = lm.gameObject.GetComponent<Vehicle>();            

            var _playerloadoutManager = AccessTools.FieldRefAccess<PlayerInput, LoadoutManager>("_loadoutManager");
            LoadoutManager plm = _playerloadoutManager(pi);
            Vehicle currentPlayerUnit = plm.gameObject.GetComponent<Vehicle>();            

            if (reloadingUnit == currentPlayerUnit)
            {
                var _readyRack = AccessTools.FieldRefAccess<AmmoFeed, GHPC.Weapons.AmmoRack>("ReadyRack");
                GHPC.Weapons.AmmoRack readyRack = _readyRack(__instance);                
                MelonLogger.Msg("Replacing reloaded round or clip: " + __instance.LoadedClipType.Name);
                var _visualSetup = AccessTools.FieldRefAccess<GHPC.Weapons.AmmoRack, bool>("_didVisualSlotSetup");
                bool visualSetup = _visualSetup(readyRack);
                if (visualSetup) { readyRack.AddClipToAnySlot(__instance.LoadedClipType); } else { readyRack.AddInvisibleClip(__instance.LoadedClipType); }
            }
        }
    }
}
