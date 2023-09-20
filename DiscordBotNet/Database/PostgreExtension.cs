﻿using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using DiscordBotNet.Database.Models;
using DiscordBotNet.LegendaryBot;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Entities.Gears;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace DiscordBotNet.Database;

public static class PostgreExtension
{
    public static DbContext GetDbContext(this IQueryable query)
    {

        
        var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        var queryCompiler = typeof(EntityQueryProvider).GetField("_queryCompiler", bindingFlags).GetValue(query.Provider);
        var queryContextFactory = queryCompiler.GetType().GetField("_queryContextFactory", bindingFlags).GetValue(queryCompiler);

        var dependencies = typeof(RelationalQueryContextFactory).GetProperty("Dependencies", bindingFlags).GetValue(queryContextFactory);
        var queryContextDependencies = typeof(DbContext).Assembly.GetType(typeof(QueryContextDependencies).FullName);
        var stateManagerProperty = queryContextDependencies.GetProperty("StateManager", bindingFlags | BindingFlags.Public).GetValue(dependencies);
        var stateManager = (IStateManager)stateManagerProperty;
        
        return  stateManager.Context;
    }


    public static IIncludableQueryable<UserData,Character?> IncludeTeam
        (this IQueryable<UserData> queryable)
    {
        return queryable.Include(i => i.Character1)
            .Include(i => i.Character2)
            .Include(i => i.Character3)
            .Include(i => i.Character4);

    }
    
    public static IIncludableQueryable<UserData,Blessing?> IncludeTeamWithBlessing
        (this IQueryable<UserData> queryable)
    {
        return queryable.Include(i => i.Character1)
            .ThenInclude(i => i.Blessing)
            .Include(i => i.Character2)
            .ThenInclude(i => i.Blessing)
            .Include(i => i.Character3)
            .ThenInclude(i => i.Blessing)
            .Include(i => i.Character4)
            .ThenInclude(i => i.Blessing);

    }
    public static IIncludableQueryable<UserData,Blessing?> IncludeTeamWithAllEquipments
        (this IQueryable<UserData> queryable)
    {

       return   queryable
            .IncludeWithAllEquipments(i => i.Character1)
            .IncludeWithAllEquipments(i => i.Character2)
            .IncludeWithAllEquipments(i => i.Character3)
            .IncludeWithAllEquipments(i => i.Character4);

    }

    public static IIncludableQueryable<UserData, Necklace?> IncludeTeamWithGears
        (this IQueryable<UserData> queryable)
    {
        return queryable
            .IncludeWithGears(i => i.Character1)
            .IncludeWithGears(i => i.Character2)
            .IncludeWithGears(i => i.Character3)
            .IncludeWithGears(i => i.Character4);
    }
    public static IIncludableQueryable<TEntity, Blessing?> IncludeWithAllEquipments<TEntity,TCharacter>
    (this IQueryable<TEntity> queryable,
        Expression<Func<TEntity,TCharacter>>
            navigationPropertyPath) where TEntity : class
    where TCharacter: Character
    {
        return queryable
            .IncludeWithGears(navigationPropertyPath)
            .Include(navigationPropertyPath)
            .ThenInclude(i => i.Blessing);
    }
    /// <summary>
    /// this not only includes all the gears of the characters, but the stats as well
    /// </summary>

 

    public static IIncludableQueryable<TEntity, Necklace?> IncludeWithGears<TEntity,TCharacter>(
        this IQueryable<TEntity> queryable, Expression<Func<TEntity,TCharacter>>
            navigationPropertyPath) 
        where TEntity : class
        where TCharacter : Character
    {
           
        return queryable
            .Include(navigationPropertyPath)
            .ThenInclude(i => i.Armor)
            .Include(navigationPropertyPath)
            .ThenInclude(i => i.Boots)
            .Include(navigationPropertyPath)
            .ThenInclude(i => i.Helmet)
            .Include(navigationPropertyPath)
            .ThenInclude(i => i.Weapon)
            .Include(navigationPropertyPath)
            .ThenInclude(i => i.Ring)
            .Include(navigationPropertyPath)
            .ThenInclude(i => i.Necklace);
    }



    public async static Task<List<T>> FindOrCreateManyAsync<T>(this IQueryable<T> queryable,
        IEnumerable<ulong> ids) 
        where T : Model, new()
    {
        List<T> data;
        var idsAsList = ids.ToList();

            data = await queryable
                .Where(i => idsAsList.Contains(i.Id))
                .ToListAsync();
   
        if (data.Count != idsAsList.Count())
        {
            var existingIds = await queryable
                .Where(i => idsAsList.Contains(i.Id))
                .Select(u => u.Id)
                .ToListAsync();
            var missingIds = idsAsList.Except(existingIds);
            List<T> missingInstances = new List<T>();
            foreach (var i in missingIds)
            {
                var missingInstance = new T() { Id = i };
                data.Add(missingInstance);
                missingInstances.Add(missingInstance);
            }

            await queryable.GetDbContext().Set<T>().AddRangeAsync(missingInstances);

        }
        return data;
    }


    public async static Task<List<V>> FindOrCreateManySelectAsync<T, V>(this IQueryable<T> queryable,
        IEnumerable<ulong> ids,
         Expression<Func<T, V>> selectExpression) where T : Model, new()
    {
        List<V> data;
        var idsAsList = ids.ToList();
        
            data = await queryable
                .Where(i => idsAsList.Contains(i.Id))
                .Select(selectExpression)
                .ToListAsync();
          
        
   
        if (data.Count != idsAsList.Count)
        {
            var existingIds = await queryable
                .Where(i => idsAsList.Contains(i.Id))
                .Select(u => u.Id)
                .ToListAsync();
            var missingIds = idsAsList.Except(existingIds);
            List<T> missingInstances = new List<T>();
            foreach (var i in missingIds)
            {
                var missingInstance = new T{ Id = i };
                data.Add(missingInstance.Map(selectExpression));
                missingInstances.Add(missingInstance);
            }

            await queryable.GetDbContext().Set<T>().AddRangeAsync(missingInstances);
        }
        return data;
    }

    public async static Task<V> FindOrCreateSelectAsync<T, V>(this IQueryable<T> queryable, ulong id,
         Expression<Func<T,V>> selectExpression) where T : Model,new()
    {
        V? data = await queryable
                .Where(i => i.Id == id)
                .Select(selectExpression)
                .FirstOrDefaultAsync();
        

        if (data is null)
        {
            var tempData = new T { Id = id };
            await queryable.GetDbContext().AddRangeAsync(tempData);
            
            data = tempData.Map(selectExpression);
        }
        return data;
    }
    /// <summary>
    /// Gets a row/instance from the database. if not found, returns a new row/instance and adds it to the database when the DatabaseContext of this dbset is saved
    /// </summary>
    /// <param name="id">The Id of the row/instance</param>
    /// <param name="includableQueryableFunc">If you want to include some shit use this</param>
    /// <typeparam name="T">The instance/row type</typeparam>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public async static Task<T> FindOrCreateAsync<T>(this IQueryable<T> queryable, ulong id) where T : Model, new()
    {
        T? data = await queryable
                .FirstOrDefaultAsync(i => i.Id == id);
     

        if (data is null)
        {
            data = new T { Id = id };
            await queryable.GetDbContext().Set<T>().AddAsync(data);
        }
        return data;
    }

}