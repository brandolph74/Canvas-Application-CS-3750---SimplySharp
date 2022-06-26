using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SimplySharp.Migrations.Submission
{
    public partial class addNotificationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignmentTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotiType = table.Column<string>(type: "nvarchar(1)", nullable: true),
                    NotiDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notification");
        }
    }
}
