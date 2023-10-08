using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot;
using DiscordBotNet.LegendaryBot.Battle;
using DiscordBotNet.LegendaryBot.Battle.Entities;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Battle.Stats;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using ConfigurationManager = System.Configuration.ConfigurationManager;


namespace DiscordBotNet.Database;

public class PostgreSqlContext :DbContext
{
    private static readonly Type[] EntityClasses;
    private static readonly Type[] GearStatClasses;
    public DbSet<UserData> UserData { get; set; }
    public DbSet<GuildData> GuildData{ get; set; }
    public DbSet<Entity> Entity { get; set; }

   
    public DbSet<Quote> Quote { get; set; }

    static PostgreSqlContext()
    {

        EntityClasses = Bot.AllAssemblyTypes
            .Where(type => type.IsRelatedToType(typeof(Entity))).ToArray();
        GearStatClasses =
            Bot.AllAssemblyTypes
                .Where(type => type.IsRelatedToType(typeof(GearStat)) && !type.IsAbstract)
                .ToArray();

    }



  
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
        // Configure the database provider and connection string
        optionsBuilder
            
            .UseNpgsql(ConfigurationManager.AppSettings["PostgreSqlConnectionString"])

            .EnableSensitiveDataLogging();
    
    }
    public void ResetDatabase()
    {
        Database.EnsureDeleted();
        Database.EnsureCreated();
      
    }

    public async Task GivePowerToUserAsync(ulong idOfUser)
    {
        var user = await UserData
            .Include(i => i.Inventory.Where(i => i is Character))
            .FirstOrDefaultAsync(i => i.Id == idOfUser);
        if (user is null) return;
        foreach (var i in user.Inventory.OfType<Character>())
        {
            i.SetLevel(60);
            foreach (var j in Bot.AllAssemblyTypes.Where(i => !i.IsAbstract && i.IsRelatedToType(typeof(Gear))))
            {
                var gear = (Gear)Activator.CreateInstance(j)!;
                Type mainStat = null;

                if (gear is Boots)
                    mainStat = GearStat.SpeedFlatType;
                else if (gear is Ring)
                {
                    mainStat = GearStat.AttackPercentageType;
                    if (i is RoyalKnight || i is Lily)
                        mainStat = GearStat.HealthPercentageType;
                }

                else if (gear is Necklace)
                {
                    mainStat = GearStat.CriticalDamageType;
                    if (i is RoyalKnight || i is Lily)
                        mainStat = GearStat.DefensePercentageType;
                }

                Type[] wantedTypes =
                {
                    GearStat.AttackPercentageType, GearStat.CriticalDamageType, GearStat.CriticalChanceType,
                    GearStat.SpeedFlatType
                };
                if (i is RoyalKnight)
                    wantedTypes = new[]
                    {
                        GearStat.HealthPercentageType, GearStat.DefensePercentageType, GearStat.SpeedFlatType,
                        GearStat.ResistanceType
                    };
                else if (i is Lily)

                    wantedTypes = new[]
                    {
                        GearStat.HealthPercentageType, GearStat.SpeedFlatType,
                        GearStat.EffectivenessType
                    };
                gear.Initiate(Rarity.FiveStar, mainStat, wantedTypes);
                gear.UserDataId = i.UserDataId;
                gear.IncreaseExp(9000000000000, wantedTypes);
                i.AddGear(gear);
                CharacterTeam team = user.GetCharacterTeam("");
                if(team.Count < 4 && i is not CoachChad)
                    user.AddToTeam(i);
                    
            }

        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entityType in EntityClasses)
        {
            modelBuilder.Entity(entityType);
        }

        foreach (var i in GearStatClasses)
        {
            modelBuilder.Owned(i);
        }
        //makes sure the character id properties are not the same, even across tables
        modelBuilder.Entity<UserData>()

            .ToTable(i => i.HasCheckConstraint("CK_character_id_properties_should_not_be_the_same",
                $"\"{nameof(Models.UserData.Character1Id)}\" != \"{nameof(Models.UserData.Character2Id)}\"" +
                $"AND \"{nameof(Models.UserData.Character1Id)}\" != \"{nameof(Models.UserData.Character3Id)}\"" +
                $"AND \"{nameof(Models.UserData.Character1Id)}\" != \"{nameof(Models.UserData.Character4Id)}\"" +
                $"AND \"{nameof(Models.UserData.Character2Id)}\" != \"{nameof(Models.UserData.Character3Id)}\"" +
                $"AND \"{nameof(Models.UserData.Character2Id)}\" != \"{nameof(Models.UserData.Character4Id)}\"" +
                $"AND \"{nameof(Models.UserData.Character3Id)}\" != \"{nameof(Models.UserData.Character4Id)}\""));
     
        modelBuilder.Entity<UserData>()
            .Property(i => i.Color)
            .HasConversion(i => i.ToString(), j => new DiscordColor(j)
            );
        modelBuilder.Entity<Entity>()
            .HasKey(i => i.Id);
        modelBuilder.Entity<Quote>()
            .HasKey(i => i.Id);
        modelBuilder.Entity<Entity>()
            .Property(i => i.Id)
            .ValueGeneratedNever();
        modelBuilder.Entity<Quote>()
            .Property(i => i.Id)
            .ValueGeneratedNever();


            
        modelBuilder.Entity<UserData>()
            .HasOne(i => i.Character1)
            .WithOne()
            .HasForeignKey<UserData>(i => i.Character1Id)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<UserData>()
            .HasOne(u => u.Character2)
            .WithOne()
            .HasForeignKey<UserData>(u => u.Character2Id)
            .OnDelete(DeleteBehavior.SetNull);

        
        modelBuilder.Entity<UserData>()
            .HasOne(u => u.Character3)
            .WithOne()
            .HasForeignKey<UserData>(u => u.Character3Id)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<UserData>()
            .HasOne(u => u.Character4)
            .WithOne()
            .HasForeignKey<UserData>(u => u.Character4Id)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Property);

        modelBuilder.Entity<UserData>()
            .HasMany(i => i.Inventory)
            .WithOne(i => i.UserData)
            .HasForeignKey(i => i.UserDataId);
        modelBuilder.Entity<Player>()
            .Property(i => i.Element)
            .HasColumnName("Element");


        modelBuilder.Entity<Character>()
            .HasOne(i => i.Helmet)
            .WithOne()
            .HasForeignKey<Character>(i => i.HelmetId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Character>()
            .HasOne(i => i.Weapon)
            .WithOne()
            .HasForeignKey<Character>(i => i.WeaponId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Character>()
            .HasOne(i => i.Armor)
            .WithOne()
            .HasForeignKey<Character>(i => i.ArmorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Character>()
            .HasOne(i => i.Necklace)
            .WithOne()
            .HasForeignKey<Character>(i => i.NecklaceId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Character>()
            .HasOne(i => i.Ring)
            .WithOne()
            .HasForeignKey<Character>(i => i.RingId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Character>()
            .HasOne(i => i.Boots)
            .WithOne()
            .HasForeignKey<Character>(i => i.BootsId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<UserData>()
            .HasMany(i => i.Quotes)
            .WithOne(i => i.UserData)
            .HasForeignKey(i => i.UserDataId);
        modelBuilder.Entity<Quote>()
            .HasMany(i => i.QuoteReactions)
            .WithOne(i => i.Quote)
            .HasForeignKey(i => i.QuoteId);
        modelBuilder.Entity<UserData>()
            .HasMany(i => i.QuoteReactions)
            .WithOne(i => i.UserData)
            .HasForeignKey(i => i.UserDataId);
        modelBuilder.Entity<Gear>(entity =>
        {
            entity
                .Property(i => i.MainStat)
                .HasConversion(GearStat.ValueConverter);
            entity
                .Property(i => i.SubStat1)
                .HasConversion(GearStat.ValueConverter!);
            entity
                .Property(i => i.SubStat2)
                .HasConversion(GearStat.ValueConverter!);
            entity
                .Property(i => i.SubStat3)
                .HasConversion(GearStat.ValueConverter!);

            entity
                .Property(i => i.SubStat4)
                .HasConversion(GearStat.ValueConverter!);
            entity.Property(i => i.Rarity)
                .HasColumnName("Rarity");

        });

        modelBuilder.Entity<UserData>()
            .HasKey(i => i.Id);
        modelBuilder.Entity<Character>()
            .HasOne(i => i.Blessing)
            .WithOne(i => i.Character)
            .HasForeignKey<Character>(i => i.BlessingId)
            .OnDelete(DeleteBehavior.SetNull);
        
    }
}