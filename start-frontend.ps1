$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$webRoot = Join-Path $root "MyHeroesClicker.Web"

Set-Location $webRoot
npm run dev