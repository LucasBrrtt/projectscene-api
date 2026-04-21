param(
    [switch]$Reset
)

$ErrorActionPreference = "Stop"

$projectPath = Join-Path $PSScriptRoot "..\ProjectScene.DbSetup\ProjectScene.DbSetup.csproj"

if ($Reset) {
    dotnet run --project $projectPath -- --reset
}
else {
    dotnet run --project $projectPath
}
