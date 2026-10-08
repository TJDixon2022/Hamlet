## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks (`SHACK_FACTS.md`, `CwSenderGate.cs`, `cw-2026-10-08-121324.wav` present; no `CoreHMI.sln`, no `MURC.sln`) and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 13:20:04 through `tools\arbiter\lock.bat take` and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. R88 lifted only as the order named, and no recording added. Version 1.13.246 to **1.13.247**. Ruling **HM-DEC-266**, the number the order gave. Nothing was recorded under §12.1. Both boards were run after every task, each one filtered `dotnet test` with a timeout.

**Task 1, the backlog starts where the station became sure** (`3acf99a4`): built, measured, **not shipped**.
- **The three tests went red first** (`TheBacklogStartsWhereSureTests`): `143906`, `121324` and the synthetic caller at 8 then 14 dB each opened on junk.
- **Built as ruled:**
  - Each sender records where the mark began at which its shape first reached 0.4.
  - When it is printed, its letters that ended before that mark are skipped.
  - The reply rule is untouched, and the later start wins.
- **Measured: the twelve-plus-three 285 against 297, w1aw 2112 against 2111, and a hard limit broken:** the first recording read `ER C HAT`.
  - The shape reaches 0.4 one to three letters into a real station's sending, so the cut took its first letters.
  - `143906` kept `NUVEE T`, because its junk sender passed 0.4 during the junk.
- **The reply cases lost their first letters:**
  - Synthetic QSO: the first station `Q CQ`, the reply `W DE K3ZZ`.
  - `144020`'s reply: `X IN`.
  - `ASenderThatNeverTakesOverPrintsNothing`: `Q CQ CQ`, red where it is green at HEAD.
- **Started one of the sender's word gaps earlier,** it read 287.
- **Reverted:** the engine is as at HEAD. The three tests stay as readings that assert nothing, and the scoreboard rows record what was measured.

**Task 2, `221851` cut at its `K`** (`08db993b`): the stretch ends at 16.5 s. Its `K` begins at 15.54 s on the offline read, and another station keys `RR TU` after it. Read offline it is `IBESTTH3<AR>W2LCQ DE NA8SBK` against the reference `BEST 73 <AR> W2L CQ DE NA8SB K`, so it stays low and the totals stand.

**Records:**
- `docs\cw-scoreboard.md`: rows for both tasks in both tables.
- `PHASE_OUTCOME.md`, both copies: `## UNIT 562 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 562.
- `Directory.Build.props`: 1.13.247.
- `CLAUDE.md` §1: a row.
- `DECISIONS.md`: HM-DEC-266.

**Build** `Hamlet.sln -warnaserror`: 0 warnings, 0 errors. **Guards:** 4 of 4. **Decision-log and voice tests:** 7 of 7.

**App carry-forward:** not clean in either run.
- **First run, 276 of 278:** two `TheFavoritesAreUnderTheGreenZoneTests` failed.
- **Second run, 277 of 278:** `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint` failed.
- **Each failure** was Avalonia headless's `You've caused dispatcher loop`, and each test passes alone (3 of 3, then 1 of 1).
- **No source changed** since the last unit's clean 278.

## 2. What the owner should expect

Rebuild and run as usual.

**Nothing you'll see on the air has changed.** Your rule couldn't ship. I built it as you said, and Hamlet only becomes sure of a station one to three letters after it has started sending. Starting the backlog there cut the start off almost every real call, and it broke your own first recording.

| Station | What it would have printed |
|---|---|
| Your first recording | `ER C HAT` |
| `144045` | `MP 57 57` |
| W1AW | lost its first word |
| KM3STU | `A DE KM3STU` |
| A synthetic reply | `W DE K3ZZ` |

It also didn't remove all the junk: `143906` still printed `NUVEE T` before `QSY DE WB2FU`.

- **Before a weak station's call** you still see what you saw before, for example `IEE I I E NUVEE T` before `QSY`.
- **A reply still prints from its first letter,** as before.
- **`221851`** is now scored only up to its `K`, but it still reads `TH3` for `73`, so it stays out of the count.
- **Both scoreboards hold:**
  - **297** for your twelve plus KM3STU's three, which is **234** without his three.
  - **2111** on W1AW.
- Pushed to `main`.

## 3. What you should see

**Both tables' rows:**

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| HEAD | 318 of 353 | 20 | 1 | **297** | 0 | 83 of 110, 3 added |
| the rule as ruled (not shipped) | 301 of 353 | 15 | 1 | **285** | 0 | 75 of 110, 3 added |
| a word gap earlier (not shipped) | 305 of 353 | 17 | 1 | **287** | 0 | 75 of 110, 3 added |
| 562 task 1 (engine as HEAD) | 318 of 353 | 20 | 1 | **297** | 0 | 83 of 110, 3 added |
| 562 task 2 | 318 of 353 | 20 | 1 | **297** (234 without) | 0 | 83 of 110, 3 added |

