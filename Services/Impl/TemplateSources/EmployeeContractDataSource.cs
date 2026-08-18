using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseMS.Services.Impl.TemplateSources;

/// <summary>员工合同数据源：绑定员工合同（hr_contract）实体字段。</summary>
public class EmployeeContractDataSource : ReflectionDataSourceBase
{
    public EmployeeContractDataSource(AppDbContext db) : base(db) { }

    public override string SourceId => "employeecontract";
    public override string DisplayName => "员工合同";

    protected override Dictionary<string, string> Schema => new()
    {
        ["ContractNo"] = "合同编号",
        ["ContractType"] = "合同类型",
        ["StartDate"] = "开始日期",
        ["EndDate"] = "结束日期",
        ["SignDate"] = "签订日期",
        ["Status"] = "状态",
        ["Remark"] = "备注"
    };

    protected override object? GetInstance(long id) =>
        Db.Contracts.FirstOrDefault(c => c.Id == id);

    public override Task<List<DataSourceInstance>> ListInstancesAsync() =>
        Db.Contracts.Select(c => new DataSourceInstance { Id = c.Id.ToString(), Name = c.ContractNo })
            .ToListAsync();
}
