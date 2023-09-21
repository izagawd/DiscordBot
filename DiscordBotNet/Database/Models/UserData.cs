using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.LegendaryBot;
using DiscordBotNet.LegendaryBot.Battle;
using DiscordBotNet.LegendaryBot.Battle.Entities;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Results;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DiscordBotNet.Database.Models;


public class UserData : Model,  ICanBeLeveledUp
{
    public UserData(ulong id)
    {
        Id = id;
    }
    public UserData(){}


    [NotMapped]
    public IEnumerable<Guid> Ids
    {
        get
        {
            foreach (var i in Team)
            {
                yield return i.Id;
            }
        }
    }

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
    public void AddToTeam(Character character)
    {
        if(Team.Contains(character)) return;
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
        
    }
    [NotMapped]
    
    public CharacterTeam Team {
        get
        {
            CharacterTeam team = new CharacterTeam(Id);
            Character?[] characterList = {Character1,Character2,Character3,Character4};
            foreach (var i in characterList)
            {
                if (i is not null)
                    team.Add(i);
            }
            return team;
        }
        
    }
    
    public async Task<Image<Rgba32>> GetInfoAsync(DiscordUser? user = null)
    {
        if (user is null)
        {
            user = await Bot.Client.GetUserAsync(Id);
        } else if (user.Id != Id)
        {
            throw new Exception("Bruh");
        }
        


        var userImage = await BasicFunction.GetImageFromUrlAsync(user.AvatarUrl);
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
      ;
            ctx.Fill(SixLabors.ImageSharp.Color.Gray, new RectangleF(130, levelBarY, levelBarMaxLevelWidth, 30));
            ctx.Fill(SixLabors.ImageSharp.Color.Green, new RectangleF(130, levelBarY, gottenExp, 30));
            ctx.Draw(SixLabors.ImageSharp.Color.Black, 3, new RectangleF(130, levelBarY, levelBarMaxLevelWidth, 30));
            var font = SystemFonts.CreateFont("Arial", 25);
            
            ctx.DrawText($"{Experience}/{GetRequiredExperienceToNextLevel()}",font,SixLabors.ImageSharp.Color.Black,new PointF(140,levelBarY));
        
            ctx.DrawText($"Level {Level}",font,SixLabors.ImageSharp.Color.Black,new PointF(140,levelBarY - 33));
           
        });

        return image;
    }

    public DateTime StartTime { get; protected set; } = DateTime.UtcNow;
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

    public ExperienceGainResult IncreaseExp(ulong exp)
    {
        throw new NotImplementedException();
    }

    public ulong Coins { get; set; } = 5000;

    public List<Guid?> CharacterIdList =>
        new () { Character1Id, Character2Id, Character3Id, Character4Id };
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
