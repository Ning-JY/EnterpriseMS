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
    // 项目合同类型（主合同 / 补充合同 / 变更合同…），由项目台账-项目详情的合同类型下拉消费。
    // 与 HR 劳动合同的 ContractType 是两套互不相干的字典，不可合并：
    // contract_type 已被员工合同模块占用（固定期限 / 无固定期限 / 劳务合同 / 实习协议）。
    public const string ProjContractType = "proj_contract_type";

    /// <summary>
    /// 全部代码登记过的字典类型。字典管理中删除这些类型会破坏对应下拉/逻辑，
    /// 因此在 DictService 删除接口中受系统保护，禁止误删。
    /// </summary>
    public static readonly HashSet<string> All = new()
    {
        CertType, ContractType, MilestoneType, BizType, ProcurementType,
        ProjectStatus, Nationality, PoliticalStatus, Education,
        TechnicalTitle, TechnicalLevel, EmployeeStatus, ContractStatus, ProjNoPrefix,
        ProjContractType
    };
}
