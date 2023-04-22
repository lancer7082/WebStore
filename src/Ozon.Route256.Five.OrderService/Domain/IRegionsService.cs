namespace Ozon.Route256.Five.OrderService.Domain;

public interface IRegionsService
{
    /// <summary>
    /// Получить список регионов
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<string[]> GetAllAsync(CancellationToken token);
}
