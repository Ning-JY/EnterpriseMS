namespace EnterpriseMS.Domain.Interfaces;

/// <summary>
/// 带附件的实体契约（审计 3.4：统一 Certificate / Contract 中重复的 DeleteFileAsync 样板）。
/// 拥有附件路径与更新人字段的实体实现本接口后，可复用 FileManageHelper.DeleteFileAsync。
/// </summary>
public interface IFileEntity
{
    string? FilePath  { get; set; }
    string? FileName  { get; set; }
    string? UpdatedBy { get; set; }
}
