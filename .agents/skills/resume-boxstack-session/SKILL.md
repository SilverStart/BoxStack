---
name: resume-boxstack-session
description: "Resume the BoxStack Unity project quickly by reading only targeted project state, git status, current runtime marker, next action, risks, and validation policy. Use when starting or resuming work in C:\\unity\\BoxStack, when asked what to do next, or before planning a BoxStack change."
argument-hint: "[no arguments]"
user-invocable: true
allowed-tools: Read, Grep, Bash
---

# Resume BoxStack Session

Recover the current BoxStack working context without rereading long history files.

## Phase 1: Confirm Repository State

Run:

```powershell
git -C C:\unity\BoxStack status --short
git -C C:\unity\BoxStack log --oneline -5
```

Report whether the worktree is clean or dirty. If dirty, list only the changed paths and avoid assuming they are yours.

## Phase 2: Read Targeted State

Use `rg` before reading full files. Prefer targeted matches:

```powershell
rg -n "Runtime Marker|Latest Commit|Dirty Worktree|Next Action|Open Questions|Risks" C:\unity\BoxStack\production\session-state\active.md
rg -n "Next Immediate Action|Routine validation policy|Open Questions|Risks|current runtime build marker" C:\unity\BoxStack\production\progress-dashboard.md
```

Read full files only if the targeted snippets conflict or are too incomplete to answer the user.

## Phase 3: Restore Operating Rules

Check these rules before proposing or making changes:

- User-facing responses are Korean.
- Ask for approval before writing or editing project files unless the user already approved the exact changeset.
- Normal C# validation prefers `dotnet build BoxStack.slnx`.
- Do not use Unity CLI Connector only for Play Mode validation.
- WebGL/mobile checks are milestone spot checks, not routine iteration.
- User performs actual gameplay feel, UI feel, difficulty, and touch checks in Unity Editor Play Mode.
- Keep `production/session-state/active.md`, `production/progress-dashboard.md`, and the code map consistent when the status, next action, feature ownership, execution flow, or risks change.

## Phase 4: Produce Resume Summary

Return a concise Korean summary:

- Runtime marker
- Latest commit
- Worktree state
- Current next action
- Top risks or open questions
- Recommended immediate action
- Validation expectation for the recommended action

Do not include long playtest history unless the user asks.

## Phase 5: Handoff Decision

If the user asked only for status, stop after the summary.

If the user asked to continue implementation, propose the smallest next change and name:

- Expected touched files
- Success criterion
- Validation command or manual check needed
