using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Bleed : StatusEffect
{
    public override string Description => "Does damage proportional to the caster's attack to the affected at the start of the affected's turn." +
                                          " Ignores 70% of the affecteed's defense";
    public override bool ExecuteStatusEffectAfterTurn { get; } = false;
    public int Attack { get;  }
    public DamageResult Detonate(Character affected)
    {
        affected.StatusEffects.Remove(this);
        return affected.Damage(        new DamageArgs(this)
        {
            DefenseToIgnore = 70,
            AffectedByCasterElement = false,
            Damage = Attack * 3,
            Caster = Caster,
            CanCrit = false,
            DamageText = $"{affected}'s bleeding splashed and dealt $ damage!"
        });
   
    }
    public override void PassTurn(Character affected)
    {
        base.PassTurn(affected);
        affected.Damage(new DamageArgs(this)
        {
            AffectedByCasterElement = false,
            Damage = Attack,
            DamageText = $"{affected} took $ bleed damage!",
            Caster = Caster,
        });
    }

    public Bleed(Character caster) : base(caster)
    {
        Attack = caster.Attack;
    }
}