| w1aw | score | right | wrong | invented | spaces |
|---|---|---|---|---|---|
| HEAD | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |
| the rule as ruled (not shipped) | 2112 | 2131 of 2151 | 19 | 0 | 437 of 440, 11 added |
| 562 tasks 1 and 2 | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |

With the rule, the first recording read `ER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`, a hard limit broken. Noise printed nothing and the carrier at seed 5195 still printed.

**The stretches the rule moved** (HEAD → rule):

| stretch | HEAD | with the rule |
|---|---|---|
| `200157` | `FER C HAT…` | `ER C HAT…` |
| `144020` at 500 Hz | `ES OK ON PA <BT>` | `OK ON PA <BT>` |
| `144020` at 600 Hz | `WX IN NETAGIT IAN TEMP` | `X IN NETAGIT IAN TEMP` |
| `144045` | `N TEMP 57 57<BT>…` | `MP 57 57<BT>…` |
| `212015` W1AW | `E NE II AND…` | `II AND…` |
| `221530` at 598 Hz | `IHES MHENI■S…` | `MHENI■S…` |
| `221548` at 598 Hz | `I M EEEARED CW…` | `NED CW…` |
| `221745` | `TEIAND TMN40M…` | `D TMN40M…` |
| `221805` | `E 40T U THESE…` | `0T U THESE…` |
| `221828` | `FER ANOTHER…` | `ER ANOTHER…` |
| `121324` | `INOTA DE KM3STU…` | `A DE KM3STU…` |
| `121357` | `RR UR 55N…` | `E UR 55N…` |
| `121414` | `R 55N 55N…` | `55N 55N…` |

**The weak cases, before and after:**

| case | HEAD | with the rule |
|---|---|---|
| `143906` | `IEE I I E NUVEE T QSYDEWB2FU` | `NUVEE T QSYDEWB2FU` |
| `121324` | `INOTA DE KM3STU K KQ4PAK` | `A DE KM3STU K KQ4PAK` |
| caller, 8 then 14 dB | `N EQ CQ DE N1XYZ N1XYZ K CQ CQ DE N1XYZ N1XYZ K` | `Q CQ DE N1XYZ N1XYZ K Q CQ DE N1XYZ N1XYZ K` |

**The reply cases:**

| case | HEAD | with the rule |
|---|---|---|
| synthetic QSO | `CQ CQ DE W1AW W1AW K TAW DE K3ZZ K3ZZ K` (red at HEAD, `TAW`) | `Q CQ DE W1AW W1AW K W DE K3ZZ K3ZZ K` |
| `144020` | `ES OK ON PA <BT> WX IN NETAGIT IAN TEMP` (red at HEAD, `IAN`) | `OK ON PA <BT> X IN NETAGIT IAN TEMP` |
| `144045`, the next over | `N TEMP 57 57<BT> BTU BOB DE KG8V K` | `MP 57 57<BT> BTU BOB DE KG8V K` (red) |
| a sender that never takes over | `CQ CQ CQ DE W1AW W1AW W1AW K` | `Q CQ CQ DE W1AW W1AW W1AW K` (red) |
| nothing beside a station prints at its pause | `KST DE W1AW QST…` | `ST DE W1AW QST…` |

## 4. What's blocking us

- **The junk before a weak call** is still printed. Ruling HM-DEC-266 was built and failed its own bar.
- **`AReplyIsReadFromItsFirstLetterTests`** has two reds at HEAD, from earlier units: the synthetic QSO's `TAW` and `144020`'s `IAN`.
- **The app carry-forward line** fails one or two tests a run on the dispatcher-loop flake. Each passes alone.

**Ask, raised 2026-10-08:** what next for the junk before a weak station's call?
- **Measured:** Hamlet becomes sure of a station only one to three letters into its sending. So starting the backlog there costs the start of every real call and broke the first recording.
- **What could follow:**
  - Leave the junk.
  - Ship the rule accepting 285 and a broken first recording.
  - A different signal for where a station begins: for example, the last silence longer than a word gap before the mark at which it became sure. That is a new rule for you to rule on.
- **Change in the tree:** none; the engine is as at HEAD.

### Asks still outstanding

The unit 561 backlog ask was answered *"yes"* (HM-DEC-266). It was built and not shipped, and is dropped from the queue.

- **Unit 562, 2026-10-08:** what next for the junk before a weak call, given the 0.4 start costs the start of every real call. Waiting on the owner. No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
