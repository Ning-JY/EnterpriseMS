using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnterpriseMS.Migrations
{
    /// <inheritdoc />
    public partial class Createdatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "info_category",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    category_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    parent_id = table.Column<long>(type: "bigint", nullable: false),
                    is_public = table.Column<int>(type: "int", nullable: false),
                    sort = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_info_category", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "kb_category",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icon = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sort = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kb_category", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_dept",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    dept_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    parent_id = table.Column<long>(type: "bigint", nullable: false),
                    ancestors = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    leader = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sort = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_dept", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_dict_type",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    dict_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dict_type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    remark = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_dict_type", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_login_log",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ip = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    browser = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    os = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    msg = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    login_time = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_login_log", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_menu",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    menu_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    parent_id = table.Column<long>(type: "bigint", nullable: false),
                    menu_type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    perms = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icon = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    path = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    component = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sort = table.Column<int>(type: "int", nullable: false),
                    visible = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_menu", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_oper_log",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    business_type = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    method = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    oper_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    oper_ip = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    oper_name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    error_msg = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    oper_time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    business_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_oper_log", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_post",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    post_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    post_code = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sort = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_post", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_role",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    role_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    role_code = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data_scope = table.Column<int>(type: "int", nullable: false),
                    sort = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    remark = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_role", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "info_article",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cover_image = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_public = table.Column<int>(type: "int", nullable: false),
                    is_top = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    publish_time = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    view_count = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_info_article", x => x.Id);
                    table.ForeignKey(
                        name: "FK_info_article_info_category_category_id",
                        column: x => x.category_id,
                        principalTable: "info_category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "kb_file",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    file_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    original_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_path = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    file_ext = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    version = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    download_count = table.Column<int>(type: "int", nullable: false),
                    is_pinned = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kb_file", x => x.Id);
                    table.ForeignKey(
                        name: "FK_kb_file_kb_category_category_id",
                        column: x => x.category_id,
                        principalTable: "kb_category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hr_employee",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    emp_no = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    real_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gender = table.Column<int>(type: "int", nullable: false),
                    id_card = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dept_id = table.Column<long>(type: "bigint", nullable: true),
                    post_id = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    entry_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    probation_end_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    formal_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    leave_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    remark = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_employee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hr_employee_sys_dept_dept_id",
                        column: x => x.dept_id,
                        principalTable: "sys_dept",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_user",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password_hash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    real_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    avatar = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dept_id = table.Column<long>(type: "bigint", nullable: true),
                    post_id = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    last_login_time = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    remark = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    employee_id = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_user", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sys_user_sys_dept_dept_id",
                        column: x => x.dept_id,
                        principalTable: "sys_dept",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_dict_data",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    dict_type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dict_label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dict_value = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sort = table.Column<int>(type: "int", nullable: false),
                    is_default = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    SysDictTypeId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_dict_data", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sys_dict_data_sys_dict_type_SysDictTypeId",
                        column: x => x.SysDictTypeId,
                        principalTable: "sys_dict_type",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_role_menu",
                columns: table => new
                {
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    menu_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_role_menu", x => new { x.role_id, x.menu_id });
                    table.ForeignKey(
                        name: "FK_sys_role_menu_sys_menu_menu_id",
                        column: x => x.menu_id,
                        principalTable: "sys_menu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sys_role_menu_sys_role_role_id",
                        column: x => x.role_id,
                        principalTable: "sys_role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "budget_task",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    task_no = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    task_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    task_type = table.Column<int>(type: "int", nullable: false),
                    dept_id = table.Column<long>(type: "bigint", nullable: true),
                    owner_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    owner_contact = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    owner_phone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contractor_name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    building_scale = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    submit_amount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    approved_amount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    reduction_rate = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    quota_basis = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fee_standard = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tech_leader_id = table.Column<long>(type: "bigint", nullable: true),
                    biz_leader_id = table.Column<long>(type: "bigint", nullable: true),
                    plan_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    remark = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budget_task", x => x.Id);
                    table.ForeignKey(
                        name: "FK_budget_task_hr_employee_biz_leader_id",
                        column: x => x.biz_leader_id,
                        principalTable: "hr_employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_budget_task_hr_employee_tech_leader_id",
                        column: x => x.tech_leader_id,
                        principalTable: "hr_employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_budget_task_sys_dept_dept_id",
                        column: x => x.dept_id,
                        principalTable: "sys_dept",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hr_certificate",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    employee_id = table.Column<long>(type: "bigint", nullable: false),
                    cert_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cert_type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cert_no = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    issue_org = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    issue_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    expire_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    file_path = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    remark = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_certificate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hr_certificate_hr_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "hr_employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hr_contract",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    employee_id = table.Column<long>(type: "bigint", nullable: false),
                    contract_no = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contract_type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    start_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    sign_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    file_path = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    remark = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hr_contract", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hr_contract_hr_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "hr_employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "proj_project",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    proj_no = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    proj_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dept_id = table.Column<long>(type: "bigint", nullable: true),
                    biz_type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    owner_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    owner_contact = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    owner_phone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    procurement_type = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    limit_price = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    contract_amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    is_joint_venture = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    our_ratio = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    tech_leader_id = table.Column<long>(type: "bigint", nullable: true),
                    biz_leader_id = table.Column<long>(type: "bigint", nullable: true),
                    sign_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    plan_end_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    actual_end_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    bid_deadline = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    progress_status = table.Column<int>(type: "int", nullable: false),
                    status_updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    building_scale = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    remark = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proj_project", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proj_project_hr_employee_biz_leader_id",
                        column: x => x.biz_leader_id,
                        principalTable: "hr_employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_proj_project_hr_employee_tech_leader_id",
                        column: x => x.tech_leader_id,
                        principalTable: "hr_employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_proj_project_sys_dept_dept_id",
                        column: x => x.dept_id,
                        principalTable: "sys_dept",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_user_role",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_user_role", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_sys_user_role_sys_role_role_id",
                        column: x => x.role_id,
                        principalTable: "sys_role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sys_user_role_sys_user_user_id",
                        column: x => x.user_id,
                        principalTable: "sys_user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "budget_section",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    task_id = table.Column<long>(type: "bigint", nullable: false),
                    section_no = table.Column<int>(type: "int", nullable: false),
                    section_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    category = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contract_amount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    submit_amount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    approved_amount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budget_section", x => x.Id);
                    table.ForeignKey(
                        name: "FK_budget_section_budget_task_task_id",
                        column: x => x.task_id,
                        principalTable: "budget_task",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "review_opinion",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    task_id = table.Column<long>(type: "bigint", nullable: false),
                    opinion_no = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    opinion_type = table.Column<int>(type: "int", nullable: false),
                    category = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    basis = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    confirm_status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_opinion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_review_opinion_budget_task_task_id",
                        column: x => x.task_id,
                        principalTable: "budget_task",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "proj_acceptance",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    accept_batch = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    accept_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    accept_amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    invoice_no = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    remark = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proj_acceptance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proj_acceptance_proj_project_project_id",
                        column: x => x.project_id,
                        principalTable: "proj_project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "proj_contract",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    contract_no = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contract_type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contract_name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    party_a = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    party_b = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    sign_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    start_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    end_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    file_path = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    remark = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proj_contract", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proj_contract_proj_project_project_id",
                        column: x => x.project_id,
                        principalTable: "proj_project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "proj_file",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    file_category = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_path = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    file_ext = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    version = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    upload_by = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proj_file", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proj_file_proj_project_project_id",
                        column: x => x.project_id,
                        principalTable: "proj_project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "proj_invoice",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    receipt_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    invoice_no = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    invoice_type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    tax_rate = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    invoice_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    payer = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_received = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    received_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    invoice_file = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    invoice_file_name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_file = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payment_file_name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    remark = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proj_invoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proj_invoice_proj_project_project_id",
                        column: x => x.project_id,
                        principalTable: "proj_project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "proj_member",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    employee_id = table.Column<long>(type: "bigint", nullable: false),
                    role = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    duty_desc = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ratio = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    join_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    leave_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proj_member", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proj_member_hr_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "hr_employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_proj_member_proj_project_project_id",
                        column: x => x.project_id,
                        principalTable: "proj_project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "proj_milestone",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    milestone_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    milestone_type = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    plan_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    actual_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    owner_id = table.Column<long>(type: "bigint", nullable: true),
                    accept_amount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    is_overdue = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    remark = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sort = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proj_milestone", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proj_milestone_hr_employee_owner_id",
                        column: x => x.owner_id,
                        principalTable: "hr_employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_proj_milestone_proj_project_project_id",
                        column: x => x.project_id,
                        principalTable: "proj_project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "proj_oper_log",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    project_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    remark = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    oper_by = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    oper_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proj_oper_log", x => x.Id);
                    table.ForeignKey(
                        name: "FK_proj_oper_log_proj_project_project_id",
                        column: x => x.project_id,
                        principalTable: "proj_project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "kb_category",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "description", "icon", "IsDeleted", "name", "sort", "status", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "常用工作模板，合同模板、报告模板等", "fa-file-word", false, "模板文件", 1, 1, null, null },
                    { 2L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "公司内部通知、公告", "fa-bullhorn", false, "公司通知", 2, 1, null, null },
                    { 3L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "工程咨询、规划、造价等行业标准规范", "fa-book", false, "行业规范", 3, 1, null, null },
                    { 4L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "公司规章制度、管理办法", "fa-gavel", false, "规章制度", 4, 1, null, null },
                    { 5L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "内部培训讲义、学习材料", "fa-graduation-cap", false, "培训资料", 5, 1, null, null },
                    { 6L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "其他共享文件", "fa-folder-open", false, "其他", 6, 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "sys_dept",
                columns: new[] { "Id", "ancestors", "CreatedAt", "CreatedBy", "dept_name", "IsDeleted", "leader", "parent_id", "phone", "sort", "status", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1L, "0", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "总公司", false, null, 0L, null, 1, 1, null, null },
                    { 2L, "0,1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "工程咨询事业部", false, null, 1L, null, 1, 1, null, null },
                    { 3L, "0,1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "交通和土地利用事业部", false, null, 1L, null, 2, 1, null, null },
                    { 4L, "0,1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "城市设计事业部", false, null, 1L, null, 3, 1, null, null },
                    { 5L, "0,1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "区域和产业经济事业部", false, null, 1L, null, 4, 1, null, null },
                    { 6L, "0,1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "生产经营部", false, null, 1L, null, 5, 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "sys_dict_data",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "dict_label", "dict_type", "dict_value", "is_default", "IsDeleted", "sort", "status", "SysDictTypeId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 101L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "可行性研究报告", "biz_type", "可行性研究报告", 0, false, 1, 1, null, null, null },
                    { 102L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "节能评估报告", "biz_type", "节能评估报告", 0, false, 2, 1, null, null, null },
                    { 103L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "稳评报告", "biz_type", "稳评报告", 0, false, 3, 1, null, null, null },
                    { 104L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "概算编制", "biz_type", "概算编制", 0, false, 4, 1, null, null, null },
                    { 105L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "预算编制", "biz_type", "预算编制", 0, false, 5, 1, null, null, null },
                    { 106L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "结算编制", "biz_type", "结算编制", 0, false, 6, 1, null, null, null },
                    { 107L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "概算评审", "biz_type", "概算评审", 0, false, 7, 1, null, null, null },
                    { 108L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "预算评审", "biz_type", "预算评审", 0, false, 8, 1, null, null, null },
                    { 109L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "结算评审", "biz_type", "结算评审", 0, false, 9, 1, null, null, null },
                    { 110L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "控制性详细规划", "biz_type", "控制性详细规划", 0, false, 10, 1, null, null, null },
                    { 111L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "专项规划", "biz_type", "专项规划", 0, false, 11, 1, null, null, null },
                    { 112L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "城市更新规划", "biz_type", "城市更新规划", 0, false, 12, 1, null, null, null },
                    { 113L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "施工图设计", "biz_type", "施工图设计", 0, false, 13, 1, null, null, null },
                    { 114L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "战略咨询", "biz_type", "战略咨询", 0, false, 14, 1, null, null, null },
                    { 115L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "施工阶段全过程管控", "biz_type", "施工阶段全过程管控", 0, false, 15, 1, null, null, null },
                    { 201L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "竞争性磋商", "procurement_type", "竞争性磋商", 0, false, 1, 1, null, null, null },
                    { 202L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "询价", "procurement_type", "询价", 0, false, 2, 1, null, null, null },
                    { 203L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "公开招标", "procurement_type", "公开招标", 0, false, 3, 1, null, null, null },
                    { 204L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "邀请招标", "procurement_type", "邀请招标", 0, false, 4, 1, null, null, null },
                    { 205L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "公开招选", "procurement_type", "公开招选", 0, false, 5, 1, null, null, null },
                    { 206L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "框架协议采购", "procurement_type", "框架协议采购", 0, false, 6, 1, null, null, null },
                    { 207L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "单一来源", "procurement_type", "单一来源", 0, false, 7, 1, null, null, null },
                    { 301L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "固定期限", "contract_type", "固定期限", 0, false, 1, 1, null, null, null },
                    { 302L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "无固定期限", "contract_type", "无固定期限", 0, false, 2, 1, null, null, null },
                    { 303L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "劳务合同", "contract_type", "劳务合同", 0, false, 3, 1, null, null, null },
                    { 304L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "实习协议", "contract_type", "实习协议", 0, false, 4, 1, null, null, null },
                    { 401L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "注册规划师", "cert_type", "注册规划师", 0, false, 1, 1, null, null, null },
                    { 402L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "造价工程师", "cert_type", "造价工程师", 0, false, 2, 1, null, null, null },
                    { 403L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "注册建筑师", "cert_type", "注册建筑师", 0, false, 3, 1, null, null, null },
                    { 404L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "注册工程师", "cert_type", "注册工程师", 0, false, 4, 1, null, null, null },
                    { 405L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "建造师", "cert_type", "建造师", 0, false, 5, 1, null, null, null },
                    { 406L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "职称证书", "cert_type", "职称证书", 0, false, 6, 1, null, null, null },
                    { 407L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "岗位证书", "cert_type", "岗位证书", 0, false, 7, 1, null, null, null },
                    { 501L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "资料收集", "milestone_type", "资料收集", 0, false, 1, 1, null, null, null },
                    { 502L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "现状调研", "milestone_type", "现状调研", 0, false, 2, 1, null, null, null },
                    { 503L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "方案设计", "milestone_type", "方案设计", 0, false, 3, 1, null, null, null },
                    { 504L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "内部评审", "milestone_type", "内部评审", 0, false, 4, 1, null, null, null },
                    { 505L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "专家评审", "milestone_type", "专家评审", 0, false, 5, 1, null, null, null },
                    { 506L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "报批上报", "milestone_type", "报批上报", 0, false, 6, 1, null, null, null },
                    { 507L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "成果交付", "milestone_type", "成果交付", 0, false, 7, 1, null, null, null },
                    { 508L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "回款", "milestone_type", "回款", 0, false, 8, 1, null, null, null },
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
                    { 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "业务类型", "biz_type", false, null, 1, null, null },
                    { 2L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "采购方式", "procurement_type", false, null, 1, null, null },
                    { 3L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "合同类型", "contract_type", false, null, 1, null, null },
                    { 4L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "证书类型", "cert_type", false, null, 1, null, null },
                    { 5L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "里程碑类型", "milestone_type", false, null, 1, null, null },
                    { 6L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "概预算任务类型", "budget_task_type", false, null, 1, null, null },
                    { 7L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "项目进度状态", "proj_status", false, null, 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "sys_menu",
                columns: new[] { "Id", "component", "CreatedAt", "CreatedBy", "icon", "IsDeleted", "menu_name", "menu_type", "parent_id", "path", "perms", "sort", "status", "UpdatedAt", "UpdatedBy", "visible" },
                values: new object[,]
                {
                    { 1L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-cogs", false, "系统管理", "M", 0L, "/system", null, 1, 1, null, null, 1 },
                    { 2L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-users", false, "员工档案", "M", 0L, "/hr", null, 2, 1, null, null, 1 },
                    { 3L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-project-diagram", false, "项目管理", "M", 0L, "/project", null, 3, 1, null, null, 1 },
                    { 4L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-file-invoice-dollar", false, "概预算结算", "M", 0L, "/budget", null, 4, 1, null, null, 1 },
                    { 5L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-user-circle", false, "个人中心", "M", 0L, "/profile", null, 5, 1, null, null, 1 },
                    { 6L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-database", false, "知识库", "M", 0L, "/kb", null, 6, 1, null, null, 1 },
                    { 7L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-chart-bar", false, "报表中心", "M", 0L, "/report", null, 7, 1, null, null, 1 },
                    { 11L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-user", false, "用户管理", "C", 1L, "/system/user", "sys:user:list", 1, 1, null, null, 1 },
                    { 12L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-user-tag", false, "角色管理", "C", 1L, "/system/role", "sys:role:list", 2, 1, null, null, 1 },
                    { 13L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-sitemap", false, "部门管理", "C", 1L, "/system/dept", "sys:dept:list", 3, 1, null, null, 1 },
                    { 14L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-book", false, "字典管理", "C", 1L, "/system/dict", "sys:dict:list", 4, 1, null, null, 1 },
                    { 15L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-history", false, "操作日志", "C", 1L, "/system/log", "sys:log:list", 5, 1, null, null, 1 },
                    { 16L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-th-list", false, "菜单管理", "C", 1L, "/system/menu", "sys:menu:list", 6, 1, null, null, 1 },
                    { 21L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-id-card", false, "员工信息", "C", 2L, "/hr/employee", "hr:employee:list", 1, 1, null, null, 1 },
                    { 22L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-file-contract", false, "合同管理", "C", 2L, "/hr/contract", "hr:contract:list", 2, 1, null, null, 1 },
                    { 23L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-certificate", false, "证书管理", "C", 2L, "/hr/cert", "hr:cert:list", 3, 1, null, null, 1 },
                    { 31L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-clipboard-list", false, "项目台账", "C", 3L, "/project", "proj:project:list", 1, 1, null, null, 1 },
                    { 41L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-calculator", false, "任务台账", "C", 4L, "/budget", "budget:task:list", 1, 1, null, null, 1 },
                    { 51L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-id-card", false, "个人资料", "C", 5L, "/profile", null, 1, 1, null, null, 1 },
                    { 52L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-chart-bar", false, "产值统计", "C", 5L, "/my-stats", null, 2, 1, null, null, 1 },
                    { 61L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-folder-open", false, "文件浏览", "C", 6L, "/kb", "kb:file:list", 1, 1, null, null, 1 },
                    { 62L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-cog", false, "文件管理", "C", 6L, "/kb/manage", "kb:file:manage", 2, 1, null, null, 1 },
                    { 71L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-hand-holding-usd", false, "回款报表", "C", 7L, "/report/receipt", "report:receipt", 1, 1, null, null, 1 },
                    { 72L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-user-chart", false, "产值报表", "C", 7L, "/report/output", "report:output", 2, 1, null, null, 1 },
                    { 111L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "新增", "F", 11L, null, "sys:user:add", 1, 1, null, null, 0 },
                    { 112L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "编辑", "F", 11L, null, "sys:user:edit", 2, 1, null, null, 0 },
                    { 113L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "删除", "F", 11L, null, "sys:user:delete", 3, 1, null, null, 0 },
                    { 114L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "重置密码", "F", 11L, null, "sys:user:reset", 4, 1, null, null, 0 },
                    { 121L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "新增", "F", 12L, null, "sys:role:add", 1, 1, null, null, 0 },
                    { 122L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "编辑", "F", 12L, null, "sys:role:edit", 2, 1, null, null, 0 },
                    { 123L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "删除", "F", 12L, null, "sys:role:delete", 3, 1, null, null, 0 },
                    { 124L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "分配权限", "F", 12L, null, "sys:role:perm", 4, 1, null, null, 0 },
                    { 131L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "新增", "F", 13L, null, "sys:dept:add", 1, 1, null, null, 0 },
                    { 132L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "编辑", "F", 13L, null, "sys:dept:edit", 2, 1, null, null, 0 },
                    { 133L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "删除", "F", 13L, null, "sys:dept:delete", 3, 1, null, null, 0 },
                    { 141L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "新增", "F", 14L, null, "sys:dict:add", 1, 1, null, null, 0 },
                    { 142L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "编辑", "F", 14L, null, "sys:dict:edit", 2, 1, null, null, 0 },
                    { 143L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "删除", "F", 14L, null, "sys:dict:delete", 3, 1, null, null, 0 },
                    { 144L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "分配权限", "F", 14L, null, "sys:dict:perm", 4, 1, null, null, 0 },
                    { 151L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "新增", "F", 15L, null, "sys:log:add", 1, 1, null, null, 0 },
                    { 152L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "编辑", "F", 15L, null, "sys:log:edit", 2, 1, null, null, 0 },
                    { 153L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "删除", "F", 15L, null, "sys:log:delete", 3, 1, null, null, 0 },
                    { 154L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "分配权限", "F", 15L, null, "sys:log:perm", 4, 1, null, null, 0 },
                    { 161L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "新增", "F", 16L, null, "sys:menu:add", 1, 1, null, null, 0 },
                    { 162L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "编辑", "F", 16L, null, "sys:menu:edit", 2, 1, null, null, 0 },
                    { 163L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "删除", "F", 16L, null, "sys:menu:delete", 3, 1, null, null, 0 },
                    { 211L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "新增", "F", 21L, null, "hr:employee:add", 1, 1, null, null, 0 },
                    { 212L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "编辑", "F", 21L, null, "hr:employee:edit", 2, 1, null, null, 0 },
                    { 213L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "转正", "F", 21L, null, "hr:employee:formal", 3, 1, null, null, 0 },
                    { 214L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "离职", "F", 21L, null, "hr:employee:leave", 4, 1, null, null, 0 },
                    { 311L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "新建项目", "F", 31L, null, "proj:project:add", 1, 1, null, null, 0 },
                    { 312L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "编辑项目", "F", 31L, null, "proj:project:edit", 2, 1, null, null, 0 },
                    { 313L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "变更状态", "F", 31L, null, "proj:project:status", 3, 1, null, null, 0 },
                    { 314L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "终止项目", "F", 31L, null, "proj:project:terminate", 4, 1, null, null, 0 },
                    { 315L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "添加成员", "F", 31L, null, "proj:member:add", 5, 1, null, null, 0 },
                    { 316L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "编辑成员", "F", 31L, null, "proj:member:edit", 6, 1, null, null, 0 },
                    { 317L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "新增节点", "F", 31L, null, "proj:milestone:add", 7, 1, null, null, 0 },
                    { 318L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "完成节点", "F", 31L, null, "proj:milestone:done", 8, 1, null, null, 0 },
                    { 319L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "录入验收", "F", 31L, null, "proj:acceptance:add", 9, 1, null, null, 0 },
                    { 411L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "新增任务", "F", 41L, null, "budget:task:add", 1, 1, null, null, 0 },
                    { 412L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "编辑任务", "F", 41L, null, "budget:task:edit", 2, 1, null, null, 0 },
                    { 413L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "提交内审", "F", 41L, null, "budget:task:submit", 3, 1, null, null, 0 },
                    { 414L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "录入意见", "F", 41L, null, "budget:opinion:add", 4, 1, null, null, 0 },
                    { 621L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "上传", "F", 62L, null, "kb:file:upload", 1, 1, null, null, 0 },
                    { 622L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "删除", "F", 62L, null, "kb:file:delete", 2, 1, null, null, 0 }
                });

            migrationBuilder.InsertData(
                table: "sys_post",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "post_code", "post_name", "sort", "status", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", false, "ceo", "总经理", 1, 1, null, null },
                    { 2L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", false, "vceo", "副总经理", 2, 1, null, null },
                    { 3L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", false, "pm", "项目负责人", 3, 1, null, null },
                    { 4L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", false, "tech", "技术负责人", 4, 1, null, null },
                    { 5L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", false, "business", "商务负责人", 5, 1, null, null },
                    { 6L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", false, "senior", "高级工程师", 6, 1, null, null },
                    { 7L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", false, "engineer", "工程师", 7, 1, null, null },
                    { 8L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", false, "assist", "助理工程师", 8, 1, null, null },
                    { 9L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", false, "admin", "行政专员", 9, 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "sys_role",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "data_scope", "IsDeleted", "remark", "role_code", "role_name", "sort", "status", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 1, false, null, "superadmin", "超级管理员", 1, 1, null, null },
                    { 2L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 1, false, null, "admin", "管理员", 2, 1, null, null },
                    { 3L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 3, false, "可查看本部门及子部门全部项目", "pm", "项目经理", 3, 1, null, null },
                    { 4L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 4, false, "只能查看本人参与的项目", "engineer", "工程师", 4, 1, null, null },
                    { 5L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 2, false, null, "finance", "财务", 5, 1, null, null },
                    { 6L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 1, false, "只有查看权限，无增删改", "readonly", "只读", 6, 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "hr_employee",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "dept_id", "email", "emp_no", "entry_date", "formal_date", "gender", "id_card", "IsDeleted", "leave_date", "phone", "post_id", "probation_end_date", "real_name", "remark", "status", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 101L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 2L, "zhangsan@company.com", "EMP20230001", new DateTime(2020, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2020, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, null, false, null, "13800000001", 3L, null, "张三", null, 1, null, null },
                    { 102L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 2L, "lisi@company.com", "EMP20230002", new DateTime(2019, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, null, false, null, "13800000002", 4L, null, "李四", null, 1, null, null },
                    { 103L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 3L, "wangwu@company.com", "EMP20230003", new DateTime(2021, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2021, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, null, false, null, "13800000003", 3L, null, "王五", null, 1, null, null },
                    { 104L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 4L, "zhaoliu@company.com", "EMP20230004", new DateTime(2018, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2018, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, null, false, null, "13800000004", 6L, null, "赵六", null, 1, null, null },
                    { 105L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 2L, "sunqi@company.com", "EMP20230005", new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, null, false, null, "13800000005", 7L, null, "孙七", null, 1, null, null },
                    { 106L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 3L, "zhouba@company.com", "EMP20230006", new DateTime(2021, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2021, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, null, false, null, "13800000006", 7L, null, "周八", null, 1, null, null },
                    { 107L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 5L, "wujiu@company.com", "EMP20230007", new DateTime(2020, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2020, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, null, false, null, "13800000007", 4L, null, "吴九", null, 1, null, null },
                    { 108L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 4L, "zhengshi@company.com", "EMP20230008", new DateTime(2017, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2017, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, null, false, null, "13800000008", 3L, null, "郑十", null, 1, null, null },
                    { 109L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 2L, "chenxm@company.com", "EMP20230009", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, null, false, null, "13800000009", 8L, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "陈晓明", null, 0, null, null },
                    { 110L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 6L, "linxy@company.com", "EMP20230010", new DateTime(2023, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, null, false, null, "13800000010", 9L, null, "林小燕", null, 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "sys_role_menu",
                columns: new[] { "menu_id", "role_id" },
                values: new object[,]
                {
                    { 1L, 1L },
                    { 2L, 1L },
                    { 3L, 1L },
                    { 4L, 1L },
                    { 5L, 1L },
                    { 6L, 1L },
                    { 7L, 1L },
                    { 11L, 1L },
                    { 12L, 1L },
                    { 13L, 1L },
                    { 14L, 1L },
                    { 15L, 1L },
                    { 16L, 1L },
                    { 21L, 1L },
                    { 22L, 1L },
                    { 23L, 1L },
                    { 31L, 1L },
                    { 41L, 1L },
                    { 51L, 1L },
                    { 52L, 1L },
                    { 61L, 1L },
                    { 62L, 1L },
                    { 71L, 1L },
                    { 72L, 1L },
                    { 111L, 1L },
                    { 112L, 1L },
                    { 113L, 1L },
                    { 114L, 1L },
                    { 121L, 1L },
                    { 122L, 1L },
                    { 123L, 1L },
                    { 124L, 1L },
                    { 131L, 1L },
                    { 132L, 1L },
                    { 133L, 1L },
                    { 141L, 1L },
                    { 142L, 1L },
                    { 143L, 1L },
                    { 144L, 1L },
                    { 151L, 1L },
                    { 152L, 1L },
                    { 153L, 1L },
                    { 154L, 1L },
                    { 161L, 1L },
                    { 162L, 1L },
                    { 163L, 1L },
                    { 211L, 1L },
                    { 212L, 1L },
                    { 213L, 1L },
                    { 214L, 1L },
                    { 311L, 1L },
                    { 312L, 1L },
                    { 313L, 1L },
                    { 314L, 1L },
                    { 315L, 1L },
                    { 316L, 1L },
                    { 317L, 1L },
                    { 318L, 1L },
                    { 319L, 1L },
                    { 411L, 1L },
                    { 412L, 1L },
                    { 413L, 1L },
                    { 414L, 1L },
                    { 621L, 1L },
                    { 622L, 1L },
                    { 2L, 3L },
                    { 3L, 3L },
                    { 4L, 3L },
                    { 5L, 3L },
                    { 21L, 3L },
                    { 22L, 3L },
                    { 23L, 3L },
                    { 31L, 3L },
                    { 41L, 3L },
                    { 51L, 3L },
                    { 52L, 3L },
                    { 311L, 3L },
                    { 312L, 3L },
                    { 313L, 3L },
                    { 315L, 3L },
                    { 316L, 3L },
                    { 317L, 3L },
                    { 318L, 3L },
                    { 319L, 3L },
                    { 413L, 3L },
                    { 414L, 3L },
                    { 3L, 4L },
                    { 4L, 4L },
                    { 5L, 4L },
                    { 31L, 4L },
                    { 41L, 4L },
                    { 51L, 4L },
                    { 52L, 4L },
                    { 317L, 4L },
                    { 318L, 4L },
                    { 319L, 4L }
                });

            migrationBuilder.InsertData(
                table: "sys_user",
                columns: new[] { "Id", "avatar", "CreatedAt", "CreatedBy", "dept_id", "email", "employee_id", "IsDeleted", "last_login_time", "password_hash", "phone", "post_id", "real_name", "remark", "status", "UpdatedAt", "UpdatedBy", "username" },
                values: new object[,]
                {
                    { 1L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 1L, null, null, false, null, "$2a$12$PpTgqbP/m/GWzU3BljZ42.JgGBGR6sE320gb6y2FWhV1m6D/zkkJG", null, 1L, "超级管理员", null, 1, null, null, "admin" },
                    { 2L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 2L, null, 101L, false, null, "$2a$12$EQdEmMQiD6riOkccykHh1O2khOkACvToK.hnNbRTKr3Hl5fLKWU7.", null, 3L, "张三", null, 1, null, null, "zhangsan" },
                    { 3L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 2L, null, 102L, false, null, "$2a$12$mSjvmFXmY/heavNXG7fIFOpzoLa0B8aGi/sjKmLKQKsuIb07dxRQW", null, 4L, "李四", null, 1, null, null, "lisi" },
                    { 4L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 3L, null, 103L, false, null, "$2a$12$6WxEdj9C94zXs7z.zPNKleeHY834ZYekV7qNtUVwKtBdOMjyIUhzO", null, 3L, "王五", null, 1, null, null, "wangwu" },
                    { 5L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 4L, null, 104L, false, null, "$2a$12$5QwnI0LbP4L1Qdwf/EYXvuBY.r4mu56gd50qNnoEEBHc3/njGP3CO", null, 6L, "赵六", null, 1, null, null, "zhaoliu" },
                    { 6L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 2L, null, 105L, false, null, "$2a$12$oWwkqxoPblCI0QX/OhJoCeQAdc2ZjplbEFF4smHpCVGaBhwsOZ4Vu", null, 7L, "孙七", null, 1, null, null, "sunqi" }
                });

            migrationBuilder.InsertData(
                table: "budget_task",
                columns: new[] { "Id", "approved_amount", "biz_leader_id", "building_scale", "contractor_name", "CreatedAt", "CreatedBy", "dept_id", "fee_standard", "IsDeleted", "owner_contact", "owner_name", "owner_phone", "plan_date", "quota_basis", "reduction_rate", "remark", "status", "submit_amount", "task_name", "task_no", "task_type", "tech_leader_id", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 6001L, 6312m, 101L, "住宅12栋，建筑面积约8.6万㎡", "中建三局第二建设工程有限责任公司", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 2L, "四川省2015费用定额", false, "赵总", "成都某房产开发有限公司", "13900000002", new DateTime(2024, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "四川省2015建设工程工程量清单计价定额", 7.85m, "核减率7.85%，共出具评审意见42条", 4, 6850m, "某住宅小区（一期）工程结算审核", "YS-2024-001", 5, 102L, null, null },
                    { 6002L, null, 103L, "双向四车道，全长3.2km", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 3L, "四川省2020费用定额", false, "李科长", "成都市交通运输局", "028-87654321", new DateTime(2024, 10, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "四川省2020市政工程定额", null, null, 1, 4200m, "某市政道路工程预算编制", "YS-2024-002", 1, 107L, null, null }
                });

            migrationBuilder.InsertData(
                table: "hr_certificate",
                columns: new[] { "Id", "cert_name", "cert_no", "cert_type", "CreatedAt", "CreatedBy", "employee_id", "expire_date", "file_name", "file_path", "IsDeleted", "issue_date", "issue_org", "remark", "status", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 2001L, "注册城乡规划师", "2019ABCD0001", "注册规划师", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 101L, new DateTime(2025, 10, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, new DateTime(2019, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "住建部", null, 0, null, null },
                    { 2002L, "一级造价工程师", "2018ABCD0002", "造价工程师", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 102L, new DateTime(2026, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, new DateTime(2018, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "住建部", null, 0, null, null },
                    { 2003L, "注册城乡规划师", "2021ABCD0003", "注册规划师", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 103L, new DateTime(2025, 10, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, new DateTime(2021, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "住建部", null, 0, null, null },
                    { 2004L, "二级建造师", "2016ABCD0004", "建造师", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 104L, null, null, null, false, new DateTime(2016, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "四川省住建厅", null, 0, null, null },
                    { 2005L, "注册建筑师", "2020ABCD0005", "注册建筑师", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 104L, new DateTime(2026, 2, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, new DateTime(2020, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "住建部", null, 0, null, null },
                    { 2006L, "助理工程师职称", "2022ABCD0006", "职称证书", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 105L, null, null, null, false, new DateTime(2022, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "四川省人社厅", null, 0, null, null },
                    { 2007L, "一级造价工程师", "2019ABCD0007", "造价工程师", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 107L, new DateTime(2026, 2, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, new DateTime(2019, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "住建部", null, 0, null, null },
                    { 2008L, "注册城乡规划师", "2016ABCD0008", "注册规划师", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 108L, new DateTime(2026, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, new DateTime(2016, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "住建部", null, 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "hr_contract",
                columns: new[] { "Id", "contract_no", "contract_type", "CreatedAt", "CreatedBy", "employee_id", "end_date", "file_name", "file_path", "IsDeleted", "remark", "sign_date", "start_date", "status", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1001L, "HT2020-001", "固定期限", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 101L, new DateTime(2023, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, null, new DateTime(2020, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2020, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, null, null },
                    { 1002L, "HT2023-001", "固定期限", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 101L, new DateTime(2026, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, null, new DateTime(2023, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, null, null },
                    { 1003L, "HT2019-001", "固定期限", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 102L, new DateTime(2022, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, null, new DateTime(2019, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2019, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, null, null },
                    { 1004L, "HT2022-001", "无固定期限", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 102L, new DateTime(2099, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, null, new DateTime(2022, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, null, null },
                    { 1005L, "HT2021-001", "固定期限", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 103L, new DateTime(2024, 3, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, null, new DateTime(2021, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2021, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, null, null },
                    { 1006L, "HT2024-001", "固定期限", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 103L, new DateTime(2027, 3, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, null, new DateTime(2024, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, null, null },
                    { 1007L, "HT2018-001", "无固定期限", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 104L, new DateTime(2099, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, null, new DateTime(2018, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2018, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, null, null },
                    { 1008L, "HT2022-002", "固定期限", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 105L, new DateTime(2025, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, null, new DateTime(2022, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2022, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, null, null },
                    { 1009L, "HT2021-002", "固定期限", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 106L, new DateTime(2024, 11, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, null, new DateTime(2021, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2021, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, null, null },
                    { 1010L, "HT2024-002", "固定期限", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 106L, new DateTime(2027, 11, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, false, null, new DateTime(2024, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "proj_project",
                columns: new[] { "Id", "actual_end_date", "bid_deadline", "biz_leader_id", "biz_type", "building_scale", "contract_amount", "CreatedAt", "CreatedBy", "dept_id", "IsDeleted", "is_joint_venture", "limit_price", "our_ratio", "owner_contact", "owner_name", "owner_phone", "plan_end_date", "procurement_type", "progress_status", "proj_name", "proj_no", "remark", "sign_date", "status_updated_at", "tech_leader_id", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1001L, null, null, 103L, "控制性详细规划", null, 98m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 4L, false, false, null, null, "王处长", "成都市规划和自然资源局", "028-12345678", new DateTime(2024, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "竞争性磋商", 6, "成都市某片区控制性详细规划", "PRJ-2024-001", "重点项目，需配合规委会评审", new DateTime(2024, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 108L, null, null },
                    { 1002L, null, null, 101L, "结算评审", null, 45m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 2L, false, false, null, null, "张总", "成都市某国有投资公司", "13900000001", new DateTime(2024, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "询价", 7, "某住宅小区结算审核", "PRJ-2024-002", "送审金额约3200万元", new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 102L, null, null },
                    { 1003L, null, null, 107L, "可行性研究报告", null, 160m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 3L, false, true, null, 60m, "李科长", "成都市交通运输局", "028-87654321", new DateTime(2024, 10, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "公开招标", 6, "某市政道路工程可行性研究报告", "PRJ-2024-003", "联合体项目，牵头方，我方占比60%", new DateTime(2024, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 103L, null, null },
                    { 1004L, new DateTime(2024, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 104L, "战略咨询", null, 32m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 5L, false, false, null, null, "陈主任", "成都高新区管委会", "13800000099", new DateTime(2024, 3, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "单一来源", 8, "某工业园区概念规划设计", "PRJ-2023-018", "已完成，已全额回款", new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 107L, null, null },
                    { 1005L, null, null, 101L, "节能评估报告", null, 28m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 2L, false, false, null, null, "刘工", "成都天府新区建设局", "13700000001", new DateTime(2024, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "竞争性磋商", 3, "某新城片区节能评估报告", "PRJ-2024-004", "投标截止2024-06-10", new DateTime(2024, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 105L, null, null }
                });

            migrationBuilder.InsertData(
                table: "sys_user_role",
                columns: new[] { "role_id", "user_id" },
                values: new object[,]
                {
                    { 1L, 1L },
                    { 3L, 2L },
                    { 4L, 3L },
                    { 3L, 4L },
                    { 4L, 5L },
                    { 4L, 6L }
                });

            migrationBuilder.InsertData(
                table: "budget_section",
                columns: new[] { "Id", "approved_amount", "category", "contract_amount", "CreatedAt", "CreatedBy", "IsDeleted", "section_name", "section_no", "status", "submit_amount", "task_id", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 7001L, 2890m, "土建工程", 3200m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", false, "建筑工程", 1, 2, 3100m, 6001L, null, null },
                    { 7002L, 1650m, "安装工程", 1800m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", false, "安装工程", 2, 2, 1750m, 6001L, null, null },
                    { 7003L, 972m, "市政管道", 1100m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", false, "室外市政配套", 3, 2, 1000m, 6001L, null, null },
                    { 7004L, 800m, "绿化景观", 800m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", false, "绿化景观工程", 4, 2, 0m, 6001L, null, null }
                });

            migrationBuilder.InsertData(
                table: "proj_acceptance",
                columns: new[] { "Id", "accept_amount", "accept_batch", "accept_date", "CreatedAt", "CreatedBy", "invoice_no", "IsDeleted", "project_id", "remark", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 5001L, 9.6m, "第一批（预付款）", new DateTime(2023, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "INV2023100001", false, 1004L, "预付款30%", null, null },
                    { 5002L, 16m, "第二批（中期款）", new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "INV2024010001", false, 1004L, "完成中期成果50%", null, null },
                    { 5003L, 6.4m, "第三批（尾款）", new DateTime(2024, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "INV2024050001", false, 1004L, "成果交付完成", null, null }
                });

            migrationBuilder.InsertData(
                table: "proj_member",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "duty_desc", "employee_id", "IsDeleted", "join_date", "leave_date", "project_id", "ratio", "role", "status", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 3001L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "总体技术把控，规划方案设计", 108L, false, new DateTime(2024, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1001L, 40m, "项目负责人", 0, null, null },
                    { 3002L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "用地分析与指标测算", 104L, false, new DateTime(2024, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1001L, 30m, "参与人员", 0, null, null },
                    { 3003L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "现状调研与CAD制图", 106L, false, new DateTime(2024, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1001L, 20m, "参与人员", 0, null, null },
                    { 3004L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "合同对接、开票及回款跟进", 103L, false, new DateTime(2024, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1001L, 10m, "商务负责人", 0, null, null },
                    { 3005L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "结算审核技术负责", 102L, false, new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1002L, 50m, "项目负责人", 0, null, null },
                    { 3006L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "工程量核算", 105L, false, new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1002L, 30m, "参与人员", 0, null, null },
                    { 3007L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "商务对接", 101L, false, new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1002L, 20m, "商务负责人", 0, null, null },
                    { 3008L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "可研报告编制", 103L, false, new DateTime(2024, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1003L, 45m, "项目负责人", 0, null, null },
                    { 3009L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "联合体协调及商务", 107L, false, new DateTime(2024, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1003L, 35m, "商务负责人", 0, null, null },
                    { 3010L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "交通量调查与分析", 106L, false, new DateTime(2024, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1003L, 20m, "参与人员", 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "proj_milestone",
                columns: new[] { "Id", "accept_amount", "actual_date", "CreatedAt", "CreatedBy", "IsDeleted", "is_overdue", "milestone_name", "milestone_type", "owner_id", "plan_date", "project_id", "remark", "sort", "status", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 4001L, null, new DateTime(2024, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", false, false, "现状调研与基础资料收集", "现状调研", 108L, new DateTime(2024, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 1001L, null, 1, 2, null, null },
                    { 4002L, null, new DateTime(2024, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", false, true, "规划方案初稿", "方案设计", 108L, new DateTime(2024, 7, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 1001L, null, 2, 2, null, null },
                    { 4003L, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", false, false, "专家评审会", "专家评审", 108L, new DateTime(2024, 10, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 1001L, null, 3, 1, null, null },
                    { 4004L, 98m, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", false, false, "成果正式交付", "成果交付", 103L, new DateTime(2024, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 1001L, null, 4, 0, null, null },
                    { 4005L, null, new DateTime(2024, 5, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", false, false, "资料收集与初步核查", "资料收集", 102L, new DateTime(2024, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 1002L, null, 1, 2, null, null },
                    { 4006L, null, new DateTime(2024, 7, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", false, true, "结算审核报告初稿", "方案设计", 102L, new DateTime(2024, 7, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 1002L, null, 2, 2, null, null },
                    { 4007L, 45m, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", false, false, "审核报告正式提交", "成果交付", 101L, new DateTime(2024, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 1002L, null, 3, 1, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_budget_section_task_id",
                table: "budget_section",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_budget_task_biz_leader_id",
                table: "budget_task",
                column: "biz_leader_id");

            migrationBuilder.CreateIndex(
                name: "IX_budget_task_dept_id",
                table: "budget_task",
                column: "dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_budget_task_tech_leader_id",
                table: "budget_task",
                column: "tech_leader_id");

            migrationBuilder.CreateIndex(
                name: "IX_hr_certificate_employee_id",
                table: "hr_certificate",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_hr_contract_employee_id",
                table: "hr_contract",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_hr_employee_dept_id",
                table: "hr_employee",
                column: "dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_info_article_category_id",
                table: "info_article",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_kb_file_category_id",
                table: "kb_file",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_proj_acceptance_project_id",
                table: "proj_acceptance",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_proj_contract_project_id",
                table: "proj_contract",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_proj_file_project_id",
                table: "proj_file",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_proj_invoice_project_id",
                table: "proj_invoice",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_proj_member_employee_id",
                table: "proj_member",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_proj_member_project_id",
                table: "proj_member",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_proj_milestone_owner_id",
                table: "proj_milestone",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_proj_milestone_project_id",
                table: "proj_milestone",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_proj_oper_log_project_id",
                table: "proj_oper_log",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_proj_project_biz_leader_id",
                table: "proj_project",
                column: "biz_leader_id");

            migrationBuilder.CreateIndex(
                name: "IX_proj_project_dept_id",
                table: "proj_project",
                column: "dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_proj_project_tech_leader_id",
                table: "proj_project",
                column: "tech_leader_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_opinion_task_id",
                table: "review_opinion",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_sys_dict_data_SysDictTypeId",
                table: "sys_dict_data",
                column: "SysDictTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_sys_role_menu_menu_id",
                table: "sys_role_menu",
                column: "menu_id");

            migrationBuilder.CreateIndex(
                name: "IX_sys_user_dept_id",
                table: "sys_user",
                column: "dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_sys_user_employee_id",
                table: "sys_user",
                column: "employee_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sys_user_role_role_id",
                table: "sys_user_role",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "budget_section");

            migrationBuilder.DropTable(
                name: "hr_certificate");

            migrationBuilder.DropTable(
                name: "hr_contract");

            migrationBuilder.DropTable(
                name: "info_article");

            migrationBuilder.DropTable(
                name: "kb_file");

            migrationBuilder.DropTable(
                name: "proj_acceptance");

            migrationBuilder.DropTable(
                name: "proj_contract");

            migrationBuilder.DropTable(
                name: "proj_file");

            migrationBuilder.DropTable(
                name: "proj_invoice");

            migrationBuilder.DropTable(
                name: "proj_member");

            migrationBuilder.DropTable(
                name: "proj_milestone");

            migrationBuilder.DropTable(
                name: "proj_oper_log");

            migrationBuilder.DropTable(
                name: "review_opinion");

            migrationBuilder.DropTable(
                name: "sys_dict_data");

            migrationBuilder.DropTable(
                name: "sys_login_log");

            migrationBuilder.DropTable(
                name: "sys_oper_log");

            migrationBuilder.DropTable(
                name: "sys_post");

            migrationBuilder.DropTable(
                name: "sys_role_menu");

            migrationBuilder.DropTable(
                name: "sys_user_role");

            migrationBuilder.DropTable(
                name: "info_category");

            migrationBuilder.DropTable(
                name: "kb_category");

            migrationBuilder.DropTable(
                name: "proj_project");

            migrationBuilder.DropTable(
                name: "budget_task");

            migrationBuilder.DropTable(
                name: "sys_dict_type");

            migrationBuilder.DropTable(
                name: "sys_menu");

            migrationBuilder.DropTable(
                name: "sys_role");

            migrationBuilder.DropTable(
                name: "sys_user");

            migrationBuilder.DropTable(
                name: "hr_employee");

            migrationBuilder.DropTable(
                name: "sys_dept");
        }
    }
}
