## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch
`main`. The prompt carries `PROJECT: Hamlet`, and all five of section 0's checks hold. Hamlet
confirmed. Nothing in this report is evidence about the radio.

**Run by hand, outside the loop.**
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released the same way.
- Nothing was written to `RUN_LEDGER.md`, and nothing under `tools\arbiter\` was touched.
- No box was ticked; R110 was appended to both `PHASE_PLAN.md` copies in the owner's words.
- No recording, fixture, floor or telemetry was read.
- Nothing under `.run-unit\` was committed, and nothing was written to the radio.

**Task 0.** Unit 501 is in the tree: `FarnsworthAndLoneLettersTests` is present and green.

**The changes, file by file** (all in `6f1d3d0c`):
- **`src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs` - one shape, one score.** Every bar that
  passes the five existing tests gets one score, and becomes a mark only at `ShapeThreshold`,
  0.25, or more. The five tests stay in the tree and stay on; nothing is loosened. There is a
  switch, `MarksNeedShape`, so the difference can be counted.
- **`src/Hamlet.RadioEngine/Cw/CwMark.cs`:** each mark carries its `CwMarkShape`, the five
  properties and their product, so the run reader, the scope and anything downstream can see it.
- **The scope's hover** (`CwEnvelopeDetector.History`, `CwTrainingGraph`, `CwScopeControl.BarTip`):
  - The hops under a called mark carry its score, and a block's hover says
    `dah, 150 ms, shape 0.87 of 1`.
  - A block the detector drew but did not hand out as a mark says `not handed out as a mark`.
  - Two tests that pinned the old hover wording were updated to the new words.
- **Tests:**
  - `TheShapeOfAKeyedToneTests`, new: the noise table, the score distributions, the scope's hops,
    and the weak station.
  - `TheBlockSaysItsShapeTests`, new: the hover's words.
  - Unit 498's noise-count helper now switches the shape off, so its counts stay what it measured.
- **Records:**
  - `## UNIT 502 - STEP 12` is in both outcome copies, and both status copies name 502.
  - Version 1.13.188 → 1.13.189.
  - R110 is in both plans.
  - HM-DEC-206, "A mark is judged by its whole shape, not by crossing five lines", is in
    `DECISIONS.md`.

**How the score is built.** Each property is a distance from the ideal of a keyed tone, from 0 to 1:
- **Flatness:** 1 minus the top's RMS wander from its own mean, divided by the flatness tolerance at
  the bar's contrast (R93).
  - The top excludes the one hop of its own rise or fall that the window can leave at an end.
  - First built without that trim, the real dahs scored 0: one hop 12 dB under the top made a
    flat dah read as unflat. That hop is the edge, which the edge score judges.
- **Edges:** for the rise and for the fall, 2 over the hops taken to reach half amplitude, capped
  at 1. Two hops is the window's own spread. The two edges are multiplied.
- **Narrowness:** how far the bar's bin stands above the louder of the bins 300 Hz either side,
  over 15 dB, capped at 1.
- **Contrast:** its level over its gaps, over 15 dB, capped at 1.
- **Length:** 1 up to a 5 WPM dah (720 ms), and that length over its own beyond it.
  - The detector knows no sender, so every length from the shortest bar up to that is a plausible
    dit or dah. On every case here length scored 1: it separates nothing, and the report says so
    rather than pretend it does.
- **Where 15 dB comes from:** the work instruction's statement that a real mark stands 15 to
  30 dB above the band.
- **Why a product:** a key down is all five at once, and a bar that is bad on one is not the shape.
  A sum or a mean would let four good properties carry a bad one. Each property is weighed the
  same, and none was weighted by what makes a test pass.

**The threshold and its reason.**
- **The floor:** the lowest score of any real mark is 0.621 on the clean call and 0.334 on the same
  call 12 dB weaker.
- **The threshold is 0.25**, a quarter under 0.334. That leaves room for a real mark rougher than
  a synthetic one.
- It was not set by looking at noise. Section 3 shows the two populations overlap, and the
  threshold sits under the overlap, not in it.

**Watched failing first.** With the threshold at nought, which is HEAD's behaviour, the noise test
was red: the shape turned away no bar, 146 in and 146 out.

