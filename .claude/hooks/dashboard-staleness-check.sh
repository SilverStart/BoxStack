#!/usr/bin/env bash
set -euo pipefail

ROOT="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"
ACTIVE="$ROOT/production/session-state/active.md"
DASHBOARD="$ROOT/production/progress-dashboard.md"

if [ ! -f "$ACTIVE" ]; then
  echo "[dashboard] Reminder: production/session-state/active.md is missing. Recreate it before ending the session."
  exit 0
fi

if [ ! -f "$DASHBOARD" ]; then
  echo "[dashboard] Reminder: production/progress-dashboard.md is missing. Create it from the active session state before ending the session."
  exit 0
fi

if [ "$ACTIVE" -nt "$DASHBOARD" ]; then
  echo "[dashboard] Reminder: production/session-state/active.md is newer than production/progress-dashboard.md. Update the dashboard before ending the session."
fi
