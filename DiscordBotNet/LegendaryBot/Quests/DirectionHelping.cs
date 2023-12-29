﻿using DiscordBotNet.Database;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleEvents;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.DialogueNamespace;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Rewards;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.Quests;

public class DirectionHelping : Quest
{
    public override string Description => "You are tasked with giving people directions";
    public override async Task<bool> StartQuest(PostgreSqlContext databaseContext, InteractionContext context,
        DiscordMessage? messageToEdit = null)
    {
        var blast = new Blast();
        var profile = blast.DialogueProfile;
        var decisionArgument = new DialogueDecisionArgument
        {
            
            DialogueProfile = profile,
            ActionRows =
            [
                new DiscordActionRowComponent([
                    new DiscordButtonComponent(ButtonStyle.Primary,
                        "right",
                        "(Point Right)"),
                    new DiscordButtonComponent(ButtonStyle.Danger,
                        "left",
                        "(Point Left)")
                ])
            ],
            DialogueText 
                = "Yo, bronze tier human, do you know where I can find the nearest restaurant? I think it's right but I'm not sure"
        };
        var dialogue = new Dialogue
        {
            Title = "directions",
            DecisionArgument = decisionArgument,
            Skippable = false
        };

        var dialogueResult = await dialogue.LoadAsync(context, messageToEdit);
        var buttonDecisionId = dialogueResult.Decision;
        if (dialogueResult.Decision == "right")
        {
            dialogue = new Dialogue()
            {
                Title = "directions",

                NormalArguments =
                [
                    new DialogueNormalArgument()
                    {
                        DialogueProfile = profile,
                        DialogueTexts = ["Guess I don't have to make an explosion out of you",
                        "I knew the direction, I just wanted to see if bronze tiers were smart",
                        $"Get stronger so trying to detonate you will be more interesting, {context.User.Username}."]
                    }
                ]
            };
            await dialogue.LoadAsync(context, dialogueResult.Message);
            return true;
        }
        dialogue = new Dialogue()
        {
            Title = "directions",

            NormalArguments =
            [
                new DialogueNormalArgument()
                {
                    DialogueProfile = profile,
                    DialogueTexts = ["Wrong... you are so dumb, guess i'll detonate you"]
                }
            ]
        };

        dialogueResult = await dialogue.LoadAsync(context, dialogueResult.Message);
        var blastTeam = new CharacterTeam(blast);
        await blastTeam.LoadAsync();
        blast.SetLevel(60);
        blast.TotalAttack = 2500;
        blast.TotalSpeed = 150;
        blast.TotalMaxHealth = 50000;
        blast.TotalCriticalChance = 80;
        blast.TotalCriticalDamage = 300;
        blast.TotalDefense = 750;
        blast.TotalMaxHealth = 40000;
        blast.Health = 40000;
        

        var userData = await databaseContext.UserData
            .IncludeTeamWithAllEquipments()
            .FindOrCreateAsync(context.User.Id);
        var userTeam = userData.GetCharacterTeam(context.User);
        await userTeam.LoadAsync(context.User);
        var battle = new BattleSimulator(userTeam,blastTeam);

        var battleResult = await battle.StartAsync(context, dialogueResult.Message);

        if (battleResult.Winners == userTeam)
        {
            dialogue = new Dialogue()
            {
                Title = "direction",
                NormalArguments =
                [
                    new DialogueNormalArgument()
                    {
                        DialogueProfile = profile,
                        DialogueTexts =
                        [
                            "How... did you win...?",
                            "You are a bronze tier... you shouldn't be able to beat me...",
                            "h-how..."
                        ]
                    }
                ]
            };
             await dialogue.LoadAsync(context, battleResult.Message);
            QuestRewards = battleResult.BattleRewards.Append(
                new TextReward(userTeam.IncreaseExp(battleResult.ExpToGain)));
            
            return true;
        }

        dialogue = new Dialogue()
        {
            Title = "direction",
            NormalArguments =
            [
                new DialogueNormalArgument()
                {
                    
                    DialogueProfile = profile,
                    DialogueTexts =
                    [
                        "Hmph, weak"
                    ]
                }
            ]
        };
         await dialogue.LoadAsync(context, battleResult.Message);

        
        return false;
    }

    public override IEnumerable<Reward> QuestRewards { get; protected set; } = [];
}