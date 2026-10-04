```
UNIT: 535 - tasks 1, 3 and 4 done; task 2 measured, red, not shipped - 2026-10-04
UNIT GOAL: the light claims no more than the printer, and a random carrier never prints
NUMBER: scoreboard 185 to 205 of 244 (240 before the references were corrected); the carrier limit is red
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`, and the order's gate held: `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `cw-2026-10-03-221828.wav` exist, there is no `CoreHMI.sln` or `MURC.sln`, and the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio beyond the owner's twelve recordings. HM-DEC-239 was free.

**How the session ran:**
- It took SESSION.lock and released it at the end.
- It wrote nothing to `RUN_LEDGER.md`, touched nothing under `tools\arbiter\`, and ticked no box.
- R88 was lifted for the twelve recordings and no other. Nothing keys, transmits or writes to the radio.
- The scoreboard was run after every task, and its row is appended to `docs\cw-scoreboard.md`.

**Task 1: the light is green only when Hamlet would print.** Commit `f28f8865`.
- The gate now publishes the pitch of the sender that has qualified and is waiting out its first word gap (`WaitingPitchHz`), beside the printed one. The decoder passes it on, and the app wires it to the detector as it wires the printed pitch.
- The light reads only those:
  - **`reading`** while a sender is printed;
  - **`shape found · hold here`** while one waits;
  - **amber** while marks stand or form;
  - **dark** otherwise.

  Green also needs a mark to have stood within the hold, so the light goes dark when the sender stops.
- No new number. The gauge stays at or under the mark while amber.
- **Watched fail first:** `LoudNoiseIsNeverGreen` (seed 5212) was red, green for 377 steps; it now passes with 0 green steps.
- **The scoreboard is unchanged by this task, 185 of 240**, as it should be for a display change.

**Task 2: a carrier keyed at random never prints.** Commit `78449f4e` (tests and the limit only; the engine unchanged).
- **The limit is in place:**
  - `ARandomCarrierAloneNeverPrints` at twenty seeds;
  - five carrier runs in the scoreboard's hard limits.

  **Both are red.** At HEAD, 9 of 20 seeds print alone, and 2 of the scoreboard's 5.
- **Which rule lets it print.** The carrier qualifies on two kinds that it shows only:
  - over its newest five marks, where the speed retry looks;
  - or as one odd mark against the rest: seed 5197 qualified on one 45 ms mark against nine from 135 to 275 ms.
- **Rules built and measured** (each from what CW is; none tuned to a seed):

| rule | scoreboard | carrier alone, of 20 | fate |
|---|---|---|---|
| none (HEAD) | 185 | 9 print | |
| two kinds held over the last ten marks | 185 | not run at 20 (1 of the first 5 seeds, 5197) | |
| ... over the last sixteen marks | 156 | not run at 20 (0 of the first 5) | lowers the total |
| ... ten, and a pick floor at the release line (0.1) | 180 | not run at 20 (5197 still printed) | lowers the total |
| **... ten, each kind recurring (two marks or more)** | **185** | **2 print** (5195, 5206) | **best; does not close the case** |
| ... and over the ten before as well | 150 (first recording broken) | 0 print | lowers the total |
| ... ten, each recurring, and the gaps showing a clean 2:1 jump | 181 | 1 prints (5195) | lowers the total |

  **No rule closes the case without lowering the total, so none ships, as the order requires.** The best rule costs nothing and leaves 2 of 20 printing; it is described in HM-DEC-239 and is not in the tree.
- **Beside a clean sender at 100, 150 and 200 Hz:**
  - At HEAD the carrier prints at 100 Hz (`ARandomCarrierNeverPrints(725)` red); the best rule closed it.
  - The clean sender reads as at HEAD: at 400 Hz it reads whole; closer, it reads garbage as before.

**Task 3: two references corrected.** Commit `09f512d1`.
- **What the audio says.** Read element by element offline, the sign-off of `cw-2026-10-03-221828` and the start of `-221851` is a 7: a 175 ms dah, a 147 ms gap, a 20 ms fragment, then 119, 87, 62 and 80 ms. It is followed by `...--`, `.-.-.`, `.--`, `..---` and `.-..`: `73 <AR> W2L`. The web session had written `EEV CW`.
- **What changed:**
  - 221828's reference ends `ES BEST 73 <AR> W`; its `2` is cut off by the end of the recording.
  - 221851's reference is `BEST 73 <AR> W2L CQ DE NA8SB K`.
  - Each element list now carries the corrected span, and confidence is unchanged. No other reference changed.
- **The yardstick moved, not Hamlet:** 185 of 240 before the correction, 185 of 244 after.

**Task 4: heavy keying turns FER into ENER.** Commit `97247000`.
- **Where the F's marks go**, on `cw-2026-10-03-221805` at 22.87 s:

| | dit | gap | dit | gap | dah | gap | dit |
|---|---|---|---|---|---|---|---|
| offline | 73 | 17 | 103 | 27 | 251 | 39 | 94 |
| detector's candidates | 66 | | 94 | | 243 | | 87 |
| stood | 66 | | - | | 243 | | 87 |
| gate's letters | `E` (66) | 161 ms gap | | | `N` (243, 87) | | |

- **The cause is plain.** The pattern gate's crowds test drops a mark that starts within half a dit of the last one as "the same tone read twice". On heavy keying, gaps inside a letter are a quarter to a half of a dit, so the F's 94 ms second dit, 27 ms after the first, was dropped.
- **The fix:** a mark crowds the last only where it overlaps it, or where it is both under half a dit after it and under half a dit long. A tone read twice overlaps or touches itself, and a piece of a tone is short.
- **`FER` reads** on both recordings, and **the scoreboard rose from 185 to 205 of 244.**

**Records:**
- HM-DEC-239 in `DECISIONS.md`, and the `CLAUDE.md` row.
- `PHASE_OUTCOME` (both copies) has `## UNIT 535 - STEP 12`.
- `PHASE_STATUS` (both copies) names 535.
- Version 1.13.219 to 1.13.220.
- `docs\cw-scoreboard.md` has five new rows and the unit's stretch table.

