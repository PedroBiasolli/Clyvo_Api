# =============================================================================
#  Clyvo Pet Care — script de validacao da Sprint 3
#  Roda restore, build e testes, gravando tudo em resultado-testes.txt (UTF-8)
#  Uso:  powershell -ExecutionPolicy Bypass -File .\run-tests.ps1
# =============================================================================

$ErrorActionPreference = 'Continue'
$ProgressPreference    = 'SilentlyContinue'

$root = $PSScriptRoot
$log  = Join-Path $root 'resultado-testes.txt'

# Forca UTF-8 na saida do dotnet e do arquivo de log
$OutputEncoding                   = [System.Text.UTF8Encoding]::new()
[Console]::OutputEncoding         = [System.Text.UTF8Encoding]::new()
$env:DOTNET_CLI_UI_LANGUAGE       = 'en'
$utf8 = [System.Text.UTF8Encoding]::new($false)

# Zera o log
[System.IO.File]::WriteAllText($log, '', $utf8)

function Write-Log {
    param([Parameter(ValueFromPipeline = $true)][string]$Text)
    process {
        Write-Host $Text
        [System.IO.File]::AppendAllText($log, "$Text`r`n", $utf8)
    }
}

Write-Log "============================================================"
Write-Log " Clyvo Pet Care - Validacao Sprint 3"
Write-Log " Data: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Write-Log " Pasta: $root"
Write-Log "============================================================"
Write-Log ""

Write-Log "--- SDKs .NET instalados ---"
dotnet --list-sdks 2>&1 | ForEach-Object { "$_" } | Write-Log
Write-Log ""

Set-Location $root

# --- 1. Restore -------------------------------------------------------------
Write-Log "============================================================"
Write-Log " ETAPA 1/3 - dotnet restore"
Write-Log "============================================================"
dotnet restore 2>&1 | ForEach-Object { "$_" } | Write-Log
$restoreExit = $LASTEXITCODE
Write-Log ""
Write-Log ">>> restore exit code: $restoreExit"
Write-Log ""

# --- 2. Build ---------------------------------------------------------------
Write-Log "============================================================"
Write-Log " ETAPA 2/3 - dotnet build"
Write-Log "============================================================"
dotnet build --no-restore 2>&1 | ForEach-Object { "$_" } | Write-Log
$buildExit = $LASTEXITCODE
Write-Log ""
Write-Log ">>> build exit code: $buildExit"
Write-Log ""

# --- 3. Testes --------------------------------------------------------------
if ($buildExit -eq 0) {
    Write-Log "============================================================"
    Write-Log " ETAPA 3/3 - dotnet test"
    Write-Log "============================================================"
    dotnet test --no-build 2>&1 | ForEach-Object { "$_" } | Write-Log
    $testExit = $LASTEXITCODE
    Write-Log ""
    Write-Log ">>> test exit code: $testExit"
} else {
    Write-Log "############################################################"
    Write-Log " Build falhou - testes nao foram executados."
    Write-Log "############################################################"
    $testExit = -1
}

# --- Resumo -----------------------------------------------------------------
Write-Log ""
Write-Log "============================================================"
Write-Log " RESUMO"
Write-Log "============================================================"
Write-Log ("restore : " + $(if ($restoreExit -eq 0) { "OK" } else { "FALHOU ($restoreExit)" }))
Write-Log ("build   : " + $(if ($buildExit   -eq 0) { "OK" } else { "FALHOU ($buildExit)" }))
Write-Log ("test    : " + $(if ($testExit    -eq 0) { "OK" } elseif ($testExit -eq -1) { "NAO EXECUTADO" } else { "FALHOU ($testExit)" }))
Write-Log "============================================================"

Write-Host ""
Write-Host "Resultado salvo em: $log" -ForegroundColor Cyan
