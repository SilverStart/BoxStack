# BoxStack 프로토타입 코드 맵

마지막 갱신: 2026-05-15
런타임 마커: B041

이 문서는 현재 프로토타입에서 어떤 파일과 메서드가 어떤 기능을 담당하는지 빠르게 찾기 위한 요약 지도입니다. 최종 제품 아키텍처 문서가 아니라, 사람이 유지보수할 때 읽을 위치를 빠르게 잡을 수 있도록 현재 구조를 정리한 문서입니다.

## 추천 읽기 순서

1. `Assets/Scripts/Prototype/BoxStackPrototypeConfig.cs`
   - 스테이지별 목표 박스 수, 이동 속도, 이동 범위, 마찰, 중력, 감쇠, 클리어 판정 시간, 되돌리기 횟수 같은 조정값을 먼저 확인합니다.
2. `Assets/Scripts/Prototype/BoxStackPrototype.cs`
   - 메인 게임 흐름을 확인합니다. 초기화, 입력, 박스 이동, 낙하 처리, 성공/실패, 카메라, 스테이지 흐름, UI 갱신 호출이 여기에 있습니다.
3. `Assets/Scripts/Prototype/BoxStackPrototypeUiStateFactory.cs`
   - 게임 상태를 UI 문구, 결과 팝업 문구, 진행률, 스테이지 버튼 상태로 변환하는 코드를 확인합니다.
4. `Assets/Scripts/Prototype/BoxStackPrototypeUi.cs`
   - UI Toolkit으로 HUD, 스테이지 선택 화면, 결과 팝업을 생성하고 갱신하는 코드를 확인합니다.
5. 보조 파일
   - 에셋 로딩, 폰트 리소스 선택, 박스 비주얼 선택, 스테이지 진행 저장, 공유 상태 enum을 확인합니다.

## 파일별 책임

### `BoxStackPrototype.cs`

현재 프로토타입의 메인 게임 진행 조정자입니다.

담당 기능:
- 프로토타입 부트스트랩과 씬 설정.
- 카메라와 바닥 생성.
- Stack-like 2D 스테이지 팔레트, 추상 배경, 바닥, 테두리 있는 단색 블록 색상 흐름 설정.
- 입력 처리.
- 활성 박스 생성과 드롭 전 좌우 이동.
- 박스 낙하, 정착, 클리어 검증 흐름.
- 성공/실패 판정.
- 되돌리기 스냅샷 저장과 복원.
- 스테이지 선택과 스테이지 해금 흐름.
- 현재 게임 상태를 UI 계층으로 전달.

주요 메서드:
- `Bootstrap`: 씬 로드 후 프로토타입 오브젝트가 없으면 생성합니다.
- `Start`: 설정과 에셋을 로드하고, 카메라/바닥/UI/진행 상태를 준비한 뒤 현재 스테이지를 시작합니다.
- `Update`: 현재 상태에 맞는 입력과 게임 판정을 처리합니다.
- `RestartGame`: 현재 런을 정리하고 선택된 스테이지를 다시 시작합니다.
- `MoveActiveBox`: 드롭 전 박스를 일정 속도로 좌우 이동시킵니다.
- `GetStackBlockTint`: 현재 스테이지 팔레트 안에서 블록 순서에 따라 아래쪽은 연하고 위쪽은 진해지도록 색상을 계산합니다.
- `ApplyCurrentStagePalette`: 현재 스테이지에 맞는 팔레트를 계산하고 배경/바닥/UI 전달 색을 갱신합니다.
- `DropActiveBox`: 활성 박스를 물리 낙하 박스로 전환합니다.
- `ResolveDrop`: 떨어진 박스가 안정될 때까지 기다린 뒤 다음 흐름으로 넘깁니다.
- `BeginClearValidation`: 목표 박스 수를 채운 뒤 생존 검증 타이머를 시작합니다.
- `UpdateClearValidation`: 완성된 스택이 검증 시간 동안 유지되는지 확인합니다.
- `EndRun`: 성공 또는 실패 상태로 진입하고 배치된 박스를 고정합니다.
- `CaptureUndoSnapshot`: 드롭 직전 현재 스택 상태를 저장합니다.
- `RestoreUndoSnapshot`: 저장된 스택 상태를 복원하고 되돌리기 횟수를 소비합니다.

### `BoxStackPrototypeConfig.cs`

프로토타입 스테이지와 튜닝값을 담는 ScriptableObject 데이터 경계입니다.

