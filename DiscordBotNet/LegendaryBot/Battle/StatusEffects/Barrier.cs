using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Battle.StatusEffects;

public class Barrier : StatusEffect
{
    public override bool IsRenewable => true;
    public override int MaxStacks => 1;
    public override StatusEffectType EffectType => StatusEffectType.Buff;
    private int _shieldValue;
    /// <summary>
    /// using this method makes sure the shield isnt more than the max health
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    public int GetShieldValue(Character owner)
    {
        var maxHealth = owner.MaxHealth;
        if (_shieldValue <= maxHealth)
        {
            return _shieldValue;
        }

        _shieldValue = maxHealth;
        return maxHealth;
    }

    public void SetShieldValue(Character owner, int value)
    {
        if (value <= 0)
        {
            value = 0;
            owner.StatusEffects.Remove(this);
        }
        _shieldValue = value;
        
    }
    public Barrier(Character caster) : base(caster)
    {
        
    }
    public Barrier(Character caster, int shieldValue) : this(caster)
    {
        _shieldValue = shieldValue;
    }

}