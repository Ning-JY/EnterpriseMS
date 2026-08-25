using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseMS.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLastLoginIp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "last_login_ip",
                table: "sys_user",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "last_login_ip", "password_hash" },
                values: new object[] { null, "$2a$12$4OK32aaSQz0pwJ01BgT9pe5XWGq4HrEREGjn/h3n6PAuXvEPzzoaa" });

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "last_login_ip", "password_hash" },
                values: new object[] { null, "$2a$12$Aftugj5/hXeXFdpDrebzEuuo9HGtCb/0KyjL0VG23XPLx9SO2EH7O" });

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "last_login_ip", "password_hash" },
                values: new object[] { null, "$2a$12$vZTQ6YqiMkeTMjOxvJK8LOmsL7iqAf3RQcSO8XWlzrZDHhVvjTT3K" });

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "last_login_ip", "password_hash" },
                values: new object[] { null, "$2a$12$wVl9QS68Ha03KdSpPBVyYOewkTp4KhVssCa2hdYYx9PEfAKDODJ/6" });

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 5L,
                columns: new[] { "last_login_ip", "password_hash" },
                values: new object[] { null, "$2a$12$g8F2p0lydrGqGAijZ.f1TurLWDeOXyxefbl.I15ZJmOwD.X.eKVZ2" });

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 6L,
                columns: new[] { "last_login_ip", "password_hash" },
                values: new object[] { null, "$2a$12$ghhzuEgvGw3f5ZjGWTlvp.4MFyrFZDF10Ytwk90MBZl3LxbImqoa2" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_login_ip",
                table: "sys_user");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 1L,
                column: "password_hash",
                value: "$2a$12$xy1BRL3w/E0ZAYePiCTyRObwxHMEaFBFXcwcYydBukWtPVZfRAAri");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 2L,
                column: "password_hash",
                value: "$2a$12$scGded5ZuUqRtk24ccG0ZevPFYEYjowUdoDzHvz2IUkROj2ew1q/S");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 3L,
                column: "password_hash",
                value: "$2a$12$NANYIo5swCcPieQ7RKZ7LOBut58zIMRHdHYnp/v.JDRKpGXFNIlEq");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 4L,
                column: "password_hash",
                value: "$2a$12$omAZhnmnnObYna/YV8rTvOFGibSkpYVt0JM6hzVlPO495ME9U6rw.");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 5L,
                column: "password_hash",
                value: "$2a$12$AmqGSBd/6c.6arjhcrd6gelrUsy.iGFwX.Ht0S3f3tfru86t16oFS");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 6L,
                column: "password_hash",
                value: "$2a$12$TCsm6lrtEpk06VxI8/R5f.W1b6IQB/TyGv3WneFPJiIHPHaDlLx3W");
        }
    }
}
