namespace EnterpriseMS.ViewComponents;

/// <summary>部门下拉视图模型（DeptSelect 组件共用）</summary>
public class DeptSelectViewModel
{
    public string Id { get; set; } = "deptId";
    public string Name { get; set; } = "deptId";
    public List<EnterpriseMS.Services.DTOs.System.DeptTreeDto> Tree { get; set; } = new();
    public long SelectedId { get; set; }
    public string EmptyText { get; set; } = "请选择部门";
    public string CssClass { get; set; } = "form-control";
    public bool Required { get; set; }
}

/// <summary>人员下拉视图模型（PersonSelect 组件共用）</summary>
public class PersonSelectViewModel
{
    public string Id { get; set; } = "employeeId";
    public string Name { get; set; } = "employeeId";
    public List<EnterpriseMS.Services.DTOs.Hr.EmployeeSimpleDto> Persons { get; set; } = new();
    public long SelectedId { get; set; }
    public string EmptyText { get; set; } = "请选择人员";
    public string CssClass { get; set; } = "form-control";
    public bool Required { get; set; }
}
