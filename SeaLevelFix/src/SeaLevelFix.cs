using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.ServerMods;

namespace SeaLevelFix;

[HarmonyPatch]
public class SeaLevelFix : ModSystem
{
    public static ICoreAPI api;
    public Harmony harmony;

    public override void Start(ICoreAPI api)
    {
        SeaLevelFix.api = api;
        ModConfig.LoadConfig(api);
        
        if (api.Side == EnumAppSide.Server)
        {
            harmony = new Harmony(Mod.Info.ModID);
            api.Logger.Event("Applying SeaLevelFix patches.");
            harmony.PatchCategory("SeaLevelFix"); // Applies all harmony patches
        }
    }

    [HarmonyPatchCategory("SeaLevelFix")]
    [HarmonyPatch(typeof(GenTerra), "StartServerSide")]
    public static class GenTerra_StartServerSide_Patch
    {
        static void DisableServerRunPhase(IServerEventAPI IServerEventAPI, EnumServerRunPhase serverRunPhase, Action handler)
        {
            // Do nothing
            return;
        }
        
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                var codeMatcher = new CodeMatcher(instructions /*, ILGenerator generator*/);

                var pos = codeMatcher.MatchStartForward(
                    CodeMatch.Calls(AccessTools.Method(typeof(IServerEventAPI), "ServerRunPhase"))
                )
                    .ThrowIfInvalid("Could not find call to IServerEventAPI.ServerRunPhase.")
                    .RemoveInstruction()
                    .InsertAndAdvance(
                        CodeInstruction.Call(() => DisableServerRunPhase(default, default, default))
                );

                return codeMatcher.Instructions();
            }
            catch (Exception e) 
            {
                api.Logger.Error($"Exception during GenTerra_StartServerSide.Transpiler: {e}");
                return instructions;
            }
        }
    }
    
    [HarmonyPatchCategory("SeaLevelFix")]
    [HarmonyPatch(typeof(ModSystem), "AssetsFinalize")]
    class GenTerra_Create_AssetsFinalize
    {
        static bool Prefix(ModSystem __instance, ICoreAPI api)
        {
            if (__instance is GenTerra)
            {
                // Sea level needs to be initialized at least before GenDeposits.AssetsFinalize
                var apiField = typeof(GenTerra).GetField("api", BindingFlags.Instance | BindingFlags.NonPublic);
                apiField!.SetValue(__instance, (ICoreServerAPI)api);

                if (((ICoreServerAPI)api).WorldManager.SaveGame.WorldType != "standard") return false;

                TerraGenConfig.seaLevel = (int)(ModConfig.Instance.SeaLevel * ((ICoreServerAPI)api).WorldManager.MapSizeY);
                ((ICoreServerAPI)api).WorldManager.SetSeaLevel(TerraGenConfig.seaLevel);
                Climate.Sealevel = TerraGenConfig.seaLevel;
                
                return false;
            }

            return true; // make sure you only skip if really necessary
        }
    }
}
