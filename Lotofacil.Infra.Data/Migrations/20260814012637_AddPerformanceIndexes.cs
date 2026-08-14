using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lotofacil.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ContestActivityLog_BaseContestName",
                table: "ContestActivityLog",
                column: "BaseContestName");

            migrationBuilder.CreateIndex(
                name: "IX_ContestActivityLog_Data",
                table: "ContestActivityLog",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_Contest_Name",
                table: "Contest",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_BaseContest_Data",
                table: "BaseContest",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_BaseContest_Name",
                table: "BaseContest",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContestActivityLog_BaseContestName",
                table: "ContestActivityLog");

            migrationBuilder.DropIndex(
                name: "IX_ContestActivityLog_Data",
                table: "ContestActivityLog");

            migrationBuilder.DropIndex(
                name: "IX_Contest_Name",
                table: "Contest");

            migrationBuilder.DropIndex(
                name: "IX_BaseContest_Data",
                table: "BaseContest");

            migrationBuilder.DropIndex(
                name: "IX_BaseContest_Name",
                table: "BaseContest");
        }
    }
}
