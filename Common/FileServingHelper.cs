using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseMS.Common;

/// <summary>
/// 从磁盘物理路径「吐文件」的纯基础设施助手。
/// 统一处理三件事，消除各 Controller 下载逻辑散落导致的不一致与隐患：
///   1. MIME 推导 —— 复用 MimeHelper（单一来源），不再各处硬编码 octet-stream；
///   2. 中文文件名 —— 借助框架 FileDownloadName 自动生成 RFC 5987 编码
///      （attachment; filename="..."; filename*=UTF-8''...），杜绝下载中文名乱码；
///   3. 流式返回 —— 用 PhysicalFileResult 由 ASP.NET Core 直接流式写出并支持 Range 请求，
///      不再 ReadAllBytes 把大文件（上限 500MB）整块读进内存。
/// 各业务模块的文件元数据（落在哪张表、哪些字段、谁能下）仍由各模块 Service / [HasPermission] 负责，
/// 本类只管“把磁盘字节安全地吐出去”，与表结构解耦。
/// </summary>
public static class FileServingHelper
{
    /// <summary>
    /// 从物理路径流式返回文件。
    /// </summary>
    /// <param name="physicalPath">磁盘绝对路径</param>
    /// <param name="fileName">给浏览器/用户的显示文件名（可含中文，自动编码）</param>
    /// <param name="fileExt">扩展名（含或不含点均可）；为空时从 fileName 推导</param>
    /// <param name="inline">true=浏览器内联预览（图片/PDF 等）；false=强制下载（默认）</param>
    public static PhysicalFileResult ServePhysicalFile(
        string physicalPath,
        string fileName,
        string? fileExt = null,
        bool inline = false)
    {
        var ext  = (fileExt ?? Path.GetExtension(fileName)).TrimStart('.');
        var mime = MimeHelper.GetMimeType(ext);

        var result = new PhysicalFileResult(physicalPath, mime);
        // inline=true：不设 FileDownloadName → 不输出 Content-Disposition，浏览器默认内联渲染；
        // inline=false（默认）：由框架自动生成 attachment; filename*=UTF-8''<编码>，强制下载且中文名不乱码。
        if (!inline) result.FileDownloadName = fileName;
        return result;
    }
}
