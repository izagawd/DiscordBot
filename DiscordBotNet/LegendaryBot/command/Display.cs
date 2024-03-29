﻿using System.Linq.Expressions;
using System.Text;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.command;

[SlashCommandGroup("display", "Display stuff from your inventory")]
public class Display : GeneralCommandClass
{
    protected static DiscordButtonComponent Next = new DiscordButtonComponent(ButtonStyle.Success, "next", "NEXT");
    protected static DiscordButtonComponent Previous = new DiscordButtonComponent(ButtonStyle.Primary, "previous", "PREVIOUS");

// Last Button
    protected static DiscordButtonComponent Last = new DiscordButtonComponent(ButtonStyle.Primary, "last", "LAST");

    
    
    
    protected static DiscordButtonComponent First = new DiscordButtonComponent(ButtonStyle.Primary, "first", "FIRST");
    
    [SlashCommand("character", "shows the details of a single character you own by their name")]
    public async Task ExecuteDisplayACharacter(InteractionContext ctx,
        [Option("character_name","The name of the character")] string characterName)
    {
        var embedBuilder = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithTitle("Hmm")
            .WithDescription("Invalid id");

        var simplifiedCharacterName = characterName.ToLower().Replace(" ", "");
        Expression<Func<UserData, IEnumerable<Entity>>> navigation = i => i.Inventory.Where(j =>
            EF.Property<string>(j, "Discriminator").ToLower() == simplifiedCharacterName
            && j is Character);
        var userData = await DatabaseContext.UserData
            .Include(navigation)
            .ThenInclude((Entity entity) => (entity as Character).Blessing)
            .Include(navigation)
            .ThenInclude((Entity entity) => (entity as Character).CharacterBuilds)
            .FindOrCreateAsync((long)ctx.User.Id);
        embedBuilder.WithColor(userData.Color);
        var character = userData.Inventory.OfType<Character>().FirstOrDefault(i => 
            i.GetType().Name.ToLower() == simplifiedCharacterName);
        if (character is null)
        {
            embedBuilder.WithDescription($"You do not have any character with the name {characterName}");
            await ctx.CreateResponseAsync(embedBuilder);
            return;
        }

        if (character is Player player) await player.LoadAsync(ctx.User, false);

        
        await using var stream = new MemoryStream();
        await (await character.GetDetailsImageAsync()).SaveAsPngAsync(stream);
        stream.Position = 0;
        embedBuilder.WithImageUrl("attachment://description.png");
        var descriptionString = $"Name: {character}";
        if (character.Blessing is not null)
            descriptionString += $"\nBlessing Id: {character.Blessing.Id}";
        embedBuilder
            .WithTitle("Here you go!")
            .WithDescription(descriptionString);
        DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder()
            .AddFile("description.png", stream)
            .WithTitle("Detail")
            .AddEmbed(embedBuilder.Build());
        await ctx.CreateResponseAsync(builder);
   
    }
    
