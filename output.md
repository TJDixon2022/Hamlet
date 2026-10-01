```
UNIT: 517 - partial (task 1 landed, task 2 dropped for a ruling) - 2026-10-01
UNIT GOAL: the fit fills gaps, the spaces come from the shape
NUMBER: bench reading floor 10 dB (was 14 dB at HEAD); strong bulletin identical with the fit on and off
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`, and the order's gate held: `SHACK_FACTS.md`, `CwRunReader.cs` and `CW_REQUIREMENTS.md` exist, there is no `CoreHMI.sln` or `MURC.sln`, and the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio. Unit 517 and HM-DEC-221 were free.

**How the session ran:**
- It took SESSION.lock through `tools\arbiter\lock.bat take` and released it at the end.
- It wrote nothing to `RUN_LEDGER.md`, touched nothing under `tools\arbiter\`, ticked no box, and added no ruling to either plan.
- It read no recording, fixture or telemetry.
- Nothing keys, transmits or writes to the radio.

**Task 1: the fit fills only what the per-hop tests left.** Commit `08b31d65`.
- **Starting point.** I restored unit 516's change from `f917267a`: the rectangle fit in `CwEnvelopeDetector`, `CwMark.FitScore`/`Fitted`, `CwPatternGate.StandingLengths`, `TheRectangleIsFittedTests`, and the shape test's per-hop-only measurement. The score, the lengths, the lobe, and the 0.7 threshold under the 24 dB lowest of 0.835 are all unchanged from 516.
- **The rule it lacked.** In `CwEnvelopeDetector`:
  - A fitted rectangle is held while any bar within two bins that overlaps its span is still running or still inside the hops the per-hop tests may call it in (`PerHopStillBusy`).
  - Once none is, the rectangle is dropped if any per-hop mark overlaps it at all, within two bins (`OverlapsPerHopMark`), or if another fitted mark covers half of it.
  - A fit held longer than a 5 WPM dah past its end is dropped.
- **The other half of the cause.** `AlreadyCalled`, which the per-hop path uses to refuse a repeat, now ignores fitted marks. A mark the per-hop tests find is never refused because a fit got there first. That refusal is how a fitted rectangle on the front of a dah took the dah's place in 516.
- **The bulletin test** is `AStrongBulletinReadsTheSameWithTheFitOnAndOff`. It sends `THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG 0123456789` at 18 WPM and 24 dB. It is held identical, fit on and off, in five versions: plain, through the 500 Hz filter, fading, and with a 2 dB AGC overshoot plain and through the filter.
- **It was not red on 516's `f917267a`.** None of the five reproduced the on-air loss. 516 also reads them identically.
  - I looked for a case where 516 breaks a bulletin the per-hop path reads whole, and found none.
  - An AGC overshoot of 3 dB or more at key-down makes the **per-hop path itself** read dits only, fit or no fit.
  - On those dahs, 516's fit split the digits (`012SMHT56TBM`); the rule reads them `0123456789`.
  - Timing ruled out lag: the fit costs about 4 points of real time (17–20% to 20–24% at 8 and 48 kHz).

**Task 2: dropped, with the reason.** Commit `653f8259` keeps the measurement.
- `TheSpacesComeFromTheShapeTests` sends the Quebec station's text by hand at 18 WPM, with a scatter of a sixth, using unit 513's fist generator. I gave the generator a word-gap parameter whose default leaves it unchanged, and added the letters this text needs.
- **Red as predicted:**
  - 7-dit word gaps read whole.
  - 5 dits read `KI1MM DEVE2JD NAME ISJEANQTHQUEBECHW`.
  - 4 dits read `KI1MMDEVE2JDNAMEISJEANQTHQUEBECHW`.
- **The order's rule cannot make it green.** "Any gap past 1.5 times the letter centre" is the reader's own line already, √(7/3) = 1.53. That is 4.5 dits, above a 4-dit word gap.
- **Any line that would work is a trade-off.** With a hand's scatter, 3-dit letter gaps reach 3.5 dits and 4-dit word gaps fall to 3.33, so a line between them puts some spaces inside words. Moving the gap kinds into the gate unchanged would change no reading.
- **So nothing moved into the gate**, and `CwRunReader` is unchanged. The 7-dit row is asserted; 4 and 5 are printed.

**Records:**
- HM-DEC-221 in `DECISIONS.md`, with 516's revert and why, and the Quebec station. It records the gap-kinds half as ruled and not built.
- The `CLAUDE.md` index row.
- `PHASE_OUTCOME` (both copies) has `## UNIT 517 - STEP 12`.
- `PHASE_STATUS` (both copies) names 517.
- **The version went from 1.13.201 to 1.13.203, not 1.13.202.** 1.13.202 was unit 516's build, which you ran and which was reverted, and HM-DEC-150 counts a unit per patch.

