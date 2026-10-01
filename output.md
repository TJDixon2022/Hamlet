```
UNIT: 516 - complete - 2026-09-30
UNIT GOAL: fit the rectangle, do not check it
NUMBER: the bench's reading floor, the call read whole: 10 dB over the noise (was 14 dB)
```

## 1. What Claude did

Claude Code ran on the development machine, on branch `main`. The prompt claimed `PROJECT: Hamlet`, and the tree agrees: `PROJECT_CARD.md` says Hamlet, the solution is `Hamlet.sln`, and the root is `C:\Source\HamLet`. Nothing in this report is evidence about the radio. Unit 516 and HM-DEC-220 were both free.

**What this session did and did not touch:**
- It took SESSION.lock through `tools\arbiter\lock.bat take` and released it at the end.
- It wrote nothing to `RUN_LEDGER.md`, touched nothing under `tools\arbiter\`, and ticked no box. R115 was appended to both plans in your words.
- Nothing keys, transmits or writes to the radio.
- It read no recording, fixture, floor or telemetry. All audio was generated in the tests.

The change is commit `f917267a`.

**`CwEnvelopeDetector`: the fit.** This is a second way for a stretch to become a candidate mark. The per-hop bar tests are unchanged.
- Each hop, in every bin, the detector fits a two-level rectangle to the stretch that ended one gap-width ago:
  - the top is the stretch's mean;
  - the floor is the mean of a shortest bar's width (25 ms) of gap on each side.
- **Every hop votes.** Running sums per bin make each fit cost a few subtractions.
- **The score** is the share of the variance the rectangle explains, from 0 to 1, with no dB figure in it.
- **Across the lobe.** The score sums the fit over the bin and the bins 100 Hz either side, each with its own top and floor.
- **Where to look:**
  - if a sender stands, at its dit and dah lengths and a fifth either side (`CwPatternGate.StandingLengths`, a read-only addition);
  - otherwise, at a sweep from 25 ms to a 5 WPM dah in steps of a quarter.
- **Candidate.** A bin's best length, at its best end in time, becomes a candidate if it scores 0.7 or more. It goes to the unchanged pattern gate as a `CwMark` with `FitScore` set (`Fitted` = true).

**The threshold is 0.7.** The lowest score any real mark of the clean call earned at 24 dB was 0.835, so 0.7 sits 0.135 under it. The highest score any dit- or dah-length stretch of noise earned was 0.239.

**Four rules I added beyond the order's text.** The measurement forced them. Without them the fit turned eight strong cases red, by inventing marks in signals the per-hop path already reads whole. None of them loosens a per-hop test, the gate or the reader.
1. **The rectangle must be its bin's.** If a lobe bin 100 Hz away stands higher over its own gaps, the rectangle belongs to another station and the score is 0. This came from the two-station cases.
2. **The step must be taller than the 1.5 dB flatness tolerance.** The score has no scale, so on a noiseless tone half a decibel of ripple scored 0.75. This is the one place a dB figure enters, and it is the existing tolerance, not a new number.
3. **The bin must be the highest within 300 Hz** over the same span (the reach the narrowness test uses). A strong tone's side lobes 225 Hz out, 31 dB down, are rectangles too.
4. **The per-hop path goes first.** A fitted rectangle is held until the per-hop path has had its turn at that stretch. It is dropped if a mark already called covers half of it or more, or matches it under the existing one-call rule. Without this, a fit ending a few hops early took the place of a whole dah.

**Tests:**
- New: `TheRectangleIsFittedTests`, which produced the tables in section 3. It asserts the call reads whole at 10 dB and up, that both noise cases print nothing, and that the fading call reads.
- Changed: `TheShapeOfAKeyedToneTests.RealMarksScoreInsideTheShapeAndNoiseOutside` now leaves fitted marks out of its per-hop shape measurement. Fitted marks have no per-hop shape, so they read as NaN. They are real dahs of the weak call.
- This is the one existing test I touched.

**Build and runs:**
- Build: 0 warnings, 0 errors.
- App carry-forward line: 278 of 278.
- Fit and shape tests: 14 of 14.

**Records:**
- R115 is in both plans.
- HM-DEC-220 is in `DECISIONS.md`, recorded as your ruling from the order.
- The `CLAUDE.md` index row sits above HM-DEC-219.
- `PHASE_OUTCOME` (both copies) has `## UNIT 516 - STEP 12`.
- `PHASE_STATUS` (both copies) names 516.
- The version went from 1.13.201 to 1.13.202.

## 2. What the owner should expect

- **Rebuild before you run it.**
- **Weaker stations should now read.** On the bench, the call used to read whole down to 14 dB over the noise. Now it reads whole down to 10 dB. At 8 dB it is still wrong, but much nearer: `CQ NITK DE N0CALL NTJCALL K`.
- **Strong stations read exactly as before.** Every strong case prints the same text as at HEAD.
- **Noise still prints nothing.** In three minutes of loud noise, the fit added one candidate, and it did not stand.
- **Your five presses tonight were weak stations at swings of 13 to 25 dB.** That is the case this unit is aimed at, but the bench's "dB over the noise" is not your meter's swing. Only the radio will tell whether those stations now read.

## 3. What you should see

**The strength table** is the call at 20 WPM, 625 Hz, on unit 502's scale, with fit off and then fit on:

