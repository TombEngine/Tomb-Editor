"""
Batch-convert every SVG in TombLib.Icons/Svg to a line-style version:
  1) Multiply every stroke-width by 0.8 (round down to 0.7 minimum).
  2) Convert any shape with fill="#FFFFFF" / "white" / "#FFF" to fill="none"
     and add a stroke="#FFFFFF" so the outline is still visible. Tiny
     circles (radius < 1.5) keep their fill -- those are accent dots
     (eye pupils, vertex markers, etc.) that read as dots, not solids.
  3) Print a one-line summary per file.

Usage:
    python linewise.py
"""

from __future__ import annotations
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent / "Svg"

# Multiplier for every numeric stroke-width.
STROKE_SCALE = 0.8
# Minimum stroke width after scaling (so 0.75 doesn't collapse to 0.6).
MIN_STROKE   = 0.7
# Default stroke width to add to a previously-filled shape that had no stroke.
DEFAULT_STROKE = 1.0
# Circles below this radius keep their fill (accent dots).
DOT_R_MAX = 1.5

# Regex helpers (SVG is XML; regex is safe enough for this single-shot pass).
RE_STROKE_W   = re.compile(r'stroke-width\s*=\s*"([0-9]*\.?[0-9]+)"')
RE_FILL_WHITE = re.compile(r'fill\s*=\s*"(#FFFFFF|#FFF|white)"', re.IGNORECASE)
RE_HAS_STROKE = re.compile(r'\bstroke\s*=\s*"[^"]*"')
RE_CIRCLE_R   = re.compile(r'<circle\b[^>]*\br\s*=\s*"([0-9]*\.?[0-9]+)"', re.IGNORECASE)

def scale_strokes(svg: str) -> str:
    def repl(m: re.Match) -> str:
        w = float(m.group(1))
        new = max(MIN_STROKE, round(w * STROKE_SCALE, 2))
        return f'stroke-width="{new}"'
    return RE_STROKE_W.sub(repl, svg)

def is_small_circle_tag(tag: str) -> bool:
    """Return True for a <circle ... r="<DOT_R_MAX" ..."""
    if not tag.lstrip().startswith("<circle"):
        return False
    m = RE_CIRCLE_R.search(tag)
    if not m:
        return False
    return float(m.group(1)) < DOT_R_MAX

def linify_fills(svg: str) -> str:
    """
    Walk every standalone tag (<path .../>, <rect .../>, <polygon .../>,
    <ellipse .../>, <circle .../>) that has fill="#FFFFFF" and convert it
    to fill="none" stroke="#FFFFFF" stroke-width=DEFAULT_STROKE (if no
    stroke is already set). Small circles are left alone.
    """
    # Match each shape tag individually.
    shape_re = re.compile(
        r'<(path|rect|polygon|ellipse|circle)\b[^>]*?/>',
        re.IGNORECASE | re.DOTALL,
    )
    def repl(m: re.Match) -> str:
        tag = m.group(0)
        if not RE_FILL_WHITE.search(tag):
            return tag
        if is_small_circle_tag(tag):
            return tag
        # Strip the white fill.
        new_tag = RE_FILL_WHITE.sub('fill="none"', tag)
        # Add stroke if absent.
        if not RE_HAS_STROKE.search(new_tag):
            insert = f' stroke="#FFFFFF" stroke-width="{DEFAULT_STROKE}"'
            new_tag = new_tag.replace("/>", f"{insert}/>", 1)
        return new_tag
    return shape_re.sub(repl, svg)

def process(path: Path) -> tuple[bool, str]:
    """Apply both transforms; return (changed, before_or_after_note)."""
    original = path.read_text(encoding="utf-8")
    rewritten = scale_strokes(linify_fills(original))
    if rewritten != original:
        path.write_text(rewritten, encoding="utf-8")
        return True, "updated"
    return False, "skipped"

def main() -> None:
    svgs = sorted(ROOT.rglob("*.svg"))
    print(f"Found {len(svgs)} SVG files under {ROOT}")
    updated = 0
    for svg in svgs:
        changed, note = process(svg)
        if changed:
            updated += 1
            rel = svg.relative_to(ROOT)
            print(f"  {note:8} {rel}")
    print(f"Done. {updated}/{len(svgs)} updated.")

if __name__ == "__main__":
    main()
