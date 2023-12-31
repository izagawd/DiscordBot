using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.Rewards;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;



public class DoNotResist : BasicAttack
{
    public override string GetDescription(Character character)
    {
        return "Tases the enemy, with a 15% chance to stun for one turn";
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        var damageResult= target.Damage(new DamageArgs(this)
        {
            Damage = owner.Attack * 1.7,
            Caster = owner,
            DamageText = $"{owner} tases {target} and dealt $ damage! it was shocking"
        });
        if (BasicFunction.RandomChance(15))
        {
            target.StatusEffects.Add(new Stun(owner){Duration = 1}, owner.Effectiveness);
        }

        return new UsageResult(this)
        {
            DamageResults = [damageResult],
            Text = "DO NOT RESIST!",
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            User = owner,

        };
    }
}

public class IAmShooting : Skill
{
    public override string GetDescription(Character character)
    {
        return "Shoots the enemy twice, causing two bleed effects for two turns";
    }

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team && !i.IsDead);
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        var damageResult = target.Damage(new DamageArgs(this)
        {
            Caster = owner,
            Damage = owner.Attack * 2,
            DamageText = $"{owner} shoots at {target} for resisting arrest, dealing $ damage"
        });
        foreach (var _ in Enumerable.Range(0,2))
        {
            target.StatusEffects.Add(new Bleed(owner), owner.Effectiveness);
        }
        return new UsageResult(this)
        {
            DamageResults = [damageResult],
            Text = "I warned you!",
            User = owner,
            TargetType = TargetType.SingleTarget,
            UsageType = usageType,
        };
    }

    public override int MaxCooldown { get; }
}
public class Police : Character
{
    public override Rarity Rarity { get; protected set; } = Rarity.TwoStar;

    public override IEnumerable<Reward> DroppedRewards
    {
        get
        {
            if (BasicFunction.RandomChance(20))
            {
                return [new EntityReward([new Police()])];
            }

            return [];
        }
    }


    public override BasicAttack BasicAttack => new DoNotResist();
    public override Skill? Skill => new IAmShooting();

    public override void OnBattleEvent(BattleEventArgs eventArgs, Character owner)
    {
        base.OnBattleEvent(eventArgs, owner);

    }

    public override Surge? Surge { get; }
}