using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DiscordBotNet.LegendaryBot.Battle;
using DiscordBotNet.LegendaryBot.Battle.Arguments;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Entities.Gears;
using DiscordBotNet.LegendaryBot.Battle.Results;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;


namespace DiscordBotNet.LegendaryBot.command;

public class Begin : BaseCommandClass
{
    public override BotCommandType BotCommandType { get; } = BotCommandType.Adventure;

    [SlashCommand("begin", "Begin your journey by playing the tutorial!")]
    public async Task Execute(InteractionContext ctx)
    {
        DiscordEmbedBuilder embedToBuild = new();
        DiscordUser author = ctx.User;

        UserData userData = await DatabaseContext.UserData
            .Include(j => j.Inventory)
            .ThenInclude(j => (j as Character).Blessing)
            .Include(i => i.Inventory.Where(i => i is Character || i is Gear))
            .FindOrCreateAsync(author.Id);
        await DatabaseContext.SaveChangesAsync();
        DiscordColor userColor = userData.Color;
        if (userData.IsOccupied)
        {
            embedToBuild
                .WithTitle("Hmm")
                .WithAuthor(author.Username, iconUrl: author.AvatarUrl)
                .WithDescription("`You are occupied`")
                .WithColor(userColor);

            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embedToBuild.Build()));
            return;
        }


        if (userData.Tier != Tier.Unranked)
        {
            embedToBuild
                .WithTitle("Hmm")
                .WithAuthor(author.Username, iconUrl: author.AvatarUrl)
                .WithDescription("`You have already begun`")
                .WithColor(userColor);

            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embedToBuild.Build()));
            return;
        }
        await MakeOccupiedAsync(userData);
        Lily lily = new Lily();
        
        if (!userData.Inventory.Any(i => i is Player))
        {
            Player player = new Player();
      
            player.SetElement(Element.Fire);



            userData.Inventory.Add(player);
            player.UserData = userData;
        }

        if (userData.Team.Count <= 0)
        {
            userData.AddToTeam(userData.Inventory.OfType<Player>().First());
        }
        CoachChad coachChad = new CoachChad();
    
        coachChad.SetLevel(100);

        List<DialogueArgument> dialogueArguments = new List<DialogueArgument>()
        {
            new()
            {
                CharacterColor = coachChad.Color, CharacterName = coachChad.Name,
                CharacterUrl = coachChad.IconUrl,
                Dialogues =
                new List<string>{
                    "Hey Izagawd! my name is Chad. So you want to register as an adventurer? That's great!",
                    "But before you go on your adventure, I would need to confirm if you are strong enough to **Battle**!",
                    "Lily will accompany you just for this fight. When you feel like you have gotten the hang of **BATTLE**, click on **FORFEIT!**"
                }
            }, new()
            {
                CharacterColor = lily.Color, CharacterName = lily.Name,
                Dialogues = new List<string>{$"Let's give it our all {author.Username}!"}
            }
        };
            


        Dialogue theDialogue = new()
        {

            Arguments = dialogueArguments,
            Title = "Tutorial",
            RespondInteraction = true,

        };
        DialogueResult result = await theDialogue.LoadAsync(ctx.Interaction);
        if (result.TimedOut)
        {
            theDialogue.Arguments = new List<DialogueArgument>
            {
                new DialogueArgument
                {
                    CharacterColor = coachChad.Color, CharacterName = coachChad.Name,
                    Dialogues =
                        new List<string>{
                        "I can't believe you slept off..."
                    }
                }
            };
            theDialogue.RemoveButtonsAtEnd = true;

            await theDialogue.LoadAsync(ctx.Interaction,result.Message);
            return;
        }


        var userTeam = userData.Team;
        userTeam.Add(lily);

        BattleResult battleResult = await  new BattleSimulator(await  userTeam.LoadAsync(), await new CharacterTeam(characters: coachChad).LoadAsync()).StartAsync(ctx.Interaction, result.Message);
        theDialogue.RemoveButtonsAtEnd = true;
        if (battleResult.TimedOut is not null)
        {
            theDialogue.Arguments = new List<DialogueArgument>
            {
                new()
                {
                    CharacterColor = coachChad.Color, CharacterName = coachChad.Name,
                    Dialogues =
                        new List<string>{
                        "I can't believe you slept off during a battle..."
                    }
                }
            };
            

            await theDialogue.LoadAsync(ctx.Interaction, result.Message);
            return;
        }

        theDialogue = new Dialogue() { RemoveButtonsAtEnd = true, RespondInteraction = false,Title = "Tutorial"};
        theDialogue.Arguments = new List<DialogueArgument>
        {
            new ()
            {
                CharacterColor = coachChad.Color, CharacterName = coachChad.Name,
                CharacterUrl = coachChad.IconUrl,
                Dialogues =
                    new List<string>{
                    "Seems like you have gotten more used to battle.",
                    "You have completed the registration and you are now a **Bronze** tier adventurer! the lowest tier! you gotta work your way up the ranks!",
                    "I will see you later then! I have other new adventurers to attend to! Let's go attend to them Lily!"
                }
            }
        };


        userData.Tier = await DatabaseContext.UserData.FindOrCreateSelectAsync(author.Id, i => i.Tier);

        if (userData.Tier == Tier.Unranked)
        {
            userData.Tier = Tier.Bronze;
        };
        await DatabaseContext.SaveChangesAsync();
        result =  await theDialogue.LoadAsync(ctx.Interaction, result.Message);
        if (result.TimedOut)
        {
            theDialogue.RemoveButtonsAtEnd = true;
            theDialogue.Arguments = new List<DialogueArgument>
            {
                new()
                {
                    CharacterColor = coachChad.Color, CharacterName = coachChad.Name,
                    Dialogues =
                        new List<string>{
                        "You slept off after becoming an adventurer... you are strange..."
                    }
                }
            };
            await theDialogue.LoadAsync(ctx.Interaction, result.Message);
        }
        if (result.Skipped)
        {
            await result.Message.ModifyAsync(
                new DiscordMessageBuilder()
                    .WithEmbed(result.Message.Embeds.First()));
        }

    }
}