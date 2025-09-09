#!/bin/sh
set -e

mkdir -p /app/certs

# Nếu có PFX trong secrets -> giải mã ra file
if [ -n "$KESTREL_PFX_BASE64" ]; then
  echo "$KESTREL_PFX_BASE64" | base64 -d > /app/certs/server.pfx
fi

# Chỉ định cert mặc định cho Kestrel
export ASPNETCORE_Kestrel__Certificates__Default__Path=/app/certs/server.pfx
export ASPNETCORE_Kestrel__Certificates__Default__Password="${KESTREL_PFX_PASSWORD}"

# App vẫn nghe HTTP 8080 để Fly proxy vào
export ASPNETCORE_URLS=${ASPNETCORE_URLS:-http://+:8080}

exec dotnet FZ.WebAPI.dll
