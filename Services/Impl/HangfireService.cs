using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Services.Impl;

/// <summary>
/// Hangfire 定时任务服务 —— 通过 IUnitOfWork 访问数据，不直接注入 DbContext。
/// </summary>
public class HangfireService : IHangfireService
{
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _cfg;
    private readonly ILogger<HangfireService> _log;

    public HangfireService(IUnitOfWork uow, IConfiguration cfg, ILogger<HangfireService> log)
    { _uow = uow; _cfg = cfg; _log = log; }

    public async Task CheckContractExpireAsync()
    {
        var days = _cfg.GetValue<int>("Hangfire:ContractWarningDays", 30);
        var n = await _uow.Contracts.CountAsync(
            c => !c.IsDeleted && c.Status == 0
              && c.EndDate <= DateTime.Today.AddDays(days));
        if (n > 0) _log.LogWarning("【合同到期预警】{Count} 份将在 {Days} 天内到期", n, days);
    }

    public async Task CheckCertExpireAsync()
    {
        var days = _cfg.GetValue<int>("Hangfire:CertWarningDays", 60);
        var n = await _uow.Certificates.CountAsync(
            c => !c.IsDeleted && c.Status == 0
              && c.ExpireDate.HasValue && c.ExpireDate <= DateTime.Today.AddDays(days));
        if (n > 0) _log.LogWarning("【证书到期预警】{Count} 张将在 {Days} 天内到期", n, days);
    }

    public async Task CheckMilestoneOverdueAsync()
    {
        var list = await _uow.Milestones.GetListAsync(
            m => !m.IsDeleted && m.Status != 2
              && m.PlanDate < DateTime.Today && !m.IsOverdue);
        if (!list.Any()) return;
        foreach (var m in list) m.IsOverdue = true;
        await _uow.SaveChangesAsync();
        _log.LogWarning("【里程碑逾期】标记 {Count} 个", list.Count);
    }
}
