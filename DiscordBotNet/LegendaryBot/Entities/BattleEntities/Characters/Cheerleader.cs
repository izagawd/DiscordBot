using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;


public class  YouCanDoIt : Skill
{
    public override string GetDescription(Character character)
    {
        return "Increases the combat readiness of a single target by 100%, increasing their attack for 2 turns. " +
               "Cannot be used on self";
    }

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.Team.Where(i => !i.IsDead && i != owner);
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        target.IncreaseCombatReadiness(100);
        target.AddStatusEffect(new AttackBuff(owner) { Duration = 2 });
        owner.CurrentBattle.AddAdditionalBattleText($"{owner} wants {target} to prevail!");
        return new UsageResult(this)
        {
            TargetType = TargetType.SingleTarget,
            User = owner,
            UsageType = usageType,
        };
    }
    public override int MaxCooldown => 2;
}

public class YouCanMakeItEveryone : Surge
{
    private int CombatIncreaseAmount => 30;
    public override string GetDescription(Character character)
    {
        return $"Encourages all allies, increasing their combat readiness by {CombatIncreaseAmount}%, and increases their attack for 2 turns";
    }

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.Team.Where(i => !i.IsDead);
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        owner.CurrentBattle.AddAdditionalBattleText($"{owner} encourages her allies!");
        var targets = GetPossibleTargets(owner).ToArray();
        foreach (var i in targets)
        {
            i.IncreaseCombatReadiness(CombatIncreaseAmount);
  
        }
        foreach (var i in targets)
        {
            i.AddStatusEffect(new AttackBuff(owner) { Duration = 2 });
        }
        return new UsageResult(this)
        {
            TargetType = TargetType.AOE,
            User = owner,
            UsageType = UsageType.NormalUsage
        };

    }

    public override int MaxCooldown => 4;
}
public class Cheerleader : Character
{
    public override int GetSpeedValue(int points)
    {
        return (base.GetSpeedValue(points) * 1.2).Round();
    }

    public override BasicAttack BasicAttack { get; } = new BasicAttackSample();
    public override Skill? Skill { get; } = new YouCanDoIt();
    public override Surge? Surge { get; } = new YouCanMakeItEveryone();
}