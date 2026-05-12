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
