# BoxStack 프로덕션 리팩토링 계획

마지막 갱신: 2026-06-06
런타임 마커: B107
상태: Result button flow split implemented and accepted

## 목적

현재 BoxStack 런타임은 빠른 감각 검증을 위해 프로토타입 우선으로 성장했다. B097 기준으로 핵심 조작감, 스테이지 폭 난이도, 결과 문구, 작은 합성 효과음은 사용자 확인을 거쳐 충분히 안정된 상태이므로, 다음 단계는 플레이 감각을 유지하면서 프로덕션 코드 구조로 옮겨갈 준비를 하는 것이다.

이 작업은 전면 재작성이나 기능 확장이 아니다. 이미 수용된 B097 게임 감각을 보존하면서, 변경 이유가 서로 다른 책임을 나누고 이후 App-in-Toss MVP 검증, 저장소 교체, 에셋 파이프라인 정리, 테스트 추가가 가능하도록 경계를 만드는 작업이다.

리팩토링의 최종 목적은 프로토타입 코드를 실제 프로덕션에 사용할 수 있는 런타임 코드로 바꾸는 것이다. ECS처럼 상태와 동작의 경계를 분명히 보는 사고방식, SOLID처럼 단일 책임과 교체 가능한 의존성을 만드는 원칙은 적용하되, 현재 Unity `MonoBehaviour` 기반 구조에 DOTS/ECS 패키지를 무리하게 도입하지는 않는다. 즉 이번 단계의 기준은 게임 감각은 유지하고, 생산 코드가 될 수 있는 책임 경계를 하나씩 만드는 것이다.

새로 추가하는 생산 후보 클래스는 가능한 한 `Prototype` 이름을 붙이지 않는다. 기존 `Prototype` 파일명은 아직 남아 있을 수 있지만, 새 경계부터는 제품 코드로 남을 수 있는 이름을 사용해 이후 정리 대상과 유지 대상이 헷갈리지 않게 한다.

## 현재 진단

### 유지할 점

- `BoxStackPrototypeConfig`가 스테이지/튜닝 데이터의 첫 제품화 경계 역할을 하고 있다.
- `BoxStackStageProgressStore`가 최고 해금 스테이지 저장을 작은 교체 지점으로 감싸고 있다.
- `BoxStackPrototypeAssetLoader`가 현재 `Resources` 경로와 Editor fallback을 한 곳에 모아 두고 있다.
- `BoxStackPrototypeBoxVisualCatalog`가 스테이지 박스 코드와 실제 비주얼 선택을 분리한다.
- `BoxStackPrototypeUiStateFactory`가 런타임 상태를 UI 표시 상태와 문구로 변환한다.
- `BoxStackPrototypeAudio`는 아직 작은 런타임 합성 효과음 경계로 충분하다.
- `BoxStackSceneComposition`이 카메라, 배경, 바닥 생성과 스테이지 팔레트 기반 씬 갱신을 맡기 시작했다.
- `BoxStackRuntimeHosts`가 UI host와 audio host GameObject 생성, UI 폰트 로드, UI 컴포넌트 초기화 경계를 맡기 시작했다.
- `BoxStackActiveBoxMotion`이 드롭 전 active box 좌우 이동, 화면 안전 이동 범위 계산, 박스 반폭 계산 경계를 맡기 시작했다.
- `BoxStackClearValidation`이 목표 박스 수 달성 후 검증 타이머 시작/초기화/완료 판단 경계를 맡기 시작했다.
- `BoxStackStageSelectSession`이 스테이지 선택 팝업 진입 전 런 상태와 타임스케일 저장/복원 경계를 맡기 시작했다.
- `BoxStackResultFlow`가 결과 팝업 버튼 액션 결정 경계를 맡기 시작했다.

### 리팩토링이 필요한 점

- `BoxStackPrototype.cs`가 약 1,500줄이며 입력, 게임 상태, 물리 판정, 스테이지 진행, 씬 생성, 카메라, UI 연결, 오디오 호출을 모두 조정한다.
- 카메라, 바닥, UI, 오디오 오브젝트가 런타임에서 직접 생성되어 프로덕션 씬/프리팹 구조로 옮기기 어렵다.
- 클리어/실패 판정이 `GameObject`, `Rigidbody2D`, `Collider2D` 접근과 강하게 묶여 있어 자동 테스트가 어렵다.
- `BoxStackPrototypeUi.cs`도 약 950줄 규모라 화면 요소가 더 늘어나면 UI 유지보수 비용이 커질 수 있다.
- 파일/클래스 이름에 `Prototype`이 넓게 남아 있어 제품 런타임 코드와 실험용 코드를 구분하기 어렵다.

