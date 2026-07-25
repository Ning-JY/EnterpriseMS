// EnterpriseMS 全局 JS

// ── 全局 HTML 转义（防 XSS）──────────────────────────────
// 视图内拼接 DB 来源文本到 innerHTML / .html() 时必须经过本函数。
// 已加载的视图若自带同名函数，会覆盖此全局定义（逻辑一致，无冲突）。
function escapeHtml(str) {
    if (str === null || str === undefined) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

// ── fetch 请求自动附加 CSRF Token ──────────────────────────
// 与 jQuery $.ajaxSetup（下方）保持一致：均使用 RequestVerificationToken 头。
// 服务端已开启全局 AutoValidateAntiforgeryToken，未带 token 的非安全请求会 400。
(function () {
    var originalFetch = window.fetch;
    window.fetch = function (url, options) {
        options = options || {};
        var method = (options.method || 'GET').toUpperCase();
        if (method === 'POST' || method === 'PUT' || method === 'DELETE' || method === 'PATCH') {
            var token = $('input[name="__RequestVerificationToken"]').val();
            if (token) {
                options.headers = options.headers || {};
                if (options.headers instanceof Headers) {
                    options.headers.set('RequestVerificationToken', token);
                } else {
                    options.headers['RequestVerificationToken'] = token;
                }
            }
        }
        return originalFetch.apply(this, arguments);
    };
})();

$(function () {
    // ── 全局 CSRF Token ─────────────────────────────────────
    var token = $('input[name="__RequestVerificationToken"]').first().val();
    if (token) {
        $.ajaxSetup({ headers: { 'RequestVerificationToken': token } });
    }

    // ── Ajax 全局错误 ────────────────────────────────────────
    $(document).ajaxError(function (event, xhr) {
        if (xhr.status === 401) {
            layer.msg('登录已过期', { icon: 2, time: 2000 }, function () {
                window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
            });
        } else if (xhr.status === 403) {
            layer.msg('您没有操作权限', { icon: 2 });
        }
    });

    // ── 可折叠卡片 ──────────────────────────────────────────
    $(document).on('click', '[data-card-widget="collapse"]', function () {
        var card = $(this).closest('.card');
        var body = card.find('.card-body');
        var icon = $(this).find('i');
        body.is(':visible')
            ? (body.slideUp(200), icon.removeClass('fa-minus').addClass('fa-plus'))
            : (body.slideDown(200), icon.removeClass('fa-plus').addClass('fa-minus'));
    });

    // ── 通知中心：事件委托 ─────────────────────────────────
    // 不依赖内联 onclick 调全局函数（避免缓存/作用域导致的 ReferenceError），
    // 同时兼容页眉铃铛与列表页动态渲染的项。
    $(document).on('click', 'a[data-notif-id]', function (e) {
        var $a = $(this);
        var id = $a.data('notif-id');
        var link = $a.data('notif-link') || '';
        if (id) markNotificationRead(id, link);
        // 无真实跳转链接时阻止默认行为
        if (!link || link.indexOf('javascript:') === 0 || link === '#') {
            e.preventDefault();
        }
    });

    $(document).on('click', '[data-notif-mark-all]', function (e) {
        e.preventDefault();
        markAllNotificationsRead();
    });
});

// 通用确认删除
function confirmDelete(url, name, cb) {
    layer.confirm('确认删除 <b>' + (name || '该记录') + '</b>？',
        { icon: 3, title: '警告', btn: ['确认删除', '取消'] },
        function (i) {
            layer.close(i);
            var load = layer.load(1);
            $.post(url, function (r) {
                layer.close(load);
                handleResult(r, function () { if (cb) cb(); else setTimeout(function(){ location.reload(); }, 1500); });
            }).fail(function () { layer.close(load); layer.msg('请求失败', { icon: 2 }); });
        });
}

// 通用 JSON POST
function ajaxPost(url, data, successCb, errorCb) {
    var load = layer.load(1);
    $.ajax({
        url: url, type: 'POST', contentType: 'application/json', data: JSON.stringify(data),
        success: function (r) {
            layer.close(load);
            if (r && r.success) { if (successCb) successCb(r); else layer.msg(r.message || '操作成功', { icon: 1 }); }
            else { if (errorCb) errorCb(r); else layer.msg((r && r.message) || '操作失败', { icon: 2 }); }
        },
        error: function () {
            layer.close(load);
            if (errorCb) errorCb({ message: '网络请求失败' }); else layer.msg('网络请求失败', { icon: 2 });
        }
    });
}

// 处理响应结果并弹窗
function handleResult(r, onSuccess) {
    if (r && r.success) {
        layer.msg(r.message || '操作成功', { icon: 1, time: 1500 }, function () { if (onSuccess) onSuccess(r); });
    } else {
        layer.msg((r && r.message) || '操作失败', { icon: 2 });
    }
}

function fmtMoney(val) { return val != null ? '¥ ' + parseFloat(val).toFixed(2) + ' 万' : '—'; }
function fmtDate(d) {
    if (!d) return '—';
    var dt = new Date(d); if (isNaN(dt.getTime())) return '—';
    return dt.getFullYear() + '-' + String(dt.getMonth()+1).padStart(2,'0') + '-' + String(dt.getDate()).padStart(2,'0');
}

// ── 通知中心 ────────────────────────────────────────────────
// 标记单条通知已读。link 参数保留以兼容铃铛内联调用；
// 使用 keepalive 确保页面跳转时请求仍完成。
function markNotificationRead(id, link) {
    if (!id) return;
    fetch('/notifications/mark-read?id=' + id, {
        method: 'POST',
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        keepalive: true
    }).then(function () {
        // 视觉上更新相关元素（铃铛下拉 + 列表页）
        document.querySelectorAll('[data-notif-id="' + id + '"]').forEach(function (el) {
            el.classList.add('is-read', 'text-muted');
            el.classList.remove('font-weight-bold');
            var dot = el.querySelector('.notif-dot'); if (dot) dot.remove();
        });
        // 页眉徽标 -1
        var badge = document.querySelector('.navbar-badge');
        if (badge) {
            var n = parseInt(badge.textContent, 10) - 1;
            if (isNaN(n) || n <= 0) badge.remove();
            else badge.textContent = n > 99 ? '99+' : n;
        }
    }).catch(function () { /* 已读失败不影响导航 */ });
}

// 全部标为已读
function markAllNotificationsRead() {
    var load = layer.load(1);
    fetch('/notifications/mark-all-read', {
        method: 'POST',
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
    }).then(function () {
        layer.close(load);
        location.reload();
    }).catch(function () {
        layer.close(load);
        layer.msg('操作失败', { icon: 2 });
    });
}