**Build and app line:** build 0 warnings, 0 errors. App carry-forward 277 of 278. The one loss, `TheStopIsAlwaysOnScreenTests…` at 1 ms ("You've caused dispatcher loop"), passes alone.

## 2. What the owner should expect

- **Rebuild.**
- **The light goes green only when Hamlet will print.**
  - "Shape found · hold here" now means a station has qualified and its letters print within a word gap.
  - "Reading" means it is printing.
  - Amber "not yet" covers the seconds while a station qualifies.
  - On loud noise it never goes green.
- **A random carrier can still print.** It is now a hard limit, and the limit is red. The best rule I found cost nothing on your recordings but still let 2 of 20 random carriers through, so by your order it was not shipped. Your call is in section 4.
- **Your recordings: 185 to 205 of 244.**
  - **The 22:17 QSO reads `FER` again:**
    - `TNX FER ANOTHER` on 22:18:05;
    - `FER ANOTHER FB QSO ES HOPE U HAVE AGN ED ES BEST` on 22:18:28 (26 to 37 of 41 letters).
  - **22:17:45** now ends `EUROWEEHIRDTOO`.
  - **22:15:30** reads `HCHAMPIMTNBK` (was `HCSAMENIONNEN`).
- **The scoreboard's yardstick changed once.** The end of 22:18:28 and the start of 22:18:51 are `73 <AR> W2L`, as you and the web session agreed, so the total is now out of 244 rather than 240.

## 3. What you should see

**The scoreboard rows:**

| row | total | hard limits |
|---|---|---|
| 534 after (HEAD) | 185 of 240 | hold (no carrier limit then) |
| 535 task 1 | 185 of 240 | hold |
| 535 task 2 | 185 of 240 | carrier limit red: 2 of 5 print |
| 535 task 3, before the correction | 185 of 240 | carrier limit red |
| 535 task 3, after the correction | 185 of 244 | carrier limit red |
| 535 task 4 | **205 of 244** | carrier limit red, as before |

**Every stretch, on the corrected references, before task 4 and after:**

