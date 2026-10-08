## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks (`SHACK_FACTS.md`, `CwSenderLane.cs`, `cw-2026-10-08-121324.wav` present; no `CoreHMI.sln`, no `MURC.sln`) and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 10:45:45 through `tools\arbiter\lock.bat take` and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. R88 lifted only as the order named; **KM3STU's three recordings and their sheets are committed** beside the tests that read them. Version 1.13.243 to **1.13.244**. Ruling **HM-DEC-263**, the number the order gave. Nothing was recorded under §12.1. Every test run was one filtered invocation; scripts went in `.run-unit\unit559-*.sh`, not committed.

**Task 1, KM3STU reads from his CQ** (`f0f53954`): cause found; the windows still miss the bar and stay off.
- **The trace:** `Km3stuReadsFromHisCqTests.TheFirstRecordingSecondBySecond` reads `cw-2026-10-08-121324` cold through the app's chain. Each second it shows the plain read's marks at 800 Hz, the grid's offered marks within 30 Hz of it, the sequences there, the marks that stood, the gate's senders and the windows. It reads through two read-only views added for traces: `OfferedNow` and `SequencesNow`.
- **The cause, windows off:** at 800 Hz, 200 Hz off the pitch and 50 Hz inside the filter's edge:
  - **The grid broke or missed his dahs:** 85 + 20 where the plain read has 154 and 156.
  - **It called noise humps beside his dits:** 20 to 35 ms, at 2 to 7 dB of their own contrast, in the gaps his AGC lifts.
  - **So his shape stayed too low to print.** His sequence mixed the humps with his 45 ms dits and scored 0.03 to 0.2, under the 0.1 a sender is let go at and never picked under. He reached 0.50 once, at 19 s. Nothing printed.
  - **The difference that matters** is neither the tone's offset itself nor the level. It is noise humps called as marks, plus broken dahs.
- **The windows on:** a provisional window opened on him at 15 s and read his dahs whole.
  - It called the same humps (about −24 dB against his −19.5).
  - Through his last four marks, those humps pulled his own window's pitch to 831 Hz, 31 Hz off his tone. His shape fell to nought, and a shadow at 831 qualified.
- **The fix, from what a key does:** a mark read by level **in a provisional window** must have its top within twice a flat top's wobble, 3 dB, of its candidate's level. A held key holds its level; a hump peaks under it.
  - In the station's own window the same rule read 236 and 235, with `?` and `E` inside `221502`'s silence, even when held to marks shorter than a dit. So it is in provisional windows only.
- **With it:** his window stays at 799 to 800 Hz and he prints from 18 s at shape 0.75 to 0.85.
  - The first recording reads `INOTA DE KM3STU K KQ4PAK`, 18 of 19, and `143906` reads `… QSYDEWB2FU`.
  - **But the windows still miss the bar:** the original twelve read 234 (237 needed), w1aw 2111 (2112 needed), and the first recording reads `NOTA DE`, not `POTA DE`. **So the windows stay off**, the new rule with them.
- **The board:** his three recordings join the twelve's table as three stretches at 800 Hz, one a recording, since stretches side by side at one pitch take each other's edge letters.
  - `121324` 14.6 to 30 s, medium: `POTA DE KM3STU K KQ 4PA K`. Its CQ alone would be high.
  - `121357` 14.3 to 29.3 s, high: `55N 55N VA VA BK BK`.
  - `121414` 0.3 to 24.1 s, high: `R 55N 55N VA VA BK BK TU RON VA ES 72 KM3STU`.
  - The scoreboard now prints the total without them as well.

**Task 2, the low-confidence stretches:** dropped. They were not re-read.

**Records:**
- `docs\cw-scoreboard.md`: a row in both tables; the twelve's row records 275 with KM3STU's three.
- `PHASE_OUTCOME.md`, both copies: `## UNIT 559 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 559.
- `Directory.Build.props`: 1.13.244.
- `CLAUDE.md` §1: a row.
- `DECISIONS.md`: HM-DEC-263.

**Build** `Hamlet.sln -warnaserror`: 0 warnings, 0 errors.

**Both guards:** 4 of 4. **The decision-log and voice tests:** 7 of 7.

**App carry-forward:** 277 of 278. The one failure, `Unit376TheTopBandTests`, was `You've caused dispatcher loop`, and it passes alone.

## 2. What the owner should expect

Rebuild and run as usual. **KM3STU's CQ still reads as it did live, because the fix that reads it is still switched off.**

- **Why his CQ went unread:**
  - **The coarse search broke his dashes.** He was 200 Hz off your pitch, near the edge of your 500 Hz filter. There Hamlet's coarse search across the band read his dashes in pieces or missed them.
  - **It took noise for dots.** The radio's AGC lifts the noise between his letters, and Hamlet took some of those noise bumps as tiny dots.
  - **Nothing printed.** Mixed with his real dots, those bumps made him look nothing like Morse to the part that decides who to print.
- **What the early windows did:** they read his dashes whole. But they picked up the same noise bumps, which pulled his window 31 Hz off his tone and lost him again.
- **The fix:** a mark has to sit at the station's own level, because a held key holds its level and noise only bumps up under it. With that, and the windows on, the first recording reads `INOTA DE KM3STU K KQ4PAK`. The weak fast station reads `QSY DE WB2FU` at last.
- **Why it's still off:** the windows still cost 3 letters on your twelve original recordings and 1 on W1AW, and his `POTA` still comes out `NOTA`. Your rule is that neither scoreboard may fall, so they stay off.
- **The scoreboard:** KM3STU's three recordings are now on it.
  - With the windows off it reads **275 including them, and 237 without**, the same as before, so nothing went backwards.
  - With the windows on it would read 297 including them, but 234 without.
  - The scoreboard now prints both totals, so a bigger yardstick isn't mistaken for a better Hamlet.
