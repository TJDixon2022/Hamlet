## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks (`SHACK_FACTS.md`, `docs\cw-scoreboard.md`, `docs\cw-survey-2026-10-09.md` present; no `CoreHMI.sln`, no `MURC.sln`) and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 10:59:48 through `tools\arbiter\lock.bat take` and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. **`before-lane-bank` tagged and pushed before the first change.** Nothing keys, transmits or writes to the radio; no recording added, removed or promoted. Version 1.13.252 to **1.13.253**. Ruling **HM-DEC-273**; HM-DEC-269 stays held. Nothing was recorded under §12.1.

**Task 1, the bank, running and feeding nothing** (`98f4ad0d`; its lane contrast set in `ab2abe9a`). `src\Hamlet.RadioEngine\Cw\CwLaneBank.cs`, run inside the detector, the one wiring every listener shares.
- **Constants:**
  - `SpacingHz` 25 Hz, the grid's own; twenty lanes per dit for a 500 Hz filter.
  - `DitSeconds` 100, 60 and 40 ms, about 12, 20 and 30 WPM.
  - `MinContrastDb` 12 dB: noise alone spreads about 8 dB there, and at 10 dB a loud noise run stood five marks at 0.68.
  - `HistoryHops` 600, three seconds of each lane's own levels; key-up is their 30th percentile and key-down their 90th, refreshed every 250 ms.
- **Only on the rig's passband:** a detector searching 100 to 3,000 Hz with no rig holds no bank.
- **Each lane is the sender window's filter:** its fourth-order Butterworth at the cutoff and delay `CwSenderLane` takes for that dit.
  - The audio is averaged to about 4 kHz and mixed per pitch with one rotating oscillator, and the lanes' filters run at about 2 kHz.
  - A mark rises at 0.4 of the contrast under key-down.
  - It ends only once the level has stayed under 0.6 for a quarter of the lane's dit, two hops at least. Without that, one hop of noise broke a 200 ms dah into 45 and 140 ms.
  - It is at least half the lane's dit.
- **Its marks go into pattern gates of its own,** one per dit, the same class and rules as the grid's.
  - They stay apart because, fed into the grid's own gate, they would have joined the grid's sequences at the same pitch and changed both boards.
  - The sender gate takes none of them in task 1.
- **Proved:**
  - On `cq-18wpm-5db` the lanes at 600 and 625 Hz read 55 to 65 and 185 to 205 ms marks against a keyed 67 and 200, two lengths from 1.8 s, shape 0.88.
  - On the four loud-noise runs no bank mark at all.
  - Both boards exactly as at HEAD.
- **CPU:** on W1AW piece-04, 283 s at 48 kHz, 30.6 s for the chain without the bank and 35.2 s with it.
  - **The bank is 4.6 s, 15% of the chain without it.**
  - At the audio's own rate it was 86%, and 30% with the lanes alone averaged; sorting each lane's levels in place every 250 ms took it to 15%.
  - I kept 25 Hz rather than halving to 50 Hz. A 100 ms dit's cutoff is about 19 Hz, so a station between two lanes 50 Hz apart would sit outside both.
- **Telemetry:** both capture sheets carry a `bank` line, and `cw_listen` a `bank` field: lanes held, how many stand, the best standing lane's pitch, shape and dit. Nothing on screen.

**Task 2, the handover** (`f56f838d`), built and **kept off by default** (`CwEnvelopeDetector.LaneHandover`).
- **The rule:** a bank lane's station goes to the sender gate only where the grid has nothing standing, no candidate, and neither a provisional window nor the station's own window within `HandoverHz`, 50 Hz. Those two bins are the tree's own figure for "the same station".
  - The best-shaped standing lane holds the station, and only its marks are handed on.
  - While it holds, the grid's marks within 50 Hz are kept out.
  - It is let go when its sequence stops standing.
- **Measured on, at the bank as it ships:**
  - **Passes:** the four 5 dB CQs read 15, 16, 16 and 17 of 21.
  - **Fails, working fixtures:** none of the five reads half (1, 3, 3, 0 and 0 of their letters). On at least two the grid stands, so the staged rule leaves them to it.
  - **Fails, the board:** the twelve-plus-three fell to 291 (313 right, 21 wrong, 1 invented). At 10 dB of lane contrast it had fallen to 281 with 15 invented.
  - **Fails, the carrier:** the random carrier printed at a second seed, 5209.
  - W1AW held at 2111.
