using System;
using EnterpriseMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseMS.Migrations
{
    /// <inheritdoc />
    [Migration("20261010180000_AddContractAndLink")]
    [DbContextAttribute(typeof(AppDbContext))]
    public partial class AddContractAndLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 合同主表
            migrationBuilder.CreateTable(
                name: "contract",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    contract_no = table.Column<string>(type: "varchar(100)", nullable: false),
                    contract_type = table.Column<string>(type: "varchar(50)", nullable: false, defaultValue: "主合同"),
                    contract_name = table.Column<string>(type: "varchar(200)", nullable: true),
                    party_a = table.Column<string>(type: "varchar(200)", nullable: false),
                    party_b = table.Column<string>(type: "varchar(200)", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    sign_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    start_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    end_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    file_path = table.Column<string>(type: "varchar(500)", nullable: true),
                    file_name = table.Column<string>(type: "varchar(200)", nullable: true),
                    status = table.Column<int>(nullable: false, defaultValue: 1),
                    remark = table.Column<string>(type: "varchar(500)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contract", x => x.Id);
                });

            // 项目-合同关联表
            migrationBuilder.CreateTable(
                name: "proj_contract_link",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    contract_id = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proj_contract_link", x => x.Id);
                    table.ForeignKey("FK_proj_contract_link_contract", x => x.contract_id, "contract", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_proj_contract_link_project_contract",
                table: "proj_contract_link",
                columns: new[] { "project_id", "contract_id" },
                unique: true);

            // 数据迁移：proj_contract → contract + proj_contract_link
            migrationBuilder.Sql(@"
                INSERT INTO `contract` (contract_no, contract_type, contract_name, party_a, party_b,
                    amount, sign_date, start_date, end_date, file_path, file_name, status, remark,
                    CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted)
                SELECT contract_no, contract_type, contract_name, party_a, party_b,
                    amount, sign_date, start_date, end_date, file_path, file_name, status, remark,
                    CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
                FROM proj_contract;
            ");
            migrationBuilder.Sql(@"
                INSERT INTO proj_contract_link (project_id, contract_id, CreatedAt, CreatedBy, IsDeleted)
                SELECT pc.project_id, c.Id, pc.CreatedAt, pc.CreatedBy, pc.IsDeleted
                FROM proj_contract pc
                INNER JOIN `contract` c ON c.contract_no = pc.contract_no
                    AND c.party_a = pc.party_a AND c.party_b = pc.party_b
                    AND c.amount = pc.amount AND c.CreatedAt = pc.CreatedAt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "proj_contract_link");
            migrationBuilder.DropTable(name: "contract");
        }
    }
}
