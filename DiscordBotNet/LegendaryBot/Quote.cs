namespace DiscordBotNet.LegendaryBot;

public class Quote
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public bool IsPermitted { get; set; } = false;
    public ulong UserDataId { get; set; }
    public string QuoteValue { get; set; } = "Nothing";

    public DateTime DateChanged { get; set; } = DateTime.UtcNow;
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