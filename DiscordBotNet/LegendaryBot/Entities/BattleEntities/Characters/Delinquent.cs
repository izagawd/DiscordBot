﻿using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;



public class BaseballBatWhack : BasicAttack
{
    public override string GetDescription(Character character)
    {
        return "Swings a baseball bat at the enemy, causing solid  damage";
    }

    protected override UsageResult HiddenUtilize(Character target, UsageType usageType)
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
                    ElementToDamageWith = User.Element,
                    CriticalChance = User.CriticalChance,
                    CriticalDamage = User.CriticalDamage,
                    Damage = User.Attack * 1.7f,
                    Caster = User,
                    DamageText = $"{User.NameWithAlphabetIdentifier} whacks {target.NameWithAlphabetIdentifier} with a baseball bat, dealing $ damage"
                })
            ],
            User = User
        };
    }
}

public class Delinquent : Character
{
    public override BasicAttack BasicAttack { get; } = new BaseballBatWhack();
    public override Skill? Skill { get; }
    public override Surge? Surge { get; }
}