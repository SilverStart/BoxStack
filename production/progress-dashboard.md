# Progress Dashboard

Last updated: 2026-05-13

## Next Immediate Action

B022 prototype asset loading boundary is implemented and verified without gameplay feel changes. Next checkpoint is user review/commit of the structure slice, then continue with the next small productization step. WebGL/mobile testing remains deferred to the next milestone spot check.

## Prototype / Playtest History

- 2026-05-02: Prototype harness created. No playable prototype or playtest verdict yet.
- 2026-05-02: First playable loop implemented as `Assets/Scripts/Prototype/BoxStackPrototype.cs`; playtest verdict pending.
- 2026-05-02: Camera follow added so taller stacks keep the active box and landing area in view; playtest verdict pending.
- 2026-05-02: Box visuals inset with thin edge frames so stack contact reads more clearly; playtest verdict pending.
- 2026-05-02: MainCamera rotation locked; camera now follows stack height by vertical movement only.
- 2026-05-02: Prototype converted from 3D cubes to 2D/2.5D parcel-box sprites using `Rigidbody2D` and `BoxCollider2D`; AI PNG asset pipeline now fits the direction.
- 2026-05-02: Basic stacking system judged good enough to continue; next prototype phase shifts from mechanics validation to 2D visual direction and PNG asset replacement.
- 2026-05-02: Lightweight art brief added and prototype code wired to load named parcel PNGs from `Assets/Art/Prototype/Parcel/` in Editor Play, with placeholder fallback.
- 2026-05-02: First AI-generated parcel PNG set added: basic, wide, tall, and stack base sprites with transparent backgrounds and Sprite importer metadata.
- 2026-05-02: Parcel PNG scaling/colliders changed to use visible alpha bounds, so transparent image padding no longer makes stacked boxes appear to float apart.
- 2026-05-03: First AI-generated logistics center background added and wired behind the prototype as a camera-fixed SpriteRenderer.
- 2026-05-03: Camera minimum y changed to derive from the floor bottom plus orthographic size with a small bottom padding, so portrait play cannot drift below the conveyor/floor baseline.
- 2026-05-04: Top HUD changed from one text line to game-like pill UI: box count pill, progress bar, and run/status pill.
- 2026-05-04: Four HUD mockup variants generated for selection: Toss Minimal, Delivery Tracker, Casual Game, and No Top HUD.
- 2026-05-04: HUD variant 01 Toss Minimal applied to the prototype as a single quiet top bar with box count, progress, and compact status text.
- 2026-05-04: 20-stage fixed-clear difficulty plan recorded at `design/quick-specs/20-stage-difficulty-plan-2026-05-04.md`; infinite stacking deferred to a later challenge-mode experiment.
- 2026-05-04: Success/fail result popup added so one play session has a clear ending and restart path.
- 2026-05-04: Logistics center image background disabled for focus testing; prototype now uses a parcel-brown solid camera background.
- 2026-05-05: Clear condition changed from box count only to single-column stacking; a settled box outside tolerance now fails immediately.
- 2026-05-05: Success now requires the completed 8-box stack to survive a 5-second validation window before the clear popup appears.
- 2026-05-05: Single-column tolerance relaxed from `0.45` to `0.75` so visually stable stacks are less likely to fail unfairly.
- 2026-05-05: Box `gravityScale` lowered from `2.4` to `1.6` to reduce excessive lower-box movement when stacks grow.
- 2026-05-05: Result popup restart now requires completing the button click on the button instead of any screen tap.
- 2026-05-05: Hardcoded `StageConfig` progression wired into the prototype with 20 fixed-clear stages, configured box sequences, speed/range multipliers, HUD stage display, and keyboard stage navigation.
- 2026-05-05: Stage-clear result popup now advances to the next stage; final stage clear loops back to stage 1.
- 2026-05-05: HUD stage label now opens a lightweight 20-stage select overlay; selecting a stage restarts the run on that stage.
- 2026-05-05: Playtest found stages 10+ significantly harder; prototype now tests a one-use undo skill that removes the last placed box instead of lowering the difficulty table.
- 2026-05-06: Undo test direction changed to a fail-popup rescue: the prototype captures a pre-drop stack snapshot, freezes the failed stack, and lets the player restore once per stage.
- 2026-05-06: Failure rescue UX changed toward a reward-ad prototype flow: the fail popup now offers "광고 보고 되돌리기" with a short mock confirmation delay before restoring the pre-drop stack snapshot.
- 2026-05-06: Fail popup now shows rescue status text, so playtests can verify whether players understand "available", "checking", and "used" states.
- 2026-05-06: Failure rescue popup copy clarified so the rescue ticket state appears before the reward-ad action: available, checking, and used states now use explicit stage-ticket wording.
- 2026-05-06: Failure rescue copy shortened toward user-facing wording: "한 번 되돌릴 수 있어요", "복구권 1회 남음", and "광고 보고 이어하기".
- 2026-05-06: Failure rescue policy changed to free-first: each stage now grants one free snapshot restore, then one mock reward-ad restore, then no further rescue.
- 2026-05-06: Stage progression now stores the highest unlocked stage with PlayerPrefs, disables locked stages in stage select, and unlocks the next stage on clear.
- 2026-05-06: Stage select now includes prototype-only progression controls for fast playtesting: reset unlock progress to stage 1 or unlock all 20 stages.
- 2026-05-06: Failure detection now also checks delayed stack collapse during normal play and drop resolution, so gravity-driven collapse can show the fail popup immediately.
- 2026-05-06: Runtime `PhysicsMaterial2D` added to parcel boxes and the floor with `friction = 1.2`, `bounciness = 0` to reduce unwanted sliding while keeping gravity collapse.
- 2026-05-06: Stage select progress test controls are now limited to Editor/development builds so reset/unlock helpers do not appear in normal user builds.
- 2026-05-06: Editor check confirmed stage select progress controls remain visible for playtesting; HUD, stage select, and result popup layout now use `Screen.safeArea` converted into IMGUI coordinates.
- 2026-05-07: AIT/WebGL browser testing found two blockers: Unity-launched Dev Server cannot find `pnpm`, and WebGL cannot load prototype PNGs through the Editor-only `AssetDatabase` fallback. Details and workaround are in `docs/workflow/ait-webgl-testing-notes.md`.
- 2026-05-07: Prototype PNGs were copied into `Assets/Resources/Prototype/...`, and the runtime loader now falls back from `Resources.Load<Sprite>` to `Resources.Load<Texture2D>` so WebGL builds can include parcel and floor sprites.
- 2026-05-07: After rebuilding AIT/WebGL, PC browser testing confirmed that parcel and conveyor/floor images now appear correctly.
- 2026-05-07: `AIT > Dev Server > Start Server` now works from the Unity menu after adding the AIT embedded pnpm folder to Windows `PATH` and fully restarting Unity Hub/Editor.
- 2026-05-08: Phone browser can reach the PC AIT Dev Server on the LAN (`172.30.1.14:5173`).
- 2026-05-08: Lower-box sliding felt too strong on phone WebGL, so parcel friction was raised to `2.4` and Rigidbody2D damping was raised to `linearDamping = 0.6`, `angularDamping = 0.8` for the next build.
- 2026-05-08: A small `B001` build marker was added below the top-right HUD area so phone tests can confirm a fresh WebGL build is loaded.
- 2026-05-08: Changes were split into focused commits through `94a6549 AIT 웹GL 빌드 설정 정리`; the worktree was clean immediately after the commit checkpoint.
- 2026-05-08: WebGL Korean text fallback was fixed by adding `NotoSansKR-VF.ttf` under `Assets/Resources/Prototype/Fonts/`, applying it to the prototype `OnGUI` styles, and bumping the build marker to `B002`.
- 2026-05-08: Phone WebGL testing still showed the bottom box sliding too easily, so floor contact now uses a separate `PhysicsMaterial2D` with `friction = 6.0` while parcel contact stays at `friction = 2.4`; the build marker is now `B003`.
- 2026-05-08: Failure rescue was narrowed to one free restore per stage for the next phone test; the mock reward-ad restore is disabled and the build marker is now `B004`.
- 2026-05-08: The first settled box now anchors its X position and rotation after landing so stack pressure cannot slide the foundation sideways; the build marker is now `B005`.
- 2026-05-08: Settled-box anchoring now freezes X position for every placed box after landing, while only the first foundation box also freezes rotation; the build marker is now `B006`.
- 2026-05-08: Settled-box anchoring made towers too stable, so the anchor logic was removed and parcel/floor friction were both raised to `8.0` for the `B007` test.
- 2026-05-08: Phone testing still showed impact-driven lower-box sliding, so falling boxes now temporarily use `gravityScale = 1.1` with a `4.5` fall-speed cap before returning to settled `gravityScale = 1.6`; the build marker is now `B008`.
- 2026-05-08: Phone testing judged the `B008` softened-drop tuning "just right", and the change was committed as `ceba02c 낙하 충격 완화로 박스 밀림 조정`.
- 2026-05-09: Pre-drop horizontal movement changed from eased sine motion to constant-speed endpoint reversal; the build marker is now `B009`.
- 2026-05-09: Pre-drop horizontal movement now starts at screen center and moves right first while keeping the B009 constant-speed endpoint reversal; the build marker is now `B010`.
- 2026-05-09: Pre-drop movement range is now clamped to the visible camera width so higher-stage range multipliers cannot push boxes outside the screen; the build marker is now `B011`.
- 2026-05-09: B011 phone testing confirmed boxes now stay inside the screen, but stage 20 movement felt slower because the screen clamp also shortened the effective movement speed.
- 2026-05-09: Screen-clamped movement now compensates speed against the unclamped stage range, so later stages keep their intended speed feel while staying visible; the build marker is now `B012`.
- 2026-05-09: The representative-stage playtest checklist was updated at `production/qa/playtests/playtest-2026-05-09-b012-screen-clamped-speed-stages.md` for stages 1, 5, 10, 15, and 20.
- 2026-05-10: B012 WebGL was rebuilt through `AIT/Dev Server/Start Server`; `webgl/Build` and `ait-build/public/Build` were refreshed at 03:51, HTTP `200 OK` was confirmed on `localhost:5173`, and the phone test URL is `http://172.30.1.14:5173/index.html`.
- 2026-05-10: Undo direction changed from fail-popup rescue to a one-use HUD undo button available during active play/drop/clear validation; the fail popup no longer offers continuation rescue, and the build marker is now `B013`.
- 2026-05-10: B013 phone testing confirmed the separate HUD undo button is the better recovery direction for later stages, and the change was committed as `12692f8 되돌리기 버튼 방식으로 변경`.
- 2026-05-10: Productization step 1 started: stage/tuning defaults moved behind `BoxStackPrototypeConfig`, with `BoxStackPrototype` loading a future `Resources/Prototype/BoxStackPrototypeConfig` asset when present and falling back to the approved B013 values otherwise. The build marker is now `B014`.
- 2026-05-10: `Assets/Resources/Prototype/BoxStackPrototypeConfig.asset` was created with the approved B013/B014 stage and tuning values. Editor Play confirmed `Resources.Load` finds the asset, runtime config loading is active, stage count remains 20, and Unity console errors/warnings are 0.
- 2026-05-10: B014 WebGL was rebuilt through AIT, copied into `ait-build/public`, served from `http://localhost:5173/index.html`, and smoke-tested in PC Chrome mobile viewport. The canvas loaded, the HUD showed Korean text plus `B014`, parcel/conveyor art rendered, and tap-to-drop advanced the counter to `1 / 4`. Phone LAN URL for manual spot testing is `http://172.30.1.14:5173/index.html`.
- 2026-05-10: B015 UI replacement pass moved the Toss Minimal HUD, HUD undo button, stage select overlay, result popup, safe-area placement, runtime theme, and Korean font assignment into `BoxStackPrototypeUi` using UI Toolkit. The unused legacy IMGUI drawing path was removed from `BoxStackPrototype.cs`. `dotnet build` passed with 0 warnings/errors; Editor Play confirmed `UIDocument` creation, B015 HUD, stage select sizing, result popup sizing, and overlay input blocking. Unity still emits one known `PanelSettings` theme warning from runtime panel creation, but the theme resource loads and the UI renders correctly.
- 2026-05-10: Development validation policy changed: use Unity Editor Play Mode for normal UI/gameplay iteration, and reserve AIT/WebGL plus phone browser builds for milestone browser/device spot checks because WebGL build time is too expensive for every development loop.
- 2026-05-13: UI direction research found the current Toss Minimal HUD reads too much like a normal app header. A/B/C mockup directions were generated and archived at `design/ui/mockups/boxstack-ui-directions-2026-05-13.png`; Direction A, Delivery Arcade, was selected as the next HUD direction and specified in `design/ui/boxstack-delivery-arcade-hud-spec-2026-05-13.md`.
- 2026-05-13: Delivery Arcade HUD implementation plan was approved for the next pass: replace the full-width Toss Minimal top bar with compact stage/undo badges, add a right-side delivery progress rail, add simple Korean central feedback, and restyle the result popup while preserving gameplay, stage select, undo, safe-area, and input-blocking behavior.
- 2026-05-13: B016 Delivery Arcade HUD first pass implemented: compact shipping-label stage badge, top-right undo ticket, right-side delivery progress rail, central Korean feedback toast, and delivery-styled result card. `dotnet build BoxStack.slnx` passed with 0 warnings/errors. Unity CLI Connector on port `8090` confirmed Editor Play starts, one `UIDocument` exists, and the UI tree contains `B016`, the delivery-label stage badge, placed/target count, undo ticket, and stage select text. Connector screenshots did not capture UI Toolkit reliably, but the user accepted the overall Editor Game view layout.
- 2026-05-13: B016 overall UI layout was accepted and committed as `7952526 B016 Delivery Arcade HUD 구조 적용`.
- 2026-05-13: B017 HUD undo cleanup removed old fail-popup rescue and mock reward-ad runtime paths from `BoxStackPrototype`, while preserving the separate one-use HUD undo behavior. `dotnet build BoxStack.slnx` passed with 0 warnings/errors. Unity Editor Play Mode confirmed one `UIDocument`, build marker `B017`, undo snapshot restoration once per stage, and failure result text without ad/reward copy; only the known `PanelSettings` and UDP warnings remain. User accepted and committed the cleanup as `af6cc91 B017 되돌리기 코드 정리`.
- 2026-05-13: B018 undo config naming cleanup verified: `BoxStackPrototypeConfig` now uses `UndosPerStage` instead of the old rescue/ad setting names, the checked-in config asset stores `UndosPerStage: 1`, and `BoxStackPrototype` reads the renamed field without changing gameplay tuning. `dotnet build BoxStack.slnx` passed with 0 warnings/errors. Unity CLI Connector Editor Play verification confirmed build marker `B018`, config load, `configUndosPerStage = 1`, `runtimeUndosRemaining = 1`, and no remaining `FreeRescuesPerStage` or `AdRescuesPerStage` fields in `TuningSettings`. User committed it as `9d6ea93 B018 되돌리기 설정명 정리`.
- 2026-05-13: B019 UI Toolkit PanelSettings cleanup verified: `Assets/Resources/Prototype/Ui/BoxStackPanelSettings.asset` is now checked in with `BoxStackRuntimeTheme`, and `BoxStackPrototypeUi` loads that PanelSettings asset instead of creating one at runtime. Editor-only `AssetDatabase` fallback is used only to make Editor Play checks robust before Resources refresh timing catches up. `dotnet build BoxStack.slnx` passed with 0 warnings/errors. Unity CLI Connector Editor Play verification confirmed build marker `B019`, one `UIDocument`, resource PanelSettings loaded and used, theme assigned, and the previous `No Theme Style Sheet set to PanelSettings` warning removed. The unrelated Visual Studio UDP warning remains.
- 2026-05-13: B019 was accepted and committed as `917f2b4 B019 UI PanelSettings 경고 정리`; the worktree was clean immediately after the checkpoint.
- 2026-05-13: B019 representative Editor Play structure check completed through Unity CLI Connector on port `8090`. Stages 1, 5, 10, 15, and 20 load, spawn from center x `0.0`, show build marker `B019`, and keep the clamped move range inside the current camera view. Stage speed intent is still preserved by range compensation, with stage 20 using a larger compensation ratio over the same visible range. Console showed only the known unrelated Visual Studio UDP warning and AIT/connector logs.
- 2026-05-13: B020 stage select hit-test fix implemented after the "전체 해금" control and later-stage buttons appeared unresponsive. Stage grid/progress control containers now participate in picking, and hidden overlays now use `display: none` so the inactive result popup cannot intercept stage-select clicks. `dotnet build BoxStack.slnx` passed with 0 warnings/errors. Unity CLI Connector Editor Play verification confirmed build marker `B020`, unlock-all changes highest unlocked stage to 20, stages 10/15/20 become enabled, their centers are picked as `Button`, and selecting them changes the current stage to 10/15/20.
- 2026-05-13: B020 stage select hit-test fix was manually confirmed and committed as `b2d99b8 B020 스테이지 선택 해금 버튼 수정`; the worktree was clean immediately after the checkpoint.
- 2026-05-13: B020 representative Editor Play structure check completed through Unity CLI Connector on port `8090`. Stages 1, 5, 10, 15, and 20 load after unlock-all, spawn from center x `0.0`, show build marker `B020`, keep one `UIDocument`, expose the Delivery Arcade HUD/stage-select controls, preserve one undo, and keep the clamped move range inside the current camera view. Stage 20 still uses the intended higher speed (`2.59`) while the visible move range remains about `2.042`. Console showed only the known unrelated Visual Studio UDP warning and AIT/connector logs.
- 2026-05-13: B020 manual playtest checklist added at `production/qa/playtests/playtest-2026-05-13-b020-representative-stages.md` for Editor Play checks on stages 1, 5, 10, 15, and 20.
- 2026-05-13: B020 representative playtest was accepted as good enough to proceed without immediate HUD, movement, physics, undo, or difficulty tuning changes.
- 2026-05-13: B021 stage progress storage boundary implemented: `BoxStackStageProgressStore` now wraps highest-unlocked-stage PlayerPrefs access so later App-in-Toss storage replacement has a clear seam. `dotnet build BoxStack.slnx` passed with 0 warnings/errors after Unity refreshed the new file. Unity CLI Connector Editor Play verification confirmed reset saves stage 1, unlock-all saves stage 20, stage 15 selection still works, one `UIDocument` exists, and `B021` appears.
- 2026-05-13: B022 prototype asset loading boundary implemented: `BoxStackPrototypeAssetLoader` now owns prototype config, Korean font, parcel/background sprite loading, Editor-only PNG fallback, runtime sprite creation, placeholder parcel creation, solid sprite creation, and visible-alpha rect calculation. `BoxStackPrototype` delegates those loading details to the loader while preserving gameplay, UI layout, physics, stage tuning, undo, and persistence behavior. `dotnet build BoxStack.slnx` passed with 0 warnings/errors after Unity refreshed the new file. Unity CLI Connector Editor Play verification confirmed loader type `BoxStackPrototypeAssetLoader`, config loaded, Korean font `NotoSansKR-VF` loaded, floor sprite `parcel_stack_base_01` loaded, 3 box visuals loaded, one `UIDocument` exists, and UI labels include `B022`.

