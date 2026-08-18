using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterpriseMS.Domain.Entities.Project;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseMS.Services.Impl.TemplateSources;

/// <summary>合同数据源：绑定项目合同（proj_contract）实体字段。</summary>
public class ProjectContractDataSource : ReflectionDataSourceBase
{
    public ProjectContractDataSource(AppDbContext db) : base(db) { }

    public override string SourceId => "projcontract";
    public override string DisplayName => "合同";

    protected override Dictionary<string, string> Schema => new()
    {
        ["ContractNo"] = "合同编号",
        ["ContractName"] = "合同名称",
        ["ContractType"] = "合同类型",
        ["PartyA"] = "甲方",
        ["PartyB"] = "乙方",
        ["Amount"] = "合同金额(元)",
        ["SignDate"] = "签订日期",
        ["StartDate"] = "开始日期",
        ["EndDate"] = "结束日期",
        ["Status"] = "状态",
        ["Remark"] = "备注"
    };

    protected override object? GetInstance(long id) =>
        Db.ProjContracts.FirstOrDefault(c => c.Id == id);

    public override Task<List<DataSourceInstance>> ListInstancesAsync() =>
        Db.ProjContracts.Select(c => new DataSourceInstance
            {
                Id = c.Id.ToString(),
                Name = (c.ContractName ?? c.ContractNo) ?? ("合同 " + c.Id)
            })
            .ToListAsync();
}
