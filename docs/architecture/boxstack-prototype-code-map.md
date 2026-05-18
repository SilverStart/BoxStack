# BoxStack 프로토타입 코드 맵

마지막 갱신: 2026-05-17
런타임 마커: B074

이 문서는 현재 프로토타입에서 어떤 파일과 메서드가 어떤 기능을 담당하는지 빠르게 찾기 위한 요약 지도입니다. 최종 제품 아키텍처 문서가 아니라, 사람이 유지보수할 때 읽을 위치를 빠르게 잡을 수 있도록 현재 구조를 정리한 문서입니다.

## 추천 읽기 순서

1. `Assets/Scripts/Prototype/BoxStackPrototypeConfig.cs`
   - 스테이지별 목표 박스 수, 이동 속도, 이동 범위, 마찰, 중력, 감쇠, 클리어 판정 시간 같은 조정값을 먼저 확인합니다.
2. `Assets/Scripts/Prototype/BoxStackPrototype.cs`
   - 메인 게임 흐름을 확인합니다. 초기화, 입력, 박스 이동, 낙하 처리, 성공/실패, 카메라, 스테이지 흐름, UI 갱신 호출이 여기 있습니다.
3. `Assets/Scripts/Prototype/BoxStackPrototypeUiStateFactory.cs`
   - 게임 상태를 결과 팝업 문구, 진행률, 스테이지 버튼 상태로 변환하는 코드를 확인합니다.
4. `Assets/Scripts/Prototype/BoxStackPrototypeUi.cs`
   - UI Toolkit으로 HUD, 스테이지 선택 화면, 결과 팝업을 생성하고 갱신하는 코드를 확인합니다.
5. 보조 파일
   - 에셋 로딩, 폰트 리소스 선택, 박스 비주얼 선택, 스테이지 진행 저장, 공유 상태 enum을 확인합니다.

## 파일별 책임

### `BoxStackPrototype.cs`

현재 프로토타입의 메인 게임 진행 조정자입니다.

담당 기능:
- 프로토타입 부트스트랩과 기본 설정.
- 카메라와 바닥 생성.
- Stack-like 2D 스테이지 팔레트, 추상 배경, 바닥, 전체 스택 그라데이션 구간을 쓰는 블록 색상 흐름 설정.
- 입력 처리.
- 활성 박스 생성과 드롭 전 좌우 이동.
- 박스 낙하, 정착, 클리어 검증 흐름.
- 성공/실패 판정.
- 스테이지 선택과 스테이지 해금 흐름.
- 현재 게임 상태를 UI 계층으로 전달.

주요 메서드:
- `Bootstrap`: 씬 로드 후 프로토타입 오브젝트가 없으면 생성합니다.
- `Start`: 설정과 에셋을 로드하고, 카메라/바닥/UI/진행 상태를 준비한 뒤 현재 스테이지를 시작합니다.
- `Update`: 현재 상태에 맞는 입력과 게임 판정을 처리합니다.
- `RestartGame`: 현재 판을 정리하고 선택된 스테이지를 다시 시작합니다.
- `CreatePrototypeBox`: 현재 스테이지 박스 비주얼, 콜라이더, 2D Rigidbody를 묶어 블록 오브젝트를 만듭니다. Stack-like 추상 블록은 박스마다 전체 스택 그라데이션의 자기 구간을 새 스프라이트로 생성하고, 보이는 스프라이트 영역은 전체 `WorldSize`로 유지하되 콜라이더는 현재 코드값인 `WorldSize * 0.98f`를 사용합니다.
- `MoveActiveBox`: 드롭 전 박스를 일정 속도로 좌우 이동시킵니다.
- `GetStackBlockTint` / `BoxStackPrototypePalette.GetStackGradientColor`: 현재 스테이지 팔레트 안에서 블록 순서에 따라 아래쪽은 배경 최상단에 가까운 아주 진한 색으로 시작하고, 위쪽과 다음 박스는 밝게 이어지도록 색상을 계산합니다.
- `ApplyCurrentStagePalette`: 현재 스테이지에 맞는 팔레트를 계산하고 배경/바닥/UI 전달 색을 갱신합니다.
- `DropActiveBox`: 활성 박스를 물리 낙하 박스로 전환합니다.
- `ResolveDrop`: 떨어진 박스가 안정될 때까지 기다린 뒤 다음 흐름으로 넘깁니다.
- `BeginClearValidation`: 목표 박스 수를 채운 뒤 생존 검증 타이머를 시작합니다.
- `UpdateClearValidation`: 완성된 스택이 검증 시간 동안 유지되는지 확인합니다.
- `EndRun`: 성공 또는 실패 상태로 진입하고 배치된 박스를 고정합니다.

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

### `BoxStackPrototypeUiStateFactory.cs`

