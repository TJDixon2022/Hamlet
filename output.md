```
UNIT: 528 - tasks 1, 2 and 5 done; task 3 built, measured and not kept; task 4 dropped with its cause - 2026-10-02
UNIT GOAL: the owner's recording reads
NUMBER: CHAT reads; the recording does not yet read whole (TheOwnersRecordingReads red)
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`, and the order's gate held: `SHACK_FACTS.md`, `CwPatternGate.cs` and `tests\fixtures\cw\captured\cw-2026-10-02-200157.wav` exist, there is no `CoreHMI.sln` or `MURC.sln`, and the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio, beyond the owner's one recording. HM-DEC-232 was free.

**How the session ran:**
- It took SESSION.lock and released it at the end.
- It wrote nothing to `RUN_LEDGER.md`, touched nothing under `tools\arbiter\`, and ticked no box.
- **R88 was lifted by the owner for `cw-2026-10-02-200157.wav` alone.** No other recording, fixture or telemetry was read.
- Nothing keys, transmits or writes to the radio.
- The WAV and its `.txt` are committed beside the test that reads them. `cases-2026-10-02.txt` is untracked and was left alone.

**Task 1: the faults, found before anything changed.** The test is `TheOwnersRecordingReadsTests`, which runs the detector, gate and reader as the app wires them, passband 600 and 500. At HEAD it read `F ER C H ET<BT> BESEMSVE Y <SK> KC4 Z GP D`.

| fault | where | what did it |
|---|---|---|
| A of CHAT read as a dit | detector, `Run`'s settle rule | the top rose over six hops, and settling took the rise as the top. The run began 40 ms early and read 250 ms, and the 35 ms gap it left to the next dit failed the gate's `Crowds` check |
| T of BEST read as a dit | detector, per-hop flatness | a 210 ms top wobbling 3 to 4 dB at 13 dB contrast broke the run; only its last 40 ms (11.725 s) was called |
| 73 after the pause lost | detector | the 7 drew no candidate, the same flatness break; the 3 read Y (70, 35, 155, 210 ms) |
| KC4 Z | gate, `GapLines` | gaps of 425 and 405 ms passed the five-dit floor (375 ms) |

**Also found:**
- the 7 before V reads M S in the reader, because its 120 ms inner gap is over the element line;
- early words split on a letter cluster of two gaps.

**Task 2: a settling top only comes down.** Commit `6b30fd4d`.
- **The change, in `CwEnvelopeDetector`:** a settling run refuses a hop that climbs past the highest of its first window (`RiseHops - 1`) by more than its tolerance. An AGC overshoot only comes down; a slow rise is not a top.
- **The recording:** CHAT reads, and the closing EWA now prints. The G of GP lost its dit and reads M.
- **New case:** `AStationBetweenTwoBinsReadsItsDahs` at 612.5, 637.5 and 662.5 Hz, through the filter with a 1 dB overshoot.
- **Regressions:** every existing case reads as before. Only candidate counts and shape scores moved, by 0.001.

**Task 3: built, measured and not kept.** Commit `e557a990` keeps the measurement only.
- **Built as ordered:** the five-dit floor retired; the word line drawn as the boundary between the sender's own letter and word clusters, or √(7/3) of the letter centre where only that cluster shows.
- **On the recording:** the line came out at 563 ms through the body, 14.9 to 25.9 s, inside 430 to 580; KC4ZGP read whole. Elsewhere it came out at 220, 731, 1648, 600 to 712 and 276 ms (table in section 3).
- **It broke six existing cases**, the ones unit 526's floor fixed: `BROWNFOX` and `2024AND` on the drifting hand, and `DEN0CALLN0CALLK` on the straight key, three rows each.
- **What stays:** the order requires every case to read as at HEAD or better, so the floor stays. **Keeping it is my report, not your ruling**; the ask is in section 4. The test now prints the line and both clusters at every letter. Its final line is 300 ms with the floor.

**Task 4: dropped, with its cause.** The 7 after the 1.23 s pause draws no candidate at all in the detector. It is the same per-hop flatness break as the T of BEST, not the reader releasing the sender or the banking.

**Task 5: bench AGC at 1 dB.** Commit `3242c7e4`.
- **The premise was false:** no shared default exists, because unit 527 dropped it. The 3 dB was in `AHandIsReadAgainstItselfTests.Under`, set by unit 526.
- **The change:** `agc`, `agc-filter` and `weak-agc-filter` are now 1 dB. `agc2-filter` and `agc3-filter` are new named stress rows on all five theories.

**Records:**
- HM-DEC-232 in `DECISIONS.md`, naming the R88 exception, the floor's retirement built and not kept, and the bench AGC correction.
- The `CLAUDE.md` index row.
- `PHASE_OUTCOME` (both copies) has `## UNIT 528 - STEP 12`.
- `PHASE_STATUS` (both copies) names 528.
- Version 1.13.212 to 1.13.213.

