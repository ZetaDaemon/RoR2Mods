using System;
using BepInEx;
using EntityStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ZetaSkills.Captain.BlasterPrimary
{
    public class Blaster
    {
        public Blaster()
        {
            GameObject captainBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainBody.prefab").WaitForCompletion();
            SkillDef railgunnerSkilldef = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/Railgunner/RailgunnerBodyFireSnipeHeavy.asset").WaitForCompletion();

            ChargeBlaster.chargeupVfxPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/ChargeLoaderFist.prefab").WaitForCompletion();
            ChargeBlaster.holdChargeVfxPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/SpearChargedVFX.prefab").WaitForCompletion();
            FireBlaster.LightTracerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgunLight.prefab").WaitForCompletion();
            FireBlaster.HeavyTracerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgunCryo.prefab").WaitForCompletion();
            FireBlaster.LightImpactPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/ImpactRailgunLight.prefab").WaitForCompletion();
            FireBlaster.HeavyImpactPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/ImpactRailgun.prefab").WaitForCompletion();
            FireBlaster.MuzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/Muzzleflash1.prefab").WaitForCompletion();

            LanguageAPI.Add("CAPTAIN_PRIMARY_BLASTER_NAME", "Charged Blaster");
            LanguageAPI.Add("CAPTAIN_PRIMARY_BLASTER_DESCRIPTION", $"Fire a laser blast for <style=cIsDamage>{FireBlaster.BaseDamage * 100}% damage</style>. Charging the attack increases the damage up to <style=cIsDamage>{FireBlaster.ChargeDamage * 100}%</style> and makes it pierce targets.");

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ChargeBlaster));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0f;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.cancelSprintingOnActivation = true;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            // For the skill icon, you will have to load a Sprite from your own AssetBundle
            mySkillDef.icon = railgunnerSkilldef.icon;
            mySkillDef.skillDescriptionToken = "CAPTAIN_PRIMARY_BLASTER_DESCRIPTION";
            mySkillDef.skillName = "CAPTAIN_PRIMARY_BLASTER_NAME";
            mySkillDef.skillNameToken = "CAPTAIN_PRIMARY_BLASTER_NAME";

            ContentAddition.AddSkillDef(mySkillDef);

            SkillLocator skillLocator = captainBodyPrefab.GetComponent<SkillLocator>();
            SkillFamily skillFamily = skillLocator.primary.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }
    }
}