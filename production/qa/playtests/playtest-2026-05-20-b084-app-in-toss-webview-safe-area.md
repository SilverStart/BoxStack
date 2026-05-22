# B084 App-in-Toss WebView Safe-Area/Viewport 점검

## 세션 정보

- 날짜: 2026-05-20
- 빌드 마커: B084
- 기준 커밋: `619ac6c B084 후반 스테이지 점검 수용 기록`
- 플랫폼: 실제 App-in-Toss WebView 또는 App-in-Toss에 가장 가까운 테스트 WebView
- 입력: 실제 터치
- 세션 유형: 외부 WebView safe-area/viewport 스팟 체크
- 이 검증은 사용자의 수동 기기 확인으로 진행한다.

## 목적

B073/B074에서 phone WebGL 기준 safe-area clipping과 stage-select bottom spacing은 수용됐다.

하지만 실제 App-in-Toss WebView는 브라우저 주소창, 앱 컨테이너, in-app safe-area, viewport 계산 방식이 다를 수 있다. 이 점검은 새 튜닝을 찾기 위한 플레이테스트가 아니라, 실제 타깃 컨테이너에서도 HUD, stage select, result popup, touch input, Korean font, WebGL loading이 깨지지 않는지 확인하는 마일스톤 스팟 체크다.

## 현재 기준

- 런타임 마커: `B084`
- UI 기준: Stack-like 2D HUD, top-center progress panel, stage badge, stage-select popup, minimal result popup
- safe-area 처리: `Screen.safeArea`를 UI Toolkit panel 좌표로 변환해 safe root, overlay, HUD, touch hit-test에 적용
- stage-select 처리: safe area 안에서 하단 여백을 계산하고, 짧은 모바일 높이에서는 tile height/gap을 줄임
- 실패 규칙: 두 개 이상의 박스가 바닥에 닿으면 실패
- 후반 16-20 스테이지 B084 기준은 사용자 수동 점검에서 수용 완료

## 빠른 준비

1. Unity에서 B084 WebGL 빌드를 새로 만든다.
2. 가능하면 `Start-AitUnityDevServer.ps1` 또는 `tools/start-ait-dev-server.ps1`로 Unity와 분리된 dev server를 먼저 띄운다.
3. App-in-Toss WebView 또는 가장 가까운 테스트 WebView에서 빌드 URL을 연다.
4. 화면의 작은 빌드 마커가 `B084`인지 확인한다.
5. 이전 캐시가 의심되면 URL에 `?v=B084-webview-safe-area` 같은 cache-busting query를 붙인다.

## 평가 항목

- WebGL canvas가 WebView 안에서 전체 영역에 맞게 로드되는가
- 상단 stage badge가 노치/상단 inset/앱 헤더와 겹치지 않는가
- 상단 중앙 progress panel이 화면 밖으로 잘리거나 stage badge와 겹치지 않는가
- 4, 6, 8, 10, 12칸 progress slot이 WebView viewport에서 읽히는가
- stage-select popup이 위/아래로 잘리지 않고, 하단에 최소한의 숨 쉴 여백이 있는가
- result popup이 clear/fail 양쪽에서 화면 중앙에 들어오고 버튼이 잘리지 않는가
- stage-select tile, close button, result button이 실제 터치로 정상 동작하는가
- 한국어 폰트가 깨지거나 fallback 사각형으로 보이지 않는가
- B084 실패 후 새 박스가 다시 생성되지 않고 결과 팝업 상태로 멈추는가
- WebView 뒤로가기/새로고침/포커스 복귀 후 입력이 이상해지지 않는가

## 대상 시나리오 기록

| 시나리오 | 기대 결과 | 결과 | 메모 |
|---|---|---|---|
| 최초 로드 | WebGL canvas와 B084 marker가 보인다 | 미점검 | |
| Stage 1 시작 | stage badge와 4칸 progress panel이 safe-area 안에 있다 | 미점검 | |
| Stage 10 선택 | stage-select popup과 8칸 progress panel이 잘리지 않는다 | 미점검 | |
| Stage 20 선택 | 12칸 progress panel이 읽히고 stage badge와 겹치지 않는다 | 미점검 | |
| Stage select 닫기 | close button touch가 정상 동작한다 | 미점검 | |
| Clear popup | popup과 다음 스테이지 버튼이 화면 안에 있다 | 미점검 | |
| Fail popup | popup과 다시하기 버튼이 화면 안에 있고 실패 원인이 보인다 | 미점검 | |
| WebView 복귀 | 앱 전환/복귀 후 touch input이 계속 동작한다 | 미점검 | |

## 판정 기준

- 수용: App-in-Toss WebView에서도 HUD, stage select, result popup, touch input, Korean font가 잘리지 않고 정상 동작한다.
- 보류: 치명적 문제는 없지만 특정 기기/컨테이너에서 추가 반복 확인이 필요하다.
- 수정 필요: HUD, stage select, result popup 중 하나라도 실제 WebView 화면 밖으로 잘리거나 주요 touch input이 동작하지 않는다.

## 판정

Pending user-led App-in-Toss WebView check.

## 기록 규칙

수동 평가가 끝나면 이 파일의 대상 시나리오 기록과 판정 값을 갱신한다.

결과가 수용되면 `production/session-state/active.md`와 `production/progress-dashboard.md`에도 App-in-Toss WebView 기준 safe-area/viewport 확인 결과를 함께 기록한다.
