using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Bleed : StatusEffect
{
    public override string Description => "Does damage at the start of the caster's turn";
    public override bool ExecuteStatusEffectAfterTurn { get; } = false;
    private int _casterAttack;
    
    public override void PassTurn(Character affected)
    {
        base.PassTurn(affected);
        affected.Damage(new DamageArgs(this)
        {
            AffectedByCasterElement = false,
            Damage = _casterAttack,
            DamageText = $"{affected} took $ bleed damage!",
            Caster = Caster,

        });
    }

    public Bleed(Character caster) : base(caster)
    {
        _casterAttack = caster.Attack;
    }
}