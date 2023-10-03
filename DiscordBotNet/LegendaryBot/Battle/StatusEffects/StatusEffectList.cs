using System.Collections;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Battle.StatusEffects;
public class StatusEffectList : ISet<StatusEffect>
{
    private HashSet<StatusEffect> statusEffects = new HashSet<StatusEffect>();

    public int Count => statusEffects.Count;
    public Character Affected { get;  }
    public bool IsReadOnly => false;
    public StatusEffectList(Character affected)
    {
        Affected = affected;
    }
/// <summary>
/// if using this, effect resistance of the affected will be ignored
/// </summary>
/// <returns>true if the status effect was successfully added</returns>
    public bool Add(StatusEffect item)
    {
        return Add(item, null);
    }
/// <summary>
/// if using this, effect resistance of the affected will be ignored
/// </summary>
/// <param name="effectiveness">the effectiveness of the caster</param>
/// <returns>true if the status effect was successfully added</returns>
    public bool Add(StatusEffect statusEffect,int? effectiveness)
    {
        if (Affected.IsDead) return false;
        List<StatusEffect> listOfType = this.Where(i => i.GetType() == statusEffect.GetType()).ToList();

        if (listOfType.Count < statusEffect.MaxStacks)
        {
            bool added = false;
            if (effectiveness is not null && statusEffect.EffectType == StatusEffectType.Debuff)
            {
                var percentToResistance =Affected.Resistance -effectiveness;
                
                if (percentToResistance < 0) percentToResistance = 0;
                if (!BasicFunction.RandomChance((int)percentToResistance))
                {
                    added = statusEffects.Add(statusEffect);
                }
                    
                
            }
            else
            {
                added = statusEffects.Add(statusEffect);
                
            }

            if (added)
            {
                Affected.CurrentBattle.AdditionalTexts.Add($"{statusEffect.Name} has been inflicted on {Affected}!");
            }
            else
            {
                Affected.CurrentBattle.AdditionalTexts.Add($"{Affected} resisted {statusEffect.Name}!");
            }

            return added;
        }
        if (!statusEffect.IsStackable && listOfType.Any() && statusEffect.IsRenewable)
        {
            StatusEffect onlyStatus = listOfType.First();
            if (statusEffect.Level > onlyStatus.Level)
            {
                onlyStatus.Level = statusEffect.Level;
            }
            if (statusEffect.Duration > onlyStatus.Duration)
            {
                onlyStatus.Duration = statusEffect.Duration;
            }
            onlyStatus.RenewWith(statusEffect);
            Affected.CurrentBattle.AdditionalTexts.Add($"{statusEffect.Name} has been optimized on {Affected}!");


            return true;
        }
        Affected.CurrentBattle.AdditionalTexts.Add($"{Affected} cannot take any more {statusEffect.Name}!");
        return false;
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

