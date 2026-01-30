using System;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;

namespace ZetaItemBalance.Items.Green
{
    public class BustlingFungus : ItemBase
    {
        protected override string CONFIG_SECTION => "Bustling Fungus";
        float HealDelay;
        float BaseHeal;
        float StackHeal;
        float HealRampMult;
        float HealRampTime;

        protected override void InitConfig()
        {
            HealDelay = BindToConfig("Heal Delay", 0.1f);
            BaseHeal = BindToConfig("Base Heal", 0.01f);
            StackHeal = BindToConfig("Stack Heal", 0.01f);
            HealRampMult = BindToConfig("Heal Ramp Mult", 5f);
            HealRampTime = BindToConfig("Heal Ramp Time", 3f);
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
                if (item != RoR2Content.Items.Mushroom)
                {
                    continue;
                }
                Utils.FixIconRarity(item, ItemTier.Tier2);
                item.tier = ItemTier.Tier2;
            }
        }

        private void ClampConfig()
        {
            HealDelay = Math.Max(0, HealDelay);
            BaseHeal = Math.Max(0, BaseHeal);
            StackHeal = Math.Max(0, StackHeal);
            HealRampMult = Math.Max(1, HealRampMult);
            HealRampTime = Math.Max(0, HealRampTime);
        }

        private void UpdateText()
        {
            string pickup = "Heal all nearby allies while standing still, healing increases over time.";
            string desc =
                $"While standing still, create a zone that <style=cIsHealing>heals</style> for <style=cIsHealing>{BaseHeal * 100}%</style> <style=cStack>(+{StackHeal * 100}% per stack)</style> of your <style=cIsHealing>health</style> every second to all allies within <style=cIsHealing>3m</style> <style=cStack>(+1.5m per stack)</style>. <style=cIsHealing>Healing</style> increases up to <style=cIsHealing>{BaseHeal * HealRampMult * 100}%</style> <style=cStack>(+{StackHeal * HealRampMult * 100}% per stack)</style> over {HealRampTime} seconds.";

            LanguageAPI.Add("ITEM_MUSHROOM_PICKUP", pickup);
            LanguageAPI.Add("ITEM_MUSHROOM_DESC", desc);
        }

        private void Hooks()
        {
            IL.RoR2.Items.MushroomBodyBehavior.FixedUpdate += new ILContext.Manipulator(IL_FixedUpdate);
            SharedHooks.Handle_CatalogSetItemDefs_Actions += ItemCatalog_SetItemDefs;
        }

        private void IL_FixedUpdate(ILContext il)
        {
            ILCursor ilcursor = new(il);
            // Overide start heal delay
            if (!ilcursor.TryGotoNext(x => x.MatchCallvirt(typeof(CharacterBody), "GetNotMoving")))
            {
                MainPlugin.ModLogger.LogError("Bustling Fungus - Heal Delay - IL Hook Failed");
                return;
            }
            ilcursor.Remove();
            ilcursor.EmitDelegate<Func<CharacterBody, bool>>(
                (cb) =>
                {
                    return cb is not null && cb.notMovingStopwatch >= HealDelay;
                }
            );

            // Apply healing ramp up
            if (
                !ilcursor.TryGotoNext(
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld(typeof(RoR2.Items.MushroomBodyBehavior), "mushroomHealingWard"),
                    x => x.MatchLdcR4(0.045f),
                    x => x.MatchLdcR4(0.0225f)
                )
            )
            {
                MainPlugin.ModLogger.LogError("Bustling Fungus - Heal Ramp - IL Hook Failed");
                return;
            }
            ilcursor.Index += 2;
            ilcursor.Next.Operand = BaseHeal;
            ilcursor.Index += 1;
            ilcursor.Next.Operand = StackHeal;

            if (!ilcursor.TryGotoNext(x => x.MatchStfld(typeof(HealingWard), "healFraction")))
            {
                MainPlugin.ModLogger.LogError("Bustling Fungus - Heal Ramp 2 - IL Hook Failed");
                return;
            }
            ilcursor.Emit(OpCodes.Ldarg_0);
            ilcursor.Emit<RoR2.Items.BaseItemBodyBehavior>(OpCodes.Call, "get_body");
            ilcursor.EmitDelegate<Func<CharacterBody, float>>(
                (cb) =>
                {
                    if (cb is null)
                    {
                        return 1;
                    }
                    return 1 + (HealRampMult - 1) * Math.Min(cb.notMovingStopwatch / HealRampTime, 1);
                }
            );
            ilcursor.Emit(OpCodes.Mul);
        }
    }
}
