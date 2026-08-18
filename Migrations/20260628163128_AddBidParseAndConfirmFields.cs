using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnterpriseMS.Migrations
{
    /// <inheritdoc />
    public partial class AddBidParseAndConfirmFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address",
                table: "hr_employee");

            migrationBuilder.DropColumn(
                name: "avatar",
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
                name: "major",
                table: "hr_employee");

            migrationBuilder.DropColumn(
                name: "native_place",
                table: "hr_employee");

            migrationBuilder.AddColumn<bool>(
                name: "is_veto",
                table: "bid_requirement",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "needs_review",
                table: "bid_requirement",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "source_ref",
                table: "bid_requirement",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "elements_confirmed_at",
                table: "bid_project",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "elements_confirmed_by",
                table: "bid_project",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "format_rule_json",
                table: "bid_project",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "parse_stage",
                table: "bid_project",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "source_file_name",
                table: "bid_project",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "source_file_path",
                table: "bid_project",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "sys_menu",
                keyColumn: "Id",
                keyValue: 915L,
                column: "sort",
                value: 6);

            migrationBuilder.UpdateData(
                table: "sys_menu",
                keyColumn: "Id",
                keyValue: 916L,
                column: "sort",
                value: 9);

            migrationBuilder.InsertData(
                table: "sys_menu",
                columns: new[] { "Id", "component", "CreatedAt", "CreatedBy", "icon", "IsDeleted", "menu_name", "menu_type", "parent_id", "path", "perms", "sort", "status", "UpdatedAt", "UpdatedBy", "visible" },
                values: new object[,]
                {
                    { 917L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "确认招标要素", "F", 91L, null, "bid:project:confirm", 5, 1, null, null, 0 },
                    { 918L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "人员匹配", "F", 91L, null, "bid:project:match", 7, 1, null, null, 0 },
                    { 919L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "导出文件", "F", 91L, null, "bid:project:export", 8, 1, null, null, 0 }
                });

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 1L,
                column: "password_hash",
                value: "$2a$12$.Y.KqZJJTUUrKXmhQHKGm.xjxqcpPsspI/byYddSv53qjsdbze5n6");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 2L,
                column: "password_hash",
                value: "$2a$12$eBPF7iGSi6QyAiSEg6MCOeJKxulHOtbqoV3FQqcL8llJPbjw8MKDi");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 3L,
                column: "password_hash",
                value: "$2a$12$FGdoerd8gP13WaXQ2dnBGOicdK0SN/7jXGzB18bgUi7P.lLkADXs.");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 4L,
                column: "password_hash",
                value: "$2a$12$LBkRcYgc0jg83JXprOGNEe0YJLifV8qLpLvKcWBD2h8XEtnuJ.MlG");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 5L,
                column: "password_hash",
                value: "$2a$12$tPgo0mUWvHu8XCOgEhJghO8saGCiAcnmSuuihZBl2mP7muE5bJXn.");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 6L,
                column: "password_hash",
                value: "$2a$12$6NlB1Z2ajBTh/1sGSHHGaOr6FsFFaK5dxAgHcp358A5.PjCLp6vbu");

            migrationBuilder.InsertData(
                table: "sys_role_menu",
                columns: new[] { "menu_id", "role_id" },
                values: new object[,]
                {
                    { 917L, 3L },
                    { 918L, 3L },
                    { 919L, 3L }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 917L, 3L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 918L, 3L });

            migrationBuilder.DeleteData(
                table: "sys_role_menu",
                keyColumns: new[] { "menu_id", "role_id" },
                keyValues: new object[] { 919L, 3L });

            migrationBuilder.DeleteData(
                table: "sys_menu",
                keyColumn: "Id",
                keyValue: 917L);

            migrationBuilder.DeleteData(
                table: "sys_menu",
                keyColumn: "Id",
                keyValue: 918L);

            migrationBuilder.DeleteData(
                table: "sys_menu",
                keyColumn: "Id",
                keyValue: 919L);

            migrationBuilder.DropColumn(
                name: "is_veto",
                table: "bid_requirement");

            migrationBuilder.DropColumn(
                name: "needs_review",
                table: "bid_requirement");

            migrationBuilder.DropColumn(
                name: "source_ref",
                table: "bid_requirement");

            migrationBuilder.DropColumn(
                name: "elements_confirmed_at",
                table: "bid_project");

            migrationBuilder.DropColumn(
                name: "elements_confirmed_by",
                table: "bid_project");

            migrationBuilder.DropColumn(
                name: "format_rule_json",
                table: "bid_project");

            migrationBuilder.DropColumn(
                name: "parse_stage",
                table: "bid_project");

            migrationBuilder.DropColumn(
                name: "source_file_name",
                table: "bid_project");

            migrationBuilder.DropColumn(
                name: "source_file_path",
                table: "bid_project");

            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "hr_employee",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "avatar",
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
                name: "major",
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

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 101L,
                columns: new[] { "address", "avatar", "bank_account", "bank_name", "birth_date", "emergency_contact", "emergency_phone", "graduate_school", "major", "native_place" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 102L,
                columns: new[] { "address", "avatar", "bank_account", "bank_name", "birth_date", "emergency_contact", "emergency_phone", "graduate_school", "major", "native_place" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 103L,
                columns: new[] { "address", "avatar", "bank_account", "bank_name", "birth_date", "emergency_contact", "emergency_phone", "graduate_school", "major", "native_place" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 104L,
                columns: new[] { "address", "avatar", "bank_account", "bank_name", "birth_date", "emergency_contact", "emergency_phone", "graduate_school", "major", "native_place" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 105L,
                columns: new[] { "address", "avatar", "bank_account", "bank_name", "birth_date", "emergency_contact", "emergency_phone", "graduate_school", "major", "native_place" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 106L,
                columns: new[] { "address", "avatar", "bank_account", "bank_name", "birth_date", "emergency_contact", "emergency_phone", "graduate_school", "major", "native_place" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 107L,
                columns: new[] { "address", "avatar", "bank_account", "bank_name", "birth_date", "emergency_contact", "emergency_phone", "graduate_school", "major", "native_place" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 108L,
                columns: new[] { "address", "avatar", "bank_account", "bank_name", "birth_date", "emergency_contact", "emergency_phone", "graduate_school", "major", "native_place" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 109L,
                columns: new[] { "address", "avatar", "bank_account", "bank_name", "birth_date", "emergency_contact", "emergency_phone", "graduate_school", "major", "native_place" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 110L,
                columns: new[] { "address", "avatar", "bank_account", "bank_name", "birth_date", "emergency_contact", "emergency_phone", "graduate_school", "major", "native_place" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "sys_menu",
                keyColumn: "Id",
                keyValue: 915L,
                column: "sort",
                value: 5);

            migrationBuilder.UpdateData(
                table: "sys_menu",
                keyColumn: "Id",
                keyValue: 916L,
                column: "sort",
                value: 6);

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
        }
    }
}
