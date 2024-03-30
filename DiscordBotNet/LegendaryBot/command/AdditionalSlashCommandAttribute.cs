namespace DiscordBotNet.LegendaryBot.command;

public class AdditionalSlashCommandAttribute : Attribute
{
    public string? Example { get; }
    public BotCommandType BotCommandType { get; }
    public AdditionalSlashCommandAttribute(string example, BotCommandType botCommandType)
    {
        Example = example;
        BotCommandType = botCommandType;
    }

    public AdditionalSlashCommandAttribute(string example)
    {
        Example = example;
        BotCommandType = BotCommandType.Other;
    }
    public AdditionalSlashCommandAttribute(BotCommandType botCommandType, string example)
        : this(example, botCommandType){}
    public AdditionalSlashCommandAttribute(BotCommandType botCommandType)
    {
  
        BotCommandType = botCommandType;
    }
}