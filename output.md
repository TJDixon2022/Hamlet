## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks (`SHACK_FACTS.md`, `docs\cw-scoreboard.md`, `tests\fixtures\cw\captured\unadjudicated` present; no `CoreHMI.sln`, no `MURC.sln`) and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 10:00:58 through `tools\arbiter\lock.bat take` and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. Nothing keys, transmits or writes to the radio. No recording added, removed or promoted; the engine unchanged. Version 1.13.250 to **1.13.251**. Ruling **HM-DEC-271**; HM-DEC-269 stays held for unit 565. Nothing was recorded under §12.1.

**The survey.** `tests\Hamlet.RadioEngine.Tests\Cw\TheCwSurveyTests.cs`, `EveryRecordingInTheTree`, writes `docs\cw-survey-2026-10-09.md`.
- **How it reads:** each file through the board's own `ReadLive`, the app's wiring, at the radio's pitch and filter from its sheet, or the boards' 600 and 500 Hz where there is none. The pitch is found by the chain.
- **How it scores:** with the board's own functions:
  - right, wrong, spaces, and invented and printed in silence on the board's keying map of the same audio;
  - files without a truth get the stray-letter figure.
- **Speed and pitch:** speed from the sender's gap dit, pitch from the letters printed.
- **The boards' rows are the boards' own `Score()`**, so the survey's figures are theirs exactly: **297** (318 of 353, 20 wrong, 1 invented) and **2111** (2131 of 2151, 20 wrong). The test asserts it.
- **Kept off the lines:** it carries the `Survey` category, and both carry-forward lines in `docs\carry-forward-tests.txt` now end `&Category!=Survey`. They still run the same tests: app 278 of 278, engine 154 tests.
- **Its own line**, named there and in the doc: `timeout 1800 dotnet test tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj --filter "FullyQualifiedName~TheCwSurveyTests.EveryRecordingInTheTree"`. It ran in 133 s; every file read.

**The count by folder:** 131 CW recordings; no CW `.wav` under `assets\fixtures\` (its PSK31, Olivia and FT8 files are not read).

| folder | files |
|---|---|
| `cw` | 6 |
| `cw\receiver` | 21 |
| `cw\synthetic-cq` | 12 (the order guessed fifteen) |
| `cw\captured` | 20 |
| `cw\captured\unadjudicated` | 65 |
| `cw\captured\scans` | 3 |
| `cw\captured\w1aw-2026-10-07` | 4 |

**Where each truth came from:**

| files | truth |
|---|---|
| synthetic fixtures | `CwFixtures.All`'s `Sent` text, prosigns written `<BT>` |
| receiver fixtures and synthetic CQs | the `text` line their generator wrote in each sheet; the grade (easy, working, edge, else ungraded) from the name |
| the boards' recordings and the W1AW pieces | the boards' references |
| August captures | no adjudicated text in their sheets; `013347` holds `VA3VRR` (HM-DEC-145) and `134712` holds `N4L` (HM-DEC-144), checked for presence only |
| unadjudicated | none; the sheet's earlier `text` stands beside today's read |
| scan catches | none; the `.json`'s own `text` is Hamlet's read at the time and stands beside today's |

**A correction the survey carries.** A sheet's earlier `text` covers everything since its transcript was cleared (its `textCovers` line), often seven to twenty-five minutes, not the 30 s file. So the doc compares today's letters with the sheet's own count for the file, from its `inThis` line, not with that text's length.

**Records:**
- `docs\cw-scoreboard.md`: a 567 row in each table, unchanged.
- `PHASE_OUTCOME.md`, both copies: `## UNIT 567 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 567.
- `Directory.Build.props`: 1.13.251.
- `CLAUDE.md` §1: a row.
- `DECISIONS.md`: HM-DEC-271.

**Build** `Hamlet.sln -warnaserror`: 0 warnings, 0 errors. **Guards:** 4 of 4. **Decision-log and voice tests:** 7 of 7. **App carry-forward:** 278 of 278. **Engine line**, run to prove the new filter: 153 of 154, the known `TheRsidDetectorTests.TheDetectorKeepsUpWithRealTime` red.

## 2. What the owner should expect

**Every CW recording in the tree, 131 files, has been read once by today's Hamlet.** Nothing about how Hamlet reads has changed, and nothing was added to either scoreboard.

