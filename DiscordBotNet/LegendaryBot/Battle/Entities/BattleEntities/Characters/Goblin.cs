using DiscordBotNet.LegendaryBot.Battle.Moves;
using DiscordBotNet.LegendaryBot.Battle.Results;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

public class SneakyAmbush : Surge
{
    public override string GetDescription(int moveLevel)
    {
        return "Attacks the enemy! this surge cannot be countered";
    }

    public override int GetMaxCooldown(int level) => 3;
    public override int MaxEnhance { get; } = 4;
    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team && !i.IsDead);
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {

        var damageResult = target.Damage(        new DamageArgs()
        {
            Damage = owner.Attack * 1.871,
            Caster = owner,
            CanCrit = true,
            DamageText =$"{owner} attacks {target} in a sneaky way that cannot be countered and dealt $ damage!",
            CanBeCountered = false
        });
        return new UsageResult(usageType, TargetType.SingleTarget)
        {
            DamageResults = new List<DamageResult> { damageResult },
            User = owner,
        };
    }
}


public class Goblin :Character
{
    public override Rarity Rarity { get; protected set; } = Rarity.TwoStar;
    public override int BaseMaxHealth => 900 + (Level / 6.0 * 200).Round();
    public override int BaseAttack => ((336 + (67.2 * Level / 6.0)) / 1.2).Round();
    public override int BaseDefense => (336 + (67.2 * Level / 6.0)).Round();
    public override int BaseSpeed => 90;
    public override Surge Surge { get; protected set; } = new SneakyAmbush();


}