## 리팩토링 원칙

- B097 조작감, 스테이지 데이터, 실패/성공 규칙, UI 흐름, 오디오 cue는 기본적으로 바꾸지 않는다.
- 한 번에 한 경계만 분리한다.
- 각 단계는 `dotnet build BoxStack.slnx`로 검증한다.
- 실제 조작감, UI 감각, WebGL/App-in-Toss 디바이스 확인은 사용자가 필요하다고 지정한 지점에서만 진행한다.
- WebGL 실기기 확인은 최종 단계로 미룬다.
- `Resources`와 `PlayerPrefs`는 현재 제품화 blocker가 아니므로, 별도 요구가 생기기 전에는 유지한다.
- Addressables, Audio Mixer, 햅틱, 서버 저장, 새 점수 경제, 새 시각 효과는 이번 리팩토링 범위가 아니다.
- 문서만 바꾸는 작업은 `git diff --check`로 검증한다.

## 단계별 계획

### 0단계: 계획 고정

목표:
- 이 문서를 기준으로 리팩토링 방향을 합의한다.
- `production/session-state/active.md`와 `production/progress-dashboard.md`의 다음 행동을 리팩토링 계획으로 맞춘다.

검증:
- `git diff --check`

완료 조건:
- 다음 작업이 코드 변경 전 어떤 경계를 분리할지 명확하다.

### 1단계: 런타임 이름과 폴더 방향 정리

목표:
- 현재 `Assets/Scripts/Prototype` 아래의 코드가 어느 범위까지 제품 런타임으로 승격될지 정한다.
- 바로 대규모 rename을 하기보다, 먼저 새 폴더/네임스페이스 후보를 정한다.

권장 방향:
- 제품 런타임 후보: `Assets/Scripts/BoxStack/Runtime`
- 제품 UI 후보: `Assets/Scripts/BoxStack/Runtime/UI`
- 제품 데이터 후보: `Assets/Scripts/BoxStack/Runtime/Data`
- 실험 유지 후보: `Assets/Scripts/Prototype`

주의:
- Unity `.meta` 파일 변동과 참조 깨짐을 줄이기 위해 rename/move는 작게 진행한다.
- `PrototypeBuildNumber`는 실제 빌드 마커 정책이 정리될 때까지 유지해도 된다.

검증:
- `dotnet build BoxStack.slnx`
- Unity Editor에서 스크립트 참조 누락 여부 확인이 필요할 수 있다.

### 2단계: 게임 상태/스테이지 진행 분리

상태:
- B098에서 첫 구현을 완료했고, 사용자 Editor Play 확인에서 수용됐다.

목표:
- 스테이지 선택, 해금, 재시작, 결과 처리 흐름을 `BoxStackPrototype` 밖으로 한 번 더 빼기 쉽게 만든다.
- `BoxStackStageProgressStore`는 유지하고, 그 위에 진행 상태를 다루는 작은 클래스를 둔다.

분리 후보:
- 현재 스테이지 인덱스
- 최고 해금 스테이지 인덱스
- 다음 스테이지 존재 여부
- 스테이지 선택 가능 여부
- 성공 시 다음 스테이지 해금
- 테스트용 전체 해금/진행 초기화

구현 결과:
- `BoxStackStageProgress`가 현재 스테이지 인덱스와 최고 해금 스테이지 인덱스를 보관한다.
- `BoxStackPrototype`은 스테이지 이동, 선택, 초기화, 전체 해금, 다음 스테이지 해금을 `BoxStackStageProgress`에 위임한다.
- `BoxStackStageProgressStore`의 `PlayerPrefs` 저장 키와 저장 방식은 바꾸지 않았다.
- 사용자 확인에서 스테이지 선택, 클리어 후 다음 스테이지 해금, 진행 초기화, 전체 해금, 최종 스테이지 클리어 후 처음부터 흐름이 정상으로 확인됐다.

검증:
- `dotnet build BoxStack.slnx`
- Editor Play에서 스테이지 선택, 클리어 후 다음 스테이지 해금, 진행 초기화, 전체 해금 확인

### 3단계: 클리어/실패 규칙 분리

상태:
- B099에서 첫 구현을 완료했고, 사용자 Editor Play 확인에서 수용됐다.

목표:
- 현재 성공/실패 규칙을 읽기 쉬운 별도 경계로 모은다.
- 목표 박스 수 도달, 5초 생존 검증, lost 판정, 바닥 다중 접촉 판정을 추후 테스트하기 쉽게 만든다.

