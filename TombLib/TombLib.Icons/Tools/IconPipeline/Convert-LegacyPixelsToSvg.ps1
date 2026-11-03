<#
.SYNOPSIS
Rebuilds the SVG icons so that they reproduce the legacy WinForms PNGs pixel for pixel.
.DESCRIPTION
Every matched legacy PNG of the inventory map is converted to an SVG made of one filled path per color (shape-rendering crispEdges),
in a view box of the PNG size. When one SVG id has several different legacy bitmaps, the preferred one (16x16, TombEditor first) keeps the id and
the bitmaps of the other tools get an id with a suffix (for example General/create_new_Forms). Writes the updated map that rasterize-legacy and
verify-legacy use. Run Export-LegacyIcons.ps1 and the inventory command first.
#>
param([string]$LegacyRoot,[string]$MapCsv,[string]$SvgRoot,[string]$MapOut,[string]$ReportPath,[string]$AdjustCsv = $null)
Add-Type -AssemblyName System.Drawing
$inScope = @('TombEditor', 'WadTool', 'SoundTool', 'TombLib/TombLib.Forms', 'TombIDE', 'TombLib/TombLib.Scripting', 'DarkUI')
$rankList = @('TombEditor', 'TombLib/TombLib.Forms', 'WadTool', 'SoundTool', 'TombIDE', 'TombLib', 'DarkUI')
function Rank($p) { for ($i = 0; $i -lt $rankList.Count; $i++) { if ($p.StartsWith($rankList[$i])) { return $i } }; return 9 }
function Label($p) {
    if ($p.StartsWith('TombLib/TombLib.Forms')) { return 'Forms' }
    if ($p.StartsWith('TombEditor')) { return 'TombEditor' }
    if ($p.StartsWith('WadTool')) { return 'WadTool' }
    if ($p.StartsWith('SoundTool')) { return 'SoundTool' }
    if ($p.StartsWith('TombIDE')) { return 'TombIDE' }
    if ($p.StartsWith('TombLib/TombLib.Scripting')) { return 'Scripting' }
    if ($p.StartsWith('DarkUI')) { return 'DarkUI' }
    return $null
}
function InScope($p) { foreach ($s in $inScope) { if ($p.StartsWith($s)) { return $true } }; return $false }

function PixelKey($bmp) {
    $sb = New-Object System.Text.StringBuilder
    for ($y = 0; $y -lt $bmp.Height; $y++) { for ($x = 0; $x -lt $bmp.Width; $x++) {
        $c = $bmp.GetPixel($x, $y)
        if ($c.A -eq 0) { $null = $sb.Append('0,') } else { $null = $sb.Append($c.ToArgb().ToString('x')).Append(',') }
    } }
    return "{0}x{1}#{2}" -f $bmp.Width, $bmp.Height, [BitConverter]::ToString([Security.Cryptography.SHA1]::Create().ComputeHash([Text.Encoding]::ASCII.GetBytes($sb.ToString()))).Replace('-', '').Substring(0, 12)
}