게임 데이터를 `BoxStackPrototypeUi.UiState`로 변환합니다.

담당 기능:
- 결과 팝업 제목/본문/버튼 문구.
- 배치/목표 박스 수.
- 스테이지 버튼 활성/선택 상태.

### `BoxStackPrototypeUi.cs`

현재 프로토타입의 런타임 UI Toolkit 구현입니다.

담당 기능:
- `UIDocument`와 `PanelSettings` 설정.
- safe area를 반영한 루트 레이아웃.
- Stack-like 2D 미니멀 HUD 요소.
- 스테이지 배지와 상단 진행 인디케이터.
- 상단 가로 박스 진행 패널.
- 상단 가로 박스 진행 패널의 채움/테두리 슬롯 색은 현재 스테이지의 Primary 역할을 하는 `palette.Accent`를 기준으로 맞춥니다.
- B048 기준 HUD/오버레이 패널은 실제 배경 블러와 테두리선 없이 반투명 틴트와 어두운 분리 레이어를 조합한 가짜 글래스 스타일을 사용합니다.
- 플레이 중 중앙 피드백 토스트는 B046에서 제거되어 표시하지 않습니다.
- 스테이지 선택 오버레이.
- 결과 팝업.
- 스테이지 선택/결과 팝업/HUD 버튼의 입력 차단과 hit-test 처리.

### `BoxStackPrototypeAssetLoader.cs`

프로토타입 에셋 로딩과 코드 생성 스프라이트 경계입니다.

담당 기능:
- `Resources`에서 설정 로드.
- 표시용 게임 폰트, 보조 UI 폰트, 한국어 fallback 폰트 로드.
- 택배 박스와 배경 스프라이트 로드.
- Editor 전용 `AssetDatabase` fallback.
- 런타임 Texture2D-to-Sprite 생성.
- placeholder 택배 박스 스프라이트 생성.
- Stack-like 2D 그라데이션 추상 블록, 팔레트 기반 바닥, 팔레트 기반 그라데이션 배경 스프라이트 생성.
- 스프라이트 맞춤과 콜라이더 계산에 사용하는 visible alpha rect 계산.

먼저 볼 곳:
- `BoxStackPrototypeAssetLoader.LoadConfig`
- `BoxStackPrototypeAssetLoader.LoadDisplayFont`
- `BoxStackPrototypeAssetLoader.LoadBodyFont`
- `BoxStackPrototypeAssetLoader.LoadKoreanFallbackFont`
- `BoxStackPrototypeAssetLoader.LoadParcelSprite`
- `BoxStackPrototypeAssetLoader.LoadBackgroundSprite`
- `BoxStackPrototypeAssetLoader.CreateStackBlockPlaceholder`
- `BoxStackPrototypeAssetLoader.LoadPrototypeSprite`
- `BoxStackPrototypeAssetLoader.GetVisibleTextureRect`

### `BoxStackPrototypeBoxVisualCatalog.cs`

박스 비주얼 선택 경계입니다.

담당 기능:
- 택배 박스 스프라이트 이름.
- 박스 비주얼 코드 크기.
- 스테이지 박스 코드에서 실제 비주얼로의 매핑.
- placeholder 비주얼 생성.
- 바닥 스프라이트와 선택적 배경 스프라이트 선택.
- B054 추상 블록 모드에서는 기존 택배 PNG 대신 코드 생성형 스택 구간 그라데이션 블록/팔레트 기반 바닥/팔레트 기반 배경을 선택합니다. 접촉 그림자는 B042/B043 테스트 후 시각적으로 어색하다고 판단되어 적용하지 않습니다. B045에서 테스트한 위쪽/오른쪽 면 분리도 자연스럽지 않다고 판단되어 적용하지 않습니다.

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
- `ValidatingClear`: 목표 스택 완성 후 생존 검증 중.
- `StageSelect`: 스테이지 선택 오버레이가 열린 상태.
- `Won`: 스테이지 또는 판 성공.
- `Failed`: 스테이지 또는 판 실패.

## 기능별 찾기

### 드롭 전 좌우 이동

먼저 볼 곳:
- `BoxStackPrototype.MoveActiveBox`
- `BoxStackPrototype.GetScreenSafeMoveRange`
- `BoxStackPrototype.GetActiveBoxHalfWidth`
- `BoxStackPrototype.CurrentMoveSpeed`
- `BoxStackPrototype.CurrentMoveRange`

활성 박스가 드롭 전 어떻게 움직이는지, 카메라 화면 안에 어떻게 머무는지, 화면 때문에 이동 범위가 줄어들어도 후반 스테이지 속도감을 어떻게 유지하는지 결정합니다.

### 박스 낙하와 충격 튜닝

