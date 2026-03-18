using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitec.FraudEngine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FKCIngestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
            name: "FK_Transactions_Customers_CustomerId",
            table: "Transactions");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
