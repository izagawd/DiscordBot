namespace DiscordBotNet.Database.Models;


public abstract class Model
{
    public override string ToString()
    {
        return GetType().Name;
    }



    public ulong Id { get; init; }








}