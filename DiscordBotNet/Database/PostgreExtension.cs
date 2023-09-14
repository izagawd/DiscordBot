using System.Linq.Expressions;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace DiscordBotNet.LegendaryBot.Database;

public static class PostgreExtension
{
    public async static Task<List<T>> FindOrCreateManyAsync<T>(this DbSet<T> set,
        IEnumerable<ulong> ids)  where T : Model,new()
    {
        return await set.FindOrCreateManyAsync<T,float>(ids, null);
        
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
        return queryable
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



    public async static Task<List<T>> FindOrCreateManyAsync<T, U>(this DbSet<T> set,
        IEnumerable<ulong> ids, Func<IQueryable<T>, IIncludableQueryable<T, U>>? includableQueryableFunc) 
        where T : Model, new()
    {
        List<T> data;
        var idsAsList = ids.ToList();
        if (includableQueryableFunc is not null)
        {
            
            var queryableSet = set.AsQueryable();
            data = await includableQueryableFunc
                .Invoke(queryableSet)
                .Where(i => idsAsList.Contains(i.Id))
                .ToListAsync();
        }
        else
        {
            data = await set
                .Where(i => idsAsList.Contains(i.Id))
                .ToListAsync();
        }
   
        if (data.Count != idsAsList.Count())
        {
            var existingIds = await set
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

            await set.AddRangeAsync(missingInstances);
        }
        return data;
    }
    public async static Task<List<U>> FindOrCreateManySelectAsync<T,  U>(this DbSet<T> set,
       IEnumerable<ulong> ids,Expression<Func<T, U>> selectExpression) where T : Model,new()
    {
        return await set.FindOrCreateManySelectAsync<T,float, U>( ids, null, selectExpression);
    }

    public async static Task<List<V>> FindOrCreateManySelectAsync<T, U,V>(this DbSet<T> set,
        IEnumerable<ulong> ids, Func<IQueryable<T>, IIncludableQueryable<T, U>>? includableQueryableFunc,
         Expression<Func<T, V>> selectExpression) where T : Model, new()
    {
        List<V> data;
        var idsAsList = ids.ToList();
        if (includableQueryableFunc is not null)
        {
            var queryableSet = set.AsQueryable();
            data = await includableQueryableFunc
                .Invoke(queryableSet)
                .Where(i => idsAsList.Contains(i.Id))
                .Select(selectExpression)
                .ToListAsync();
        }
        else
        {


            data = await set
                .Where(i => idsAsList.Contains(i.Id))
                .Select(selectExpression)
                .ToListAsync();
          
        }
   
        if (data.Count != idsAsList.Count)
        {
            var existingIds = await set
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

            await set.AddRangeAsync(missingInstances);
        }
        return data;
    }
    public async static Task<U> FindOrCreateSelectAsync<T,  U>(this DbSet<T> set,
        ulong id,Expression<Func<T, U>> selectExpression) where T : Model,new()
    {
        return await set.FindOrCreateSelectAsync<T,float, U>( id, null, selectExpression);
    }
    public async static Task<V> FindOrCreateSelectAsync<T, U, V>(this DbSet<T> set, ulong id,
        Func<IQueryable<T>, IIncludableQueryable<T, U>>? includableQueryableFunc, Expression<Func<T,V>> selectExpression) where T : Model,new()
    {
        V? data;
        if (includableQueryableFunc is not null)
        {
            var queryableSet = set.AsQueryable();
            data = await includableQueryableFunc
                .Invoke(queryableSet)
                .Where(i => i.Id == id)
                .Select(selectExpression)
                .FirstOrDefaultAsync();
        }
        else
        {
            data = await set
                .Where(i => i.Id == id)
                .Select(selectExpression)
                .FirstOrDefaultAsync();
        }

        if (data is null)
        {
            var tempData = new T { Id = id };
            await set.AddAsync(tempData);
            
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
    public async static Task<T> FindOrCreateAsync<T,U>(this DbSet<T> set, ulong id,Func<IQueryable<T>, IIncludableQueryable<T,U>>? includableQueryableFunc) where T : Model, new()
    {
        T? data;
        if (includableQueryableFunc is not null)
        {
            var queryableSet = set.AsQueryable();
            data = await includableQueryableFunc
                .Invoke(queryableSet)
                .FirstOrDefaultAsync(i => i.Id == id);
        }
        else
        {
            data = await set
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        if (data is null)
        {
            data = new T { Id = id };
            await set.AddAsync(data);
        }
        return data;
    }
    /// <summary>
    /// Gets a row/instance from the database. if not found, returns a new row/instance and adds it to the database when the DatabaseContext of this dbset is saved
    /// </summary>
    /// <param name="id">The Id of the row/instance</param>
    /// <typeparam name="T">The instance/row type</typeparam>
    /// <typeparam name="U"></typeparam>
    /// <returns></returns>
    public async static Task<T> FindOrCreateAsync<T>(this DbSet<T> set, ulong id) where T : Model, new()
    {
        return await set.FindOrCreateAsync<T, float>(id, null);
    }
}