**How much of what was sent Hamlet reads right today:**

| group | files | right of what was sent |
|---|---|---|
| the six synthetic fixtures | 6 | 46 of 67 (69%) |
| receiver fixtures, easy | 5 | 74 of 90 (82%) |
| receiver fixtures, working | 5 | 5 of 81 (6%) |
| receiver fixtures, edge | 5 | 0 of 81 |
| receiver fixtures, ungraded | 6 | 49 of 98 (50%) |
| synthetic CQs at 15 dB | 4 | 73 of 84 (87%) |
| synthetic CQs at 5 dB | 4 | 0 of 84 |
| synthetic CQs at 0 dB | 4 | 0 of 84 |
| your twelve plus KM3STU's three | 16 | 318 of 353 (90%), score **297** |
| W1AW | 4 | 2131 of 2151 (99%), score **2111** |
| August captures, unadjudicated captures, scan catches | 72 | no record of what was sent |

**Three things the survey shows that the two boards don't:**

1. **Below a strong, steady signal, Hamlet prints nothing.** It invents nothing in any of the generated recordings, but it also reads nothing of the CQs at 5 dB or 0 dB, nothing of the edge fixtures, and almost nothing of the working ones. Worst are `coverage-working` (`ITTEIIIT` for `1234567890 QRZ? DE/N0CALL`) and `fast-working` (`EOXDOTAN`). Also `two-station`, which prints nothing at all, and `prosigns-easy`, nothing. The two calls you adjudicated in August, `VA3VRR` and `N4L`, don't read either.
2. **The easy, working and edge grades still mean something,** and sharply: 82%, then 6%, then nothing. Today's reader is a strong-signal reader. Its strength is your real recordings and W1AW, not the weak end of these fixtures.
3. **On the unadjudicated captures, today prints fewer letters than the decoder of the day did,** in 36 of the 56 whose sheets say how many that decoder printed in the file: 1,452 against 2,359, of which 329 the old decoder marked unsure. Today prints nothing at all in 16 of the 65.
   - **By eye, the five most different are mostly losses of signal, not junk.** `cw-2026-09-24-003901` printed 92 letters then and nothing now. `cw-2026-09-24-003919` printed 110 then and 21 now (`EAN ETNXNIK ANQNIK EAN■EK`). `cw-2026-08-25-021629` (the weak `559 559 IN MI MI` buried in noise), `cw-2026-08-31-002829` and `002759` printed 77, 74 and 68 then, with 19, 40 and 63 of them unsure, and nothing now.
   - **Where today does print, it reads words the old session transcript never had:** `ROBIN`, `KA2GJV`, `ANTHONY`, `W1AW/8`, `COORDINATOR`.

Rebuild if you like; nothing on screen changes. Pushed to `main`. To run it again: the one line in `docs\cw-survey-2026-10-09.md`, about two minutes.

## 3. What you should see

**The summary** (from `docs\cw-survey-2026-10-09.md`):

| group | files | with a truth | letters right of sent | wrong | invented | score | worst three by score |
|---|---|---|---|---|---|---|---|
| synthetic fixtures | 6 | 6 | 46 of 67 (69%) | 2 | 0 | **44** | `noisy-18wpm` 0, `interference-18wpm` 2, `fading-18wpm` 8 |
| receiver fixtures: easy | 5 | 5 | 74 of 90 (82%) | 7 | 0 | **67** | `prosigns-easy` 0, `tightfist-easy` 12, `coverage-easy` 16 |
| receiver fixtures: working | 5 | 5 | 5 of 81 (6%) | 9 | 0 | **-4** | `coverage-working` -2, `fast-working` -2, `exchange-working` 0 |
| receiver fixtures: edge | 5 | 5 | 0 of 81 (0%) | 0 | 0 | **0** | `coverage-edge` 0, `exchange-edge` 0, `fast-edge` 0 |
| receiver fixtures: ungraded | 6 | 6 | 49 of 98 (50%) | 5 | 0 | **44** | `two-station` 0, `two-station-first` 0, `farnsworth-light` 3 |
| synthetic CQs: 15 dB | 4 | 4 | 73 of 84 (87%) | 11 | 0 | **62** | `cq-12wpm-15db` 7, `cq-18wpm-15db-char5` 17, `cq-18wpm-15db` 19 |
| synthetic CQs: 5 dB | 4 | 4 | 0 of 84 (0%) | 0 | 0 | **0** | all four 0 |
| synthetic CQs: 0 dB | 4 | 4 | 0 of 84 (0%) | 0 | 0 | **0** | all four 0 |
| August adjudicated | 4 | 0 | - | - | - | - | `VA3VRR` and `N4L` do not read |
| the boards: twelve-plus-three | 16 | 16 | 318 of 353 (90%) | 20 | 1 | **297** | `cw-2026-10-03-221851` -1, `143906` 0, `143951` 0 |
| W1AW pieces | 4 | 4 | 2131 of 2151 (99%) | 20 | 0 | **2111** | `piece-01` 242, `piece-02` 388, `piece-04` 688 |
| unadjudicated | 65 | 0 | - | - | - | - | - |
| scan catches | 3 | 0 | - | - | - | - | - |

