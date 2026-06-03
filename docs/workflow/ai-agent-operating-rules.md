# AI Agent Operating Rules

These rules are model-neutral project operating rules. They apply to any AI
assistant working in this repository, including Codex, Claude Code, Gemini, and
other coding agents.

## Shared Rule Source

- Treat this document as the shared place for user preferences that should
  persist across AI tools.
- When the user says a behavior should happen automatically in the future,
  record the behavior here unless it belongs only to one specific tool.
- Tool-specific files may link back to this document, but should not be the only
  place where cross-agent rules are stored.

## Temporary Verification Artifacts

- Unity Editor Play Mode, Unity CLI Connector, WebGL smoke checks, screenshots,
  and performance/test probes may create temporary verification artifacts.
- After the relevant test or verification pass is complete, remove temporary
  artifacts that are not intended to become source assets or documented
  evidence.
- Keep generated files such as `.tmp/`, `Screenshots/`, and
  `Assets/Resources/PerformanceTestRun*.json` out of commits unless the user
  explicitly asks to preserve them as evidence.
- If a new recurring temporary artifact appears, add it to `.gitignore` and
  clean it up before handing work back to the user.

## Runtime Verification Policy

- 일반 C# 코드 변경 후 기본 자동 검증은 `dotnet build BoxStack.slnx`를
  우선 사용한다.
- 검증 도구나 생성 프로젝트 파일의 한계 때문에 설계상 맞는 코드 구조를
  임시로 흐리거나 우회하지 않는다.
- 새 파일, 별도 클래스, 폴더 구조가 설계상 맞다면 그 구조를 유지하고,
  검증 실패는 코드 구조 변경이 아니라 검증 환경 문제로 분리해 보고한다.
- Unity 프로젝트 파일 재생성, Editor refresh, csproj 갱신처럼 검증 환경
  문제가 있을 때는 코드 구조를 바꾸기 전에 사용자에게 원인과 선택지를
  설명하고 승인을 받는다.
- 임시 우회가 정말 필요할 때는 먼저 이유, 범위, 되돌릴 방법을 설명하고
  사용자 승인을 받은 뒤 진행한다.
- AI 에이전트는 검증만을 목적으로 Unity CLI Connector를 사용해 Unity
  Editor Play Mode를 실행하거나 조작하지 않는다.
- 실제 게임 플레이 감각, UI 체감, 난이도, 터치/조작감 검증은 사용자가
  직접 Unity Editor Play Mode에서 확인한다.
- Unity CLI Connector는 AI 에이전트가 작업 진행을 위해 현재 열려 있는
  Unity Editor를 직접 조작해야 하는 경우에만 사용한다.
- WebGL/mobile 검증은 일반 반복 작업에서 제외하고 milestone spot check
  때만 진행한다.

## Git Hygiene

- Do not commit generated, temporary, or local validation artifacts unless the
  user explicitly asks for them to be preserved.
- Before committing, check `git status --short -uall` and confirm that only
  intentional source, asset, documentation, and metadata changes are staged.
- Leave unrelated user changes untouched.

## Documentation Updates

- Keep `production/session-state/active.md` and
  `production/progress-dashboard.md` consistent when status, next action,
  playtest verdict, or risk changes.
- 진행상황 확인, 다음 작업 리스트업, 커밋 직후 상태 점검처럼 현재 상태의
  요약만 필요한 요청에서는 `production/session-state/active.md`와
  `production/progress-dashboard.md`를 통째로 읽지 않는다. 먼저 `rg`로
  `STATUS`, `Next Action`, `Next Immediate Action`, `Current Decisions`,
  `Open Questions`, `Risks` 같은 관련 섹션 위치만 찾고, 필요한 주변
  문맥만 제한적으로 읽는다.
- 위 상태 문서의 전체 읽기는 문서를 직접 수정해야 하거나, 표적 검색 결과가
  서로 모순되거나, 사용자가 명시적으로 전체 검토를 요청한 경우로 제한한다.
- When a user preference affects future AI behavior across tools, update this
  document and reference it from tool-specific guidance when useful.
- When updating shared harness or operating rules in this project, also check
  whether the same rule should be mirrored into the starter-pack source at
  `C:\unity\starter-packs` so future projects inherit the improvement.
- Project documentation should be written in Korean by default. Keep source
  identifiers, API names, file paths, and quoted external terms unchanged when
  translating or maintaining documentation.
- When changing BoxStack prototype code, review
  `docs/architecture/boxstack-prototype-code-map.md` before handing work back.
  If feature ownership, file responsibility, method names, execution flow, or
  important caveats changed, update the code map in the same change so it does
  not become stale.
