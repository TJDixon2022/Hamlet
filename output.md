```
UNIT: 534 - tasks 1 and 2 done, task 3 measured and not kept - 2026-10-03
UNIT GOAL: the owner's recordings are the scoreboard
NUMBER: scoreboard 169 to 185 of 240; thirteen rules removed; both hard limits hold
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`, and the order's gate held: `SHACK_FACTS.md`, `CwRunReader.cs` and `cw-2026-10-03-221828.wav` exist, there is no `CoreHMI.sln` or `MURC.sln`, and the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio beyond the owner's twelve recordings. HM-DEC-238 was free.

**How the session ran:**
- It took SESSION.lock and released it at the end.
- It wrote nothing to `RUN_LEDGER.md`, touched nothing under `tools\arbiter\`, and ticked no box.
- **HEAD was tagged `before-scoreboard` (331cb73e) and pushed** before any change.
- R88 was lifted for the twelve recordings and no other. The seven of 22:15 to 22:18 were committed with their sheets (`6c42860c`). The untracked `cases-*.txt` sheets were not read or committed.
- Nothing keys, transmits or writes to the radio.

**Task 1: the scoreboard.** Commit `2eb80ea0`.
- **`TheRecordingsScoreboard`** reads every recording through the live path as the app wires it: detector, sender's window, gate and reader, at the CW pitch and filter width from each sheet. Every sheet reads 600 Hz and 500 Hz.
- **Scoring:** what printed is scored against §8 as written. Letters right means the reference's letters less the edit distance with free ends, spaces ignored, a prosign counting as one letter. A printed letter belongs to the stretch of its recording nearest its pitch, within 60 Hz and a second.
- **The hard limits are asserted:** the first recording's exact text, and four noise runs (30 s and 180 s at two seeds each) that print nothing.
- **Baseline: 169 of 240** over the medium-or-better stretches, written to `docs\cw-scoreboard.md`.
- **`EachStretchReadOffline`** reads each stretch non-causally at its pitch, through a 40 Hz two-pole low-pass run forward and back, cut at the midpoint of its own two level clusters. The differences from §8 are in section 3; no reference was changed.

**Task 2: every rule measured against the recordings.** Commit `067f763b`.
- **The method:**
  - `CwRules` gives each shape-side rule a switch, and every one of 24 rules was switched off alone (the rule table is in section 3).
  - The rules whose removal raised or held the total were then removed one at a time, best gain first, re-measuring after each.
  - Every remaining rule was then switched off again with the removed ones out, twice more, until no removal raised or held the total.
- **Removed, in order, with the total after each:**
  1. the 0.2 standing line, 182;
  2. three lone letters dropped together, 184;
  3. quieter-mark admission inside a letter, 184;
  4. the pause that is not a word, 184;
  5. a hand's two kinds, 185;
  6. key-up, 185;
  7. the five-dit floor, 185;
  8. the shape's inside-letter gap tightness, 185;
  9. the rectangle fit, 185;
  10. narrowness, 185;
  11. the neighbour judgement of gaps, 185;
  12. the shape's letter-gap tightness, 185;
  13. the neighbour split of marks, 185.
- **Removed from the code, not switched off:**
  - The scoreboard read 185 with the code gone, the same as the switches gave.
  - `TheRectangleIsFittedTests` and `MostEdgedNoiseBarsAreNotNarrow` were deleted with their rules; the "before" rows that switched key-up, narrowness or the fit off were removed from three diagnostics.
  - Removing a hand's two kinds also removes it from the light's "shape forming" count.
- **Eleven rules kept**, each lowering the total or breaking a hard limit when off (section 3). `CwRules` keeps a switch for each so the next unit can measure again.
- **The handover backlog holds the total when off and stays:** switching it off means putting back the skip it replaced, a rule added rather than removed. No recording has a reply that overlaps the first station's last letter, so nothing here exercises it.

**Task 3: heavy keying.**
- The one line still drawn from the dit is the gap unit before the sender's gaps settle it (`gapDit = dit + smear`).
- Taking it from the median of the sender's own gaps inside letters held the total at 185. That is not a rise, so it was reverted and nothing was committed.
- The 22:17 QSO's `FER` still reads `ENER`; the fault is not that line.

**Records:**
- HM-DEC-238 in `DECISIONS.md`, and the `CLAUDE.md` row.
- `PHASE_OUTCOME` (both copies) has `## UNIT 534 - STEP 12`.
- `PHASE_STATUS` (both copies) names 534.
- Version 1.13.218 to 1.13.219.
- `docs\cw-scoreboard.md` holds both board tables and the rule table.

