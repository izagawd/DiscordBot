using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Battle.StatusEffects;

public class Immortality : StatusEffect
{
    public override bool IsRenewable { get; } = true;
    public override int MaxStacks { get; } = 1;
    public override StatusEffectType EffectType { get; } = StatusEffectType.Buff;

    public Immortality(Character caster) : base(caster)
    {
    }


}