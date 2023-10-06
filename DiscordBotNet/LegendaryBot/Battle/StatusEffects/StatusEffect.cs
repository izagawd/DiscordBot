using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Results;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;

namespace DiscordBotNet.LegendaryBot.Battle.StatusEffects;

public abstract class StatusEffect
{
   
    public virtual string IconUrl => $"{Website.DomainName}/battle_images/status_effects/{GetType().Name}.png";
    // is renewable means that if a new status effect of the same type should be added to the character,
    // if it has a higher level it will override the old ones level. if it has a higher duration it will override
    // the old ones duration
    public virtual bool IsRenewable => false;
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

    private int _level = 1;
    
    /// <summary>
    /// Returns true if the status effect has a max level higher than 1
    /// </summary>
    public virtual bool HasLevels => MaxLevel > 1;
    /// <summary>
    /// The max level of a status effects
    /// </summary>
    public virtual int MaxLevel => 1;

    public async Task<Image<Rgba32>> GetImage()
    {
        var backgroundColor = Color.Red;
        if (EffectType == StatusEffectType.Buff)
        {
            backgroundColor = Color.ParseHex("#67B0D8");
        }

        var image = await BasicFunction.GetImageFromUrlAsync(IconUrl);
        image.Mutate(ctx =>
        {
            


            ctx.BackgroundColor(backgroundColor);
            ctx.Resize(new Size(100, 100));
            ctx.Fill(Color.Black, new RectangleF(0, 0, 40, 40));
            var font = SystemFonts.CreateFont("Arial", 40, FontStyle.Bold);
            ctx.DrawText(Duration.ToString(), font, Color.White, new PointF(5, 0));

        });
        return image;
    }
    public virtual StatusEffectType EffectType => StatusEffectType.Buff;

    ///<summary>With an override turn type of enum number 1 or more, the status effect can modify the user's decision for a turn.
    /// if there is more than one status effect with an override turn type enum number at least 1, it is the status effect with the
    /// highest override turn type enum number that will take effect</summary>
    public virtual OverrideTurnType OverrideTurnType => OverrideTurnType.None;
   /// <summary>
   /// Some status effects may have levels. The higher the level, the stronger it is
   /// </summary>
    public int Level { get => _level;
        set { _level = value; if (_level > MaxLevel) _level = MaxLevel; if (_level <= 0) _level = 1; } }
    public virtual void RenewWith(StatusEffect statusEffect)
    {}
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
    public virtual string Name => BasicFunction.Englishify(GetType().Name);

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

    public StatusEffect Copy() 
    {
        return (StatusEffect) MemberwiseClone();
    }

    public override string ToString()
    {
        return Name;
    }

}