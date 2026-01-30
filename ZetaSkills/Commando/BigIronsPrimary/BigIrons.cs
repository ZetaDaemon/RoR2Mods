using System;
using BepInEx;
using EntityStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ZetaSkills.Commando.BigIronsPrimary
{
    public class BigIrons
    {
        public BigIrons()
        {
            GameObject commandoBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoBody.prefab").WaitForCompletion();
            SkillDef banditSkilldef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Bandit2/ResetRevolver.asset").WaitForCompletion();

            FireRevolver.TracerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/TracerBanditPistol.prefab").WaitForCompletion();
            FireRevolver.ImpactPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/HitsparkBandit2Pistol.prefab").WaitForCompletion();
            FireRevolver.MuzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/MuzzleflashBandit2.prefab").WaitForCompletion();

            LanguageAPI.Add("COMMANDO_PRIMARY_BIGIRONS_NAME", "Big Irons");
            LanguageAPI.Add("COMMANDO_PRIMARY_BIGIRONS_DESCRIPTION", $"Fire dual revolvers for <style=cIsDamage>{FireRevolver.Damage * 100}% damage</style>, reloading after shooting both revolvers.");

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(FireRevolver));
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
            mySkillDef.icon = banditSkilldef.icon;
            mySkillDef.skillDescriptionToken = "COMMANDO_PRIMARY_BIGIRONS_DESCRIPTION";
            mySkillDef.skillName = "COMMANDO_PRIMARY_BIGIRONS_NAME";
            mySkillDef.skillNameToken = "COMMANDO_PRIMARY_BIGIRONS_NAME";

            ContentAddition.AddSkillDef(mySkillDef);

            SkillLocator skillLocator = commandoBodyPrefab.GetComponent<SkillLocator>();
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