    [SlashCommand("characters", "display all your owned characters")]
    public async Task ExecuteDisplayCharacters(InteractionContext context)
    {
        var userData = await DatabaseContext.UserData
            .Include(i => i.Inventory.Where(j => j is Character))
            .ThenInclude((Entity i) => (i as Character).Blessing)
            .FindOrCreateAsync((long)context.User.Id);

        List<List<string>> displayList = [];
        var count = 0;
        List<string> currentList = [];
        
        displayList.Add(currentList);
        var displaySectionLimit = 10;
        foreach (var i in userData.Inventory.OfType<Character>())
        {
            if (i is Player player)
                await player.LoadAsync(context.User, false);
            if (count >= displaySectionLimit)
            {
                currentList = new List<string>();
                displayList.Add(currentList);
                count = 0;
            }

            var stringToUse = $"Name: {i.Name};     Level: {i.Level}";
            if (i.Blessing is not null)
                stringToUse += $"\n     Blessing Name: {i.Blessing.Name}               Blessing Level: {i.Blessing.Level}\n" +
                               $"Blessing Id: {i.Blessing.Id}";
            currentList.Add(stringToUse);
            count++;
        }

        var index = 0;

        DiscordMessage? message = null;

    
        while (true)
        {
        
            var embed = new DiscordEmbedBuilder()
                .WithUser(context.User)
                .WithColor(userData.Color)
                .WithTitle($"Page {index + 1}")
                .WithDescription(displayList[index].Join("\n\n"));
            var messageBuilder = new DiscordMessageBuilder()
                .AddComponents(First,Previous,Next,Last)
                .WithEmbed(embed);
            
            if (message is null)
            {
                var response = new DiscordInteractionResponseBuilder(messageBuilder);
                await context.CreateResponseAsync(response);
                message = await context.GetOriginalResponseAsync();
            }
            else
            {
                message = await message.ModifyAsync(messageBuilder);
            }

            var result = await message.WaitForButtonAsync(context.User);
            
            if(result.TimedOut) break;
            await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            switch (result.Result.Id.ToLower())
            {
                case "next":
                    index++;
                    break;
                case "previous":
                    index--;
                    break;
                case "last":
                    index = displayList.Count - 1;
                    break;
                case "first":
                    index = 0;
                    break;
            }

            if (index < 0) index = 0;
            if (index > displayList.Count - 1) index = displayList.Count - 1;
        }
        

    }
    [SlashCommand("blessing", "shows the details of a single blessing you own by their id")]
    public async Task ExecuteDisplayABlessing(InteractionContext ctx,
        [Option("blessing_id","The id of the blessing you want to get details about")] long blessingId)
    {
        var embedBuilder = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithTitle("Hmm")
            .WithDescription("Invalid id");

  
        var userData = await DatabaseContext.UserData
            .Include(i => i.Inventory.Where(j => j.Id == blessingId && j is Blessing))
            .ThenInclude((Entity entity) => (entity as Blessing).Character)
            .FindOrCreateAsync((long)ctx.User.Id);
        embedBuilder.WithColor(userData.Color);
        var blessing = userData.Inventory.OfType<Blessing>().FirstOrDefault(i => i.Id == blessingId);
        if (blessing is null)
        {
            embedBuilder.WithDescription($"You do not have any blessing with the id {blessingId}");
            await ctx.CreateResponseAsync(embedBuilder);
            return;
        }

        

        
        await using var stream = new MemoryStream();
        await (await blessing.GetDetailsImageAsync()).SaveAsPngAsync(stream);
        stream.Position = 0;
        embedBuilder.WithImageUrl("attachment://description.png");

        embedBuilder
            .WithTitle("Here you go!")
            .WithDescription($"Name: {blessing}\nId: {blessing.Id}");
        DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder()
            .AddFile("description.png", stream)
            .WithTitle("Detail")
            .AddEmbed(embedBuilder.Build());
        await ctx.CreateResponseAsync(builder);
   
    }
    [SlashCommand("blessings", "display all your owned blessings")]
    public async Task ExecuteDisplayBlessings(InteractionContext context)
    {
        var userData = await DatabaseContext.UserData
            .Include(i => i.Inventory.Where(j => j is Blessing))
            .ThenInclude((Entity i) => (i as Blessing).Character)
            .FindOrCreateAsync((long)context.User.Id);

        List<List<string>> displayList = [];
        var count = 0;
        List<string> currentList = [];
        
        displayList.Add(currentList);
        var displaySectionLimit = 10;
        foreach (var i in userData.Inventory.OfType<Blessing>())
        {
            if (count >= displaySectionLimit)
            {
                currentList = new List<string>();
                displayList.Add(currentList);
                count = 0;
            }
            var stringToUse = $"Name: {i.Name};     Level: {i.Level};       Id: {i.Id}";
            if (i.Character is not null)
            {
                if (i.Character is Player player) await player.LoadAsync(context.User, false);
                stringToUse += $"\n     Character Name: {i.Character.Name}               Character Level: {i.Character.Level}";
            }

  
            currentList.Add(stringToUse);
            count++;
        }

        var index = 0;

        DiscordMessage? message = null;

    
        while (true)
        {
        
            var embed = new DiscordEmbedBuilder()
                .WithUser(context.User)
                .WithColor(userData.Color)
                .WithTitle($"Page {index + 1}")
                .WithDescription(displayList[index].Join("\n\n"));
            var messageBuilder = new DiscordMessageBuilder()
                .AddComponents(First,Previous,Next,Last)
                .WithEmbed(embed);
            
            if (message is null)
            {
                var response = new DiscordInteractionResponseBuilder(messageBuilder);
                await context.CreateResponseAsync(response);
                message = await context.GetOriginalResponseAsync();
            }
            else
            {
                message = await message.ModifyAsync(messageBuilder);
            }

            var result = await message.WaitForButtonAsync(context.User);
            
            if(result.TimedOut) break;
            await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            switch (result.Result.Id.ToLower())
            {
                case "next":
                    index++;
                    break;
                case "previous":
                    index--;
                    break;
                case "last":
                    index = displayList.Count - 1;
                    break;
                case "first":
                    index = 0;
                    break;
            }

            if (index < 0) index = 0;
            if (index > displayList.Count - 1) index = displayList.Count - 1;
        }
        

    }
    [SlashCommand("teams","displays all your teams")]
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

}