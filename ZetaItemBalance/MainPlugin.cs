using System;
using System.Linq;
using BepInEx;
using R2API;
using ZetaItemBalance.Items;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace ZetaItemBalance
{
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(DamageAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class MainPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Zetters";
        public const string PluginName = "ItemBalanceTweaks";
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
            SharedHooks.Setup();
        }

        private void EnableChanges()
        {
            var types = typeof(MainPlugin)
                .Assembly.GetTypes()
                .Where(t => !t.IsAbstract && typeof(ItemBase).IsAssignableFrom(t));
            foreach (var type in types)
            {
                Activator.CreateInstance(type);
            }
        }
    }
}
