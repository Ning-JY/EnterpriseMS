/**
 * 投标管理 - AI 功能模块
 * 包含：文档解析、流式生成、人员匹配、标书审查、文件导出
 * 
 * 依赖：jQuery, Layer, site.js
 * 页面：/Bid/Detail/{id}
 */

// ============================================================
// 工具函数
// ============================================================

/**
 * HTML 转义（防止 XSS）
 * @param {string} str - 原始字符串
 * @returns {string} 转义后的字符串
 */
function escapeHtml(str) {
    if (str == null) return '';
    if (typeof str !== 'string') str = String(str);
    return str
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

/**
 * 显示/隐藏加载状态
 * @param {HTMLElement} container - 要显示加载状态的容器
 * @param {boolean} show - true=显示，false=隐藏
 */
function setLoading(container, show) {
    if (show) {
        container.innerHTML = '<div class="text-center py-4"><div class="spinner-border text-primary"></div><p class="mt-2">加载中...</p></div>';
    } else {
        container.innerHTML = '';
    }
}

// ============================================================
// 招标文件 AI 解析
// ============================================================

/**
 * 分析招标文件（上传并解析）
 * @param {number} bidProjectId - 投标项目ID
 */
async function analyzeDocument(bidProjectId) {
    var fileInput = document.getElementById('bidFile');
    if (!fileInput || !fileInput.files.length) {
        layer.msg('请选择招标文件', { icon: 2 });
        return;
    }

    var formData = new FormData();
    formData.append('File', fileInput.files[0]);
    formData.append('BidProjectId', bidProjectId);

    var progressEl = document.getElementById('analyzeProgress');
    if (progressEl) progressEl.style.display = 'block';

    try {
        var response = await fetch('/Bid/Analyze', {
            method: 'POST',
            body: formData
        });
        var result = await response.json();

        if (result.success) {
            layer.msg('解析完成！页面将刷新显示结果，请重点核对"否决性条款"和"待人工确认"两个区块。', { icon: 1 }, function() {
                location.reload();
            });
        } else {
            layer.msg('解析失败：' + result.message, { icon: 2 });
        }
    } catch (e) {
        layer.msg('请求失败：' + e.message, { icon: 2 });
    } finally {
        if (progressEl) progressEl.style.display = 'none';
    }
}

// ============================================================
// 人工确认招标要素表
// ============================================================

/**
 * 确认招标要素表（人工核对后确认）
 * @param {number} bidProjectId - 投标项目ID
 */
async function confirmElements(bidProjectId) {
    if (!confirm('确认后将锁定本次招标要素表，作为后续内容生成与人员匹配的依据，确定要继续吗？')) return;

    try {
        var response = await fetch('/Bid/ConfirmElements', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ BidProjectId: bidProjectId })
        });
        var result = await response.json();

        if (result.success) {
            layer.msg('招标要素表已确认。', { icon: 1 }, function() {
                location.reload();
            });
        } else {
            layer.msg('无法确认：' + result.message, { icon: 2 });
        }
    } catch (e) {
        layer.msg('请求失败：' + e.message, { icon: 2 });
    }
}

// ============================================================
// 核对单条要求
// ============================================================

/**
 * 核对单条"待确认"要求
 * @param {HTMLElement} btn - 触发按钮
 */
async function resolveRequirement(btn) {
    var requirementId = btn.dataset.reqId;
    var content = btn.dataset.content;
    var sourceRef = prompt(
        '核对以下条目，并填写在原文中的出处定位（如 "p.14" 或 "投标人须知 §3.2"）：\n\n' + content,
        ''
    );
    if (sourceRef === null) return;
    if (!sourceRef.trim()) {
        layer.msg('出处不能为空，否则该条目会继续保留"待确认"状态', { icon: 2 });
        return;
    }
    var isVeto = confirm('该条目是否为否决性条款（不满足将直接导致废标）？\n点击"确定"=是否决项，点击"取消"=不是否决项');

    try {
        var response = await fetch('/Bid/ResolveRequirement', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ RequirementId: requirementId, IsVeto: isVeto, SourceRef: sourceRef.trim() })
        });
        var result = await response.json();
        if (result.success) {
            location.reload();
        } else {
            layer.msg('保存失败：' + result.message, { icon: 2 });
        }
    } catch (e) {
        layer.msg('请求失败：' + e.message, { icon: 2 });
    }
}

