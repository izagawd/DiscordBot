﻿using DiscordBotNet.Extensions;
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
            Damage = owner.Attack * 1.7,
            DamageText = $"{owner.NameWithAlphabetIdentifier} used a slime attack at {target.NameWithAlphabetIdentifier} and dealt $ damage!"
        });
        if (BasicFunctionality.RandomChance(10))
        {
            target.AddStatusEffect(new Poison(owner));
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
    public override float GetSpeedValue(int points)
    {
        return (base.GetSpeedValue(points) * 0.8).Round();
    }

    public override float GetAttackValue(int points)
    {
        return (base.GetAttackValue(points) * 0.7).Round();
    }
    
    public override IEnumerable<Reward> DroppedRewards
    {
        get
        {
            List<Reward> droppedRewards = [];
            if(BasicFunctionality.RandomChance(10))
                droppedRewards.Add(new EntityReward([new Slime()]));
            return droppedRewards;
        }
    }

    public override Rarity Rarity { get; protected set; } = Rarity.TwoStar;

    public override BasicAttack BasicAttack { get; } = new GooeyStrike();
    public override Skill? Skill => null;
    public override Surge? Surge  => null;

}