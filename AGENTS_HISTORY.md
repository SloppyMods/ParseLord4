---
tags:
  - type/log
  - project/parselord4
  - status/active
type: log
project: parselord4
status: active
aliases: []
---
# AGENTS_HISTORY

## 2026-05-15 - GNB job activation (rotation + combos)

- Created `GunbreakerRotation.cs` — full auto-rotation for GNB (JobId 37) with:
  - Gnashing Fang combo priority (Savage Claw → Wicked Talon)
  - Reign of Beasts combo (Noble Blood → Lion Heart) from Bloodfest
  - Sonic Break / Double Down during No Mercy burst
  - Ammo dump via Burst Strike / Fated Circle before overcap
  - All Continuation oGCDs (Jugular Rip, Abdomen Tear, Eye Gouge, Hypervelocity, Fated Brand)
  - Blasting Zone, Bow Shock, Trajectory weaving
  - AoE/ST branching with level-sync fallbacks
- Created `GNBCombo.cs` — main ST combo: Keen Edge → Brutal Shell → Solid Barrel
- Created `GNBWcButtonCombos.cs` — 6 WC button combines:
  - AoE Basic Combo (Demon Slice → Demon Slaughter)
  - Continuation (status-driven follow-ups)
  - Gnashing Fang Combo (adjusted action chain)
  - Burst Strike / Fated Circle (Hypervelocity / Fated Brand follow-ups)
  - Reign of Beasts (Noble Blood → Lion Heart chain)
  - No Mercy / Sonic Break / Double Down burst button
