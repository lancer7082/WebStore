using Microsoft.Extensions.Caching.Distributed;
using Ozon.Route256.Five.OrderService.Domain;
using Ozon.Route256.Five.OrderService.Domain.Dto;
using Ozon.Route256.Five.OrderService.Domain.Exceptions;
using Ozon.Route256.Five.OrderService.Infrastructure.Repositories;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Redis
{
    public class RedisCustomersCache : ICustomersCache
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            Converters = { new JsonStringEnumConverter() }
        };

        private static readonly DistributedCacheEntryOptions CacheEntryOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        };

        private readonly IDistributedCache _redisCache;

        public RedisCustomersCache(IDistributedCache redisCache)
        {
            _redisCache = redisCache;
        }

        private static string GetKey(long customerId) => $"customer:{customerId}";

        public async Task<CustomerDto?> FindAsync(long id, CancellationToken token)
        {
            var stringResult = await _redisCache.GetStringAsync(GetKey(id), token);
            if (string.IsNullOrWhiteSpace(stringResult))
                return null;

            return JsonSerializer.Deserialize<CustomerDto>(stringResult, JsonSerializerOptions);
        }
        public async Task<CustomerDto> GetAsync(long id, CancellationToken token)
        {
            var stringResult = await _redisCache.GetStringAsync(GetKey(id), token);
            if (string.IsNullOrWhiteSpace(stringResult))
            {
                throw new NotFoundException($"customer {id} not found");
            }

            return JsonSerializer.Deserialize<CustomerDto>(stringResult, JsonSerializerOptions)!; // stringResult здесь не null
        }

        public async Task Insert(CustomerDto customer, CancellationToken token)
        {
            var key = GetKey(customer.Id);
            var existingValue = await _redisCache.GetStringAsync(key, token);
            if (!string.IsNullOrWhiteSpace(existingValue))
            {
                throw new RepositoryException($"Customer {customer.Id} already exist");
            }
            await _redisCache.SetStringAsync(key, JsonSerializer.Serialize(customer), token);
        }

        public async Task Update(CustomerDto customer, CancellationToken token)
        {
            var key = GetKey(customer.Id);
            var existingValue = await _redisCache.GetStringAsync(key, token);
            if (string.IsNullOrWhiteSpace(existingValue))
            {
                throw new RepositoryException($"Customer {customer.Id} not found");
            }
            await _redisCache.SetStringAsync(key, JsonSerializer.Serialize(customer), CacheEntryOptions, token);
        }

        public async Task<bool> IsExists(long customerId, CancellationToken token)
        {
            var key = GetKey(customerId);
            var existingValue = await _redisCache.GetStringAsync(key, token);
            return !string.IsNullOrWhiteSpace(existingValue);
        }

        public async Task<CustomerDto[]> FindManyAsync(long[] ids, CancellationToken token)
        {
            var result = new List<CustomerDto>();
            foreach(var id in ids)
            {
                var customer = await FindAsync(id, token);
                if (customer != null)
                {
                    result.Add(customer);
                }
            }
            return result.ToArray();
        }
    }
}
