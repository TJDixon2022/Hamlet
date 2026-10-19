# Work instruction 565 - The word line sits between the sender's two gap clusters

## 1. What Claude did

- **Surface and gate:** Claude Code on the development computer. The prompt claimed `PROJECT: Hamlet`, and the tree confirmed it:
  - `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `docs\carry-forward-tests.txt` exist.
  - Neither `CoreHMI.sln` nor `MURC.sln` exists.

  The order is dated 2026-10-08 and re-presented on 2026-10-19, after `output.md`. Its id, HM-DEC-269, was held for it and unused, so it is current work. Nothing in this report is evidence about the radio.
- **Branch:** `main`, three commits, pushed.
- **Run by hand:**
  - `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released at the end.
  - Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` was touched, and no box in `PHASE_PLAN.md` was ticked.
  - Nothing keys or transmits, nothing was written to the radio, and no recording was added, removed or promoted.
- **How the line is set today:** `CwPatternGate.GapLines`.
  - From three word gaps, the equal-error crossing of the sender's own letter and word clusters (`Crossing`).
  - Before that, the larger of √21 times the centre of its gaps inside letters, and √(7/3) times its letter-gap mean.
- **Task 1, the line between the clusters** (`70a63bd0`):
  - **The rule, `CwPatternGate.BetweenClusters`:** with three word gaps or more, the geometric mean of the letter cluster's top (90th percentile) and the word cluster's bottom (10th). With no word cluster, that top times √3.
  - **The clusters are the ones the gate already holds,** its run gaps at the sender's speed now (section 4).
  - **Measured first,** with the line as it was (`TheSpacesByCauseTests.TheGapsAgainstTheLines`).
  - **Measured once on:** it failed its ship condition, so it is **off**, a rule only a test can choose (`CwRules.OffByDefault`, `CwRules.Choose`).
  - **The SKCC hand:** `AWordLineIsWhereTheGapsCrossTests.TheSkccHandAndTheLineBetweenTheClusters`, with the touching hand beside it, both ways.
- **Task 2, the clusters on the sheet** (`728f0cce`):
  - Every capture sheet, Record and automatic, gains `clusters` under `speed`: `letter centre 188, top 220 (4 gaps); word centre 415, bottom 295 (3 gaps); line 254 ms`.
  - The gate writes it at each printed letter (`CwSenderGate.ClustersLine`), and `TheClustersAreOnTheSheetTests` holds it.
- **Records** (the closing commit):
  - Version 1.13.258 to 1.13.259.
  - HM-DEC-269, placed under HM-DEC-277 and above HM-DEC-276 so the log stays newest first, and its `CLAUDE.md` §1 row in the same place.
  - `DecisionLogOrderTests` no longer lists 269 as a known gap.
  - A 565 row in each scoreboard table.
  - `PHASE_OUTCOME.md` and `PHASE_STATUS.md`, both copies.
  - The survey rerun, with a new `Rerun` line.
- **Recorded under §12.1:** nothing.

## 2. What the owner should expect

- **No more word spaces are found, and no letters were lost, because the line did not move.**
  - The twelve-plus-three still reads 86 of 110 spaces with 3 added, and W1AW still 2111.
  - Moving the line as ordered lost spaces: the twelve-plus-three fell to 73 of 110 (2 added), with the letters still 318 of 353, 297.
- **The SKCC lines read as they did.** `QTHWVWVNAMEIS JOEL` would still be `QTHWVWVNAMEIS JOEL`. On a synthetic SKCC hand the moved line read one space fewer at one seed and the same at the other two.
- **Why it doesn't work:** at the moment Hamlet judges these gaps, it has already filed the sender's word gaps in with his letter gaps. The top of his "letter" cluster sits at the very gap that should be a word. A line drawn above the letter cluster's top therefore lands higher than today's, not lower. The fault is the split between the two clusters, not where the line sits between them.
- **What changes tonight:** every capture sheet has a `clusters` line, so an SKCC capture on SLOW shows the sender's letter and word clusters and where the line was, without a replay.
- **Suite:**
  - Build clean, 0 warnings.
  - App line 278 of 278. Survey line 1 of 1, every group unchanged.
  - **The engine line passed 145 and then its test host crashed.** The one retry did the same. It is reported as not measured whole; this is the crash of the last two units.
  - The two scoreboard row checks and `DecisionLogOrderTests` passed after the records were written.
- **Pushed to `main`.** Left uncommitted, as found: `RUN_LEDGER.md`, `PARKED.md`, `.run-unit\denials.txt`, the `.run-unit\reports` files and the `cases-*.txt` files.
- **What will look wrong but is not:**
  - The order quotes the synthetic touching hand at 12, 14 and 13 of 14. It reads 12, 14 and 12 as shipped, and its test (at most one added, at most two missing) passes.

## 3. What you should see

**The gaps, with the line as it was, and where the line between the clusters would fall on the same clusters** (ms):

| recording | around | kind | gap | letter centre | letter top | word centre | word bottom (gaps) | the line was | between the clusters | then |
|---|---|---|---|---|---|---|---|---|---|---|
| `221530` | `6C\|HAM` | letter gap spaced | 212 | 138 | 138 | 212 | 212 (1) | 211 | 239 | letter, right |
| `221530` | `6CHA\|MPI` | letter gap spaced | 541 | 176 | 212 | 581 | 541 (2) | 269 | 367 | word, still wrong |
| `221530` | `SAGE\|12I` | word gap missed | 252 | 181 | 220 | 415 | 295 (3) | 254 | 255 | letter, still wrong |
| `221548` | `EEMY\|SCO` | word gap missed | 283 | 205 | 283 | 557 | 460 (2) | 313 | 490 | letter, still wrong |
| `221548` | `SCOU\|TMA` | letter gap spaced | 452 | 187 | 258 | 489 | 388 (4) | 313 | 317 | word, still wrong |
| `221745` | `E\|EDA` | word gap missed | 380 | 301 | 405 | - | - | 460 | 701 | letter, still wrong |
| `221745` | `EE\|DAN` | word gap missed | 305 | 301 | 405 | - | - | 460 | 701 | letter, still wrong |
| `221745` | `NDON\|40M` | word gap missed | 405 | 301 | 405 | - | - | 460 | 701 | letter, still wrong |
| `221805` | `DAYS\|.TN` | word gap missed | 356 | 261 | 400 | - | - | 399 | 693 | letter, still wrong |
| `221805` | `XFER\|ANO` | word gap missed | 372 | 242 | 372 | 684 | 665 (3) | 508 | 498 | letter, still wrong |
| `221805` | `THER\|FT` | word gap missed | 424 | 240 | 372 | 684 | 665 (3) | 508 | 498 | letter, still wrong |
| `221828` | `EDES\|BES` | word gap missed | 286 | 208 | 276 | 449 | 304 (10) | 300 | 290 | letter, still wrong |
| `121324` | `UKKQ\|4PA` | word gap missed | 248 | 212 | 232 | - | - | 324 | 401 | letter, still wrong |
| `121324` | `Q4PA\|K` | word gap missed | 232 | 219 | 248 | - | - | 334 | 430 | letter, still wrong |
| `121414` | `BKBK\|TUR` | word gap missed | 288 | 171 | 174 | 288 | 288 (1) | 301 | 301 | letter, still wrong |
| `121414` | `BKTU\|RON` | word gap missed | 212 | 225 | 288 | - | - | 343 | 498 | letter, still wrong |
| `121414` | `ES72\|KM3` | word gap missed | 312 | 211 | 288 | 553 | 435 (8) | 315 | 354 | letter, still wrong |

**What the table says:**
- The order's twelve are all here: the nine missed under the line and the three spaced over it, plus five more the tree's trace now names.
- In the only-letter cases the letter top is the missed word gap itself (405, 400), so √3 above it is far past every word gap.
- In the both-clusters cases the word cluster's bottom stands well above the missed gap, the letter top is at or near it (372, 283, 288), and the line between them is above the gap.
- It finds none, and stops spacing one letter gap (`6C|HAM`).

**The rule as stated, measured once on:**

| | as shipped | the line between the clusters |
|---|---|---|
| twelve-plus-three letters | 318 of 353, 20 wrong, 1 invented, **297** | **297** |
| twelve-plus-three spaces | **86 of 110, 3 added** | **73 of 110, 2 added** |
| W1AW letters | 2131 of 2151, 20 wrong, 0 invented, **2111** | **2111** (2131 of 2151, 20 wrong, 0 invented) |
| W1AW spaces | 438 of 440, 11 added | not measured |
| four 5 dB CQs | 13, 14, 16, 19 of 21 | the same |
| carrier, silences, first recording | 1 seed, none, whole | 1 seed, none, whole |

**The synthetic hands** (word spaces of 14):

| hand | seed | as shipped | between the clusters |
|---|---|---|---|
| SKCC, 15 WPM, letter gaps 2 to 4 dits, word gaps 4 to 6 | 5401 | 10, 2 added | 9, 3 added |
| | 5402 | 12, 7 added | 12, 7 added |
| | 5403 | 12, 3 added | 12, 3 added |
| touching, 20 WPM, letter 3 to 5.5, word 6 to 10 | 5381 | 12, 1 added | 12, 1 added |
| | 5382 | 14 | 13 |
| | 5383 | 12 | 12 |

Letters 43 of 43 at every seed, both ways.

**The survey's groups, before and after:** every group unchanged.

| Group | Score |
|---|---|
| Synthetic fixtures | 44 |
| Receiver fixtures, easy | 75 |
| Receiver fixtures, working | -4 |
| Receiver fixtures, edge | 0 |
| Receiver fixtures, ungraded | 63 |
| Synthetic CQs, 15 dB | 72 |
| Synthetic CQs, 5 dB | 46 |
| Synthetic CQs, 0 dB | 0 |
| The boards | 297 and 2111 |

The strays are 3.3 at the highest and 21.9 summed.

**The scoreboards' rows:**
- Twelve-plus-three: `| 565 | 2026-10-19 | 318 of 353 | 20 | 1 | **297** | 0 | 86 of 110, 3 added | …`
- W1AW: `| 565 | 2026-10-19 | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added | …`

## 4. What's blocking us

- **The ship failed: the line between the clusters lost 13 spaces.** It is off. What the table names is the split itself: a sender's word gaps sorted into his letter cluster (`CwPatternGate.LetterAndWordGaps`, `Split`). That is a different rule, for a ruling.
- **Choice taken: the clusters are the gate's own run gaps at the sender's speed now,** not ten marks' worth. Ten marks hold three or four gaps, too few to stand a cluster of three.
- **Choice taken: HM-DEC-269 sits below HM-DEC-277 in both logs,** since its date is 2026-10-19 and the order test reads ids within a date.
- **Choice taken: the order test's known gaps lose 269,** now recorded.
- **The spaces by cause now** (the tree's trace; the order's table was older): 8 invisible, 7 under the line with a word cluster shown, 7 with none yet, 2 unprinted, 3 added.
- **The eight word gaps as short as the letter gaps:**
  - `221530`: `EN|FBW` 185, `WHEN|IWA` 209, `HENI|WAS` 160, `GE12|ILE` 226, `SING|AV` 190;
  - `221745`: `.EUR|EEH` 137;
  - `221828`: `T73|W` 203;
  - `121414`: `VAES|72K` 205 ms.
- **The two with a letter either side not printed:** `144020` `TEMP|E` and `221530` `PION|BK`.
- **The engine line's test host crashes after its last tests,** in this unit and the two before, with no reason in the log. It was not traced.

### Asks still outstanding

- **Unit 562, 2026-10-08:** what next for the junk before a weak call, given the 0.4 start costs the start of every real call. Waiting on the owner. No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524 and by the shadow rule. Still open before a sender stands, and for a sender 50 Hz or nearer.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
