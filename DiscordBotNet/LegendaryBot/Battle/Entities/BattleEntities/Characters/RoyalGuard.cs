using DiscordBotNet.LegendaryBot.Battle.Moves;
using DiscordBotNet.LegendaryBot.Battle.Results;
using DiscordBotNet.LegendaryBot.Battle.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

public class IWillBeYourShield : Skill
{
    public override int GetMaxCooldown(int level)
    {
        if (level >= 1) return 3;
        return 4;
    }

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.Team.Where(i =>!i.IsDead);
    }

    public override string GetDescription(int moveLevel)
    {
        return "Increases the defense and gives a shield to the target and caster for 3 turns";
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        target.StatusEffects.Add(new Shield(owner, 1000){Duration = 3});
        target.StatusEffects.Add(new DefenseBuff(owner) { Duration = 3 });

        return new UsageResult($"As a loyal knight, {owner} helps {target}!", usageType);
    }
}

public class IWillProtectUs : Surge
{
    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.Team.Where(i => !i.IsDead);
    }

    public override int GetMaxCooldown(int level)
    {
        if (level >= 1) return 4;
        return 5;
    }

    public override string GetDescription(int moveLevel)
    {
        return "Increases the defense of all allies for 2 turns";
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        foreach (var i in GetPossibleTargets(owner))
        {
            i.StatusEffects.Add(new DefenseBuff(owner) { Duration = 2 });
            
        }

        return new UsageResult($"As a loyal knight, {owner} increases the defense of all allies!", usageType);
    }
}
public class RoyalGuard : Character
{
    public override Element Element { get; protected set; } = Element.Ice;
    public override int BaseMaxHealth => 1200 + (70 * Level);
    public override int BaseAttack => (110 + (10 * Level));
    public override int BaseDefense => (110 + (7.2 * Level)).Round();
    public override int BaseSpeed => 99;
    public override Rarity Rarity => Rarity.ThreeStar;
    public override Surge Surge { get; protected set; } = new IWillProtectUs();
    public override Skill Skill { get; protected set; } = new IWillBeYourShield();
}
