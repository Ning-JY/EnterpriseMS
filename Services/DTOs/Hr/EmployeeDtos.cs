namespace EnterpriseMS.Services.DTOs.Hr;

public class EmployeeSimpleDto
{
    public long    Id       { get; set; }
    public string  RealName { get; set; } = "";
    public string? DeptName { get; set; }
    /// <summary>显示名：姓名（部门），用于下拉选项</summary>
    public string  Display  => string.IsNullOrEmpty(DeptName)
                               ? RealName : $"{RealName}（{DeptName}）";
}