- Updated `JobGaugeReader.cs` — added `Ammo` and `AmmoComboStep` accessors for `GNBGauge`.
- Updated `RotationManager.cs` — registered `GunbreakerRotation`.
- Updated `_config/action-ids.md` — added GNB action and status ID tables.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and redeploys the DLL to `%APPDATA%\XIVLauncher\devPlugins\ParseLord4\`.

## 2026-05-14 - Adloquium stack same-action target fix

- Investigated the imported/current `Adloquium` stack in PL4 config: trigger `185`, same-action
  stack items, target-pronoun routing, missing `Galvanize` (`297`) checks, and `BlockOriginal`.
- Root cause: `UseActionHook` treated any resolved stack item with the same action ID as the pressed
  action as a failed stack when `BlockOriginal` was enabled, even when the stack had selected a new
  target. This blocked same-action target overrides like Adloquium.
- Changed the hook to block only the explicit no-target failure result; same-action target swaps now
  call the original `UseAction` with the resolved target.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and redeploys the DLL to `%APPDATA%\XIVLauncher\devPlugins\ParseLord4\`.

## 2026-05-14 - Stack editor width fix

- Fixed Stacks tab editor clipping where the main window could not expand past the status field.
- Raised the PL4 window maximum size from `980x820` to `1500x980`.
- Wrapped stack item HP/status/missing/delete controls onto a second line so imported ActionStacksEX
  stacks remain editable without needing extreme width.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and redeploys the DLL to `%APPDATA%\XIVLauncher\devPlugins\ParseLord4\`.

## 2026-05-14 - ActionStacksEX import compatibility

- Added ActionStacksEX import support for clipboard `ASEX_` exports and the installed
  `%APPDATA%\XIVLauncher\pluginConfigs\ActionStacksEX.json` config file.
- Converted ActionStacksEX `Actions[]` trigger lists, adjusted-trigger flags, modifier exact-match
  bit, item enabled state, HP/status filters, and target pronoun IDs into PL4 stack fields.
- Aligned PL4 pronoun IDs with ActionStacksEX's core IDs while keeping legacy current-target ID `0`
  as a supported alias.
- Added Stack tab `Import ASEX Config` button. Current local ActionStacksEX config contains 21
  stacks and uses target IDs covered by the mapper.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and redeploys the DLL to `%APPDATA%\XIVLauncher\devPlugins\ParseLord4\`.

## 2026-05-14 - Stack tab blank-add fix

- Checked live PL4 config and found `ActionStacks` was empty, so no stack could resolve or fire.
- Root cause for the "Add does nothing" feel: the Stack tab only created a stack when the name field
  was non-empty.
- Changed Add to always create a stack, using `New Stack N` when the name field is blank.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and redeploys the DLL to `%APPDATA%\XIVLauncher\devPlugins\ParseLord4\`.

## 2026-05-14 - ActionStacksEX stack feature integration

- Continued Layer 2 stack work using `C:\Users\you\Documents\Projects\ActionStacksEX` as the
  reference for stack behavior and editor UX.
- Added adjusted-trigger matching, exact/non-exact modifier matching, item enable toggles,
  same-action stack items via action ID `0`, HP threshold filters, status/missing-status filters,
  richer pronoun targets, and PL4 stack import/export through clipboard.
- Updated stack resolution so stack items can use adjusted action IDs and ActionStacksEX-style target
  filters before selecting the first usable item.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and redeploys the DLL to `%APPDATA%\XIVLauncher\devPlugins\ParseLord4\`.

## 2026-05-13 - Full BLM WC button-combine pass

- Filled out the BLM `WC Button Combines` list from WrathCombo's standalone miscellaneous BLM
  presets, still excluding simple/advanced mode rotation scaffolding.
- Added Transpose/Umbral Soul, Triplecast protection, Fire/Blizzard one-button variants,
  Flare/Freeze, Blizzard IV/Despair, Flare Star/Paradox, Amplifier/Xeno, Xeno/Foul Thunder swaps,
  Scathe/Xeno, Ley Lines, and Aetherial Manipulation/Between the Lines.
- Retargeted Aetherial Manipulation was left out because the current PL4 combo layer only returns
  replacement action IDs and does not own target rewriting.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and redeploys the DLL to `%APPDATA%\XIVLauncher\devPlugins\ParseLord4\`.

## 2026-05-13 - WC-style button combine options

- Used `C:\Users\you\Downloads\WrathCombo-main\WrathCombo-main` as the reference for standalone
  button-combine presets and intentionally left out WC simple/advanced mode scaffolding.
- Added combo metadata for UI grouping, descriptions, default enablement, and deterministic ordering.
- Added a separate `WC Button Combines` section in the Combo tab; these options are disabled by
  default so the current core Assist/Full Auto behavior is unchanged until a preset is enabled.
- Added first-pass WC-style combines for BLM, BRD, DRG, PLD, and WAR, including ST/AoE combo buttons,
  proc/follow-up buttons, burst buttons, and selected movement/utility replacements.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and redeploys the DLL to `%APPDATA%\XIVLauncher\devPlugins\ParseLord4\`.

## 2026-05-12 - BLM Manafont early-use fix

- Investigated the live report that BLM was wasting `Manafont` at combat start.
- Checked The Balance BLM basic guide: outside the opener, `Manafont` should primarily be weaved at
  the tail end of an Astral Fire phase to get full value from the second fire phase.
- Root cause: the oGCD weave branch used only `inFire && AstralFireStacks == 3`, so entering AF3
  after `Fire III` made `Manafont` valid even before MP was spent.
- Changed `Manafont` to require confirmed tail-end fire progress: after `Despair` for ST, after
  `FlareStar` for level 100 AoE, or after the two-`Flare` segment for pre-100 AoE. A successful
  `Manafont` now resets fire-spend tracking so the extension can proceed as a fresh fire segment.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder.

## 2026-05-12 - BLM level 90 ST Paradox rejection fix

- Investigated the live level 90 single-target report that BLM was getting stuck.
- Runtime trace showed repeated `Rejected action 25797 ... status=568`; `25797` is `Paradox`,
  and the rotation kept selecting it because the gauge still reported Paradox active.
- Checked The Balance BLM leveling guide: the level 90-99 ST loop includes Paradox, but the initial
  Umbral Ice Paradox can be unavailable depending on how the phase was entered.
- Root cause: ST fire/ice and movement routing used targetless action availability for `Paradox`
  and Thunder, so target-specific rejection did not let the rotation fall through to Fire III,
  Xenoglossy, Fire IV, or the next legal action.
- Changed ST Paradox and Thunder selections, plus the movement instant gate, to require
  target-specific action status `0` before returning those GCDs.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder.

## 2026-05-12 - BLM level 90 AoE second Flare wait fix

- Investigated the live level 90 AoE report that BLM was looping `Flare > Freeze > Flare >
  Freeze`, with occasional Thunder filler, instead of holding fire long enough for `Flare x2`.
- Checked The Balance BLM leveling guide: level 58-99 AoE uses the `Transpose > Flare x2 >
  Transpose` fire segment.
- Root cause: after one successful `Flare`, the previous recovery change allowed the modern AoE fire
  branch to fall through during the short post-cast window where `Flare` was not yet ready but
  `Transpose` was available, so it swapped to ice before the second `Flare`.
- Added a bounded second-`Flare` wait after the first observed `Flare`; the branch now waits through
  the next GCD opportunity for `Flare`, but still falls through to recovery if the second cast stays
  impossible.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder.

## 2026-05-12 - BLM AoE ice lockup recovery fix

- Investigated the live report that BLM could lock up during the ice phase after the modern AoE
  second-`Flare` guard.
- Checked the recent runtime trace and The Balance BLM AoE guidance: `Freeze` is the preferred MP
  setup for the Flare loop, while `Foul`/Thunder can still be used as filler and movement tools
  around `Transpose`.
- Root cause: the modern AoE ice branch returned no action whenever it still wanted hearts but
  could not hard-cast immediately, which blocked instant fallback actions and `Transpose`.
- Changed the hearts gate to attempt `Freeze`/the ice AoE swap only when hard-casting is currently
  possible, then continue to Polyglot/Transpose/recovery options instead of dead-ending.
- Also relaxed the modern second-`Flare` guard so it still waits for the first `Flare`, but no
  longer dead-ends forever after one successful `Flare` if the second cast is temporarily illegal.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder.

## 2026-05-12 - BLM level 80 AoE second Flare guard

- Investigated the live level 80 AoE report that BLM appeared to skip the second `Flare` and swap
  back to ice too quickly.
- Checked The Balance BLM leveling guide: levels 58-99 AoE should route through `Freeze >
  Foul/Thunder II/Freeze > Transpose > Flare x2 > Transpose`.
- Root cause: the modern AoE fire branch cast `Flare` when currently ready, but immediately allowed
  `Transpose` if `Flare` was briefly not ready after the first successful cast, so the second `Flare`
  could be skipped.
- Added a successful-action counter for modern AoE fire phases and block Transpose until two
  successful `Flare` casts have been observed.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder.

## 2026-05-12 - BLM level 80 AF1 fire-entry correction

- Investigated the live level 80 ST report that BLM could start a fire round at one Astral Fire
  stack and continue spending instead of correcting to three stacks.
- Checked The Balance BLM leveling guide: level 72-89 ST still routes `Blizzard III > Blizzard IV >
  Fire III > Fire IV x7 > Despair`, and level 80 replaces ST `Foul` with `Xenoglossy`.
- Root cause: once the gauge reported `inFire`, the ST fire branch went straight into
  Paradox/Thunder/Polyglot/Despair/Fire IV logic and had no guard for `AstralFireStacks < 3`.
- Added a ST fire-entry guard for Fire IV levels: if BLM is in Astral Fire with fewer than three AF
  stacks, cast target-ready `Fire III` before fire spenders.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder.

## 2026-05-12 - BLM ST ice Blizzard I regression fix

- Investigated the live ST report that BLM was casting `Blizzard III`/ice entry and then falling
  into `Blizzard I`, causing slow MP recovery.
- Runtime trace showed the high-level ST loop selecting `Blizzard` (`142`) after `Despair`, later
  mixing `Blizzard IV` (`3576`) with more `Blizzard` filler.
- Checked The Balance BLM leveling guide: 60-71 and 72-89 ST loops are `Blizzard III > Blizzard IV
  > Fire III ...`, with 58+ replacing the old `Blizzard` filler.
- Root cause: the fire-to-ice/recovery helper preferred adjusted `Blizzard` before `Blizzard III`,
  and the ice phase treated transient `Blizzard IV` action availability as if the player had no
  `Blizzard IV` follow-up path.
- Changed ST ice entry/recovery to prefer `Blizzard III` over `Blizzard`, and changed learned
  `Blizzard IV` to keep controlling the fixed ST ice follow-up regardless of transient availability.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder.

## 2026-05-12 - BLM exact ST/AoE profile switching fix

- Investigated the live report that BLM was not switching from ST into AoE, then could stick in AoE
  after combat conditions changed.
- Root cause: BLM had diverged from the shared `DataCenter.HostilesInSplashRange` AoE count used by
  the other working jobs, and its private target profile used hysteresis that intentionally stayed in
  AoE below the configured entry threshold.
- Changed BLM back to the shared filtered hostile count and made the ST/AoE profile use the exact
  configured `AoETargetCount` threshold in both directions every tick, while preserving mid-combat
  updates during fire phase.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder.

## 2026-05-12 - BLM level 72 post-Foul Despair lockup fix

- Investigated the live "Full Auto bricked" and "only using Foul" reports after adding the level
  70+ Polyglot spender.
- Runtime trace showed `Foul` (`7422`) casting successfully, then Full Auto repeatedly selecting
  `Despair` (`16505`) and getting rejected with status `568`; the same window also showed
  `Blizzard III` (`154`) rejected with status `582`.
- Root cause: `Despair` used learned/unlocked checks instead of target-aware current usability, and
  the fire-to-ice fallback could return hard-coded `Blizzard III` even when the adjusted/current
  action was unavailable.
- Changed `Despair` to require target-aware status `0`, and changed fire-to-ice/no-element/ice
  recovery to pick the first target-ready adjusted Blizzard action.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder. Deployed DLL timestamp:
  `2026-05-12 19:45:44`.

## 2026-05-12 - BLM level 72 Foul Polyglot spender fix

- Investigated the confirmed live report that level 72 BLM was not spending `Foul` in any
  scenario.
- Checked The Balance BLM leveling guide: at 70-79 `Foul` should be spent without overcapping,
  and AoE 58-99 uses `Foul` as an Umbral Ice filler/spender.
- Root cause: PL4 only modeled `Xenoglossy` as the Polyglot spender, but `Xenoglossy` is not
  learned until level 80, leaving levels 70-79 with no Polyglot action path.
- Added `Foul` (`7422`) as the level 70+ Polyglot action, kept `Xenoglossy` preferred for
  level 80+ single-target, kept `Foul` preferred for AoE, and only treats `Foul` as a movement
  instant at level 80+.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder. Deployed DLL timestamp:
  `2026-05-12 19:42:19`.

## 2026-05-12 - BLM movement instant and level 50 AoE Blizzard II loop fix

- Investigated live reports that BLM kept trying hard casts while moving and later that AoE fell
  into `Blizzard II` spam.
- Root cause: movement handling only protected a narrow AoE hard-cast path, while normal ST and
  AoE routing could still return hard casts; the level 50-57 AoE Flare loop also depended on
  current `Flare` availability, which can be false while in ice and let generic AoE filler choose
  `Blizzard II`.
- Added movement GCD selection before hard-cast branches: Firestarter `Fire III`, Thunderhead,
  `Xenoglossy`, `Paradox`, then `Scathe` fallback.
- Changed the level 50-57 AoE loop to route by player level/learned `Flare` rather than current
  phase availability, preventing `Blizzard II` filler during the Freeze loop.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder. Deployed DLL timestamp:
  `2026-05-12 19:07:40`.

## 2026-05-12 - BLM level 50 ST Firestarter Fire III fix

- Investigated the live report that single-target BLM did not cast `Fire III` with a Firestarter
  proc.
- Checked The Balance BLM leveling guide: levels 35-59 use `Fire III`/`Blizzard III` for element
  swapping, and Firestarter procs should be used as they appear without delaying casts.
- Runtime trace showed ST falling into the generic low-level Transpose loop (`Blizzard`/`Transpose`/
  `Thunder III`/`Fire`) instead of selecting `Fire III` (`152`).
- Root cause: phase-swap spell routing used `GetActionStatus`-style availability as an unlock
  test, which can misclassify `Fire III`/`Blizzard III` during elemental phases, and the fire phase
  had no level 42-59 Firestarter spend path.
- Changed elemental-swap routing to use player-level learned checks for `Fire III`/`Blizzard III`,
  added an Astral Fire Firestarter spend for levels 42-59, and kept the high-level saved-proc ice
  exit behavior intact.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder. Deployed DLL timestamp:
  `2026-05-12 18:56:39`.

## 2026-05-12 - BLM level 50 AoE Blizzard II fallback fix

- Investigated the live report that level 50 AoE was casting `Freeze`, then `Blizzard II`.
- Runtime trace confirmed the sequence with `Freeze` (`159`) followed by repeated `Blizzard II`
  (`25793`) and one `Blizzard` (`142`) fallback.
- Root cause: the generic low-level fallback could still run before the AoE-specific Flare loop,
  and the AoE branch classified the low-level loop indirectly through Blizzard IV availability.
- Changed level 50-57 AoE to route through the dedicated AoE branch before generic filler can emit
  `Blizzard II`, and made the low-level Flare loop explicit from current player level plus `Flare`
  availability.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder. Deployed DLL timestamp:
  `2026-05-12 18:53:42`.

## 2026-05-12 - BLM level 50 no-action Fire IV rejection fix

- Investigated the live "doing nothing" report after the level 50 AoE availability fix.
- Runtime log evidence showed Full Auto selecting `Fire IV` (`3577`) and getting rejected with
  action status `573`, then repeating the same unavailable high-level spell.
- Root cause: BLM's action availability helper still treated status `573` as usable, and the
  target profile was not refreshed while in Astral Fire, so AoE tests could stay routed through
  stale single-target logic.
- Changed `IsUnlocked` to treat `572`, `573`, and `582` as unavailable and updated target-profile
  detection every tick from the selected actionable target instead of freezing it during fire.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder. Deployed DLL timestamp:
  `2026-05-12 18:48:54`.

## 2026-05-12 - BLM level-sync action availability fix

- Investigated the live "literally no change" report after the Flare/Freeze pause patch.
- Runtime log evidence showed Full Auto on BLM selecting `Blizzard IV` (`3576`) and getting
  rejected with action status `582`, then returning no action in combat.
- Root cause: BLM's `IsUnlocked` helper only treated status `572` as locked, so level-sync/current
  availability status `582` still counted as unlocked and let level 50 route into Blizzard IV.
- Changed the helper to treat both `572` and `582` as unavailable and added a successful-action
  trace in `FrameworkUpdateHook` so future live tests show the actual action sequence.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder.

## 2026-05-12 - BLM level 50 AoE Flare to Freeze pause fix

- Investigated the live report that level 50 AoE had a two-second pause after `Flare`, only cast
  one `Freeze`, and did not cast `Thunder II`.
- Root cause: the phase-swap helper waited for the normal weave threshold, so after long casts it
  could delay `Transpose` until the GCD was almost ready; the low-level AoE ice counter also needed
  protection against duplicate action observations.
- Changed phase-swap checks to allow `Transpose` as soon as the player is no longer casting or
  animation locked, skipped rotation execution while the player is actively casting, and debounced
  observed successful actions.
- Tightened Thunder DoT checks to only count the player's own target debuff and added the High
  Thunder II status id for current/high-level AoE tracking.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder.

## 2026-05-12 - BLM level 50 AoE Thunder slot and pause fix

- Investigated the live level 50 AoE report that the loop was moving but still had large pauses
  and did not spend Thunderhead on Thunder II.
- Root cause: Thunder II was either restricted to movement in fire or placed before the explicit
  low-level ice budget, which could skip the proc or turn `Freeze x2` into an extra ice GCD.
- Changed level 50 AoE to treat Thunder II as the second low-level ice GCD when the DoT needs a
  refresh, while still doing `Freeze -> Freeze` when Thunder is not needed.
- Also allowed AoE Thunder refresh in fire and made the Thunder decision trust action availability
  in addition to the Thunderhead buff snapshot.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder.

## 2026-05-12 - BLM level 50 AoE ice lockup fix

- Investigated the level 50 AoE report that the ice rotation locked up after the explicit `Freeze x2`
  change.
- Log evidence showed repeated Full Auto `No action` entries for BLM, meaning the rotation was
  returning null rather than having actions rejected by `ActionManager`.
- Root cause: the `Freeze`/`Fire II`/`Flare` progress flags were advanced when the rotation selected
  an action, not when `UseAction` succeeded, and the low-level AoE branches also checked hard-cast
  movement gates before the instant Transpose phase exit.
- Added a small `IRotationActionObserver` callback so confirmed `UseAction` success updates BLM
  phase progress, and moved low-level AoE Transpose checks outside the hard-cast gates.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder.

## 2026-05-12 - BLM level 50 AoE GCD flow fix

- Followed up on the level 50 AoE GCD continuity issue after the Thunderhead refresh fix.
- Root cause: the level 50-57 AoE ice branch still used an MP threshold plus a one-Freeze guard,
  which could route into Blizzard II or wait on Transpose/MP timing instead of following the
  deterministic leveling loop.
- Changed the level 50-57 AoE branch to track `Freeze x2` explicitly, then Transpose in Full Auto,
  and to track one `Fire II` then one `Flare` before the fire-to-ice Transpose.
- Removed the level 50 AoE Blizzard II fallback from that branch so it does not insert a
  non-guide GCD after Freeze.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder.

## 2026-05-12 - BLM level 50 Thunderhead refresh fix

- Investigated the level 50 AoE report that Thunderhead procs were not being used.
- Re-checked The Balance BLM leveling guide for the level 50-57 AoE loop: `Freeze x2 >
  Transpose > Fire II > Flare > Transpose`, with AoE Thunder kept up and refreshed under 3s.
- Root cause: PL4's Thunder refresh check only looked at the single-target Thunder DoT through
  `GameState.CurrentHostile`, while the level 50 AoE proc uses Thunder II and the rotation can pick
  an actionable target from `DataCenter` instead of the current target.
- Changed Thunder refresh tracking to inspect the selected target directly and include the Thunder II
  target debuff status, so level 50 AoE can spend Thunderhead on Thunder II when the AoE DoT is
  missing or about to expire.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder.

## 2026-05-12 - BLM AoE ST fallback leak fix

- Investigated the AoE report that BLM was selecting Blizzard III and then bricking during combat.
- Log evidence showed repeated Full Auto rejections for Blizzard IV (`3576`) and High Thunder
  (`36986`), followed by `No action` while still in combat.
- Root cause: the AoE selector could return null and fall through into the single-target Fire/Ice
  blocks, and the AoE branch also had explicit single-target Blizzard III and 2-target Blizzard IV
  fallbacks.
- Changed BLM targeting to prefer an actionable hostile from `DataCenter`, made AoE return directly
  from `GetNextAoEAction`, and removed Blizzard III/Blizzard IV from the AoE selector so AoE stays on
  the Freeze/Transpose/Flare path.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder.

## 2026-05-12 - BLM low-level Full Auto recovery fix

- Investigated the level 50 BLM AoE report that ice phase was still spamming Freeze, followed by
  Full Auto appearing to do nothing and ST lower-level phase exits breaking.
- Root cause: low-level AoE ice exit required `Weaving.CanWeave` before Transpose, so after a Freeze
  hard cast the selector could run with the GCD ready, skip Transpose, and fall through to Freeze.
  The first guard then overcorrected by returning no action when Transpose or Fire II was not
  immediately executable.
- Matched the WC/RSR-style low-level recovery by allowing Transpose at a real phase-swap point,
  adding GCD fallbacks for ST and AoE exits, and restoring the one-Freeze-per-ice guard so AoE can
  wait out Transpose without hard-stopping or looping Freeze.
- Changed the Full Auto execution pre-check to validate actions against the selected target instead
  of a targetless status check, and added throttled FrameworkUpdate diagnostics for no-action,
  rejected-action, and failed `UseAction` cases.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder.

## 2026-05-12 - Stage 06 level-sync rotation pass

- Used The Balance leveling guides plus PL3, WC, and RSR references for lower-level rotation shape.
- Fixed BLM lower-level AoE by using the current Blizzard II ID, removing proposed-action counters,
  and using MP/action readiness for Freeze -> Transpose/Fire II -> Flare transitions.
- Added level-gated AoE/ST fallbacks for current PL4 classes: PLD Total Eclipse/Prominence/Holy
  Circle and Rage of Halone fallback; WAR Overpower/Mythril Tempest/Steel Cyclone/Decimate/Orogeny;
  DRG Doom Spike/Sonic Thrust/Coerthan Torment plus Chaos Thrust fallback; BRD Quick Nock/Wide Volley
  and Rain of Death AoE handling.
- Updated `_config/action-ids.md` with the newly referenced PLD/WAR/BLM/DRG/BRD action IDs.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys the active Stage 06 DLL to `devPlugins\ParseLord4`.

## 2026-05-12 - ActionStacksEX stack reference

- Recorded `C:\Users\you\Documents\Projects\ActionStacksEX` as the external reference project for
  future PL4 stack work.
- Added routing notes in root `CONTEXT.md` and the active Stage 06 contract so stack changes start
  from ActionStacksEX patterns for stack execution, pronouns, import/export, and editor UX.
- Verified the reference project exists and contains `ActionStackManager.cs`, `PronounManager.cs`,
  `Configuration.cs`, and `PluginUI.cs`.

## 2026-05-12 - Active code formatting and GitHub remote

- Added the GitHub remote `origin` pointing to `https://github.com/SloppyMods/ParseLord4.git`.
- Ran `dotnet format` on the active Stage 06 project to normalize C# whitespace formatting.
- Verified formatting with `dotnet format ... --verify-no-changes`.
- Re-scanned active-stage C# for TODO/FIXME markers, empty catches, console logging, blocking
  `.Result`, and `.Wait()` usage; no matches were found.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and redeploys ParseLord4 to the devPlugins folder.

