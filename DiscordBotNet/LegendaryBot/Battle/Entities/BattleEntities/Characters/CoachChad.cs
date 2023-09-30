using DiscordBotNet.LegendaryBot.Battle.BattleEvents;
using DiscordBotNet.LegendaryBot.Battle.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Battle.Moves;
using DiscordBotNet.LegendaryBot.Battle.Results;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
public class GigaPunch : BasicAttack
{
    public override string GetDescription(int moveLevel)
    {
        return "Punch is thrown gigaly";
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        return new UsageResult(this)
        {
            DamageResults = new[]
            {
                target.Damage(new DamageArgs(this)
                {
                    Caster = owner,
                    Damage = owner.Attack * 2,
                    DamageText = $"{owner} smiles chadly, and punches {target} in a cool way and dealt $ damage!"

                })
            },
            TargetType = TargetType.SingleTarget,
            User = owner,
            UsageType = usageType,
            Text = "Hrrah!"
        };
    }
}

public class MuscleFlex : Surge
{
    public override int MaxEnhance { get; }
    public override string GetDescription(int moveLevel)
    {
        return "Flexes muscles";
    }

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return new[] { owner };
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        owner.CurrentBattle.AdditionalTexts.Add($"owner... flexed his muscles?");
        return new UsageResult(this)
        {
            Text = $"Hmph!",
            TargetType = TargetType.None,
            User = owner,
            UsageType = usageType
        };
    
    }

    public override int GetMaxCooldown(int level)
    {
        return 1;
    }
}

public class ThumbsUp : Skill
{
    public override int MaxEnhance { get; } = 4;

    public override string GetDescription(int moveLevel)
    {
        return "Gives the enemy a thumbs up!";
    }

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team&& !i.IsDead);
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        owner.CurrentBattle.AdditionalTexts.Add($"{owner} is cheering {target} on!");
        return new UsageResult(this)
        {
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            Text = $"{owner} gave {target} a thumbs up!",
            User = owner
        };

    }

    public override int GetMaxCooldown(int level) => 1;
}
public class CoachChad : Character, IBattleEvent<CharacterDeathEventArgs>, IBattleEvent<TurnEndEventArgs>
{


    public override int BaseMaxHealth => 9000;
    public override int BaseAttack => 100;
    public override int BaseSpeed => 120;
    public override int BaseDefense => 100;
    public override int BaseCriticalDamage => 150;
    public override int BaseResistance => 0;

    public override BasicAttack BasicAttack { get; } = new GigaPunch();
    public override Rarity Rarity { get; protected set; } = Rarity.FiveStar;
    public override Skill Skill { get;  } = new ThumbsUp();
    public override Surge Surge { get; } = new MuscleFlex();

    public override DiscordColor Color => DiscordColor.Purple;


    public override void NonPlayerCharacterAi(ref Character target, ref BattleDecision decision)
    {

        decision = BattleDecision.BasicAttack;


        target = BasicFunction.RandomChoice(BasicAttack.GetPossibleTargets(this));


    }

    public void OnBattleEvent(CharacterDeathEventArgs eventArgs, Character owner)
    {
        if (eventArgs.Killed == this)
        {
            Revive();
            
        }
    }

    public void OnBattleEvent(TurnEndEventArgs eventArgs, Character owner)
    {
        if (eventArgs.Character != owner) return;
        Health += (int)(0.05 * MaxHealth);
    }
}