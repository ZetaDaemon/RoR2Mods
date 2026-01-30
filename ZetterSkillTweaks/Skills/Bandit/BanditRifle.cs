using HG.GeneralSerializer;
using RoR2;
using UnityEngine.AddressableAssets;

namespace ZetterSkillTweaks.Skills.Bandit;

public class BanditRifle : SkillBase
{
    protected override string CONFIG_SECTION => "Blast";

    protected override void InitConfig() { }

    protected override void Setup()
    {
        EntityStateConfiguration rifleStatePrefab = Addressables
            .LoadAssetAsync<EntityStateConfiguration>(
                RoR2BepInExPack
                    .GameAssetPaths
                    .Version_1_39_0
                    .RoR2_Base_Bandit2
                    .EntityStates_Bandit2_Weapon_Bandit2FireRifle_asset
            )
            .WaitForCompletion();
        ref SerializedField spreadBloomValue = ref rifleStatePrefab.serializedFieldsCollection.GetOrCreateField(
            "spreadBloomValue"
        );
        spreadBloomValue.fieldValue.stringValue = "0";
        ref SerializedField bulletRadius = ref rifleStatePrefab.serializedFieldsCollection.GetOrCreateField(
            "spreadBloomValue"
        );
        bulletRadius.fieldValue.stringValue = "0.5";
    }
}