먼저 볼 곳:
- `BoxStackPrototype.DropActiveBox`
- `BoxStackPrototype.ClampDroppingBoxFallSpeed`
- `BoxStackPrototype.ResolveDrop`
- `BoxStackPrototype.CreatePhysicsMaterials`
- `BoxStackPrototypeConfig.TuningSettings`

박스가 떨어질 때의 중력, 최대 낙하 속도, 마찰, 감쇠, 정착 판정을 처리합니다.

### 블록 비주얼

먼저 볼 곳:
- `BoxStackPrototype.CreatePrototypeBox`
- `BoxStackPrototype.GetStackBlockTint`
- `BoxStackPrototypeAssetLoader.CreateStackBlockPlaceholder`

블록의 내부 그라데이션, 스테이지 진행에 따른 색상, 테두리 유무를 조정합니다. B053에서는 전체 스택 그라데이션을 박스별 구간으로 나눴고, B054에서는 어두운 테두리 밴드를 제거했습니다. B070에서 테스트한 박스 코드/순서별 색 변주와 B072에서 테스트한 박스 타입별 비색상 명암/질감 변주는 쌓인 모습이 어색해 적용하지 않습니다. B042/B043에서 테스트한 접촉 그림자와 B045에서 테스트한 위쪽/오른쪽 내부 면 분리는 자연스럽지 않아 되돌렸습니다.

### 성공과 실패 규칙

먼저 볼 곳:
- `BoxStackPrototype.AnyBoxLost`
- `BoxStackPrototype.BoxIsLost`
- `BoxStackPrototype.StackIsSingleColumn`
- `BoxStackPrototype.BeginClearValidation`


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

스테이지 선택 오버레이 열기, 스테이지 선택, 진행 저장, 해금/잠금 버튼 표시를 처리합니다. B069 기준 스테이지 타일과 닫기 버튼은 현재/해금/잠김 상태별 라벨과 팔레트 기반 색상을 사용합니다.

### UI 문구

먼저 볼 곳:
- `BoxStackPrototypeUiStateFactory.GetResultTitle`
- `BoxStackPrototypeUiStateFactory.GetResultBody`
- `BoxStackPrototypeUiStateFactory.GetResultButtonLabel`

결과 팝업 문구 변경이 필요할 때 이 파일을 먼저 확인합니다. 플레이 중 중앙 피드백 토스트는 B046에서 제거했으므로 `좋아요`, `유지!` 같은 착지 피드백 문구는 더 이상 생성하지 않습니다. `BoxStackPrototypeUi.cs`는 가능하면 정적인 레이아웃 텍스트에만 문구를 둡니다.

### UI 레이아웃과 스타일

먼저 볼 곳:
- `BoxStackPrototypeUi.BuildHud`
- `BoxStackPrototypeUi.BuildStageOverlay`
- `BoxStackPrototypeUi.BuildResultOverlay`
- `BoxStackPrototypeUi.ApplySafeArea`
- `BoxStackPrototypeUi.ApplyHudLayout`
- `BoxStackPrototypeUi.ApplyPanelStyle`
- `BoxStackPrototypeUi.ApplyDisplayText`
- `BoxStackPrototypeUi.ApplyBodyText`
- `BoxStackPrototypeUi.ApplyText`
- `BoxStackPrototypeUi.ApplyButtonColors`

런타임 생성 UI Toolkit 레이아웃과 스타일을 제어합니다. B071 기준 상단 중앙 진행 패널은 safe-area 폭이 좁을 때 슬롯 크기뿐 아니라 패널 최대 폭도 줄여 왼쪽 스테이지 배지와 겹치지 않도록 합니다. B073 기준 `Screen.safeArea`는 UI Toolkit 패널 좌표로 변환한 뒤 safe root, overlay bounds, HUD layout, touch hit-test에 적용합니다. B074 기준 스테이지 선택 패널은 safe-area 높이에서 상단 여백과 하단 여백을 뺀 값을 최대 높이로 사용하고, 세로 공간이 빠듯할 때 타일 높이와 줄 간격을 조금 줄입니다.

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
-> `ResolveDrop`
-> `SpawnNextBox`, `BeginClearValidation`, `EndRun` 중 하나로 이동

### 스테이지 클리어

배치된 박스 수가 현재 목표에 도달
-> `BeginClearValidation`
-> `UpdateClearValidation`
-> 스택이 `ClearValidationSeconds` 동안 유지됨
-> `EndRun(true)`


`DropActiveBox`가 드롭 직전 스냅샷 저장
-> `Playing` 상태로 복귀
-> `SpawnNextBox`

## 현재 프로토타입 주의점

