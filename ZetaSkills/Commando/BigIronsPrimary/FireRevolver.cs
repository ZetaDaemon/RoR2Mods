using EntityStates;
using RoR2;
using UnityEngine;

namespace ZetaSkills.Commando.BigIronsPrimary
{
    public class FireRevolver : GenericBulletBaseState
    {
        public const float Damage = 4f;
        private const float BaseDuration = 0.2f;
        private const string FireSoundString = "Play_bandit2_R_fire";
        public static GameObject TracerPrefab;
        public static GameObject ImpactPrefab;
        public static GameObject MuzzleFlashPrefab;

        private readonly bool Left;

        public FireRevolver()
        {
            Left = false;
            Setup();
        }
        public FireRevolver(bool left)
        {
            Left = left;
            Setup();
        }

        private void Setup()
        {
            damageCoefficient = Damage;
            baseDuration = BaseDuration;
            fireSoundString = FireSoundString;
            tracerEffectPrefab = TracerPrefab;
            muzzleFlashPrefab = MuzzleFlashPrefab;
            hitEffectPrefab = ImpactPrefab;
            recoilAmplitudeY = 1;
            recoilAmplitudeX = 1;
            minSpread = 0;
            maxSpread = 0;
            force = 500;
            maxDistance = 200;
            spreadBloomValue = 1;
            useSmartCollision = true;
            bulletRadius = 1;
            if (Left)
            {
                muzzleName = "MuzzleLeft";
            }
            else
            {
                muzzleName = "MuzzleRight";
            }
        }
        public override void ModifyBullet(BulletAttack bulletAttack)
        {
            base.ModifyBullet(bulletAttack);
            bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
        }

        public override void PlayFireAnimation()
        {
            if (Left)
            {
                PlayAnimation("Gesture Additive, Left", "FirePistol, Left");
            }
            else
            {
                PlayAnimation("Gesture Additive, Right", "FirePistol, Right");
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (Left)
            {
                var shurikenBehavior = characterBody.GetComponent<PrimarySkillShurikenBehavior>();
                if (shurikenBehavior)
                {
                    shurikenBehavior.OnSkillActivated(skillLocator.primary);
                }
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

        public override EntityState InstantiateNextState()
        {
            if (Left)
            {
                return new Reload();
            }
            return new FireRevolver(true);
        }

    }
}