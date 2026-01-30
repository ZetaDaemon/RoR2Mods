using BepInEx.Configuration;

namespace ZetaItemBalance
{
    public static class Configs
    {
        public static ConfigFile ModConfig;
        public static string ConfigFolderPath
        {
            get => System.IO.Path.Combine(BepInEx.Paths.ConfigPath, MainPlugin.pluginInfo.Metadata.GUID);
        }

        public static void Setup()
        {
            if (MainPlugin.DisableConfig)
            {
                ModConfig = null;
                return;
            }
            ModConfig = new ConfigFile(System.IO.Path.Combine(ConfigFolderPath, $"ModConfig.cfg"), true);
        }
    }
}
