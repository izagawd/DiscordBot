using System.Collections.Immutable;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.command;
[SlashCommandGroup("stat_up","Raise the stats of a character")]
public class StatUp : BaseCommandClass
{
    private UserData _userData;

    static StatUp()
    {
        List<DiscordButtonComponent> rowOne = [];
        List<DiscordButtonComponent> rowTwo = [];
        var count = 0;
        var rowToUse = rowOne;
        foreach (var i in Enum.GetValues<StatType>())
        {
            count++;
            rowToUse.Add(new DiscordButtonComponent(ButtonStyle.Success, i.ToString(), BasicFunction.Englishify(i.ToString())));
            if (count >= 4)
            {
                rowToUse = rowTwo;
                count = -4;
;            }
        }

        _statsButtonsRowOne = new DiscordActionRowComponent(rowOne);
        _statsButtonsRowTwo = new DiscordActionRowComponent(rowTwo);
    }

    private static readonly DiscordActionRowComponent _statsButtonsRowOne;
    
    private static readonly DiscordActionRowComponent _statsButtonsRowTwo;
    [SlashCommand("by_name","get a character to raise their stats by their name")]
    public async Task ExecuteEditByName(InteractionContext ctx,[Option("name","the name of the character")] string name)
    {
        var robotified = BasicFunction.Robotify(name).Print();
        var userData = await DatabaseContext.UserData
            .Include(i => i.Inventory.Where(j => j is Character 
            && EF.Property<string>(j,"Discriminator") == robotified))
            .ThenInclude<UserData,Entity, CharacterBuild>(i =>
                (i as Character).EquippedCharacterBuild)
            .FindOrCreateAsync((long)ctx.User.Id);
        await HandleCharacter(ctx, userData.Inventory.OfType<Character>().FirstOrDefault(), userData.Color);
    }

    public async Task HandleCharacter(InteractionContext ctx, Character? character, DiscordColor color)
    {
        var embed = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithColor(color)
            .WithTitle("Hmm")
            .WithDescription("Either that character does not exist or you do not have the  character, or both");
        if (character is null)
        {
            await ctx.CreateResponseAsync(embed);
            return;
        }
        
        character.LoadBuild();
        if (character is Player player) await player.LoadAsync(false);


        var image = await character.GetDetailsImageAsync();

        var stream = new MemoryStream();

        await image.SaveAsPngAsync(stream);
        stream.Position = 0;
        embed
            .WithTitle($"{character.Name}'s stats")
            .WithDescription("Edit away!")
            .WithImageUrl("attachment://details.png");


        var responseBuilder = new DiscordInteractionResponseBuilder()
            .AddEmbed(embed)
            .AddComponents(_statsButtonsRowOne.Components)
            .AddComponents(_statsButtonsRowTwo.Components)
            .AddFile("details.png", stream);

        await ctx.CreateResponseAsync(responseBuilder);
        image.Dispose();
        await stream.DisposeAsync();
        var message = await ctx.GetOriginalResponseAsync();

        while (true)
        {
          var result =   await message.WaitForButtonAsync(ctx.User);
          if(result.TimedOut) break;

          if (character.EquippedCharacterBuild is null)
              throw new Exception("Character does not have a build equipped, which it shouldn't be");

          var statType = Enum.Parse<StatType>(result.Result.Id);
          if (character.EquippedCharacterBuild.TotalPoints < character.EquippedCharacterBuild.MaxPoints)
          {
              character.EquippedCharacterBuild.IncreasePoint(statType);
          }
          else
          {
              responseBuilder = new DiscordInteractionResponseBuilder()
                  .AddEmbed(embed);
              await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                  responseBuilder);
              break;
          }
          character.LoadBuild();
          image = await character.GetDetailsImageAsync();

          stream = new MemoryStream();
          await image.SaveAsPngAsync(stream);

          stream.Position = 0;
           responseBuilder = new DiscordInteractionResponseBuilder()
               .AddEmbed(embed)
               .AddComponents(_statsButtonsRowOne.Components)
               .AddComponents(_statsButtonsRowTwo.Components)
               .AddFile("details.png", stream);
           await DatabaseContext.SaveChangesAsync();
           await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, responseBuilder);
           await stream.DisposeAsync();
           image.Dispose();
        }
        
    }
    [SlashCommand("by_id", "get a character to raise their stats by id")]
    public async Task ExecuteEditById(InteractionContext ctx,[Option("id","the id of the charae")] long id)
    {

        var userData = await DatabaseContext.UserData
            .Include(i => i.Inventory.Where(j => j is Character 
                                                 && j.Id == id))
            .ThenInclude<UserData,Entity, CharacterBuild>(i =>
                (i as Character).EquippedCharacterBuild)
            .FindOrCreateAsync((long)ctx.User.Id);
        await HandleCharacter(ctx, userData.Inventory.OfType<Character>().FirstOrDefault() as Character, userData.Color);
    }
}