// ============================================================
// 一键生成全部章节
// ============================================================

/**
 * 一键生成全部章节
 * @param {number} bidProjectId - 投标项目ID
 */
async function generateFullBid(bidProjectId) {
    if (!confirm('确定要一键生成全部章节吗？这将覆盖现有内容。')) return;

    var progressEl = document.getElementById('generatingProgress');
    var outputEl = document.getElementById('generatingOutput');
    if (progressEl) progressEl.style.display = 'block';

    try {
        var response = await fetch('/Bid/GenerateFull', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ BidProjectId: bidProjectId })
        });
        var result = await response.json();

        if (result.success) {
            layer.msg('生成完成！页面将刷新显示结果。', { icon: 1 }, function() {
                location.reload();
            });
        } else {
            layer.msg('生成失败：' + result.message, { icon: 2 });
        }
    } catch (e) {
        layer.msg('请求失败：' + e.message, { icon: 2 });
    } finally {
        if (progressEl) progressEl.style.display = 'none';
    }
}

// ============================================================
// 单章节生成（支持流式输出）
// ============================================================

/**
 * 生成单章节（支持自定义要求）
 * @param {number} bidProjectId - 投标项目ID
 * @param {string} chapterName - 章节名称
 * @param {string} customRequirements - 自定义要求（可选）
 */
async function generateChapterStream(bidProjectId, chapterName, customRequirements) {
    var progressEl = document.getElementById('generatingProgress');
    var outputEl = document.getElementById('generatingOutput');

    if (progressEl) progressEl.style.display = 'block';
    if (outputEl) {
        outputEl.style.display = 'block';
        outputEl.innerHTML = '';
    }

    var chapterLabel = document.getElementById('generatingChapter');
    if (chapterLabel) chapterLabel.textContent = chapterName;

    await generateChapterStreamInternal({
        BidProjectId: bidProjectId,
        ChapterName: chapterName,
        TargetWordCount: 2000,
        CustomRequirements: customRequirements || ''
    }, function(chunk) {
        if (outputEl) outputEl.innerHTML += chunk;
    }, function() {
        setTimeout(function() { location.reload(); }, 1000);
    });
}

/**
 * 流式生成章节的内部实现
 * @param {Object} params - 生成参数
 * @param {Function} onChunk - 每个数据块的回调
 * @param {Function} onComplete - 完成时的回调
 */
async function generateChapterStreamInternal(params, onChunk, onComplete) {
    try {
        var response = await fetch('/Bid/GenerateChapter', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(params)
        });

        if (!response.ok) {
            throw new Error('HTTP ' + response.status);
        }

        var reader = response.body.getReader();
        var decoder = new TextDecoder();
        var buffer = '';

        while (true) {
            var result = await reader.read();
            if (result.done) break;

            buffer += decoder.decode(result.value, { stream: true });
            var lines = buffer.split('\n');
            buffer = lines.pop() || '';

            for (var i = 0; i < lines.length; i++) {
                var line = lines[i].trim();
                if (line.startsWith('data:')) {
                    var data = line.substring(5).trim();
                    if (data === '[DONE]') {
                        if (onComplete) onComplete();
                        return;
                    }
                    try {
                        var json = JSON.parse(data);
                        if (json.content) {
                            if (onChunk) onChunk(json.content);
                        }
                    } catch (e) {
                        // 忽略解析错误
                    }
                }
            }
        }

        if (onComplete) onComplete();
    } catch (e) {
        if (onChunk) onChunk('<div class="text-danger">生成失败：' + escapeHtml(e.message) + '</div>');
        if (onComplete) onComplete();
    }
}

/**
 * 添加新章节
 * @param {number} bidProjectId - 投标项目ID
 */
function showAddChapter(bidProjectId) {
    var name = prompt('请输入章节名称：');
    if (name && name.trim()) {
        generateChapterStream(bidProjectId, name.trim(), '');
    }
}

