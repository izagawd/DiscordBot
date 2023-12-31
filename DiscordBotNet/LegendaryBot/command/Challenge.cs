
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.Rewards;
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
        var player1Team = await player1User.EquippedPlayerTeam.LoadAsync(player1);
        var player2Team = await player2User.EquippedPlayerTeam.LoadAsync(player2);
        var simulator = new BattleSimulator(player1Team,player2Team);
        var battleResult = await simulator.StartAsync(ctx,message);
        DiscordUser winnerDiscord;
        UserData winnerUserData;
        if (battleResult.Winners.TryGetUserDataId == player1.Id)
        {
            winnerDiscord = player1;
            winnerUserData = player1User;
        }
        else
        {
            winnerDiscord = player2;
            winnerUserData = player2User;
        }


        await DatabaseContext.SaveChangesAsync();
        await message.ModifyAsync(new DiscordMessageBuilder()
        {
            Embed = new DiscordEmbedBuilder()
                .WithColor(winnerUserData.Color)
                .WithTitle("Battle Ended")
                .WithDescription($"{winnerDiscord.Username} won the battle! ")
        });
    }
}