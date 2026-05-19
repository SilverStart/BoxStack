<!-- STATUS -->
Epic: Prototype Harness
Feature: BoxStack 2D App-in-App Prototype
Task: B083 settle-based next-box timing
Runtime Marker: B083
Latest Commit: 5fba60e B081 블록 색상 실험 중단 결정 기록
Dirty Worktree: B082/B083 documentation and runtime update in progress
<!-- /STATUS -->

# Active Session State

이 문서는 현재 작업 재개에 필요한 핵심 상태만 유지한다. 긴 구현 히스토리와 과거 플레이테스트 기록은 `production/session-state/history.md` 또는 `production/progress-dashboard.md`에서 검색으로 확인한다.

## Project Mode

- 캐주얼 게임, prototype-first 진행.
- 무거운 GDD/ADR/review 흐름은 사용자가 요청할 때만 적용한다.
- 개발 중 검증은 기본적으로 `dotnet build BoxStack.slnx`와 사용자 주도 Unity Editor Play Mode 확인을 사용한다.

## Current Snapshot

- 현재 런타임 마커는 `B083`이다.
- 최신 커밋은 `5fba60e B081 블록 색상 실험 중단 결정 기록`이다.
- 현재 비주얼 방향은 Ketchapp `Stack`을 참고한 2D 추상 블록, 세로 그라데이션 배경, borderless faux-glass HUD다.
- 블록 비주얼은 전체 크기를 사용하고, 콜라이더는 현재 코드 기준 `boxVisual.WorldSize * 0.98f`로 유지한다.
- 중앙 착지 피드백 토스트, 접촉 그림자, 블록 테두리선, 결과 팝업 상단 `CLEAR/MISS` 스탬프, 되돌리기 기능은 사용하지 않는다.
- 상단 중앙 진행 인디케이터는 현재 스테이지 primary/accent 계열 색상을 사용한다.
- B070 박스 코드/순서별 색 변주 실험은 쌓을 때 색상이 자연스럽게 이어지지 않아 되돌렸고, `dotnet build BoxStack.slnx` 재검증을 통과했다.
- B071은 좁은 모바일 safe-area에서 상단 스테이지 배지와 중앙 진행 패널이 겹치지 않도록 진행 패널 최대 폭을 safe-area 기준으로 제한했고, `dotnet build BoxStack.slnx` 검증을 통과했다.
- 사용자 주도 Unity Editor Play Mode 수동 점검에서 B071 HUD safe-area 처리, 4-12박스 진행 슬롯 가독성, no-undo 후반 난이도는 현재 기준 괜찮은 것으로 수용했다.
- B072 비색상 명암/질감 변주 실험은 색상 진행을 바꾸지는 않았지만 실제 체감이 어색해 미채택으로 결정했고, 코드는 B071 상태로 되돌렸다.
- `Assets/Art/Prototype/...`와 `Assets/Resources/Prototype/...`의 parcel/background PNG는 현재 해시와 용량이 같은 복사본이다. 프로토타입 동안은 WebGL 포함 안정성을 위해 `Resources` 복사본을 유지한다. 제품화 전환의 기본 후보는 씬/프리팹/ScriptableObject 직렬화 참조이며, 원격 다운로드/카탈로그 업데이트/스킨 또는 스테이지 팩 단위 로딩/빌드 크기 압박/App-in-Toss 패키징 요구가 생길 때만 Addressables 도입을 검토한다.
- 사용자 주도 phone WebGL 확인에서 B071은 상단 중앙 박스 인디케이터, 클리어 결과 팝업, 스테이지 선택 팝업이 화면 밖으로 잘리는 문제가 재현되었다.
- B073은 UI Toolkit `PanelSettings` 스케일링 환경에서 `Screen.safeArea` 픽셀 좌표를 패널 좌표로 변환해 `_safeRoot`, overlay, HUD, touch hit-test에 적용한다.
- B073 수정 후 `dotnet build BoxStack.slnx` 검증은 경고/오류 없이 통과했다.
- B074는 모바일에서 스테이지 선택 팝업이 하단 safe-area 끝에 너무 붙어 보이지 않도록 safe-area 높이 기준으로 상단 여백, 하단 여백, 패널 최대 높이, 타일 높이/간격을 다시 계산한다.
- B074 수정 후 `dotnet build BoxStack.slnx` 검증은 경고/오류 없이 통과했다.
- 사용자 주도 phone WebGL 확인에서 B074 스테이지 선택 팝업 하단 여백과 B073 safe-area 클리핑 수정은 현재 프로토타입 기준 수용했다.
- 사용자 주도 확인에서 B074 기준 후반 no-undo 난이도는 현재 기준 이 정도면 충분한 것으로 수용했다.
- 박스끼리 옆으로 밀어내는 현상을 마찰력으로 더 줄이거나 settled 박스 X 이동을 제약하는 실험은 이번 체크포인트에서는 진행하지 않는다.
- `com.unity.addressables`는 현재 `Packages/manifest.json`에 없으므로, 제품화 트리거가 생기기 전에는 새 패키지를 추가하지 않는다.
- 스테이지 진행 저장은 현재 최고 해금 스테이지 번호 하나만 `BoxStackStageProgressStore`를 통해 `PlayerPrefs`에 저장한다. 프로토타입 동안은 유지하고, App-in-Toss 생산 환경의 특정 저장 API 요구, 계정/기기 간 동기화, 별/재화/이벤트 진행 같은 확장 저장값이 생길 때만 내부 저장 방식을 교체한다.
- 반복 mobile WebGL 확인은 Unity AIT 메뉴 대신 공용 `Start-AitUnityDevServer.ps1` 또는 프로젝트 로컬 `tools/start-ait-dev-server.ps1`로 별도 PowerShell 서버를 띄우는 방식으로 진행한다.
- `design/ui/delivery-arcade-assets/` PNG 미니팩은 현재 Stack-like 2D 런타임 방향에 적용하지 않는 역사적 디자인 참고 자료로 둔다.
- B076 수동 점검에서 현재 B074 Stack-like 2D 화면의 폰트 가독성, 블록 판정 가독성, 배경 대비, HUD/stage-select 흐름, replay/브랜드핏은 모두 수용됐다.
- B077은 B076에서 이미 수용된 폰트/블록 가독성/HUD/stage-select/색 흐름 질문을 dashboard open question에서 닫는 문서 정리다.
- B078은 B074 후반 no-undo 수용으로 이미 답이 나온 B008/B012/B074 물리·속도·clear 판정 관련 stale 질문을 dashboard open question/risk에서 닫는 문서 정리다.
- B079는 Editor/development 빌드에서만 보이는 `진행 초기화`와 `전체 해금` 버튼을 현재 프로토타입 검증용 정책으로 유지하고, 별도 숨김 제스처는 외부 배포/패키지 테스트 단계에서 필요할 때만 다시 검토하기로 정리한다.
- B080은 현재 B074/B076 기준에서 배경을 screen-fixed Stack-like gradient로 유지하고, 스택 높이에 따른 스크롤/패럴랙스는 가독성 저하 위험이 있어 별도 시각 실험을 다시 열 때만 검토하기로 정리한다.
- B081은 블록 반복감/깊이를 개선하기 위한 색상, 명암, bevel, 질감 계열 실험을 더 진행하지 않기로 정리한다. 현재 B074/B076에서 수용한 자연스러운 stack gradient와 flat abstract block 기준을 유지한다.
- B082는 중앙 착지 피드백 토스트를 현재 프로토타입에서는 되살리지 않기로 정리한다. 피드백은 나중에 placement quality, combo, scoring 같은 실제 규칙이 생겼을 때만 다시 검토한다.
- B083은 박스를 놓은 뒤 고정 `DropSettleSeconds` 1초를 기다리는 방식 대신, 떨어진 박스와 기존 탑의 선형/각속도가 안정 기준 아래로 내려가 일정 시간 유지될 때 다음 박스를 생성하도록 바꾼다. 런타임 마커는 `B083`으로 올렸다.

