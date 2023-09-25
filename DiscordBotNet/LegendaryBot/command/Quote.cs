using DiscordBotNet.Database;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.command;
[SlashCommandGroup("Quote", "Post a quote or send a quote")]
public class Quote : BaseCommandClass
{
    [SlashCommand("read", "gets a random quote")]
    public async Task Read(InteractionContext ctx)
    {
        var color = await DatabaseContext.UserData.FindOrCreateSelectAsync(ctx.User.Id, i => i.Color);
        var randomQuote = await DatabaseContext.Quote.Where(i => i.IsPermitted)
            .Include(i => i.UserData).RandomAsync();
        if (randomQuote is null)
        {
            await ctx.CreateResponseAsync("damn");
            return;
        }
        var ownerOfQuote = await ctx.Client.GetUserAsync(randomQuote.UserDataId);
        var quoteDate = randomQuote.DateCreated;
        var embedBuilder = new DiscordEmbedBuilder()
            .WithUser(ownerOfQuote)
            .WithColor(randomQuote.UserData.Color)
            .WithTitle($"{ownerOfQuote.Username}'s quote")
            .WithDescription(randomQuote.QuoteValue)
            .WithFooter($"Date and time created: {quoteDate:MM/dd/yyyy HH:mm:ss}");
        await ctx.CreateResponseAsync(embedBuilder);
    }
    [SlashCommand("write", "creates a quote")]
    public async Task Write(InteractionContext ctx, [Option("text", "the quote")] string text)
    {
        var userData = await DatabaseContext.UserData.FindOrCreateAsync(ctx.User.Id);
        var embedBuilder = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithColor(userData.Color);
        if (text.Length > 1000)
        {
            embedBuilder
                .WithTitle("Hmm")
                .WithDescription("Quote length should be shorter than 1000 characters");

            await ctx.CreateResponseAsync(embedBuilder);
        } else if (text.Length <= 0)
        {
            embedBuilder
                .WithTitle("bruh")
                .WithDescription("Write something in the quote!");
            await ctx.CreateResponseAsync(embedBuilder);
        }
        userData.Quotes.Add(new LegendaryBot.Quote(){QuoteValue = text});
        await DatabaseContext.SaveChangesAsync();
        embedBuilder
            .WithTitle("Success!")
            .WithDescription("Your quote has been saved, and is waiting to be approved!")
            .AddField("Your quote was: ", text);
        await ctx.CreateResponseAsync(embedBuilder);
    }
}