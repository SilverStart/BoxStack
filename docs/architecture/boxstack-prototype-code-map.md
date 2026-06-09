# BoxStack 프로토타입 코드 맵

마지막 갱신: 2026-06-09
런타임 마커: B112

이 문서는 현재 프로토타입에서 어떤 파일과 메서드가 어떤 기능을 담당하는지 빠르게 찾기 위한 요약 지도입니다. 최종 제품 아키텍처 문서가 아니라, 사람이 유지보수할 때 읽을 위치를 빠르게 잡을 수 있도록 현재 구조를 정리한 문서입니다.

## 추천 읽기 순서

1. `Assets/Scripts/Prototype/BoxStackPrototypeConfig.cs`
   - 스테이지별 목표 박스 수, 이동 속도, 이동 범위, 마찰, 중력, 감쇠, 클리어 판정 시간 같은 조정값을 먼저 확인합니다.
2. `Assets/Scripts/Prototype/BoxStackPrototype.cs`
   - 메인 게임 흐름을 확인합니다. 초기화, 입력, 박스 이동, 낙하 처리, 성공/실패, 카메라, 스테이지 흐름, UI 갱신 호출이 여기 있습니다.
3. `Assets/Scripts/Prototype/BoxStackRunRules.cs`
   - 박스 이탈, 드롭 실패, 바닥 다중 접촉 실패 판정을 확인합니다.
4. `Assets/Scripts/Prototype/BoxStackClearValidation.cs`
   - 목표 박스 수 달성 후 클리어 검증 타이머 시작/초기화/완료 판단을 확인합니다.
5. `Assets/Scripts/Prototype/BoxStackStageSelectSession.cs`
   - 스테이지 선택 팝업 진입 전 런 상태와 타임스케일 저장/복원을 확인합니다.
6. `Assets/Scripts/Prototype/BoxStackResultFlow.cs`
   - 결과 팝업 버튼 액션 결정을 확인합니다.
7. `Assets/Scripts/Prototype/BoxStackStageFlow.cs`
   - 스테이지 조작 후 팔레트 적용, 런 재시작, UI 갱신 액션 결정을 확인합니다.
8. `Assets/Scripts/Prototype/BoxStackRunCleanup.cs`
   - 재시작 시 현재 런의 active/dropping/placed box 오브젝트 정리를 확인합니다.
9. `Assets/Scripts/Prototype/BoxStackDroppedBoxSettlement.cs`
   - 안정된 드롭 박스를 placed stack에 편입하는 정착 처리를 확인합니다.
10. `Assets/Scripts/Prototype/BoxStackPostDropFlow.cs`
   - 드롭 정착 후 클리어 검증 또는 다음 박스 생성 액션 결정을 확인합니다.
11. `Assets/Scripts/Prototype/BoxStackRunEndFlow.cs`
   - 런 종료 시 최종 상태, 최고 기록 갱신 여부, 다음 스테이지 해금 필요 여부 결정을 확인합니다.
12. `Assets/Scripts/Prototype/BoxStackDropPhysics.cs`
   - 낙하 시작, 접촉 직전 y속도 리셋, 낙하 속도 제한, 스택 안정 판정, 결과 진입 시 물리 정지를 확인합니다.
13. `Assets/Scripts/Prototype/BoxStackBoxFactory.cs`
   - 박스 오브젝트 생성, 비주얼 선택/적용, 콜라이더/Rigidbody 초기 설정을 확인합니다.
14. `Assets/Scripts/Prototype/BoxStackActiveBoxMotion.cs`
   - 드롭 전 active box 좌우 이동, 화면 안전 이동 범위, active box 반폭 계산을 확인합니다.
15. `Assets/Scripts/Prototype/BoxStackSceneComposition.cs`
   - 카메라 설정, 배경 생성/리사이즈, 바닥 오브젝트/콜라이더 생성, 스테이지 팔레트 기반 배경/바닥 갱신을 확인합니다.
16. `Assets/Scripts/Prototype/BoxStackRuntimeHosts.cs`
   - UI host와 audio host GameObject 생성, UI 폰트 로드, UI/audio 컴포넌트 초기화를 확인합니다.
17. `Assets/Scripts/Prototype/BoxStackPrototypeUiStateFactory.cs`
   - 게임 상태를 결과 팝업 문구, 진행률, 스테이지 버튼 상태로 변환하는 코드를 확인합니다.
18. `Assets/Scripts/Prototype/BoxStackPrototypeUi.cs`
   - UI Toolkit으로 HUD, 스테이지 선택 화면, 결과 팝업을 생성하고 갱신하는 코드를 확인합니다.
19. 보조 파일
   - 에셋 로딩, 폰트 리소스 선택, 박스 비주얼 선택, 합성 효과음, 스테이지 진행 상태/저장, 공유 상태 enum을 확인합니다.

## 파일별 책임

### `BoxStackPrototype.cs`

현재 프로토타입의 메인 게임 진행 조정자입니다.

담당 기능:
- 프로토타입 부트스트랩과 기본 설정.
- 씬 구성 객체 생성과 카메라/바닥 콜라이더 참조 보관.
- 런타임 host 객체 생성과 UI/오디오 컴포넌트 참조 보관.
- Stack-like 2D 스테이지 팔레트 계산과 블록 색상 흐름 설정.
- 입력 처리.
- 활성 박스 생성 요청과 드롭 전 좌우 이동 호출.
- 박스 낙하, 정착, 클리어 검증 흐름 호출.
- 성공/실패 판정 결과를 받아 런 상태를 전환.
- 스테이지 선택 세션, 스테이지 선택, 스테이지 해금 흐름 호출.
- 현재 게임 상태를 UI 계층으로 전달.
- 배치 안정, 클리어, 실패 시점에 작은 합성 효과음을 호출.

