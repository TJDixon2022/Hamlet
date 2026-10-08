## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks (`SHACK_FACTS.md`, `CwSenderLane.cs`, `cw-2026-10-08-121324.wav` present; no `CoreHMI.sln`, no `MURC.sln`) and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 11:28:36 through `tools\arbiter\lock.bat take` and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. R88 lifted only as the order named, and no recording added. Version 1.13.244 to **1.13.245**. Ruling **HM-DEC-264**, the number the order gave. Nothing was recorded under §12.1. Every test run was one filtered invocation; scripts went in `.run-unit\unit560-*.sh`, not committed.

**Task 1, the windows go on** (`8fb6f94d`), **shipped at the owner's word:**
- **The windows:** `CwEnvelopeDetector.CandidateWindows` is on by default, with the level rule for marks in a provisional window. The environment switch is gone, and the tag `before-candidate-windows` holds the tree without them.
- **Re-measured at HEAD:** every stretch reads exactly as last unit measured with them on. 297 with KM3STU's three, 234 without, w1aw 2111.
  - Noise prints nothing, the carrier prints at seed 5195 only, and the first recording reads whole.
  - One letter is invented, on `221851`.
- **Both guards' rows reset** to these, naming the owner's ruling: the twelve-plus-three at 297 and w1aw at 2111. The scoreboard still prints the twelve's total without KM3STU's three.
- **The light:** `Km3stuReadsFromHisCqTests` now traces it each second.
- **The scan's and the light's tests:** 133 run together, one failed. `TheCatchScanTests.ASlowEarLosesNothing` timed a delivery at 4.2 ms against 2 ms under the load of the whole run. Alone it passed twice, at 0.06 and 0.02 ms. Everything else passed.

**Task 2, `NOTA` for `POTA`** (`64dc6557`, the rows): **the cold re-read in a candidate's window was built and reverted.**
- **What it did:** when a candidate's window opened, the audio from its first mark was read through it by level, and the gate rebuilt the unprinted sender from those marks.
- **Result:** 230 with KM3STU's three, 169 without, and 1887 on w1aw. `POTA` still read `NOTA`.
- **Why:** the window's two seconds of backfill hold too little settled level to read a station's opening against.

**Task 3, housekeeping:**
- **The 48 WPM ceiling stands,** recorded in HM-DEC-264, and the ask is off the list.
- **The low-confidence stretches** were not re-read (dropped).

