using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class WindSlash : Skill
{

    public override string GetDescription(Character character) => "Attacks all enemies with a sharp wind";
    

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team);
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        List<DamageResult> damageResults = [];
        foreach (var i in GetPossibleTargets(owner))
        {
            var damageResult = i.Damage(new DamageArgs(this)
                { Caster = owner, Damage = owner.Attack * 1.7, DamageText = $"The slash dealt $ damage to {i}!" });
            if(damageResult is not null)
                damageResults.Add(damageResult);
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

public class SimpleSlashOfPrecision : BasicAttack
{
    private int BleedChance => 50;
    public override string GetDescription(Character character) =>$"Does a simple slash. Always lands a critical hit, with a {BleedChance}% chance to cause bleed for 2 turns";
    

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        var damageResult = target.Damage(new DamageArgs(this)
        {
            Caster = owner,
            Damage = owner.Attack * 1.7,
            AlwaysCrits = true
        });
        if (BasicFunction.RandomChance(BleedChance))
        {
            target.StatusEffects.Add(new Bleed(owner));
        }
        return new UsageResult(this)
        {
            DamageResults = [damageResult],
            TargetType = TargetType.SingleTarget,
            Text = $"{owner} does a simple slash to {target}!",
            User = owner,
            UsageType = usageType
        };
    }
}
public class ConsecutiveSlashesOfPrecision : Surge
{

    public override string GetDescription(Character character)
        =>"Slashes the enemy many times, dealing crazy damage. This attack will always deal a critical hit";

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team && !i.IsDead);
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        var damageResult =target.Damage(new DamageArgs(this)
        {
            CanCrit = true,
            Caster = owner,
            Damage = owner.Attack * 1.7 *2,
            AlwaysCrits = true,
            DamageText = $"The slash was so precise it dealt $ damage to {target}!",
     
        });

        return new UsageResult(this)
        {
            DamageResults =  [damageResult],
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            User = owner
        };
    }

    public override int MaxCooldown => 5;
}
public class Slasher : Character
{
    public override Rarity Rarity { get; protected set; } = Rarity.FiveStar;
    public override DiscordColor Color { get; protected set; } = DiscordColor.Brown;

    public override Element Element { get; protected set; } = Element.Earth;

    public override Surge? Surge { get; } = new ConsecutiveSlashesOfPrecision();
    public override Skill? Skill { get; } = new WindSlash();
    public override BasicAttack BasicAttack { get;  } = new SimpleSlashOfPrecision();
}