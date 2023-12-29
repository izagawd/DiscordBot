﻿using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Results;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.DialogueNamespace;

/// <summary>
/// Creates dialogue of a character
/// </summary>
public class Dialogue
{

    public  required string Title { get; init; } = "untitled";
    public IEnumerable<DialogueNormalArgument> NormalArguments { get; init; } = [];
    /// <summary>
    /// Whether or not the dialogue can be skipped
    /// </summary>
    public bool Skippable { get; init; } = true;
    
    /// <summary>
    /// Set it to something when you want the user to make a decision at the end of the dialogue
    /// </summary>
    public DialogueDecisionArgument? DecisionArgument { get; init; }

    /// <summary>
    /// Responds to the interaction of the provided context if true. Used if the command is being responded to
    /// with a dialogue
    /// </summary>
    public bool RespondInteraction { get; init; }

    private DiscordMessage _message = null!;
    private InteractionContext _interactionContext= null!;
    private DiscordEmbedBuilder _embedBuilder= null!;
    
    private bool _timedOut = false;
    private bool _skipped = false;
    private bool _finished = false;
    /// <summary>
    /// Whether or not to remove all buttons at the end of dialogue. should be used if the last dialogue text
    /// is the last thing that happens in a command
    /// </summary>
    public bool RemoveButtonsAtEnd { get; init; } 
    private static readonly DiscordButtonComponent Next = new(ButtonStyle.Success, "next", "NEXT");
    private static readonly DiscordButtonComponent Skip = new(ButtonStyle.Success, "skip", "SKIP");


    private async Task HandleArgumentDisplay(string text, bool isLast,
        params DiscordActionRowComponent[] discordActionRows)
    {
    
        if (discordActionRows.Any(i =>
                i.Components.Any(j => j.CustomId == "skip")))
        {
            discordActionRows.ToList().ForEach(i => i.Components.ToList().ForEach(j =>j.CustomId.Print()));
            throw new Exception("No discord component in the provided action rows should have an Id of \"skip\"\n" +
                                "since it is already preserved for another purpose");
        }

        if (discordActionRows.Any(i => i.Components.Any(j => j is not DiscordButtonComponent)))
        {
            throw new Exception("Only buttons are allowed in the action rows");
        }
        var lastActionRow = discordActionRows.LastOrDefault();

        if (lastActionRow is not null && lastActionRow.Components.Count < 5 && Skippable)
        {
            lastActionRow = new DiscordActionRowComponent([..lastActionRow.Components, Skip]);
            discordActionRows[discordActionRows.Length -1] = lastActionRow;
        } else if (Skippable)
        {
            discordActionRows = [..discordActionRows, new DiscordActionRowComponent([Skip])];
        }

        _embedBuilder.WithDescription(text);
        DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
            .AddEmbed(_embedBuilder)
            .AddComponents(discordActionRows.AsEnumerable());
        
        if(isLast && RemoveButtonsAtEnd && DecisionArgument is null)
        {
            messageBuilder.ClearComponents();
          
        }

        var interaction = _interactionContext.Interaction;
        if (RespondInteraction && _message is null)
        {
            var responseBuilder = new DiscordInteractionResponseBuilder()
                .AddEmbed(_embedBuilder.Build()).AddComponents(Next, Skip);
      
            if (isLast && RemoveButtonsAtEnd && DecisionArgument is null)
            {
                responseBuilder.ClearComponents();
            }

            await interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                responseBuilder);

            _message = await interaction.GetOriginalResponseAsync();
        }
        else if (_message is null)
        {
            _message = await interaction.Channel.SendMessageAsync(messageBuilder); 
        } else
        {
            _message = await _message.ModifyAsync(messageBuilder);
        }
    }


    private async Task HandleInteractionResultAsync(InteractivityResult<ComponentInteractionCreateEventArgs> args,
        bool defer = true)
    {
        if (defer)
        {
            args.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        }

        var answer = args.Result.Id;
        if (answer == "skip")
        {
            _skipped = true;
        }
        if(args.TimedOut)
        {
            _timedOut = true;
            _finished = true;
            return;
        }
        if (_skipped)
        {
            _finished = true;

        }
    }
    /// <summary>
    /// Initiates the dialogue of a character
    /// </summary>
    /// <param name="context">The context of the interaction</param>
    /// <param name="message">if not null, will edit the message provided with the dialogue</param>
    /// <returns></returns>
    public async Task<DialogueResult> LoadAsync(InteractionContext context,DiscordMessage? message = null)
    {
        if (NormalArguments.Count() <= 0 && DecisionArgument is null)
        {
            throw new Exception("There is no decision argument provided");
        }
        _interactionContext = context;


        _message = message!;

        var loadedDialogueArguments = NormalArguments.ToArray();
        _embedBuilder = new DiscordEmbedBuilder()
            .WithTitle(Title);

        for(int i = 0; i < loadedDialogueArguments.Length; i++)
        {
            _embedBuilder
                .WithAuthor(loadedDialogueArguments[i].CharacterName, iconUrl: loadedDialogueArguments[i].CharacterUrl)
                .WithColor(loadedDialogueArguments[i].CharacterColor);
            var normalArgument = loadedDialogueArguments[i];
      

            var dialogueTexts = normalArgument.DialogueTexts.ToArray();
            for(int j = 0; j < dialogueTexts.Length;   j++)
            {
                var isLast = i == loadedDialogueArguments.Length
                    - 1 && j == dialogueTexts.Length - 1;
                await HandleArgumentDisplay(dialogueTexts[j], isLast,
                    [new DiscordActionRowComponent([Next])]);

                
                if(isLast && RemoveButtonsAtEnd && DecisionArgument is null) break;
                var result = await _message
                    .WaitForButtonAsync(e => e.User == _interactionContext.User);
                await HandleInteractionResultAsync(result);

                if(_finished) break;
                
            }
            
            if (_finished) break;
        }

        string? decision = null;
        if (!_finished && DecisionArgument is not null)
        {
            _embedBuilder
                .WithAuthor(DecisionArgument.CharacterName, iconUrl: DecisionArgument.CharacterUrl)
                .WithColor(DecisionArgument.CharacterColor);
            await HandleArgumentDisplay(DecisionArgument.DialogueText, true,
                DecisionArgument.ActionRows.ToArray());
            var result = await _message.WaitForButtonAsync(e
                => e.User == _interactionContext.User);
            decision = result.Result.Id;
            var defer = true;

            if (RemoveButtonsAtEnd)
            {
                var messageBuilder = new DiscordMessageBuilder(_message);
                messageBuilder.ClearComponents();
                await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder(messageBuilder));
                _message = await _message.ModifyAsync(messageBuilder);
                defer = false;
            }
            await HandleInteractionResultAsync(result, defer);
        }
        return new DialogueResult
        {
            Skipped = _skipped,
            TimedOut = _timedOut,
            Message = _message,
            Decision = decision
        };
    }
}