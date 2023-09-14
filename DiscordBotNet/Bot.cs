
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


namespace DiscordBotNet;



public class Bot
{
    
    public static BaseCommandClass[]? CommandArray;
    public static readonly ImmutableList<Type> AllAssemblyTypes = Assembly.GetExecutingAssembly().GetTypes().ToImmutableList();


    private static void Main(string[] args) => new Bot().RunBotAsync(args).GetAwaiter().GetResult();

    public static SlashCommandsExtension SlashCommandsExtension { get; protected set; }
    public static ulong Izasid => 216230858783326209;
    public static ulong Testersid => 266157684380663809;
    public static DiscordClient Client { get; private set; }


    public List<Character> idk = new List<Character>();


    private async Task RunBotAsync(string[] args)
    {

        var commandArrayType = AllAssemblyTypes.Where(t =>  t.IsSubclassOf(typeof(BaseCommandClass))).ToArray();
        while (true)
        {
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
            Token = "MzQwMDU0NjEwOTg5NDE2NDYw.GlAoYq.Ld1L9YN2SoS713RS9YGbeNTfdRYzUOH6jknDE4",
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