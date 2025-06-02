using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.ServerMods;

namespace SeaLevelFix;

[HarmonyPatch]
public class SurfaceDepositFix : ModSystem
{
    public static ICoreAPI api;
    public Harmony harmony;

    public override double ExecuteOrder() => -0.1;
    
    public override void Start(ICoreAPI api)
    {
        SurfaceDepositFix.api = api;
        ModConfig.LoadConfig(api);

        if (api.Side != EnumAppSide.Server) return;
        
        if (ModConfig.SupportedVersions.Contains(GameVersion.ShortGameVersion))
        {
            harmony = new Harmony(Mod.Info.ModID);
            api.Logger.Event("Applying SurfaceDepositFix patches.");
            harmony.PatchCategory("SurfaceDepositFix"); // Applies all harmony patches
        }
        else
        {
            api.Logger.Error($"Skipping SurfaceDepositFix patches. Unsupported game version: {GameVersion.ShortGameVersion}.");
        }
    }

    [HarmonyPatchCategory("SurfaceDepositFix")]
    [HarmonyPatch(typeof(DiscDepositGenerator), "GenDeposit")]
    public static class DiscDepositGenerator_genDeposit_Patch
    {
        public static float CalculateDepthChance(float factor)
        {
            return ModConfig.Instance.SurfaceOreMaxDepth * (TerraGenConfig.seaLevel / 110f);
        }
        
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                var codeMatcher = new CodeMatcher(instructions /*, ILGenerator generator*/);

                codeMatcher.MatchStartForward(
                        CodeMatch.LoadsConstant(9.0f)
                    )
                    .ThrowIfInvalid("Could not find loading of constant 9.0f.")
                    .Advance(1)
                    .InsertAndAdvance(
                        CodeInstruction.Call(() => CalculateDepthChance(0))
                    );

                return codeMatcher.Instructions();
            }
            catch (Exception e) 
            {
                api.Logger.Error($"Exception during DiscDepositGenerator_genDeposit.Transpiler: {e}");
                return instructions;
            }
        }
    }
}