[CmdletBinding()]
param(
    [string]$ConfigPath = (Join-Path $env:USERPROFILE ".codex\config.toml"),
    [string]$Configuration = "Debug",
    [switch]$NoBuild
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$projectDir = $PSScriptRoot
$projectPath = Join-Path $projectDir "CrazyStorm.McpServer.csproj"
$exePath = Join-Path $projectDir "bin\$Configuration\CrazyStorm.McpServer.exe"

if (-not $NoBuild) {
    dotnet msbuild $projectPath /p:Configuration=$Configuration /p:Platform=AnyCPU /nologo
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

if (-not (Test-Path -LiteralPath $exePath)) {
    throw "MCP server executable was not found: $exePath"
}

$configDir = Split-Path -Parent $ConfigPath
if (-not [string]::IsNullOrWhiteSpace($configDir) -and -not (Test-Path -LiteralPath $configDir)) {
    New-Item -ItemType Directory -Path $configDir | Out-Null
}

$content = ""
if (Test-Path -LiteralPath $ConfigPath) {
    $content = [System.IO.File]::ReadAllText($ConfigPath, [System.Text.Encoding]::UTF8)
}

$escapedExe = $exePath.Replace("'", "''")
$block = @"
[mcp_servers.crazystorm]
args = []
command = '$escapedExe'
enabled = true
startup_timeout_sec = 30
"@

$pattern = "(?ms)^\[mcp_servers\.crazystorm\]\r?\n.*?(?=^\[|\z)"
if ([regex]::IsMatch($content, $pattern)) {
    $content = [regex]::Replace($content, $pattern, $block.TrimEnd() + [Environment]::NewLine)
}
else {
    if (-not $content.EndsWith([Environment]::NewLine) -and $content.Length -gt 0) {
        $content += [Environment]::NewLine
    }
    $content += [Environment]::NewLine + $block.TrimEnd() + [Environment]::NewLine
}

[System.IO.File]::WriteAllText($ConfigPath, $content, [System.Text.Encoding]::UTF8)

Write-Host "Installed CrazyStorm MCP server into Codex config:"
Write-Host "  $ConfigPath"
Write-Host ""
Write-Host "Server command:"
Write-Host "  $exePath"
Write-Host ""
Write-Host "Run this to verify the executable:"
Write-Host "  `"$exePath`" --self-test"
Write-Host ""
Write-Host "Restart Codex so the new MCP tool list is loaded."
