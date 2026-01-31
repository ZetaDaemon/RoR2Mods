using BepInEx;
using R2API;

namespace BleedRework
{
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class MainPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "ZetaDaemon";
        public const string PluginName = "BleedRework";
        public const string PluginVersion = "1.0.0";

        internal static BepInEx.Logging.ManualLogSource ModLogger;
        public static PluginInfo pluginInfo;

        private void Awake()
        {
            ModLogger = Logger;
            pluginInfo = Info;
            Configs.Setup();
            EnableChanges();
        }

        private void EnableChanges()
        {
            new Bleed();
        }
    }
}
