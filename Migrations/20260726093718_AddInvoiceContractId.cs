using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnterpriseMS.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceContractId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 601L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 602L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 603L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 604L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 605L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 606L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 701L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 702L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 703L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 704L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 705L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 706L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 707L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 708L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 709L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 710L);

            migrationBuilder.DeleteData(
                table: "sys_dict_type",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "sys_dict_type",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.AddColumn<long>(
                name: "contract_id",
                table: "proj_invoice",
                type: "bigint",
                nullable: true);

            // 注意：employee_status / contract_status 等字典数据由运行时种子器（DictSeeds + SystemSeedService）
            // 幂等写入，不应放在被 .gitignore 的迁移里（否则部署时迁移源码缺失会导致字典丢失），
            // 故此处不再 InsertData，避免与已存在的字典数据主键冲突、进而使迁移失败被吞。

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 1301L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 1302L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 1303L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 1401L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 1402L);

            migrationBuilder.DeleteData(
                table: "sys_dict_data",
                keyColumn: "Id",
                keyValue: 1403L);

            migrationBuilder.DeleteData(
                table: "sys_dict_type",
                keyColumn: "Id",
                keyValue: 13L);

            migrationBuilder.DeleteData(
                table: "sys_dict_type",
                keyColumn: "Id",
                keyValue: 14L);

            migrationBuilder.DropColumn(
                name: "contract_id",
                table: "proj_invoice");

            migrationBuilder.InsertData(
                table: "sys_dict_data",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "dict_label", "dict_type", "dict_value", "is_default", "IsDeleted", "sort", "status", "SysDictTypeId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 601L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "概算编制", "budget_task_type", "0", 0, false, 1, 1, null, null, null },
                    { 602L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "预算编制", "budget_task_type", "1", 0, false, 2, 1, null, null, null },
                    { 603L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "结算编制", "budget_task_type", "2", 0, false, 3, 1, null, null, null },
                    { 604L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "概算评审", "budget_task_type", "3", 0, false, 4, 1, null, null, null },
                    { 605L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "预算评审", "budget_task_type", "4", 0, false, 5, 1, null, null, null },
                    { 606L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "结算评审", "budget_task_type", "5", 0, false, 6, 1, null, null, null },
                    { 701L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "前期商务", "proj_status", "0", 0, false, 1, 1, null, null, null },
                    { 702L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "预计启动", "proj_status", "1", 0, false, 2, 1, null, null, null },
                    { 703L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "标书制作中", "proj_status", "2", 0, false, 3, 1, null, null, null },
                    { 704L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "投标/磋商中", "proj_status", "3", 0, false, 4, 1, null, null, null },
                    { 705L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "已中标·签订合同中", "proj_status", "4", 0, false, 5, 1, null, null, null },
                    { 706L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "已签回合同", "proj_status", "5", 0, false, 6, 1, null, null, null },
                    { 707L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "执行中", "proj_status", "6", 0, false, 7, 1, null, null, null },
                    { 708L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "成果提交", "proj_status", "7", 0, false, 8, 1, null, null, null },
                    { 709L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "已完成", "proj_status", "8", 0, false, 9, 1, null, null, null },
                    { 710L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "已终止", "proj_status", "9", 0, false, 10, 1, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "sys_dict_type",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "dict_name", "dict_type", "IsDeleted", "remark", "status", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 6L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "概预算任务类型", "budget_task_type", false, null, 1, null, null },
                    { 7L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "项目进度状态", "proj_status", false, null, 1, null, null }
                });

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 1L,
                column: "password_hash",
                value: "$2a$12$IAm7x3yIqg4YkckXWcQofe8AeYPZEEd9R2DYTaYfFNsQTsiL5Yv6u");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 2L,
                column: "password_hash",
                value: "$2a$12$dH5qEaxJ1PzJSrck9tlUqO5WivM54Ac6XLO.RYm/Dp9efGsufktGu");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 3L,
                column: "password_hash",
                value: "$2a$12$n1rWBUZ56DL.qG.fUsSyROcWpSYP4w7kuvhoM8RbpeiQo6FRBbUXK");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 4L,
                column: "password_hash",
                value: "$2a$12$Y601L/fZF6w44/Df7LyME.7z4yy6gCoQ2tC4RC3GsM6Hp5V8EetR6");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 5L,
                column: "password_hash",
                value: "$2a$12$jvPx6kyOQcDBJLj0L20W0u/lj0BDJVGB.4q3hedOz3kFF5R9/m2AO");

            migrationBuilder.UpdateData(
                table: "sys_user",
                keyColumn: "Id",
                keyValue: 6L,
                column: "password_hash",
                value: "$2a$12$2/cHZqIvHFY3ICiR1.6xsuVeMgiezdwxWPhj5zaCD.40pD1rgYBP2");
        }
    }
}
