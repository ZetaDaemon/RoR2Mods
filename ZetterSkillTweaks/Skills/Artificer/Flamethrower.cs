using System.Security.Permissions;
using BepInEx.Configuration;
using EntityStates;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]


#pragma warning restore CS0618 // Type or member is obsolete

namespace ZetterSkillTweaks.Skills.Artificer;

public class Flamethrower : SkillBase
{
    protected override string CONFIG_SECTION => "Flamethrower";
    private static float Distance;
    private static float MaxTicks;
    private static float BaseEntryDuration;
    private static float BaseTickDuration;
    private static float TickDamage;
    private static float ProcCoefficientPerTick;
    private static int BaseStocks;
    private static int BaseCooldown;

    // I dont think these are worth even adding to the config
    private static readonly float Radius = 3;
    private static readonly float Force = 100;
    private static readonly float recoilForce = 0;
    private static readonly float ParticleDurationScale = 1.5f;

    public SkillDef FlamethrowerSkill;

    protected override void InitConfig()
    {
        Distance = BindToConfig("Distance", 30);
        MaxTicks = BindToConfig("Max Ticks", 30);
        BaseEntryDuration = BindToConfig("Base Entry Duration", 0.4f);
        BaseTickDuration = BindToConfig("Base Tick Duration", 0.2f);
        TickDamage = BindToConfig("Tick Damage", 1.5f);
        ProcCoefficientPerTick = BindToConfig("Proc Coefficient Per Tick", 1f);
        BaseStocks = BindToConfig("BaseStocks", 5);
        BaseCooldown = BindToConfig("Base Cooldown", 1);
    }

    protected override void Setup()
    {
        R2API.ContentAddition.AddEntityState(typeof(FlamethrowerState), out bool added);

        FlamethrowerSkill = Addressables
            .LoadAssetAsync<SkillDef>(
                RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Mage.MageBodyFlamethrower_asset
            )
            .WaitForCompletion();

        FlamethrowerSkill.activationState = new SerializableEntityStateType(typeof(FlamethrowerState));
        FlamethrowerSkill.interruptPriority = InterruptPriority.Any;
        FlamethrowerSkill.baseRechargeInterval = BaseCooldown;
        FlamethrowerSkill.baseMaxStock = BaseStocks;
        FlamethrowerSkill.requiredStock = 0;
        FlamethrowerSkill.cancelSprintingOnActivation = true;
        FlamethrowerSkill.canceledFromSprinting = false;

        On.RoR2.GenericSkill.CalculateFinalRechargeInterval += GenericSkill_CalculateFinalRechargeInterval;

        On.RoR2.Skills.SkillDef.HasRequiredStockAndDelay += SkillDef_HasRequiredStockAndDelay;

        On.RoR2.GenericSkill.RunRecharge += GenericSkill_RunRecharge;
    }

    private float GenericSkill_CalculateFinalRechargeInterval(
        On.RoR2.GenericSkill.orig_CalculateFinalRechargeInterval orig,
        GenericSkill self
    )
    {
        if (self.skillDef == FlamethrowerSkill)
        {
            return orig(self) * self.skillDef.baseMaxStock / self.maxStock;
        }
        return orig(self);
    }

    private bool SkillDef_HasRequiredStockAndDelay(
        On.RoR2.Skills.SkillDef.orig_HasRequiredStockAndDelay orig,
        SkillDef self,
        GenericSkill skillSlot
    )
    {
        if (self == FlamethrowerSkill)
        {
            return skillSlot.stock >= skillSlot.maxStock;
        }
        return orig(self, skillSlot);
    }

    private void GenericSkill_RunRecharge(On.RoR2.GenericSkill.orig_RunRecharge orig, GenericSkill self, float dt)
    {
        if (self.skillDef == FlamethrowerSkill)
        {
            self.RecalculateFinalRechargeInterval();
        }
        orig(self, dt);
    }

