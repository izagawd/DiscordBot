namespace DiscordBotNet;

public class RandomBullshit
{
    public static event Action<string> Die;
    public static void Run() => RunAsync().GetAwaiter().GetResult();

    public static void Penis(string idk)
    {
        Console.WriteLine($"wow {idk}");
    }


    public static void Drenis(string idk)
    {
        Console.WriteLine($"eded {idk}");
    }
    public static async  Task RunAsync()
    {


    }
    
}
