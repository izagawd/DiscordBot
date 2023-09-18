using System.Diagnostics;
using DiscordBotNet.Database;
using DiscordBotNet.LegendaryBot.Battle;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.command;

public class Hunt : BaseCommandClass
{
    public override BotCommandType BotCommandType { get; } = BotCommandType.Adventure;

    [SlashCommand("hunt", "Hunt for mobs to get materials and be stronger!")]
    public async Task Execute(InteractionContext ctx,
        [Option("mob_name", "The name of the mob you want to hunt")] string characterName )
    {
        var author = ctx.User;

        var stop = new Stopwatch();
        var userData = await DatabaseContext.UserData
            .FindOrCreateAsync(author.Id,
                i => i.IncludeTeamWithAllEquipments());

        var embedToBuild = new DiscordEmbedBuilder()
            .WithUser(author)
            .WithTitle("Hmm")
            .WithColor(userData.Color)
            .WithDescription($"You cannot hunt because you have not yet become an adventurer with {Tier.Unranked}");
        if (userData.IsOccupied)
        {
            embedToBuild
                .WithDescription("You are occupied!");
            await ctx.CreateResponseAsync(embedToBuild);
            return;
        }

        if (userData.Tier == Tier.Unranked)
        {
            await ctx.CreateResponseAsync(embedToBuild.Build());
            return;
        }
        
        var characterType = Character.CharacterTypeList.FirstOrDefault(
            i =>  i.Name.ToLower() == characterName.ToLower().Replace(" ", "") && i.IsSubclassOf(typeof(Character)) && !i.IsRelatedToType(typeof(Player)));
        if (characterType is null)
        {
            embedToBuild =
                embedToBuild.WithDescription($"Mob {characterName} does not exist!");
            await ctx.CreateResponseAsync(embedToBuild.Build());
            return;
        }

        await MakeOccupiedAsync(userData);
        Character enemy =(Character) Activator.CreateInstance(characterType)!;

        embedToBuild = embedToBuild
            .WithTitle($"Keep your guard up!")
            .WithDescription($"A wild {enemy} has appeared!");
        await ctx.CreateResponseAsync(embedToBuild.Build());
        var message = await ctx.GetOriginalResponseAsync();
        var userTeam = await userData.Team.LoadAsync(author);
        enemy.SetLevel(userTeam.Select(i => i.Level).Average().Round());
        var simulator = new BattleSimulator(userTeam, await new CharacterTeam(enemy).LoadAsync());

 

        var battleResult = await simulator.Start(ctx.Interaction, message);
        var expToGain = BattleFunction.ExpGainFormula(enemy.Level);
        if (battleResult.Winners != userTeam)
        {
            expToGain /= 2;
        }
        string expGainText = userTeam.IncreaseExp(expToGain);
        embedToBuild = embedToBuild
            .WithTitle($"Nice going bud!")
            .WithDescription("You won!\n"+ expGainText);
        if (battleResult.Winners == userTeam)
        {
            await message.ModifyAsync(new DiscordMessageBuilder(){Embed = embedToBuild.Build()});
        }
        else
        {
            embedToBuild = embedToBuild
                .WithTitle($"Ah, too bad")
                .WithDescription($"You lost boii\n"+expGainText);
            await message.ModifyAsync(new DiscordMessageBuilder(){Embed = embedToBuild.Build()});
            
        }

        await DatabaseContext.SaveChangesAsync();

    }
}