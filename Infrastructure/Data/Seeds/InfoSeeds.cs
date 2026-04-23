using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Domain.Entities.System;
using EnterpriseMS.Domain.Entities.Info;

namespace EnterpriseMS.Infrastructure.Data.Seeds;

public static class InfoSeeds
{
    public static void Seed(ModelBuilder mb)
    {
        var dt = new DateTime(2026, 1, 1);

        // ── 知识库分类 ────────────────────────────────────────────
        mb.Entity<KbCategory>().HasData(
            new KbCategory { Id = 1, Name = "模板文件", Icon = "fa-file-word", Description = "常用工作模板，合同模板、报告模板等", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new KbCategory { Id = 2, Name = "公司通知", Icon = "fa-bullhorn", Description = "公司内部通知、公告", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new KbCategory { Id = 3, Name = "行业规范", Icon = "fa-book", Description = "工程咨询、规划、造价等行业标准规范", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new KbCategory { Id = 4, Name = "规章制度", Icon = "fa-gavel", Description = "公司规章制度、管理办法", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new KbCategory { Id = 5, Name = "培训资料", Icon = "fa-graduation-cap", Description = "内部培训讲义、学习材料", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new KbCategory { Id = 6, Name = "其他", Icon = "fa-folder-open", Description = "其他共享文件", Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system" }
        );

        // ── 菜单 ──────────────────────────────────────────────
        mb.Entity<SysMenu>().HasData(
            // ── 一级目录 ──────────────────────────────────────
            new SysMenu { Id = 1, MenuName = "系统管理", ParentId = 0, MenuType = "M", Icon = "fa-cogs", Path = "/system", Sort = 1, Visible = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 2, MenuName = "员工档案", ParentId = 0, MenuType = "M", Icon = "fa-users", Path = "/hr", Sort = 2, Visible = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 3, MenuName = "项目管理", ParentId = 0, MenuType = "M", Icon = "fa-project-diagram", Path = "/project", Sort = 3, Visible = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 4, MenuName = "概预算结算", ParentId = 0, MenuType = "M", Icon = "fa-file-invoice-dollar", Path = "/budget", Sort = 4, Visible = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 5, MenuName = "个人中心", ParentId = 0, MenuType = "M", Icon = "fa-user-circle", Path = "/profile", Sort = 5, Visible = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 6, MenuName = "知识库", ParentId = 0, MenuType = "M", Icon = "fa-database", Path = "/kb", Sort = 6, Visible = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 7, MenuName = "报表中心", ParentId = 0, MenuType = "M", Icon = "fa-chart-bar", Path = "/report", Sort = 7, Visible = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 8, MenuName = "造价小工具", ParentId = 0, MenuType = "M", Icon = "fa-calculator", Path = "/tool", Sort = 8, Visible = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // ── 系统管理子菜单 ────────────────────────────────
            new SysMenu { Id = 11, MenuName = "用户管理", ParentId = 1, MenuType = "C", Icon = "fa-user", Path = "/system/user", Sort = 1, Visible = 1, Status = 1, Perms = "sys:user:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 12, MenuName = "角色管理", ParentId = 1, MenuType = "C", Icon = "fa-user-tag", Path = "/system/role", Sort = 2, Visible = 1, Status = 1, Perms = "sys:role:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 13, MenuName = "部门管理", ParentId = 1, MenuType = "C", Icon = "fa-sitemap", Path = "/system/dept", Sort = 3, Visible = 1, Status = 1, Perms = "sys:dept:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 14, MenuName = "字典管理", ParentId = 1, MenuType = "C", Icon = "fa-book", Path = "/system/dict", Sort = 4, Visible = 1, Status = 1, Perms = "sys:dict:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 15, MenuName = "操作日志", ParentId = 1, MenuType = "C", Icon = "fa-history", Path = "/system/log", Sort = 5, Visible = 1, Status = 1, Perms = "sys:log:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 16, MenuName = "菜单管理", ParentId = 1, MenuType = "C", Icon = "fa-th-list", Path = "/system/menu", Sort = 6, Visible = 1, Status = 1, Perms = "sys:menu:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 17, MenuName = "Debug工具", ParentId = 1, MenuType = "C", Icon = "fa-bug", Path = "/system/debug", Sort = 9, Visible = 1, Status = 1, Perms = "sys:debug:index", CreatedAt = dt, CreatedBy = "system" },
            // 用户管理按钮
            new SysMenu { Id = 111, MenuName = "新增", ParentId = 11, MenuType = "F", Sort = 1, Visible = 0, Status = 1, Perms = "sys:user:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 112, MenuName = "编辑", ParentId = 11, MenuType = "F", Sort = 2, Visible = 0, Status = 1, Perms = "sys:user:edit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 113, MenuName = "删除", ParentId = 11, MenuType = "F", Sort = 3, Visible = 0, Status = 1, Perms = "sys:user:delete", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 114, MenuName = "重置密码", ParentId = 11, MenuType = "F", Sort = 4, Visible = 0, Status = 1, Perms = "sys:user:reset", CreatedAt = dt, CreatedBy = "system" },
            // 角色管理按钮
            new SysMenu { Id = 121, MenuName = "新增", ParentId = 12, MenuType = "F", Sort = 1, Visible = 0, Status = 1, Perms = "sys:role:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 122, MenuName = "编辑", ParentId = 12, MenuType = "F", Sort = 2, Visible = 0, Status = 1, Perms = "sys:role:edit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 123, MenuName = "删除", ParentId = 12, MenuType = "F", Sort = 3, Visible = 0, Status = 1, Perms = "sys:role:delete", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 124, MenuName = "分配权限", ParentId = 12, MenuType = "F", Sort = 4, Visible = 0, Status = 1, Perms = "sys:role:perm", CreatedAt = dt, CreatedBy = "system" },
            // 部门管理按钮
            new SysMenu { Id = 131, MenuName = "新增", ParentId = 13, MenuType = "F", Sort = 1, Visible = 0, Status = 1, Perms = "sys:dept:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 132, MenuName = "编辑", ParentId = 13, MenuType = "F", Sort = 2, Visible = 0, Status = 1, Perms = "sys:dept:edit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 133, MenuName = "删除", ParentId = 13, MenuType = "F", Sort = 3, Visible = 0, Status = 1, Perms = "sys:dept:delete", CreatedAt = dt, CreatedBy = "system" },
            // 字典管理按钮
            new SysMenu { Id = 141, MenuName = "新增", ParentId = 14, MenuType = "F", Sort = 1, Visible = 0, Status = 1, Perms = "sys:dict:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 142, MenuName = "编辑", ParentId = 14, MenuType = "F", Sort = 2, Visible = 0, Status = 1, Perms = "sys:dict:edit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 143, MenuName = "删除", ParentId = 14, MenuType = "F", Sort = 3, Visible = 0, Status = 1, Perms = "sys:dict:delete", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 144, MenuName = "分配权限", ParentId = 14, MenuType = "F", Sort = 4, Visible = 0, Status = 1, Perms = "sys:dict:perm", CreatedAt = dt, CreatedBy = "system" },
            // 操作日志管理按钮
            new SysMenu { Id = 151, MenuName = "新增", ParentId = 15, MenuType = "F", Sort = 1, Visible = 0, Status = 1, Perms = "sys:log:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 152, MenuName = "编辑", ParentId = 15, MenuType = "F", Sort = 2, Visible = 0, Status = 1, Perms = "sys:log:edit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 153, MenuName = "删除", ParentId = 15, MenuType = "F", Sort = 3, Visible = 0, Status = 1, Perms = "sys:log:delete", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 154, MenuName = "分配权限", ParentId = 15, MenuType = "F", Sort = 4, Visible = 0, Status = 1, Perms = "sys:log:perm", CreatedAt = dt, CreatedBy = "system" },
            // 菜单管理按钮
            new SysMenu { Id = 161, MenuName = "新增", ParentId = 16, MenuType = "F", Sort = 1, Visible = 0, Status = 1, Perms = "sys:menu:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 162, MenuName = "编辑", ParentId = 16, MenuType = "F", Sort = 2, Visible = 0, Status = 1, Perms = "sys:menu:edit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 163, MenuName = "删除", ParentId = 16, MenuType = "F", Sort = 3, Visible = 0, Status = 1, Perms = "sys:menu:delete", CreatedAt = dt, CreatedBy = "system" },
            // 知识库子菜单
            new SysMenu { Id = 61, MenuName = "文件浏览", ParentId = 6, MenuType = "C", Icon = "fa-folder-open", Path = "/kb", Sort = 1, Visible = 1, Status = 1, Perms = "kb:file:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 62, MenuName = "文件管理", ParentId = 6, MenuType = "C", Icon = "fa-cog", Path = "/kb/manage", Sort = 2, Visible = 1, Status = 1, Perms = "kb:file:manage", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 621, MenuName = "上传", ParentId = 62, MenuType = "F", Sort = 1, Visible = 0, Status = 1, Perms = "kb:file:upload", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 622, MenuName = "删除", ParentId = 62, MenuType = "F", Sort = 2, Visible = 0, Status = 1, Perms = "kb:file:delete", CreatedAt = dt, CreatedBy = "system" },
            // 报表中心子菜单
            new SysMenu { Id = 71, MenuName = "回款报表", ParentId = 7, MenuType = "C", Icon = "fa-hand-holding-usd", Path = "/report/receipt", Sort = 1, Visible = 1, Status = 1, Perms = "report:receipt", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 72, MenuName = "产值报表", ParentId = 7, MenuType = "C", Icon = "fa-user-chart", Path = "/report/output", Sort = 2, Visible = 1, Status = 1, Perms = "report:output", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 81, MenuName = "报告生成", ParentId = 8, MenuType = "C", Icon = "fa-file-word", Path = "/tool/report", Sort = 1, Visible = 1, Status = 1, Perms = null, CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 82, MenuName = "费用计算器", ParentId = 8, MenuType = "C", Icon = "fa-coins", Path = "/tool/calculator", Sort = 2, Visible = 1, Status = 1, Perms = null, CreatedAt = dt, CreatedBy = "system" },
            // 个人中心子菜单
            new SysMenu { Id = 51, MenuName = "个人资料", ParentId = 5, MenuType = "C", Icon = "fa-id-card", Path = "/profile", Sort = 1, Visible = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 52, MenuName = "产值统计", ParentId = 5, MenuType = "C", Icon = "fa-chart-bar", Path = "/my-stats", Sort = 2, Visible = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 员工档案
            new SysMenu { Id = 21, MenuName = "员工信息", ParentId = 2, MenuType = "C", Icon = "fa-id-card", Path = "/hr/employee", Sort = 1, Visible = 1, Status = 1, Perms = "hr:employee:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 22, MenuName = "合同管理", ParentId = 2, MenuType = "C", Icon = "fa-file-contract", Path = "/hr/contract", Sort = 2, Visible = 1, Status = 1, Perms = "hr:contract:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 23, MenuName = "证书管理", ParentId = 2, MenuType = "C", Icon = "fa-certificate", Path = "/hr/cert", Sort = 3, Visible = 1, Status = 1, Perms = "hr:cert:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 211, MenuName = "新增", ParentId = 21, MenuType = "F", Sort = 1, Visible = 0, Status = 1, Perms = "hr:employee:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 212, MenuName = "编辑", ParentId = 21, MenuType = "F", Sort = 2, Visible = 0, Status = 1, Perms = "hr:employee:edit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 213, MenuName = "转正", ParentId = 21, MenuType = "F", Sort = 3, Visible = 0, Status = 1, Perms = "hr:employee:formal", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 214, MenuName = "离职", ParentId = 21, MenuType = "F", Sort = 4, Visible = 0, Status = 1, Perms = "hr:employee:leave", CreatedAt = dt, CreatedBy = "system" },
            // 项目管理
            new SysMenu { Id = 31, MenuName = "项目台账", ParentId = 3, MenuType = "C", Icon = "fa-clipboard-list", Path = "/project", Sort = 1, Visible = 1, Status = 1, Perms = "proj:project:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 311, MenuName = "新建项目", ParentId = 31, MenuType = "F", Sort = 1, Visible = 0, Status = 1, Perms = "proj:project:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 312, MenuName = "编辑项目", ParentId = 31, MenuType = "F", Sort = 2, Visible = 0, Status = 1, Perms = "proj:project:edit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 313, MenuName = "变更状态", ParentId = 31, MenuType = "F", Sort = 3, Visible = 0, Status = 1, Perms = "proj:project:status", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 314, MenuName = "终止项目", ParentId = 31, MenuType = "F", Sort = 4, Visible = 0, Status = 1, Perms = "proj:project:terminate", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 315, MenuName = "添加成员", ParentId = 31, MenuType = "F", Sort = 5, Visible = 0, Status = 1, Perms = "proj:member:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 316, MenuName = "编辑成员", ParentId = 31, MenuType = "F", Sort = 6, Visible = 0, Status = 1, Perms = "proj:member:edit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 317, MenuName = "新增节点", ParentId = 31, MenuType = "F", Sort = 7, Visible = 0, Status = 1, Perms = "proj:milestone:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 318, MenuName = "完成节点", ParentId = 31, MenuType = "F", Sort = 8, Visible = 0, Status = 1, Perms = "proj:milestone:done", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 319, MenuName = "录入验收", ParentId = 31, MenuType = "F", Sort = 9, Visible = 0, Status = 1, Perms = "proj:acceptance:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 320, MenuName = "批量导入", ParentId = 31, MenuType = "F", Sort = 10, Visible = 0, Status = 1, Perms = "proj:project:import", CreatedAt = dt, CreatedBy = "system" },
            // 概预算结算
            new SysMenu { Id = 41, MenuName = "任务台账", ParentId = 4, MenuType = "C", Icon = "fa-calculator", Path = "/budget", Sort = 1, Visible = 1, Status = 1, Perms = "budget:task:list", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 411, MenuName = "新增任务", ParentId = 41, MenuType = "F", Sort = 1, Visible = 0, Status = 1, Perms = "budget:task:add", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 412, MenuName = "编辑任务", ParentId = 41, MenuType = "F", Sort = 2, Visible = 0, Status = 1, Perms = "budget:task:edit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 413, MenuName = "提交内审", ParentId = 41, MenuType = "F", Sort = 3, Visible = 0, Status = 1, Perms = "budget:task:submit", CreatedAt = dt, CreatedBy = "system" },
            new SysMenu { Id = 414, MenuName = "录入意见", ParentId = 41, MenuType = "F", Sort = 4, Visible = 0, Status = 1, Perms = "budget:opinion:add", CreatedAt = dt, CreatedBy = "system" }
        );
    }
}