## 2026-05-12 - ParseLord4 standalone Git root

- Initialized `C:\Users\you\Documents\Projects\ParseLord4` as its own Git repository so Git no
  longer resolves the project root to `C:\Users\you`.
- Set the local branch name to `main`.
- Added a project-scoped `.gitignore` for build outputs, IDE/user files, logs, OS files, and
  Graphify working/cache files while leaving final Graphify report artifacts trackable.
- Verification: `git rev-parse --show-toplevel` now resolves to the ParseLord4 folder and
  `git branch --show-current` returns `main`.

## 2026-05-12 - Graphify HTML report artifact

- Reworked the Graphify report according to the global HTML artifact workflow: Markdown remains the
  canonical report, and a rich self-contained HTML review artifact was added alongside it.
- Added `graphify-out/GRAPH_REPORT.html` with summary cards, extraction caveat, god nodes,
  confidence/relation breakdowns, surprising connections, hyperedges, searchable/filterable
  communities, edge samples, suggested-question copy buttons, and local links to the Markdown report,
  interactive graph, and raw graph JSON.
- Verification: checked the HTML title, canonical Markdown notice, community search, suggested
  questions, file size, and absence of external HTTP/script/link dependencies.

## 2026-05-12 - BLM fixed Blizzard IV follow-up

