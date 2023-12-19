using DiscordBotNet.LegendaryBot.Battle.Arguments;
using DiscordBotNet.LegendaryBot.Battle.Results;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace DiscordBotNet.LegendaryBot.Battle;

/// <summary>
/// Creates dialogue of a character
/// </summary>
public class Dialogue
{

    public string Title { get; init; } = "untitled";
    public IEnumerable<DialogueArgument> Arguments { get; set; } = [];
    /// <summary>
    /// Responds to the interaction if true
    /// </summary>
    public bool RespondInteraction { get; init; } 
        
    public bool RemoveButtonsAtEnd { get; set; } 
    private static readonly DiscordButtonComponent Next = new(ButtonStyle.Success, "next", "NEXT");
    private static readonly DiscordButtonComponent Skip = new(ButtonStyle.Success, "skip", "SKIP");
    /// <summary>
    /// Initiates the dialogue of a character
    /// </summary>
    /// <param name="interaction"></param>
    /// <param name="message"></param>
    /// <returns></returns>

    public async Task<DialogueResult> LoadAsync(DiscordInteraction interaction,DiscordMessage? message = null)
    {

        var loadedDialogueArguments = Arguments.ToArray();
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithTitle(Title);

        bool timedOut = false;
        bool skipped = false;
        bool finished = false;
        for(int i = 0; i < loadedDialogueArguments.Length; i++)
        {
            embedBuilder.WithAuthor(loadedDialogueArguments[i].CharacterName, iconUrl: loadedDialogueArguments[i].CharacterUrl)
                .WithColor(loadedDialogueArguments[i].CharacterColor);
            for (int j = 0; j < loadedDialogueArguments[i].Dialogues.Count;   j++)
            {
                var dialogues = loadedDialogueArguments[i].Dialogues;
                embedBuilder.WithDescription(dialogues[j]);
                DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder().AddEmbed(embedBuilder.Build()).AddComponents(Next,Skip);
                if(i == loadedDialogueArguments.Length-1 && j == loadedDialogueArguments[i].Dialogues.Count -1 && RemoveButtonsAtEnd)
                {
                    messageBuilder.ClearComponents();
          
                }
                 
                if (RespondInteraction && message is null)
                {
                    DiscordInteractionResponseBuilder responseBuilder = new DiscordInteractionResponseBuilder()
                        .AddEmbed(embedBuilder.Build()).AddComponents(Next, Skip);
      
                    if (i == loadedDialogueArguments.Length - 1 && RemoveButtonsAtEnd)
                    {
                        responseBuilder.ClearComponents();
                    }

                    await interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        responseBuilder);

                    message = await interaction.GetOriginalResponseAsync();
                }
                else if (message is null)
                {
                    message = await interaction.Channel.SendMessageAsync(messageBuilder); 
                } else
                {
                    message = await message.ModifyAsync(messageBuilder);
                }
                if (i == loadedDialogueArguments.Length - 1 && RemoveButtonsAtEnd && j == dialogues.Count -1)
                {
                    finished = true;
                    break;
                }
                var idk = await message.WaitForButtonAsync(e =>
                {

                    string customid = e.Interaction.Data.CustomId;
                    if (customid == "skip") skipped = true;
                    e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                    return true;
                });
                if(idk.TimedOut)
                {
                    timedOut = true;
                    finished = true;
                    break;
                }

                if (skipped)
                {
                    finished = true;
                    break;
                }
            }

            if (finished)
            {
                break;
            }
            

        }
        return new DialogueResult
        {
            Skipped = skipped,
            TimedOut = timedOut,
            Message = message!,
                
                
        };
    }
}