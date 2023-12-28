using DiscordBotNet.LegendaryBot.DialogueNamespace;
using DiscordBotNet.LegendaryBot.Rewards;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.Quests;

public class DirectionHelping : Quest
{
    public override string Description => "You are tasked with giving people directions";
    public override async  Task<bool> StartQuest(InteractionContext context, DiscordMessage? messageToEdit = null)
    {
        var profile = new DialogueProfile()
        {
            CharacterColor = DiscordColor.Blue,
            CharacterName = "Hibari",
            CharacterUrl =
                "https://static.wikia.nocookie.net/reborn/images/e/e9/Hibari_Kyoya.PNG/revision/latest/scale-to-width-down/1000?cb=20100421080639"
        };
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
            DialogueText = "Hello, do you know where school is? I think it's right I'm not sure"
        };
        var dialogue = new Dialogue
        {
            Title = "directions",
            DecisionArgument = decisionArgument,
        };

        var result = await dialogue.LoadAsync(context, messageToEdit);
        var buttonDecisionId = result.Decision;
        if (result.Decision == "right")
        {
            dialogue = new Dialogue()
            {
                Title = "directions",

                NormalArguments =
                [
                    new DialogueNormalArgument()
                    {
                        DialogueProfile = profile,
                        DialogueTexts = ["Guess I don't have to bite you to death",
                        "I knew the direction, I just wanted to see if bronze tiers were smart",
                        $"Get stronger so I can bite you to death, {context.User.Username}."]
                    }
                ]
            };
        }
        else
        {
            dialogue = new Dialogue()
            {
                Title = "directions",

                NormalArguments =
                [
                    new DialogueNormalArgument()
                    {
                        DialogueProfile = profile,
                        DialogueTexts = ["Wrong... you are so dumb, guess i'll bite you to death"]
                    }
                ]
            };
        }

        await dialogue.LoadAsync(context, result.Message);
        return buttonDecisionId == "right";
    }

    public override IEnumerable<Reward> QuestRewards { get; protected set; } = [];
}