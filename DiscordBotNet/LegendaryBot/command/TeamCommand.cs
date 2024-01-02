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
            var equipped = "";
            if (userData.EquippedPlayerTeam == i)
                equipped = " (equipped)";
            teamStringBuilder.Append($"1.{equipped} {i.TeamName}. Members: ");
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



    [SlashCommand("equip_team", "Changes your equipped team")]
    public async Task ExecuteEquip(InteractionContext context,
        [Option("team_name", "the name of the team")] string teamName)
    {
        var anon = await DatabaseContext.UserData
            .FindOrCreateSelectAsync((long)context.User.Id,
                i => new { team = i.PlayerTeams.FirstOrDefault(j => j.TeamName.ToLower() == teamName.ToLower()), userData = i });

        var userData = anon.userData;
        var embed = new DiscordEmbedBuilder()
            .WithTitle("Hmm")
            .WithDescription("Team does not seem to exist");
        if (anon.team is null)
        {
            await context.CreateResponseAsync(embed);
            return;
        }

        userData.EquippedPlayerTeam = anon.team;
        await DatabaseContext.SaveChangesAsync();
        embed.WithTitle("Success!")
            .WithDescription($"Team {anon.team.TeamName} is now equipped!");
        await context.CreateResponseAsync(embed);

    }

    [SlashCommand("remove_character", "removes a character from a team")]
    public async Task ExecuteRemoveFromTeam(InteractionContext context,
        [Option("character_name", "The name of the character")] string characterName,
        [Option("team_name", "the name of the team.")]
        string teamName)
    {
    

        var simplifiedCharacterName = characterName.ToLower().Replace(" ", "");
        var userData = await DatabaseContext.UserData
            .Include(i => i.PlayerTeams.Where(j => j.TeamName.ToLower() == teamName.ToLower()))
            .Include(i => i.EquippedPlayerTeam)
            .Include(i => i.Inventory.Where(i => i is Character
                                                 && EF.Property<string>(i, "Discriminator").ToLower() == simplifiedCharacterName))
            .FindOrCreateAsync((long)context.User.Id);
        PlayerTeam? gottenTeam =  userData.PlayerTeams.FirstOrDefault(i => i.TeamName.ToLower() == teamName.ToLower());
        

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

    [SlashCommand("rename_team", "renames team")]

    
    public async Task ExecuteRenameTeam(InteractionContext context,
        [Option("team_name", "the name of the team you want to rename")]
        string teamName,
        [Option("new_name", "The new name of the team you want to remain. ")]
        string newName)
    {
        var userData = await DatabaseContext.UserData
            .Include(i => i.PlayerTeams)
            .FindOrCreateAsync((long)context.User.Id);

        var embed = new DiscordEmbedBuilder()
            .WithColor(userData.Color)
            .WithUser(context.User)
            .WithTitle("Hmm")
            .WithDescription($"You do not have a team with name {teamName}");
        var team = userData.PlayerTeams.FirstOrDefault(i => i.TeamName.ToLower() == teamName);
        if (team is null)
        {
            await context.CreateResponseAsync(embed);
            return;
        }

        if (userData.PlayerTeams.Except([team]).Any(i => i.TeamName.ToLower() == newName.ToLower()))
        {
            embed.WithDescription($"You already have a team with the name {newName}");
            await context.CreateResponseAsync(embed);
            return;
        }

        team.TeamName = newName;
        await DatabaseContext.SaveChangesAsync();
        embed.WithTitle("Success!")
            .WithDescription($"Team {teamName} is now {newName!}");

        await context.CreateResponseAsync(embed);

    }
    [SlashCommand("add_character", "adds a character to a team")]
    public async Task ExecuteAddToTeam(InteractionContext context, [Option("character_name","The name of the character")] string characterName,
        [Option("team_name","the name of the team.")] string teamName)
    {


        var simplifiedCharacterName = characterName.ToLower().Replace(" ", "");
        var userData = await DatabaseContext.UserData
            .Include(i => i.PlayerTeams.Where(j => j.TeamName.ToLower() == teamName.ToLower()))
            .Include(i => i.EquippedPlayerTeam)
            .Include(i => i.Inventory.Where(i => i is Character
                                                 && EF.Property<string>(i, "Discriminator").ToLower() == simplifiedCharacterName))
            .FindOrCreateAsync((long)context.User.Id);
        PlayerTeam? gottenTeam = userData.PlayerTeams.FirstOrDefault(i => i.TeamName.ToLower() == teamName.ToLower());
        

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

        gottenTeam.Add(character);
        await DatabaseContext.SaveChangesAsync();
        if (character is Player player) await player.LoadAsync(context.User, false);
        embed.WithTitle("Success!").WithDescription($"{character} has been added to team {gottenTeam.TeamName}!");
        await context.CreateResponseAsync(embed);

    }
}