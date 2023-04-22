using FluentMigrator;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Db.Migrations;

[Migration(1)]
public class CreateTableMigration : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("addresses")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("region_id").AsInt32().NotNullable()
            .WithColumn("city").AsString().NotNullable()
            .WithColumn("street").AsString().NotNullable()
            .WithColumn("building").AsString().NotNullable()
            .WithColumn("apartment").AsString().Nullable()
            .WithColumn("coordinates").AsCustom("point").NotNullable()
            ;

        Create.Table("regions")
            .WithColumn("id").AsInt32().PrimaryKey()
            .WithColumn("name").AsString().NotNullable().Unique()
            .WithColumn("warehouse_address_id").AsInt64().ForeignKey("addresses", "id")
            ;

        Create.Table("orders")
            .WithColumn("id").AsInt64().NotNullable().PrimaryKey()
            .WithColumn("quantity").AsInt32().NotNullable()
            .WithColumn("sum").AsDecimal(18, 2).NotNullable()
            .WithColumn("weight").AsDecimal(18, 4).NotNullable()
            .WithColumn("order_source").AsByte().NotNullable()
            .WithColumn("date").AsDateTimeOffset().NotNullable()
            .WithColumn("region_id").AsInt32().NotNullable()
            .WithColumn("order_state").AsByte().NotNullable()
            .WithColumn("customer_id").AsInt64().NotNullable()
            .WithColumn("address_id").AsInt64().ForeignKey("addresses", "id");
        ;
    }
}
