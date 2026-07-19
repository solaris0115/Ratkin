---
name: create-release-note
description: Create a Ratkin user-facing release note draft from 80_ReleaseNote/*_DAILY.md. Use when the user invokes /create-release-note, $create-release-note, asks to create release notes from Daily notes, or asks for the Cursor create-release-note command to work in Codex.
---

# Create Release Note

Mirror `.cursor/commands/create-release-note.md` for Codex.

## Core Principles

- Write for mod users.
- Do not include commit titles, commit hashes, implementation details, or code-change narration.
- Summarize only what changed or what was fixed.
- Keep each item to one concise user-facing line.
- Do not use `90_Tools/release_notes.py create-release`; it is the old path for this workflow.

## Workflow

1. Read every `80_ReleaseNote/*_DAILY.md` file.
2. If there are no Daily files, tell the user there is nothing to draft.
3. Classify Daily content into these sections:
   - `버그 수정` for fixed issues.
   - `변경` for balance changes or behavior changes.
   - `추가` for new features or content.
4. Omit any empty section.
5. Write each item as a one-line user-facing summary.
6. Save the result to `80_ReleaseNote/YY.MM.DD_RELEASE_NOTES_DRAFT.md`.
   - Use the command execution date for `YY.MM.DD`.
   - If the user specifies a date, use that date instead.
7. Report the saved file path.

## Output Format

```markdown
# Ratkin 릴리즈 노트 - YY.MM.DD

## 버그 수정
- 소드오프 산탄총이 벽에 맞았을 때 벽 뒤로 피해가 전달되던 문제 수정
- 바이오텍 DLC 없이 인간에게 랫킨 귀가 붙는 버그 수정

## 변경
- 전투복과 가스마스크의 독성 환경 저항 수치 조정
- 랫홀릭 건의 사격 경험치 획득량 조정

## 추가
- 초월공학 꼬리 부위와 이식 수술 추가
```

## Style Checks

- Prefer plain Korean wording.
- Replace internal labels like `[AI]`, Def names, class names, and patch names with player-visible effects.
- Avoid overclaiming unfinished placeholder content as complete gameplay content.
