using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Results;

namespace DiscordBotNet.LegendaryBot.Battle.Moves;
/// <summary>
/// A sample class for basic attack
/// </summary>
public class BasicAttackSample : BasicAttack
{
    public override string Description => "Take that!";
    
    protected  override UsageResult HiddenUtilize(Character owner,Character target, UsageType usageType)
    {
 
        DamageResult damageResult = target.Damage(       new DamageArgs(this)
        {
            Damage = owner.Attack * 1.871,
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
            DamageResults = new[] { damageResult }
        };
    }
}