## Current Decisions

- Project mode is casual game, prototype-first.
- Keep planning lightweight: active state plus dashboard, no heavy GDD/ADR/review workflow by default.
- Record playtest outcomes briefly in this dashboard and `production/session-state/active.md`.
- Target launch context is Toss app-in-app, so prototype direction favors lightweight 2D/2.5D sprites over modeled 3D assets.
- Current prototype uses timing-based screen-clamped centered constant-speed drops, runtime placeholder parcel sprites, 2D physics stacking, rotation-locked orthographic camera follow, stage-specific target counts, and quick restart.
- Next asset direction is AI-generated 2D PNGs with clear silhouettes, visible contact edges, transparent backgrounds, and mobile-readable parcel details.
- Expected prototype sprite names are `parcel_box_basic_01.png`, `parcel_box_wide_01.png`, `parcel_box_tall_01.png`, and `parcel_stack_base_01.png`.
- Prototype sprite loading also falls back from Sprite assets to Texture2D-to-Sprite creation, so Play mode remains tolerant of importer refresh timing.
- Parcel PNG visuals and `BoxCollider2D` sizes are based on visible alpha bounds rather than the full transparent source image rectangle.
- Unity CLI Connector is installed and verified; use `docs/workflow/unity-cli-connector.md` for live Editor inspection, asset refresh, Play/Stop, console reads, screenshots, and injected C# checks.
- Background direction is a bright 2D parcel logistics center with a calm central play lane, not a full 3D modeled warehouse.
- Current background test uses a parcel-brown solid color because the logistics center image was distracting during play.
- Camera bottom clamp is tied to the prototype floor constants instead of a fixed `CameraBaseY` number.
- Current implemented UI is B016 Delivery Arcade first pass: compact shipping-label stage badge, undo ticket, side delivery progress rail, central landing feedback, and delivery-styled result cards while preserving the clean Toss app-in-app tone.
- Main progression direction is fixed-count clear stages, not infinite stacking, for the first Toss app-in-app MVP.
- Clear requires every settled box to stay within the single-column x tolerance from the first placed box; crossing the tolerance fails immediately, and the completed stage stack must survive 5 seconds before success.
- Pre-drop horizontal movement should start at screen center, move right first, keep the same speed through side-edge direction changes, clamp to the visible camera width, and preserve stage speed even when the range clamp shortens the path for the B012 timing-feel test.
- Use the approved `B008` high-friction plus softened falling-box impact baseline rather than X-axis constraints to reduce sideways slide, because constraints made towers too stable.
- Stage implementation uses `Assets/Resources/Prototype/BoxStackPrototypeConfig.asset`, a HUD-opened stage select overlay, and a result-popup next-stage flow; playtest stages 1, 5, 10, 15, and 20 before tuning the full run.
- `BoxStackPrototypeConfig` is now the first productization boundary for stage and tuning data; the checked-in config asset carries the approved B013/B014 values, while code fallback remains as a safety net.
- Later-stage difficulty should use one timely HUD undo before considering any fail-popup or reward-ad recovery prompt.
- Actual ad SDK integration is deferred; the current prototype no longer exposes the mock reward-ad rescue during normal playtesting.
- The fail popup should stay focused on retry/next actions; timely correction now belongs to the HUD undo button.
- B020 fixes the stage select unlock/test button hit-test issue and is now committed; automated structure checks pass and the user accepted the current feel. B021 preserves that baseline while moving stage progress storage behind a small replacement boundary; B022 preserves it again while moving prototype asset loading behind a small replacement boundary.
- The next UI pass should be Delivery Arcade visual/structural work only; do not bundle gameplay tuning, scoring rules, stars, combo systems, or reward-ad rescue changes into that pass.
- Delivery Arcade first pass implementation scope is fixed: `BoxStackPrototypeUi.cs` owns the visual/structural HUD changes, while `BoxStackPrototype.cs` should only change for build marker `B016` and any minimal UI state needed for simple landing feedback.
- B016 Delivery Arcade code pass, B017 HUD undo cleanup, B018 undo config naming cleanup, and B019 UI Toolkit PanelSettings cleanup were accepted and committed.
- Stage unlock state uses PlayerPrefs for prototype speed; replace or wrap it later if App-in-Toss storage policy requires a different persistence layer.
- Stage select may expose prototype progress test controls while validating difficulty and unlock persistence, but only in Editor/development builds.
- Prototype UI Toolkit layout should respect mobile safe area for HUD, stage select, and result popup placement before App-in-Toss package testing.
- WebGL visual parity with Editor Play should use build-included prototype sprites; the current prototype uses duplicate PNGs under `Assets/Resources/Prototype/...` for speed.
- The Unity AIT Dev Server menu depends on the embedded AIT pnpm folder (`C:\Users\Ahneunsung\AppData\Local\.ait-unity-sdk\nodejs\v24.13.0\win-x64`) being available on Windows `PATH`.
- Use a tiny in-game build marker (`B016`, then increment manually when code changes again) during milestone mobile WebGL tests to distinguish a fresh build from a cached old build.

