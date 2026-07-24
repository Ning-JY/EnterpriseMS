using EnterpriseMS.Services.DTOs.Report;

namespace EnterpriseMS.Services.Interfaces;

/// <summary>
/// 报表查询服务 —— 将原 ReportController 中直接写在 Controller 内的 EF 查询与聚合逻辑
/// 下沉到 Service 层，Controller 仅负责参数校验与路由分发（消除 Controller 越层直连 DbContext）。
/// </summary>
public interface IReportService
{
    Task<ReceiptReportDto> GetReceiptReportAsync(int? year, long? deptId, string? keyword);
    Task<OutputReportDto>  GetOutputReportAsync(int? year, long? deptId, string? keyword);
    Task<(byte[] Bytes, string FileName)> ExportReceiptAsync(int? year, long? deptId, string? keyword);
    Task<(byte[] Bytes, string FileName)> ExportOutputAsync(int? year, long? deptId, string? keyword);
}
