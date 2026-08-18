using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Domain.Entities.System;

namespace EnterpriseMS.Infrastructure.Data.Seeds;

public static class SystemSeeds
{
    public static void Seed(ModelBuilder mb)
    {
        var dt = new DateTime(2026, 1, 1);

        // ── 部门 ──────────────────────────────────────────────
        mb.Entity<SysDept>().HasData(
            new SysDept { Id = 1, DeptName = "总公司", ParentId = 0, Ancestors = "0", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDept { Id = 2, DeptName = "工程咨询事业部", ParentId = 1, Ancestors = "0,1", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDept { Id = 3, DeptName = "交通和土地利用事业部", ParentId = 1, Ancestors = "0,1", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDept { Id = 4, DeptName = "城市设计事业部", ParentId = 1, Ancestors = "0,1", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDept { Id = 5, DeptName = "区域和产业经济事业部", ParentId = 1, Ancestors = "0,1", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDept { Id = 6, DeptName = "生产经营部", ParentId = 1, Ancestors = "0,1", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 岗位 ──────────────────────────────────────────────
        mb.Entity<SysPost>().HasData(
            new SysPost { Id = 1, PostName = "总经理", PostCode = "ceo", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysPost { Id = 2, PostName = "副总经理", PostCode = "vceo", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysPost { Id = 3, PostName = "项目负责人", PostCode = "pm", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysPost { Id = 4, PostName = "技术负责人", PostCode = "tech", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysPost { Id = 5, PostName = "商务负责人", PostCode = "business", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysPost { Id = 6, PostName = "高级工程师", PostCode = "senior", Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysPost { Id = 7, PostName = "工程师", PostCode = "engineer", Sort = 7, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysPost { Id = 8, PostName = "助理工程师", PostCode = "assist", Sort = 8, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysPost { Id = 9, PostName = "行政专员", PostCode = "admin", Sort = 9, Status = 1, CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 角色 ──────────────────────────────────────────────
        mb.Entity<SysRole>().HasData(
            new SysRole { Id = 1, RoleName = "超级管理员", RoleCode = "superadmin", DataScope = 1, Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysRole { Id = 2, RoleName = "管理员", RoleCode = "admin", DataScope = 1, Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysRole { Id = 3, RoleName = "项目经理", RoleCode = "pm", DataScope = 3, Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system", Remark = "可查看本部门及子部门全部项目" },
            new SysRole { Id = 4, RoleName = "工程师", RoleCode = "engineer", DataScope = 4, Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system", Remark = "只能查看本人参与的项目" },
            new SysRole { Id = 5, RoleName = "财务", RoleCode = "finance", DataScope = 2, Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysRole { Id = 6, RoleName = "只读", RoleCode = "readonly", DataScope = 1, Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system", Remark = "只有查看权限，无增删改" }
        );

        // ── 用户 ──────────────────────────────────────────────
        mb.Entity<SysUser>().HasData(
            new SysUser { Id = 1, Username = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", 12), RealName = "超级管理员", DeptId = 1, PostId = 1, Status = 1, EmployeeId = null, CreatedAt = dt, CreatedBy = "system" },
            new SysUser { Id = 2, Username = "ningjinyuan", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", 12), RealName = "甯金元", DeptId = 2, PostId = 3, Status = 1, EmployeeId = null, CreatedAt = dt, CreatedBy = "system" },
            new SysUser { Id = 3, Username = "caolijun", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", 12), RealName = "曹丽君", DeptId = 2, PostId = 4, Status = 1, EmployeeId = null, CreatedAt = dt, CreatedBy = "system" },
            new SysUser { Id = 4, Username = "liurunze", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", 12), RealName = "刘润泽", DeptId = 3, PostId = 3, Status = 1, EmployeeId = null, CreatedAt = dt, CreatedBy = "system" },
            new SysUser { Id = 5, Username = "wangshuaiwei", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", 12), RealName = "王帅伟", DeptId = 4, PostId = 6, Status = 1, EmployeeId = null, CreatedAt = dt, CreatedBy = "system" },
            new SysUser { Id = 6, Username = "yangtong", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", 12), RealName = "杨通", DeptId = 2, PostId = 7, Status = 1, EmployeeId = null, CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 用户角色 ──────────────────────────────────────────
        mb.Entity<SysUserRole>().HasData(
            new SysUserRole { UserId = 1, RoleId = 1 },
            new SysUserRole { UserId = 2, RoleId = 3 },
            new SysUserRole { UserId = 3, RoleId = 4 },
            new SysUserRole { UserId = 4, RoleId = 3 },
            new SysUserRole { UserId = 5, RoleId = 4 },
            new SysUserRole { UserId = 6, RoleId = 4 }
        );

        // ── 超管拥有全部权限（含资讯公告菜单 41/411-414）────────
        var allMenuIds = new long[]
        {
            1,2,3,5,6,7,8,
            11,12,13,14,15,16,17,
            111,112,113,114,
            121,122,123,124,
            131,132,133,
            141,142,143,144,
            151,152,153,154,
            161,162,163,
            18,
            51,52,
            61,62,621,622,
            71,72,
            81,82,83,
            21,22,23,
            211,212,213,214,
            31, 311,312,313,314,315,316,317,318,319,320,
            91, 911,912,913,914,915,916,
            41, 411, 412, 413, 414
        };
        mb.Entity<SysRoleMenu>().HasData(
            allMenuIds.Select(mid => new SysRoleMenu { RoleId = 1, MenuId = mid }).ToArray()
        );

        // 项目经理角色菜单（员工档案只读 + 项目管理全部 + 投标管理 + 资讯公告只读）
        var pmMenuIds = new long[]
        {
            5, 51, 52,              // 个人中心（所有人可见）
            2, 21, 22, 23,          // 员工档案查看
            3, 31, 311,312,313,315,316,317,318,319,  // 项目管理全部
            9, 91, 911,912,913,914,917,915,918,919,916,  // 投标管理全部
            8, 83,                  // 造价小工具：成果报告模板管理
            41, 411, 412,            // 资讯公告（查看）
        };
        mb.Entity<SysRoleMenu>().HasData(
            pmMenuIds.Select(mid => new SysRoleMenu { RoleId = 3, MenuId = mid }).ToArray()
        );

        // 工程师角色菜单（项目查看 + 自己参与的操作）
        var engMenuIds = new long[]
        {
            5, 51, 52,              // 个人中心（所有人可见）
            3, 31, 317, 318, 319,   // 项目台账+完成节点+录入验收
            41, 411,                // 资讯公告（查看）
        };
        mb.Entity<SysRoleMenu>().HasData(
            engMenuIds.Select(mid => new SysRoleMenu { RoleId = 4, MenuId = mid }).ToArray()
        );
    }
}