분리 후보:
- `AnyBoxLost`
- `BoxIsLost`
- `StackHasMultipleFloorContacts`
- `BoxIsTouchingFloor`
- `BeginClearValidation`
- `UpdateClearValidation`

구현 결과:
- `BoxStackRunRules`가 드롭 실패, 기존 스택 이탈, 바닥 다중 접촉 실패 판정을 맡는다.
- `BoxStackClearValidation`이 목표 박스 수 달성 후 클리어 검증 타이머 시작, 초기화, 완료 판단을 맡는다.
- `BoxStackPrototype`은 룰 판정 결과를 받아 `EndRun`, 결과 UI/오디오, 스테이지 진행 처리만 계속 조정한다.
- 5초 클리어 검증 타이머, 실패/성공 규칙, 결과 문구, 결과음, 스테이지 해금 흐름은 바꾸지 않았다.
- B105에서 클리어 검증 타이머의 종료 시간 보관과 완료 판단을 `BoxStackClearValidation`으로 분리했다. 검증 중 실패 판정과 성공/실패 결과 처리 흐름은 `BoxStackPrototype`에 유지했다.
- 사용자 확인에서 정상 클리어, 낙하 박스 놓침, 쌓인 박스 이탈, 바닥 2개 접촉 실패, 검증 중 실패, 해금/리플레이 흐름이 정상으로 확인됐다.
- B105 사용자 확인에서 런타임 마커 B105, 클리어 조건 달성 후 `VERIFYING` 상태, 검증 시간 후 성공 팝업/성공음, 검증 중 탑 붕괴 실패 팝업, 결과 팝업의 재시작/다음 스테이지 흐름이 정상으로 확인됐다.

주의:
- 물리 쿼리 자체는 Unity Collider에 남아도 된다.
- 먼저 판정 이름과 호출 흐름을 명확히 하고, 순수 C# 테스트화는 그 다음 단계로 둔다.

검증:
- `dotnet build BoxStack.slnx`
- Editor Play에서 실패 원인 `STACK LOST`, `STACK SPREAD`, 클리어 5초 검증 확인

### 4단계: 박스 생성/드롭 처리 분리

상태:
- B100에서 드롭 물리 처리 분리의 첫 구현을 완료했고, 사용자 Editor Play 확인에서 수용됐다.
- B101에서 박스 생성 factory와 비주얼 적용 흐름을 `BoxStackBoxFactory`로 분리했고, 사용자 Editor Play 확인에서 수용됐다. 새 생산 후보 클래스에는 `Prototype` 이름을 붙이지 않는 원칙을 적용했다.
- B102에서 카메라 생성/설정, 배경 생성/리사이즈, 바닥 오브젝트/콜라이더 생성, 팔레트 기반 배경/바닥 갱신을 `BoxStackSceneComposition`으로 분리했고, 사용자 Editor Play 확인에서 수용됐다. 새 생산 후보 클래스에는 `Prototype` 이름을 붙이지 않는 원칙을 적용했다.
- B103에서 UI host GameObject 생성, 런타임 UI 폰트 로드, UI 컴포넌트 초기화, audio host GameObject 생성을 `BoxStackRuntimeHosts`로 분리했고, 사용자 Editor Play 확인에서 수용됐다. 새 생산 후보 클래스에는 `Prototype` 이름을 붙이지 않는 원칙을 적용했다.
- B104에서 드롭 전 active box 좌우 이동, 화면 안전 이동 범위 계산, collider 기반 박스 반폭 계산을 `BoxStackActiveBoxMotion`으로 분리했고, 사용자 Editor Play 확인에서 수용됐다. 새 생산 후보 클래스에는 `Prototype` 이름을 붙이지 않는 원칙을 적용했다.

목표:
- 박스 생성, 비주얼 적용, 콜라이더/Rigidbody 설정, 낙하 속도 조정, 정착 대기 흐름을 메인 진행자에서 덜어낸다.

분리 후보:
- 박스 생성 factory
- 드롭 전 이동 계산
- 낙하 시작 처리
- 접촉 직전 y속도 리셋
- 최대 낙하 속도 제한
- 스택 안정 판정

