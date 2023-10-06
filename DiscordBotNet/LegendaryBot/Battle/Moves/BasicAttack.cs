using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Results;

namespace DiscordBotNet.LegendaryBot.Battle.Moves;

public abstract class BasicAttack : Move
{





    public sealed override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team && !i.IsDead);
    }

    public sealed override UsageResult Utilize(Character owner, Character target, UsageType usageType)
    {
        return base.Utilize(owner, target, usageType);
    }


}