using DiscordBotNet.Extensions;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.command;

public class Help : BaseCommandClass
{



    
    public override BotCommandType BotCommandType  => BotCommandType.Other;
    [SlashCommand("help","the help")]
    public async Task Execute(InteractionContext ctx,
    [Option("command","put if you want to check information about a command")] string? cmd = null)
    {
        DiscordUser author = ctx.User;

        DiscordColor color = await DatabaseContext.UserData.FindOrCreateSelectAsync(author.Id, i => i.Color);

        DiscordEmbedBuilder embedToBuild = new DiscordEmbedBuilder()
            .WithTitle("Help")
            .WithAuthor(author.Username, iconUrl: author.AvatarUrl)
            .WithColor(color)
            .WithTimestamp(DateTime.Now);
        if (cmd is null)
        {
            foreach (var i in Enum.GetValues<BotCommandType>())
            {
                
                string commandString = "";
                foreach (var j in Bot.CommandInstanceSamples
                             .Where(k => k.BotCommandType == i))
                {
                    commandString += $"`{j.Name}` ";
                }
                embedToBuild.AddField(i.ToString(), commandString);
            }
        }
        else
        {
            IEnumerable<BaseCommandClass> tempCommandArray = Bot.CommandInstanceSamples!.Where(i =>
                i.Name == cmd.ToLower()).ToArray();
            if (tempCommandArray.Any())
            {
                BaseCommandClass foundCommand = tempCommandArray.First();
                if (!foundCommand.HasSubCommands)
                {
                    embedToBuild.WithTitle($"**{foundCommand.Name}**");
                    embedToBuild.AddField("Description", foundCommand.Description);
                    embedToBuild.AddField("Example(s)", foundCommand.Example);
                }
                else
                {
                    embedToBuild.Title = foundCommand.Name;
                    embedToBuild.WithDescription(foundCommand.Description);
             
                    foreach (var i in _commandResults[foundCommand.GetType()])
                    {
                        embedToBuild.AddField(foundCommand.Name + " " + i.Name,
                            i.Description);
                    }
                    embedToBuild.AddField("Example(s)",foundCommand.Example);
                }

            } else
            {
                embedToBuild.WithTitle("**Hmm**").WithDescription("`That command does not exist`");
            }
                
        }
        await ctx.CreateResponseAsync(embed: embedToBuild.Build());

            
    }



       
}