구현 결과:
- `BoxStackDropPhysics`가 낙하 시작 시 Rigidbody 전환, B086 접촉 직전 y속도 리셋, B097 최대 낙하 속도 제한, 스택 안정 판정, 결과 진입 시 placed box 물리 정지를 맡는다.
- `BoxStackBoxFactory`가 박스 오브젝트 생성, 비주얼 선택/적용, 콜라이더/Rigidbody 초기 설정, Stack-like 추상 블록용 그라데이션 스프라이트 생성을 맡도록 분리했다.
- `BoxStackActiveBoxMotion`이 드롭 전 active box의 좌우 왕복 위치 계산, 화면 폭 기준 안전 이동 범위 계산, active box collider 반폭 계산을 맡도록 분리했다.
- `BoxStackPrototype`은 드롭 시작/해소 코루틴, 실패/성공 전환, 결과 UI/오디오, 스테이지 진행 처리를 계속 조정한다.
- B086 접촉 직전 y속도 리셋, B097 낙하 템포, clear/fail 룰, 결과 문구, 결과음, 스테이지 해금 흐름은 바꾸지 않았다.
- 사용자 확인에서 일반 드롭, 접촉 직전 충격 완화, 낙하 속도 상한, 안정 후 다음 박스 생성, 실패 흐름, 클리어 흐름, 대표 스테이지 낙하 감각이 정상으로 확인됐다.
- B101 사용자 확인에서 초기/중반/후반 스테이지의 박스 생성, 비주얼, 드롭, 실패/클리어 흐름이 정상으로 확인됐다.
- B102 사용자 확인에서 런타임 마커 B102, 배경/바닥/카메라 위치, 스테이지 팔레트 갱신, 낙하/착지/실패/클리어, 스테이지 선택/결과 팝업 흐름이 정상으로 확인됐다.
- B103 사용자 확인에서 런타임 마커 B103, 스테이지 선택 팝업 열기/닫기/선택, 결과 팝업 버튼, 배치/클리어/실패 사운드가 정상으로 확인됐다.
- B104 사용자 확인에서 런타임 마커 B104, 스테이지 1 좌우 왕복 이동, 박스 배치/낙하/착지 사운드, 후반 스테이지 화면 밖 이탈 여부, 클리어/실패 결과 팝업이 정상으로 확인됐다.
- Unity CLI Connector refresh 후 Unity 생성 `Assembly-CSharp.csproj`에 새 `BoxStackBoxFactory.cs`가 포함됐고, `dotnet build BoxStack.slnx`가 경고 0개, 오류 0개로 통과했다.

주의:
- B086 접촉 직전 y속도 리셋과 B097 낙하 템포는 유지한다.
- 기존 박스 x축 속도 감쇠/제한은 재도입하지 않는다.

검증:
- `dotnet build BoxStack.slnx`
- 사용자 주도 Editor Play에서 초기/중반/후반 스테이지 낙하 감각 확인

### 5단계: 씬/프리팹 구성 준비

상태:
- B102에서 카메라, 배경, 바닥 생성과 팔레트 갱신 흐름을 `BoxStackSceneComposition`으로 분리했고, 사용자 Editor Play 확인에서 수용됐다.
- B103에서 UI/audio host 생성 흐름을 `BoxStackRuntimeHosts`로 분리했고, 사용자 Editor Play 확인에서 수용됐다.

목표:
- 카메라, 바닥, UI, 오디오를 코드가 직접 생성하는 구조에서 씬/프리팹 참조 구조로 옮길 준비를 한다.

분리 후보:
- 카메라 설정자
- 바닥/배경 view
- UI view
- 오디오 view

구현 결과:
- `BoxStackSceneComposition`이 메인 카메라 확보/설정, 배경 SpriteRenderer 생성과 카메라 커버 스케일, 바닥 SpriteRenderer와 `BoxCollider2D` 생성, 최소 카메라 Y 계산, 스테이지 팔레트에 따른 배경/바닥 스프라이트 갱신을 맡는다.
- `BoxStackPrototype`은 씬 구성 객체를 만들고, 카메라/바닥 콜라이더/최소 카메라 Y 참조를 받아 게임 진행과 룰 판정에 사용한다.
- `BoxStackRuntimeHosts`가 UI host GameObject 생성, 런타임 UI 폰트 로드, `BoxStackPrototypeUi` 초기화, audio host GameObject 생성과 `BoxStackPrototypeAudio` 추가를 맡는다.
- `BoxStackPrototype`은 런타임 host를 만들고, 생성된 UI/오디오 컴포넌트 참조만 받아 HUD 갱신과 사운드 호출에 사용한다.

주의:
- 이 단계는 Unity scene/prefab 참조를 만질 가능성이 있으므로 변경 범위를 더 작게 나눠야 한다.
- 실기기 WebGL 확인 전에는 큰 씬 구조 변경을 무리하게 밀지 않는다.

