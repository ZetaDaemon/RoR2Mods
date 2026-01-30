using System;
using System.Linq;
using BepInEx.Configuration;

namespace ZetterSkillTweaks.Skills;

public abstract class SkillBase
{
    protected abstract string CONFIG_SECTION { get; }
    private readonly bool Enabled;

    protected T BindToConfig<T>(string label, T defaultValue, string? description = null)
    {
        if (Configs.ModConfig is ConfigFile config)
        {
            return config
                .Bind(
                    $"{GetType().Namespace.Split('.').Last()}: {CONFIG_SECTION}",
                    label,
                    defaultValue,
                    description ?? label
                )
                .Value;
            ;
        }
        return defaultValue;
    }

    protected abstract void InitConfig();
    protected abstract void Setup();

    public SkillBase()
    {
        Enabled = BindToConfig("Enabled", true);
        InitConfig();
        if (Enabled)
        {
            Setup();
        }
    }
}