function WriteSvg($bmp, $outFile, $svgId) {
    $w = $bmp.Width; $h = $bmp.Height
    $byColor = @{}
    for ($y = 0; $y -lt $h; $y++) { for ($x = 0; $x -lt $w; $x++) {
        $c = $bmp.GetPixel($x, $y)
        if ($c.A -eq 0) { continue }
        $key = '{0:x2}{1:x2}{2:x2}{3:x2}' -f $c.R, $c.G, $c.B, $c.A
        $override = $adjust["$svgId|$x|$y"]
        if ($override) { $key = '{0:x2}{1:x2}{2:x2}{3:x2}' -f $override[0], $override[1], $override[2], $override[3] }
        if (-not $byColor.ContainsKey($key)) { $byColor[$key] = New-Object 'bool[,]' $h, $w }
        $byColor[$key][$y, $x] = $true
    } }
    $sb = New-Object System.Text.StringBuilder
    $null = $sb.Append("<svg xmlns=`"http://www.w3.org/2000/svg`" width=`"$w`" height=`"$h`" viewBox=`"0 0 $w $h`" shape-rendering=`"crispEdges`">`r`n")
    foreach ($key in ($byColor.Keys | Sort-Object)) {
        $mask = $byColor[$key]; $used = New-Object 'bool[,]' $h, $w; $d = New-Object System.Text.StringBuilder
        for ($y = 0; $y -lt $h; $y++) {
            $x = 0
            while ($x -lt $w) {
                if (-not $mask[$y, $x] -or $used[$y, $x]) { $x++; continue }
                $x2 = $x
                while ($x2 + 1 -lt $w -and $mask[$y, ($x2 + 1)] -and -not $used[$y, ($x2 + 1)]) { $x2++ }
                $y2 = $y
                while ($y2 + 1 -lt $h) {
                    $ok = $true
                    for ($xx = $x; $xx -le $x2; $xx++) { if (-not $mask[($y2 + 1), $xx] -or $used[($y2 + 1), $xx]) { $ok = $false; break } }
                    if (-not $ok) { break }
                    $y2++
                }
                for ($yy = $y; $yy -le $y2; $yy++) { for ($xx = $x; $xx -le $x2; $xx++) { $used[$yy, $xx] = $true } }
                $null = $d.Append("M$x $y" + "h$($x2 - $x + 1)" + "v$($y2 - $y + 1)" + "h-$($x2 - $x + 1)" + "z")
                $x = $x2 + 1
            }
        }
        $a = [Convert]::ToInt32($key.Substring(6, 2), 16); $op = ''
        if ($a -lt 255) { $op = ' fill-opacity="' + ([Math]::Round($a / 255.0, 4).ToString([Globalization.CultureInfo]::InvariantCulture)) + '"' }
        $null = $sb.Append("  <path fill=`"#$($key.Substring(0, 6))`"$op d=`"$($d.ToString())`"/>`r`n")
    }
    $null = $sb.Append("</svg>`r`n")
    $dir = Split-Path $outFile; if (-not (Test-Path $dir)) { New-Item -ItemType Directory $dir -Force | Out-Null }
    [IO.File]::WriteAllText($outFile, $sb.ToString(), (New-Object Text.UTF8Encoding $false))
}

$adjust = @{}
if ($AdjustCsv -and (Test-Path $AdjustCsv)) { foreach ($a in (Import-Csv $AdjustCsv)) { $adjust["$($a.svg_id)|$($a.x)|$($a.y)"] = @([int]$a.r, [int]$a.g, [int]$a.b, [int]$a.a) } }
$map = Import-Csv $MapCsv
$rowsOut = New-Object System.Collections.Generic.List[object]
$report = New-Object System.Collections.Generic.List[string]
$report.Add('svg_id,kind,source,width,height,files')
foreach ($row in ($map | Where-Object { $_.status -ne 'matched' })) { $rowsOut.Add($row) }
foreach ($group in ($map | Where-Object { $_.status -eq 'matched' } | Group-Object svg_id)) {
    $files = @()
    foreach ($r in $group.Group) {
        $bmp = New-Object System.Drawing.Bitmap (Join-Path $LegacyRoot ($r.legacy_png -replace '/', '\'))
        $files += [pscustomobject]@{ Rel = $r.legacy_png; W = $bmp.Width; H = $bmp.Height; Rank = (Rank $r.legacy_png); Key = (PixelKey $bmp); Bmp = $bmp }
    }
    $ui = @($files | Where-Object { $_.W -le 75 -and $_.H -le 75 })
    if ($ui.Count -eq 0) { $ui = $files }
    $pick = $ui | Sort-Object @{ Expression = { if ($_.W -eq 16 -and $_.H -eq 16) { 0 } else { 1 } } }, Rank, Rel | Select-Object -First 1
    $variantNames = @{}
    foreach ($artGroup in ($files | Group-Object Key)) {
        $members = @($artGroup.Group | Sort-Object Rank, Rel)
        $first = $members[0]
        if ($artGroup.Name -eq $pick.Key) { $targetId = $group.Name; $kind = 'primary' }
        else {
            $scoped = @($members | Where-Object { (InScope $_.Rel) -and $_.W -le 75 -and $_.H -le 75 })
            if ($scoped.Count -eq 0) {
                foreach ($m in $members) { $report.Add("$($group.Name),skipped,$($m.Rel),$($m.W),$($m.H),1") }
                foreach ($m in $members) { $rowsOut.Add([pscustomobject]@{ legacy_png = $m.Rel; svg_id = $group.Name; status = 'matched' }) }
                continue
            }
            $first = $scoped[0]
            $suffix = Label $first.Rel
            if ($first.W -ne $pick.W -or $first.H -ne $pick.H) { $suffix += "$($first.W)" }
            $targetId = "$($group.Name)_$suffix"; $kind = 'variant'
        }
        WriteSvg $first.Bmp (Join-Path $SvgRoot (($targetId -replace '/', '\') + '.svg')) $targetId
        foreach ($m in $members) { $rowsOut.Add([pscustomobject]@{ legacy_png = $m.Rel; svg_id = $targetId; status = 'matched' }) }
        $report.Add("$targetId,$kind,$($first.Rel),$($first.W),$($first.H),$($members.Count)")
    }
    foreach ($f in $files) { $f.Bmp.Dispose() }
}
$rowsOut | Select-Object legacy_png, svg_id, status | Export-Csv $MapOut -NoTypeInformation
$report | Set-Content $ReportPath -Encoding ASCII
"svgs written: $(($report | Where-Object { $_ -match ',(primary|variant),' }).Count)"
