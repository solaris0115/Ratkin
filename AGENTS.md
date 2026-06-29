# Ratkin Codex Commands

- When the user enters `/create-release-note`, use `$create-release-note` and follow `.agents/skills/create-release-note/SKILL.md`.

# Ratkin Codex Auto Commit Rule

- At the end of any Codex task, if Codex changed, added, deleted, generated, or rebuilt files and the task is complete, automatically commit the Codex-made changes before the final response unless the user explicitly says not to commit.
- Use the project Cursor commit rules as the source of truth:
  - `.cursor/rules/05-git-workflow.mdc`
  - `.cursor/commands/commit.md`
- Before committing, run `git status` and stage only files changed by this Codex session.
- Never stage or commit pre-existing user changes, unrelated untracked files, or files changed by another actor. If a file contains mixed user and Codex edits and it cannot be safely staged without including user work, ask before committing.
- Commit message format must follow the Cursor rule:
  - title starts with `[AI] `
  - title summarizes the user request or task objective in one line
  - body is used only for multi-topic or split changes, and describes intent/background/issues, not file-by-file diff details
- After a successful task commit, run `python 90_Tools/release_notes.py append-daily` from the repository root. If this creates or changes Daily release note files, commit those Daily files separately as Codex-made changes, but do not run `append-daily` again for that Daily-only commit to avoid recursive release-note churn.
- Do not push unless the user explicitly asks.
