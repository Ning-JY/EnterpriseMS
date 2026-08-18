using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseMS.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "TemplateDefinitions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "TemplateDefinitions");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 1L,
                column: "password_hash",
                value: "$2a$12$yTyQUqE0Qs8BakwZxAqFPuho0Dwrzd5vP4pJwXza2cVV0S4swDU4O");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 2L,
                column: "password_hash",
                value: "$2a$12$v7knWGk5ULxFMzdUVVYfFuGuF9f6EaXuhh8llfxh9nHTthsWF8eL.");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 3L,
                column: "password_hash",
                value: "$2a$12$eG7NNk3LY1Vzw8BfDLYUduQO8HddaGHlX5ghiF4HKVWmsmc5CY9KO");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 4L,
                column: "password_hash",
                value: "$2a$12$2wiLBEDS.TYF535U28d2pu.8iYzG8wWi0ZdHdDwQ95UZ8qPbsQcZa");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 5L,
                column: "password_hash",
                value: "$2a$12$tELVfz3i9gqWf0F0ynsqiu6j1f9ud51RvGHeVrRSfkGvpFhDZf.Vq");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 6L,
                column: "password_hash",
                value: "$2a$12$bkHU3BnApFfRROfxyM7S3uux.5TENHKlXdDAUJoz62tVJ1taqtJUO");
        }
    }
}
