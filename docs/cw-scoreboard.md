# The CW scoreboard

**The owner's recordings are the yardstick** (work instruction 534, HM-DEC-238). The owner, 2026-10-03: *"Right now we
suck."* And: *"I want to run it against all the recordings that we've done over the last two days."*

`TheRecordingsScoreboardTests.TheRecordingsScoreboard` reads the owner's twelve recordings in
`tests\fixtures\cw\captured\` through the live path, as the app wires it (the detector, the sender's window, the gate and
the reader), at the radio's state from each recording's own sheet. It then scores what printed against the web session's
references.

- **A stretch's score:** the reference's letters read right, spaces ignored, by the scorer's edit distance with free ends.
  A prosign counts as one letter. A printed letter belongs to the stretch of its recording nearest its pitch, within
  60 Hz and within a second of the stretch's span.
- **The total:** the sum over every stretch of confidence `medium` or better. `low` stretches are scored and reported but
  not totalled; `none` is reported only.
- **Two hard limits, whatever the score:**
  - loud noise, 30 s and three minutes at two seeds each, prints nothing;
  - `cw-2026-10-02-200157` reads `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`.
- **The references are not certain.** They were read offline with thresholds set by hand per station. A unit is better if
  the total rises.

**Every later unit appends a row** to the totals table and adds its own stretch table below the others.

## Totals

| unit | date | total (medium or better) | hard limits | what changed | spaces right (medium or better) |
|---|---|---|---|---|---|
| 534 baseline | 2026-10-03 | 169 of 240 | hold | before any change; tag `before-scoreboard` | not scored |
| 534 after | 2026-10-03 | 185 of 240 | hold | thirteen shape-side rules removed, each measured (see below); tag `before-scoreboard` holds them | not scored |
| 535 task 1 | 2026-10-04 | 185 of 240 | hold | the light reads the gate: green only while a sender is printed or qualified and waiting; display only, the reading unchanged | not scored |
| 535 task 2 | 2026-10-04 | 185 of 240 | carrier limit red: 2 of 5 seeds print | a carrier keyed at random added as a third hard limit; no rule found closes it without lowering the total, so none shipped (best: two kinds held over the last ten marks, each kind recurring, 185 and 2 of 20 seeds still printing) | not scored |
| 535 task 3, before the correction | 2026-10-04 | 185 of 240 | carrier limit red | the yardstick as it was | not scored |
| 535 task 3, after the correction | 2026-10-04 | 185 of 244 | carrier limit red | two references corrected, not Hamlet: the sign-off of 221828 and 221851 reads 73 <AR> W2L where the web session wrote EEV CW; 221828 gains 4 reference letters and reads 26 of 41 | not scored |
| 535 task 4 | 2026-10-04 | 205 of 244 | carrier limit red, as before | a mark is not dropped as the last one read again unless it overlaps it or is itself a piece under half a dit: the F of FER keeps its second dit on heavy keying, and FER reads | not scored |
| 536 task 1 | 2026-10-04 | 205 of 244 | carrier limit red: 1 of 5 seeds print (2 of 20 in the test) | a sender qualifies only on two kinds held over its last ten marks, each kind recurring | not scored |
| 536 task 2 | 2026-10-04 | 205 of 244 | carrier limit red: 1 of 5 seeds print (1 of 20 in the test) | and its gaps fall into kinds: a clean jump of √3 among the nine gaps, at least two either side | not scored |
| 537 | 2026-10-04 | 205 of 244 | carrier limit red: 1 of 5, as before | the cleanup: the repo, the capture sheet and three reds; nothing that reads was changed, and the scoreboard read 205 after every task | not scored |
| 538 task 1 | 2026-10-04 | 205 of 244 | carrier limit red: 1 of 5, as before | the scoreboard scores spaces: printed letters aligned to the reference letter by letter, and each reference boundary read between the letters aligned either side; nothing that reads was changed | **54 of 77**, 6 added |
| 538 task 2 | 2026-10-04 | 206 of 244 | carrier limit red: 1 of 5, as before | a word line is where the sender's own letter and word gaps cross: where they show no clean jump they are split in two where they overlap, and the line is the equal-error point given each cluster's centre and spread | **58 of 77**, 4 added |
| 538 task 3 | 2026-10-04 | 206 of 244 | carrier limit red: 1 of 5, as before | the sender who spaced its letters: on 221530 at 598 Hz a wide word cluster pulled the spread-count boundary to 160 ms, under letter gaps to 171; at the crossing (task 2's rule, shipped with it) the line is 241 ms and no space is added, at the cost of two real spaces there (7 to 5 of 12); no further change | **58 of 77**, 4 added |

## Stretches at the baseline (unit 534, before any change)

| recording | pitch | stretch | confidence | reference | printed | right |
|---|---|---|---|---|---|---|
| `cw-2026-10-02-200157` | 662.8 | 0-30 s | verified | `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` | `FERCHAT<BT>BEST7V73<SK>KC4ZGPDEWA` | 27 of 27 |
| `cw-2026-10-03-143906` | 514.2 | 0-30 s | none | `` | `` | 0 of 0 |
| `cw-2026-10-03-143951` | 499.5 | 14.5-30 s | low | `O WAEIIEURD U AGN ES` | `` | 0 of 16 |
| `cw-2026-10-03-144020` | 499.5 | 0-12.5 s | medium | `ES OK ON PA <BT>` | `ESOKONPA<BT>` | 9 of 9 |
| `cw-2026-10-03-144020` | 599.9 | 9.5-30 s | medium | `WX IN NETAGIT IUN TEMP E` | `WXINNETAGITIENTEMP` | 17 of 19 |
| `cw-2026-10-03-144045` | 599.9 | 0-30 s | high | `N TEMP 57 57<BT> BTU BOB DE KG8V K` | `NTEMP5757<BT>BTUBOBD` | 17 of 23 |
| `cw-2026-10-03-221502` | 491.5 | 0-30 s | low | `ED OF ITS OWN HEE BK BK WHAT BUG AE US E ENIE EE ITS A 66 K` | `EDOFITSOWNHEENEKESKESITHAHDEN` | 20 of 42 |
| `cw-2026-10-03-221530` | 491.5 | 0-9.5 s | medium | `6 CHAMPION BK` | `BCSAMENION` | 5 of 11 |
| `cw-2026-10-03-221530` | 598.4 | 9-30 s | medium | `EN FB WHEN I WAS AGE 12 I LEARNED CW USING A V` | `IIESMHENI■SAGE12ILE<AR>EDCWUSINGAV` | 25 of 34 |
| `cw-2026-10-03-221548` | 597.7 | 0-18.5 s | low | `2 I LEARNED CW USING A V BPLX Z EPS` | `IMRIUSINGAVBPLXZEPS` | 17 of 26 |
| `cw-2026-10-03-221548` | 498.0 | 18-30 s | medium | `YRHEE MY SCOUT MASTER` | `EYRHSCOMASTEN` | 11 of 18 |
| `cw-2026-10-03-221745` | 501.7 | 0-28 s | medium | `E E DAND ON 40M TONITE . EUR EE H RD TOO` | `IANDTMN4MEMMTONI` | 8 of 29 |
| `cw-2026-10-03-221805` | 601.3 | 5-30 s | medium | `ET ON 40T S THESE DAYS . TNX FER ANOTHER FT` | `MEUMTTSTHESEDAYI.TNXENERANOTHERF` | 24 of 33 |
| `cw-2026-10-03-221828` | 601.3 | 0-30 s | medium | `FER ANOTHER FB QSO ES HOPE U HAVE AGN ED ES BEST` | `ENERANOTHERFNEQSMESHMWIEVEATNEDESBESTEHEEIER` | 26 of 37 |
| `cw-2026-10-03-221851` | 601.3 | 0-30 s | low | `BEST EEV CW 2L CQ DE NA8SB K` | `SESTILCTADENAME` | 8 of 21 |

**Total (medium or better): 169 of 240.** Printed outside every stretch: `MEEI` in `cw-2026-10-03-221745`. Hard limits hold:
the first recording reads `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`, and all four noise runs print nothing.

## Stretches after unit 534 (thirteen rules removed)

| recording | pitch | stretch | confidence | reference | printed | right |
|---|---|---|---|---|---|---|
| `cw-2026-10-02-200157` | 662.8 | 0-30 s | verified | `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` | `FERCHAT<BT>BEST7V73<SK>KC4ZGPDEWA` | 27 of 27 |
| `cw-2026-10-03-143906` | 514.2 | 0-30 s | none | `` | `SEIIENUVIQSYQSYDEWB2FUVEE` | 0 of 0 |
| `cw-2026-10-03-143951` | 499.5 | 14.5-30 s | low | `O WAEIIEURD U AGN ES` | `EOMTTTOMOTMOEES` | 4 of 16 |
| `cw-2026-10-03-144020` | 499.5 | 0-12.5 s | medium | `ES OK ON PA <BT>` | `ESOKONPA<BT>` | 9 of 9 |
| `cw-2026-10-03-144020` | 599.9 | 9.5-30 s | medium | `WX IN NETAGIT IUN TEMP E` | `WXINNETAGITIENTEMP` | 17 of 19 |
| `cw-2026-10-03-144045` | 599.9 | 0-30 s | high | `N TEMP 57 57<BT> BTU BOB DE KG8V K` | `NEMP5757<BT>BTUBOBDEKG8VK` | 22 of 23 |
| `cw-2026-10-03-221502` | 491.5 | 0-30 s | low | `ED OF ITS OWN HEE BK BK WHAT BUG AE US E ENIE EE ITS A 66 K` | `EDOFITSMTWNHEEBKBKWHATBUGAEIEUIE■■` | 27 of 42 |
| `cw-2026-10-03-221530` | 491.5 | 0-9.5 s | medium | `6 CHAMPION BK` | `HCSAMENIONNEN` | 5 of 11 |
| `cw-2026-10-03-221530` | 598.4 | 9-30 s | medium | `EN FB WHEN I WAS AGE 12 I LEARNED CW USING A V` | `IHESMHENI■SAGE12ILEAREDCWUSINGAV` | 27 of 34 |
| `cw-2026-10-03-221548` | 597.7 | 0-18.5 s | low | `2 I LEARNED CW USING A V BPLX Z EPS` | `IMEEEAREDCWUSINGAVBPLXZEPS` | 22 of 26 |
| `cw-2026-10-03-221548` | 498.0 | 18-30 s | medium | `YRHEE MY SCOUT MASTER` | `EWRHEEMYSCOETTQSTEN` | 13 of 18 |
| `cw-2026-10-03-221745` | 501.7 | 0-28 s | medium | `E E DAND ON 40M TONITE . EUR EE H RD TOO` | `TEMMTONITE.EIEEMESIRDTOTT` | 14 of 29 |
| `cw-2026-10-03-221805` | 601.3 | 5-30 s | medium | `ET ON 40T S THESE DAYS . TNX FER ANOTHER FT` | `E40TUTHESEDAYI.TNXENERANOTHERF` | 25 of 33 |
| `cw-2026-10-03-221828` | 601.3 | 0-30 s | medium | `FER ANOTHER FB QSO ES HOPE U HAVE AGN ED ES BEST` | `ENERANOTHERFNEQSMESHMWETEIEVEATNEDESBEST` | 26 of 37 |
| `cw-2026-10-03-221851` | 601.3 | 0-30 s | low | `BEST EEV CW 2L CQ DE NA8SB K` | `TTTMEEEUIEMTICWJLKTADENAMEEENEK■DETSKETEER5SEXIEEESIIIIEEEI` | 8 of 21 |

**Total (medium or better): 185 of 240.** Hard limits hold: the first recording reads `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`, and all four
noise runs print nothing.

## What each rule was worth (unit 534)

Each rule switched off alone at the baseline (169), then removed best first and re-measured, then every remaining rule
switched off again with the removed ones out, until no removal raised or held the total. "Broke" is a hard limit broken.
For a removed rule, "after" is the total once it came out; for a kept rule, the total with all thirteen out and it off too.

| rule | off alone at baseline | after | fate |
|---|---|---|---|
| the 0.2 standing line | 182 | 182 with it out | removed, round 1 |
| three lone letters dropped | 172 | 184 | removed, round 1 |
| quieter-mark admission inside a letter | 172 | 184 | removed, round 1 |
| the pause that is not a word | 169 | 184 | removed, round 1 |
| a hand's two kinds | 165 | 185 (sweep with four out) | removed, round 2 |
| key-up | 159 | 185 (sweep with four out) | removed, round 2 |
| five-dit floor | 167 | 185 | removed, round 2 |
| shape: inside-letter gap tightness | 160 | 185 | removed, round 2 |
| rectangle fit | 165 | 185 | removed, round 2 |
| narrowness | 166 | 185 | removed, round 2 |
| neighbour judgement of gaps | 174 | 185 (sweep with ten out) | removed, round 3 |
| shape: letter-gap tightness | 168 | 185 (sweep with ten out) | removed, round 3 |
| neighbour split of marks | 169 | 185 (sweep with twelve out) | removed, last |
| lone letter | 173, broke | 182 with it off | kept |
| cold-start word line at √21 | 169, broke | 185, broke (`F ER C H AT`) | kept for a hard limit |
| retry over newer marks when speed changes | 161 | 152 | kept |
| settle at key-down | 155 | 179 | kept |
| the sender's own window | 149, broke | 148, broke | kept |
| release under 0.1 | 181 | 180 | kept |
| first pick waits one word gap | 169 | 181 | kept |
| handover backlog | 169 | 185 | kept: off means putting back the skip it replaced, a rule added rather than removed |
| a silent sender is not a candidate | 141 | 155 | kept |
| edges | 144 | 113, broke (noise prints) | kept |
| a mark's own shape | 151 | 162, broke (noise prints) | kept |

Task 3, gap lines from the sender's own gaps rather than its dit (the gap unit from the median of its gaps inside letters):
185 of 240, held, did not rise, so not kept.

## Stretches after unit 535 (references corrected at the sign-off; FER keeps its F)

| recording | pitch | stretch | confidence | reference | printed | right |
|---|---|---|---|---|---|---|
| `cw-2026-10-02-200157` | 662.8 | 0-30 s | verified | `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` | `FERCHAT<BT>BEST7V73<SK>KC4ZGPDEWA` | 27 of 27 |
| `cw-2026-10-03-143906` | 514.2 | 0-30 s | none | `` | `SEIIENUVIQSYQSYDEWB2FUHEE5` | 0 of 0 |
| `cw-2026-10-03-143951` | 499.5 | 14.5-30 s | low | `O WAEIIEURD U AGN ES` | `EOMTTTOOOTMGEES` | 4 of 16 |
| `cw-2026-10-03-144020` | 499.5 | 0-12.5 s | medium | `ES OK ON PA <BT>` | `ESOKONPA<BT>` | 9 of 9 |
| `cw-2026-10-03-144020` | 599.9 | 9.5-30 s | medium | `WX IN NETAGIT IUN TEMP E` | `WXINNETAGITIENTEMP` | 17 of 19 |
| `cw-2026-10-03-144045` | 599.9 | 0-30 s | high | `N TEMP 57 57<BT> BTU BOB DE KG8V K` | `NEMP5757<BT>BTUBOBDEKG8VK` | 22 of 23 |
| `cw-2026-10-03-221502` | 491.5 | 0-30 s | low | `ED OF ITS OWN HEE BK BK WHAT BUG AE US E ENIE EE ITS A 66 K` | `EDOFITSMTWNHEEBKBKWHATBUGAEIEUIE■■` | 27 of 42 |
| `cw-2026-10-03-221530` | 491.5 | 0-9.5 s | medium | `6 CHAMPION BK` | `HCHAMPIMTNBK` | 8 of 11 |
| `cw-2026-10-03-221530` | 598.4 | 9-30 s | medium | `EN FB WHEN I WAS AGE 12 I LEARNED CW USING A V` | `IHESMHENI■SAGE12ILEAREDCWUSINGAV` | 27 of 34 |
| `cw-2026-10-03-221548` | 597.7 | 0-18.5 s | low | `2 I LEARNED CW USING A V BPLX Z EPS` | `IMEEEAREDCWUSINGAVBPLXZEPS` | 22 of 26 |
| `cw-2026-10-03-221548` | 498.0 | 18-30 s | medium | `YRHEE MY SCOUT MASTER` | `EWRHEEMYSCOETTQSTEN` | 13 of 18 |
| `cw-2026-10-03-221745` | 501.7 | 0-28 s | medium | `E E DAND ON 40M TONITE . EUR EE H RD TOO` | `TEMMTONITE.EUROWEEHIRDTOO` | 17 of 29 |
| `cw-2026-10-03-221805` | 601.3 | 5-30 s | medium | `ET ON 40T S THESE DAYS . TNX FER ANOTHER FT` | `E40TUTHESEDAYS.TNXFERANOTHERF` | 28 of 33 |
| `cw-2026-10-03-221828` | 601.3 | 0-30 s | medium | `FER ANOTHER FB QSO ES HOPE U HAVE AGN ED ES BEST 73 <AR> W` | `FERANOTHERFBQSOESHOPEUHAVEAGNEDESBESTEVAMU` | 37 of 41 |
| `cw-2026-10-03-221851` | 601.3 | 0-30 s | low | `BEST 73 <AR> W2L CQ DE NA8SB K` | `TTTMEESSTEARWJLCQDENA8SBN■DET■EIETEERHIESESEEIEEIISEEI` | 13 of 20 |

**Total (medium or better): 205 of 244.** Loud noise prints nothing and the first recording reads whole. The carrier limit
is red: seeds 5195 and 5197 of the five print.

## Stretches at unit 538 task 1 (spaces scored)

A stretch's spaces: each boundary between two reference letters, read on the printed side between the letters aligned to them. The references' spaces are the sender's timing as the web session read it, less certain than their letters.

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

**Total (medium or better): 205 of 244 letters; 54 of 77 spaces, 6 added.** Hard limits as before: the first recording reads whole with its spaces, noise prints nothing, the carrier prints on seed 5195.

## Word lines at unit 538, before and after task 2

Each stretch's word line beside its letter and word clusters, as the gate held them when the stretch's last letter printed, with the line's median and range over the stretch.

**Before** (HEAD, both rules off):

| recording | pitch | letter gaps | word gaps | word line at the last letter | spaces |
|---|---|---|---|---|---|
| `cw-2026-10-02-200157` | 663 | 18 gaps, 170-430 ms, centre 292 | 8 gaps, 579-2296 ms, centre 911 | 457 ms (median 458, 327-605) | 8 of 8, 0 added |
| `cw-2026-10-03-143951` | 500 | 2 gaps, 345-390 ms, centre 367 | 1 gaps, 1050-1050 ms, centre 1050 | 561 ms (median 561, 561-561) | 3 of 4, 1 added |
| `cw-2026-10-03-144020` | 500 | 4 gaps, 274-420 ms, centre 338 | 4 gaps, 968-1068 ms, centre 1012 | 654 ms (median 584, 529-681) | 4 of 4, 0 added |
| `cw-2026-10-03-144020` | 600 | 13 gaps, 187-583 ms, centre 372 | 4 gaps, 895-1018 ms, centre 955 | 770 ms (median 543, 458-770) | 4 of 5, 0 added |
| `cw-2026-10-03-144045` | 600 | 22 gaps, 195-1163 ms, centre 546 | none | 958 ms (median 958, 298-1194) | 5 of 8, 1 added |
| `cw-2026-10-03-221502` | 492 | 19 gaps, 118-264 ms, centre 168 | 10 gaps, 368-1328 ms, centre 562 | 267 ms (median 275, 216-309) | 8 of 17, 0 added |
| `cw-2026-10-03-221530` | 492 | none | none | 292 ms (median 279, 277-328) | 1 of 2, 1 added |
| `cw-2026-10-03-221530` | 598 | 11 gaps, 111-171 ms, centre 141 | 20 gaps, 184-1361 ms, centre 274 | 160 ms (median 296, 159-333) | 7 of 12, 3 added |
| `cw-2026-10-03-221548` | 598 | 16 gaps, 130-200 ms, centre 168 | 12 gaps, 231-1590 ms, centre 513 | 208 ms (median 318, 208-389) | 6 of 9, 3 added |
| `cw-2026-10-03-221548` | 498 | 18 gaps, 123-661 ms, centre 229 | none | 394 ms (median 319, 296-422) | 1 of 3, 1 added |
| `cw-2026-10-03-221745` | 502 | 22 gaps, 65-311 ms, centre 182 | 7 gaps, 390-929 ms, centre 558 | 325 ms (median 298, 287-325) | 9 of 11, 0 added |
| `cw-2026-10-03-221805` | 601 | 25 gaps, 137-424 ms, centre 233 | 3 gaps, 665-713 ms, centre 684 | 530 ms (median 341, 240-530) | 7 of 10, 0 added |
| `cw-2026-10-03-221828` | 601 | 40 gaps, 137-1011 ms, centre 270 | none | 470 ms (median 378, 235-470) | 8 of 14, 0 added |
| `cw-2026-10-03-221851` | 601 | 1 gaps, 155-155 ms, centre 155 | 2 gaps, 320-460 ms, centre 384 | 240 ms (median 240, 240-240) | 3 of 7, 6 added |

**After** (both rules on):

| recording | pitch | letter gaps | word gaps | word line at the last letter | spaces |
|---|---|---|---|---|---|
| `cw-2026-10-02-200157` | 663 | 18 gaps, 170-430 ms, centre 292 | 8 gaps, 579-2296 ms, centre 911 | 477 ms (median 479, 327-669) | 8 of 8, 0 added |
| `cw-2026-10-03-143951` | 500 | 2 gaps, 345-390 ms, centre 367 | 1 gaps, 1050-1050 ms, centre 1050 | 561 ms (median 561, 561-561) | 3 of 4, 1 added |
| `cw-2026-10-03-144020` | 500 | 4 gaps, 274-420 ms, centre 338 | 4 gaps, 968-1068 ms, centre 1012 | 651 ms (median 584, 529-675) | 4 of 4, 0 added |
| `cw-2026-10-03-144020` | 600 | 13 gaps, 187-583 ms, centre 372 | 4 gaps, 895-1018 ms, centre 955 | 739 ms (median 543, 458-739) | 4 of 5, 0 added |
| `cw-2026-10-03-144045` | 600 | 15 gaps, 195-709 ms, centre 403 | 7 gaps, 925-1163 ms, centre 1051 | 815 ms (median 640, 298-815) | 8 of 8, 1 added |
| `cw-2026-10-03-221502` | 492 | 19 gaps, 118-264 ms, centre 168 | 10 gaps, 368-1328 ms, centre 562 | 275 ms (median 279, 216-309) | 8 of 17, 0 added |
| `cw-2026-10-03-221530` | 492 | none | none | 292 ms (median 279, 277-328) | 1 of 2, 1 added |
| `cw-2026-10-03-221530` | 598 | 23 gaps, 111-226 ms, centre 168 | 8 gaps, 252-1361 ms, centre 447 | 241 ms (median 296, 230-333) | 5 of 12, 0 added |
| `cw-2026-10-03-221548` | 598 | 17 gaps, 130-231 ms, centre 171 | 11 gaps, 290-1590 ms, centre 552 | 239 ms (median 318, 239-383) | 6 of 9, 2 added |
| `cw-2026-10-03-221548` | 498 | 14 gaps, 123-283 ms, centre 185 | 4 gaps, 388-661 ms, centre 481 | 313 ms (median 312, 296-322) | 2 of 3, 1 added |
| `cw-2026-10-03-221745` | 502 | 20 gaps, 97-311 ms, centre 187 | 7 gaps, 390-929 ms, centre 558 | 312 ms (median 298, 287-312) | 9 of 11, 0 added |
| `cw-2026-10-03-221805` | 601 | 25 gaps, 137-424 ms, centre 233 | 3 gaps, 665-713 ms, centre 684 | 513 ms (median 341, 240-513) | 7 of 10, 0 added |
| `cw-2026-10-03-221828` | 601 | 27 gaps, 137-285 ms, centre 203 | 13 gaps, 304-1011 ms, centre 488 | 291 ms (median 293, 243-358) | 10 of 14, 1 added |
| `cw-2026-10-03-221851` | 601 | 1 gaps, 155-155 ms, centre 155 | 2 gaps, 320-460 ms, centre 384 | 237 ms (median 237, 237-237) | 4 of 7, 2 added |

**Each rule alone** (medium or better): both on, letters 206 and spaces 58 of 77 with 4 added; the split alone, 206 and 59 with 8 added; the crossing alone, 205 and 52 with 3 added; both off, 205 and 54 with 6 added.

## The score counts wrong and invented (work instruction 539, HM-DEC-243)

The owner, 2026-10-04: *"We are still having way too much false character."* Letters right alone cost nothing for a letter never sent. From unit 539 the scoreboard's number is:

**score = letters right - wrong - invented.**

- **wrong:** a printed letter the alignment counts as not the reference's, wrong in its place or extra, inside a stretch of medium confidence or better, and not invented.
- **invented:** a printed letter, in any recording, whose marks overlap no keying on the keying map (`docs\cw-keying-map.md`) within one bin either side of its pitch.
- **a hard limit:** nothing prints inside a silence of two seconds or more on the keying map.

### Score totals

| unit | date | right | wrong | invented | score | printed in silence | spaces right | what changed |
|---|---|---|---|---|---|---|---|---|
| 539 baseline | 2026-10-04 | 206 of 244 | 18 | 2 | **186** | 1 (`221502` 21.4-24.5 s, `■` at 21.46 s) | 58 of 77, 4 added | the score as HEAD reads it; tag `before-false-characters` |

### Stretches at the 539 baseline

| recording | pitch | stretch | confidence | reference | printed | right | spaces right | missing | added | wrong | invented |
|---|---|---|---|---|---|---|---|---|---|---|---|
| `cw-2026-10-02-200157` | 662.8 | 0-30 s | verified | `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` | `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` | 27 of 27 | 8 of 8 | 0 | 0 | 0 | 0 |
| `cw-2026-10-03-143906` | 514.2 | 0-30 s | none | `` | `IEE II E NIEEE IET TAEEKEEIEEII IMESE ITT` | 0 of 0 | 0 of 0 | 0 | 0 | 0 | 0 |
| `cw-2026-10-03-143951` | 499.5 | 14.5-30 s | low | `O WAEIIEURD U AGN ES` | `EWAE IIEWRK I AGN ES` | 12 of 16 | 3 of 4 | 1 | 1 | 3 | 1 |
| `cw-2026-10-03-144020` | 499.5 | 0-12.5 s | medium | `ES OK ON PA <BT>` | `ES OK ON PA <BT>` | 9 of 9 | 4 of 4 | 0 | 0 | 0 | 0 |
| `cw-2026-10-03-144020` | 599.9 | 9.5-30 s | medium | `WX IN NETAGIT IUN TEMP E` | `WX IN NETAGIT IEN TEMP` | 17 of 19 | 4 of 5 | 1 | 0 | 1 | 0 |
| `cw-2026-10-03-144045` | 599.9 | 0-30 s | high | `N TEMP 57 57<BT> BTU BOB DE KG8V K` | `N E MP 57 57<BT> BTU BOB DE KG8V K` | 22 of 23 | 8 of 8 | 0 | 1 | 0 | 0 |
| `cw-2026-10-03-221502` | 491.5 | 0-30 s | low | `ED OF ITS OWN HEE BK BK WHAT BUG AE US E ENIE EE ITS A 66 K` | `ED OF ITS OWN HEE BK BK WHATBUGAEI EUI E■ ■` | 29 of 42 | 8 of 17 | 9 | 0 | 1 | 1 |
| `cw-2026-10-03-221530` | 491.5 | 0-9.5 s | medium | `6 CHAMPION BK` | `H CHA MPIMTN` | 6 of 11 | 1 of 2 | 1 | 1 | 1 | 0 |
| `cw-2026-10-03-221530` | 598.4 | 9-30 s | medium | `EN FB WHEN I WAS AGE 12 I LEARNED CW USING A V` | `IHES MHENI■S AGE12ILEARED CW USINGA V` | 27 of 34 | 5 of 12 | 7 | 0 | 3 | 0 |
| `cw-2026-10-03-221548` | 597.7 | 0-18.5 s | low | `2 I LEARNED CW USING A V BPLX Z EPS` | `I M IEE IEE E USINGA V BPLX Z EPS` | 18 of 26 | 6 of 9 | 3 | 2 | 5 | 0 |
| `cw-2026-10-03-221548` | 498.0 | 18-30 s | medium | `YRHEE MY SCOUT MASTER` | `E WRHEE MYSCOET T MASTEN` | 14 of 18 | 2 of 3 | 1 | 1 | 3 | 0 |
| `cw-2026-10-03-221745` | 501.7 | 0-28 s | medium | `E E DAND ON 40M TONITE . EUR EE H RD TOO` | `E IAND TMN 4MEMM TONITE . EUROWEE H IRD TOO` | 19 of 29 | 9 of 11 | 2 | 0 | 9 | 0 |
| `cw-2026-10-03-221805` | 601.3 | 5-30 s | medium | `ET ON 40T S THESE DAYS . TNX FER ANOTHER FT` | `E 40T U THESE DAYS. TNX FERANOTHERF` | 28 of 33 | 7 of 10 | 3 | 0 | 1 | 0 |
| `cw-2026-10-03-221828` | 601.3 | 0-30 s | medium | `FER ANOTHER FB QSO ES HOPE U HAVE AGN ED ES BEST 73 <AR> W` | `FER ANOTHER FB QSO ES HOPE U HA VE AGN ED ESBEST EV A MU` | 37 of 41 | 10 of 14 | 4 | 1 | 0 | 0 |
| `cw-2026-10-03-221851` | 601.3 | 0-30 s | low | `BEST 73 <AR> W2L CQ DE NA8SB K` | `SES E IE E IEA E I GE EI E NAERE I BK` | 4 of 20 | 4 of 7 | 3 | 2 | 6 | 0 |

Invented at the baseline: `143951`, an `E` at 16.6 s at 500 Hz, and `221502`, a `■` at 21.5 s at 500 Hz, the second printed 60 ms after the station's last mark, inside the silence that follows.

### Every rule alone on the new score, from HEAD (work instruction 539, task 3)

Each of the kept and added rules taken out, and each of the thirteen removed in work instruction 534 brought back from the tag `before-scoreboard` behind a switch, one at a time. The carrier is counted over the twenty seeds of its own test.

| change | right | wrong | invented | score | spaces | first | noise prints | carriers print | silence prints |
| none (the base) | 206 | 18 | 2 | **186** | 58 | reads | 0 | 1 of 20 | 1 |
| out: first pick waits one word gap | 205 | 14 | 3 | **188** | 54 | reads | 0 | 2 of 20 | 2 |
| back in: narrowness | 209 | 17 | 4 | **188** | 58 | reads | 0 | 1 of 20 | 1 |
| back in: three lone letters dropped | 204 | 16 | 1 | **187** | 55 | reads | 0 | 1 of 20 | 1 |
| back in: key-up | 207 | 17 | 3 | **187** | 59 | reads | 0 | 1 of 20 | 3 |
| back in: the neighbour judgement of gaps | 207 | 18 | 2 | **187** | 58 | reads | 0 | 1 of 20 | 1 |
| out: cold-start word line at √21 | 206 | 18 | 2 | **186** | 58 | `F ER C H AT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` | 0 | 1 of 20 | 1 |
| out: handover backlog | 206 | 18 | 2 | **186** | 58 | reads | 0 | 1 of 20 | 1 |
| out: gap kinds | 206 | 18 | 2 | **186** | 58 | reads | 0 | 2 of 20 | 1 |
| out: word line at the clusters' crossing | 206 | 18 | 2 | **186** | 59 | reads | 0 | 1 of 20 | 1 |
| back in: the pause | 206 | 18 | 2 | **186** | 58 | reads | 0 | 1 of 20 | 1 |
| back in: the five-dit floor | 206 | 18 | 2 | **186** | 59 | reads | 0 | 1 of 20 | 1 |
| back in: the rectangle fit | 206 | 18 | 2 | **186** | 60 | reads | 0 | 1 of 20 | 1 |
| back in: the neighbour split of marks | 206 | 18 | 2 | **186** | 58 | reads | 0 | 1 of 20 | 1 |
| out: split overlapping letter and word gaps | 205 | 18 | 2 | **185** | 52 | reads | 0 | 1 of 20 | 1 |
| back in: the shape's inside-letter tightness | 205 | 18 | 2 | **185** | 58 | reads | 0 | 1 of 20 | 1 |
| back in: the shape's letter-gap tightness | 205 | 18 | 2 | **185** | 58 | reads | 0 | 1 of 20 | 1 |
| out: lone letter | 208 | 21 | 4 | **183** | 58 | reads | 0 | 1 of 20 | 2 |
| out: settle at key-down | 200 | 18 | 2 | **180** | 56 | reads | 0 | 1 of 20 | 1 |
| out: release under 0.1 | 203 | 21 | 2 | **180** | 59 | reads | 0 | 1 of 20 | 1 |
| back in: the 0.2 standing line | 197 | 15 | 5 | **177** | 53 | reads | 0 | 1 of 20 | 3 |
| back in: quieter marks | 199 | 17 | 5 | **177** | 56 | reads | 0 | 1 of 20 | 4 |
| out: a mark's own shape | 184 | 12 | 0 | **172** | 50 | reads | 0 | 1 of 20 | 0 |
| out: two kinds held over the last ten marks | 206 | 16 | 21 | **169** | 55 | reads | 0 | 9 of 20 | 2 |
| back in: a hand's two kinds | 210 | 18 | 24 | **168** | 58 | reads | 0 | 5 of 20 | 16 |
| out: a silent sender is not a candidate | 175 | 14 | 2 | **159** | 52 | reads | 0 | 1 of 20 | 1 |
| out: a mark crowds the last only where it overlaps it or is a piece | 187 | 38 | 2 | **147** | 60 | reads | 0 | 1 of 20 | 1 |
| out: retry over newer marks when speed changes | 156 | 11 | 1 | **144** | 42 | reads | 0 | 0 of 20 | 1 |
| out: edges | 95 | 3 | 3 | **89** | 27 | reads | 0 | 1 of 20 | 1 |
| out: the sender's own window | 146 | 56 | 2 | **88** | 50 | `FER C HAT<BT> BESI THV EEE4 <SK>IDC4ZMPDEAA` | 0 | 1 of 20 | 0 |

### The search, one change at a time

A change is taken only where no hard limit is worse than at HEAD: the first recording reads, noise prints nothing, the carrier prints at no more than 1 of 20 seeds, and no more letters print in a silence.

| step | change | right | wrong | invented | score | carriers print | silence prints |
|---|---|---|---|---|---|---|---|
| 0 | HEAD | 206 | 18 | 2 | 186 | 1 of 20 | 1 |
| 1 | narrowness back in | 209 | 17 | 4 | 188 | 1 of 20 | 1 |
| 2 | three lone letters dropped back in | 207 | 15 | 2 | 190 | 1 of 20 | 1 |
| 3 | the neighbour judgement of gaps back in | 208 | 15 | 2 | **191** | 1 of 20 | 1 |
| - | no further change raises the score with the limits held | | | | | | |
| (B) | and gap kinds out | 208 | 15 | 0 | **193** | **2 of 20** | 0 |

**Neither set meets the bar, so none shipped:** set A (step 3) raises the score to 191 but invented does not fall (2, as at HEAD) and one letter still prints in a silence; set B reaches 193 with nothing invented and nothing printed in a silence, but the carrier prints at seed 5206 as well as 5195. No single change from B brings the carrier back to 1 of 20. Taking out the handover backlog or the gap crossing leaves the score the same at every step; neither was taken out, since nothing shipped. The search's first pass counted the carrier over five seeds and walked to B; the twenty seeds caught it.

| unit | date | right | wrong | invented | score | printed in silence | spaces right | what changed |
|---|---|---|---|---|---|---|---|---|
| 539 task 3 | 2026-10-04 | 206 of 244 | 18 | 2 | **186** | 1 | 58 of 77, 4 added | the thirteen removed rules back in the tree behind switches, off; every rule measured; no set met the bar, so the reading is HEAD's |
| 541 task 1 | 2026-10-05 | 208 of 244 | 15 | 2 | **191** | 1 (`221502`, an `S` at 21.46 s) | 55 of 77, 2 added | set A ships: narrowness, three lone letters dropped, and a gap judged against its neighbours on by default (the owner, 2026-10-05: *ship it*); every hard limit as at HEAD |
| 541 task 2 | 2026-10-05 | 208 of 244 | 15 | 2 | **191** | 1, as task 1 | 55 of 77, 2 added | the ten rules that did not earn their place leave the tree, 844 lines of decoder source net; every stretch reads as after task 1 |
| 541 task 3 | 2026-10-05 | 208 of 244 | 15 | 2 | **191** | 1, as task 1 | 55 of 77, 2 added | the scan remembers its length and stays in Hamlet's settings; nothing that reads was changed |
| 544 task 1 | 2026-10-05 | 208 of 244 | 15 | 2 | **191** | 1, as at HEAD | 55 of 77, 3 added | a sender keeps the dot and dash line it last showed while a few marks hide the jump between its two kinds, the line redrawn from the marks now; a piece of a mark left out of the line and joined to its neighbour was measured at 188 (and 190 without the join) and not shipped |
| 544 task 2 | 2026-10-05 | 208 of 244 | 15 | 2 | **191** | 1, as at HEAD | 55 of 77, 3 added | the scan centres the tone at the CW pitch before the stay, measuring again after each move and learning which way the tone moves; nothing that reads was changed |
| 544 task 3 | 2026-10-05 | 208 of 244 | 15 | 2 | **191** | 1, as at HEAD | 55 of 77, 3 added | the scan calls a tone that never keys a carrier after 8 s (key-up under 0.15 of its 20 ms frames), leaves it, and passes by any peak within 400 Hz of its true frequency; nothing that reads was changed, and both noise limits print nothing |
| 544 task 4 | 2026-10-05 | 208 of 244 | 15 | 2 | **191** | 1, as at HEAD | 55 of 77, 3 added | a survey lists a place only where its tops at some height outnumber what noise that high gives by chance (under one false station in a hundred surveys, three sweeps at least); a tone is judged over all its pieces and over their strongest quarter; a peak drawn at 20 or higher earns a 6 s listen before it is empty; nothing that reads was changed |
| 546 task 1 | 2026-10-06 | 208 of 244 | 15 | 2 | **191** | 1, as at HEAD | 55 of 77, 3 added | the nine broken dahs are a real dip of 8 to 20 dB, 8 to 24 ms long, on the sender's own envelope, so the detector stands two marks; a gap under half the sender's own inside gap joined to one mark was measured at 154 (from the sender's first gaps) and 186 (once it had shown two kinds, its letter gaps and ten inside gaps), joined 2 of the nine and dits of the owner's fists, and was not shipped |
| 546 task 2 | 2026-10-06 | 208 of 244 | 15 | 2 | **191** | 1, as at HEAD | 55 of 77, 3 added | W1AW's form at 5 and 7.5 WPM overall reads as words from cold; after 18 WPM ordinary sending it prints letter by letter, because the sender's old letter and word gaps stay in its window and the line falls between them, under the new letter gaps. Three gaps in a row past the word line, each by √(7/3), taken as a new spacing, read the slow section as words and measured 191, 55 of 77, 2 added: spaces right did not rise, so not shipped |
| 546 task 3 | 2026-10-06 | 208 of 244 | 15 | 2 | **191** | 1, as at HEAD | 55 of 77, 3 added | the three spaces were lost to three lone letters dropped, not to the neighbour judgement (off: 55 of 77, 4 added): the opening `E` of 221745 and of 221805, and the space beside each with it; that rule off reads 191 (210 right, 17 wrong), 58 of 77, 4 added, so spaces added rises and it is not shipped; task 2's change does not bring them back |
| 546 task 4 | 2026-10-06 | 208 of 244 | 15 | 2 | **191** | 1, as at HEAD | 55 of 77, 3 added | traced, not fixed: on 143906 and on a synthetic 25 WPM call at 10 dB the detector stands 68 and 70 marks where the plain read hears 102 and 110, and keeps about 5 dahs of about 40; with the edge test off the 10 dB call keeps 34 dahs and reads its second half, so the flat-top test breaks the dahs into pieces the edge test then refuses; at 12 dB 98 marks stand and the call reads from its second QSY |
| 546 task 5 | 2026-10-06 | 208 of 244 | 15 | 2 | **191** | 1, as at HEAD | 55 of 77, 3 added | tidy: `neighbour` in the rule's name and `centre` in the scan's advancing line become American and `VoiceTests` passes; the edge test on noise candidates retires with the ten rules (HM-DEC-245); nothing that reads changed |

## Scans (work instruction 544, HM-DEC-248)

R88 is lifted for the scan catches work instruction 544 names; three are in the tree. `TheStrongStationsOverTests.TheScansTable` reads each station catch through the app's chain at the scan's passband (pitch 600 Hz, filter 500 Hz) and scores it against the session's offline read. **Every reference is pending until the owner confirms it by ear: pending stretches are scored and reported, never totalled.** When he confirms one, it joins the total.

| unit | catch | tone | reference | right | wrong | printed |
|---|---|---|---|---|---|---|
| 544 before (as at HEAD) | `catch-153810-7033367` | 860 Hz | **pending** | 103 of 152 | 39 | `A S A H HE MATRESS ISSOFT ESCANT FIN IA SAFESAEMTWI IEOUT WAE EHEE<BT> IV VER H TG SACK PAIN SOTHIW TIEUGODAY HAVE RM OTINE6 KKUAEESMG STT FMEEU RMYT MSEE WHATITA ISTE IHEE<BT> <BT>IREMEMBER AT M` |
| 544 task 1 | `catch-153810-7033367` | 860 Hz | **pending** | 109 of 152 | 33 | `A S A H HE MATRESS ISSOFT ESCANT FIN IA SAFESAEMTWI IEOUT WAE EHEE<BT> IV VER H TD SACK PAIN SOTHIS TIEURSDAY HAVE RM UTINE6 KKUAEESMG STT FMEEU RAYT MSEE WHATITA ISTE IHEE<BT> <BT>IREMEMBER AT I` |
| 544 before (as at HEAD) | `catch-154819-7050903` | 470 Hz | **pending** | 25 of 35 | 6 | `N E ANEI BTW■ I E GG IRIATE YOUR NICE KEYI N` |
| 544 task 1 | `catch-154819-7050903` | 470 Hz | **pending** | 25 of 35 | 6 | as before |

**The references, for the owner to confirm by ear:**

- `catch-153810-7033367`, 860 Hz: `THE MATRESS IS SOFT ES CANT FIND A SAFE SPOT WITHOUT PAIN HEE IVE NEVER HAD BACK PAIN SO THIS THURSDAY HAVE ROUTINE X EEE KK UW ESGG TO ASK FOR XRAY TO SEE WHAT I TWINTED HEE<BT> I REMEMBER AT AGE`
- `catch-154819-7050903`, 470 Hz: `KS AND BTW, I AGGREIRIATE YOUR NICE KEYIE R`