- 아직 최종 제품 아키텍처가 아니라 프로토타입 코드입니다.
- `BoxStackPrototype.cs`가 여전히 여러 게임플레이 책임을 함께 가지고 있습니다.
- 현재 UI는 UXML이 아니라 C# 코드로 생성합니다.
- 에셋 로딩은 빠른 프로토타입과 WebGL 포함을 위해 `Resources`를 사용합니다. parcel/background PNG는 현재 `Assets/Art/Prototype/...` 원본과 `Assets/Resources/Prototype/...` 런타임 복사본을 함께 유지하고, 제품화 단계에서 Addressables 또는 직렬화된 참조로 교체합니다.
- 스테이지 진행은 `PlayerPrefs`를 사용합니다.
- WebGL/mobile 검증은 milestone spot check로 남기고, AI 에이전트의 일반 C# 검증은 `dotnet build BoxStack.slnx`를 우선 사용합니다. 실제 플레이 감각과 UI 체감 검증은 사용자가 Unity Editor Play Mode에서 직접 확인합니다.
- 런타임 UI는 던파 비트비트체 v2를 HUD/버튼/제목 표시 폰트로, Gmarket Sans Bold를 보조 설명 텍스트로, Noto Sans KR을 fallback으로 사용합니다. UI Toolkit 렌더링 반영을 위해 `unityFont`와 `unityFontDefinition`을 함께 지정합니다.
- B069는 배송/택배 시각 테마를 걷어내고 Stack-like 2D 디자인 가이드에 맞춰 스테이지별 유사 색 계열 팔레트, 전체 스택 그라데이션을 박스별 구간으로 나눠 굽는 컬러 블록, 코드 생성형 뒷배경 그라데이션, 테두리 없는 반투명 글래스풍 HUD를 사용합니다. B050에서 스택 전체를 하나의 세로 그라데이션처럼 읽히도록 아래 박스는 더 진하고 위/다음 박스는 더 밝게 이어지는 색상 진행을 적용했고, B053에서 각 박스 스프라이트가 해당 색상 흐름의 자기 구간을 직접 갖도록 바꿨습니다. B054에서는 B053의 어두운 블록 테두리 밴드를 제거해 경계선 없는 블록을 테스트했고, B055에서는 시작 색을 배경 최상단에 가까운 아주 진한 색으로 당겨 대비를 키웠습니다. B056에서는 블록 텍스처의 투명 가장자리를 제거하고 전체 픽셀을 불투명 그라데이션으로 채워, 쌓인 박스 사이에 배경색이 얇게 비치는 현상을 줄였습니다. B061에서는 사용자가 수치 조정을 진행할 수 있도록 비주얼은 전체 `WorldSize`로 유지하고 콜라이더만 `WorldSize * 0.96f`로 다시 줄인 상태를 튜닝 시작점으로 두었습니다. B062에서는 상단 중앙 진행 인디케이터의 채움/테두리 색을 현재 스테이지 Primary 역할의 `palette.Accent`와 맞췄고, B063-B065에서는 결과 팝업 스탬프 제거와 버튼 테마 색상을 정리했습니다. B068에서는 스테이지 선택 타일의 크기, 현재/해금/잠김 상태 라벨, 팔레트 기반 버튼 색상을 다듬었고, B069에서는 닫기 버튼도 같은 스테이지 테마 색상 체계에 맞췄습니다. 시각적으로 어색하다고 판단된 B042/B043 접촉 그림자 실험, B045 위쪽/오른쪽 면 분리, B049 박스 모양별 그라데이션 차별화는 적용하지 않습니다. 플레이 중 중앙 피드백 토스트도 B046에서 제거했습니다.
- B070 박스 코드/순서별 색 변주 실험은 쌓을 때 색상이 자연스럽게 이어지지 않아 커밋하지 않고 되돌렸습니다. 런타임 마커는 B069를 유지합니다.
- B071은 좁은 모바일 safe-area에서 상단 중앙 진행 패널이 스테이지 배지와 겹치지 않도록 진행 패널 최대 폭을 safe-area 기준으로 제한합니다.
- B072는 B070처럼 색을 흔들지 않고 생성 블록 스프라이트에 박스 타입별 아주 약한 명암/질감 중심 차이만 추가했지만, 실제 체감이 어색해 적용하지 않습니다.
- B073은 phone WebGL에서 상단 중앙 박스 인디케이터, 결과 팝업, 스테이지 선택 팝업이 화면 밖으로 잘리는 문제를 겨냥해 `Screen.safeArea` 픽셀 좌표를 UI Toolkit 패널 좌표로 변환합니다.

- B074는 스테이지 선택 팝업이 모바일 화면 하단에 너무 붙지 않도록 safe-area 높이 기준의 하단 여백과 패널 최대 높이를 함께 계산합니다.
- 코드 변경으로 기능 책임, 파일 위치, 주요 메서드, 실행 흐름, 주의점이 달라지면 이 코드 맵도 같은 변경 묶음에서 갱신합니다.
