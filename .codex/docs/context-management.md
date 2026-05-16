# Context Management

Context is the most critical resource in a Claude Code session. Manage it actively.

## File-Backed State (Primary Strategy)

**The file is the memory, not the conversation.** Conversations are ephemeral and
will be compacted or lost. Files on disk persist across compactions and session crashes.

### Instruction Usage Log

Maintain `production/instruction-usage-log.md` as a lightweight record of which
project instructions, skill files, and imported harness guidance are actually
read and used during work.

Update the log when an agent reads or considers instructions beyond the usual
top-level project context:

- Mark instructions as **Applied** when they changed a decision, edit, test, or
  response.
- Mark instructions as **Referenced** when they were read for orientation only.
- Mark instructions or skills as **Not used** when they were considered but
  skipped, with a short reason.
- Add a future-harness note such as **Keep**, **Trim**, **Merge**, **Retire**, or
  **Watch** so the useful parts can be carried into the next project.

### Session State File

Maintain `production/session-state/active.md` as a living checkpoint. Update it
after each significant milestone:

- Design section approved and written to file
- Architecture decision made
- Implementation milestone reached
- Test results obtained

The state file should contain: current task, progress checklist, key decisions
made, files being worked on, and open questions.

### Status Line Block (Production+ only)

When the project is in Production, Polish, or Release stage, include a structured
status block in `active.md` that the status line script can parse:

```markdown
<!-- STATUS -->
Epic: Combat System
Feature: Melee Combat
Task: Implement hitbox detection
<!-- /STATUS -->
```

- All three fields (Epic, Feature, Task) are optional — include only what applies
- Update this block when switching focus areas
- The status line displays it as a breadcrumb: `Combat System > Melee Combat > Hitboxes`
- Remove or empty the block when no active work focus exists

After any disruption (compaction, crash, `/clear`), recover state with targeted
reads first. Search the status block and relevant sections before reading the
full state file.

### Incremental File Writing

When creating multi-section documents (design docs, architecture docs, lore entries):

1. Create the file immediately with a skeleton (all section headers, empty bodies)
2. Discuss and draft one section at a time in conversation
3. Write each section to the file as soon as it's approved
4. Update the session state file after each section
5. After writing a section, previous discussion about that section can be safely
   compacted — the decisions are in the file

This keeps the context window holding only the *current* section's discussion
(~3-5k tokens) instead of the entire document's conversation history (~30-50k tokens).

## Proactive Compaction

- **Compact proactively** at ~60-70% context usage, not reactively at the limit
- **Use `/clear`** between unrelated tasks, or after 2+ failed correction attempts
- **Natural compaction points:** after writing a section to file, after committing,
  after completing a task, before starting a new topic
- **Focused compaction:** `/compact Focus on [current task] — sections 1-3 are
  written to file, working on section 4`

## Context Budgets by Task Type

- Light (read/review): ~3k tokens startup
- Medium (implement feature): ~8k tokens
- Heavy (multi-system refactor): ~15k tokens

## Low-Token Status and Next-Task Checks

진행상황 확인, 다음 작업 리스트업, 커밋 직후 상태 확인처럼 상태 요약만 필요한 요청은
대화 컨텍스트를 크게 쓰지 않는 경로를 기본으로 한다.

1. 먼저 `git status --short --branch`로 워크트리와 브랜치 상태만 확인한다.
2. `production/session-state/active.md`와 `production/progress-dashboard.md`는
   전체 읽기를 하지 않는다. 먼저 `rg -n`으로 `STATUS`, `Next Action`,
   `Next Immediate Action`, `Open Questions`, `Risks`의 라인 위치만 찾는다.
3. 검색 결과만으로 답할 수 있으면 문서 본문을 추가로 읽지 않는다.
4. 본문 확인이 꼭 필요하면 관련 섹션 주변 10-20줄만 읽는다. 20줄을 넘겨야 할 때는
   그 이유를 먼저 스스로 확인하고, 진행 히스토리나 전체 결정사항 블록을 열지 않는다.
5. `Prototype / Playtest History`, `Current Decisions`, `Risks` 전체 블록은 사용자가
   명시적으로 요구하거나 실제 변경 판단에 필수일 때만 읽는다.
6. 다음 작업 리스트는 기본적으로 3-5개만 제시하고, 오래된 히스토리 재요약은 생략한다.

이 규칙은 `context-diet` 스킬을 언급했는지와 무관하게 BoxStack의 상태 확인/다음 작업
요청에 항상 적용한다.

## Token-Efficient `active.md` Format

`production/session-state/active.md`는 현재 작업 재개에 필요한 핫 상태만 담는다.
긴 구현 히스토리, 과거 playtest 기록, 오래된 B-number 변경 내역은
`production/session-state/history.md` 또는 `production/progress-dashboard.md`로 분리한다.

- 권장 전체 크기: 6,000자 이하.
- 권장 줄 길이: 220자 이하. 500자를 넘는 줄은 원칙적으로 만들지 않는다.
- `## Current Prototype`처럼 계속 누적되는 장문 섹션을 만들지 않는다.
- 상태 확인과 다음 작업 리스트업에 필요한 정보는 `STATUS`, `Current Snapshot`,
  `Active Decisions`, `Next Action`, `Open Questions`, `Risks`, `References`에 짧게 나눈다.
- 오래된 상세 맥락이 필요하면 `rg -n`으로 history/dashboard에서 관련 키워드만 찾고,
  필요한 주변 구간만 읽는다.
- 문서 정리 후에는 문자 수와 긴 줄 수를 계측해서 포맷이 유지되는지 확인한다.

## Subagent Delegation

Use subagents for research and exploration to keep the main session clean.
Subagents run in their own context window and return only summaries:

- **Use subagents** when investigating across multiple files, exploring unfamiliar code,
  or doing research that would consume >5k tokens of file reads
- **Use direct reads** when you know exactly which 1-2 files to check
- Subagents do not inherit conversation history — provide full context in the prompt

## Compaction Instructions

When context is compacted, preserve the following in the summary:

- Reference to `production/session-state/active.md` (read it to recover state)
- List of files modified in this session and their purpose
- Any architectural decisions made and their rationale
- Active sprint tasks and their current status
- Agent invocations and their outcomes (success/failure/blocked)
- Test results (pass/fail counts, specific failures)
- Instruction usage log updates made during the session
- Unresolved blockers or questions awaiting user input
- The current task and what step we are on
- Which sections of the current document are written to file vs. still in progress

**After compaction:** Use targeted reads on `production/session-state/active.md`
first, then inspect any files being actively worked on. For state recovery or
next-task listing, search the status block, next action, open questions, and risks
before reading large files in full. The files contain the decisions; the
conversation history is secondary.

## Recovery After Session Crash

If a session dies ("prompt too long") or you start a new session to continue work:

1. The `session-start.sh` hook will detect and preview `active.md` automatically
2. Search the state file for the status block, next action, open questions, and
   risks before reading any large file in full
3. Read the partially-completed file(s) listed in the state
4. Continue from the next incomplete section or task
