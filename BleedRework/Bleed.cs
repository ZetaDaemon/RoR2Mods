using System.Collections.Generic;
using System.Security.Permissions;
using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace BleedRework
{
    public class Bleed
    {
        internal static readonly float TritipBleedDamage_Default = 0.2f;
        internal static readonly float SpleenBleedDamage_Default = 0.4f;
        internal static readonly float SpleenExplosionDamage_Default = 1f;
        internal static readonly float ThornBleedDamage_Default = 0.2f;
        static readonly float BleedDuration_Default = 3f;
        static readonly float BleedTickInterval_Default = 0.333f;
        static readonly float SuperBleedDuration_Default = 6f;
        static readonly float SuperBleedFactor_Default = 6f;
        static readonly float SuperBleedTickInterval_Default = 0.2f;

        internal static float TritipBleedDamage = TritipBleedDamage_Default;
        internal static float SpleenBleedDamage = SpleenBleedDamage_Default;
        internal static float SpleenExplosionDamage = SpleenExplosionDamage_Default;
        internal static float ThornBleedDamage = ThornBleedDamage_Default;
        static readonly float BleedDuration = BleedDuration_Default;
        static readonly float BleedTickInterval = BleedTickInterval_Default;
        static readonly float SuperBleedDuration = SuperBleedDuration_Default;
        static readonly float SuperBleedFactor = SuperBleedFactor_Default;
        static readonly float SuperBleedTickInterval = SuperBleedTickInterval_Default;

        static Bleed()
        {
            On.RoR2.DotController.InitDotCatalog += DotController_InitDotCatalog;

            On.RoR2.GlobalEventManager.ProcessHitEnemy += GlobalEventManager_ProcessHitEnemy;
            IL.RoR2.GlobalEventManager.ProcessHitEnemy += new ILContext.Manipulator(IL_ProcessHitEnemy);

            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            IL.RoR2.GlobalEventManager.OnCharacterDeath += new ILContext.Manipulator(IL_OnCharacterDeath);

            On.RoR2.CharacterBody.TriggerEnemyDebuffs += CharacterBody_TriggerEnemyDebuffs;

            IL.RoR2.DotController.AddDot_GameObject_float_HurtBox_DotIndex_float_Nullable1_Nullable1_Nullable1 +=
                new ILContext.Manipulator(IL_AddDot);

            LanguageAPI.Add(
                "ITEM_BLEEDONHIT_DESC",
                $"Attacks <style=cIsDamage>bleed</style> enemies for <style=cIsDamage>{TritipBleedDamage * 100}%</style> "
                    + $"<style=cStack>(+{TritipBleedDamage * 100}% per stack)</style> TOTAL damage."
            );
            LanguageAPI.Add("ITEM_BLEEDONHIT_PICKUP", $"Deal +{TritipBleedDamage}% damage as bleed over time.");

            LanguageAPI.Add(
                "ITEM_BLEEDONHITANDEXPLODE_DESC",
                $"Critical strikes <style=cIsDamage>bleed</style> enemies for <style=cIsDamage>{SpleenBleedDamage * 100}%</style> "
                    + $"<style=cStack>(+{SpleenBleedDamage * 100}% per stack)</style> TOTAL damage.\n"
                    + $"<style=cIsDamage>Bleeding</style> enemies <style=cIsDamage>explode</style> on death for <style=cIsDamage>"
                    + $"{SpleenExplosionDamage * 100}%</style> <style=cStack>(+{SpleenExplosionDamage * 100}% per stack)</style> "
                    + "<style=cIsDamage>bleed</style> dot damage"
            );
            LanguageAPI.Add(
                "ITEM_BLEEDONHITANDEXPLODE_PICKUP",
                "Critical strikes bleed enemies. Bleeding enemies now explode on death."
            );

            LanguageAPI.Add(
                "ITEM_TRIGGERENEMYDEBUFFS_DESC",
                $"Attacks <style=cIsDamage>bleed</style> enemies for <style=cIsDamage>{ThornBleedDamage * 100}%</style> TOTAL damage.\n"
                    + $"On killing an enemy, <style=cIsDamage>transfer 33%</style> of every debuff stack to <style=cIsDamage>1 enemy</style> "
                    + $"<style=cStack>(+1 per stack)</style> within <style=cIsUtility>20m</style> <style=cStack>(+5m per stack)</style>."
            );
            LanguageAPI.Add(
                "ITEM_TRIGGERENEMYDEBUFFS_PICKUP",
                $"Deal +{TritipBleedDamage}% damage as bleed over time."
            );
        }

        internal static void DotController_InitDotCatalog(On.RoR2.DotController.orig_InitDotCatalog orig)
        {
            orig();
            DotController.DotDef bleed = DotController.dotDefs[(int)DotController.DotIndex.Bleed];
            bleed.damageCoefficient = 1;
            bleed.interval = BleedTickInterval;
            bleed.resetTimerOnAdd = false;

            DotController.DotDef superBleed = DotController.dotDefs[(int)DotController.DotIndex.SuperBleed];
            superBleed.damageCoefficient = 1;
            superBleed.interval = SuperBleedTickInterval;
            superBleed.resetTimerOnAdd = false;
        }

        static void AttemptBleed(
            DamageInfo damageInfo,
            GameObject victim,
            CharacterBody characterBody,
            Inventory inventory
        )
        {
            if (damageInfo.procChainMask.HasProc(ProcType.BleedOnHit))
            {
                return;
            }
            int tritipCount = inventory.GetItemCountEffective(RoR2Content.Items.BleedOnHit);
            int spleenCount = inventory.GetItemCountEffective(RoR2Content.Items.BleedOnHitAndExplode);

            float bleedFactor = 0;
            bool isBleedOnHitDamage = (damageInfo.damageType & DamageType.BleedOnHit) != 0;
            if (isBleedOnHitDamage)
            {
                bleedFactor += 0.2f;
            }
            if (tritipCount > 0)
            {
                bleedFactor += TritipBleedDamage * tritipCount;
            }
            if (damageInfo.crit && spleenCount > 0)
            {
                bleedFactor += SpleenBleedDamage * spleenCount;
            }
            if (inventory.GetItemCountEffective(DLC2Content.Items.TriggerEnemyDebuffs) > 0)
            {
                bleedFactor += ThornBleedDamage;
            }
            if (
                damageInfo.inflictor != null
                && damageInfo.inflictor.TryGetComponent(out BoomerangProjectile sawmerangComponent)
                && !damageInfo.procChainMask.HasProc(ProcType.BleedOnHit)
                && !sawmerangComponent.isStunAndPierce
            )
            {
                if (inventory.GetEquipmentIndex() == RoR2Content.Equipment.Saw.equipmentIndex || isBleedOnHitDamage)
                {
                    damageInfo.procChainMask.AddProc(ProcType.BleedOnHit);
                    bleedFactor += 0.2f;
                }
            }
            if (bleedFactor == 0)
            {
                return;
            }
            float bleedDamage =
                damageInfo.damage / characterBody.damage * bleedFactor * BleedTickInterval / BleedDuration;
            if (damageInfo.crit)
            {
                bleedDamage *= characterBody.critMultiplier;
            }
            DotController.InflictDot(
                victim,
                damageInfo.attacker,
                damageInfo.inflictedHurtbox,
                DotController.DotIndex.Bleed,
                BleedDuration,
                bleedDamage
            );
        }

        static void AttemptSuperBleed(
            DamageInfo damageInfo,
            GameObject victim,
            CharacterBody characterBody,
            Inventory inventory
        )
        {
            if (!damageInfo.crit)
            {
                return;
            }
            if ((damageInfo.damageType & DamageType.SuperBleedOnCrit) == 0)
            {
                return;
            }
            float superBleedDamage =
                damageInfo.damage
                / characterBody.damage
                * SuperBleedFactor
                * SuperBleedTickInterval
                / SuperBleedDuration
                * characterBody.critMultiplier;
            DotController.InflictDot(
                victim,
                damageInfo.attacker,
                damageInfo.inflictedHurtbox,
                DotController.DotIndex.SuperBleed,
                SuperBleedDuration,
                superBleedDamage
            );
        }

        internal static void GlobalEventManager_ProcessHitEnemy(
            On.RoR2.GlobalEventManager.orig_ProcessHitEnemy orig,
            GlobalEventManager self,
            DamageInfo damageInfo,
            GameObject victim
        )
        {
            orig(self, damageInfo, victim);
            CharacterBody? characterBody = damageInfo.attacker
                ? damageInfo.attacker.GetComponent<CharacterBody>()
                : null;
            if (characterBody is null)
            {
                return;
            }
            CharacterMaster master = characterBody.master;
            if (master is null)
            {
                return;
            }
            Inventory inventory = master.inventory;

            AttemptBleed(damageInfo, victim, characterBody, inventory);
            AttemptSuperBleed(damageInfo, victim, characterBody, inventory);
        }

        static float GetDotTotalDamage(
            DotController controller,
            List<DotController.DotIndex> indexes,
            bool remainingDamage = true
        )
        {
            float totalDamage = 0;
            foreach (DotController.DotStack stack in controller.dotStackList)
            {
                if (!indexes.Contains(stack.dotIndex))
                {
                    continue;
                }
                if (remainingDamage)
                {
                    totalDamage += stack.damage * stack.timer;
                }
                else
                {
                    totalDamage += stack.damage * stack.totalDuration;
                }
            }
            return totalDamage;
        }

        static float GetDotTotalDamagePerTick(DotController controller, List<DotController.DotIndex> indexes)
        {
            float totalDamage = 0;
            foreach (DotController.DotStack stack in controller.dotStackList)
            {
                if (!indexes.Contains(stack.dotIndex))
                {
                    continue;
                }
                totalDamage += stack.damage;
            }
            return totalDamage;
        }

        static void DoDeathExplosion(GlobalEventManager self, DamageReport damageReport)
        {
            CharacterBody victimBody = damageReport.victimBody;
            if (!(victimBody.HasBuff(RoR2Content.Buffs.Bleeding) || victimBody.HasBuff(RoR2Content.Buffs.SuperBleed)))
            {
                return;
            }
            CharacterBody? attackerBody = damageReport.attacker
                ? damageReport.attacker.GetComponent<CharacterBody>()
                : null;
            if (attackerBody is null)
            {
                return;
            }
            CharacterMaster attackerMaster = attackerBody.master;
            Inventory attackerInventory = attackerMaster.inventory;
            int spleenCount = attackerInventory.GetItemCountEffective(RoR2Content.Items.BleedOnHitAndExplode);
            if (spleenCount <= 0)
            {
                return;
            }
            DotController dotController = DotController.FindDotController(victimBody.gameObject);
            float totalBleedDamage = GetDotTotalDamage(
                dotController,
                [DotController.DotIndex.Bleed, DotController.DotIndex.SuperBleed]
            );
            if (totalBleedDamage <= 0)
            {
                return;
            }

            Util.PlaySound("Play_bleedOnCritAndExplode_explode", victimBody.gameObject);
            if (!(bool)victimBody.gameObject.transform)
            {
                return;
            }
            Vector3 spawnPosition = victimBody.gameObject.transform.position;
            GameObject explosionObj = UnityEngine.Object.Instantiate(
                GlobalEventManager.CommonAssets.bleedOnHitAndExplodeBlastEffect,
                spawnPosition,
                Quaternion.identity
            );
            DelayBlast delayBlast = explosionObj.GetComponent<DelayBlast>();
            delayBlast.position = spawnPosition;
            delayBlast.baseDamage = totalBleedDamage / BleedTickInterval * SpleenExplosionDamage * spleenCount;
            delayBlast.baseForce = 0f;
            delayBlast.radius = 16f;
            delayBlast.attacker = damageReport.attacker;
            delayBlast.inflictor = null;
            delayBlast.crit = Util.CheckRoll(attackerBody.crit, attackerMaster);
            delayBlast.maxTimer = 0f;
            delayBlast.damageColorIndex = DamageColorIndex.Item;
            delayBlast.falloffModel = BlastAttack.FalloffModel.SweetSpot;
            delayBlast.procChainMask.AddProc(ProcType.BleedOnHit);
            explosionObj.GetComponent<TeamFilter>().teamIndex = damageReport.attackerTeamIndex;
            NetworkServer.Spawn(explosionObj);
        }

        internal static void GlobalEventManager_OnCharacterDeath(
            On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig,
            GlobalEventManager self,
            DamageReport damageReport
        )
        {
            DoDeathExplosion(self, damageReport);
            orig(self, damageReport);
        }

        static void CreateVineOrbChain(
            GameObject sourceGameObject,
            HurtBox targetHurtbox,
            List<VineOrb.SplitDebuffInformation> debuffInfoList
        )
        {
            VineOrb vineOrb = new VineOrb();
            vineOrb.origin = sourceGameObject.transform.position;
            vineOrb.target = targetHurtbox;
            vineOrb.splitDebuffInformation = debuffInfoList;
            RoR2.Orbs.OrbManager.instance.AddOrb(vineOrb);
        }

        internal static void CharacterBody_TriggerEnemyDebuffs(
            On.RoR2.CharacterBody.orig_TriggerEnemyDebuffs orig,
            CharacterBody self,
            DamageReport damageReport
        )
        {
            int itemCountEffective = self.inventory.GetItemCountEffective(DLC2Content.Items.TriggerEnemyDebuffs);
            if (itemCountEffective == 0)
            {
                return;
            }
            List<VineOrb.SplitDebuffInformation> list = [];
            DotController dotController = DotController.FindDotController(damageReport.victimBody.gameObject);
            BuffIndex[] debuffAndDotsIndicesExcludingNoxiousThorns =
                BuffCatalog.debuffAndDotsIndicesExcludingNoxiousThorns;
            foreach (BuffIndex buffIndex in debuffAndDotsIndicesExcludingNoxiousThorns)
            {
                BuffDef buffDef = BuffCatalog.GetBuffDef(buffIndex);
                int buffCount = damageReport.victimBody.GetBuffCount(buffDef);
                if (buffCount > 0)
                {
                    int count = Mathf.CeilToInt(buffCount * 0.33f);
                    bool isTimed = false;
                    float totalDuration = 0f;
                    float damageMultiplier = 0;
                    if (buffDef.isDOT && dotController != null)
                    {
                        DotController.DotIndex dotDefIndex = DotController.GetDotDefIndex(buffDef);
                        DotController.GetDotDef(dotDefIndex);
                        isTimed = dotController.GetDotStackTotalDurationForIndex(dotDefIndex, out totalDuration);
                        damageMultiplier =
                            GetDotTotalDamagePerTick(dotController, [dotDefIndex])
                            / buffCount
                            / damageReport.attackerBody.damage;
                    }
                    else if (buffDef.isDebuff)
                    {
                        isTimed = damageReport.victimBody.GetTimedBuffTotalDurationForIndex(
                            buffIndex,
                            out totalDuration
                        );
                    }
                    VineOrb.SplitDebuffInformation splitDebuffInformation = default;
                    splitDebuffInformation.attacker = self.gameObject;
                    splitDebuffInformation.attackerMaster = self.master;
                    splitDebuffInformation.index = buffIndex;
                    splitDebuffInformation.isTimed = isTimed;
                    splitDebuffInformation.duration = totalDuration;
                    splitDebuffInformation.count = count;
                    splitDebuffInformation.dotDamageMultiplier = damageMultiplier;
                    VineOrb.SplitDebuffInformation item = splitDebuffInformation;
                    list.Add(item);
                }
            }
            if (list.Count == 0)
            {
                return;
            }
            SphereSearch sphereSearch = new SphereSearch();
            List<HurtBox> list2 = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
            sphereSearch.mask = LayerIndex.entityPrecise.mask;
            sphereSearch.origin = damageReport.victimBody.gameObject.transform.position;
            sphereSearch.radius = 20f + 5f * (float)(itemCountEffective - 1);
            sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
            sphereSearch.RefreshCandidates();
            sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(self.teamComponent.teamIndex));
            sphereSearch.OrderCandidatesByDistance();
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            sphereSearch.GetHurtBoxes(list2);
            sphereSearch.ClearCandidates();
            int num = itemCountEffective;
            for (int j = 0; j < list2.Count; j++)
            {
                HurtBox hurtBox = list2[j];
                CharacterBody body = hurtBox.healthComponent.body;
                if (
                    (bool)hurtBox
                    && (bool)hurtBox.healthComponent
                    && hurtBox.healthComponent.alive
                    && body != damageReport.victimBody
                    && body != self
                )
                {
                    CreateVineOrbChain(damageReport.victimBody.gameObject, hurtBox, list);
                    num--;
                    if (num == 0)
                    {
                        return;
                    }
                }
            }
            CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(list2);
        }

        private static void IL_ProcessHitEnemy(ILContext il)
        {
            ILCursor ilcursor = new(il);
            if (!ilcursor.TryGotoNext(x => x.MatchBrfalse(out _), x => x.MatchLdloc(39)))
            {
                MainPlugin.ModLogger.LogError("Bleed 1 IL Hook Failed");
                return;
            }
            ilcursor.Emit(OpCodes.Pop);
            ilcursor.Emit(OpCodes.Ldc_I4_0);

            if (!ilcursor.TryGotoNext(x => x.MatchLdloc(123), x => x.MatchLdnull()))
            {
                MainPlugin.ModLogger.LogError("Bleed 2 IL Hook Failed");
                return;
            }
            ilcursor.Index += 3;
            ilcursor.Emit(OpCodes.Pop);
            ilcursor.Emit(OpCodes.Ldc_I4_0);

            if (!ilcursor.TryGotoNext(x => x.MatchLdcI4(134217728)))
            {
                MainPlugin.ModLogger.LogError("Bleed 3 IL Hook Failed");
                return;
            }
            ilcursor.Index += 4;
            ilcursor.Emit(OpCodes.Pop);
            ilcursor.Emit(OpCodes.Ldc_I4_0);
        }

        private static void IL_OnCharacterDeath(ILContext il)
        {
            ILCursor ilcursor = new(il);
            if (!ilcursor.TryGotoNext(x => x.MatchLdloc(57), x => x.MatchLdcI4(0), x => x.MatchBle(out _)))
            {
                MainPlugin.ModLogger.LogError("Spleen IL Hook Failed");
                return;
            }
            ilcursor.Index += 1;
            ilcursor.Emit(OpCodes.Pop);
            ilcursor.Emit(OpCodes.Ldc_I4_0);
        }

        private static void IL_AddDot(ILContext il)
        {
            ILCursor ilcursor = new(il);
            if (!ilcursor.TryGotoNext(x => x.MatchLdarg(4), x => x.MatchSwitch(out _)))
            {
                MainPlugin.ModLogger.LogError("Bleed IL Hook Failed");
                return;
            }
            ilcursor.Index += 2;
            ILLabel label = ilcursor.MarkLabel();
            ilcursor.Index -= 1;
            ilcursor.Emit(OpCodes.Ldc_I4_0);
            ilcursor.Emit(OpCodes.Beq_S, label);
            ilcursor.Emit(OpCodes.Ldarg, 4);
            ilcursor.Emit(OpCodes.Ldc_I4_6);
            ilcursor.Emit(OpCodes.Beq_S, label);
            ilcursor.Emit(OpCodes.Ldarg, 4);
        }
    }
}
