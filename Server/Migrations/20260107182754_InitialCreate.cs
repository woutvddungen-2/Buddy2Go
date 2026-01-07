using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phonenumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Buddys",
                columns: table => new
                {
                    RequesterId = table.Column<int>(type: "int", nullable: false),
                    AddresseeId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buddys", x => new { x.RequesterId, x.AddresseeId });
                    table.ForeignKey(
                        name: "FK_Buddys_Users_AddresseeId",
                        column: x => x.AddresseeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Buddys_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DangerousPlace",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReportedById = table.Column<int>(type: "int", nullable: false),
                    PlaceType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GPS = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReportedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangerousPlace", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DangerousPlace_Users_ReportedById",
                        column: x => x.ReportedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Journeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StartGPS = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EndGPS = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Journeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Journeys_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "JourneyParticipants",
                columns: table => new
                {
                    JourneyId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JourneyParticipants", x => new { x.UserId, x.JourneyId });
                    table.ForeignKey(
                        name: "FK_JourneyParticipants_Journeys_JourneyId",
                        column: x => x.JourneyId,
                        principalTable: "Journeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JourneyParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    JourneyId = table.Column<int>(type: "int", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SentAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Journeys_JourneyId",
                        column: x => x.JourneyId,
                        principalTable: "Journeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Journeys",
                columns: new[] { "Id", "CreatedAt", "EndGPS", "FinishedAt", "StartGPS", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(4565), "51.924420,4.477733", null, "52.370216,4.895168", null },
                    { 2, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(4780), "51.441642,5.469722", null, "52.090737,5.121420", null },
                    { 3, new DateTime(2026, 1, 6, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(4781), "444556677", null, "111222333", null },
                    { 4, new DateTime(2026, 1, 6, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(4812), "111222333", new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(4813), "444556677", null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "PasswordHash", "Phonenumber", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(118), "alice@test.com", "Btd5kOga0bCQboFgEC27wQXmHO/7+ycka95ivGi4EXXAEOj303ehnFqmaGr3+rHi", "0600000000", "Alice" },
                    { 2, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(250), "bob@test.com", "DbjdjPrHA2CdSDtuDrpWqWbAcxPQIoxHxNz73a0P8CFWd/Sg55yo/+FTDbdsxtdL", "0611111111", "Bob" },
                    { 3, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(251), "charlie@test.com", "JULrd1HVJ17woc2HEqrjpsOx6Ac+z60MWP0lmhPlKB7HupLEX7ANdCZeqTABaBOO", "0622222222", "Charlie" },
                    { 4, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(252), "Joseph@test.com", "JULrd1HVJ17woc2HEqrjpsOx6Ac+z60MWP0lmhPlKB7HupLEX7ANdCZeqTABaBOO", "0633333333", "Joseph" },
                    { 5, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(253), "Diana@test.com", "JULrd1HVJ17woc2HEqrjpsOx6Ac+z60MWP0lmhPlKB7HupLEX7ANdCZeqTABaBOO", "0644444444", "Diana" }
                });

            migrationBuilder.InsertData(
                table: "Buddys",
                columns: new[] { "AddresseeId", "RequesterId", "RequestedAt", "Status" },
                values: new object[,]
                {
                    { 2, 1, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(7077), 1 },
                    { 3, 1, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(7277), 1 },
                    { 4, 2, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(7279), 1 },
                    { 4, 3, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(7281), 0 },
                    { 1, 4, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(7279), 1 },
                    { 2, 5, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(7280), 1 }
                });

            migrationBuilder.InsertData(
                table: "DangerousPlace",
                columns: new[] { "Id", "Description", "GPS", "PlaceType", "ReportedAt", "ReportedById" },
                values: new object[,]
                {
                    { 1, "Very dark street, watch out!", "52.370216,4.895168", 3, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1 },
                    { 2, "Lots of garbage here", "51.924420,4.477733", 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2 }
                });

            migrationBuilder.InsertData(
                table: "JourneyParticipants",
                columns: new[] { "JourneyId", "UserId", "JoinedAt", "Role" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(5442), 0 },
                    { 2, 1, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(5517), 1 },
                    { 1, 2, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(5513), 1 },
                    { 2, 2, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(5516), 0 },
                    { 1, 3, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(5514), 1 },
                    { 1, 4, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(5515), 1 },
                    { 2, 4, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(5518), 1 },
                    { 3, 4, new DateTime(2026, 1, 6, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(5520), 0 },
                    { 4, 4, new DateTime(2026, 1, 6, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(5522), 0 },
                    { 2, 5, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(5519), 1 }
                });

            migrationBuilder.InsertData(
                table: "Messages",
                columns: new[] { "Id", "Content", "JourneyId", "SenderId", "SentAt" },
                values: new object[,]
                {
                    { 1, "Hi Bob!", 1, 1, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(6062) },
                    { 2, "Hey Alice!", 1, 2, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(6132) },
                    { 3, "Hello Charlie!", 1, 2, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(6133) },
                    { 4, "Hi Bob!", 2, 1, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(6134) },
                    { 5, "Hey Alice!", 2, 2, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(6135) },
                    { 6, "Hello Charlie!", 2, 2, new DateTime(2026, 1, 7, 18, 27, 53, 941, DateTimeKind.Utc).AddTicks(6147) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buddys_AddresseeId",
                table: "Buddys",
                column: "AddresseeId");

            migrationBuilder.CreateIndex(
                name: "IX_DangerousPlace_ReportedById",
                table: "DangerousPlace",
                column: "ReportedById");

            migrationBuilder.CreateIndex(
                name: "IX_JourneyParticipants_JourneyId",
                table: "JourneyParticipants",
                column: "JourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_Journeys_UserId",
                table: "Journeys",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_JourneyId",
                table: "Messages",
                column: "JourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Buddys");

            migrationBuilder.DropTable(
                name: "DangerousPlace");

            migrationBuilder.DropTable(
                name: "JourneyParticipants");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Journeys");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