- Investigated the reported single-target BLM pause between Blizzard III and Blizzard IV in the ice
  phase.
- Root cause: the Blizzard IV request flag could be set before Blizzard IV was actually actionable,
  so the rotation believed it had already requested the ice filler and then waited for hearts/retry
  timing instead of pressing Blizzard IV as soon as it became available.
- Changed the single-target ice branch so Blizzard IV remains the selected GCD until it has been
  requested and the ice phase has full Umbral Hearts, removing the old 4-second retry gate.
- Left AoE structure unchanged after clarifying that Blizzard II / High Blizzard II is not the
  desired meta framing for AoE.
- Verification: static guard check confirms the old Blizzard IV retry timer is gone;
  `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and 0 errors and
  deploys ParseLord4 to the devPlugins folder.

## 2026-05-11 - ParseLord4 status docs refresh

- Updated the current status snapshot in root `CONTEXT.md` to reflect Stage 06 as the active build and
  BLM polish stage.
- Updated `stages/06-polish/CONTEXT.md` with the current Assist/FullAuto contract, supported
  test-class rotation status, BLM Firestarter/ice/Triplecast/AoE status, and the next in-game BLM
  validation checkpoint.
- Updated the active README rotation coverage table and clarified that Stage 05 is now the
  historical ST baseline, while current post-handoff status lives in root and Stage 06 docs.
- Added a `CLAUDE.md` pointer to the canonical status sources so future work starts from the updated
  status snapshot.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors and deploys ParseLord4 to the devPlugins folder.

## 2026-05-11 - BLM Firestarter held for ice exit

- Clarified the BLM Firestarter rule after the user confirmed Firestarter should be saved until the
  end of ice.
- Removed the fire-phase low-stack Firestarter Fire III branch, so the rotation no longer spends
  Firestarter as a general fire-phase proc.
- Firestarter Fire III is now only selected from the single-target ice exit after the ice phase is
  ready: UI3, the Blizzard IV step for current high-level ice, and full Umbral Hearts.
- Updated `check-blm-firestarter-entry.ps1` so it fails if the fire phase spends Firestarter again.
- Verification: BLM Firestarter, ST ice two-spell, low-level Transpose fallback, AoE mode,
  AoE movement, AoE Freeze-once, Triplecast transition, Triplecast guard, and DataCenter actionable
  AoE checks pass; `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors.

