# BoxStack Game UI Research

Last updated: 2026-05-13
Status: Draft for direction selection
Scope: Visual/UI direction research only. No implementation changes.

## Goal

현재 BoxStack 프로토타입 UI가 일반 앱처럼 느껴지는 문제를 줄이고, Toss 앱인토스 안에서도 즉시 "게임"으로 인식되는 HUD/피드백 방향을 고른다.

## Current Read

현재 적용 방향은 `design/ui/hud-variants/hud-variant-01-toss-minimal.png` 계열이다.

장점:

- Toss 앱 안에 들어가도 낯설지 않은 조용한 톤이다.
- 스테이지, 박스 수, 진행률, 되돌리기 같은 정보 구조가 명확하다.
- UI Toolkit 구현으로 안전 영역, 스테이지 선택, 결과 팝업까지 정리되어 있다.

문제:

- 흰색 상단 바, 진행률, 텍스트 버튼 조합이 금융/생산성 앱 헤더처럼 읽힌다.
- 게임의 핵심 감정인 "잘했다", "아슬아슬하다", "조금만 더"가 HUD에서 거의 드러나지 않는다.
- 택배/적재 세계관이 UI 언어에 충분히 들어오지 않아, 상자 스프라이트와 UI가 따로 노는 느낌이 난다.
- 현재 단색 배경은 가독성에는 좋지만, 첫인상에서 놀이 공간보다 테스트 화면처럼 보일 위험이 있다.

## Reference Takeaways

### Stack by Ketchapp

Source: https://apps.apple.com/us/app/stack/id1080487957

- UI를 극단적으로 줄이고, 블록과 점수 감각을 전면에 둔다.
- BoxStack에 그대로 적용하려면 HUD를 줄이는 대신 상자 착지 피드백, 실패 연출, 점수/기록 갱신 감각을 훨씬 강하게 만들어야 한다.

### Suika Game+

Source: https://apps.apple.com/us/app/suika-game/id6741622025

- 단순한 드롭 규칙을 점수, 진화, 랭킹, 오버플로우 위험으로 게임화한다.
- BoxStack은 "배송 완료", "완벽 적재", "흔들림 위험", "스테이지 별점" 같은 상태 언어가 어울린다.

### Good Pizza, Great Pizza

Source: https://apps.apple.com/us/app/good-pizza-great-pizza/id911121200

- UI가 기능 패널이 아니라 가게 운영 세계 안에 붙어 있다.
- BoxStack도 일반 카드 UI보다 송장, 배송 라벨, 적재 스탬프, 작업 현황판 같은 메타포가 맞다.

### Boba Story

Source: https://apps.apple.com/us/app/boba-story/id1563575361

- 귀여운 재료, 꾸미기, 상점 분위기가 반복 플레이 의욕을 만든다.
- BoxStack에 적용하면 박스 스킨, 테이프, 스티커, 배송지 테마, 클리어 보상 연출이 동기 요소가 된다.

### Pokemon Cafe ReMix

Source: https://apps.apple.com/us/app/pok%C3%A9mon-caf%C3%A9-remix/id1496738228

- 목표, 별점, 캐릭터/보상 피드백이 화면을 게임처럼 잡아준다.
- BoxStack은 캐릭터가 없어도 별점, 칭찬 토스트, 콤보 배지, 클리어 리본으로 비슷한 효과를 낼 수 있다.

### App in Toss Context

Sources:

- https://www.yna.co.kr/view/AKR20260209062400017
- https://m.dailian.co.kr/news/view/1608199

인사이트:

- 앱인토스에서는 설치 없이 즉시 실행되는 게임이 강점으로 언급된다.
- 따라서 "Toss답게 조용한 앱"보다 "Toss 안에서 바로 이해되는 가벼운 게임" 쪽이 더 적합하다.
- 첫 3초 안에 게임 목표, 조작, 보상 감각이 보여야 한다.

## Design Principles

1. HUD는 앱 헤더가 아니라 게임 장치처럼 보여야 한다.
2. 정보는 유지하되, 표시 방식은 배송/적재 세계관에 묶는다.
3. 착지 순간 피드백을 중앙 플레이 영역 가까이에 둔다.
4. 상단 흰색 대형 바 의존도를 줄인다.
5. 결과 팝업은 일반 확인 모달보다 클리어 스탬프/배송 영수증처럼 만든다.
6. Toss fit은 "무채색 앱 UI"가 아니라 "정돈된 캐주얼 게임"으로 해석한다.

## Direction A: Delivery Arcade

한 줄 요약: 현재 02 Delivery Tracker와 03 Casual Game을 섞어, 배송 진행도와 착지 피드백을 강하게 만든다.

구성:

- 상단 좌측: 작은 송장형 스테이지 배지.
- 상단 우측: 되돌리기 아이콘/티켓 배지.
- 화면 한쪽: 세로 배송 게이지. 남은 박스 수를 노드로 표시한다.
- 중앙: 착지 순간 `PERFECT`, `좋아요`, `아슬아슬`, `배송 완료` 토스트.
- 결과 팝업: 스탬프가 찍힌 배송 완료 카드.

장점:

