using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ZetaItemBalance
{
    public class SharedHooks
    {
        public delegate void Handle_GlobalHitEvent(CharacterBody victim, CharacterBody attacker, DamageInfo damageInfo);
        public static Handle_GlobalHitEvent Handle_GlobalHitEvent_Actions;

        public delegate void Handle_CatalogSetItemDefs(ItemDef[] newItemDefs);
        public static Handle_CatalogSetItemDefs Handle_CatalogSetItemDefs_Actions;

        public delegate void ModifyDamageRecieved(HealthComponent self, DamageInfo damageInfo, ref float damage);
        public static ModifyDamageRecieved ModifyDamageRecieved_Actions;

        public static void Setup()
        {
            if (Handle_GlobalHitEvent_Actions != null)
            {
                On.RoR2.GlobalEventManager.ProcessHitEnemy += GlobalEventManager_ProcessHitEnemy;
            }
            if (Handle_CatalogSetItemDefs_Actions != null)
            {
                On.RoR2.ItemCatalog.SetItemDefs += ItemCatalog_SetItemDefs;
            }
            if (ModifyDamageRecieved_Actions != null)
            {
                IL.RoR2.HealthComponent.TakeDamageProcess += new ILContext.Manipulator(IL_TakeDamageProcess);
            }
        }

        internal static void GlobalEventManager_ProcessHitEnemy(
            On.RoR2.GlobalEventManager.orig_ProcessHitEnemy orig,
            GlobalEventManager self,
            DamageInfo damageInfo,
            GameObject victim
        )
        {
            orig(self, damageInfo, victim);
            if (!NetworkServer.active)
            {
                return;
            }
            if (victim && damageInfo.attacker)
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                CharacterBody victimBody = victim.GetComponent<CharacterBody>();
                if (attackerBody && victimBody)
                {
                    Handle_GlobalHitEvent_Actions.Invoke(victimBody, attackerBody, damageInfo);
                }
            }
        }

        internal static void ItemCatalog_SetItemDefs(On.RoR2.ItemCatalog.orig_SetItemDefs orig, ItemDef[] newItemDefs)
        {
            orig(newItemDefs);
            Utils.OnItemTierCatalogInit();
            Handle_CatalogSetItemDefs_Actions.Invoke(newItemDefs);
        }

        private static void IL_TakeDamageProcess(ILContext il)
        {
            int idx = -1;
            ILCursor ilcursor = new(il);
            if (
                !ilcursor.TryGotoNext(
                    x => x.MatchLdloc(out idx),
                    x => x.MatchLdloc(out _),
                    x => x.MatchNewobj<DamageReport>()
                )
            )
            {
                MainPlugin.ModLogger.LogError("Shared Hooks Modify Damage 1 - IL Hook Failed");
                return;
            }
            ilcursor.Index = 0;
            if (!ilcursor.TryGotoNext(x => x.MatchStloc(idx)))
            {
                MainPlugin.ModLogger.LogError("Shared Hooks Modify Damage 2 - IL Hook Failed");
                return;
            }
            ilcursor.Emit(OpCodes.Ldarg_0);
            ilcursor.Emit(OpCodes.Ldarg_1);
            ilcursor.EmitDelegate<Func<float, HealthComponent, DamageInfo, float>>(
                (damage, healthComponent, damageInfo) =>
                {
                    ModifyDamageRecieved_Actions.Invoke(healthComponent, damageInfo, ref damage);
                    return damage;
                }
            );
        }
    }
}