**The worst ten files across all groups:**
- **Ordering:** by score, then by letters sent.
- **The board's `221851`:** its stretch is low confidence, so only its one invented letter counts.
- **Eleven files are tied at 0 of 21** (the eight CQs at 5 and 0 dB); `cq-25wpm-5db` and `cq-18wpm-5db` are left off by order.

| # | file | group | score | right | read | truth |
|---|---|---|---|---|---|---|
| 1 | `coverage-working` | receiver, working | -2 | 1 of 23 | `ITTEIIIT` | `1234567890 QRZ? DE/N0CALL` |
| 2 | `fast-working` | receiver, working | -2 | 3 of 19 | `EOXDOTAN` | `CQ CQ DE N0CALL N0CALL K` |
| 3 | `cw-2026-10-03-221851` | the board | -1 | (low, not counted) | `SESE E IE EIEA2LCQ DE NA8SBN` | `BEST 73 <AR> W2L CQ DE NA8SB K` |
| 4 | `two-station` | receiver, ungraded | 0 | 0 of 27 | nothing | `CQ CQ DE N0CALL K N0CALL DE W1XYZ K` |
| 5 | `coverage-edge` | receiver, edge | 0 | 0 of 23 | nothing | `1234567890 QRZ? DE/N0CALL` |
| 6 | `cq-12wpm-0db` | CQ, 0 dB | 0 | 0 of 21 | nothing | `CQ CQ CQ DE N0CALL N0CALL K` |
| 7 | `cq-18wpm-0db` | CQ, 0 dB | 0 | 0 of 21 | nothing | the same |
| 8 | `cq-18wpm-0db-char5` | CQ, 0 dB | 0 | 0 of 21 | nothing | the same |
| 9 | `cq-25wpm-0db` | CQ, 0 dB | 0 | 0 of 21 | nothing | the same |
| 10 | `cq-12wpm-5db` | CQ, 5 dB | 0 | 0 of 21 | nothing | the same |

**Both scoreboards' rows:**

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| HEAD | 318 of 353 | 20 | 1 | **297** | 0 | 83 of 110, 3 added |
| 567 | 318 of 353 | 20 | 1 | **297** (234 without) | 0 | 83 of 110, 3 added |

| w1aw | score | right | wrong | invented | spaces |
|---|---|---|---|---|---|
| HEAD | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |
| 567 | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |

## 4. What's blocking us

- **Every file read**; none failed. Two unadjudicated WAVs have no sheet (`cw-2026-09-23-125515`, `cw-2026-09-23-173723`) and were read at 600 and 500 Hz with no earlier read beside them.
- **Three unadjudicated sheets carry no `text` line**, and nine carry no `inThis` count, so they have nothing to compare with.
- **The August sheets carry no adjudicated text;** only two callsigns from the decision record are checked.
- **The scan catches have no truth.** Their pending references are in `TheStrongStationsOverTests`, unconfirmed, and not scored here.
- **`W1AW-ARLP034-PROPOSED-TRUTH.md`** in `unadjudicated` is a proposed truth, not adjudicated, and is not scored.
- **`TheRsidDetectorTests.TheDetectorKeepsUpWithRealTime`** is red on the engine line run whole, and the two reply tests (`TAW`, `IAN`) are red at HEAD.

### Asks still outstanding

- **Unit 562, 2026-10-08:** what next for the junk before a weak call, given the 0.4 start costs the start of every real call. Waiting on the owner. No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
