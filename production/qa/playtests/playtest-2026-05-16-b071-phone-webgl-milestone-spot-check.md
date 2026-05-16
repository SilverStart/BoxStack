# B071 Phone WebGL 마일스톤 스팟 체크

## 세션 정보

- 날짜: 2026-05-16
- 빌드 마커: B071
- 기준 커밋: `f92af7c B071 수동 점검 수용 기록`
- 플랫폼: AIT WebGL Dev Server, phone browser
- 세션 유형: milestone browser/device spot check
- 해당 검증은 사용자의 수동 phone 테스트로 진행한다.

## 목적

Unity Editor Play Mode에서 수용된 B071 상태가 실제 phone WebGL 환경에서도 유지되는지 확인한다.

이번 체크는 새 튜닝을 찾기 위한 반복 플레이가 아니라, App-in-Toss에 가까운 브라우저/기기 조건에서 캐시, 한국어 폰트, safe-area, touch 입력, 진행 HUD, 에셋 로딩이 깨지지 않는지 보는 마일스톤 스팟 체크다.

## 빠른 준비

1. Unity에서 WebGL 빌드를 최신 B071 코드로 다시 만든다.
2. `AIT > Dev Server > Start Server`를 실행한다.
3. Dev Server 출력의 `Network:` URL을 확인한다.
4. phone이 PC와 같은 네트워크에 있는지 확인한다.
5. phone browser에서 `http://<PC LAN IP>:5173/index.html`을 연다.
6. 화면의 작은 빌드 마커가 `B071`인지 확인한다.

예시 URL:

```text
http://172.30.1.14:5173/index.html
```

phone에서는 `localhost`를 쓰지 않는다. phone의 `localhost`는 PC가 아니라 phone 자신이다.

## 체크 항목

- 페이지가 connection refused, timeout, infinite loading 없이 열린다.
- 빌드 마커가 `B071`로 보인다.
- HUD, stage select, result popup의 한국어가 깨지지 않는다.
- DNF BitBit v2와 Gmarket Sans Bold 적용이 phone browser에서도 어색하지 않다.
- `Assets/Resources/Prototype/...` 기반 블록/배경/UI 리소스가 placeholder 없이 보인다.
- 상단 stage badge와 중앙 progress panel이 phone safe-area에서 겹치지 않는다.
- 4, 6, 8, 10, 12칸 progress slot이 phone viewport에서 읽힌다.
- tap-to-drop 입력이 안정적으로 들어간다.
- stage badge를 눌러 stage select를 열 수 있다.
- stage select에서 개발용 `진행 초기화`와 `전체 해금`이 테스트 빌드 조건에 맞게 노출된다.
- result popup의 retry/next 버튼이 touch로 반응한다.
- stage 1, 5, 10, 15, 20이 적어도 짧게 플레이 가능하다.
- stack collapse 중 심한 frame drop, browser crash, freeze가 없다.
- browser 새로고침 후에도 캐시된 옛 빌드가 아니라 `B071`이 유지된다.

## 대표 스테이지 기록

| 스테이지 | 목표 박스 | 로드/입력 | HUD safe-area | 진행 슬롯 | 한국어/폰트 | 성능 | 메모 |
|---|---:|---|---|---|---|---|---|
| 1 | 4 | 미확인 | 미확인 | 미확인 | 미확인 | 미확인 | |
| 5 | 6 | 미확인 | 미확인 | 미확인 | 미확인 | 미확인 | |
| 10 | 8 | 미확인 | 미확인 | 미확인 | 미확인 | 미확인 | |
| 15 | 10 | 미확인 | 미확인 | 미확인 | 미확인 | 미확인 | |
| 20 | 12 | 미확인 | 미확인 | 미확인 | 미확인 | 미확인 | |

## 판정

- phone WebGL 로드/캐시: 미정
- phone safe-area HUD: 미정
- 한국어 폰트: 미정
- touch 입력: 미정
- WebGL 성능: 미정
- 즉시 수정 필요 여부: 미정
- 다음 행동: phone WebGL 스팟 체크 후 결정

## 기록 규칙

수동 phone WebGL 점검이 끝나면 이 파일의 대표 스테이지 기록과 판정 값을 갱신한다.

결과가 수용되면 `production/session-state/active.md`, `production/progress-dashboard.md`, `docs/workflow/ait-webgl-testing-notes.md`에도 milestone 결과를 함께 기록한다.
