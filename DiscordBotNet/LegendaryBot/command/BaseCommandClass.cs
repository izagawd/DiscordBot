using System.Reflection;
using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.command;

public abstract class BaseCommandClass : ApplicationCommandModule
{

    protected static Dictionary<Type, IEnumerable<SlashCommandAttribute>> _commandResults = new();
    protected static Dictionary<Type, SlashCommandGroupAttribute?> _slashCommandGroupAttributes = new();

    public string Name { get; }
    public string Description { get; }
    public bool HasSubCommands { get; } = false;
    public virtual BotCommandType BotCommandType { get;  } = BotCommandType.Other;
    protected SlashCommandAttribute CommandResult { get; set; }
    public virtual string Example => "None";
    static BaseCommandClass()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var i in types.Where(i => i.IsSubclassOf(typeof(BaseCommandClass)) && !i.IsAbstract))
        {

            _slashCommandGroupAttributes.Add(i, i.GetCustomAttribute<SlashCommandGroupAttribute>());

            var typeEnumerables = i.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(j => j.GetCustomAttribute<SlashCommandAttribute>() is not null)
                .Select(j => j.GetCustomAttribute<SlashCommandAttribute>()!);
            _commandResults.Add(i, typeEnumerables);

        }
        
    }

    public override Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
    {
        DatabaseContext = new PostgreSqlContext();
        
        
        return Task.FromResult(true);
    }

    public override async Task AfterSlashExecutionAsync(InteractionContext ctx)
    {

        if(OccupiedUserDatasIds.Count > 0)
        {
         
            await using var tempCtx = new PostgreSqlContext();
            await tempCtx.UserData
                .Where(i => OccupiedUserDatasIds.Contains(i.Id))
                .ForEachAsync(i => i.IsOccupied = false);
            await tempCtx.SaveChangesAsync();
 
        }
        await DatabaseContext.DisposeAsync();
    }

    private List<long> OccupiedUserDatasIds { get; } = new();

    protected async Task MakeOccupiedAsync(params long[] userDataIds)
    {

        await using var tempCtx = new PostgreSqlContext();
        await tempCtx.UserData
            .Where(i => userDataIds.Contains(i.Id))
            .ForEachAsync(i => i.IsOccupied = true);
        
        OccupiedUserDatasIds.AddRange(userDataIds);
        await tempCtx.SaveChangesAsync();
    }
    protected Task MakeOccupiedAsync(params UserData[] userDatas)
    {
        foreach (var i in userDatas)
        {
            i.IsOccupied = true;
        }

        return MakeOccupiedAsync(userDatas.Select(i => i.Id).ToArray());
    }
    /// <summary>
    /// This exists cuz it's disposed at the end of a slash command and cuz I tend to forget to dispose disposable stuff
    /// </summary>
    protected PostgreSqlContext DatabaseContext { get; private set; }
    public BaseCommandClass()
    {
        Name = BasicFunctionality.Englishify(GetType().Name).ToLower();
        var type = GetType();
        var theCommandResultsOfThisInstance = _commandResults[type];
        if (_slashCommandGroupAttributes[type]  is null)
        {
           
            var firstResult = theCommandResultsOfThisInstance.First();
            Description = firstResult.Description;
            Name = firstResult.Name;
        }
        else
        {
            HasSubCommands = true;
            var commandGroupAttribute = _slashCommandGroupAttributes[type];
            Name = commandGroupAttribute.Name;
            Description = commandGroupAttribute.Description;
           
        }
    }


}