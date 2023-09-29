using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using System.Reflection;
using DiscordBotNet.LegendaryBot.Battle.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Battle.Entities.Gears;
using DiscordBotNet.LegendaryBot.Battle.ModifierInterfaces;
using DiscordBotNet.LegendaryBot.Battle.Moves;
using DiscordBotNet.LegendaryBot.Battle.Results;
using DiscordBotNet.LegendaryBot.Battle.StatusEffects;
using DSharpPlus.Entities;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;


namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;


public abstract  class Character : BattleEntity
{
    public virtual bool IsInStandardBanner => true;


    public virtual Blessing? Blessing { get; set; }
    public Guid? BlessingId { get; set; }

    public Shield? Shield => StatusEffects.OfType<Shield>().FirstOrDefault();

    [NotMapped]
    public Move[] MoveList=> new Move[]{ BasicAttack, Skill, Surge }.Where(i => i is not null).ToArray();

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">the type of stats modifier you want</typeparam>
    public List<T> GetAllStatsModifierArgs<T>() where T : StatsModifierArgs
    {
        List<T> theStatsModified = new List<T>();
        if (CurrentBattle is not null)
        {
            theStatsModified.AddRange(CurrentBattle
                .GetAllStatsModifierArgsInBattle()
                .OfType<T>()
                .Where(i => i.CharacterToAffect == this));
        }

        return theStatsModified;
    }
    
    public static Type[] CharacterTypeArray { get; }
    static Character()
    {
        var allTypes = Assembly.GetExecutingAssembly().GetTypes();
        CharacterTypeArray = allTypes.Where(i => !i.IsAbstract && i.IsSubclassOf(typeof(Character)))
            .ToArray();

    }
    [NotMapped]
    private int _health = 1;
    [NotMapped]
    private double _combatReadiness;
    
    public bool IsDead => Health <= 0;
    [NotMapped]
    public CharacterTeam Team { get; set; }
    [NotMapped]
    public double CombatReadiness
    {
        get => _combatReadiness;
        set
        {
            _combatReadiness = value;
            if (_combatReadiness > 100) _combatReadiness = 100;
            if (_combatReadiness < 0) _combatReadiness = 0;
        }
    } 

    [NotMapped]
    public virtual int Health { 
        get => _health;
         set
        {
            if (_health <= 0 )return;
            var tempMaxHealth = MaxHealth;
       
            
            _health = value;
            if (_health <= 0)
            {
                _health = 0;
                CurrentBattle.AdditionalTexts.Add($"{Name} has died");
                CurrentBattle.InvokeBattleEvent(
                    new CharacterDeathEventArgs(this));
            }
            if(_health > tempMaxHealth) _health = tempMaxHealth;
        }
    }

    public void Revive()
    {
        _health = 1;
        StatusEffects.Clear();
        CurrentBattle.AdditionalTexts.Add($"{Name} has been revived");
    }


    public override string IconUrl =>$"https://legendarygawds.com/character-pictures/{GetType().Name}.png";
    public float ShieldPercentage
    {
        get
        {
            var shield = Shield;
            if (shield is null) return 0;
            
            return (float)(shield.GetShieldValue(this) * 1.0 / MaxHealth * 100.0);
        }
    }

