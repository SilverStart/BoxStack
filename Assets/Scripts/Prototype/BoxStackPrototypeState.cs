// 프로토타입 실행 상태 - 게임 흐름과 UI 표현이 같은 상태 이름을 공유하도록 둔다.

internal enum BoxStackPrototypeState
{
    Playing,
    ResolvingDrop,
    ValidatingClear,
    StageSelect,
    Won,
    Failed
}
