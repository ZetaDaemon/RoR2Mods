using System.Collections.Generic;
using HG;
using RoR2;
using RoR2.Orbs;
using UnityEngine;

public class VineOrb : Orb
{
    public struct SplitDebuffInformation
    {
        public GameObject attacker;

        public CharacterMaster attackerMaster;

        public BuffIndex index;

        public bool isTimed;

        public float duration;

        public int count;

        public float dotDamageMultiplier;
    }

    public List<SplitDebuffInformation> splitDebuffInformation;

    public override void Begin()
    {
        base.duration = 0.1f;
        EffectData effectData = new EffectData
        {
            origin = origin,
            genericFloat = base.duration
        };
        effectData.SetHurtBoxReference(target);
        EffectManager.SpawnEffect(OrbStorageUtility.Get("Prefabs/Effects/OrbEffects/ChainVineOrbEffect"), effectData, transmit: true);
    }

    public override void OnArrival()
    {
        CharacterBody characterBody = target.AsValidOrNull()?.healthComponent.AsValidOrNull()?.body;
        if (!characterBody)
        {
            return;
        }
        foreach (SplitDebuffInformation item in splitDebuffInformation)
        {
            BuffDef buffDef = BuffCatalog.GetBuffDef(item.index);
            if (buffDef == null)
            {
                continue;
            }
            if (buffDef.isDOT)
            {
                DotController.DotIndex dotDefIndex = DotController.GetDotDefIndex(buffDef);
                DotController.DotDef dotDef = DotController.GetDotDef(dotDefIndex);
                if (dotDef == null)
                {
                    continue;
                }
                InflictDotInfo inflictDotInfo = default;
                inflictDotInfo.attackerObject = item.attacker;
                inflictDotInfo.victimObject = characterBody.gameObject;
                inflictDotInfo.damageMultiplier = item.dotDamageMultiplier;
                inflictDotInfo.dotIndex = dotDefIndex;
                inflictDotInfo.duration = (item.duration != 0f) ? item.duration : dotDef.interval;
                InflictDotInfo inflictDotInfo2 = inflictDotInfo;
                for (int i = 0; i < item.count; i++)
                {
                    DotController.InflictDot(ref inflictDotInfo2);
                }
            }
            else if (buffDef.isDebuff)
            {
                if (item.isTimed)
                {
                    for (int j = 0; j < item.count; j++)
                    {
                        characterBody.AddTimedBuff(item.index, item.duration);
                    }
                }
                else
                {
                    for (int k = 0; k < item.count; k++)
                    {
                        characterBody.AddBuff(item.index);
                    }
                }
            }
            GlobalEventManager.ProcDeathMark(target.gameObject, characterBody, item.attackerMaster);
        }
        Util.PlaySound("Play_item_proc_triggerEnemyDebuffs", characterBody.gameObject);
    }
}