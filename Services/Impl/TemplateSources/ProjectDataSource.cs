using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterpriseMS.Domain.Entities.Project;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseMS.Services.Impl.TemplateSources;

/// <summary>项目数据源：绑定项目实体的常用字段。</summary>
public class ProjectDataSource : ReflectionDataSourceBase
{
    public ProjectDataSource(AppDbContext db) : base(db) { }

    public override string SourceId => "project";
    public override string DisplayName => "项目";

    protected override Dictionary<string, string> Schema => new()
    {
        ["ProjNo"] = "项目编号",
        ["ProjName"] = "工程名称",
        ["OwnerName"] = "建设单位",
        ["OwnerContact"] = "建设单位联系人",
        ["OwnerPhone"] = "建设单位联系电话",
        ["BuildingScale"] = "建设规模",
        ["LimitPrice"] = "限价(元)",
        ["ContractAmount"] = "合同金额(元)",
        ["BizType"] = "业务类型",
        ["ProcurementType"] = "采购方式",
        ["SignDate"] = "合同签订日期",
        ["PlanEndDate"] = "计划竣工日期",
        ["ActualEndDate"] = "实际竣工日期",
        ["BidDeadline"] = "投标截止日期",
        ["ProgressStatus"] = "进度状态",
        ["ProjectCategory"] = "项目类别",
        ["OurRatio"] = "我方占比",
        ["ProjectOverview"] = "项目概况",
        ["Remark"] = "备注"
    };

    protected override object? GetInstance(long id) =>
        Db.Projects.FirstOrDefault(p => p.Id == id);

    public override Task<List<DataSourceInstance>> ListInstancesAsync() =>
        Db.Projects.Select(p => new DataSourceInstance { Id = p.Id.ToString(), Name = p.ProjName })
            .ToListAsync();
}