담당 기능:
- 기본 튜닝 fallback 값.
- 기본 20스테이지 구성.
- `Assets/Resources/Prototype/BoxStackPrototypeConfig.asset`에서 사용하는 직렬화된 튜닝/스테이지 데이터.

먼저 확인할 변경:
- 목표 박스 수.
- 스테이지별 박스 시퀀스.
- 스테이지별 속도/범위 배율.
- 기본 이동 범위와 이동 속도.
- 마찰, 중력, 감쇠, 낙하 속도 제한.
- 클리어 검증 시간.
- 단일 컬럼 허용 오차.
- 스테이지당 되돌리기 횟수.

### `BoxStackPrototypeUiStateFactory.cs`

게임 데이터를 `BoxStackPrototypeUi.UiState`로 변환합니다.

담당 기능:
- HUD 상태 문구.
- 중앙 피드백 문구.
- 결과 팝업 제목/본문/버튼 문구.
- 배치/목표 박스 수.
- 스테이지 버튼 활성/선택 상태.

먼저 확인할 변경:
- 한국어 UI 문구.
- 게임 상태별 피드백 문구.
- 결과 팝업 문구.
- UI에 표시되는 스테이지 버튼 상태 로직.

### `BoxStackPrototypeUi.cs`

현재 프로토타입의 런타임 UI Toolkit 구현입니다.

담당 기능:
- `UIDocument`와 `PanelSettings` 설정.
- safe area를 반영한 루트 레이아웃.
- Stack-like 2D 미니멀 HUD 요소.
- 스테이지 배지와 되돌리기 버튼.
- 상단 가로 박스 진행 패널.
- 스테이지 선택 오버레이.
- 결과 팝업.
- 스테이지 선택/결과 팝업/HUD 버튼의 입력 차단과 hit-test 처리.

먼저 확인할 변경:
- UI 요소 위치, 크기, 색, 타이포그래피, 레이아웃.
- 상단 박스 진행 패널의 채운 네모/테두리 네모 표시.
- 스테이지 선택 화면의 시각 구조.
- 결과 팝업의 시각 구조.
- 버튼 hit 영역 또는 오버레이 표시/숨김 동작.

### `BoxStackPrototypeAssetLoader.cs`

프로토타입 에셋 로딩 경계입니다.

담당 기능:
- `Resources`에서 설정 로드.
- 표시용 게임 폰트, 보조 UI 폰트, 한국어 fallback 폰트 로드.
- 택배 박스와 배경 스프라이트 로드.
- Editor 전용 `AssetDatabase` fallback.
- 런타임 Texture2D-to-Sprite 생성.
- placeholder 택배 박스 스프라이트 생성.
- Stack-like 2D 테두리 있는 단색 추상 블록, 팔레트 기반 바닥, 팔레트 기반 그라데이션 배경 스프라이트 생성.
- 스프라이트 맞춤과 콜라이더 계산에 사용하는 visible alpha rect 계산.

먼저 확인할 변경:
- `Resources` 경로.
- Editor fallback 로딩.
- placeholder 비주얼.
- visible alpha bounds 동작.

### `BoxStackPrototypeBoxVisualCatalog.cs`

박스 비주얼 선택 경계입니다.

담당 기능:
- 택배 박스 스프라이트 이름.
- 박스 비주얼 월드 크기.
- 스테이지 박스 코드에서 실제 비주얼로의 매핑.
- placeholder 비주얼 생성.
- 바닥 스프라이트와 선택적 배경 스프라이트 선택.
- B041 추상 블록 모드에서는 기존 택배 PNG 대신 코드 생성형 테두리와 테스트용 고대비 내부 그라데이션이 있는 블록/팔레트 기반 바닥/팔레트 기반 배경을 선택.

먼저 확인할 변경:
- basic/wide/tall 박스가 어떤 스프라이트를 사용하는지.
- 박스 비주얼 크기.
- 스테이지 시퀀스 코드 해석.
- 바닥/배경 스프라이트 이름.

### `BoxStackStageProgressStore.cs`

스테이지 진행 저장 경계입니다.

담당 기능:
- `PlayerPrefs`에서 최고 해금 스테이지를 로드.
- `PlayerPrefs`에 최고 해금 스테이지 번호를 저장.

App-in-Toss 또는 다른 제품 저장소로 교체할 때 먼저 확인해야 하는 파일입니다.

### `BoxStackPrototypeState.cs`

프로토타입 흐름 상태를 공유하는 enum입니다.

