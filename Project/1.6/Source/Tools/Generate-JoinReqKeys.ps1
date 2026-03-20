# JoinRequirement.xml에서 RK_JoinReq_* 키를 추출하여 JoinReqKeys.generated.cs 생성.
# 단일 소스: Project/Contents/Languages/*/Keyed/JoinRequirement.xml
param(
    [string]$XmlPath = "",
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projDir = Split-Path -Parent (Split-Path -Parent $scriptDir)
$projectDir = Split-Path -Parent (Split-Path -Parent $projDir)

if ([string]::IsNullOrEmpty($XmlPath)) {
    $XmlPath = Join-Path $projectDir "Contents\Languages\Korean\Keyed\JoinRequirement.xml"
}
if ([string]::IsNullOrEmpty($OutputPath)) {
    $OutputPath = Join-Path $projDir "WanderingTrader\JoinReqKeys.generated.cs"
}

$xmlPathResolved = Resolve-Path $XmlPath -ErrorAction SilentlyContinue
if (-not $xmlPathResolved) {
    Write-Error "JoinRequirement.xml not found: $XmlPath"
}

[xml]$xml = Get-Content $xmlPathResolved -Encoding UTF8
$prefix = "RK_JoinReq_"
$keys = @($xml.SelectNodes("//*[starts-with(name(), 'RK_JoinReq_')]") | ForEach-Object { $_.Name } | Sort-Object -Unique)

$sb = [System.Text.StringBuilder]::new()
[void]$sb.AppendLine("// Auto-generated from JoinRequirement.xml. Do not edit.")
[void]$sb.AppendLine("// Run: .\Tools\Generate-JoinReqKeys.ps1")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("namespace NewRatkin")
[void]$sb.AppendLine("{")
[void]$sb.AppendLine("	internal static partial class JoinReqKeys")
[void]$sb.AppendLine("	{")
[void]$sb.AppendLine("		public const string Prefix = `"$prefix`";")
[void]$sb.AppendLine("")

foreach ($key in $keys) {
    $suffix = $key.Substring($prefix.Length)
    $constName = $suffix -replace "[^a-zA-Z0-9_]", "_"
    [void]$sb.AppendLine("		public const string $constName = `"$key`";")
}

[void]$sb.AppendLine("	}")
[void]$sb.AppendLine("}")

$outDir = Split-Path -Parent $OutputPath
if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }
$sb.ToString() | Set-Content -Path $OutputPath -Encoding UTF8 -NoNewline
$sb.AppendLine() | Out-Null

Write-Host "Generated $($keys.Count) keys -> $OutputPath"