주요 메서드:
- `Bootstrap`: 씬 로드 후 프로토타입 오브젝트가 없으면 생성합니다.
- `Start`: 설정과 에셋을 로드하고, 씬 구성/UI/오디오/진행 상태를 준비한 뒤 현재 스테이지를 시작합니다.
- `Update`: 현재 상태에 맞는 입력과 게임 판정을 처리합니다.
- `RestartGame`: 현재 판을 정리하고 선택된 스테이지를 다시 시작합니다.
- `SpawnNextBox`: 현재 스테이지와 박스 순번을 기준으로 `BoxStackBoxFactory`에 활성 박스 생성을 위임하고, 생성된 박스를 드롭 전 위치에 둡니다.
- `MoveActiveBox`: 드롭 전 박스 이동을 `BoxStackActiveBoxMotion`에 위임합니다.
- `GetStackBlockTint` / `BoxStackPrototypePalette.GetStackGradientColor`: 현재 스테이지 팔레트 안에서 블록 순서에 따라 아래쪽은 배경 최상단에 가까운 아주 진한 색으로 시작하고, 위쪽과 다음 박스는 밝게 이어지도록 색상을 계산합니다.
- `CreateSceneComposition`: 카메라/배경/바닥 생성을 `BoxStackSceneComposition`에 위임하고, 게임 진행에 필요한 카메라와 바닥 콜라이더 참조를 받아옵니다.
- `CreateRuntimeHosts`: UI/audio host 생성을 맡는 `BoxStackRuntimeHosts`를 준비합니다.
- `EnsureRuntimeUi` / `EnsureRuntimeAudio`: UI/오디오 GameObject 생성 세부를 runtime host에 위임하고, 생성된 컴포넌트 참조만 받아옵니다.
- `ApplyCurrentStagePalette`: 현재 스테이지에 맞는 팔레트를 계산하고 씬 구성 객체에 배경/바닥 갱신을 위임합니다.
- `DropActiveBox`: 활성 박스를 물리 낙하 박스로 전환하라고 `BoxStackDropPhysics`에 위임하고, 드롭 해소 코루틴을 시작합니다.
- `ResolveDrop`: 고정 시간 대신 떨어진 박스와 기존 탑의 물리 속도가 안정될 때까지 기다리고, 안정된 박스의 placed stack 편입은 `BoxStackDroppedBoxSettlement`에, 정착 후 다음 액션 결정은 `BoxStackPostDropFlow`에 위임한 뒤 다음 흐름으로 넘깁니다.
- `BeginClearValidation`: 목표 박스 수를 채운 뒤 생존 검증 타이머를 시작합니다.
- `UpdateClearValidation`: 완성된 스택이 검증 시간 동안 유지되는지 `BoxStackClearValidation`과 실패 판정으로 확인합니다.
- `EndRun`: 런 종료 결과 결정을 `BoxStackRunEndFlow`에 위임하고, 성공 또는 실패 상태 반영, 스테이지 해금, 오브젝트 정리, 배치된 박스 고정, 결과음, UI 갱신을 실행합니다.
- `OpenStageSelect` / `CloseStageSelect`: 스테이지 선택 상태 전환을 조정하고, 이전 상태/타임스케일 저장과 복원은 `BoxStackStageSelectSession`에 위임합니다.
- `HandleCurrentResultButton`: 결과 팝업 버튼 액션 결정을 `BoxStackResultFlow`에 위임하고, 결정된 액션을 실행합니다.

### `BoxStackRunRules.cs`

현재 프로토타입의 런 단위 성공/실패 판정 경계입니다.

담당 기능:
- 떨어진 박스가 빗나갔는지 판정.
- 기존 스택 박스가 화면 아래/옆 이탈 기준을 넘었는지 판정.
- 바닥 콜라이더에 닿은 박스가 2개 이상인지 판정.
- 실패 상태 문자열 `MISSED`, `STACK LOST`, `STACK SPREAD`를 한곳에서 관리.

먼저 확인할 변경:
- `TryEvaluateDroppedBoxFailure`
- `TryEvaluateStackFailure`
- `AnyBoxLost`
- `StackHasMultipleFloorContacts`
- `BoxIsLost`

B099 기준으로 `BoxStackPrototype`은 이 클래스에 실패 판정을 위임합니다. 룰 자체는 B084/B097 기준을 유지하며, 상태 전환, 결과 UI/오디오, 스테이지 해금은 여전히 `BoxStackPrototype.EndRun` 이후 흐름에서 처리합니다.

### `BoxStackClearValidation.cs`

현재 런타임의 클리어 검증 타이머 경계입니다.

담당 기능:
- 목표 박스 수 달성 후 검증 타이머 시작.
- 새 런 시작 시 검증 타이머 초기화.
- 현재 시간이 검증 종료 시간을 지났는지 완료 판단.

먼저 확인할 변경:
- `Begin`
- `Reset`
- `IsComplete`

