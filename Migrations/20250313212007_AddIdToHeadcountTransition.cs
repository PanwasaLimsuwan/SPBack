using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIdToHeadcountTransition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_HeadcountTransition",
                table: "HeadcountTransition");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "HeadcountTransition",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HeadcountTransition",
                table: "HeadcountTransition",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_HeadcountTransition",
                table: "HeadcountTransition");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "HeadcountTransition");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HeadcountTransition",
                table: "HeadcountTransition",
                column: "DateTime");
        }
    }
}
