## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch
`main`. The prompt and `WORK_INSTRUCTIONS.md` both carry `PROJECT: Hamlet`, and the tree agrees.
Hamlet confirmed. Nothing in this report is evidence about the radio.

**Run by hand, outside the loop.**
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released the same way.
- Nothing was written to `RUN_LEDGER.md`, and nothing under `tools\arbiter\` was touched.
- No box was ticked in `PHASE_PLAN.md`.
- No recording, fixture, floor or telemetry file was read. The rows are the instruction's table.
- Nothing under `.run-unit\` was committed.

**The cause of the null, named.** `CwEnvelopeDetector` set the reading's pitch only on the hop a
mark was up (`up ? watched.Hz : NaN`). Unit 485's hold keeps keying true through the gaps between
marks, so most keying readings had no pitch. On a synthetic 625 Hz station at 23 WPM, 1118 of 1408
keying readings carried none. That is the `scopePitchHz: null` on the owner's rows.

**Two more things the tree showed.** The rows don't show what they seemed to:
- **The tab's rung was never fed null.** Since unit 486 it was fed the detector's watched bin
  (`WatchedHz`) while keying, which is always a number.
- **The rows' "mixing" was not where the decoder mixed.** The panel and the verdict row showed
  the tracker's own pitch (`Report.ToneHz` / `trackerHz`). The 584 was the tracker, not the
  decoder.

**The changes, file by file** (all in `ec9ef6d2`):
- **`src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs`.** The reading's pitch is the watched bin
  whenever keying is true, and NaN otherwise. Pitch and keying now go together.
- **`src/Hamlet.RadioEngine/Cw/CwDecoder.cs`.** A new `MixingHz` says where the decoder is
  actually mixing. The rung order is unchanged: lock, then detector, then tracker.
- **`src/Hamlet.App/ViewModels/MainWindowViewModel.cs`.**
  - A new `PitchForTheDecoder(reading)` feeds the rung the reading's pitch while keying and NaN
    otherwise.
  - The scope's "mixing" number now reads `MixingHz`.
- **`src/Hamlet.App/ViewModels/CwHearingViewModel.cs`.** The verdict row gains `mixingHz`, the
  real mixing pitch. The `trackerHz` doc no longer claims it is where the decoder mixes.
- **Tests.**
  - `ThePitchTheDetectorFoundReachesTheDecoderTests` (engine), four tests.
  - `TheDecoderIsFedTheDetectorsPitchTests` (app), three tests. One asserts that a reading
    keying at 625 feeds 625.
- **Records.**
  - `DECISIONS.md` has HM-DEC-193, "The detector's pitch is what the decoder mixes at".
  - Both `PHASE_OUTCOME.md` copies have `## UNIT 488 - STEP 12`.
  - Both `PHASE_STATUS.md` copies name 488.
  - Version 1.13.174 → 1.13.175.

**Watched failing first.** Before change one, on the same synthetic station:
- 1118 of 1408 keying readings had no pitch.
- The decoder fell back to the tracker in the gaps: 305 keyed chunks were not at 625.

**Section 3's green is not met, and nothing was tuned.** Every keying reading now carries a pitch,
but 1021 of 1408 carry 575 or 675 Hz, not 625:

| pitch carried | readings |
|---|---|
| 575 Hz | 536 |
| 600 Hz | 81 |
| 625 Hz | 387 |
| 650 Hz | 2 |
| 675 Hz | 400 |
| other | 2 |

- **Why:** on those hops the 625 Hz bin calls no bars of its own. Its gaps measure about −20 dB,
  where the bins 50 Hz either side measure −42. So the bars are called in the shoulders of the
  tone's lobe, and the watched bin is a shoulder.
- **An attempt, reverted.** Choosing the loudest keying bin in the lobe was tried and moved almost
  nothing (1021 → 1001), because 625 is not keying on those hops. Making it key is a detector
  decision.
- `ThatPitchIsTheStationsOwn` is committed red on purpose, to state the defect.

**Verification.**
- The build: 0 warnings, 0 errors.
- The app carry-forward line: 278 of 278.
- The app scope, transcript and layout types: 81 of 82. `TheTopRowTests` failed once in the long
  run and passed alone, 15 of 15.
- The engine detector and gate types: 19 of 22. The three reds:
  - `ThatPitchIsTheStationsOwn`, the named red above.
  - `AMarkIsTheEnvelopeOverAThresholdTests`' 10 and 15 dB cases, at the same counts as before (40
    and 208).

## 2. What the owner should expect

1. Rebuild.
2. On a station, the panel's *mixing* number is now where the decoder really mixes, and it stays
   put through the gaps between letters.
3. **The *tone* and *mixing* numbers will often not be the same number, and will sit 50 Hz off
   the station.** That is the problem this unit found, now visible instead of hidden behind the
   tracker's pitch.
4. **Expect the same near-silence as last night** on a strong, clean station. The decoder has
   been pointed at the detector's bin since unit 486, and that bin is usually beside the station,
   not on it.
5. The next verdict row carries `mixingHz` and a `scopePitchHz` that is never null while the bars
   say keying.

## 3. What you should see

**Would the station of 23:38:34 have been decoded with the pitch carried? No.** A synthetic 625 Hz,
23 WPM station, driven through the detector, the gate and the block rule, with nothing tuned:

| the rung fed | settled text | characters |
|---|---|---|
| sent | `CQ CQ DE N0CALL N0CALL K` | 20 |
| nothing (what the rows seemed to say) | `CQQ   DEN0CAL 0L K` | 13 |
| the carried pitch (this unit; what the tab has fed since unit 486) | `RE    N  D   K` | 5 |
| 625 Hz while keying (the station's own bin) | `RQ DEN0CAL 0L K` | 12 |

- **Before and after this unit, the tab reads the same 5 characters**, because the tab already fed
  the watched bin, which now equals the carried pitch.
- **The next thing in the way:** the detector calls the station's bars 50 Hz to one side.
- Fed the station's own pitch, the same decoder reads 12 characters.

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **The station's own bin does not call bars on two hops in three.** 625 Hz reads gaps of about
  −20 dB while the shoulders read −42, so the watched pitch is 575 or 675. That is the next unit's
  cause to find, in the detector; `ThatPitchIsTheStationsOwn` is the red test waiting for it.
- **Fed the station's own pitch, the decoder still misses letters** (`RQ DEN0CAL 0L K` against
  the sent call). That is the decoder's own reading, untouched here as the instruction required.
- **Pre-existing reds, not this unit's:**
  - `HowMuchTheApplicationSaysTests` (the CW tab at 585 against 550).
  - `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten`.
  - `AMarkIsTheEnvelopeOverAThresholdTests`' two cases.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is ever
  revised, at the cost of seconds of lag. It is waiting on the owner; no change sits in the tree.