/**
 * 重新生成章节
 * @param {number} bidProjectId - 投标项目ID
 * @param {string} chapterName - 章节名称
 */
function regenerateChapter(bidProjectId, chapterName) {
    var customReq = prompt(
        '请输入自定义要求（可选，直接点确定跳过）：\n\n例如：\n- 重点突出公司在轨道交通领域的经验\n- 增加BIM技术应用方案\n- 补充质量控制措施',
        ''
    );
    generateChapterStream(bidProjectId, chapterName, customReq || '');
}

// ============================================================
// 章节预览与编辑
// ============================================================

var currentDocId = null;
var isEditMode = false;

/**
 * 预览章节内容
 * @param {number} docId - 文档ID
 * @param {string} chapterName - 章节名称
 */
async function previewChapter(docId, chapterName) {
    currentDocId = docId;
    isEditMode = false;

    var titleEl = document.getElementById('chapterModalTitle');
    var previewEl = document.getElementById('chapterPreview');
    var editAreaEl = document.getElementById('chapterContentEdit');
    var toggleBtn = document.getElementById('btnToggleEdit');
    var saveBtn = document.getElementById('btnSaveContent');

    if (titleEl) titleEl.textContent = chapterName;
    if (previewEl) {
        previewEl.style.display = 'block';
        previewEl.innerHTML = '<div class="text-center"><div class="spinner-border text-primary"></div><p>加载中...</p></div>';
    }
    if (editAreaEl) editAreaEl.style.display = 'none';
    if (toggleBtn) toggleBtn.textContent = '编辑';
    if (saveBtn) saveBtn.style.display = 'none';

    $('#chapterModal').modal('show');

    try {
        var response = await fetch('/Bid/GetDocument?docId=' + docId, {
            credentials: 'same-origin'
        });

        if (response.redirected) {
            window.location.href = response.url;
            return;
        }

        var result = await response.json();
        var doc = result.data || result;

        var content = doc.Content || doc.content || '';

        if (previewEl) {
            if (content) {
                previewEl.innerHTML = '<div style="white-space: pre-wrap; line-height: 1.8;">' + escapeHtml(content) + '</div>';
            } else {
                previewEl.innerHTML = '<div class="text-muted">暂无内容，请先生成标书</div>';
            }
        }
        if (editAreaEl) editAreaEl.value = content;
    } catch (e) {
        if (previewEl) {
            previewEl.innerHTML = '<div class="text-danger">加载失败：' + escapeHtml(e.message) + '</div>';
        }
    }
}

/**
 * 切换编辑模式
 */
function toggleEditMode() {
    isEditMode = !isEditMode;

    var preview = document.getElementById('chapterPreview');
    var editArea = document.getElementById('chapterContentEdit');
    var btnToggle = document.getElementById('btnToggleEdit');
    var btnSave = document.getElementById('btnSaveContent');

    if (isEditMode) {
        if (editArea) editArea.value = preview ? preview.innerText : '';
        if (preview) preview.style.display = 'none';
        if (editArea) editArea.style.display = 'block';
        if (btnToggle) btnToggle.textContent = '预览';
        if (btnSave) btnSave.style.display = 'inline-block';
    } else {
        if (preview) preview.innerHTML = '<div style="white-space: pre-wrap;">' + escapeHtml(editArea ? editArea.value : '') + '</div>';
        if (preview) preview.style.display = 'block';
        if (editArea) editArea.style.display = 'none';
        if (btnToggle) btnToggle.textContent = '编辑';
        if (btnSave) btnSave.style.display = 'none';
    }
}

/**
 * 保存章节内容
 */
async function saveChapterContent() {
    if (!currentDocId) return;

    var editArea = document.getElementById('chapterContentEdit');
    var content = editArea ? editArea.value : '';

    try {
        var response = await fetch('/Bid/UpdateContent?documentId=' + currentDocId, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Content: content })
        });
        var result = await response.json();

        if (result.success) {
            layer.msg('保存成功！', { icon: 1 }, function() {
                location.reload();
            });
        } else {
            layer.msg('保存失败：' + result.message, { icon: 2 });
        }
    } catch (e) {
        layer.msg('请求失败：' + e.message, { icon: 2 });
    }
}

