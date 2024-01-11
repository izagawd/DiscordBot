using System.Collections;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.StatusEffects;
public class StatusEffectSet : ISet<StatusEffect>
{
    private readonly HashSet<StatusEffect> statusEffects = new HashSet<StatusEffect>();

    public int Count => statusEffects.Count;
    
    /// <summary>
    /// The character who has the status effects
    /// </summary>
    public Character Affected { get;  }
    public bool IsReadOnly => false;
    public StatusEffectSet(Character affected)
    {
        Affected = affected;
    }
/// <summary>
/// if using this, effect resistance of the affected will be ignored
/// </summary>
/// <returns>true if the status effect was successfully added</returns>
    public bool Add(StatusEffect item)
    {
        return statusEffects.Add(item);
    }

  




    public void Clear()
    {
        statusEffects.Clear();
    }

    public bool Contains(StatusEffect item)
    {
        return statusEffects.Contains(item);
    }

    public void CopyTo(StatusEffect[] array, int arrayIndex)
    {
        statusEffects.CopyTo(array, arrayIndex);
    }

    public void ExceptWith(IEnumerable<StatusEffect> other)
    {
        statusEffects.ExceptWith(other);
    }

    public IEnumerator<StatusEffect> GetEnumerator()
    {
        return statusEffects.GetEnumerator();
    }

    public void IntersectWith(IEnumerable<StatusEffect> other)
    {
        statusEffects.IntersectWith(other);
    }

    public bool IsProperSubsetOf(IEnumerable<StatusEffect> other)
    {
        return statusEffects.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<StatusEffect> other)
    {
        return statusEffects.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<StatusEffect> other)
    {
        return statusEffects.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<StatusEffect> other)
    {
        return statusEffects.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<StatusEffect> other)
    {
        return statusEffects.Overlaps(other);
    }

    public bool Remove(StatusEffect item)
    {
        return statusEffects.Remove(item);
    }

    public bool SetEquals(IEnumerable<StatusEffect> other)
    {
        return statusEffects.SetEquals(other);
    }

    public void SymmetricExceptWith(IEnumerable<StatusEffect> other)
    {
        statusEffects.SymmetricExceptWith(other);
    }

    public void UnionWith(IEnumerable<StatusEffect> other)
    {
        statusEffects.UnionWith(other);
    }

    void ICollection<StatusEffect>.Add(StatusEffect item)
    {
        statusEffects.Add(item);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