B105 기준으로 `BoxStackPrototype`은 `ValidatingClear` 상태 진입, 검증 중 스택 실패 판정, 성공/실패 결과 처리를 계속 조정하고, 검증 타이머의 시작/초기화/완료 판단만 `BoxStackClearValidation`에 위임합니다. 사용자 확인에서 B105 마커, `VERIFYING` 상태, 검증 시간 후 성공 팝업/성공음, 검증 중 실패 팝업, 결과 팝업의 재시작/다음 스테이지 흐름이 정상으로 수용됐습니다.

### `BoxStackStageSelectSession.cs`

현재 런타임의 스테이지 선택 세션 상태 경계입니다.

담당 기능:
- 스테이지 선택 팝업을 열기 전 현재 런 상태 저장.
- 스테이지 선택 팝업을 열기 전 현재 `Time.timeScale` 저장.
- 스테이지 선택 팝업을 닫을 때 복귀할 상태와 복원할 타임스케일 반환.

먼저 확인할 변경:
- `Begin`
- `End`

B106 기준으로 `BoxStackPrototype`은 스테이지 선택 상태 전환, UI 갱신, 스테이지 타일 선택, 재시작 흐름을 계속 조정하고, 이전 상태/타임스케일 저장과 복원 판단만 `BoxStackStageSelectSession`에 위임합니다. 사용자 확인에서 B106 마커, 플레이 중 스테이지 선택 일시정지/복귀, 결과 팝업 상태에서 열기/닫기, 스테이지 타일 선택 후 재시작 흐름이 정상으로 수용됐습니다.

### `BoxStackResultFlow.cs`

현재 런타임의 결과 팝업 버튼 액션 결정 경계입니다.

담당 기능:
- 클리어 상태에서 다음 스테이지가 있으면 다음 스테이지 이동 액션을 반환.
- 클리어 상태에서 다음 스테이지가 없으면 1스테이지 재시작 액션을 반환.
- 실패 상태에서는 현재 스테이지 재시작 액션을 반환.
- 결과 상태가 아니면 아무 동작도 하지 않는 액션을 반환.

먼저 확인할 변경:
- `GetResultButtonAction`
- `BoxStackResultAction`

B107 기준으로 `BoxStackPrototype`은 `BoxStackResultFlow`가 반환한 액션에 따라 기존 `ChangeStage`, `SelectFirstStage`, `RestartGame` 호출을 실행합니다. 사용자 확인에서 B107 마커, 클리어 후 다음 스테이지 이동, 최종 클리어 후 1스테이지 재시작, 실패 후 현재 스테이지 재시작, 결과 팝업 외 상태에서 비발동 흐름이 정상으로 수용됐습니다.

### `BoxStackStageFlow.cs`

현재 런타임의 스테이지 조작 후속 액션 결정 경계입니다.

담당 기능:
- 스테이지 이동이 성공하면 팔레트 적용과 런 재시작 액션을 반환.
- 스테이지 선택이 성공하면 다른 스테이지 선택은 팔레트 적용과 런 재시작, 같은 스테이지 재선택은 런 재시작 액션을 반환.
- 진행 초기화 후 팔레트 적용과 런 재시작 액션을 반환.
- 전체 해금 후 UI 갱신 액션을 반환.
- 다음 스테이지 해금은 저장 상태만 갱신하고 별도 런타임 액션을 반환하지 않음.

먼저 확인할 변경:
- `Move`
- `Select`
- `Reset`
- `UnlockAll`
- `UnlockNext`
- `BoxStackStageFlowResult`

B108 기준으로 `BoxStackStageProgress`는 현재/최고 해금 상태와 저장 책임을 유지하고, `BoxStackStageFlow`는 스테이지 조작 뒤 어떤 런타임 액션이 필요한지만 결정합니다. 사용자 확인에서 B108 마커, 이전/다음 스테이지 버튼, 다른 스테이지 선택, 같은 스테이지 재선택, `전체 해금`, `진행 초기화`, 클리어 후 다음 스테이지 해금/결과 팝업 흐름이 정상으로 수용됐습니다.

### `BoxStackRunCleanup.cs`

현재 런타임의 재시작 오브젝트 정리 경계입니다.

담당 기능:
- active box가 있으면 파괴하고 참조를 비움.
- dropping box가 있으면 파괴하고 참조를 비움.
- placed box 목록의 GameObject를 파괴하고 목록을 비움.

먼저 확인할 변경:
- `ClearRunObjects`

B109 기준으로 `BoxStackPrototype.RestartGame`은 코루틴 중단, 런 상태 초기화, 다음 박스 생성, UI 갱신 흐름을 유지하고, 현재 런의 box GameObject 정리만 `BoxStackRunCleanup`에 위임합니다. 사용자 확인에서 B109 마커, 일반 재시작/결과 팝업 재시작, 낙하 중 또는 실패 직후 재시작, 스테이지 선택 후 재시작, 클리어 후 다음 스테이지 이동/재시작 흐름이 정상으로 수용됐습니다.

### `BoxStackDroppedBoxSettlement.cs`

현재 런타임의 드롭 박스 정착 경계입니다.

담당 기능:
- 안정된 드롭 박스에 placed tint 적용.
- 안정된 드롭 박스의 Rigidbody gravity scale을 settled 값으로 전환.
- 안정된 드롭 박스를 placed box 목록에 추가.

먼저 확인할 변경:
- `Settle`

