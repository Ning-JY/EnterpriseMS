using Microsoft.AspNetCore.Mvc;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.ViewComponents;

/// <summary>
/// 部门下拉统一组件。所有页面需要部门选择时统一调用：
///   @await Component.InvokeAsync("DeptSelect", new { id = "deptId", selectedId = Model.DeptId, emptyText = "全部部门" })
/// 自取部门树数据，避免各页面重复 ViewBag.Depts + 内联 option 渲染。
/// </summary>
public class DeptSelectViewComponent : ViewComponent
{
    private readonly IDeptService _deptSvc;
    public DeptSelectViewComponent(IDeptService deptSvc) => _deptSvc = deptSvc;

    public async Task<IViewComponentResult> InvokeAsync(
        string id = "deptId", string name = null, long selectedId = 0,
        string emptyText = "请选择部门", string cssClass = "form-control", bool required = false)
    {
        var tree = await _deptSvc.GetTreeAsync();
        var model = new DeptSelectViewModel
        {
            Id = id,
            Name = name ?? id,
            Tree = tree,
            SelectedId = selectedId,
            EmptyText = emptyText,
            CssClass = cssClass,
            Required = required,
        };
        return View(model);
    }
}
