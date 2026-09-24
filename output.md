READ IN THIS ORDER.

A. Phase goal: Hamlet reads a CQ call correctly. Steps 0, 1, 2 done;
   3 partial on 3.4 and 3.6, with 3.6 parked as P19 on the floors;
   4 and 5 not started; 6 partial on 6.5 alone now that 6.3 is met; 7 partial on
   7.1 to 7.4 and 7.6.
B. Step 6, criterion 6.3 clause by clause: General, 14.050 MHz covered and
   14.010 MHz not, both from the privileges file's rows; the three panels'
   left edges and widths at both, before and after; the lines that moved
   them (BandGovernsTheMapPanel's two fit questions); the test watched red,
   its message quoted, then green; the panel's words and color shown still
   changing. Then 6.6 at exit, held.
C. The rest. Section 4 raises 9 items, 8 carried and 1 new (P21, the row's
   height). None of them is in the way of 6.3.

```
UNIT:       423 - complete at task 4 of 4, none dropped - 2026-09-24 17:01
PHASE GOAL: Hamlet reads a Morse CQ call correctly, measured against a key, and the screen stops asserting what it does not know while the decoder work waits on the owner.
UNIT GOAL:  Tuning to a frequency Tim's license does not cover no longer moves the map, the neighborhood card or the rig face; only the privilege panel's words and color change, and a headless test proves it at both frequencies.
ADVANCED:   yes - 6.3 met and ticked: its test was committed red at fb11154c and is green at 89394369 at all four sizes, going out and coming back
NUMBER:     panels that move outside his privileges at the default size, 0 of 3 -> 0 of 3 (nothing moved at 1100 x 780 even before); largest shift at any size 562 px -> 0 px (the sun map at 1400 x 1040)
DRIFT:      0
```

## 1. What Claude did

**Complete, 4 of 4 tasks, none dropped.** Machine QUIVERFULL, project claimed Hamlet, confirmed by `SHACK_FACTS.md` and `src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` present and `CoreHMI.sln` and `MURC.sln` absent at `C:\Source\HamLet`. Branch `main`. Everything was measured on the development computer, headless, so **nothing in this report is evidence about the radio** (HM-DEC-093).

**Section 5 checks, reported and not repaired.**
- Section 4's claims hold, with one exception. `TopRow` is at line 3143 with `*,Auto` and `MaxHeight="300"`. `BandRow` is at 3161. `TopRowCardScroller` holds `widget.map`. `GreenZoneMap` is there. `BandGovernsTheMapPanel.cs` was 273 lines at entry, with `MeasureOverride` at 92. The tone bindings are at 706 to 791, and Culture, Reassurance and UpgradePrompt at 931 to 957. `PrivilegeTone` is `Unknown`, `Yours`, `ListenOnly`, with amber for outside privileges. The five top-row types and the two licensing types all exist.
  - **Mismatch:** `BandRow`'s third child is not `ctl:RigDisplayControl` itself. It is the amber `Border` at line 3211 that wraps it; the control is at 3223. The trace measures the `Border`, because that is what the panel places.
- **Clause 4 was the smaller half of the cause.**
  - Clause 4 (`CardFits`, the map giving width back) moved things at 1280 x 720 and 1366 x 728.
  - The large jump was the left-edge question from work instruction 389 (`AtTheLeftEdge`).
- **The license class** is `AppSettings.Operator.LicenseClass`, read by `MainWindowViewModel.UpdatePrivileges`. A test sets it in the settings it hands the view model, plus a `FixedLicenseLookup` giving the same answer for KC3QIS, so nothing reaches the network.
- **The two frequencies** come from `data\privileges\us-part97-privileges.json`, both CW on 20 m. CW is allowed on any authorized frequency (97.305(a), the file's line 167).
  - **Covered: 14.050 MHz**, in General's 20 m row `14025000 to 14150000`, line 132, 97.301(d).
  - **Not covered: 14.010 MHz.** On 20 m only Extra's row `14000000 to 14350000` reaches it, line 97, 97.301(b). General's 20 m rows start at 14.025.
- **How a headless test tunes: the rig face's own write.** The wheel over a digit calls `SetCurrentValue(RigDisplayControl.FrequencyHzProperty, ...)`. That property is two-way bound to `MainWindowViewModel.FrequencyHz`, and its change handler runs `UpdateModeLine` and then `UpdatePrivileges`. The tests write the same property the same way.
  - Connected, the training radio goes through `ToggleConnectCommand`, as in unit 422. The radio answers on 40 m, so the trace presses the 20 m pill before tuning.
- **Window sizes the existing top-row tests use:**
  - 1920 x 1040, 1400 x 1040, 1100 x 780 (the opening size, `MainWindow.axaml:12`), 900 x 620, 1280 x 720, 1366 x 728, 1536 x 824, 1920 x 1017 and 2560 x 1400
  - 1200 x 900 and 1400 x 900
  - 1100 x 620, 580, 540 and 500
- **The reload's two disagreements, repaired neither.**
  - `PROJECT_STATUS.md`'s `RULES_AT` reads HM-DEC-165. `CLAUDE.md` §1's top row is **HM-DEC-176** (2026-09-24), not "CPS-DEC-0176" as the reload wrote it.
  - The four root files were uncommitted at entry. `RUN_LEDGER.md` and `WORK_INSTRUCTIONS.md` are left uncommitted.
  - `PHASE_OUTCOME.md` and `PHASE_STATUS.md` had to be committed for task 0, so ae6b0092 also carries the launcher's own additions to them: the appended `UNIT 2 - STEP 6` block and the `HEARTBEAT` line.

**Task 0, the record, ae6b0092.**
- Version 1.13.109 to 1.13.110.
- `PHASE_STATUS.md` names unit 423 and `CURRENT_STEP: 6`.
- `PHASE_OUTCOME.md` has its `UNIT 423 - STEP 6` entry.
- Entry round, one type per invocation:
  - build with warnings as errors: green
  - engine carry-forward 178 of 178
  - app carry-forward 276 of 278, with two lost to the dispatcher loop and green alone (`ThePowerIsOfferedTests` 3 of 3, `BindingHealthTests` 1 of 1)
  - floors: captures 51 of 51, adjudicated 13 of 13, clean 2 of 2, keyed 13 of 13
  - `EveryControlSaysWhatItDoesTests` 3 of 3
  - top-row types: `TheBandRowIsWhereItWasRuledTests` 8 of 8, `TheTopRowTests` 15 of 15, `Unit376TheTopBandTests` 5 of 5, `TheSunMapStandsWhereItWasLeftTests` 2 of 2, `TheWindowHoldsBelowItsMinimumTests` 3 of 3
  - `OutOfBandTests` 16 of 16 and `PrivilegeStatusLineTests` 9 of 9
  - red as expected: P16 and P20

**Task 1, the trace, 987858b6.** `WhereThePanelsStandOutsideHisPrivilegesTests` asserts nothing.
- It runs at all 15 sizes on the CW and the Digital tab. At each size a General operator is tuned 14.050, then 14.010, then back to 14.050.
- It widens to every class at 1100 x 780, against 14.010 and against 14.360 (out of band), and to the connected state at 1100 x 780 and 1920 x 1040.
- For each state it prints: the three panels' bounds, the tone, the green block's background, every sentence it draws with drawn and wanted width, and the panel's decision (map height, left edge or not, pills reach, the card's slot, width given back, the card's content height).
- **The report was right, and the window moved at five sizes out of fifteen.** Section 3 has the figures.
- **Two sizes did worse than move.** At 1920 x 1040 and 1920 x 1017, tuning to 14.010 threw Avalonia's *Infinite layout loop detected*.
- **What moved them:** `BandGovernsTheMapPanel.MeasureOverride`'s two fit questions on the card's height, `CardFits` (`card.DesiredSize.Height <= rowHeight + 0.5`) and `AtTheLeftEdge` (`> rowHeight + 0.5`). Outside his privileges the green block adds *Receiving is never restricted. Any license may listen anywhere.* and the *What would Extra unlock?* button, and the verdict grows from *yours to use* to *listen all you like, but don't transmit*. The card gets 22 to 42 px taller, the fit fails, and the map is placed again.
- **Other classes and out of band:**
  - For Novice, Technician and Unknown, 14.050 and 14.010 have the same tone, so nothing changes between them.
  - For Extra both are covered.
  - 14.360 grows the card further at 1100 x 780. No left edge or width moved there for any class.

**Task 2, the test, fb11154c, committed red and alone.** `ThePanelsHoldTheirColumnsOutsideHisPrivilegesTests` builds the window headless on the CW tab and tunes by the rig face's write. It runs at 1100 x 780 (the default), 1280 x 720 (the smallest size where task 1 saw the panels move), and also 1400 x 1040 (the largest jump) and 1920 x 1040 (the layout loop). It reads the left edge and width of the card, the map and the rig face off the laid-out window, and fails when:
- any of the six figures moves more than a pixel, naming the panel and both figures
- the layout does not settle
- the tone, the green block's color or its words are the same at both frequencies

**Task 3, the columns held, 89394369.** Three changes to `BandGovernsTheMapPanel`, each measured:
1. **`OutsideTheFit`**, an attached flag set in `MainWindow.axaml` on the reassurance sentence, the upgrade row and the ladder. These are the lines drawn only outside his privileges. They are still measured, arranged and drawn, but they are left out of the two fit questions. This alone ended the 1920 layout loop, but at 1280 and 1400 the longer verdict still moved the map.
2. **`HeldAcross`**, bound to `PrivilegeStatus.Tone`. If the width, the rig face and the pills row are the same as when the arrangement was decided and only the tone differs, the panel keeps that arrangement. Any other change decides afresh, as before.
3. **A stale-measure guard.** After the first two changes, the trace showed the map coming back from 14.010 to 327 x 178 beside the card at 1400 x 1040, where it had started at 393 x 214 at the left edge. A probe found the cause:
   - The rig face's new digits get the row re-measured before the card's changed lines.
   - The card, asked again at the width it had last had, answers from its cache with the outside-privileges height.
   - The fallback then re-measures everything, so nothing ever asks again.

   The fix: before the fit questions, any card line still waiting to be measured marks the path up to the card as waiting too. The first try, dropping `AffectsMeasure` from `HeldAcross`, was not enough on its own, and `HeldAcross` stays without it.

**Decisions recorded for myself under §12.1, each overrulable, in full:**
- *Holding across the tone.* Where only the privilege panel's tone has changed since the arrangement was decided, the card, the map and the rig face stay where they were decided. Why: the verdict line exists inside his privileges too, so leaving it out of the fit would change arrangements the existing tests pin. A hold keyed on the tone changes nothing inside his privileges.
  - Rejected: holding across every change of the card's words, which would break `TheSunMapStandsWhereItWasLeftTests`' FT8 to PSK31 switch in one window.
  - Rejected: fitting the card as if it were always outside privileges, which would move the map off the left edge at 1400 on FT8 inside his privileges.
  - Known consequence: a window opened outside his privileges keeps that arrangement when he tunes inside, until a resize or a change inside the card under one tone.
- *The ladder is marked `OutsideTheFit`* along with the reassurance and the upgrade row. It is drawn only outside his privileges and only when he presses the upgrade button.
- *The test's sizes go beyond the two named.* 1400 x 1040 and 1920 x 1040 are added, because the largest jump and the layout loop are there.
- *A return check was added to task 2's test in task 3*: back at 14.050 the panels must stand where they stood at 14.050. It was never committed red on its own. Its red is the trace's 1400 x 1040 figure above, before the stale-measure guard.
- *P21 is parked* (section 4).

**Task 4, the exit round (6.6 held).**
- `Hamlet.sln` builds non-incremental with warnings as errors: 0 warnings, 0 errors. The DLLs were rebuilt at 16:46.
- Carry-forward: engine 178 of 178. App 277 of 278; `TheTestsStayOffTheNetworkTests.ThePlainFixtureTakesGeneralFromTheFixedAnswer` was lost to the dispatcher loop, and the type is 5 of 5 alone.
- Floors: captures 51 of 51, adjudicated 13 of 13, clean 2 of 2, keyed 13 of 13.
- Green: `BindingHealthTests` 1, `EveryControlSaysWhatItDoesTests` 3, `WhatEveryControlSaysOnHoverTests` 1, the five top-row types at their entry counts (8, 15, 5, 2, 3), task 2's test 1 of 1, task 1's fact 1 of 1, `OutOfBandTests` 16, `PrivilegeStatusLineTests` 9.
- Other types that read the band row, green: `TheFavoritesAreChipsTests` 4, `TheWorkingPanelsTests` 8, `Unit388TraceTests` 1, `Unit389TraceTests` 2.
- Red as at entry: P16 and P20.
- The transmit files print nothing against `7e209cb4`. `src/Hamlet.RadioEngine` and `data` print nothing against entry `0456cad7`. `src` differs only in `BandGovernsTheMapPanel.cs` and `MainWindow.axaml`.
- **Nothing is red that was green at entry. 6.3 is ticked** in `PHASE_PLAN.md`.
- Pushed: yes, each task's commit pushed to `origin main` with rc 0. Task 4's push result is in the delivery message.

## 2. What the owner should expect

Tuning outside your privileges no longer moves the map, the neighborhood panel or the radio panel: each keeps its left edge and its width, going out and coming back, at every window size the tests use. What still changes is the privilege panel's words and color: the verdict turns to *listen all you like, but don't transmit*, *Receiving is never restricted* and the *What would Extra unlock?* button appear, and the block goes from green to amber. What caused the jump: the row asked whether the neighborhood card still fit beside the map at full size, and outside your privileges those extra lines made the card too tall, so the map was moved or shrunk to make room.

**What will look wrong but is not.**
- **The top row still gets taller outside your privileges.** At 1400 x 1040 the extra lines make it 42 px taller and the working panels below move down by that much. That is P21, parked for your call. The columns hold; the height follows the words, as before.
- **The two red tests are the known baseline.** `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten` is P16 and `Unit302CeilingHoldsStillTests.TheLiveReadoutsAreStillOnScreen` is P20. Both were red at entry.
- **The figures are headless.** The test host draws text in a fixed-width stand-in font, so a pixel here is not a pixel on your screen. Every top-row test measures the same way.

## 3. What you should see

**Task 1's table: the three panels at 14.050 (covered) and 14.010 (not covered), General, CW tab.** Left and width; top and height where they differ. "Before" is 987858b6's code, "after" is 89394369. At 14.050 the two are identical.

| Size | Panel | 14.050, before and after | 14.010, before | 14.010, after |
|---|---|---|---|---|
| 1100 x 780 (default) | card | 16.0, 248.0 (h 345) | 16.0, 248.0 (h 523) | 16.0, 248.0 (h 523) |
| | map | 278.0, 246.0 | 278.0, 246.0 | 278.0, 246.0 |
| | rig | 538.0, 546.0 | 538.0, 546.0 | 538.0, 546.0 |
| 1280 x 720 | card | 16.0, 401.0 | 16.0, **428.0** | 16.0, 401.0 |
| | map | 431.0, 273.0 x 149 | **458.0, 246.0** x 134 | 431.0, 273.0 x 149 |
| | rig | 718.0, 546.0 | 718.0, 546.0 | 718.0, 546.0 |
| 1366 x 728 | card | 16.0, 433.0 | 16.0, **514.0** | 16.0, 433.0 |
| | map | 463.0, 327.0 x 178 | **544.0, 246.0** x 134 | 463.0, 327.0 x 178 |
| | rig | 804.0, 546.0 | 804.0, 546.0 | 804.0, 546.0 |
| 1400 x 1040 | card | 423.0, 401.0 | **16.0, 548.0** | 423.0, 401.0 |
| | map | 16.0, 393.0 x 214 (left edge, top 83) | **578.0, 246.0** x 134 (top 119) | 16.0, 393.0 x 214 (left edge, top 83) |
| | rig | 838.0, 546.0 | 838.0, 546.0 | 838.0, 546.0 |
| 1536 x 824 | card | 423.0, 537.0 | **16.0, 684.0** | 423.0, 537.0 |
| | map | 16.0, 393.0 x 214 (left edge) | **714.0, 246.0** x 134 | 16.0, 393.0 x 214 (left edge) |
| | rig | 974.0, 546.0 | 974.0, 546.0 | 974.0, 546.0 |
| 1920 x 1040 | all three | card 423.0, 921.0; map 16.0, 393.0; rig 1358.0, 546.0 | **Infinite layout loop detected** | same as 14.050, settled |

- **All 15 sizes, both tabs, after:** every panel's left and width moved 0.0 px. This includes 1400 x 900 and 1920 x 1017, which moved or looped before, and connected at 1100 x 780 and 1920 x 1040.
- **Back at 14.050 after 14.010:** the arrangement is the one it started from at every size.
- **Heights after, card and rig face:** +2 px at 1920, +22 at 1536 x 824, +42 at 1280 x 720, 1366 x 728 and 1400 x 1040, +90 at 1200 x 900, and 345 to 523 at 1100 x 780 (P21).
- **The words and color still change** at every size:
  - tone `Yours` to `ListenOnly`
  - block background `#ffe9f6ec` to `#fffdf1e0`
  - verdict *Morse · yours to use* to *Morse · listen all you like, but don't transmit*
  - license line *Your General license covers Morse here. Call away · 97.305(a)* to *General privileges do not reach this frequency; it needs Extra · 97.301(d)*
  - two lines added: *Receiving is never restricted. Any license may listen anywhere.* and *What would Extra unlock?*

**The red message, fb11154c:**

```
1280 x 720: the neighborhood panel's width is 401.0 at 14.050 MHz and 428.0 at 14.010 MHz
1280 x 720: the sun map's left edge is 431.0 at 14.050 MHz and 458.0 at 14.010 MHz
1280 x 720: the sun map's width is 273.0 at 14.050 MHz and 246.0 at 14.010 MHz
1400 x 1040: the neighborhood panel's left edge is 423.0 at 14.050 MHz and 16.0 at 14.010 MHz
1400 x 1040: the neighborhood panel's width is 401.0 at 14.050 MHz and 548.0 at 14.010 MHz
1400 x 1040: the sun map's left edge is 16.0 at 14.050 MHz and 578.0 at 14.010 MHz
1400 x 1040: the sun map's width is 393.0 at 14.050 MHz and 246.0 at 14.010 MHz
1920 x 1040: tuned to 14.010 MHz the layout did not settle - InvalidOperationException: Infinite layout loop detected
```

**The green run:** at 89394369 and again at exit, `ThePanelsHoldTheirColumnsOutsideHisPrivilegesTests` passes 1 of 1 at all four sizes, with the return check included.

## 4. What's blocking us

Nothing here blocks 6.3, and nothing halts the phase (R65). The carried items are copied word for word from the work instruction (HM-DEC-139).

1. **P19**: 3.6's added strays stand above the bar and every floor they sit under is at its count, so none can leave without the owner's ruling. The ruling is proposed in `PARKED.md`.
2. **P12** stays parked: the `captured` and `broadcast` clock lines.
3. **The `clipping` and `inputFloor` question** stays parked.
4. **P14, P15, P16 and P18** stay parked. *(P16 was red again at exit, message unchanged.)*
5. **P20** stays parked: `Unit302CeilingHoldsStillTests.TheLiveReadoutsAreStillOnScreen` looks for `TurnRingCountText`, and no element in `src` carries that name. *(Red again at exit.)*
6. **Unit 421's R71 count:** R71's prose says 40 unkeyed captures, and the tree has 29 unkeyed rows of 51. Recorded, not a ruling.
7. **Unit 421's `SpanMarginForRecord` remark** stays until a unit touching `CwCharacter` rewrites it with the trace beside it (12.6).
8. **Unit 422's reading of "the band row"**: the header strip that holds Connect, plus `BandRow` and `BandPills`. It is author's and overrulable.
9. **New, P21, parked: outside his privileges the top row still grows taller.**
   - **Proposed, the owner's:** leave the height following the words, as now.
   - **Reasoning:** holding the height too would make the card scroll *Receiving is never restricted* and the upgrade button out of sight inside the row. §0.5 allows that, but it hides the sentence that removes the fear until he scrolls.
   - **Rejected:** capping the card at the row's height in this unit. 6.3 names columns, not heights, and the choice between a moving panel edge and a hidden sentence is his.
   - **Measured:** the rise is 2 to 90 px at the tested sizes (42 at 1400 x 1040). Where the row is under the 300 px cap, the working panels below move down by that much.
   - Not blocking.

**Asks still outstanding**

- P19, first raised by unit 421 on 2026-09-24. It waits on the owner's ruling on whether a key-aligned added character may leave a floor. The floors are in the engine's floor tests, and the proposal is in `docs\phase-correctness\PARKED.md` P19.
- P20, first raised by unit 422 on 2026-09-24. It waits on whether the slot countdown returns to the Digital tab or the test retires. The test is `tests\Hamlet.App.Tests\Views\Unit302CeilingHoldsStillTests.cs`, and the entry is `PARKED.md` P20.
- Unit 422's reading of "the band row", 2026-09-24. It waits on the owner's reading. The lists are in `tests\Hamlet.App.Tests\Views\EveryControlSaysWhatItDoesTests.cs`.
- P21, new from unit 423, 2026-09-24. It waits on whether the top row's height should also hold outside his privileges. The change sits in `src\Hamlet.App\Controls\BandGovernsTheMapPanel.cs`, and the entry is `PARKED.md` P21.
