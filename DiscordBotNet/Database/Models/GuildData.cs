
namespace DiscordBotNet.Database.Models;

public class GuildData : Model
{
    public List<string> EnabledCommands { get; set; } = new();
}
