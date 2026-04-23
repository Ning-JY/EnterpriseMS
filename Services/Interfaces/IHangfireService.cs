namespace EnterpriseMS.Services.Interfaces;

public interface IHangfireService
{
    Task CheckContractExpireAsync();
    Task CheckCertExpireAsync();
    Task CheckMilestoneOverdueAsync();
}