// ============================================================
// 组卷功能
// ============================================================

var assembleData = null;

/**
 * 组卷（组装完整投标文件）
 * @param {number} bidProjectId - 投标项目ID
 * @param {string} part - 零件类型：'all'|'technical'|'commercial'
 */
async function assembleDocument(bidProjectId, part) {
    var titleEl = document.getElementById('assembleModalTitle');
    var infoEl = document.getElementById('assembleInfo');
    var tabsEl = document.getElementById('assembleTabs');
    var contentEl = document.getElementById('assembleTabContent');
    var loadingEl = document.getElementById('assembleLoading');

    if (titleEl) titleEl.textContent = part === 'all' ? '投标文件（完整）' :
        part === 'technical' ? '技术部分' : '商务部分';
    if (infoEl) infoEl.innerHTML = '';
    if (tabsEl) tabsEl.innerHTML = '';
    if (contentEl) contentEl.innerHTML = '';
    if (loadingEl) loadingEl.style.display = 'block';

    $('#assembleModal').modal('show');

    try {
        var response = await fetch('/Bid/Assemble', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ BidProjectId: bidProjectId, Part: part })
        });
        var result = await response.json();

        if (loadingEl) loadingEl.style.display = 'none';

        if (result.success && result.data) {
            assembleData = result.data;
            displayAssembleResult(result.data);
        } else {
            if (infoEl) infoEl.innerHTML = '<div class="alert alert-warning">' + escapeHtml(result.message || '组卷失败') + '</div>';
        }
    } catch (e) {
        if (loadingEl) loadingEl.style.display = 'none';
        if (infoEl) infoEl.innerHTML = '<div class="alert alert-danger">请求失败：' + escapeHtml(e.message) + '</div>';
    }
}

/**
 * 显示组卷结果
 * @param {Object} data - 组卷结果数据
 */
function displayAssembleResult(data) {
    var infoEl = document.getElementById('assembleInfo');
    var tabsEl = document.getElementById('assembleTabs');
    var contentEl = document.getElementById('assembleTabContent');

    if (infoEl) {
        infoEl.innerHTML = '<div class="alert alert-info py-2"><strong>' + escapeHtml(data.ProjectName) + '</strong> - 组装时间：' + escapeHtml(data.AssembleTime) + '</div>';
    }

    var tabsHtml = '';
    var contentHtml = '';
    var first = true;

    if (data.TechnicalPart && data.TechnicalPart.Chapters && data.TechnicalPart.Chapters.length > 0) {
        tabsHtml += '<li class="nav-item"><a class="nav-link' + (first ? ' active' : '') + '" data-toggle="tab" href="#tabTechnical">技术部分 (' + data.TechnicalPart.WordCount + '字)</a></li>';
        contentHtml += '<div class="tab-pane fade' + (first ? ' show active' : '') + '" id="tabTechnical">';
        contentHtml += buildPartContent(data.TechnicalPart);
        contentHtml += '</div>';
        first = false;
    }

    if (data.CommercialPart && data.CommercialPart.Chapters && data.CommercialPart.Chapters.length > 0) {
        tabsHtml += '<li class="nav-item"><a class="nav-link' + (first ? ' active' : '') + '" data-toggle="tab" href="#tabCommercial">商务部分 (' + data.CommercialPart.WordCount + '字)</a></li>';
        contentHtml += '<div class="tab-pane fade' + (first ? ' show active' : '') + '" id="tabCommercial">';
        contentHtml += buildPartContent(data.CommercialPart);
        contentHtml += '</div>';
        first = false;
    }

    if (data.FullDocument && data.FullDocument.Chapters && data.FullDocument.Chapters.length > 0) {
        tabsHtml += '<li class="nav-item"><a class="nav-link' + (first ? ' active' : '') + '" data-toggle="tab" href="#tabFull">完整文件 (' + data.FullDocument.WordCount + '字)</a></li>';
        contentHtml += '<div class="tab-pane fade' + (first ? ' show active' : '') + '" id="tabFull">';
        contentHtml += '<div style="white-space: pre-wrap; line-height: 1.8; padding: 15px; background: #f8f9fa; border-radius: 5px; max-height: 60vh; overflow-y: auto;">';
        contentHtml += escapeHtml(data.FullDocument.Content);
        contentHtml += '</div></div>';
    }

    if (tabsEl) tabsEl.innerHTML = tabsHtml;
    if (contentEl) contentEl.innerHTML = contentHtml;
}

