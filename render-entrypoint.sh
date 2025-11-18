#!/usr/bin/env bash
set -euo pipefail

PORT_VALUE="${PORT:-8080}"

if [ -z "${ASPNETCORE_URLS:-}" ]; then
  export ASPNETCORE_URLS="http://0.0.0.0:${PORT_VALUE}"
fi

exec dotnet projetos.dll
