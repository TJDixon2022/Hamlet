## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks (`SHACK_FACTS.md`, `CwSenderGate.cs`, `cw-2026-10-08-121324.wav` present; no `CoreHMI.sln`, no `MURC.sln`) and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 11:58:18 through `tools\arbiter\lock.bat take` and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. R88 lifted only as the order named, and no recording added. Version 1.13.245 to **1.13.246**. Ruling **HM-DEC-265**, the number the order gave. Nothing was recorded under §12.1. Both boards were run at HEAD and after every task, each one filtered `dotnet test` with a timeout.

**Task 1, a station prints once its shape reaches 0.4** (`cb2d2ee4`), shipped:
- **Measured first:** `AStationPrintsOnceSureTests.TheShapeAtFirstPrint` traced every station given the terminal on both tables and the three scan catches. Every one had reached 0.4 by the time it was picked.
  - Two printed under 0.4: `221502` at 3.5 s (shape 0.26) and `221745` at 24.7 s (0.16, the same sender picked again). Both had passed 0.4 earlier.
  - No stretch would print later, and none would never print.
- **The rule:**
  - `CwSenderGate` latches `Sure` on a sender once its shape reaches `PrintScore`, which reads the light's own `CwShapeLights.GreenScore` (0.4).
  - A sender is first picked only when `Sure`. Release under 0.1 and the backlog are unchanged.
  - It is the rule `CwRules.SureFirst`, on.
