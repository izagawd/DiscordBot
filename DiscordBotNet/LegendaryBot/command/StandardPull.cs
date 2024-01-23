using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.command;
 
public class StandardPull : BaseCommandClass
{
    public override BotCommandType BotCommandType => BotCommandType.Battle;
    private static readonly Dictionary<PullChoice, double> pullChances = new ()
    {
        { PullChoice.FiveStarCharacter, 2.0 },
        { PullChoice.FiveStarBlessing, 2.5 },
        { PullChoice.FourStarCharacter, 10.0 },
        { PullChoice.FourStarBlessing, 10.0 },
        { PullChoice.ThreeStarCharacter, 35.25 }, // Adjusted to maintain a total of 100%
        { PullChoice.ThreeStarBlessing, 40.25 } // Adjusted to maintain a total of 100%
    };
    [SlashCommand("standard_pull", "Pull for a character and add it to your collection!")]
    public async Task Execute(InteractionContext ctx)
    {
        var userData = await DatabaseContext.UserData.FindOrCreateAsync((long)ctx.User.Id);
        var embed = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithColor(userData.Color);
        if (userData.StandardPrayers <= 0)
        {
            embed.WithTitle("Hmm")
                .WithDescription($"{ctx.User.Username} does not have any Standard/s Prayers");
            await ctx.CreateResponseAsync(embed);
            return;
        }
        var choice  = BasicFunctionality.GetRandom(pullChances);

        Type acquiredType = null;
 
        switch (choice)
        {
            case PullChoice.ThreeStarBlessing:
                acquiredType = BasicFunctionality.RandomChoice(Blessing.ThreeStarBlessingExamples
                    .Where(i => i.IsInStandardBanner)).GetType();
                break;
            case PullChoice.FourStarBlessing:
                acquiredType = BasicFunctionality.RandomChoice(Blessing.FourStarBlessingExamples
                    .Where(i => i.IsInStandardBanner)).GetType();
                break;
            case PullChoice.FiveStarBlessing:
                acquiredType = BasicFunctionality.RandomChoice(Blessing.FiveStarBlessingExamples
                    .Where(i => i.IsInStandardBanner)).GetType();
                break;
            case PullChoice.ThreeStarCharacter:
                acquiredType = BasicFunctionality.RandomChoice(Character.ThreeStarCharacterExamples
                    .Where(i => i.IsInStandardBanner)).GetType();
                break;
            case PullChoice.FourStarCharacter:
                acquiredType = BasicFunctionality.RandomChoice(Character.FourStarCharacterExamples
                    .Where(i => i.IsInStandardBanner)).GetType();
                break;

            case PullChoice.FiveStarCharacter:
                acquiredType = BasicFunctionality.RandomChoice(Character.FiveStarCharacterExamples
                        .Where(i => i.IsInStandardBanner))
                    .GetType();
                break;
        }

        if (acquiredType is not null)
        {
            userData.StandardPrayers--;
            var acquiredEntity = (BattleEntity) Activator.CreateInstance(acquiredType)!;
            userData.Inventory.Add(acquiredEntity);
            if(acquiredEntity is ISetup setup) setup.Setup();
            await DatabaseContext.SaveChangesAsync();

            embed.WithTitle("Nice!!")
                .WithDescription($"You pulled a {BasicFunctionality.Englishify(choice.ToString())} called {acquiredType.Name}!");
            await using var stream = new MemoryStream();
            using var detailsImage =await acquiredEntity.GetDetailsImageAsync();
            await detailsImage.SaveAsPngAsync(stream);
            stream.Position = 0;
            embed
                .WithImageUrl("attachment://pull.png");
            var response = new DiscordInteractionResponseBuilder()
                .AddEmbed(embed)
                .AddFile("pull.png",stream);
            await ctx.CreateResponseAsync(response);

        }
        else
        {
            embed.WithTitle("hmm")
                .WithDescription("something went wrong boi");
            await ctx.CreateResponseAsync(embed);
        }
    
 
    }

 
}