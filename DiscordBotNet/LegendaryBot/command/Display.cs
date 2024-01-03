using System.Text;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.command;

[SlashCommandGroup("display", "Display stuff from your inventory")]
public class Display : BaseCommandClass
{
    protected static DiscordButtonComponent Next = new DiscordButtonComponent(ButtonStyle.Success, "next", "NEXT");
    protected static DiscordButtonComponent Previous = new DiscordButtonComponent(ButtonStyle.Primary, "previous", "PREVIOUS");

// Last Button
    protected static DiscordButtonComponent Last = new DiscordButtonComponent(ButtonStyle.Primary, "last", "LAST");

// First Button
    protected static DiscordButtonComponent First = new DiscordButtonComponent(ButtonStyle.Primary, "first", "FIRST");
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
                stringToUse += $"     Blessing Name: {i.Blessing.Name}               Blessing Level: {i.Blessing.Level}";
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
                .WithDescription(displayList[index].Join("\n").Print());
            var messageBuilder = new DiscordMessageBuilder()
                .AddComponents(First,Previous,Next,Last)
                .WithEmbed(embed);
            
            if (message is null)
            {
                await using var response = new DiscordInteractionResponseBuilder(messageBuilder);
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