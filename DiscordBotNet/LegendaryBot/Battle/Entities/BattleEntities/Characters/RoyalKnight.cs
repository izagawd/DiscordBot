using DiscordBotNet.LegendaryBot.Battle.Moves;
using DiscordBotNet.LegendaryBot.Battle.Results;
using DiscordBotNet.LegendaryBot.Battle.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
public class ShieldBash : BasicAttack
{
    public override string GetDescription(int moveLevel)
    {
        return $"Bashes the shield to the enemy, with a {GetShieldStunChanceByBash(moveLevel)}% chance to stun"!;
    }

    public int GetShieldStunChanceByBash(int moveLevel)
    {
        switch (moveLevel)
        {
            case 0:
                return 5;
            case 1:
                return 6;
            case 2:
                return 7;
            case 3:
                return 8;
            case 4:
                return 9;
            default:
                return 10;
        }
    }
    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        var usageResult =  new UsageResult(this)
        {
            DamageResults = new []
            {
                target.Damage(new DamageArgs(this)
                {
                    Caster = owner,
                    DamageText =
                        $"{owner} bashes {target} with his shield , making them receive $ damage!",
                    Damage = owner.Attack * 1.7

                }),
            },
            User = owner,
            TargetType = TargetType.SingleTarget,
            Text = "Hrraagh!!",
            UsageType = usageType

        };
        if (BasicFunction.RandomChance(GetShieldStunChanceByBash(owner.GetMoveLevel(this))))
        {
            target.StatusEffects.Add(new Stun(owner));
        }

        return usageResult;
    }
}
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

    public override int MaxEnhance { get; } = 5;

    public override string GetDescription(int moveLevel)
    {
        return "Increases the defense and gives a shield to the target and caster for 3 turns";
    }

    public int GetShieldBasedOnDefense(int moveLevel)
    {
        switch (moveLevel)
        {
            case 0:
                return 150;
            case 1:
                return 170;
            case 2:
                return 200;
            case 3:
                return 250;
            case 4:
                return 270;
            default:
                return 300;
        }
    }
    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        target.StatusEffects.Add(new Shield(owner, (GetShieldBasedOnDefense(owner.GetMoveLevel(this)) * 0.01 * owner.Defense).Round()){Duration = 3});
        target.StatusEffects.Add(new DefenseBuff(owner) { Duration = 3 });

        return new UsageResult(this)
        {
            Text = $"As a loyal knight, {owner} helps {target}!",
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            User = owner
        };
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

    public override int MaxEnhance { get; } = 5;

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

        return new UsageResult(this)
        {
            UsageType = usageType,
            TargetType = TargetType.AOE,
            Text = $"As a loyal knight, {owner} increases the defense of all allies for three turns",
            User = owner
        };
    }
}

public class RoyalKnight : Character
{
    public override Element Element { get; protected set; } = Element.Ice;
    public override int BaseMaxHealth => 1200 + (70 * Level);
    public override int BaseAttack => (110 + (10 * Level));
    public override int BaseDefense => (110 + (7.2 * Level)).Round();
    public override int BaseSpeed => 99;
    public override BasicAttack BasicAttack { get; } = new ShieldBash();
    public override Rarity Rarity => Rarity.ThreeStar;
    public override Surge Surge { get; } = new IWillProtectUs();
    public override Skill Skill { get;  } = new IWillBeYourShield();
}
