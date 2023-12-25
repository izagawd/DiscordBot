using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;



public class GooeyStrike : BasicAttack
{
    public override string Description => "Slams it's body on the enemy";
    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        var damageResult=target.Damage(new DamageArgs(this)
        {
            Caster = owner,
            Damage = 5
        });
        owner.CurrentBattle.AdditionalTexts.Add($"{owner} used a slime attack at {target}!");
        return new UsageResult(this)
        {
            DamageResults = [damageResult],
            TargetType = TargetType.SingleTarget,
            UsageType = usageType,
            Text = "Slime attacku!",
            User = owner
        };
    }
}
public class Slime : Character
{
    public override BasicAttack BasicAttack { get; } = new GooeyStrike();
    public override Skill? Skill => null;
    public override Surge? Surge  => null;
}