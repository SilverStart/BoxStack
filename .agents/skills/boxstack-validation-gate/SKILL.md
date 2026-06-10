---
name: boxstack-validation-gate
description: "Choose the correct validation path for BoxStack Unity changes. Use after planning or implementing a BoxStack change, before handoff, before commit, or when deciding whether dotnet build, Unity refresh, Unity CLI Connector, WebGL, mobile, or user Play Mode checks are required."
argument-hint: "[change summary]"
user-invocable: true
allowed-tools: Read, Grep, Bash
---

# BoxStack Validation Gate

Classify the change and choose the lightest validation that preserves confidence.

## Phase 1: Identify Change Type

Inspect the intended or actual changed files with:

```powershell
git -C C:\unity\BoxStack status --short
```

Classify the change:

- C# runtime logic
- C# editor/build tooling
- Unity asset, prefab, scene, `.meta`, Resources, font, sprite, or config asset
- UI Toolkit layout/style or runtime UI state
- WebGL template, AIT bridge, build script, or browser/mobile behavior
- Documentation-only
- Skill, agent, hook, or operating-rule change

## Phase 2: Select Validation

Use the smallest sufficient gate:

| Change type | Required validation |
| --- | --- |
| Documentation-only | No build unless docs describe changed code behavior that must be checked |
| Skill/agent/rule change | Static readthrough plus `git status`; run a skill test only if the project provides one for the changed skill |
| C# runtime logic | `dotnet build BoxStack.slnx` |
| New, removed, or renamed C# files | Refresh/sync Unity project files if needed, then `dotnet build BoxStack.slnx` |
| Editor/build tooling | `dotnet build BoxStack.slnx`; run the build script only when the changed behavior itself is the script |
| Unity asset/config/meta change | Verify the asset path and `.meta` pairing; use Unity refresh only when project files or generated metadata require it |
| UI Toolkit layout or user-visible UI state | `dotnet build BoxStack.slnx`, then ask the user to confirm UI feel in Unity Editor Play Mode when layout, touch behavior, or readability changed |
| Gameplay feel, physics, difficulty, timing, touch input | `dotnet build BoxStack.slnx`, then user-led Unity Editor Play Mode check |
| WebGL template, AIT bridge, Resources loading, Korean fonts, safe area, cache/build marker | Escalate to milestone WebGL/browser/mobile spot check only when the change affects WebGL-specific behavior |

## Phase 3: Respect Project Policy

Do not use Unity CLI Connector just to run Play Mode validation. Use it only when active work requires inspecting or controlling the already-open Unity Editor.

Do not run WebGL builds for ordinary C# iteration. Use `tools/build-webgl.ps1` only for milestone spot checks or when WebGL build automation itself changed.

If validation requires the user's sensory judgment, state exactly what the user should check in Editor Play Mode, but do not claim the feel is accepted until the user says so.

## Phase 4: Report Gate

Return a Korean validation plan or result:

- Change type
- Required automated check
- Manual/user check, if any
- WebGL/mobile escalation, if any
- Documentation updates required, if any

Use verdict words:

- `PASS` when the required checks passed
- `NEEDS USER CHECK` when automated checks passed but user feel/play checks remain
- `BLOCKED` when required validation cannot run
- `NOT NEEDED` when a heavier validation path is intentionally skipped

## Phase 5: Documentation Follow-Up

If the change affects runtime marker, next action, accepted playtest baseline, risks, feature ownership, or execution flow, update or recommend updates to:

- `production/session-state/active.md`
- `production/progress-dashboard.md`
- `docs/architecture/boxstack-prototype-code-map.md`

Keep updates targeted. Do not rewrite long history sections unless the user asks.
