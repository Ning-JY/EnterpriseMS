using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnterpriseMS.Common;
using EnterpriseMS.Common.Extensions;
using EnterpriseMS.Domain.Entities.Info;
using EnterpriseMS.Domain.Interfaces;
using EnterpriseMS.Filters;
using EnterpriseMS.Services.Interfaces;

namespace EnterpriseMS.Controllers;

/// <summary>
/// 资讯公告模块。
/// - 公开展示：Index / Detail 对任意匿名用户可见，经由 Program.cs 的 pub/{action} 路由对外发布（/pub、/pub/Detail/{id}）。
/// - 后台管理：Manage / Category 及增删改接口需登录。
/// 数据层（InfoArticle / InfoCategory 表、仓储、迁移）早已就绪，本控制器补齐缺失的业务与界面。
/// </summary>
[Authorize]
public class InfoController : BaseAuthController
{
    private readonly IUnitOfWork _uow;

    public InfoController(IUnitOfWork uow, IPermissionService permSvc)
        : base(permSvc)
    {
        _uow = uow;
    }

    // ── 公开列表（匿名可访问，pub 路由）─────────────────────
    [AllowAnonymous]
    public async Task<IActionResult> Index(long? categoryId, int page = 1)
    {
        var cats = await _uow.InfoCategories.Query()
            .Where(c => c.Status == 1 && !c.IsDeleted)
            .OrderBy(c => c.Sort).ToListAsync();

        var q = _uow.InfoArticles.Query()
            .Include(a => a.Category)
            .Where(a => !a.IsDeleted && a.Status == 1 && a.IsPublic == 1);
        if (categoryId.HasValue) q = q.Where(a => a.CategoryId == categoryId.Value);

        var total = await q.CountAsync();
        var list  = await q.OrderByDescending(a => a.IsTop)
                           .ThenByDescending(a => a.PublishTime ?? a.CreatedAt)
                           .Skip((page - 1) * 12).Take(12).ToListAsync();

        ViewBag.Categories  = cats;
        ViewBag.CategoryId  = categoryId;
        ViewBag.Total       = total;
        ViewBag.Page        = page;
        return View(list);
    }

    // ── 最新公开公告（供顶部走马灯 / 前端拉取，匿名可访问）────────
    [AllowAnonymous]
    [HttpGet("latest")]
    public async Task<IActionResult> Latest(int n = 8)
    {
        var list = await _uow.InfoArticles.Query()
            .Where(a => !a.IsDeleted && a.Status == 1 && a.IsPublic == 1)
            .OrderByDescending(a => a.IsTop)
            .ThenByDescending(a => a.PublishTime ?? a.CreatedAt)
            .Take(n).ToListAsync();
        var data = list.Select(a => new { id = a.Id, title = a.Title });
        return Json(new { success = true, data });
    }

    // ── 公开详情（匿名可访问）──────────────────────────────
    [AllowAnonymous]
    public async Task<IActionResult> Detail(long id)
    {
        var a = await _uow.InfoArticles.Query()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.Status == 1);
        if (a == null) return NotFound("公告不存在");

        // 阅读量自增（匿名写入，幂等无伤）
        a.ViewCount++;
        _uow.InfoArticles.Update(a);
        await _uow.SaveChangesAsync();

