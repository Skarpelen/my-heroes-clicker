$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$migratorProject = Join-Path $root "MyHeroesClicker.DbMigrator\MyHeroesClicker.DbMigrator.csproj"
$apiProject = Join-Path $root "MyHeroesClicker.API\MyHeroesClicker.API.csproj"
$databasePath = Join-Path $env:LOCALAPPDATA "MyHeroesClicker\clicker.sqlite"

Set-Location $root
$env:ASPNETCORE_ENVIRONMENT = "Development"

dotnet run --project $migratorProject -- --database $databasePath
dotnet run --project $apiProject --launch-profile "MyHeroesClicker.API"