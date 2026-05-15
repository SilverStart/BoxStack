---
name: context-diet
description: Reduce unnecessary token usage during Codex work by planning file reads, avoiding repeated large-file reads, preferring search/section extraction before full reads, using Unity-specific scanners for serialized assets, and recording reusable context summaries. Use when the user asks to reduce token usage, optimize context, audit repeated reads, improve agent workflow efficiency, or create a low-token work process for this project.
---

# Context Diet

중간 이상 규모의 분석/구현 작업을 시작하기 전, 또는 사용자가 토큰 사용량/반복 읽기/컨텍스트 효율을 언급했을 때 이 스킬을 사용한다.

## 목표

정확한 판단에 필요한 최소 문맥만 읽는다.

중요한 문맥을 생략하지 않는다. 대신 넓은 전체 읽기를 단계적 읽기로 바꾼다.

## 읽기 전략

1. 먼저 작업 질문을 한 문장으로 정리한다.
2. 관련 가능성이 높은 파일 후보를 나열한다.
3. 각 파일을 분류한다.
   - 작은 파일: 바로 읽는다.
   - 큰 상태/문서 파일: 헤더, 상태 블록, 관련 섹션부터 읽는다.
   - 큰 코드 파일: 심볼명, 메서드명, 필드명을 먼저 검색한다.
   - Unity YAML asset: `unity-scanner`가 설치되어 있으면 우선 사용한다.
   - 생성물/build/cache 파일: 직접 관련이 없으면 읽지 않는다.
4. 먼저 일치하는 최소 문맥만 읽는다.
5. 표적 읽기로 부족하거나 모순이 있으면 전체 읽기로 확장한다.

## BoxStack 주요 대형 파일

다음 파일은 기본적으로 표적 읽기를 우선한다.

- `production/session-state/active.md`
- `production/progress-dashboard.md`
- `Assets/Scripts/Prototype/BoxStackPrototype.cs`
- `docs/workflow/unity-cli-connector.md`
- `.codex/skills/**/SKILL.md`
- `.agents/skills/**/SKILL.md`

`.codex/skills`와 `.agents/skills`에 같은 스킬이 있을 때는, 비교 작업이 아닌 이상 현재 활성 skill root의 파일 하나만 읽는다.

## 검색 우선 패턴

큰 파일은 `Get-Content`로 전체를 읽기 전에 `rg -n`으로 위치를 먼저 찾는다.

```powershell
rg -n "STATUS|Next Action|Open Questions|Risks" production/session-state/active.md
rg -n "Next Immediate Action|Current Decisions|Open Questions|Risks" production/progress-dashboard.md
rg -n "RefreshPrototypeUi|CanUseUndoSkill|DropPressed" Assets/Scripts/Prototype/BoxStackPrototype.cs
```

## 빠른 상태 확인 경로

사용자가 "현재 진행상황 확인", "다음 작업 리스트업", "남은 작업 알려줘", "커밋 직후 상태 확인"처럼 요약을 요청하면 다음 경로를 기본값으로 사용한다.

1. `git status --short --branch`로 브랜치와 워크트리 상태만 확인한다.
2. `production/session-state/active.md`와 `production/progress-dashboard.md`는 전체 읽기를 금지하고, 아래 검색 결과만 먼저 본다.

```powershell
rg -n "STATUS|Next Action|Open Questions|Risks" production/session-state/active.md
rg -n "Next Immediate Action|Current Decisions|Open Questions|Risks" production/progress-dashboard.md
```

3. 검색 결과만으로 답할 수 없을 때는 해당 섹션 주변부만 제한적으로 읽는다.
4. 전체 읽기는 문서를 직접 수정해야 하거나, 표적 검색 결과가 모순되거나, 사용자가 전체 검토를 명시적으로 요청한 경우에만 한다.

코드는 먼저 심볼을 검색한 뒤, 필요한 주변 구간만 읽는다.

Unity serialized 파일은 가능하면 다음을 우선한다.

```powershell
unity-scanner read -p C:\unity\BoxStack <asset-path> --depth 3 --limit 80
unity-scanner refs -p C:\unity\BoxStack <source-path> Assets --detail
```

`unity-scanner`가 설치되어 있지 않으면 다음 검색으로 대체한다.

```powershell
rg -n "m_Name:|m_Script:|guid:" Assets
```

## 요약 재사용

큰 파일을 읽었고 그 결론이 이후 작업에도 중요하다면, 응답이나 승인된 프로젝트 상태 파일에 재사용 가능한 사실만 짧게 요약한다.

토큰 절약만을 목적으로 프로젝트 파일을 수정하지 않는다. 파일 갱신은 사용자가 승인했을 때만 한다.

## 절감량 측정

정확한 API usage 로그가 있으면 그 값을 우선한다.

사용량 로그가 없을 때는 `tools/context-budget/Measure-Context.ps1`로 모델에 넣을 텍스트 규모를 근사 측정한다. 기본 추정식은 `문자 수 / 3.5`이다.

전체 파일 기준 비용을 볼 때:

```powershell
.\tools\context-budget\Measure-Context.ps1 -Path production\progress-dashboard.md
```

표적 검색 결과를 baseline과 비교할 때:

```powershell
rg -n "Next Immediate Action|Current Decisions|Risks" production\progress-dashboard.md |
  .\tools\context-budget\Measure-Context.ps1 -Label "dashboard targeted" -BaselinePath production\progress-dashboard.md
```

측정값은 근사치이다. 목적은 청구 토큰을 정확히 재는 것이 아니라, 전체 읽기와 표적 읽기의 상대적 차이를 확인하는 것이다.

## 전체 읽기가 맞는 경우

다음 경우에는 전체 읽기를 피하지 않는다.

- 해당 파일을 직접 수정해야 할 때
- 버그 리뷰나 설계 리뷰처럼 전체 맥락이 필요한 때
- 정확한 문구가 중요한 때
- 표적 읽기 결과가 불완전하거나 서로 모순될 때
- 파일이 충분히 작아서 표적 읽기보다 전체 읽기가 더 단순할 때

## 보고 방식

이 스킬을 사용한 뒤에는 짧게 보고한다.

- 어떤 큰 전체 읽기를 피했는지
- 어떤 검색 또는 scanner 명령을 사용했는지
- 그래도 전체 읽기가 필요했던 파일이 있었는지
- 새로 발견한 프로젝트별 컨텍스트 절약 개선점이 있는지