## Open Questions

- Which parcel-box visual style best fits Toss app-in-app: clean fintech-casual, cute toy-like, or lightly realistic delivery packaging?
- How many box variations are needed before the stack stops feeling repetitive?
- Should the next playtest measure visual clarity, replay desire, or perceived brand fit?
- Should the background stay screen-fixed for prototype readability, or eventually scroll/parallax with stack height?
- What bottom padding feels best on the target portrait app-in-app viewport?
- Does the safe-area-aware UI Toolkit layout keep HUD and popups clear of Toss webview/status-bar insets on real devices?
- Is the duplicate `Assets/Resources/Prototype/...` copy acceptable through the prototype phase, or should it be replaced later with serialized/build-included asset references?
- At the next milestone WebGL checkpoint, does the phone browser load the latest AIT/WebGL build reliably through the PC LAN IP?
- At the next milestone WebGL checkpoint, does the WebGL build render Korean text correctly across HUD, stage select, result popup, and undo button states?
- For the Delivery Arcade pass, should the delivery progress rail live on the left or right side of the target phone viewport?
- Should the next HUD playtest use Korean-only UI copy, English arcade feedback, or a mixed prototype copy set?
- Should the first landing feedback use generic messages only, or wait for an actual placement-quality scoring signal?
- Is the HUD stage-label click plus stage select overlay enough for prototype stage testing?
- Does the parcel-brown solid background improve focus compared with the logistics center image?
- Does screen-clamped centered constant-speed movement keep all box shapes visible while preserving harder-stage speed feel?
- Are the current single-column tolerance (`0.75`) and 5-second clear validation window fair enough across stages 1, 5, 10, 15, and 20?
- Does immediate collapse detection feel fair, or does it punish harmless physics wobble too quickly?
- Does the approved B008 falling-box impact tuning still feel right across stages 1, 5, 10, 15, and 20?
- Is PlayerPrefs enough for prototype progression testing before App-in-Toss storage requirements are confirmed?
- Are the Editor/development-only `진행 초기화` and `전체 해금` controls enough for fast stage testing, or should they move behind a less visible debug gesture later?

