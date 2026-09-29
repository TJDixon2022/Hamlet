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

**Task 0.** Units 496 and 497 are in the tree: `ThatPitchIsTheStationsOwn` green (0 of 1404) and
`MarksNeedEdges` present.

**The changes, file by file** (all in `c3a576a3`):
- **`src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs` - a mark is narrow.**
  - **The rule:** a completed bar is handed out as a mark only if, over the mark's own hops, its
    bin stands at least `NarrowDepthDb` (6 dB) above the bins `NarrowBins` (12 bins, 300 Hz) either
    side.
  - **A side off the bins is not read**, as at the edge of a narrow passband.
  - **The switch:** `MarksNeedNarrowness`, on, so the difference can be counted.
  - **What it gates:** only the marks. The pairing, the keying verdict, the light and the scope see
    every bar as before.
- **`src/Hamlet.RadioEngine/Cw/CwRunReader.cs` - a lone letter waits to be confirmed.**
  - **The rule:** a letter of one mark, E or T, is banked until the same sender sends another
    letter within `ConfirmSeconds`. It is then printed in its own place. If nothing follows inside
    that time, it is dropped and never printed.
  - **Unprinted runs are split again:** when a sender is printed, its not-yet-printed runs are
    re-split at the character gap it has shown by then.
- **`tests/Hamlet.RadioEngine.Tests/Cw/ACharacterIsARunOfMarksThatAgreeTests.cs`:**
  - Three new tests: the narrowness count, `TEST DE W1AW K`, and the stray dit after the call.
  - The mark counter can switch narrowness off.
- **Records.**
  - R108 is in both `PHASE_PLAN.md` copies, and `DECISIONS.md` has HM-DEC-202.
  - Both outcome copies have `## UNIT 498 - STEP 12`, and both status copies name 498.
  - Version 1.13.184 → 1.13.185.
- **Not loosened:** the flatness tolerance's value, the shortest bar, the pairing, the wander check,
  unit 491's, 492's, 496's and 497's rules.

**The narrowness distance and depth - the author's, overrulable, not fitted or tuned.**
- **300 Hz either side.** The ten millisecond window's main lobe reaches 200 Hz either side of a
  tone. A keyed tone's necessary bandwidth at the 45 WPM ceiling of `CW_SPEC.md` §7 is about
  190 Hz, 95 either side. 300 Hz is past both, so what is read there is the band beside the tone
  and not the tone.
- **Both sides must clear.** A second station 200 Hz away sits 100 Hz from the probe point there,
  down its own lobe. In the two-station case the call keeps all 65 of its marks.
- **6 dB.** Half amplitude, as for unit 497's edges.

**The confirmation window - the author's, overrulable.**
- **Twice the sender's word-gap threshold**, about nine dits, holds a lone letter inside a word and
  one at the end of a word, like the E of DE.
- **Measured on the gaps inside the sender's letters, not on its marks.** It was first built on the
  marks' dit and was too tight on real text:
  - On a synthetic 23 WPM TEST the detector reads dits at 35 to 45 ms against 52, and the word gap
    at 370 ms against 365.
  - So a window built on the marks came out at about 385 ms and dropped TEST's last T.
  - The gap inside a letter is one dit plus the same smear, the unit the gaps between letters are
    measured in. Built on it, the window holds.
- **Also a wait for the follower:** a banked letter waits for the window plus the sender's longest
  mark and the calling lag, because the following letter's first mark reaches the reader only once
  it has ended.

**Unit 493's rule is gone.** Once a sender was printed, every one-mark letter it produced printed
with it. That is how a stray mark at the sender's pitch after it stopped became an `E`. It is
replaced by the banking.

**The flat middle, measured rather than assumed.** At HEAD a bar is already a run in which every
hop sits within the flatness tolerance of the whole run's mean, and a run must be at least four hops
(25 ms of audio) to be a bar at all. So a mark's flat middle is already at least the shortest bar.
It is not "two hops inside a longer run", and nothing needed tightening there.

**Watched failing first.** The reader without the banking, on the new cases:
- **`TEST DE W1AW K`** read `NST DE W1AW K`: T and E joined into N before the sender's dit was
  known.
- **The call, silence and one stray dit** read `CQ CQ DE N0CALL N0CALL K E`.

**Verification.**
- The build: 0 warnings, 0 errors.
- **The app carry-forward line: 274 of 278.** The four failures ran in 1 ms each -
  `TheCarrierHoldsTheButtonsTests` and three `TheFavoritesAreChipsTests` names - and pass alone
  (8 of 8, 4 of 4).
- **The app one-truth, layout, voice and W1AW types: 96 of 99.** Two top-row names failed in the
  long run and pass alone (15 of 15). The British spelling red is from 2026-09-26.
- **The engine run, gate and detector types: 39 of 41.** The reds are
  `AMarkIsTheEnvelopeOverAThresholdTests`' two cases, at 70 and 208, unchanged.

## 2. What the owner should expect

1. Rebuild.
2. **The stray `T`s and `E`s should be gone.**
   - A single-element letter now has to be confirmed by another letter from the same sender,
     within about two word gaps.
   - A mark now has to be narrow, standing clear of the band on either side, as well as flat and
     sharp-edged.
3. `TEST` still reads `TEST`.
4. A real station is unaffected: its marks are narrow, and its letters confirm each other.
5. **A lone E or T at the very end of a transmission is not printed**, because nothing follows to
   confirm it. That is the price of the banking.
6. **If real letters go missing, that is the one thing to report back.**

## 3. What you should see

**The noise table** (thirty seconds of loud noise):

| passing the older tests | with edges (unit 497) | of those, narrow |
|---|---|---|
| 1452 | 1069 | **146** |

- **Marks handed out on noise:** about 36 a second before, about 5 a second after.
- **Three minutes of noise:** 341 marks in the last minute before, 115 after.
- **Both noise tests stay green:** nothing printed.

**The cases**, terminal and scroll identical:

| case | before (HEAD) | after |
|---|---|---|
| clean call | `CQ CQ DE N0CALL N0CALL K` | `CQ CQ DE N0CALL N0CALL K` |
| call with bursts in every gap | `CQ CQ DE N0CALL N0CALL K` | `CQ CQ DE N0CALL N0CALL K` |
| the call plus a second station 200 Hz away | `CQ CQ DE N0CALL N0CALL K` | `CQ CQ DE N0CALL N0CALL K` |
| a lone dit | nothing | nothing |
| a lone dah | nothing | nothing |
| `TEST DE W1AW K`, new | `NST DE W1AW K` | `TEST DE W1AW K` |
| the call, silence, one stray dit, new | `CQ CQ DE N0CALL N0CALL K E` | `CQ CQ DE N0CALL N0CALL K` |

**Every mark the printed letters were read from is the call's** - 65 of 65 - in the two-station case
and the bursts case.

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **A lone E or T that ends a transmission is never printed.** A sign-off ending in `E`, or a single
  `T` sent alone, will not appear.
- **A banked letter prints only once the next letter arrives.** It shows about a letter late, at
  once with the letter that confirms it.
- **The narrowness test needs 300 Hz of bins on at least one side.** In a passband narrower than
  600 Hz it may read one side only, and in one narrower than 300 Hz it reads neither and turns
  nothing away.
- **Pre-existing reds, not this unit's:**
  - `VoiceTests`' British spelling.
  - `HowMuchTheApplicationSaysTests` (the CW tab at 653 against 550).
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