        return View(a);
    }

    // ── 后台管理列表 ───────────────────────────────────────
    public async Task<IActionResult> Manage(long? categoryId, string? keyword, int page = 1)
    {
        var cats = await _uow.InfoCategories.Query()
            .Where(c => !c.IsDeleted).OrderBy(c => c.Sort).ToListAsync();

        var q = _uow.InfoArticles.Query().Include(a => a.Category).Where(a => !a.IsDeleted);
        if (categoryId.HasValue) q = q.Where(a => a.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(a => a.Title.Contains(keyword));

        var total = await q.CountAsync();
        var list  = await q.OrderByDescending(a => a.IsTop)
                           .ThenByDescending(a => a.CreatedAt)
                           .Skip((page - 1) * 15).Take(15).ToListAsync();

        ViewBag.Categories = cats;
        ViewBag.CategoryId = categoryId;
        ViewBag.Keyword    = keyword;
        ViewBag.Total      = total;
        ViewBag.Page       = page;
        return View(list);
    }

    // ── 获取单条（编辑回填）───────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Article(long id)
    {
        var a = await _uow.InfoArticles.GetByIdAsync(id);
        if (a == null || a.IsDeleted) return ApiFail("公告不存在");
        return ApiOk(new
        {
            id         = a.Id,
            categoryId = a.CategoryId,
            title      = a.Title,
            content    = a.Content,
            isTop      = a.IsTop,
            isPublic   = a.IsPublic,
            status     = a.Status,
        });
    }

    // ── 保存（新增 / 编辑）────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(long id, long categoryId, string title,
        string? content, bool isTop = false, bool isPublic = false, int status = 1)
    {
        if (string.IsNullOrWhiteSpace(title))
            return ApiFail("标题不能为空");
        if (categoryId <= 0)
            return ApiFail("请选择分类");

        var oper = User.GetRealName();
        if (id > 0)
        {
            var a = await _uow.InfoArticles.GetByIdAsync(id);
            if (a == null || a.IsDeleted) return ApiFail("公告不存在");
            a.CategoryId = categoryId;
            a.Title      = title;
            a.Content    = content ?? "";
            a.IsTop      = isTop   ? 1 : 0;
            a.IsPublic   = isPublic ? 1 : 0;
            a.Status     = status;
            a.UpdatedAt  = DateTime.Now;
            a.UpdatedBy  = oper;
            _uow.InfoArticles.Update(a);
        }
        else
        {
            var a = new InfoArticle
            {
                CategoryId = categoryId,
                Title      = title,
                Content    = content ?? "",
                IsTop      = isTop   ? 1 : 0,
                IsPublic   = isPublic ? 1 : 0,
                Status     = status,
                PublishTime = status == 1 ? DateTime.Now : null,
                ViewCount  = 0,
                CreatedAt  = DateTime.Now,
                CreatedBy  = oper,
            };
            await _uow.InfoArticles.AddAsync(a);
        }

        await _uow.SaveChangesAsync();
        return ApiOk<object>(null!, "保存成功");
    }

    // ── 删除（软删除）─────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var a = await _uow.InfoArticles.GetByIdAsync(id);
        if (a == null || a.IsDeleted) return ApiFail("公告不存在");
        _uow.InfoArticles.SoftDelete(a);
        await _uow.SaveChangesAsync();
        return ApiOk<object>(null!, "已删除");
    }

    // ── 分类管理 ──────────────────────────────────────────
    public async Task<IActionResult> Category()
    {
        var list = await _uow.InfoCategories.Query()
            .Where(c => !c.IsDeleted).OrderBy(c => c.Sort).ToListAsync();
        return View(list);
    }

    // ── 获取单个分类（编辑回填）───────────────────────────
    [HttpGet("api/category/{id}")]
    public async Task<IActionResult> CategoryApi(long id)
    {
        var c = await _uow.InfoCategories.GetByIdAsync(id);
        if (c == null || c.IsDeleted) return ApiFail("分类不存在");
        return ApiOk(new
        {
            id           = c.Id,
            categoryName = c.CategoryName,
            sort         = c.Sort,
            status       = c.Status,
            isPublic     = c.IsPublic,
        });
    }

    [HttpPost("api/category/save"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCategory(long id, string categoryName,
        int sort = 0, int status = 1, bool isPublic = false)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return ApiFail("分类名称不能为空");

        var oper = User.GetRealName();
        if (id > 0)
        {
            var c = await _uow.InfoCategories.GetByIdAsync(id);
            if (c == null || c.IsDeleted) return ApiFail("分类不存在");
            c.CategoryName = categoryName;
            c.Sort         = sort;
            c.Status       = status;
            c.IsPublic     = isPublic ? 1 : 0;
            c.UpdatedAt    = DateTime.Now;
            c.UpdatedBy    = oper;
            _uow.InfoCategories.Update(c);
        }
        else
        {
            await _uow.InfoCategories.AddAsync(new InfoCategory
            {
                CategoryName = categoryName,
                ParentId     = 0,
                Sort         = sort,
                Status       = status,
                IsPublic     = isPublic ? 1 : 0,
                CreatedAt    = DateTime.Now,
                CreatedBy    = oper,
            });
        }

        await _uow.SaveChangesAsync();
        return ApiOk<object>(null!, "保存成功");
    }

    [HttpPost("api/category/delete/{id}")]
    public async Task<IActionResult> DeleteCategory(long id)
    {
        var c = await _uow.InfoCategories.GetByIdAsync(id);
        if (c == null || c.IsDeleted) return ApiFail("分类不存在");

        var hasArticles = await _uow.InfoArticles.AnyAsync(a => a.CategoryId == id && !a.IsDeleted);
        if (hasArticles) return ApiFail("该分类下还有公告，请先迁移或删除");

        _uow.InfoCategories.SoftDelete(c);
        await _uow.SaveChangesAsync();
        return ApiOk<object>(null!, "已删除");
    }
}
