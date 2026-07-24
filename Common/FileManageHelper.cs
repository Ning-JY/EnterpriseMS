using System.IO;
using EnterpriseMS.Domain.Base;
using EnterpriseMS.Domain.Interfaces;

namespace EnterpriseMS.Common;

/// <summary>
/// 附件管理辅助（审计 3.4）：消除 Certificate / Contract 中逐字重复的 DeleteFileAsync 物理删除样板。
/// 仅负责"清空文件路径字段 + 删除磁盘文件"，持久化由调用方以 save 回调提交，保持仓储抽象一致。
/// </summary>
public static class FileManageHelper
{
    public static async Task DeleteFileAsync<T>(IRepository<T> repo, Func<Task> save, long id, string operBy)
        where T : BaseEntity, IFileEntity
    {
        var entity = await repo.GetByIdAsync(id)
            ?? throw new NotFoundException("记录不存在");
        if (entity.FilePath != null && File.Exists(entity.FilePath))
            File.Delete(entity.FilePath);
        entity.FilePath  = null;
        entity.FileName  = null;
        entity.UpdatedBy = operBy;
        repo.Update(entity);
        await save();
    }
}
