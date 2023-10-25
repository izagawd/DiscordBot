using DSharpPlus.Entities;
using System.Diagnostics;
using System.Reflection;
using DiscordBotNet.Database;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.command;

using DSharpPlus;

using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using DSharpPlus.VoiceNext;
using Microsoft.EntityFrameworkCore;
using ConfigurationManager = System.Configuration.ConfigurationManager;


namespace DiscordBotNet;



public class Bot
{

    public static BaseCommandClass[] CommandArray { get; private set; } = null!;
    public static readonly Type[] AllAssemblyTypes = Assembly.GetExecutingAssembly()
        .GetTypes().ToArray();


    private static async Task Main(string[] args) => await new Bot().RunBotAsync(args);


    /// <summary>
    /// This is my discord user Id because it's too long to memorize
    /// </summary>
    public static ulong Izasid => 216230858783326209;
    /// <summary>
    /// this is the discord user Id of another account of mine that i use to test stuff
    /// </summary>
    public static ulong Testersid => 266157684380663809;

    public static ulong Surjidid => 1025325026955767849;
    public static DiscordClient Client { get; private set; }

    public async Task DoShit()
    {
        
    }

    public static string GlobalFontName => "Arial";



    /// <summary>
    /// This is where the program starts
    /// </summary>
    private async Task RunBotAsync(string[] args)
    {
        
        var commandArrayType = AllAssemblyTypes.Where(t =>  t.IsSubclassOf(typeof(BaseCommandClass))).ToArray();
        var stopwatch = new Stopwatch(); 
        Console.WriteLine("Making all users unoccupied...");
        stopwatch.Start();

        await using (var ctx = new PostgreSqlContext())
        {
            await ctx.UserData
                .ForEachAsync(i => i.IsOccupied = false);
            var count = await ctx.UserData.CountAsync();
            await ctx.SaveChangesAsync();

            Console.WriteLine($"Took a total of {stopwatch.Elapsed.TotalMilliseconds}ms to make {count} users unoccupied");
        }

        
        CommandArray = Array.ConvertAll(commandArrayType, element => (BaseCommandClass)Activator.CreateInstance(element)!)!;
        var config = new DiscordConfiguration
        {
            Token = ConfigurationManager.AppSettings["BotToken"]!,
            Intents = DiscordIntents.All,
            AutoReconnect = true,
        };
        
        var client = new DiscordClient(config);
        Client = client;
        var slashCommandsExtension = client.UseSlashCommands();
        
        slashCommandsExtension.RegisterCommands(Assembly.GetExecutingAssembly());
        
        slashCommandsExtension.SlashCommandErrored += OnSlashCommandError;
        client.UseVoiceNext(new VoiceNextConfiguration { AudioFormat = AudioFormat.Default});
        var interactivityConfiguration = new InteractivityConfiguration
        {
            Timeout = TimeSpan.FromMinutes(2),
        };
        client.UseInteractivity(interactivityConfiguration);
        client.SocketOpened += OnReady;
  
        await client.ConnectAsync();

        await Website.Start(args);
        
    }


    private async  Task OnSlashCommandError(SlashCommandsExtension extension,SlashCommandErrorEventArgs ev)
    {
        Console.WriteLine(ev.Exception);
        await using var databaseContext = new PostgreSqlContext();
        var involvedUsers = new List<DiscordUser>();
        involvedUsers.Add(ev.Context.User);
        if (ev.Context.ResolvedUserMentions is not null)
            involvedUsers.AddRange(ev.Context.ResolvedUserMentions);
        var involvedIds = involvedUsers.Select(i => i.Id).ToArray();
        await databaseContext.UserData.Where(i => involvedIds.Contains(i.Id))
            .ForEachAsync(i =>
                i.IsOccupied = false);
        var color = await databaseContext.UserData.FindOrCreateSelectAsync(ev.Context.User.Id, i => i.Color);
        await databaseContext.SaveChangesAsync();
        var embed = new DiscordEmbedBuilder()
            .WithColor(color)
            .WithTitle("hmm")
            .WithDescription("Something went wrong");

        await ev.Context.Channel.SendMessageAsync(embed);
    }
    

    private Task OnReady(DiscordClient client, SocketEventArgs e)
    {

        Console.WriteLine("Ready!");
        return Task.CompletedTask;
    }
}