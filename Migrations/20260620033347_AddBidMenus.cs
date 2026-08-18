using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnterpriseMS.Migrations
{
    /// <inheritdoc />
    public partial class AddBidMenus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "sys_menu",
                columns: new[] { "Id", "component", "CreatedAt", "CreatedBy", "icon", "IsDeleted", "menu_name", "menu_type", "parent_id", "path", "perms", "sort", "status", "UpdatedAt", "UpdatedBy", "visible" },
                values: new object[,]
                {
                    { 9L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-file-signature", false, "投标管理", "M", 0L, "/bid", null, 9, 1, null, null, 1 },
                    { 91L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-clipboard-list", false, "投标台账", "C", 9L, "/bid", "bid:project:list", 1, 1, null, null, 1 },
                    { 911L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "新建投标", "F", 91L, null, "bid:project:add", 1, 1, null, null, 0 },
                    { 912L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "编辑投标", "F", 91L, null, "bid:project:edit", 2, 1, null, null, 0 },
                    { 913L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "删除投标", "F", 91L, null, "bid:project:delete", 3, 1, null, null, 0 },
                    { 914L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "AI解析", "F", 91L, null, "bid:project:analyze", 4, 1, null, null, 0 },
                    { 915L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "AI生成", "F", 91L, null, "bid:project:generate", 5, 1, null, null, 0 },
                    { 916L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "AI审查", "F", 91L, null, "bid:project:review", 6, 1, null, null, 0 }
                });

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 1L,
                column: "password_hash",
                value: "$2a$12$fnTpkSXC4KK4h4qtJJtjC.yVIHVYmHEO0xHukjAngrcLD5jiIJg7q");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 2L,
                column: "password_hash",
                value: "$2a$12$GfUSaA0SqJHm4C1a9QhMieXciRXCrsVA4yDb8NW/Hj7dpSrTQCeMm");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 3L,
                column: "password_hash",
                value: "$2a$12$3GQ3pYFzaqhJxfkKjqYUsu9Jf7/Vi2Q2fETHYZmm4x4kXtr7CIDWK");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 4L,
                column: "password_hash",
                value: "$2a$12$WG1pE3s.miG7IK9ix0QaE.nYJP2pKGTDS4Ks2EWvJDi/XtxMO5Tqi");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 5L,
                column: "password_hash",
                value: "$2a$12$aPlES1TnIZdDiGsBm3Y7.OiaKvSA59GJY8gz5oRZJkell9OqbbwFS");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 6L,
                column: "password_hash",
                value: "$2a$12$1uK2LIp.Y8CiIJ5cblrzM.wyRmgmfpg5USEMJ3drDls7dsWd.UjQK");

            migrationBuilder.InsertData(
                table: "sys_role_menu",
                columns: new[] { "menu_id", "role_id" },
                values: new object[,]
                {
                    { 9L, 1L },
                    { 91L, 1L },
                    { 911L, 1L },
                    { 912L, 1L },
                    { 913L, 1L },
                    { 914L, 1L },
                    { 915L, 1L },
                    { 916L, 1L },
                    { 9L, 3L },
                    { 91L, 3L },
                    { 911L, 3L },
                    { 912L, 3L },
                    { 913L, 3L },
                    { 914L, 3L },
                    { 915L, 3L },
                    { 916L, 3L }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 9L, 1L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 91L, 1L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 911L, 1L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 912L, 1L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 913L, 1L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 914L, 1L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 915L, 1L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 916L, 1L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 9L, 3L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 91L, 3L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 911L, 3L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 912L, 3L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 913L, 3L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 914L, 3L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 915L, 3L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 916L, 3L });

            migrationBuilder.DeleteData(
                table: "sys_menu",
                keyColumn: "Id",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                table: "sys_menu",
                keyColumn: "Id",
                keyValue: 91L);

            migrationBuilder.DeleteData(
                table: "sys_menu",
                keyColumn: "Id",
                keyValue: 911L);

            migrationBuilder.DeleteData(
                table: "sys_menu",
                keyColumn: "Id",
                keyValue: 912L);

            migrationBuilder.DeleteData(
                table: "sys_menu",
                keyColumn: "Id",
                keyValue: 913L);

            migrationBuilder.DeleteData(
                table: "sys_menu",
                keyColumn: "Id",
                keyValue: 914L);

            migrationBuilder.DeleteData(
                table: "sys_menu",
                keyColumn: "Id",
                keyValue: 915L);

            migrationBuilder.DeleteData(
                table: "sys_menu",
                keyColumn: "Id",
                keyValue: 916L);

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 1L,
                column: "password_hash",
                value: "$2a$12$Km3dFpJdyv37Myhj/gMDx.BQAhuZHPKUNvgVMCrCxD4YJNMIPcjNS");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 2L,
                column: "password_hash",
                value: "$2a$12$Vw4yaRzeud9F5D0lK46pdOCUMqcM8LRRpGrLpsTBVgrepgUuHN3oS");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 3L,
                column: "password_hash",
                value: "$2a$12$xLL3mRE9fu2/2pBmktIwi..0ss3SPpFCG198s3IK411Y2Rv3/3bHO");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 4L,
                column: "password_hash",
                value: "$2a$12$GC3dmmsoMW7H.WtYE71n8.RByCkWsJUyUCOLOi.yHBpz5Ti1Kb1NS");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 5L,
                column: "password_hash",
                value: "$2a$12$Dx/6hpukSSo.C6KZ2Pc8pu4O3RNLO8Csnyhcv0BYdMm0/QlvRctHu");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 6L,
                column: "password_hash",
                value: "$2a$12$.9PuraBDpCHjVSCw2.aDiOnAxn/B8z5RNZjxP863wrYl.eh4LLUVu");
        }
    }
}
