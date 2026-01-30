using System;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;

namespace ZetaItemBalance.Items.White
{
    public class LeechingSeed : ItemBase
    {
        protected override string CONFIG_SECTION => "Leeching Seed";
        internal static float BaseHeal;
        internal static float StackHeal;

        protected override void InitConfig()
        {
            BaseHeal = BindToConfig("Base Heal", 0.01f);
            StackHeal = BindToConfig("Stack Heal", 0.01f);
        }

        protected override void Setup()
        {
            ClampConfig();
            UpdateText();
            Hooks();
        }

        public void ItemCatalog_SetItemDefs(ItemDef[] newItemDefs)
        {
            foreach (ItemDef item in newItemDefs)
            {
                if (item != RoR2Content.Items.Seed)
                {
                    continue;
                }
                Utils.FixIconRarity(item, ItemTier.Tier1);
                item.tier = ItemTier.Tier1;
            }
        }

        private void ClampConfig()
        {
            BaseHeal = Math.Max(0, BaseHeal);
            StackHeal = Math.Max(0, StackHeal);
        }

        private void UpdateText()
        {
            string desc =
                $"Dealing damage <style=cIsHealing>heals</style> you for <style=cIsHealing>{BaseHeal * 100}% </style> <style=cStack>(+{StackHeal * 100}% per stack)</style> of the damage dealt.";
            LanguageAPI.Add("ITEM_SEED_DESC", desc);
        }

        private void Hooks()
        {
            IL.RoR2.GlobalEventManager.ProcessHitEnemy += new ILContext.Manipulator(IL_ProcessHitEnemy);
            SharedHooks.Handle_GlobalHitEvent_Actions += GlobalEventManager_ProcessHitEnemy;
            SharedHooks.Handle_CatalogSetItemDefs_Actions += ItemCatalog_SetItemDefs;
        }

        private void GlobalEventManager_ProcessHitEnemy(
            CharacterBody victimBody,
            CharacterBody attackerBody,
            DamageInfo damageInfo
        )
        {
            Inventory inventory = attackerBody?.master?.inventory;
            if (inventory is null)
            {
                return;
            }
            int itemCount = inventory.GetItemCountEffective(RoR2Content.Items.Seed);
            if (itemCount <= 0)
            {
                return;
            }
            HealthComponent healthComponent = attackerBody?.GetComponent<HealthComponent>();
            if (healthComponent is null)
            {
                return;
            }
            damageInfo?.procChainMask.AddProc(ProcType.HealOnHit);
            float healAmount = (BaseHeal + (itemCount - 1) * StackHeal) * damageInfo.damage; // * damageInfo.procCoefficient;
            healthComponent.Heal(healAmount, damageInfo.procChainMask);
        }

        private void IL_ProcessHitEnemy(ILContext il)
        {
            ILCursor ilcursor = new(il);
            if (!ilcursor.TryGotoNext(x => x.MatchLdsfld(typeof(RoR2Content.Items), "Seed")))
            {
                MainPlugin.ModLogger.LogError("Leeching Seed - Heal - IL Hook Failed");
                return;
            }
            ilcursor.Index += 2;
            ilcursor.Emit(OpCodes.Pop);
            ilcursor.Emit(OpCodes.Ldc_I4_0);
        }
    }
}
