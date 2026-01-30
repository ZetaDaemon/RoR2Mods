using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace ZetaItemBalance.Items.Void
{
    public class LysateCell : ItemBase
    {
        protected override string CONFIG_SECTION => "Lystate Cell";
        float Cooldown;

        protected override void InitConfig()
        {
            Cooldown = BindToConfig("Cooldown", 0);
        }

        protected override void Setup()
        {
            ClampConfig();
            UpdateText();
            Hooks();
        }

        private void ClampConfig()
        {
            Cooldown = Mathf.Clamp(Cooldown, 0, 1);
        }

        private void UpdateText() { }

        private void Hooks()
        {
            IL.RoR2.CharacterBody.RecalculateStats += new ILContext.Manipulator(IL_RecalculateStats);
        }

        private void IL_RecalculateStats(ILContext il)
        {
            ILCursor ilcursor = new(il);
            if (!ilcursor.TryGotoNext(x => x.MatchLdsfld(typeof(RoR2.DLC1Content.Items), "EquipmentMagazineVoid")))
            {
                MainPlugin.ModLogger.LogError("Lysate Cell 1 - Cooldown - IL Hook Failed");
                return;
            }
            ilcursor.Index += 2;
            int lystateStackLocation = ((VariableDefinition)ilcursor.Next.Operand).Index;
            if (!ilcursor.TryGotoNext(x => x.MatchLdloc(lystateStackLocation)))
            {
                MainPlugin.ModLogger.LogError("Lysate Cell 2 - Cooldown - IL Hook Failed");
                return;
            }
            ilcursor.Index += 8;
            ilcursor.Next.Operand = 1 - Cooldown;
        }
    }
}
