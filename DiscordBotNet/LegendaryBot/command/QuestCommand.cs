using DiscordBotNet.Database;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Quests;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.command;

public class QuestCommand : BaseCommandClass
{
    [SlashCommand("quest", "do a daily quest")]
    public async Task Execute(InteractionContext ctx)
    {
        var author = ctx.User;

        await PostgreSqlContext.CheckForNewDayAsync(author.Id);
        var userData = await DatabaseContext.UserData
            .Include(i => i.Quests)
            .FindOrCreateAsync(author.Id);
        

        var questString = "";
        var embed = new DiscordEmbedBuilder()
            .WithUser(author)
            .WithColor(userData.Color)
            .WithTitle("Hmm")
            .WithDescription("you have no quests");
        if (userData.Quests.Count <= 0)
        {
           await ctx.CreateResponseAsync(embed);
           return;
        }

        var count = 1;
        foreach (var i in userData.Quests)
        {
            var preparedString = $"{count}. {i.Title}: {i.Description}.";
            if (i.Completed)
                preparedString += " Completed.";
            questString += $"{preparedString}\n";
            count++;
        }

        var questsShouldDisable = userData.Quests.Select(i => i.Completed).ToList();
        while(questsShouldDisable.Count < 4)
            questsShouldDisable.Add(true);
        DiscordButtonComponent One = new DiscordButtonComponent(ButtonStyle.Primary, "1", "1",
            questsShouldDisable[0]);
        DiscordButtonComponent Two = new DiscordButtonComponent(ButtonStyle.Primary, "2", "2",questsShouldDisable[1]);
        DiscordButtonComponent Three = new DiscordButtonComponent(ButtonStyle.Primary, "3", "3",questsShouldDisable[2]);
        DiscordButtonComponent Four = new DiscordButtonComponent(ButtonStyle.Primary, "4", "4",questsShouldDisable[3]);
        embed
            .WithTitle("These are your available quests")
            .WithDescription(questString);
        await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder()
            .AddComponents([One, Two, Three, Four])
            .AddEmbed(embed));
        IEnumerable<string> possibleIds = ["1", "2", "3", "4"];
        var message =await  ctx.GetOriginalResponseAsync();
        
        Quest quest = null;
        await message.WaitForButtonAsync(i =>
        {
            if (i.User.Id != userData.Id) return false;
            if (!possibleIds.Contains(i.Id)) return false;
            quest = userData.Quests[int.Parse(i.Id) -1];
            return true;
        });
        if (quest is null)
        {
            return;
        }

        var succeeded = await quest.StartQuest(ctx, message);
        
        if (succeeded)
        {
            quest.Completed = true;
            var rewardString = userData.ReceiveRewards(ctx.User.Username, quest.GetQuestRewards());
            embed
                .WithTitle("Nice!!")
                .WithDescription("You completed the quest!\n" +rewardString);
            await DatabaseContext.SaveChangesAsync();
            await message.ModifyAsync(new DiscordMessageBuilder() { Embed = embed });
            return;
        }
        embed
            .WithTitle("Damn")
            .WithDescription("You failed");
        await message.ModifyAsync(new DiscordMessageBuilder() { Embed = embed });
       
    }
}