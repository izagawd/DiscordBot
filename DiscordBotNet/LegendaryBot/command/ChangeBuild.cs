using System.Linq.Expressions;
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
[SlashCommandGroup("build","edits the builds of characters")]
public class CharacterBuildCommand : BaseCommandClass
{
    [SlashCommand("equip_build","changes the build of a character to another")]
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