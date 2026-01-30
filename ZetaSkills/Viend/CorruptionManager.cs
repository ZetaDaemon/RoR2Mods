using System;
using System.Collections.Generic;
using RoR2;
using RoR2.Skills;

namespace ZetaSkills.Viend
{
    public class CorruptionManager
    {
        private static Dictionary<SkillDef, SkillDef> Overrides;

        public CorruptionManager()
        {
            Overrides = new Dictionary<SkillDef, SkillDef>();

            On.EntityStates.VoidSurvivor.CorruptMode.CorruptMode.OnEnter += (orig, self) =>
            {
                SkillDef overrideSkill;
                if (Overrides.TryGetValue(self.skillLocator.primary.skillDef, out overrideSkill))
                {
                    self.primaryOverrideSkillDef = overrideSkill;
                }
                if (Overrides.TryGetValue(self.skillLocator.secondary.skillDef, out overrideSkill))
                {
                    self.secondaryOverrideSkillDef = overrideSkill;
                }
                if (Overrides.TryGetValue(self.skillLocator.utility.skillDef, out overrideSkill))
                {
                    self.utilityOverrideSkillDef = overrideSkill;
                }
                if (Overrides.TryGetValue(self.skillLocator.special.skillDef, out overrideSkill))
                {
                    self.specialOverrideSkillDef = overrideSkill;
                }
                orig(self);
            };
        }

        public static void AddOverride(SkillDef normal, SkillDef corrupted)
        {
            Overrides.Add(normal, corrupted);
        }
    }
}