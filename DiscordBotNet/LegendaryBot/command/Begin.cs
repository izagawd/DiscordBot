using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.DialogueNamespace;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Results;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using Npgsql;


namespace DiscordBotNet.LegendaryBot.command;

public class Begin : BaseCommandClass
{
    public override BotCommandType BotCommandType => BotCommandType.Battle;

    [SlashCommand("begin", "Begin your journey by playing the tutorial!")]
    public async Task Execute(InteractionContext ctx)
    {
        DiscordEmbedBuilder embedToBuild = new();
        DiscordUser author = ctx.User;

        UserData userData = await DatabaseContext.UserData
            .Include(j => j.Inventory)
            .ThenInclude(j => (j as Character).Blessing)
            .Include(i => i.EquippedPlayerTeam)
            .Include(i => i.Inventory.Where(i => i is Character || i is Gear))
            .FindOrCreateAsync((long)author.Id);
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
            player.UserDataId = userData.Id;
        }

        if (userData.EquippedPlayerTeam is null)
        {
            var playerTeam = new PlayerTeam();
            userData.EquippedPlayerTeam = playerTeam;
            playerTeam.UserDataId = userData.Id;
            userData.PlayerTeams.Add(playerTeam);
        }

        
        
        if (!userData.EquippedPlayerTeam.Any())
        {
            userData.EquippedPlayerTeam.Add(userData.Inventory.OfType<Player>().First());
        }
        var coachChad = new CoachChad();
    
        coachChad.SetLevel(100);
        var coachChadProfile = new DialogueProfile
        {
            CharacterColor = coachChad.Color, CharacterName = coachChad.Name,
            CharacterUrl = coachChad.IconUrl,
        };
        
        
        DialogueNormalArgument[] dialogueArguments =
        [
            new()
            {
                DialogueProfile = coachChadProfile,
                DialogueTexts =
                    [
                        "Hey Izagawd! my name is Chad. So you want to register as an adventurer? That's great!",
                        "But before you go on your adventure, I would need to confirm if you are strong enough to **Battle**!",
                        "Lily will accompany you just for this fight. When you feel like you have gotten the hang of **BATTLE**, click on **FORFEIT!**"
                    ]
            },
            new()
            {
                DialogueProfile = lily.DialogueProfile,
                DialogueTexts = [$"Let's give it our all {author.Username}!"]
            }
        ];



        Dialogue theDialogue = new()
        {
            NormalArguments = dialogueArguments,

            Title = "Tutorial",
            RespondInteraction = true,

        };
        DialogueResult result = await theDialogue.LoadAsync(ctx);

        if (result.TimedOut)
        {
            theDialogue = new Dialogue()
            {
                Title = "Beginning Of Journey",
                NormalArguments =
                [
                    new DialogueNormalArgument
                    {
                        DialogueProfile = coachChadProfile,
                        DialogueTexts = [
                            "I can't believe you slept off..."
                        ],
                    }
                ],
                RemoveButtonsAtEnd = true
            };


            await theDialogue.LoadAsync(ctx,result.Message);
            return;
        }


        var userTeam = userData.EquippedPlayerTeam;
        userTeam.Add(lily);

        BattleResult battleResult = await new BattleSimulator(await userTeam.LoadAsync(), 
            await new CharacterTeam(characters: coachChad).LoadAsync()).StartAsync(ctx, result.Message);

        if (battleResult.TimedOut is not null)
        {
            theDialogue = new Dialogue
            {
                Title = "Begin!",
                NormalArguments =
                [
                    new()
                    {
                        DialogueProfile = coachChadProfile,
                        DialogueTexts = ["I can't believe you slept off during a battle..."]
                    }
                ],
                RemoveButtonsAtEnd = true
            };
    
            

            await theDialogue.LoadAsync(ctx, result.Message);
            return;
        }

        theDialogue = new Dialogue()
        {

            NormalArguments =        [
                new ()
                {
                    DialogueProfile = coachChadProfile,
                    DialogueTexts =
                    [
                        "Seems like you have gotten more used to battle.",
                        "You have completed the registration and you are now a **Bronze** tier adventurer! the lowest tier! you gotta work your way up the ranks!",
                        "I will see you later then! I have other new adventurers to attend to! Let's go attend to them Lily!"
                    ]
                }
            ],
            RemoveButtonsAtEnd = true, 
            RespondInteraction = false,
            Title = "Tutorial"
        };


        userData.Tier = await DatabaseContext.UserData.FindOrCreateSelectAsync((long)author.Id, i => i.Tier);

        if (userData.Tier == Tier.Unranked)
        {
            userData.Tier = Tier.Bronze;
            userData.LastTimeChecked = DateTime.UtcNow.AddDays(-1);
        };
        userTeam.Remove(lily);
        try
        {
            await DatabaseContext.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            userTeam.OfType<Player>().First().Id.Print();
            if (e.InnerException is NpgsqlException eInnerException)
            {
                eInnerException.Message.Print();
            }
            e.InnerException.GetType().Print();
            foreach (var i in e.Entries)
            {
                i.Entity.GetType().Print();
            }
            throw e;
        }
      
        result =  await theDialogue.LoadAsync(ctx, result.Message);
        if (result.TimedOut)
        {
            theDialogue = new Dialogue
            {
                Title = "Beginning of journey",
                RemoveButtonsAtEnd = true,
                NormalArguments =
                [
                    new DialogueNormalArgument
                    {
                        DialogueProfile = coachChadProfile,
                        DialogueTexts =

                        [
                            "You slept off after becoming an adventurer... you are strange..."
                        ],
   
                    }
                ]
            };
                

            await theDialogue.LoadAsync(ctx, result.Message);
        }
        if (result.Skipped)
        {
            await result.Message.ModifyAsync(
                new DiscordMessageBuilder()
                    .WithEmbed(result.Message.Embeds.First()));
        }

    }
}