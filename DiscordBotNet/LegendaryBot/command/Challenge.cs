
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Battle;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Results;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.command;

public class Challenge :BaseCommandClass
{   
    public override BotCommandType BotCommandType { get; } = BotCommandType.Adventure;
    private static readonly DiscordButtonComponent yes = new(ButtonStyle.Primary, "yes", "YES");
    private static readonly DiscordButtonComponent no = new(ButtonStyle.Primary, "no", "NO");

    [SlashCommand("challenge", "Challenge other players to a duel!")]
    public async Task Execute(InteractionContext ctx,[Option("user", "User to challenge")] DiscordUser opponent)
    {
        
        var player1 = ctx.User;
        var player2 = opponent;

        var player1User = await DatabaseContext.UserData
            .IncludeTeamWithAllEquipments()
            .FindOrCreateAsync(player1.Id);
        DiscordEmbedBuilder embedToBuild;
        if (player1User.IsOccupied)
        {
            embedToBuild = new DiscordEmbedBuilder()
                .WithTitle($"Hmm")
                .WithColor(player1User.Color)
                .WithAuthor(player1.Username, iconUrl: player1.AvatarUrl)
                .WithDescription("You are occupied");
            await ctx.CreateResponseAsync(embedToBuild.Build());
            return;
        }
        embedToBuild = new DiscordEmbedBuilder()
            .WithTitle($"Hmm")
            .WithColor(player1User.Color)
            .WithAuthor(player1.Username, iconUrl: player1.AvatarUrl)
            .WithDescription("You cannot fight yourself");

        if (player1.Id == player2.Id)
        {
            await ctx.CreateResponseAsync(embedToBuild.Build());
            return;
            
        }

        var player2User = await DatabaseContext.UserData
            .IncludeTeamWithAllEquipments()
            .Include(i => i.Inventory.Where(i => i is Character))
            .FindOrCreateAsync(player2.Id);
        if (player2User.IsOccupied)
        {
            embedToBuild = new DiscordEmbedBuilder()
                .WithTitle($"Hmm")
                .WithColor(player1User.Color)
                .WithAuthor(player1.Username, iconUrl: player1.AvatarUrl)
                .WithDescription($"{player2.Username} is occupied");
            await ctx.CreateResponseAsync(embedToBuild.Build());
            return;
        }
        if (player2User.Tier == Tier.Unranked || player1User.Tier == Tier.Unranked)
        {
            embedToBuild = embedToBuild
                .WithTitle($"Hmm")
                .WithDescription("One of you have not begun your journey with /begin");
            await ctx.CreateResponseAsync(embedToBuild.Build());
            return;
            
        }
        embedToBuild = embedToBuild
            .WithTitle($"{player2.Username}, ")
            
            .WithDescription($"`do you accept {player1.Username}'s challenge?`");
        await MakeOccupiedAsync(player1User);
        await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
            .AddEmbed(embedToBuild.Build())
            .AddComponents(yes,no));
        var message = await ctx.GetOriginalResponseAsync();
        string? decision = null;
        await message.WaitForButtonAsync(i =>
        {
            if (i.User.Id == player2.Id)
            {
                decision = i.Interaction.Data.CustomId;
                i.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                return true;
            }

            return false;
        });
        if (decision == "no")
        {
            await message.ModifyAsync(new DiscordMessageBuilder().WithEmbed(embedToBuild.WithTitle($"Hmm")

                .WithDescription("Battle request declined")
                
                .Build())
                );
            return;
            
        }

        await MakeOccupiedAsync(player2User);
        var simulator = new BattleSimulator(await player1User.GetCharacterTeam(player1).LoadAsync(player1), await player2User.GetCharacterTeam(player2).LoadAsync(player2));
        BattleResult battleResult = await simulator.StartAsync(ctx.Interaction,message);
        DiscordUser winnerDiscord;
        UserData winnerUserData;
        if (battleResult.Winners.UserId == player1.Id)
        {
            winnerDiscord = player1;
            winnerUserData = player1User;
        }
        else
        {
            winnerDiscord = player2;
            winnerUserData = player2User;
        }

        var player2Team = player2User.GetCharacterTeam(player2);
        var player1Team = player1User.GetCharacterTeam(player1);
        var expToGainForUser1 = BattleFunction.ExpGainFormula((int)player2Team.Average(i => i.Level));
        var expToGainForUser2 = BattleFunction.ExpGainFormula((int)player1Team.Average(i => i.Level));
        if (winnerUserData != player1User)
        {
            expToGainForUser2 /= 2;
        }
        else
        {
            expToGainForUser1 /= 2;
        }

        var player1LevelUpString = string.Join("\n", player1Team.Select(i => i.IncreaseExp(expToGainForUser1).Text));
        var player2LevelUpString = string.Join("\n", player2Team.Select(i => i.IncreaseExp(expToGainForUser2).Text));
        await DatabaseContext.SaveChangesAsync();
        await message.ModifyAsync(new DiscordMessageBuilder()
        {
            Embed = new DiscordEmbedBuilder()
                .WithColor(winnerUserData.Color)
                .WithTitle("Battle Ended")
                .WithDescription($"{winnerDiscord.Username} won the battle! ")
                .AddField($"{player1.Username}'s characters gained: ",player1LevelUpString)
                .AddField($"{player2.Username}'s characters gained: ",player2LevelUpString)
        });
    }
}