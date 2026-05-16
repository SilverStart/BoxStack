# Instruction Usage Log

This log tracks which project instructions, skill files, and harness guidance are
actually read and used during development. The goal is to identify the smallest
effective harness to reuse after this project ends.

## How to Record Usage

Add a short entry whenever an agent reads project instructions, skill files, or
workflow guidance as part of a task.

- **Applied**: The instruction directly changed a decision, edit, test, or response.
- **Referenced**: The instruction was read for orientation but did not materially
  change the work.
- **Not used**: The instruction or skill was considered but skipped, with a short
  reason.

Each entry should prefer evidence over ceremony: file path, why it was opened,
how it affected the task, and whether it looks worth keeping for future projects.

## Decision Tags

- **Keep**: Useful enough to carry into the next project with little change.
- **Trim**: Useful, but too broad or too noisy for routine work.
- **Merge**: Useful content should be folded into another file or skill.
- **Retire**: Rarely useful, stale, or not relevant to this project type.
- **Watch**: Too early to decide.

## Entries

### 2026-05-16 - active.md token-efficient format applied

**Task**: Reduce context spikes caused by long lines and accumulated history in
`production/session-state/active.md`.

| File or Skill | Status | Effect on Work | Future Harness Note |
| --- | --- | --- | --- |
| `production/session-state/active.md` | Applied | Replaced the long accumulated history section with a short hot-state snapshot for state recovery and next-task checks. | Keep |
| `production/session-state/history.md` | Created | Moved the previous long `Current Prototype` notes into a searchable archive that should not be read during routine status checks. | Keep |
| `.codex/docs/context-management.md` | Applied | Added explicit `active.md` size, line-length, and history-separation rules so context-diet workflows can work reliably. | Keep |

**Notes**:

- The failure mode was line-count based partial reads: 100 lines could still
  output tens of thousands of characters because many bullets were extremely long.
- Future active session files should stay under roughly 6,000 characters and
  avoid lines over 500 characters.

### 2026-05-16 - Low-token status check rule tightened

**Task**: Prevent repeated context spikes during "progress check" and
"next-task list" requests.

| File or Skill | Status | Effect on Work | Future Harness Note |
| --- | --- | --- | --- |
| `.codex/skills/context-diet/SKILL.md` | Applied | Confirmed the intended targeted-read workflow, but exposed that the previous response still read too much dashboard content. | Keep |
| `.codex/docs/context-management.md` | Applied | Added a mandatory low-token status/next-task check rule: use `rg` first, avoid full dashboard reads, cap section reads to 10-20 lines, and keep task lists short. | Keep |
| `production/instruction-usage-log.md` | Applied | Recorded the mistake and the durable rule change so future agents can carry the lesson across projects. | Keep |

**Notes**:

- The failure mode was not missing guidance; it was reading broad dashboard
  sections after already finding the target lines.
- For this project, status and next-task requests should avoid opening full
  playtest history, current-decision, or risk blocks unless explicitly needed.

### 2026-05-13 - Bootstrap instruction usage tracking

**Task**: Create a lightweight way to track which imported harness instructions
and skills are actually used during this Unity project.

| File or Skill | Status | Effect on Work | Future Harness Note |
| --- | --- | --- | --- |
| `AGENTS.md` | Applied | Confirmed Korean responses, approval-before-editing rule, and project documentation structure. | Keep |
| `.codex/docs/context-management.md` | Applied | Chosen as the home for the ongoing logging rule because `AGENTS.md` already points to it. | Keep |
| Skill files | Not used | No task-specific skill was needed; this was a small project-operations documentation change. | Watch |

**Notes**:

- This file starts as an operations log, not a formal audit report.
- Future entries should stay concise so the log remains useful at project end.
