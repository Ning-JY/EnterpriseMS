using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterpriseMS.Domain.Entities.Hr;
using EnterpriseMS.Infrastructure.Data;
using EnterpriseMS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseMS.Services.Impl.TemplateSources;

/// <summary>员工数据源：绑定员工实体的常用字段。</summary>
public class EmployeeDataSource : ReflectionDataSourceBase
{
    public EmployeeDataSource(AppDbContext db) : base(db) { }

    public override string SourceId => "employee";
    public override string DisplayName => "员工";

    protected override Dictionary<string, string> Schema => new()
    {
        ["EmpNo"] = "工号",
        ["RealName"] = "姓名",
        ["Gender"] = "性别",
        ["IdCard"] = "身份证号",
        ["Phone"] = "手机号",
        ["Email"] = "邮箱",
        ["Education"] = "学历",
        ["NativePlace"] = "籍贯",
        ["Address"] = "地址",
        ["EntryDate"] = "入职日期",
        ["FormalDate"] = "转正日期",
        ["LeaveDate"] = "离职日期",
        ["TechnicalTitle"] = "技术职称",
        ["TechnicalLevel"] = "技术等级",
        ["BankName"] = "开户行",
        ["BankAccount"] = "银行账号",
        ["Remark"] = "备注"
    };

    protected override object? GetInstance(long id) =>
        Db.Employees.FirstOrDefault(e => e.Id == id);

    public override Task<List<DataSourceInstance>> ListInstancesAsync() =>
        Db.Employees.Select(e => new DataSourceInstance { Id = e.Id.ToString(), Name = e.RealName })
            .ToListAsync();
}
