<!-- STATUS -->
Epic: Prototype Harness
Feature: BoxStack 2D App-in-App Prototype
Task: Stage block width progression pass
Runtime Marker: B096
Latest Commit: 70a78cb B095 이후 상태 문서 동기화
Dirty Worktree: B096 stage block width progression accepted, pending commit
<!-- /STATUS -->

# Active Session State

이 문서는 현재 작업 재개에 필요한 핵심 상태만 유지한다. 긴 구현 히스토리와 과거 플레이테스트 기록은 `production/session-state/history.md` 또는 `production/progress-dashboard.md`에서 확인한다.

## Project Mode

- 캐주얼 게임, prototype-first 진행.
- 무거운 GDD/ADR/review 흐름은 사용자가 요청할 때만 적용한다.
- 개발 중 검증은 기본적으로 `dotnet build BoxStack.slnx`를 사용하고, 실제 조작감과 UI 감각은 사용자 주도 Unity Editor Play Mode 확인을 사용한다.

## Current Snapshot

- 현재 런타임 마커는 `B096`이다.
- 최신 커밋은 `70a78cb B095 이후 상태 문서 동기화`이다.
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
- B085의 이미 놓인 박스 x축 속도 감쇠/제한 실험은 모바일 테스트에서 부자연스럽게 느껴져 되돌렸다. 현재는 B084 물리/실패 판정 기준을 유지한다.
- B086은 낙하 속도 상한을 유지하면서, 떨어지는 박스가 기존 박스와 거의 닿기 직전 y축 속도를 1회 0으로 리셋해 박스끼리 충돌로 서로 좌우로 밀어내는 현상을 줄인다. 기존에 놓인 박스의 x축 속도나 Rigidbody constraint는 건드리지 않는다. 사용자 확인에서 B085보다 훨씬 괜찮다고 수용했다.
- 착지 정확도 감각을 진행 슬롯 색/테두리로 아주 작게 표시하는 B087 실험은 체감 변화가 약하고 굳이 필요 없는 효과로 판단되어 적용하지 않는다. 현재 런타임은 B086 기준으로 유지한다.
- B088은 타워 안정성 드라마 후보의 첫 번째 작은 실험이다. 새 HUD/게이지 없이 결과 팝업 문구만 탑 상태와 연결해, 클리어 본문은 `탑이 안정됐어요`, 바닥 접촉 실패 본문은 `탑이 무너졌어요`로 통일한다.
- B089에서 시도한 스테이지 도전 라벨은 철회했다. B090은 스테이지 선택 타일을 `01`, `02`, `03` 같은 큰 번호 중심으로 바꾸고, 현재/해금/잠김 상태는 라벨 텍스트 대신 색과 비활성 상태로 구분한다. 사용자 확인에서 이 방향이 더 낫다고 수용했다.
- B091은 기록 추구 후보의 첫 번째 작은 실험이다. 별도 점수/콤보/정확도 기록은 만들지 않고, 스테이지 클리어가 기존 최고 진행 지점을 갱신하는 경우 결과 팝업 본문에 `최고 기록 갱신` 한 줄만 추가한다. 사용자 확인에서 이 문구는 수용되었다.
- B092는 오디오/햅틱 후보 중 오디오만 아주 작게 확인하는 실험이다. 별도 음원 파일, Audio Mixer, 햅틱, 플랫폼 브리지는 추가하지 않고, 런타임 합성 효과음으로 안정 배치 click, 클리어 chime, 실패 thud만 재생한다.
- B093은 B092 배치 click을 도-레-미-파-솔-라-시-도 순서로 한 단계씩 올라가는 피아노풍 합성 톤으로 바꾼다. 실제 피아노 음원 파일은 추가하지 않고, 후반 9번째 이상 박스는 다음 옥타브의 레/미/파처럼 계속 상승한다.
- B094는 B093 음계 구조를 유지하면서 마스터 볼륨과 배치/클리어/실패 cue 볼륨을 올려 Editor Play에서 더 잘 들리게 한다. 사용자 확인에서 피드백 사운드는 마음에 들고 이 정도면 충분하다고 수용했다.
- B095는 성공 결과음을 더 강하게 만들기 위해 기존 짧은 clear chime을 도-레-미-파-솔-라-시-도 연속 상승 피아노풍 glissando로 바꾼다. 배치음과 실패음은 B094 기준을 유지한다. 사용자 확인에서 이대로 괜찮다고 수용했다.
- B096은 스테이지별 블록 시퀀스에서 좌우로 넓어지는 블록을 제거하고, 기본 정사각형 블록에서 좁은 블록과 더 좁은 블록으로만 난이도를 올린다. 사용자 확인에서 전체적으로 문제 없다고 수용했다.
- B085 WebGL 빌드는 테스트용으로 한 번 만들어졌고, B091 기준 WebGL 빌드도 한 번 새로 만들었다. 하지만 WebGL을 통한 실제 디바이스 테스트는 가장 마지막 단계에서 진행하기로 했으므로 B096 확인은 우선 Unity Editor Play Mode에서 진행한다.
- 스테이지 선택 팝업의 `진행 초기화`와 `전체 해금` 테스트 컨트롤은 현재 코드 기준 `Application.isEditor || Debug.isDebugBuild`일 때만 노출된다. B079 결정과 구현이 여전히 일치하므로 추가 런타임 변경 없이 유지한다.
- `Assets/Art/Prototype/...`와 `Assets/Resources/Prototype/...`의 parcel/background PNG는 현재 일시적으로 같은 복사본이다. 프로토타입 동안은 WebGL 포함 안정성을 위해 `Resources` 복사본을 유지한다. `docs/architecture/boxstack-prototype-asset-loading-decision.md` 기준으로 현재는 `Resources` 런타임 경로와 `BoxStackPrototypeAssetLoader` 교체 경계를 유지한다.
- `com.unity.addressables`는 현재 `Packages/manifest.json`에 없으며, 제품화 트리거가 생기기 전에는 패키지를 추가하지 않는다. 제품화 기본 후보는 Addressables가 아니라 씬/프리팹/ScriptableObject 직렬화 참조다.
- BoxStack 고유 정체성 후보는 `design/ui/boxstack-identity-directions-2026-05-25.md`에 정리 중이다. 색상/블록 외형 실험은 재개하지 않고, 우선순위는 착지 정확도 감각, 타워 안정성 드라마, 스테이지 프레이밍, 기록 추구, 오디오/햅틱 순서다.
- 스테이지 진행 저장은 현재 최고 해금 스테이지 번호 하나만 `BoxStackStageProgressStore`를 통해 `PlayerPrefs`에 저장한다. `docs/architecture/boxstack-stage-progress-storage-decision.md` 기준으로 현재 프로토타입에서는 충분하며, App-in-Toss 정책, 계정/기기 동기화, 저장 데이터 확장, migration/analytics/anti-tamper/server validation 요구가 생길 때만 내부를 교체한다.
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

