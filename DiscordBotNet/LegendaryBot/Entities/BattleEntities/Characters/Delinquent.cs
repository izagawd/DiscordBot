using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;



public class BaseballBatWhack : BasicAttack
{
    public override string GetDescription(Character character)
    {
        return "Swings a baseball bat at the enemy, causing solid  damage";
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
                    Damage = owner.Attack * 1.7,
                    Caster = owner,
                    DamageText = $"{owner.NameWithAlphabetIdentifier} whacks {target.NameWithAlphabetIdentifier} with a baseball bat, dealing $ damage"
                })
            ],
            User = owner
        };
    }
}

public class Delinquent : Character
{
    public override BasicAttack BasicAttack { get; } = new BaseballBatWhack();
    public override Skill? Skill { get; }
    public override Surge? Surge { get; }
}