    public float HealthPercentage => (float)(Health * 1.0 / MaxHealth * 100.0);
    public sealed override async  Task<Image<Rgba32>> GetDetailsImageAsync()
    {
        var characterImage = await GetInfoAsync();
        var image = new Image<Rgba32>(1280, 1000);

        var characterImageSize = characterImage.Size;
        IImageProcessingContext imageCtx = null;
        image.Mutate(i => imageCtx = i);
       
        int yOffSet = characterImageSize.Height + 50;
        foreach (var i in MoveList)
        {

            int moveLevel;
            switch (i.MoveType)
            {
                case MoveType.Skill:
                    moveLevel = SkillLevel;
                    break;
                case MoveType.Surge:
                    moveLevel = SurgeLevel;
                    break;
                default:
                    moveLevel = BasicAttackLevel;
                    break;
            }
            var moveImage = await i.GetImageAsync(moveLevel);
            moveImage.Mutate(j => j
                .Resize(100,100)
            );
            int xOffset = 20;
            
            var description = i.GetDescription(this);
            
            imageCtx.DrawImage(moveImage, new Point(xOffset, yOffSet), new GraphicsOptions());
            Font font = SystemFonts.CreateFont($"Arial", 40);
            
            RichTextOptions options = new RichTextOptions(font){WrappingLength = 1000};
            options.Origin = new Vector2(120 + xOffset,   yOffSet);

            var fontRectangle = TextMeasurer.MeasureSize(description, options);
            imageCtx.DrawText(options, description, SixLabors.ImageSharp.Color.Black);
 
            var max = float.Max(moveImage.Height, fontRectangle.Height);
            yOffSet += (20 + max + 10).Round();
            
        }

        
        imageCtx.BackgroundColor(Color.ToImageSharpColor());
        imageCtx.DrawImage(characterImage, new Point(((image.Width / 2.0f)- (characterImage.Width /2.0f)).Round(), 20),new GraphicsOptions());
        return image;
    }
    public Helmet? Helmet { get; set; }
    public Guid? HelmetId { get; set; }

    public Weapon? Weapon { get; set; }
    public Guid? WeaponId { get; set; }

    public Armor? Armor { get; set; }
    public Guid? ArmorId { get; set; }

    public Necklace? Necklace { get; set; }
    public Guid? NecklaceId { get; set; }

