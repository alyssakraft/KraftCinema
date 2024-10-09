using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KraftCinema.Data.Migrations
{
    /// <inheritdoc />
    public partial class MovieActorController : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MovieActor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovieID = table.Column<int>(type: "int", nullable: true),
                    ActorID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieActor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovieActor_Actor_ActorID",
                        column: x => x.ActorID,
                        principalTable: "Actor",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MovieActor_Movie_MovieID",
                        column: x => x.MovieID,
                        principalTable: "Movie",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovieActor_ActorID",
                table: "MovieActor",
                column: "ActorID");

            migrationBuilder.CreateIndex(
                name: "IX_MovieActor_MovieID",
                table: "MovieActor",
                column: "MovieID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovieActor");
        }
    }
}
