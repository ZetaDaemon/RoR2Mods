using EntityStates;
using EntityStates.Bandit2.Weapon;
using RoR2;
using RoR2.Skills;
using RoR2.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ZetterSkillTweaks.Skills.Bandit;

public class Desperado : SkillBase
{
    protected override string CONFIG_SECTION => "Desperado";
    public static int BaseMaxStocks;
    private static float BasePrepDuration = 0.5f;
    private static float BaseFireDuration = 0.2f;
    private static float TargetMaxFireDuration = 0.1f;
    private static float DamageCoefficient = 3;
    private static float DamagePerStock = 0.05f;
    private static readonly float baseRecoilAmplitude = 1;
    private static readonly float bulletRadius = 1;
    private static readonly float trajectoryAimAssistMultiplier = 0.75f;

    // Stupid maths but it automatically works out how much to
    // reduce the duration per stock so it reaches the desired amount
    private static readonly float fireDurationReductionPerStock =
        (1f - BaseFireDuration / TargetMaxFireDuration) / (1f - BaseMaxStocks);

    protected override void InitConfig()
    {
        BaseMaxStocks = BindToConfig("Base Max Stocks", 6);
        BasePrepDuration = BindToConfig("Base Prep Duration", 0.5f);
        BaseFireDuration = BindToConfig("Base Fire Duration", 0.2f);
        TargetMaxFireDuration = BindToConfig(
            "Max Fire Duration",
            0.1f,
            "The fire duration if at the base max stocks, will be lower with more stocks."
        );
        DamageCoefficient = BindToConfig("Damage Coefficient", 3f);
        DamagePerStock = BindToConfig("Damage Per Stock", 0.05f);
    }

    protected override void Setup()
    {
        SkillDef desperadoSkillDef = Addressables
            .LoadAssetAsync<SkillDef>(
                RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Bandit2.SkullRevolver_asset
            )
            .WaitForCompletion();
        desperadoSkillDef.baseMaxStock = BaseMaxStocks;
        desperadoSkillDef.stockToConsume = 0;
        desperadoSkillDef.baseRechargeInterval = 2;
        desperadoSkillDef.fullRestockOnAssign = false;
        desperadoSkillDef.beginSkillCooldownOnSkillEnd = true;
        desperadoSkillDef.canceledFromSprinting = false;
        desperadoSkillDef.activationState = new SerializableEntityStateType(typeof(SkullRevolver));
        desperadoSkillDef.interruptPriority = InterruptPriority.Any;
    }

    public class SkullRevolver : BaseState
    {
        public virtual string ExitAnimationStateName => "BufferEmpty";
        private readonly string enterSoundString = "Play_bandit2_R_load";
        private readonly string attackSoundString = "Play_bandit2_R_fire";
        private static readonly int MainToSideStateHash = Animator.StringToHash("MainToSide");
        private static readonly int MainToSideParamHash = Animator.StringToHash("MainToSide.playbackRate");
        private static readonly int FireSideWeaponStateHash = Animator.StringToHash("FireSideWeapon");
        private static readonly int FireSideWeaponParamHash = Animator.StringToHash("FireSideWeapon.playbackRate");

        private GameObject prepCrosshairOverridePrefab;
        private readonly GameObject effectPrefab;
        private readonly GameObject hitEffectPrefab;
        private readonly GameObject tracerEffectPrefab;

        private float prepDuration;
        private float fireDuration;
        private float fireAgeOffset;
        private int storedStocks;
        private float recoilAmplitude;
        private Animator animator;
        private int bodySideWeaponLayerIndex;
        private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