/**
 * 构建部分内容 HTML
 * @param {Object} part - 部分数据
 * @returns {string} HTML 字符串
 */
function buildPartContent(part) {
    var html = '';
    if (part.Chapters) {
        part.Chapters.forEach(function(chapter, index) {
            html += '<div class="card mb-3">';
            html += '<div class="card-header"><strong>' + (index + 1) + '. ' + escapeHtml(chapter.Name || '') + '</strong>';
            html += ' <span class="badge badge-secondary ml-2">' + (chapter.WordCount || 0) + '字</span></div>';
            html += '<div class="card-body">';
            html += '<div style="white-space: pre-wrap; line-height: 1.8;">' + escapeHtml(chapter.Content || '') + '</div>';
            html += '</div></div>';
        });
    }
    return html;
}

/**
 * 复制组卷内容到剪贴板
 */
function copyAssembleContent() {
    if (!assembleData) return;

    var content = '';
    if (assembleData.FullDocument && assembleData.FullDocument.Content) {
        content = assembleData.FullDocument.Content;
    } else if (assembleData.TechnicalPart && assembleData.CommercialPart) {
        content = (assembleData.TechnicalPart.Content || '') + '\n\n' + (assembleData.CommercialPart.Content || '');
    } else if (assembleData.TechnicalPart) {
        content = assembleData.TechnicalPart.Content || '';
    } else if (assembleData.CommercialPart) {
        content = assembleData.CommercialPart.Content || '';
    }

    navigator.clipboard.writeText(content).then(function() {
        layer.msg('内容已复制到剪贴板！', { icon: 1 });
    }).catch(function() {
        var textarea = document.createElement('textarea');
        textarea.value = content;
        document.body.appendChild(textarea);
        textarea.select();
        document.execCommand('copy');
        document.body.removeChild(textarea);
        layer.msg('内容已复制到剪贴板！', { icon: 1 });
    });
}

// ============================================================
// 文件导出
// ============================================================

/**
 * 导出投标文件
 * @param {number} bidProjectId - 投标项目ID
 * @param {string} format - 格式：'word'|'pdf'
 */
