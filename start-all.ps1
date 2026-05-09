$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$backendScript = Join-Path $root "start-backend.ps1"
$frontendScript = Join-Path $root "start-frontend.ps1"

Start-Process powershell.exe -ArgumentList @(
  "-NoExit",
  "-ExecutionPolicy", "Bypass",
  "-File", $backendScript
) -WorkingDirectory $root -WindowStyle Normal

$backendReadyUrl = "http://localhost:51617/api/scenarios/status"
$deadline = (Get-Date).AddSeconds(60)

do {
  try {
    $response = Invoke-WebRequest -Uri $backendReadyUrl -UseBasicParsing -TimeoutSec 2

    if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 500) {
      break
    }
  } catch {
    Start-Sleep -Seconds 1
  }

  if ((Get-Date) -gt $deadline) {
    throw "Backend did not become ready in 60 seconds."
  }
} while ($true)

Start-Process powershell.exe -ArgumentList @(
  "-NoExit",
  "-ExecutionPolicy", "Bypass",
  "-File", $frontendScript
) -WorkingDirectory $root -WindowStyle Normal
