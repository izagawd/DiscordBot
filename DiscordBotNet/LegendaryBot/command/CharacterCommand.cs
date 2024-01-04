using System.Linq.Expressions;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.command;
[SlashCommandGroup("character","changes things about the character")]
public class CharacterCommand : BaseCommandClass
{
    
    static CharacterCommand()
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

    private static TextInputComponent modalText = new TextInputComponent("New Increase Amount", "change_increase_amount_value", "1");
    private static DiscordButtonComponent increaseAmountButton = new DiscordButtonComponent(ButtonStyle.Success, "change_increase_amount_button",
        "Change Increase Amount");

    private static DiscordButtonComponent reset = new DiscordButtonComponent(ButtonStyle.Danger, "reset", "Reset");
    private static DiscordButtonComponent stop = new DiscordButtonComponent(ButtonStyle.Danger, "stop", "Stop");
   
    [SlashCommand("increase_stat", "Raise a character's stats on their currently equipped build")]
    public async Task ExecuteIncreaseStat(InteractionContext ctx,[Option("name","the name of the character")] string name)
    {
        var robotifiedName = BasicFunction.Robotify(name);
        var userData = await DatabaseContext.UserData
            .Include(i => i.Inventory.Where(j => j is Character 
                                                 && EF.Property<string>(j,"Discriminator") == robotifiedName))
            .ThenInclude<UserData,Entity, CharacterBuild>(i =>
                (i as Character).EquippedCharacterBuild)
            .FindOrCreateAsync((long)ctx.User.Id);

        var color = userData.Color;
        
        var embed = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithColor(color)
            .WithTitle("Hmm")
            .WithDescription("Either that character does not exist or you do not have the  character, or both");

        var character = userData.Inventory.OfType<Character>().FirstOrDefault();
        if (character is null)
        {
            await ctx.CreateResponseAsync(embed);
            return;
        }

        if (userData.IsOccupied)
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
            .AddComponents(stop,reset,increaseAmountButton,increaseAmountLabel)
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
          if (result.Result.Id == stop.CustomId)
          {
              responseBuilder = new DiscordInteractionResponseBuilder()
                  .AddEmbed(embed);
              await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                  responseBuilder);
              break;
          }
          if (result.Result.Id == reset.CustomId)
          {
              characterBuild.ResetPoints();
              updated = true;
          }
          else if (result.Result.Id == increaseAmountButton.CustomId)
          {
              Task.Run(async () =>
              {
                  var localResponse = new DiscordInteractionResponseBuilder()
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
                          .AddComponents(stop,reset,increaseAmountButton, increaseAmountLabel);
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
                  .AddComponents(stop,reset,increaseAmountButton, increaseAmountLabel)
                  .AddFile("details.png", stream);
              await DatabaseContext.SaveChangesAsync();
              await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, responseBuilder);
              await stream.DisposeAsync();
              image.Dispose();
          }

        }

    }
    [SlashCommand("equip_blessing", "Makes a character equip a blessing")]
    public async Task ExecuteEquipBlessing(InteractionContext context,
        [Option("characterName","The name of the character")] string characterName,
        [Option("blessing_id", "The ID of the blessing")] long blessingId)
    {
        var simplifiedCharacterName = characterName.Replace(" ", "").ToLower();
        var userData = await DatabaseContext.UserData
            .Include(i => i.Inventory.Where(j => (j is Character
                                                  && EF.Property<string>(j, "Discriminator").ToLower() ==
                                                  simplifiedCharacterName) || (j is Blessing &&
                                                                               j.Id == blessingId)))
            .ThenInclude((Entity entity) => (entity as Blessing).Character)
            .FindOrCreateAsync((long)context.User.Id);


        var character = userData.Inventory.OfType<Character>()
            .FirstOrDefault(i => i.GetType().Name.ToLower() == simplifiedCharacterName);
        var embed = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithColor(userData.Color)
            .WithTitle("hmm")
            .WithDescription($"You don't have a character with name {characterName}");
        if (character is null)
        {
            await context.CreateResponseAsync(embed);
            return;
        }

        var blessing = userData.Inventory.OfType<Blessing>().FirstOrDefault(i => i.Id == blessingId);
        if (blessing is null)
        {
            embed.WithDescription($"You don't have a blessing of id {blessingId}");
            await context.CreateResponseAsync(embed);
            return;
        }

        character.Blessing = blessing;
        blessing.Character = character;
        
        await DatabaseContext.SaveChangesAsync();
        if (character is Player player) await player.LoadAsync(context.User, false);
        embed.WithTitle("Nice!")
            .WithDescription($"{character} has equipped {blessing}!");
        await context.CreateResponseAsync(embed);
    }
    
    [SlashCommand("change_build","changes the build of a character to another")]
    public async Task ExecuteEquipBuild(InteractionContext context,
        [Option("character_name", "The name of the character you want to change the build of")]
        string characterName)
    {

        var simplifiedCharacterName = characterName.ToLower().Replace(" ", "");
        Expression<Func<UserData,IEnumerable<Entity>>> includeLambda =i => i.Inventory.Where(j => j is Character
                                                                   && EF.Property<string>(j, "Discriminator")
                                                                       .ToLower() ==
                                                                   simplifiedCharacterName);
        var userData = await DatabaseContext.UserData
            .Include(includeLambda)
            .ThenInclude((Entity entity) => (entity as Character ).CharacterBuilds)
            .Include(includeLambda)
            .ThenInclude((Entity entity) => (entity as Character).Blessing)
            .FindOrCreateAsync((long)context.User.Id);

        var character = userData.Inventory.FirstOrDefault(i => i.GetType().Name.ToLower() == simplifiedCharacterName) as Character;
        var embed = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithColor(userData.Color)
            .WithTitle("Hmm")
            .WithDescription($"You do not have a character with the name {characterName}");
        if (character is null)
        {
            await context.CreateResponseAsync(embed);
            return;
        }

        List<DiscordButtonComponent> buildButtons = [];
        var index = 0;
        List<CharacterBuild> buildList = character.CharacterBuilds.OrderBy(i => i.BuildName).ToList();
        foreach (var i in buildList)
        {
            buildButtons.Add(new DiscordButtonComponent(ButtonStyle.Success,index.ToString(),(index + 1).ToString()));
            index++;
        }
        buildButtons.Add(new DiscordButtonComponent(ButtonStyle.Danger,"stop","STOP"));
        index = 0;
        if (character is Player player) await player.LoadAsync(context.User, false);
        using var image = await character.GetDetailsImageAsync();
        using var stream = new MemoryStream();
        await image.SaveAsPngAsync(stream);
        stream.Position = 0;
        embed.WithTitle("Select your build")
            .WithDescription($"{character}'s build")
            .WithImageUrl("attachment://detail.png");

        //all characters should have at least 4 builds. more than that is weird
        var responseBuilder = new DiscordInteractionResponseBuilder()
            .AddFile("detail.png", stream)
            .AddEmbed(embed)
 
            .AddComponents(buildButtons);
        await context.CreateResponseAsync(responseBuilder);
        var message = await context.GetOriginalResponseAsync();
        while (true)
        {
            var interactivityResult = await message.WaitForButtonAsync(context.User);
            if(interactivityResult.TimedOut) break;
            var decision = interactivityResult.Result.Id;
            if (decision == "stop")
            {
                await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder().AddEmbed(embed));
                return;
            }
            index = int.Parse(decision);
            character.EquippedCharacterBuild = buildList[index];
            await DatabaseContext.SaveChangesAsync();
            using var inImage = await character.GetDetailsImageAsync();
            await using var inStream = new MemoryStream();
            await inImage.SaveAsPngAsync(inStream);
            inStream.Position = 0;
            await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                new DiscordInteractionResponseBuilder()
                    .AddFile("detail.png", inStream)
                    .AddComponents(buildButtons)
                    .AddEmbed(embed)
                      );
            
        }
    }
    
}