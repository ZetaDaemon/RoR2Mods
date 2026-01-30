using EntityStates;
using RoR2;
using UnityEngine;

namespace ZetaSkills.Viend.HailPrimary
{
    public class FireHail : GenericBulletBaseState
    {
        public const float Damage = 0.75f;
        private const float BaseDuration = 0.1f;
        private const float SpreadBloom = 0.1f;
        private const string FireSoundString = "Play_voidman_m1_shoot";
        private const string animationLayerName = "LeftArm, Override";
        private const string animationStateName = "FireHandBeam";
        private const string animationPlaybackRateParam = "HandBeam.playbackRate";
        public static GameObject TracerPrefab;
        public static GameObject ImpactPrefab;
        public static GameObject MuzzleFlashPrefab;
        public FireHail()
        {
            Setup();
        }

        private void Setup()
        {
            damageCoefficient = Damage;
            baseDuration = BaseDuration;
            fireSoundString = FireSoundString;
            tracerEffectPrefab = TracerPrefab;
            muzzleName = "MuzzleHandBeam";
            muzzleFlashPrefab = MuzzleFlashPrefab;
            hitEffectPrefab = ImpactPrefab;
            recoilAmplitudeY = 0;
            recoilAmplitudeX = 0;
            minSpread = 1;
            maxSpread = 1;
            force = 100;
            maxDistance = 100;
            useSmartCollision = true;
            bulletRadius = 0.1f;
            spreadBloomValue = SpreadBloom;
        }
        public override void ModifyBullet(BulletAttack bulletAttack)
        {
            base.ModifyBullet(bulletAttack);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
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