using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Results;

namespace DiscordBotNet.LegendaryBot.Battle.Moves;

public class BasicAttack : Move
{
    public sealed override MoveType MoveType => MoveType.BasicAttack;
    public sealed override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team && !i.IsDead);
    }

    public sealed override UsageResult Utilize(Character owner, Character target, UsageType usageType)
    {
        return base.Utilize(owner, target, usageType);
    }

    protected  override UsageResult HiddenUtilize(Character owner,Character target, UsageType usageType)
    {
 
        DamageResult damageResult = target.Damage(       new DamageArgs()
        {
            Damage = owner.Attack * 1.871,
            Caster = owner,
            CanCrit = true,
            DamageText = $"{owner} gave" +
                         $" {target} a punch and dealt $ damage!"
        });
        return new UsageResult(usageType) { DamageResults = new List<DamageResult> { damageResult } };
    }
}