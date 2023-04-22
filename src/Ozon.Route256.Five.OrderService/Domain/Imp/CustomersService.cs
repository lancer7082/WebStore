using Ozon.Route256.Five.OrderService.Domain.Dto;

namespace Ozon.Route256.Five.OrderService.Domain;

public class CustomersService : ICustomersService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomersService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerDto[]> GetAllAsync(CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Array.Empty<CustomerDto>();

        var result = await _customerRepository.GetAllAsync(token);
        return result;
    }

    public Task<CustomerDto> GetAsync(long customerId, CancellationToken token)
    {
        return _customerRepository.GetAsync(customerId, token);
    }

    public Task<CustomerDto?> FindAsync(long customerId, CancellationToken token)
    {
        return _customerRepository.FindAsync(customerId, token);
    }
}