상태:
- `Playing`: 일반 플레이 중.
- `ResolvingDrop`: 떨어진 박스가 정착되는 중.
- `ValidatingClear`: 목표 스택을 완성한 뒤 생존 검증 중.
- `StageSelect`: 스테이지 선택 오버레이가 열린 상태.
- `Won`: 스테이지 또는 런 성공.
- `Failed`: 스테이지 또는 런 실패.

## 기능별 찾기

### 드롭 전 좌우 이동

먼저 볼 곳:
- `BoxStackPrototype.MoveActiveBox`
- `BoxStackPrototype.GetScreenSafeMoveRange`
- `BoxStackPrototype.GetActiveBoxHalfWidth`
- `BoxStackPrototype.CurrentMoveSpeed`
- `BoxStackPrototype.CurrentMoveRange`

활성 박스가 드롭 전에 어떻게 움직이는지, 카메라 화면 안에 어떻게 머무는지, 화면 때문에 이동 범위가 줄어들어도 후반 스테이지 속도감을 어떻게 유지하는지 결정합니다.

### 박스 낙하와 충격 튜닝

먼저 볼 곳:
- `BoxStackPrototype.DropActiveBox`
- `BoxStackPrototype.ClampDroppingBoxFallSpeed`
- `BoxStackPrototype.ResolveDrop`
- `BoxStackPrototype.CreatePhysicsMaterials`
- `BoxStackPrototypeConfig.TuningSettings`

박스가 떨어질 때의 중력, 최대 낙하 속도, 마찰, 감쇠, 정착 판정을 처리합니다.

### 성공과 실패 규칙

먼저 볼 곳:
- `BoxStackPrototype.AnyBoxLost`
- `BoxStackPrototype.BoxIsLost`
- `BoxStackPrototype.StackIsSingleColumn`
- `BoxStackPrototype.BeginClearValidation`
- `BoxStackPrototype.UpdateClearValidation`
- `BoxStackPrototype.EndRun`

박스가 떨어졌는지, 스택이 단일 컬럼을 유지하는지, 완성된 스택이 검증 시간을 버티는지 판단합니다.

### 되돌리기

먼저 볼 곳:
- `BoxStackPrototype.CaptureUndoSnapshot`
- `BoxStackPrototype.CanUseUndoSkill`
- `BoxStackPrototype.UndoLastPlacedBox`
- `BoxStackPrototype.RestoreUndoSnapshot`
- `BoxStackPrototypeUiStateFactory.Create`
- `BoxStackPrototypeUi.Refresh`

게임 파일은 스냅샷 저장/복원을 담당합니다. UI 상태 팩토리와 UI 클래스는 되돌리기 버튼 표시와 활성 상태를 담당합니다.

### 스테이지 선택과 해금

먼저 볼 곳:
- `BoxStackPrototype.OpenStageSelect`
- `BoxStackPrototype.CloseStageSelect`
- `BoxStackPrototype.SelectStage`
- `BoxStackPrototype.UnlockNextStage`
- `BoxStackPrototype.UnlockAllStagesForPlaytest`
- `BoxStackPrototype.ResetStageProgress`
- `BoxStackStageProgressStore`
- `BoxStackPrototypeUi.RefreshStageButtons`

스테이지 선택 오버레이 열기, 스테이지 선택, 진행 저장, 해금/잠금 버튼 표시를 처리합니다.

### UI 문구

먼저 볼 곳:
- `BoxStackPrototypeUiStateFactory.GetHudFeedbackLabel`
- `BoxStackPrototypeUiStateFactory.GetResultTitle`
- `BoxStackPrototypeUiStateFactory.GetResultBody`
- `BoxStackPrototypeUiStateFactory.GetResultButtonLabel`

문구 변경은 이 파일을 먼저 확인합니다. `BoxStackPrototypeUi.cs`는 가능하면 정적인 레이아웃 텍스트에만 문구를 둡니다.

### UI 레이아웃과 스타일

먼저 볼 곳:
- `BoxStackPrototypeUi.BuildHud`
- `BoxStackPrototypeUi.BuildStageOverlay`
- `BoxStackPrototypeUi.BuildResultOverlay`
- `BoxStackPrototypeUi.ApplySafeArea`
- `BoxStackPrototypeUi.ApplyPanelStyle`
- `BoxStackPrototypeUi.ApplyDisplayText`
- `BoxStackPrototypeUi.ApplyBodyText`
- `BoxStackPrototypeUi.ApplyText`
- `BoxStackPrototypeUi.ApplyButtonColors`

런타임 생성 UI Toolkit 레이아웃과 스타일을 제어합니다.

### 에셋 로딩

