#!/usr/bin/env python3
"""修复 ApiOk 方法调用中的多余参数"""
import re
import os
from pathlib import Path

def fix_apiok_calls(content):
    """修复 ApiOk<object>(null!, data, "msg") -> ApiOk(data, "msg")"""
    # 模式: ApiOk<object>(null!, something, "message")
    pattern = r'ApiOk<object>\(null!,\s*([^,]+),\s*("[^"]*")\)'
    replacement = r'ApiOk(\1, \2)'
    content = re.sub(pattern, replacement, content)

    # 模式: ApiOk<object>(null!, something)  (只有2个参数的，保持不变)
    # 这个不需要修改

    return content

def process_file(filepath):
    """处理单个文件"""
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()

        original = content
        content = fix_apiok_calls(content)

        if content != original:
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(content)
            return True
        return False
    except Exception as e:
        print(f"[ERROR] {filepath}: {e}")
        return False

def main():
    base_dir = Path(r"C:\Users\ningj\source\repos\EnterpriseMS\Controllers")
    fixed = 0

    for csfile in base_dir.rglob("*.cs"):
        if process_file(csfile):
            fixed += 1
            print(f"[FIXED] {csfile.relative_to(base_dir)}")

    print(f"\nDone! Fixed {fixed} files")

if __name__ == "__main__":
    main()