B110 기준으로 `BoxStackPrototype.ResolveDrop`은 드롭 안정 대기, 실패 판정, 바닥 다중 접촉 확인, 배치음, 클리어 검증/다음 박스 전환 흐름을 유지하고, 안정된 드롭 박스를 placed stack에 편입하는 세부 처리만 `BoxStackDroppedBoxSettlement`에 위임합니다. 사용자 확인에서 B110 마커, 박스 배치 후 다음 박스 생성, placed 색상/피아노 배치음/카메라 추적, 빗나간 드롭 실패, 목표 개수 후 `VERIFYING`과 클리어 결과 팝업 흐름이 정상으로 수용됐습니다.

### `BoxStackPostDropFlow.cs`

현재 런타임의 드롭 정착 후 액션 결정 경계입니다.

담당 기능:
- placed box 수와 목표 box 수를 비교.
- 목표 개수에 도달했으면 클리어 검증 시작 액션 반환.
- 아직 목표 개수 미만이면 계속 플레이 액션 반환.

먼저 확인할 변경:
- `GetNextAction`
- `BoxStackPostDropAction`

B111 기준으로 `BoxStackPrototype.ResolveDrop`은 `BoxStackPostDropFlow`가 반환한 액션을 `ApplyPostDropFlowResult`에서 실행합니다. 실제 Unity 상태 변경, `BeginClearValidation`, `SpawnNextBox` 호출은 `BoxStackPrototype`에 남아 있고, 새 클래스는 목표 개수 기준 다음 액션 결정만 맡습니다. 사용자 확인에서 B111 마커, 목표 개수 미만 배치 후 다음 박스 생성, 목표 개수 도달 후 `VERIFYING` 진입, 검증 후 클리어 팝업, 빗나간 드롭/바닥 2개 접촉 실패 흐름이 정상으로 수용됐습니다.

### `BoxStackRunEndFlow.cs`

현재 런타임의 런 종료 결과 결정 경계입니다.

담당 기능:
- 이미 `Won` 또는 `Failed` 상태인 런은 중복 종료하지 않도록 판단.
- 성공이면 최종 상태를 `Won`, 실패면 `Failed`로 결정.
- 결과 status text를 전달.
- 성공이 현재 최고 해금 스테이지 이상에서 발생했는지로 최고 기록 갱신 여부 판단.
- 성공 시 다음 스테이지 해금 필요 여부 반환.

먼저 확인할 변경:
- `GetEndResult`
- `BoxStackRunEndResult`

B112 기준으로 `BoxStackPrototype.EndRun`은 이 클래스가 반환한 결과를 받아 상태, 결과 문구, 최고 기록 갱신 플래그, 다음 스테이지 해금을 반영합니다. active/dropping box 정리, placed box 물리 정지, 결과음 재생, UI 갱신은 여전히 `BoxStackPrototype`이 실행합니다. 사용자 확인에서 B112 마커, 성공/실패 결과 진입, 최고 기록 갱신 문구, 다음 스테이지 해금, 결과 팝업 재시작/이동 흐름이 정상으로 수용됐습니다.

### `BoxStackDropPhysics.cs`

현재 프로토타입의 드롭 중 Rigidbody/Collider 기반 물리 처리 경계입니다.

담당 기능:
- 활성 박스를 실제 낙하 박스로 전환.
- B086 접촉 직전 y속도 0 리셋.
- B097 최대 낙하 속도 제한.
- 드롭 해소 중 스택 안정 판정.
- 성공/실패 결과 진입 시 배치된 박스 물리 정지.

먼저 확인할 변경:
- `BeginDrop`
- `ResetVelocityBeforeStackContact`
- `ClampFallSpeed`
- `StackMotionIsStable`
- `FreezePlacedBoxPhysics`

B100 기준으로 `BoxStackPrototype`은 이 클래스에 드롭 물리 세부 처리를 위임합니다. B086/B097 낙하 감각은 유지하며, 드롭 코루틴의 상태 전환, 실패/성공 처리, 결과 UI/오디오, 스테이지 해금은 여전히 `BoxStackPrototype`이 조정합니다.

### `BoxStackBoxFactory.cs`

현재 런타임의 박스 생성 경계입니다.

담당 기능:
- 스테이지 박스 코드와 박스 순번을 기준으로 비주얼을 선택.
- Stack-like 추상 블록일 때 박스별 그라데이션 스프라이트 생성.
- `SpriteRenderer`, `BoxCollider2D`, `Rigidbody2D` 초기 설정.
- 정착 후 placeholder 블록 tint 적용.

먼저 확인할 변경:
- `CreateBox`
- `TryApplyPlacedTint`
- `CreateStackBlockSprite`

B101 기준으로 `BoxStackPrototype`은 박스를 언제 생성하고 어디에 배치할지만 조정하고, 박스 오브젝트 조립과 비주얼/물리 컴포넌트 초기화는 `BoxStackBoxFactory`가 맡습니다. 새 생산 후보 클래스에는 `Prototype` 이름을 붙이지 않는 원칙을 적용했습니다. 사용자 확인에서 초기/중반/후반 스테이지의 박스 생성, 비주얼, 드롭, 실패/클리어 흐름이 정상으로 수용됐습니다. 기존 `BoxStackPrototypeAssetLoader`, `BoxStackPrototypeBoxVisualCatalog`, `BoxStackPrototypePalette` 의존성은 아직 남아 있으므로 이후 제품 런타임 이름/폴더 정리 단계에서 다시 확인합니다.

### `BoxStackActiveBoxMotion.cs`

현재 런타임의 드롭 전 active box 이동 계산 경계입니다.

