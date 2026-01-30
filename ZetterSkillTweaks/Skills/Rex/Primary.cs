using System;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ZetterSkillTweaks.Skills.Rex;

public class Primary : SkillBase
{
    protected override string CONFIG_SECTION => "Directive Inject";
    private float ProcCoefficient;

    protected override void InitConfig()
    {
        ProcCoefficient = BindToConfig("ProcCoefficient", 1);
    }

    protected override void Setup()
    {
        GameObject projectilePrefab = Addressables
            .LoadAssetAsync<GameObject>(
                RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Treebot.SyringeProjectile_prefab
            )
            .WaitForCompletion();
        if (projectilePrefab.TryGetComponent(out ProjectileController controller))
        {
            controller.procCoefficient = ProcCoefficient;
        }
    }
}
