## 1. What Claude did

- **Session:** development computer, Claude Code. The prompt claimed `PROJECT: Hamlet`, and the order's gate held in `C:\Source\HamLet`:
  - present: `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `cw-2026-10-03-144045.wav`;
  - absent: `CoreHMI.sln` and `MURC.sln`.
- Nothing in this report is evidence about the radio.
- **Work instruction 538**, run by hand on `main`:
  - SESSION.lock was taken and released.
  - Nothing was written to RUN_LEDGER.md, nothing under `tools\arbiter\` was touched, and nothing in PHASE_PLAN.md was ticked.
  - Nothing was keyed or transmitted.
- **R88:** lifted for the owner's twelve recordings only. Nothing was tuned to a recording: every line is drawn from the sender's own gaps.
- **Version:** 1.13.222 → 1.13.223. **Ruling:** HM-DEC-242.

**Task 1: the scoreboard scores spaces** (commits `b424c44f`, `38ce1c18`).
- **How it scores:**
  - Each printed letter carries whether a word end opened it.
  - A stretch's printed letters are aligned to the reference letter by letter, with the alignment the letters score already uses.
  - Each boundary between two reference letters is read on the printed side, between the letters aligned either side of it.
  - A space in both is right, a space in the reference only is missing, and a space printed between two adjacent aligned letters the reference runs together is added.
- **The total** is spaces right over reference spaces, for stretches of medium confidence or better.
- **Baseline: 54 of 77, 6 added**, written as a new column in `docs\cw-scoreboard.md`.
- **Tests:**
  - `TheSpacesScoreCountsWhereTheWordsBreak` pins the scorer on six cases.
  - `EachStretchsWordBreaksReadOffline` reads each stretch's word breaks offline from its own gaps, beside the reference (section 3).
- **No reference was changed.**

**Task 2: the word line from the sender's own overlap** (commit `f898ee98`).
- **What was found:**
  - Where a hand's letter and word gaps overlap, no two neighbouring gaps differ by √(7/3), so the walk made every gap one letter cluster.
  - With no word cluster, the line was √(7/3) of that mixed cluster's centre.
  - On `144045` that was one cluster of 195 to 1163 ms, with the line at 958 ms.
- **What changed,** in `CwPatternGate`:
  - **The split.** With no clean jump, the gaps are split in two by log-length 2-means. They are kept as two kinds only where the two centres sit √(7/3) apart, the walk's own jump. A hand's letter gaps alone part at centres about 1.36 apart, which is under it.
  - **The crossing.** The word line, and the settle between the two clusters, are now the sender's own equal-error crossing: where in log-length a gap is as likely to belong to either cluster, given each one's centre and spread. This replaces the boundary that set a gap as many of one cluster's spreads from its centre as of the other's.
  - **Two rules,** `OverlapSplit` and `GapCrossing`, are in `CwRules`, so each can be measured alone.
- **The fault, shown first:** a synthetic hand at 20 WPM, with letter gaps of 3 to 5.5 dits and word gaps of 6 to 10, read 4, 7 and 7 of 14 word spaces at three seeds. It now reads 12, 14 and 13 (`AHandWhoseLetterAndWordGapsTouchKeepsItsWords`).
- **On `144045`:**
  - The clusters are now 195 to 709 ms and 925 to 1163 ms, with the line at 815 ms.
  - 8 of 8 reference spaces are read: `N E MP 57 57<BT> BTU BOB DE KG8V K`.
  - The order's `<BT> B TU` is not reached (section 4).
- **The bar:** spaces right went from 54 to 58, and letters from 205 to 206. Shipped.

**Task 3: the sender who spaced its letters** (commit `b8bb5774`).
- **Found in the recordings:** `cw-2026-10-03-221530` at 598 Hz printed `AGE 12ILEAR E D CW U SIN G A V`.
- **Why its line fell under its letter gaps:**
  - Its letter gaps run 111 to 171 ms. Its word cluster ran 184 to 1361 ms with a centre at 274, because pauses and the longer letter gaps were settled into it.
  - The spread-count boundary sits near the tighter cluster whenever the other is wide, so it drew the line to 160 ms, under its own letter gaps.
- **The fix is task 2's crossing,** which landed in that commit:
  - The line is now 241 ms and no space is added.
  - The cost is two real spaces on that stretch, 7 down to 5 of 12.
  - `TheSenderWhoSpacedItsLettersIsNotSplit` pins it: 3 spaces added with the crossing off, none with it on.
- **The order's synthetic case** (18 WPM, letter gaps 4 to 5 dits, word gaps 8 to 12) read whole before and after, so it stays as a guard.

**Recorded, at the owner's order:** HM-DEC-242 in `DECISIONS.md`, with its row in the `CLAUDE.md` §1 index. Full text:

> **A word line is where the sender's own letter and word gaps cross.** The line between a sender's letter gaps and its word gaps is drawn where a gap is as likely to belong to either cluster, given each cluster's centre and spread in log-length.
> - **When the gaps show no clean jump**, they are split in two by log-length 2-means. They are kept as two kinds only where the two centres sit √(7/3) apart, the walk's own jump.
> - **The same crossing settles the two clusters.** It replaces the boundary that set a gap as many of one cluster's spreads from its centre as of the other's.
>
> **The scoreboard scores spaces.** A new column, spaces right over the reference's spaces for stretches of medium confidence or better:
> - printed letters are aligned to the reference letter by letter;
> - each reference boundary is read on the printed side, between the letters aligned either side of it.
>
> **The numbers:**
>
> | | baseline | after |
> |---|---|---|
> | spaces right | 54 of 77 (6 added) | 58 of 77 (4 added) |
> | letters | 205 of 244 | 206 of 244 |
>
> The first recording reads whole with its spaces, noise prints nothing, and the random carrier prints at seed 5195 alone, as at HEAD.
>
> **Why.** The owner saw words run together on a hand and every letter split on another sender. Both were the word line.
> - **On `cw-2026-10-03-144045`** the hand's letter gaps reach 709 ms and its word gaps start at 639. No two neighbours differ by √(7/3), so every gap was one letter cluster, with the line at 958 ms above every word gap. Its clusters are now 195 to 709 and 925 to 1163 ms, the line 815 ms, and 8 of 8 reference spaces are read.
> - **On `cw-2026-10-03-221530` at 598 Hz** the letter gaps run 111 to 171 ms. A word cluster centred at 274 ms but wide pulled the old boundary to 160 ms, under its own letter gaps, and it printed `AGE 12ILEAR E D CW U SIN G A V`. At the crossing the line is 241 ms and no space is added; two real spaces there are lost.
> - **A synthetic hand** whose letter gaps reach 5.5 dits and word gaps start at 6 read 4, 7 and 7 of 14 word spaces at three seeds. It now reads 12, 14 and 13.
>
> **Each rule alone** (medium or better):
>
> | rules on | letters | spaces right | spaces added |
> |---|---|---|---|
> | neither | 205 | 54 | 6 |
> | the split alone | 206 | 59 | 8 |
> | the crossing alone | 205 | 52 | 3 |
> | both (shipped) | 206 | 58 | 4 |
>
> Both ship because the split alone splits more letters, which is the owner's second fault.
>
> **The references' spaces are less certain than their letters.** Each stretch's word breaks are read offline from its own gaps and printed beside them; no reference was changed.

## 2. What the owner should expect

- **Rebuild.** The build is clean with warnings as errors.
- **The scoreboard now counts spaces as well as letters.** Totals, medium confidence or better:

  | | before | after |
  |---|---|---|
  | letters | 205 of 244 | 206 of 244 |
  | spaces right | 54 of 77, with 6 added | 58 of 77, with 4 added |

- **A hand whose letter and word gaps overlap no longer runs its words together.** The 14:40:45 station now reads `BOB DE KG8V K` where it read `BOB DEKG8V K`.
- **A sender who leaves long gaps between letters no longer gets every letter split.** The 22:15:30 station no longer prints `LEAR E D` or `U SIN G`.
- **Hard limits:**
  - The first recording reads `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`, spaces included.
  - Loud noise prints nothing.
  - The random carrier prints at seed 5195 alone, as at HEAD, so the scoreboard test is red on that one seed as before.
- **Will look wrong but is not:** three `AReplyIsReadFromItsFirstLetter` tests are red. They were red at HEAD too, on letters alone, which I checked with this unit's changes set aside:
  - `TheNextOverReadsItsLetters`
  - `TheReplyOnTheOwnersRecordingIsReadFromItsFirstLetter`
  - `ASyntheticQsoReadsBothSendersInOrder`
- **App carry-forward:** 277 of 278. The one loss, `TheFavoritesAreChipsTests.ThreeChipsCostTheTopBandNothing`, is the known dispatcher-loop flake and passes alone.
- **Pushed:** everything, to `main`.
- **Left uncommitted, from before this unit:**
  - `PARKED.md`, `RUN_LEDGER.md`, `WORK_INSTRUCTIONS.md` and `.run-unit\denials.txt`
  - the four `.run-unit\reports` files
  - `tests\fixtures\cw\captured\cases-2026-10-02.txt` and `cases-2026-10-03.txt`

## 3. What you should see

**The scoreboard before (HEAD):**

| recording | pitch | stretch | confidence | reference | printed | right | spaces right | missing | added |
|---|---|---|---|---|---|---|---|---|---|
| `cw-2026-10-02-200157` | 662.8 | 0-30 s | verified | `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` | `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` | 27 of 27 | 8 of 8 | 0 | 0 |
| `cw-2026-10-03-143906` | 514.2 | 0-30 s | none | `` | `IEE II E NIEEE IET TAEEKEEIEEII IMESE ITT` | 0 of 0 | 0 of 0 | 0 | 0 |
| `cw-2026-10-03-143951` | 499.5 | 14.5-30 s | low | `O WAEIIEURD U AGN ES` | `EWAE IIEWRK I AGN ES` | 12 of 16 | 3 of 4 | 1 | 1 |
| `cw-2026-10-03-144020` | 499.5 | 0-12.5 s | medium | `ES OK ON PA <BT>` | `ES OK ON PA <BT>` | 9 of 9 | 4 of 4 | 0 | 0 |
| `cw-2026-10-03-144020` | 599.9 | 9.5-30 s | medium | `WX IN NETAGIT IUN TEMP E` | `WX IN NETAGIT IEN TEMP` | 17 of 19 | 4 of 5 | 1 | 0 |
| `cw-2026-10-03-144045` | 599.9 | 0-30 s | high | `N TEMP 57 57<BT> BTU BOB DE KG8V K` | `N E MP 57 57<BT>BTUBOB DEKG8V K` | 22 of 23 | 5 of 8 | 3 | 1 |
| `cw-2026-10-03-221502` | 491.5 | 0-30 s | low | `ED OF ITS OWN HEE BK BK WHAT BUG AE US E ENIE EE ITS A 66 K` | `ED OF ITS OWN HEE BK BK WHATBUGAEI EUI E■ ■` | 29 of 42 | 8 of 17 | 9 | 0 |
| `cw-2026-10-03-221530` | 491.5 | 0-9.5 s | medium | `6 CHAMPION BK` | `H CHA MPIMTN` | 6 of 11 | 1 of 2 | 1 | 1 |
| `cw-2026-10-03-221530` | 598.4 | 9-30 s | medium | `EN FB WHEN I WAS AGE 12 I LEARNED CW USING A V` | `IHES MHENI■S AGE 12ILEAR E D CW U SIN G A V` | 27 of 34 | 7 of 12 | 5 | 3 |
| `cw-2026-10-03-221548` | 597.7 | 0-18.5 s | low | `2 I LEARNED CW USING A V BPLX Z EPS` | `I M IEE IEE E USINGA V BPLX Z E PS` | 18 of 26 | 6 of 9 | 3 | 3 |
| `cw-2026-10-03-221548` | 498.0 | 18-30 s | medium | `YRHEE MY SCOUT MASTER` | `E WRHEE MYSCOA TQSTEN` | 13 of 18 | 1 of 3 | 2 | 1 |
| `cw-2026-10-03-221745` | 501.7 | 0-28 s | medium | `E E DAND ON 40M TONITE . EUR EE H RD TOO` | `E IAND TMN 4MEMM TONITE . EUROWEE H IRD TOO` | 19 of 29 | 9 of 11 | 2 | 0 |
| `cw-2026-10-03-221805` | 601.3 | 5-30 s | medium | `ET ON 40T S THESE DAYS . TNX FER ANOTHER FT` | `E 40T U THESE DAYS. TNX FERANOTHERF` | 28 of 33 | 7 of 10 | 3 | 0 |
| `cw-2026-10-03-221828` | 601.3 | 0-30 s | medium | `FER ANOTHER FB QSO ES HOPE U HAVE AGN ED ES BEST 73 <AR> W` | `FER ANOTHER FBQSOES HOPE U HAVE AGN ED ESBEST EV A MU` | 37 of 41 | 8 of 14 | 6 | 0 |
| `cw-2026-10-03-221851` | 601.3 | 0-30 s | low | `BEST 73 <AR> W2L CQ DE NA8SB K` | `SES E IE E IEA E I GE EI E NAFE I BK` | 5 of 20 | 3 of 7 | 4 | 6 |

Total, medium confidence or better: **205 of 244 letters and 54 of 77 spaces, with 6 added.**

**The scoreboard after:**

| recording | pitch | stretch | confidence | reference | printed | right | spaces right | missing | added |
|---|---|---|---|---|---|---|---|---|---|
| `cw-2026-10-02-200157` | 662.8 | 0-30 s | verified | `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` | `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` | 27 of 27 | 8 of 8 | 0 | 0 |
| `cw-2026-10-03-143906` | 514.2 | 0-30 s | none | `` | `IEE II E NIEEE IET TAEEKEEIEEII IMESE ITT` | 0 of 0 | 0 of 0 | 0 | 0 |
| `cw-2026-10-03-143951` | 499.5 | 14.5-30 s | low | `O WAEIIEURD U AGN ES` | `EWAE IIEWRK I AGN ES` | 12 of 16 | 3 of 4 | 1 | 1 |
| `cw-2026-10-03-144020` | 499.5 | 0-12.5 s | medium | `ES OK ON PA <BT>` | `ES OK ON PA <BT>` | 9 of 9 | 4 of 4 | 0 | 0 |
| `cw-2026-10-03-144020` | 599.9 | 9.5-30 s | medium | `WX IN NETAGIT IUN TEMP E` | `WX IN NETAGIT IEN TEMP` | 17 of 19 | 4 of 5 | 1 | 0 |
| `cw-2026-10-03-144045` | 599.9 | 0-30 s | high | `N TEMP 57 57<BT> BTU BOB DE KG8V K` | `N E MP 57 57<BT> BTU BOB DE KG8V K` | 22 of 23 | 8 of 8 | 0 | 1 |
| `cw-2026-10-03-221502` | 491.5 | 0-30 s | low | `ED OF ITS OWN HEE BK BK WHAT BUG AE US E ENIE EE ITS A 66 K` | `ED OF ITS OWN HEE BK BK WHATBUGAEI EUI E■ ■` | 29 of 42 | 8 of 17 | 9 | 0 |
| `cw-2026-10-03-221530` | 491.5 | 0-9.5 s | medium | `6 CHAMPION BK` | `H CHA MPIMTN` | 6 of 11 | 1 of 2 | 1 | 1 |
| `cw-2026-10-03-221530` | 598.4 | 9-30 s | medium | `EN FB WHEN I WAS AGE 12 I LEARNED CW USING A V` | `IHES MHENI■S AGE12ILEARED CW USINGA V` | 27 of 34 | 5 of 12 | 7 | 0 |
| `cw-2026-10-03-221548` | 597.7 | 0-18.5 s | low | `2 I LEARNED CW USING A V BPLX Z EPS` | `I M IEE IEE E USINGA V BPLX Z EPS` | 18 of 26 | 6 of 9 | 3 | 2 |
| `cw-2026-10-03-221548` | 498.0 | 18-30 s | medium | `YRHEE MY SCOUT MASTER` | `E WRHEE MYSCOET T MASTEN` | 14 of 18 | 2 of 3 | 1 | 1 |
| `cw-2026-10-03-221745` | 501.7 | 0-28 s | medium | `E E DAND ON 40M TONITE . EUR EE H RD TOO` | `E IAND TMN 4MEMM TONITE . EUROWEE H IRD TOO` | 19 of 29 | 9 of 11 | 2 | 0 |
| `cw-2026-10-03-221805` | 601.3 | 5-30 s | medium | `ET ON 40T S THESE DAYS . TNX FER ANOTHER FT` | `E 40T U THESE DAYS. TNX FERANOTHERF` | 28 of 33 | 7 of 10 | 3 | 0 |
| `cw-2026-10-03-221828` | 601.3 | 0-30 s | medium | `FER ANOTHER FB QSO ES HOPE U HAVE AGN ED ES BEST 73 <AR> W` | `FER ANOTHER FB QSO ES HOPE U HA VE AGN ED ESBEST EV A MU` | 37 of 41 | 10 of 14 | 4 | 1 |
| `cw-2026-10-03-221851` | 601.3 | 0-30 s | low | `BEST 73 <AR> W2L CQ DE NA8SB K` | `SES E IE E IEA E I GE EI E NAERE I BK` | 4 of 20 | 4 of 7 | 3 | 2 |

Total, medium confidence or better: **206 of 244 letters and 58 of 77 spaces, with 4 added.**

**Each rule alone,** medium confidence or better:

| rules on | letters | spaces right | added |
|---|---|---|---|
| neither (HEAD) | 205 | 54 of 77 | 6 |
| the split alone | 206 | 59 of 77 | 8 |
| the crossing alone | 205 | 52 of 77 | 3 |
| both (shipped) | 206 | 58 of 77 | 4 |

**Each stretch's word line beside its clusters,** as the gate held them when the stretch's last letter printed. In brackets are the line's median and range over the stretch.

| recording | pitch | letter gaps, before | word gaps, before | line, before | letter gaps, after | word gaps, after | line, after | spaces, before → after |
|---|---|---|---|---|---|---|---|---|
| `200157` | 663 | 18, 170-430 ms, centre 292 | 8, 579-2296 ms, centre 911 | 457 (458, 327-605) | the same | the same | 477 (479, 327-669) | 8 of 8 → 8 of 8 |
| `143951` | 500 | 2, 345-390 | 1, 1050 | 561 | the same | the same | 561 | 3 of 4 → 3 of 4 |
| `144020` | 500 | 4, 274-420, centre 338 | 4, 968-1068, centre 1012 | 654 (584, 529-681) | the same | the same | 651 (584, 529-675) | 4 of 4 → 4 of 4 |
| `144020` | 600 | 13, 187-583, centre 372 | 4, 895-1018, centre 955 | 770 (543, 458-770) | the same | the same | 739 (543, 458-739) | 4 of 5 → 4 of 5 |
| `144045` | 600 | 22, 195-1163, centre 546 | none | 958 (958, 298-1194) | 15, 195-709, centre 403 | 7, 925-1163, centre 1051 | 815 (640, 298-815) | 5 of 8, 1 added → 8 of 8, 1 added |
| `221502` | 492 | 19, 118-264, centre 168 | 10, 368-1328, centre 562 | 267 (275, 216-309) | the same | the same | 275 (279, 216-309) | 8 of 17 → 8 of 17 |
| `221530` | 492 | none | none | 292 (279, 277-328) | none | none | 292 | 1 of 2 → 1 of 2 |
| `221530` | 598 | 11, 111-171, centre 141 | 20, 184-1361, centre 274 | 160 (296, 159-333) | 23, 111-226, centre 168 | 8, 252-1361, centre 447 | 241 (296, 230-333) | 7 of 12, 3 added → 5 of 12, 0 added |
| `221548` | 598 | 16, 130-200, centre 168 | 12, 231-1590, centre 513 | 208 (318, 208-389) | 17, 130-231, centre 171 | 11, 290-1590, centre 552 | 239 (318, 239-383) | 6 of 9, 3 added → 6 of 9, 2 added |
| `221548` | 498 | 18, 123-661, centre 229 | none | 394 (319, 296-422) | 14, 123-283, centre 185 | 4, 388-661, centre 481 | 313 (312, 296-322) | 1 of 3 → 2 of 3 |
| `221745` | 502 | 22, 65-311, centre 182 | 7, 390-929, centre 558 | 325 (298, 287-325) | 20, 97-311, centre 187 | 7, 390-929, centre 558 | 312 (298, 287-312) | 9 of 11 → 9 of 11 |
| `221805` | 601 | 25, 137-424, centre 233 | 3, 665-713, centre 684 | 530 (341, 240-530) | the same | the same | 513 (341, 240-513) | 7 of 10 → 7 of 10 |
| `221828` | 601 | 40, 137-1011, centre 270 | none | 470 (378, 235-470) | 27, 137-285, centre 203 | 13, 304-1011, centre 488 | 291 (293, 243-358) | 8 of 14 → 10 of 14, 1 added |
| `221851` | 601 | 1, 155 | 2, 320-460, centre 384 | 240 | the same | the same | 237 | 3 of 7, 6 added → 4 of 7, 2 added |

The same tables, in full, are in `docs\cw-scoreboard.md` under *Word lines at unit 538*.

**Word breaks read offline from each stretch's own gaps, where they differ from the reference.**
- **How:** the line is 2-means in log-length over the gaps listed in the reference's own elements.
- **Limit:** those lists hold only gaps over the web session's letter line, so a short word gap cannot show here.

| stretch | read offline | reference |
|---|---|---|
| `143951` @500 | `O WAE IIEUR D U A GN ES` | `O WAEIIEURD U AGN ES` |
| `144045` @600 | `N TEMP 57 57 <BT> BTU BOB DEKG8V K` | `N TEMP 57 57<BT> BTU BOB DE KG8V K` |
| `221502` @492 | `... WHATBUGAE US ...` | `... WHAT BUG AE US ...` |
| `221530` @492 | `6 CHA MPION BK` | `6 CHAMPION BK` |
| `221530` @598 | `ENFB WHENI■S AGE12ILEARED CW USINGA V` | `EN FB WHEN I WAS AGE 12 I LEARNED CW USING A V` |
| `221548` @598 | `2ILEARED CW USINGA V BPLX Z EPS` | `2 I LEARNED CW USING A V BPLX Z EPS` |
| `221548` @498 | `YRHEE MYSCOET T MASTER` | `YRHEE MY SCOUT MASTER` |
| `221745` @502 | `E E DAND ON40MTONITE . EUR■EE H RD TOO` | `E E DAND ON 40M TONITE . EUR EE H RD TOO` |
| `221805` @601 | `... TNXFER ANOTHER FT` | `... TNX FER ANOTHER FT` |
| `221828` @601 | `FER ANOTH ER FB QSO ES HOP E U H AVE AGN ED ES BEST73<AR>W■` | `FER ANOTHER FB QSO ES HOPE U HAVE AGN ED ES BEST 73 <AR> W` |
| `221851` @601 | `IBEST73<AR>W2LCQDE NA8SBK ...` | `BEST 73 <AR> W2L CQ DE NA8SB K` |

The first recording and both 144020 stretches break as their references do.

## 4. What's blocking us

- **The `144045` target conflicts with its reference.**
  - The order's target is `57 57 <BT> B TU BOB DE KG8V K`; the reference reads `57 57<BT> BTU BOB DE KG8V K`, and the score uses the reference.
  - The sender's gap after the second 57 is 710 ms and after the B 415 ms, against letter gaps reaching 709 ms. No line drawn from his own gaps puts a word there and not between his letters.
  - The owner settles it by ear.
- **The split alone reads one more space right** (59 against 58), but it adds 8 spaces against 4. Both rules shipped, because added spaces are the owner's second fault.
- **The crossing costs two real spaces on `221530` at 598 Hz** (`AGE12`, `USINGA`). Its word gaps there run as short as 252 ms, against letter gaps up to 226.
- **Three `AReplyIsReadFromItsFirstLetter` tests are red on letters,** as they were at HEAD.
- **The random-carrier hard limit is red on seed 5195,** as at HEAD.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
