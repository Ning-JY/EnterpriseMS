/* ==========================================================================
   EnterpriseMS 公共 JS —— 全站唯一的交互层
   --------------------------------------------------------------------------
   目标：业务页面只描述「有什么字段、调哪个接口」，交互流程全部走这里。
   典型的一个列表页只需要：

       ems.ready(function () {
           ems.initTable({
               elem: '#userTable', id: 'userTable', url: '/system/user/list',
               cols: [[ ... ]],
               toolbar: { add: function () { ems.openAdd({ url: '/system/user/form' }); } },
               tool:    {
                   edit: function (d) { ems.openEdit({ url: '/system/user/form?id=' + d.Id }); },
                   del:  function (d) { ems.del({ url: '/system/user/delete/' + d.Id, name: d.RealName }); }
               }
           });
       });

   后端契约（沿用现有 ApiResult / PagedResult，无需改造）：
       列表  GET  → { code:200, message, data:{ Total, Items:[...] } }
       其它  POST → { code:200, message, data, success }
   ========================================================================== */
(function (window) {
    'use strict';

    var ems = {};
    var _ready = false;
    var _queue = [];
    var _autoFitReg = {};      // 记录需要自适应列宽的表格：{ id: { max, min } }
    var _autoFitTimer = null;  // 窗口 resize 防抖计时器
    var $, layer, table, form, laydate, upload, element, laytpl, dropdown;

    /* ======================================================================
       0. 启动
       ====================================================================== */

    layui.use(['table', 'form', 'layer', 'laydate', 'upload', 'element',
               'util', 'laytpl', 'dropdown', 'tree'], function () {
        $        = layui.$;
        layer    = layui.layer;
        table    = layui.table;
        form     = layui.form;
        laydate  = layui.laydate;
        upload   = layui.upload;
        element  = layui.element;
        laytpl   = layui.laytpl;
        dropdown = layui.dropdown;

        // 供遗留代码使用（layui 内置的就是完整版 jQuery）
        window.$ = window.jQuery = $;
        window.layer = layer;

        bindGlobalAjax();
        autoRenderDate();

        // 窗口缩放时，重新测量并适配所有「列宽自适应」表格（防抖，避免频繁重排）
        $(window).off('resize.emsAutoFit').on('resize.emsAutoFit', function () {
            if (_autoFitTimer) clearTimeout(_autoFitTimer);
            _autoFitTimer = setTimeout(function () {
                for (var tid in _autoFitReg) {
                    if (!_autoFitReg.hasOwnProperty(tid)) continue;
                    ems.autoFitColumns(tid, _autoFitReg[tid].max, _autoFitReg[tid].min);
                }
            }, 200);
        });

        _ready = true;
        for (var i = 0; i < _queue.length; i++) _queue[i]();
        _queue = [];
    });

    /** 注册页面初始化逻辑，确保 layui 模块已就绪 */
    ems.ready = function (fn) {
        if (typeof fn !== 'function') return;
        _ready ? fn() : _queue.push(fn);
    };

    /* ======================================================================
       1. 请求层：CSRF、统一错误处理
       ====================================================================== */

    /** 取防伪令牌（本页或主框架页） */
    function token() {
        var t = $('input[name="__RequestVerificationToken"]').first().val();
        if (!t && window.parent !== window) {
            try {
                t = window.parent.$('input[name="__RequestVerificationToken"]').first().val();
            } catch (e) { /* 跨域时忽略 */ }
        }
        return t || '';
    }

    function bindGlobalAjax() {
        $.ajaxSetup({ headers: { 'RequestVerificationToken': token() } });

        $(document).ajaxError(function (evt, xhr) {
            if (xhr.status === 401) {
                ems.msg('登录已过期，即将跳转登录页', { icon: 2, time: 1800 }, function () {
                    (window.top || window).location.href = '/Account/Login';
                });
            } else if (xhr.status === 403) {
                ems.error('您没有该操作的权限');
            }
        });
    }

    /**
     * 统一响应处理：成功走 onOk，失败弹错误。
     * 兼容 ApiResult 的 success / code 两种判定。
     */
    function handle(res, onOk, okMsg) {
        var ok = res && (res.success === true || res.code === 200);
        if (ok) {
            ems.success(okMsg || res.message || '操作成功', function () {
                if (onOk) onOk(res);
            });
        } else {
            ems.error((res && res.message) || '操作失败');
        }
    }
    ems.handle = handle;

    /** GET 请求 */
    ems.get = function (url, data, cb) {
        if (typeof data === 'function') { cb = data; data = null; }
        return $.get(url, data, function (res) {
            if (res && (res.success === true || res.code === 200)) {
                if (cb) cb(res.data, res);
            } else {
                ems.error((res && res.message) || '加载失败');
            }
        });
    };

    /**
     * POST 请求
     * @param {object} opt { url, data, json:是否以 JSON 提交, loading, okMsg, done, fail }
     */
    ems.post = function (opt) {
        var load = opt.loading === false ? null : ems.loading();
        var isJson = opt.json !== false;

        return $.ajax({
            url: opt.url,
            type: 'POST',
            contentType: isJson ? 'application/json' : 'application/x-www-form-urlencoded',
            data: isJson ? JSON.stringify(opt.data || {}) : (opt.data || {}),
            headers: { 'RequestVerificationToken': token() },
            success: function (res) {
                ems.close(load);
                var ok = res && (res.success === true || res.code === 200);
                if (ok) {
                    handle(res, opt.done, opt.okMsg);
                } else {
                    ems.error((res && res.message) || '操作失败');
                    if (opt.fail) opt.fail(res);
                }
            },
            error: function () {
                ems.close(load);
                ems.error('网络请求失败');
                if (opt.fail) opt.fail();
            }
        });
    };

    /* ======================================================================
       2. 提示层
       ====================================================================== */

    ems.msg     = function (m, o, cb) { return layer.msg(m, o, cb); };
    ems.success = function (m, cb) { return layer.msg(m || '操作成功', { icon: 1, time: 1200 }, cb); };
    ems.error   = function (m, cb) { return layer.msg(m || '操作失败', { icon: 2, time: 2000 }, cb); };
    ems.warn    = function (m, cb) { return layer.msg(m, { icon: 0, time: 2000 }, cb); };
    ems.alert   = function (m, cb) { return layer.alert(m, { icon: 0, title: '提示', skin: 'ems-layer' }, cb); };
    ems.loading = function () { return layer.load(2, { shade: [0.1, '#fff'] }); };
    ems.close   = function (i) { if (i !== null && i !== undefined) layer.close(i); };
    /* 关闭全部弹层（可选 type：'dialog' | 'page' | 'loading' | 'tips'，缺省全部） */
    ems.closeAll = function (type) { layer.closeAll(type); };

    /** 确认框：ems.confirm('确定？', function(){ ... }) */
    ems.confirm = function (msg, opt, yes) {
        if (typeof opt === 'function') { yes = opt; opt = {}; }
        opt = $.extend({ icon: 3, title: '请确认', skin: 'ems-layer' }, opt || {});
        return layer.confirm(msg, opt, function (index) {
            layer.close(index);
            if (yes) yes();
        });
    };

    /**
     * 输入框：ems.prompt({ title:'请输入', minLength:6 }, function(val){ ... })
     * formType: 0=文本 1=密码 2=多行
     */
    ems.prompt = function (opt, cb) {
        if (typeof opt === 'string') opt = { title: opt };
        var min = opt.minLength || 0;
        return layer.prompt({
            title: opt.title || '请输入',
            formType: opt.formType === undefined ? 0 : opt.formType,
            value: opt.value || '',
            skin: 'ems-layer'
        }, function (val, index) {
            if (min && val.length < min) {
                ems.error('至少需要 ' + min + ' 个字符');
                return;
            }
            layer.close(index);
            if (cb) cb(val);
        });
    };

    /**
     * 带版式的输入框弹窗：布局与 ems.confirm（删除/状态等弹窗）一致——
     * 居中图标 + 居中文本 + 留白输入框，避免文本框左贴边、下贴分割线。
     * @param {object} opt { title, icon(layui-icon 类名), msg, placeholder,
     *                        formType(0 文本 / 1 密码 / 2 多行), value, minLength,
     *                        btn:['确定','取消'] }
     * @param {function} cb 点击「确定」回调，参数为输入值
     */
    ems.promptDialog = function (opt, cb) {
        if (typeof opt === 'string') opt = { title: opt };
        opt = opt || {};
        var min = opt.minLength || 0;
        var uid = 'ems_pd_' + Date.now() + '_' + Math.floor(Math.random() * 1000);
        var inputHtml;
        if (opt.formType === 2) {
            inputHtml = '<textarea id="' + uid + '" class="layui-textarea" placeholder="' +
                ems.escapeHtml(opt.placeholder || '') + '"></textarea>';
        } else {
            var type = opt.formType === 1 ? 'password' : 'text';
            inputHtml = '<input type="' + type + '" id="' + uid + '" class="layui-input" placeholder="' +
                ems.escapeHtml(opt.placeholder || '') + '" value="' + ems.escapeHtml(opt.value || '') + '">';
        }
        var iconHtml = opt.icon ? '<div class="ems-prompt-icon"><i class="layui-icon ' + opt.icon + '"></i></div>' : '';
        var msgHtml = opt.msg ? '<div class="ems-prompt-msg">' + opt.msg + '</div>' : '';
        var headHtml = (opt.icon || opt.msg)
            ? '<div class="ems-prompt-head">' + iconHtml + msgHtml + '</div>'
            : '';
        return layer.open({
            type: 1,
            title: opt.title || '请输入',
            skin: 'ems-layer ems-layer-prompt2',
            area: opt.area || ['420px', ''],
            btn: opt.btn || ['确定', '取消'],
            content: '<div class="ems-prompt-box">' + headHtml +
                     '<div class="ems-prompt-field">' + inputHtml + '</div></div>',
            yes: function (index) {
                var el = document.getElementById(uid);
                var v = el ? el.value : '';
                if (min && v.length < min) { ems.error('至少需要 ' + min + ' 个字符'); return; }
                layer.close(index);
                if (cb) cb(v);
            }
        });
    };

    /* ======================================================================
       3. 表格
       ====================================================================== */

    /**
     * 把后端 ApiResult 翻译成 layui table 需要的结构。
     * 同时兼容两种返回：
     *   分页 data:{ Total, Items }   → 正常分页
     *   数组 data:[ ... ]            → 不分页（字典、子表等）
     */
    function parseData(res) {
        var d = res ? res.data : null;
        var paged = d && !Array.isArray(d) && typeof d === 'object' && ('Items' in d);
        return {
            code:  (res && res.code === 200) ? 0 : ((res && res.code) || 500),
            msg:   res ? res.message : '加载失败',
            count: paged ? d.Total : (Array.isArray(d) ? d.length : 0),
            data:  paged ? d.Items : (Array.isArray(d) ? d : [])
        };
    }
    ems.parseData = parseData;

    /**
     * 渲染表格。除 layui 原生 table 参数外，额外支持：
     *   tool     {key: fn}  行工具条事件（对应 lay-event）
     *   toolbar  {key: fn}  头部工具栏事件（对应 lay-event）；传对象时自动套默认模板
     *   pagesize            默认每页条数
     */
    ems.initTable = function (opt) {
        var id = opt.id || (opt.elem || '').replace('#', '');
        var toolEvents = opt.tool || {};
        var barEvents  = opt.toolbar || {};
        var hasToolbar = !!(opt.toolbarTpl || Object.keys(barEvents).length > 0 || opt.defaultToolbar);

        var conf = $.extend(true, {
            elem: opt.elem,
            id: id,
            url: opt.url,
            method: 'get',
            page: opt.page !== false,
            limit: opt.pagesize || 10,
            limits: [10, 20, 50, 100],
            even: true,
            size: 'sm',
            cellMinWidth: opt.cellMinWidth || 60,
            text: { none: '暂无数据' },
            // 后端 QueryDto 用的是 Page / Size，这里直接改名，免去后端适配
            request: { pageName: 'Page', limitName: 'Size' },
            parseData: parseData
        }, opt.extend || {});

        conf.cols = opt.cols;
        if (opt.where)  conf.where  = opt.where;
        if (opt.height) conf.height = opt.height;
        // 默认不强制高度：卡片高度随内容自适应，配合「去掉水平滚动条」要求

        if (hasToolbar) {
            conf.toolbar = opt.toolbarTpl || (opt.defaultToolbar ? '#emsToolbar' : null);
            conf.defaultToolbar = opt.defaultToolbar || ['filter', 'print', 'exports'];
        }

        // 列宽自适应：去掉固定列宽，按内容实际宽度分配，超宽则换行。
        // 记录到 _autoFitReg，便于窗口 resize 时重新测量。每次加载/重载 done 都会重算。
        if (opt.autoFit !== false) {
            _autoFitReg[id] = { max: opt.maxColWidth || 240, min: opt.minColWidth || 60 };
        } else {
            delete _autoFitReg[id];
        }

        // 卡片头「共 N 条」计数联动：表格加载后回填同卡片内的 [data-list-count]
        var userDone = opt.done;
        conf.done = function (res, curr, count) {
            try {
                var $card = $(opt.elem).closest('.ems-card');
                $card.find('[data-list-count]').text(count || 0);
            } catch (e) { /* ignore */ }
            if (opt.autoFit !== false) {
                ems.autoFitColumns(id, opt.maxColWidth, opt.minColWidth);
            }
            if (typeof userDone === 'function') userDone(res, curr, count);
        };

        var ins = table.render(conf);

        // 行工具条
        table.on('tool(' + id + ')', function (obj) {
            var fn = toolEvents[obj.event];
            if (fn) fn(obj.data, obj);
        });

        // 头部工具栏
        table.on('toolbar(' + id + ')', function (obj) {
            var fn = barEvents[obj.event];
            if (!fn) return;
            fn(table.checkStatus(id), obj);
        });

        // 单元格编辑 / 行双击等透传
        if (opt.onEdit) {
            table.on('edit(' + id + ')', function (obj) { opt.onEdit(obj.data, obj); });
        }
        if (opt.onRowDouble) {
            table.on('rowDouble(' + id + ')', function (obj) { opt.onRowDouble(obj.data, obj); });
        }

        ems._lastTableId = id;
        return ins;
    };

    /* ======================================================================
       3.1 列宽自适应：去掉固定列宽，按内容实际宽度分配，超过上限则在该列内换行
       ----------------------------------------------------------------------
       设计：
         · 不使用写死的 minWidth/width，而是测量每列「表头 + 各单元格」的真实
           内容宽度，取最大值作为该列宽度（clamp 到 [min, max]）。
         · 若各列宽之和小于容器宽度，把剩余空间均分给数据列，使表格铺满卡片、
           不留右侧空白。
         · 超过 max 的列由 CSS（.layui-table-cell 允许换行）自动折行显示。
         · 给单元格写「内联 width」，优先级高于 layui 自身样式，确保自适应结果
           在 layui 的 resize 之后依然生效；窗口 resize 时自动重新测量。
       ====================================================================== */
    ems.autoFitColumns = function (id, maxWidth, minWidth) {
        maxWidth = maxWidth || 240;
        minWidth = minWidth || 60;

        var view = $('.layui-table-view').filter(function () {
            return $(this).find('table[lay-id="' + id + '"]').length > 0;
        });
        if (!view.length) return;

        var $cols     = view.find('.layui-table-header colgroup col');
        var $bodyCols = view.find('.layui-table-body colgroup col');
        var $ths      = view.find('.layui-table-header thead th');
        var $rows     = view.find('.layui-table-body tbody tr');

        var box = view.find('.layui-table-box');
        var boxW = box.length ? box.width() : 0;
        if (!boxW) return;

        var cfg = null;
        try { cfg = table.getOptions(id); } catch (e) { /* ignore */ }

        var PAD = 30; // 单元格左右内边距估算，避免内容贴边
        var widths = [];

        $ths.each(function (idx) {
            var $th = $(this);

            // 序号 / 复选框 / 单选等 layui 特殊列：保持其自然宽度
            if ($th.hasClass('layui-table-col-special')) {
                widths[idx] = { w: $th.outerWidth() || 48, special: true };
                return;
            }
            // 被隐藏的列（筛选列关闭）：跳过
            if ($th.css('display') === 'none') { widths[idx] = null; return; }

            // 显式固定宽度的列（如「操作」工具列）：保持不动，不测量、不参与剩余分配
            var colDef = (cfg && cfg.cols && cfg.cols[0]) ? cfg.cols[0][idx] : null;
            if (colDef && colDef.width) {
                widths[idx] = { w: parseFloat(colDef.width) || 170, fixed: true };
                return;
            }

            var titleW = measure($th.find('.layui-table-cell').html()) + PAD;
            var maxBody = 0;
            $rows.each(function () {
                var $td = $(this).children('td').eq(idx);
                if (!$td.length) return;
                var w = measure($td.find('.layui-table-cell').html()) + PAD;
                if (w > maxBody) maxBody = w;
            });
            var cw = Math.max(titleW, maxBody);
            if (!isFinite(cw) || cw <= 0) cw = minWidth;
            cw = Math.min(Math.max(cw, minWidth), maxWidth);
            widths[idx] = { w: cw, special: false };
        });

        // 计算数据列总宽；若小于容器，把剩余空间均分给数据列，使表格铺满卡片
        var total = 0, dataCount = 0;
        for (var i = 0; i < widths.length; i++) {
            if (widths[i]) {
                total += widths[i].w;
                if (!widths[i].special && !widths[i].fixed) dataCount++;
            }
        }
        var leftover = boxW - total;
        if (leftover > 0 && dataCount > 0) {
            var add = leftover / dataCount;
            for (var j = 0; j < widths.length; j++) {
                if (widths[j] && !widths[j].special && !widths[j].fixed) widths[j].w += add;
            }
        }

        // 应用：写入 colgroup + 单元格内联宽度（内联优先级最高，覆盖 layui 默认）
        for (var k = 0; k < widths.length; k++) {
            var o = widths[k];
            if (!o) continue;
            var w = Math.round(o.w);
            if ($cols.eq(k).length)     $cols.eq(k).attr('width', w);
            if ($bodyCols.eq(k).length) $bodyCols.eq(k).attr('width', w);
            $ths.eq(k).find('.layui-table-cell').css('width', w);
            $rows.each(function () {
                $(this).children('td').eq(k).find('.layui-table-cell').css('width', w);
            });
            // 同步到 layui 配置，便于 reload 后保持（done 会再次测量，双保险）
            try {
                if (cfg && cfg.cols && cfg.cols[0] && cfg.cols[0][k] && !o.fixed) {
                    cfg.cols[0][k].width = w;
                }
            } catch (e) { /* ignore */ }
        }

        // 测量某段 HTML 在不换行时的真实宽度（避免受全局 white-space:normal 影响）
        function measure(html) {
            if (html == null) html = '';
            var $m = $('<span style="position:absolute;left:-9999px;top:-9999px;'
                    + 'white-space:nowrap;display:inline-block;font-size:14px;"></span>')
                    .html(html);
            $('body').append($m);
            var w = $m.outerWidth();
            $m.remove();
            return w || 0;
        }
    };

    /** 导出当前表格数据
     *  @param type 'xls' | 'csv'
     */
    ems.exportTable = function (id, filename, type) {
        id = id || ems._lastTableId;
        if (!id) return;
        var data = table.cache[id] || [];
        table.exportFile(id, data, type || 'xls');
    };

    /** 通用下拉面板：点击按钮后在按钮下方展开一个面板
     *  @param btn   按钮（jQuery 选择器或 DOM/jQuery）
     *  @param html  面板内容
     *  @param onShow($panel, $btn) 面板创建后的回调
     *  @param align 'left'（左对齐到按钮） | 'right'（右对齐，默认）
     */
    var _ddBtn = null;
    function _closeDropdown() {
        $('.ems-dropdown-panel').remove();
        _ddBtn = null;
        $(document).off('click.emsDropdown');
    }

    /** 通用下拉面板：点击按钮后在按钮下方展开一个面板（再次点击同按钮 = 收起；点击面板外任意处自动关闭） */
    ems._dropdown = function (btn, html, onShow, align) {
        var $btn = $(btn);
        // 再次点击同一按钮 → 收起
        if (_ddBtn && _ddBtn[0] === $btn[0]) { _closeDropdown(); return; }
        _closeDropdown();
        var $panel = $('<div class="ems-dropdown-panel"></div>').html(html);
        $btn.parent().css('position', 'relative').append($panel);
        if (align === 'left') $panel.css({ left: 0, right: 'auto' });
        else $panel.css({ right: 0, left: 'auto' });
        _ddBtn = $btn;
        if (typeof onShow === 'function') onShow($panel, $btn);
        // 面板内点击阻止冒泡，避免误触关闭（勾选等操作照常生效）
        $panel.on('click', function (e) { e.stopPropagation(); });
        // 延迟绑定外部点击关闭，避免本次「打开」点击立即触发关闭
        setTimeout(function () {
            $(document).on('click.emsDropdown', function (e) {
                if (!$panel.is(e.target) && $panel.has(e.target).length === 0 &&
                    !$btn.is(e.target) && $btn.has(e.target).length === 0) {
                    _closeDropdown();
                }
            });
        }, 0);
        return $panel;
    };

    /** 列筛选：下拉面板内勾选，实时隐藏/显示列（列名可见，左对齐到按钮） */
    ems.colFilter = function (id, cols, align) {
        id = id || ems._lastTableId;
        if (!id || !cols || !cols.length) return;
        var $btn = $('#btnFilter');
        if (!$btn.length) return;
        var html = '';
        cols.forEach(function (c) {
            if (!c.title || c.title === '操作') return;
            var field = c.field || '';
            var checked = c.hide ? '' : 'checked';
            html += '<label class="ems-col-item"><input type="checkbox" name="' + field +
                    '" ' + checked + '> ' + c.title + '</label>';
        });
        ems._dropdown($btn, html, function ($panel) {
            $panel.find('input[type=checkbox]').on('change', function () {
                table.hideCol(id, { field: this.name, hide: !this.checked });
                // 列显隐变化后重新适配列宽（剩余空间重新分配）
                var reg = _autoFitReg[id];
                ems.autoFitColumns(id, reg ? reg.max : 240, reg ? reg.min : 60);
            });
        }, align || 'left');
    };

    /** 导出：下拉二次选择（范围 × 格式），右对齐到按钮 */
    ems.exportMenu = function (id, filename, align) {
        id = id || ems._lastTableId;
        if (!id) return;
        var $btn = $('#btnExport');
        if (!$btn.length) return;
        var html =
            '<div class="ems-dropdown-item" data-act="page" data-type="xls">导出当前页 (Excel)</div>' +
            '<div class="ems-dropdown-item" data-act="page" data-type="csv">导出当前页 (CSV)</div>' +
            '<div class="ems-dropdown-item" data-act="all"  data-type="xls">导出全部 (Excel)</div>' +
            '<div class="ems-dropdown-item" data-act="all"  data-type="csv">导出全部 (CSV)</div>';
        ems._dropdown($btn, html, function ($panel) {
            $panel.find('[data-act]').on('click', function () {
                var act  = $(this).data('act');
                var type = $(this).data('type');
                if (act === 'all') {
                    var count = table.getOptions ? (table.getOptions(id) || {}).count : 0;
                    if (count && count > (table.cache[id] || []).length) {
                        ems.msg('全量导出接口待接入，已导出当前页');
                    }
                }
                ems.exportTable(id, filename, type);
                $panel.remove();
            });
        }, align || 'right');
    };

    /** 重载表格（不传 id 时刷新页面上最近一次渲染的表格） */
    ems.reloadTable = function (id, where, resetPage) {
        id = id || ems._lastTableId;
        if (!id) return;
        var o = {};
        if (where) o.where = where;
        if (resetPage !== false) o.page = { curr: 1 };
        table.reloadData(id, o);
    };

    /** 供 iframe 子页调用：刷新父框架里的表格 */
    ems.reloadParentTable = function (id) {
        try {
            if (window.parent && window.parent.layui && window.parent.layui.table) {
                window.parent.layui.table.reloadData(id, {});
            }
        } catch (e) { /* ignore */ }
    };

    /**
     * 搜索：读取搜索表单的值，作为查询条件重载表格
     * @param {object} opt { form:表单 lay-filter, table:表格 id, extra:附加条件 }
     */
    ems.search = function (opt) {
        opt = opt || {};
        var vals = opt.form ? form.val(opt.form) : {};
        delete vals[''];
        ems.reloadTable(opt.table, $.extend({}, vals, opt.extra || {}));
    };

    /**
     * 重置：清空搜索表单并重载表格
     * @param {object} opt { form:表单 lay-filter, elem:表单选择器, table:表格 id }
     */
    ems.reset = function (opt) {
        opt = opt || {};
        var sel = opt.elem || ('form[lay-filter="' + opt.form + '"]');
        var $f = $(sel);
        if ($f.length) {
            $f[0].reset();
            $f.find('input[type=hidden]').not('[name="__RequestVerificationToken"]').val('');
            form.render(null, opt.form);
        }
        ems.reloadTable(opt.table, {});
    };

    /**
     * 一行绑定「搜索 + 重置」：
     *   ems.bindSearch('userSearch', 'userTable')
     * 搜索按钮写 lay-submit lay-filter="userSearch"，
     * 重置按钮写 type="reset" 并加 class="ems-reset" 即可，无需额外代码。
     */
    ems.bindSearch = function (filter, tableId, extra) {
        form.on('submit(' + filter + ')', function (data) {
            ems.reloadTable(tableId, $.extend({}, data.field, extra || {}));
            return false;
        });

        // 原生 reset 会在事件之后才清空，故延迟一帧再取值重载
        $('form[lay-filter="' + filter + '"]').on('click', '.ems-reset, [type=reset]', function () {
            setTimeout(function () {
                ems.reset({ form: filter, table: tableId });
            }, 0);
        });
    };

    /** 取表格勾选行；未勾选时提示 */
    ems.checked = function (id, silent) {
        var rows = table.checkStatus(id || ems._lastTableId).data;
        if (!rows.length && !silent) { ems.warn('请先勾选数据'); return null; }
        return rows;
    };

    /* ======================================================================
       4. 弹层（iframe 表单页）
       ====================================================================== */

    /**
     * 通用 iframe 弹层
     * @param {object} opt { url, title, area, full, end, table:关闭后要刷新的表格 }
     */
    /**
     * 通用弹层（type:2 iframe）。
     * - 默认（详情 / 分配权限 / 小提醒窗）：量内容自然宽 + iframeAuto 量高 + 手动重算居中；适合内容不定、需垂直居中的场景。
     * - 固定顶模式（新增 / 编辑，pinTop=true）：固定宽 + 高度随内容 + 顶部钉死（offset），
     *   高度向下生长永不偏下；适合含异步加载的表单类弹窗。
     */
    ems.open = function (opt) {
        var url = opt.url;
        url += (url.indexOf('?') > -1 ? '&' : '?') + '_dialog=1';

        var pinned = !!opt.pinTop;
        var area = opt.area ? opt.area.slice() : (pinned ? ['860px', 'auto'] : ['860px', '620px']);
        if (pinned && Array.isArray(area)) area[1] = 'auto';   // 固定顶：高度随内容向下生长
        // 小屏自动铺满，避免表单被裁切
        if ($(window).width() < 768) area = ['100%', '100%'];

        var conf = {
            type: 2,
            title: opt.title || '',
            skin: 'ems-layer',
            area: area,
            maxmin: opt.maxmin !== false,
            shadeClose: false,
            content: url,
            success: function (layero, index) {
                var ifr = layero.find('iframe')[0];

                // iframe 内容（含 layui 组件渲染）加载完成后再量高，避免打开瞬间内容未就绪导致偏下。
                var fit = function () {
                    // 1) 量真实高（layui 只改高，不重算 top）
                    layer.iframeAuto(index);
                    var titleH = layero.find('.layui-layer-title').outerHeight() || 0;
                    var curH = layero.outerHeight();
                    var vpH = $(window).height();
                    var topGap = pinned ? 100 : 20;            // 固定顶：上 80 + 下 20
                    var maxH = Math.max(vpH - topGap, 200);
                    if (curH > maxH) {                         // 超高：限高 + iframe 内滚动
                        var ifrH = maxH - titleH;
                        layer.style(index, { height: maxH + 'px' });
                        ifr.style.height = ifrH + 'px';
                        if (ifr.contentDocument) ifr.contentDocument.body.style.overflow = 'auto';
                    }
                    // 2) 居中模式（非 pinTop）：量内容自然宽 → 收窄弹窗 → 重算水平+垂直居中
                    if (!pinned) {
                        try {
                            var win = ifr.contentWindow, doc = win && win.document;
                            if (doc && doc.body) {
                                var form = doc.querySelector('.layui-form') ||
                                           doc.querySelector('.ems-page') || doc.body;
                                doc.body.style.margin = '0';
                                if (form) {
                                    var prevW = form.style.width;
                                    form.style.width = 'max-content';
                                    var naturalW = form.offsetWidth;
                                    form.style.width = prevW;
                                    if (naturalW && naturalW >= 300) {
                                        var finalW = Math.min(Math.max(naturalW + 24, 460), $(window).width() - 40);
                                        layer.style(index, { width: finalW + 'px' });
                                    }
                                }
                            }
                        } catch (e) {}
                    }

                    // 3) 关键：layui 的 iframeAuto / layer.style 只改尺寸、不重算定位。
                    //     固定顶(pinTop)：top 钉死但 left 可能因内容宽度与 area 不匹配而偏移，需重算水平居中。
                    //     居中模式：改宽后 left/top 都需重算。
                    layero.css('left', Math.max(0, ($(window).width() - layero.outerWidth()) / 2) + 'px');
                    if (!pinned) {
                        layero.css('top', Math.max(0, (vpH - layero.outerHeight()) / 2) + 'px');
                    }
                };

                var doc0 = ifr.contentDocument;
                if (doc0 && doc0.readyState === 'complete') {
                    fit();
                } else {
                    ifr.onload = fit;
                    setTimeout(fit, 3000); // 兜底：onload 未触发时 3s 后强制量一次
                }

                if (opt.success) opt.success(layero, index);
            },
            end: function () {
                if (opt.table) ems.reloadTable(opt.table, null, false);
                if (opt.end) opt.end();
            }
        };

        // 固定顶模式：钉死顶部（距顶 80px）、水平居中；高度随内容向下生长。
        if (pinned) conf.offset = ['80px', 'center'];

        return layer.open(conf);
    };

    /** 新增弹层（固定顶模式：pinTop） */
    ems.openAdd = function (opt) {
        return ems.open($.extend({ title: '新增', pinTop: true }, opt));
    };

    /** 编辑弹层（固定顶模式：pinTop） */
    ems.openEdit = function (opt) {
        return ems.open($.extend({ title: '编辑', pinTop: true }, opt));
    };

    /** 只读详情弹层 */
    ems.openDetail = function (opt) {
        return ems.open($.extend({ title: '详情', maxmin: true }, opt));
    };

    /** 在主框架中打开一个内容页（单页模式：直接替换当前内容区，不保留标签） */
    ems.openTab = function (url, title) {
        var top = window.top;
        if (top && top.emsOpenTab) top.emsOpenTab(url, title);
        else window.location.href = url;
    };

    /** 表单页关闭自己；传 tableId 则同时刷新父页表格 */
    ems.closeSelf = function (tableId) {
        if (tableId) ems.reloadParentTable(tableId);
        try {
            var idx = window.parent.layer.getFrameIndex(window.name);
            window.parent.layer.close(idx);
        } catch (e) {
            history.back();
        }
    };

    /* ======================================================================
       5. 表单
       ====================================================================== */

    /** 重新渲染表单（select / checkbox / radio 动态变更后必须调用） */
    ems.renderForm = function (filter, type) {
        form.render(type || null, filter);
    };

    /** 回填表单数据；data 为后端 DTO（PascalCase） */
    ems.fillForm = function (filter, data) {
        if (!data) return;
        form.val(filter, data);
        form.render(null, filter);
    };

    /**
     * 远程填充下拉框
     * @param {object} opt {
     *   elem:  select 选择器
     *   url:   数据接口
     *   value: 取值字段名（默认 Id）
     *   text:  显示字段名（默认 Name）
     *   empty: 空选项文案，传 false 不加
     *   selected: 默认选中值
     *   filter: 表单 lay-filter，用于渲染
     *   done:  填充完成回调
     * }
     */
    ems.loadSelect = function (opt) {
        var $sel = $(opt.elem);
        if (!$sel.length) return;

        ems.get(opt.url, opt.query || null, function (list) {
            var rows = Array.isArray(list) ? list : (list && list.Items) || [];
            var vf = opt.value || 'Id';
            var tf = opt.text  || 'Name';
            var html = opt.empty === false ? '' :
                '<option value="">' + (opt.empty || '请选择') + '</option>';

            for (var i = 0; i < rows.length; i++) {
                var r = rows[i];
                var v = r[vf];
                var t = (typeof tf === 'function') ? tf(r) : r[tf];
                var sel = (opt.selected !== undefined && opt.selected !== null && opt.selected !== ''
                           && String(opt.selected) === String(v)) ? ' selected' : '';
                var dis = '';
                if (typeof opt.disabled === 'function') dis = opt.disabled(r) ? ' disabled' : '';
                else if (r.Disabled) dis = ' disabled';

                html += '<option value="' + v + '"' + sel + dis + '>' + ems.escapeHtml(t) + '</option>';
            }
            $sel.html(html);
            ems.renderForm(opt.filter, 'select');
            if (opt.done) opt.done(rows);
        });
    };

    /**
     * 提交表单
     * @param {object} opt {
     *   url, data, json, okMsg,
     *   closeSelf: 成功后关闭弹层（默认 true）
     *   table:     成功后刷新的父页表格 id
     *   done:      自定义成功回调（传了就不走默认关闭逻辑）
     * }
     */
    ems.save = function (opt) {
        return ems.post({
            url: opt.url,
            data: opt.data,
            json: opt.json,
            okMsg: opt.okMsg,
            fail: opt.fail,
            done: function (res) {
                if (opt.done) { opt.done(res); return; }
                if (opt.closeSelf === false) {
                    if (opt.table) ems.reloadTable(opt.table);
                } else {
                    ems.closeSelf(opt.table);
                }
            }
        });
    };

    /**
     * 绑定 layui 表单提交（lay-submit + lay-filter）
     * ems.bindSubmit('userForm', function(field){ return { url:'...', data:field }; })
     * builder 收到的是已处理过的 field 对象，返回 ems.save 的参数。
     */
    ems.bindSubmit = function (filter, builder) {
        form.on('submit(' + filter + ')', function (data) {
            var conf = builder(data.field, data);
            if (conf) ems.save(conf);
            return false;
        });
    };

    /** 空字符串转 null，其余转数字——用于可空外键字段（long?） */
    ems.toNum = function (v) {
        if (v === '' || v === null || v === undefined) return null;
        var n = Number(v);
        return isNaN(n) ? null : n;
    };

    /**
     * 批量规整表单字段，避免 "" 传给后端的 long? / int? 造成模型绑定失败
     * ems.nums(field, ['DeptId','PostId','EmployeeId'])
     */
    ems.nums = function (field, keys) {
        (keys || []).forEach(function (k) { field[k] = ems.toNum(field[k]); });
        return field;
    };

    /** 收集同名复选框的值（layui 原生 checkbox 组），返回数字数组 */
    ems.checkboxValues = function (name, asNumber) {
        var arr = [];
        $('input[type=checkbox][name="' + name + '"]:checked').each(function () {
            arr.push(asNumber === false ? this.value : Number(this.value));
        });
        return arr;
    };

    /* ======================================================================
       6. 删除
       ====================================================================== */

    /**
     * 删除单条
     * @param {object} opt { url, name:用于提示的名称, table, json, done, msg }
     */
    ems.del = function (opt) {
        var msg = opt.msg ||
            ('确认删除' + (opt.name ? ' <b>' + ems.escapeHtml(opt.name) + '</b> ' : '该记录') +
             '？此操作不可恢复！');

        ems.confirm(msg, { icon: 3, title: '警告', btn: ['确认删除', '取消'] }, function () {
            ems.post({
                url: opt.url,
                data: opt.data || {},
                json: opt.json === true,
                okMsg: opt.okMsg,
                done: function (res) {
                    if (opt.done) opt.done(res);
                    else ems.reloadTable(opt.table, null, false);
                }
            });
        });
    };

    /**
     * 批量删除：自动取勾选行的主键
     * @param {object} opt { url, table, key:主键字段(默认 Id), field:提交字段名(默认 ids) }
     */
    ems.batchDel = function (opt) {
        var rows = ems.checked(opt.table);
        if (!rows) return;

        var key = opt.key || 'Id';
        var ids = rows.map(function (r) { return r[key]; });
        var data = {};
        data[opt.field || 'ids'] = ids;

        ems.confirm('确认删除选中的 <b>' + ids.length + '</b> 条记录？此操作不可恢复！',
            { icon: 3, title: '警告', btn: ['确认删除', '取消'] }, function () {
                ems.post({
                    url: opt.url,
                    data: data,
                    json: opt.json !== false,
                    done: function () { ems.reloadTable(opt.table, null, false); }
                });
            });
    };

    /**
     * 批量删除（后端没有批量接口时用）：对每一条勾选记录逐个调用删除接口
     * @param {object} opt {
     *   table, key(默认 Id), name(用于提示的字段，默认 RealName/Name),
     *   url: function(row) 返回该行的删除接口地址
     * }
     */
    ems.batchDelEach = function (opt) {
        var rows = ems.checked(opt.table);
        if (!rows) return;

        var key = opt.key || 'Id';
        ems.confirm('确认删除选中的 <b>' + rows.length + '</b> 条记录？此操作不可恢复！',
            { icon: 3, title: '警告', btn: ['确认删除', '取消'] }, function () {
                var load = ems.loading();
                var done = 0, failed = 0;

                rows.forEach(function (row) {
                    $.ajax({
                        url: typeof opt.url === 'function' ? opt.url(row) : (opt.url + row[key]),
                        type: 'POST',
                        headers: { 'RequestVerificationToken': token() },
                        complete: function (xhr) {
                            var res = xhr.responseJSON;
                            if (!res || !(res.success === true || res.code === 200)) failed++;
                            if (++done === rows.length) {
                                ems.close(load);
                                if (failed) ems.error('完成，其中 ' + failed + ' 条删除失败');
                                else ems.success('删除成功');
                                ems.reloadTable(opt.table, null, false);
                            }
                        }
                    });
                });
            });
    };

    /** 启用 / 禁用等状态切换 */
    ems.setStatus = function (opt) {
        var tip = opt.confirm || ('确定' + (opt.status === 1 ? '启用' : '禁用') + '该记录？');
        ems.confirm(tip, function () {
            ems.post({
                url: opt.url,
                data: opt.data,
                json: opt.json === true,
                done: function () { ems.reloadTable(opt.table, null, false); }
            });
        });
    };

    /* ======================================================================
       7. 上传
       ====================================================================== */

    /**
     * 文件上传
     * @param {object} opt {
     *   elem:   触发按钮选择器
     *   url:    上传接口
     *   accept: file/images/video/audio
     *   exts:   允许的后缀
     *   multiple, size(KB), field(表单字段名，默认 file)
     *   data:   附加参数（对象或返回对象的函数）
     *   done:   成功回调 (data, res)
     *   table:  成功后刷新的表格
     * }
     */
    ems.upload = function (opt) {
        var load;
        return upload.render({
            elem: opt.elem,
            url: opt.url,
            accept: opt.accept || 'file',
            exts: opt.exts,
            field: opt.field || 'file',
            multiple: opt.multiple === true,
            number: opt.number || 0,
            size: opt.size || 0,
            acceptMime: opt.acceptMime,
            headers: { 'RequestVerificationToken': token() },
            data: opt.data,
            before: function () {
                load = ems.loading();
                if (opt.before) opt.before.apply(this, arguments);
            },
            done: function (res) {
                ems.close(load);
                handle(res, function () {
                    if (opt.done) opt.done(res.data, res);
                    else ems.reloadTable(opt.table, null, false);
                }, opt.okMsg || '上传成功');
            },
            error: function () {
                ems.close(load);
                ems.error('上传失败');
                if (opt.error) opt.error();
            }
        });
    };

    /** 下载（走表单提交，避免 window.open 被拦截） */
    ems.download = function (url) {
        var f = $('<form method="get" target="_blank"></form>').attr('action', url);
        $('body').append(f);
        f.submit();
        f.remove();
    };

    /**
     * 文件在线预览：图片 / PDF 等浏览器可内联渲染的类型直接在弹层里预览；
     * doc / xls / zip 等不支持内联的类型提示下载。
     * @param {string} url   文件下载地址（方法会自动补 ?inline=1 触发后端内联返回）
     * @param {string} title 弹层标题（默认“文件预览”）
     */
    ems.preview = function (url, title) {
        if (!url) return;
        var sep = url.indexOf('?') >= 0 ? '&' : '?';
        var previewUrl = url + sep + 'inline=1';
        var ext = (url.split('?')[0].split('.').pop() || '').toLowerCase();
        var noInline = ['doc', 'docx', 'xls', 'xlsx', 'ppt', 'pptx', 'zip', 'rar', '7z'];
        if (noInline.indexOf(ext) >= 0) {
            ems.confirm('该文件类型（.' + ext + '）不支持在线预览，是否直接下载查看？',
                { icon: 3, btn: ['下载', '取消'] }, function (i) {
                    ems.close(i);
                    ems.download(url.replace(/[?&]inline=1$/, ''));
                });
            return;
        }
        layer.open({
            type: 1, title: title || '文件预览', skin: 'ems-layer', area: ['82%', '90%'],
            content: '<div style="width:100%;height:100%;background:#3a3d42">' +
                     '<iframe src="' + previewUrl + '" style="width:100%;height:100%;border:0;background:#fff"></iframe></div>',
            success: function (layero) { $(layero).find('.layui-layer-content').css('overflow', 'hidden'); }
        });
    };

    /* ======================================================================
       8. 日期
       ====================================================================== */

    /** 自动渲染带 data-date / data-daterange 属性的输入框，页面无需写初始化代码 */
    function autoRenderDate() {
        $('[data-date]').each(function () {
            laydate.render({ elem: this, type: $(this).data('date') || 'date', trigger: 'click' });
        });
        $('[data-daterange]').each(function () {
            laydate.render({
                elem: this,
                type: $(this).data('daterange') || 'date',
                range: '~',
                trigger: 'click'
            });
        });
    }
    ems.renderDate = autoRenderDate;

    /* ======================================================================
       9. 模板 / 格式化工具
       ====================================================================== */

    ems.escapeHtml = function (s) {
        if (s === null || s === undefined) return '';
        return String(s)
            .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;').replace(/'/g, '&#39;');
    };

    ems.fmtDate = function (d) {
        if (!d) return '-';
        var t = new Date(d);
        if (isNaN(t.getTime())) return '-';
        return t.getFullYear() + '-' +
            String(t.getMonth() + 1).padStart(2, '0') + '-' +
            String(t.getDate()).padStart(2, '0');
    };

    ems.fmtDateTime = function (d) {
        if (!d) return '-';
        var t = new Date(d);
        if (isNaN(t.getTime())) return '-';
        return ems.fmtDate(d) + ' ' +
            String(t.getHours()).padStart(2, '0') + ':' +
            String(t.getMinutes()).padStart(2, '0');
    };

    ems.fmtMoney = function (v, unit) {
        if (v === null || v === undefined || v === '') return '-';
        var n = parseFloat(v);
        if (isNaN(n)) return '-';
        return '¥' + n.toFixed(2) + (unit || '');
    };

    /** 表格 templet 快捷生成器 */
    ems.tpl = {
        /** 状态徽章：ems.tpl.badge('Status', {1:['正常','green'], 0:['禁用','red']}) */
        badge: function (field, map, def) {
            return function (d) {
                var hit = map[d[field]];
                if (!hit) return def || '-';
                return '<span class="layui-badge layui-bg-' + hit[1] + '">' +
                       ems.escapeHtml(hit[0]) + '</span>';
            };
        },
        /** 单个标签：有值显示彩色徽章，无值显示 - */
        tag: function (field, color) {
            return function (d) {
                var v = d[field];
                if (v === null || v === undefined || v === '') return '-';
                return '<span class="layui-badge layui-bg-' + (color || 'green') + '">' +
                       ems.escapeHtml(v) + '</span>';
            };
        },
        /** 多个标签：ems.tpl.tags('RoleNames') */
        tags: function (field, color) {
            return function (d) {
                var arr = d[field];
                if (!arr || !arr.length) return '-';
                return arr.map(function (t) {
                    return '<span class="layui-badge layui-bg-' + (color || 'blue') + '">' +
                           ems.escapeHtml(t) + '</span>';
                }).join(' ');
            };
        },
        date:     function (field) { return function (d) { return ems.fmtDate(d[field]); }; },
        dateTime: function (field) { return function (d) { return ems.fmtDateTime(d[field]); }; },
        money:    function (field, unit) { return function (d) { return ems.fmtMoney(d[field], unit); }; },
        /** 空值占位 */
        text:     function (field) { return function (d) { return ems.escapeHtml(d[field]) || '-'; }; }
    };

    /** 读取 URL 查询参数 */
    ems.query = function (name) {
        var m = new RegExp('[?&]' + name + '=([^&]*)').exec(location.search);
        return m ? decodeURIComponent(m[1]) : '';
    };

    /** 今天日期字符串（yyyy-MM-dd），用于表单默认值 */
    ems.today = function () {
        var d = new Date();
        var m = ('0' + (d.getMonth() + 1)).slice(-2);
        var day = ('0' + d.getDate()).slice(-2);
        return d.getFullYear() + '-' + m + '-' + day;
    };

    window.ems = ems;

})(window);
