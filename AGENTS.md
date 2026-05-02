# Codex Game Studios -- Unity Project Configuration

Indie Unity game development managed through coordinated Codex subagents.
Each agent owns a specific domain, enforcing separation of concerns and quality.

## Technology Stack

- **Engine**: Unity
- **Language**: C#
- **Version Control**: Git with trunk-based development
- **Build System**: Unity Editor, local CI, or Unity Cloud Build
- **Asset Pipeline**: Unity `Assets/`, `Packages/`, `ProjectSettings/`; use Addressables when the project needs explicit asset loading and memory control

> **Note**: Prefer Unity-specialist agents for engine work:
> `unity-specialist`, `unity-ui-specialist`, `unity-shader-specialist`,
> `unity-dots-specialist`, and `unity-addressables-specialist`.

## Unity Project Structure

Expected Unity project roots:

- `Assets/` -- game code, scenes, prefabs, ScriptableObjects, art, audio, VFX, shaders
- `Assets/Scripts/` -- runtime C# gameplay and engine code
- `Assets/Editor/` -- Unity editor tools
- `Assets/Tests/` or `Tests/` -- test assemblies, depending on the project setup
- `Packages/` -- Unity package manifest and lockfile
- `ProjectSettings/` -- Unity project settings
- `design/` -- GDDs, UX specs, art bible, asset specs, and registries
- `docs/` -- architecture docs, ADRs, engine references, workflow docs
- `production/` -- sprint plans, milestones, release tracking, session state

## Engine Version Reference

@docs/engine-reference/unity/VERSION.md

## Technical Preferences

@.codex/docs/technical-preferences.md

## Coordination Rules

@.codex/docs/coordination-rules.md

## Collaboration Protocol

User-driven collaboration, not autonomous execution.

- Before writing or editing project files, present a short draft/summary and ask approval.
- Multi-file changes require explicit approval for the full changeset.
- No commits without user instruction.
- User-facing responses must be written in Korean.

## Progress Dashboard Maintenance

`production/progress-dashboard.md`는 프로젝트 진행도의 한 눈에 보기 요약입니다.
다음 사건 직후 dashboard도 함께 갱신해 stale 상태를 방지합니다:

- 새 prototype 작성 또는 playtest verdict 확정
- 핵심 조작/룰 변경
- 현재 milestone 또는 다음 행동 변경
- 새 리스크 식별 또는 기존 리스크 등급 변경
- `production/session-state/active.md`의 STATUS block 변경

갱신 항목:

1. "Last updated" 헤더 날짜
2. "다음 즉시 행동"
3. "Prototype / Playtest history"
4. "현재 결정"
5. "Open Questions"
6. "리스크"

세션 종료 직전에 dashboard와 `production/session-state/active.md`의 일관성을 점검합니다.
Stop hook(`.claude/hooks/dashboard-staleness-check.sh`)이 reminder를 emit하면 다음 turn에서 dashboard를 갱신합니다.

## Coding Standards

@.codex/docs/coding-standards.md

Unity-specific defaults:

- Prefer data-driven gameplay values via ScriptableObjects, config assets, or serialized fields
- Avoid hardcoded tuning constants in runtime systems
- Keep runtime code out of `Assets/Editor/`
- Keep editor-only APIs behind editor assemblies or `#if UNITY_EDITOR`
- Use Unity Test Framework for edit-mode and play-mode tests
- Check `docs/engine-reference/unity/` before relying on Unity 6 APIs
- Prefer Addressables for large or dynamically loaded content
- Prefer UI Toolkit for new runtime UI unless the project has a reason to use UGUI

## Context Management

@.codex/docs/context-management.md

## First Session

If this is a brand-new Unity game, start with:

1. `/brainstorm` for the game concept
2. `/map-systems` after the concept is approved
3. `/art-bible` before asset production
4. `/create-architecture` before implementation
5. `/test-setup` before the first gameplay story

