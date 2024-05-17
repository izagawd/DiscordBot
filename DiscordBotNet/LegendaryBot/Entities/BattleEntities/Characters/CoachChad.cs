﻿using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
public class GigaPunch : BasicAttack
{
    public override string GetDescription(Character character) => "Punch is thrown gigaly";
    
    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        return new UsageResult(this)
        {
            DamageResults =
            [
            target.Damage(new DamageArgs(this)
                {
                    ElementToDamageWith = owner.Element,
                    CriticalDamage = owner.CriticalDamage,
                    CriticalChance = owner.CriticalChance,
                    Caster = owner,
                    Damage = owner.Attack * 1.7f,
                    DamageText = $"{owner.NameWithAlphabetIdentifier} smiles chadly, and punches {target.NameWithAlphabetIdentifier} in a cool way and dealt $ damage!"

                })
            ],
            TargetType = TargetType.SingleTarget,
            User = owner,
            UsageType = usageType,
            Text = "Hrrah!"
        };
    }
}

public class MuscleFlex : Surge
{
  
    public override string GetDescription(Character character) => "Flexes muscles";
    

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return new[] { owner };
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        owner.CurrentBattle.AddAdditionalBattleText($"{owner.NameWithAlphabetIdentifier}... flexed his muscles?");
        return new UsageResult(this)
        {
            Text = $"Hmph!",
            TargetType = TargetType.None,
            User = owner,
            UsageType = usageType
        };
    
    }

    public override int MaxCooldown => 1;
}

public class ThumbsUp : Skill
{

    public override string GetDescription(Character character) => "Gives the enemy a thumbs up!";
    

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team&& !i.IsDead);
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        owner.CurrentBattle.AddAdditionalBattleText($"{owner.NameWithAlphabetIdentifier} is cheering {target.NameWithAlphabetIdentifier} on!");
        return new UsageResult(this)
        {
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            Text = $"{owner.NameWithAlphabetIdentifier} gave {target.NameWithAlphabetIdentifier} a thumbs up!",
            User = owner
        };

    }

    public override int MaxCooldown => 1;
}
public class CoachChad : Character, IBattleEventListener
{
    public override float GetResistanceValue(int points)
    {
        return 50;
    }

    public override float GetSpeedValue(int points)
    {
        return (base.GetSpeedValue(points) * 1.02f).Round();
    }

    public override BasicAttack BasicAttack { get; } = new GigaPunch();

    public override Skill? Skill { get;  } = new ThumbsUp();
    public override Surge? Surge { get; } = new MuscleFlex();

    public override DiscordColor Color => DiscordColor.Purple;


    public override void NonPlayerCharacterAi(ref Character target, ref BattleDecision decision)
    {
        List<BattleDecision> possibleDecisions = [BattleDecision.BasicAttack];
        
        
        if(Skill.CanBeUsed(this))
            possibleDecisions.Add(BattleDecision.Skill);
        if(Surge.CanBeUsed(this))
            possibleDecisions.Add(BattleDecision.Surge);
        decision = BasicFunctionality.RandomChoice(possibleDecisions.AsEnumerable());
        target = BasicFunctionality.RandomChoice(BasicAttack.GetPossibleTargets(this));


    }

    [BattleEventListenerMethod]
    private void HandleRevive(CharacterDeathEventArgs deathEventArgs, Character owner)
    {
        
        if (deathEventArgs.Killed != this) return;
        Revive();
        
    }
    [BattleEventListenerMethod]
    private void HandleTurnEnd(TurnEndEventArgs turnEnd, Character owner)
    {
        if (turnEnd.Character != owner) return;
        RecoverHealth(100);

    }


}