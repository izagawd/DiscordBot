﻿using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Rewards;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Results;

public class BattleResult
{
    public required DiscordMessage Message { get; init; }
    public IEnumerable<Reward> BattleRewards { get; init; } = [];

    public required CharacterTeam Winners { get; init; }

    public required int Turns { get; init; }
    public required CharacterTeam? TimedOut { get; init; } 
    public  CharacterTeam? Forfeited { get; init; }
}