﻿using DiscordBotNet.LegendaryBot.BattleEvents;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.ModifierInterfaces;
using DiscordBotNet.LegendaryBot.Results;
using Microsoft.Extensions.Caching.Memory;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public abstract class StatusEffect : ICloneable ,  IStatsModifier
{
    public virtual string Description => "Does the bla bla bla of the bla bla bla";
    public virtual string IconUrl => $"{Website.DomainName}/battle_images/status_effects/{GetType().Name}.png";

    /// <summary>
    /// Returns true if the status effect is executed after the character's turn
    /// </summary>
    public virtual bool ExecuteStatusEffectAfterTurn => true;
    /// <summary>
    /// Returns true if the status effect is executed before the character's turn
    /// </summary>
    public bool ExecuteStatusEffectBeforeTurn => !ExecuteStatusEffectAfterTurn;
    /// <summary>
    /// Returns true if the status effect can be on a character more than once
    /// </summary>
    public bool IsStackable => MaxStacks > 1;

    private static MemoryCache _cachedResizedCombatImages = new MemoryCache(new MemoryCacheOptions());
    
    protected static MemoryCacheEntryOptions EntryOptions { get; } = new()
    {
        SlidingExpiration = new TimeSpan(0,30,0),
        PostEvictionCallbacks = { new PostEvictionCallbackRegistration(){EvictionCallback = BasicFunctionality.DisposeEvictionCallback} }
    };
    protected static MemoryCacheEntryOptions ExpiredEntryOptions { get; } = new()
    {
        SlidingExpiration = new TimeSpan(0,30,0),
        PostEvictionCallbacks = { new PostEvictionCallbackRegistration(){EvictionCallback = BasicFunctionality.DisposeEvictionCallback} }
    };


    public async Task<Image<Rgba32>> GetImageForCombatAsync()
    {
        var url = IconUrl;

        if (!_cachedResizedCombatImages.TryGetValue(url, out Image<Rgba32> image))
        {
            
            image = await BasicFunctionality.GetImageFromUrlAsync(IconUrl);

            var backgroundColor = Color.Red;
            if (EffectType == StatusEffectType.Buff)
            {
                backgroundColor = Color.ParseHex("#67B0D8");
            }
            image.Mutate(ctx =>
            {
                ctx.BackgroundColor(backgroundColor);
                ctx.Resize(new Size(20, 20));
            });

            var entry = EntryOptions;
            if (!url.Contains(Website.DomainName)) entry = ExpiredEntryOptions;
            _cachedResizedCombatImages.Set(url, image, entry);

        }
        image = image.Clone();

        image.Mutate(ctx =>
        {
 
            var x = 1;
            var xOffset = 0;
            var duration = Duration.ToString();
            if (duration.Length > 1)
            {
                x = 0;
                xOffset = 3;
            }

            ctx.Fill(Color.Black, new RectangleF(0, 0, 9+xOffset, 9));
            var font = SystemFonts.CreateFont(Bot.GlobalFontName, 9, FontStyle.Bold);
 
            ctx.DrawText(Duration.ToString(), font, Color.White, new PointF(x, 0));
        });

        return image;
    }
    public virtual StatusEffectType EffectType => StatusEffectType.Buff;

    ///<summary>With an override turn type of enum number 1 or more, the status effect can modify the user's decision for a turn.
    /// if there is more than one status effect with an override turn type enum number at least 1, it is the status effect with the
    /// highest override turn type enum number that will take effect</summary>
    public virtual OverrideTurnType OverrideTurnType => OverrideTurnType.None;


    /// <summary>
    /// When status effect optimizing has occured this will be called
    /// </summary>
    /// <param name="statusEffect">The status effect to optimize with. the status effect instance that calls this method will be used as the status effect, not the parameter</param>
    public virtual void OptimizeWith(StatusEffect statusEffect)
    {
        if (statusEffect.Duration > Duration)
            Duration = statusEffect.Duration;
    }
    public virtual int MaxStacks => int.MaxValue;
    /// <summary>
    /// The duration of the status effect
    /// </summary>
    public int Duration { get; set; } = 2;
    /// <summary>
    /// The person who casted the status effect
    /// </summary>
    public Character Caster { get; set; }
    /// <summary>
    /// The name of the status effect
    /// </summary>
    public virtual string Name => BasicFunctionality.Englishify(GetType().Name);

    public virtual UsageResult OverridenUsage(Character affected,ref Character target, ref BattleDecision decision, UsageType usageType) // the status effect might or might not replace the player's decision
    {
        return new UsageResult(this)
        {
            UsageType = usageType,
            TargetType = TargetType.None,
            User = affected
        };
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="affected"></param>
    /// <returns>if the status effect has any additional texts it will return a string if not it returns null</returns>
    public virtual void PassTurn(Character affected)
    {
        Duration -= 1;

    }

    /// <param name="caster">The one who casted the status effect</param>
    public StatusEffect(Character caster)
    {
        Caster = caster;
    }



    public override string ToString()
    {
        return Name;
    }

    public virtual  IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs(Character owner)
    {
        yield break;
    }

 

    object ICloneable.Clone()
    {
        return Clone();
    }

    public StatusEffect Clone()
    {
        return (StatusEffect) MemberwiseClone();
    }

    public static IEnumerable<StatusEffect> operator *(StatusEffect effect, int number)
    {
        if (number <= 0)
        {
            throw new Exception("Entity times a negative number or 0 doesn't make sense");
            
        }

        List<StatusEffect> clonedStatusEffects = [];
        foreach (var i in Enumerable.Range(0, number))
        {
            clonedStatusEffects.Add(effect.Clone());
        }

        return clonedStatusEffects;
    }
}