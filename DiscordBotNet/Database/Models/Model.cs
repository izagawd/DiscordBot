

namespace DiscordBotNet.LegendaryBot.Database.Models;


public abstract class Model
{
    public override string ToString()
    {
        return GetType().Name;
    }



    public ulong Id { get; set; }








}