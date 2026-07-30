#!/usr/bin/env python3
"""批量修复 Controller 文件中的 API 返回方式、构造函数和异常处理"""
import re
import os
from pathlib import Path

# 目标文件列表
TARGET_FILES = [
    "Controllers/System/DebugController.cs",
    "Controllers/Kb/KbController.cs",
    "Controllers/Project/ProjectImportController.cs",
    "Controllers/Tool/TemplateReportController.cs",
    "Controllers/Bid/BidController.cs",
    "Controllers/Project/ProjectController.cs",
]

def fix_api_returns(content):
    """修复 API 返回方式"""
    # Json(ApiResult<object>.Ok(...)) -> ApiOk<object>(null!, ...)
    content = re.sub(
        r'return\s+Json\(ApiResult<object>\.Ok\(([^)]+)\)\)',
        r'return ApiOk<object>(null!, \1)',
        content
    )

    # Json(ApiResult<T>.Ok(...)) -> ApiOk(...)
    content = re.sub(
        r'return\s+Json\(ApiResult<(\w+)>\.Ok\(([^)]+)\)\)',
        r'return ApiOk(\2)',
        content
    )

    # Json(ApiResult.Ok(...)) -> ApiOk<object>(null!, ...)
    content = re.sub(
        r'return\s+Json\(ApiResult\.Ok\(([^)]+)\)\)',
        r'return ApiOk<object>(null!, \1)',
        content
    )

    # Json(ApiResult<object>.Fail(...)) -> ApiFail(...)
    content = re.sub(
        r'return\s+Json\(ApiResult<object>\.Fail\(([^)]+)\)\)',
        r'return ApiFail(\1)',
        content
    )

    # Json(ApiResult.Fail(...)) -> ApiFail(...)
    content = re.sub(
        r'return\s+Json\(ApiResult\.Fail\(([^)]+)\)\)',
        r'return ApiFail(\1)',
        content
    )

    return content

def fix_exception_handling(content):
    """统一异常处理模式"""
    # catch (BusinessException ex) { return ... } -> 统一格式
    content = re.sub(
        r'catch\s*\(BusinessException\s+ex\)\s*\{\s*return\s+([^}]+)\s*\}',
        r'catch (Exception ex) when (ex is BusinessException or NotFoundException)\n        {\n            return \1\n        }',
        content
    )

    # 单行 catch 改为多行
    content = re.sub(
        r'catch\s*\(Exception\s+ex\)\s+when\s+\([^)]+\)\s*\{\s*return\s+([^}]+)\s*\}',
        r'catch (Exception ex) when (ex is BusinessException or NotFoundException)\n        {\n            return \1\n        }',
        content
    )

    return content

def process_file(filepath):
    """处理单个文件"""
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()

        original_content = content

        # 应用修复
        content = fix_api_returns(content)
        content = fix_exception_handling(content)

        # 只有内容变化时才写入
        if content != original_content:
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(content)
            return True
        return False
    except Exception as e:
        print(f"[ERROR] {filepath}: {e}")
        return False

def main():
    base_dir = Path(__file__).parent
    fixed_count = 0

    print("Starting batch fix...")
    for rel_path in TARGET_FILES:
        filepath = base_dir / rel_path
        if filepath.exists():
            if process_file(filepath):
                print(f"[OK] {rel_path}")
                fixed_count += 1
            else:
                print(f"[SKIP] {rel_path} (no changes)")
        else:
            print(f"[WARN] File not found: {rel_path}")

    print(f"\nDone! Fixed {fixed_count} files")

if __name__ == "__main__":
    main()
