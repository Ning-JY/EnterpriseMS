using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnterpriseMS.Migrations
{
    /// <inheritdoc />
    public partial class NotificationInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 注：本迁移仅做“新增结构”，不修改任何已有种子/用户数据 ──
            // 原模型与上一迁移之间存在未迁移的 Schema 漂移（hr_education、hr_work_experience、
            // sys_config、proj_project/hr_employee 扩展字段），随本次一并补齐；同时新增通知中心两张表。
            // 刻意不包含任何 DeleteData/InsertData/UpdateData（尤其是 sys_user.password_hash），
            // 以免应用迁移时重置用户密码或误删菜单。

            migrationBuilder.AddColumn<string>(
                name: "contract_scan_file",
                table: "proj_project",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "contract_signed",
                table: "proj_project",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "contract_signed_date",
                table: "proj_project",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "project_category",
                table: "proj_project",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "project_leader_id",
                table: "proj_project",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "project_overview",
                table: "proj_project",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "hr_employee",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "bank_account",
                table: "hr_employee",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "bank_name",
                table: "hr_employee",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "birth_date",
                table: "hr_employee",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "emergency_contact",
                table: "hr_employee",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "emergency_phone",
                table: "hr_employee",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "graduate_school",
                table: "hr_employee",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "highest_degree",
                table: "hr_employee",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "major",
                table: "hr_employee",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "nationality",
                table: "hr_employee",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "native_place",
                table: "hr_employee",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "political_status",
                table: "hr_employee",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "profile_photo",
                table: "hr_employee",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "social_insurance_no",
                table: "hr_employee",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "technical_level",
                table: "hr_employee",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "technical_title",
                table: "hr_employee",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "work_start_date",
                table: "hr_employee",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "hr_education",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    employee_id = table.Column<long>(type: "bigint", nullable: false),
                    school_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    major = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    degree = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    start_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    end_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_full_time = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_hr_education", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hr_education_hr_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "hr_employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hr_work_experience",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    employee_id = table.Column<long>(type: "bigint", nullable: false),
                    company_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    position = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    start_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    end_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    table.PrimaryKey("PK_hr_work_experience", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hr_work_experience_hr_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "hr_employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_notification_read",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    NotificationId = table.Column<long>(type: "bigint", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
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
                    table.PrimaryKey("PK_sys_notification_read", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_notification",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Link = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Level = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequiredPerm = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientId = table.Column<long>(type: "bigint", nullable: true),
                    SourceKey = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsRead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    table.PrimaryKey("PK_sys_notification", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_config",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    config_key = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    config_value = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    config_type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    group_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sort = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_sys_config", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_proj_project_project_leader_id",
                table: "proj_project",
                column: "project_leader_id");

            migrationBuilder.CreateIndex(
                name: "IX_hr_education_employee_id",
                table: "hr_education",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_hr_work_experience_employee_id",
                table: "hr_work_experience",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_sys_notification_read_UserId_NotificationId",
                table: "sys_notification_read",
                columns: new[] { "UserId", "NotificationId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_proj_project_hr_employee_project_leader_id",
                table: "proj_project",
                column: "project_leader_id",
                principalTable: "hr_employee",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_proj_project_hr_employee_project_leader_id",
                table: "proj_project");

            migrationBuilder.DropTable(
                name: "hr_education");

            migrationBuilder.DropTable(
                name: "hr_work_experience");

            migrationBuilder.DropTable(
                name: "sys_notification_read");

            migrationBuilder.DropTable(
                name: "sys_notification");

            migrationBuilder.DropTable(
                name: "sys_config");

            migrationBuilder.DropIndex(
                name: "IX_proj_project_project_leader_id",
                table: "proj_project");

            migrationBuilder.DropColumn(
                name: "contract_scan_file",
                table: "proj_project");

            migrationBuilder.DropColumn(
                name: "contract_signed",
                table: "proj_project");

            migrationBuilder.DropColumn(
                name: "contract_signed_date",
                table: "proj_project");

            migrationBuilder.DropColumn(
                name: "project_category",
                table: "proj_project");

            migrationBuilder.DropColumn(
                name: "project_leader_id",
                table: "proj_project");

            migrationBuilder.DropColumn(
                name: "project_overview",
                table: "proj_project");

            migrationBuilder.DropColumn(
                name: "address",
                table: "hr_employee");

            migrationBuilder.DropColumn(
                name: "bank_account",
                table: "hr_employee");

            migrationBuilder.DropColumn(
                name: "bank_name",
                table: "hr_employee");

            migrationBuilder.DropColumn(
                name: "birth_date",
                table: "hr_employee");

            migrationBuilder.DropColumn(
                name: "emergency_contact",
                table: "hr_employee");

            migrationBuilder.DropColumn(
                name: "emergency_phone",
                table: "hr_employee");

            migrationBuilder.DropColumn(
                name: "graduate_school",
                table: "hr_employee");

            migrationBuilder.DropColumn(
                name: "highest_degree",
                table: "hr_employee");

            migrationBuilder.DropColumn(
                name: "major",
                table: "hr_employee");

            migrationBuilder.DropColumn(
                name: "nationality",
                table: "hr_employee");

            migrationBuilder.DropColumn(
                name: "native_place",
                table: "hr_employee");

            migrationBuilder.DropColumn(
                name: "political_status",
                table: "hr_employee");

            migrationBuilder.DropColumn(
                name: "profile_photo",
                table: "hr_employee");

            migrationBuilder.DropColumn(
                name: "social_insurance_no",
                table: "hr_employee");

            migrationBuilder.DropColumn(
                name: "technical_level",
                table: "hr_employee");

            migrationBuilder.DropColumn(
                name: "technical_title",
                table: "hr_employee");

            migrationBuilder.DropColumn(
                name: "work_start_date",
                table: "hr_employee");
        }
    }
}
