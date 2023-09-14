
namespace DiscordBotNet.LegendaryBot.Database.Models;

public class GuildData : Model
{
    public List<string> EnabledCommands { get; set; } = new List<string>();
}
