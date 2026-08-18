using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnterpriseMS.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProjectTechBizLeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_proj_project_hr_employee_biz_leader_id",
                table: "proj_project");

            migrationBuilder.DropForeignKey(
                name: "FK_proj_project_hr_employee_tech_leader_id",
                table: "proj_project");

            migrationBuilder.DropIndex(
                name: "IX_proj_project_biz_leader_id",
                table: "proj_project");

            migrationBuilder.DropIndex(
                name: "IX_proj_project_tech_leader_id",
                table: "proj_project");

            migrationBuilder.DropColumn(
                name: "biz_leader_id",
                table: "proj_project");

            migrationBuilder.DropColumn(
                name: "tech_leader_id",
                table: "proj_project");

            migrationBuilder.InsertData(
                table: "sys_dict_data",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "dict_label", "dict_type", "dict_value", "is_default", "IsDeleted", "sort", "status", "SysDictTypeId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1501L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "造价", "proj_no_prefix", "造价", 1, false, 1, 1, null, null, null },
                    { 1502L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "设计", "proj_no_prefix", "设计", 0, false, 2, 1, null, null, null },
                    { 1503L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "勘察", "proj_no_prefix", "勘察", 0, false, 3, 1, null, null, null },
                    { 1504L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "监理", "proj_no_prefix", "监理", 0, false, 4, 1, null, null, null },
                    { 1505L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "咨询", "proj_no_prefix", "咨询", 0, false, 5, 1, null, null, null },
                    { 1506L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "全过程", "proj_no_prefix", "全过程", 0, false, 6, 1, null, null, null },
                    { 1507L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "其他", "proj_no_prefix", "其他", 0, false, 7, 1, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "sys_dict_type",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "dict_name", "dict_type", "IsDeleted", "remark", "status", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 15L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "项目编号前缀", "proj_no_prefix", false, null, 1, null, null });

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

            migrationBuilder.CreateIndex(
                name: "IX_hr_employee_post_id",
                table: "hr_employee",
                column: "post_id");

            migrationBuilder.AddForeignKey(
                name: "FK_hr_employee_sys_post_post_id",
                table: "hr_employee",
                column: "post_id",
                principalTable: "sys_post",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_hr_employee_sys_post_post_id",
                table: "hr_employee");

            migrationBuilder.DropIndex(
                name: "IX_hr_employee_post_id",
                table: "hr_employee");

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 1501L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 1502L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 1503L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 1504L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 1505L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 1506L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 1507L);

            migrationBuilder.DeleteData(
                table: "sys_dict_type",
                keyColumn: "Id",
                keyValue: 15L);

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
                table: "proj_project",
                keyColumn: "Id",
                keyValue: 1001L,
                columns: new[] { "biz_leader_id", "tech_leader_id" },
                values: new object[] { 103L, 108L });

            migrationBuilder.UpdateData(
                table: "proj_project",
                keyColumn: "Id",
                keyValue: 1002L,
                columns: new[] { "biz_leader_id", "tech_leader_id" },
                values: new object[] { 101L, 102L });

            migrationBuilder.UpdateData(
                table: "proj_project",
                keyColumn: "Id",
                keyValue: 1003L,
                columns: new[] { "biz_leader_id", "tech_leader_id" },
                values: new object[] { 107L, 103L });

            migrationBuilder.UpdateData(
                table: "proj_project",
                keyColumn: "Id",
                keyValue: 1004L,
                columns: new[] { "biz_leader_id", "tech_leader_id" },
                values: new object[] { 104L, 107L });

            migrationBuilder.UpdateData(
                table: "proj_project",
                keyColumn: "Id",
                keyValue: 1005L,
                columns: new[] { "biz_leader_id", "tech_leader_id" },
                values: new object[] { 101L, 105L });

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 1L,
                column: "password_hash",
                value: "$2a$12$OSKIyX1828pdnOKsXRHA4OeJt45KDp/Bl9u/jQEND46WzNys73mAi");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 2L,
                column: "password_hash",
                value: "$2a$12$BlbAru.2.WNUJcUtw/AOEuqz5MceVdf9zJOpoe08RvHBmfBCULrHa");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 3L,
                column: "password_hash",
                value: "$2a$12$AYMiNATtr1pxrVsi1ahtAuSuL07C0e9aRR/rNXwfhh3CdoLgZCz4W");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 4L,
                column: "password_hash",
                value: "$2a$12$3N1GXMkCnIi4XPyPN3K0ZepOYbxFOx3KOiwMAYfwP1e3JkFERSiIm");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 5L,
                column: "password_hash",
                value: "$2a$12$HkqdVUKBndyYTp6pHSw6cu3imoixSRb0neh/L3xZWDY.YRaGVg8KS");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 6L,
                column: "password_hash",
                value: "$2a$12$gR8YADSH2WkLrIkcgu/rxO86Wuq7sVfWSjM9yhtxWsSJyWclZ4ZdC");

            migrationBuilder.CreateIndex(
                name: "IX_proj_project_biz_leader_id",
                table: "proj_project",
                column: "biz_leader_id");

            migrationBuilder.CreateIndex(
                name: "IX_proj_project_tech_leader_id",
                table: "proj_project",
                column: "tech_leader_id");

            migrationBuilder.AddForeignKey(
                name: "FK_proj_project_hr_employee_biz_leader_id",
                table: "proj_project",
                column: "biz_leader_id",
                principalTable: "hr_employee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_proj_project_hr_employee_tech_leader_id",
                table: "proj_project",
                column: "tech_leader_id",
                principalTable: "hr_employee",
                principalColumn: "Id");
        }
    }
}
