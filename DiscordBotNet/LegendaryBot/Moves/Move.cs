using DiscordBotNet.LegendaryBot.BattleEvents;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.ModifierInterfaces;
using DiscordBotNet.LegendaryBot.Results;
using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBotNet.LegendaryBot.Moves;

public abstract class Move : IBattleEventListener, IStatsModifier
{
    /// <summary>
    /// The maximum amount this move can be enhanced to
    /// </summary>
 
    
    public virtual string IconUrl => $"{Website.DomainName}/battle_images/moves/{GetType().Name}.png";

    protected static MemoryCache _croppedCombatImagesMemoryCache { get; } = new(new MemoryCacheOptions());

    protected static MemoryCacheEntryOptions ExpireEntryOptions { get; } = new()
    {
        SlidingExpiration = new TimeSpan(0,30,0),
        PostEvictionCallbacks = { new PostEvictionCallbackRegistration(){EvictionCallback = BasicFunctionality.DisposeEvictionCallback} }
    };
    protected static MemoryCacheEntryOptions EntryOptions { get; } = new()
    {
        SlidingExpiration = new TimeSpan(0,30,0),
        PostEvictionCallbacks = { new PostEvictionCallbackRegistration(){EvictionCallback = BasicFunctionality.DisposeEvictionCallback} }
    };
    public async Task<Image<Rgba32>> GetImageForCombatAsync()
    {
        var url = IconUrl;
        if (!_croppedCombatImagesMemoryCache.TryGetValue(url, out Image<Rgba32> image))
        {
            image = await BasicFunctionality.GetImageFromUrlAsync(IconUrl);
            image.Mutate(i => i
                .Resize(25, 25)
                .Draw(Color.Black, 3, new RectangleF(0, 0, 24,24)));
            var entryOption = EntryOptions;
            if (!url.Contains(Website.DomainName))
                entryOption = ExpireEntryOptions;
            _croppedCombatImagesMemoryCache.Set(url, image, entryOption);
        }




        return image.Clone();
    }

    /// <summary>
    /// Gets the description of the Move, based on the MoveType
    /// </summary>


    /// <summary>
    /// Gets description based on character. The description is mostly affected by the character's level
    /// </summary>
    /// <param name="level"></param>
    public abstract string GetDescription(Character character);



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
        owner.CurrentBattle.InvokeBattleEvent(new CharacterUseMoveEventArgs(temp));
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

        return Name;
    }

    public virtual IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs(Character owner)
    {
        return [];
    }

    public virtual string Name => BasicFunctionality.Englishify(GetType().Name);

    public virtual void OnBattleEvent(BattleEventArgs eventArgs, Character owner)
    {
        
    }
}