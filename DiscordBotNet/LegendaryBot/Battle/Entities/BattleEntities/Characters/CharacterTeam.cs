using System.Collections;
using System.Security.Claims;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

public class CharacterTeam : ISet<Character>
{
    /// <summary>
    /// If the owner of the team is a real person, this should be their discord id
    /// </summary>
    public ulong? UserId { get;  }
    /// <summary>
    /// The name of the owner of the team. Does not need to be a user.<br/>
    /// But if the owner of the team is a user, this must be set to their id. it probably would be unless I suck at coding 
    /// </summary>
    public string? UserName { get; } 
    public bool IsPlayerTeam => UserId is not null;
    /// <summary>
    /// increases exp of every character in the team and returns useful text
    /// </summary>
    /// <param name="exp"></param>
    /// <returns></returns>
    public string IncreaseExp(ulong exp)
    {
        var text = "";
        foreach (var i in this)
        {
            text += i.IncreaseExp(exp) +"\n";
        }

        return text;
    }
    public BattleSimulator CurrentBattle { get; set; }
    private HashSet<Character> Characters { get; }
    public IEnumerator<Character> GetEnumerator()
    {
        return Characters.GetEnumerator();
    }
    /// <summary>
    /// when a character is gotten from a database, the stats are not set in response to it's level sometimes. this does the trick for every character in the team
    /// </summary>
    /// <returns></returns>
    public async Task<CharacterTeam> LoadAsync()
    {
        foreach (var i in Characters)
        {
            i.Team = this;
            await i.LoadAsync();
        }
        
        return this;
    }

    public async Task<CharacterTeam> LoadAsync(DiscordUser user)
    {
        foreach (var i in Characters)
        {
            i.Team = this;
            if (i is Player player) await player.LoadAsync(user);
            else await i.LoadAsync();
        }
    
        return this;
    }

    /// <summary>
    /// True if all characters in the team have been loaded with LoadAsync. throws an error in BattleSimulator if false
    /// </summary>
    public bool IsLoaded => Characters.All(i => i.IsLoaded);
    public async Task<CharacterTeam> LoadAsync(ClaimsPrincipal user)
    {
        foreach (var i in Characters)
        {
            i.Team = this;
            if (i is Player player)
            {
                await player.LoadAsync(user);
            }
            else
            {
                await i.LoadAsync();
            }
        }


        return this;
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Add(Character character)
    {
        return Add(character, false);
    }

        /// <param name="character"></param>
        /// <param name="ignoreLoad">If true, will not throw exception if character is not loaded</param>
        /// <returns></returns>

    public bool Add(Character character, bool ignoreLoad)
    {
        if (!character.IsLoaded && !ignoreLoad)
            throw new Exception("Character that was attempted to be added to the team has not been loaded");
        character.Team = this;
        return Characters.Add(character);
    }


    public void ExceptWith(IEnumerable<Character> other)
    {
        Characters.ExceptWith(other);
    }

    public void IntersectWith(IEnumerable<Character> other)
    {
        Characters.IntersectWith(other);
    }

    public bool IsProperSubsetOf(IEnumerable<Character> other)
    {
        return Characters.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<Character> other)
    {
        return Characters.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<Character> other)
    {
        return Characters.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<Character> other)
    {
        return Characters.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<Character> other)
    {
        return Characters.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<Character> other)
    {
        return Characters.SetEquals(other);
    }

    public void SymmetricExceptWith(IEnumerable<Character> other)
    {
        Characters.SymmetricExceptWith(other);
    }

    public void UnionWith(IEnumerable<Character> other)
    {
        Characters.UnionWith(other);
    }


    void ICollection<Character>.Add(Character item)
    {
        Add(item);
    }

    public void Clear()
    {
        Characters.Clear();
    }

    public CharacterTeam(params Character[] characters)
    {

        Characters = characters.ToHashSet();
    }


    public CharacterTeam(ulong userId, string userName,params Character[] characters) : this(userName,characters)
    {
        UserId = userId;
        
    }
    public CharacterTeam(string userName,params Character[] characters) : this(characters)
    {
        UserName = userName;
    }
    public CharacterTeam(DiscordUser user,params Character[] characters) : this(user.Id,user.Username,characters)
    {

    }
    public bool Contains(Character character)
    {
        return Characters.Contains(character);
    }

    public void CopyTo(Character[] array, int arrayIndex)
    {
        Characters.CopyTo(array,arrayIndex);
    }

    public bool Remove(Character character)
    {
        return Characters.Remove(character);
    }

    public int Count => Characters.Count;
    public bool IsReadOnly { get; }
}