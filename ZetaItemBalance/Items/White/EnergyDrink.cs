using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;

namespace ZetaItemBalance.Items.White;

public class EnergyDrink : ItemBase
{
    protected override string CONFIG_SECTION => "EnergyDrink";
    private float SprintPerStack;

    protected override void InitConfig()
    {
        SprintPerStack = BindToConfig("SprintPerStack", 0.14f);
    }

    protected override void Setup()
    {
        IL.RoR2.CharacterBody.RecalculateStats += new ILContext.Manipulator(IL_RecalculateStats);
    }

    private void IL_RecalculateStats(ILContext il)
    {
        ILCursor ilcursor = new(il);
        int drinkLoc = -1;
        // get energy drink location and disable regular boost
        if (
            !ilcursor.TryGotoNext(
                x => x.MatchLdloc(out drinkLoc),
                x => x.MatchConvR4(),
                x => x.MatchMul(),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CharacterBody>(nameof(CharacterBody.sprintingSpeedMultiplier))
            )
        )
        {
            MainPlugin.ModLogger.LogError("Energy Drink 1 - IL Hook Failed");
            return;
        }
        ilcursor.Index += 1;
        ilcursor.Emit(OpCodes.Pop);
        ilcursor.Emit(OpCodes.Ldc_I4_0);

        ilcursor.Index = 0;
        //multiply the sprint mult
        if (
            !ilcursor.TryGotoNext(
                MoveType.After,
                x => x.MatchLdloc(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CharacterBody>(nameof(CharacterBody.sprintingSpeedMultiplier))
            )
        )
        {
            MainPlugin.ModLogger.LogError("Energy Drink 2 - IL Hook Failed");
            return;
        }
        ilcursor.Emit(OpCodes.Ldc_R4, 1f);
        ilcursor.Emit(OpCodes.Ldc_R4, SprintPerStack);
        ilcursor.Emit(OpCodes.Ldloc, drinkLoc);
        ilcursor.Emit(OpCodes.Conv_R4);
        ilcursor.Emit(OpCodes.Mul);
        ilcursor.Emit(OpCodes.Add);
        ilcursor.Emit(OpCodes.Mul);
    }
}
