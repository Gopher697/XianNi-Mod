param(
    [Parameter(Mandatory = $true)]
    [string] $ModDir,

    [switch] $DryRun
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$sourceDll = Join-Path $repoRoot 'build_staging\XianniMod.dll'
$sourceLocales = @(
    Join-Path $repoRoot 'Locales\en.json'
    Join-Path $repoRoot 'Locales\cz.json'
)

if (-not (Test-Path -LiteralPath $sourceDll -PathType Leaf)) {
    throw "Missing build output: $sourceDll. Run 'dotnet build .\XianniMod.csproj -c Release' first."
}

if (-not (Test-Path -LiteralPath $ModDir -PathType Container)) {
    throw "Target mod directory does not exist: $ModDir"
}

foreach ($locale in $sourceLocales) {
    if (-not (Test-Path -LiteralPath $locale -PathType Leaf)) {
        throw "Missing locale file: $locale"
    }
}

$targetLocales = Join-Path $ModDir 'Locales'
$copyPlan = @(
    [pscustomobject]@{
        Source = $sourceDll
        Target = Join-Path $ModDir 'XianniMod.dll'
    }
)

foreach ($locale in $sourceLocales) {
    $copyPlan += [pscustomobject]@{
        Source = $locale
        Target = Join-Path $targetLocales (Split-Path -Leaf $locale)
    }
}

if ($DryRun) {
    Write-Host "Dry run: no files will be copied."
}
elseif (-not (Test-Path -LiteralPath $targetLocales -PathType Container)) {
    New-Item -ItemType Directory -Path $targetLocales | Out-Null
}

foreach ($item in $copyPlan) {
    $sourceInfo = Get-Item -LiteralPath $item.Source

    if (-not $DryRun) {
        Copy-Item -LiteralPath $item.Source -Destination $item.Target -Force
    }

    [pscustomobject]@{
        Source = $item.Source
        Target = $item.Target
        Size = $sourceInfo.Length
        LastWriteTimeUtc = $sourceInfo.LastWriteTimeUtc
        Copied = -not $DryRun
    }
}
