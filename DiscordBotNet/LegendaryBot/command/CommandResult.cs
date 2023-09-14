using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.command;

public class CommandResult
{
    public SlashCommandAttribute SlashCommandAttribute { get; set; }
    public CommandArgsAttribute CommandArgsAttribute { get; set; }
    public BotCommandType BotCommandType { get; set; }
}