**Build and app line:** build 0 warnings, 0 errors. App carry-forward 276 of 278. The two losses, both `ThePsk31ConversationCardTests` at 1 ms ("You've caused dispatcher loop"), pass alone.

## 2. What the owner should expect

- **Rebuild.**
- **The scoreboard went from 169 to 185 of 240 letters right** on your own recordings. Loud noise still prints nothing, and your first recording still reads `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`.
- **Thirteen rules came out.** Each had been added to fix a synthetic signal, and on your recordings Hamlet read as well or better without it:
  - the 0.2 shape line a station had to clear before it could print;
  - how evenly a sender spaced its letters, which counted against hand senders;
  - the five-dit word floor;
  - key-up, narrowness and the rectangle fit;
  - the rest are listed in section 1.
- **What your QSOs read now:**
  - **14:40:45** reaches `DE KG8V K` (was cut off after `D`), but the T of TEMP is lost: `NEMP5757<BT>BTUBOBDEKG8VK`.
  - **14:39:06** now prints `SEIIENUVIQSYQSYDEWB2FUVEE`, the `QSY DE W` fragments you heard.
  - **22:15:48** reads `MY SCOET` (was `SCOMASTEN`).
  - **22:17:45** reads `TONITE.EIEEMESIRDTOTT` (was 8 letters right, now 14).
  - **22:18:05** and **22:18:28** are about the same, and `FER` still reads `ENER` on the heavy keying.
  - **14:40:20** is unchanged.
- **Two things look worse and need you** (section 4):
  - On loud noise the light now turns green ("shape found · hold here") for a few seconds at one noise seed, though nothing prints.
  - A carrier keyed at random now also prints beside a clean sender, and at 725 Hz instead of 825.
- **Synthetic cases read worse in places**, mostly word spaces: a drifting hand runs `BROWNFOX` together, and the straight key runs `SKCCDEN0CALL` together. Section 3 lists them.
- **Some of your references may be wrong.** Reading each stretch offline, a few letters come out differently from the web session's references. The biggest is the end of 22:18:28 and the start of 22:18:51, where the web session wrote `EEV CW` and the audio looks like `73 <AR> W`. Section 3 has both element lists; please listen.

## 3. What you should see

**The scoreboard, before (`before-scoreboard`) and after:**

