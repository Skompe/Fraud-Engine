using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitec.FraudEngine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatrIdentitySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "FraudFlags",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "FraudFlags",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                table: "FraudFlags");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "FraudFlags");
        }
    }
}
