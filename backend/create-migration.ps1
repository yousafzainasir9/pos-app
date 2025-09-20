#!/usr/bin/env pwsh
Write-Host "========================================"
Write-Host "   POS Migration Generator"
Write-Host "========================================"
Write-Host ""

if ($args.Count -eq 0) {
    Write-Host "Usage: ./create-migration.ps1 [MigrationName]"
    Write-Host "Example: ./create-migration.ps1 AddImageUrlToCategories"
    Write-Host ""
    exit 1
}

$migrationName = $args[0]
Write-Host "Creating migration: $migrationName"
Write-Host ""

Set-Location src/POS.Infrastructure
dotnet ef migrations add $migrationName -s ../POS.WebAPI

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "❌ Migration creation failed!" -ForegroundColor Red
    Write-Host ""
    pause
    exit 1
}

Write-Host ""
Write-Host "✅ Migration created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Now running migrator to apply the migration..."
Write-Host ""
Set-Location ../../
Set-Location src/POS.Migrator
dotnet run

pause