| recording | pitch | conf. | reference | printed before | right | printed after | right |
|---|---|---|---|---|---|---|---|
| 200157 | 662.8 | verified | `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` | `FERCHAT<BT>BEST7V73<SK>KC4ZGPDEWA` | 27/27 | same | 27/27 |
| 143906 | 514.2 | none | - | `` | - | `SEIIENUVIQSYQSYDEWB2FUVEE` | - |
| 143951 | 499.5 | low | `O WAEIIEURD U AGN ES` | `` | 0/16 | `EOMTTTOMOTMOEES` | 4/16 |
| 144020 | 499.5 | medium | `ES OK ON PA <BT>` | `ESOKONPA<BT>` | 9/9 | same | 9/9 |
| 144020 | 599.9 | medium | `WX IN NETAGIT IUN TEMP E` | `WXINNETAGITIENTEMP` | 17/19 | same | 17/19 |
| 144045 | 599.9 | high | `N TEMP 57 57<BT> BTU BOB DE KG8V K` | `NTEMP5757<BT>BTUBOBD` | 17/23 | `NEMP5757<BT>BTUBOBDEKG8VK` | 22/23 |
| 221502 | 491.5 | low | `ED OF ITS OWN HEE BK BK WHAT BUG AE US E ENIE EE ITS A 66 K` | `EDOFITSOWNHEENEKESKESITHAHDEN` | 20/42 | `EDOFITSMTWNHEEBKBKWHATBUGAEIEUIE■■` | 27/42 |
| 221530 | 491.5 | medium | `6 CHAMPION BK` | `BCSAMENION` | 5/11 | `HCSAMENIONNEN` | 5/11 |
| 221530 | 598.4 | medium | `EN FB WHEN I WAS AGE 12 I LEARNED CW USING A V` | `IIESMHENI■SAGE12ILE<AR>EDCWUSINGAV` | 25/34 | `IHESMHENI■SAGE12ILEAREDCWUSINGAV` | 27/34 |
| 221548 | 597.7 | low | `2 I LEARNED CW USING A V BPLX Z EPS` | `IMRIUSINGAVBPLXZEPS` | 17/26 | `IMEEEAREDCWUSINGAVBPLXZEPS` | 22/26 |
| 221548 | 498.0 | medium | `YRHEE MY SCOUT MASTER` | `EYRHSCOMASTEN` | 11/18 | `EWRHEEMYSCOETTQSTEN` | 13/18 |
| 221745 | 501.7 | medium | `E E DAND ON 40M TONITE . EUR EE H RD TOO` | `IANDTMN4MEMMTONI` | 8/29 | `TEMMTONITE.EIEEMESIRDTOTT` | 14/29 |
| 221805 | 601.3 | medium | `ET ON 40T S THESE DAYS . TNX FER ANOTHER FT` | `MEUMTTSTHESEDAYI.TNXENERANOTHERF` | 24/33 | `E40TUTHESEDAYI.TNXENERANOTHERF` | 25/33 |
| 221828 | 601.3 | medium | `FER ANOTHER FB QSO ES HOPE U HAVE AGN ED ES BEST` | `ENERANOTHERFNEQSMESHMWIEVEATNEDESBESTEHEEIER` | 26/37 | `ENERANOTHERFNEQSMESHMWETEIEVEATNEDESBEST` | 26/37 |
| 221851 | 601.3 | low | `BEST EEV CW 2L CQ DE NA8SB K` | `SESTILCTADENAME` | 8/21 | `TTTMEEEUIEMTICWJLKTADENAMEEENEK■DETSKETEER5SEXIEEESIIIIEEEI` | 8/21 |
| **total** | | | | | **169/240** | | **185/240** |

**The rule table.** Each rule switched off alone at the baseline of 169. "After" is the total once a removed rule came out, or, for a kept rule, with all thirteen out and it off too. "Broke" means a hard limit broke.

| rule | off alone | after | fate |
|---|---|---|---|
| the 0.2 standing line | 182 | 182 | removed |
| three lone letters dropped | 172 | 184 | removed |
| quieter-mark admission | 172 | 184 | removed |
| the pause | 169 | 184 | removed |
| a hand's two kinds | 165 | 185 | removed |
| key-up | 159 | 185 | removed |
| five-dit floor | 167 | 185 | removed |
| shape: inside-letter gap tightness | 160 | 185 | removed |
| rectangle fit | 165 | 185 | removed |
| narrowness | 166 | 185 | removed |
| neighbour judgement of gaps | 174 | 185 | removed |
| shape: letter-gap tightness | 168 | 185 | removed |
| neighbour split of marks | 169 | 185 | removed |
| lone letter | 173, broke (`HA T`) | 182 | kept |
| cold-start word line at √21 | 169, broke (`F ER C H AT`) | 185, broke | kept for a hard limit |
| retry over newer marks | 161 | 152 | kept |
| settle at key-down | 155 | 179 | kept |
| the sender's own window | 149, broke | 148, broke | kept |
| release under 0.1 | 181 | 180 | kept |
| first pick waits a word gap | 169 | 181 | kept |
| handover backlog | 169 | 185 | kept (off is a skip added) |
| a silent sender is not a candidate | 141 | 155 | kept |
| edges | 144 | 113, broke (noise prints) | kept |
| a mark's own shape | 151 | 162, broke (noise prints) | kept |

