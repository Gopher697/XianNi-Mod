param(
    [Parameter(Mandatory = $true)]
    [string] $ModDir,

    [switch] $DryRun
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$sourceDll = Join-Path $repoRoot 'build_staging\XianniMod.dll'
$sourceLocalesDir = Join-Path $repoRoot 'Locales'
$sourceGameResourcesDir = Join-Path $repoRoot 'GameResources'
$sourceTitleDir = Join-Path $repoRoot 'Title'
$sourceLocales = Get-ChildItem -LiteralPath $sourceLocalesDir -Filter '*.json' -File |
    Sort-Object Name
$sourceFiles = @(
    [pscustomobject]@{
        Source = Join-Path $repoRoot 'mod.json'
        Target = Join-Path $ModDir 'mod.json'
    },
    [pscustomobject]@{
        Source = Join-Path $repoRoot 'default_config.json'
        Target = Join-Path $ModDir 'default_config.json'
    },
    [pscustomobject]@{
        Source = Join-Path $repoRoot 'icon.png'
        Target = Join-Path $ModDir 'icon.png'
    }
)
$sourceDirectories = @(
    [pscustomobject]@{
        Source = $sourceGameResourcesDir
        Target = Join-Path $ModDir 'GameResources'
    },
    [pscustomobject]@{
        Source = $sourceTitleDir
        Target = Join-Path $ModDir 'Title'
    }
)

if (-not (Test-Path -LiteralPath $sourceDll -PathType Leaf)) {
    throw "Missing build output: $sourceDll. Run 'dotnet build .\XianniMod.csproj -c Release' first."
}

if (-not (Test-Path -LiteralPath $ModDir -PathType Container)) {
    throw "Target mod directory does not exist: $ModDir"
}

if ($sourceLocales.Count -eq 0) {
    throw "No locale JSON files found in: $sourceLocalesDir"
}

foreach ($item in $sourceFiles) {
    if (-not (Test-Path -LiteralPath $item.Source -PathType Leaf)) {
        throw "Missing package file: $($item.Source)"
    }
}

foreach ($item in $sourceDirectories) {
    if (-not (Test-Path -LiteralPath $item.Source -PathType Container)) {
        throw "Missing package directory: $($item.Source)"
    }
}

$targetLocales = Join-Path $ModDir 'Locales'
$copyPlan = @(
    [pscustomobject]@{
        Type = 'File'
        Source = $sourceDll
        Target = Join-Path $ModDir 'XianniMod.dll'
    }
)

$copyPlan += $sourceFiles | ForEach-Object {
    [pscustomobject]@{
        Type = 'File'
        Source = $_.Source
        Target = $_.Target
    }
}

foreach ($locale in $sourceLocales) {
    $copyPlan += [pscustomobject]@{
        Type = 'File'
        Source = $locale.FullName
        Target = Join-Path $targetLocales $locale.Name
    }
}

$copyPlan += $sourceDirectories | ForEach-Object {
    [pscustomobject]@{
        Type = 'Directory'
        Source = $_.Source
        Target = $_.Target
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
    $size = $sourceInfo.Length
    $fileCount = 1

    if ($item.Type -eq 'Directory') {
        $children = Get-ChildItem -LiteralPath $item.Source -Recurse -File
        $size = ($children | Measure-Object -Property Length -Sum).Sum
        if ($null -eq $size) {
            $size = 0
        }
        $fileCount = $children.Count
    }

    if (-not $DryRun) {
        if ($item.Type -eq 'Directory') {
            if (-not (Test-Path -LiteralPath $item.Target -PathType Container)) {
                New-Item -ItemType Directory -Path $item.Target | Out-Null
            }
            Get-ChildItem -LiteralPath $item.Source -Force | ForEach-Object {
                Copy-Item -LiteralPath $_.FullName -Destination $item.Target -Recurse -Force
            }
        }
        else {
            $targetParent = Split-Path -Parent $item.Target
            if (-not (Test-Path -LiteralPath $targetParent -PathType Container)) {
                New-Item -ItemType Directory -Path $targetParent | Out-Null
            }
            Copy-Item -LiteralPath $item.Source -Destination $item.Target -Force
        }
    }

    [pscustomobject]@{
        Type = $item.Type
        Source = $item.Source
        Target = $item.Target
        Files = $fileCount
        Size = $size
        LastWriteTimeUtc = $sourceInfo.LastWriteTimeUtc
        Copied = -not $DryRun
    }
}
