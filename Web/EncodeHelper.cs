using System.Text.Encodings.Web;

namespace EnterpriseMS.Web;

/// <summary>
/// 视图层共用的 JS 字符串编码助手。
/// <see cref="JavaScriptEncoder.Default.Encode(string)"/> 在传入 null 时会抛
/// <see cref="System.ArgumentNullException"/>（Parameter 'value'），而大量列表页把可空的
/// Leader/Phone/RealName/FileExt 等字段直接传入，登录态下渲染编辑/删除按钮即会崩溃。
/// 统一经 <see cref="SafeEncode"/> 兜底空值，避免散落各处的 null 编码异常。
/// </summary>
public static class EncodeHelper
{
    public static string SafeEncode(string? value) => JavaScriptEncoder.Default.Encode(value ?? "");
}
