#!/usr/bin/env bash
set -euo pipefail

ROOT="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"
ACTIVE="$ROOT/production/session-state/active.md"

echo "[compact] After compaction: reload production/session-state/active.md before continuing."

if [ -f "$ACTIVE" ]; then
  echo "[compact] Active state preview:"
  sed -n '1,80p' "$ACTIVE"
else
  echo "[compact] production/session-state/active.md is missing. Recreate it from the latest known session state."
fi
