# 정적 코드 검토 전용: 테스트, 모드 실행, DLL 생성, MSBuild/restore를 수행하지 않는다.
param(
    [string]$RimWorldDir = 'C:/Program Files (x86)/Steam/steamapps/common/RimWorld',
    [string]$HarmonyAssemblyPath = 'C:/Program Files (x86)/Steam/steamapps/workshop/content/294100/2009463077/1.7/Assemblies/0Harmony.dll',
    [switch]$DevFeatures
)

$ErrorActionPreference = 'Stop'
$auditRoot = Split-Path $PSScriptRoot -Parent
if ([System.Environment]::Version.Major -lt 10) {
    throw '정적 검토에는 .NET 10 SDK와 .NET 10 기반 PowerShell이 필요합니다.'
}
# PowerShell 자체의 Roslyn을 사용하여 SDK 버전과의 어셈블리 충돌을 피한다.
Add-Type -Path (Join-Path $PSHOME 'Microsoft.CodeAnalysis.dll')
Add-Type -Path (Join-Path $PSHOME 'Microsoft.CodeAnalysis.CSharp.dll')

$auditRefPackRoot = Join-Path ${env:ProgramFiles} 'dotnet/packs/Microsoft.NETCore.App.Ref'
$auditRefPack = Get-ChildItem $auditRefPackRoot -Directory | Where-Object Name -Like '10.*' |
    Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
if (!$auditRefPack) { throw '.NET 10 참조 팩을 찾을 수 없습니다.' }

$auditReferences = [System.Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]::new()
foreach ($auditDll in Get-ChildItem (Join-Path $auditRefPack.FullName 'ref/net10.0') -Filter '*.dll') {
    $auditReferences.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile($auditDll.FullName))
}
foreach ($auditDllName in 'Assembly-CSharp.dll', 'UnityStubs.dll', 'Waylith.dll', 'Waylith.UnityCompat.dll') {
    $auditReferences.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile((Join-Path $RimWorldDir $auditDllName)))
}
$auditReferences.Add([Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile($HarmonyAssemblyPath))

[string[]]$auditSymbols = @('RIMWORLD_1_7', 'TRACE')
if ($DevFeatures) { $auditSymbols += 'RATKIN_DEV_FEATURES' }
$auditParseOptions = [Microsoft.CodeAnalysis.CSharp.CSharpParseOptions]::Default.WithPreprocessorSymbols([System.Collections.Generic.IEnumerable[string]]$auditSymbols)
$auditTrees = [System.Collections.Generic.List[Microsoft.CodeAnalysis.SyntaxTree]]::new()
$auditDevFiles = @('AutoTests.cs', 'Patch_InfoCard_OpenHotkey.cs', 'Patch_InfoCard_HotSwap.cs')
foreach ($auditFile in Get-ChildItem (Join-Path $auditRoot 'Project/1.6/Source') -Recurse -Filter '*.cs') {
    if ($auditFile.FullName -match '[\\/](obj|bin)[\\/]') { continue }
    if (!$DevFeatures -and $auditFile.Name -in $auditDevFiles) { continue }
    $auditText = [System.IO.File]::ReadAllText($auditFile.FullName)
    $auditTrees.Add([Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($auditText, $auditParseOptions, $auditFile.FullName))
}
$auditOptions = [Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions]::new([Microsoft.CodeAnalysis.OutputKind]::DynamicallyLinkedLibrary).WithMetadataImportOptions([Microsoft.CodeAnalysis.MetadataImportOptions]::All)
$auditCompilation = [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create('NewRatkin.StaticReview', $auditTrees, $auditReferences, $auditOptions)
$auditDiagnostics = @($auditCompilation.GetDiagnostics() | Where-Object Severity -In @('Error', 'Warning') | Sort-Object { $_.ToString() } -Unique)
$auditDiagnostics | ForEach-Object { $_.ToString() }
$auditErrors = @($auditDiagnostics | Where-Object Severity -EQ 'Error').Count
$auditWarnings = @($auditDiagnostics | Where-Object Severity -EQ 'Warning').Count
Write-Output "Static review: sources=$($auditTrees.Count), errors=$auditErrors, warnings=$auditWarnings, dev=$($DevFeatures.IsPresent); no assembly emitted."
if ($auditErrors -gt 0) { exit 1 }
