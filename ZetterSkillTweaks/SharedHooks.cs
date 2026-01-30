using System;
using RoR2.Skills;

namespace ZetterSkillTweaks;

public class SharedHooks
{
    public delegate void Handle_SkillCatalog_PostSetSkillDefs();
    public static Handle_SkillCatalog_PostSetSkillDefs Handle_SkillCatalog_PostSetSkillDefs_Actions;

    public static void Setup()
    {
        if (Handle_SkillCatalog_PostSetSkillDefs_Actions != null)
        {
            On.RoR2.Skills.SkillCatalog.SetSkillDefs += SkillCatalog_SetSkillDefs;
        }
    }

    static void SkillCatalog_SetSkillDefs(On.RoR2.Skills.SkillCatalog.orig_SetSkillDefs orig, SkillDef[] newSkillDefs)
    {
        orig(newSkillDefs);
        Handle_SkillCatalog_PostSetSkillDefs_Actions.Invoke();
    }
}
