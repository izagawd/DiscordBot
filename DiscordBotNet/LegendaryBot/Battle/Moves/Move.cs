using System.Diagnostics;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Battle.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Results;
using SixLabors.ImageSharp.Drawing.Processing;

namespace DiscordBotNet.LegendaryBot.Battle.Moves;

public abstract class Move
{
    /// <summary>
    /// The maximum amount this move can be enhanced to
    /// </summary>
 
    
 
    public virtual string IconUrl => $"{Website.DomainName}/battle_images/moves/{GetType().Name}.png";

    public async Task<Image<Rgba32>> GetImageAsync()
    {

        var image = await BasicFunction.GetImageFromUrlAsync(IconUrl);
        image.Mutate(i => i
            .EntropyCrop()
            .Resize(100, 100)
            .Draw(Color.Black, 8, new RectangleF(0, 0, 100,100)));
        return image;
    }

    /// <summary>
    /// Gets the description of the Move, based on the MoveType
    /// </summary>

    public abstract string Description { get; }

    public int MaxSkillEnhancement => 5;

    /// <summary>
    /// Gets the description of the Move, based on the move level
    /// </summary>



    /// <summary>
    /// Gets all the possible targets this move can be used on based on the owner of the move
    /// </summary>

    public abstract IEnumerable<Character> GetPossibleTargets(Character owner);



    /// <summary>
    /// This is where the custom functionality of a move is created
    /// </summary>
    /// <param name="owner">The owner of the move</param>
    /// <param name="target">The target</param>
    /// <param name="usageType">What type of usage this is</param>

    protected abstract UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType);
    /// <summary>
    /// This is where the general functionality of a move is done. It does some checks before HiddenUtilize is called
    /// </summary>
    /// <param name="owner">The owner of the move</param>
    /// <param name="target">The target</param>
    /// <param name="usageType">What type of usage this is</param>
    public virtual UsageResult Utilize(Character owner, Character target, UsageType usageType)
    {
        var temp = HiddenUtilize(owner, target, usageType);

        owner.CurrentBattle.InvokeBattleEvent(new CharacterUseSkillEventArgs(temp));
        return temp;
    }
    /// <summary>
    /// Checks if this move can be used based on the owner
    /// </summary>

    public virtual bool CanBeUsed(Character owner)
    {
        return GetPossibleTargets(owner).Any() && !owner.IsOverriden;
    }

    public override string ToString()
    {

        return BasicFunction.Englishify(GetType().Name);
    }
}