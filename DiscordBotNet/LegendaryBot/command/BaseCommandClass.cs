using System.Reflection;
using DiscordBotNet.LegendaryBot.Battle;
using DiscordBotNet.LegendaryBot.Database;
using DiscordBotNet.LegendaryBot.Database.Models;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.command;

public abstract class BaseCommandClass : ApplicationCommandModule
{
    protected static Dictionary<Type, IEnumerable<CommandResult>> _commandResults = new();
    protected static Dictionary<Type, SlashCommandGroupAttribute?> _slashCommandGroupAttributes = new();

    public string Name { get; }
    public string Description { get; }
    public bool HasSubCommands { get; } = false;
    public virtual BotCommandType BotCommandType { get;  } = BotCommandType.Other;
    protected CommandResult CommandResult { get; set; }
    public virtual string Example { get; } = "None";
    public BaseCommandClass()
    {
        Name = BasicFunction.Englishify(GetType().Name).ToLower();
        var type = GetType();
        var theCommandResultsOfThisInstance = _commandResults[type];
        if (_slashCommandGroupAttributes[type]  is null)
        {
           
            var firstResult = theCommandResultsOfThisInstance.First();
            Description = firstResult.SlashCommandAttribute.Description;
            Name = firstResult.SlashCommandAttribute.Name;
        }
        else
        {
            HasSubCommands = true;
            var commandGroupAttribute = _slashCommandGroupAttributes[type];
            Name = commandGroupAttribute.Name;
            Description = commandGroupAttribute.Description;
        }
    }
    private List<UserData> UserDatasToCheck { get; set; }
   
    /// <summary>
    /// loads and saves all the attributes of the command
    /// </summary>
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
                    var slashCommandAttribute = j.GetCustomAttribute<SlashCommandAttribute>()!;
                    var commandArgumentsAttribute = j.GetCustomAttribute<CommandArgsAttribute>();
           
  
                    if (commandArgumentsAttribute is null)
                    {
                        commandArgumentsAttribute = new CommandArgsAttribute();
                    }
                    return new CommandResult
                    {
                        CommandArgsAttribute = commandArgumentsAttribute,
                        SlashCommandAttribute = slashCommandAttribute,
          
                 
                    };
                });
            _commandResults.Add(i, typeEnumerables);

        }
        
    }

 
    protected PostgreSqlContext DatabaseContext { get;  } = new();

    public override async Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
    {
        var commandResults = _commandResults[GetType()];
        List<DiscordUser> usersToCheck = new List<DiscordUser>();
        usersToCheck.Add(ctx.User);
        if (ctx.ResolvedUserMentions is not null)
        {
            usersToCheck.AddRange(ctx.ResolvedUserMentions);
        }

        UserDatasToCheck =
            await DatabaseContext.UserData.FindOrCreateManyAsync(usersToCheck.Select(i => i.Id));
        if (UserDatasToCheck.Any(i => i.IsOccupied))
            
        {
            var embedToBuild = new DiscordEmbedBuilder()
                .WithUser(ctx.User)
                .WithColor(UserDatasToCheck.First(i => i.Id == ctx.User.Id).Color)
                .WithTitle("Hmm")
                .WithDescription(
                    "Seems like either you or someone you mentioned is occupied");
            await ctx.CreateResponseAsync(embedToBuild.Build());
            return false;
        }
        //QualifiedName shows the name of the command like <CommandName> if it is a sub command then it does it like
        //<CommandName> <SubCommandName>
        CommandResult = commandResults.First(i => i.SlashCommandAttribute.Name == ctx.QualifiedName.Split(" ").Last());
        
        if(CommandResult.CommandArgsAttribute is null) return true;
        var commandArguments = CommandResult.CommandArgsAttribute;
        if (commandArguments.RequiresBegun || commandArguments.MakesUsersOccupied)
        {

            var embedToBuild = new DiscordEmbedBuilder()
                .WithUser(ctx.User)
                .WithColor(UserDatasToCheck.First(i => i.Id == ctx.User.Id).Color)
                .WithTitle("Hmm");

            if (UserDatasToCheck.Any(i => i.Tier == Tier.Unranked) && commandArguments.RequiresBegun)
            {
                embedToBuild
                    .WithDescription(
                        "Seems like either you or someone you mentioned has not used the **begin** command");
                await ctx.CreateResponseAsync(embedToBuild.Build());
                return false;
            }
            
            if (commandArguments.MakesUsersOccupied)
            {
                UserDatasToCheck.ForEach(i => i.IsOccupied = true);
            }
            await DatabaseContext.SaveChangesAsync();
        }
        return true;
    }

    public override async Task AfterSlashExecutionAsync(InteractionContext ctx)
    {
        if (CommandResult.CommandArgsAttribute.MakesUsersOccupied)
        {
            UserDatasToCheck.ForEach(i => i.IsOccupied = false);
            await DatabaseContext.SaveChangesAsync();
        }

        await DatabaseContext.DisposeAsync();
    }
}