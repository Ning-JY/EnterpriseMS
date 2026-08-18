using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseMS.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hr_attachment",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    employee_id = table.Column<long>(type: "bigint", nullable: false),
                    file_name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_path = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    file_type = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    remark = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_attachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hr_attachment_hr_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "hr_employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 1L,
                column: "password_hash",
                value: "$2a$12$br0Zf/0j9.g83bR0rAwUM.5Sy1KwvS8y.y08X4Q0ndMj9md2PpCmO");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 2L,
                column: "password_hash",
                value: "$2a$12$m6x9vFVEbtOpDs4Pn3V0tO6dI1bfeXedur8csGpiOes6bD8uLhRRy");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 3L,
                column: "password_hash",
                value: "$2a$12$2Tfty7cy9E3.n/Khgt/mKORnvhwFCgYmEjhGGkNzAYwCk0bhzuspO");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 4L,
                column: "password_hash",
                value: "$2a$12$vRKbW7cxOsKOtTHTkRqb5u6T6j5O0S3nWp7ZzPmcfhN4WQl9eHhLa");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 5L,
                column: "password_hash",
                value: "$2a$12$6JGoOi8TdUJZDQ2jWOTrLe0VPr/CWa3UcXgfK9AxFZzWL.rVtWoBe");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 6L,
                column: "password_hash",
                value: "$2a$12$LKFfcJWnqFvJEHjKBhelyeSX4qD8E.R.jE8zPfyUfGvWHZZZqx/ea");

            migrationBuilder.CreateIndex(
                name: "IX_hr_attachment_employee_id",
                table: "hr_attachment",
                column: "employee_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hr_attachment");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 1L,
                column: "password_hash",
                value: "$2a$12$IajWRLpa0MxLrtI5ixnYGeHNTvRTLTE0NUHrdALb4esZMbOykeYHC");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 2L,
                column: "password_hash",
                value: "$2a$12$YedIPZlds3mvBpI.MzNh3uRJW3.3NZz2mNxLMZ0HpYDN5zVAdfjfG");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 3L,
                column: "password_hash",
                value: "$2a$12$47t97cOD2RlU86DrJ/gvi.ZO4Kb/kJyODzfw/5qKQwTZpY/N7cGQK");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 4L,
                column: "password_hash",
                value: "$2a$12$3coMaqZFA04/qpRm77zXVuLXw2dcZWspF5qkhRB8k2nYOsVBBJRmC");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 5L,
                column: "password_hash",
                value: "$2a$12$gfWhrPTHjrSPa9ih/Codl.JYlnXM4U/9kS/HFp5llMyytg1M7/zJ2");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 6L,
                column: "password_hash",
                value: "$2a$12$sBe.DL/xko3GZLRDWZ29Pu2dwmA9i3B2RCSCt961Rbj.cFZiqTDd6");
        }
    }
}
