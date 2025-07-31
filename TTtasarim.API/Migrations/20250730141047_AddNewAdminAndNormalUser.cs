using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TTtasarim.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNewAdminAndNormalUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "GSM", "Password", "UserType", "Username" },
                values: new object[,]
                {
                    { new Guid("12121212-1212-1212-1212-121212121212"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@ttortam.com", "5551234567", "admin123", "admin", "admin2" },
                    { new Guid("13131313-1313-1313-1313-131313131313"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mehmet@ttortam.com", "5559876543", "123", "normal", "mehmet" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"));
        }
    }
}
