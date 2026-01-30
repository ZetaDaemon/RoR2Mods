using System;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;

namespace ZetaItemBalance.Items.Void
{
    public class SingularityBand : ItemBase
    {
        protected override string CONFIG_SECTION => "Singularity Band";
        internal static float BaseDamage;
        internal static float StackDamage;
        internal static int Cooldown;

        protected override void InitConfig()
        {
            BaseDamage = BindToConfig("Base Damage", 2f);
            StackDamage = BindToConfig("Stack Damage", 2f);
            Cooldown = BindToConfig("Cooldown", 10);
        }

        protected override void Setup()
        {
            ClampConfig();
            UpdateText();
            Hooks();
        }

        private void ClampConfig()
        {
            BaseDamage = Math.Max(0, BaseDamage);
            StackDamage = Math.Max(0, StackDamage);
            Cooldown = Math.Max(0, Cooldown);
        }

        private void UpdateText()
        {
            string desc =
                $"Hits that deal <style=cIsDamage>more than 400% damage</style> also fire a black hole that <style=cIsUtility>draws enemies within 15m into its center</style>. Lasts <style=cIsUtility>5</style> seconds before collapsing, dealing <style=cIsDamage>{BaseDamage * 100}%</style> <style=cStack>(+{StackDamage * 100}% per stack)</style> TOTAL damage. Recharges every <style=cIsUtility>{Cooldown}</style> seconds. <style=cIsVoid>Corrupts all Runald's and Kjaro's Bands</style>.";

            LanguageAPI.Add("ITEM_ELEMENTALRINGVOID_DESC", desc);
        }

        private void Hooks()
        {
            IL.RoR2.GlobalEventManager.OnHitEnemy += new ILContext.Manipulator(IL_OnHitEnemy);
        }

        private void IL_OnHitEnemy(ILContext il)
        {
            ILCursor ilcursor = new(il);
            if (
                !ilcursor.TryGotoNext(x =>
                    x.MatchLdsfld(typeof(RoR2.DLC1Content.Buffs), "ElementalRingVoidCooldown")
                /*x => x.MatchLdloc(99),
                x => x.MatchConvR4(),
                x => x.MatchLdcR4(20f),
                x => x.MatchBle(out var _)*/
                )
            )
            {
                MainPlugin.ModLogger.LogError("Singularity Band - Cooldown - IL Hook Failed");
                return;
            }
            ilcursor.Index += 10;
            ilcursor.Next.Operand = (float)Cooldown;

            if (
                !ilcursor.TryGotoNext(x =>
                    x.MatchLdstr("Prefabs/Projectiles/ElementalRingVoidBlackHole")
                /*x => x.MatchLdcR4(1),
                x => x.MatchLdloc(97),
                x => x.MatchConvR4(),
                x => x.MatchMul(),
                x => x.MatchStloc(101)*/
                )
            )
            {
                MainPlugin.ModLogger.LogError("Singularity Band - Damage - IL Hook Failed");
                return;
            }
            ilcursor.Index += 3;
            ilcursor.Next.Operand = StackDamage;
            ilcursor.Index += 2;
            ilcursor.Emit(OpCodes.Ldc_I4_1);
            ilcursor.Emit(OpCodes.Sub);
            ilcursor.Index += 2;
            ilcursor.Emit(OpCodes.Ldc_R4, BaseDamage);
            ilcursor.Emit(OpCodes.Add);
        }
    }
}