## 2026-05-11 - BLM ice Firestarter skip guard

- Investigated the report that BLM was casting instant Firestarter Fire III immediately after
  Blizzard III, skipping the intended Blizzard IV ice filler.
- Root cause: the ST ice readiness guard could trust stale/full Umbral Hearts before the rotation had
  actually requested Blizzard IV in the current ice phase.
- Changed high-level ST ice readiness to require UI3, full Umbral Hearts, and a Blizzard IV request in
  the current ice phase before Paradox, Thunder, Firestarter Fire III, or hard-cast Fire III can be
  selected.
- If Blizzard IV was requested but hearts have not updated, the rotation now waits instead of choosing
  an exit GCD, with a short retry window if the request did not land.
- Added `check-blm-lowlevel-transpose.ps1` to cover pre-Fire-III/Blizzard-III sync behavior separately.
- Verification: BLM ST ice two-spell, Firestarter, AoE mode, AoE movement, AoE Freeze-once,
  Triplecast transition, Triplecast guard, DataCenter actionable AoE, and low-level Transpose fallback
  checks pass; `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors.

## 2026-05-11 - BLM normal Transpose automation removal

- Revisited BLM after the user noted Transpose should not be needed in the automated rotation.
- Checked The Balance Black Mage guidance first: Transpose is described as an AoE optimization, while
  the normal single-target and leveling phase swaps use Fire III / Blizzard III.
- Removed normal/high-level Transpose action selections from `BlackMageRotation`: the general ice oGCD
  weave and both AoE helper transitions now fall back to normal Fire/Blizzard GCD phase changes.
- Later low-level sync handling restored Transpose only for the pre-Fire-III/Blizzard-III FullAuto
  fallback, where The Balance leveling guide still recommends it.
- Updated the AoE regression checks so BLM fails if Transpose returns to the AoE helper.
- Verification: BLM AoE mode, AoE movement, ST ice two-spell, Firestarter, AoE Freeze-once,
  Triplecast transition, Triplecast guard, and DataCenter actionable AoE checks pass;
  `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and 0 errors.

