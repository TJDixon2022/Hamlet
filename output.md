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

**What the detector already exposed, and what had to be added.**
- **Already there:**
  - Every bin called its own bars, and paired two only when they were within 1.5 dB of each
    other.
  - Each hop's level was kept for four seconds.
  - Unit 487's `BlocksBetween` counted the watched bin's blocks.
- **What the decoder got:** a count and a pitch to mix at. It never got a mark's pitch or level.
- **Added:** every completed bar in every bin becomes one `CwMark` with its pitch, level in dB,
  contrast and times.
  - The pitch is the peak of the tone's lobe: from the bar's bin, step to the neighbor with the
    higher mean level over the mark's own hops until none is higher.
  - So a tone that lights bins 200 Hz either side is one mark at its own pitch, not sixteen.

**The changes, file by file** (all in `af6687e0`):
- **`src/Hamlet.RadioEngine/Cw/CwMark.cs`**, new: the mark record, and a batch of marks with the
  detector's clock.
- **`src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs`:** calls the marks and hands them out through
  `MarksSince(sequence)`. What it already did is unchanged.
- **`src/Hamlet.RadioEngine/Cw/CwRunReader.cs`**, new: the second, simpler reader.
  - **A run** is consecutive marks within one bin of pitch and within the level tolerance of the
    run's own mean.
  - **A run ends** at a gap longer than the sender's character gap.
  - **A mark that breaks the agreement** starts its own run and is never folded in, while the run
    it didn't join goes on.
  - **The letter:** dits and dahs split at the geometric mean of the sender's own short and long
    marks. A word gap is placed at the geometric mean of three dits and seven.
  - **One sender is printed:** the first to make two runs, the one with the most marks if several
    have. It is held until it has been silent for a second.
  - **Nothing printed is taken back.**
  - Each letter is also raised with the marks it was read from.
- **`src/Hamlet.RadioEngine/Cw/CwDecoder.cs`:**
  - `DetectorMarks` takes the marks. With `ReadsRuns` on, which is the default, only what the runs
    read reaches the terminal and the scope.
  - The timing-only path stays behind `ReadsRuns`, unchanged.
  - While transmitting, or in a digital mode, the marks are taken and dropped (HM-DEC-147).
  - **Unit 489's three switches stay off as it left them.** The keying gate and the block rule
    aren't needed on this path, since a letter appears only where its run exists.
  - The lattice, speed grid, unit estimator and emission gate were not touched.
- **`src/Hamlet.App/ViewModels/MainWindowViewModel.cs`** gives the decoder the detector's marks.
- **`src/Hamlet.App/ViewModels/CwHearingViewModel.cs`:** the scope's hover says the terminal and
  the scope read the same marks. Unit 489's "the two can disagree" is no longer true.
- **Records.**
  - R103 is in both `PHASE_PLAN.md` copies, in the owner's words, and `DECISIONS.md` has
    HM-DEC-195.
  - Both outcome and status copies name 490.
  - Version 1.13.176 → 1.13.177.

**The two tolerances, the author's, chosen before any result and not tuned after:**
- **Pitch: one bin, 25 Hz.** A human's keying holds one pitch, and the detector puts each mark on
  the bin at its lobe's peak, so a tone between two bins lands on either one. One bin is that
  rounding.
- **Level: twice the detector's own flatness tolerance at the run's contrast** (R93: at least
  1.5 dB, more for weaker signals). A sender holds one level, and every hop of a mark sits within
  one tolerance of the mark's mean. A mark's mean and the run's mean, each measured that way, can
  differ by twice it.

**The other rules, also the author's:**
- **Dits and dahs are two kinds only when the widest step between sorted lengths is at least 2 to
  1.** Every fist measured here sends a dah at least 2.7 times a dit (HM-DEC-144, HM-DEC-145).
- **A sender is printed after two runs.** One agreeing run can be a lone blip.

**Watched failing first.** The reader was first built to print every run. Against that version:
- **Case 2 was red:** 3 of the marks under its letters weren't the call's.
- **Case 3 was red:** 21 weren't.
- **With the one-sender rule, both are 0.**

**Case 3's answering station was shortened to `TEST DE W1AW K`, before the one-sender rule was
run.** That way it ends while the call is still going, and the case asks only what happens while
both key at once.

