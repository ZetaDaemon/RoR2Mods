using BepInEx.Configuration;

namespace BleedRework
{
    public static class Configs
    {
        public static ConfigFile ModConfig;
        public static string ConfigFolderPath
        {
            get =>
                System.IO.Path.Combine(
                    BepInEx.Paths.ConfigPath,
                    MainPlugin.pluginInfo.Metadata.GUID
                );
        }

        public static void Setup()
        {
            ModConfig = new ConfigFile(
                System.IO.Path.Combine(ConfigFolderPath, $"ModConfig.cfg"),
                true
            );
        }

        public static T BindToConfig<T>(
            string section,
            string label,
            T defaultValue,
            string? description = null
        )
        {
            if (ModConfig != null)
            {
                return ModConfig.Bind(section, label, defaultValue, description ?? label).Value;
            }
            return defaultValue;
        }
    }
}