**References read differently offline** (§8's element list, then the reading here; the reference was not changed):

| stretch | §8 | here | letters |
|---|---|---|---|
| 200157 at 662.8 | `- [670] -- [115] ... [360] ...-` | `- [662] --.... [351] ...-` | §8 corrected to 7V; here the 7 carries an extra dit |
| 143951 at 499.5 | `.-. [525] -..` and `-. [1045] . [335] ...` | `.- [519] -.-` and `-. [638] . [373] . [329] ...` | `RD` / `AK`; an extra E |
| 144020 at 499.5 | `-.- [1070] ---` | `-.- [742] . [298] ---` | an E between K and O |
| 144020 at 599.9 | `.. [420] ..- [250] -.` | `.. [411] .- [240] -.` | `U` / `A`: the U's last two elements are keyed with a 2 dB notch for a gap; Hamlet prints `E`. Two dits before the W at 9.5 to 11 s here |
| 144045 at 599.9 | `..- [940] -...` | `..- [298] . [612] -...` | a faint E in the 940 ms gap |
| 221502 at 491.5 | `. [635] -...` … `. [685] ..- [265] ... [375] . [355] . [150] -. [170] .. [130] .` | `. [407] . [196] -...` … `.. [162] . [129] . [147] ..- [262] ..- [177] ... [129] .. [142] -. [167] --. [152] ..--..` | a rough fist; `HEE` / `HEEE`, `US E ENIE` / `IEEUUSING?` |
| 221530 at 491.5 | `-...` (end) | `--.` | `BK` / `GK` |
| 221530 at 598.4, 221548 at 597.7 | `.. [130] -.` | `..-.` | `USING` / `USFG` (a 130 ms gap at a 100 ms letter line); `.-.. [180] -..-` reads as one letter here |
| 221548 at 498.0 | `- [150] . [170] .-.` | `- [149] ...-.` | `TER` / `T■` |
| 221745 at 501.7 | `. [440] -.. [180] .-` | `- [367] .-.. [173] .-` | `E DA` / `T LA`: a 155 ms element read as a dah at the 150 ms split; further on `.---.-.` / `---.-.` and `.... [745] .-.` / `..- [252] .-. [161] .-.` |
| 221805 at 601.3 | `-----` | `----` | `0` / `■` |
| 221828 and 221851 at 601.3 | `- [790] . [145] . [195] ...- [510] -.-. [205] .-- [175] ..--` | `- [223] - [255] .... [191] ...-- [129] .-.-. [196] .-- [171] ..--` | `T E E V C W 2` / `T T H 3 <AR> W 2`: possibly `73 <AR> W2L`. The later part of 221851 differs throughout |

**Synthetic cases that changed** (against unit 533's runs, which match `before-scoreboard`):

| case | before | now |
|---|---|---|
| a lone `T E T T E` after a call | `CQ CQ DE N0CALL N0CALL K` | `CQ CQ DE N0CALL N0CALL K T` (three-lone drop removed; red) |
| clean call, every mark's shape | lowest 0.43 | three fragments of shape near 0 stand (key-up and narrowness removed; red) |
| a weak dit inside a letter | `CQ CQ DE N0CALL N0CALL K` | `CQ CQ DE N0CA DL N0CALL K` (quieter marks removed; red) |
| 35 WPM at 10 dB | `CQ CQ DE N0CALL N0CALL K` | `CQ CQ SE N0CALL N0CALL K` (red) |
| the call at 10 dB, weak | `CQ CQ DE N0CALL N0CALL K` | `CQ NEQDEN0CALL N0CAEIL A` (test passes on marks) |
| loud noise, light (seed 5212) | never green | green for 377 steps (red) |
| loud noise, 3 minutes | 0 marks stood | 37 stood; prints nothing |
| a random carrier | printed at 825 Hz (red) | prints at 725 Hz, no longer at 825 (red) |
| clean sender 100 Hz from a carrier | `` | `TTNOAM0TT ETMTTYTMTT` |
| clean sender alone through the filter | `CQ CQ DE N0CALL N0CALL K` | `CQ CQ DE N■ CALL N0 RALL K` |
| a fist scattered by a third | `CQ CQ DE N0CALL N0CALL K` | `CQ CQ DE N0NNALL N0CALLK` |
| drifting hand 13/18/13 | `TEXT IS FROM SEPTEMBER 2024 AND THE QUICK BROWN FOX …` | `TEXT IS FROMSEPTEMBER 2024AND THE QUICK BROWNFOX …` (5 reds) |
| straight key, two fifths | `CQ CQ SKCC DE N0CALL N0CALLK` | `CQCQ SKCCDEN0CALL N0CALLK` (5 reds) |
| 25/35/25 WPM, weak through the filter | whole | `2024ANDTHEQUICKBROWNFOX` (red) |
| the tightening fist | `CQCQ DE N0CALL N0CALL K` | `CQCQ DE N■CALL N0CALL K` |
| synthetic QSO | `… K WE MAW DE K3ZZ …` | `… K TAW DE K3ZZ …` |
| 14:40:45 letters test | `NTEMP5757<BT>BTUBOBD` | `NEMP5757<BT>BTUBOBDEKG8VK` (still red, on the T) |

**Reds unchanged:**
- the 8 dB call;
- Farnsworth and fast at 10 dB;
- a letter from noise (blocks);
- the clean sender at the edge;
- the strength table at 8 and 10 dB;
- `AFistThatTightensIsFollowed`;
- unit 533's three reply tests.

## 4. What's blocking us

1. **The light turns green on loud noise. Your ruling, because it is what the screen asserts (§0.0):**
   - What happens: with the shape's gap terms gone, noise sequences can score over the light's 0.2 line, and at one seed the light read "shape found · hold here" for 377 steps. Nothing printed.
   - Industry answer: a light should claim no more than the printer does. A means the light is green only while a sender is printed or qualified to print.

   | option | for | against |
   |---|---|---|
   | A. green only when the gate would print the sender | the light and the terminal agree; no new number | the light goes green a word gap later |
   | B. raise the light's line until noise never reaches it | keeps the light early | a number fitted to noise seeds |
   | C. put the 0.2 standing line back | restores the old light | gives back 13 letters on your recordings |

2. **A carrier keyed at random still prints**, now at 725 Hz rather than 825, and now also beside a clean sender 100 Hz away (synthetic). Loud noise, the hard limit, still prints nothing. Should a random-carrier case become a third hard limit for the scoreboard?
3. **References to listen to**, section 3: above all the end of 22:18:28 and the start of 22:18:51 (`EEV CW` against what reads as `73 <AR> W`), and 14:40:45's 940 ms gap, where a faint E reads offline.
4. **Heavy keying (22:17 QSO): `FER` reads `ENER`**, and gap lines from the sender's own gaps did not change the total. The F's first dit and its 40 ms gap are where to look next.
5. **Word spacing on hand senders is worse on the synthetic cases**, with the five-dit floor gone. Your recordings did not miss it, and the word line now rests on √21 and √(7/3) alone.
6. **Pre-existing app reds** outside the line are untouched.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
