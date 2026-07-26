/* ============================================================
   企业管理系统 · 多选下拉组件 (jQuery)
   依赖：jQuery（站点已全局加载）
   用法：$('#mySelect[multiple]').emsMultiselect();
   说明：保留原生 <select multiple> 作为数据载体，saveUser 等
        既有的 $('#id').val() 逻辑无需改动；本组件只负责
        把「平铺列表框」换成「下拉 checkbox」体验。
   ============================================================ */
(function ($) {
    'use strict';

    $.fn.emsMultiselect = function () {
        return this.each(function () {
            var $sel = $(this);
            if (!$sel.is('select[multiple]') || $sel.data('ems-ms')) return;
            $sel.data('ems-ms', true);
            $sel.hide();

            var $wrap = $(
                '<div class="ems-ms">' +
                '  <button type="button" class="ems-ms-btn placeholder">请选择</button>' +
                '  <div class="ems-ms-panel" style="display:none">' +
                '    <input type="text" class="ems-ms-search" placeholder="搜索...">' +
                '  </div>' +
                '</div>'
            );
            $sel.after($wrap);

            var $btn = $wrap.find('.ems-ms-btn');
            var $panel = $wrap.find('.ems-ms-panel');
            var $search = $wrap.find('.ems-ms-search');

            function selectedOptions() {
                return $sel.find('option:selected');
            }

            function updateBtn() {
                var sel = selectedOptions();
                if (sel.length === 0) {
                    $btn.text('请选择').addClass('placeholder');
                } else if (sel.length === 1) {
                    $btn.text(sel.first().text()).removeClass('placeholder');
                } else {
                    $btn.text('已选 ' + sel.length + ' 项').removeClass('placeholder');
                }
            }

            function renderItems(filter) {
                $panel.find('.ems-ms-item, .ems-ms-empty').remove();
                var opts = $sel.find('option');
                var shown = 0;
                opts.each(function () {
                    var $o = $(this);
                    var text = $o.text();
                    if (filter && text.indexOf(filter) === -1) return;
                    shown++;
                    var $item = $(
                        '<label class="ems-ms-item">' +
                        '<input type="checkbox" ' + ($o.prop('selected') ? 'checked' : '') + '>' +
                        ' <span></span></label>'
                    );
                    $item.find('span').text(text);
                    $item.find('input').on('change', function () {
                        $o.prop('selected', this.checked);
                        updateBtn();
                    });
                    $panel.append($item);
                });
                if (shown === 0) {
                    $panel.append('<div class="ems-ms-empty">无匹配选项</div>');
                }
            }

            function syncFromSelect() {
                renderItems($search.val());
                updateBtn();
            }

            $btn.on('click', function (e) {
                e.stopPropagation();
                var open = $panel.toggle().css('display') !== 'none';
                if (open) { $search.val(''); renderItems(''); $search.trigger('focus'); }
            });

            $search.on('input', function () {
                renderItems($search.val());
            });

            $(document).on('click', function (e) {
                if (!$wrap.has(e.target).length) $panel.hide();
            });

            // 选项被外部（如 loadRoles）增删时自动重绘
            var observer = new MutationObserver(function () { syncFromSelect(); });
            observer.observe($sel[0], { childList: true });

            syncFromSelect();
        });
    };
})(jQuery);
