using System.Reflection;
using DiscordBotNet.LegendaryBot.Moves;

namespace DiscordBotNet.LegendaryBot.Entities.Items;

public class Item : Entity
{
    private static Type[] _default_types = { typeof(Skill), typeof(Special), typeof(Surge), typeof(Move), typeof(BasicAttack)};
    

    public bool IsDefault => _default_types.Contains(GetType());

    public static Dictionary<string, Type> AllDictionary = new();
    static Item()
    {
        foreach (Type i in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(Item)) && !t.IsAbstract))
        {
            AllDictionary.Add(BasicFunction.Englishify(i.Name), i);
        }

    }



    

    public virtual Tier Tier { get; } = Tier.Bronze;




    /// <summary>
    /// Creates and returns an item instance based by their name, spaced out
    /// </summary>


    public override bool Equals(object? obj)
    {
        if(obj is null) return false;
        if(obj is not Item other) return false; 
        return this == other;
    }

    public virtual string? Description { get; } = null;

    public override int GetHashCode()
    {
        int hashType = GetType().GetHashCode();


        return hashType;

    }


 


}