    public Ring? Ring { get; set; }
    public Guid? RingId { get; set; }
    public Boots? Boots { get; set; }
    public Guid? BootsId { get; set; }
    [NotMapped]
    public IEnumerable<Gear> Gears => new List<Gear?> { Armor, Helmet, Weapon, Necklace, Ring, Boots }
        .Where(i => i is not null).OfType<Gear>();
    public int GetMoveLevel(Move move)
    {
        switch (move.MoveType)
        {
            case MoveType.Skill:
                return SkillLevel;
            case MoveType.Surge:
                return SurgeLevel;
            case MoveType.BasicAttack:
                return BasicAttackLevel;
            default:
                return 0;
        }
    }
    public async Task<Image<Rgba32>> GetCombatImageAsync()
    {
        var image = new Image<Rgba32>(1280, 720);
        var characterImage = await  BasicFunction.GetImageFromUrlAsync(IconUrl);;
        
        characterImage.Mutate(ctx =>
        {
            ctx.Resize(new Size(100, 100));
            
            
        });
     
        IImageProcessingContext ctx = null;
        image.Mutate(idk => ctx = idk);
  
        ctx.DrawImage(characterImage, new Point(0,0), new GraphicsOptions());
        ctx.Draw(SixLabors.ImageSharp.Color.Black, 2, new Rectangle(new Point(0,0),characterImage.Size));
        ctx.DrawText($"{CombatReadiness.Round()}%", SystemFonts.CreateFont("Arial", 20),
            SixLabors.ImageSharp.Color.Black, new PointF(110, 10));

        ctx.Draw(SixLabors.ImageSharp.Color.Black, 2,
            new RectangleF(105, 10, 70,23));
        ctx.DrawText($"Lvl {Level}", SystemFonts.CreateFont("Arial", 20),
            SixLabors.ImageSharp.Color.Black, new PointF(110, 40));

        ctx.Draw(SixLabors.ImageSharp.Color.Black, 2,
            new RectangleF(105, 40, 140,23));

        ctx.DrawText(Name + $" [{Position}]", SystemFonts.CreateFont("Arial", 22),
            SixLabors.ImageSharp.Color.Black, new PointF(110, 70));
        ctx.Draw(SixLabors.ImageSharp.Color.Black, 2,
            new RectangleF(105, 70, 230,25));
        var healthPercentage = HealthPercentage;
        int width = 350;
        int filledWidth = (width * healthPercentage/100.0).Round();
        int filledShieldWidth = (width * ShieldPercentage / 100).Round();
        int barXOffset = 0;
        int barHeight = 33;
        ctx.Fill(SixLabors.ImageSharp.Color.Red, new Rectangle(barXOffset,100,width,barHeight));
        ctx.Fill(SixLabors.ImageSharp.Color.Green, new Rectangle(barXOffset,100,filledWidth,barHeight));
        int shieldXposition = barXOffset + filledWidth;
        if (shieldXposition + filledShieldWidth > width)
        {
            shieldXposition = width - filledShieldWidth;
        }
        ctx.Fill(SixLabors.ImageSharp.Color.White, new RectangleF(shieldXposition,100, filledShieldWidth, barHeight));
        
        //creates border for health bar
        ctx.Draw(SixLabors.ImageSharp.Color.Black, 1, new Rectangle(0, 100, width, barHeight));
        ctx.DrawText($"{Health}/{MaxHealth}", SystemFonts.CreateFont("Arial", 29),
            SixLabors.ImageSharp.Color.Black, new PointF(5, 100));
        
        int xOffSet = 0;
        int yOffSet = 100 + barHeight + 10;
        int statusLength = 30;
        int moveLength = 50;
        foreach (var i in MoveList)
        {

            var moveImage = await i.GetImageAsync(GetMoveLevel(i));
            moveImage.Mutate(context =>
            {
                context.Resize(new Size(moveLength, moveLength));
            });
            ctx.DrawImage(moveImage, new Point(xOffSet, yOffSet), new GraphicsOptions());
            xOffSet += moveLength;
            int cooldown = 0;
            if (i is Special special)
            {
                cooldown = special.Cooldown;
            }

            var cooldownString = "";
            if (cooldown > 0)
            {
                cooldownString = cooldown.ToString();
            }
            ctx.DrawText(cooldownString, SystemFonts.CreateFont("Arial", moveLength),
                SixLabors.ImageSharp.Color.Black, new PointF(xOffSet+10, yOffSet));
            xOffSet += moveLength;
        }
        
        xOffSet = 0;
        yOffSet += moveLength + 10;
        foreach (var i in StatusEffects)
        {


            var statusImage = await i.GetImage();

            statusImage.Mutate(context =>
            {
                context.Resize(new Size(statusLength, statusLength));
            });
            if (xOffSet + statusLength + 5>= 300)
            {
                xOffSet = 0;
                yOffSet += statusLength + 5;
            }
            ctx.DrawImage(statusImage, new Point(xOffSet, yOffSet), new GraphicsOptions());
            xOffSet += 35;

        }

        if (IsDead)
        {
            ctx.Opacity(0.5f);
        }


        ctx.EntropyCrop(0.05f);


        return image;
    }



    [NotMapped]
    public virtual int BaseMaxHealth { get; protected set; }

    public int MaxHealth
    {
        get
        {
            double percentage = 100;
            double originalMaxHealth = TotalMaxHealth;
            var modifiedStats = GetAllStatsModifierArgs<StatsModifierArgs>();
            foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<MaxHealthPercentageModifierArgs>())
            {
                originalMaxHealth += originalMaxHealth * 0.01 * i.ValueToChangeWith;
            }
            foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<MaxHealthFlatModifierArgs>())
            {
                originalMaxHealth += i.ValueToChangeWith;
            }
            double flat = 0;

            foreach (var i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<MaxHealthPercentageModifierArgs>())
            {
                percentage +=i.ValueToChangeWith;
            }
            foreach (var i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<MaxHealthFlatModifierArgs>())
            {
                flat += i.ValueToChangeWith;
            }

