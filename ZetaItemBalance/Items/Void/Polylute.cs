using System;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;

namespace ZetaItemBalance.Items.Void
{
    public class Polylute : ItemBase
    {
        protected override string CONFIG_SECTION => "Polylute";
        internal static int BaseExplosions;
        internal static int StackExplosions;

        protected override void InitConfig()
        {
            BaseExplosions = BindToConfig("Base Explosions", 2);
            StackExplosions = BindToConfig("Stack Explosions", 2);
        }

        protected override void Setup()
        {
            ClampConfig();
            UpdateText();
            Hooks();
        }

        private void ClampConfig()
        {
            BaseExplosions = Math.Max(0, BaseExplosions);
            StackExplosions = Math.Max(0, StackExplosions);
        }

        private void UpdateText()
        {
            string desc =
                $"<style=cIsDamage>25%</style> chance to fire <style=cIsDamage>lightning</style> for <style=cIsDamage>60%</style> TOTAL damage <style=cIsDamage>{BaseExplosions} <style=cStack>(+{StackExplosions} per stack)</style></style> times. <style=cIsVoid>Corrupts all Ukuleles</style>.";

            LanguageAPI.Add("ITEM_CHAINLIGHTNINGVOID_DESC", desc);
        }

        private void Hooks()
        {
            IL.RoR2.GlobalEventManager.ProcessHitEnemy += new ILContext.Manipulator(IL_ProcessHitEnemy);
        }

        private void IL_ProcessHitEnemy(ILContext il)
        {
            ILCursor ilcursor = new(il);
            if (!ilcursor.TryGotoNext(x => x.MatchStfld(typeof(RoR2.Orbs.VoidLightningOrb), "totalStrikes")))
            {
                MainPlugin.ModLogger.LogError("Polylute - Explosion Count - IL Hook Failed");
                return;
            }
            ilcursor.Index -= 3;
            ilcursor.Next.Operand = StackExplosions;
            ilcursor.Index += 2;
            ilcursor.Emit(OpCodes.Ldc_I4_1);
            ilcursor.Emit(OpCodes.Sub);
            ilcursor.Index += 1;
            ilcursor.Emit(OpCodes.Ldc_I4, BaseExplosions);
            ilcursor.Emit(OpCodes.Add);
        }
    }
}