## 2026-05-11 - BLM Umbral Hearts ice-exit guard

- Revisited BLM single-target ice after the report that MP could lag briefly after Blizzard IV and
  cause a third ice spell before Fire III.
- Consulted The Balance Black Mage basic guide before changing the rotation decision.
- Kept the short ice goal, but changed the completion signal away from MP timing: single-target ice is
  ready to exit at UI3 when Umbral Hearts are full or Blizzard IV has already been requested in the
  current ice phase.
- Added a `_requestedBlizzard4ThisIcePhase` guard, reset when leaving Umbral Ice, so the rotation does
  not request another ice filler while waiting for the hearts gauge to catch up.
- Allowed Paradox and Thunder in single-target ice after the Blizzard IV heart-fill is complete, while
  still keeping Xenoglossy for fire.
- Tightened the FullAuto ice Transpose weave so it does not jump over pending ice Paradox or Thunder.
- Updated the ST ice and Firestarter regression checks to cover the readiness guard.
- Verification: BLM ST ice two-spell, Firestarter, AoE mode, AoE movement, AoE Freeze-once,
  Triplecast transition, Triplecast guard, and DataCenter actionable AoE checks pass;
  `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and 0 errors.

## 2026-05-11 - Actionable hostile AoE filtering

- Investigated in-game report that AoE detection could flip on from extra aggro-list enemies that
  were not actually valid targets.
- Root cause: `DataCenter` trusted broad object-table hostile flags for splash counting, so dead,
  placeholder, pet, or non-enemy target-class objects could inflate `HostilesInSplashRange`.
- Reworked hostile collection through `IsActionableHostile`: targets must be valid, targetable,
  alive, in combat, non-pet battle NPCs, and classified by the client action target classifier as
  `ActionManager.TargetCategory.Enemy`.
- Kept target-centered splash counting, but now returns zero if the selected hostile itself fails
  the same actionable-hostile filter.
- Added `check-datacenter-actionable-aoe.ps1` and updated the AoE-mode checker for the selected-target
  guard.
- Verification: DataCenter actionable AoE regression check passes; BLM AoE mode, movement,
  Freeze-once, ST ice two-spell, Firestarter, Triplecast transition, and Triplecast guard checks pass;
  `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and 0 errors.

## 2026-05-11 - BLM AoE movement and Freeze repeat guard

- Investigated in-game reports that BLM did not react to movement in AoE mode and could cast Freeze
  twice during AoE ice.
- Consulted The Balance Black Mage basic guide before changing AoE movement decisions.
- Compared the local RSR BLM movement pattern; RSR routes movement through `IsMoving` and spends
  Triplecast when needed.
- Root causes:
  - ParseLord4 did not expose movement state to rotations, so BLM AoE kept selecting hard-cast AoE
    spells while moving.
  - AoE ice only checked `hearts < 3`, so if Freeze did not immediately advance the gauge state as
    expected, it could request Freeze again in the same ice phase.
- Added `GameState.IsMoving` from ECommons `Player.IsMoving` and exposed it on the Debug tab.
- BLM FullAuto now spends the reserved Triplecast charge for AoE movement when no instant AoE movement
  GCD is already available.
- BLM AoE Assist/FullAuto now gate hard-cast AoE spells while moving without Triplecast and prefer
  instant High Thunder II when it is needed.
- BLM AoE now tracks Freeze use per ice phase, resets the tracker after leaving Umbral Ice, and no
  longer uses Freeze as the neutral AoE opener.
- Added `check-blm-aoe-movement.ps1` and `check-blm-aoe-freeze-once.ps1`.
- Verification: BLM AoE movement regression check passes; BLM AoE Freeze-once regression check passes;
  BLM ST ice two-spell, AoE mode, Firestarter, Triplecast transition, and Triplecast guard checks pass;
  `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and 0 errors.

## 2026-05-11 - BLM short single-target ice phase

- Investigated in-game report that after the phase-gated AoE patch, BLM picked up an RSR-like habit
  of spending three GCDs in the single-target ice phase.
- Consulted The Balance Black Mage basic guide before changing ST ice decisions.
- Root cause: ParseLord4's ST ice branch still allowed Paradox, Thunder, and Xenoglossy before the
  Blizzard IV heart-fill and Fire III exit, which could extend Umbral Ice beyond the desired
  Blizzard III -> Blizzard IV -> Fire III routing.
- Constrained the single-target ice branch to fill hearts with Blizzard IV, then leave with
  Firestarter Fire III or hard-cast Fire III. Paradox, Thunder, and Xenoglossy remain available in
  fire instead of adding an extra ST ice GCD.
- Kept AoE filler behavior separate in the AoE helper.
- Added `check-blm-st-ice-two-spells.ps1` to guard this behavior.
- Verification: BLM ST ice two-spell regression check passes; BLM AoE mode regression check passes;
  BLM Firestarter entry regression check passes; BLM Triplecast transition and guard checks pass;
  `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and 0 errors.

## 2026-05-11 - BLM phase-gated AoE switching

- Investigated in-game report that BLM struggled switching between single-target and AoE mid-combat.
- Consulted The Balance Black Mage basic guide and compared local RotationSolver/RSR-style BLM logic
  before changing the rotation.
- Root cause: ParseLord4 treated AoE as a raw per-tick branch, so enemy-count changes could interrupt
  an active fire phase and flip between ST and AoE decisions at poor phase points.
- Added a BLM-only target profile latch with hysteresis. It enters AoE from the configured AoE target
  count, stays AoE down to one target below that threshold, and only reevaluates outside Astral Fire.
- Kept the current fire phase coherent when packs split or collapse, then switches profile on the next
  ice/no-element decision point.
- Passed `includeAbilities` into the AoE helper so Transpose automation only happens in FullAuto; Assist
  continues to automate GCDs while leaving oGCD transitions to the player.
- Added a Firestarter Fire III guard for Astral Fire I re-entry after Transpose so the ST route can
  quickly restore Astral Fire III.
- Verification: BLM AoE mode regression check passes; BLM Firestarter entry regression check passes;
  BLM Triplecast transition and guard checks pass; `.\scripts\build-active.ps1 -Configuration Release`
  succeeds with 0 warnings and 0 errors.

## 2026-05-11 - BLM AoE target-count routing

- Investigated in-game report that BLM acted like it was still on one enemy while testing in AoE
  settings.