- B096 스테이지별 블록 폭 규칙 변경은 사용자 확인에서 수용됐다. 다음 작업은 커밋 후 push 여부 결정 또는 남은 제품화 후보 중에서 고른다.

## Open Questions

- 실제 App-in-Toss MVP 에셋 목록이 고정되면 어떤 에셋은 직렬화 참조로 두고 어떤 에셋만 Addressables 후보로 둘지 분류해야 한다.
- 실제 App-in-Toss 저장 정책이 확정되면 `PlayerPrefs`를 계속 써도 되는지, 아니면 AIT 저장 브리지나 서버 저장으로 교체해야 하는지 확인해야 한다. 현재 프로토타입에서는 교체하지 않는다.
- WebGL/App-in-Toss에서 오디오 자동 재생, 음소거, 지연이 Editor Play와 다르게 동작하는지 마지막 실기기 단계에서 확인해야 한다.

## Risks

- `active.md`가 다시 긴 히스토리 누적 문서가 되면 상태 확인 요청마다 토큰을 크게 소모한다.
- B074 phone WebGL 평가는 수용됐지만, 이후 UI 레이아웃을 바꾸면 실제 모바일 WebGL에서 다시 확인해야 한다.
- B084 후반 12박스 스테이지 재확인은 수용되었다. 후반 난이도는 물리, 이동, clear 검증, 실패 규칙을 다시 바꿀 때만 재점검한다.
- B096 블록 폭 난이도는 Editor Play에서 수용됐지만, 이후 목표 박스 수나 물리 튜닝을 바꾸면 초반/중반/후반 대표 스테이지를 다시 확인해야 한다.

## References

- 긴 세션 히스토리: `production/session-state/history.md`
- 진행 대시보드: `production/progress-dashboard.md`
- 프로토타입 코드맵: `docs/architecture/boxstack-prototype-code-map.md`
- Unity CLI 사용 정책: `docs/workflow/unity-cli-connector.md`
