#!/usr/bin/env bash
set -euo pipefail

ROOT="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"
ACTIVE="$ROOT/production/session-state/active.md"

if [ ! -f "$ACTIVE" ]; then
  echo "[session] production/session-state/active.md is missing. Recreate it before continuing prototype work."
  exit 0
fi

echo "[session] Current active state:"
sed -n '1,80p' "$ACTIVE"
