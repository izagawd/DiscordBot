
using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DSharpPlus;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;


namespace DiscordBotNet.LegendaryBot.command;
[SlashCommandGroup("Quote", "Post a quote or send a quote")]
public class Quote : BaseCommandClass
{
  
    [SlashCommand("read", "gets a random quote")]
    public async Task Read(InteractionContext ctx)
    {

        var color = await DatabaseContext.UserData.FindOrCreateSelectAsync(ctx.User.Id, i => i.Color);
        LegendaryBot.Quote randomQuote = (await DatabaseContext.Quote.Where(i => i.IsPermitted)
            .Include(i => i.UserData).RandomAsync())!;

        if (randomQuote is null)
        {
            await ctx.CreateResponseAsync("damn");
            return;
        }


        var counts = await DatabaseContext.Quote
            .Where(i => i.Id == randomQuote.Id)
            .Select(j =>
                new
                {
                    likes = j.QuoteReactions.Count(k => k.IsLike),
                    dislikes = j.QuoteReactions.Count(k => !k.IsLike)
                })
            .FirstAsync();
        var ownerImageUrl = (await ctx.Client.GetUserAsync(randomQuote.UserDataId)).AvatarUrl;
        var ownerImage = await BasicFunction.GetImageFromUrlAsync(ownerImageUrl);
        DiscordButtonComponent like = new(ButtonStyle.Primary,"like",null,false,new DiscordComponentEmoji("👍"));
        DiscordButtonComponent dislike = new(ButtonStyle.Primary, "dislike",null,false,new DiscordComponentEmoji("👎"));
        var ownerOfQuote = await ctx.Client.GetUserAsync(randomQuote.UserDataId);
        var quoteDate = randomQuote.DateCreated;
        
        var embedBuilder = new DiscordEmbedBuilder()
            .WithUser(ownerOfQuote)
            .WithImageUrl("attachment://quote.png")
            .WithColor(randomQuote.UserData.Color)
            .WithTitle($"{ownerOfQuote.Username}'s quote")
            .WithDescription(randomQuote.QuoteValue)
            .WithFooter($"Date and Time Created: {quoteDate:MM/dd/yyyy HH:mm:ss}\nLikes: {counts.likes} Dislikes: {counts.dislikes}");
        
        var message = await ctx.GetOriginalResponseAsync();
        await message.WaitForButtonAsync(i =>
        {
            Task.Run(async () =>
            {
                var choice = i.Interaction.Data.CustomId;
                if (!new[] { "like", "dislike" }.Contains(choice)) return;
                var newDbContext = new PostgreSqlContext();
                var anonymous = await newDbContext.Set<QuoteReaction>()
                    .Where(j => j.QuoteId == randomQuote.Id && j.UserDataId == i.User.Id)
                    .Select(j => new
                    {
                        quote = j.Quote, quoteReaction = j,
                    })
                    .FirstOrDefaultAsync();

                var quoteReaction = anonymous?.quoteReaction;
                if (anonymous?.quote is not null)
                {
                    randomQuote = anonymous.quote;
                }

                var isNew = false;
                if (quoteReaction is null)
                {
                    quoteReaction = new QuoteReaction();
                    await newDbContext.Set<QuoteReaction>().AddAsync(quoteReaction);
                    //assigning it to id instead of instance cuz instance might not be of the same dbcontext as newDbContext
                    quoteReaction.QuoteId = randomQuote.Id;
                    quoteReaction.UserDataId = i.User.Id;
                    isNew = true;
                }

                if (!await newDbContext.UserData.AnyAsync(j => j.Id == i.User.Id))
                    await newDbContext.UserData.AddAsync(new UserData(i.User.Id));
                if (!isNew &&
                    ((choice == "like" && quoteReaction.IsLike) || (choice == "dislike" && !quoteReaction.IsLike)))
                {
                    newDbContext.Set<QuoteReaction>().Remove(quoteReaction);

                }
                else if (choice == "like")
                {
                    quoteReaction.IsLike = true;
                }
                else
                {
                    quoteReaction.IsLike = false;
                }

                await newDbContext.SaveChangesAsync();
                var localCounts = await newDbContext.Quote.Where(i => i.Id == randomQuote.Id)
                    .Select(i => new
                    {
                        likes = i.QuoteReactions.Count(j => j.IsLike),
                        dislikes = i.QuoteReactions.Count(j => !j.IsLike)
                    }).FirstAsync();
                newDbContext.DisposeAsync();
                embedBuilder
                    .WithFooter(
                        $"Date and Time Created: {quoteDate:MM/dd/yyyy HH:mm:ss}\nLikes: {localCounts.likes} Dislikes: {localCounts.dislikes}");

                await i.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder()

                        .AddEmbed(embedBuilder)
                        .AddComponents(like, dislike));


            });
            return false;
        },  new TimeSpan(0,10,0));
        
    }
    [SlashCommand("write", "creates a quote")]
    public async Task Write(InteractionContext ctx, [Option("text", "the quote")] string text)
    {
        var userData = await DatabaseContext.UserData.FindOrCreateAsync(ctx.User.Id);
        var embedBuilder = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithColor(userData.Color);
        if (text.Length > 200)
        {
            embedBuilder
                .WithTitle("Hmm")
                .WithDescription("Quote length should be shorter than 200 characters");

            await ctx.CreateResponseAsync(embedBuilder);
            return;
        } 
        if (text.Length <= 0)
        {
            embedBuilder
                .WithTitle("bruh")
                .WithDescription("Write something in the quote!");
            await ctx.CreateResponseAsync(embedBuilder);
            return;
        }
        userData.Quotes.Add(new LegendaryBot.Quote{QuoteValue = text});
        await DatabaseContext.SaveChangesAsync();
        embedBuilder
            .WithTitle("Success!")
            .WithDescription("Your quote has been saved, and is waiting to be approved!")
            .AddField("Your quote was: ", text);
        await ctx.CreateResponseAsync(embedBuilder);
    }
}