            double newMaxHealth = originalMaxHealth  * percentage * 0.01;
            newMaxHealth += flat;
            if (newMaxHealth < 0) newMaxHealth = 0;
            return newMaxHealth.Round();
        }
    }

    [NotMapped] public virtual int BaseDefense { get; } = 200;
    [NotMapped] public virtual Element Element { get; protected set; } = Element.Fire;

    [NotMapped] public virtual int BaseSpeed { get; } = 80;
    public int Speed
    {
        get
        {
            double percentage = 100;
            double originalSpeed = TotalSpeed;

            var modifiedStats = GetAllStatsModifierArgs<StatsModifierArgs>();
            foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<SpeedPercentageModifierArgs>())
            {
                originalSpeed += originalSpeed * 0.01 * i.ValueToChangeWith;
            }
            foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<SpeedFlatModifierArgs>())
            {
                originalSpeed += i.ValueToChangeWith;
            }
            double flat = 0;


            foreach (var i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<SpeedPercentageModifierArgs>())
            {
                percentage +=i.ValueToChangeWith;
            }
            foreach (var i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<SpeedFlatModifierArgs>())
            {
                flat += i.ValueToChangeWith;
            }

            double newSpeed = originalSpeed  * percentage * 0.01;
            newSpeed += flat;
            if (newSpeed < 0) newSpeed = 0;
         
            return newSpeed.Round();
        }
    }

    public int Defense { 
        get
        {
            double percentage = 100;
            double originalDefense = TotalDefense;
            var modifiedStats = GetAllStatsModifierArgs<StatsModifierArgs>();
            foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<DefensePercentageModifierArgs>())
            {
                originalDefense += originalDefense * 0.01 * i.ValueToChangeWith;
            }
            foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<DefenseFlatModifierArgs>())
            {
                originalDefense += i.ValueToChangeWith;
            }
            double flat = 0;

            foreach (var i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<DefensePercentageModifierArgs>())
            {
                percentage +=i.ValueToChangeWith;
            }
            foreach (var i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<DefenseFlatModifierArgs>())
            {
                flat += i.ValueToChangeWith;
            }

            double newDefense = originalDefense  * percentage * 0.01;
            newDefense += flat;
            if (newDefense < 0) newDefense = 0;
            return newDefense.Round();
        } 
    }

    [NotMapped] public virtual int BaseAttack => 100;

    public int Attack { 
        get     
        {
            double percentage = 100;
            double originalAttack = TotalAttack;
            var modifiedStats = GetAllStatsModifierArgs<StatsModifierArgs>();
            foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<AttackPercentageModifierArgs>())
            {
                originalAttack += originalAttack * 0.01 * i.ValueToChangeWith;
            }
            foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<AttackFlatModifierArgs>())
            {
                originalAttack += i.ValueToChangeWith;
            }
            double flat = 0;
            foreach (var i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<AttackPercentageModifierArgs>())
            {
       
                percentage += i.ValueToChangeWith;
            }

            foreach (var i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<AttackFlatModifierArgs>())
            {
                flat += i.ValueToChangeWith;
            }
            double newAttack = originalAttack  * percentage * 0.01;
            newAttack += flat;
            if (newAttack < 0) newAttack = 0;
            return newAttack.Round();
        } 
    }

    [NotMapped] public virtual int BaseCriticalDamage => 150;

    public int CriticalDamage {
        get
        {
            double percentage = 0;
            double originalCriticalDamage = BaseCriticalDamage;
            var modifiedStats = GetAllStatsModifierArgs<StatsModifierArgs>();
            foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<CriticalDamageModifierArgs>())
            {
                originalCriticalDamage += i.ValueToChangeWith;
            }
            
            foreach (CriticalDamageModifierArgs i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<CriticalDamageModifierArgs>())
            {
                percentage +=i.ValueToChangeWith;
            }


            double newCriticalDamage = originalCriticalDamage + percentage;

            if (newCriticalDamage < 0) newCriticalDamage = 0;
            return newCriticalDamage.Round();
        }
    }

    [NotMapped]
    public virtual int BaseEffectiveness { get;  }
    [NotMapped]
    public int Resistance {
        get
        {
            {
                double percentage = 0;
                double originalResistance = TotalResistance;
                var modifiedStats = GetAllStatsModifierArgs<StatsModifierArgs>();
                foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<ResistanceModifierArgs>())
                {
                    originalResistance += i.ValueToChangeWith;
                }

                foreach (ResistanceModifierArgs i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<ResistanceModifierArgs>())
                {
                    percentage +=i.ValueToChangeWith;
                }
                double newResistance = originalResistance + percentage;

                if (newResistance < 0) newResistance = 0;
                return newResistance.Round();
            }
        } 
    }
    /// <summary>
    /// this will be used to get the items this character will drop if killed
    /// </summary>
    [NotMapped]
    public virtual Entity[] DroppedItems {
        get
        {
            return new Entity[0];
        } 
    }

    /// <summary>
    /// Use this if you want this character to drop any extra items
    /// </summary>
    [NotMapped]
    public virtual List<Entity> ExtraItemsToDrop { get; set; } = new();

    protected int _basicAttackLevel = 0;
    public int BasicAttackLevel
    {
        get => _basicAttackLevel;
        set
        {
            if (value < 0)
            {
                value = 0;
            }

            if (value > 5)
            {
                value = 5;
            }

            _basicAttackLevel = value;
        } 
    }
    public async Task<Image<Rgba32>> GetInfoAsync()
    {
        var userImage = await BasicFunction.GetImageFromUrlAsync(IconUrl);
        var image = new Image<Rgba32>(500, 150);
        userImage.Mutate(ctx => ctx.Resize(new Size(100,100)));
        image.Mutate(ctx =>
        {
            ctx.BackgroundColor(Color.ToImageSharpColor());
            var userImagePoint = new Point(20, 20);
            ctx.DrawImage(userImage,userImagePoint, new GraphicsOptions());
            ctx.Draw(SixLabors.ImageSharp.Color.Black, 3, new RectangleF(userImagePoint,userImage.Size));
            var levelBarMaxLevelWidth = 300ul;
            var gottenExp = levelBarMaxLevelWidth * (Experience/(GetRequiredExperienceToNextLevel() * 1.0f));
            var levelBarY = userImage.Height - 30 + userImagePoint.Y;
            ;
            ctx.Fill(SixLabors.ImageSharp.Color.Gray, new RectangleF(130, levelBarY, levelBarMaxLevelWidth, 30));
            ctx.Fill(SixLabors.ImageSharp.Color.Green, new RectangleF(130, levelBarY, gottenExp, 30));
            ctx.Draw(SixLabors.ImageSharp.Color.Black, 3, new RectangleF(130, levelBarY, levelBarMaxLevelWidth, 30));
            var font = SystemFonts.CreateFont("Arial", 25);
            var xPos = 135;
            ctx.DrawText($"{Experience}/{GetRequiredExperienceToNextLevel()}",font,SixLabors.ImageSharp.Color.Black,new PointF(xPos,levelBarY+2));
            ctx.DrawText($"Name: {Name}", font, SixLabors.ImageSharp.Color.Black, new PointF(xPos, levelBarY -57));
            ctx.DrawText($"Level: {Level}",font,SixLabors.ImageSharp.Color.Black,new PointF(xPos,levelBarY - 30));
            ctx.Resize(1000, 300);
        });

        return image;
    }
    protected int _skillLevel = 0;
    public int SkillLevel {        get => _skillLevel;
        set
        {
            if (value < 0)
            {
                value = 0;
            }

            if (value > 5)
            {
                value = 5;
            }

            _skillLevel = value;
        }  }

    protected int _surgeLevel = 0;
    public int SurgeLevel {         get => _surgeLevel;
        set
        {
            if (value < 0)
            {
                value = 0;
            }

            if (value > 5)
            {
                value = 5;
            }

            _surgeLevel= value;
        }
        
    }

   
    /// <summary>
    /// if this character is not being controlled by the player, it will use custom AI
    /// </summary>
    /// <param name="target"></param>
    /// <param name="decision"></param>
    public virtual void NonPlayerCharacterAi(ref Character target, ref BattleDecision decision)
    {
        List<BattleDecision> possibleDecisions = new() { BattleDecision.BasicAttack };


        if(Skill.CanBeUsed(this))
            possibleDecisions.Add(BattleDecision.Skill);
        if(Surge.CanBeUsed(this))
            possibleDecisions.Add(BattleDecision.Surge);

        IEnumerable<Character> possibleTargets = new List<Character>();
        Move move;
        BattleDecision moveDecision = BattleDecision.BasicAttack;
        while (!possibleTargets.Any())
        {
            moveDecision =BasicFunction.RandomChoice<BattleDecision>(possibleDecisions);
            move = this[moveDecision]!;
            possibleTargets = move.GetPossibleTargets(this);
            possibleDecisions.Remove(moveDecision);
        }

        
        target = BasicFunction.RandomChoice(possibleTargets);

        decision = moveDecision;

    }

    public Move? this[BattleDecision battleDecision]
    {
        get
        {
            switch (battleDecision)
            {
                case BattleDecision.Skill:
                    return Skill;
                case BattleDecision.Surge:
                    return Surge;
                case BattleDecision.BasicAttack:
                    return BasicAttack;
                default:
                    return null;
            }
        }
    }

    [NotMapped]
    public StatusEffectList StatusEffects { get; set; } 

    [NotMapped]
    public virtual DiscordColor Color { get; protected set; }

    [NotMapped]
    public int Effectiveness
    {
        get
        {
            double percentage =0;
            double originalEffectiveness = TotalEffectiveness;
            var modifiedStats = GetAllStatsModifierArgs<StatsModifierArgs>();
            foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<EffectivenessModifierArgs>())
            {
                originalEffectiveness += i.ValueToChangeWith;
            }

            foreach (EffectivenessModifierArgs i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<EffectivenessModifierArgs>())
            {
                percentage +=i.ValueToChangeWith;
            }
            double newEffectiveness = originalEffectiveness  +percentage;

            if (newEffectiveness < 0) newEffectiveness = 0;
            return newEffectiveness.Round();
        }
    }

    [NotMapped]
    public virtual int BaseResistance
    {
        get;
   
    }


    [NotMapped]
    public int CriticalChance {
        get
        {
            {
                double percentage = 0;
                double originalCriticalChance = TotalCriticalChance;
                var modifiedStats = GetAllStatsModifierArgs<StatsModifierArgs>();
                foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<CriticalChanceModifierArgs>())
                {
                    originalCriticalChance += i.ValueToChangeWith;
                }

                foreach (CriticalChanceModifierArgs i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<CriticalChanceModifierArgs>())
                {
                    percentage +=i.ValueToChangeWith;
                }


                double newCriticalChance = originalCriticalChance  + percentage;

                if (newCriticalChance < 0) newCriticalChance = 0;
                return newCriticalChance.Round();
            }
        }
        
    }


    [NotMapped] public virtual int BaseCriticalChance => 20;

    [NotMapped]
    public BattleSimulator CurrentBattle { get; set; }

    public Character() 
    {
        StatusEffects = new(this);

    }

    public void SetGear(Gear gear)
    {
        if (gear is Armor armor)
        {
            Armor = armor;
        }
        else if (gear is Boots boots)
        {
            Boots = boots;
        }
        else if (gear is Helmet helmet)
        {
            Helmet = helmet;
        }
        else if (gear is Weapon weapon)
        {
            Weapon = weapon;
        }
        else if (gear is Ring ring)
        {
            Ring = ring;
        }
        else if (gear is Necklace necklace)
        {
            Necklace = necklace;
        }

    }
    public override int MaxLevel => 100;

    public void SetLevel(int level)
    {
        Level = level;
    }
    [NotMapped]
    public double TotalAttack { get; set; }
    [NotMapped]
    public double TotalDefense { get; set; }
    [NotMapped]
    public double TotalMaxHealth { get; set; }
    [NotMapped]
    public double TotalSpeed { get; set; }
    [NotMapped]
    public double TotalCriticalChance { get; set; }
    [NotMapped]
    public double TotalCriticalDamage { get; set; }
    [NotMapped]
    public double TotalEffectiveness { get; set; }
    [NotMapped]
    public double TotalResistance { get; set; }
    
    public override async Task LoadAsync()
    {
        await base.LoadAsync();
        TotalAttack = BaseAttack;
        TotalDefense = BaseDefense;
        TotalSpeed = BaseSpeed;
        TotalCriticalDamage = BaseCriticalDamage;
        TotalCriticalChance = BaseCriticalChance;
        TotalResistance = BaseResistance;
        TotalEffectiveness = BaseEffectiveness;
        TotalMaxHealth = BaseMaxHealth;
        
        foreach (Gear gear in Gears)
        {
            await gear.LoadAsync();

            foreach (var stat in gear.AllStats)
            {
                stat.AddStats(this);
            }
        }
        if (Blessing is not null)
        {
   
            TotalAttack += Blessing.Attack;
            TotalDefense += Blessing.Defense;
        }

        Health = TotalMaxHealth.Round();

    }
    
  

    protected void TakeDamageWhileConsideringShield(int damage)
    {
        Shield? shield = Shield;

        if (shield is null)
        {
            Health -= damage;
            return;
        }

        var shieldValue = shield.GetShieldValue(this);
        // Check if the shield can absorb some of the damage
        if (shieldValue > 0)
        {
            shieldValue -= damage;
            if (shieldValue < 0)
            {
                Health += shieldValue;
                shieldValue = 0;
            }
        }
        else
        {
            // If no shield, damage affects health directly
            Health -= damage;
        }
        shield.SetShieldValue(this,shieldValue);
    }

    /// <summary>
    /// Used to damage this character
    /// </summary>
    /// <param name="damage">The potential damage</param>
    /// <param name="damageText">if there is a text for the damage, then use this. Use $ in the string and it will be replaced with the damage dealt</param>
    /// <param name="caster">The character causing the damage</param>
    /// <param name="canCrit">Whether the damage can cause a critical hit or not</param>
    /// <param name="damageElement">The element of the damage</param>
    /// <returns>The results of the damage</returns>
    public virtual DamageResult Damage(DamageArgs damageArgs)
    {
        var damageText = damageArgs.DamageText;
        var damage = damageArgs.Damage;
        var caster = damageArgs.Caster;
        var canBeCountered = damageArgs.CanBeCountered;
        var canCrit = damageArgs.CanCrit;
        bool didCrit = false;
        int damageModifyPercentage = 0;
        damage = BattleFunction.DamageFormula(damage, Defense);

        switch (BattleFunction.GetAdvantageLevel(caster.Element, Element)){
            case ElementalAdvantage.Disadvantage:
                damageModifyPercentage -= 30;
                break;
            case ElementalAdvantage.Advantage:

                damageModifyPercentage += 30;
                break;
        }

        damage = (int)Math.Round(damage*0.01*(damageModifyPercentage+100));

        if (BasicFunction.RandomChance(caster.CriticalChance)&& canCrit)
        {
            damage *= caster.CriticalDamage / 100.0;
            didCrit = true;
        }

        int actualDamage = damage.Round();
        if (damageText is null)
        {
            damageText = $"{caster} dealt {actualDamage} damage!";
        }

        damageText = damageText.Replace("$", actualDamage.ToString());
        if (didCrit)
            damageText = "A critical hit! " + damageText;
        CurrentBattle.AdditionalTexts.Add(damageText);
        
        TakeDamageWhileConsideringShield(actualDamage);
        DamageResult damageResult;
        if (damageArgs.Move is not null)
        {
            damageResult = new DamageResult(damageArgs.Move)
            {
                WasCrit = didCrit,
                Damage = actualDamage,
                DamageDealer = caster,
                DamageReceiver = this,
                CanBeCountered = canBeCountered
            };
        }
        else
        {
            damageResult = new DamageResult(damageArgs.StatusEffect)
            {
                WasCrit = didCrit,
                Damage = actualDamage,
                DamageDealer = caster,
                DamageReceiver = this,
                CanBeCountered = canBeCountered
            };
        }
        CurrentBattle.InvokeBattleEvent(new CharacterDamageEventArgs(damageResult));
        return damageResult;
    }
    

    [NotMapped]
    public abstract BasicAttack BasicAttack { get; }
    
    public string GetNameWithPosition(bool IsEnemy)
    {
        string side = "enemy";
        if (!IsEnemy)
        {
            side = "team mate";
        }
        return $"{Name} ({side}) ({Position})";
    }
    public abstract override Rarity Rarity { get; }
    [NotMapped] public abstract Skill Skill { get; } 
    public int Position => Array.IndexOf(CurrentBattle.Characters.ToArray(),this) +1;
    [NotMapped] public abstract Surge Surge { get; }
    public bool IsOverriden
    {
        get
        {
            return StatusEffects.Any(i => i.OverrideTurnType > 0);
        }
    }
    /// <param name="damageText">if there is a text for the damage, then use this. Use $ in the string and it will be replaced with the damage dealt</param>
  /// <param name="caster">The character causing the damage</param>
