using BepInEx.Configuration;

namespace BleedRework
{
    public static class Configs
    {
        public static ConfigFile ModConfig;
        public static string ConfigFolderPath
        {
            get => System.IO.Path.Combine(BepInEx.Paths.ConfigPath, MainPlugin.pluginInfo.Metadata.GUID);
        }
        private const string Section_Bleed = "Bleed";

        public static void Setup()
        {
            ModConfig = new ConfigFile(System.IO.Path.Combine(ConfigFolderPath, $"ModConfig.cfg"), true);
            Read_AllConfigs();
        }

        private static void Read_AllConfigs()
        {
            Bleed.TritipBleedDamage = ModConfig
                .Bind(Section_Bleed, "Tritip Bleed Damage", Bleed.TritipBleedDamage_Default, "Tritip Bleed Damage.")
                .Value;
            Bleed.SpleenBleedDamage = ModConfig
                .Bind(
                    Section_Bleed,
                    "Shatterspleen Bleed Damage",
                    Bleed.SpleenBleedDamage_Default,
                    "Shatterspleen Bleed Damage."
                )
                .Value;
        }
    }
}
