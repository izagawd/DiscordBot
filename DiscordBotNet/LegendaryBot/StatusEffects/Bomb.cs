using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Bomb : StatusEffect, IDetonatable
{
    
    public override string Description => "Detonates on the affected when the duration of this status effect finishes." +
                                          " Detonation damage is proportional to the caster's attack. After detonation, the affected gets stunned";
    public override StatusEffectType EffectType => StatusEffectType.Debuff;
    public int Attack { get; }
    public Bomb(Character caster) : base(caster)
    {
        Attack = caster.Attack;
    }

    public override void PassTurn(Character affected)
    {
        base.PassTurn(affected);
        if (Duration == 0)
        {
            Detonate(affected, Caster);
        }

        
    }

    public DamageResult? Detonate(Character affected, Character detonator)
    {
        affected.RemoveStatusEffect(this);
        affected.AddStatusEffect(new Stun(detonator));
        return affected.Damage(        new DamageArgs(this)
        {
            AffectedByCasterElement = false,
            Damage = Attack * 3,
            Caster = Caster,
            CanCrit = false,
            DamageText = $"Bomb detonated on {affected} and dealt $ damage!"
        });
   
    }
    public override bool ExecuteStatusEffectAfterTurn => false;
}