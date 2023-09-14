
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using DiscordBotNet.LegendaryBot;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.command;
using DiscordBotNet.LegendaryBot.Database;
using DiscordBotNet.LegendaryBot.Database.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using DSharpPlus.VoiceNext;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using ConfigurationManager = System.Configuration.ConfigurationManager;


namespace DiscordBotNet;



public class Bot
{
    
    public static BaseCommandClass[]? CommandArray;
    public static readonly ImmutableList<Type> AllAssemblyTypes = Assembly.GetExecutingAssembly().GetTypes().ToImmutableList();


    private static void Main(string[] args) => new Bot().RunBotAsync(args).GetAwaiter().GetResult();

    public static SlashCommandsExtension SlashCommandsExtension { get; protected set; }
    /// <summary>
    /// This is my discord user Id because it's too long to memorize
    /// </summary>
    public static ulong Izasid => 216230858783326209;
    /// <summary>
    /// this is the discord user Id of another account of mine that i use to test stuff
    /// </summary>
    public static ulong Testersid => 266157684380663809;
    public static DiscordClient Client { get; private set; }

    /// <summary>
    /// this is where the program starts
    /// </summary>
    private async Task RunBotAsync(string[] args)
    {

        var commandArrayType = AllAssemblyTypes.Where(t =>  t.IsSubclassOf(typeof(BaseCommandClass))).ToArray();
        while (true)
        {
            //sometimes BasicFunction.LoadAsync throws an error so it repeats
            // when an exception is thrown
            try
            {
                await BasicFunction.LoadAsync();
                break;
            }
            catch
            {
                // ignored
            }
        }
        
        var ctx = new PostgreSqlContext();

        await ctx.UserData.ForEachAsync(i => i.IsOccupied = false);
        await ctx.SaveChangesAsync();
        
        await ctx.DisposeAsync();
        BasicFunction.imageMapper.Count.Print();
        CommandArray = Array.ConvertAll(commandArrayType, element => (BaseCommandClass)Activator.CreateInstance(element)!)!;
        var config = new DiscordConfiguration
        {
            Token = ConfigurationManager.AppSettings["BotToken"]!,
            Intents = DiscordIntents.All,
            AutoReconnect = true,
        };
        
        var client = new DiscordClient(config);
        Client = client;
        SlashCommandsExtension = client.UseSlashCommands();
        
        SlashCommandsExtension.RegisterCommands(Assembly.GetExecutingAssembly());
        
        SlashCommandsExtension.SlashCommandErrored += OnSlashCommandError;
        client.UseVoiceNext(new VoiceNextConfiguration { AudioFormat = AudioFormat.Default});
        var interactivityConfiguration = new InteractivityConfiguration
        {
            Timeout = TimeSpan.FromMinutes(2)
        };
        client.UseInteractivity(interactivityConfiguration);
        client.SocketOpened += OnReady;
        await client.ConnectAsync();
        await Website.Start(args);

    }


    private async  Task OnSlashCommandError(SlashCommandsExtension extension,SlashCommandErrorEventArgs ev)
    {
        Console.WriteLine(ev.Exception);
        var databaseContext = new PostgreSqlContext();
        var involvedUsers = new List<DiscordUser>();
        involvedUsers.Add(ev.Context.User);
        if (ev.Context.ResolvedUserMentions is not null)
        {
            involvedUsers.AddRange(ev.Context.ResolvedUserMentions);
        }
        
        var involvedIds = involvedUsers.Select(i => i.Id).ToList();
        await databaseContext.UserData.Where(i => involvedIds.Contains(i.Id))
            .ForEachAsync(i =>
            {
                i.IsOccupied = false;
            });
        await databaseContext.SaveChangesAsync();
        await databaseContext.DisposeAsync();
        await ev.Context.Channel.SendMessageAsync(new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Green)
            .WithTitle("hmm")
            .WithDescription("Something went wrong"));
    }
    

    private Task OnReady(DiscordClient client, SocketEventArgs e)
    {
        foreach(var i in client.Guilds.Values)
        {

            i.BulkOverwriteApplicationCommandsAsync(new List<DiscordApplicationCommand>());
        }
        Console.WriteLine("Ready!");
        return Task.CompletedTask;
    }
}