**Build and app line:** build 0 warnings, 0 errors. App carry-forward 276 of 278; the two 1 ms dispatcher-loop losses passed alone.

## 2. What the owner should expect

- **Rebuild before you run it.**
- **Weaker stations should read further down.** On the bench the call reads whole down to 10 dB over the noise, where HEAD stopped at 14.
- **A strong station reads exactly as it did.** The fit now fills only where the per-hop tests found nothing, and never touches or replaces a mark they found. The bulletin reads identically with the fit on and off.
- **Hand senders who barely pause between words still run together.** That half was not built, because the line the order gave is already the line in use. Section 4 has the decision.
- **Watch W1AW on the air.** The bench never reproduced 516's dits-only reading. If it happens again with this build, the per-hop path is the next suspect: on the bench, an AGC overshoot at key-down of 3 dB or more turns it to dits with no fit involved.

## 3. What you should see

**The bulletin**, at 18 WPM and 24 dB, fit off and fit on:

| version | fit off | fit on (fitted) |
|---|---|---|
| plain | `THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG 0123456789` | same (0) |
| 500 Hz filter | same | same (0) |
| AGC +2 dB | same | same (0) |
| AGC +2 dB, filter | same | same (0) |
| fading 6 dB over 4 s | `THE G NK D WN U T JU S OS HE L M DM OJ M4H E8M` | same (0) |

All five read the same on 516's `f917267a`. The fading row reads badly at HEAD too, with no fit involved; it is a per-hop finding.

**AGC overshoot of 3 dB or more breaks the per-hop path.** These rows are printed, not held:

| version | fit off (HEAD) | 516 | now |
|---|---|---|---|
| AGC +3 dB | `HE E I INEE SE E I E U E I ES S HE E IA I E I I S H 5 H S I` | `…AMITMANS MTS EEN THE EDATDKT…` | `HE E I INEE SE E I E U JUMPS OVER THE LAZY DOG 0123456789` |
| AGC +3 dB, filter | `HE EII S FT I I ES S HE E IE I E I I S H 5 H S I` | `HE EII S FTMX JUMPS OVER THE LAZY DOG 012SMHT56TBM ITMN` | `HE EII S N FOX JUMPS OVER THE LAZY DOG 0123456789` |
| AGC +6 dB | nothing | `IEN I IMAT I MTI EA T…` | `IEN I I MW I OITEA T I EAG Y T OG 01234 H6789` |

**The strength table** is the call at 20 WPM, fit off then fit on:

| dB | stood off | reads off | stood on (fitted) | reads on |
|---|---|---|---|---|
| 24 | 65 | whole | 65 (0) | whole |
| 16 | 70 | whole | 70 (0) | whole |
| 14 | 65 | whole | 65 (0) | whole |
| 12 | 64 | `CT A CQ DE N0CALL N0CALL K` | 65 (1) | whole |
| 10 | 62 | `CQ RMT IE N0CALL N0CALL K` | 65 (3) | whole |
| 8 | 58 | `N ET A EI A DE N0CALL NTJCE AEL K` | 65 (7) | `CQ NIQ DE N0CALL NTJCALL K` |

**Fading station, 24 dB to 10 and back:** whole, fit off and on, with nothing fitted.

**Score distributions:**
- Real marks:
  - 24 dB: lowest 0.835, median 0.892.
  - 16 dB: 5% 0.807; one mark is zeroed by the lobe rules.
  - 12 dB: lowest 0.738.
  - 8 dB: 5% 0.671; 6 marks under 0.7.
- Noise: highest 0.239 and 0.189, 95% under 0.05.
- No overlap, apart from the real marks the rules set to nought.

