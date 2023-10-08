using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Battle.Moves;
using DiscordBotNet.LegendaryBot.Battle.Results;
using DiscordBotNet.LegendaryBot.Battle.StatusEffects;
using DSharpPlus.Entities;
using Barrier = DiscordBotNet.LegendaryBot.Battle.StatusEffects.Barrier;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
public class ShieldBash : BasicAttack
{
    public override string Description=> $"Bashes the shield to the enemy, with a {ShieldStunChanceByBash}% chance to stun"!;


    public int ShieldStunChanceByBash => 10;
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
        if (BasicFunction.RandomChance(ShieldStunChanceByBash))
        {
            target.StatusEffects.Add(new Stun(owner));
        }

        return usageResult;
    }
}
public class IWillBeYourShield : Skill
{
    public override int MaxCooldown => 4;

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.Team.Where(i =>!i.IsDead);
    }

  
    public override string Description=> "gives a shield to the target and caster for 3 turns. Shield strength is proportional to the caster's defense";


    public int ShieldBasedOnDefense => 300;
    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        target.StatusEffects.Add(new Barrier(owner, (ShieldBasedOnDefense * 0.01 * owner.Defense).Round()){Duration = 3});


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

    public override int MaxCooldown => 5;
  

    public override string Description=> "Increases the defense of all allies for 3 turns";
    

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
    public override DiscordColor Color => DiscordColor.Blue;
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