담당 기능:
- 드롭 전 active box의 좌우 왕복 위치 계산.
- 스테이지 이동 속도/범위 배율 적용.
- 카메라 화면 폭 기준 안전 이동 범위 계산.
- active box collider 기준 반폭 계산.
- 화면 clamp로 이동 범위가 줄어들어도 기존 후반 스테이지 속도감을 유지하는 range compensation 적용.

먼저 확인할 변경:
- `Move`
- `GetScreenSafeMoveRange`
- `GetActiveBoxHalfWidth`

B104 기준으로 `BoxStackPrototype`은 active box, 카메라, 스폰 높이, 이동 시작 시간, 현재 스테이지, 튜닝, fallback 박스 크기만 넘기고, 이동 수학과 위치 적용은 `BoxStackActiveBoxMotion`이 맡습니다. 기존 B012 화면 clamp와 속도 보정, B097 드롭 감각은 바꾸지 않았습니다. 사용자 확인에서 B104 마커, 스테이지 1 좌우 왕복 이동, 후반 스테이지 화면 밖 이탈 여부, 클리어/실패 결과 팝업이 정상으로 수용됐습니다.

### `BoxStackSceneComposition.cs`

현재 런타임의 씬 구성 경계입니다.

담당 기능:
- 메인 카메라 확보와 기본 orthographic 설정.
- Stack-like 배경 오브젝트 생성과 카메라 크기에 맞춘 cover 스케일 조정.
- 바닥 오브젝트, 바닥 비주얼, 바닥 `BoxCollider2D` 생성.
- 최소 카메라 Y 위치 계산.
- 스테이지 팔레트 변경 시 배경/바닥 스프라이트 갱신.

먼저 확인할 변경:
- `Initialize`
- `UpdateBackground`
- `ApplyPalette`
- `CreateFloor`
- `CreateBackground`

B102 기준으로 `BoxStackPrototype`은 게임 진행에 필요한 카메라, 바닥 콜라이더, 최소 카메라 Y 참조만 받아 쓰고, 카메라/배경/바닥 생성 세부 구현과 팔레트 기반 배경/바닥 갱신은 `BoxStackSceneComposition`이 맡습니다. 사용자 확인에서 B102 마커, 배경/바닥/카메라 위치, 스테이지 팔레트 갱신, 낙하/착지/실패/클리어, 스테이지 선택/결과 팝업 흐름이 정상으로 수용됐습니다.

### `BoxStackRuntimeHosts.cs`

현재 런타임의 UI/audio host 생성 경계입니다.

담당 기능:
- UI host GameObject 생성.
- 런타임 UI 초기화에 필요한 표시 폰트, 본문 폰트, 한국어 fallback 폰트 로드.
- `BoxStackPrototypeUi` 컴포넌트 추가와 콜백 연결.
- audio host GameObject 생성.
- `BoxStackPrototypeAudio` 컴포넌트 추가.

먼저 확인할 변경:
- `EnsureUi`
- `EnsureAudio`
- `UiCallbacks`

B103 기준으로 `BoxStackPrototype`은 UI/audio GameObject 생성 방식과 폰트 로딩 세부를 직접 알지 않고, `BoxStackRuntimeHosts`에서 생성된 `BoxStackPrototypeUi`, `BoxStackPrototypeAudio` 참조만 받아 사용합니다. 사용자 확인에서 B103 마커, 스테이지 선택 팝업 열기/닫기/선택, 결과 팝업 버튼, 배치/클리어/실패 사운드가 정상으로 수용됐습니다.

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
- 마찰, 중력, 감쇠, 낙하 속도 제한, 기존 스택 접촉 직전 낙하 속도 리셋 거리.
- 클리어 검증 시간.
- 박스 이탈 기준.

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

### `BoxStackPrototypeAudio.cs`

프로토타입 전용 합성 효과음 경계입니다.

담당 기능:
- 별도 음원 파일 없이 런타임에서 짧은 톤 클립 생성.
- 안정 배치용 도-레-미-파-솔-라-시-도 계열의 짧은 피아노풍 톤 재생.
- 스테이지 클리어용 도-레-미-파-솔-라-시-도 피아노풍 glissando 재생.
- 실패용 낮은 thud 재생.

먼저 확인할 변경:
- `BoxStackPrototypeAudio.PlayPlacement`
- `BoxStackPrototypeAudio.PlayClear`
- `BoxStackPrototypeAudio.PlayFailure`
- `BoxStackPrototypeAudio.CreateToneClip`
- `BoxStackPrototypeAudio.CreatePianoToneClip`
- `BoxStackPrototypeAudio.CreatePianoGlissandoClip`

B097 기준 이 파일은 오디오 정체성 후보를 아주 작게 확인하기 위한 프로토타입 코드입니다. 배치음은 실제 피아노 샘플이 아니라 런타임 합성 피아노풍 톤이며, 놓인 박스 순서에 따라 C major 음계를 한 단계씩 올립니다. B095는 성공 결과음을 짧은 chime 대신 도-레-미-파-솔-라-시-도 연속 상승 glissando로 바꿔 성공감을 더 강하게 만듭니다. B096은 스테이지별 블록 시퀀스에서 좌우로 넓어지는 블록을 제거하고, 정사각형 기본 블록에서 좁은 블록과 더 좁은 블록으로만 난이도를 올립니다. B097은 B086의 접촉 직전 y속도 리셋을 유지한 채 낙하 중력과 최대 낙하속도만 조금 올려 박스가 내려오는 템포를 빠르게 합니다. 오디오 에셋, Audio Mixer, 햅틱, 플랫폼 브리지는 아직 추가하지 않습니다.

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