**Verification.**
- **The build:** 0 warnings, 0 errors.
- **The app carry-forward line: 276 of 278.** The two failures are dispatcher-loop losses at 1 ms,
  `TheWindowHoldsBelowItsMinimumTests` and `TheCarrierHoldsTheButtonsTests`, and both pass alone (3
  of 3 and 8 of 8).
- **The app scope, layout and voice types: 86 of 87.** The red is `VoiceTests`' British spelling,
  "centre" in two lines from 2026-09-26, which is not from this unit.
- **The engine run, gate and detector types: 23 of 28.** The five reds:
  - `TheCallReadsWholeThroughTheBlips` and `TheStationPrintedReadsWhole`, both new and red on
    purpose (section 4 says why).
  - `ThatPitchIsTheStationsOwn`.
  - `AMarkIsTheEnvelopeOverAThresholdTests`' two cases, at the same counts as before (40 and 208).

## 2. What the owner should expect

1. Rebuild.
2. On a station, the letters in the terminal are the letters over the blocks, because both now come
   from the same run of marks.
3. Noise between letters no longer becomes letters, because it doesn't match the station's pitch or
   loudness.
4. Two stations at once read as one station, not as a mixture.
5. **What will look wrong but is expected.** The terminal waits until a letter's run has ended, so
   it runs about a letter behind, and the tip no longer changes while you watch.
6. **What is still wrong.** Right beside a burst of noise, or where a second station keys close by,
   the detector can miss the station's own marks. You then get a wrong or missing letter there
   rather than the noise's letter.

## 3. What you should see

Three synthetic cases. The old path is the decoder as unit 489 left it; the new path is the same
decoder reading runs.

| case | sent | old path | new path |
|---|---|---|---|
| 1. clean call, one pitch, one level | `CQ CQ DE N0CALL N0CALL K` | `CQ CQ DE N0CALL N0CALL K` | `CQ CQ DE N0CALL N0CALL K` |
| 2. the call with bursts between the letters | `CQ CQ DE N0CALL N0CALL K` | `CQ■DEN0CALL N0CALL K` | `CQ CT A K EE N0CAAEI D N0CALL K` |
| 3. the call at 625 Hz with `TEST DE W1AW K` at 825 Hz | both | `CQ CQ DE N0W DL N0CALL K` | `CGE CEN K EE N0 FALL N0CALL K` |

- **Case 2:** every one of the 65 marks the new path's letters were read from is the call's.
  - The bursts sit 8 dB below the station, at 550 to 675 Hz, 40 ms long, in every gap between
    letters. None of them became a letter.
  - The wrong letters come from the call's own marks going missing beside each burst.
- **Case 3:** the new path prints the call and never the answer.
  - The call is louder, faster and started first, so it made two runs first and holds the terminal
    while it keeps sending.
  - Every mark its letters were read from is the call's, where printing every run took in 21 of
    the answer's.

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **The detector loses a station's marks beside a burst or a second station.**
  - A burst in a gap lights the station's own bin.
  - The gap then fails the detector's check that bars stand clear of their gaps' wander, so the
    bars on either side are never paired.
  - Measured on case 2: none of the first CQ's four marks were called.
  - `TheCallReadsWholeThroughTheBlips` and `TheStationPrintedReadsWhole` are the reds waiting for
    it. The detector's pairing wasn't this unit's to change.
- **The panel's "decoding at N Hz" is the old path's mixing pitch.** The terminal now reads runs at
  the sender's own pitch, so that number no longer describes what the terminal reads. The next
  unit should show the printed sender's pitch there.
- **The first station to make two runs holds the terminal until it is silent for a second.** A
  louder station arriving later waits.
- **Pre-existing reds, not this unit's:**
  - `VoiceTests`' British spelling.
  - `HowMuchTheApplicationSaysTests`.
  - `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten`.
  - `ThatPitchIsTheStationsOwn`.
  - `AMarkIsTheEnvelopeOverAThresholdTests`' two cases.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is ever
  revised, at the cost of seconds of lag. On the run path the terminal now does show only settled
  text. The ask is still the owner's for the timing-only path, and no change for it sits in the
  tree.
