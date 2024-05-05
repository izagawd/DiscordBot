using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;



public class ThugPunch : BasicAttack
{
    public override string GetDescription(Character character)
    {
        return "Punches the enemy in a thug way";
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        return new UsageResult(this)
        {
            TargetType = TargetType.SingleTarget,
            UsageType = usageType,
            Text = "Uraaah!",
            DamageResults =
            [
                target.Damage(new DamageArgs(this)
                {
                    ElementToDamageWith = owner.Element,
                    CriticalChance = owner.CriticalChance,
                    CriticalDamage = owner.CriticalDamage,
                    Damage = owner.Attack * 1.7f,
                    Caster = owner,
                    DamageText = $"{owner.NameWithAlphabetIdentifier} punches {target.NameWithAlphabetIdentifier} in a thug way!"
                })
            ],
            User = owner
        };
    }
}

public class Thug : Character
{
    public override BasicAttack BasicAttack { get; } = new ThugPunch();
    public override Skill? Skill { get;  }
    public override Surge? Surge { get; }
}