    private class FlamethrowerState : BaseState
    {
        private readonly GameObject flamethrowerEffectPrefab;
        private readonly GameObject impactEffectPrefab;
        private static readonly string startAttackSoundString = "Play_mage_R_start";
        private static readonly string endAttackSoundString = "Play_mage_R_end";
        private static readonly int ExitFlamethrowerStateHash = Animator.StringToHash("ExitFlamethrower");
        private static readonly int FlamethrowerParamHash = Animator.StringToHash("Flamethrower.playbackRate");
        private static readonly int FlamethrowerStateHash = Animator.StringToHash("Flamethrower");
        private static readonly int PrepFlamethrowerStateHash = Animator.StringToHash("PrepFlamethrower");
        private float flamethrowerStopwatch;
        private float entryDuration;
        private float tickDuration;
        private int ticksDone;
        private bool hasBegunFlamethrower;
        private ChildLocator childLocator;
        private Transform leftFlamethrowerTransform;
        private Transform rightFlamethrowerTransform;
        private GameObject leftFlamethrowerEffectInstance;
        private GameObject rightFlamethrowerEffectInstance;

        public FlamethrowerState()
        {
            flamethrowerEffectPrefab = Addressables
                .LoadAssetAsync<GameObject>(
                    RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Mage.MageFlamethrowerEffect_prefab
                )
                .WaitForCompletion();

            impactEffectPrefab = Addressables
                .LoadAssetAsync<GameObject>(
                    RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Common.MissileExplosionVFX_prefab
                )
                .WaitForCompletion();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            ticksDone = 0;
            entryDuration = BaseEntryDuration / attackSpeedStat;
            tickDuration = BaseTickDuration / attackSpeedStat;
            Transform modelTransform = GetModelTransform();
            if ((bool)characterBody)
            {
                characterBody.SetAimTimer(entryDuration + tickDuration * (MaxTicks + 1));
            }
            if ((bool)modelTransform)
            {
                childLocator = modelTransform.GetComponent<ChildLocator>();
            }
            PlayAnimation("Gesture, Additive", PrepFlamethrowerStateHash, FlamethrowerParamHash, entryDuration);
        }

        public override void OnExit()
        {
            Util.PlaySound(endAttackSoundString, gameObject);
            PlayCrossfade("Gesture, Additive", ExitFlamethrowerStateHash, 0.1f);
            if ((bool)leftFlamethrowerTransform)
            {
                Destroy(leftFlamethrowerTransform.gameObject);
            }
            if ((bool)rightFlamethrowerTransform)
            {
                Destroy(rightFlamethrowerTransform.gameObject);
            }
            base.OnExit();
        }

        private void FireGauntlet(string muzzleString)
        {
            ticksDone += 1;
            Ray aimRay = GetAimRay();
            if (isAuthority)
            {
                BulletAttack bulletAttack = new()
                {
                    owner = gameObject,
                    weapon = gameObject,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,
                    minSpread = 0f,
                    damage = TickDamage * damageStat,
                    force = Force,
                    muzzleName = muzzleString,
                    hitEffectPrefab = impactEffectPrefab,
                    isCrit = Util.CheckRoll(critStat, characterBody.master),
                    radius = Radius,
                    falloffModel = BulletAttack.FalloffModel.None,
                    stopperMask = LayerIndex.world.mask,
                    procCoefficient = ProcCoefficientPerTick,
                    maxDistance = Distance,
                    smartCollision = true,
                    damageType = DamageType.IgniteOnHit,
                    allowTrajectoryAimAssist = false,
                };
                bulletAttack.damageType.damageSource = DamageSource.Special;
                bulletAttack.Fire();
                if ((bool)characterMotor)
                {
                    characterMotor.ApplyForce(aimRay.direction * (0f - recoilForce));
                }
            }
        }

        private float CalculateFlamethrowerParticleDuration()
        {
            return tickDuration * MaxTicks * ParticleDurationScale;
        }

