using EntityStates;
using RoR2;
using UnityEngine;

namespace ZetaSkills.Captain.BlasterPrimary
{
    public class FireBlaster : GenericBulletBaseState
    {
        public static float BaseDamage = 1.5f;
        public static float ChargeDamage = 10f;
        private readonly float ChargePercent;
        private const float ChargeThreshold = 0.75f;
        private const string LightSoundString = "Play_railgunner_m2_alt_fire";
        private const string HeavySoundString = "Play_railgunner_m2_fire";
        public static GameObject LightTracerPrefab;
        public static GameObject HeavyTracerPrefab;
        public static GameObject LightImpactPrefab;
        public static GameObject HeavyImpactPrefab;
        public static GameObject MuzzleFlashPrefab;

        public FireBlaster()
        {
            ChargePercent = 0f;
            Setup();
        }

        public FireBlaster(float chargePercent)
        {
            ChargePercent = chargePercent;
            Setup();
        }

        private void Setup()
        {
            damageCoefficient = BaseDamage + ChargePercent * (ChargeDamage - BaseDamage);
            baseDuration = 0.2f + 0.2f * ChargePercent;
            bulletRadius = 0.5f + ChargePercent;
            recoilAmplitudeY = 4 * ChargePercent;
            recoilAmplitudeX = 2 * ChargePercent;
            muzzleName = "MuzzleGun";
            tracerEffectPrefab = (ChargePercent <= ChargeThreshold) ? LightTracerPrefab : HeavyTracerPrefab;
            minSpread = 0;
            maxSpread = 0;
            useSmartCollision = true;
            muzzleFlashPrefab = MuzzleFlashPrefab;
            maxDistance = 4000;
            procCoefficient = 1 + 0.5f * ChargePercent;
            fireSoundString = (ChargePercent <= ChargeThreshold) ? LightSoundString : HeavySoundString;
            hitEffectPrefab = (ChargePercent <= ChargeThreshold) ? LightImpactPrefab : HeavyImpactPrefab;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation("Gesture, Additive", "FireCaptainShotgun");
            PlayAnimation("Gesture, Override", "FireCaptainShotgun");
        }
        public override void ModifyBullet(BulletAttack bulletAttack)
        {
            base.ModifyBullet(bulletAttack);
            bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
            bulletAttack.minSpread = 0;
            bulletAttack.maxSpread = 0;
            if (ChargePercent > ChargeThreshold)
            {
                bulletAttack.stopperMask = LayerIndex.world.mask;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

    }
}