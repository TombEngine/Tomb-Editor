# TombLib.Icons asset pipeline

`IconPipeline` inventories the SVG catalog, maps legacy PNGs, checks the checked-in raster buckets, renders PNGs, and creates review sheets. It uses the same SharpVectors package and WPF renderer as `TombLib.Icons`. The utility targets `net8.0-windows`; major-version runtime roll-forward lets it run on newer installed .NET Windows runtimes.

Run from the repository root:

```powershell
$legacyRoot = Join-Path $env:TEMP ("TombLibLegacyExport-" + [Guid]::NewGuid().ToString('N'))
& TombLib/TombLib.Icons/Tools/IconPipeline/Export-LegacyIcons.ps1 -OutputDirectory $legacyRoot
dotnet run --project TombLib/TombLib.Icons/Tools/IconPipeline/IconPipeline.csproj -- inventory --legacy-root $legacyRoot --output TombLib/TombLib.Icons/Tools/IconPipeline/legacy-icon-map.csv
dotnet run --project TombLib/TombLib.Icons/Tools/IconPipeline/IconPipeline.csproj -- validate-png
dotnet run --project TombLib/TombLib.Icons/Tools/IconPipeline/IconPipeline.csproj -- rasterize --bucket all
dotnet run --project TombLib/TombLib.Icons/Tools/IconPipeline/IconPipeline.csproj -- contact-sheet --legacy-root $legacyRoot --output (Join-Path $env:TEMP 'icons.png')
dotnet run --project TombLib/TombLib.Icons/Tools/IconPipeline/IconPipeline.csproj -- rasterize-legacy --legacy-root $legacyRoot --destination (Get-Location).Path --plan-output (Join-Path $env:TEMP 'legacy-plan.csv')
```

`--root` selects a different `TombLib.Icons` directory. `--output` for rasterization selects the PNG root. Use `--bucket 1`, `2`, `3`, `4`, or `all`; generated files go under `@1x` through `@4x` and remain arranged by category. Rasterization writes transparent 16, 32, 48, or 64 pixel canvases from each SVG's 16 by 16 viewport. The SVG is rendered with text converted to geometry and the source canvas bounds retained, so intentional padding is not stretched away.

## Legacy map

Inventory scans every PNG below `--legacy-root`. It recognizes `icons_<category>` folders and category prefixes such as `actions_` or `general_` in shared resource folders. It removes common `-16`, `-24`, `-32`, and `-48` size suffixes. Known old `General/search-and-replace` names map to `General/Find-and-replace`; otherwise, a unique SVG basename can resolve an icon from a differently named resource folder. A matched row has the relative legacy PNG path, the `Category/Name` SVG ID, and `matched` status. Legacy files without an SVG are `legacy-only`; SVGs without an old PNG are `svg-only`. With no legacy root, every SVG row is marked `svg-only-legacy-not-provided`. Multiple legacy files may map to the same SVG ID.

The mapping is a review aid. Ambiguous old resource names and semantic aliases need human review; the tool does not claim that a filename match proves the silhouette, color, or meaning is equivalent. Keep legacy inputs in their original files and use the contact sheets to review them beside the SVG. A source file in a folder named `icons_<category>` takes that folder's category; otherwise a recognized category prefix is used, with unprefixed files assigned to `Misc`.

## Validation and contact sheets

`validate-png` checks that every SVG has exactly one same-path PNG per scale bucket, verifies PNG readability and dimensions, and reports missing, unexpected, or incorrectly sized files. It exits with code 1 when validation finds an error.

`rasterize-legacy` creates a path-by-path plan from matched rows in the CSV, using each source PNG's original dimensions. The default is a dry run. Review its counts and plan file, then pass `--apply true` to replace existing mapped PNGs. The destination and export roots must be explicitly supplied; paths are checked to remain inside both roots, every destination must already exist, and no unmatched or unlisted PNG is written. It renders square legacy icons at their original 16, 24, or 48 pixel size and preserves a non-square image's aspect ratio on its original canvas. It excludes images larger than 48 pixels square except the seven reviewed 75x21 transform charts; current 100x100 shadow and 89x89 service object textures therefore remain untouched.

`contact-sheet` writes one page per category and splits larger categories into numbered pages. Each page has four paired legacy/current cards per row, a dark background behind icons, a 48 pixel preview, and dimensions no larger than 1520 by 1820 pixels. Legacy-only records appear as cards with an empty SVG side. Use this output for visual review; the map CSV remains the path-level source mapping.

## Current catalog baseline

The initial catalog had 235 SVGs and 940 PNGs: Actions 34, General 47, Misc 69, Objects 16, Sectortype 30, Texture 11, and Toolbox 28. The fully expanded catalog now has 261 SVGs: Actions 57, General 47, Misc 69, Objects 16, Sectortype 30, Texture 13, and Toolbox 29. A complete set of four raster buckets for that catalog contains 1044 PNGs. Each category SVG uses the 16 by 16 viewport, including nested `Actions/Transport` and `Actions/Transform` IDs.

