﻿using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;



public class ThugPunch : BasicAttack
{
    public override string GetDescription(Character character)
    {
        return "Punches the enemy in a thug way";
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
                    DamageText = $"{User.NameWithAlphabetIdentifier} punches {target.NameWithAlphabetIdentifier} in a thug way!"
                })
            ],
            User = User
        };
    }
}

public class Thug : Character
{
    public override BasicAttack BasicAttack { get; } = new ThugPunch();
    public override Skill? Skill { get;  }
    public override Surge? Surge { get; }
}