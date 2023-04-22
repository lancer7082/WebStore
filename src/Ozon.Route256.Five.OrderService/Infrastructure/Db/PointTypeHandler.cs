using System.Data;
using Dapper;
using NpgsqlTypes;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Db;

public class PointTypeHandler : SqlMapper.TypeHandler<NpgsqlPoint>
{
    public override NpgsqlPoint Parse(object value)
        => (NpgsqlPoint)value;

    public override void SetValue(IDbDataParameter parameter, NpgsqlPoint value)
    {
        parameter.Value = value;
    }
}