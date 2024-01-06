namespace DiscordBotNet.Database.Models;

public interface ISetup
{

    /// <summary>
    /// Used to setup an entity properly before adding it to the dbcontext
    /// </summary>
    /// <param name="context">The context where the entity came from</param>
    /// <returns>The number of state entries written to the database</returns>
    public Task<int> SetupAsync(PostgreSqlContext context);
}