async function exportDocument(bidProjectId, format) {
    var partSelect = document.getElementById('exportPart');
    var part = partSelect ? partSelect.value : 'all';
    var statusEl = document.getElementById('exportStatus');

    if (statusEl) {
        statusEl.innerHTML = '<span class="text-muted"><i class="fas fa-spinner fa-spin mr-1"></i>正在生成' + (format === 'pdf' ? 'PDF' : 'Word') + '文件...</span>';
    }

    var url = '/Bid/Export' + (format === 'pdf' ? 'Pdf' : 'Word') + '?bidProjectId=' + bidProjectId + '&part=' + part;

    try {
        var response = await fetch(url, { method: 'GET' });
        var contentType = response.headers.get('Content-Type') || '';

        if (contentType.indexOf('application/json') !== -1) {
            var result = await response.json();
            if (statusEl) {
                statusEl.innerHTML = '<div class="alert alert-warning py-1 px-2 mb-0">' + escapeHtml(result.message || '导出失败') + '</div>';
            }
            return;
        }

        if (!response.ok) {
            if (statusEl) {
                statusEl.innerHTML = '<div class="alert alert-danger py-1 px-2 mb-0">导出失败（HTTP ' + response.status + '）</div>';
            }
            return;
        }

        var warningsHeader = response.headers.get('X-Export-Warnings');
        var blob = await response.blob();
        var disposition = response.headers.get('Content-Disposition') || '';
        var match = /filename\*?=(?:UTF-8'')?["']?([^"';]+)/i.exec(disposition);
        var fileName = match ? decodeURIComponent(match[1]) : ('export.' + (format === 'pdf' ? 'pdf' : 'docx'));

        var a = document.createElement('a');
        var objectUrl = URL.createObjectURL(blob);
        a.href = objectUrl;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(objectUrl);

        if (warningsHeader && statusEl) {
            var warnings = decodeURIComponent(warningsHeader).split(' | ');
            var html = '<div class="alert alert-warning py-1 px-2 mb-0">';
            warnings.forEach(function(w) {
                html += '<div>' + escapeHtml(w) + '</div>';
            });
            html += '</div>';
            statusEl.innerHTML = html;
        } else if (statusEl) {
            statusEl.innerHTML = '<span class="text-success"><i class="fas fa-check-circle mr-1"></i>已下载</span>';
        }
    } catch (e) {
        if (statusEl) {
            statusEl.innerHTML = '<div class="alert alert-danger py-1 px-2 mb-0">请求失败：' + escapeHtml(e.message) + '</div>';
        }
    }
}

// ============================================================
// 人员智能匹配
// ============================================================

/**
 * 智能匹配人员
 * @param {number} bidProjectId - 投标项目ID
 * @param {number} maxPersonnel - 最大人数，默认10
 */
async function matchPersonnel(bidProjectId, maxPersonnel) {
    maxPersonnel = maxPersonnel || 10;

    var progressEl = document.getElementById('generatingProgress');
    var outputEl = document.getElementById('generatingOutput');
    var chapterLabel = document.getElementById('generatingChapter');

    if (progressEl) progressEl.style.display = 'block';
    if (outputEl) {
        outputEl.style.display = 'block';
        outputEl.innerHTML = '<div class="text-center"><div class="spinner-border text-primary"></div><p>正在从数据库中匹配符合要求的人员...</p></div>';
    }
    if (chapterLabel) chapterLabel.textContent = '智能匹配人员';

    try {
        var response = await fetch('/Bid/MatchPersonnel', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'same-origin',
            body: JSON.stringify({ BidProjectId: bidProjectId, MaxPersonnel: maxPersonnel })
        });

        if (response.redirected) {
            window.location.href = response.url;
            return;
        }

        var text = await response.text();
        var result = JSON.parse(text);
        var data = result.data || result;

        var matched = data.matchedPersonnel || data.MatchedPersonnel;
        if (data && matched) {
            data.matchedPersonnel = matched;
            data.unmatchedRequirements = data.unmatchedRequirements || data.UnmatchedRequirements || [];
            data.summary = data.summary || data.Summary || '';
            displayPersonnelMatchResult(data);
        } else {
            var msg = (data && (data.message || data.Message)) || result.message || '未找到匹配人员';
            if (outputEl) outputEl.innerHTML = '<div class="alert alert-warning">' + escapeHtml(msg) + '</div>';
        }
    } catch (e) {
        if (outputEl) outputEl.innerHTML = '<div class="alert alert-danger">请求失败：' + escapeHtml(e.message) + '</div>';
    } finally {
        if (progressEl) progressEl.style.display = 'none';
    }
}

/**
 * 显示人员匹配结果
 * @param {Object} data - 匹配结果数据
 */
