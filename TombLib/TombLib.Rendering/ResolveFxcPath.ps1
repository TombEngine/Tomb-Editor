param(
    [string]$BinRoot,
    [string]$Architecture = "x64"
)

$searchRoots = @(
    $BinRoot,
    (Join-Path ${env:ProgramFiles(x86)} "Windows Kits\10\bin"),
    (Join-Path $env:ProgramFiles "Windows Kits\10\bin")
) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique

foreach ($root in $searchRoots)
{
    if (-not (Test-Path -LiteralPath $root))
    {
        continue
    }

    $directPath = Join-Path $root (Join-Path $Architecture "fxc.exe")

    if (Test-Path -LiteralPath $directPath)
    {
        Write-Output $directPath
        exit 0
    }

    $versionedPath = Get-ChildItem -LiteralPath $root -Directory -ErrorAction SilentlyContinue |
        Sort-Object Name -Descending |
        ForEach-Object {
            Join-Path $_.FullName (Join-Path $Architecture "fxc.exe")
        } |
        Where-Object { Test-Path -LiteralPath $_ } |
        Select-Object -First 1

    if ($versionedPath)
    {
        Write-Output $versionedPath
        exit 0
    }
}

exit 1
