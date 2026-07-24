namespace EnterpriseMS.Domain.Constants;

/// <summary>
/// 字典类型常量（审计 4.4：消除 "cert_type" / "contract_type" 等魔法字符串）。
/// 新增字典类型时在此处登记，调用方统一引用，避免拼写错误与散落硬编码。
/// </summary>
public static class DictType
{
    public const string CertType      = "cert_type";
    public const string ContractType  = "contract_type";
    public const string MilestoneType = "milestone_type";
}
