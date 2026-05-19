# B076 시각 가독성 / 폰트 / 브랜드핏 수동 점검

## 세션 정보

- 날짜: 2026-05-19
- 런타임 마커: B074
- 문서 체크포인트: B076
- 기준 커밋: `64d10d3 B075 Delivery Arcade 에셋 참고 결정 기록`
- 플랫폼: Unity Editor Play Mode 우선, 필요 시 Phone WebGL 추가 확인
- 입력: 마우스/터치 에뮬레이션 또는 실제 터치
- 세션 유형: Stack-like 2D 시각 기준 수용 점검
- 해당 검증은 사용자의 수동 플레이로 진행한다.

## 목적

B074/B075 기준으로 safe-area, 스테이지 선택 하단 여백, 후반 no-undo 난이도, Delivery Arcade 에셋 보관 결정은 정리됐다.

이번 점검은 새 비주얼 실험을 추가하지 않고, 현재 Stack-like 2D 화면이 다음 프로토타입 단계로 넘어갈 만큼 읽히는지 확인한다. 특히 남은 open question 중 폰트 적합성, 추상 블록 충돌 가독성, 배경 대비, HUD/stage-select 사용성, replay desire, 브랜드핏을 한 번에 본다.

## 현재 기준

- UI 방향: Stack-like 2D, 배송/택배 문구 제거, borderless faux-glass HUD
- 블록: 코드 생성형 추상 컬러 블록, 전체 스택 그라데이션을 박스별 구간으로 나눠 사용
- 블록 경계: 별도 테두리, 접촉 그림자, top/right face split 없음
- 폰트: DNF BitBit v2는 HUD/title/button, Gmarket Sans Bold는 보조 텍스트, Noto Sans KR은 fallback
- 결과 팝업: 간단한 `클리어`/`실패` 문구와 stage-themed 버튼
- 피드백: 중앙 착지 토스트 없음
- 되돌리기: 없음

## 빠른 준비

1. Unity Editor에서 `SampleScene`을 연다.
2. Game view를 세로 모바일 비율로 둔다.
3. Play Mode를 시작한다.
4. 화면의 작은 빌드 마커가 `B074`인지 확인한다.
5. 스테이지 배지를 눌러 스테이지 선택을 열고, 필요하면 개발용 `전체 해금`을 사용한다.
6. stage 1, 5, 10, 15, 20을 최소 1회씩 플레이한다.
7. 시간이 있으면 phone WebGL에서도 stage 1과 stage 20만 짧게 spot-check한다.

## 점검 항목

- DNF BitBit v2가 작은 HUD 숫자와 버튼 텍스트에서도 읽히는가
- Gmarket Sans Bold가 결과 팝업/보조 문구에서 너무 무겁거나 앱처럼 보이지 않는가
- 추상 블록의 실루엣이 이동 중에도 충돌 위치를 충분히 보여주는가
- 쌓인 블록의 아래-어두움/위-밝음 색 흐름이 자연스럽게 이어지는가
- 배경 그라데이션이 블록 색과 겹쳐 플레이 중앙을 흐리지 않는가
- 상단 stage badge와 top-center progress panel이 게임 HUD처럼 읽히는가
- 스테이지 배지를 눌러 stage select를 여는 흐름이 프로토타입 테스트용으로 충분히 자연스러운가
- stage select와 result popup이 현재 faux-glass 스타일 안에서 너무 앱 설정창처럼 보이지 않는가
- 중앙 착지 토스트가 없어도 플레이 피드백이 부족하다고 느껴지지 않는가
- 실패 후 다시 한 번 하고 싶은가, 아니면 화면 톤이 동기를 충분히 만들지 못하는가
- 전체 인상이 `Stack` 참고는 느껴지되 BoxStack만의 작은 차이가 있는가

## 대표 스테이지 기록

| 스테이지 | 목표 박스 | 폰트 가독성 | 블록 판정 가독성 | 배경 대비 | HUD/진행 슬롯 | stage select 흐름 | replay/브랜드핏 | 메모 |
|---|---:|---|---|---|---|---|---|---|
| 1 | 4 | 수용 | 수용 | 수용 | 수용 | 수용 | 수용 |  |
| 5 | 6 | 수용 | 수용 | 수용 | 수용 | 수용 | 수용 |  |
| 10 | 8 | 수용 | 수용 | 수용 | 수용 | 수용 | 수용 |  |
| 15 | 10 | 수용 | 수용 | 수용 | 수용 | 수용 | 수용 |  |
| 20 | 12 | 수용 | 수용 | 수용 | 수용 | 수용 | 수용 |  |

## 판정 기준

- 수용: 현재 Stack-like 2D 방향, 폰트, HUD, stage select 흐름이 다음 단계로 넘어가도 될 만큼 읽힌다.
- 보류: 큰 문제는 없지만 폰트/블록/배경 중 하나가 더 많은 실제 기기 확인을 필요로 한다.
- 수정 필요: 폰트가 작게 읽히지 않거나, 블록 충돌 위치가 불명확하거나, 전체 톤이 게임보다 앱/테스트 화면처럼 보인다.

## 판정

Accepted by user-led check on 2026-05-19.

The user accepted all B076 visual readability and brand-fit criteria. Keep the current Stack-like 2D visual baseline, DNF BitBit v2 / Gmarket Sans Bold font pairing, abstract block readability, background contrast, HUD/stage-select flow, and no-toast feedback direction for the next checkpoint.

## 기록 규칙

수동 점검이 끝나면 이 파일의 대표 스테이지 기록과 판정 값을 갱신한다.

결과가 수용되면 `production/session-state/active.md`와 `production/progress-dashboard.md`에도 B076 시각 가독성/브랜드핏 점검 결과를 함께 기록한다.
