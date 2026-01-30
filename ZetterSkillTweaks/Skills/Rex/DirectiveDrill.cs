using System;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ZetterSkillTweaks.Skills.Rex;

public class DirectiveDrill : SkillBase
{
    protected override string CONFIG_SECTION => "Directive Drill";
    private float ProcCoefficient;

    protected override void InitConfig()
    {
        ProcCoefficient = BindToConfig("ProcCoefficient", 0.75f);
    }

    protected override void Setup()
    {
        GameObject projectilePrefab = Addressables
            .LoadAssetAsync<GameObject>(
                RoR2BepInExPack.GameAssetPaths.Version_1_39_0.RoR2_Base_Treebot.TreebotMortarRain_prefab
            )
            .WaitForCompletion();
        if (projectilePrefab.TryGetComponent(out ProjectileDotZone dotZone))
        {
            dotZone.overlapProcCoefficient = ProcCoefficient;
        }
    }
}
