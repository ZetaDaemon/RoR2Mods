using EntityStates.Commando.CommandoWeapon;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using On_ThrowGrenade = On.EntityStates.Commando.CommandoWeapon.ThrowGrenade;

namespace ZetterSkillTweaks.Skills.Commando;

public class Grenade : SkillBase
{
    protected override string CONFIG_SECTION => "Grenade";
    private static float GrenadeDamage;
    private static float GrenadeMeshScale;
    private static float BlastRadius;

    // Scales the particle based on the new blast radius, vanilla radius is 11
    // also the vanilla particle is a little small hence the * 1.25
    private static readonly float grenadeExplosionParticleScale = BlastRadius / 11 * 1.25f;

    protected override void InitConfig()
    {
        GrenadeDamage = BindToConfig("Grenade Damage", 8);
        GrenadeMeshScale = BindToConfig("Grenade Mesh Scale", 2);
        BlastRadius = BindToConfig("Blast Radius", 13);
    }

    protected override void Setup()
    {
        EntityStateConfiguration throwGrenadeState = Addressables
            .LoadAssetAsync<EntityStateConfiguration>(
                RoR2BepInExPack
                    .GameAssetPaths
                    .Version_1_39_0
                    .RoR2_Base_Commando
                    .EntityStates_Commando_CommandoWeapon_ThrowGrenade_asset
            )
            .WaitForCompletion();
        ref HG.GeneralSerializer.SerializedField field =
            ref throwGrenadeState.serializedFieldsCollection.GetOrCreateField("damageCoefficient");
        field.fieldValue.stringValue = $"{GrenadeDamage}";

        LanguageAPI.Add(
            "COMMANDO_SPECIAL_ALT1_DESCRIPTION",
            $"Throw a grenade that explodes for <style=cIsDamage>{GrenadeDamage * 100}% damage</style> and stuns enemies. Can hold up to 2."
        );

        GameObject projectilePrefab = Addressables
            .LoadAssetAsync<GameObject>(
                RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Commando.CommandoGrenadeProjectile_prefab
            )
            .WaitForCompletion();
        projectilePrefab?.transform.localScale = new(GrenadeMeshScale, GrenadeMeshScale, GrenadeMeshScale);
        if (projectilePrefab.TryGetComponent(out ProjectileImpactExplosion explosion))
        {
            explosion.impactEffect.transform.localScale = new(
                grenadeExplosionParticleScale,
                grenadeExplosionParticleScale,
                grenadeExplosionParticleScale
            );
            explosion.blastRadius = BlastRadius;
        }

        GameObject projectileGhostPrefab = Addressables
            .LoadAssetAsync<GameObject>(
                RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Commando.CommandoGrenadeGhost_prefab
            )
            .WaitForCompletion();
        projectileGhostPrefab?.transform.localScale = new(GrenadeMeshScale, GrenadeMeshScale, GrenadeMeshScale);

        On_ThrowGrenade.ModifyProjectileInfo += ThrowGrenade_ModifyProjectileInfo;
    }

    private void ThrowGrenade_ModifyProjectileInfo(
        On_ThrowGrenade.orig_ModifyProjectileInfo orig,
        ThrowGrenade self,
        ref FireProjectileInfo fireProjectileInfo
    )
    {
        orig(self, ref fireProjectileInfo);
        fireProjectileInfo.damageTypeOverride = new DamageTypeCombo(
            DamageType.Stun1s,
            DamageTypeExtended.Generic,
            DamageSource.Special
        );
    }
}
