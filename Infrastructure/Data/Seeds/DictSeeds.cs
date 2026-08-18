using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Domain.Entities.System;

namespace EnterpriseMS.Infrastructure.Data.Seeds;

/// <summary>
/// 字典种子（独立文件）。
/// 从原 SystemSeeds 中拆出，便于单独维护字典类型与字典数据。
/// 新增字典项后运行 Debug → “写入菜单/字典” 即可幂等入库（无需重新迁移）。
/// </summary>
public static class DictSeeds
{
    public static void Seed(ModelBuilder mb)
    {
        var dt = new DateTime(2026, 1, 1);

        // ── 字典类型 ──────────────────────────────────────────
        mb.Entity<SysDictType>().HasData(
            new SysDictType { Id = 1,  DictName = "业务类型",         DictType = "biz_type",         Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 2,  DictName = "采购方式",         DictType = "procurement_type", Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 3,  DictName = "合同类型",         DictType = "contract_type",    Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 4,  DictName = "证书类型",         DictType = "cert_type",        Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 5,  DictName = "里程碑类型",       DictType = "milestone_type",   Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 8,  DictName = "民族",             DictType = "nationality",      Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 9,  DictName = "政治面貌",         DictType = "political_status", Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 10, DictName = "学历",             DictType = "education",        Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 11, DictName = "技术职称",         DictType = "technical_title",  Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 12, DictName = "技术等级",         DictType = "technical_level",  Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 以下两类原本在字典中做了枚举镜像，但代码仍以枚举为准、从不读字典，属双数据源陷阱，已移除。
            // 项目进度(proj_status) 以 ProjectStatus 枚举为唯一真源；概预算模块已整体移除。
            // 状态类改为字典驱动：员工状态 / 合同状态（见下）
            new SysDictType { Id = 13, DictName = "员工状态",         DictType = "employee_status",  Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 14, DictName = "合同状态",         DictType = "contract_status",  Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictType { Id = 15, DictName = "项目编号前缀",     DictType = "proj_no_prefix",   Status = 1, CreatedAt = dt, CreatedBy = "system" }
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
            // 员工状态（字典驱动：管理员可在字典管理中增删选项）
            // DictValue 与 EmployeeStatus 枚举 int 保持一致，作为逻辑 key
            new SysDictData { Id = 1301, DictType = "employee_status", DictLabel = "试用期", DictValue = "0", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 1302, DictType = "employee_status", DictLabel = "在职",   DictValue = "1", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 1303, DictType = "employee_status", DictLabel = "离职",   DictValue = "2", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 合同状态（字典驱动）
            new SysDictData { Id = 1401, DictType = "contract_status", DictLabel = "生效中", DictValue = "0", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 1402, DictType = "contract_status", DictLabel = "已终止", DictValue = "1", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 1403, DictType = "contract_status", DictLabel = "已到期", DictValue = "2", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 民族
            new SysDictData { Id = 801, DictType = "nationality", DictLabel = "汉族", DictValue = "汉族", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 802, DictType = "nationality", DictLabel = "壮族", DictValue = "壮族", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 803, DictType = "nationality", DictLabel = "满族", DictValue = "满族", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 804, DictType = "nationality", DictLabel = "回族", DictValue = "回族", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 805, DictType = "nationality", DictLabel = "苗族", DictValue = "苗族", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 806, DictType = "nationality", DictLabel = "维吾尔族", DictValue = "维吾尔族", Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 807, DictType = "nationality", DictLabel = "土家族", DictValue = "土家族", Sort = 7, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 808, DictType = "nationality", DictLabel = "彝族", DictValue = "彝族", Sort = 8, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 809, DictType = "nationality", DictLabel = "蒙古族", DictValue = "蒙古族", Sort = 9, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 810, DictType = "nationality", DictLabel = "藏族", DictValue = "藏族", Sort = 10, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 811, DictType = "nationality", DictLabel = "布依族", DictValue = "布依族", Sort = 11, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 812, DictType = "nationality", DictLabel = "朝鲜族", DictValue = "朝鲜族", Sort = 12, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 政治面貌
            new SysDictData { Id = 901, DictType = "political_status", DictLabel = "群众", DictValue = "群众", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 902, DictType = "political_status", DictLabel = "中共党员", DictValue = "中共党员", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 903, DictType = "political_status", DictLabel = "共青团员", DictValue = "共青团员", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 904, DictType = "political_status", DictLabel = "无党派人士", DictValue = "无党派人士", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 学历
            new SysDictData { Id = 1001, DictType = "education", DictLabel = "高中", DictValue = "高中", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 1002, DictType = "education", DictLabel = "大专", DictValue = "大专", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 1003, DictType = "education", DictLabel = "本科", DictValue = "本科", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 1004, DictType = "education", DictLabel = "硕士", DictValue = "硕士", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 1005, DictType = "education", DictLabel = "博士", DictValue = "博士", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 技术职称
            new SysDictData { Id = 1101, DictType = "technical_title", DictLabel = "助理工程师", DictValue = "助理工程师", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 1102, DictType = "technical_title", DictLabel = "工程师", DictValue = "工程师", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 1103, DictType = "technical_title", DictLabel = "高级工程师", DictValue = "高级工程师", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 1104, DictType = "technical_title", DictLabel = "正高级工程师", DictValue = "正高级工程师", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 技术等级
            new SysDictData { Id = 1201, DictType = "technical_level", DictLabel = "初级", DictValue = "初级", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 1202, DictType = "technical_level", DictLabel = "中级", DictValue = "中级", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 1203, DictType = "technical_level", DictLabel = "高级", DictValue = "高级", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 1204, DictType = "technical_level", DictLabel = "正高级", DictValue = "正高级", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            // 项目编号前缀（字典驱动：管理员可在字典管理中动态增删前缀，满足造价/设计等不同前缀需求）
            new SysDictData { Id = 1501, DictType = "proj_no_prefix", DictLabel = "造价",   DictValue = "造价",   Sort = 1, IsDefault = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 1502, DictType = "proj_no_prefix", DictLabel = "设计",   DictValue = "设计",   Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 1503, DictType = "proj_no_prefix", DictLabel = "勘察",   DictValue = "勘察",   Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 1504, DictType = "proj_no_prefix", DictLabel = "监理",   DictValue = "监理",   Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 1505, DictType = "proj_no_prefix", DictLabel = "咨询",   DictValue = "咨询",   Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 1506, DictType = "proj_no_prefix", DictLabel = "全过程", DictValue = "全过程", Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new SysDictData { Id = 1507, DictType = "proj_no_prefix", DictLabel = "其他",   DictValue = "其他",   Sort = 7, Status = 1, CreatedAt = dt, CreatedBy = "system" }
        );
    }
}