`Export-LegacyIcons.ps1` exports a curated icon source set from pinned `origin/develop` commit `8c32757740d1477fcaaa85934e327dee35dd33a2`. It archives the TombEditor category folders, TombIDE and TombLib resource folders, DarkUI example resources, and all `WadTool/Resources` PNGs, including `icons_transport` and `icons_transform`. It retains legacy files whose basename matches a current SVG or whose folder is an `icons_*` category folder, and records the source commit in the export directory. The commit must already exist in the local Git object database; the script does not fetch it.

At the reviewed baseline, the exported source contains 363 PNGs across TombEditor, TombIDE, WadTool, TombLib, and DarkUI example resources. The manifest reports 363 matched legacy files, no legacy-only files, and 14 SVGs without a legacy match. It includes duplicate resources shipped in multiple applications. The map normalizes old transport and transform folder names to the current `Actions/Transport` and `Actions/Transform` IDs. The former TombEditor-only set had 156 files; its Mirror, Rotate, and Invisible gaps now have current SVGs. The approved UI raster plan contains 359 existing files: 331 at 16x16, 18 at 24x24, 3 at 48x48, and 7 transform charts at 75x21. Four larger non-UI textures are excluded. Regenerate the map after changing artwork or updating the curated legacy source set.

Do not run `linewise.py` as an automatic step in this pipeline. It rewrites SVG source styles globally; filled subjects and color accents can carry their original meaning. Update source SVGs by reviewing paired legacy/current sheets, then regenerate the raster buckets from those sources.

## Hand-drawn line icons

Every icon that exists as a bitmap in the WinForms tools has an SVG drawn as a simplified vector line icon: stroke based (1 unit, round caps and joins), a handful of nodes made of lines, arcs, circles and rounded rectangles, original colors, solid parts only where the bitmap is solid. A letter "A" is three lines, not a traced outline. Each SVG is aligned with its original so that the two overlay; the SVG view box is the size of the bitmap (16 by 16 for almost all icons, 24 by 24 for the transport icons, 75 by 21 for the transform charts, 7 by 7 and similar for the DarkUI glyphs, 48 by 48 for two TombIDE icons). `TombLib.Icons` and the pipeline clip the drawing to the view box declared by the SVG.

When the same icon is drawn differently in different applications, the 16 pixel bitmap of TombEditor keeps the plain id and the other drawings get a suffix: `General/create_new_Forms` (TombLib.Forms), `General/clipboard_Forms`, `Actions/Split_WadTool`, `Actions/Transport/Play24` (24 pixel), `General/search_TombIDE`, `Actions/cut_TombIDE`, `General/Open_TombIDE48`, `Misc/import_TombIDE48`. `legacy-icon-overrides.csv` holds these hand-reviewed mappings, the legacy files whose name has no SVG (`opened_folder-16`, `replace-16`, `volume_up-16`, `Camera-161`, `general_clipboard-161`, `general_Save As-16`), the glyph and message box bitmaps of the WinForms DarkUI controls (`Misc/DarkUI/*`) and the bitmap embedded in `FormMeshEditor.resx` (`Misc/hide_mesh`); `inventory` applies it. XAML must use the id of the drawing the WinForms tool shows (for example WadTool uses `Actions/Split_WadTool`, the animation editor transport uses `Actions/Transport/Play24`). WPF shows the four message box icons through `CMessageBox.*IconOverride` in `WPFInitializer`.

Review a drawing against its original with the headless helpers (`npm install` once in the `node` folder; they use resvg, so no desktop session is needed):

```powershell
$legacyRoot = Join-Path $env:TEMP ("TombLibLegacyExport-" + [Guid]::NewGuid().ToString('N'))
& TombLib/TombLib.Icons/Tools/IconPipeline/Export-LegacyIcons.ps1 -OutputDirectory $legacyRoot
dotnet run --project TombLib/TombLib.Icons/Tools/IconPipeline/IconPipeline.csproj -- inventory --legacy-root $legacyRoot --output TombLib/TombLib.Icons/Tools/IconPipeline/legacy-icon-map.csv
dotnet run --project TombLib/TombLib.Icons/Tools/IconPipeline/IconPipeline.csproj -- legacy-sources --legacy-root $legacyRoot --output (Join-Path $env:TEMP 'sources.csv')
node TombLib/TombLib.Icons/Tools/IconPipeline/node/compare.js --legacy-root $legacyRoot --sources (Join-Path $env:TEMP 'sources.csv') --ids "General/trash,Actions/cut" --out (Join-Path $env:TEMP 'compare.png')
node TombLib/TombLib.Icons/Tools/IconPipeline/node/buckets.js
```

`compare.js` draws, for up to eight ids, the original (8x), the SVG at the original size (8x), their overlay (red = original only, cyan = SVG only, white = both) and the SVG at a large size, and prints a mean error per icon (about 10 of 255 means the shapes overlay; the rest is anti-aliasing). `buckets.js` writes the `Png/@1x` to `@4x` rasters of every SVG. `compare-sheet`, `rasterize`, `contact-sheet` and `verify-legacy` do the same with the WPF renderer and need an interactive desktop session (off-screen WPF renders blank otherwise). Icons that exist only in SVG (no legacy bitmap) are listed in `WPF-PARITY-FIXES.md`.