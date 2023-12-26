using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Burn : StatusEffect
{
    private int _characterAttack;
    public override StatusEffectType EffectType => StatusEffectType.Debuff;
    public override bool HasLevels => false;
    public override bool ExecuteStatusEffectAfterTurn => false;
    public Burn( Character caster) : base(caster)
    {
        _characterAttack = caster.Attack;
    }


    public override void PassTurn(Character affected)
    {
        base.PassTurn(affected);
        affected.Damage(        new DamageArgs(this)
        {
       
            Damage = _characterAttack *3,
            Caster = Caster,
            CanCrit = false,
            DamageText =$"{affected} took $ damage from burn!"
        });

    }
}