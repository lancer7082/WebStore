using FluentMigrator;

namespace Ozon.Route256.Five.CustomerService.Migrations;

[Migration(2)]
public class InsertAddressesMigration: ForwardOnlyMigration
{
    public override void Up()
    {
        var regionsInfo = new[] {
            (id: 1, name: "Moscow",        lat: 55.753927, lng: 37.609460),
            (id: 2, name: "StPetersburg",  lat: 59.938955, lng: 30.315644),
            (id: 3, name: "Novosibirsk",   lat: 55.030204, lng: 82.920430)
        };


        foreach (var region in regionsInfo)
        {
            Insert.IntoTable("addresses")
                .Row(new
                {
                    region_id = region.id,
                    city = region.name,
                    street = Faker.Address.StreetName(),
                    building = Faker.RandomNumber.Next().ToString(),
                    apartment = Faker.RandomNumber.Next().ToString(),
                    coordinates = $"({region.lat.ToString().Replace(',', '.')},{region.lng.ToString().Replace(',', '.')})"
                })
                ;

            Insert.IntoTable("regions")
                .Row(new
                {
                    region.id,
                    region.name,
                    warehouse_address_id = region.id
                })
                ;
        }
    }
}