**Noise:**
- 30 s: 0 fitted, prints nothing.
- 180 s: 1 fitted (3,932 → 3,933 candidates), 80 stood as at HEAD, prints nothing.

**The word-gap cases** are a hand sender at 18 WPM, unchanged reader:

| word gap | true letter / word gap | reads |
|---|---|---|
| 7 dits | 207 / 439 ms | `KI1MM DE VE2JD NAME IS JEAN QTH QUEBEC HW` |
| 5 dits | 199 / 328 ms | `KI1MM DEVE2JD NAME ISJEANQTHQUEBECHW` |
| 4 dits | 205 / 268 ms | `KI1MMDEVE2JDNAMEISJEANQTHQUEBECHW` |

**Every existing case:** I ran unit 515's filter and diffed every printed line against HEAD's run. 113 lines each side; all are identical except these, each nearer the sent text:
- 12 dB: `CT A CQ DE N0CALL N0CALL K` → `CQ CQ DE N0CALL N0CALL K`
- 8 dB: `N ET A EI A DE N0CALL NTJCE AEL K` → `CQ NIQ DE N0CALL NTJCALL K`
- 5 WPM Farnsworth at 10 dB: `CK CK DE E■CASL N0RALL N` → `CK CQ DE N0CALL N0CALL K`
- The weak call: `CGE N EQ DE N0CALL NT ON EALL A` → `CGE CQ DE N0CALL N0CALL K`. In the shape measurement, its per-hop marks went from 61 to 60, because a fitted mark in the sequence changes which per-hop mark the gate stands.
- 180 s noise candidates: 3,932 → 3,933.

Reds in those classes go from 4 at HEAD to 3. `TheCallReadsAtEveryStrength(12)` is now green. Still red, as at HEAD:
- `TheCallReadsAtEveryStrength(8)`
- `FarnsworthAndFastReadAtTenDecibels`
- `ALetterReadFromNoiseDoesNotReachTheScreen(blocks: True)`

## 4. What's blocking us

1. **Task 2 waits on a ruling: where a word gap starts for a hand sender.**
   - **Ruling asked:** the line between a letter gap and a word gap, when a sender's gaps form only one cluster.
   - **Industry standard:** fldigi and CW Skimmer use an adaptive threshold near 5 dits on the measured dit (between Morse's 3 and 7). The cost of a false space inside a callsign is the reason they don't go lower.

   | Option | Line | Pros | Cons |
   |---|---|---|---|
   | A (recommended) | 5 dits of gap, from the sender's own element gap | Parts 5-dit word gaps; a hand's 3-dit letter gaps rarely reach 5; one place, in the gate | 4-dit word gaps still run together |
   | B | 1.25 × the letter centre (about 3.75 dits) | Parts most 4-dit word gaps | A hand scattering a sixth puts spaces inside words, `KI1 MM`, which is §0.0's wrong-callsign case |
   | C | Keep 1.53 × the letter centre (today) | Nothing moves | The Quebec screen stays as it was |

   With a ruling, the next unit builds it in `CwPatternGate` as the order designed, and `CwRunReader` loses its gap arithmetic.
2. **516's on-air failure was not reproduced on the bench.** The fix closes the mechanism the order named. AGC overshoot, which breaks the per-hop path itself, is the leading other suspect. A capture at the radio would settle it, but the ban on reading recordings stands.
3. **A slow 6 dB fade breaks the per-hop path at 24 dB**, with no fit involved. This is a new finding, not this unit's work.
4. **`DecisionLogOrderTests.EveryRulingAppearsOnceAndTheGapsAreTheKnownOnes` is red.** It was already red for gaps at HEAD (HM-DEC-166, 182, 189, …); HM-DEC-220, reverted with 516, is now one more. The order check passes.
5. **Unit 512 is still unrun.** The next order is 518 or later, with ruling HM-DEC-222 or later.

### Asks still outstanding

- **Unit 517, 2026-10-01: the hand sender's word-gap line**, item 1 above. Waiting on the owner. The measurement sits in `TheSpacesComeFromTheShapeTests`; no change sits in the reader or the gate.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is ever revised, at the cost of seconds of lag.
  - On the run path, which is now the only path to the screen, the terminal already shows only settled text.
  - The ask stands only for the timing-only path, which no longer reaches the screen.
  - No change for it sits in the tree.
