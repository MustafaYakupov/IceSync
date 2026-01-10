using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IceSync.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Workflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Unique Identifier"),
                    ApiWorkflowId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Universal Loader Workflow unique identifier (from API)"),
                    WorkflowName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, comment: "Workflow name"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, comment: "Shows whether workflow is active"),
                    MultiExecBehavior = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, comment: "Multi exec behavior"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, comment: "Shows whether Workflow is deleted or not")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflows", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_ApiWorkflowId",
                table: "Workflows",
                column: "ApiWorkflowId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Workflows");
        }
    }
}
