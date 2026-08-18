namespace EnterpriseMS.Services.DTOs.Report;

/// <summary>
/// 报告模板「字段数据来源」的单一真相源。
/// 前端配置向导用它生成「绑定项目字段 / 系统配置」的可选项；
/// 后端 ProjectService 解析占位符时也用它（避免两处维护导致漂移）。
/// </summary>
public static class ReportFieldSources
{
    /// <summary>可绑定到 ProjectDetailDto 的属性名 → 中文标签。</summary>
    public static readonly IReadOnlyList<ReportFieldSourceItem> ProjectFields = new List<ReportFieldSourceItem>
    {
        new("ProjName", "工程名称"),
        new("OwnerName", "建设单位"),
        new("ProjNo", "项目编号"),
        new("BuildingScale", "建设规模"),
        new("LimitPrice", "限价(万元)"),
        new("ContractAmount", "合同金额(万元)"),
    };
}

/// <summary>字段来源可选项（key=绑定键 / ConfigKey，label=展示文案）。</summary>
public record ReportFieldSourceItem(string Key, string Label);
