# BoxStack 프로토타입 에셋 로딩 전환 결정

날짜: 2026-05-18
상태: Accepted for prototype
런타임 마커: B074

## 배경

현재 BoxStack 프로토타입은 WebGL 빌드 포함 안정성을 위해 `Assets/Resources/Prototype/...` 아래의 설정, 폰트, UI 리소스, parcel/background PNG 복사본을 사용한다.

`BoxStackPrototypeAssetLoader`가 `Resources.Load` 경로를 먼저 시도하고, Editor Play에서는 `AssetDatabase` 또는 파일 기반 fallback을 통해 원본 아트 경로도 읽을 수 있게 해둔 상태다. 이 구조는 빠른 프로토타입 반복과 phone WebGL 확인에는 충분히 단순하고 안정적이었다.

다만 Unity 6.3 기준으로 장기적인 에셋 관리에는 Addressables 또는 명시적인 직렬화 참조가 더 적합하다. 특히 `Resources`는 빌드 포함 범위가 넓고 동기 로딩이라, 제품화 단계에서는 빌드 무게와 로딩 제어 측면에서 다시 판단해야 한다.

## 결정

프로토타입 단계에서는 현재 `Resources` 기반 런타임 경로를 유지한다.

제품화 전환 시 기본 후보는 Addressables가 아니라 **씬/프리팹/ScriptableObject의 직렬화 참조**로 둔다. 현재 에셋은 수가 적고, 런타임 원격 다운로드나 카탈로그 업데이트 요구가 없으며, App-in-Toss WebGL 빌드 안에 함께 포함되는 정적 리소스에 가깝기 때문이다.

Addressables는 다음 조건 중 하나가 실제 요구사항이 될 때 도입한다.

- 원격 에셋 다운로드나 카탈로그 업데이트가 필요하다.
- 스킨, 테마, 스테이지 팩처럼 묶음 단위 에셋 로딩/해제가 필요하다.
- 빌드 크기나 메모리 사용량 때문에 에셋 그룹별 관리가 필요하다.
- App-in-Toss 배포 정책에서 Addressables 또는 AssetBundle 계열 분리가 필요하다.

현재 `Packages/manifest.json`에는 `com.unity.addressables` 패키지가 없으므로, 지금 단계에서는 새 패키지를 추가하지 않는다.

## 유지할 것

- `Assets/Resources/Prototype/BoxStackPrototypeConfig.asset`
- `Assets/Resources/Prototype/Fonts/...`
- `Assets/Resources/Prototype/Ui/...`
- `Assets/Resources/Prototype/Parcel/...`
- `Assets/Resources/Prototype/Backgrounds/...`
- `Assets/Art/Prototype/...` 원본/참조 아트 위치
- `BoxStackPrototypeAssetLoader`를 에셋 로딩 교체 경계로 유지

## 제품화 전환 기준

전환 작업은 프로토타입의 플레이 감각과 UI 방향이 더 안정된 뒤 진행한다.

1. 먼저 실제 App-in-Toss MVP에 필요한 에셋 목록을 고정한다.
2. 정적이고 수가 적은 에셋은 씬/프리팹/ScriptableObject 직렬화 참조로 옮긴다.
3. 원격/묶음/다운로드 요구가 생긴 에셋만 Addressables 후보로 분리한다.
4. `Resources` 복사본을 삭제하기 전 WebGL 빌드에서 폰트, UI 리소스, 블록/배경 리소스가 모두 정상 렌더링되는지 확인한다.

## 영향

긍정적 영향:
- 지금 당장 검증된 WebGL 포함 경로를 흔들지 않는다.
- Addressables 패키지와 빌드 파이프라인 복잡도를 premature하게 추가하지 않는다.
- 제품화 때 교체해야 할 경계가 `BoxStackPrototypeAssetLoader`로 이미 모여 있다.

부정적 영향:
- 프로토타입 동안은 `Assets/Art/Prototype/...`와 `Assets/Resources/Prototype/...` 중복이 계속 남는다.
- `Resources` 폴더 안의 폰트와 PNG가 빌드 크기를 키울 수 있다.
- 제품화 전에 한 번은 에셋 참조 방식을 정리해야 한다.

## 검증 기준

전환 전:
- B074 기준 phone WebGL에서 폰트, UI, 블록, 배경이 placeholder 없이 렌더링된다.
- `Resources` 복사본과 원본 아트의 역할이 문서에 명확히 남아 있다.

전환 후:
- `dotnet build BoxStack.slnx`가 통과한다.
- Unity Editor Play Mode에서 프로토타입 설정, 폰트, UI 리소스, 블록/배경 리소스가 모두 로드된다.
- WebGL 빌드에서 Korean font, UI Toolkit theme/panel settings, block/background visuals가 유지된다.
- 삭제한 `Resources` 복사본 때문에 WebGL placeholder fallback이 보이지 않는다.

## 관련 문서

- `docs/workflow/ait-webgl-testing-notes.md`
- `docs/architecture/boxstack-prototype-code-map.md`
- `production/progress-dashboard.md`
