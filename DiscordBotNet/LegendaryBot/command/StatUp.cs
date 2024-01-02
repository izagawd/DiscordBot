using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DSharpPlus;
using DSharpPlus.Entities;
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
    private static TextInputComponent modalText = new TextInputComponent("New Increase Amount", "change_increase_amount_value", "1");
    private static DiscordButtonComponent increaseAmountButton = new DiscordButtonComponent(ButtonStyle.Success, "change_increase_amount_button",
        "Change Increase Amount");

    private static DiscordButtonComponent reset = new DiscordButtonComponent(ButtonStyle.Danger, "reset", "Reset");
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

        if (character.UserData.IsOccupied)
        {
            embed.WithDescription("You are occupied");
            await ctx.CreateResponseAsync(embed);
            return;
        }
        character.LoadBuild();
        if (character is Player player) await player.LoadAsync(false);

   
        var image = await character.GetDetailsImageAsync();

        var stream = new MemoryStream();
        var increaseAmountLabel = new DiscordButtonComponent(ButtonStyle.Secondary, "amount_increase_label", "Increase Amount: 1", true);
        await image.SaveAsPngAsync(stream);
        stream.Position = 0;
        embed
            .WithTitle($"{character.Name}'s stats")
            .WithDescription($"Remember: a stat can have a maximum of {CharacterBuild.MaxPointsPerStat} points")
            .WithImageUrl("attachment://details.png");

 
        var responseBuilder = new DiscordInteractionResponseBuilder()
            .AddEmbed(embed)
            .AddComponents(_statsButtonsRowOne.Components)
            .AddComponents(_statsButtonsRowTwo.Components)
            .AddComponents(reset,increaseAmountButton,increaseAmountLabel)
            .AddFile("details.png", stream);

        await ctx.CreateResponseAsync(responseBuilder);
        image.Dispose();
        await stream.DisposeAsync();
        var message = await ctx.GetOriginalResponseAsync();
        await MakeOccupiedAsync((long)ctx.User.Id);
        int increaseAmount = 1;
        while (true)
        {
          var result =   await message.WaitForButtonAsync(ctx.User);
          if(result.TimedOut) break;

          if (character.EquippedCharacterBuild is null)
              throw new Exception("Character does not have a build equipped, which it shouldn't be");
          bool updated = false;
          StatType statType; 
          var didParse = Enum.TryParse(result.Result.Id, out statType);
          var characterBuild = character.EquippedCharacterBuild;
          if (result.Result.Id == reset.CustomId)
          {
              characterBuild.ResetPoints();
              updated = true;
          }
          else if (result.Result.Id == increaseAmountButton.CustomId)
          {
              Task.Run(async () =>
              {
                  await using var localResponse = new DiscordInteractionResponseBuilder()
                      .WithTitle("Change the increase amount")
                      .WithCustomId("change_increase_amount")
                      .AddComponents(modalText);
                  await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.Modal,
                      localResponse);
                  var modalResult =  await ctx.Client.GetInteractivity()
                      .WaitForModalAsync("change_increase_amount", ctx.User);
                  if(modalResult.TimedOut) return;
                  var stringedNewAmount = modalResult.Result.Values.Select(i => i.Value).First();
                  var parsed =int.TryParse(stringedNewAmount, out int newAmount);
                  if (!parsed)
                  {
                      await modalResult.Result.Interaction.CreateResponseAsync(
                          InteractionResponseType.ChannelMessageWithSource,
                          new DiscordInteractionResponseBuilder()
                              .WithContent("Input a whole number thats at least 1!"));
                  }
                  else
                  {
                      if (newAmount <= 0) newAmount = 1;
                      increaseAmount = newAmount;

                      increaseAmountLabel = new DiscordButtonComponent(increaseAmountLabel.Style,
                          increaseAmountLabel.CustomId, $"Increase Amount: {increaseAmount}", true);

                      await modalResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                      var messageBuilder = new DiscordMessageBuilder()
                          .AddEmbed(embed)
                          .AddComponents(_statsButtonsRowOne.Components)
                          .AddComponents(_statsButtonsRowTwo.Components)
                          .AddComponents(reset,increaseAmountButton, increaseAmountLabel);
                      message =await message.ModifyAsync(messageBuilder);
                  }
              });
              continue;
          }
          else if (characterBuild.TotalPoints < characterBuild.MaxPoints 
              && characterBuild[statType] < CharacterBuild.MaxPointsPerStat)
          {
              updated = true;
              foreach (var i in Enumerable.Range(0,increaseAmount))
              {
                  if( characterBuild.TotalPoints >= characterBuild.MaxPoints)
                     break;
                  if(characterBuild[statType] + 1 > CharacterBuild.MaxPointsPerStat)
                      break;
                  characterBuild.IncreasePoint(statType);

              }
    
             
          }
          else
          {
              if (characterBuild.TotalPoints >= characterBuild.MaxPoints)
              {
                 
                  responseBuilder = new DiscordInteractionResponseBuilder()
                      .AddEmbed(embed);
                  await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                      responseBuilder);
                  break;
              } 
              if (characterBuild[statType] >= CharacterBuild.MaxPointsPerStat)
              {
   
                  await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                      new DiscordInteractionResponseBuilder()
                          .AsEphemeral()
                          .WithContent($"stat type {BasicFunction.Englishify(statType.ToString())} has already reached it's maximum amount of points"));
              }

          }
            
          if (updated)
          {
              character.LoadBuild();
              image = await character.GetDetailsImageAsync();

              stream = new MemoryStream();
              await image.SaveAsPngAsync(stream);

              stream.Position = 0;
              responseBuilder = new DiscordInteractionResponseBuilder()
                  .AddEmbed(embed)
                  .AddComponents(_statsButtonsRowOne.Components)
                  .AddComponents(_statsButtonsRowTwo.Components)
                  .AddComponents(reset,increaseAmountButton, increaseAmountLabel)
                  .AddFile("details.png", stream);
              await DatabaseContext.SaveChangesAsync();
              await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, responseBuilder);
              await stream.DisposeAsync();
              image.Dispose();
          }

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
        await HandleCharacter(ctx, userData.Inventory.OfType<Character>().FirstOrDefault() , userData.Color);
    }
}