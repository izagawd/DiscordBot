using DiscordBotNet.Database.Models;

namespace DiscordBotNet.LegendaryBot;

public class Quote
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public bool IsPermitted { get; set; } = false;
    public ulong UserDataId { get; set; }
    public string QuoteValue { get; set; } = "Nothing";
    public UserData UserData { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public override string ToString()
    {
        return QuoteValue;
    }

    public Quote(string quote)
    {
        QuoteValue = quote;
    }
    public Quote(){}
}