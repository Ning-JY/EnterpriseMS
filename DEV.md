# EnterpriseMS 开发文档

> 适用版本：layui 重构版（EnterpriseMS-New 同步所得）。涵盖模板报告引擎、造价小工具报告生成、预算模板自动带出，以及踩过的坑。
> 最后整理：2026-08-18

---

## 1. 项目概述

| 项 | 说明 |
|---|---|
| 框架 | ASP.NET 9 MVC + EF Core 9 |
| 数据库 | MySQL 8.0.36（Pomelo 驱动） |
| 前端 | layui 2.11.5（`wwwroot/lib/layui`，原 adminlte/bootstrap/fontawesome/layer 已弃用） |
| 报告渲染 | MiniWord 0.9.2 + OpenXML（WordprocessingDocument）自研渲染器 |
| 运行端口 | 5090（`Properties/launchSettings.json` 的 `applicationUrl`） |
| 迁移 | `Program.cs` 启动时 `MigrateAsync` 幂等自动应用待执行迁移 |
| 约定 | 服务“可关不可主动启”；本地调试完即用进程关闭，不长期驻留 |

---

## 2. 模板报告引擎（TemplateReport）

### 2.1 数据模型（单一真相源）

- `TemplateDefinitions`：`Id, Name, FileName, Description, CreatedAt, ContextSource?, Category?, Fields`
- `TemplateFields`：`Id, TemplateId, Name, Label, Required, Type, Source, Binding?, ConfigKey?, DefaultValue?, HelpText?, Sort`
- `ITemplateDataSource`（可插拔）：`SourceId / DisplayName / GetFieldSchema() / ListInstancesAsync() / ResolveAsync()`。DI 注入 `IEnumerable<ITemplateDataSource>` 自动发现。
- 内置 Provider：`manual / config / project / employee / projcontract / employeecontract`。
  - **注意**：本库无 `SysConfigs` 表，`config` 数据源不可用，编制单位等只能走 `manual`。

### 2.2 端点（`TemplateReportController`，多为 `[AllowAnonymous]` 可免登录调试）

| 端点 | 说明 |
|---|---|
| `GET /templatereport/data-sources` | 列出全部数据源 schema |
| `GET /templatereport/data-contexts/{sourceId}` | 某数据源的实例列表 |
| `GET /templatereport/preview-fields` | 预览模板字段（带上下文自动带出） |
| `GET /templatereport/categories` / `list` | 分类与模板列表 |
| `GET /templatereport/template/{id}` | 模板详情 |
| `POST /templatereport/configure-template` | 配置模板（FormData + 文件 + CSRF） |
| `POST /templatereport/delete` | 删除模板（JSON `{TemplateId}`） |
| `POST /templatereport/generate` | MiniWord 渲染 + OpenXML 兜底 |
| `POST /templatereport/generate-adhoc` | OpenXML 自研渲染器（造价小工具走这条） |

### 2.3 渲染双路径

```
generate        → GenerateDocument  → MiniWord 标量替换
                                  → ContainsPlaceholder 检测残留 {{ }}
                                  → 有残留则回退 GenerateAdhocReport 补全（保证零残留）

generate-adhoc  → GenerateAdhocReport（纯 OpenXML 手写）
```

`GenerateAdhocReport` 能力：
- **标量**：`{{字段}}` 段落/单元格文本替换；`RebuildParagraph` 复用首 run 格式。
- **列表行循环**：字段值为 `List<Dictionary>`/`IEnumerable`（非 string）时，用 `{{key}}` 标记所在表格行做**克隆循环**：逐 item 克隆行 → 填 item 字典键 → 清 `{{key}}` 标记 → 插原行前 → 删原行。模板里数据行首列放 `{{明细}}` 作锚点（表头留空即可）。
- **VML 艺术字**：Word 艺术字文本在 `<v:textpath string="...">` 属性里，既非 `<w:p>` 也非 `<w:t>`，段落遍历覆盖不到。`GenerateAdhocReport` 单独遍历 `DocumentFormat.OpenXml.Vml.TextPath` 替换；`GenerateDocument` 再加 MiniWord 残留检测兜底。
  - ⚠️ `using DocumentFormat.OpenXml.Vml;` 与 `System.IO.Path` 冲突 CS0104，须用全限定名 `DocumentFormat.OpenXml.Vml.TextPath`。
- **换行**：`RebuildParagraph` 按 `\n` 拆行、行间插 `<w:br/>`，保留多行值（如审核明细）。

### 2.4 关键坑（必读）

