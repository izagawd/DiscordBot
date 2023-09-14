using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.command;
[SlashCommandGroup("summon", "idk boi")]
public class Summon: BaseCommandClass
{
    public override BotCommandType BotCommandType { get; } = BotCommandType.Fun;
    [CommandArgs(RequiresBegun = true,MakesUsersOccupied = true)]
    [SlashCommand("sumr", "summon a character to join you in battle!")]
    public async Task Execute(InteractionContext ctx)
    {
        
        
    }

}