@echo off
setlocal
set "REPO_ROOT=%~dp0"
if "%REPO_ROOT:~-1%"=="\" set "REPO_ROOT=%REPO_ROOT:~0,-1%"
set "UNITY_PROJECT=%REPO_ROOT%\Moviles-TP2-Unity"
set "LOG_FILE=%UNITY_PROJECT%\Logs\headless-editor.log"

echo Checking for an existing Unity Editor on "%UNITY_PROJECT%"...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$ErrorActionPreference = 'Stop';" ^
  "$proj = ([System.IO.Path]::GetFullPath('%UNITY_PROJECT%')).TrimEnd('\','/');" ^
  "$status = unity status --json 2>$null | ConvertFrom-Json;" ^
  "if ($status -and $status.success -and $status.data.items) {" ^
  "  foreach ($item in $status.data.items) {" ^
  "    if ($item.projectPath -and ([System.IO.Path]::GetFullPath($item.projectPath)).TrimEnd('\','/').Equals($proj, [System.StringComparison]::OrdinalIgnoreCase)) {" ^
  "      Write-Host ('Unity Editor is already running (PID ' + $item.pid + ', port ' + $item.port + ').');" ^
  "      exit 0;" ^
  "    }" ^
  "  }" ^
  "}" ^
  "$verLine = Get-Content '%UNITY_PROJECT%\ProjectSettings\ProjectVersion.txt' | Where-Object { $_ -match '^m_EditorVersion:\s*(.+)$' } | Select-Object -First 1;" ^
  "$ver = if ($verLine -match '^m_EditorVersion:\s*(.+)$') { $matches[1].Trim() } else { '6000.3.8f1' };" ^
  "$editors = unity editors --format json | ConvertFrom-Json;" ^
  "$unityExe = ($editors.data.items | Where-Object { $_.version -eq $ver -and $_.location } | Select-Object -First 1).location;" ^
  "if (-not $unityExe) { $unityExe = 'C:\Program Files\Unity\Hub\Editor\' + $ver + '\Editor\Unity.exe' };" ^
  "if (-not (Test-Path $unityExe)) { throw ('Unity.exe not found for version ' + $ver + ' at ' + $unityExe) };" ^
  "New-Item -ItemType Directory -Force -Path (Split-Path '%LOG_FILE%') | Out-Null;" ^
  "$env:UNITY_MCP_ALLOW_BATCH = '1';" ^
  "Write-Host ('Starting headless Unity ' + $ver + ' with MCP enabled...');" ^
  "Start-Process -FilePath $unityExe -ArgumentList @('-batchmode', '-nographics', '-projectPath', $proj, '-logFile', '%LOG_FILE%') -WindowStyle Hidden;" ^
  "Write-Host 'Waiting for Pipeline server to become ready...';" ^
  "unity status --until-ready --project-path $proj;" ^
  "if ($LASTEXITCODE -ne 0) { throw ('Unity failed to become ready. Check log: %LOG_FILE%') };" ^
  "Write-Host 'Headless Unity Editor is ready. Log: %LOG_FILE%';"

if errorlevel 1 (
  echo Failed to start headless Unity Editor.
  pause
  exit /b 1
)
endlocal
