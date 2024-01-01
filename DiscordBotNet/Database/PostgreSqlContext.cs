using System.Reflection;
using DiscordBotNet.Database.ManyToManyInstances;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

using DiscordBotNet.LegendaryBot.Quests;

using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using ConfigurationManager = System.Configuration.ConfigurationManager;


namespace DiscordBotNet.Database;

public class PostgreSqlContext : DbContext
{
    
    private static readonly Type[] EntityClasses;

    public DbSet<UserData> UserData { get; set; }
    public DbSet<GuildData> GuildData { get; set; }
    public DbSet<Entity> Entity { get; set; }
    public DbSet<Quest> Quests { get; set; }

    public DbSet<Quote> Quote { get; set; }

    /// <summary>
    /// this should be called before any query if u want to ever use this method
    /// </summary>
    /// <param name="userId"> the user id u want  to refresh to a new day</param>
    public  async Task CheckForNewDayAsync(long userId)
    {

        var user = await UserData
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


    }

    private static readonly Type[] _assemblyTypes;
    static PostgreSqlContext()
    {
        _assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
        EntityClasses = _assemblyTypes
            .Where(type => type.IsRelatedToType(typeof(Entity))).ToArray();

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


        //makes sure the character id properties are not the same, even across tables

     
        modelBuilder.Entity<UserData>()
            .Property(i => i.Color)
            .HasConversion(i => i.ToString(), j => new DiscordColor(j)
            );
        modelBuilder.Entity<Entity>()
            .HasKey(i => i.Id);
        modelBuilder.Entity<Quote>()
            .HasKey(i => i.Id);
        modelBuilder.Entity<Quote>()
            .Property(i => i.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Entity>()
            .Property(i => i.Id)
            .ValueGeneratedOnAdd();
        modelBuilder.Entity<PlayerTeam>()
            .HasIndex(i => new { i.UserDataId, i.Label })
            .IsUnique();

        modelBuilder.Entity<PlayerTeam>()
            .HasKey(i => i.Id);

        modelBuilder.Entity<PlayerTeam>()
            .Property(i => i.Id)
            .ValueGeneratedOnAdd();
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

        modelBuilder.Entity<CharacterBuild>()
            .HasKey(i => i.Id);

        modelBuilder.Entity<Character>()
            .HasOne(i => i.EquippedCharacterBuild)
            .WithOne()
            .HasForeignKey<Character>(i => i.EquippedCharacterBuildId);
        modelBuilder.Entity<CharacterBuild>()
            .Property(i => i.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Character>()
            .HasMany(i => i.CharacterBuilds)
            .WithOne(i => i.Character)
            .HasForeignKey(i => i.CharacterId);
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