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
            ElementToDamageWith = owner.Element,
            CriticalChance = owner.CriticalChance,
            CriticalDamage = owner.CriticalDamage,
            Damage = owner.Attack * 1.7f,
            Caster = owner,
            DamageText = $"{owner.NameWithAlphabetIdentifier} tases {target.NameWithAlphabetIdentifier} and dealt $ damage! it was shocking"
        });
        if (BasicFunctionality.RandomChance(15))
        {
            target.AddStatusEffect(new Stun(owner){Duration = 1}, owner.Effectiveness);
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
            ElementToDamageWith = owner.Element,
            CriticalChance = owner.CriticalChance,
            CriticalDamage = owner.CriticalDamage,
            Caster = owner,
            Damage = owner.Attack * 2,
            DamageText = $"{owner.NameWithAlphabetIdentifier} shoots at {target.NameWithAlphabetIdentifier} for resisting arrest, dealing $ damage"
        });
        foreach (var _ in Enumerable.Range(0,2))
        {
            target.AddStatusEffect(new Bleed(owner), owner.Effectiveness);
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

    public override int MaxCooldown => 3;
}
public class Police : Character
{
    public override Rarity Rarity { get; protected set; } = Rarity.TwoStar;

    public override IEnumerable<Reward> DroppedRewards
    {
        get
        {
            if (BasicFunctionality.RandomChance(20))
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