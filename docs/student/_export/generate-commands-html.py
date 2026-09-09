#!/usr/bin/env python3
"""
docs/student/*.md からフェンス済みコードブロックを抽出し、
ワンクリックコピー付き commands.html を生成する。

配布フロー:
  MPE で all.pdf 生成 → このスクリプトで commands.html 生成
  → 2ファイルセットで学生に配布
学生の使い方:
  PDF を読み物として使い、コマンドは commands.html の「コピー」で
  クリップボードに入れる。PDF テキスト抽出の環境依存問題を回避。
"""
import html
import re
import sys
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parent
STUDENT_DIR = SCRIPT_DIR.parent
OUTPUT_PATH = SCRIPT_DIR / "commands.html"

CHAPTER_FILES = [
    "01-はじめに.md",
    "02-用語とツールの基礎知識.md",
    "03-環境構築-Windows.md",
    "04-環境構築-Mac.md",
    "05-動作確認-HelloWorld.md",
    "06-トラブルシューティング.md",
]

FENCE_RE = re.compile(r"^```(.*)$")
HEADING_RE = re.compile(r"^(#+)\s+(.+)$")


# このタグを付けたコードブロックは commands.html から除外する
# （期待出力・プロンプト外観の例など、学生が打ち込む必要のないもの）
SKIP_LANG = "output"


def parse_markdown(md_path):
    """Return list of (heading_breadcrumb, lang, code_text).
    ` ```output ` タグのブロックは skip する。"""
    text = md_path.read_text(encoding="utf-8")
    stack = []
    in_fence = False
    fence_lang = ""
    code_buf = []
    blocks = []
    for raw in text.split("\n"):
        line = raw.rstrip("\r")
        fm = FENCE_RE.match(line)
        if fm and not in_fence:
            in_fence = True
            fence_lang = fm.group(1).strip()
            code_buf = []
            continue
        if fm and in_fence:
            in_fence = False
            if fence_lang != SKIP_LANG:
                breadcrumb = [(lvl, txt) for (lvl, txt) in stack if lvl > 1]
                blocks.append((breadcrumb, fence_lang, "\n".join(code_buf)))
            continue
        if in_fence:
            code_buf.append(line)
            continue
        hm = HEADING_RE.match(line)
        if hm:
            level = len(hm.group(1))
            title = hm.group(2).strip()
            stack = [(l, t) for (l, t) in stack if l < level]
            stack.append((level, title))
    return blocks


def chapter_title(md_path):
    with md_path.open(encoding="utf-8") as f:
        for line in f:
            m = HEADING_RE.match(line.rstrip("\r\n"))
            if m and len(m.group(1)) == 1:
                return m.group(2).strip()
    return md_path.stem


def render_chapter(chapter_idx, chapter_id, title, blocks):
    parts = [f'<details class="chapter" id="{chapter_id}">']
    parts.append(
        f'<summary class="chapter-summary">'
        f'<span class="chapter-title">{html.escape(title)}</span>'
        f'<span class="count">{len(blocks)} 個</span>'
        f'</summary>'
    )
    parts.append('<div class="chapter-body">')
    if not blocks:
        parts.append('<p class="empty">この章にコマンドはありません。</p>')
        parts.append('</div></details>')
        return "".join(parts)

    prev_breadcrumb = None
    for i, (breadcrumb, lang, code) in enumerate(blocks, 1):
        cmd_id = f"{chapter_idx:02d}-{i}"
        crumb_key = tuple(t for (_, t) in breadcrumb)
        if crumb_key != prev_breadcrumb:
            if breadcrumb:
                crumb_html = " › ".join(html.escape(t) for t in crumb_key)
                parts.append(f'<h3 class="section">{crumb_html}</h3>')
            prev_breadcrumb = crumb_key
        lang_disp = lang if lang else "shell"
        lang_class = f" lang-{re.sub(r'[^a-zA-Z0-9]', '', lang_disp) or 'shell'}"
        code_stripped = code.rstrip("\n")
        parts.append(
            f'<div class="cmd{lang_class}" id="{cmd_id}">'
            f'<div class="cmd-head">'
            f'<span class="cmd-id">#{cmd_id}</span>'
            f'<span class="lang-tag">{html.escape(lang_disp)}</span>'
            f'<button class="cmd-copy" type="button">コピー</button>'
            f'</div>'
            f'<pre class="cmd-body"><code>{html.escape(code_stripped)}</code></pre>'
            f'</div>'
        )
    parts.append('</div></details>')
    return "".join(parts)


