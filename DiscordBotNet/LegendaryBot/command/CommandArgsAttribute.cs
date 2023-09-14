namespace DiscordBotNet.LegendaryBot.command;
[AttributeUsage(AttributeTargets.Method)]
public class CommandArgsAttribute : Attribute
{
    public bool RequiresBegun { get; set; } = false;
    public bool MakesUsersOccupied { get; set; } = false;
}