using BepInEx;
using R2API;
using ZetaSkills.Captain.BlasterPrimary;
using ZetaSkills.Commando.BigIronsPrimary;
using ZetaSkills.Viend;
using ZetaSkills.Viend.HailPrimary;

namespace ZetaSkills
{
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]

    [BepInDependency(LanguageAPI.PluginGUID)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class MainPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "ZetaDaemon";
        public const string PluginName = "ZetaSkills";
        public const string PluginVersion = "1.0.0";

        internal static BepInEx.Logging.ManualLogSource ModLogger;
        public static PluginInfo pluginInfo;

        private void Awake()
        {
            ModLogger = this.Logger;
            pluginInfo = Info;
            //Configs.Setup();
            EnableChanges();
            //SharedHooks.Setup();
        }
        private void EnableChanges()
        {
            new CorruptionManager();

            new Blaster();
            new BigIrons();
            new Hail();
        }
    }
}
