using System.Collections;
using DiscordBotNet.LegendaryBot.Battle.Moves;
using DiscordBotNet.LegendaryBot.Battle.Results;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

public class WindSlash : Skill
{
    public override int MaxEnhance { get; } = 4;
    public override string Description => "Attacks all enemies with a sharp wind";
    

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team);
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        List<DamageResult> damageResults = new List<DamageResult>();
        foreach (var i in target.Team)
        {
            damageResults.Add(i.Damage(new DamageArgs(this){Caster = owner,Damage = owner.Attack * 1.7, DamageText = $"The slash dealt $ damage to {i}!"}));
        }

        return new UsageResult(this)
        {
            DamageResults = damageResults,
            TargetType = TargetType.AOE,
            User = owner,
            Text = "Wind Slash!",
            UsageType = usageType

        };
    }

    public override int MaxCooldown => 2;
}

public class SimpleSlash : BasicAttack
{
    public override string Description =>"Does a simple slash";
    

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        return new UsageResult(this)
        {
            DamageResults = new DamageResult[]
            {
                target.Damage(new DamageArgs(this)
                {
                    Caster = owner,
                    Damage = owner.Attack * 1.7
                    
                })
            },
            TargetType = TargetType.SingleTarget,
            Text = $"{owner} does a simple slash to {target}!",
            User = owner,
            UsageType = usageType
        };
    }
}
public class SlashOfPrecision : Surge
{
    public override int MaxEnhance { get; } = 5;
    public override string Description=>"Slashes the enemy many times, dealing crazy damage. This attack will always deal a critical hit";

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team);
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        var damageResult =target.Damage(new DamageArgs(this)
        {
            CanCrit = true,
            Caster = owner,
            Damage = owner.Attack * 3,
            AlwaysCrits = true,
            DamageText = $"The slash was so precise it dealt $ damage to {target}!",
     
        });

        return new UsageResult(this)
        {
            DamageResults = new List<DamageResult>() { damageResult },
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            User = owner
        };
    }

    public override int MaxCooldown => 5;
}
public class Slasher : Character
{
    public override DiscordColor Color { get; protected set; } = DiscordColor.Brown;
    public override Rarity Rarity { get; protected set; } = Rarity.FiveStar;
    public override Element Element { get; protected set; } = Element.Earth;
    public override int BaseMaxHealth => 1100 + (60 * Level);
    public override int BaseAttack => (120 + (13 * Level));
    public override int BaseDefense => (100 + (5.2 * Level)).Round();
    public override int BaseSpeed => 105;
    public override Surge Surge { get; } = new SlashOfPrecision();
    public override Skill Skill { get; } = new WindSlash();
    public override BasicAttack BasicAttack { get;  } = new SimpleSlash();
}