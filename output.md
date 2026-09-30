```
UNIT: 513 - complete - 2026-09-30
UNIT GOAL: a fist is read by which cluster is nearer, not a hard line
NUMBER: fist cases whole: 5 of 5 (2 red at HEAD); existing readings moved: 1 red case and 3 diagnostic rows
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`; the order's gate held: `SHACK_FACTS.md`, `CwRunReader.cs` and `CW_REQUIREMENTS.md` exist, there is no `CoreHMI.sln` or `MURC.sln`, the root is `C:\Source\HamLet`, and `PROJECT_CARD.md` says Hamlet. Nothing in this report is evidence about the radio.

**Numbering.** Unit 513 and HM-DEC-217 were free. Unit 512 and HM-DEC-216 were never used, because the order numbered itself past them.

SESSION.lock was taken through `tools\arbiter\lock.bat take` and released at the end. Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, no box was ticked, and R113 was appended to both plans. **Only `CwRunReader.cs` changed under `src`.** Scratch variants and probes are under `.run-unit\` and not committed.

**What changed in `CwRunReader`** (`0627c6c7`).

- **Dit or dah (`Kinds`).**
  - Where the sender's sorted mark lengths have a clean gap, two neighbours 2× apart, the kinds split there as before. Only the line between them moves, from the geometric mean of the two sides' means to the point that is as many spreads from one centre as from the other (`Boundary`).
  - Where a fist leaves no clean gap, which HEAD read as "one kind", the two kinds are found as two clusters. They're settled by the nearer centre in log-length (`Refine`), starting from the widest-ratio cut.
  - Those clusters are taken only when three things hold: their centres are 2× apart; each centre is two spreads from the boundary; and neither cluster is wider than **0.25** in log-length. Otherwise the fallback is as at HEAD.
- **Gaps.**
  - The line between a gap inside a letter and one between letters (`CharacterGapSeconds`) is the boundary between the sender's own two clusters once both are measured: its gaps inside letters, at least three, and its letter gaps. Until then it's HEAD's √3 gap dits.
  - The letter gaps are settled against the word gaps by the nearer centre (`GapClusters`). The word line stays at the letter centre × √(7/3), which for a regular sender is the boundary between the two clusters.
- **Diagnostics.** `LastClusters` and `Describe` report the printed sender's clusters, for the test's report only.

**The figures and their reasons** (the author's, overrulable, derived from what a hand does):

| Figure | Value | Reason |
|---|---|---|
| `SpreadFloor` | 0.1 in log-length | The detector reads a length to one 5 ms hop, smeared by its 10 ms window: a tenth to a fifth of a dit from 12 to 35 WPM. |
| `SeparationSpreads` | 2 each side | The boundary then has about nineteen in twenty of each cluster on its own side. A fist a third either way clears it; evenly spread noise lengths do not. |
| `HandSpread` | 0.25 in log-length | The widest fist named (unit 504's third either way) is about 0.19, plus the detector's 0.1 in quadrature, about 0.22. |

**How the figures were reached, stated plainly.**
- **The hand-spread limit was added after a failure.** It became necessary when the first build turned the speed-change case red: `... K TEST DE W1AW K` read `... K ■HW1AW K`. A sender going from 10 to 20 WPM leaves 60, 120 and 180 ms marks together in its history, 0.45 wide: two speeds, not one fist. The value comes from the hand, not from that case.
- **Measured and not taken.**
  - Using the measured word-gap cluster for the word line moved the 8 dB row from `NTJCE AEL K` to `NTJCEAELK`, and no fist case needed it.
  - Settling clusters even where HEAD found a clean gap is what broke the speed change, so the clean-gap path is kept as at HEAD.

**Commit mistake.** `0627c6c7`'s message quotes the tightening fist's red reading as `CQ CQ DE DEN■CALL N0CALL K`. What was measured is `CQ CQ DEN■CALL N0CALL K`.

**Tests.**
- New file: `tests/Hamlet.RadioEngine.Tests/Cw/AFistIsReadByTheNearerClusterTests.cs`. Its fist keyer is written in the test: a 625 Hz tone with 4 ms raised-cosine edges, 24 dB over seeded Gaussian noise, every element and gap its ideal length × (1 + u), with u drawn uniformly from −s to +s.
- Build `Hamlet.sln` with warnings as errors: RC=0.
- App carry-forward line: **278 of 278**.
- Eight synthetic reader classes plus the fist cases: 57 of 60. The three reds are unit 507's, as at HEAD.
- The app cases through the reader: 16 of 16.

**Records.**
- Version 1.13.198 to 1.13.199.
- `PHASE_OUTCOME.md` (both copies): `## UNIT 513 - STEP 12`.
- `PHASE_STATUS.md` (both copies) names 513.
- R113 appended to both `PHASE_PLAN.md` copies, with no checkbox touched.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md` HM-DEC-217, in full:

> **A fist is read by the nearer cluster, not a hard line.** Tim, 2026-09-30, R113. At 21:44 UTC on 7.0299 a station hand-sent at about 27 WPM reached the reader as a clean stream of marks - 18 to 22 in four seconds, a 45 ms dit, every press *agree* - and printed real words inside wrong letters. No case on the bench had been both fast and human: every fast case was machine-sent.
>
> **What is built, in `CwRunReader` alone.** Where the sender's sorted mark lengths show a clean gap, two neighbors twice apart, the dits and dahs split there as before, the line moved to the boundary weighted by each side's spread. Where a fist leaves no clean gap, the two kinds are found as two clusters settled by the nearer centre in log-length, and taken only when their centres stand twice apart, two spreads each side of the boundary, and neither is wider than a hand makes. The line between a gap inside a letter and one between letters is the boundary between the sender's own two clusters once both are measured; the letter gaps are settled against the word gaps, and the word line stays at the letter centre times √(7/3). Before the clusters are measured, the old lines stand.
>
> **The figures and why, the author's, overrulable.** A spread is never under 0.1 in log-length, the detector's own reading error of one hop and its window. Two kinds stand two spreads each side of their boundary, where nineteen in twenty of each fall on their own side. A cluster is one kind only up to 0.25: the widest fist named, a third either way, is about 0.19 and the detector adds 0.1 in quadrature. That last was found needed when a sender going from 10 to 20 WPM read `TEST DE` as `■H`: its mixed history is 0.45 wide, two speeds rather than one fist.
>
> **What was measured and not taken.** The word line from the word gaps' own cluster moved unit 507's 8 dB call from `NTJCE AEL K` to `NTJCEAELK` and no fist case needed it.
>
> **What moved.** Every synthetic reading is as at HEAD but the red 5 WPM Farnsworth row at 10 dB, `CK C TA DE E■CAEIL N0RALL N` to `CK CK DE E■CASL N0RALL N`, and three rows of unit 504's all-gates-off diagnostic.

## 2. What the owner should expect

- **Rebuild.**
- **Hand-sent stations at speed should read.** The reader now sorts each dit, dah and gap by which of the sender's own clusters it's nearer, the way an ear copes with a rough fist, instead of by a fixed line. On the bench, a 27 WPM fist whose elements wander by 30% now reads `CQ CQ DE N0CALL N0CALL K`, where it read `CQ CQ DE N0CALL N0■LL D`.
- **Machine-sent and slow senders are unchanged.** Every existing bench case reads as before, apart from one already-wrong slow Farnsworth case, whose wrong text changed.
- **If a fist still reads wrong at the radio,** the cluster table in section 3 sets what the reader measured beside the truth. If the measured centres are near the truth, the sorting was at fault. If they're far off, the measurement was.
- **One thing this bench cannot show:** my fist scatters evenly around the ideal. A real hand's gaps can drift as a whole, element gaps toward 80 ms and letter gaps toward 100, and only the owner's station tests that.
- **Still red, as before:**
  - `DecisionLogOrderTests.EveryRulingAppearsOnceAndTheGapsAreTheKnownOnes`, whose gaps now also include the unused 216;
  - `VoiceTests.NoOperatorFacingStringUsesABritishSpelling`, on unit 450's two "centre"s;
  - unit 507's three strength reds.

## 3. What you should see

**The fist cases.** "True" is the generator's; "reader" is what the reader measured by the end. Centres are geometric means; ± is the spread as the SD of the log. The reader measures marks slightly short and gaps slightly long, which is the detector's known smear.

| Case | Before (HEAD) | After | True: dit / dah; gaps element / letter / word | Reader after: dit / dah (split); gaps element / letter / word (element-letter line, word line) |
|---|---|---|---|---|
| 27 WPM, 20% | `CQ CQ DE N0CALL N0CALL K` | the same | 45±.11 / 131±.12; 44±.11 / 131±.11 / 346±.02 | 38±.12 / 123±.14 (66); 50±.13 / 136±.11 / 352±.02 (86, 224 ms) |
| 27 WPM, 30% | `CQ CQ DE N0CALL N0■LL D` | **`CQ CQ DE N0CALL N0CALL K`** | 42±.17 / 132±.16; 44±.16 / 131±.18 / 338±.14 | 35±.16 / 126±.19 (62); 50±.15 / 136±.17 / 344±.14 (80, 226 ms) |
| 12 WPM, 20% | `CQ CQ DE N0CALL N0CALL K` | the same | 100±.11 / 290±.12; 101±.12 / 292±.13 / 698±.07 | 94±.12 / 284±.12 (162); 108±.12 / 298±.12 / 703±.07 (178, 477 ms) |
| 35 WPM, 20% | `CQ CQ DE N0CALL N0CALL K` | the same | 35±.10 / 99±.13; 33±.11 / 103±.13 / 262±.07 | 29±.13 / 93±.13 (52); 39±.11 / 110±.12 / 268±.07 (65, 178 ms) |
| 27 WPM, 30% then 10% | `CQ CQ DEN■CALL N0CALL K` | **`CQ CQ DE N0CALL N0CALL K`** | 44±.11 / 130±.15; 45±.11 / 134±.13 / 287±.12 | 38±.12 / 121±.10 (71); 50±.10 / **171±.36 / none** (66, 280 ms) |

HEAD's reader on the 30% fist, for comparison: dit 55 ms, split 95 ms, letter line 113 ms. On the tightening fist: letter gap 183 ms, word line 280 ms.

**In the tightening fist, the letter and word gaps ended up as one cluster** (171 ms ±0.36, no word cluster). It reads whole anyway, but the measurement is imperfect there.

**The existing cases**, eight synthetic reader classes: every printed reading identical to HEAD, including these:
- the speed change, 10 then 20 WPM: `... TEST DE W1AW K`;
- the strength table: 16 and 24 dB whole; 8 dB `N ET A EI A DE N0CALL NTJCE AEL K`; 12 dB `CT A CQ DE N0CALL N0CALL K`;
- 35 WPM at 10 dB whole;
- the bursts, the hesitation, `TEST DE W1AW K`, `DE DE`, the lone and stray marks;
- both noise cases: 635 candidates, 0 stood, and 3,932, 80 stood, nothing printed;
- the two-station case: loud one whole, quiet one 12 stood;
- unit 511's quieter dit and dah: whole.

Except:

| Case | HEAD | Now |
|---|---|---|
| 5 WPM Farnsworth at 10 dB (red at HEAD) | `CK C TA DE E■CAEIL N0RALL N` | `CK CK DE E■CASL N0RALL N` |
| `WhichGateTurnsAwayW1aw`, all gates off, 600/500 | `MTN OTN IIL K` | `N CI IIL K` |
| the same, pointed 725 | `MTN OTN IIL K` | `N CI IIL K` |
| the same, whole band | `CALII N/ CI IIL D` | `CALII N N CI IIL D` |

## 4. What's blocking us

Nothing blocks. The items:

1. **Letter and word gaps can merge on a tightening fist.** The letter and word clusters of the fist that tightens ended as one (171 ms ±0.36). It reads whole here; a longer transmission might not.
2. **A drifting fist is not on the bench.** The generator scatters around the ideal. A hand whose element and letter gaps drift toward each other as a whole, which is the owner's 21:44 description, is not tested; your report at the radio is.
3. **Unit 512 and HM-DEC-216 were skipped** by the order's numbering. The next order should be 514 or later, with ruling id HM-DEC-218 or later.
4. **Commit message slip.** `0627c6c7` quotes the tightening fist's red reading wrongly; section 3 has the measured text.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is ever
  revised, at the cost of seconds of lag. On the run path, now the only path to the screen, the
  terminal shows only settled text. The ask stands only for the timing-only path, which no longer
  reaches the screen; no change for it sits in the tree.
