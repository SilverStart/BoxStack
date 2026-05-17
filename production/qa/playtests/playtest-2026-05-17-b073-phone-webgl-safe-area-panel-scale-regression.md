# B073 Phone WebGL safe-area 패널 스케일 회귀 점검

## 세션 정보

- 날짜: 2026-05-17
- 빌드 마커: B073
- 플랫폼: AIT WebGL Dev Server, phone browser
- 세션 유형: phone WebGL safe-area regression check
- 배경: B071 phone WebGL에서 상단 중앙 박스 인디케이터, 클리어 결과 팝업, 스테이지 선택 팝업이 화면 밖으로 잘려 보였다.

## 목적

UI Toolkit `PanelSettings`가 `Scale With Screen Size`를 사용할 때 `Screen.safeArea` 픽셀 좌표를 패널 좌표로 변환한 B073 수정이 실제 phone WebGL에서 잘림 문제를 해결했는지 확인한다.

## 빠른 준비

1. Unity에서 WebGL 빌드를 최신 B073 코드로 다시 만든다.
2. `AIT > Dev Server > Start Server`를 실행한다.
3. phone browser에서 `http://<PC LAN IP>:5173/index.html`을 연다.
4. 화면의 작은 빌드 마커가 `B073`인지 확인한다.

## 체크 항목

- 상단 중앙 박스 인디케이터가 화면 위/좌우 밖으로 잘리지 않는다.
- stage badge와 중앙 progress panel이 서로 겹치지 않는다.
- 4, 6, 8, 10, 12칸 progress slot이 phone viewport 안에서 읽힌다.
- 스테이지 선택 팝업 전체가 화면 안에 보이고, 타일/진행 초기화/전체 해금/닫기 버튼이 잘리지 않는다.
- 클리어/실패 결과 팝업 전체가 화면 안에 보이고, 제목/본문/버튼이 잘리지 않는다.
- stage select, retry/next 버튼이 touch로 반응한다.
- B073 새로고침 후에도 캐시된 B071이 아니라 B073이 유지된다.

## 판정

- B073 build marker: 미정
- HUD safe-area: 미정
- stage select popup: 미정
- result popup: 미정
- touch 입력: 미정
- 즉시 추가 수정 필요 여부: 미정
- 다음 행동: B073 phone WebGL 재점검 후 결정
