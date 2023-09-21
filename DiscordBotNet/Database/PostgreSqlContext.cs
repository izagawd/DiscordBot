﻿using System.Text.Json;
using System.Text.Json.Nodes;
using DiscordBotNet.Database.Models;
using DiscordBotNet.LegendaryBot;
using DiscordBotNet.LegendaryBot.Battle.Entities;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Entities.Gears;
using DiscordBotNet.LegendaryBot.Battle.Stats;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using ConfigurationManager = System.Configuration.ConfigurationManager;


namespace DiscordBotNet.Database;

public class PostgreSqlContext :DbContext
{
    private static List<Type> entityClasses;
    private static List<Type> gearStatClasses;
    public DbSet<UserData> UserData { get; set; }
    public DbSet<GuildData> GuildData{ get; set; }
    public DbSet<Entity> Entity { get; set; }

   
    public DbSet<Quote> Quote { get; set; }

    static PostgreSqlContext()
    {

        entityClasses = Bot.AllAssemblyTypes
            .Where(type => type.IsRelatedToType(typeof(Entity))).ToList();
        gearStatClasses =
            Bot.AllAssemblyTypes
                .Where(type => type.IsRelatedToType(typeof(GearStat)) && !type.IsAbstract)
                .ToList();

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entityType in entityClasses)
        {
            modelBuilder.Entity(entityType);
            
        }

        foreach (var i in gearStatClasses)
        {
            modelBuilder.Owned(i);
        }

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
            .WithOne()
            .HasForeignKey(i => i.UserDataId);
        
        modelBuilder.Entity<Gear>(entity =>
        {
            entity
                .Property(i => i.MainStat)
                .HasConversion(GearStat.ValueConverter);
            entity
                .Property(i => i.SubStat1)
                .HasConversion(GearStat.ValueConverter);
            entity
                .Property(i => i.SubStat2)
                .HasConversion(GearStat.ValueConverter);
            entity
                .Property(i => i.SubStat3)
                .HasConversion(GearStat.ValueConverter);

            entity
                .Property(i => i.SubStat4)
                .HasConversion(GearStat.ValueConverter);


        });


        modelBuilder.Entity<Character>()
            .HasOne(i => i.Blessing)
            .WithOne(i => i.Character)
            .HasForeignKey<Character>(i => i.BlessingId)
            .OnDelete(DeleteBehavior.SetNull);
        
    }
}