- 가장 게임처럼 보이는 변화가 빠르게 난다.
- 기존 박스/배송 소재와 잘 맞는다.
- 현재 UI Toolkit 구조에서 점진적으로 구현 가능하다.

위험:

- 피드백이 과하면 Toss 안에서 너무 광고 게임처럼 보일 수 있다.
- 세로 게이지가 플레이 영역이나 손가락 입력과 충돌하지 않도록 위치 검증이 필요하다.

적합도: 높음
구현 난이도: 중간
추천도: 1순위

## Direction B: Clean Game, Not App

한 줄 요약: Toss Minimal을 유지하되 앱 헤더 느낌을 제거하고, 작은 게임 HUD 배지로 바꾼다.

구성:

- 기존 흰 상단 바 제거 또는 높이 축소.
- 스테이지, 박스 수, 진행률을 각각 작은 캡슐/배지로 분리.
- 진행률은 얇은 바 대신 박스 아이콘 슬롯으로 표현.
- 되돌리기는 텍스트 버튼보다 티켓/되감기 아이콘 배지로 변경.
- 결과 팝업은 현재 구조를 유지하되 스탬프, 별, 리본을 추가.

장점:

- 현재 구현과 가장 잘 이어진다.
- 앱인토스 안에서도 이질감이 적다.
- 기능 리스크가 낮다.

위험:

- 배경과 피드백이 약하면 여전히 앱처럼 보일 수 있다.
- 강한 첫인상 개선은 Direction A보다 약하다.

적합도: 높음
구현 난이도: 낮음
추천도: 2순위

## Direction C: Toy Parcel

한 줄 요약: 박스, 테이프, 스티커, 플랫폼을 장난감처럼 강화하고 UI도 귀여운 라벨 스타일로 통일한다.

구성:

- 박스마다 작은 표정, 스티커, 테이프 색 차이를 준다.
- HUD는 배송 라벨, 스탬프, 스티커 보드 느낌으로 처리한다.
- 클리어 시 리본, 별, 종이 조각, 스티커 보상 연출.
- 배경은 물류센터보다 작은 데스크/포장대/토이박스 느낌으로 단순화.

장점:

- 반복 플레이 의욕과 애착을 만들기 좋다.
- 박스 스킨/수집/꾸미기 확장과 잘 맞는다.

위험:

- Toss fit과 멀어질 수 있다.
- 아트 방향을 크게 바꾸므로 UI보다 에셋 작업량이 커진다.

적합도: 중간
구현 난이도: 높음
추천도: 3순위

## Recommendation

1차 개선안은 Direction A: Delivery Arcade를 추천한다.

이유:

- 현재 문제인 "일반 앱 같다"는 인상을 가장 직접적으로 줄인다.
- 배송/적재라는 소재를 UI 언어로 끌어올 수 있다.
- 기존 02, 03 시안의 좋은 부분을 재사용할 수 있어 완전히 새 방향은 아니다.
- Toss 앱인토스 맥락에서도 과하게 무거운 게임 UI가 아니라, 즉시 이해되는 캐주얼 게임 HUD로 조절 가능하다.

보수적으로 가려면 Direction B를 먼저 적용하고, Direction A의 중앙 피드백만 일부 섞는 것도 가능하다.

## Proposed Next Mockups

다음 시안 제작 시 3장을 만든다.

1. Delivery Arcade
   - 세로 배송 게이지, 착지 피드백, 송장형 결과창 포함.

2. Clean Game, Not App
   - 기존 Toss Minimal을 재해석한 낮은 리스크 개선안.

3. Toy Parcel
   - 귀여운 상자/스티커/보상 감각을 강조한 감성형 방향.

각 시안은 같은 게임 장면을 기준으로 비교한다.

- Portrait mobile viewport
- Stage 5 기준, `4 / 7` 진행 상태
- 되돌리기 1회 사용 가능
- 상자 3개 이상 쌓인 상태
- 착지 피드백이 방금 표시된 순간

## Validation Criteria

시안 선택 전 확인할 질문:

- 3초 안에 게임처럼 보이는가?
- 목표와 남은 박스 수가 즉시 보이는가?
- 플레이 영역을 가리지 않는가?
- Toss 앱 안에 있어도 너무 이질적이지 않은가?
- 다시 한 판 하고 싶게 만드는 보상/피드백이 있는가?
- 작은 모바일 화면에서 텍스트 없이도 상태가 읽히는가?

## Decision

Selected direction: **A: Delivery Arcade**

Decision date: 2026-05-13

Rationale:

- It most directly fixes the current "normal app UI" read.
- It uses the parcel/delivery theme as UI language instead of leaving the HUD as a generic app header.
- It can reuse the useful parts of existing variants 02 and 03 without discarding the clean Toss-compatible tone.
- Direction B should remain as a restraint principle: keep the UI clean, lightweight, and readable even while adding stronger game feedback.

Next document:

- `design/ui/boxstack-delivery-arcade-hud-spec-2026-05-13.md`

Next implementation posture:

- Do not change gameplay rules during the first UI pass.
- Replace the large top bar with compact game HUD badges.
- Add a side delivery progress rail.
- Add central landing feedback.
- Restyle result popups as delivery completion/failure cards.
