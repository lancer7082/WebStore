using System.Data.Common;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Db;

public interface IDbConnectionFactory
{
    Task<string> GetConnectionStringAsync();
    Task<DbConnection> GetConnectionAsync();
}