**Records:**
- `docs\cw-scoreboard.md`: rows for tasks 1 and 2 in both tables, the guard rows at 297 and 2111.
- `PHASE_OUTCOME.md`, both copies: `## UNIT 560 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 560.
- `Directory.Build.props`: 1.13.245.
- `CLAUDE.md` §1: a row.
- `DECISIONS.md`: HM-DEC-264, superseding HM-DEC-262 and 263 on the windows being off.

**Build** `Hamlet.sln -warnaserror`: 0 warnings, 0 errors.

**Both guards:** 4 of 4. **The decision-log and voice tests:** 7 of 7. **App carry-forward:** 278 of 278.

## 2. What the owner should expect

Rebuild and run as usual.

- **A weak or off-pitch station now gets its own listening window from its first seconds,** as soon as Hamlet has heard a few marks that include a dot and a dash. Before, it had to prove itself on the coarse search first, and a weak one 200 Hz off your pitch never did.
- **KM3STU:** his first recording now reads `INOTA DE KM3STU K KQ4PAK`. Live it printed nothing.
- **The weak fast station** reads `QSY DE WB2FU`.
- **The light** goes amber as soon as his window opens and green while he's printing. Before, it never went green for him.
- **`POTA` still reads `NOTA`.** His P's first dot comes before his window opens. Re-reading the audio from his first mark through the window broke far more than it fixed, so it isn't in.
- **The cost you accepted:** 3 letters on your twelve original recordings and 1 on W1AW. W1AW still starts `E NE II AND` without its P. The first word of a reply reads as before.
- **The scoreboards** now hold at what this reads:
  - **297** for your twelve plus KM3STU's three, which is **234** without his three.
  - **2111** for W1AW.
  - The guards check against these from now on.
- **The 48 WPM top speed stands,** as you said, and it's off the questions list.
- Pushed to `main`.

## 3. What you should see

**Both tables' rows:**

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| HEAD (windows off), with KM3STU's three | 304 of 353 | 29 | 0 | **275** | 0 | 79 of 110, 6 added |
| HEAD, the twelve alone | 254 of 288 | 17 | 0 | **237** | 0 | 67 of 87, 3 added |
| 560 task 1 (windows on), with KM3STU's three — **the guard's row** | 318 of 353 | 20 | 1 | **297** | 0 | 83 of 110, 3 added |
| 560 task 1, the twelve alone | 254 of 288 | 19 | 1 | **234** | | |
| 560 task 2 | 318 of 353 | 20 | 1 | **297** | 0 | 83 of 110, 3 added |
| (the read-back, reverted) | 250 of 353 | 20 | 0 | 230 | 0 | 72 of 110, 5 added |

| w1aw | score | right | wrong | invented | spaces |
|---|---|---|---|---|---|
| HEAD | 2112 | 2131 of 2151 | 19 | 0 | 438 of 440, 11 added |
| 560 task 1 — **the guard's row** | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |
| 560 task 2 | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |
| (the read-back, reverted) | 1887 | 1910 of 2151 | 23 | 0 | 391 of 440, 11 added |

**KM3STU's three and the weak station:**

| recording | live | HEAD | now |
|---|---|---|---|
| `121324` | nothing | `IEU EE IE TSIE A EAEE IAGAK` (4 of 19) | `INOTA DE KM3STU K KQ4PAK` (18 of 19) |
| `121357` | `NAT 4PAK IEE■ N TR I5N 55N VAVABK BK TU` | `… RR UR 55N 55N VAVABK BKT` (14 of 14) | `… RR UR 55N 55NVAVABK BKT` (14 of 14) |
| `121414` | `… TURON VA ES72KM3STU` | `R 55N 55N VA VA BK BKTURON VA ES72KM3STUEE` (32 of 32) | the same |
| `143906` | | `IEE I I E NIEE IE T T AEEYDEWB2FU` | `IEE I I E NUVEE T QSYDEWB2FU` |

**W1AW cold and the reply's first word:**

| | HEAD | now | with the read-back (reverted) |
|---|---|---|---|
| W1AW cold, `212015` | `E NE II AND … THIS NME IS` | the same, no P | `■E II AND …` |
| synthetic QSO reply | `TAW DE K3ZZ K3ZZ K` | the same | |
| `144020` reply | `WX IN NETAGIT IAN TEMP` | the same | nothing |

**The light on KM3STU's first recording**, second by second:

| s | his sender | windows | light |
|---|---|---|---|
| 1 to 14 | none | none | `Listening` |
| 15 | none yet | window 793 Hz | `Forming` |
| 16 | 799 Hz, shape 0.19 | window 800 Hz | `Forming` |
| 17 | 799 Hz, 0.71 | window 799 Hz | `Forming` |
| 18 to 22 | 799 Hz, 0.78 to 0.85, PRINTED | his own window 799 to 800 Hz | `Reading` |
| 23 to 28 | 799 Hz, between his calls | 799 Hz | `Forming` |
| 29 | 799 Hz, 0.91, PRINTED | 799 Hz | `Reading` |

With the windows off (HEAD) he never printed, so the light never went past `Forming`.

## 4. What's blocking us

- **`POTA` reads `NOTA`:** the station's first dit comes before its window opens, and the read-back measured far worse.
- **W1AW cold** still misses its P, for the same reason.
- **The low-confidence stretches** (`221851`, `221548` at 598 Hz, `143951`) were not re-read.
- **`TheCatchScanTests.ASlowEarLosesNothing`** times a delivery under 2 ms. It failed once under the load of a 133-test run, and passes alone.

### Asks still outstanding

The 48 WPM ceiling is ruled (HM-DEC-264) and dropped from the queue.

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