- **So it ships off,** and the next unit starts from here. No guard was added for the 5 dB CQs, since their figures did not ship.

**Task 3, the survey** (`ab2abe9a`): rerun with the bank running and the handover off. `docs\cw-survey-2026-10-09.md` gained a `rerun` line in its header, and every row and the summary are as before. It ran in 147 s, from 126.

**Records:**
- `docs\cw-scoreboard.md`: rows for tasks 1 and 2 in each table, at HEAD's figures.
- `CW_SPEC.md` §2: the finders, the bank and the staged handover, one paragraph. The decoder and scanner brief is archived; this is its successor.
- `PHASE_OUTCOME.md`, both copies: `## UNIT 569 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 569.
- `Directory.Build.props`: 1.13.253.
- `CLAUDE.md` §1: a row.
- `DECISIONS.md`: HM-DEC-273.

**Build** `Hamlet.sln -warnaserror`: 0 warnings, 0 errors. **Guards:** 4 of 4. **Decision-log and voice tests:** 7 of 7. **App carry-forward:** 278 of 278. **Engine line:** 153 of 154, the known `TheRsidDetectorTests.TheDetectorKeepsUpWithRealTime` red. **Survey line:** passed, 147 s.

## 2. What the owner should expect

**Weak stations do not read yet in the app, but they can.**

**What the bank is:** a second set of ears beside the grid, a narrow filter every 25 Hz across your CW filter at three speeds. Unlike the grid's coarse slices, each one hears a weak station's dots and dashes whole.

**What it hears:** on a 5 dB CQ the bank hears the true dot and dash lengths within two seconds. On plain noise it hears nothing. It costs about a sixth more processing.

**What the handover is:** the rule for letting the bank's station print, only where the grid hears nothing nearby, so the stations Hamlet reads today are untouched.

**Turned on, the handover reads the four 5 dB CQs** that print nothing today:

| CQ | reads | right |
|---|---|---|
| 12 WPM | `FG CQ CQ DE N0 ELL N0 LL K` | 15 of 21 |
| 18 WPM | `RQ K QCQ DE N0CALL N0K DL K` | 16 of 21 |
| 18 WPM, wide letter gaps | `RQCQN DEN0KALLN0KALLK` | 16 of 21 |
| 25 WPM | `NQ CQ CG E0CALL N0CALL K` | 17 of 21 |

The two-station QSO also reads 21 of 27, where it read nothing. **But it cost your recordings six letters** (297 to 291), a second random carrier printed letters, and the weak fading fixtures still read junk. So the handover is in the tree and **switched off**, and nothing you see changes.

| measure | before | after |
|---|---|---|
| your twelve plus KM3STU's three | 297 | 297 |
| W1AW | 2111 | 2111 |
| every survey group | as before | as before |

Rebuild if you like. Pushed to `main`.

## 3. What you should see

**The four 5 dB CQs against the ceiling** (the sender's own window opened by hand):

| file | ceiling | handover on | today (off) |
|---|---|---|---|
| `cq-12wpm-5db` | 21 of 21 | `FG CQ CQ DE N0 ELL N0 LL K`, 15 of 21 | nothing |
| `cq-18wpm-5db` | 21 of 21 | `RQ K QCQ DE N0CALL N0K DL K`, 16 of 21 | nothing |
| `cq-18wpm-5db-char5` | 21 of 21 | `RQCQN DEN0KALLN0KALLK`, 16 of 21 | nothing |
| `cq-25wpm-5db` | 21 of 21 | `NQ CQ CG E0CALL N0CALL K`, 17 of 21 | nothing |

**The eighteen** (the weak-end trace at HEAD; the full row with the wide bin and the chain's stages is in `TheWeakEndTests.WhereEachWeakFileDies`):

| file | sent | own window: contrast median / p10 | level marks found of sent, whole | dies at (handover off) | window read, right of sent | lane read (handover on), right of sent |
|---|---|---|---|---|---|---|
| `cq-12wpm-0db` | 0 dB, 615 Hz, 12 WPM, 73 | 12.2 / 10.9 dB | 77 of 73, 70 whole | **candidate** | `CQ CQ C■ DE S0CA■ N0CALL K`, 17 of 21 | `FQ T E NO`, 3 of 21 |
| `cq-12wpm-5db` | 5 dB, 615 Hz, 12 WPM, 73 | 16.9 / 16.2 dB | 73 of 73, 73 whole | **window** | `CQ CQ CQ DE N0CALL N0CALL K`, 21 of 21 | `FG CQ CQ DE N0 ELL N0 LL K`, 15 of 21 |
| `cq-18wpm-0db` | 0 dB, 615 Hz, 18 WPM, 73 | 10.4 / 8.9 dB | 76 of 73, 66 whole | **candidate** | `CQ CQ TN■ DE N■I■ALL N,K SLL K`, 12 of 21 | nothing, 0 of 21 |
| `cq-18wpm-0db-char5` | 0 dB, 615 Hz, 18 WPM, 73 | 10.0 / 8.5 dB | 82 of 73, 62 whole | **window** | `■Q C■ K ■ DE S■6A■L N■CALL K`, 12 of 21 | nothing, 0 of 21 |
| `cq-18wpm-5db` | 5 dB, 615 Hz, 18 WPM, 73 | 15.7 / 14.9 dB | 73 of 73, 73 whole | **window** | `CQ CQ CQ DE N0CALL N0CALL K`, 21 of 21 | `RQ K QCQ DE N0CALL N0K DL K`, 16 of 21 |
| `cq-18wpm-5db-char5` | 5 dB, 615 Hz, 18 WPM, 73 | 15.3 / 14.5 dB | 73 of 73, 73 whole | **candidate** | `CQ CQ CQ DE N0CALL N0CALL K`, 21 of 21 | `RQCQN DEN0KALLN0KALLK`, 16 of 21 |
| `cq-25wpm-0db` | 0 dB, 615 Hz, 25 WPM, 73 | 9.4 / 7.7 dB | 90 of 73, 58 whole | **candidate** | `CHS ■Q /■ DE NT■<AR>A■HE N■ERAL5 B E`, 5 of 21 | nothing, 0 of 21 |
| `cq-25wpm-5db` | 5 dB, 615 Hz, 25 WPM, 73 | 13.9 / 13.2 dB | 73 of 73, 73 whole | **candidate** | `CQ CQ CQ DE N0CALL N0CALL K`, 21 of 21 | `NQ CQ CG E0CALL N0CALL K`, 17 of 21 |
| `coverage-edge` | 0 dB, 615 Hz, 12 WPM, 97 | 11.9 / 10.7 dB | 98 of 97, 94 whole | **candidate** | `■23■56Z 890 QRZ? DE/N0CALL`, 20 of 23 | `■I DEX T L`, 3 of 23 |
| `coverage-working` | 5 dB, 615 Hz, 12 WPM, 97 | 16.7 / 11.2 dB | 97 of 97, 93 whole | **reader** | `12EW456■890 QRZ? TEE/S0CALL`, 17 of 23 | `ITHIHIHU TETMFETESI`, 1 of 23 |
| `exchange-edge` | 0 dB, 615 Hz, 12 WPM, 65 | 12.4 / 10.6 dB | 68 of 65, 62 whole | **candidate** | `CQ CQ DE N0CAL5 N■CALL K`, 17 of 19 | nothing, 0 of 19 |
| `exchange-working` | 5 dB, 615 Hz, 12 WPM, 65 | 17.1 / 11.0 dB | 68 of 65, 60 whole | **reader** | `CQ CQ LE N■CA5AI N0■AR L K`, 12 of 19 | `KSNNAIMNR`, 3 of 19 |
| `fast-edge` | 0 dB, 615 Hz, 25 WPM, 65 | 9.2 / 7.1 dB | 78 of 65, 52 whole | **candidate** | `■- ■T■ DIN■■AR R N■6 TED5 ■ E E`, 4 of 19 | nothing, 0 of 19 |
| `fast-working` | 5 dB, 615 Hz, 25 WPM, 65 | 14.8 / 9.1 dB | 65 of 65, 51 whole | **reader** | `CDI C■ DE N■T■AEEEL T 1NT ALE I K`, 5 of 19 | `ETMXDOTAN`, 3 of 19 |
| `prosigns-edge` | 0 dB, 615 Hz, 12 WPM, 37 | 12.2 / 11.1 dB | 38 of 37, 37 whole | **candidate** | `<BT> N0CALL <AR> <SK> E`, 9 of 9 | `4N ■V`, 1 of 9 |
| `prosigns-working` | 5 dB, 615 Hz, 12 WPM, 37 | 16.9 / 11.4 dB | 38 of 37, 36 whole | **marks** | `■ NOMCALL <AR> <SK>`, 6 of 9 | nothing, 0 of 9 |
| `tightfist-edge` | 0 dB, 615 Hz, 13 WPM, 19 | 12.5 / 11.2 dB | 20 of 19, 18 whole | **candidate** | `TEST DE TEST X`, 10 of 11 | nothing, 0 of 11 |
| `tightfist-working` | 5 dB, 615 Hz, 13 WPM, 19 | 16.9 / 10.3 dB | 19 of 19, 19 whole | **sure** | `TEST DE TEST K`, 11 of 11 | nothing, 0 of 11 |

**The survey's summary, before and after** (the handover off, as shipped; the same document, plus a `rerun` line):

| group | before | after |
|---|---|---|
| synthetic fixtures | 46 of 67, 44 | 46 of 67, 44 |
| receiver easy | 74 of 90, 67 | 74 of 90, 67 |
| receiver working | 5 of 81, -4 | 5 of 81, -4 |
| receiver edge | 0 of 81, 0 | 0 of 81, 0 |
| receiver ungraded | 49 of 98, 44 | 49 of 98, 44 |
| CQs at 15 dB | 73 of 84, 62 | 73 of 84, 62 |
| CQs at 5 dB | 0 of 84, 0 | 0 of 84, 0 |
| CQs at 0 dB | 0 of 84, 0 | 0 of 84, 0 |
| twelve-plus-three | 318 of 353, **297** | 318 of 353, **297** |
| W1AW | 2131 of 2151, **2111** | 2131 of 2151, **2111** |

**Both scoreboards' rows:**

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| HEAD | 318 of 353 | 20 | 1 | **297** | 0 | 83 of 110, 3 added |
| handover on, lane contrast 10 dB (not shipped) | 313 of 353 | 17 | 15 | 281 | 0 | 85 of 110, 3 added |
| handover on, lane contrast 12 dB (not shipped) | 313 of 353 | 21 | 1 | 291 | 0 | 84 of 110, 3 added |
| 569 (bank on, handover off) | 318 of 353 | 20 | 1 | **297** (234 without) | 0 | 83 of 110, 3 added |

| w1aw | score | right | wrong | invented | spaces |
|---|---|---|---|---|---|
| HEAD | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |
| handover on (not shipped) | 2111 | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |
| 569 (bank on, handover off) | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |

With the handover on at 12 dB, the random carrier printed at seeds 5195 and 5209 (`NE T E I EEEEI E E EI E T IE N E EI`).

## 4. What's blocking us

- **The handover costs the twelve-plus-three six letters.** 221530 loses `EN`, 221805 loses `ET ON 40T S`, and 143906 and 143951 move. Where the grid has nothing within 50 Hz but did not print either, the bank now prints over a station's opening.
- **The weak fading fixtures** (QSB 12 dB at 0.7 Hz) read junk or nothing either way. On at least two the grid stands and keeps them under the staged rule, and the bank's three-second key-up and key-down do not follow a fade.
- **A random carrier** at 24 dB is clean CW-shaped keying to the bank. The grid's own guards held it to one seed, and with the handover on a second seed printed.
- **0 dB:** the hand-opened window reads the 12 and 18 WPM CQs 12 to 17 of 21. The bank at 12 dB of lane contrast calls little there, 0 to 3 of 21 with the handover on.
- **`two-station`** reads nothing with the handover off, and `FG CQ DE N■N ALL K ■CALL DE W1XYZ K`, 21 of 27, with it on.
- **The bank runs at 15% of the chain's CPU** and hands nothing on while the handover is off.
- **`TheRsidDetectorTests.TheDetectorKeepsUpWithRealTime`** is red on the engine line, and the two reply tests (`TAW`, `IAN`) are red at HEAD.

### Asks still outstanding

- **Unit 569, 2026-10-09:** whether the handover ships on at its cost (291 against 297, a second carrier seed printing) for the 5 dB CQs read 15 to 17 of 21, or the next unit narrows its cost first. Waiting on the owner. The change sits in the tree behind `CwEnvelopeDetector.LaneHandover`, off.
- **Unit 562, 2026-10-08:** what next for the junk before a weak call, given the 0.4 start costs the start of every real call. Waiting on the owner. No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
