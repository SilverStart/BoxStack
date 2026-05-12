# AI Agent Operating Rules

These rules are model-neutral project operating rules. They apply to any AI
assistant working in this repository, including Codex, Claude Code, Gemini, and
other coding agents.

## Shared Rule Source

- Treat this document as the shared place for user preferences that should
  persist across AI tools.
- When the user says a behavior should happen automatically in the future,
  record the behavior here unless it belongs only to one specific tool.
- Tool-specific files may link back to this document, but should not be the only
  place where cross-agent rules are stored.

## Temporary Verification Artifacts

- Unity Editor Play Mode, Unity CLI Connector, WebGL smoke checks, screenshots,
  and performance/test probes may create temporary verification artifacts.
- After the relevant test or verification pass is complete, remove temporary
  artifacts that are not intended to become source assets or documented
  evidence.
- Keep generated files such as `.tmp/`, `Screenshots/`, and
  `Assets/Resources/PerformanceTestRun*.json` out of commits unless the user
  explicitly asks to preserve them as evidence.
- If a new recurring temporary artifact appears, add it to `.gitignore` and
  clean it up before handing work back to the user.

## Git Hygiene

- Do not commit generated, temporary, or local validation artifacts unless the
  user explicitly asks for them to be preserved.
- Before committing, check `git status --short -uall` and confirm that only
  intentional source, asset, documentation, and metadata changes are staged.
- Leave unrelated user changes untouched.

## Documentation Updates

- Keep `production/session-state/active.md` and
  `production/progress-dashboard.md` consistent when status, next action,
  playtest verdict, or risk changes.
- When a user preference affects future AI behavior across tools, update this
  document and reference it from tool-specific guidance when useful.
