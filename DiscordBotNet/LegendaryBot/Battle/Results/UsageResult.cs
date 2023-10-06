﻿using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Moves;
using DiscordBotNet.LegendaryBot.Battle.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Battle.Results;

public enum TargetType
{
    None, SingleTarget, AOE, InBetween, 
}
public class UsageResult
{
    /// <summary>
    /// If not null, this text will be used as the main text
    /// </summary>
    public string? Text { get; init; }
    /// <summary>
    /// If the usage is an attack
    /// </summary>
    public bool IsAttackUsage => DamageResults.Any();
    /// <summary>
    /// If the usage is not an attack
    /// </summary>
    public bool IsNonAttackUsage => !IsAttackUsage;
    public required TargetType TargetType { get; init; }

 

    /// <summary>
    /// if the skill deals any sort of damage, the damage results should be in this list
    /// </summary>
    private readonly DamageResult[] _damageResults = Array.Empty<DamageResult>();
    public IEnumerable<DamageResult> DamageResults
    {
        get => _damageResults;
        init => _damageResults = value.ToArray();
    } 
    /// <summary>
    /// The character who used it, or if it is a status effect, the character in which the status effect affected
    /// </summary>
    public required Character User { get; init; }

    /// <summary>
    /// Determines if the usage was from a normal skill use or a follow up use.  this must be set
    /// </summary>
    public required UsageType UsageType { get; init; }
    /// <summary>
    /// The move used to execute this skill
    /// </summary>
    public  Move? MoveUsed { get; private set; }
    public StatusEffect? StatusEffectUsed { get; private set; }

    public UsageResult(Move moveUsed)
    {
        MoveUsed = moveUsed;
    }

    public UsageResult(StatusEffect statusEffectUsed)
    {
        StatusEffectUsed = statusEffectUsed;
    }
}