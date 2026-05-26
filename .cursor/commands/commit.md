# !커밋

`!커밋` 입력 시 이 커맨드를 실행합니다.

이 **Cursor 채팅 세션**에서 에이전트가 수정·추가한 파일만 스테이징하고 커밋합니다.

## 참조 규칙

- [05-git-workflow.mdc](../rules/05-git-workflow.mdc) — Safety, PowerShell, `!커밋` 메시지 형식
- [90_Tools/release_notes.py](../../90_Tools/release_notes.py) — 커밋 후 Daily 노트 추가

## 에이전트 실행 체크리스트

1. `git status`로 변경 파일 목록 확인
2. **이 세션에서 손댄 경로만** `git add` (세션에 없는 수정이 있으면 사용자에게 포함 여부 확인)
3. 커밋 메시지 작성 후 `git commit`
   - **제목**: `[AI] ` + 사용자 요청·작업 목적 한 줄 요약
   - **본문**(여러 주제/의미 단위일 때): 의도·이슈·배경만. 파일/라인 나열 금지
4. 커밋이 **성공한 직후**, 저장소 루트에서 Daily 노트에 HEAD를 기록한다.

```powershell
Set-Location "<저장소 루트>"
python 90_Tools/release_notes.py append-daily
```

- 파일은 `80_ReleaseNote/YY.MM.DD_DAILY.md` 형식이며, **커밋일**(git 커미터 날짜, `YY.MM.DD`) 기준으로 같은 날 커밋이 누적된다.
- `append-daily` 실패 시(git 없음 등) stderr를 보고 사용자에게만 알린다(커밋은 이미 완료된 상태이므로 되돌리지 않는다).
5. Daily·스크립트가 새로 생기면 필요 시 `git add` 후 **별도 커밋**으로 묶을지 사용자에게 묻거나, 다음 세션 `!커밋`에 포함할지 안내한다.
6. **push 하지 않음** (사용자가 따로 요청할 때만)

## 커밋 메시지 예시 (PowerShell)

워크스페이스 루트에서. 경로는 세션에서 실제로 바꾼 파일로 교체.

```powershell
git status
git add "path/to/file1" "path/to/file2"

$msg = @"
[AI] Git 워크플로에 AI 세션 전용 커밋 절차 추가

규칙 문서에 !커밋 흐름을 명시하고, Cursor 커맨드로 동일 절차를 호출할 수 있게 했다.
의도·이슈 중심 본문은 diff 중복을 줄이기 위함이다.
"@
git commit -m $msg
```

한 줄 제목만 쓸 때:

```powershell
git add "path/to/file"
git commit -m "[AI] 터널 건물 Def 설명 및 비용 밸런스 조정"
```

## 주의

- `git reset --hard`, `git clean -fd` 등 파괴적 작업은 [05-git-workflow.mdc](../rules/05-git-workflow.mdc) 금지 목록 준수