| dB over noise | fit off: stood | fit off: reads | fit on: stood (fitted) | fit on: reads |
|---|---|---|---|---|
| 24 | 65 | `CQ CQ DE N0CALL N0CALL K` | 65 (0) | `CQ CQ DE N0CALL N0CALL K` |
| 16 | 70 | `CQ CQ DE N0CALL N0CALL K` | 70 (0) | `CQ CQ DE N0CALL N0CALL K` |
| 14 | 65 | `CQ CQ DE N0CALL N0CALL K` | 65 (0) | `CQ CQ DE N0CALL N0CALL K` |
| 12 | 64 | `CT A CQ DE N0CALL N0CALL K` | 65 (1) | `CQ CQ DE N0CALL N0CALL K` |
| 10 | 62 | `CQ RMT IE N0CALL N0CALL K` | 65 (3) | `CQ CQ DE N0CALL N0CALL K` |
| 8 | 58 | `N ET A EI A DE N0CALL NTJCE AEL K` | 65 (7) | `CQ NITK DE N0CALL NTJCALL K` |

"Stood" counts the marks at the pitch that stood; the call has 65. The 16 dB row's extra 5 marks were there at HEAD too.

**The score distribution.** Real marks are scored at their true spans. Noise is dit (60 ms) and dah (180 ms) spans every 25 ms over 30 s.

| | spans | lowest | 5% | median | 95% | highest | under 0.7 |
|---|---|---|---|---|---|---|---|
| real, 24 dB | 65 | 0.835 | 0.848 | 0.892 | 0.915 | 0.919 | 0 |
| real, 16 dB | 65 | 0.000 | 0.807 | 0.870 | 0.901 | 0.907 | 1 |
| real, 12 dB | 65 | 0.738 | 0.774 | 0.835 | 0.890 | 0.900 | 0 |
| real, 8 dB | 65 | 0.000 | 0.671 | 0.778 | 0.831 | 0.899 | 6 |
| noise, call's level | 1160 | 0.000 | 0.000 | 0.000 | 0.048 | 0.239 | (0 at or over) |
| noise, loud | 1160 | 0.000 | 0.000 | 0.000 | 0.041 | 0.189 | (0 at or over) |

**They do not overlap:**
- No noise span reaches 0.24.
- Every real mark with a nonzero score is at least 0.57.
- The zeros at 16 and 8 dB are real marks that rules 1 to 3 set to 0. At 8 dB, the 6 under the threshold include those zeros.

**Both noise tests:**
- 30 s: 635 candidates, 0 fitted, 0 stood, prints nothing.
- 180 s: 3,933 candidates (3,932 at HEAD), 1 fitted, 80 stood as at HEAD, prints nothing.

**The fading station** is the call fading from 24 dB to 10 dB and back over its length. It reads whole before and after, with nothing fitted. So the bench's fade does not break the per-hop path, and this case does not show the fit's value.

**Every existing case** was run with unit 515's filter: speeds, Farnsworth, the speed change, fists, bursts, hesitation, TEST DE W1AW K, DE DE, lone and stray marks, two stations, five pitches, the drifting station, and 511's quiet dit and dah. Diffed line by line against HEAD's run, all 113 printed lines are identical except these, each nearer the sent text:

| case | HEAD | now |
|---|---|---|
| 20 WPM at 12 dB | `CT A CQ DE N0CALL N0CALL K` | `CQ CQ DE N0CALL N0CALL K` |
| 20 WPM at 8 dB | `N ET A EI A DE N0CALL NTJCE AEL K` | `CQ NITK DE N0CALL NTJCALL K` |
| 5 WPM Farnsworth at 10 dB | `CK CK DE E■CASL N0RALL N` | `CK CQ DE N0CALL N0CALL K` |
| the weak call (61 → 65 marks) | `CGE N EQ DE N0CALL NT ON EALL A` | `CGE NNQ DE N0CALL NT■NNALL K` |

Plus the 180 s noise candidate count, 3,932 → 3,933.

**Reds in those classes went from 4 at HEAD to 3.** `TheCallReadsAtEveryStrength(12)` is now green. Still red, as at HEAD:
- `TheCallReadsAtEveryStrength(8)`
- `FarnsworthAndFastReadAtTenDecibels`
- `ALetterReadFromNoiseDoesNotReachTheScreen(blocks: True)`

## 4. What's blocking us

Nothing blocks. The items:

1. **Rules 1 to 4 in section 1 were added by me, beyond the order's text, to keep strong signals unchanged.** Rule 2 brings the existing 1.5 dB tolerance into the fit. They are in HM-DEC-220 as built; overrule any of them if they aren't what you meant.
2. **8 dB still misreads.** 7 marks are fitted, and the reader still splits `CQ` and `N0CALL`. The next unit is either the reader at 8 dB or a fit at the gap level.
3. **The fading case does not exercise the fit.** A deeper fade, down to 6 or 8 dB, would.
4. **`DecisionLogOrderTests.EveryRulingAppearsOnceAndTheGapsAreTheKnownOnes` is red.** The index is missing HM-DEC-166, 182, 189 and more, which were already missing at HEAD and are not touched here. The order check passes.
5. **The old timing-path noise test is still red**, as at 515.
6. **Unit 512 is still unrun.** The next order is 517 or later, with ruling id HM-DEC-221 or later.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is ever revised, at the cost of seconds of lag.
  - On the run path, which is now the only path to the screen, the terminal already shows only settled text.
  - The ask stands only for the timing-only path, which no longer reaches the screen.
  - No change for it sits in the tree.
