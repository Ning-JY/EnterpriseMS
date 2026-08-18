using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnterpriseMS.Migrations
{
    /// <inheritdoc />
    public partial class Addzaojia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 101L,
                column: "real_name",
                value: "甯金元");

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 102L,
                columns: new[] { "gender", "real_name" },
                values: new object[] { 2, "曹丽君" });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 103L,
                column: "real_name",
                value: "刘润泽");

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 104L,
                columns: new[] { "gender", "real_name" },
                values: new object[] { 1, "王帅伟" });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 105L,
                column: "real_name",
                value: "杨通");

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 106L,
                columns: new[] { "gender", "real_name" },
                values: new object[] { 1, "郭家松" });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 107L,
                column: "real_name",
                value: "陈俊童");

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 108L,
                columns: new[] { "gender", "real_name" },
                values: new object[] { 2, "舒影" });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 109L,
                columns: new[] { "gender", "real_name" },
                values: new object[] { 2, "肖玲" });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 110L,
                column: "real_name",
                value: "魏利");

            migrationBuilder.InsertData(
                table: "sys_menu",
                columns: new[] { "Id", "component", "CreatedAt", "CreatedBy", "icon", "IsDeleted", "menu_name", "menu_type", "parent_id", "path", "perms", "sort", "status", "UpdatedAt", "UpdatedBy", "visible" },
                values: new object[,]
                {
                    { 8L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-calculator", false, "造价小工具", "M", 0L, "/tool", null, 8, 1, null, null, 1 },
                    { 17L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-bug", false, "Debug工具", "C", 1L, "/system/debug", "sys:debug:index", 9, 1, null, null, 1 },
                    { 81L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-file-word", false, "报告生成", "C", 8L, "/tool/report", null, 1, 1, null, null, 1 },
                    { 82L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-coins", false, "费用计算器", "C", 8L, "/tool/calculator", null, 2, 1, null, null, 1 },
                    { 320L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "批量导入", "F", 31L, null, "proj:project:import", 10, 1, null, null, 0 }
                });

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 1L,
                column: "password_hash",
                value: "$2a$12$v5WFKROXEG/wff9BT.BZ6OkU1/71d2clQmy0QH9CiFyX8ow69Wpbq");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "password_hash", "real_name", "username" },
                values: new object[] { "$2a$12$DX05tDJsaxos7lH6h0ChJ.OcC9WsUyQNLj6543GzaXddDKwZsJH6y", "甯金元", "ningjinyuan" });

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "password_hash", "real_name", "username" },
                values: new object[] { "$2a$12$rSOUgcrvaltbJOZzCxP2Tu61.nu18ZJG.toM0hIPDkevDVqRXVxqS", "曹丽君", "caolijun" });

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "password_hash", "real_name", "username" },
                values: new object[] { "$2a$12$PLp.UAYedll.rBOIkwOOBek/VkERgJMHdzhRtMmflBGex.Ql8IPAG", "刘润泽", "liurunze" });

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 5L,
                columns: new[] { "password_hash", "real_name", "username" },
                values: new object[] { "$2a$12$Sdloth5NhOSEuc33RSO6ku8iJa/8sfzTvVzMi/ROkTOtyPzt8tyae", "王帅伟", "wangshuaiwei" });

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 6L,
                columns: new[] { "password_hash", "real_name", "username" },
                values: new object[] { "$2a$12$CgUHf5SP6HLmly3BEGKXQekrRAkgxfnZV.zt7lTqqUo/TuKPkbodW", "杨通", "yangtong" });

            migrationBuilder.InsertData(
                table: "sys_role_menu",
                columns: new[] { "menu_id", "role_id" },
                values: new object[,]
                {
                    { 8L, 1L },
                    { 17L, 1L },
                    { 81L, 1L },
                    { 82L, 1L },
                    { 320L, 1L }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 8L, 1L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 17L, 1L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 81L, 1L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 82L, 1L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 320L, 1L });

            migrationBuilder.DeleteData(
                table: "sys_menu",
                keyColumn: "Id",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "sys_menu",
                keyColumn: "Id",
                keyValue: 17L);

            migrationBuilder.DeleteData(
                table: "sys_menu",
                keyColumn: "Id",
                keyValue: 81L);

            migrationBuilder.DeleteData(
                table: "sys_menu",
                keyColumn: "Id",
                keyValue: 82L);

            migrationBuilder.DeleteData(
                table: "sys_menu",
                keyColumn: "Id",
                keyValue: 320L);

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 101L,
                column: "real_name",
                value: "张三");

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 102L,
                columns: new[] { "gender", "real_name" },
                values: new object[] { 1, "李四" });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 103L,
                column: "real_name",
                value: "王五");

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 104L,
                columns: new[] { "gender", "real_name" },
                values: new object[] { 2, "赵六" });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 105L,
                column: "real_name",
                value: "孙七");

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 106L,
                columns: new[] { "gender", "real_name" },
                values: new object[] { 2, "周八" });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 107L,
                column: "real_name",
                value: "吴九");

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 108L,
                columns: new[] { "gender", "real_name" },
                values: new object[] { 1, "郑十" });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 109L,
                columns: new[] { "gender", "real_name" },
                values: new object[] { 1, "陈晓明" });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 110L,
                column: "real_name",
                value: "林小燕");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 1L,
                column: "password_hash",
                value: "$2a$12$PpTgqbP/m/GWzU3BljZ42.JgGBGR6sE320gb6y2FWhV1m6D/zkkJG");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "password_hash", "real_name", "username" },
                values: new object[] { "$2a$12$EQdEmMQiD6riOkccykHh1O2khOkACvToK.hnNbRTKr3Hl5fLKWU7.", "张三", "zhangsan" });

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "password_hash", "real_name", "username" },
                values: new object[] { "$2a$12$mSjvmFXmY/heavNXG7fIFOpzoLa0B8aGi/sjKmLKQKsuIb07dxRQW", "李四", "lisi" });

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "password_hash", "real_name", "username" },
                values: new object[] { "$2a$12$6WxEdj9C94zXs7z.zPNKleeHY834ZYekV7qNtUVwKtBdOMjyIUhzO", "王五", "wangwu" });

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 5L,
                columns: new[] { "password_hash", "real_name", "username" },
                values: new object[] { "$2a$12$5QwnI0LbP4L1Qdwf/EYXvuBY.r4mu56gd50qNnoEEBHc3/njGP3CO", "赵六", "zhaoliu" });

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 6L,
                columns: new[] { "password_hash", "real_name", "username" },
                values: new object[] { "$2a$12$oWwkqxoPblCI0QX/OhJoCeQAdc2ZjplbEFF4smHpCVGaBhwsOZ4Vu", "孙七", "sunqi" });
        }
    }
}
