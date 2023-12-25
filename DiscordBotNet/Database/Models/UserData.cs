using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Quests;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.Rewards;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBotNet.Database.Models;


public class UserData : Model,  ICanBeLeveledUp
{


    public UserData(ulong id)
    {
        Id = id;
    }
    public UserData(){}

    
    [NotMapped] public IEnumerable<Guid> Ids => CharacterTeamArray.Select(i => i.Id);
    public List<QuoteReaction> QuoteReactions { get; set; } = new();

    public void RemoveFromTeam(Character character)
    {
        if (Character1 == character)
        {
            Character1 = null;
        }
        else if (Character2 == character)
        {
            Character2 = null;
        }
        else if (Character3 == character)
        {
            Character3 = null;
        }
        else if (Character4 == character)
        {
            Character4 = null;
        }
    }
    [NotMapped]
    public Character[] CharacterTeamArray =>
        new [] { Character1, Character2, Character3, Character4 }
            .Where(i => i is not null).OfType<Character>().ToArray();
    public void AddToTeam(Character character)
    {
        if(CharacterTeamArray.Contains(character)) return;
        if (Character1 is null)
        {
            Character1 = character;
        }
        else if (Character2 is null)
        {
            Character2 = character;
        }
        else if (Character3 is null)
        {
            Character3 = character;
        }
        else if (Character4 is null)
        {
            Character4 = character;
        }

        character.UserDataId = Id;
        character.UserData = this;
    }
    
    /// <param name="userName">The username of the owner of the team</param>
    public CharacterTeam GetCharacterTeam(string userName)
    {
        CharacterTeam team = new CharacterTeam(Id,userName);
        Character?[] characterArray = {Character1,Character2,Character3,Character4};
        foreach (var i in characterArray)
        {
            if (i is not null)
                team.Add(i);
        }
        return team;
    }

    public CharacterTeam GetCharacterTeam(DiscordUser user) => GetCharacterTeam(user.Username);
    public async Task<Image<Rgba32>> GetInfoAsync(DiscordUser? user = null)
    {
        if (user is null)
        {
            user = await Bot.Client.GetUserAsync(Id);
        } 
        else if (user.Id != Id)
        {
            throw new Exception("discord user's ID does not match user data's id");
        }
        using var userImage = await BasicFunction.GetImageFromUrlAsync(user.AvatarUrl);
        var image = new Image<Rgba32>(500, 150);
        userImage.Mutate(ctx => ctx.Resize(new Size(100,100)));
        image.Mutate(ctx =>
        {
            ctx.BackgroundColor(Color.ToImageSharpColor());
            var userImagePoint = new Point(20, 20);
            ctx.DrawImage(userImage,userImagePoint, new GraphicsOptions());
            ctx.Draw(SixLabors.ImageSharp.Color.Black, 3, new RectangleF(userImagePoint,userImage.Size));
            var levelBarMaxLevelWidth = 300ul;
            var gottenExp = levelBarMaxLevelWidth * (Experience/(GetRequiredExperienceToNextLevel() * 1.0f));
            var levelBarY = userImage.Height - 30 + userImagePoint.Y;
            ctx.Fill(SixLabors.ImageSharp.Color.Gray, new RectangleF(130, levelBarY, levelBarMaxLevelWidth, 30));
            ctx.Fill(SixLabors.ImageSharp.Color.Green, new RectangleF(130, levelBarY, gottenExp, 30));
            ctx.Draw(SixLabors.ImageSharp.Color.Black, 3, new RectangleF(130, levelBarY, levelBarMaxLevelWidth, 30));
            var font = SystemFonts.CreateFont("Arial", 25);
            ctx.DrawText($"{Experience}/{GetRequiredExperienceToNextLevel()}",font,SixLabors.ImageSharp.Color.Black,new PointF(140,levelBarY));
            ctx.DrawText($"Level {Level}",font,SixLabors.ImageSharp.Color.Black,new PointF(140,levelBarY - 33));
        });

        return image;
    }
    /// <summary>
    /// Time that this player started their journey
    /// </summary>
    public DateTime StartTime { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Call this method if you want this instance to update it's values if it should be updated because it is a new day
    /// 
    /// </summary>
    public void CheckForNewDay()
    {
        var currentDateTime = DateTime.UtcNow;
        if (currentDateTime.Date != LastTimeChecked.Date)
        {
            GenerateQuests();

        }

        LastTimeChecked = currentDateTime;
    }

    public void GenerateQuests()
    {
        Quests.Clear();
        
        foreach (var i in Quest.ExampleQuests)
        {
            if(Quests.Select(j => j.GetType()).Contains(i.GetType())) continue;
            Quests.Add((Quest) Activator.CreateInstance(i.GetType())!);
        }
    }

    public List<Quest> Quests { get; set; } = [];
    /// <summary>
    /// This is used to refresh stuff like daily quests
    /// </summary>
    public DateTime LastTimeChecked { get; private set; } = DateTime.UtcNow.AddDays(-1);
    public List<Quote> Quotes { get; protected set; } = new();
    public bool IsOccupied { get; set; } = false;
    public ulong Experience { get; protected set; }
    public ulong GetRequiredExperienceToNextLevel(int level)
    {
        return BattleFunction.NextLevelFormula(level) * 10;
    }
    public ulong GetRequiredExperienceToNextLevel()
    {
        return GetRequiredExperienceToNextLevel(Level);
    }
    /// <summary>
    /// Receives rewards
    /// </summary>
    /// <param name="name">the name of the user</param>
    /// <param name="rewards">the rewards</param>
    /// <returns>the receive rewards text</returns>
    public string ReceiveRewards(string name, params Reward[] rewards)
    {
        var rewardStringBuilder = new StringBuilder();
        foreach (var i in rewards)
        {
            rewardStringBuilder.Append($"{i.GiveRewardTo(this, name)}\n");

        }

        return rewardStringBuilder.ToString();
    }
    /// <summary>
    /// Receives rewards
    /// </summary>
    /// <param name="name">the name of the user</param>
    /// <param name="rewards">the rewards</param>
    /// <returns>the receive rewards text</returns>
    public string ReceiveRewards(string name,IEnumerable<Reward> rewards)
    {
        return ReceiveRewards(name,rewards.ToArray());
    }
    public ExperienceGainResult IncreaseExp(ulong exp)
    {


        throw new NotImplementedException();
    }

    public ulong StandardPrayers { get; set; } = 0;
    
    public ulong SupremePrayers { get; set; } = 0;
    public ulong Coins { get; set; } = 5000;
    public List<Guid?> CharacterIdList => [Character1Id, Character2Id, Character3Id, Character4Id];
    public Guid? Character1Id { get; set; }
    public Character? Character1 { get; set; }
    public Guid? Character2Id { get; set; }
    public Character? Character2 { get; set; }
    public Guid? Character3Id { get; set; }
    public Character? Character3 { get; set; }
    
    public Guid? Character4Id { get; set; }
    public Character? Character4 { get; set; }
    public Tier Tier { get; set; } = Tier.Unranked;
    public int Level { get; set; } = 1;
    public DiscordColor Color { get; set; } = DiscordColor.Green;
    public string Language { get; set; } = "english";
    public List<Entity> Inventory { get; protected set; } = new();
    
}
