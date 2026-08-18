using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnterpriseMS.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bid_template",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    category = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    industry = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_default = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_bid_template", x => x.Id);
                })
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
                name: "sys_config",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    config_key = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    config_value = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    config_type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    group_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sort = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_sys_config", x => x.Id);
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
                name: "sys_notification",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Link = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Level = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequiredPerm = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientId = table.Column<long>(type: "bigint", nullable: true),
                    SourceKey = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsRead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    table.PrimaryKey("PK_sys_notification", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sys_notification_read",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    NotificationId = table.Column<long>(type: "bigint", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
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
                    table.PrimaryKey("PK_sys_notification_read", x => x.Id);
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
                name: "TemplateDefinitions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContextSource = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateDefinitions", x => x.Id);
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
                    education = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    remark = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nationality = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    birth_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    political_status = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    native_place = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    highest_degree = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    graduate_school = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    major = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    work_start_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    technical_title = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    technical_level = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    emergency_contact = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    emergency_phone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bank_account = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bank_name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    social_insurance_no = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    profile_photo = table.Column<string>(type: "longtext", nullable: true)
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
                    table.ForeignKey(
                        name: "FK_hr_employee_sys_post_post_id",
                        column: x => x.post_id,
                        principalTable: "sys_post",
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
                name: "TemplateFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TemplateId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Required = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Source = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Binding = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConfigKey = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HelpText = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Sort = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateFields_TemplateDefinitions_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "TemplateDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "hr_attachment",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    employee_id = table.Column<long>(type: "bigint", nullable: false),
                    file_name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_path = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    file_type = table.Column<string>(type: "longtext", nullable: true)
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
                    table.PrimaryKey("PK_hr_attachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hr_attachment_hr_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "hr_employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "hr_education",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    employee_id = table.Column<long>(type: "bigint", nullable: false),
                    school_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    major = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    degree = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    start_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    end_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_full_time = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_hr_education", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hr_education_hr_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "hr_employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "hr_work_experience",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    employee_id = table.Column<long>(type: "bigint", nullable: false),
                    company_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    position = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    start_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    end_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    table.PrimaryKey("PK_hr_work_experience", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hr_work_experience_hr_employee_employee_id",
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
                    sign_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    plan_end_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    actual_end_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    actual_start_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    bid_deadline = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    progress_status = table.Column<int>(type: "int", nullable: false),
                    status_updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    building_scale = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cooperation_unit = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    project_category = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    project_leader_id = table.Column<long>(type: "bigint", nullable: true),
                    project_overview = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contract_signed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    contract_signed_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    contract_scan_file = table.Column<string>(type: "longtext", nullable: true)
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
                        name: "FK_proj_project_hr_employee_project_leader_id",
                        column: x => x.project_leader_id,
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
                name: "bid_project",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    project_id = table.Column<long>(type: "bigint", nullable: true),
                    project_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    project_code = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tenderer = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    budget = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    deadline = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    remark = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    parse_stage = table.Column<int>(type: "int", nullable: false),
                    format_rule_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_file_path = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source_file_name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    elements_confirmed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    elements_confirmed_by = table.Column<string>(type: "longtext", nullable: true)
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
                    table.PrimaryKey("PK_bid_project", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bid_project_proj_project_project_id",
                        column: x => x.project_id,
                        principalTable: "proj_project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    contract_id = table.Column<long>(type: "bigint", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "bid_document",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    bid_project_id = table.Column<long>(type: "bigint", nullable: false),
                    chapter_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    chapter_type = table.Column<int>(type: "int", nullable: false),
                    content = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    word_count = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_bid_document", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bid_document_bid_project_bid_project_id",
                        column: x => x.bid_project_id,
                        principalTable: "bid_project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bid_requirement",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    bid_project_id = table.Column<long>(type: "bigint", nullable: false),
                    category = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    score_weight = table.Column<int>(type: "int", nullable: true),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_veto = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    source_ref = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    needs_review = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_bid_requirement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bid_requirement_bid_project_bid_project_id",
                        column: x => x.bid_project_id,
                        principalTable: "bid_project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                    { 801L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "汉族", "nationality", "汉族", 0, false, 1, 1, null, null, null },
                    { 802L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "壮族", "nationality", "壮族", 0, false, 2, 1, null, null, null },
                    { 803L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "满族", "nationality", "满族", 0, false, 3, 1, null, null, null },
                    { 804L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "回族", "nationality", "回族", 0, false, 4, 1, null, null, null },
                    { 805L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "苗族", "nationality", "苗族", 0, false, 5, 1, null, null, null },
                    { 806L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "维吾尔族", "nationality", "维吾尔族", 0, false, 6, 1, null, null, null },
                    { 807L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "土家族", "nationality", "土家族", 0, false, 7, 1, null, null, null },
                    { 808L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "彝族", "nationality", "彝族", 0, false, 8, 1, null, null, null },
                    { 809L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "蒙古族", "nationality", "蒙古族", 0, false, 9, 1, null, null, null },
                    { 810L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "藏族", "nationality", "藏族", 0, false, 10, 1, null, null, null },
                    { 811L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "布依族", "nationality", "布依族", 0, false, 11, 1, null, null, null },
                    { 812L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "朝鲜族", "nationality", "朝鲜族", 0, false, 12, 1, null, null, null },
                    { 901L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "群众", "political_status", "群众", 0, false, 1, 1, null, null, null },
                    { 902L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "中共党员", "political_status", "中共党员", 0, false, 2, 1, null, null, null },
                    { 903L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "共青团员", "political_status", "共青团员", 0, false, 3, 1, null, null, null },
                    { 904L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "无党派人士", "political_status", "无党派人士", 0, false, 4, 1, null, null, null },
                    { 1001L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "高中", "education", "高中", 0, false, 1, 1, null, null, null },
                    { 1002L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "大专", "education", "大专", 0, false, 2, 1, null, null, null },
                    { 1003L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "本科", "education", "本科", 0, false, 3, 1, null, null, null },
                    { 1004L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "硕士", "education", "硕士", 0, false, 4, 1, null, null, null },
                    { 1005L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "博士", "education", "博士", 0, false, 5, 1, null, null, null },
                    { 1101L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "助理工程师", "technical_title", "助理工程师", 0, false, 1, 1, null, null, null },
                    { 1102L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "工程师", "technical_title", "工程师", 0, false, 2, 1, null, null, null },
                    { 1103L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "高级工程师", "technical_title", "高级工程师", 0, false, 3, 1, null, null, null },
                    { 1104L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "正高级工程师", "technical_title", "正高级工程师", 0, false, 4, 1, null, null, null },
                    { 1201L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "初级", "technical_level", "初级", 0, false, 1, 1, null, null, null },
                    { 1202L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "中级", "technical_level", "中级", 0, false, 2, 1, null, null, null },
                    { 1203L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "高级", "technical_level", "高级", 0, false, 3, 1, null, null, null },
                    { 1204L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "正高级", "technical_level", "正高级", 0, false, 4, 1, null, null, null },
                    { 1301L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "试用期", "employee_status", "0", 0, false, 1, 1, null, null, null },
                    { 1302L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "在职", "employee_status", "1", 0, false, 2, 1, null, null, null },
                    { 1303L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "离职", "employee_status", "2", 0, false, 3, 1, null, null, null },
                    { 1401L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "生效中", "contract_status", "0", 0, false, 1, 1, null, null, null },
                    { 1402L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "已终止", "contract_status", "1", 0, false, 2, 1, null, null, null },
                    { 1403L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "已到期", "contract_status", "2", 0, false, 3, 1, null, null, null },
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
                values: new object[,]
                {
                    { 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "业务类型", "biz_type", false, null, 1, null, null },
                    { 2L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "采购方式", "procurement_type", false, null, 1, null, null },
                    { 3L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "合同类型", "contract_type", false, null, 1, null, null },
                    { 4L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "证书类型", "cert_type", false, null, 1, null, null },
                    { 5L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "里程碑类型", "milestone_type", false, null, 1, null, null },
                    { 8L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "民族", "nationality", false, null, 1, null, null },
                    { 9L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "政治面貌", "political_status", false, null, 1, null, null },
                    { 10L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "学历", "education", false, null, 1, null, null },
                    { 11L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "技术职称", "technical_title", false, null, 1, null, null },
                    { 12L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "技术等级", "technical_level", false, null, 1, null, null },
                    { 13L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "员工状态", "employee_status", false, null, 1, null, null },
                    { 14L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "合同状态", "contract_status", false, null, 1, null, null },
                    { 15L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "项目编号前缀", "proj_no_prefix", false, null, 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "sys_menu",
                columns: new[] { "Id", "component", "CreatedAt", "CreatedBy", "icon", "IsDeleted", "menu_name", "menu_type", "parent_id", "path", "perms", "sort", "status", "UpdatedAt", "UpdatedBy", "visible" },
                values: new object[,]
                {
                    { 1L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-cogs", false, "系统管理", "M", 0L, "/system", null, 99, 1, null, null, 1 },
                    { 2L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-users", false, "员工档案", "M", 0L, "/hr", null, 2, 1, null, null, 1 },
                    { 3L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-project-diagram", false, "项目管理", "M", 0L, "/project", null, 3, 1, null, null, 1 },
                    { 5L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-user-circle", false, "个人中心", "M", 0L, "/profile", null, 5, 1, null, null, 1 },
                    { 6L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-database", false, "知识库", "M", 0L, "/kb", null, 6, 1, null, null, 1 },
                    { 7L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-chart-bar", false, "报表中心", "M", 0L, "/report", null, 7, 1, null, null, 1 },
                    { 8L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-calculator", false, "造价小工具", "M", 0L, "/tool", null, 8, 1, null, null, 1 },
                    { 9L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-file-signature", false, "投标管理", "M", 0L, "/bid", null, 9, 1, null, null, 1 },
                    { 11L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-user", false, "用户管理", "C", 1L, "/system/user", "sys:user:list", 1, 1, null, null, 1 },
                    { 12L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-user-tag", false, "角色管理", "C", 1L, "/system/role", "sys:role:list", 2, 1, null, null, 1 },
                    { 13L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-sitemap", false, "部门管理", "C", 1L, "/system/dept", "sys:dept:list", 3, 1, null, null, 1 },
                    { 14L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-book", false, "字典管理", "C", 1L, "/system/dict", "sys:dict:list", 4, 1, null, null, 1 },
                    { 15L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-history", false, "操作日志", "C", 1L, "/system/log", "sys:log:list", 5, 1, null, null, 1 },
                    { 16L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-th-list", false, "菜单管理", "C", 1L, "/system/menu", "sys:menu:list", 6, 1, null, null, 1 },
                    { 17L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-bug", false, "Debug工具", "C", 1L, "/system/debug", "sys:debug:index", 9, 1, null, null, 1 },
                    { 18L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-sliders-h", false, "系统参数设置", "C", 1L, "/system/config", "sys:config:list", 10, 1, null, null, 1 },
                    { 21L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-id-card", false, "员工信息", "C", 2L, "/hr/employee", "hr:employee:list", 1, 1, null, null, 1 },
                    { 22L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-file-contract", false, "合同管理", "C", 2L, "/hr/contract", "hr:contract:list", 2, 1, null, null, 1 },
                    { 23L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-certificate", false, "证书管理", "C", 2L, "/hr/cert", "hr:cert:list", 3, 1, null, null, 1 },
                    { 31L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-clipboard-list", false, "项目台账", "C", 3L, "/project", "proj:project:list", 1, 1, null, null, 1 },
                    { 32L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-hand-holding-usd", false, "回款管理", "C", 3L, "/project/receipt", "proj:project:list", 2, 1, null, null, 1 },
                    { 41L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-bullhorn", false, "资讯公告", "M", 0L, "/info/manage", null, 4, 1, null, null, 1 },
                    { 51L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-id-card", false, "个人资料", "C", 5L, "/profile", null, 1, 1, null, null, 1 },
                    { 52L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-chart-bar", false, "产值统计", "C", 5L, "/my-stats", null, 2, 1, null, null, 1 },
                    { 61L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-folder-open", false, "文件浏览", "C", 6L, "/kb", "kb:file:list", 1, 1, null, null, 1 },
                    { 62L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-cog", false, "文件管理", "C", 6L, "/kb/manage", "kb:file:manage", 2, 1, null, null, 1 },
                    { 71L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-hand-holding-usd", false, "回款报表", "C", 7L, "/report/receipt", "report:receipt", 1, 1, null, null, 1 },
                    { 72L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-user-chart", false, "产值报表", "C", 7L, "/report/output", "report:output", 2, 1, null, null, 1 },
                    { 81L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-file-word", false, "报告生成", "C", 8L, "/tool/report", null, 1, 1, null, null, 1 },
                    { 82L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-coins", false, "费用计算器", "C", 8L, "/tool/calculator", null, 2, 1, null, null, 1 },
                    { 83L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-layer-group", false, "成果报告模板", "C", 8L, "/templatereport/manage", null, 3, 1, null, null, 1 },
                    { 91L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-clipboard-list", false, "投标台账", "C", 9L, "/bid", "bid:project:list", 1, 1, null, null, 1 },
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
                    { 320L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "批量导入", "F", 31L, null, "proj:project:import", 10, 1, null, null, 0 },
                    { 321L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "新增回款", "F", 32L, null, "proj:project:list", 1, 1, null, null, 0 },
                    { 322L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "编辑回款", "F", 32L, null, "proj:project:list", 2, 1, null, null, 0 },
                    { 323L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "删除回款", "F", 32L, null, "proj:project:list", 3, 1, null, null, 0 },
                    { 324L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "确认收款", "F", 32L, null, "proj:project:list", 4, 1, null, null, 0 },
                    { 411L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-bullhorn", false, "公告管理", "C", 41L, "/info/manage", "info:article:list", 1, 1, null, null, 1 },
                    { 412L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", "fa-tags", false, "分类管理", "C", 41L, "/info/category", "info:category:list", 2, 1, null, null, 1 },
                    { 413L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "新增", "F", 411L, null, "info:article:add", 1, 1, null, null, 0 },
                    { 414L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "删除", "F", 411L, null, "info:article:delete", 2, 1, null, null, 0 },
                    { 621L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "上传", "F", 62L, null, "kb:file:upload", 1, 1, null, null, 0 },
                    { 622L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "删除", "F", 62L, null, "kb:file:delete", 2, 1, null, null, 0 },
                    { 911L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "新建投标", "F", 91L, null, "bid:project:add", 1, 1, null, null, 0 },
                    { 912L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "编辑投标", "F", 91L, null, "bid:project:edit", 2, 1, null, null, 0 },
                    { 913L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "删除投标", "F", 91L, null, "bid:project:delete", 3, 1, null, null, 0 },
                    { 914L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "AI解析", "F", 91L, null, "bid:project:analyze", 4, 1, null, null, 0 },
                    { 915L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "AI生成", "F", 91L, null, "bid:project:generate", 6, 1, null, null, 0 },
                    { 916L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "AI审查", "F", 91L, null, "bid:project:review", 9, 1, null, null, 0 },
                    { 917L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "确认招标要素", "F", 91L, null, "bid:project:confirm", 5, 1, null, null, 0 },
                    { 918L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "人员匹配", "F", 91L, null, "bid:project:match", 7, 1, null, null, 0 },
                    { 919L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", null, false, "导出文件", "F", 91L, null, "bid:project:export", 8, 1, null, null, 0 }
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
                table: "sys_role_menu",
                columns: new[] { "menu_id", "role_id" },
                values: new object[,]
                {
                    { 1L, 1L },
                    { 2L, 1L },
                    { 3L, 1L },
                    { 5L, 1L },
                    { 6L, 1L },
                    { 7L, 1L },
                    { 8L, 1L },
                    { 11L, 1L },
                    { 12L, 1L },
                    { 13L, 1L },
                    { 14L, 1L },
                    { 15L, 1L },
                    { 16L, 1L },
                    { 17L, 1L },
                    { 18L, 1L },
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
                    { 81L, 1L },
                    { 82L, 1L },
                    { 83L, 1L },
                    { 91L, 1L },
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
                    { 320L, 1L },
                    { 411L, 1L },
                    { 412L, 1L },
                    { 413L, 1L },
                    { 414L, 1L },
                    { 621L, 1L },
                    { 622L, 1L },
                    { 911L, 1L },
                    { 912L, 1L },
                    { 913L, 1L },
                    { 914L, 1L },
                    { 915L, 1L },
                    { 916L, 1L },
                    { 2L, 3L },
                    { 3L, 3L },
                    { 5L, 3L },
                    { 8L, 3L },
                    { 9L, 3L },
                    { 21L, 3L },
                    { 22L, 3L },
                    { 23L, 3L },
                    { 31L, 3L },
                    { 41L, 3L },
                    { 51L, 3L },
                    { 52L, 3L },
                    { 83L, 3L },
                    { 91L, 3L },
                    { 311L, 3L },
                    { 312L, 3L },
                    { 313L, 3L },
                    { 315L, 3L },
                    { 316L, 3L },
                    { 317L, 3L },
                    { 318L, 3L },
                    { 319L, 3L },
                    { 411L, 3L },
                    { 412L, 3L },
                    { 911L, 3L },
                    { 912L, 3L },
                    { 913L, 3L },
                    { 914L, 3L },
                    { 915L, 3L },
                    { 916L, 3L },
                    { 917L, 3L },
                    { 918L, 3L },
                    { 919L, 3L },
                    { 3L, 4L },
                    { 5L, 4L },
                    { 31L, 4L },
                    { 41L, 4L },
                    { 51L, 4L },
                    { 52L, 4L },
                    { 317L, 4L },
                    { 318L, 4L },
                    { 319L, 4L },
                    { 411L, 4L }
                });

            migrationBuilder.InsertData(
                table: "sys_user",
                columns: new[] { "Id", "avatar", "CreatedAt", "CreatedBy", "dept_id", "email", "employee_id", "IsDeleted", "last_login_time", "password_hash", "phone", "post_id", "real_name", "remark", "status", "UpdatedAt", "UpdatedBy", "username" },
                values: new object[,]
                {
                    { 1L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 1L, null, null, false, null, "$2a$12$xy1BRL3w/E0ZAYePiCTyRObwxHMEaFBFXcwcYydBukWtPVZfRAAri", null, 1L, "超级管理员", null, 1, null, null, "admin" },
                    { 2L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 2L, null, null, false, null, "$2a$12$scGded5ZuUqRtk24ccG0ZevPFYEYjowUdoDzHvz2IUkROj2ew1q/S", null, 3L, "甯金元", null, 1, null, null, "ningjinyuan" },
                    { 3L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 2L, null, null, false, null, "$2a$12$NANYIo5swCcPieQ7RKZ7LOBut58zIMRHdHYnp/v.JDRKpGXFNIlEq", null, 4L, "曹丽君", null, 1, null, null, "caolijun" },
                    { 4L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 3L, null, null, false, null, "$2a$12$omAZhnmnnObYna/YV8rTvOFGibSkpYVt0JM6hzVlPO495ME9U6rw.", null, 3L, "刘润泽", null, 1, null, null, "liurunze" },
                    { 5L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 4L, null, null, false, null, "$2a$12$AmqGSBd/6c.6arjhcrd6gelrUsy.iGFwX.Ht0S3f3tfru86t16oFS", null, 6L, "王帅伟", null, 1, null, null, "wangshuaiwei" },
                    { 6L, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "system", 2L, null, null, false, null, "$2a$12$TCsm6lrtEpk06VxI8/R5f.W1b6IQB/TyGv3WneFPJiIHPHaDlLx3W", null, 7L, "杨通", null, 1, null, null, "yangtong" }
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

            migrationBuilder.CreateIndex(
                name: "IX_bid_document_bid_project_id",
                table: "bid_document",
                column: "bid_project_id");

            migrationBuilder.CreateIndex(
                name: "IX_bid_project_project_id",
                table: "bid_project",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_bid_requirement_bid_project_id",
                table: "bid_requirement",
                column: "bid_project_id");

            migrationBuilder.CreateIndex(
                name: "IX_hr_attachment_employee_id",
                table: "hr_attachment",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_hr_certificate_employee_id",
                table: "hr_certificate",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_hr_contract_employee_id",
                table: "hr_contract",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_hr_education_employee_id",
                table: "hr_education",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_hr_employee_dept_id",
                table: "hr_employee",
                column: "dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_hr_employee_post_id",
                table: "hr_employee",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_hr_work_experience_employee_id",
                table: "hr_work_experience",
                column: "employee_id");

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
                name: "IX_proj_project_dept_id",
                table: "proj_project",
                column: "dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_proj_project_project_leader_id",
                table: "proj_project",
                column: "project_leader_id");

            migrationBuilder.CreateIndex(
                name: "IX_sys_dict_data_SysDictTypeId",
                table: "sys_dict_data",
                column: "SysDictTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_sys_notification_read_UserId_NotificationId",
                table: "sys_notification_read",
                columns: new[] { "UserId", "NotificationId" },
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_TemplateFields_TemplateId",
                table: "TemplateFields",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bid_document");

            migrationBuilder.DropTable(
                name: "bid_requirement");

            migrationBuilder.DropTable(
                name: "bid_template");

            migrationBuilder.DropTable(
                name: "hr_attachment");

            migrationBuilder.DropTable(
                name: "hr_certificate");

            migrationBuilder.DropTable(
                name: "hr_contract");

            migrationBuilder.DropTable(
                name: "hr_education");

            migrationBuilder.DropTable(
                name: "hr_work_experience");

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
                name: "sys_config");

            migrationBuilder.DropTable(
                name: "sys_dict_data");

            migrationBuilder.DropTable(
                name: "sys_login_log");

            migrationBuilder.DropTable(
                name: "sys_notification");

            migrationBuilder.DropTable(
                name: "sys_notification_read");

            migrationBuilder.DropTable(
                name: "sys_oper_log");

            migrationBuilder.DropTable(
                name: "sys_role_menu");

            migrationBuilder.DropTable(
                name: "sys_user_role");

            migrationBuilder.DropTable(
                name: "TemplateFields");

            migrationBuilder.DropTable(
                name: "bid_project");

            migrationBuilder.DropTable(
                name: "info_category");

            migrationBuilder.DropTable(
                name: "kb_category");

            migrationBuilder.DropTable(
                name: "sys_dict_type");

            migrationBuilder.DropTable(
                name: "sys_menu");

            migrationBuilder.DropTable(
                name: "sys_role");

            migrationBuilder.DropTable(
                name: "sys_user");

            migrationBuilder.DropTable(
                name: "TemplateDefinitions");

            migrationBuilder.DropTable(
                name: "proj_project");

            migrationBuilder.DropTable(
                name: "hr_employee");

            migrationBuilder.DropTable(
                name: "sys_dept");

            migrationBuilder.DropTable(
                name: "sys_post");
        }
    }
}
