namespace EnterpriseMS.Common;

/// <summary>
/// 上传大小上限 —— 全站单一来源。
/// Program.cs（Kestrel + FormOptions）与各 Controller 的 [RequestSizeLimit] 统一引用此处，
/// 避免多处各写一个数字导致"改了一处、另一处还在拦"。
///
/// ⚠️ 部署注意：若站点前面挂了 Nginx / 1Panel OpenResty 反向代理，
/// 其 client_max_body_size 必须 ≥ 本值，否则超限请求在到达应用之前
/// 就会被反代以 413 (Request Entity Too Large) 拒绝，应用层永远看不到该请求。
/// 表现为"代码里明明放了 500MB，却只能传 100MB" —— 100m 正是 1Panel 站点的默认值。
/// </summary>
public static class UploadLimits
{
    /// <summary>500MB</summary>
    public const long MaxUploadBytes = 500L * 1024 * 1024;
}
