using System.Text;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.command;
[SlashCommandGroup("team","teams related command")]
public class TeamCommand : BaseCommandClass
{
    
    
    
    
    
    
    [SlashCommand("display","displays all your teams")]
    public async Task ExecuteDisplayTeams(InteractionContext context)
    {
        var userData = await DatabaseContext.UserData
            .Include(i => i.PlayerTeams)
            .FindOrCreateAsync((long)context.User.Id);

        var teamStringBuilder = new StringBuilder();
        var count = 0;

        foreach (var i in userData.PlayerTeams.SelectMany(i => i).OfType<Player>())
        {
            await i.LoadAsync(context.User,false);
        }
  
        foreach (var i in userData.PlayerTeams)
        {
            count++;
            teamStringBuilder.Append($"1. {i.TeamName}. Members: ");
            foreach (var j in i)
            {
                teamStringBuilder.Append($"{j}, ");
            }

            teamStringBuilder.Append("\n");
        }

        var embed = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithTitle("Here's a list of all your teams")
            .WithColor(userData.Color)
            .WithDescription(teamStringBuilder.ToString());

        await context.CreateResponseAsync(embed);
    }





    [SlashCommand("remove_character", "removes a character from a team")]
    public async Task ExecuteRemoveFromTeam(InteractionContext context,
        [Option("character_name", "The name of the character")] string characterName,
        [Option("team_name", "the name of the team. if not set, it will used the currently equipped team")]
        string? teamName = null)
    {
        if (teamName is null) teamName = "";

        var simplifiedCharacterName = characterName.ToLower().Replace(" ", "");
        var userData = await DatabaseContext.UserData
            .Include(i => i.PlayerTeams.Where(j => j.TeamName.ToLower() == teamName.ToLower()))
            .Include(i => i.EquippedPlayerTeam)
            .Include(i => i.Inventory.Where(i => i is Character
                                                 && EF.Property<string>(i, "Discriminator").ToLower() == simplifiedCharacterName))
            .FindOrCreateAsync((long)context.User.Id);
        PlayerTeam? gottenTeam = null;
        if (teamName == "")
        {
            gottenTeam = userData.EquippedPlayerTeam;
        }
        else
        {
            gottenTeam = userData.PlayerTeams.FirstOrDefault(i => i.TeamName.ToLower() == teamName.ToLower());
        }

        var embed = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithColor(userData.Color)
            .WithTitle("Hmm")
            .WithDescription($"Team with name {teamName} does not exist");
        if (gottenTeam is null)
        {
            await context.CreateResponseAsync(embed);
            return;
        }

        if (gottenTeam.Count <= 1)
        {
            embed.WithDescription("There should be at least one character in a team");
            await context.CreateResponseAsync(embed);
            return;
        }
        var character = userData.Inventory.OfType<Character>()
            .Where(i => i.GetType().Name.ToLower() == simplifiedCharacterName)
            .FirstOrDefault();
        if (character is null)
        {
            embed.WithDescription($"Character with name {characterName} could not be found");
            await context.CreateResponseAsync(embed);
            return;
        }

        if (character is Player player)
            await player.LoadAsync(context.User,false);
        if (!gottenTeam.Contains(character))
        {
            embed.WithDescription($"Character {character} is not in team {gottenTeam.TeamName}");
            return;
        }

        gottenTeam.Remove(character);
        
        await DatabaseContext.SaveChangesAsync();

        embed.WithTitle("Success!").WithDescription($"{character} has been removed from team {gottenTeam.TeamName}!");
        await context.CreateResponseAsync(embed);

    }

    [SlashCommand("add_character","adds a character to a team")]
    public async Task ExecuteAddToTeam(InteractionContext context, [Option("character_name","The name of the character")] string characterName,
        [Option("team_name","the name of the team. if not set, it will used the currently equipped team")] string? teamName = null)
    {
        if (teamName is null) teamName = "";

        var simplifiedCharacterName = characterName.ToLower().Replace(" ", "");
        var userData = await DatabaseContext.UserData
            .Include(i => i.PlayerTeams.Where(j => j.TeamName.ToLower() == teamName.ToLower()))
            .Include(i => i.EquippedPlayerTeam)
            .Include(i => i.Inventory.Where(i => i is Character
                                                 && EF.Property<string>(i, "Discriminator").ToLower() == simplifiedCharacterName))
            .FindOrCreateAsync((long)context.User.Id);
        PlayerTeam? gottenTeam = null;
        if (teamName == "")
        {
            gottenTeam = userData.EquippedPlayerTeam;
        }
        else
        {
            gottenTeam = userData.PlayerTeams.FirstOrDefault(i => i.TeamName.ToLower() == teamName.ToLower());
        }

        var embed = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithColor(userData.Color)
            .WithTitle("Hmm")
            .WithDescription($"Team with name {teamName} does not exist");
        if (gottenTeam is null)
        {
            await context.CreateResponseAsync(embed);
            return;
        }

        if (gottenTeam.IsFull)
        {
            embed.WithDescription("The provided team is full");
            await context.CreateResponseAsync(embed);
        }
        var character = userData.Inventory.OfType<Character>()
            .Where(i => i.GetType().Name.ToLower() == simplifiedCharacterName)
            .FirstOrDefault();
        if (character is null)
        {
            embed.WithDescription($"Character with name {characterName} could not be found");
            await context.CreateResponseAsync(embed);
            return;
        }

        gottenTeam.Add(character);
        await DatabaseContext.SaveChangesAsync();
        if (character is Player player) await player.LoadAsync(context.User, false);
        embed.WithTitle("Success!").WithDescription($"{character} has been added to team {gottenTeam.TeamName}!");
        await context.CreateResponseAsync(embed);

    }
}