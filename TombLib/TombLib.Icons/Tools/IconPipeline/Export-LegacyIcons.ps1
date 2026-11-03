param(
    [string]$OutputDirectory = (Join-Path $env:TEMP 'TombLibLegacyReview\curated'),
    [string]$Commit = '8c32757740d1477fcaaa85934e327dee35dd33a2'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (& git -C $PSScriptRoot rev-parse --show-toplevel).Trim()
if ($LASTEXITCODE -ne 0) {
    throw 'Could not find the repository root from the script location.'
}

& git -C $repositoryRoot cat-file -e "$Commit^{commit}"
if ($LASTEXITCODE -ne 0) {
    throw "Legacy commit is not available locally: $Commit"
}

$destination = [IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path -LiteralPath $destination) {
    $existingFiles = Get-ChildItem -LiteralPath $destination -Force
    if ($existingFiles.Count -gt 0) {
        throw "Output directory must be empty: $destination"
    }
}
else {
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
}

$staging = Join-Path ([IO.Path]::GetTempPath()) ("TombLibLegacyExport-" + [Guid]::NewGuid().ToString('N'))
$archive = "$staging.zip"
$resourcePaths = @(
    'TombEditor/Resources/icons_actions',
    'TombEditor/Resources/icons_general',
    'TombEditor/Resources/misc',
    'TombEditor/Resources/icons_objects',
    'TombEditor/Resources/icons_sectortype',
    'TombEditor/Resources/icons_texture',
    'TombEditor/Resources/icons_toolbox',
    'TombEditor/Resources/icons_transport',
    'TombIDE/TombIDE.ProjectMaster/Resources',
    'TombIDE/TombIDE.ScriptingStudio/Resources',
    'TombIDE/TombIDE/Resources',
    'TombLib/TombLib.Forms/Resources',
    'TombLib/TombLib.Rendering/Rendering/ServiceObjectTextures',
    'TombLib/TombLib.Scripting.ClassicScript/Resources/Icons',
    'TombLib/TombLib.Scripting.Tomb1Main/Resources/Icons',
    'DarkUI/Example/Resources',
    'DarkUI/DarkUI/Resources',
    'WadTool/Resources',
    'SoundTool/Resources'
)

try {
    & git -C $repositoryRoot archive --format=zip --output=$archive $Commit @resourcePaths
    if ($LASTEXITCODE -ne 0) {
        throw "Could not archive legacy icon sources from $Commit."
    }

    New-Item -ItemType Directory -Path $staging -Force | Out-Null
    Expand-Archive -LiteralPath $archive -DestinationPath $staging -Force
    $svgRoot = Join-Path $repositoryRoot 'TombLib/TombLib.Icons/Svg'
    $svgNames = @{}
    Get-ChildItem -LiteralPath $svgRoot -Recurse -Filter '*.svg' | ForEach-Object {
        $svgNames[$_.BaseName.ToLowerInvariant()] = $true
    }
    $legacyAliases = @{
        'search-and-replace' = $true
    }

    $copied = 0
    Get-ChildItem -LiteralPath $staging -Recurse -Filter '*.png' | ForEach-Object {
        $stem = $_.BaseName -replace '(-|_)(16|24|32|48)$', ''
        $stem = $stem -replace '^(actions|general|misc|objects|sectortype|texture|toolbox)_', ''
        $isIconFolder = $_.FullName -match '[\\/]icons_[^\\/]+[\\/]'
        $matchesCurrentName = $svgNames.ContainsKey($stem.ToLowerInvariant()) -or $legacyAliases.ContainsKey($stem.ToLowerInvariant())
        if ($isIconFolder -or $matchesCurrentName) {
            $relative = $_.FullName.Substring($staging.Length).TrimStart([IO.Path]::DirectorySeparatorChar)
            $target = Join-Path $destination $relative
            New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
            Copy-Item -LiteralPath $_.FullName -Destination $target -Force
            $copied++
        }
    }

    # Icons that have no SVG of their own name but are used by the tools; the overrides file maps them to an SVG.
    $extraFiles = @(
        'WadTool/Resources/opened_folder-16.png',
        'WadTool/Resources/replace-16.png',
        'WadTool/Resources/volume_up-16.png',
        'WadTool/Resources/Camera-161.png',
        'TombLib/TombLib.Forms/Resources/general_clipboard-161.png',
        'TombLib/TombLib.Forms/Resources/general_Save As-16.png'
    )
    foreach ($extra in $extraFiles) {
        $source = Join-Path $staging ($extra -replace '/', [IO.Path]::DirectorySeparatorChar)
        $target = Join-Path $destination ($extra -replace '/', [IO.Path]::DirectorySeparatorChar)
        if ((Test-Path -LiteralPath $source) -and -not (Test-Path -LiteralPath $target)) {
            New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
            Copy-Item -LiteralPath $source -Destination $target -Force
            $copied++
        }
    }

    # Every glyph and message box icon of the WinForms DarkUI controls (arrows, close buttons, tick, tree nodes, message icons).
    Get-ChildItem -LiteralPath (Join-Path $staging 'DarkUI/DarkUI/Resources') -Filter '*.png' | ForEach-Object {
        $target = Join-Path $destination "DarkUI/DarkUI/Resources/$($_.Name)"
        if (-not (Test-Path -LiteralPath $target)) {
            New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
            Copy-Item -LiteralPath $_.FullName -Destination $target -Force
            $copied++
        }
    }
    # Bitmaps embedded in a form resource instead of the Resources folder.
    Add-Type -AssemblyName System.Drawing
    $embedded = @(
        @{ Resx = 'WadTool/Forms/FormMeshEditor.resx'; Name = 'butHide.Image'; Target = 'WadTool/Resources/embedded/FormMeshEditor-butHide.png' }
    )
    foreach ($item in $embedded) {
        $xmlText = (& git -C $repositoryRoot show "${Commit}:$($item.Resx)") -join "`n"
        [xml]$resx = $xmlText
        $entry = $resx.root.data | Where-Object { $_.name -eq $item.Name }
        $bytes = [Convert]::FromBase64String(($entry.value -replace '\s', ''))
        $target = Join-Path $destination ($item.Target -replace '/', [IO.Path]::DirectorySeparatorChar)
        New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
        $stream = New-Object IO.MemoryStream (, $bytes)
        $bitmap = New-Object System.Drawing.Bitmap $stream
        $bitmap.Save($target, [System.Drawing.Imaging.ImageFormat]::Png)
        $bitmap.Dispose(); $stream.Dispose()
        $copied++
    }
    [IO.File]::WriteAllText((Join-Path $destination 'source-commit.txt'), "$Commit`r`n", [Text.Encoding]::ASCII)
    Write-Output "Exported $copied legacy PNGs from $Commit to $destination"
}
finally {
    if (Test-Path -LiteralPath $archive) {
        Remove-Item -LiteralPath $archive -Force
    }
    if (Test-Path -LiteralPath $staging) {
        $resolvedStaging = (Resolve-Path -LiteralPath $staging).Path
        $resolvedTemp = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
        $tempPrefix = $resolvedTemp.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
        if ($resolvedStaging.StartsWith($tempPrefix, [StringComparison]::OrdinalIgnoreCase)) {
            Remove-Item -LiteralPath $resolvedStaging -Recurse -Force
        }
    }
}
