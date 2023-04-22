using System.Data;
using Dapper;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Db;

public class ArrayTypeHandler : SqlMapper.TypeHandler<int[]>
{
    public override int[] Parse(object value)
        => (int[])value; 
            
    public override void SetValue(IDbDataParameter parameter, int[] value)
    {
        parameter.Value = value;
    }
}
