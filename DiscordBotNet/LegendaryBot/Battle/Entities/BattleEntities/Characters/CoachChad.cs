using DiscordBotNet.LegendaryBot.Battle.BattleEvents;
using DiscordBotNet.LegendaryBot.Battle.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Battle.Moves;
using DiscordBotNet.LegendaryBot.Battle.Results;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

public class ThumbsUp : Skill
{
    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team&& !i.IsDead);
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        owner.CurrentBattle.AdditionalTexts.Add($"{owner} is cheering {target} on!");
        return new UsageResult($"{owner} gave {target} a thumbs up!", usageType)
        {
   
            
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

    public override Skill Skill { get;  protected set; } = new ThumbsUp();
    public override DiscordColor Color => DiscordColor.Purple;


    public override void NonPlayerCharacterAi(ref Character target, ref string decision)
    {

        decision = "basicattack";


        target = BasicFunction.RandomChoice(BasicAttack.GetPossibleTargets(this));


    }

    public void OnEvent(CharacterDeathEventArgs eventArgs, Character owner)
    {
        if (eventArgs.Killed == this)
        {
            Revive();
            
        }
    }

    public void OnEvent(TurnEndEventArgs eventArgs, Character owner)
    {
        if (eventArgs.Character != owner) return;
        Health += (int)(0.05 * MaxHealth);
    }
}