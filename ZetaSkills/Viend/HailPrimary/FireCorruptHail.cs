using EntityStates;
using RoR2;
using UnityEngine;

namespace ZetaSkills.Viend.HailPrimary
{
    public class FireCorruptHail : GenericBulletBaseState
    {
        public const float Damage = 1.5f;
        private const float BaseDuration = 0.5f;
        private const float SpreadBloom = 0.0f;
        private const string FireSoundString = "Play_voidman_m1_shoot";
        private const string animationLayerName = "LeftArm, Override";
        private const string animationEnterStateName = "FireCorruptHandBeam";
        private const string animationExitStateName = "ExitHandBeam";
        public static GameObject TracerPrefab;
        public static GameObject ImpactPrefab;
        public static GameObject MuzzleFlashPrefab;
        public FireCorruptHail()
        {
            Setup();
        }

        private void Setup()
        {
            damageCoefficient = Damage;
            baseDuration = BaseDuration;
            fireSoundString = FireSoundString;
            tracerEffectPrefab = TracerPrefab;
            muzzleFlashPrefab = MuzzleFlashPrefab;
            muzzleName = "MuzzleHandBeam";
            hitEffectPrefab = ImpactPrefab;
            recoilAmplitudeY = 0;
            recoilAmplitudeX = 0;
            minSpread = 2;
            maxSpread = 2;
            bulletCount = 3;
            force = 100;
            maxDistance = 2000;
            useSmartCollision = true;
            bulletRadius = 0.5f;
            spreadBloomValue = SpreadBloom;
            procCoefficient = 0.75f;
        }
        public override void ModifyBullet(BulletAttack bulletAttack)
        {
            base.ModifyBullet(bulletAttack);
            bulletAttack.minSpread = 0;
            bulletAttack.maxSpread = 0;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            base.characterBody.SetSpreadBloom(0);
            PlayAnimation(animationLayerName, animationEnterStateName);
        }

        public override void OnExit()
        {
            base.OnExit();
            PlayAnimation(animationLayerName, animationExitStateName);
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