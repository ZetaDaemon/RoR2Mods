using System.Linq;
using BepInEx;
using R2API;
using ZetterSkillTweaks.Skills;

namespace ZetterSkillTweaks
{
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class MainPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "ZetaDaemon";
        public const string PluginName = "SkillTweaks";
        public const string PluginVersion = "1.0.0";

        internal static BepInEx.Logging.ManualLogSource ModLogger;
        public static PluginInfo pluginInfo;

        public static readonly bool DisableConfig = true;

        private void Awake()
        {
            ModLogger = Logger;
            pluginInfo = Info;
            Configs.Setup();
            EnableChanges();
        }

        private void EnableChanges()
        {
            var types = typeof(MainPlugin)
                .Assembly.GetTypes()
                .Where(t => !t.IsAbstract && typeof(SkillBase).IsAssignableFrom(t));
            foreach (var type in types)
            {
                System.Activator.CreateInstance(type);
            }
        }
    }
}