- **Result: both tables and every stretch unchanged,** 297 (234 without KM3STU's three) and 2111. Guards 4 of 4.
  - With the rule off and on, the weak cases read identically: `143906`, `121324`, `221502`, `221745`, and synthetic weak callers from 4 to 10 dB that strengthen by 6 dB.
  - **The junk before a weak call comes from the backlog** of a sender already over 0.4 when it is picked, which the order kept as it was.

**Task 2, the low-confidence stretches** (`5f3928e9`): `143951`, `221548` at 598 Hz and `221851` were read offline again at envelope cutoffs of 30, 40 and 60 Hz. None agrees with its reference at any cutoff, so all stay low and the totals stand.

**Task 3, the scan test** (`e1ec34a1`): `ASlowEarLosesNothing` already asserted no dropped chunk, no hole, the same WAV length and the same text. Its wall-clock assertion is gone, and the delivery time is reported: 0.70 ms as it is, 0.04 ms with the slow ear. It passes.

**Records:**
- `docs\cw-scoreboard.md`: rows for tasks 1 and 2 in both tables.
- `PHASE_OUTCOME.md`, both copies: `## UNIT 561 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 561.
- `Directory.Build.props`: 1.13.246.
- `CLAUDE.md` §1: a row.
- `DECISIONS.md`: HM-DEC-265.

**Build** `Hamlet.sln -warnaserror`: 0 warnings, 0 errors. **Guards:** 4 of 4. **Decision-log and voice tests:** 7 of 7. **App carry-forward:** 278 of 278.

## 2. What the owner should expect

Rebuild and run as usual.

- **Before a weak station's call you will still see junk.** On the recordings, `143906` still opens `IEE I I E NUVEE T` before `QSY DE WB2FU`, and a weak synthetic caller still opens `E EU` or `N EQ`.
  - The new rule holds a station back until its shape reaches the green line, 0.4.
  - But every station on your recordings had already reached 0.4 by the time Hamlet picked it. The junk is what it heard from that station before it was sure, printed all at once as the backlog when it starts.
  - The order kept that backlog as it was, so the rule changed nothing here. Your on-air case from today isn't in the tree, so I couldn't measure it.
- **No rough hand prints later.** Every stretch reads letter for letter as before.
- **The scoreboards** hold:
  - **297** for your twelve plus KM3STU's three, which is **234** without his three.
  - **2111** on W1AW.
- **The three low-confidence stretches stay low:** a careful re-read still disagrees with each reference.
- **The scan's timing test** no longer fails under load.
- Pushed to `main`.

## 3. What you should see

**Both tables' rows:**

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| HEAD, with KM3STU's three | 318 of 353 | 20 | 1 | **297** | 0 | 83 of 110, 3 added |
| 561 task 1 | 318 of 353 | 20 | 1 | **297** (234 without) | 0 | 83 of 110, 3 added |
| 561 task 2 | 318 of 353 | 20 | 1 | **297** | 0 | 83 of 110, 3 added |
| 561 task 3 | 318 of 353 | 20 | 1 | **297** | 0 | 83 of 110, 3 added |

| w1aw | score | right | wrong | invented | spaces |
|---|---|---|---|---|---|
| HEAD | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |
| 561 tasks 1 to 3 | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |

Noise prints nothing; the random carrier prints at seed 5195 only, as at HEAD; the first recording reads whole.

**The first-print measurement** (each time a station was given the terminal; identical with the rule on):

| recording | first print | pitch | shape then | peak in 10 s | reached 0.4 | letters | before 0.4 |
|---|---|---|---|---|---|---|---|
| `200157` | 3.63 s / 10.55 / 16.67 | 663 Hz | 0.85 / 0.92 / 0.92 | 0.91 to 0.94 | at once | 5 / 6 / 13 | 0 |
| `143906` | 16.49 s | 515 Hz | 0.52 | 0.78 | at once | 13 | 0 |
| `143951` | 19.96 / 29.66 s | 500 Hz | 0.73 / 0.80 | 0.73 / 0.80 | at once | 12 / 1 | 0 |
| `144020` | 5.11 / 17.44 / 18.79 s | 500 / 600 / 600 Hz | 0.83 / 0.85 / 0.87 | 0.85 to 0.90 | at once | 8 / 1 / 14 | 0 |
| `144045` | 5.39 s | 600 Hz | 0.83 | 0.94 | at once | 18 | 0 |
| `221502` | **3.50 s** / 13.19 | 491 / 499 Hz | **0.26** / 0.71 | 0.78 / 0.89 | **5.57 s** / at once | 14 / 12 | **4, `ITSM`** |
| `221530` | 3.15 / 12.76 / 24.51 s | 491 / 599 / 599 Hz | 0.76 / 0.66 / 0.94 | 0.76 to 0.95 | at once | 12 / 18 / 9 | 0 |
| `221548` | 4.59 / 6.11 / 20.34 s | 599 / 599 / 500 Hz | 0.81 / 0.82 / 0.79 | 0.81 to 0.94 | at once | 0 / 20 / 17 | 0 |
| `221745` | 10.10 / 15.55 / 17.36 / **24.71 s** | 501 to 502 Hz | 0.49 / 0.70 / 0.70 / **0.16** | 0.61 / 0.72 / 0.78 / 0.23 | at once, then **never** | 8 / 1 / 10 / 2 | **2, `TO`** |
| `221805` | 17.77 s | 601 Hz | 0.71 | 0.79 | at once | 17 | 0 |
| `221828` | 3.21 s | 601 Hz | 0.62 | 0.80 | at once | 38 | 0 |
| `221851` | 8.52 / 20.83 / 22.58 / 24.29 / 29.19 s | 601 to 604 Hz | 0.61 to 0.80 | 0.61 to 0.80 | at once | 11 / 0 / 2 / 2 / 0 | 0 |
| `212015` W1AW | 3.93 / 18.74 / 28.23 s | 600 Hz | 0.82 / 0.94 / 0.94 | 0.94 | at once | 22 / 13 / 4 | 0 |
| `121324` | 17.74 / 28.05 s | 800 Hz | 0.78 / 0.84 | 0.85 / 0.91 | at once | 13 / 1 | 0 |
| `121357` | 17.89 s | 799 Hz | 0.82 | 0.93 | at once | 15 | 0 |
| `121414` | 7.55 s | 800 Hz | 0.90 | 0.93 | at once | 26 | 0 |
| W1AW pieces 1 to 4 | 11 picks | 600 to 602 Hz | 0.73 to 0.95 | 0.94 to 0.95 | at once | 20 to 726 | 0 |
| catch `153810` | 6 picks | 856 to 859 Hz | 0.47 to 0.80 | 0.79 to 0.82 | at once | 7 to 66 | 0 |
| catch `154819` | 3 picks | 470 to 471 Hz | 0.46 to 0.82 | 0.68 to 0.82 | at once | 1 to 10 | 0 |
| catch `154614` | none | | | | | | |

`221502`'s first pick and `221745`'s fourth were senders already `Sure` from earlier, so the rule lets both print as before.

**The weak cases, before and after** (identical with the rule off and on):

| case | reads |
|---|---|
| `143906` | `IEE I I E NUVEE T QSYDEWB2FU` |
| `121324` | `INOTA DE KM3STU K KQ4PAK` |
| synthetic caller, 10 dB then 16 dB | `CQ CQ DE N1XYZ N1XYZ K CQ CQ DE N1XYZ N1XYZ K` |
| the same at 8 then 14 dB | `N EQ CQ DE N1XYZ N1XYZ K CQ CQ DE N1XYZ N1XYZ K` |
| the same at 6 then 12 dB | `E EU CQ DE N1XYZ N1XYZ K CQ CQ DE N1XYZ N1XYZ K` |
| the same at 4 then 10 dB | `CQ CQ DE N1XYZ N1XYZ K` (the weak call not read) |

**The low-confidence stretches read again:**

| stretch | reference | 30 Hz | 40 Hz | 60 Hz |
|---|---|---|---|---|
| `143951` | `O WAEIIEURD U AGN ES` | `OWAEIIEUAK U AGN ES` | `O WAEIIEUAK U AGN EES` | `O WUEIIEUAK V APR ESE` |
| `221548` at 598 Hz | `2 I LEARNED CW USING A V BPLX Z EPS` | `E2ILE<AR>ED CW USFGA V BPL■ EZ EPS` | `E2ILEARED CW US■GA V BP■ EZ EPS` | `E2ILEARED CW US■GA V BP■ E■ EPSE` |
| `221851` | `BEST 73 <AR> W2L CQ DE NA8SB K` | `IBESTTH3<AR>W2LCQ DE NA8SBK ■ X ■ RR X STRAY GN` | `… ■ V ■ RR TU STRAY DN` | `… ■ E X ■ RR TU ESTRAY DN` |

## 4. What's blocking us

- **The junk before a weak call is in the backlog,** not in when a station starts printing. Removing it means a ruling on the backlog, below.
- **Today's on-air recording of the portable station** isn't in the tree, so the case that prompted this unit was not measured.
- **`221851`'s stretch runs to 30 s,** but its reference stops at `K`. Even a perfect read would not agree until the stretch is cut, and that is a reference change.

**Ask, raised 2026-10-08:** when a station is first printed, does its backlog start from where its shape first reached 0.4, rather than from its first mark?
- **Why:** the junk before a weak call is marks the sender made before Hamlet was sure of it. They print as backlog at the pick, so the 0.4 line does not remove them.
- **What it would cost:** a reply's first letters on a station that strengthens slowly, the loss unit 533 fixed (`HM-DEC-237`, a reply read from its first letter).
- **Not built,** because the order kept the backlog as it is and what the screen shows is the owner's.
- **Change in the tree:** none.

### Asks still outstanding

- **Unit 561, 2026-10-08:** whether a station's backlog starts where its shape first reached 0.4. Waiting on the owner. No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
