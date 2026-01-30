using EntityStates;
using RoR2;
using UnityEngine;

namespace ZetaSkills.Captain.BlasterPrimary
{
    public class ChargeBlaster : BaseSkillState
    {
        private const float baseChargeDuration = 1.2f;
        private const string muzzleName = "MuzzleGun";
        private const string playChargeSoundString = "Play_captain_m1_shotgun_charge_loop";
        private const string stopChargeSoundString = "Stop_captain_m1_shotgun_charge_loop";
        private const string enterSoundString = "Play_captain_m1_chargeStart";
        public static GameObject chargeupVfxPrefab;
        public static GameObject holdChargeVfxPrefab;

        private float chargeDuration;
        private bool released;
        private Transform muzzleTransform;
        private GameObject chargeupVfxGameObject;
        private GameObject holdChargeVfxGameObject;
        private uint enterSoundID;

        public override void OnEnter()
        {
            base.OnEnter();
            chargeDuration = baseChargeDuration / attackSpeedStat;
            PlayCrossfade("Gesture, Override", "ChargeCaptainShotgun", "ChargeCaptainShotgun.playbackRate", chargeDuration, 0.1f);
            PlayCrossfade("Gesture, Additive", "ChargeCaptainShotgun", "ChargeCaptainShotgun.playbackRate", chargeDuration, 0.1f);
            muzzleTransform = FindModelChild(muzzleName);
            if ((bool)muzzleTransform)
            {
                chargeupVfxGameObject = Object.Instantiate(chargeupVfxPrefab, muzzleTransform);
                chargeupVfxGameObject.GetComponent<ScaleParticleSystemDuration>().newDuration = chargeDuration;
            }
            enterSoundID = Util.PlayAttackSpeedSound(enterSoundString, base.gameObject, attackSpeedStat);
            Util.PlaySound(playChargeSoundString, base.gameObject);
        }

        public override void OnExit()
        {
            if ((bool)chargeupVfxGameObject)
            {
                EntityState.Destroy(chargeupVfxGameObject);
                chargeupVfxGameObject = null;
            }
            if ((bool)holdChargeVfxGameObject)
            {
                EntityState.Destroy(holdChargeVfxGameObject);
                holdChargeVfxGameObject = null;
            }
            AkSoundEngine.StopPlayingID(enterSoundID);
            Util.PlaySound(stopChargeSoundString, base.gameObject);
            base.OnExit();
        }
        public override void Update()
        {
            base.Update();
            base.characterBody.SetSpreadBloom(1f - base.age / chargeDuration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.characterBody.SetAimTimer(1f);
            float charge = base.fixedAge / chargeDuration;
            charge = Mathf.Clamp(charge, 0, 1);
            if (charge >= 1)
            {
                if ((bool)chargeupVfxGameObject)
                {
                    EntityState.Destroy(chargeupVfxGameObject);
                    chargeupVfxGameObject = null;
                }
                if (!holdChargeVfxGameObject && (bool)muzzleTransform)
                {
                    holdChargeVfxGameObject = Object.Instantiate(holdChargeVfxPrefab, muzzleTransform);
                }
            }
            if (base.isAuthority)
            {
                if (!released && (!base.inputBank || !base.inputBank.skill1.down))
                {
                    released = true;
                }
                if (released)
                {
                    outer.SetNextState(new FireBlaster(charge));
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}