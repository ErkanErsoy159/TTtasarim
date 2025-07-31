using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TTtasarim.API.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyGuidsStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Credits",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Invoices",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.DeleteData(
                table: "Invoices",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

            migrationBuilder.DeleteData(
                table: "Invoices",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"));

            migrationBuilder.DeleteData(
                table: "Invoices",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"));

            migrationBuilder.DeleteData(
                table: "Invoices",
                keyColumn: "Id",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"));

            migrationBuilder.DeleteData(
                table: "UserDealers",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("12121212-1212-1212-1212-121212121212"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("13131313-1313-1313-1313-131313131313"));

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"));

            migrationBuilder.DeleteData(
                table: "Dealers",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "Code", "CreatedAt", "Name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000020"), "TEDAS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Trakya Elektrik Dağıtım A.Ş." },
                    { new Guid("00000000-0000-0000-0000-000000000021"), "IGDAS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "İstanbul Gaz Dağıtım A.Ş." },
                    { new Guid("00000000-0000-0000-0000-000000000022"), "ISKI", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "İstanbul Su ve Kanalizasyon İdaresi" },
                    { new Guid("00000000-0000-0000-0000-000000000023"), "TTNET", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Türk Telekom" }
                });

            migrationBuilder.InsertData(
                table: "Dealers",
                columns: new[] { "Id", "Code", "CreatedAt", "Name", "Status" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000010"), "BAY001", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Test Bayisi", "aktif" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "GSM", "Password", "UserType", "Username" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@tttasarim.com", "5555555555", "Admin123!", "admin", "admin" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@ttortam.com", "5551234567", "admin123", "admin", "admin2" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mehmet@ttortam.com", "5559876543", "123", "normal", "mehmet" }
                });

            migrationBuilder.InsertData(
                table: "Credits",
                columns: new[] { "Id", "CreatedAt", "CurrentValue", "DealerId" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000030"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1000.00m, new Guid("00000000-0000-0000-0000-000000000010") });

            migrationBuilder.InsertData(
                table: "Invoices",
                columns: new[] { "Id", "AccessNo", "Amount", "CompanyId", "CreatedAt", "Description", "DueDate", "Status" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000100"), "5551234567", 250.50m, new Guid("00000000-0000-0000-0000-000000000020"), new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Elektrik Faturası - Ocak 2025", new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "beklemede" },
                    { new Guid("00000000-0000-0000-0000-000000000101"), "5551234567", 180.75m, new Guid("00000000-0000-0000-0000-000000000021"), new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Doğalgaz Faturası - Ocak 2025", new DateTime(2025, 2, 10, 0, 0, 0, 0, DateTimeKind.Utc), "beklemede" },
                    { new Guid("00000000-0000-0000-0000-000000000102"), "5551234567", 95.30m, new Guid("00000000-0000-0000-0000-000000000022"), new DateTime(2025, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Su Faturası - Ocak 2025", new DateTime(2025, 2, 5, 0, 0, 0, 0, DateTimeKind.Utc), "beklemede" },
                    { new Guid("00000000-0000-0000-0000-000000000103"), "2129876543", 320.00m, new Guid("00000000-0000-0000-0000-000000000023"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "İnternet + Telefon Faturası - Ocak 2025", new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "beklemede" },
                    { new Guid("00000000-0000-0000-0000-000000000104"), "5559876543", 420.80m, new Guid("00000000-0000-0000-0000-000000000020"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Elektrik Faturası - Büyük Tüketici", new DateTime(2025, 1, 28, 0, 0, 0, 0, DateTimeKind.Utc), "beklemede" }
                });

            migrationBuilder.InsertData(
                table: "UserDealers",
                columns: new[] { "Id", "CreatedAt", "DealerId", "UserId" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000040"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000010"), new Guid("00000000-0000-0000-0000-000000000001") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Credits",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000030"));

            migrationBuilder.DeleteData(
                table: "Invoices",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000100"));

            migrationBuilder.DeleteData(
                table: "Invoices",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000101"));

            migrationBuilder.DeleteData(
                table: "Invoices",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000102"));

            migrationBuilder.DeleteData(
                table: "Invoices",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000103"));

            migrationBuilder.DeleteData(
                table: "Invoices",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000104"));

            migrationBuilder.DeleteData(
                table: "UserDealers",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000040"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000020"));

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000021"));

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000022"));

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000023"));

            migrationBuilder.DeleteData(
                table: "Dealers",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "Code", "CreatedAt", "Name" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444444"), "TEDAS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Trakya Elektrik Dağıtım A.Ş." },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "IGDAS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "İstanbul Gaz Dağıtım A.Ş." },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "ISKI", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "İstanbul Su ve Kanalizasyon İdaresi" },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "TTNET", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Türk Telekom" }
                });

            migrationBuilder.InsertData(
                table: "Dealers",
                columns: new[] { "Id", "Code", "CreatedAt", "Name", "Status" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), "BAY001", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Test Bayisi", "aktif" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "GSM", "Password", "UserType", "Username" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@tttasarim.com", "5555555555", "Admin123!", "admin", "admin" },
                    { new Guid("12121212-1212-1212-1212-121212121212"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@ttortam.com", "5551234567", "admin123", "admin", "admin2" },
                    { new Guid("13131313-1313-1313-1313-131313131313"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mehmet@ttortam.com", "5559876543", "123", "normal", "mehmet" }
                });

            migrationBuilder.InsertData(
                table: "Credits",
                columns: new[] { "Id", "CreatedAt", "CurrentValue", "DealerId" },
                values: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1000.00m, new Guid("22222222-2222-2222-2222-222222222222") });

            migrationBuilder.InsertData(
                table: "Invoices",
                columns: new[] { "Id", "AccessNo", "Amount", "CompanyId", "CreatedAt", "Description", "DueDate", "Status" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "5551234567", 250.50m, new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Elektrik Faturası - Ocak 2025", new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "beklemede" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "5551234567", 180.75m, new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2025, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Doğalgaz Faturası - Ocak 2025", new DateTime(2025, 2, 10, 0, 0, 0, 0, DateTimeKind.Utc), "beklemede" },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "5551234567", 95.30m, new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2025, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Su Faturası - Ocak 2025", new DateTime(2025, 2, 5, 0, 0, 0, 0, DateTimeKind.Utc), "beklemede" },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "2129876543", 320.00m, new Guid("77777777-7777-7777-7777-777777777777"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "İnternet + Telefon Faturası - Ocak 2025", new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "beklemede" },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "5559876543", 420.80m, new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Elektrik Faturası - Büyük Tüketici", new DateTime(2025, 1, 28, 0, 0, 0, 0, DateTimeKind.Utc), "beklemede" }
                });

            migrationBuilder.InsertData(
                table: "UserDealers",
                columns: new[] { "Id", "CreatedAt", "DealerId", "UserId" },
                values: new object[] { new Guid("88888888-8888-8888-8888-888888888888"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), new Guid("11111111-1111-1111-1111-111111111111") });
        }
    }
}
