using Microsoft.AspNetCore.Mvc;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.ViewComponents;

/// <summary>
/// 人员下拉统一组件。所有页面需要人员选择时统一调用：
///   @await Component.InvokeAsync("PersonSelect", new { id = "techLeader", selectedId = Model.TechLeaderId, emptyText = "请选择" })
/// 自取在职员工数据，避免各页面重复 ViewBag.Members + 内联 option 渲染。
/// </summary>
public class PersonSelectViewComponent : ViewComponent
{
    private readonly IEmployeeQueryService _empQrySvc;
    public PersonSelectViewComponent(IEmployeeQueryService empQrySvc) => _empQrySvc = empQrySvc;

    public async Task<IViewComponentResult> InvokeAsync(
        string id = "employeeId", string name = null, long selectedId = 0,
        string emptyText = "请选择人员", string cssClass = "form-control", bool required = false,
        string onchange = null)
    {
        var persons = await _empQrySvc.GetAllOnJobAsync();
        var model = new PersonSelectViewModel
        {
            Id = id,
            Name = name ?? id,
            Persons = persons,
            SelectedId = selectedId,
            EmptyText = emptyText,
            CssClass = cssClass,
            Required = required,
            Onchange = onchange,
        };
        return View(model);
    }
}
