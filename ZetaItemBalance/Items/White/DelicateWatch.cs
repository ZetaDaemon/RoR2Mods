using System;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;

namespace ZetaItemBalance.Items.White
{
    public class DelicateWatch : ItemBase
    {
        protected override string CONFIG_SECTION => "Delicate Watch";
        internal static float BaseDamage;
        internal static float StackDamage;

        protected override void InitConfig()
        {
            BaseDamage = BindToConfig("Base Damage", 0.1f);
            StackDamage = BindToConfig("Stack Damage", 0.1f);
        }

        protected override void Setup()
        {
            ClampConfig();
            UpdateText();
            Hooks();
        }

        private void ClampConfig()
        {
            BaseDamage = Math.Max(0f, BaseDamage);
            StackDamage = Math.Max(0f, StackDamage);
        }

        private void UpdateText()
        {
            string desc =
                $"Increase damage by <style=cIsDamage>{BaseDamage * 100}%</style> <style=cStack>(+{StackDamage * 100}% per stack)</style>. Taking damage to below <style=cIsHealth>25% health</style> <style=cIsUtility>breaks</style> this item. At the start of each stage, it regenerates.";

            LanguageAPI.Add("ITEM_FRAGILEDAMAGEBONUS_DESC", desc);
        }

        private void Hooks()
        {
            IL.RoR2.HealthComponent.TakeDamageProcess += new ILContext.Manipulator(IL_TakeDamageProcess);
            On.RoR2.CharacterMaster.TryRegenerateScrap += CharacterMaster_TryRegenerateScrap;
        }

        private static void CharacterMaster_TryRegenerateScrap(
            On.RoR2.CharacterMaster.orig_TryRegenerateScrap orig,
            CharacterMaster self
        )
        {
            orig(self);
            Inventory inventory = self?.inventory;
            if (inventory is null)
            {
                return;
            }
            int itemCount = inventory.GetItemCountEffective(DLC1Content.Items.FragileDamageBonusConsumed);
            if (itemCount > 0)
            {
                inventory.RemoveItemPermanent(DLC1Content.Items.FragileDamageBonusConsumed, itemCount);
                inventory.GiveItemPermanent(DLC1Content.Items.FragileDamageBonus, itemCount);
                CharacterMasterNotificationQueue.SendTransformNotification(
                    self,
                    DLC1Content.Items.FragileDamageBonusConsumed.itemIndex,
                    DLC1Content.Items.FragileDamageBonus.itemIndex,
                    CharacterMasterNotificationQueue.TransformationType.RegeneratingScrapRegen
                );
            }
        }

        private void IL_TakeDamageProcess(ILContext il)
        {
            ILCursor ilcursor = new(il);
            if (!ilcursor.TryGotoNext(x => x.MatchLdsfld(typeof(DLC1Content.Items), "FragileDamageBonus")))
            {
                MainPlugin.ModLogger.LogError("Delicate Watch - Damage - IL Hook Failed");
                return;
            }
            ilcursor.Index += 9;
            ilcursor.Emit(OpCodes.Ldc_I4_1);
            ilcursor.Emit(OpCodes.Sub);
            ilcursor.Index += 1;
            ilcursor.Next.Operand = StackDamage;
            ilcursor.Index += 2;
            ilcursor.Emit(OpCodes.Ldc_R4, BaseDamage);
            ilcursor.Emit(OpCodes.Add);
        }
    }
}