**Build and app line:** build 0 warnings, 0 errors. App carry-forward 278 of 278.

## 2. What the owner should expect

- **Rebuild.**
- **What the recording reads now, beside what was sent:**
  - sent: `FER CHAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`
  - now: `F ER C H AT<BT> BESEMSV E Y <SK> KC4 Z MPDEWA`
- **A station between two bins reads its dahs.** CHAT is right, and a clean sender at 612.5, 637.5 or 662.5 Hz through your filter reads whole. The settle fix also brings the closing DEWA through, but costs the G of GP its dit.
- **The callsign is still split by a long letter gap.** Retiring the five-dit floor fixed KC4ZGP but broke the drifting hand and the straight key, which that floor had fixed yesterday, so it is not in. It needs your ruling (section 4).
- **The T of BEST and the 73 are still lost.** A dah whose top wobbles 3 to 4 dB on a 13 dB signal breaks the detector's flatness test.

## 3. What you should see

**The recording's text:**

| | text |
|---|---|
| sent | `FER CHAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` |
| live, as you saw it | `FER CHET<BT> BESE7V E ■ <SK> KC4 Z GP DEWA` |
| HEAD before 528 (test) | `F ER C H ET<BT> BESEMSVE Y <SK> KC4 Z GP D` |
| now | `F ER C H AT<BT> BESEMSV E Y <SK> KC4 Z MPDEWA` |
| with the floor retired (not kept) | `F ER C H AT<BT> BESEMSVE Y <SK> KC4ZMPDEWA` |

**The elements as Hamlet found them now** (start, length, pitch bin; gaps over 105 ms in brackets):

| sent | found |
|---|---|
| F `..-.` | 0.165 75 · 0.315 75 · 0.465 210 · 0.750 75 |
| E | [285] 1.110 75 |
| R `.-.` | [170] 1.355 80 · 1.510 205 · 1.770 100 |
| C `-.-.` | [930] 2.800 210 · 3.085 75 · 3.235 210 · 3.520 75 |
| H | [585] 4.180 65 · 4.320 80 · 4.480 75 · 4.630 75 |
| A `.-` | [290] 4.995 80 · 5.150 210 (**was the fault, now a dah**) |
| T | [395] 5.755 210 |
| `<BT>` | [435] 6.400 205 · 6.680 80 · 6.830 80 · 6.980 80 · 7.130 215 |
| B | [2295] 9.640 210 · 9.925 75 · 10.080 75 · 10.230 65 |
| E, S | [275] 10.570 70 · [260] 10.900 70 · 11.045 75 · 11.195 75 |
| T | [455] 11.725 **40** (the last 40 ms of a 210 ms dah) |
| 7 `--...` | [690] 12.455 210 · 12.740 210 · [120] 13.070 75 · 13.225 75 · 13.375 70 (M S) |
| V | [380] 13.825 45 · 13.960 75 · 14.110 80 · 14.260 210 |
| 7 `--...` | nothing found 14.9 to 17.0 |
| 3 `...--` | 17.090 70 · 17.280 35 · 17.445 155 · 17.675 210 (Y) |
| `<SK>` | [965] 18.850 70 · 19.000 75 · 19.190 25 · 19.285 225 · 19.565 95 · 19.730 215 |
| K C 4 Z | [830] 20.775 … [325] 21.750 … [370] 22.920 … [425] 24.160 … (as sent) |
| G `--.` | [405] 25.360 210 · 25.645 210 (its dit lost: M) |
| P D E W A | [440] 26.295 … [595] 27.685 … as sent |

**The word line for this sender, task 3 as built** (floor retired):

