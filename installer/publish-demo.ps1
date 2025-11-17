param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release",
    [string]$OutDir = "dist/OficinaDemo-win-x64",
    [int]$Port = 5076
)

Write-Host "Publicando aplicação self-contained para $Runtime ..."
dotnet publish `
  -c $Configuration `
  -r $Runtime `
  --self-contained true `
  /p:PublishSingleFile=true `
  /p:IncludeNativeLibrariesForSelfExtract=true `
  -o $OutDir

if ($LASTEXITCODE -ne 0) { throw "Falha na publicação." }

Write-Host "Gerando scripts de execução..."
$exe = Get-ChildItem "$OutDir" -Filter "*.exe" | Select-Object -First 1
Set-Content -Path (Join-Path $OutDir "start-demo.bat") -Value @"
@echo off
set PORT=%1
if "%PORT%"=="" set PORT=$Port
echo Iniciando Oficina Demo na porta %PORT% ...
start "Oficina Demo" "%~dp0$($exe.Name)" --urls http://localhost:%PORT%
timeout /t 2 >nul
start http://localhost:%PORT%
"@

Set-Content -Path (Join-Path $OutDir "README.txt") -Value @"
Oficina Demo
============

Como executar (sem instalar nada):
1) Dê duplo-clique em start-demo.bat (abre na porta $Port).
2) O navegador abrirá automaticamente em http://localhost:$Port

Observações:
- Este pacote é self-contained: não precisa do .NET instalado.
- Para mudar a porta: start-demo.bat 8080
"@

Write-Host "Pacote gerado em: $OutDir"
