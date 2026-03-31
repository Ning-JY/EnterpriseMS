// EnterpriseMS 全局 JS

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