검증:
- `dotnet build BoxStack.slnx`
- Unity Editor에서 scene/prefab 참조 누락 확인

### 6단계: 테스트 가능한 규칙 확보

목표:
- 최소한 스테이지 진행과 룰 판정 일부를 EditMode 테스트 가능한 형태로 만든다.

우선 테스트 후보:
- 최고 해금 스테이지 clamp
- 성공 시 다음 스테이지 해금
- 이미 해금된 낮은 스테이지 클리어 시 새 최고 기록 아님
- 목표 박스 수 도달 후 clear validation 진입
- clear validation 시간이 지나기 전에는 성공하지 않음

검증:
- `dotnet build BoxStack.slnx`
- Unity Test Framework 설정 상태에 따라 EditMode 테스트 추가 검토

### 현재 추가 분리: 스테이지 선택 세션 상태

상태:
- B106에서 첫 구현을 완료했고, 사용자 Editor Play 확인에서 수용됐다.

목표:
- 스테이지 선택 팝업을 열 때의 이전 런 상태와 이전 `Time.timeScale` 저장/복원 책임을 `BoxStackPrototype` 밖으로 분리한다.
- 스테이지 선택 UI 흐름은 유지하면서, 이후 결과 버튼 처리/스테이지 변경/재시작 정리 같은 남은 런 상태 전환 책임을 더 작게 나누기 쉽게 만든다.

구현 결과:
- `BoxStackStageSelectSession`이 `Begin`에서 현재 상태와 타임스케일을 저장하고, `End`에서 복귀할 상태와 복원할 타임스케일을 반환한다.
- `BoxStackPrototype.OpenStageSelect`와 `CloseStageSelect`는 상태 전환과 UI 갱신만 조정하고, 저장/복원 세부는 새 클래스에 위임한다.
- 플레이 중 스테이지 선택, 결과 팝업 상태에서 열기/닫기, 스테이지 타일 선택 후 재시작 흐름은 바꾸지 않았다.

검증:
- `dotnet build BoxStack.slnx`
- Editor Play에서 B106 마커, 플레이 중 스테이지 선택 일시정지/복귀, 결과 팝업 상태 복귀, 스테이지 타일 선택 후 재시작 확인

### 현재 추가 분리: 결과 버튼 흐름

상태:
- B107에서 첫 구현을 완료했고, 사용자 Editor Play 확인에서 수용됐다.

목표:
- 결과 팝업 버튼을 눌렀을 때 다음 스테이지 이동, 1스테이지 복귀, 현재 스테이지 재시작 중 어떤 액션을 실행할지 결정하는 책임을 `BoxStackPrototype` 밖으로 분리한다.
- 실제 스테이지 이동, 첫 스테이지 선택, 재시작 정리는 기존 흐름을 유지해 동작 변경 없이 분기 판단만 작게 분리한다.

구현 결과:
- `BoxStackResultFlow`가 현재 런 상태와 다음 스테이지 존재 여부를 받아 `BoxStackResultAction`을 반환한다.
- `BoxStackPrototype.HandleCurrentResultButton`은 결과 액션을 실행하는 조정자 역할만 유지한다.
- 클리어 후 다음 스테이지 이동, 마지막 스테이지 클리어 후 1스테이지 재시작, 실패 후 현재 스테이지 재시작 흐름은 바꾸지 않았다.

검증:
- `dotnet build BoxStack.slnx`
- Editor Play에서 B107 마커, 클리어 후 다음 스테이지 이동, 최종 클리어 후 1스테이지 재시작, 실패 후 현재 스테이지 재시작, 결과 팝업 외 상태 비발동 확인

## 이번 리팩토링에서 하지 않을 일

- 스테이지 난이도 재튜닝
- 블록 색상, bevel, 질감, 그림자, depth 재실험
- 스테이지 라벨/설명 추가 재시도
- 새로운 점수, 별, 콤보, 일일 도전, 보상형 광고 흐름
- Addressables 도입
- App-in-Toss 저장 브리지 도입
- Audio Mixer, 외부 음원 파일, 햅틱 추가
- WebGL 실기기 반복 확인

## 다음 즉시 행동

B107 결과 버튼 흐름 분리는 검증과 사용자 확인에서 수용됐다. 다음 후보는 `BoxStackPrototype`에 남은 스테이지 변경, 재시작 정리, 드롭 해소 후 상태 전환 흐름 중 하나를 작은 단위로 분리하는 것이다. 폴더/네임스페이스 rename은 Unity 참조 churn이 크므로 실제 책임 분리가 더 진행된 뒤 최종 정리로 넘긴다.
