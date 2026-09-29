## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch
`main`. The prompt carries `PROJECT: Hamlet`, and all five of section 0's checks hold. Hamlet
confirmed. Nothing in this report is evidence about the radio.

**Run by hand, outside the loop.**
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released the same way.
- Nothing was written to `RUN_LEDGER.md`, and nothing under `tools\arbiter\` was touched.
- No box was ticked; `PHASE_PLAN.md` has 69 before and after.
- No recording, fixture, floor or telemetry was read.
- Nothing under `.run-unit\` was committed.

**Measured at HEAD, before any change: the marks were already on the station's bin, and the
watched bin was not.** Synthetic stations at four pitches:

| tone | detector's reading ("tone heard") | loud marks' pitch | reader's printing pitch |
|---|---|---|---|
| 625 Hz | 575 ×539, 675 ×400, 625 ×384 | 625 ×65 of 66 | 625 |
| 640 Hz | 650 ×705, 600 ×504 | 650 ×65 | 650 |
| 612 Hz | 650 ×512, 575 ×449, 600 ×438 | 600 ×66 | 600 |

- **Unit 490's walk to the lobe's top reaches the peak and does not stop early.** It lands on the
  bin nearest the tone.
- **The run reader was already grouping and printing at the station's bin.** It read the 625 Hz
  call whole.
- **The wrong number was the detector's watched bin**, the one with the most bars. It gives the
  scope its blocks and the panel its "tone N heard", and on the shoulders it wins on bar count.
- **The owner's `tone 600 heard · decoding at 666` is both halves.** 600 is the watched shoulder.
  666 is the reader's *mean* over marks landing on 650 and 675, for a tone that sits between them.

**The changes, file by file** (all in `a71853e5`):
- **`src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs`:**
  - **`StationBin`, one estimator for the station's bin.** From the apex the walk reaches, a
    parabola through the levels in dB two bins either side places the top of the lobe, and the
    nearest bin is the station's.
  - **Each mark's pitch** uses it.
  - **The reading's pitch** ("tone heard") uses it for the watched bin's latest bar, held through
    the gaps while keying. The watched bin still gives the scope its blocks.
- **`src/Hamlet.RadioEngine/Cw/CwRunReader.cs`:** the printing pitch ("decoding at") is rounded to
  the bin grid, so a mean over marks either side of a tone can't name a pitch that is no bin.
- **`tests/Hamlet.RadioEngine.Tests/Cw/ThePitchTheDetectorFoundReachesTheDecoderTests.cs`:**
  `ThatPitchIsTheStationsOwn` now judges readings from the end of the station's first pair. That
  is C's dah, gap and dit, five units at 23 WPM, three seconds in. Readings before that are
  printed, not counted, with the reason in its remarks.
- **Records.**
  - `DECISIONS.md` has HM-DEC-200.
  - Both outcome copies have `## UNIT 496 - STEP 12`, and both status copies name 496.
  - Version 1.13.182 → 1.13.183.
- **Not touched:** the flatness tolerance, the shortest bar, the pairing agreement and the wander
  check.

**How the peak is found, and what "at the same time" means - the author's, overrulable.**
- **Where the parabola's points sit.** A ten millisecond window's lobe is about 400 Hz wide, so its
  top is nearly flat and noise can tip the walk a bin either way. So the top is placed by a parabola
  through the levels two bins (50 Hz) either side of the apex. There the lobe has fallen a decibel
  or so, and the curve is well defined.
- **Why not wider.** A second station 200 Hz away adds almost nothing at ±50 Hz. A wider window
  would take in its lobe.
- **"At the same time" means over the same hops.** Every level compared is the mean over the mark's
  own span, so a bin is judged by what it did while that key was down. There is no separate
  tolerance.
- Nothing was fitted to a recording or tuned after a result.

**Two tries that measured nothing and were taken back:**
- a fallback for when the watched bin has no bar yet;
- choosing the loudest keying bin when no bin wins the usual choice.

Both were aimed at the one reading at 3.230 s. That reading turned out to be the detector's hold on
a noise pair at 1550 Hz, before the station could be keying. That is a real pitch for what the
detector heard, and neither change moved it.

**Verification.**
- The build: 0 warnings, 0 errors.
- **The app carry-forward line: 277 of 278.** The one failure ran in 1 ms,
  `TheTestsStayOffTheNetworkTests`, and passes alone (5 of 5).
- **The app one-truth, scope, layout and voice types: 96 of 97.** The red is the British spelling
  from 2026-09-26, not this unit.
- **The engine run, gate and detector types: 32 of 34.** The reds are
  `AMarkIsTheEnvelopeOverAThresholdTests`' two cases, at 70 and 208, unchanged.

## 2. What the owner should expect

1. Rebuild.
2. On a station, the panel's two numbers - **tone N heard** and **decoding at N** - should now be
   the same number.
   - A station between two of Hamlet's 25 Hz steps shows the nearer step in both: W1AW near 660
     should read 650 in both.
3. W1AW should read.
4. **If the two numbers are still different on a station**, that pair of numbers is the fault, and
   it is the one thing to report back.

## 3. What you should see

**`ThatPitchIsTheStationsOwn`:**

| | before | after |
|---|---|---|
| keying readings off 625 Hz | 1024 of 1408 | 0 of 1404 |

- **A 640 Hz tone** now reads 650 on every keying reading, where it was 600 or 650.
- **A 660 Hz tone** reads 650.
- **A 612 Hz tone**, almost midway between two steps, reads 600 on 1354 and 625 on 53.

**The five cases** - identical in the terminal and the scroll:

| case | terminal and scroll |
|---|---|
| clean call | `CQ CQ DE N0CALL N0CALL K` |
| call with bursts in every gap | `CQ CQ DE N0CALL N0CALL K` |
| loud noise alone (and three minutes of noise) | nothing |
| a lone dit; a lone dah | nothing |
| the call at 625 Hz plus `TEST DE W1AW K` at 825 Hz | `CQ CQ DE N0CALL N0CALL K` |

**The two-station case stays two stations.** It prints the 625 Hz call. All 65 of the call's marks
are called, and every mark its letters were read from is the call's; none are the answer's.

**The 625 Hz station's text, before and after:** the run reader printed
`CQ CQ DE N0CALL N0CALL K` before this change and prints it after. Unit 488's 5 against 12
characters came from the timing-only path, fed a shoulder pitch or the station's own. That path no
longer reaches the screen, and its figures are unchanged (13, 5 and 12).

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **The scope's trace and blocks still come from the watched bin**, which can be a shoulder. Its
  bars keep the same timing as the station's, so the blocks line up, but the trace's level is the
  shoulder's.
- **The detector's hold can report a noise pair's pitch** for a hop before a station is found: once
  at 3.230 s on the synthetic call, at 1550 Hz.
- **Pre-existing reds, not this unit's:**
  - `VoiceTests`' British spelling.
  - `HowMuchTheApplicationSaysTests` (the CW tab at 597 against 550).
  - `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten`.
  - `AMarkIsTheEnvelopeOverAThresholdTests`' two cases.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is ever
  revised, at the cost of seconds of lag. On the run path, now the only path to the screen, the
  terminal shows only settled text. The ask stands only for the timing-only path, which no longer
  reaches the screen; no change for it sits in the tree.
