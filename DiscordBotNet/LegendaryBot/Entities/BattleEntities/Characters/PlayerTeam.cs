using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class PlayerTeam : CharacterTeam
{
    [NotMapped]
    private DiscordUser _cachedDiscordUser { get; }
    public UserData UserData { get; protected set; }
    public override bool Add(Character character)
    {
        if (Count >= 4) return false;
 
        if(UserDataId != 0)
            character.UserDataId = UserDataId;

        return base.Add(character);
    }


    public PlayerTeam(ulong tryGetUserDataId, string userName,params Character[] characters) : base(userName,characters)
    {
        UserDataId = tryGetUserDataId;
        
    }
    public PlayerTeam(DiscordUser user,params Character[] characters) : this(user.Id,user.Username,characters)
    {
        _cachedDiscordUser = user;
    }
    public PlayerTeam(params Character[] characters) : base(characters)
    {

    }

    protected PlayerTeam()
    {
        
    }
    public Guid Id { get; set; } =  Guid.NewGuid();
    public ulong UserDataId { get; set; }
    
}