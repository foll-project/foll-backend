<#
reset_migrations.ps1

Usage examples (run from project root where foll-backend.csproj is):
  # Backup migrations, remove them, drop DB (default)
  .\scripts\reset_migrations.ps1

  # Just backup and remove migrations, keep DB
  .\scripts\reset_migrations.ps1 -DropDatabase:$false

Parameters:
  -DropDatabase (switch) : drop the database (default: $true)
  -BackupMigrations (switch) : backup existing Migrations folder (default: $true)
  -RemoveMigrations (switch) : remove Migrations folder after backup (default: $true)
#>

param(
    [switch]$DropDatabase = $true,
    [switch]$BackupMigrations = $true,
    [switch]$RemoveMigrations = $true
)

Set-StrictMode -Version Latest

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path | Split-Path -Parent
Push-Location $projectRoot

Write-Host "Project root: $projectRoot"

$migrationsPath = Join-Path $projectRoot 'Migrations'
if (Test-Path $migrationsPath) {
    if ($BackupMigrations) {
        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        $backupPath = Join-Path $projectRoot "Migrations_backup_$timestamp"
        Write-Host "Backing up Migrations -> $backupPath"
        Move-Item -Path $migrationsPath -Destination $backupPath -Force
    }
    elseif ($RemoveMigrations) {
        Write-Host "Removing Migrations folder"
        Remove-Item -Path $migrationsPath -Recurse -Force
    }
}
else {
    Write-Host "No Migrations folder found."
}

function Get-ConnectionStringValues {
    $appsettings = Join-Path $projectRoot 'appsettings.json'
    if (-not (Test-Path $appsettings)) { return $null }
    $json = Get-Content $appsettings -Raw | ConvertFrom-Json
    $cs = $json.ConnectionStrings.DefaultConnection
    if (-not $cs) { return $null }

    $pairs = @{}
    $cs.Split(';') | ForEach-Object {
        if ($_ -match '=') {
            $kv = $_.Split('=',2)
            $pairs[$kv[0].Trim()] = $kv[1].Trim()
        }
    }
    return $pairs
}

$csvals = Get-ConnectionStringValues

if ($DropDatabase) {
    Write-Host "Dropping database..."
    # Try dotnet ef database drop first
    try {
        dotnet ef database drop --force --no-build 2>&1 | ForEach-Object { Write-Host $_ }
        Write-Host "dotnet ef database drop finished (or no database)."
    }
    catch {
        Write-Host "dotnet ef failed or not available. Trying psql drop (if credentials present)."
        if ($null -ne $csvals) {
            $db = $csvals['Database']
            $host = $csvals['Host']
            $port = $csvals['Port']
            $user = $csvals['Username']
            $pwd = $csvals['Password']

            if ($db -and $host -and $user) {
                $env:PGPASSWORD = $pwd
                $psqlArgs = @("-h", $host, "-p", $port, "-U", $user, "-c", "DROP DATABASE IF EXISTS $db;")
                Write-Host "Executing: psql $($psqlArgs -join ' ')"
                & psql @psqlArgs
                Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
            }
            else {
                Write-Host "Connection info incomplete; cannot drop DB via psql."
            }
        }
        else {
            Write-Host "No connection string found in appsettings.json"
        }
    }
}

if ($RemoveMigrations -and (Test-Path $migrationsPath)) {
    Write-Host "Removing (remaining) Migrations folder"
    Remove-Item -Path $migrationsPath -Recurse -Force
}

Pop-Location
Write-Host "Done. Review backups (if any) before committing changes."