/// <param name="damage">The potential damage</param>
    public DamageResult FixedDamage(DamageArgs damageArgs)
    {
        var damageText = damageArgs.DamageText;
        var damage = damageArgs.Damage;
        var caster = damageArgs.Caster;
        var canBeCountered = damageArgs.CanBeCountered;
        if (damageText is null)
        {
            damageText = $"{this} took $ fixed damage!";
        }
        CurrentBattle.AdditionalTexts.Add(damageText.Replace("$", damage.ToString()));
        TakeDamageWhileConsideringShield(damage.Round());
        DamageResult damageResult;
        if (damageArgs.Move is not null)
        {
            damageResult = new DamageResult(damageArgs.Move)
            {
              
                WasCrit = false,
                Damage = damage.Round(),
                DamageDealer = caster, 
                DamageReceiver = this,
                CanBeCountered = canBeCountered
            };
        }
        else
        {
            damageResult = new DamageResult(damageArgs.StatusEffect)
            {
              
                WasCrit = false,
                Damage = damage.Round(),
                DamageDealer = caster, 
                DamageReceiver = this,
                CanBeCountered = canBeCountered
            };
        }
            
        CurrentBattle.InvokeBattleEvent(new CharacterDamageEventArgs(damageResult));
        return damageResult;
    }
    /// <summary>
    /// Recovers the health of this character
    /// </summary>
    /// <param name="toRecover">Amount to recover</param>
    /// <returns>Amount recovered</returns>
    public virtual int RecoverHealth(int toRecover, bool mentionRecovery =  true)
    {
        
        Health +=  toRecover;
        if (mentionRecovery)
        {
            CurrentBattle.AdditionalTexts.Add($"{this} recovered {toRecover} health!");
        }
        return  toRecover;
    }
    /// <summary>
    /// checks if it is currently the character's turn
    /// </summary>
    public bool IsActive => CurrentBattle.ActiveCharacter == this;



    public override ulong GetRequiredExperienceToNextLevel(int level)
    {
       return BattleFunction.NextLevelFormula(Level);
    }

    /// <summary>
