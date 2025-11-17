Param(
    [string]$Message = "Update"
)

$ErrorActionPreference = "Stop"

# Gera a tag com data/hora
$date = Get-Date -Format "yyyy-MM-dd"
$timeStamp = Get-Date -Format "HH:mm:ss"
$tag = "v" + (Get-Date -Format "yyyy.MM.dd.HHmm")

Write-Host "🔧 Criando versão: $tag" -ForegroundColor Cyan

# 🔹 Commit e Tag
git add .
git commit -m "$Message"
git tag $tag

# ---------------------------------------------
# 🔥 Atualização automática do CHANGELOG.md
# ---------------------------------------------
$changelogPath = "CHANGELOG.md"

if (Test-Path $changelogPath) {

    $newEntry = @"
## [$tag] - $date $timeStamp
- $Message

"@

    # Lê o changelog atual
    $current = Get-Content $changelogPath -Raw

    # Insere a nova entrada logo após o título "# Changelog"
    if ($current -match "# Changelog") {
        $updated = $current -replace "(# Changelog\s*)", "`$1`r`n$newEntry"
        $updated | Set-Content $changelogPath -Encoding UTF8
        Write-Host "✔ CHANGELOG atualizado" -ForegroundColor Green
    }
    else {
        # Caso raro: se não tiver título correto
        "$newEntry`r`n$current" | Set-Content $changelogPath -Encoding UTF8
        Write-Host "✔ CHANGELOG criado/atualizado" -ForegroundColor Green
    }
}
else {
    # Se não existir, ele cria o changelog do zero
    @"
# Changelog

$newEntry
"@ | Set-Content $changelogPath -Encoding UTF8
    Write-Host "📄 CHANGELOG criado" -ForegroundColor Green
}

Write-Host "✔ Versão gerada e registrada: $tag" -ForegroundColor Green
