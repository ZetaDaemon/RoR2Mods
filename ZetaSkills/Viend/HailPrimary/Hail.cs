using System;
using BepInEx;
using EntityStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ZetaSkills.Viend.HailPrimary
{
    public class Hail
    {
        public static SkillDef mySkillDef;
        public Hail()
        {
            new CorruptHail();

            GameObject viendBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBody.prefab").WaitForCompletion();
            SkillDef commandoSkilldef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Commando/CommandoBodyFirePistol.asset").WaitForCompletion();

            FireHail.TracerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/TracerLunarWispMinigun.prefab").WaitForCompletion();
            FireHail.ImpactPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamImpact.prefab").WaitForCompletion();
            FireHail.MuzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamMuzzleflash.prefab").WaitForCompletion();

            LanguageAPI.Add("VIEND_PRIMARY_HAIL_NAME", "「Ha?il】");
            LanguageAPI.Add("VIEND_PRIMARY_HAIL_DESCRIPTION", $"Rapidly fire a short-range beam for <style=cIsDamage>{FireHail.Damage * 100}% damage</style>.");
            LanguageAPI.Add("VIEND_PRIMARY_HAIL_UPRADE_TOOLTIP", $"<style=cKeywordName>【Corruption Upgrade】</style><style=cSub>Transform into a 2000% damage long-range beam.</style>");

            mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(FireHail));
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
            mySkillDef.icon = commandoSkilldef.icon;
            mySkillDef.skillDescriptionToken = "VIEND_PRIMARY_HAIL_DESCRIPTION";
            mySkillDef.skillName = "VIEND_PRIMARY_HAIL_NAME";
            mySkillDef.skillNameToken = "VIEND_PRIMARY_HAIL_NAME";
            mySkillDef.keywordTokens = ["VIEND_PRIMARY_HAIL_UPRADE_TOOLTIP"];

            ContentAddition.AddSkillDef(mySkillDef);

            SkillLocator skillLocator = viendBodyPrefab.GetComponent<SkillLocator>();
            SkillFamily skillFamily = skillLocator.primary.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };

            CorruptionManager.AddOverride(mySkillDef, CorruptHail.mySkillDef);
        }
    }
}