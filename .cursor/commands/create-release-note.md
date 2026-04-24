---
description: Daily 노트를 모아 실행일 기준 릴리즈 노트 초안 생성
---

# /create-release-note

사용자가 `/create-release-note`를 실행했다. `99_ReleaseNote` 아래의 모든 `*_DAILY.md`를 읽고, **커맨드 실행일**(또는 사용자가 지정한 날짜)을 집계일로 하는 릴리즈 노트 **초안**을 만든다.

## 참조

- [tools/release_notes.py](../../tools/release_notes.py) — 집계 로직
- [05-git-workflow.mdc](../rules/05-git-workflow.mdc) — PowerShell(`;` 사용, `&&` 금지)

## 에이전트 실행 순서

1. 워크스페이스 **저장소 루트**에서 아래를 실행한다.

```powershell
Set-Location "<저장소 루트>"
python tools/release_notes.py create-release
```

2. **날짜 지정**(집계일·출력 파일명의 `YY.MM.DD`):

```powershell
python tools/release_notes.py create-release --date 26.04.24
```

3. **출력 경로 지정**(기본은 `99_ReleaseNote/YY.MM.DD_RELEASE_NOTES_DRAFT.md` — 수동 작성 `*_RELEASE_NOTES.md`와 구분):

```powershell
python tools/release_notes.py create-release -o "99_ReleaseNote/26.04.24_RELEASE_NOTES.md"
```

4. 실패 시(stderr·exit code) 원인만 짧게 전달한다. 성공 시 생성된 **상대 경로**를 알린다.

5. 초안이므로 사용자에게 **문구 정리·섹션 재구성**이 필요할 수 있음을 한 줄로 안내한다.

## 동작 요약

- `99_ReleaseNote/YY.MM.DD_DAILY.md` 전부를 날짜순으로 읽는다.
- 상단에 Daily에서 수집한 **커밋 제목** 목록, 이어서 **일별 Daily 원문**을 붙인 마크다운을 쓴다.
- Daily가 하나도 없으면 스크립트가 비정상 종료한다.
