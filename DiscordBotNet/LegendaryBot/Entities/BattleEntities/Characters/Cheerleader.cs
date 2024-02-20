using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class PomPomAttack : BasicAttack
{
    public override string GetDescription(Character character)
    {
        return "Caster hits the enemy with a pom-pom... and that it";
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {

        return new UsageResult(this)
        {
            DamageResults =
            [
                target.Damage(new DamageArgs(this)
                {
                    Caster = owner,
                    Damage = 1,
                    DamageText = $"{owner} hits {target} with their pompoms, dealing $ damage!"
                })
            ],
            TargetType = TargetType.SingleTarget,
            User = owner,
            UsageType = usageType
        };
    }
}

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
        owner.CurrentBattle.AddAdditionalBattleText($"{owner} wants {target} to prevail!");
        target.IncreaseCombatReadiness(100);
        target.AddStatusEffect(new AttackBuff(owner) { Duration = 2 });

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

        using (owner.CurrentBattle.PauseBattleEventScope)
        {
            foreach (var i in targets)
            {
                i.IncreaseCombatReadiness(CombatIncreaseAmount);
            }
            foreach (var i in targets)
            {
                i.AddStatusEffect(new AttackBuff(owner) { Duration = 2 });
            }

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
    public override float GetSpeedValue(int points)
    {
        return base.GetSpeedValue(points) * 1.2f;
    }

    public override float GetMaxHealthValue(int points)
    {
        return base.GetMaxHealthValue(points) * 1.2f;
    }

    public override BasicAttack BasicAttack { get; } = new PomPomAttack();
    public override Skill? Skill { get; } = new YouCanDoIt();
    public override Surge? Surge { get; } = new YouCanMakeItEveryone();
}