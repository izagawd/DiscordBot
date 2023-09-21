using System.Reflection;
using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DiscordBotNet.LegendaryBot.Battle;
using DSharpPlus.Entities;
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
    public virtual string Example { get; } = "None";
    static BaseCommandClass()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var i in types.Where(i => i.IsSubclassOf(typeof(BaseCommandClass))))
        {

            _slashCommandGroupAttributes.Add(i, i.GetCustomAttribute<SlashCommandGroupAttribute>());

            var typeEnumerables = i.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(j =>
                {
            
                    return j.GetCustomAttribute<SlashCommandAttribute>() is not null;
                })
                .Select(j =>
                {
                    return j.GetCustomAttribute<SlashCommandAttribute>()!;
                    
                });
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
        foreach (var i in OccupiedUserDatas)
        {
            i.IsOccupied = false;
        }
        if(OccupiedUserDatas.Any())
        {
            var ids = OccupiedUserDatas.Select(i => i.Id).ToArray();
            var tempCtx = new PostgreSqlContext();
            await tempCtx.UserData
                .Where(i => ids.Contains(i.Id))
                .ForEachAsync(i => i.IsOccupied = false);
            await tempCtx.DisposeAsync();
        }
        await DatabaseContext.DisposeAsync();
    }

    protected List<UserData> OccupiedUserDatas { get; } = new();

    public async Task MakeOccupiedAsync(params UserData[] userDatas)
    {
        foreach (var i in userDatas)
        {
            i.IsOccupied = true;
        }
        OccupiedUserDatas.AddRange(userDatas);
        var ids = userDatas.Select(i => i.Id).ToArray();
        var tempCtx = new PostgreSqlContext();
        await tempCtx.UserData
            .Where(i => ids.Contains(i.Id))
            .ForEachAsync(i => i.IsOccupied = true);
        await tempCtx.SaveChangesAsync();
        tempCtx.DisposeAsync();
    }
    protected PostgreSqlContext DatabaseContext { get; private set; }
    public BaseCommandClass()
    {
        Name = BasicFunction.Englishify(GetType().Name).ToLower();
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