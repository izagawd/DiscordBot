namespace DiscordBotNet.Database.Models;

public interface ISetup
{

    /// <summary>
    /// Used to setup an entity properly in the  database and for client use. The setup should be implemented without a transaction,
    /// so you can use a transaction if you want to bulk setup multiple things. This method calls savechanges async, so beware. <br/>
    /// If you want it to be done with a transaction, use <see cref="SetupWithTransactionAsync"/>. it does that for you.
    /// Use <see cref="PostgreSqlContext.HardSaveChangesAsync"/> when you want to save changes in this implementation. If not, a recursion you
    /// will not like will occur
    /// </summary>
    /// <param name="context">The context where the entity came from</param>
    /// <returns>The number of state entries written to the database</returns>
    public Task<int> SetupAsync(PostgreSqlContext context, bool acceptAllChangesOnSuccess =  true , CancellationToken token = new());

    /// <summary>
    /// Sets up the entity with a transaction. Useful when u want to use some of tbe entities features.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<int> SetupWithTransactionAsync(PostgreSqlContext context)
    {
        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            try
            {
                var count = await SetupAsync(context);
                await transaction.CommitAsync();
                return count;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}