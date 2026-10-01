```
UNIT: 519 - complete, two of the order's cases not met as written - 2026-10-01
UNIT GOAL: shape picks the sender, and loudness picks nothing
NUMBER: clean senders score 0.405 to 0.853, noise at most 0.107; nothing ranks by level
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`, and the order's gate held: `SHACK_FACTS.md`, `CwPatternGate.cs` and `CW_REQUIREMENTS.md` exist, there is no `CoreHMI.sln` or `MURC.sln`, and the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio. Unit 519 and HM-DEC-223 were free.

**How the session ran:**
- It took SESSION.lock through `tools\arbiter\lock.bat take` and released it at the end.
- It wrote nothing to `RUN_LEDGER.md`, touched nothing under `tools\arbiter\`, and ticked no box.
- It read no recording, fixture or telemetry.
- Nothing keys, transmits or writes to the radio.
- The change is commit `8d7e27ea`.

**File by file:**
- **`CwSequenceShape.cs` (new).** One score per sender, from 0 to 1, from ratios alone. It is the product of eight figures:
  - rectangle: the mean of its marks' `ShapeOnly`, or a fitted mark's fit score;
  - dit tightness and dah tightness: one less the spread in log-length over 0.25, a hand's widest (unit 513);
  - separation: nought at 2:1, one at 3:1;
  - element-gap tightness and letter-gap tightness, measured the same way;
  - consistency: the share of marks within √2 of the nearer centre;
  - evidence: 1 − e^(−count/10).

  **Why a product:** a keyed tone is all of these at once, so crisp on four and wrong on one is not a keyed tone, as unit 502 chose for a mark. A fist's overlapping lengths are split by unit 513's nearer-centre refinement.
- **`CwMark.cs`.** `CwMarkShape.ShapeOnly` is flatness × edges × narrowness × length, with **no contrast**.
- **`CwPatternGate.cs`.** Every sequence carries an id and its `Shape`. `Standing` returns them (`StandingSequence`). The gate's own rules are unchanged.
- **`CwRunReader.cs`.** Of the qualified senders, the one printed is the one with the **highest shape score**. Unit 490's most-marks, louder-on-a-tie pick is retired. A sender scoring nought prints nothing. A printed sender is held to its existing release.
- **`CwEnvelopeDetector.cs`.**
  - The reading's pitch, the light and the scope follow the **printed** sender (a new `PrintedPitch` source) while it stands, and the best-shaped standing sequence otherwise. Unit 515's loudest is retired.
  - The reading carries `ShapeScore` and `SequencesStanding`.
- **`CwDecoder.cs`** gains `RunsPrintingHz`. **`MainWindowViewModel`** wires `detector.PrintedPitch` to it.
- **`CwHearingViewModel`.** The verdict row gains `shapeScore` and `sequencesStanding`. The two row tests list them.
- **Before and after from the same audio.** `ShapePicks` (internal, on by default) on the reader and the detector restores HEAD's rules, so every case prints both.

**Every score that had a level term, and what replaced it:**
- The detector's choice of standing sequence ranked by level. It now ranks by shape score.
- The reader's printed-sender tie-break was level. It is removed.
- A mark's shape carried contrast. Ranking now uses `ShapeOnly`.
- **Left as they were, and why:**
  - `CwMarkShape.Score` (with contrast) still decides unit 502's shape gate and shades the scope's blocks and the training graph. §3 keeps the per-mark gates and the scope's drawing unchanged.
  - Level still groups a sender's marks (490/511 tolerance) and ranks nothing.
  - The fit's lobe-peak check (517) and the mark's peak-bin attribution (496) still compare levels within one tone's lobe. That picks a bin, not a sender.

**Where I departed from the order, and why:**
- **The hold is the reader's existing release, not "word gap plus a dah".** I tried a switch at the printed sender's word gap plus a dah. A mark is seen only once it has ended, so a letter gap followed by a dah reads as that much silence. It switched mid-word, losing the last `L` of `N0CALL`. It also switched before the final `K` of `TheStationPrintedReadsWhole`. The existing release, twice the word gap plus a dah, is "as unit 511 holds it", and it never switches mid-sentence.
- **The reading follows the printed sender, not the best shape alone.** Following the best shape alone put the reading on a noise sequence at 1050 Hz early in a 16 dB call, when both had few marks. `TheRowDescribesTheSenderBeingPrinted` now wires `PrintedPitch` to its reader, as the app does. That is the one harness change to an existing case.

**Records:**
- R116 in both plans. R115 is not there: it was reverted with unit 516's records.
- HM-DEC-223 in `DECISIONS.md`, with the 490 and 515 rules it retires and every level term.
- The `CLAUDE.md` index row.
- `PHASE_OUTCOME` (both copies) has `## UNIT 519 - STEP 12`.
- `PHASE_STATUS` (both copies) names 519.
- Version 1.13.203 to 1.13.204.

**Build and app line:** build 0 warnings, 0 errors. App carry-forward 278 of 278; one 1 ms loss passed alone.

## 2. What the owner should expect

- **Rebuild before you run it.**
- **The station with the cleanest rhythm is the one Hamlet chooses**, however loud its neighbours. A sender that does not sound like code at all, like a carrier keyed at random, scores nought and is never printed.
- **Once a station is being read, Hamlet stays on it until it pauses.** It does not change stations mid-sentence.
- **The catch is in that hold.** If a sloppier, louder station stands first and keeps sending, Hamlet stays on it until it pauses, even though the clean one ranks higher. On the bench that is case 2, and it is not fixed. Section 4 has the decision.
- **The row now carries the printed station's `shapeScore` and `sequencesStanding`.** When it picks wrong, you can see the score of what it chose and how many it chose among.
- **Letters are read exactly as before**, and nothing about them changed.

