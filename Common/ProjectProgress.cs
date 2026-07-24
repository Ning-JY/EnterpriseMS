namespace EnterpriseMS.Common;

/// <summary>
/// 项目进度状态文本（审计 3.2：原 ProjectService 与 ReportController 各有一份逐字重复的 GetProgressText）。
/// 统一在此维护，避免两处状态文案不同步。
/// </summary>
public static class ProjectProgress
{
    public static string GetProgressText(int status) => status switch
    {
        0 => "前期商务",
        1 => "预计启动",
        2 => "标书制作中",
        3 => "投标/磋商中",
        4 => "已中标·签订合同中",
        5 => "已签回合同",
        6 => "执行中",
        7 => "成果提交",
        8 => "已完成",
        9 => "已终止",
        _ => "未知"
    };
}