먼저 볼 곳:
- `BoxStackPrototypeAssetLoader.LoadConfig`
- `BoxStackPrototypeAssetLoader.LoadDisplayFont`
- `BoxStackPrototypeAssetLoader.LoadBodyFont`
- `BoxStackPrototypeAssetLoader.LoadKoreanFallbackFont`
- `BoxStackPrototypeAssetLoader.LoadParcelSprite`
- `BoxStackPrototypeAssetLoader.LoadBackgroundSprite`
- `BoxStackPrototypeAssetLoader.LoadPrototypeSprite`
- `BoxStackPrototypeAssetLoader.GetVisibleTextureRect`

`Resources` 로딩, 폰트 리소스 선택, Editor fallback 로딩, 런타임 스프라이트 생성, visible-alpha bounds 계산을 처리합니다.

### 박스 비주얼 매핑

먼저 볼 곳:
- `BoxStackPrototypeBoxVisualCatalog.Load`
- `BoxStackPrototypeBoxVisualCatalog.GetVisual`
- `BoxStackPrototypeBoxVisualCatalog.GetStageBoxCode`
- `BoxStackPrototypeBoxVisualCatalog.GetBoxVisualIndex`

스테이지 박스 시퀀스 코드를 실제 박스 비주얼로 매핑합니다.

## 주요 실행 흐름

### 게임 시작

`Bootstrap`
-> `Start`
-> `LoadPrototypeConfig`
-> `LoadPrototypeSprites`
-> `CreatePhysicsMaterials`
-> `EnsureCamera`
-> `CreateFloor`
-> `LoadStageProgress`
-> `EnsurePrototypeUi`
-> `RestartGame`
-> `SpawnNextBox`

### 박스 하나 드롭

`Update`
-> `MoveActiveBox`
-> `DropPressed`
-> `DropActiveBox`
-> `CaptureUndoSnapshot`
-> `ResolveDrop`
-> `SpawnNextBox`, `BeginClearValidation`, `EndRun` 중 하나로 이동

### 스테이지 클리어

배치된 박스 수가 현재 목표에 도달
-> `BeginClearValidation`
-> `UpdateClearValidation`
-> 스택이 `ClearValidationSeconds` 동안 유지됨
-> `EndRun(true)`
-> `UnlockNextStage`

### 스테이지 실패

`AnyBoxLost` 또는 `StackIsSingleColumn` 실패
-> `EndRun(false)`
-> 배치된 박스 고정
-> 결과 팝업 표시

### 되돌리기 사용

`DropActiveBox`가 드롭 직전 스냅샷 저장
-> HUD 되돌리기 버튼이 `HandleUndoButton` 호출
-> `UndoLastPlacedBox`
-> `RestoreUndoSnapshot`
-> `Playing` 상태로 복귀
-> `SpawnNextBox`

## 현재 프로토타입 주의점

- 아직 최종 제품 아키텍처가 아니라 프로토타입 코드입니다.
- `BoxStackPrototype.cs`가 여전히 여러 게임플레이 책임을 함께 가지고 있습니다.
- 런타임 UI는 UXML이 아니라 C# 코드로 생성합니다.
- 에셋 로딩은 빠른 프로토타입과 WebGL 포함을 위해 `Resources`를 사용합니다.
- 스테이지 진행은 `PlayerPrefs`를 사용합니다.
- WebGL/mobile 검증은 milestone spot check로 남기고, 일반 반복 검증은 Unity Editor Play Mode를 사용합니다.
- 런타임 UI는 던파 비트비트체 v2를 HUD/버튼/제목 표시 폰트로, Gmarket Sans Bold를 보조 설명 텍스트로, Noto Sans KR을 fallback으로 사용합니다. UI Toolkit 렌더링 반영을 위해 `unityFont`와 `unityFontDefinition`을 함께 지정합니다.
- B041은 배송/택배 시각 테마를 걷어내고 Stack-like 2D 디자인 가이드에 맞춰 스테이지별 유사 색 계열 팔레트, 테두리와 테스트용 고대비 내부 그라데이션이 있는 컬러 블록, 코드 생성형 뒷배경 그라데이션, 반투명 미니멀 HUD를 사용합니다. B041 변경은 B039의 착지 색상 고정과 위로 갈수록 진해지는 색상 흐름을 유지하면서, B040의 약한 그라데이션이 잘 보이지 않아 블록 면 안쪽 명도 대비를 크게 올린 테스트값입니다.
- 코드 변경으로 기능 책임, 파일 위치, 주요 메서드, 실행 흐름, 주의점이 달라지면 이 코드 맵도 같은 변경 묶음에서 갱신합니다.