/// Increases the Exp of a character and returns useful text
/// </summary>
/// <returns></returns>
    public override ExperienceGainResult IncreaseExp(ulong exp)
    {
        string expGainText = "";

        var levelBefore = Level;
        Experience += exp;


        var nextLevelEXP =GetRequiredExperienceToNextLevel(Level);
        while (Experience >= nextLevelEXP && Level < MaxLevel)
        {
            Experience -= nextLevelEXP;
            Level += 1;
            nextLevelEXP = GetRequiredExperienceToNextLevel(Level);
        }

        expGainText += $"{this} gained {exp} exp";
        if (levelBefore != Level)
        {
            expGainText += $", and moved from level {levelBefore} to level {Level}";
        }
        ulong excessExp = 0;
        if (Experience > nextLevelEXP)
        {
            excessExp = Experience - nextLevelEXP;
        }
        expGainText += "!";
        return new ExperienceGainResult(){ExcessExperience = excessExp, Text = expGainText};
    }

    public Move? this[MoveType? moveType]
    {
        get
        {
            switch (moveType)
            {
                case  MoveType.Skill:
                    return Skill;
                case MoveType.BasicAttack:
                    return BasicAttack;
                        
                case MoveType.Surge:
                    return Surge;
                default:
                    return null;
            } 
        } set{}
    }



}