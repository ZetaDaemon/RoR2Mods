using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;

namespace ZetaItemBalance.Items.Red;

public class RunicLens : ItemBase
{
    protected override string CONFIG_SECTION => "Runic Lens";

    protected override void InitConfig() { }

    protected override void Setup()
    {
        IL.RoR2.MeteorAttackOnHighDamageBodyBehavior.DetonateRunicLensMeteor += new ILContext.Manipulator(
            IL_DetonateRunicLensMeteor
        );
    }

    private void IL_DetonateRunicLensMeteor(ILContext il)
    {
        ILCursor ilcursor = new(il);
        if (!ilcursor.TryGotoNext(x => x.MatchInitobj(typeof(ProcChainMask))))
        {
            MainPlugin.ModLogger.LogError("Runic Lens - IL Hook Failed");
            return;
        }
        ilcursor.Index += 1;
        ilcursor.Emit(OpCodes.Dup);
        ilcursor.Emit(OpCodes.Ldarg_0);
        ilcursor.EmitDelegate<Func<MeteorAttackOnHighDamageBodyBehavior, ProcChainMask>>(
            (bodyBehavior) =>
            {
                if (bodyBehavior is null)
                {
                    return new();
                }
                ProcChainMask procMask = bodyBehavior.body.runicLensDamageInfo.procChainMask;
                procMask.AddProc(ProcType.MeteorAttackOnHighDamage);
                return procMask;
            }
        );
        ilcursor.Emit<BlastAttack>(OpCodes.Stfld, nameof(BlastAttack.procChainMask));
    }
}
