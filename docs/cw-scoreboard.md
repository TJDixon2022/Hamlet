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

**Every later unit appends a row** to the totals table, and replaces the stretch table below it with its own.

## Totals

| unit | date | total (medium or better) | hard limits | what changed |
|---|---|---|---|---|
| 534 baseline | 2026-10-03 | 169 of 240 | hold | before any change; tag `before-scoreboard` |

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
