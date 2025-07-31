﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TTtasarim.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dealers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dealers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GSM = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Credits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DealerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Credits_Dealers_DealerId",
                        column: x => x.DealerId,
                        principalTable: "Dealers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserDealers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DealerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDealers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDealers_Dealers_DealerId",
                        column: x => x.DealerId,
                        principalTable: "Dealers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserDealers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccessNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DealerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceTransactions_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceTransactions_Dealers_DealerId",
                        column: x => x.DealerId,
                        principalTable: "Dealers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceTransactions_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CreditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OperationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditLogs_Credits_CreditId",
                        column: x => x.CreditId,
                        principalTable: "Credits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@tttasarim.com", "5555555555", "Admin123!", "admin", "admin" });

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

            migrationBuilder.CreateIndex(
                name: "IX_CreditLogs_CreditId",
                table: "CreditLogs",
                column: "CreditId");

            migrationBuilder.CreateIndex(
                name: "IX_Credits_DealerId",
                table: "Credits",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CompanyId",
                table: "Invoices",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceTransactions_CompanyId",
                table: "InvoiceTransactions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceTransactions_DealerId",
                table: "InvoiceTransactions",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceTransactions_InvoiceId",
                table: "InvoiceTransactions",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceTransactions_UserId",
                table: "InvoiceTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDealers_DealerId",
                table: "UserDealers",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDealers_UserId",
                table: "UserDealers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditLogs");

            migrationBuilder.DropTable(
                name: "InvoiceTransactions");

            migrationBuilder.DropTable(
                name: "UserDealers");

            migrationBuilder.DropTable(
                name: "Credits");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Dealers");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