function displayPersonnelMatchResult(data) {
    var outputEl = document.getElementById('generatingOutput');
    if (!outputEl) return;

    var html = '<div class="alert alert-info py-2">' + escapeHtml(data.summary || data.Summary || '') + '</div>';

    var personnel = data.matchedPersonnel || data.MatchedPersonnel || [];
    if (personnel.length > 0) {
        personnel.forEach(function(p) {
            var name = p.name || p.Name || '';
            var dept = p.deptName || p.DeptName || '-';
            var edu = p.education || p.Education || '-';
            var years = p.workYears || p.WorkYears || 0;
            var score = p.matchScore || p.MatchScore || 0;
            var certs = p.certificates || p.Certificates || [];
            var basis = p.matchBasis || p.MatchBasis || [];
            var conflicts = p.conflictWarnings || p.ConflictWarnings || [];
            var hasConflict = conflicts.length > 0;

            html += '<div class="card mb-2" style="border-left:4px solid ' + (hasConflict ? '#d9822b' : '#28a745') + ';">';
            html += '<div class="card-body p-2">';
            html += '<div class="d-flex justify-content-between align-items-start">';
            html += '<div><strong>' + escapeHtml(name) + '</strong>';
            html += ' <span class="text-muted" style="font-size:12px;">' + escapeHtml(dept) + ' · ' + escapeHtml(edu) + ' · 工作' + years + '年</span></div>';
            html += '<span class="badge badge-primary">匹配得分 ' + score + '</span>';
            html += '</div>';

            if (hasConflict) {
                html += '<div class="alert alert-warning py-1 px-2 mt-2 mb-1" style="font-size:11.5px;">';
                conflicts.forEach(function(c) {
                    html += '<div><i class="fas fa-exclamation-triangle mr-1"></i>' + escapeHtml(c) + '</div>';
                });
                html += '</div>';
            }

            html += '<div class="mt-2" style="font-size:12px;">';
            certs.forEach(function(c) {
                var certName = c.certName || c.CertName || '';
                var validity = c.validityStatus || c.ValidityStatus || 'Valid';
                var badgeClass = validity === 'Expired' ? 'badge-danger' :
                    (validity === 'ExpiringSoon' ? 'badge-warning' :
                        (validity === 'Unknown' ? 'badge-secondary' : 'badge-success'));
                var validityLabel = validity === 'Expired' ? '已过期' :
                    (validity === 'ExpiringSoon' ? '即将到期' :
                        (validity === 'Unknown' ? '未登记有效期' : '有效'));
                if (certName) {
                    html += '<span class="badge ' + badgeClass + ' mr-1 mb-1">' + escapeHtml(certName) + ' · ' + validityLabel + '</span>';
                }
            });
            html += '</div>';

            if (basis.length > 0) {
                html += '<ul class="mb-0 mt-1" style="font-size:11.5px;color:#495057;padding-left:18px;">';
                basis.forEach(function(b) {
                    html += '<li>' + escapeHtml(b) + '</li>';
                });
                html += '</ul>';
            }

            html += '</div></div>';
        });
    } else {
        html += '<div class="alert alert-warning py-2"><i class="fas fa-exclamation-triangle"></i> 暂无符合条件的人员</div>';
    }

    var unmatched = data.unmatchedRequirements || data.UnmatchedRequirements || [];
    if (unmatched.length > 0) {
        html += '<div class="alert alert-warning py-2 mt-2"><strong>未匹配的要求：</strong><ul class="mb-0">';
        unmatched.forEach(function(req) {
            html += '<li>' + escapeHtml(req) + '</li>';
        });
        html += '</ul></div>';
    }

    var unrecognized = data.unrecognizedRequirements || data.UnrecognizedRequirements || [];
    if (unrecognized.length > 0) {
        html += '<div class="alert alert-info py-2 mt-2" style="background:#e9f1fb;border-color:#b9d3ef;">';
        html += '<strong><i class="fas fa-search mr-1"></i>系统无法自动识别证书类型，需人工直接核对：</strong><ul class="mb-0">';
        unrecognized.forEach(function(req) {
            html += '<li>' + escapeHtml(req) + '</li>';
        });
        html += '</ul></div>';
    }

    html += '<div class="mt-3"><button class="btn btn-primary" onclick="generatePersonnelSection(' + bidProjectId + ')"><i class="fas fa-magic mr-1"></i>根据匹配结果生成人员配置文档</button>';
    html += ' <small class="text-muted ml-2">存在岗位冲突的人员不会被写入正文，需人工确认后再补充</small></div>';

    outputEl.innerHTML = html;
}

/**
 * 根据匹配结果生成人员配置文档
 * @param {number} bidProjectId - 投标项目ID
 */
