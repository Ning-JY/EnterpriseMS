using System.Collections.Generic;

namespace EnterpriseMS.Domain.Constants;

/// <summary>
/// 字典类型常量（审计 4.4：消除 "cert_type" / "contract_type" 等魔法字符串）。
/// 新增字典类型时在此处登记，调用方统一引用，避免拼写错误与散落硬编码。
/// </summary>
public static class DictType
{
    // ── 已登记类型 ──────────────────────────────────────────
    public const string CertType         = "cert_type";
    public const string ContractType     = "contract_type";
    public const string MilestoneType    = "milestone_type";
    public const string BizType          = "biz_type";
    public const string ProcurementType  = "procurement_type";
    public const string ProjectStatus    = "proj_status";
    public const string Nationality      = "nationality";
    public const string PoliticalStatus  = "political_status";
    public const string Education        = "education";
    public const string TechnicalTitle   = "technical_title";
    public const string TechnicalLevel   = "technical_level";
    public const string EmployeeStatus   = "employee_status";
    public const string ContractStatus   = "contract_status";
    public const string ProjNoPrefix     = "proj_no_prefix";

    /// <summary>
    /// 全部代码登记过的字典类型。字典管理中删除这些类型会破坏对应下拉/逻辑，
    /// 因此在 DictService 删除接口中受系统保护，禁止误删。
    /// </summary>
    public static readonly HashSet<string> All = new()
    {
        CertType, ContractType, MilestoneType, BizType, ProcurementType,
        ProjectStatus, Nationality, PoliticalStatus, Education,
        TechnicalTitle, TechnicalLevel, EmployeeStatus, ContractStatus, ProjNoPrefix
    };
}
