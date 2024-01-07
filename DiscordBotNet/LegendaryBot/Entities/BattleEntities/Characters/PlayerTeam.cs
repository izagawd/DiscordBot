using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.Database.Models;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class PlayerTeam : CharacterTeam
{
    [NotMapped]
    public bool IsFull => Count >= 4;
    public string TeamName { get;  set; } = "default";

    public UserData UserData { get; protected set; }
    
    
    public long? EquippedUserDataId { get;  set; }
    public override bool Add(Character character)
    {
        if (Count >= 4) return false;
 
        if(UserDataId != 0)
            character.UserDataId = UserDataId;

        return base.Add(character);
    }


    public PlayerTeam(long tryGetUserDataId, string userName,params Character[] characters) : base(userName,characters)
    {
        UserDataId = tryGetUserDataId;
        
    }
    public PlayerTeam(DiscordUser user,params Character[] characters) : this((long)user.Id,user.Username,characters)
    {

    }
    public PlayerTeam(params Character[] characters) : base(characters)
    {

    }

    public PlayerTeam()
    {
        
    }
    public long Id { get; set; } 
    public long UserDataId { get; set; }
    
}