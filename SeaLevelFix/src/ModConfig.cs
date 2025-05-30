using System.Collections.ObjectModel;
using Vintagestory.API.Common;

namespace SeaLevelFix;

class ModConfig
{
    public static ModConfig Instance { get; set; } = new ModConfig();

    public static void LoadConfig(ICoreAPI api)
    {
        try
        {
            ModConfig file;
            if ((file = api.LoadModConfig<ModConfig>("SeaLevelFix.json")) == null)
            {
                api.StoreModConfig<ModConfig>(ModConfig.Instance, "SeaLevelFix.json");
            }
            else
            {
                ModConfig.Instance = file;
            }
        }
        catch
        {
            api.StoreModConfig<ModConfig>(ModConfig.Instance, "SeaLevelFix.json");
        }
    }

    /// <summary>
    /// Sea Level in Percentage of World Height
    /// </summary>
    public double SeaLevel { get { return _seaLevel; }
        set
        {
            if (value < 0.1)
                _seaLevel = 0.1f;
            else if (value > 0.9)
                _seaLevel = 0.9f;
            else
                _seaLevel = value;
        }
    }
    private double _seaLevel = 22.0 / 51.0;

    /// <summary>
    /// How deep an ore deposit can be and still generate surface deposits.
    /// </summary>
    public float SurfaceOreMaxDepth { get { return _surfaceOreMaxDepth; } set { _surfaceOreMaxDepth = value >= 2 ? value : 0; } }
    private float _surfaceOreMaxDepth = 9.0f;

    public static readonly IReadOnlyList<string> SupportedVersions = new List<string>()
    {
        "1.20.4", "1.20.5-rc.1", "1.20.5-rc.2", "1.20.5-rc.3", "1.20.5", "1.20.6", "1.20.7", "1.20.8-rc.1",
        "1.20.8-rc.2", "1.20.8", "1.20.9", "1.20.10", "1.20.11-rc.1", "1.20.11"
    };
}
