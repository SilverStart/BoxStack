<!-- STATUS -->
Epic: Prototype Harness
Feature: BoxStack 2D App-in-App Prototype
Task: Result flow production refactor pass
Runtime Marker: B107
Latest Commit: This change set B107 결과 버튼 흐름 분리
Dirty Worktree: Clean after B107 result flow refactor commit
<!-- /STATUS -->

# Active Session State

이 문서는 현재 작업 재개에 필요한 핵심 상태만 유지한다. 긴 구현 히스토리와 과거 플레이테스트 기록은 `production/session-state/history.md` 또는 `production/progress-dashboard.md`에서 확인한다.

## Project Mode

- 캐주얼 게임, prototype-first 진행.
- 무거운 GDD/ADR/review 흐름은 사용자가 요청할 때만 적용한다.
- 개발 중 검증은 기본적으로 `dotnet build BoxStack.slnx`를 사용하고, 실제 조작감과 UI 감각은 사용자 주도 Unity Editor Play Mode 확인을 사용한다.

## Current Snapshot

- 현재 런타임 마커는 `B107`이다.
- 최신 변경 묶음은 `B107 결과 버튼 흐름 분리`이다.
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
- B097은 B086의 접촉 직전 y속도 리셋을 유지한 채 낙하 중력과 최대 낙하속도만 조금 올려 박스가 내려오는 템포를 빠르게 한다. 사용자 확인에서 현재 속도로 결정했다.
- B097 기준으로 핵심 조작감과 작은 정체성 실험이 충분히 안정됐으므로, 다음 작업 방향은 전면 재작성 없이 프로토타입 런타임을 단계적으로 프로덕션 코드 경계로 나누는 것이다. 계획은 `docs/architecture/boxstack-production-refactor-plan.md`에 둔다.
- B098은 프로덕션 리팩토링의 첫 코드 단계로, `BoxStackPrototype`에 있던 현재 스테이지/최고 해금 스테이지 인덱스와 선택/해금/초기화 흐름을 `BoxStackStageProgress`로 분리한다. `BoxStackStageProgressStore`의 `PlayerPrefs` 저장 방식과 저장 키는 유지한다.
- B098 사용자 Editor Play 확인은 완료됐다. 스테이지 선택, 클리어 후 다음 스테이지 해금, `진행 초기화`, `전체 해금`, 최종 클리어 후 처음부터 흐름 모두 문제 없음으로 수용했다.
- B099는 프로덕션 리팩토링 3단계로, `BoxStackPrototype` 안에 있던 박스 이탈, 드롭 실패, 바닥 다중 접촉 실패 판정을 `BoxStackRunRules`로 분리한다. 실패/성공 규칙, 5초 클리어 검증, 결과 UI/오디오, 스테이지 해금 흐름은 바꾸지 않았다.
- B099 사용자 Editor Play 확인은 완료됐다. 정상 클리어, 낙하 박스 놓침, 쌓인 박스 이탈, 바닥 2개 접촉 실패, 검증 중 실패, 해금/리플레이 흐름이 문제 없음으로 수용됐다.
- B100은 프로덕션 리팩토링 4단계의 첫 코드 단계로, 낙하 시작 Rigidbody 전환, 접촉 직전 y속도 리셋, 최대 낙하 속도 제한, 스택 안정 판정, 결과 진입 시 placed box 물리 정지를 `BoxStackDropPhysics`로 분리한다. B086 접촉 직전 y속도 리셋과 B097 낙하 템포, clear/fail 룰, UI/오디오 흐름은 바꾸지 않았다.
- B100 사용자 Editor Play 확인은 완료됐다. 일반 드롭, 접촉 직전 충격 완화, 낙하 속도 상한, 안정 후 다음 박스 생성, 실패 흐름, 클리어 흐름, 대표 스테이지 낙하 감각이 문제 없음으로 수용됐다.
- B101은 프로덕션 리팩토링 4단계의 남은 코드 단계로, 박스 오브젝트 생성, 비주얼 선택/적용, 콜라이더/Rigidbody 초기 설정, Stack-like 추상 블록용 그라데이션 스프라이트 생성을 `BoxStackBoxFactory`로 분리한다. 새 생산 후보 클래스에는 `Prototype` 이름을 붙이지 않는 원칙을 적용했다.
- B101 사용자 Editor Play 확인은 완료됐다. 초기/중반/후반 스테이지의 박스 생성, 비주얼, 드롭 감각, 실패/클리어 흐름이 문제 없음으로 수용됐다. Unity CLI Connector refresh 후 `dotnet build BoxStack.slnx`도 경고 0개, 오류 0개로 통과했다.
- B102는 프로덕션 리팩토링 5단계의 첫 코드 단계로, 카메라 생성/설정, 배경 생성/리사이즈, 바닥 오브젝트/콜라이더 생성, 스테이지 팔레트 기반 배경/바닥 갱신을 `BoxStackSceneComposition`으로 분리한다. `BoxStackPrototype`은 게임 루프, 입력, 드롭/클리어/실패 조정, UI/오디오 호출을 유지하고, 카메라와 바닥 콜라이더 참조만 받아 사용한다.
- B102 사용자 Editor Play 확인은 완료됐다. HUD 마커 B102, 배경/바닥/카메라 위치, 스테이지 변경 시 팔레트 갱신, 박스 낙하/착지/실패/클리어, 스테이지 선택/결과 팝업 흐름이 문제 없음으로 수용됐다. Unity CLI Connector refresh 후 `dotnet build BoxStack.slnx`도 경고 0개, 오류 0개로 통과했다.
- B103은 프로덕션 리팩토링 5단계의 남은 런타임 구성 경계로, UI host GameObject 생성/폰트 로드/UI 컴포넌트 초기화와 audio host GameObject 생성을 `BoxStackRuntimeHosts`로 분리한다. `BoxStackPrototype`은 UI/오디오 생성 세부를 모르고, 생성된 `BoxStackPrototypeUi`, `BoxStackPrototypeAudio` 참조만 받아 HUD 갱신과 사운드 재생 호출에 사용한다.
- B103 사용자 Editor Play 확인은 완료됐다. HUD 마커 B103, 스테이지 선택 팝업 열기/닫기/선택, 결과 팝업 버튼, 배치/클리어/실패 사운드가 문제 없음으로 수용됐다. Unity CLI Connector refresh 후 `dotnet build BoxStack.slnx`도 경고 0개, 오류 0개로 통과했다.
- B104는 프로덕션 리팩토링의 다음 작은 경계로, 드롭 전 active box의 좌우 왕복 이동, 화면 안전 이동 범위 계산, collider 기반 박스 반폭 계산을 `BoxStackActiveBoxMotion`으로 분리한다. `BoxStackPrototype`은 현재 active box, 카메라, 스폰 높이, 시작 시간, 스테이지/튜닝 값을 전달하고 이동 세부 계산은 새 클래스에 위임한다.
- B104 사용자 Editor Play 확인은 완료됐다. 런타임 마커 B104, 스테이지 1 좌우 왕복 이동, 박스 배치/낙하/착지 사운드, 후반 스테이지 화면 밖 이탈 여부, 클리어/실패 결과 팝업이 문제 없음으로 수용됐다. Unity CLI Connector refresh 후 `dotnet build BoxStack.slnx`도 경고 0개, 오류 0개로 통과했다.
- B105는 프로덕션 리팩토링의 다음 작은 경계로, 목표 박스 수 달성 후 클리어 검증 타이머 시작/초기화/완료 판단을 `BoxStackClearValidation`으로 분리한다. `BoxStackPrototype`은 검증 상태 진입, 스택 실패 판정, 성공/실패 결과 처리만 계속 조정한다.
- B105 사용자 Editor Play 확인은 완료됐다. 런타임 마커 B105, 클리어 조건 달성 후 `VERIFYING` 상태, 검증 시간 후 성공 팝업/성공음, 검증 중 탑 붕괴 실패 팝업, 결과 팝업의 재시작/다음 스테이지 흐름이 문제 없음으로 수용됐다. Unity CLI Connector refresh 후 `dotnet build BoxStack.slnx`도 경고 0개, 오류 0개로 통과했다.
- B106은 프로덕션 리팩토링의 다음 작은 경계로, 스테이지 선택 팝업을 열 때의 이전 런 상태와 이전 `Time.timeScale` 저장/복원 책임을 `BoxStackStageSelectSession`으로 분리한다. `BoxStackPrototype`은 스테이지 선택 상태 전환, UI 갱신, 스테이지 선택/재시작 흐름만 계속 조정한다.
- B106 사용자 Editor Play 확인은 완료됐다. 런타임 마커 B106, 플레이 중 스테이지 선택 팝업 일시정지/복귀, 결과 팝업 상태에서 열기/닫기, 스테이지 타일 선택 후 재시작 흐름이 문제 없음으로 수용됐다. Unity CLI Connector refresh 후 `dotnet build BoxStack.slnx`도 경고 0개, 오류 0개로 통과했다.
- B107은 프로덕션 리팩토링의 다음 작은 경계로, 결과 팝업 버튼을 눌렀을 때의 액션 결정 책임을 `BoxStackResultFlow`로 분리한다. `BoxStackPrototype`은 결정된 액션에 따라 기존 `ChangeStage`, `SelectFirstStage`, `RestartGame` 호출만 실행한다.
- B107 사용자 Editor Play 확인은 완료됐다. 런타임 마커 B107, 클리어 후 다음 스테이지 이동, 최종 클리어 후 1스테이지 재시작, 실패 후 현재 스테이지 재시작, 결과 팝업 외 상태에서의 비발동 흐름이 문제 없음으로 수용됐다. Unity CLI Connector refresh 후 `dotnet build BoxStack.slnx`도 경고 0개, 오류 0개로 통과했다.
- B085 WebGL 빌드는 테스트용으로 한 번 만들어졌고, B091 기준 WebGL 빌드도 한 번 새로 만들었다. 하지만 WebGL을 통한 실제 디바이스 테스트는 가장 마지막 단계에서 진행하기로 했으므로 B097 확인은 우선 Unity Editor Play Mode에서 진행한다.
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
- 프로덕션 리팩토링은 B097 플레이 감각을 보존하는 경계 분리로 진행한다. `Resources`, `PlayerPrefs`, Addressables, Audio Mixer, 햅틱, 서버 저장은 별도 제품 요구가 확정되기 전에는 건드리지 않는다.
- BoxStack 현재 프로젝트는 기존 코드 스타일을 우선하므로 Unity C# private field는 `_camelCase`를 허용한다. Starter-pack의 `_` prefix 금지 규칙은 이 프로젝트에 소급 적용하지 않는다. 다만 하네스/운영 규칙 개선은 필요 시 starter-pack 원본에도 계속 반영한다.