| at | line | letter cluster | word cluster |
|---|---|---|---|
| 0.8 to 5.4 s | 220 ms | [170] | [285, 290] |
| 6.0 to 11.3 s | 717 to 731 ms | [260 … 585] | [930] |
| 11.8 to 14.5 s | 1625 to 1648 ms | [260 … 930] | [2295], the pause |
| **14.9 to 25.9 s** | **563 ms** | [325, 370, 405, 425] | [830] |
| 27.1 to 28.5 s | 600 to 712 ms | [325 … 595] | none |
| 29.3 to 29.9 s | 276 ms | [180, 195, 215] | [405 … 595] |

Between 430 and 580 only through the body.

**The between-bin cases (new):**

| pitch | through filter, 1 dB AGC |
|---|---|
| 612.5 Hz | 70 stood, `FER CHAT BEST 73 KC4ZGP DE WA` |
| 637.5 Hz | 70 stood, `FER CHAT BEST 73 KC4ZGP DE WA` |
| 662.5 Hz | 70 stood, `FER CHAT BEST 73 KC4ZGP DE WA` |

**The existing cases:**
- **After task 2:** every reading in sets a, b1, b3, b4 and b5 is unchanged from unit 526, apart from candidate counts and shape scores moving by 0.001. Two letters changed inside an already-red test: case 2's `EEN■` became `E N■`, and its 800 Hz `EEE` became `EE`.
- **The reds are unit 526's:**
  - the call at 8 dB;
  - Farnsworth and fast at 10 dB;
  - a letter from noise (blocks);
  - the random carrier at 725 and 775;
  - the clean sender at the edge.
- **Task 3 as built (not kept):**

| case | at HEAD | floor retired |
|---|---|---|
| drifting hand, plain, agc, agc-filter | `… 2024 AND THE QUICK BROWN FOX …` | `… 2024AND THE QUICK BROWNFOX …` |
| drifting hand, weak | `… 2024 A E I HE QUICK …` | `… 2024AE I HEQUICKBROWNFOXJUMPSOVER …` |
| straight key, plain, agc, agc-filter | `CQ CQ SKCC DE N0CALL N0CALLK` | `CQ CQ SKCC DEN0CALLN0CALLK` |

- **Task 5, 1 dB against 3 dB:**

| case | 3 dB | 1 dB |
|---|---|---|
| drifting hand, weak-agc-filter | `… 2024 A E I HE QUICK … LAZY TIOG` | whole |
| straight key, weak-agc-filter | `CK■ASKCC DE N0CALL N0CALLK` | `CQ CQ SKCC DE N0CALL N0CALLK` |
| 25, 35, 25 WPM, weak-agc-filter | whole, 175 printed | `TEXT IS FROM SEPTEMBER 2024 AND I SFE`, 81 printed, **red** |
| every `agc` and `agc-filter` row | as before | unchanged |
| new `agc2-filter`, `agc3-filter` rows (10) | — | all whole |

## 4. What's blocking us

1. **The word line: rule on the five-dit floor.** Ruling asked: *the five-dit floor stays until the sender's clusters are measured right; the clusters are fixed first.* Why: the order's line is right where the clusters are right (563 ms), but the clusters are mis-measured for this hand. Two gaps at the start, a 2.3 s pause taken as the only word gap, and letter gaps split at 215 to 405 ms at the end. Retiring the floor now breaks six cases unit 526 fixed. Rejected: retiring it anyway, which costs BROWN FOX and DE N0CALL to gain KC4ZGP.
2. **The detector breaks a wobbling top.** The T of BEST and the 7 after the pause draw no full mark at 13 dB contrast, because per-hop flatness is broken by a 3 to 4 dB wobble. A below-mean tolerance was tried in task 2 and made the recording worse.
3. **The 7 before V reads M S.** A 120 ms gap inside a letter of a hand whose letter gaps start at 150 ms.
4. **G of GP lost its dit** after the settle fix: a 100 ms dit at −19.7 dB after a 50 ms gap.
5. **The weak 25-35-25 WPM row at 1 dB is red**, printing 81 of 194 stood marks.
6. **Pre-existing app reds** are outside the carry-forward line and were not touched:
   - the scope's tone-line tests;
   - two British spellings (`centre`);
   - `DecisionLogOrderTests`' gaps check.

### Asks still outstanding

- **Unit 528, 2026-10-02:** the five-dit floor (item 1 above). The retired form is not in the tree; it is kept at `.run-unit/unit528-gate-retired.cs`, uncommitted.
- **Unit 522, 2026-10-01:** whether to switch shape-first on before its failures are fixed. Still off.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
