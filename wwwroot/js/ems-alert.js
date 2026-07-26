/*
 * ems-alert.js — 全站统一的弹窗/提醒封装
 * 底层仍使用 layer（保证外观一致），但所有页面统一通过 window.ems.* 调用，
 * 后续若要换皮肤/换库，只改这里即可，不用逐个页面改。
 */
(function () {
    // 统一 layer 默认外观（动画、时长）
    if (typeof layer !== 'undefined') {
        try { layer.config({ anim: 2, time: 2000, shade: [0.3, '#000'] }); } catch (e) { }
    }

    // 内部取全局 layer（含 _Layout 中的降级 fallback）
    function L() { return (typeof layer !== 'undefined') ? layer : window.layer; }

    var ems = {
        msg: function (text, opts) { return L().msg(text, opts); },
        success: function (text, opts) { return L().msg(text, Object.assign({ icon: 1, time: 1800 }, opts || {})); },
        error: function (text, opts) { return L().msg(text, Object.assign({ icon: 2, time: 2500 }, opts || {})); },
        warn: function (text, opts) { return L().msg(text, Object.assign({ icon: 7, time: 2500 }, opts || {})); },
        info: function (text, opts) { return L().msg(text, Object.assign({ icon: 6, time: 1800 }, opts || {})); },
        alert: function (text, opts) { return L().alert(text, opts); },
        confirm: function (text, opts, yes, no) { return L().confirm(text, opts, yes, no); },
        prompt: function (opts, cb) { return L().prompt(opts, cb); },
        load: function (text) { return L().load(1, { shade: [0.3, '#000'], content: text }); },
        close: function (i) { return L().close(i); },
        closeAll: function () { return L().closeAll(); },
        open: function (o) { return L().open(o); }
    };

    window.ems = ems;
})(());
