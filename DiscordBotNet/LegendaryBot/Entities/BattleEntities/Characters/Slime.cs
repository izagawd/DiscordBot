using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleEvents;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.Rewards;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;



public class GooeyStrike : BasicAttack
{
    
    public override string GetDescription(Character character) => "Slams it's body on the enemy, with a 10% chance to inflict poison";
    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        var damageResult = target.Damage(new DamageArgs(this)
        {
            Caster = owner,
            Damage = owner.Attack * 2,
            DamageText = $"{owner} used a slime attack at {target} and dealt $ damage!"
        });
        if (BasicFunction.RandomChance(10))
        {
            target.StatusEffects.Add(new Poison(owner));
        }
        return new UsageResult(this)
        {
            DamageResults = [damageResult],
            TargetType = TargetType.SingleTarget,
            UsageType = usageType,
            Text = "Sulaimu attacku!",
            User = owner
        };
    }
}
public class Slime : Character
{
    public override IEnumerable<Reward> DroppedRewards
    {
        get
        {
            List<Reward> droppedRewards = [];
            if(BasicFunction.RandomChance(10))
                droppedRewards.Add(new EntityReward([new Slime()]));
            return droppedRewards;
        }
    }

    public override int BaseMaxHealth => 700 + (30 * Level);
    public override int BaseAttack => (80 + (7 * Level));
    public override int BaseDefense => (70 + (3.5 * Level)).Round();
    public override int BaseSpeed => 70;
    public override BasicAttack BasicAttack { get; } = new GooeyStrike();
    public override Skill? Skill => null;
    public override Surge? Surge  => null;

}