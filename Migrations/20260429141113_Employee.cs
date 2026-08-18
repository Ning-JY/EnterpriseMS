using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseMS.Migrations
{
    /// <inheritdoc />
    public partial class Employee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "education",
                table: "hr_employee",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

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
                columns: new[] { "address", "avatar", "bank_account", "bank_name", "birth_date", "education", "emergency_contact", "emergency_phone", "graduate_school", "major", "native_place" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 102L,
                columns: new[] { "address", "avatar", "bank_account", "bank_name", "birth_date", "education", "emergency_contact", "emergency_phone", "graduate_school", "major", "native_place" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 103L,
                columns: new[] { "address", "avatar", "bank_account", "bank_name", "birth_date", "education", "emergency_contact", "emergency_phone", "graduate_school", "major", "native_place" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 104L,
                columns: new[] { "address", "avatar", "bank_account", "bank_name", "birth_date", "education", "emergency_contact", "emergency_phone", "graduate_school", "major", "native_place" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 105L,
                columns: new[] { "address", "avatar", "bank_account", "bank_name", "birth_date", "education", "emergency_contact", "emergency_phone", "graduate_school", "major", "native_place" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 106L,
                columns: new[] { "address", "avatar", "bank_account", "bank_name", "birth_date", "education", "emergency_contact", "emergency_phone", "graduate_school", "major", "native_place" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 107L,
                columns: new[] { "address", "avatar", "bank_account", "bank_name", "birth_date", "education", "emergency_contact", "emergency_phone", "graduate_school", "major", "native_place" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 108L,
                columns: new[] { "address", "avatar", "bank_account", "bank_name", "birth_date", "education", "emergency_contact", "emergency_phone", "graduate_school", "major", "native_place" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 109L,
                columns: new[] { "address", "avatar", "bank_account", "bank_name", "birth_date", "education", "emergency_contact", "emergency_phone", "graduate_school", "major", "native_place" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "hr_employee",
                keyColumn: "Id",
                keyValue: 110L,
                columns: new[] { "address", "avatar", "bank_account", "bank_name", "birth_date", "education", "emergency_contact", "emergency_phone", "graduate_school", "major", "native_place" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 1L,
                column: "password_hash",
                value: "$2a$12$0peGqHqPbd4D5HeCfQSMieqsBhOctxdskCOklhk1W9XN5dD/L.D4.");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 2L,
                column: "password_hash",
                value: "$2a$12$/SvxVD3x4nA/xujos6TU5.n/4GYIe.VJek9JroRO4HKWTWaHsDEdW");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 3L,
                column: "password_hash",
                value: "$2a$12$v4ZLJr6gzJegbRl6ndSmfuELZCFFDVhLK/r5ltQuZlNE7odU8RFPa");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 4L,
                column: "password_hash",
                value: "$2a$12$7CPJFSDnvyqwsiBreCFDiuwbDXibofwpcAuNTTbHVW8.aCC9DoIuu");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 5L,
                column: "password_hash",
                value: "$2a$12$HbK.4lsJwNdaD0ob4IWou.tw5Arx2u1QrnFFL5tNNjWeJZ4QkwK0O");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 6L,
                column: "password_hash",
                value: "$2a$12$n4Nb40K2I/FrnBNZ7Rf0Hu0qQESnvBuntMl52C2gGAHwn.2FdIl02");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "education",
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
                column: "password_hash",
                value: "$2a$12$DX05tDJsaxos7lH6h0ChJ.OcC9WsUyQNLj6543GzaXddDKwZsJH6y");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 3L,
                column: "password_hash",
                value: "$2a$12$rSOUgcrvaltbJOZzCxP2Tu61.nu18ZJG.toM0hIPDkevDVqRXVxqS");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 4L,
                column: "password_hash",
                value: "$2a$12$PLp.UAYedll.rBOIkwOOBek/VkERgJMHdzhRtMmflBGex.Ql8IPAG");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 5L,
                column: "password_hash",
                value: "$2a$12$Sdloth5NhOSEuc33RSO6ku8iJa/8sfzTvVzMi/ROkTOtyPzt8tyae");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 6L,
                column: "password_hash",
                value: "$2a$12$CgUHf5SP6HLmly3BEGKXQekrRAkgxfnZV.zt7lTqqUo/TuKPkbodW");
        }
    }
}
