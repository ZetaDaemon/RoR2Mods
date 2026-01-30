using System;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ZetterSkillTweaks.Skills.Commando;

public class PhaseRound : SkillBase
{
    protected override string CONFIG_SECTION => "Phase Round";
    private static float BaseDamage;
    private static float PenetrateDamageScale;

    private static readonly float Force = 2000;
    private static readonly float BaseDuration = 0.5f;
    private static readonly float RecoilAmplitude = 1.5f;
    private static readonly float BulletRadius = 2f;
    private static readonly float MaxDistance = 9999;

    private static GameObject MuzzleFlashPrefab;
    private static GameObject TracerEffectPrefab;
    private static GameObject HitEffectPrefab;

    protected override void InitConfig()
    {
        BaseDamage = BindToConfig("Base Damage", 3);
        PenetrateDamageScale = BindToConfig("Penetrate Damage Scale", 0.5f);
    }

    protected override void Setup()
    {
        R2API.ContentAddition.AddEntityState(typeof(FireFMJ), out bool added);

        SkillDef PhaseroundSkill = Addressables
            .LoadAssetAsync<SkillDef>(
                RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Commando.CommandoBodyFireFMJ_asset
            )
            .WaitForCompletion();

        PhaseroundSkill.activationState = new SerializableEntityStateType(typeof(FireFMJ));

        MuzzleFlashPrefab = Addressables
            .LoadAssetAsync<GameObject>(
                RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Commando.MuzzleflashFMJ_prefab
            )
            .WaitForCompletion();
        TracerEffectPrefab = Addressables
            .LoadAssetAsync<GameObject>(
                RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_DLC1_Railgunner.TracerRailgunCryo_prefab
            )
            .WaitForCompletion();
        HitEffectPrefab = Addressables
            .LoadAssetAsync<GameObject>(
                RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Huntress.OmniImpactVFXHuntress_prefab
            )
            .WaitForCompletion();
    }

    private class FireFMJ : GenericBulletBaseState
    {
        private readonly int FireFMJStateHash = Animator.StringToHash("FireFMJ");
        private readonly int FireFMJParamHash = Animator.StringToHash("FireFMJ.playbackRate");

        public override void OnEnter()
        {
            damageCoefficient = BaseDamage;
            force = Force;
            baseDuration = BaseDuration;
            recoilAmplitudeX = recoilAmplitudeY = RecoilAmplitude;
            bulletRadius = BulletRadius;
            minSpread = maxSpread = 0;
            spreadBloomValue = 0.3f;
            maxDistance = MaxDistance;
            useSmartCollision = true;

            fireSoundString = "Play_commando_M2";
            muzzleName = "MuzzleCenter";

            muzzleFlashPrefab = MuzzleFlashPrefab;
            tracerEffectPrefab = TracerEffectPrefab;
            hitEffectPrefab = HitEffectPrefab;
            base.OnEnter();
        }

        public override void PlayFireAnimation()
        {
            if ((bool)GetModelAnimator())
            {
                PlayAnimation("Gesture, Additive", FireFMJStateHash, FireFMJParamHash, duration);
                PlayAnimation("Gesture, Override", FireFMJStateHash, FireFMJParamHash, duration);
            }
        }

        public override void ModifyBullet(BulletAttack bulletAttack)
        {
            bulletAttack.damageType = new DamageTypeCombo(
                DamageTypeCombo.Generic,
                DamageTypeExtended.Generic,
                DamageSource.Secondary
            );
            bulletAttack.allowTrajectoryAimAssist = false;
            bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
            bulletAttack.stopperMask = LayerIndex.world.mask;
            bulletAttack.modifyOutgoingDamageCallback = delegate(
                BulletAttack _bulletAttack,
                ref BulletAttack.BulletHit hitInfo,
                DamageInfo damageInfo
            )
            {
                _bulletAttack.damage *= 1 + PenetrateDamageScale;
            };
        }
    }
}