1. **MiniWord 0.9.2 不支持 `List<Dictionary>` 表格行循环**：实测把列表 `ToString()` 进单元格、行不复制、`{{序号}}` 原样残留。任何“多行明细 + docx 模板”需求都用 `generate-adhoc` + 模板里 `{{明细}}` 锚点行，而非 MiniWord 的 List 循环。
2. **VML textpath 漏替换**：预算模板 `{{编制单位}}` 有 1 处在艺术字里（`宜{{编制单位}}司`），MiniWord 与 OpenXML 段落遍历都漏。已用 `TextPath` 单独处理 + MiniWord 兜底解决。
3. **CSRF**：全局 `AutoValidateAntiforgeryToken`。所有 POST 必须同时带 `RequestVerificationToken` **头**与 cookie，否则返回 **400 空响应**（非 CSRF 报错文案）。调试用 urllib 需手动回传 `Set-Cookie` 原始头（cookie 域被解析成 localhost.local 不回传）。
4. **JSON 模型绑定**：`ReportFillRequest.ExcelRows` / `SupplementaryFields` 的值类型为 `object`（非 `string`）。前端从 Excel 读出的 `序号`/`送审金额` 等是 number，若模型要求 `string` 会抛 `The JSON value could not be converted to System.String. Path: $.ExcelRows[0].序号` → 400。渲染侧 `ToStringDict` 统一 `ToString()`，number 进来安全。
5. **中文 templateId**：含中文的模板 Id 在下载/详情 URL 需 `encodeURIComponent`。

### 2.5 向导自动带出（ContextSource + 字段 Source/Binding）

- `MergeAutoFields` → `BuildReportFieldValuesAsync(source, instanceId, tpl, manual)`：`Source='manual'` 用 manual 值；否则按 `Source` 取对应 provider，`ResolveAsync(instanceId)` 后用 `f.Binding`（config 用 `f.ConfigKey`）**反射取实体属性值**。
- **绑定填 C# 实体属性名**（非 DB 列名）。
- 预算模板：`ContextSource='project'`，4 字段绑定 `ProjName / OwnerName / ProjectOverview / BizType` 自动带出（工程名称/建设单位/工程概况/业务类型）；其余 13 字段保持 `manual`（送审/审定金额、审减、审核明细、编制单位、文号等——审计/乙方专属，无法从项目推导）。
- `BuildFieldValues`：含 `送审金额` + `审定金额` 且 `审减金额`/`审减率` 为空时，自动算 `审减金额 = 送审 - 审定`、`审减率 = 审减/送审*100%`。

---

## 3. 造价小工具报告生成（`/tool/report`）

- 工具栏“模板”下拉：拉 `GET /templatereport/templates` 过滤 `Category='造价'` 填选项。
- 导出流程：前端解析 Excel → 勾选明细行 → 组装 `ExcelRows`（每行字典）/ `ExcelColumns`（列映射 FieldName↔ColumnName）/ `SupplementaryFields`（标量合计等）→ `POST /templatereport/generate-adhoc` → `saveAs` 下载 docx。
- 造价模板 `cost-audit-report.docx`（`wwwroot/templates/`）：标题 + 标量段 + 12 列表格，数据行首列 `{{明细}}` 为循环锚点。
- `ExcelRows` 与 `ExcelColumns` **需同时非空**才会聚合为 `明细` 列表字段触发行循环；只发 `ExcelRows` 不循环。
- 前端 `exportWordReport()` 必带 `RequestVerificationToken` 头（见 §2.4.3）。

---

## 4. 预算审核报告模板（`budget-audit-report`）

- 纯标量 17 占位符、无表格，走 `generate`（MiniWord + art-textpath 回退）。
- 桌面模板 `{{送审进度}}` 已改为 `{{送审金额}}`，落盘 `wwwroot/templates/budget-audit-report.docx`。
- DB 注册：`Category='预算'`，`ContextSource='project'`，4 字段自动带出，审减自动算。
- 验证结果：生成 HTTP 200、零残留、VML `{{编制单位}}` 已填、审核明细 `\n`→`<w:br/>`、审减金额/率自动算。

---

## 5. 数据库迁移 / 历史漂移

- 共享库：`192.168.1.100/enterprise_db`（必要时 `pymysql` 直写补 `__EFMigrationsHistory`、补列）。
- 漂移恢复：给已建表补迁移历史行后 `dotnet ef database update` **仅跑待应用迁移**，切勿整库 `drop` 或 `update <target>`。
- 曾删除预算模块：首迁移 `Up` 含 `InsertData(budget_*)` 会抛 “no entity mapped”，需删对应 `InsertData`。

---

## 6. 运行与部署

```bash
# 本地运行（端口 5090）
dotnet run --urls "http://localhost:5090"

# 编译校验
dotnet build -c Debug --nologo
```

- `Program.cs` 启动即 `MigrateAsync`，新增迁移后无需手动建表。
- 调试完用 `Stop-Process -Name EnterpriseMS`（PowerShell）或等价方式关闭，避免 bin 被锁导致后续 `dotnet build` 报 MSB3027。