- **W1AW:** 2112, unchanged.
- Pushed to `main`, with the three recordings.

## 3. What you should see

**Both tables' rows:**

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| HEAD, the twelve | 254 of 288 | 17 | 0 | **237** | 0 | 67 of 87, 3 added |
| 559 task 1, with KM3STU's three | 304 of 353 | 29 | 0 | **275** | 0 | 79 of 110, 6 added |
| 559 task 1, without them | 254 of 288 | 17 | 0 | **237** | 0 | 67 of 87, 3 added |

| w1aw | score | right | wrong | invented | spaces |
|---|---|---|---|---|---|
| HEAD | **2112** | 2131 of 2151 | 19 | 0 | 438 of 440, 11 added |
| 559 task 1 | **2112** | 2131 of 2151 | 19 | 0 | 438 of 440, 11 added |

With the windows on (as built, off): 297 with KM3STU's three, **234** without (254 right, 19 wrong, 1 invented on `221851`), w1aw **2111**, nothing in a silence.

**The first recording, second by second, windows off** (plain read at 800 Hz; what the grid offered within 30 Hz; his sequence; the gate):

| s | plain | grid offered | sequence near 800 | gate |
|---|---|---|---|---|
| 15 | `23. 20. 78- 154-` | | 792 Hz, 3 marks [20 20 40], shape 0.62 | |
| 16 | `156- 59. 157- 172- 166-` | 20, 20, 40, 25 ms | 796 Hz STANDING [20 20 40 25 85 20 145], 0.40 | 800 Hz shape 0.36, 6 marks |
| 17 | `159- 55. 158-` | 85, 20 (c2.1), 145, 20 (c0.4) | [… 145 20 50], one length, 0.17 | 798 Hz shape 0.16 |
| 18 | `159- 159- 62.` | 50, 25 (c1.0), 45 | [… 25 45 45], 0.07 | 799 Hz shape 0.08 |
| 19 | `158- 56. 161- 158- 159-` | 20, 45, 45 | [… 45 145], 0.06 | 800 Hz shape 0.50, qualified |
| 20 | `20. 52. 52. 57. 163- 154- 51.` | 25, 145, 45, 45, 45 | 19 marks, 0.03 | 798 Hz shape 0.05 |
| 21 | `54. 55. 158- 58. 55. 160-` | 25 (c0.5), 20, 45 | 21 marks, 0.20 | 798 Hz shape 0.07 |

Printed: `IEU EE IE TSIE A EAEE IAGAK`, none of it at his CQ.

**Windows on, before the level rule:**

| s | gate | windows / his own window |
|---|---|---|
| 16 | 799 Hz shape 0.19 | window 800 Hz, 4 marks |
| 17 | 799 Hz shape 0.71 | window 799 Hz, 8 marks |
| 18 | 799 Hz shape 0.73, PRINTED | his own window at **831 Hz** |
| 21 | 800 Hz shape 0.74; 831 Hz shadow 0.79, qualified | 800 Hz |
| 22 | 800 Hz shape 0.13 (humps 23 to 34 ms in its marks) | 800 Hz |

Printed: `EENOTA IE E RTT K KIAGAK`.

**Windows on, with the level rule:**

| s | gate | his own window |
|---|---|---|
| 16 | 799 Hz shape 0.19 | (window 800 Hz) |
| 17 | 799 Hz shape 0.71 | (window 799 Hz) |
| 18 | 799 Hz shape 0.78, PRINTED | 800 Hz |
| 19 to 22 | 799 Hz shape 0.82, 0.85, 0.75, 0.77, PRINTED | 799 Hz |

Printed: `INOTA DE KM3STU K KQ4PAK`.

**The three recordings' text:**

| recording | live, 2026-10-08 | at HEAD (windows off) | windows on |
|---|---|---|---|
| `121324` | nothing | `IEU EE IE TSIE A EAEE IAGAK` (stretch 4 of 19, 12 wrong) | `INOTA DE KM3STU K KQ4PAK` (18 of 19) |
| `121357` | `NAT 4PAK IEE■ N TR I5N 55N VAVABK BK TU` (whole session) | `I E E IEE ESE RR UR 55N 55N VAVABK BKT` (14 of 14) | `I E E IEE ESE RR UR 55N 55NVAVABK BKT` (14 of 14) |
| `121414` | `… TURON VA ES72KM3STU` | `R 55N 55N VA VA BK BKTURON VA ES72KM3STUEE` (32 of 32) | the same |

## 4. What's blocking us

- **The candidate windows:** with the level rule they read KM3STU's CQ and the weak fast station's call. On the original twelve they still lose 3 (one borderline gap inside `221530`'s O, one letter invented on `221851`), and w1aw loses 1.
- **`POTA` reads `NOTA`** with the windows on. P's first dit comes before his window has opened.
- **The low-confidence stretches** (task 2) were not re-read.

### Asks still outstanding

- **Unit 557, 2026-10-08:** whether Hamlet's fastest readable speed is 48 WPM, as `CwDecoder.FastestPlausibleWpm` has it (the radio keyer's fastest), or 40 WPM, as work instruction 557 assumed. It waits on the owner. No change for it sits in the tree; the floor measured from either number is reverted.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
