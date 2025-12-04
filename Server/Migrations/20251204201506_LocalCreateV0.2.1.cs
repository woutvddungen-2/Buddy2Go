using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class LocalCreateV021 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Places",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    City = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    District = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CentreGPS = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Places", x => x.Id);
                })
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
                    IsVerified = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Journeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StartId = table.Column<int>(type: "int", nullable: false),
                    EndId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    StartAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Journeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Journeys_Places_EndId",
                        column: x => x.EndId,
                        principalTable: "Places",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Journeys_Places_StartId",
                        column: x => x.StartId,
                        principalTable: "Places",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "DangerousPlaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReportedById = table.Column<int>(type: "int", nullable: false),
                    PlaceType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GPS = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReportedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangerousPlaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DangerousPlaces_Users_ReportedById",
                        column: x => x.ReportedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Code = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Username = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserVerifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "JourneyMessages",
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
                    table.PrimaryKey("PK_JourneyMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JourneyMessages_Journeys_JourneyId",
                        column: x => x.JourneyId,
                        principalTable: "Journeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JourneyMessages_Users_SenderId",
                        column: x => x.SenderId,
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
                    Role = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
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
                name: "Ratings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    JourneyId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    RatingValue = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ratings_Journeys_JourneyId",
                        column: x => x.JourneyId,
                        principalTable: "Journeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ratings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Places",
                columns: new[] { "Id", "CentreGPS", "City", "District" },
                values: new object[,]
                {
                    { 1, "51.4416,5.4697", "Eindhoven", "Centrum" },
                    { 2, "51.4480,5.4485", "Eindhoven", "Strijp" },
                    { 3, "51.4147,5.4688", "Eindhoven", "Gestel" },
                    { 4, "51.4220,5.4938", "Eindhoven", "Stratum" },
                    { 5, "51.4440,5.5075", "Eindhoven", "Tongelre" },
                    { 6, "51.4582,5.4779", "Eindhoven", "Woensel-Zuid" },
                    { 7, "51.4886,5.4672", "Eindhoven", "Woensel-Noord" },
                    { 8, "51.4186,5.4028", "Veldhoven", "Centrum" },
                    { 9, "51.5075,5.3953", "Best", null },
                    { 10, "51.5096,5.4904", "Son en Breugel", null },
                    { 11, "51.3915,5.4590", "Waalre", null },
                    { 12, "51.4215,5.5590", "Geldrop", null },
                    { 13, "51.4750,5.5480", "Nuenen", null },
                    { 14, "51.4792,5.6570", "Helmond", null },
                    { 15, "51.4439,5.6204", "Mierlo", null },
                    { 16, "51.5052,5.3137", "Oirschot", null },
                    { 17, "51.3831,5.5728", "Heeze", null },
                    { 18, "51.3502,5.5492", "Leende", null },
                    { 19, "51.3141,5.6284", "Maarheeze", null },
                    { 20, "51.3012,5.6028", "Soerendonk", null },
                    { 21, "51.4310,5.2886", "Vessem", null },
                    { 22, "51.4099,5.3340", "Knegsel", null },
                    { 23, "51.4306,5.3822", "Wintelre", null },
                    { 24, "51.3503,5.3818", "Riethoven", null },
                    { 25, "51.3846,5.3704", "Steensel", null },
                    { 26, "51.3340,5.4138", "Westerhoven", null },
                    { 27, "51.5106,5.3501", "Spoordonk", null },
                    { 28, "51.5157,5.4813", "Breugel", null },
                    { 29, "51.5033,5.4132", "Aarle", null },
                    { 30, "51.4278,5.5322", "Zesgehuchten", null },
                    { 31, "51.4624,5.5622", "Eeneind", null },
                    { 32, "51.3646,5.2577", "Hapert", null },
                    { 33, "51.3800,5.2560", "Hoogeloon", null },
                    { 34, "51.3686,5.2196", "Bladel", null },
                    { 35, "51.4094,5.2338", "Casteren", null },
                    { 36, "51.4850,5.5400", "Gerwen", null },
                    { 37, "51.4254,5.3648", "Oerle", null },
                    { 38, "51.4203,5.4155", "Veldhoven", "Heikant" },
                    { 39, "51.3967,5.4642", "Aalst", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buddys_AddresseeId",
                table: "Buddys",
                column: "AddresseeId");

            migrationBuilder.CreateIndex(
                name: "IX_DangerousPlaces_ReportedById",
                table: "DangerousPlaces",
                column: "ReportedById");

            migrationBuilder.CreateIndex(
                name: "IX_JourneyMessages_JourneyId",
                table: "JourneyMessages",
                column: "JourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_JourneyMessages_SenderId",
                table: "JourneyMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_JourneyParticipants_JourneyId",
                table: "JourneyParticipants",
                column: "JourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_Journeys_EndId",
                table: "Journeys",
                column: "EndId");

            migrationBuilder.CreateIndex(
                name: "IX_Journeys_StartId",
                table: "Journeys",
                column: "StartId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_JourneyId_UserId",
                table: "Ratings",
                columns: new[] { "JourneyId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_UserId",
                table: "Ratings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVerifications_UserId",
                table: "UserVerifications",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Buddys");

            migrationBuilder.DropTable(
                name: "DangerousPlaces");

            migrationBuilder.DropTable(
                name: "JourneyMessages");

            migrationBuilder.DropTable(
                name: "JourneyParticipants");

            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropTable(
                name: "UserVerifications");

            migrationBuilder.DropTable(
                name: "Journeys");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Places");
        }
    }
}