**Verification.**
- **The build:** 0 warnings, 0 errors.
- **Shape, reader, speed and Farnsworth tests: 37 of 37.**
- **The decoder's run-path tests: 10 of 12.** The reds are `AMarkIsTheEnvelopeOverAThresholdTests`'
  two cases, unchanged from before.
- **The app carry-forward line: 278 of 278.**

## 2. What the owner should expect

1. Rebuild.
2. **Fewer false marks still.** A mark now has to look like a keyed tone as a whole, not just clear
   five separate bars. On loud noise, less than half as many marks reach the reader: about 2 a
   second where there were 5.
3. **A real station, strong or weak, is unaffected.** Its marks score well inside the shape. The
   synthetic call at about 22 dB scores 0.62 to 0.94, and at about 10 dB 0.33 to 0.78, against a
   threshold of 0.25.
4. **Hover over a block on the CW tab's scope** and it says the mark's shape score, or that the
   block was not handed out as a mark.
5. **If real letters go missing, that is the threshold.** Section 3's score table names the floor.

## 3. What you should see

**Thirty seconds of loud noise:**

| passing the older tests | with edges | narrow | inside the shape |
|---|---|---|---|
| 1452 | 1069 | 146 | **66** |

- **Marks handed out a second:** 4.9 before, 2.2 after.
- **Three minutes of noise:** 115 marks in the last minute before, 54 after.
- Both noise tests still print nothing.

**The scores:**

| | marks | lowest | median | highest |
|---|---|---|---|---|
| clean call, about 22 dB | 65 | **0.621** | 0.874 | 0.944 |
| weak call, about 10 dB | 61 | **0.334** | 0.652 | 0.776 |
| loud noise passing all five | 146 | 0.000 | 0.229 | **0.800** |

- **They overlap.** 42 of the 146 noise bars score at or above the lowest real mark, 0.334. The
  highest noise bar, 0.800, is above most of the clean call's marks.
- The threshold is **0.25**, under the overlap.
- The noise bars that score high are all 20 to 30 ms long, the shortest a bar can be. The real
  marks are all 40 ms or longer.

**The cases.** Every one reads exactly as at HEAD:
- The calls at 5, 10, 18 and 35 WPM, and the 10-to-20 WPM sender, read whole.
- Farnsworth at 5, 10 and 13 WPM overall, and Farnsworth `TEST DE W1AW K`, read whole.
- The clean call, the call with bursts, the two-station case and the stray dit each read
  `CQ CQ DE N0CALL N0CALL K`.
- `TEST DE W1AW K` and `DE DE` read whole.
- The lone dit, the lone dah, `T E T T E` alone and the call followed by `T E T T E` print nothing
  beyond the call.
- Both noise tests print nothing.

**The weak station, about 10 dB over the noise:**
- It keeps all 61 of its marks with the shape on.
- It reads `CG N EQ DE N0CALL NT ON EAE IL A` before and after. That reading predates this unit:
  the score takes nothing from it and does not repair it.

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **Noise still overlaps the shape.** 42 of the noise bars that passed the five tests score like
  real marks. The score halves what gets through; it does not end it.
- **Length separates nothing without a sender.** The noise that scores high is at the shortest bar
  length, but the detector cannot call that too short without knowing the sender's dit. The reader
  knows it, and a length test against the sender's own dit would sit there.
- **A weak station, about 10 dB over the noise, still reads wrongly**:
  `CG N EQ DE N0CALL NT ON EAE IL A`, as at HEAD.
- **The detector crash reported by unit 501 is still in the tree.** `CwEnvelopeDetector.cs` in
  `CallMarks` can read the hop before hop zero while it reaches a mark back to where its tone rose,
  and throws `IndexOutOfRangeException` early in the audio. It is a one-line guard for the owner to
  approve.
- **The verdict row does not yet write the score.** The mark carries it, so the row can when asked.
- **The five scores, the product, the 15 dB ideal, the threshold and its margin are the author's**,
  and overrulable.
- **Pre-existing reds, not this unit's:**
  - `VoiceTests`' British spelling.
  - `TheVerdictCarriesTheScopeTests`' four cases, red on HEAD too, checked in a clean worktree.
  - `HowMuchTheApplicationSaysTests`.
  - `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten`.
  - `TheLicenceCardAnswersForTheTabAndNotForMorseAlways`.
  - `NoBandPillIsOnTheGreenZoneAndTheMapTookTheirWidth`.
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
