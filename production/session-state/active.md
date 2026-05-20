<!-- STATUS -->
Epic: Prototype Harness
Feature: BoxStack 2D App-in-App Prototype
Task: Next task selection after B084 late-stage acceptance
Runtime Marker: B084
Latest Commit: e14c89c B084 바닥 접촉 실패 판정
Dirty Worktree: B084 checklist acceptance documentation pending commit
<!-- /STATUS -->

# Active Session State

이 문서는 현재 작업 재개에 필요한 핵심 상태만 유지한다. 긴 구현 히스토리와 과거 플레이테스트 기록은 `production/session-state/history.md` 또는 `production/progress-dashboard.md`에서 확인한다.

## Project Mode

- 캐주얼 게임, prototype-first 진행.
- 무거운 GDD/ADR/review 흐름은 사용자가 요청할 때만 적용한다.
- 개발 중 검증은 기본적으로 `dotnet build BoxStack.slnx`를 사용하고, 실제 조작감과 UI 감각은 사용자 주도 Unity Editor Play Mode 확인을 사용한다.

## Current Snapshot

- 현재 런타임 마커는 `B084`이다.
- 최신 커밋은 `e14c89c B084 바닥 접촉 실패 판정`이다.
- 현재 비주얼 방향은 Ketchapp `Stack`을 참고한 2D 추상 블록, 세로 그라데이션 배경, borderless faux-glass HUD다.
- 블록 비주얼은 전체 크기를 사용하고, 콜라이더는 현재 코드 기준 `boxVisual.WorldSize * 0.98f`로 유지한다.
- 중앙 착지 피드백 토스트, 접촉 그림자, 블록 테두리선, 결과 팝업 상단 `CLEAR/MISS` 스탬프, 되돌리기 기능은 사용하지 않는다.
- 상단 중앙 진행 인디케이터는 현재 스테이지 primary/accent 계열 색상을 사용한다.
- B074 phone WebGL 확인에서 stage-select bottom spacing과 B073 safe-area 처리는 현재 기준으로 수용되었다.
- B076 수동 확인에서 현재 B074 Stack-like 2D 화면의 폰트 가독성, 블록 접촉 가독성, 배경 대비, HUD/stage-select 흐름, replay/브랜드핏은 모두 수용되었다.
- B081은 블록 색상, 명암, bevel, 질감 계열 실험을 더 진행하지 않기로 정리했다.
- B082는 중앙 착지 피드백 토스트를 현재 프로토타입에서는 내지 않기로 정리했다.
- B083은 박스를 놓은 뒤 고정 `DropSettleSeconds` 1초를 기다리는 방식 대신, 떨어진 박스와 기존 탑의 선형/각속도가 안정 기준 아래로 내려가 일정 시간 유지될 때 다음 박스를 생성하도록 바꿨다.
- B084는 좌우 single-column 허용 오차를 넘으면 즉시 실패하던 규칙을 제거하고, 두 개 이상의 박스가 바닥 콜라이더에 닿으면 실패하도록 바꾼다. `STACK SPREAD` 실패에서는 원인 박스를 삭제하지 않고 남겨 정지시켜, 팝업 뒤에서도 실패 이유가 보이게 한다. 실패 이후 기존 `ResolveDrop` 코루틴이 이어져 새 박스를 생성하지 않도록 상태 guard도 추가했고, 사용자 테스트에서 실패 후 새 박스가 생성되지 않는 것을 확인했다.
- `StackLineTolerance` 튜닝 값은 B084 규칙에서 더 이상 쓰지 않으므로 config와 config asset에서 제거한다.
- `Assets/Art/Prototype/...`와 `Assets/Resources/Prototype/...`의 parcel/background PNG는 현재 일시적으로 같은 복사본이다. 프로토타입 동안은 WebGL 포함 안정성을 위해 `Resources` 복사본을 유지한다.
- `com.unity.addressables`는 현재 `Packages/manifest.json`에 없으며, 제품화 트리거가 생기기 전에는 패키지를 추가하지 않는다.
- 스테이지 진행 저장은 현재 최고 해금 스테이지 번호 하나만 `BoxStackStageProgressStore`를 통해 `PlayerPrefs`에 저장한다.
- 반복 mobile WebGL 확인은 Unity AIT 메뉴 대신 공용 `Start-AitUnityDevServer.ps1` 또는 프로젝트 로컬 `tools/start-ait-dev-server.ps1`로 별도 PowerShell 서버를 띄우는 방식을 우선한다.
- `design/ui/delivery-arcade-assets/` PNG 미니팩은 현재 Stack-like 2D 방향에 적용하지 않는 역사적 디자인 참고 자료로 둔다.

## Active Decisions

- 사용자 응답은 한국어로 작성한다.
- 코드 주석과 public API 설명 주석은 한국어로 작성한다.
- 코드 변경 시 코드맵과 상태 문서 갱신 필요 여부를 확인한다.
- WebGL 반복 빌드는 비효율적이므로 개발 중에는 Unity Editor Play Mode 확인을 우선한다.
- phone WebGL 반복 확인이 필요하면 공용 `Start-AitUnityDevServer.ps1` 또는 `tools/start-ait-dev-server.ps1`를 먼저 실행하고, Unity에서는 WebGL 빌드만 다시 만든 뒤 브라우저를 새로고침한다.
- Unity CLI Connector는 에디터 직접 조작/검사가 필요할 때만 사용하고, 단순 Play Mode 검증용으로는 사용하지 않는다.
- 상태 확인/다음 작업 리스트업에서는 문서 전체 읽기를 피하고 `rg` 검색 결과와 직전 주요 구간만 사용한다.

## Next Action

- B084 이후 후반 12박스 스테이지 난이도와 클리어 납득감 재확인은 사용자 수동 확인에서 문제 없음으로 수용되었고, 결과를 `production/qa/playtests/playtest-2026-05-20-b084-late-stage-floor-contact-fairness.md`에 기록했다. 다음 행동은 현재 문서 변경을 커밋한 뒤, 실제 App-in-Toss WebView 기준 확인 또는 다음 남은 작업을 선택하는 것이다.

## Open Questions

- 실제 App-in-Toss MVP 에셋 목록이 고정되면 어떤 에셋은 직렬화 참조로 두고 어떤 에셋만 Addressables 후보로 둘지 분류해야 한다.
- 실제 App-in-Toss 저장 정책이 확정되면 `PlayerPrefs`를 계속 써도 되는지, 아니면 AIT 저장 브리지나 서버 저장으로 교체해야 하는지 확인해야 한다.

## Risks

- `active.md`가 다시 긴 히스토리 누적 문서가 되면 상태 확인 요청마다 토큰을 크게 소모한다.
- B074 phone WebGL 평가는 수용됐지만, 이후 UI 레이아웃을 바꾸면 실제 모바일 WebGL에서 다시 확인해야 한다.
- B084 후반 12박스 스테이지 재확인은 수용되었다. 후반 난이도는 물리, 이동, clear 검증, 실패 규칙을 다시 바꿀 때만 재점검한다.

## References

- 긴 세션 히스토리: `production/session-state/history.md`
- 진행 대시보드: `production/progress-dashboard.md`
- 프로토타입 코드맵: `docs/architecture/boxstack-prototype-code-map.md`
- Unity CLI 사용 정책: `docs/workflow/unity-cli-connector.md`
