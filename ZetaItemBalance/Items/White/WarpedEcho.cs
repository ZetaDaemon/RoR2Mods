using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static R2API.DamageAPI;

namespace ZetaItemBalance.Items.White;

public class WarpedEcho : ItemBase
{
    protected override string CONFIG_SECTION => "Warped Echo";
    private static int BaseSplits;
    private static int StackSplits;
    private static float BaseCooldown;
    private static float StackCooldown;
    private static ModdedDamageType wechoDamageType;

    protected override void InitConfig()
    {
        BaseSplits = BindToConfig("Base Splits", 1);
        StackSplits = BindToConfig("Stack Splits", 1);
        BaseCooldown = BindToConfig("Base Cooldown", 15);
        StackCooldown = BindToConfig("Stack Cooldown", 0.05f);
    }

    protected override void Setup()
    {
        IL.RoR2.HealthComponent.TakeDamageProcess += new ILContext.Manipulator(IL_TakeDamageProcess);
        On.RoR2.CharacterBody.UpdateDelayedDamage += CharacterBody_UpdateDelayedDamaage;

        On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;

        LanguageAPI.Add("ITEM_DELAYEDDAMAGE_PICKUP", "Incoming damage is split into multiple hits over time.");
        LanguageAPI.Add(
            "ITEM_DELAYEDDAMAGE_DESC",
            $"The next source of damage is <style=cIsHealing>spread</style> into <style=cIsUtility>{BaseSplits + 1} <style=cStack>(+{StackSplits} per stack)</style></style> hits. Recharges every <style=cIsUtility>{BaseCooldown} <style=cStack>(-{StackCooldown * 100}% per stack)</style> seconds</style>."
        );

        wechoDamageType = ReserveDamageType();

        BuffDef wechoDebuff = Addressables
            .LoadAssetAsync<BuffDef>(
                RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_DLC2_Items_DelayedDamage.bdDelayedDamageDebuff_asset
            )
            .WaitForCompletion();
        wechoDebuff.isCooldown = true;
    }

    private void HealthComponent_TakeDamageProcess(
        On.RoR2.HealthComponent.orig_TakeDamageProcess orig,
        HealthComponent self,
        DamageInfo damageInfo
    )
    {
        if (self.body is not CharacterBody body)
        {
            orig(self, damageInfo);
            return;
        }
        if (body.TryGetComponent(out WechoBehavior wecho))
        {
            wecho.TryWecho(damageInfo);
        }
        orig(self, damageInfo);
    }

    private class WechoBehavior : BaseItemBodyBehavior
    {
        public class DelayedDamageInfo
        {
            public DamageInfo damageInfo;
            public float tickCounter;
            public float stopwatch;

            public DelayedDamageInfo()
            {
                stopwatch = TICK_INTERVAL;
            }
        }

        private const float TICK_INTERVAL = 0.75f;

        private readonly List<DelayedDamageInfo> DelayedDamageData = [];

        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        public static ItemDef GetItemDef()
        {
            return DLC2Content.Items.DelayedDamage;
        }

        private void UpdateBuff()
        {
            if (stack <= 0)
            {
                return;
            }
            if (body.HasBuff(DLC2Content.Buffs.DelayedDamageBuff))
            {
                return;
            }
            if (body.HasBuff(DLC2Content.Buffs.DelayedDamageDebuff))
            {
                return;
            }
            body.AddBuff(DLC2Content.Buffs.DelayedDamageBuff);
        }

        private void FixedUpdate()
        {
            UpdateBuff();
            float deltaTime = Time.deltaTime;
            foreach (DelayedDamageInfo delayedDamage in DelayedDamageData.ToList())
            {
                delayedDamage.stopwatch -= deltaTime;
                if (delayedDamage.stopwatch > 0)
                {
                    continue;
                }
                delayedDamage.damageInfo.position = body.GetBody().corePosition;
                body.healthComponent.TakeDamage(delayedDamage.damageInfo);
                delayedDamage.tickCounter -= 1;
                if (delayedDamage.tickCounter <= 0)
                {
                    DelayedDamageData.Remove(delayedDamage);
                    continue;
                }
                delayedDamage.stopwatch += TICK_INTERVAL;
            }
        }

        public void TryWecho(DamageInfo damageInfo)
        {
            if (body.HasBuff(DLC2Content.Buffs.HiddenRejectAllDamage))
            {
                return;
            }
            if (DamageAPI.HasModdedDamageType(damageInfo, wechoDamageType))
            {
                return;
            }
            int damageTicks = BaseSplits + StackSplits * (stack - 1);
            if (stack <= 0 || !body.HasBuff(DLC2Content.Buffs.DelayedDamageBuff))
            {
                return;
            }
            body.RemoveBuff(DLC2Content.Buffs.DelayedDamageBuff);
            body.AddTimedBuff(DLC2Content.Buffs.DelayedDamageDebuff, BaseCooldown / (1 + StackCooldown * (stack - 1)));

            damageInfo.damage /= damageTicks + 1;
            DamageInfo repeatDamageInfo = new()
            {
                crit = damageInfo.crit,
                damage = damageInfo.damage,
                damageType = damageInfo.damageType | DamageType.Silent,
                attacker = damageInfo.attacker,
                position = damageInfo.position,
                inflictor = damageInfo.inflictor,
                damageColorIndex = DamageColorIndex.DelayedDamage,
            };
            DamageAPI.AddModdedDamageType(repeatDamageInfo, wechoDamageType);
            DelayedDamageInfo delayedDamageInfo = new() { damageInfo = repeatDamageInfo, tickCounter = damageTicks };
            DelayedDamageData.Add(delayedDamageInfo);
        }
    }

    private void IL_TakeDamageProcess(ILContext il)
    {
        ILCursor ilcursor = new(il);
        if (
            !ilcursor.TryGotoNext(
                x => x.MatchLdsfld(typeof(DLC2Content.Buffs), nameof(DLC2Content.Buffs.DelayedDamageBuff)),
                x => x.MatchCallOrCallvirt(typeof(CharacterBody), nameof(CharacterBody.HasBuff))
            )
        )
        {
            MainPlugin.ModLogger.LogError("Wecho take damage - IL Hook Failed");
            return;
        }
        ilcursor.Index += 2;
        ilcursor.Emit(OpCodes.Pop);
        ilcursor.Emit(OpCodes.Ldc_I4_0);
    }

    private void CharacterBody_UpdateDelayedDamaage(
        On.RoR2.CharacterBody.orig_UpdateDelayedDamage orig,
        CharacterBody self,
        float deltaTime
    ) { }
}
