using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class Seed_Data : Migration
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
                    OwnedBy = table.Column<int>(type: "int", nullable: false),
                    StartGPS = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EndGPS = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Journeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Journeys_Users_OwnedBy",
                        column: x => x.OwnedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "JourneyParticipants",
                columns: table => new
                {
                    JourneyId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
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
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "PasswordHash", "Phonenumber", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 26, 17, 14, 46, 37, DateTimeKind.Utc).AddTicks(830), "alice@test.com", "hashed1", "0612345678", "Alice" },
                    { 2, new DateTime(2025, 10, 26, 17, 14, 46, 37, DateTimeKind.Utc).AddTicks(935), "bob@test.com", "hashed2", "0687654321", "Bob" },
                    { 3, new DateTime(2025, 10, 26, 17, 14, 46, 37, DateTimeKind.Utc).AddTicks(936), "charlie@test.com", "hashed3", "0678901234", "Charlie" }
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
                table: "Journeys",
                columns: new[] { "Id", "CreatedAt", "EndGPS", "FinishedAt", "OwnedBy", "StartGPS" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 26, 17, 14, 46, 37, DateTimeKind.Utc).AddTicks(4482), "51.924420,4.477733", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "52.370216,4.895168" },
                    { 2, new DateTime(2025, 10, 26, 17, 14, 46, 37, DateTimeKind.Utc).AddTicks(4578), "51.441642,5.469722", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "52.090737,5.121420" }
                });

            migrationBuilder.InsertData(
                table: "JourneyParticipants",
                columns: new[] { "JourneyId", "UserId", "JoinedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 10, 26, 17, 14, 46, 37, DateTimeKind.Utc).AddTicks(5037) },
                    { 1, 2, new DateTime(2025, 10, 26, 17, 14, 46, 37, DateTimeKind.Utc).AddTicks(5114) },
                    { 2, 2, new DateTime(2025, 10, 26, 17, 14, 46, 37, DateTimeKind.Utc).AddTicks(5115) },
                    { 2, 3, new DateTime(2025, 10, 26, 17, 14, 46, 37, DateTimeKind.Utc).AddTicks(5116) }
                });

            migrationBuilder.InsertData(
                table: "Messages",
                columns: new[] { "Id", "Content", "JourneyId", "SenderId", "SentAt" },
                values: new object[,]
                {
                    { 1, "Hi Bob!", 1, 1, new DateTime(2025, 10, 26, 17, 14, 46, 37, DateTimeKind.Utc).AddTicks(5592) },
                    { 2, "Hey Alice!", 1, 2, new DateTime(2025, 10, 26, 17, 14, 46, 37, DateTimeKind.Utc).AddTicks(5659) },
                    { 3, "Hello Charlie!", 2, 2, new DateTime(2025, 10, 26, 17, 14, 46, 37, DateTimeKind.Utc).AddTicks(5660) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DangerousPlace_ReportedById",
                table: "DangerousPlace",
                column: "ReportedById");

            migrationBuilder.CreateIndex(
                name: "IX_JourneyParticipants_JourneyId",
                table: "JourneyParticipants",
                column: "JourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_Journeys_OwnedBy",
                table: "Journeys",
                column: "OwnedBy");

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