- Consulted The Balance Black Mage basic guide before changing AoE rotation decisions.
- Root causes:
  - `BlackMageRotation` had AoE action IDs defined but never branched on the configured AoE target
    count, so it always used the single-target loop.
  - `DataCenter.HostilesInSplashRange` counted enemies around the player, which undercounts ranged
    caster AoE packs centered on the selected hostile.
- Centered `HostilesInSplashRange` on the current hostile when available.
- Added a conservative BLM AoE branch for configured target counts using High Thunder II, Freeze or
  Blizzard IV for two-target fallback, Transpose, Flare, Flare Star, High Blizzard II, and High Fire
  II fallback.
- Added BLM AoE action IDs to `_config/action-ids.md`.
- Verification: BLM AoE mode regression check passes; BLM Firestarter entry regression check passes;
  BLM Triplecast transition and guard checks pass; `.\scripts\build-active.ps1 -Configuration
  Release` succeeds with 0 warnings and 0 errors.

## 2026-05-11 - BLM Firestarter ice exit fix

- Investigated in-game report that BLM would hard-cast Fire III back into fire but would miss the
  Firestarter-proc Fire III route.
- Consulted The Balance Black Mage basic guide before changing the rotation decision.
- Root cause: ice-phase Firestarter logic was behind the hard-cast Fire III path, so the rotation
  could leave ice with a normal Fire III instead of consuming the instant Firestarter proc.
- Kept Blizzard IV first to fill three Umbral Hearts, then preferred Firestarter Fire III over
  hard-cast Fire III once `hearts == 3 && uiStacks == 3`.
- Added BLM Fire III and Firestarter IDs to `_config/action-ids.md`.
- Verification: BLM Firestarter entry regression check passes; BLM Triplecast transition and guard
  checks pass; `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors.

## 2026-05-11 - BLM Triplecast charged-action execution fix

- Investigated continued in-game report that BLM still did not use Triplecast after the transition
  gate correction.
- Root causes:
  - BLM still gated Triplecast through `IsReady`, which includes `IsActionOffCooldown`; charged
    actions can be usable while the recast group is active.
  - The Triplecast request still lived inside the generic `Weaving.CanWeave` oGCD block, which can
    miss BLM's short post-Fire IV transition window.
- Moved Triplecast's transition request before the generic oGCD block, kept it scoped to the
  Flare Star transition, checked charged-action usability with `GetActionStatus == 0`, and retained
  the `> 1` charge reserve plus duplicate-cast guards.
- Tightened the BLM Triplecast regression checks to reject `IsReady(am, Triplecast)` and require a
  Triplecast-specific transition window.
- Verification: BLM Triplecast transition regression check passes; BLM Triplecast guard regression
  check passes; `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors.

## 2026-05-11 - BLM Triplecast transition gate correction

- Investigated in-game report that BLM stopped using Triplecast after the one-charge reserve change.
- Root cause: the transition helper waited for `GameState.CurrentMp < 2400`, but the rotation selects
  Flare Star immediately at 6 Astral Soul stacks before reaching the low-MP fallback path.
- Removed the low-MP requirement so Triplecast can fire in the actual pre-Flare-Star weave window,
  while still requiring the spare charge, no active Triplecast buff, and no recent Triplecast request.
- Tightened the BLM Triplecast transition regression check so this low-MP gate cannot return.
- Verification: BLM Triplecast transition regression check passes; BLM Triplecast guard regression
  check passes; `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors.

## 2026-05-11 - BLM Triplecast transition reserve

- Refined BLM FullAuto Triplecast use after in-game testing showed the generic cooldown spend was not
  valuable for normal Fire IV casts.
- Triplecast is now reserved for the end-of-fire Flare Star -> Blizzard III -> Blizzard IV transition
  and only fires when more than one Triplecast charge is available, leaving one charge for manual
  movement.
- Kept the existing Triplecast buff guard and short local request lockout so charged recast timing
  cannot cause an immediate duplicate request.
- Added BLM Blizzard III, Blizzard IV, and Flare Star IDs to `_config/action-ids.md`.
- Verification: BLM Triplecast transition regression check passes; BLM Triplecast guard regression
  check passes; `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors.

## 2026-05-11 - BLM Triplecast duplicate guard

- Investigated in-game report that BLM Full Auto tried to double-cast Triplecast, with another addon
  blocking the duplicate.
- Root cause: BLM oGCD logic only checked `IsReady(Triplecast)`. Because Triplecast has charges and
  buff application can lag by a frame, the rotation could request it again while Triplecast was
  active or immediately after the previous request.
- Added a Triplecast status guard and a short local request lockout to `BlackMageRotation`.
- Added BLM Triplecast action/status IDs to `_config/action-ids.md`.
- Verification: BLM Triplecast guard regression check passes; Assist GCD-only regression check passes;
  `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and 0 errors.

## 2026-05-11 - Assist mode GCD automation contract

- Updated Assist mode to match the intended FFXIV use case: keep the GCD rolling while the player
  handles oGCD weaving manually.
- `FrameworkUpdateHook` now runs in Assist and FullAuto.
- `RotationManager.Tick(OperationMode mode)` passes `includeAbilities: false` for Assist and
  `includeAbilities: true` for FullAuto.
- Updated `IRotation` plus PLD, WAR, DRG, BRD, and BLM rotations so oGCD blocks only run when
  `includeAbilities` is true.
- Updated UI/debug/README/architecture wording to distinguish GCD automation from oGCD automation.
- Verification: Assist GCD-only regression check passes; Assist combo regression check passes;
  job-refresh regression check passes; `.\scripts\build-active.ps1 -Configuration Release` succeeds
  with 0 warnings and 0 errors.

## 2026-05-11 - Assist mode combo activation fix

- Investigated Assist mode after in-game report that it appeared to do nothing.
- Root causes:
  - Fresh configs had no enabled combo presets, so Assist could enable Layer 1 without any preset
    allowed to replace actions.
  - `AdjustedActionHook` matched combo triggers against the game-adjusted action ID instead of the
    pressed/base action ID, so built-in upgrades could bypass ParseLord4 combo triggers.
  - BRD's preset only duplicated Heavy Shot to Burst Shot, which the game already handles, so Bard
    Assist had no visible effect.
- Updated `AdjustedActionHook` to evaluate combo triggers using the pressed/base action ID, fall back
  to the game's original adjusted action when no custom replacement exists, and game-adjust custom
  replacements before returning them.
- Updated `ComboRegistry` to enable discovered combo presets for a fresh empty config.
- Updated BRD Assist preset so filler can become Refulgent Arrow/Straight Shot while the proc is
  active.
- Verification: Assist combo regression check passes; job-refresh regression check passes;
  `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and 0 errors.