## Active Decisions

- 사용자 응답은 한국어로 작성한다.
- 코드 주석과 public API 설명 주석은 한국어로 작성한다.
- 코드 변경 후 코드맵, 진행 대시보드, active 상태 문서 갱신 필요 여부를 확인한다.
- WebGL 반복 빌드는 비효율적이므로 개발 중에는 Unity Editor Play Mode 확인을 우선한다.
- phone WebGL 반복 확인이 필요하면 공용 `Start-AitUnityDevServer.ps1` 또는 `tools/start-ait-dev-server.ps1`를 먼저 실행하고, Unity에서는 WebGL 빌드만 다시 만든 뒤 브라우저를 새로고침한다.
- Unity CLI Connector는 에디터 직접 조작/검사가 필요할 때만 사용하고, 단순 Play Mode 검증용으로는 사용하지 않는다.
- 상태 확인/다음 작업 리스트업에서는 문서 전체 읽기를 피하고 `rg` 검색 결과와 직전 주요 구간만 사용한다.

## Next Action

- B083 드롭 안정 감지 방식이 너무 빠르거나 느리지 않은지 Unity Editor Play에서 확인한다.

## Open Questions

- 실제 App-in-Toss MVP 에셋 목록이 고정되면 어떤 에셋을 직렬화 참조로 옮기고 어떤 에셋만 Addressables 후보로 둘지 분류해야 한다.
- 실제 App-in-Toss 저장 정책이 확정되면 `PlayerPrefs`를 계속 쓸 수 있는지, 아니면 AIT 저장 브리지나 서버 저장으로 교체해야 하는지 확인해야 한다.

## Risks

- `active.md`가 다시 긴 히스토리 누적 문서가 되면 상태 확인 요청마다 토큰을 크게 소모한다.
- B074 phone WebGL 점검은 수용됐지만, 이후 UI 레이아웃을 바꾸면 실제 모바일 WebGL에서 다시 확인해야 한다.
- 후반 12박스 스테이지는 현재 기준 수용됐지만, 이후 물리/난이도 튜닝을 바꾸면 다시 확인해야 한다.

## References

- 긴 세션 히스토리: `production/session-state/history.md`
- 진행 대시보드: `production/progress-dashboard.md`
- 프로토타입 코드맵: `docs/architecture/boxstack-prototype-code-map.md`
- Unity CLI 사용 정책: `docs/workflow/unity-cli-connector.md`