스테이지 진행 상태와 저장 경계입니다.

담당 기능:
- `BoxStackStageProgress`가 현재 스테이지 인덱스와 최고 해금 스테이지 인덱스를 보관.
- 스테이지 이동, 선택, 첫 스테이지 선택, 다음 스테이지 존재 여부, 스테이지 해금 여부 판정.
- 진행 초기화, 전체 해금, 성공 시 다음 스테이지 해금.
- `PlayerPrefs`에서 최고 해금 스테이지를 로드.
- `PlayerPrefs`에 최고 해금 스테이지 번호를 저장.

B098 기준으로 `BoxStackPrototype`은 이 클래스를 통해 스테이지 진행 상태를 조작합니다. 프로토타입에서는 `PlayerPrefs`를 유지합니다. App-in-Toss 또는 다른 제품 저장소로 교체할 때 먼저 확인해야 하는 파일이며, 교체 기준은 `docs/architecture/boxstack-stage-progress-storage-decision.md`에 기록합니다.

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
- `BoxStackActiveBoxMotion.Move`
- `BoxStackActiveBoxMotion.GetScreenSafeMoveRange`
- `BoxStackActiveBoxMotion.GetActiveBoxHalfWidth`

활성 박스가 드롭 전 어떻게 움직이는지, 카메라 화면 안에 어떻게 머무는지, 화면 때문에 이동 범위가 줄어들어도 후반 스테이지 속도감을 어떻게 유지하는지 결정합니다. B104 기준 실제 이동 계산은 `BoxStackActiveBoxMotion`이 맡고, `BoxStackPrototype.MoveActiveBox`는 흐름상 언제 이동을 적용할지만 연결합니다.

### 박스 낙하와 충격 튜닝

먼저 볼 곳:
- `BoxStackPrototype.DropActiveBox`
- `BoxStackPrototype.ResolveDrop`
- `BoxStackDropPhysics.BeginDrop`
- `BoxStackDropPhysics.ResetVelocityBeforeStackContact`
- `BoxStackDropPhysics.ClampFallSpeed`
- `BoxStackDropPhysics.StackMotionIsStable`
- `BoxStackDropPhysics.FreezePlacedBoxPhysics`
- `BoxStackPrototype.CreatePhysicsMaterials`
- `BoxStackPrototypeConfig.TuningSettings`

박스가 떨어질 때의 중력, 최대 낙하 속도, 기존 스택 접촉 직전 속도 리셋, 마찰, 감쇠, 정착 판정을 처리합니다. B100 기준 드롭 중 물리 세부 처리는 `BoxStackDropPhysics`가 맡고, `BoxStackPrototype`은 언제 드롭을 시작하고 다음 흐름으로 넘길지만 조정합니다.

### 블록 비주얼

먼저 볼 곳:
- `BoxStackPrototype.CreatePrototypeBox`
- `BoxStackPrototype.GetStackBlockTint`
- `BoxStackPrototypeAssetLoader.CreateStackBlockPlaceholder`

블록의 내부 그라데이션, 스테이지 진행에 따른 색상, 테두리 유무를 조정합니다. B053에서는 전체 스택 그라데이션을 박스별 구간으로 나눴고, B054에서는 어두운 테두리 밴드를 제거했습니다. B070에서 테스트한 박스 코드/순서별 색 변주와 B072에서 테스트한 박스 타입별 비색상 명암/질감 변주는 쌓인 모습이 어색해 적용하지 않습니다. B042/B043에서 테스트한 접촉 그림자와 B045에서 테스트한 위쪽/오른쪽 내부 면 분리는 자연스럽지 않아 되돌렸습니다.

### 성공과 실패 규칙

먼저 볼 곳:
- `BoxStackRunRules.TryEvaluateDroppedBoxFailure`
- `BoxStackRunRules.TryEvaluateStackFailure`
- `BoxStackRunRules.AnyBoxLost`
- `BoxStackRunRules.StackHasMultipleFloorContacts`
- `BoxStackRunRules.BoxIsLost`
- `BoxStackClearValidation.Begin`
- `BoxStackClearValidation.IsComplete`
- `BoxStackPrototype.BeginClearValidation`
- `BoxStackPrototype.UpdateClearValidation`

B099 기준 실패 판정은 `BoxStackRunRules`가 맡고, B105 기준 클리어 검증 타이머는 `BoxStackClearValidation`이 맡습니다. B112 기준 런 종료 결과 결정은 `BoxStackRunEndFlow`가 맡고, `BoxStackPrototype`은 판정 결과를 받아 `EndRun`에서 실제 Unity 상태 전환, 결과 UI/오디오, 스테이지 해금을 실행합니다. 목표 박스 수 도달 후 5초 생존 검증 흐름은 유지합니다.


### 스테이지 선택과 해금

먼저 볼 곳:
- `BoxStackPrototype.OpenStageSelect`
- `BoxStackPrototype.CloseStageSelect`
- `BoxStackStageSelectSession`
- `BoxStackStageFlow`
- `BoxStackPrototype.SelectStage`
- `BoxStackPrototype.UnlockNextStage`
- `BoxStackPrototype.UnlockAllStagesForPlaytest`
- `BoxStackPrototype.ResetStageProgress`
- `BoxStackStageProgress`
- `BoxStackStageProgressStore`
- `BoxStackPrototypeUi.RefreshStageButtons`

