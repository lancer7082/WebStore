namespace Ozon.Route256.Five.OrderService.Infrastructure.Repositories;

internal class RepositoryException : Exception
{
    public RepositoryException()
    {
    }

    public RepositoryException(string message) : base(message)
    {
    }
}
