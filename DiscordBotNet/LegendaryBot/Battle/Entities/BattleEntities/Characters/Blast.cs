using DiscordBotNet.LegendaryBot.Battle.Moves;
using DiscordBotNet.LegendaryBot.Battle.Results;
using DiscordBotNet.LegendaryBot.Battle.StatusEffects;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

public class MethaneSlap : BasicAttack
{
    public override string GetDescription( int level)
    {
        
        return $"Slaps the enemy, producing methane around the enemy, with a {GetDetonateChance(level)}% chance to detonate all the bombs the target has";
    }

    public override int MaxEnhance { get; } = 4;

    public int GetDetonateChance(int level)
    {
        switch (level)
        {
            case 0:
                return 40;
            case 1:
            case 2:    
                return 50;
            case 3:
                return 60;
            default:
                return 75;
        }
    }
    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        var result = new UsageResult(usageType, TargetType.SingleTarget);

        result.DamageResults.Add(target.Damage(        new DamageArgs()
        {
            Damage = owner.Attack* 1.871,
            Caster = owner,
            CanCrit = true,
            DamageText ="That was a harsh slap that dealt $ damage!"
        }));
        result.Text = "Methane Slap!";
        if (BasicFunction.RandomChance(GetDetonateChance(owner.BasicAttackLevel)))
        {
            foreach (var i in target.StatusEffects.OfType<Bomb>())
            {
                i.Detonate(target);
            }
        }
        return result;

    }
}
public class BlowAway : Skill
{
    public override int GetMaxCooldown(int level) => 4;
    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team&& !i.IsDead);
    }
    public override int MaxEnhance { get; } = 4;
    public int GetBombInflictChange(int level)
    {
        switch (level)
        {
            case 0:
                return 65;
            case 1:
            case 2:    
                return 75;
            case 3:
                return 85;
            default:
                return 100;
        }
    }
    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
                
        owner.CurrentBattle.AdditionalTexts.Add($"{owner} threw multiple bombs at the opposing team!");
        foreach (var i in target.Team)
        {

            foreach (var _ in Enumerable.Range(0,1))
            {
                if (BasicFunction.RandomChance(GetBombInflictChange(owner.SkillLevel)))
                {
                                
                    i.StatusEffects.Add(new Bomb(owner){Duration = 2}, owner.Effectiveness);
                }
            }

        }

        return new UsageResult(usageType, TargetType.AOE,"Blow Away!");
        
    }

    public override string GetDescription(int moveLevel)
    {
        return
            $"Throws multiple bombs at the enemy, with a {GetBombInflictChange(moveLevel)} each to inflict Bomb status effect";
    }
}
public class VolcanicEruption : Surge
{
    public override string GetDescription(int moveLevel)
    {
        return $"Makes the user charge up a very powerful explosion that hits all enemies for 4 turns!";
    }

    public override int GetMaxCooldown(int level)  => 6;
    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team&& !i.IsDead);
    }
    public override int MaxEnhance { get; } = 4;
    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        owner.StatusEffects.Add(new VolcanicEruptionCharging(owner){Duration = 4});
        owner.CurrentBattle.AdditionalTexts.Add($"{owner} is charging up a very powerful attack!");
        return new UsageResult(usageType, TargetType.AOE);
    }
}
public class Blast : Character
{
    public override DiscordColor Color { get; protected set; } = DiscordColor.Brown;
    public override Rarity Rarity { get; protected set; } = Rarity.FiveStar;
    public override Element Element { get; protected set; } = Element.Fire;
    public override int BaseMaxHealth => 1100 + (60 * Level);
    public override int BaseAttack => (120 + (13 * Level));
    public override int BaseDefense => (100 + (5.2 * Level)).Round();
    public override int BaseSpeed => 105;
    public override Surge Surge { get; protected set; } = new VolcanicEruption();
    public override Skill Skill { get; protected set; } = new BlowAway();
    public override BasicAttack BasicAttack { get;  } = new MethaneSlap();
}