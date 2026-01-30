using System;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using UnityEngine;

namespace ZetaItemBalance.Items.Green
{
    public class SquidPolyp : ItemBase
    {
        protected override string CONFIG_SECTION => "Squid Polyp";
        internal static float Knockback;
        internal static float StackAttackSpeed;
        internal static float StackDamage;
        internal static ItemDef SqualopItem;

        protected override void InitConfig()
        {
            Knockback = BindToConfig("Knockback", 0.25f);
            StackAttackSpeed = BindToConfig("StackAttackSpeed", 0.75f);
            StackDamage = BindToConfig("StackDamage", 0.15f);
        }

        protected override void Setup()
        {
            ClampConfig();
            UpdateText();
            Hooks();
            SqualopItem = ScriptableObject.CreateInstance<ItemDef>();
            SqualopItem.tier = ItemTier.NoTier;
            SqualopItem.hidden = true;
        }

        private void ClampConfig()
        {
            Knockback = Math.Max(0, Knockback);
            StackAttackSpeed = Math.Max(0, StackAttackSpeed);
            StackDamage = Math.Max(0, StackDamage);
        }

        private void UpdateText()
        {
            string desc =
                $"Activating an interactable summons a <style=cIsDamage>Squid Turret</style> that attacks nearby enemies, gaining <style=cIsDamage><style=cStack>+{StackAttackSpeed * 100}%</style> attack speed</style> and <style=cIsDamage><style=cStack>+{StackDamage * 100}%</style> damage</style> per stack. Lasts <style=cIsUtility>30</style> seconds.";

            LanguageAPI.Add("ITEM_SQUIDTURRET_DESC", desc);
        }

        private void Hooks()
        {
            IL.RoR2.CharacterBody.RecalculateStats += new ILContext.Manipulator(IL_RecalculateStats);
            IL.EntityStates.Squid.SquidWeapon.FireSpine.FireOrbArrow += new ILContext.Manipulator(IL_FireOrb);
        }

        private void IL_RecalculateStats(ILContext il)
        {
            ILCursor ilcursor = new(il);
            if (!ilcursor.TryGotoNext(x => x.MatchLdsfld(typeof(RoR2Content.Items), "BoostAttackSpeed")))
            {
                MainPlugin.ModLogger.LogError("Squid Polyp - Speed 1 - IL Hook Failed");
                return;
            }
            ilcursor.Index += 2;
            int boostAttackSpeedCountLoc = ((VariableDefinition)ilcursor.Next.Operand).Index;
            if (!ilcursor.TryGotoNext(x => x.MatchLdloc(boostAttackSpeedCountLoc)))
            {
                MainPlugin.ModLogger.LogError("Squid Polyp - Speed 2 - IL Hook Failed");
                return;
            }
            ilcursor.Index += 3;
            ilcursor.Emit(OpCodes.Pop);
            ilcursor.Emit(OpCodes.Ldarg_0);
            ilcursor.EmitDelegate<Func<CharacterBody, float>>(
                (body) =>
                {
                    if (body?.bodyIndex == BodyCatalog.FindBodyIndex("SquidTurretBody"))
                    {
                        return StackAttackSpeed;
                    }
                    return 0.1f;
                }
            );

            ilcursor = new(il);
            if (!ilcursor.TryGotoNext(x => x.MatchStfld(typeof(CharacterBody), "damageFromRecalculateStats")))
            {
                MainPlugin.ModLogger.LogError("Squid Polyp - Damage 1 - IL Hook Failed");
                return;
            }
            ilcursor.Index -= 4;
            ilcursor.Emit(OpCodes.Ldarg_0);
            ilcursor.Emit(OpCodes.Ldloc, boostAttackSpeedCountLoc);
            ilcursor.EmitDelegate<Func<CharacterBody, int, float>>(
                (body, stacks) =>
                {
                    if (body?.bodyIndex == BodyCatalog.FindBodyIndex("SquidTurretBody"))
                    {
                        return 1f + stacks / 10f * StackDamage;
                    }
                    return 1f;
                }
            );
            ilcursor.Emit(OpCodes.Mul);
        }

        private void IL_FireOrb(ILContext il)
        {
            ILCursor ilcursor = new(il);
            if (
                !ilcursor.TryGotoNext(
                    x => x.MatchLdsfld("EntityStates.Squid.SquidWeapon.FireSpine", "forceScalar"),
                    x => x.MatchStfld(typeof(RoR2.Orbs.SquidOrb), "forceScalar")
                )
            )
            {
                MainPlugin.ModLogger.LogError("Squid Polyp - Knockback - IL Hook Failed");
                return;
            }
            ilcursor.Index += 1;
            ilcursor.Emit(OpCodes.Pop);
            ilcursor.Emit(OpCodes.Ldc_R4, Knockback);
        }
    }
}
