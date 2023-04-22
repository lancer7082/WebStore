using System.Data.Common;
using Npgsql;
using Ozon.Route256.Five.OrderService.Infrastructure.ClientBalancing;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Db.Postgres;

public class SdConnectionFactory : IDbConnectionFactory
{
    private readonly IDbStore _dbStore;

    public SdConnectionFactory(IDbStore dbStore)
        => _dbStore = dbStore;

    public async Task<string> GetConnectionStringAsync()
    {
        var result = await _dbStore.GetNextEndpointAsync();
        return $"Server={result.Host}:{result.Port};Database=orders;User Id=postgres;Password=postgres;";
    }

    public async Task<DbConnection> GetConnectionAsync()
    {
        var connectionString = await GetConnectionStringAsync();
        return new NpgsqlConnection(connectionString);
    }
}