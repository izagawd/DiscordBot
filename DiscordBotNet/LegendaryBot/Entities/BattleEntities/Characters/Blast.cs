using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class MethaneSlap : BasicAttack
{
    public override string GetDescription(Character character) => $"Slaps the enemy, " +
                                                                  $"producing methane around the enemy, with a " +
                                                                  $"{DetonateChance}% chance to detonate all the bombs the target has";
    public int DetonateChance => 75;
    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        var damageResult = target.Damage(new DamageArgs(this)
        {
            Damage = owner.Attack * 1.7,
            Caster = owner,
            CanCrit = true,
            DamageText = $"That was a harsh slap on {target} dealt $ damage!"
        });
        var damageResultList = new []{ damageResult };
        var result = new UsageResult(this)
        {
            Text = "Methane Slap!",User = owner,
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            DamageResults = damageResultList
        };
        if (BasicFunction.RandomChance(DetonateChance))
        {
            foreach (var i in target.StatusEffects.OfType<Bomb>())
                i.Detonate(target);
        }
        return result;

    }
}
public class BlowAway : Skill
{
    
    public override int MaxCooldown => 4;
    public override string GetDescription(Character character) => $"Throws multiple bombs at the enemy, with a {BombInflictChance} each to inflict Bomb status effect";

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team&& !i.IsDead);
    }

    public int BombInflictChance => 100;
    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
                
        owner.CurrentBattle.AdditionalTexts.Add($"{owner} threw multiple bombs at the opposing team!");
        foreach (var i in GetPossibleTargets(owner))
        {

            foreach (var _ in Enumerable.Range(0,1))
            {
                if (BasicFunction.RandomChance(BombInflictChance))
                {
                                
                    i.StatusEffects.Add(new Bomb(owner){Duration = 2}, owner.Effectiveness);
                }
            }

        }

        return new UsageResult(this){TargetType = TargetType.AOE,Text = "Blow Away!",User = owner,UsageType = usageType};
        
    }


}
public class VolcanicEruption : Surge
{
    public override string GetDescription(Character character) => $"Makes the user charge up a very powerful explosion that hits all enemies for 4 turns!";
    

    public override int MaxCooldown  => 6;
    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team&& !i.IsDead);
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        owner.StatusEffects.Add(new VolcanicEruptionCharging(owner){Duration = 3});
        owner.CurrentBattle.AdditionalTexts.Add($"{owner} is charging up a very powerful attack!");
        return new UsageResult(this){UsageType = usageType, TargetType = TargetType.AOE, User = owner, Text = "What's this?"};
    }
}
public class Blast : Character
{
    public override Rarity Rarity { get; protected set; } = Rarity.FourStar;
    public override DiscordColor Color { get; protected set; } = DiscordColor.Brown;

    public override Element Element { get; protected set; } = Element.Fire;
    public override int BaseMaxHealth => 1100 + (60 * Level);
    public override int BaseAttack => (120 + (13 * Level));
    public override int BaseDefense => (100 + (5.2 * Level)).Round();
    public override int BaseSpeed => 105;
    public override Surge? Surge { get;  } = new VolcanicEruption();
    public override Skill? Skill { get; } = new BlowAway();
    public override BasicAttack BasicAttack { get; } = new MethaneSlap();
}