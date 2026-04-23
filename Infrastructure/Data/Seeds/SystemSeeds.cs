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
            new SysUser { Id = 2, Username = "ningjinyuan", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", 12), RealName = "甯金元", DeptId = 2, PostId = 3, Status = 1, EmployeeId = 101, CreatedAt = dt, CreatedBy = "system" },
            new SysUser { Id = 3, Username = "caolijun", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", 12), RealName = "曹丽君", DeptId = 2, PostId = 4, Status = 1, EmployeeId = 102, CreatedAt = dt, CreatedBy = "system" },
            new SysUser { Id = 4, Username = "liurunze", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", 12), RealName = "刘润泽", DeptId = 3, PostId = 3, Status = 1, EmployeeId = 103, CreatedAt = dt, CreatedBy = "system" },
            new SysUser { Id = 5, Username = "wangshuaiwei", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", 12), RealName = "王帅伟", DeptId = 4, PostId = 6, Status = 1, EmployeeId = 104, CreatedAt = dt, CreatedBy = "system" },
            new SysUser { Id = 6, Username = "yangtong", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", 12), RealName = "杨通", DeptId = 2, PostId = 7, Status = 1, EmployeeId = 105, CreatedAt = dt, CreatedBy = "system" }
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

        // ── 字典类型 ──────────────────────────────────────────
        mb.Entity<SysDictType>().HasData(
            new SysDictType { Id = 1, DictName = "业务类型", DictType = "biz_type", Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 2, DictName = "采购方式", DictType = "procurement_type", Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 3, DictName = "合同类型", DictType = "contract_type", Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 4, DictName = "证书类型", DictType = "cert_type", Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 5, DictName = "里程碑类型", DictType = "milestone_type", Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 6, DictName = "概预算任务类型", DictType = "budget_task_type", Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 7, DictName = "项目进度状态", DictType = "proj_status", Status = 1, CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 字典数据 ──────────────────────────────────────────
        mb.Entity<SysDictData>().HasData(
            // 业务类型
            new SysDictData { Id = 101, DictType = "biz_type", DictLabel = "可行性研究报告", DictValue = "可行性研究报告", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 102, DictType = "biz_type", DictLabel = "节能评估报告", DictValue = "节能评估报告", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 103, DictType = "biz_type", DictLabel = "稳评报告", DictValue = "稳评报告", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 104, DictType = "biz_type", DictLabel = "概算编制", DictValue = "概算编制", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 105, DictType = "biz_type", DictLabel = "预算编制", DictValue = "预算编制", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 106, DictType = "biz_type", DictLabel = "结算编制", DictValue = "结算编制", Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 107, DictType = "biz_type", DictLabel = "概算评审", DictValue = "概算评审", Sort = 7, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 108, DictType = "biz_type", DictLabel = "预算评审", DictValue = "预算评审", Sort = 8, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 109, DictType = "biz_type", DictLabel = "结算评审", DictValue = "结算评审", Sort = 9, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 110, DictType = "biz_type", DictLabel = "控制性详细规划", DictValue = "控制性详细规划", Sort = 10, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 111, DictType = "biz_type", DictLabel = "专项规划", DictValue = "专项规划", Sort = 11, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 112, DictType = "biz_type", DictLabel = "城市更新规划", DictValue = "城市更新规划", Sort = 12, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 113, DictType = "biz_type", DictLabel = "施工图设计", DictValue = "施工图设计", Sort = 13, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 114, DictType = "biz_type", DictLabel = "战略咨询", DictValue = "战略咨询", Sort = 14, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 115, DictType = "biz_type", DictLabel = "施工阶段全过程管控", DictValue = "施工阶段全过程管控", Sort = 15, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 采购方式
            new SysDictData { Id = 201, DictType = "procurement_type", DictLabel = "竞争性磋商", DictValue = "竞争性磋商", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 202, DictType = "procurement_type", DictLabel = "询价", DictValue = "询价", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 203, DictType = "procurement_type", DictLabel = "公开招标", DictValue = "公开招标", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 204, DictType = "procurement_type", DictLabel = "邀请招标", DictValue = "邀请招标", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 205, DictType = "procurement_type", DictLabel = "公开招选", DictValue = "公开招选", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 206, DictType = "procurement_type", DictLabel = "框架协议采购", DictValue = "框架协议采购", Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 207, DictType = "procurement_type", DictLabel = "单一来源", DictValue = "单一来源", Sort = 7, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 合同类型
            new SysDictData { Id = 301, DictType = "contract_type", DictLabel = "固定期限", DictValue = "固定期限", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 302, DictType = "contract_type", DictLabel = "无固定期限", DictValue = "无固定期限", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 303, DictType = "contract_type", DictLabel = "劳务合同", DictValue = "劳务合同", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 304, DictType = "contract_type", DictLabel = "实习协议", DictValue = "实习协议", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 证书类型
            new SysDictData { Id = 401, DictType = "cert_type", DictLabel = "注册规划师", DictValue = "注册规划师", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 402, DictType = "cert_type", DictLabel = "造价工程师", DictValue = "造价工程师", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 403, DictType = "cert_type", DictLabel = "注册建筑师", DictValue = "注册建筑师", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 404, DictType = "cert_type", DictLabel = "注册工程师", DictValue = "注册工程师", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 405, DictType = "cert_type", DictLabel = "建造师", DictValue = "建造师", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 406, DictType = "cert_type", DictLabel = "职称证书", DictValue = "职称证书", Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 407, DictType = "cert_type", DictLabel = "岗位证书", DictValue = "岗位证书", Sort = 7, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 里程碑类型
            new SysDictData { Id = 501, DictType = "milestone_type", DictLabel = "资料收集", DictValue = "资料收集", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 502, DictType = "milestone_type", DictLabel = "现状调研", DictValue = "现状调研", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 503, DictType = "milestone_type", DictLabel = "方案设计", DictValue = "方案设计", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 504, DictType = "milestone_type", DictLabel = "内部评审", DictValue = "内部评审", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 505, DictType = "milestone_type", DictLabel = "专家评审", DictValue = "专家评审", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 506, DictType = "milestone_type", DictLabel = "报批上报", DictValue = "报批上报", Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 507, DictType = "milestone_type", DictLabel = "成果交付", DictValue = "成果交付", Sort = 7, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 508, DictType = "milestone_type", DictLabel = "回款", DictValue = "回款", Sort = 8, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 概预算任务类型
            new SysDictData { Id = 601, DictType = "budget_task_type", DictLabel = "概算编制", DictValue = "0", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 602, DictType = "budget_task_type", DictLabel = "预算编制", DictValue = "1", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 603, DictType = "budget_task_type", DictLabel = "结算编制", DictValue = "2", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 604, DictType = "budget_task_type", DictLabel = "概算评审", DictValue = "3", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 605, DictType = "budget_task_type", DictLabel = "预算评审", DictValue = "4", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 606, DictType = "budget_task_type", DictLabel = "结算评审", DictValue = "5", Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 项目进度状态
            new SysDictData { Id = 701, DictType = "proj_status", DictLabel = "前期商务", DictValue = "0", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 702, DictType = "proj_status", DictLabel = "预计启动", DictValue = "1", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 703, DictType = "proj_status", DictLabel = "标书制作中", DictValue = "2", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 704, DictType = "proj_status", DictLabel = "投标/磋商中", DictValue = "3", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 705, DictType = "proj_status", DictLabel = "已中标·签订合同中", DictValue = "4", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 706, DictType = "proj_status", DictLabel = "已签回合同", DictValue = "5", Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 707, DictType = "proj_status", DictLabel = "执行中", DictValue = "6", Sort = 7, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 708, DictType = "proj_status", DictLabel = "成果提交", DictValue = "7", Sort = 8, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 709, DictType = "proj_status", DictLabel = "已完成", DictValue = "8", Sort = 9, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 710, DictType = "proj_status", DictLabel = "已终止", DictValue = "9", Sort = 10, Status = 1, CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 超管拥有全部权限 ──────────────────────────────────
        var allMenuIds = new long[]
        {
            1,2,3,4,5,6,7,8,
            11,12,13,14,15,16,17,
            111,112,113,114,
            121,122,123,124,
            131,132,133,
            141,142,143,144,
            151,152,153,154,
            161,162,163,
            51,52,
            61,62,621,622,
            71,72,
            81,82,
            21,22,23,
            211,212,213,214,
            31, 311,312,313,314,315,316,317,318,319,320,
            41, 411,412,413,414,
        };
        mb.Entity<SysRoleMenu>().HasData(
            allMenuIds.Select(mid => new SysRoleMenu { RoleId = 1, MenuId = mid }).ToArray()
        );

        // 项目经理角色菜单（员工档案只读 + 项目管理全部）
        var pmMenuIds = new long[]
        {
            5, 51, 52,              // 个人中心（所有人可见）
            2, 21, 22, 23,          // 员工档案查看
            3, 31, 311,312,313,315,316,317,318,319,  // 项目管理全部
            4, 41, 413, 414,        // 概预算查看+录入意见
        };
        mb.Entity<SysRoleMenu>().HasData(
            pmMenuIds.Select(mid => new SysRoleMenu { RoleId = 3, MenuId = mid }).ToArray()
        );

        // 工程师角色菜单（项目查看 + 自己参与的操作）
        var engMenuIds = new long[]
        {
            5, 51, 52,              // 个人中心（所有人可见）
            3, 31, 317, 318, 319,   // 项目台账+完成节点+录入验收
            4, 41,                  // 概预算查看
        };
        mb.Entity<SysRoleMenu>().HasData(
            engMenuIds.Select(mid => new SysRoleMenu { RoleId = 4, MenuId = mid }).ToArray()
        );
    }
}
