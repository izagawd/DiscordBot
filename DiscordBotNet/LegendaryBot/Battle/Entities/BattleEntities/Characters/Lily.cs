using DiscordBotNet.LegendaryBot.Battle.Moves;
using DiscordBotNet.LegendaryBot.Battle.Results;
using DiscordBotNet.LegendaryBot.Battle.StatusEffects;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

public class ChamomileSachetWhack : BasicAttack
{
    public override string GetDescription(int level)
    {
        return $"With the power of Chamomile, whacks an enemy with a sack filled with Chamomile, with a {GetSleepChance(level)}% chance of making the enemy sleep";
    }
    public override int MaxEnhance { get; } = 4;
    public int GetSleepChance(int level)
    {
        switch (level)
        {
            case 0:
                return 15;
            case 1:
            case 2:    
                return 20;
            case 3:
                return 30;
            default:
                return 40;
        }
    }
    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {     

        var result = new UsageResult(usageType, TargetType.SingleTarget);
        result.DamageResults.Add(target.Damage(new DamageArgs
        {
            Damage = owner.Attack * 1.9,
            Caster = owner,
            CanCrit = true,
            DamageText ="That was a harsh snoozy whack that dealt $ damage!",

        }));
        result.Text = "Chamomile Whack!";
        if (BasicFunction.RandomChance(GetSleepChance(owner.BasicAttackLevel)))
        {
            target.StatusEffects.Add(new Sleep(owner), owner.Effectiveness);
        }
        return result;
    }
}
public class BlossomTouch : Skill
{
    public override int GetMaxCooldown(int level) => 3;

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.Team.Where(i =>!i.IsDead);
    }

    public int GetHealthHealScaling(int level)
    {
        switch (level)
        {
            case 0:
            case 1:
            case 2:    
                return 15;
            case 3:
                return 23;
            default:
                return 30;
        }
    }
    public override int MaxEnhance { get; } = 4;
    public override string GetDescription(int level)
    {
        return  $"With the power of flowers, recovers the hp of an ally with {GetHealthHealScaling(level)}% of the caster's max health, dispelling one debuff";
    }
 
    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        target.RecoverHealth((owner.MaxHealth * GetHealthHealScaling(owner.SurgeLevel) * 0.01).Round());
        return new UsageResult(usageType, TargetType.SingleTarget,$"{owner} used Blossom Touch!");
    }
}
public class LilyOfTheValley : Surge
{
    public override int GetMaxCooldown(int level)  => 4;

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team && !i.IsDead);
    }

    public int GetPoisonInflictChance(int level)
    {
        switch (level)
        {
            case 0:
                return 65;
            case 1:
            case 2:    
                return 75;
            case 3:
                return 85;
            default:
                return 100;
        }
    }
    public override int MaxEnhance { get; } = 4;
    public int GetStunInflictChance(int level)
    {
        switch (level)
        {
            case 0:
                return 40;
            case 1:
            case 2:    
                return 50;
            case 3:
                return 65;
            default:
                return 85;
        }
    }
    public override string GetDescription(int level)
    {
        return $"Releases a poisonous gas to all enemies, with an {GetStunInflictChance(level)}% chance of inflicting stun for 1 turn and a {GetPoisonInflictChance(level)}% chance of inflicting poison for one turn";
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        foreach (var i in target.Team)
        {
            if (BasicFunction.RandomChance(GetPoisonInflictChance(owner.SurgeLevel)))
            {
                i.StatusEffects.Add(new Poison(owner){Duration = 2}, owner.Effectiveness);

            }
            if (BasicFunction.RandomChance(GetStunInflictChance(owner.SurgeLevel)))
            {
                i.StatusEffects.Add(new Stun(owner){Duration = 1}, owner.Effectiveness);
            }
        }
        return new UsageResult( usageType, TargetType.AOE,
            $"{owner} used Lily of The Valley, and released a dangerous gas to the enemy team!");
    }
}
public class Lily : Character
{
    public override int BaseMaxHealth => 1500 + (60 * Level);
    public override int BaseAttack => (100 + (8 * Level));
    public override int BaseDefense => (100 + (7.2 * Level)).Round();
    public override int BaseSpeed => 115;
    

    public override DiscordColor Color { get; protected set; } = DiscordColor.HotPink;

    public override void NonPlayerCharacterAi(ref Character target, ref string decision)
    {
        if (Surge.CanBeUsed(this))
        {
            decision = "surge";
            target = Surge.GetPossibleTargets(this).First();
            return;
        }
        var teamMateWithLowestHealth = Team.OrderBy(i => i.Health).First();
        if (Skill.CanBeUsed(this) && teamMateWithLowestHealth.Health < teamMateWithLowestHealth.MaxHealth * 0.7)
        {
            decision = "skill";
            target = teamMateWithLowestHealth;
            return;
        }

        decision = "basicattack";
        target = BasicAttack.GetPossibleTargets(this).OrderBy(i => i.Health).First();

    }

    public override Skill Skill { get; protected set; } = new BlossomTouch();
    public override Surge Surge { get; protected set; } = new LilyOfTheValley();
    public override BasicAttack BasicAttack { get; } = new ChamomileSachetWhack();
    public override Rarity Rarity { get; protected set; } = Rarity.FourStar;
    public override Element Element => Element.Earth;


}
