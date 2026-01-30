using System;
using EntityStates.JunkCube;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace ZetaItemBalance.Items.Red;

public class ShatteringJustice : ItemBase
{
    protected override string CONFIG_SECTION => "Shattering Justice";
    private float Cooldown;
    private float BaseDamage;
    private float StackDamage;

    public ShatteringJustice()
    {
        Cooldown = BindToConfig("Cooldown", 1);
        BaseDamage = BindToConfig("Base Damage", 1);
        StackDamage = BindToConfig("StackDamage", 1);
    }

    protected override void InitConfig()
    {
        Cooldown = BindToConfig("Cooldown", 1);
        BaseDamage = BindToConfig("Base Damage", 1);
        StackDamage = BindToConfig("StackDamage", 1);
    }

    protected override void Setup()
    {
        IL.RoR2.HealthComponent.TakeDamageProcess += new ILContext.Manipulator(IL_TakeDamageProcess);
        SharedHooks.ModifyDamageRecieved_Actions += ModifyDamage;

        LanguageAPI.Add("ITEM_ARMORREDUCTIONONHIT_PICKUP", "Undamaged enemies take increased damage.");
        LanguageAPI.Add(
            "ITEM_ARMORREDUCTIONONHIT_DESC",
            $"Your next hit deals <style=cIsDamage>+{BaseDamage * 100}%</style> "
                + $"<style=cStack>(+{StackDamage * 100}% per stack)</style>  increased damage. "
                + $"This has a {Cooldown}s cooldown for each enemy."
        );

        BuffDef wechoDebuff = Addressables
            .LoadAssetAsync<BuffDef>(
                RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_ArmorReductionOnHit.bdPulverizeBuildup_asset
            )
            .WaitForCompletion();
        wechoDebuff.isCooldown = true;
        wechoDebuff.isDebuff = false;
    }

    private void IL_TakeDamageProcess(ILContext il)
    {
        ILCursor ilcursor = new(il);

        //Disable the old mechanic
        if (
            !ilcursor.TryGotoNext(x =>
                x.MatchLdsfld(typeof(RoR2Content.Items), nameof(RoR2Content.Items.ArmorReductionOnHit))
            )
        )
        {
            MainPlugin.ModLogger.LogError("Shustice block - IL Hook Failed");
            return;
        }
        ilcursor.Index += 2;
        ilcursor.Emit(OpCodes.Pop);
        ilcursor.Emit(OpCodes.Ldc_I4_0);
    }

    private void ModifyDamage(HealthComponent self, DamageInfo damageInfo, ref float damage)
    {
        CharacterBody body = self.body;
        if (!body)
        {
            return;
        }
        if (!damageInfo.attacker)
        {
            return;
        }
        if (!damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody))
        {
            return;
        }
        Inventory inventory = attackerBody.inventory;
        if (!inventory)
        {
            return;
        }
        if (body.HasBuff(RoR2Content.Buffs.PulverizeBuildup))
        {
            return;
        }
        int stacks = inventory.GetItemCountEffective(RoR2Content.Items.ArmorReductionOnHit);
        if (stacks <= 0)
        {
            return;
        }
        body.AddTimedBuff(RoR2Content.Buffs.PulverizeBuildup, Cooldown);
        damage *= 1 + BaseDamage + StackDamage * (stacks - 1);
    }
}
