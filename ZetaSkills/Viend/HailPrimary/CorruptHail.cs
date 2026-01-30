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
    public class CorruptHail
    {
        public static SkillDef mySkillDef;
        public CorruptHail()
        {
            SkillDef commandoSkilldef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Commando/CommandoBodyFireFMJ.asset").WaitForCompletion();

            FireCorruptHail.TracerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamTracer.prefab").WaitForCompletion();
            FireCorruptHail.ImpactPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamImpact.prefab").WaitForCompletion();
            FireCorruptHail.MuzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBeamMuzzleflash.prefab").WaitForCompletion();

            LanguageAPI.Add("VIEND_PRIMARY_CORRUPTHAIL_NAME", "「Ha?il】");

            mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(FireCorruptHail));
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
            mySkillDef.skillName = "VIEND_PRIMARY_CORRUPTHAIL_NAME";
            mySkillDef.skillNameToken = "VIEND_PRIMARY_CORRUPTHAIL_NAME";

            ContentAddition.AddSkillDef(mySkillDef);
        }
    }
}