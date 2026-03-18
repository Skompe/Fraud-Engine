using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitec.FraudEngine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Updates1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Transactions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.CreateIndex(
                name: "Ix_Transactions_CustomerId_Timestamp",
                table: "Transactions",
                columns: new[] { "CustomerId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_RuleConfigurations_RuleName",
                table: "RuleConfigurations",
                column: "RuleName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "Ix_Transactions_CustomerId_Timestamp",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_RuleConfigurations_RuleName",
                table: "RuleConfigurations");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Transactions",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);
        }
    }
}
