using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using Ozon.Route256.Five.OrderService.Domain.Dto;
using Ozon.Route256.Five.OrderService.Domain.Model;
using Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.DbProvider.Dto;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.InMemoryProvider;

public class InMemoryProvider
{
    public readonly ConcurrentDictionary<long, DbRegionDto> Regions = new(2, 3);
    public readonly ConcurrentDictionary<long, DbAddressDto> Addresses = new(2, 10);
    public readonly ConcurrentDictionary<long, CustomerDto> Customers = new(2, 10);
    public readonly ConcurrentDictionary<long, DbOrderDto> Orders = new(2, 10);

    public InMemoryProvider()
    {
        FillRegions();
        FillAddresses();
        FillCustomers();
        FillOrders();
    }

    private void FillRegions()
    {
        var regionsInfo = new[] { 
            (name: "Moscow",        lat: 55.753927, lng: 37.609460), 
            (name: "StPetersburg",  lat: 59.938955, lng: 30.315644), 
            (name: "Novosibirsk",   lat: 55.030204, lng: 82.920430)
        };

        var regionsDto = regionsInfo.Select((x, i) => new DbRegionDto(
            i + 1,
            x.name,
        i + 1
        ));

        var addresses = regionsInfo.Select((x, i) => new DbAddressDto(i + 1,
            Faker.Address.City(),
            Faker.Address.StreetName(),
            Faker.RandomNumber.Next().ToString(),
            Faker.RandomNumber.Next().ToString(),
            x.lat,
            x.lng,
            i + 1
        ));

        foreach (var region in regionsDto)
        {
            Regions[region.Id] = region;
        }
    }

    private void FillAddresses()
    {
        var addresses = Enumerable.Range(0, 100).Select(x => new DbAddressDto(
                x + 1,
                Faker.Address.City(),
                Faker.Address.StreetName(),
                Faker.RandomNumber.Next().ToString(),
                Faker.RandomNumber.Next().ToString(),
                Faker.RandomNumber.Next(),
                Faker.RandomNumber.Next(),
                Faker.RandomNumber.Next(1, 3)
            )
        );
        foreach (var address in addresses)
        {
            Addresses[address.Id] = address;
        }
    }

    private void FillCustomers()
    {
        var customers = Enumerable.Range(0, 10).Select(x => new CustomerDto(
            x + 1,
            Faker.Name.First(),
            Faker.Name.Last(),
            Faker.Phone.Number(),
            Faker.Internet.Email()
        ));
        foreach (var customer in customers)
        {
            Customers[customer.Id] = customer;
        }
    }

    private void FillOrders()
    {
        var orders = Enumerable.Range(0, 10).Select(x => new DbOrderDto(
            x + 1,
            Faker.RandomNumber.Next(1, 99),
            Faker.RandomNumber.Next(1, 999),
            Faker.RandomNumber.Next(1, 10),
            (byte)Faker.RandomNumber.Next(1, 3),
            DateTime.Now.AddMinutes(Faker.RandomNumber.Next(-60 * 24 * 3, 0)),
            Faker.RandomNumber.Next(1, 3),
            (byte)Faker.RandomNumber.Next(1, 5),
            x + 1,
            x + 1
           ));
        foreach (var order in orders)
        {
            Orders[order.Id] = order;
        }
    }
}