## 3. What you should see

**Case 1: clean 20 WPM at 12 dB at 625 Hz, beside a 24 dB carrier keyed at random.**

| carrier | before | after | clean shape | carrier shape |
|---|---|---|---|---|
| 1025 Hz | `CQ CQ DE N0CALL N0CALL K` | `CQ CQ DE N0CALL N0CALL K` | 0.549 | 0.000 |
| 825 Hz | `NOAM■HIV5■` (carrier) | `NOAM■HIV5■` (carrier) | 0.352, 21 of 65 marks | 0.000 |

At 200 Hz the carrier's keying costs the clean sender most of its marks at the per-mark gates. That is unchanged by this unit, so choosing can't help. At 400 Hz HEAD's rule already read it whole. **No version of case 1 is red under HEAD's rule and green under this one.**

**Case 2: clean 20 WPM at 10 dB at 825 Hz through the 500 Hz filter on 600, beside a 20 dB fist scattered by a third at 600.**

| | prints at 600 | prints at 825 | shapes |
|---|---|---|---|
| clean alone through the filter | | `CQCQDEN0CALLN0CALLK` | 0.492 |
| before | `TESTDEW1AWTESTDEW1AWK` | `ILK` | |
| after | `TESTDEW1AWTESTDEW1AWK` | `ILK` | clean 0.413, fist 0.000 |

**Not met.** The shape ranks the clean sender above the fist, and the test now asserts that. But the fist qualified first, and the hold keeps it through its whole call.

**Case 3: 24 dB clean at 625 Hz and 10 dB clean at 825 Hz.** The loud one prints whole, scoring 0.853. The quiet one stood only 11 marks and scored 0.405, held back by its evidence and its rectangle. The two scores are not close.

**Case 4: a fist scattered by a fifth at 625 Hz, 24 dB, printing; a clean sender at 825 Hz, 20 dB, starts at 8 s.**
- Reads `CQ CQ DE N0CALL N0CALL K 1AW K TEST DE W1AW K`.
- The fist is printed whole; its last mark is at 18.35 s.
- The clean sender takes over 1.2 s later, at 19.58 s. Its letters sent while the fist held the terminal are not printed.
- Shapes: fist 0.069, clean 0.665.

**Case 5: noise.**
- 30 s: highest shape score 0.057, prints nothing.
- 180 s: highest shape score 0.107, prints nothing.
- **Against the real senders:** clean senders in cases 1 to 4 score 0.405 to 0.853, so there is no overlap with noise. **Fists do overlap:** 0.069 for a fifth's scatter and 0.000 for a third's, at or under noise's best.

**Every existing case:** I ran unit 515's filter, unit 517's fit and word-gap tests, and these cases, and diffed every printed line against HEAD's run. All are identical except:
- **The W1AW all-gates-off diagnostic rows**, at 600/500, at 600/500 pointed at 725, and on the whole band. They read `N CI IIL K` and `CALII N N CI IIL D` at HEAD, and now read nothing. Those senders score nought.
- **The bulletin faded by 6 dB**, wrong both ways. HEAD read `THE G NK D WN U T JU S OS HE L M DM OJ M4H E8M`; now `THE G NK D ERO WN U EOD T JU MP TER T AZN M DM OJ M4H E8M`. A different fading sender is picked.

Still red, as at HEAD:
- `TheCallReadsAtEveryStrength(8)`
- `FarnsworthAndFastReadAtTenDecibels`
- `ALetterReadFromNoiseDoesNotReachTheScreen(blocks: True)`

## 4. What's blocking us

1. **Decision: may a better-shaped sender take the terminal before the printed one pauses?** Case 2 can't be met under the hold the order set.

   | Option | Rule | Pros | Cons |
   |---|---|---|---|
   | A (recommended) | Delay the first pick: when the first sender qualifies, wait a word gap for others to qualify, then print the best | Case 2 met; nothing switches mid-sentence | The first letters print a word later |
   | B | Switch at the printed sender's measured word gap, the gap itself, when a better shape stands | Case 2 met | Needs a clean word-gap measure; tried this unit as word gap plus a dah, it split words |
   | C | Keep the hold as built | Never switches mid-sentence | A sloppy station that stands first keeps the terminal |

2. **Fists rank at or under noise's best.** Tightness reaches nought at a hand's widest spread. A fist alone still prints in every existing case, but one scattered by a third scores nought, and the nought rule would refuse it if it stood alone. The limit is the author's figure; overrule it if you want fists ranked above noise.
3. **Case 1 never failed on the choice.** At 200 Hz the per-mark gates lose the clean sender's marks to the carrier, and that is the next problem, not this unit's.
4. **`CwMarkShape.Score` still carries contrast for unit 502's gate and the scope's shading.** I left them because §3 keeps them unchanged. Overrule if you want contrast out of the gate too.
5. **`DecisionLogOrderTests` gaps check** was already red at HEAD for HM-DEC-166, 182, 189 and others. It now also lists HM-DEC-220, reverted, and HM-DEC-222, held for unit 518. The order check passes.
6. **Unit 518 is next**, as the order says.

### Asks still outstanding

- **Unit 519, 2026-10-01:** whether a better-shaped sender may take the terminal before the printed one pauses (item 1). Waiting on the owner. The shape score and the hold are in the tree.
- **Unit 517, 2026-10-01:** the hand sender's word-gap line, 5 dits recommended. Waiting on the owner. The measurement is in `TheSpacesComeFromTheShapeTests`, and no change sits in the reader or the gate.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
