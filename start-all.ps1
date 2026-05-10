$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$backendScript = Join-Path $root "start-backend.ps1"
$frontendScript = Join-Path $root "start-frontend.ps1"

function Get-TcpExcludedRanges {
  netsh interface ipv4 show excludedportrange protocol=tcp |
    Select-String '^\s*(\d+)\s+(\d+)' |
    ForEach-Object {
      [pscustomobject]@{
        Start = [int]$_.Matches[0].Groups[1].Value
        End = [int]$_.Matches[0].Groups[2].Value
      }
    }
}

function Test-TcpPortAvailable {
  param(
    [int]$Port,
    $ExcludedRanges
  )

  foreach ($range in $ExcludedRanges) {
    if ($Port -ge $range.Start -and $Port -le $range.End) {
      return $false
    }
  }

  $connection = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue

  return $null -eq $connection
}

function Get-FreeTcpPort {
  param(
    [int]$StartPort,
    $ExcludedRanges
  )

  for ($port = $StartPort; $port -lt 65000; $port++) {
    if (Test-TcpPortAvailable -Port $port -ExcludedRanges $ExcludedRanges) {
      return $port
    }
  }

  throw "Could not find a free TCP port starting from $StartPort."
}

$excludedRanges = Get-TcpExcludedRanges
$httpsPort = Get-FreeTcpPort -StartPort 51616 -ExcludedRanges $excludedRanges
$httpPort = Get-FreeTcpPort -StartPort ($httpsPort + 1) -ExcludedRanges $excludedRanges

$env:MYHEROES_API_HTTPS_URL = "https://localhost:$httpsPort"
$env:MYHEROES_API_HTTP_URL = "http://localhost:$httpPort"
$env:ASPNETCORE_URLS = "$($env:MYHEROES_API_HTTPS_URL);$($env:MYHEROES_API_HTTP_URL)"
$env:VITE_API_TARGET = $env:MYHEROES_API_HTTP_URL

Write-Host "Backend URLs: $($env:ASPNETCORE_URLS)"
Write-Host "Frontend API proxy target: $($env:VITE_API_TARGET)"

Start-Process powershell.exe -ArgumentList @(
  "-NoExit",
  "-ExecutionPolicy", "Bypass",
  "-File", $backendScript
) -WorkingDirectory $root -WindowStyle Normal

$backendReadyUrl = "$($env:MYHEROES_API_HTTP_URL)/api/scenarios/status"
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