스테이지 선택 오버레이 열기, 스테이지 선택, 진행 저장, 해금/잠금 버튼 표시를 처리합니다. B098 기준 현재/최고 해금 인덱스와 선택/해금/초기화 판단은 `BoxStackStageProgress`가 맡고, `BoxStackStageProgressStore`는 `PlayerPrefs` 저장만 맡습니다. B106 기준 스테이지 선택 팝업을 열기 전 상태와 타임스케일 저장/복원은 `BoxStackStageSelectSession`이 맡습니다. B108 기준 스테이지 조작 후 팔레트 적용, 런 재시작, UI 갱신 중 무엇을 할지 결정하는 책임은 `BoxStackStageFlow`가 맡습니다. B090 기준 스테이지 타일은 두 줄 라벨 대신 `01`, `02`, `03` 같은 큰 번호만 표시합니다. 현재/해금/잠김 상태는 타일 색상과 비활성 상태로 구분합니다.

### UI 문구

먼저 볼 곳:
- `BoxStackResultFlow`
- `BoxStackPrototype.HandleCurrentResultButton`
- `BoxStackPrototypeUiStateFactory.GetResultTitle`
- `BoxStackPrototypeUiStateFactory.GetResultBody`
- `BoxStackPrototypeUiStateFactory.GetResultButtonLabel`

결과 팝업 문구를 바꿀 때는 `BoxStackPrototypeUiStateFactory`를 먼저 확인하고, 결과 버튼을 눌렀을 때의 흐름을 바꿀 때는 `BoxStackResultFlow`를 먼저 확인합니다. B088 기준 결과 팝업 본문은 클리어를 `탑이 안정됐어요`, `STACK SPREAD` 실패를 `탑이 무너졌어요`로 짧게 표현합니다. B091 기준 새 최고 진행 기록을 갱신한 클리어는 결과 본문에 `최고 기록 갱신`을 한 줄 더 표시합니다. B107 기준 결과 버튼 액션 결정은 `BoxStackResultFlow`가 맡습니다. B089에서 시도한 스테이지 도전 라벨은 철회했고, B090 기준 스테이지 타일 문구는 `BoxStackPrototypeUi.GetStageButtonLabel`에서 큰 번호 한 줄만 표시합니다. 플레이 중 중앙 피드백 토스트는 B046에서 제거했으므로 `좋아요`, `유지!` 같은 착지 피드백 문구는 더 이상 생성하지 않습니다. `BoxStackPrototypeUi.cs`는 가능하면 정적인 레이아웃 텍스트에만 문구를 둡니다.

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
-> `CreateSceneComposition`
-> `CreateRuntimeHosts`
-> `LoadStageProgress`
-> `EnsureRuntimeUi`
-> `EnsureRuntimeAudio`
-> `RestartGame`
-> `SpawnNextBox`

### 박스 하나 드롭

`Update`
-> `MoveActiveBox`
-> `DropPressed`
-> `DropActiveBox`
-> `ResolveDrop`에서 떨어진 박스와 기존 탑의 움직임이 안정될 때까지 대기
-> 안정 배치가 확정되면 놓인 박스 순서로 `BoxStackPrototypeAudio.PlayPlacement`
-> `SpawnNextBox`, `BeginClearValidation`, `EndRun` 중 하나로 이동

### 스테이지 클리어

배치된 박스 수가 현재 목표에 도달
-> `BeginClearValidation`
-> `UpdateClearValidation`
-> 스택이 `ClearValidationSeconds` 동안 유지됨
-> `EndRun(true)`
-> `BoxStackPrototypeAudio.PlayClear`


`DropActiveBox`가 드롭 직전 스냅샷 저장
-> `Playing` 상태로 복귀
-> `SpawnNextBox`

## 현재 프로토타입 주의점

