
using System.Numerics;
using DiscordBotNet.LegendaryBot.Battle.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Results;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DiscordBotNet.LegendaryBot.Battle.Moves;

public abstract class Move
{
    /// <summary>
    /// The maximum amount this move can be enhanced to
    /// </summary>
    public abstract int MaxEnhance { get; } 

    [Image]
    public virtual string IconUrl => $"{Website.DomainName}/battle_images/moves/{GetType().Name}.png";

    public async Task<Image<Rgba32>> GetImageAsync(int? level = null)
    {

        var image = await BasicFunction.GetImageFromUrlAsync(IconUrl);
        var max = int.Max(image.Height, image.Width);
        
        image.Mutate(i => i
            .EntropyCrop()
            .Resize(max, max)
            .Draw(Color.Black, 8, new RectangleF(0, 0, max, max)));
  
        if (level is not null && level > 0)
        {
            var options = new RichTextOptions(SystemFonts.CreateFont("Arial",max/ 3.0f));
            
            options.Origin = new Vector2(10, 0);

            var rectangle = new RectangleF(0, 0, 30, 30);
            image.Mutate(i => i
                .Draw(Color.Black,4,rectangle)
                .Fill(Color.Black, rectangle)
                .DrawText(options, $"{level}", Color.White));
        }

        return image;
    }
    /// <summary>
    /// Gets the description of the Move, based on the MoveType
    /// </summary>

    public  string GetDescription(Character owner)
    {
        switch (MoveType)
        {
            case MoveType.Skill:
                return GetDescription(owner.SkillLevel);
            case MoveType.BasicAttack:
                return GetDescription(owner.BasicAttackLevel);
            default:
                return GetDescription(owner.SurgeLevel);
        }
    }

    public int MaxSkillEnhancement => 5;

    /// <summary>
    /// Gets the description of the Move, based on the move level
    /// </summary>
    public abstract string GetDescription(int moveLevel);


    /// <summary>
    /// Gets all the possible targets this move can be used on based on the owner of the move
    /// </summary>

    public abstract IEnumerable<Character> GetPossibleTargets(Character owner);

    public virtual MoveType MoveType => MoveType.BasicAttack;

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
        return GetPossibleTargets(owner).Any() && GetType() != typeof(SurgeSample) && GetType() != typeof(SkillSample);
    }

    public override string ToString()
    {

        return BasicFunction.Englishify(GetType().Name);
    }
}