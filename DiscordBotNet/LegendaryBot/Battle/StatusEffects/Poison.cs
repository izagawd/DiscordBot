using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Results;

namespace DiscordBotNet.LegendaryBot.Battle.StatusEffects;

public class Poison : StatusEffect
{

    public override StatusEffectType EffectType => StatusEffectType.Debuff;
    public override bool HasLevels => false;
    public override bool ExecuteStatusEffectAfterTurn => false;
    public Poison( Character caster) : base(caster)
    {

    }


    public override void PassTurn(Character affected)
    {
        base.PassTurn(affected);

        DamageResult damageResult = affected.FixedDamage(        new DamageArgs()
        {
            StatusEffect = this,
            Damage = affected.MaxHealth * 0.05,
            Caster = Caster,
            CanCrit = false,
            DamageText =$"{affected} took $ damage from being poisoned!"
        });

    }
}