        public SkullRevolver()
        {
            prepCrosshairOverridePrefab = Addressables
                .LoadAssetAsync<GameObject>(
                    RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Bandit2.Bandit2CrosshairPrepRevolver_prefab
                )
                .WaitForCompletion();

            effectPrefab = Addressables
                .LoadAssetAsync<GameObject>(
                    RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Bandit2.MuzzleflashBandit2_prefab
                )
                .WaitForCompletion();
            hitEffectPrefab = Addressables
                .LoadAssetAsync<GameObject>(
                    RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Bandit2.HitsparkBandit2Pistol_prefab
                )
                .WaitForCompletion();
            tracerEffectPrefab = Addressables
                .LoadAssetAsync<GameObject>(
                    RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Bandit2.TracerBanditPistol_prefab
                )
                .WaitForCompletion();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            fireAgeOffset = 0;
            storedStocks = skillLocator.special.stock;
            recoilAmplitude = baseRecoilAmplitude / (0.95f + 0.05f * storedStocks);
            animator = GetModelAnimator();
            prepDuration = BasePrepDuration / attackSpeedStat;
            fireDuration =
                BaseFireDuration
                / attackSpeedStat
                / (1 - fireDurationReductionPerStock + fireDurationReductionPerStock * storedStocks);
            if ((bool)animator)
            {
                bodySideWeaponLayerIndex = animator.GetLayerIndex("Body, SideWeapon");
                animator.SetLayerWeight(bodySideWeaponLayerIndex, 1f);
            }
            if ((bool)prepCrosshairOverridePrefab)
            {
                crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(
                    characterBody,
                    prepCrosshairOverridePrefab,
                    CrosshairUtils.OverridePriority.Skill
                );
            }
            base.characterBody.SetAimTimer(3f);
            PlayAnimation("Gesture, Additive", MainToSideStateHash, MainToSideParamHash, prepDuration);
            Util.PlaySound(enterSoundString, gameObject);
        }

        private bool FireRevolver()
        {
            if (fixedAge - fireAgeOffset < fireDuration)
            {
                return false;
            }
            if (skillLocator.special.stock == 0)
            {
                return true;
            }
            skillLocator.special.DeductStock(1);
            fireAgeOffset = fixedAge;

            AddRecoil(-3f * recoilAmplitude, -4f * recoilAmplitude, -0.5f * recoilAmplitude, 0.5f * recoilAmplitude);
            Ray aimRay = GetAimRay();
            StartAimMode(aimRay);
            string muzzleName = "MuzzlePistol";
            Util.PlaySound(attackSoundString, gameObject);
            PlayAnimation("Gesture, Additive", FireSideWeaponStateHash, FireSideWeaponParamHash, fireDuration);
            if ((bool)effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(effectPrefab, gameObject, muzzleName, transmit: false);
            }
            if (!isAuthority)
            {
                return false;
            }
            BulletAttack bulletAttack = new()
            {
                owner = gameObject,
                weapon = gameObject,
                origin = aimRay.origin,
                aimVector = aimRay.direction,
                minSpread = 0,
                maxSpread = 0,
                bulletCount = 1u,
                damage = DamageCoefficient * damageStat * (1 - DamagePerStock + storedStocks * DamagePerStock),
                force = 1500,
                falloffModel = BulletAttack.FalloffModel.None,
                tracerEffectPrefab = tracerEffectPrefab,
                muzzleName = muzzleName,
                hitEffectPrefab = hitEffectPrefab,
                isCrit = RollCrit(),
                HitEffectNormal = false,
                radius = bulletRadius,
                damageType = DamageTypeCombo.GenericSpecial,
                smartCollision = true,
                trajectoryAimAssistMultiplier = trajectoryAimAssistMultiplier,
            };
            //ModifyBullet(bulletAttack);
            bulletAttack.Fire();
            return false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            characterBody.isSprinting = false;
            if (fixedAge < prepDuration)
            {
                return;
            }
            if (fireAgeOffset == 0)
            {
                fireAgeOffset = fixedAge - fireDuration;
            }
            if (FireRevolver())
            {
                outer.SetNextState(new ExitSidearmRevolver());
            }
        }

        public override void OnExit()
        {
            if ((bool)animator)
            {
                animator.SetLayerWeight(bodySideWeaponLayerIndex, 0f);
            }
            PlayAnimation("Gesture, Additive", ExitAnimationStateName);
            crosshairOverrideRequest?.Dispose();
            Transform transform = FindModelChild("SpinningPistolFX");
            if ((bool)transform)
            {
                transform.gameObject.SetActive(value: false);
            }
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
