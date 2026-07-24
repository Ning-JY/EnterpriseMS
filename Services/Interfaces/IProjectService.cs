using EnterpriseMS.Common;
using EnterpriseMS.Domain.Entities.Project;
using EnterpriseMS.Services.DTOs.Project;

namespace EnterpriseMS.Services.Interfaces;

public interface IProjectService
{
    Task<PagedResult<ProjectListDto>> GetPagedAsync(ProjectQueryDto query, long operUserId);
    Task<ProjectDetailDto?>           GetDetailAsync(long id, long operUserId);
    Dictionary<string, string>        BuildReportFieldValues(ProjectDetailDto p);
    Task<long>                        CreateAsync(CreateProjectDto dto, string operBy);
    Task                              UpdateAsync(UpdateProjectDto dto, string operBy);
    Task                              ChangeStatusAsync(ChangeStatusDto dto, string operBy);
    Task                              TerminateAsync(long id, string reason, string operBy);
    Task<string>                      GenerateProjNoAsync();
    Task<string>                      GenerateProjNoSuffixAsync();
    // 批量导入（由 Excel 解析出的实体集合，统一在此持久化）
    Task                             ImportProjectsAsync(List<Project> projects);
    // 成员
    Task<long>   AddMemberAsync(long projectId, CreateMemberDto dto, string operBy);
    Task         UpdateMemberAsync(long projectId, UpdateMemberDto dto, string operBy);
    Task         RemoveMemberAsync(long projectId, long memberId, string operBy);
    // 里程碑
    Task<long>   AddMilestoneAsync(long projectId, CreateMilestoneDto dto, string operBy);
    Task         UpdateMilestoneAsync(long projectId, UpdateMilestoneDto dto, string operBy);
    Task         DeleteMilestoneAsync(long milestoneId);
    Task         CompleteMilestoneAsync(long milestoneId, string operBy);
    // 验收
    Task<long>    AddAcceptanceAsync(CreateAcceptanceDto dto, string operBy);
    Task          UpdateAcceptanceAsync(UpdateAcceptanceDto dto, string operBy);
    Task          DeleteAcceptanceAsync(long acceptanceId);
    Task<decimal> GetTotalReceivedAsync(long projectId);
    // 合同
    Task<long>   AddContractAsync(CreateContractDto dto, string operBy);
    Task         UpdateContractAsync(UpdateContractDto dto, string operBy);
    Task         DeleteContractAsync(long contractId);
    Task         UploadContractFileAsync(long contractId, string fileName, string filePath, string operBy);
    Task         DeleteContractFileAsync(long contractId, string operBy);
    Task<(string? filePath, string? fileName)> GetContractFileAsync(long contractId);
    // 发票
    Task<long>   AddInvoiceAsync(CreateInvoiceDto dto, string operBy);
    Task         ConfirmInvoiceReceivedAsync(long invoiceId, DateTime receivedDate, string operBy);
    Task         DeleteInvoiceAsync(long invoiceId);
    Task         UploadInvoiceFileAsync(long invoiceId, string fileType, string fileName, string filePath, string operBy);
    Task<(string? filePath, string? fileName)> GetInvoiceFileAsync(long invoiceId, string fileType);
    // 文件
    Task<long>   AddFileAsync(long projectId, string category, string fileName,
        string filePath, long fileSize, string? description, string? version, string operBy);
    Task         DeleteFileAsync(long fileId);
    Task<(string? filePath, string? fileName, string? fileExt)?> GetFileAsync(long fileId);
    // 操作日志（分页）
    Task<PagedResult<ProjectLogDto>> GetLogsPagedAsync(long projectId, int page, int size);
    // 统计
    Task<object> GetDashboardStatsAsync();
    Task<object> GetMyStatsAsync(long employeeId);
}