- 아직 최종 제품 아키텍처가 아니라 프로토타입 코드입니다.
- `BoxStackPrototype.cs`가 여전히 여러 게임플레이 책임을 함께 가지고 있습니다.
- 현재 UI는 UXML이 아니라 C# 코드로 생성합니다.
- 에셋 로딩은 빠른 프로토타입과 WebGL 포함을 위해 `Resources`를 사용합니다. parcel/background PNG는 현재 `Assets/Art/Prototype/...` 원본과 `Assets/Resources/Prototype/...` 런타임 복사본을 함께 유지합니다. 제품화 전환의 기본 후보는 Addressables가 아니라 씬/프리팹/ScriptableObject 직렬화 참조이며, 원격 다운로드/카탈로그 업데이트/스킨 또는 스테이지 팩 단위 로딩/빌드 크기 압박/App-in-Toss 패키징 요구가 생길 때만 Addressables 도입을 검토합니다. 결정 근거는 `docs/architecture/boxstack-prototype-asset-loading-decision.md`에 기록합니다.
- 스테이지 진행은 `PlayerPrefs`를 사용합니다. 현재 저장값은 최고 해금 스테이지 번호 하나뿐이므로 프로토타입 동안은 유지하고, App-in-Toss 생산 환경에서 특정 저장 API가 필요하거나 계정/기기 간 동기화, 별/재화/이벤트 진행 같은 확장 저장값이 생길 때 `BoxStackStageProgressStore` 내부를 교체합니다. 결정 근거는 `docs/architecture/boxstack-stage-progress-storage-decision.md`에 기록합니다.
- WebGL/mobile 검증은 milestone spot check로 남기고, AI 에이전트의 일반 C# 검증은 `dotnet build BoxStack.slnx`를 우선 사용합니다. 실제 플레이 감각과 UI 체감 검증은 사용자가 Unity Editor Play Mode에서 직접 확인합니다.
- 런타임 UI는 던파 비트비트체 v2를 HUD/버튼/제목 표시 폰트로, Gmarket Sans Bold를 보조 설명 텍스트로, Noto Sans KR을 fallback으로 사용합니다. UI Toolkit 렌더링 반영을 위해 `unityFont`와 `unityFontDefinition`을 함께 지정합니다.
- B069는 배송/택배 시각 테마를 걷어내고 Stack-like 2D 디자인 가이드에 맞춰 스테이지별 유사 색 계열 팔레트, 전체 스택 그라데이션을 박스별 구간으로 나눠 굽는 컬러 블록, 코드 생성형 뒷배경 그라데이션, 테두리 없는 반투명 글래스풍 HUD를 사용합니다. B050에서 스택 전체를 하나의 세로 그라데이션처럼 읽히도록 아래 박스는 더 진하고 위/다음 박스는 더 밝게 이어지는 색상 진행을 적용했고, B053에서 각 박스 스프라이트가 해당 색상 흐름의 자기 구간을 직접 갖도록 바꿨습니다. B054에서는 B053의 어두운 블록 테두리 밴드를 제거해 경계선 없는 블록을 테스트했고, B055에서는 시작 색을 배경 최상단에 가까운 아주 진한 색으로 당겨 대비를 키웠습니다. B056에서는 블록 텍스처의 투명 가장자리를 제거하고 전체 픽셀을 불투명 그라데이션으로 채워, 쌓인 박스 사이에 배경색이 얇게 비치는 현상을 줄였습니다. B061에서는 사용자가 수치 조정을 진행할 수 있도록 비주얼은 전체 `WorldSize`로 유지하고 콜라이더만 `WorldSize * 0.96f`로 다시 줄인 상태를 튜닝 시작점으로 두었습니다. B062에서는 상단 중앙 진행 인디케이터의 채움/테두리 색을 현재 스테이지 Primary 역할의 `palette.Accent`와 맞췄고, B063-B065에서는 결과 팝업 스탬프 제거와 버튼 테마 색상을 정리했습니다. B068에서는 스테이지 선택 타일의 크기, 현재/해금/잠김 상태 라벨, 팔레트 기반 버튼 색상을 다듬었고, B069에서는 닫기 버튼도 같은 스테이지 테마 색상 체계에 맞췄습니다. 시각적으로 어색하다고 판단된 B042/B043 접촉 그림자 실험, B045 위쪽/오른쪽 면 분리, B049 박스 모양별 그라데이션 차별화는 적용하지 않습니다. 플레이 중 중앙 피드백 토스트도 B046에서 제거했습니다.
- B070 박스 코드/순서별 색 변주 실험은 쌓을 때 색상이 자연스럽게 이어지지 않아 커밋하지 않고 되돌렸습니다. 런타임 마커는 B069를 유지합니다.
- B071은 좁은 모바일 safe-area에서 상단 중앙 진행 패널이 스테이지 배지와 겹치지 않도록 진행 패널 최대 폭을 safe-area 기준으로 제한합니다.
- B072는 B070처럼 색을 흔들지 않고 생성 블록 스프라이트에 박스 타입별 아주 약한 명암/질감 중심 차이만 추가했지만, 실제 체감이 어색해 적용하지 않습니다.
- B073은 phone WebGL에서 상단 중앙 박스 인디케이터, 결과 팝업, 스테이지 선택 팝업이 화면 밖으로 잘리는 문제를 겨냥해 `Screen.safeArea` 픽셀 좌표를 UI Toolkit 패널 좌표로 변환합니다.

- B074는 스테이지 선택 팝업이 모바일 화면 하단에 너무 붙지 않도록 safe-area 높이 기준의 하단 여백과 패널 최대 높이를 함께 계산합니다.
- B083은 박스를 놓은 뒤 고정 1초 대기 대신 떨어진 박스와 기존 탑의 선형/각속도가 안정 기준 아래로 유지될 때 다음 박스를 생성합니다.
- B084는 single-column x 허용 오차 실패를 제거하고, 두 개 이상의 박스가 바닥 콜라이더에 닿으면 실패하도록 바꿉니다. `STACK SPREAD` 실패에서는 떨어진 원인 박스를 삭제하지 않고 남겨 정지시켜 실패 이유가 보이게 합니다. 실패 이후 기존 `ResolveDrop` 코루틴이 이어져 다시 `Playing`으로 돌아가지 않도록 상태 guard를 둡니다.
- B085의 놓인 박스 x축 속도 감쇠/제한 실험은 테스트 결과 부자연스러워 적용하지 않습니다. 현재 활성 런타임 기준은 B084입니다.
- B086은 낙하 속도 상한은 유지하되, 낙하 박스가 기존 박스와 거의 닿기 직전 y축 속도를 1회 0으로 리셋해 박스끼리 충돌 충격으로 서로 좌우로 밀어내는 현상을 줄입니다. 기존에 놓인 박스의 x축 속도나 Rigidbody constraint는 건드리지 않으며, 사용자 확인에서 B085보다 자연스럽다고 수용했습니다.
- 코드 변경으로 기능 책임, 파일 위치, 주요 메서드, 실행 흐름, 주의점이 달라지면 이 코드 맵도 같은 변경 묶음에서 갱신합니다.