| recording | pitch | before | right | after | right |
|---|---|---|---|---|---|
| 200157 | 662.8 | `FERCHAT<BT>BEST7V73<SK>KC4ZGPDEWA` | 27/27 | same | 27/27 |
| 143906 | 514.2 | `SEIIENUVIQSYQSYDEWB2FUVEE` | - | `SEIIENUVIQSYQSYDEWB2FUHEE5` | - |
| 143951 | 499.5 | `EOMTTTOMOTMOEES` | 4/16 | `EOMTTTOOOTMGEES` | 4/16 |
| 144020 | 499.5 | `ESOKONPA<BT>` | 9/9 | same | 9/9 |
| 144020 | 599.9 | `WXINNETAGITIENTEMP` | 17/19 | same | 17/19 |
| 144045 | 599.9 | `NEMP5757<BT>BTUBOBDEKG8VK` | 22/23 | same | 22/23 |
| 221502 | 491.5 | `EDOFITSMTWNHEEBKBKWHATBUGAEIEUIE■■` | 27/42 | same | 27/42 |
| 221530 | 491.5 | `HCSAMENIONNEN` | 5/11 | `HCHAMPIMTNBK` | 8/11 |
| 221530 | 598.4 | `IHESMHENI■SAGE12ILEAREDCWUSINGAV` | 27/34 | same | 27/34 |
| 221548 | 597.7 | `IMEEEAREDCWUSINGAVBPLXZEPS` | 22/26 | same | 22/26 |
| 221548 | 498.0 | `EWRHEEMYSCOETTQSTEN` | 13/18 | same | 13/18 |
| 221745 | 501.7 | `TEMMTONITE.EIEEMESIRDTOTT` | 14/29 | `TEMMTONITE.EUROWEEHIRDTOO` | 17/29 |
| 221805 | 601.3 | `E40TUTHESEDAYI.TNXENERANOTHERF` | 25/33 | `E40TUTHESEDAYS.TNXFERANOTHERF` | 28/33 |
| 221828 | 601.3 | `ENERANOTHERFNEQSMESHMWETEIEVEATNEDESBEST` | 26/41 | `FERANOTHERFBQSOESHOPEUHAVEAGNEDESBESTEVAMU` | 37/41 |
| 221851 | 601.3 | `TTTMEEEUIEMTICWJLKTADENAMEEENEK■DETSKETEER5SEXIEEESIIIIEEEI` | 7/20 | `TTTMEESSTEARWJLCQDENA8SBN■DET■EIETEERHIESESEEIEEIISEEI` | 13/20 |
| **total** | | | **185/244** | | **205/244** |

**The light's sequence:**
- **On loud noise (seed 5212):**
  - before: green for 377 steps;
  - now: green for 0 steps, amber while noise forms (26% of the time at one mark or more), never past the mark.
- **On a clean call:**
  - 3.32 to 3.68 s, `shape forming · 2, 3, 4 of 5`;
  - 4.04 s, `shape forming · not yet`;
  - 6.09 s, `shape found · hold here`;
  - 6.39 s, `reading`, the first letter printed at 6.37 s;
  - 19.34 s, amber;
  - 19.48 s, `listening`.

  Before, it went green at the fifth mark, two seconds before the gate would print.

**The carrier cases at HEAD** (the engine was not changed for them):

| case | reads |
|---|---|
| alone, 20 seeds | 9 print, e.g. seed 5205 `NITMTMT E NT MT E EMTEFETTUTTEAN`, seed 5206 `TTMET ETTMTNEEEAATAUTTAT TOMOTEG` |
| scoreboard, 5 seeds | 5195 `TMTTTNEAITT ETANTTTTK`, 5197 `TTMTTIETET MTT T KT TM` |
| beside a clean sender, 100 Hz | the carrier prints (red) |
| beside, 150 / 200 / 400 Hz | the carrier prints nothing; the clean sender reads as at HEAD |

**The references changed** (task 3):

| recording | was | now | element list there |
|---|---|---|---|
| 221828 | `… ED ES BEST` | `… ED ES BEST 73 <AR> W` | `- [222] --... [190] ...-- [128] .-.-. [195] .-- [168] ..--` |
| 221851 | `BEST EEV CW 2L CQ DE NA8SB K` | `BEST 73 <AR> W2L CQ DE NA8SB K` | `- [223] --... [190] ...-- [129] .-.-. [195] .-- [169] ..--- [160] .-..` |

**The FER trace** is in section 1, task 4.

**Synthetic sets, against unit 534:** sets a, b1, b4 and b5 read identically, line for line, except for the twenty new carrier lines and two untotalled recording lines. The reds are the same set as unit 534's, plus this unit's carrier cases.

## 4. What's blocking us

1. **The random-carrier limit is red.** The best rule from CW's own pattern costs nothing on your recordings and closes 7 of the 9 seeds that print. Should it ship as a partial step?

   | option | for | against |
   |---|---|---|
   | A. ship it now | 7 of 9 printing seeds silenced, 100 Hz beside closed, 0 letters lost | the hard limit stays red at 2 seeds |
   | B. keep the case red until one rule closes it | the order as written | carriers keep printing at 9 of 20 meanwhile |

   The usual practice is A: a change that loses nothing and removes most of a fault ships, and the limit stays red to show what is left.
2. **Gap kinds close one more seed but cost 4 letters** on your recordings. Where those 4 letters go is the next question if the carrier is to close fully.
3. **The other reference differences** from unit 534 still wait for you to listen; only the sign-off changed.
4. **Pre-existing app reds** outside the line are untouched.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
