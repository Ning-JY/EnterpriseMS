# EnterpriseMS 前端功能实现缺口分析（正本 source/repos）

> 方法：拉取后端全部 Controller Action 地图（路由 + HTTP 动词 + 权限），再逐一对前端（40 个 `.cshtml` + 2 个 JS 文件 `site.js`/`bid-ai.js`，及嵌在各视图的内联脚本）做接线核对。
> 结论：**绝大多数功能前端已完整接线**；真正"没做/接坏"的只有 2 处——① 资讯公告模块整块缺失（无控制器无页面）；② 通知标记已读因路由错位实际失效。

---

## 一、明确缺口 / BUG

### 1. 🔴 资讯公告模块（InfoArticle / InfoCategory）整块未实现
- 数据层齐全：`UnitOfWork` 已注入 `InfoArticles`、`InfoCategories` 仓储；`AppDbContext` 有对应实体；`Program.cs` 还注册了公开路由 `app.MapControllerRoute("public", "pub/{action=Index}/{id?}", new { controller = "Info" })`。
- 但 **`Controllers/` 下不存在 `InfoController`**（`Glob Controllers/**/Info*.cs` 无结果），也**没有任何 `Views/Info` 或公开资讯页**。
- 后果：
  - 后台「资讯/公告管理」页面缺失（无法增删改查公告）；
  - 前台 `/pub/...` 公开路由全部 404；
  - 若系统菜单里有「公告/资讯」入口，点进去会 404。
- 修复：新建 `InfoController`（含列表/详情/管理 CRUD，区分匿名公开与后台管理权限）+ 对应 Views + 菜单项接线。

### 2. 🟠 通知「标记已读」路由错位（已实现但失效，等同没做）
- 前端 `wwwroot/js/site.js`：
  - 单条：`fetch('/notifications/mark-read?id=' + id)` （第 139 行）
  - 全部：`fetch('/notifications/mark-all-read')` （第 163 行）
- 后端 `NotificationController`：`MarkRead` / `MarkAllRead` 仅带 `[HttpPost]`，**无 `[Route]`**，约定路由为 `/notifications/markread`、`/notifications/markallread`。
- 问题：URL 中的连字符 `mark-read` 无法匹配 action 名 `MarkRead`（ASP.NET 约定路由按字面匹配，连字符不忽略）→ 前端请求实际 **404**，点通知/「全部已读」无效果（仅页面视觉上变了，刷新后未读仍在）。
- 修复（二选一，成本极低）：
  - 前端改 URL 为 `/notifications/markread` 与 `/notifications/markallread`；或
  - 后端 action 加 `[Route("mark-read")]` / `[Route("mark-all-read")]`。
- 注：`Notification/Index.cshtml` 的 `@section Scripts` 为空，依赖 `site.js` 全局事件委托，逻辑本身存在，只是 URL 错。

---

## 二、易误判、实际已实现（澄清，非缺口）

| 疑似项 | 真相 |
|---|---|
| Bid 导出 Word/PDF 未接线 | 已接。`Bid/Detail.cshtml:654` 调 `/Bid/ExportWord`、`/Bid/ExportPdf`（之前用小写 `exportword` 搜漏了，实为混合大小写） |
| 个人产值统计(my-stats) 无取数 | 服务端渲染（Controller 直接传 Model），无需 ajax，已完整 |
| 费用计算器无计算逻辑 | `Tool/Calculator.cshtml` 内联完整累进计费 JS，已实现 |
| 系统配置页静态 | `Config.cshtml:167` 已 `ajaxPost('/system/config/save', …)`；`all` 由服务端渲染 |
| 模板报告删除/列表/下载/预览/生成缺失 | `TemplateReport/Index|Manage.cshtml` 全部接线（templates/download/delete/template/preview/generate） |
| AI 章节生成/整标生成/人员匹配/审校缺失 | `bid-ai.js` + `Bid/Detail.cshtml` 全部接线 |

---

## 三、各模块前端接线完整性清单

| 模块 | 页面(View) | 增删改查/交互 | 说明 |
|---|---|---|---|
| 系统-用户 | ✅ | ✅ | resetpwd/status/changepwd 均接 |
| 系统-角色 | ✅ | ✅ | assignmenus(授权) 已接 |
| 系统-菜单 | ✅ | ✅ | 树形增删改 |
| 系统-部门 | ✅ | ✅ | |
| 系统-字典 | ✅ | ✅ | 类型+数据 CRUD |
| 系统-配置 | ✅ | ✅ | save 已接 |
| 系统-日志 | ✅ | ✅ | 只读列表 |
| 系统-调试 | ✅ | ✅ | seed/clear-cache/migrate/ai-config/ai-test 全接 |
| HR-员工 | ✅ | ✅ | 转正/离职/详情 已接 |
| HR-合同/证书/学历/工作经历 | ✅ | ✅ | 上传/下载/删除 已接 |
| 项目 | ✅ | ✅ | 成员/里程碑/验收/合同/回款/文件 各 tab 均接 |
| 项目导入 | ✅ | ✅ | import/execute + 模板下载 |
| 投标(Bid) | ✅ | ✅ | 解析/确认要素/流式生成章节/整标生成/拼装/人员匹配/审校/导出Word/PDF 全接 |
| 知识库(Kb) | ✅ | ✅ | 上传/下载/预览/置顶/删除 |
| 报表 | ✅ | ✅ | 收/付款收据 + 导出 |
| 模板报告 | ✅ | ✅ | 配置/扫描占位符/预览/生成/删除/下载 |
| 通知中心 | ✅ | ⚠️ | 列表页在，但「标记已读」路由错位失效（见一.2） |
| 首页/个人 | ✅ | ✅ | 个人资料改密、个人产值 |
| 工具-计算器 | ✅ | ✅ | |
| 工具-报告 | ✅ | ✅ | exportWordReport 已接 |
| 登录/登出 | ✅ | ✅ | |
| **资讯公告** | ❌ | ❌ | **无控制器、无页面（见一.1）** |

---

## 四、建议修复顺序
1. **资讯公告模块**：补 `InfoController` + Views + 菜单（功能完全缺失，最该做）。
2. **通知标记已读**：改 URL 或加 `[Route]`，1 行级修复，立即恢复「标记已读」。

> 附：其余如岗位(Post)、评审意见(ReviewOpinion)等仅有数据层/仓储、无独立管理页，属于参考数据或内部存储，通常不必单独立页，列为低优先观察项。
