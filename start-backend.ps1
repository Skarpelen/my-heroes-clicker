$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$apiProject = Join-Path $root "MyHeroesClicker.API\MyHeroesClicker.API.csproj"

Set-Location $root
$env:ASPNETCORE_ENVIRONMENT = "Development"

dotnet run --project $apiProject --launch-profile "MyHeroesClicker.API"