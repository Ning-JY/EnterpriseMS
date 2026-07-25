using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Domain.Entities.Info;

namespace EnterpriseMS.Infrastructure.Data.Seeds;

/// <summary>
/// 资讯/知识库种子（菜单已拆分到 MenuSeeds，字典已拆分到 DictSeeds）。
/// </summary>
public static class InfoSeeds
{
    public static void Seed(ModelBuilder mb)
    {
        var dt = new DateTime(2026, 1, 1);

        // ── 知识库分类 ────────────────────────────────────────────
        mb.Entity<KbCategory>().HasData(
            new KbCategory { Id = 1, Name = "模板文件", Icon = "fa-file-word", Description = "常用工作模板，合同模板、报告模板等", Sort = 1, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new KbCategory { Id = 2, Name = "公司通知", Icon = "fa-bullhorn", Description = "公司内部通知、公告", Sort = 2, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new KbCategory { Id = 3, Name = "行业规范", Icon = "fa-book", Description = "工程咨询、规划、造价等行业标准规范", Sort = 3, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new KbCategory { Id = 4, Name = "规章制度", Icon = "fa-gavel", Description = "公司规章制度、管理办法", Sort = 4, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new KbCategory { Id = 5, Name = "培训资料", Icon = "fa-graduation-cap", Description = "内部培训讲义、学习材料", Sort = 5, Status = 1, CreatedAt = dt, CreatedBy = "system" },
            new KbCategory { Id = 6, Name = "其他", Icon = "fa-folder-open", Description = "其他共享文件", Sort = 6, Status = 1, CreatedAt = dt, CreatedBy = "system" }
        );
    }
}
