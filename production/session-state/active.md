<!-- STATUS -->
Epic: Prototype Harness
Feature: BoxStack 2D App-in-App Prototype
Task: choose next small polish target
Runtime Marker: B069
Latest Commit: 9396a91 B070 색상 변주 실험 미채택 기록
Dirty Worktree: clean
<!-- /STATUS -->

# Active Session State

이 문서는 현재 작업 재개에 필요한 핵심 상태만 유지한다. 긴 구현 히스토리와 과거 플레이테스트 기록은 `production/session-state/history.md` 또는 `production/progress-dashboard.md`에서 검색으로 확인한다.

## Project Mode

- 캐주얼 게임, prototype-first 진행.
- 무거운 GDD/ADR/review 흐름은 사용자가 요청할 때만 적용한다.
- 개발 중 검증은 기본적으로 `dotnet build BoxStack.slnx`와 사용자 주도 Unity Editor Play Mode 확인을 사용한다.

## Current Snapshot

- 현재 런타임 마커는 `B069`이다.
- 최신 커밋은 `9396a91 B070 색상 변주 실험 미채택 기록`이다.
- 현재 비주얼 방향은 Ketchapp `Stack`을 참고한 2D 추상 블록, 세로 그라데이션 배경, borderless faux-glass HUD다.
- 블록 비주얼은 전체 크기를 사용하고, 콜라이더는 현재 코드 기준 `boxVisual.WorldSize * 0.98f`로 유지한다.
- 중앙 착지 피드백 토스트, 접촉 그림자, 블록 테두리선, 결과 팝업 상단 `CLEAR/MISS` 스탬프, 되돌리기 기능은 사용하지 않는다.
- 상단 중앙 진행 인디케이터는 현재 스테이지 primary/accent 계열 색상을 사용한다.
- B070 박스 코드/순서별 색 변주 실험은 쌓을 때 색상이 자연스럽게 이어지지 않아 되돌렸고, `dotnet build BoxStack.slnx` 재검증을 통과했다.

## Active Decisions

- 사용자 응답은 한국어로 작성한다.
- 코드 주석과 public API 설명 주석은 한국어로 작성한다.
- 코드 변경 후 코드맵, 진행 대시보드, active 상태 문서 갱신 필요 여부를 확인한다.
- WebGL 반복 빌드는 비효율적이므로 개발 중에는 Unity Editor Play Mode 확인을 우선한다.
- Unity CLI Connector는 에디터 직접 조작/검사가 필요할 때만 사용하고, 단순 Play Mode 검증용으로는 사용하지 않는다.
- 상태 확인/다음 작업 리스트업에서는 문서 전체 읽기를 피하고 `rg` 검색 결과와 직전 주요 구간만 사용한다.

## Next Action

- 다음 작업은 모바일 HUD 안전영역 점검 또는 색 흐름을 끊지 않는 다른 반복감 완화 접근 중 하나를 고른다.

## Open Questions

- 블록 반복감 완화는 색 변주 대신 어떤 방식으로 다시 접근할까?
- `Assets/Resources/Prototype/...` 중복 리소스 구조는 프로토타입 동안 유지할까, 다음 제품화 단계에서 정리할까?

## Risks

- `active.md`가 다시 긴 히스토리 누적 문서가 되면 상태 확인 요청마다 토큰을 크게 소모한다.
- 모바일 실기기 safe-area와 WebGL 캐시 문제는 Editor Game view만으로는 완전히 확인되지 않는다.
- 후반 12박스 스테이지는 물리 흔들림이 커지면 타이밍 실력보다 운으로 느껴질 수 있다.

## References

- 긴 세션 히스토리: `production/session-state/history.md`
- 진행 대시보드: `production/progress-dashboard.md`
- 프로토타입 코드맵: `docs/architecture/boxstack-prototype-code-map.md`
- Unity CLI 사용 정책: `docs/workflow/unity-cli-connector.md`
