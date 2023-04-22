using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Ozon.Route256.Five.OrderService.Infrastructure.ClientBalancing;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Db.Postgres;

public class PostgresConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _configuration;

    public PostgresConnectionFactory(IConfiguration configuration)
    { 
        _configuration = configuration;
    }

    public Task<string> GetConnectionStringAsync()
    {
        var connectionString = _configuration.GetValue<string>("Postgres:ConnectionString");
        return Task.FromResult(connectionString);
    }

    public Task<DbConnection> GetConnectionAsync()
    {
        var connectionString = GetConnectionStringAsync().Result;
        DbConnection result = new NpgsqlConnection(connectionString);
        return Task.FromResult(result);
    }
}