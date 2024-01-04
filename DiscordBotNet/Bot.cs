using System.Collections.Immutable;
using DSharpPlus.Entities;
using System.Diagnostics;
using System.Reflection;

using DiscordBotNet.Database;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.command;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DSharpPlus;

using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using DSharpPlus.VoiceNext;
using Microsoft.EntityFrameworkCore;
using Color = SixLabors.ImageSharp.Color;
using ConfigurationManager = System.Configuration.ConfigurationManager;


namespace DiscordBotNet;




public static class Bot
{

    public static ImmutableArray<BaseCommandClass> CommandInstanceSamples { get; private set; } = [];



    private static readonly IEnumerable<Type> AllAssemblyTypes = Assembly.GetExecutingAssembly()
        .GetTypes().ToImmutableArray();

    private static async Task DoShit()
    {
        
        Character.CalculateStat(800, 5000, 20).Print();
    }

    private static async Task Main(string[] args)
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

        CommandInstanceSamples = commandArrayType
            .Select(i => (BaseCommandClass)Activator.CreateInstance(i)!)
            .ToImmutableArray();

        var config = new DiscordConfiguration
        {
            Token = ConfigurationManager.AppSettings["BotToken"]!,
            Intents = DiscordIntents.All,
            AutoReconnect = true,
        };
        
        
        Client = new DiscordClient(config);
        
        var slashCommandsExtension = Client.UseSlashCommands();
        
        slashCommandsExtension.RegisterCommands(Assembly.GetExecutingAssembly());
        
        slashCommandsExtension.SlashCommandErrored += OnSlashCommandError;
        Client.UseVoiceNext(new VoiceNextConfiguration { AudioFormat = AudioFormat.Default});
        var interactivityConfiguration = new InteractivityConfiguration
        {
            Timeout = TimeSpan.FromMinutes(2),
        };
        Interactivity = Client.UseInteractivity(interactivityConfiguration);
        Client.SocketOpened += OnReady;
  
        await Client.ConnectAsync();

        await Website.Start(args);
    }
    public static  InteractivityExtension Interactivity { get; private set; }

    /// <summary>
    /// This is my discord user Id because it's too long to memorize
    /// </summary>
    public static long Izasid => 216230858783326209;
    /// <summary>
    /// this is the discord user Id of another account of mine that i use to test stuff
    /// </summary>
    public static long Testersid => 266157684380663809;

    public static long Surjidid => 1025325026955767849;
    public static DiscordClient Client { get; private set; }



    public static string GlobalFontName => "Arial";


    private static async  Task OnSlashCommandError(SlashCommandsExtension extension,SlashCommandErrorEventArgs ev)
    {
        Console.WriteLine(ev.Exception);

        List<DiscordUser> involvedUsers = [ev.Context.User];
        if (ev.Context.ResolvedUserMentions is not null)
            involvedUsers.AddRange(ev.Context.ResolvedUserMentions);
        var involvedIds = involvedUsers.Select(i => i.Id).ToArray();
        await using var databaseContext = new PostgreSqlContext();
        await databaseContext.UserData
            .Where(i => involvedIds.Contains((ulong)i.Id))
            .ForEachAsync(i => i.IsOccupied = false);
        var color = await databaseContext.UserData.FindOrCreateSelectAsync((long)ev.Context.User.Id, i => i.Color);
        await databaseContext.SaveChangesAsync();
        
        
        var embed = new DiscordEmbedBuilder()
            .WithColor(color)
            .WithTitle("hmm")
            .WithDescription("Something went wrong").Build();

        await ev.Context.Channel.SendMessageAsync(embed);
    }
    

    private static Task OnReady(DiscordClient client, SocketEventArgs e)
    {
        Console.WriteLine("Ready!");
        return Task.CompletedTask;
    }
}