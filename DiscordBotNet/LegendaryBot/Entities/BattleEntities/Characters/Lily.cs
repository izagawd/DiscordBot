using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class ChamomileSachetWhack : BasicAttack
{
    public override string Description=> $"With the power of Chamomile, whacks an enemy with a sack filled with Chamomile, with a {SleepChance}% chance of making the enemy sleep";
    

    public int SleepChance => 40;
    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        var damageResult = target.Damage(new DamageArgs(this)
        {
            Damage = owner.Attack * 1.7,
            Caster = owner,
            CanCrit = true,
            DamageText = $"That was a harsh snoozy whack that dealt $ damage on {target}!",

        });
        var result = new UsageResult(this)
        {
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            User = owner,
            Text = "Chamomile Whack!",
            DamageResults = new []{damageResult}
        };
    

        if (BasicFunction.RandomChance(SleepChance))
        {
            target.StatusEffects.Add(new Sleep(owner), owner.Effectiveness);
        }
        return result;
    }
}
public class BlossomTouch : Skill
{
    public override int MaxCooldown => 3;

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.Team.Where(i =>!i.IsDead);
    }

    public int HealthHealScaling => 30;
  
    public override string Description =>  $"With the power of flowers, recovers the hp of an ally with {HealthHealScaling}% of the caster's max health, dispelling one debuff";
    
 
    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        target.RecoverHealth((owner.MaxHealth *HealthHealScaling* 0.01).Round());
        return new UsageResult(this)
        {
            Text = $"{owner} used Blossom Touch!",
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            User = owner
        };
    }
}
public class LilyOfTheValley : Surge
{
    public override int MaxCooldown  => 5;

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team && !i.IsDead);
    }

    public int PoisonInflictChance => 100;

    public int StunInflictChance => 50;
    public override  string Description => $"Releases a poisonous gas to all enemies, with an {StunInflictChance}% chance of inflicting stun for 1 turn and a {PoisonInflictChance}% chance of inflicting poison for one turn";
    

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        foreach (var i in GetPossibleTargets(owner))
        {
            if (BasicFunction.RandomChance(PoisonInflictChance))
            {
                i.StatusEffects.Add(new Poison(owner){Duration = 2}, owner.Effectiveness);

            }
            if (BasicFunction.RandomChance(StunInflictChance))
            {
                i.StatusEffects.Add(new Stun(owner){Duration = 1}, owner.Effectiveness);
            }
        }

        return new UsageResult(this)
        {
            Text =  $"{owner} used Lily of The Valley, and released a dangerous gas to the enemy team!",
            TargetType = TargetType.AOE,
            UsageType = usageType,
            User = owner
        };
    }
}
public class Lily : Character
{


    public override int BaseMaxHealth => 1500 + (60 * Level);
    public override int BaseAttack => (100 + (8 * Level));
    public override int BaseDefense => (100 + (7.2 * Level)).Round();
    public override int BaseSpeed => 115;
    
    public override Rarity Rarity { get; protected set; } = Rarity.FiveStar;
    public override DiscordColor Color { get; protected set; } = DiscordColor.HotPink;

    public override void NonPlayerCharacterAi(ref Character target, ref BattleDecision decision)
    {
        if (Surge.CanBeUsed(this))
        {
            decision = BattleDecision.Surge;
            target = Surge.GetPossibleTargets(this).First();
            return;
        }

        var teamMateWithLowestHealth = Team.OrderBy(i => i.Health).First();
        if (Skill.CanBeUsed(this) && teamMateWithLowestHealth.Health < teamMateWithLowestHealth.MaxHealth * 0.7)
        {
            decision = BattleDecision.Skill;
            target = teamMateWithLowestHealth;
            return;
        }

        decision = BattleDecision.BasicAttack;
        
        target = BasicAttack.GetPossibleTargets(this).OrderBy(i => i.Health).First();

    }

    public override Skill Skill { get;  } = new BlossomTouch();
    public override Surge Surge { get; } = new LilyOfTheValley();
    public override BasicAttack BasicAttack { get; } = new ChamomileSachetWhack();
    public override Element Element => Element.Earth;


}
