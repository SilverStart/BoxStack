# BoxStack 스테이지 진행 저장 전환 결정

날짜: 2026-05-18
상태: Accepted for prototype
런타임 마커: B074

## 배경

현재 프로토타입은 최고 해금 스테이지를 `BoxStackStageProgressStore`를 통해 저장한다. 이 클래스는 아래 `PlayerPrefs` 키 하나를 감싼다.

```text
BoxStackPrototype.HighestUnlockedStage
```

현재 저장값은 최고 해금 스테이지 번호 하나뿐이다. 스테이지 선택의 디버그 컨트롤은 Editor/development 빌드에서 이 값을 stage 1로 초기화하거나 전체 해금으로 바꾸고, 일반 플레이는 스테이지 클리어 후 다음 스테이지를 해금한다.

Apps in Toss WebGL 템플릿은 `Assets/WebGLTemplates/AITTemplate/Runtime/appsintoss-unity-bridge.js`에서 JavaScript 저장 헬퍼도 노출한다.

- `aitSetStorageData`
- `aitGetStorageData`
- `aitRemoveStorageData`

이 헬퍼들은 현재 브라우저 `localStorage`에 `ait_` prefix를 붙여 저장한다. App-in-Toss 브리지 후보로 참고할 수는 있지만, 현재 프로토타입 C# 런타임은 아직 이 함수들을 호출하지 않는다.

## 결정

프로토타입 단계에서는 스테이지 진행 저장에 `PlayerPrefs`를 유지한다.

`PlayerPrefs`로 충족할 수 없는 실제 제품 요구가 생기기 전까지는 새 App-in-Toss 저장 wrapper를 추가하지 않는다.

`BoxStackStageProgressStore`는 저장소 교체 경계로 유지한다. 나중에 제품 저장소가 필요해지면 스테이지 선택, 해금 흐름, UI state 생성 로직은 건드리지 않고 이 클래스 내부를 교체하거나 그 뒤에 작은 interface를 둔다.

## 지금은 충분한 이유

- 프로토타입은 플레이테스트 편의를 위한 local integer 하나만 필요하다.
- 현재 테스트 범위에서는 Unity WebGL `PlayerPrefs`가 로컬 브라우저 persistence 용도로 충분하다.
- 지금 비동기 JavaScript 브리지를 추가하면 현재 스테이지 루프 검증에는 도움이 적고 callback/state 복잡도만 늘어난다.
- 기존 `BoxStackStageProgressStore`가 이미 저장 로직을 `BoxStackPrototype` 밖으로 분리하고 있다.
- 현재 AIT 템플릿 브리지 자체도 localStorage 기반이라, 즉시 전환한다고 더 강한 제품 persistence를 검증하는 것은 아니다.

## 제품화 전환 기준

아래 조건 중 하나가 실제 요구사항이 되면 이 결정을 다시 본다.

- App-in-Toss production policy가 Unity WebGL `PlayerPrefs` 대신 특정 저장 API를 요구한다.
- 진행도가 기기, 계정, 재설치/재오픈 경계, Toss user identity 사이에서 동기화되어야 한다.
- 저장 데이터가 최고 해금 스테이지를 넘어 별, 재화, 데일리 보상, 구매, 이벤트 진행 등으로 늘어난다.
- 저장 읽기/쓰기에 analytics, migration, encryption, anti-tamper, server validation이 필요해진다.
- 실제 App-in-Toss 패키지 테스트에서 target webview의 `PlayerPrefs` persistence가 불안정하다고 확인된다.

## 전환 형태

나중에 전환한다면 선호하는 형태는 다음과 같다.

1. `BoxStackStageProgressStore`의 외부 동작은 유지한다.
2. local prototype storage와 App-in-Toss bridge storage처럼 실제 backend가 둘 이상 필요해질 때만 내부 backend abstraction을 추가한다.
3. 저장 필드가 늘어나면 schema를 작게 유지하고 version을 둔다.
4. 기존 프로토타입 키가 의미 있으면 `BoxStackPrototype.HighestUnlockedStage`에서 1회 migration을 추가한다.
5. 진행 초기화, 전체 해금, 일반 스테이지 클리어 해금, 브라우저 새로고침 persistence, target App-in-Toss webview persistence를 확인한다.

## 현재 검증

이번 결정에서는 코드를 변경하지 않았다. 문서 전용 체크포인트이므로 `dotnet build BoxStack.slnx`는 필요하지 않다.

관련 파일:

- `Assets/Scripts/Prototype/BoxStackStageProgressStore.cs`
- `Assets/Scripts/Prototype/BoxStackPrototype.cs`
- `Assets/WebGLTemplates/AITTemplate/Runtime/appsintoss-unity-bridge.js`
- `docs/architecture/boxstack-prototype-code-map.md`
- `production/session-state/active.md`
- `production/progress-dashboard.md`
