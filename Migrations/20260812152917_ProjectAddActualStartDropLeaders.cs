using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseMS.Migrations
{
    /// <inheritdoc />
    public partial class ProjectAddActualStartDropLeaders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "actual_start_date",
                table: "proj_project",
                type: "datetime(6)",
                nullable: true);

            // 清理历史迁移漂移遗留的孤立列（RemoveProjectTechBizLeader 迁移虽记入历史，
            // 但目标库仍残留 biz_leader_id / tech_leader_id 及其 FK/索引）。
            // 全部使用 IF EXISTS，保证在结构已一致的库上幂等、不报错。
            migrationBuilder.Sql("ALTER TABLE proj_project DROP FOREIGN KEY IF EXISTS FK_proj_project_hr_employee_biz_leader_id;");
            migrationBuilder.Sql("ALTER TABLE proj_project DROP FOREIGN KEY IF EXISTS FK_proj_project_hr_employee_tech_leader_id;");
            migrationBuilder.Sql("ALTER TABLE proj_project DROP INDEX IF EXISTS IX_proj_project_biz_leader_id;");
            migrationBuilder.Sql("ALTER TABLE proj_project DROP INDEX IF EXISTS IX_proj_project_tech_leader_id;");
            migrationBuilder.Sql("ALTER TABLE proj_project DROP COLUMN IF EXISTS biz_leader_id;");
            migrationBuilder.Sql("ALTER TABLE proj_project DROP COLUMN IF EXISTS tech_leader_id;");

            migrationBuilder.UpdateData(
                table: "proj_project",
                keyColumn: "Id",
                keyValue: 1001L,
                column: "actual_start_date",
                value: null);

            migrationBuilder.UpdateData(
                table: "proj_project",
                keyColumn: "Id",
                keyValue: 1002L,
                column: "actual_start_date",
                value: null);

            migrationBuilder.UpdateData(
                table: "proj_project",
                keyColumn: "Id",
                keyValue: 1003L,
                column: "actual_start_date",
                value: null);

            migrationBuilder.UpdateData(
                table: "proj_project",
                keyColumn: "Id",
                keyValue: 1004L,
                column: "actual_start_date",
                value: null);

            migrationBuilder.UpdateData(
                table: "proj_project",
                keyColumn: "Id",
                keyValue: 1005L,
                column: "actual_start_date",
                value: null);

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 1L,
                column: "password_hash",
                value: "$2a$12$EUmFkdWA3EiMxOyOyJuNnO6hcV5QpiExerY0L3L0r4lYlMhkU6WnS");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 2L,
                column: "password_hash",
                value: "$2a$12$GgGCfc0KkLo/TDUGtO5GyeHyh.feErSiyiUCsq2J2WqOeFYMkCVLq");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 3L,
                column: "password_hash",
                value: "$2a$12$MGw8BGaBDnKBymkdkheOEOeKPmvvBF4CTRJzd1YWBldkWVQxFihJ2");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 4L,
                column: "password_hash",
                value: "$2a$12$GxSjHnJPNmR4Lgy0MJ/nX.IduHHgcV4Gp2MLVJw0np3YHzU1GOz3u");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 5L,
                column: "password_hash",
                value: "$2a$12$lzroBoOjLcfBv9Hp9QnBq.OTly2D/tg2V3bU7TeZ1uQ2roNFUInbC");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 6L,
                column: "password_hash",
                value: "$2a$12$XCRL/s07YReMCIIFNNcE5OQKvoqSUz/1rjH7KFyxecLCzlvIE4D3u");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "actual_start_date",
                table: "proj_project");

            // 回滚时恢复被清理的孤立列（无外键约束，保持简单可逆）
            migrationBuilder.AddColumn<long>(
                name: "biz_leader_id",
                table: "proj_project",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "tech_leader_id",
                table: "proj_project",
                type: "bigint",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 1L,
                column: "password_hash",
                value: "$2a$12$m1cm.W/xkT9EUXufyi.GZ.sjQAjLDvr6FagOE3MJ8aN2G2CosktZe");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 2L,
                column: "password_hash",
                value: "$2a$12$KnJC.2UpTf0dN15ZXFku4emWBR8BOTwoLP0o8mo1dJc91Q9qOkuQy");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 3L,
                column: "password_hash",
                value: "$2a$12$xkfYHrjBhliET4BJa9S6nu.szbrex8FJQK1RhGE46EOIReUncpnv2");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 4L,
                column: "password_hash",
                value: "$2a$12$T7hY5daVZr/nppbxo8bwp.z.UbBsExDostYexsJS7j.5Na23E5Ti.");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 5L,
                column: "password_hash",
                value: "$2a$12$QZCtagZpVvaiQVXgdZDfj.SM7OXGH6p8K63kNHJeFw3jVFvUVfKy6");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 6L,
                column: "password_hash",
                value: "$2a$12$C5tf/WAZgQcDlIxJtFrP5.OUWiITpjiJv2qhLohQRyabK1ZnccaYK");
        }
    }
}
