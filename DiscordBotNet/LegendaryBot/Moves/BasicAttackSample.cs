using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.Moves;
/// <summary>
/// A sample class for basic attack
/// </summary>
public class BasicAttackSample : BasicAttack
{
    public override string GetDescription(Character character) => "Take that!";
    
    protected  override UsageResult HiddenUtilize(Character owner,Character target, UsageType usageType)
    {
 
        DamageResult? damageResult = target.Damage(       new DamageArgs(this)
        {
            Damage = owner.Attack * 1.7,
            Caster = owner,
            CanCrit = true,
            DamageText = $"{owner} gave" +
                         $" {target} a punch and dealt $ damage!"
        });
        return new UsageResult(this)
        {
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            User = owner,
            DamageResults = [damageResult]
        };
    }
}