## Next Action

- B107 결과 버튼 흐름 분리는 사용자 확인 후 수용됐다. 다음 작업 후보는 프로덕션 리팩토링의 남은 생산화 경계이며, `BoxStackPrototype`에 남은 스테이지 변경 흐름, 재시작 정리 흐름, 드롭 해소 후 상태 전환 중 어떤 작은 책임부터 분리할지 정한다. 폴더/네임스페이스 rename은 Unity 참조 churn이 커질 수 있으므로 책임 분리가 더 진행된 뒤 별도 범위로 다룬다.

## Open Questions

- 실제 App-in-Toss MVP 에셋 목록이 고정되면 어떤 에셋은 직렬화 참조로 두고 어떤 에셋만 Addressables 후보로 둘지 분류해야 한다.
- 실제 App-in-Toss 저장 정책이 확정되면 `PlayerPrefs`를 계속 써도 되는지, 아니면 AIT 저장 브리지나 서버 저장으로 교체해야 하는지 확인해야 한다. 현재 프로토타입에서는 교체하지 않는다.
- WebGL/App-in-Toss에서 오디오 자동 재생, 음소거, 지연이 Editor Play와 다르게 동작하는지 마지막 실기기 단계에서 확인해야 한다.

## Risks

- `active.md`가 다시 긴 히스토리 누적 문서가 되면 상태 확인 요청마다 토큰을 크게 소모한다.
- B074 phone WebGL 평가는 수용됐지만, 이후 UI 레이아웃을 바꾸면 실제 모바일 WebGL에서 다시 확인해야 한다.
- B084 후반 12박스 스테이지 재확인은 수용되었다. 후반 난이도는 물리, 이동, clear 검증, 실패 규칙을 다시 바꿀 때만 재점검한다.
- B097 낙하 속도 상승은 Editor Play에서 수용됐지만, 이후 물리/이동/블록 폭 튜닝을 바꾸면 착지 충격과 좌우 밀림을 다시 확인해야 한다.
- 프로덕션 리팩토링 중 씬/프리팹 이동이나 대규모 파일 rename을 너무 이르게 진행하면 Unity 참조 churn이 커질 수 있으므로, 먼저 작은 책임 분리부터 진행한다.
- B098 스테이지 진행 상태 분리, B099 클리어/실패 규칙 분리, B100 드롭 물리 처리 분리, B101 박스 생성 팩토리 분리, B102 씬 구성 경계 분리, B103 런타임 호스트 경계 분리, B104 드롭 전 이동 경계 분리, B105 클리어 검증 타이머 분리, B106 스테이지 선택 세션 분리, B107 결과 버튼 흐름 분리는 사용자 확인에서 수용됐다. 다음 리팩토링 단계에서 UI/오디오 host, active box 이동, 클리어 검증, 런 상태 전환 경계를 건드리면 HUD, 스테이지 선택, 결과 팝업, 배치/클리어/실패 사운드, 초기/중반/후반 스테이지의 드롭/클리어/실패 흐름을 다시 확인해야 한다.

## References

- 긴 세션 히스토리: `production/session-state/history.md`
- 진행 대시보드: `production/progress-dashboard.md`
- 프로토타입 코드맵: `docs/architecture/boxstack-prototype-code-map.md`
- 프로덕션 리팩토링 계획: `docs/architecture/boxstack-production-refactor-plan.md`
- Unity CLI 사용 정책: `docs/workflow/unity-cli-connector.md`