## Risks

- Dashboard staleness if `production/session-state/active.md` changes without this file being updated.
- AI-generated assets may look inconsistent unless the first prompt set tightly controls angle, outline, color, and export rules.
- Visual polish may hide physics readability issues if box silhouettes and contact edges are not kept clear.
- Background detail may reduce falling-box readability if the center play lane feels too busy on mobile.
- Camera floor clamp can make the starting view feel too high if the target portrait aspect ratio changes significantly.
- B017 removed the old fail-popup rescue and mock reward-ad runtime paths from `BoxStackPrototype.cs`. B018 migrates the config asset to the current `UndosPerStage` name while keeping `FormerlySerializedAs` for older serialized assets.
- The previous runtime-created UI Toolkit `PanelSettings` theme warning is fixed by loading the checked-in `BoxStackPanelSettings` asset. One unrelated Visual Studio UDP warning still appears in this local Editor setup.
- The prototype now includes a full Korean font in `Resources`, which is acceptable for fast WebGL validation but adds build weight until UI/font handling is replaced or subsetted.
- Safe area is handled in the prototype UI Toolkit layer, but real App-in-Toss WebGL/device testing is still needed because Editor Game view may not emulate every inset.
- AIT Dev Server now launches from the Unity menu after the PATH fix, but this depends on the AIT embedded pnpm folder remaining available on Windows `PATH`.
- WebGL sprite parity is confirmed in PC browser after the AIT/WebGL rebuild, but phone browser testing is still needed.
- Later 12-box stages may feel random if physics instability dominates player timing skill.
- Screen-clamped movement should keep boxes visible on narrow mobile viewports, but B012 phone testing still needs to confirm the compensated later-stage speed feels fair instead of frantic.
- Softened falling-box impact felt good enough in B008 phone testing, but B012 representative stage testing should confirm it still works after the movement feel change.
- HUD undo is accepted as the current recovery direction, but the button may still need repositioning later if it competes with drop input or crowds the top HUD on small screens.
- Delivery Arcade should fix the app-like UI read, but it could become too busy or ad-game-like if feedback badges, side rail, and result stamps are all pushed too strongly in the first pass.
- The new config asset is confirmed in Editor Play and PC WebGL smoke, but phone browser testing is now a milestone spot check for real device safe-area, touch feel, and performance rather than the default development loop.
- Reward-style rescue is currently disabled, but reintroducing it too early could make failure feel monetized before the stage difficulty is accepted as fair.
- PlayerPrefs is fine for local prototype persistence, but App-in-Toss production storage requirements may require replacing it later.
- Prototype progression controls are useful for playtest speed, and are now hidden from non-development user builds; confirm this again before any App-in-Toss package test.
- Unity batchmode import could not run while the project was already open, so in-editor refresh/play verification is still needed.
- Unity CLI Connector requires the Unity Editor to be open with this project loaded; if port `8090` does not respond, scan `8091` through `8099`.