HTML_HEAD = """<!DOCTYPE html>
<html lang="ja">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Intern-Server-2026 コマンド一覧（コピペ用）</title>
<style>
:root {
  color-scheme: light;
  --bg: #f7f8fa;
  --panel: #ffffff;
  --border: #e0e3e8;
  --text: #1a1f26;
  --muted: #6b7684;
  --code-bg: #1e2530;
  --code-text: #e8ecf1;
  --primary: #1e6feb;
  --primary-hover: #1558c7;
  --success: #2e9c5b;
  --tag-bg: #eef2f6;
}
* { box-sizing: border-box; }
body {
  font-family: -apple-system, "Hiragino Sans", "Yu Gothic", "Meiryo", sans-serif;
  background: var(--bg);
  color: var(--text);
  line-height: 1.65;
  margin: 0;
  padding: 2rem 1rem 6rem;
  max-width: 960px;
  margin-left: auto;
  margin-right: auto;
}
h1 { margin: 0 0 0.5em; font-size: 1.8em; border-bottom: 3px solid var(--primary); padding-bottom: 0.3em; }
h3.section { margin: 1.6em 0 0.6em; font-size: 1.05em; color: var(--muted); font-weight: 600; }
.intro { background: #e8f0fe; border-left: 4px solid var(--primary); padding: 1em 1.2em; margin: 1em 0 2em; border-radius: 0 8px 8px 0; }
.intro p { margin: 0.4em 0; }
.intro code { background: #d8e4fb; padding: 1px 6px; border-radius: 3px; font-size: 0.92em; }
.toc { background: var(--panel); border: 1px solid var(--border); border-radius: 10px; padding: 1em 1.5em; margin-bottom: 1em; }
.toc h2 { border: none; padding: 0; margin: 0 0 0.5em; font-size: 1em; color: var(--muted); }
.toc ol { margin: 0; padding-left: 1.3em; }
.toc a { color: var(--primary); text-decoration: none; }
.toc a:hover { text-decoration: underline; }
.toc .count { color: var(--muted); font-size: 0.9em; }
.controls { display: flex; gap: 0.6em; margin-bottom: 2em; flex-wrap: wrap; }
.controls button { background: var(--panel); border: 1px solid var(--border); color: var(--text); padding: 6px 14px; border-radius: 6px; font-size: 0.9em; cursor: pointer; font-family: inherit; }
.controls button:hover { background: var(--tag-bg); }
.chapter { margin-bottom: 0.8em; background: var(--panel); border: 1px solid var(--border); border-radius: 10px; overflow: hidden; }
.chapter-summary { padding: 0.9em 1.2em; cursor: pointer; display: flex; align-items: center; gap: 0.8em; list-style: none; user-select: none; }
.chapter-summary::-webkit-details-marker { display: none; }
.chapter-summary::before { content: "▶"; font-size: 0.75em; color: var(--muted); transition: transform 0.15s; display: inline-block; }
.chapter[open] > .chapter-summary::before { transform: rotate(90deg); }
.chapter-summary:hover { background: var(--tag-bg); }
.chapter-title { font-size: 1.15em; font-weight: 600; flex: 1; }
.chapter .count { color: var(--muted); font-size: 0.9em; background: var(--tag-bg); padding: 2px 10px; border-radius: 12px; }
.chapter-body { padding: 0.4em 1.4em 1.4em; border-top: 1px solid var(--border); }
.empty { color: var(--muted); font-style: italic; }
.cmd { background: var(--panel); border: 1px solid var(--border); border-radius: 10px; padding: 0.8em 1em; margin: 0.8em 0; }
.cmd-head { display: flex; align-items: center; gap: 0.6em; margin-bottom: 0.5em; flex-wrap: wrap; }
.cmd-id { font-family: "SF Mono", "Menlo", "Consolas", monospace; font-size: 0.82em; color: var(--muted); background: var(--tag-bg); padding: 2px 8px; border-radius: 4px; }
.lang-tag { font-size: 0.75em; color: var(--muted); background: var(--tag-bg); padding: 2px 8px; border-radius: 4px; text-transform: lowercase; }
.cmd-copy { margin-left: auto; background: var(--primary); color: white; border: none; padding: 6px 16px; border-radius: 6px; font-size: 0.9em; cursor: pointer; font-family: inherit; transition: background 0.15s; }
.cmd-copy:hover { background: var(--primary-hover); }
.cmd-copy.copied { background: var(--success); }
.cmd-copy:disabled { opacity: 0.6; cursor: not-allowed; }
.cmd-body { background: var(--code-bg); color: var(--code-text); padding: 0.85em 1em; border-radius: 6px; font-family: "SF Mono", "Menlo", "Consolas", "Roboto Mono", monospace; font-size: 0.92em; overflow-x: auto; margin: 0; white-space: pre; }
.cmd-body code { font-family: inherit; }
.cmd.lang-csharp .cmd-body { background: #2b2036; }
.cmd.lang-csharp .lang-tag { background: #efe3ff; color: #6d2eb8; }
footer { margin-top: 4em; padding-top: 1em; border-top: 1px solid var(--border); color: var(--muted); font-size: 0.85em; text-align: center; }
</style>
</head>
<body>
<h1>Intern-Server-2026 コマンド一覧（コピペ用）</h1>
<div class="intro">
  <p><strong>このページの使い方</strong></p>
  <p>PDF ガイドで案内されているコマンドは、このページの <strong>「コピー」</strong> ボタンでクリップボードに入ります。あとはターミナルで貼り付け（<code>Cmd+V</code> / <code>Ctrl+V</code>）して実行してください。</p>
  <p>各コマンドの <code>#XX-N</code> は「XX 章の N 番目」を意味する識別子です。PDF と同じ章・見出しの位置に並んでいるので、そちらで見つけてください。</p>
  <p><small>💡 PDF から直接コピーすると環境によっては改行が正しく取れないことがあります。このページからコピーするのが確実です。</small></p>
</div>
"""

