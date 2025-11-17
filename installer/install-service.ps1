param(
    [ValidateSet('install','uninstall')] [string]$Action = 'install',
    [string]$Runtime = 'win-x64',
    [string]$ServiceName = 'OficinaDemo',
    [string]$InstallDir = 'C:\OficinaDemo',
    [int]$Port = 8080
)

if ($Action -eq 'install') {
    Write-Host "Publicando pacote self-contained..."
    $outDir = Join-Path (Split-Path $MyInvocation.MyCommand.Path -Parent) "dist/OficinaService-$Runtime"
    dotnet publish -c Release -r $Runtime --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true -o $outDir
    if ($LASTEXITCODE -ne 0) { throw "Falha na publicação." }

    Write-Host "Copiando para $InstallDir ..."
    New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
    Copy-Item "$outDir\*" $InstallDir -Recurse -Force

    $exe = Get-ChildItem $InstallDir -Filter "*.exe" | Select-Object -First 1
    if (-not $exe) { throw "Executável não encontrado em $InstallDir" }

    $binPath = '"' + $exe.FullName + '" --urls http://+:' + $Port + ' '
    Write-Host "Criando serviço $ServiceName ..."
    sc.exe create $ServiceName binPath= $binPath start= auto | Out-Null
    sc.exe description $ServiceName "Oficina Demo (ASP.NET Core) - Porta $Port" | Out-Null

    Write-Host "Liberando porta no firewall..."
    netsh advfirewall firewall add rule name="$ServiceName ($Port)" dir=in action=allow protocol=TCP localport=$Port | Out-Null

    sc.exe start $ServiceName | Out-Null
    Write-Host "Instalação concluída. Acesse: http://localhost:$Port"
}
else {
    Write-Host "Parando e removendo serviço $ServiceName ..."
    sc.exe stop $ServiceName | Out-Null
    sc.exe delete $ServiceName | Out-Null
    Write-Host "Removendo regra de firewall..."
    netsh advfirewall firewall delete rule name="$ServiceName ($Port)" | Out-Null
    Write-Host "Concluído."
}