        private void InstantiateFlamethowerTransforms()
        {
            if ((bool)childLocator)
            {
                Transform transform = childLocator.FindChild("MuzzleLeft");
                Transform transform2 = childLocator.FindChild("MuzzleRight");
                if ((bool)transform)
                {
                    leftFlamethrowerEffectInstance = Object.Instantiate(flamethrowerEffectPrefab, transform);
                    leftFlamethrowerTransform = leftFlamethrowerEffectInstance.transform;
                }
                if ((bool)transform2)
                {
                    rightFlamethrowerEffectInstance = Object.Instantiate(flamethrowerEffectPrefab, transform2);
                    rightFlamethrowerTransform = rightFlamethrowerEffectInstance.transform;
                }
                float particleDuration = CalculateFlamethrowerParticleDuration();
                float baseParticleDuration = BaseTickDuration * MaxTicks * ParticleDurationScale;
                if (leftFlamethrowerTransform.Find("Bone1").TryGetComponent(out AnimateShaderAlpha animateShaderAlpha))
                {
                    Object.Destroy(animateShaderAlpha);
                }
                if (rightFlamethrowerTransform.Find("Bone1").TryGetComponent(out animateShaderAlpha))
                {
                    Object.Destroy(animateShaderAlpha);
                }
                if (
                    (bool)leftFlamethrowerTransform
                    && leftFlamethrowerTransform.TryGetComponent(
                        out ScaleParticleSystemDuration scaleParticleSystemDuration
                    )
                )
                {
                    scaleParticleSystemDuration.initialDuration = baseParticleDuration;
                    scaleParticleSystemDuration.newDuration = particleDuration;
                }
                if (
                    (bool)rightFlamethrowerTransform
                    && rightFlamethrowerTransform.TryGetComponent(out scaleParticleSystemDuration)
                )
                {
                    scaleParticleSystemDuration.initialDuration = particleDuration;
                    scaleParticleSystemDuration.newDuration = baseParticleDuration;
                }
            }
        }

        private void UpdateFlamethrowerTransformDurations(float durationDelta)
        {
            if (!(bool)leftFlamethrowerTransform || !(bool)rightFlamethrowerTransform)
            {
                InstantiateFlamethowerTransforms();
                return;
            }
            if (leftFlamethrowerTransform.TryGetComponent(out DestroyOnTimer destroyOnTimer))
            {
                MainPlugin.ModLogger.LogInfo($"left {destroyOnTimer.age}");
                destroyOnTimer.age -= durationDelta;
            }
            if (rightFlamethrowerTransform.TryGetComponent(out destroyOnTimer))
            {
                MainPlugin.ModLogger.LogInfo($"right {destroyOnTimer.age}");
                destroyOnTimer.age -= durationDelta;
            }
            if (leftFlamethrowerTransform.TryGetComponent(out ScaleParticleSystemDuration scaleParticleSystemDuration))
            {
                scaleParticleSystemDuration.newDuration += durationDelta;
            }
            if (rightFlamethrowerTransform.TryGetComponent(out scaleParticleSystemDuration))
            {
                scaleParticleSystemDuration.newDuration += durationDelta;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            characterBody.isSprinting = false;
            if (fixedAge >= entryDuration && !hasBegunFlamethrower)
            {
                hasBegunFlamethrower = true;
                Util.PlaySound(startAttackSoundString, gameObject);
                flamethrowerStopwatch = 0;
                PlayAnimation(
                    "Gesture, Additive",
                    FlamethrowerStateHash,
                    FlamethrowerParamHash,
                    tickDuration * MaxTicks
                );
                InstantiateFlamethowerTransforms();
                FireGauntlet("MuzzleCenter");
            }
            if (!hasBegunFlamethrower)
            {
                return;
            }
            flamethrowerStopwatch += Time.deltaTime;
            UpdateFlamethrowerEffect();
            if (flamethrowerStopwatch <= tickDuration)
            {
                return;
            }
            if (ticksDone < MaxTicks)
            {
                flamethrowerStopwatch -= tickDuration;
                FireGauntlet("MuzzleCenter");
            }
            else if (inputBank.skill4.down && skillLocator.special.stock > 0)
            {
                flamethrowerStopwatch -= tickDuration;
                ticksDone = 0;
                tickDuration = BaseTickDuration / attackSpeedStat;
                skillLocator.special.DeductStock(1);
                characterBody.SetAimTimer(CalculateFlamethrowerParticleDuration());
                PlayAnimation(
                    "Gesture, Additive",
                    FlamethrowerStateHash,
                    FlamethrowerParamHash,
                    tickDuration * MaxTicks
                );
                UpdateFlamethrowerTransformDurations(MaxTicks * tickDuration);
                FireGauntlet("MuzzleCenter");
            }
            else if (isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        private void UpdateFlamethrowerEffect()
        {
            Ray aimRay = GetAimRay();
            Vector3 direction = aimRay.direction;
            Vector3 direction2 = aimRay.direction;
            if ((bool)leftFlamethrowerTransform)
            {
                leftFlamethrowerTransform.forward = direction;
            }
            if ((bool)rightFlamethrowerTransform)
            {
                rightFlamethrowerTransform.forward = direction2;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
