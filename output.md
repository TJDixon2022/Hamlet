```
UNIT: 527 - tasks 1 and 2 done, task 3 dropped with its count - 2026-10-02
UNIT GOAL: the record button is on the screen
NUMBER: Record inside the window at 1600 by 900 and 1100 by 800 (at y 393; the old press sat at y 832)
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`, and the order's gate held: `SHACK_FACTS.md`, `CwPatternGate.cs` and `CW_REQUIREMENTS.md` exist, there is no `CoreHMI.sln` or `MURC.sln`, and the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio. HM-DEC-231 was free.

**How the session ran:**
- It took SESSION.lock and released it at the end.
- It wrote nothing to `RUN_LEDGER.md`, touched nothing under `tools\arbiter\`, and ticked no box.
- It read no recording, fixture or telemetry.
- Nothing keys, transmits or writes to the radio.

**Task 1: why it was not visible.** No code changed. A probe built the real `MainWindow` headless on the CW tab and walked the capture press's ancestors; the probe was not committed.

| window | IsVisible | effectively | enabled | press at | edge it falls past |
|---|---|---|---|---|---|
| 1600 × 900 | True | True | False (not decoding) | y 832 | receive widget ends at 813; workspace at 826 |
| 1100 × 800 | True | True | False | y 832 | window ends at 800 |

- Every ancestor was visible, so it was **clipped by a fixed height and pushed outside the window**. It was not hidden by a binding or collapsed by a panel.
- **The fixed height:** since unit 373 (`e352dcb6`, 2026-09-20) the CW workspace's height has been set by the window, and the receive widget's content is clipped at it.
- **The push:** rows were added above the press since. Unit 476's scope row (`1d398861`, 2026-09-28) and unit 521's shape light (`4c4bf565`, 2026-10-01) pushed it below the line.
- Unit 526 checked the code and not the screen.

**Task 2: a Record button.** Commit `d9faf9bf`.
- **`MainWindow.axaml`:** `RecordButton`, labelled `Record`, sits in the CW terminal's header beside `Copy` and `Clear`. It is bound to the same capture command, enabled while listening, and keeps its hover when grayed (`ToolTip.ShowOnDisabled`). The old capture row is removed.
- **`MainWindowViewModel.CaptureTip`:** when grayed it opens "Grayed because Hamlet is not listening". It always says it records the last half minute as `cw-<time>.wav` and `cw-<time>.txt`, and names the folder. It follows `IsDecoding`.
- **Tests:**
  - New `TheRecordButtonIsOnTheScreenTests` has three cases: at both sizes the button is effectively visible and its rectangle lies inside the window and the CW workspace (y 393); and a press over the training radio's audio writes the `.wav` (82,444 bytes for five seconds) and the `.txt`. Red before the change: there was no `Record`.
  - `TheCapturePressIsOnTheScreenTests` now looks for `Record`.
  - The hover registry names `RecordButton` in place of `I hear a station` and its hint mark.
- **The label `Record` supersedes the ruling of 2026-08-26**, at your request.

**Task 3: dropped, with its count.**
- **Why:** there is no single shared sender. Each test class builds its audio through the engine's `CwSignal.Generate`, which the training radio also uses.
- **The experiment:** as an uncommitted change, a 3 dB overshoot at every key-down and the 500 Hz filter on 600 were put into `CwSignal.Generate`, and every reading case was run. It was reverted.
- **12 tests turned red:**
  - the quiet dit (`CQ CQ DE N0CA DL N0CALL K`);
  - two stations (825 Hz stood 0);
  - the strength table at 10 dB (fit off `CQ DE RALR NO`, fit on `CQ CGE DE N0CALL N0NIALL K`);
  - the fading station (`CQ CQ DE N0CAE IL N0CALL K`);
  - the carrier 200 Hz away (`NOAM N0CALL N0CALL K`);
  - seven bulletin rows (fading and AGC 2, 3 and 4, with and without filter). These apply their own AGC and filter on top, so their overshoot was doubled and they were filtered twice.
- **One turned green:** the call at 8 dB.
- **150 printed reading lines changed.**

**Records:**
- HM-DEC-231 in `DECISIONS.md`.
- The `CLAUDE.md` index row.
- `PHASE_OUTCOME` (both copies) has `## UNIT 527 - STEP 12`.
- `PHASE_STATUS` (both copies) names 527.
- Version 1.13.211 to 1.13.212.

**Build and app line:** build 0 warnings, 0 errors. App carry-forward 277 of 278. The one loss, `ThePsk31ConversationCardTests.NoSlotClockUnderPsk31AndFt8AndFt4StillShowIt` at 1 ms ("You've caused dispatcher loop"), passed alone. The new test is not in that line.

## 2. What the owner should expect

- **Rebuild.**
- **A `Record` button beside `Clear` and `Copy`**, at the top of the CW terminal, always shown. It is live while Hamlet is listening and grayed when it isn't; hover it and it says why. Pressing it does what the old one did: it keeps the last half minute as a `.wav`, with what the radio was doing in a `.txt` beside it, in the capture folder, and puts the station on tonight's list.
- **What hid it:** it was at the bottom of the receive panel. The CW area has a fixed height, and the scope and the green light added above it since late September pushed it below that. On a smaller window it was off the screen entirely.

## 3. What you should see

**The button's visibility:**

| window | before (the old press) | now (`Record`) |
|---|---|---|
| 1600 × 900 | at y 832, below the receive widget's bottom (813) | at y 393, inside the window and the workspace, visible |
| 1100 × 800 | at y 832, outside the 800-pixel window | at y 393, inside the window and the workspace, visible |

- While not listening, `Record` is grayed and its hover begins "Grayed because Hamlet is not listening".
- While listening to the training radio, it writes `cw-…wav` and `cw-…txt` with the same stamp.

**Task 3's changed cases (experiment only, nothing changed in the tree):**

| case | now | under AGC and filter |
|---|---|---|
| the quiet dit | whole | `CQ CQ DE N0CA DL N0CALL K` |
| two stations | both stand | 825 Hz stands 0 (still reads the loud one) |
| strength table, 10 dB fit off | whole | `CQ DE RALR NO` |
| strength table, 10 dB fit on | whole | `CQ CGE DE N0CALL N0NIALL K` |
| fading station 24 → 10 → 24 | whole | `CQ CQ DE N0CAE IL N0CALL K` |
| carrier 200 Hz away | `KDEN0CALL N0CALL K` | `NOAM N0CALL N0CALL K` |
| AGC and fading bulletins (7 rows) | whole | broken, with their own AGC and filter doubled |
| the call at 8 dB | red | whole |

## 4. What's blocking us

1. **Task 3 dropped.** Making the radio's AGC and filter the default for every reading case needs one shared bench-audio helper for the test classes. The training radio must keep clean `CwSignal`. Under the radio's conditions, 12 reading tests would turn red today, seven of them only because they apply their own AGC and filter on top.
2. **Other widgets can overflow the same way.** Nothing checks that every control on the CW tab lies inside the window. `TheRecordButtonIsOnTheScreenTests` covers `Record` only.
3. **Pre-existing app reds** are outside the carry-forward line and were not touched:
   - the scope's tone-line tests;
   - two British spellings (`centre`);
   - `DecisionLogOrderTests`' gaps check.

### Asks still outstanding

- **Unit 522, 2026-10-01:** whether to switch shape-first on before its failures are fixed. Still off.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
