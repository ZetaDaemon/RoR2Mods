using System;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;

namespace ZetaItemBalance.Items.Green
{
    public class HuntersHarpoon : ItemBase
    {
        protected override string CONFIG_SECTION => "Hunters Harpoon";
        float BaseSpeed;
        float StackSpeed;
        float BaseDuration;
        float StackDuration;

        protected override void InitConfig()
        {
            BaseSpeed = BindToConfig("Base Speed", 0.75f);
            StackSpeed = BindToConfig("Stack Speed", 0f);
            BaseDuration = BindToConfig("Base Duration", 2f);
            StackDuration = BindToConfig("Stack Duration", 2f);
        }

        protected override void Setup()
        {
            ClampConfig();
            UpdateText();
            Hooks();
        }

        private void ClampConfig()
        {
            BaseSpeed = Math.Max(0, BaseSpeed);
            StackSpeed = Math.Max(0, StackSpeed);
            BaseDuration = Math.Max(0, BaseDuration);
            StackDuration = Math.Max(0, StackDuration);
        }

        private void UpdateText()
        {
            string desc = "";
            if (StackSpeed > 0)
            {
                desc =
                    $"Killing an enemy increases <style=cIsUtility>movement speed</style> by <style=cIsUtility>{BaseSpeed * 100}%</style> <style=cStack>(+{StackSpeed * 100}% per stack)</style> for <style=cIsUtility>{BaseDuration}</style> <style=cStack>(+{StackDuration} per stack)</style> seconds.";
            }
            else
            {
                desc =
                    $"Killing an enemy increases <style=cIsUtility>movement speed</style> by <style=cIsUtility>{BaseSpeed * 100}%</style> for <style=cIsUtility>{BaseDuration}</style> <style=cStack>(+{StackDuration} per stack)</style> seconds.";
            }

            LanguageAPI.Add("ITEM_MOVESPEEDONKILL_DESC", desc);
        }

        private void Hooks()
        {
            IL.RoR2.GlobalEventManager.OnCharacterDeath += new ILContext.Manipulator(IL_OnCharacterDeath);
            IL.RoR2.CharacterBody.RecalculateStats += new ILContext.Manipulator(IL_RecalculateStats);
        }

        private void IL_OnCharacterDeath(ILContext il)
        {
            ILCursor ilcursor = new(il);
            if (!ilcursor.TryGotoNext(x => x.MatchLdsfld("RoR2.DLC1Content/Items", "MoveSpeedOnKill")))
            {
                MainPlugin.ModLogger.LogError("Hunters Harpoon - Duration - IL Hook Failed");
                return;
            }
            ilcursor.Index += 11;
            ilcursor.Emit(OpCodes.Pop);
            ilcursor.Emit(OpCodes.Ldc_I4_1);
            ilcursor.Index += 1;
            ilcursor.Next.Operand = BaseDuration;
            ilcursor.Index += 3;
            ilcursor.Next.Operand = StackDuration;
        }

        private void IL_RecalculateStats(ILContext il)
        {
            ILCursor ilcursor = new(il);
            if (
                !ilcursor.TryGotoNext(
                    x => x.MatchLdcR4(0.25f),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdsfld(typeof(RoR2.DLC1Content.Buffs), "KillMoveSpeed")
                )
            )
            {
                MainPlugin.ModLogger.LogError("Hunters Harpoon - Speed - IL Hook Failed");
                return;
            }
            ilcursor.Next.Operand = BaseSpeed;
            if (StackSpeed > 0)
            {
                ilcursor.Index += 1;
                ilcursor.Emit(OpCodes.Ldarg_0);
                ilcursor.EmitDelegate<Func<CharacterBody, int>>(
                    (cb) =>
                    {
                        Inventory inventory = cb?.master?.inventory;
                        if (inventory is null)
                        {
                            return 0;
                        }
                        return inventory.GetItemCountEffective(DLC1Content.Items.MoveSpeedOnKill);
                    }
                );
                ilcursor.Emit(OpCodes.Ldc_I4_1);
                ilcursor.Emit(OpCodes.Sub);
                ilcursor.Emit(OpCodes.Conv_R4);
                ilcursor.Emit(OpCodes.Ldc_R4, StackSpeed);
                ilcursor.Emit(OpCodes.Mul);
                ilcursor.Emit(OpCodes.Add);
            }
        }
    }
}
