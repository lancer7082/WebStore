namespace Ozon.Route256.Five.OrderService.Infrastructure.ClientBalancing;

public record DbEndpoint(
    string Host, 
    int Port, 
    DbReplicaType DbReplica,
    int[] Buckets);