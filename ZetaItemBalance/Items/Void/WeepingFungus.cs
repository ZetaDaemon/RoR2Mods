using System;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using UnityEngine.AddressableAssets;

namespace ZetaItemBalance.Items.Void
{
    public class WeepingFungus : ItemBase
    {
        protected override string CONFIG_SECTION => "Weeping Fungus";

        internal static float BaseHeal;
        internal static float StackHeal;

        protected override void InitConfig()
        {
            BaseHeal = BindToConfig("Base Heal", 0.02f);
            StackHeal = BindToConfig("Stack Heal", 0.02f);
        }

        protected override void Setup()
        {
            ClampConfig();
            UpdateText();
            Hooks();

            var item_def = Addressables
                .LoadAssetAsync<ItemDef>("RoR2/DLC1/MushroomVoid/MushroomVoid.asset")
                .WaitForCompletion();
            item_def._itemTierDef = Addressables
                .LoadAssetAsync<ItemTierDef>("RoR2/DLC1/Common/VoidTier2Def.asset")
                .WaitForCompletion();
        }

        private void ClampConfig()
        {
            BaseHeal = Math.Max(0, BaseHeal);
            StackHeal = Math.Max(0, StackHeal);
        }

        private void UpdateText()
        {
            string desc =
                $"<style=cIsHealing>Heals</style> for <style=cIsHealing>{BaseHeal * 100}%</style> <style=cStack>(+{StackHeal * 100}% per stack)</style> of your <style=cIsHealing>health</style> every second <style=cIsUtility>while sprinting</style>. <style=cIsVoid>Corrupts all Bustling Fungi</style>.";

            LanguageAPI.Add("ITEM_MUSHROOMVOID_DESC", desc);
        }

        private void Hooks()
        {
            IL.RoR2.MushroomVoidBehavior.FixedUpdate += new ILContext.Manipulator(IL_FixedUpdate);
        }

        private void IL_FixedUpdate(ILContext il)
        {
            ILCursor ilcursor = new(il);
            if (
                !ilcursor.TryGotoNext(x =>
                    x.MatchLdfld(typeof(MushroomVoidBehavior), nameof(MushroomVoidBehavior.healthComponent))
                )
            )
            {
                MainPlugin.ModLogger.LogError("Bustling Fungus - Heal Delay - IL Hook Failed");
                return;
            }
            ilcursor.Index += 1;
            ilcursor.Next.Operand = StackHeal * 0.5f;
            ilcursor.Index += 3;
            ilcursor.Emit(OpCodes.Ldc_I4_1);
            ilcursor.Emit(OpCodes.Sub);
            ilcursor.Index += 2;
            ilcursor.Emit(OpCodes.Ldc_R4, BaseHeal * 0.5f);
            ilcursor.Emit(OpCodes.Add);
        }
    }
}
