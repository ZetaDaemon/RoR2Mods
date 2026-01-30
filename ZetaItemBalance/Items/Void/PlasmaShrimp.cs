using System;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;

namespace ZetaItemBalance.Items.Void
{
    public class PlasmaShrimp : ItemBase
    {
        protected override string CONFIG_SECTION => "Plasma Shrimp";
        float BaseDamage;
        float StackDamage;

        protected override void InitConfig()
        {
            BaseDamage = BindToConfig("Base Damage", 0.3f);
            StackDamage = BindToConfig("Stack Damage", 0.3f);
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
                $"Gain a <style=cIsHealing>shield</style> equal to <style=cIsHealing>10%</style> of your maximum health. While you have a <style=cIsHealing>shield</style>, hitting an enemy fires a missile that deals <style=cIsDamage>{BaseDamage * 100}%</style> <style=cStack>(+{StackDamage * 100}% per stack)</style> TOTAL damage. <style=cIsVoid>Corrupts all AtG Missile Mk. 1s</style>.";

            LanguageAPI.Add("ITEM_MISSILEVOID_DESC", desc);
        }

        private void Hooks()
        {
            IL.RoR2.GlobalEventManager.ProcessHitEnemy += new ILContext.Manipulator(IL_ProcessHitEnemy);
        }

        private void IL_ProcessHitEnemy(ILContext il)
        {
            ILCursor ilcursor = new(il);
            if (!ilcursor.TryGotoNext(x => x.MatchLdsfld("RoR2.DLC1Content/Items", "MissileVoid")))
            {
                MainPlugin.ModLogger.LogError("Plasma Shrimp 1 - Damage - IL Hook Failed");
                return;
            }
            if (
                !ilcursor.TryGotoNext(x =>
                    x.MatchCallOrCallvirt(
                        typeof(RoR2.MissileUtils),
                        nameof(RoR2.MissileUtils.GetMoreMissileDamageMultiplier)
                    )
                )
            )
            {
                MainPlugin.ModLogger.LogError("Plasma Shrimp 2 - Damage - IL Hook Failed");
                return;
            }
            ilcursor.Index += 2;
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
