using EntityStates;
using RoR2;
using UnityEngine;

namespace ZetaSkills.Commando.BigIronsPrimary
{
    public class Reload : BaseSkillState
    {
        private const float BaseDuration = 0.7f;
        private float Duration;

        public override void OnEnter()
        {
            base.OnEnter();
            Duration = BaseDuration / attackSpeedStat;
            Util.PlaySound("Play_bandit2_R_load", base.gameObject);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > Duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}