using System.Reflection;
using DiscordBotNet.Database.ManyToManyInstances;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Quests;
using DiscordBotNet.LegendaryBot.Stats;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using ConfigurationManager = System.Configuration.ConfigurationManager;


namespace DiscordBotNet.Database;

public class PostgreSqlContext : DbContext
{
    
    private static readonly Type[] EntityClasses;
    private static readonly Type[] GearStatClasses;
    public DbSet<UserData> UserData { get; set; }
    public DbSet<GuildData> GuildData { get; set; }
    public DbSet<Entity> Entity { get; set; }
    public DbSet<Quest> Quests { get; set; }
   
    public DbSet<Quote> Quote { get; set; }

    /// <summary>
    /// this should be called before any query if u want to ever use this method
    /// </summary>
    /// <param name="userId"> the user id u want  to refresh to a new day</param>
    public static async Task CheckForNewDayAsync(ulong userId)
    {
        await using var context = new PostgreSqlContext();
        var user = await context.UserData
            .Include(i => i.Quests)
            .FindOrCreateAsync(userId);
        var rightNowUtc = DateTime.UtcNow;
        if (user.LastTimeChecked.Date == rightNowUtc.Date) return;
        
        user.Quests.Clear();
        var availableQuests = Quest.QuestSampleInstances
            .Where(i => i.QuestTier == user.Tier)
            .Select(i => i.GetType())
            .ToList();

        while (user.Quests.Count < 4)
        {
            if(availableQuests.Count <= 0) break;
            var randomQuestType = BasicFunction.RandomChoice(availableQuests.AsEnumerable());
            availableQuests.Remove(randomQuestType);
            if (user.Quests.Any(j => j.GetType() == randomQuestType)) continue;
            user.Quests.Add((Quest) Activator.CreateInstance(randomQuestType)!);
            
        }

        
        user.LastTimeChecked = DateTime.UtcNow;
        await context.SaveChangesAsync();

    }

    private static readonly Type[] _assemblyTypes;
    static PostgreSqlContext()
    {
        _assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
        EntityClasses = _assemblyTypes
            .Where(type => type.IsRelatedToType(typeof(Entity))).ToArray();
        GearStatClasses = _assemblyTypes
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
            .Include(i => i.Inventory.Where(j => j is Character))
            .FirstOrDefaultAsync(i => i.Id == idOfUser);
        if (user is null) return;
        foreach (var i in user.Inventory.OfType<Character>())
        {
            i.SetLevel(60);
            foreach (var j in _assemblyTypes.Where(i => !i.IsAbstract && i.IsRelatedToType(typeof(Gear))))
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
                    wantedTypes =
                    [
                        GearStat.HealthPercentageType, GearStat.DefensePercentageType, GearStat.SpeedFlatType,
                        GearStat.ResistanceType
                    ];
                else if (i is Lily)

                    wantedTypes = 
                    [
                        GearStat.HealthPercentageType, GearStat.SpeedFlatType,
                        GearStat.EffectivenessType
                    ];
                gear.Initiate(Rarity.FiveStar, mainStat, wantedTypes);
                gear.UserDataId = i.UserDataId;
                gear.IncreaseExp(9000000000000, wantedTypes);
                i.AddGear(gear);

                if (user.EquippedPlayerTeam is null)
                {
                    var newTeam = new PlayerTeam();
                    user.EquippedPlayerTeam = newTeam;
                      
                    user.PlayerTeams.Add(newTeam);
                }
                    
              
                CharacterTeam team = user.EquippedPlayerTeam;
              
                if (team.Count < 4 && i is not CoachChad)
                    team.Add(i);
     

            }

        }
    }

    private static readonly IEnumerable<Type> QuestTypes =
        Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(i => i.IsSubclassOf(typeof(Quest)) 
                        && !i.IsAbstract).ToArray();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var i in QuestTypes)
        {
            modelBuilder.Entity(i);
        }
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




        modelBuilder.Entity<PlayerTeam>()
            .HasKey(i => i.Id);

        modelBuilder.Entity<PlayerTeam>()
            .Property(i => i.Id)
            .ValueGeneratedNever();
        modelBuilder.Entity<UserData>()
            .HasMany(i => i.PlayerTeams)
            .WithOne(i => i.UserData)
            .HasForeignKey(i => i.UserDataId);

        modelBuilder.Entity<PlayerTeam>()
            .HasMany<Character>(i => i.Characters)
            .WithMany(i => i.PlayerTeams)
            .UsingEntity<CharacterPlayerTeam>(i
                => i.HasOne<Character>().WithMany().HasForeignKey(j => j.CharacterId), i =>
                i.HasOne<PlayerTeam>().WithMany().HasForeignKey(j => j.PlayerTeamId), i =>
                i.HasKey(j => new { j.CharacterId, j.PlayerTeamId }));
        modelBuilder.Entity<UserData>()
            .HasOne(i => i.EquippedPlayerTeam)
            .WithOne()
            .HasForeignKey<UserData>(i => i.EquippedPlayerTeamId);
        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Property);

        modelBuilder.Entity<UserData>()
            .HasMany(i => i.Inventory)
            .WithOne(i => i.UserData)
            .HasForeignKey(i => i.UserDataId);
        modelBuilder.Entity<Player>()
            .Property(i => i.Element)
            .HasColumnName("Element");

        modelBuilder.Entity<UserData>()
            .HasMany(i => i.Quests)
            .WithOne()
            .HasForeignKey(i => i.UserDataId);

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



        modelBuilder.Entity<PlayerTeam>()
            .Navigation(i => i.Characters)
            .AutoInclude();
        
        modelBuilder.Entity<UserData>()
            .HasKey(i => i.Id);
        modelBuilder.Entity<Character>()
            .HasOne(i => i.Blessing)
            .WithOne(i => i.Character)
            .HasForeignKey<Character>(i => i.BlessingId)
            .OnDelete(DeleteBehavior.SetNull);
        
    }
}