## 2026-05-11 - BRD job 23 stale UI refresh fix

- Verified the job mapping: BRD/Bard is job ID `23` in `_config/action-ids.md`,
  `BardRotation.JobId`, and `BRDMainCombo.JobId`.
- Root cause was stale read paths, not a bad BRD ID: `RotationManager.ActiveRotation` only refreshed
  during combat ticks, and `ComboRegistry.Filtered` only refreshed after the adjusted-action hook ran.
- Updated `RotationManager.ActiveRotation` to refresh the current job before returning `_active`.
- Updated `ComboRegistry.Filtered` to refresh from `GameState.JobId` before returning the current-job
  combo list.
- Verification: the job-refresh regression check passes; `.\scripts\build-active.ps1 -Configuration
  Release` succeeds with 0 warnings and 0 errors.

## 2026-05-11 - Stage 06 UI and IPC polish start

- Polished the Stage 06 main window with a header, larger constraints, scrollable window behavior, and
  a clearer conflict warning banner.
- Expanded the Rotation tab with live mode switching, mode descriptions, opener toggle, AoE target
  count, emergency HP threshold, HP bar, target, combat, and hostile count readouts.
- Expanded the Combo tab with current-job enable/disable-all controls, trigger ID visibility, and
  known preset count.
- Added stack editor validation hints for missing triggers/items and clarified stack item priority.
- Grouped Debug tab runtime, hook, weaving, and DataCenter state.
- Reworked `IPCProvider` to expose EzIPC provider methods and wired it into `Plugin`.
- Added Stage 06 `README.md` with install, commands, modes, stack editor, conflict, and IPC notes.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and
  0 errors.

## 2026-05-11 - Stage 06 polish handoff

- Accepted the user's call that Stage 05 single-target test-class rotations are good enough to move
  on for now.
- Copied `stages/05-auto-rotation/output/` into `stages/06-polish/output/`.
- Updated routing so `stages/06-polish` is the active stage and `scripts/build-active.ps1` builds the
  Stage 06 project snapshot.
- Marked Stage 05 as done for ST with AoE and broader job coverage deferred.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` builds
  `stages/06-polish/output/ParseLord4/ParseLord4.csproj` successfully with 0 warnings and 0 errors.

## 2026-05-11 - Global ICM task workflow skill

- Added the global `icm-task-workflow` skill for Claude at
  `C:\Users\you\.claude\skills\icm-task-workflow\` and mirrored it for Codex/agents at
  `C:\Users\you\.agents\skills\icm-task-workflow\`.
- Included a conservative `scripts/scaffold_icm.py` helper that creates missing ICM routers,
  `_config/`, `shared/`, `setup/`, and numbered stage contracts without overwriting existing files
  by default.
- Wired ParseLord4's `CLAUDE.md`, `CONTEXT.md`, and `_config/icm-workflow.md` to treat the global
  skill as the reusable ICM rule and this project file as the local staged-code adaptation.
- Updated the ParseLord4 status snapshot from the stale Dalamud SDK 14 blocker to the current
  Dalamud API 15 Release build success.
- Verification: both skill installs pass `quick_validate.py`; the scaffolder succeeds in dry-run and
  scratch write modes; `python -m py_compile` succeeds for the scaffolder; the active Release build
  succeeds with 0 warnings and 0 errors.

## 2026-05-11 - ICM bootstrap for ParseLord4

- Normalized the Claude entry path around ICM layers: `CLAUDE.md` to `CONTEXT.md` to stage contracts.
- Added local ICM rules in `_config/icm-workflow.md`.
- Added canonical contracts for Stage 03, Stage 04, and active Stage 05.
- Converted legacy planning stage folders into pointers to canonical stage folders.
- Updated project build routing to the active Stage 05 project file.
- Verification: active Release build currently stops before compilation because `Dalamud.NET.Sdk/14.0.1`
  does not resolve in the local .NET/NuGet environment.

## 2026-05-11 - Patch target correction

- Corrected ParseLord4's target to FFXIV Patch 7.5 in project config, setup answers,
  action ID notes, and the design architecture artifact.
- Verification: no remaining `7.4` references found in project markdown/json/csproj files. Active Release
  build still stops before compilation because `Dalamud.NET.Sdk/14.0.1` does not resolve locally.

## 2026-05-11 - BRD Heartbreak Shot charge fix

- Fixed `BardRotation.GetCharges()` so inactive recast detail means Bloodletter/Heartbreak Shot is at
  full charges instead of zero charges.
- This addresses Heartbreak Shot only starting after a manual first cast, because a manual cast activates
  the recast detail and previously masked the bad full-charge calculation.
- Fixed local Codex build execution by adding `scripts/build-active.ps1`, which sets missing Windows
  profile/system environment variables before invoking dotnet.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and 0 errors.
  The deployed DLL at `%APPDATA%\XIVLauncher\devPlugins\ParseLord4\ParseLord4.dll` was updated.

## 2026-05-11 - Dalamud API 15 metadata fix

- Updated Stage 05 project from `Dalamud.NET.Sdk/14.0.1` to `Dalamud.NET.Sdk/15.0.0`.
- Updated `DalamudPackager` to `15.0.0` and deployed manifest `DalamudApiLevel` to `15`.
- Registered `UiBuilder.OpenConfigUi` so Dalamud's plugin validation can open the settings UI.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and 0 errors.
  Deployed `ParseLord4.json` now reports `DalamudApiLevel: 15`.

## 2026-05-11 - BRD song cycle fix

- Replaced the no-song-only BRD song logic with an explicit cycle:
  Wanderer's Minuet -> Mage's Ballad -> Army's Paeon -> Wanderer's Minuet.
- Added a 3-second song cast lockout to prevent multiple songs from being fired before the new song buff
  appears in game state.
- Swapped to the next song when the current song has 2.5 seconds or less remaining, if the next song is
  ready.
- Songs now target the current hostile instead of self.
- Verification: `.\scripts\build-active.ps1 -Configuration Release` succeeds with 0 warnings and 0 errors.
  Deployed DLL updated in `devPlugins\ParseLord4`.
