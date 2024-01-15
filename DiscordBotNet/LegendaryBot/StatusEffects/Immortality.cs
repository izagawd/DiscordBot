using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Immortality : StatusEffect
{
    public override string Description { get; } =
        "Makes the affected not able to die by preventing their hp from going below one";


    public override int MaxStacks { get; } = 1;
    public override StatusEffectType EffectType { get; } = StatusEffectType.Buff;
    public Immortality(Character caster) : base(caster)
    {
    }


}