async function generatePersonnelSection(bidProjectId) {
    var outputEl = document.getElementById('generatingOutput');
    if (outputEl) {
        outputEl.innerHTML += '<div class="mt-3"><div class="alert alert-info"><i class="fas fa-spinner fa-spin"></i> 正在生成人员配置文档...</div></div>';
    }

    try {
        var response = await fetch('/Bid/GeneratePersonnelSection', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'same-origin',
            body: JSON.stringify({ BidProjectId: bidProjectId, MaxPersonnel: 10 })
        });
        var result = await response.json();

        if (result.success) {
            layer.msg('人员配置文档已生成！页面将刷新显示结果。', { icon: 1 }, function() {
                location.reload();
            });
        } else {
            layer.msg('生成失败：' + result.message, { icon: 2 });
        }
    } catch (e) {
        layer.msg('请求失败：' + e.message, { icon: 2 });
    }
}

// ============================================================
// AI 标书审查
// ============================================================

/**
 * AI 审查标书
 * @param {number} bidProjectId - 投标项目ID
 */
async function reviewBid(bidProjectId) {
    var resultEl = document.getElementById('reviewResult');
    var contentEl = document.getElementById('reviewContent');

    if (resultEl) resultEl.style.display = 'block';
    if (contentEl) {
        contentEl.innerHTML = '<div class="text-center"><div class="spinner-border text-primary"></div><p>正在审查...</p></div>';
    }

    try {
        var response = await fetch('/Bid/Review', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ BidProjectId: bidProjectId })
        });
        var result = await response.json();

        if (result.success && contentEl) {
            displayReviewResult(result.data, contentEl);
        } else if (contentEl) {
            contentEl.innerHTML = '<div class="alert alert-danger">' + escapeHtml(result.message) + '</div>';
        }
    } catch (e) {
        if (contentEl) contentEl.innerHTML = '<div class="alert alert-danger">请求失败：' + escapeHtml(e.message) + '</div>';
    }
}

/**
 * 显示审查结果
 * @param {Object} data - 审查结果数据
 * @param {HTMLElement} container - 容器元素
 */
function displayReviewResult(data, container) {
    var html = '<div class="mb-3"><strong>综合评分：</strong>';
    var scoreClass = data.overallScore >= 80 ? 'success' : data.overallScore >= 60 ? 'warning' : 'danger';
    html += '<span class="badge bg-' + scoreClass + ' fs-6">' + data.overallScore + '分</span></div>';

    if (data.missingItems && data.missingItems.length > 0) {
        html += '<h6 class="text-danger">缺失内容：</h6><ul>';
        data.missingItems.forEach(function(item) {
            html += '<li>' + escapeHtml(item) + '</li>';
        });
        html += '</ul>';
    }

    if (data.issues && data.issues.length > 0) {
        html += '<h6 class="text-warning">问题列表：</h6>';
        data.issues.forEach(function(issue) {
            var alertClass = issue.severity === 'high' ? 'danger' : issue.severity === 'medium' ? 'warning' : 'info';
            html += '<div class="alert alert-' + alertClass + ' py-2">';
            html += '<strong>[' + escapeHtml(issue.chapter || '') + ']</strong> ' + escapeHtml(issue.description || '');
            if (issue.suggestion) {
                html += '<br><small class="text-muted">建议：' + escapeHtml(issue.suggestion) + '</small>';
            }
            html += '</div>';
        });
    }

    if (data.suggestions && data.suggestions.length > 0) {
        html += '<h6 class="text-primary">改进建议：</h6><ul>';
        data.suggestions.forEach(function(item) {
            html += '<li>' + escapeHtml(item) + '</li>';
        });
        html += '</ul>';
    }

    container.innerHTML = html;
}

// ============================================================
// 导出模块（兼容旧代码）
// ============================================================

// 保持全局函数供页面调用
window.BidAi = {
    analyzeDocument: analyzeDocument,
    confirmElements: confirmElements,
    resolveRequirement: resolveRequirement,
    generateFullBid: generateFullBid,
    generateChapterStream: generateChapterStream,
    showAddChapter: showAddChapter,
    regenerateChapter: regenerateChapter,
    previewChapter: previewChapter,
    toggleEditMode: toggleEditMode,
    saveChapterContent: saveChapterContent,
    assembleDocument: assembleDocument,
    copyAssembleContent: copyAssembleContent,
    exportDocument: exportDocument,
    matchPersonnel: matchPersonnel,
    generatePersonnelSection: generatePersonnelSection,
    reviewBid: reviewBid
};