HTML_TAIL = """<footer>
  自動生成: <code>docs/student/_export/generate-commands-html.py</code>
</footer>
<script>
document.querySelectorAll(".cmd-copy").forEach(function (btn) {
  btn.addEventListener("click", function () {
    var codeEl = btn.closest(".cmd").querySelector(".cmd-body code");
    var text = codeEl.innerText.replace(/\\n$/, "");
    navigator.clipboard.writeText(text).then(function () {
      var original = btn.textContent;
      btn.textContent = "✓ コピー完了";
      btn.classList.add("copied");
      btn.disabled = true;
      setTimeout(function () {
        btn.textContent = original;
        btn.classList.remove("copied");
        btn.disabled = false;
      }, 1400);
    }).catch(function () {
      btn.textContent = "コピー失敗";
    });
  });
});
document.getElementById("expand-all").addEventListener("click", function () {
  document.querySelectorAll("details.chapter").forEach(function (d) { d.open = true; });
});
document.getElementById("collapse-all").addEventListener("click", function () {
  document.querySelectorAll("details.chapter").forEach(function (d) { d.open = false; });
});
</script>
</body>
</html>
"""


def build():
    toc_items = []
    body_parts = []
    for idx, filename in enumerate(CHAPTER_FILES, 1):
        md_path = STUDENT_DIR / filename
        if not md_path.exists():
            print(f"warn: {md_path} not found, skip", file=sys.stderr)
            continue
        blocks = parse_markdown(md_path)
        title = chapter_title(md_path)
        chapter_id = f"ch{idx:02d}"
        count = len(blocks)
        toc_items.append(
            f'<li><a href="#{chapter_id}">{html.escape(title)}</a>'
            f' <span class="count">({count} 個)</span></li>'
        )
        body_parts.append(render_chapter(idx, chapter_id, title, blocks))
    toc_html = (
        '<nav class="toc"><h2>目次</h2><ol>'
        + "\n".join(toc_items)
        + "</ol></nav>"
    )
    controls_html = (
        '<div class="controls">'
        '<button type="button" id="expand-all">全部開く</button>'
        '<button type="button" id="collapse-all">全部畳む</button>'
        '</div>'
    )
    return HTML_HEAD + toc_html + controls_html + "\n".join(body_parts) + HTML_TAIL


def main():
    out = build()
    OUTPUT_PATH.write_text(out, encoding="utf-8")
    print(f"generated: {OUTPUT_PATH.relative_to(STUDENT_DIR.parent.parent)}", file=sys.stderr)


if __name__ == "__main__":
    main()
