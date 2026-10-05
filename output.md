## 1. What Claude did

- **Session:** the development computer, in Claude Code. The prompt claimed `PROJECT: Hamlet`.
- **Gate:** checked in `C:\Source\HamLet`.
  - Present, as required: `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `cw-2026-10-03-221828.wav`.
  - Absent, as required: `CoreHMI.sln` and `MURC.sln`.
- **The first paste of this order was refused** at the gate. It named `cw-2026-10-04-230144.wav`, which is not in the tree. The second paste named the twelve recordings on the scoreboard and passed.
- **Nothing in this report is evidence about the radio.**
- **Work instruction 539, run by hand on `main`:**
  - SESSION.lock was taken and released.
  - Nothing was written to RUN_LEDGER.md, and nothing under `tools\arbiter\` was touched.
  - No box in PHASE_PLAN.md was ticked.
  - Nothing was keyed or transmitted.
  - R88 was lifted for the owner's twelve recordings only.
- **Tag:** HEAD was tagged `before-false-characters` and pushed before any engine change.
- **Version:** 1.13.223 → 1.13.224. **Ruling:** HM-DEC-243.

**Task 1: the score counts wrong and invented** (commit `2ae73649`).
- **The keying map** (`TheKeyingMapTests`, `docs\cw-keying-map.md`):
  - Each recording is read offline and non-causally at every 25 Hz pitch from 300 to 900 Hz. The audio is mixed to nought and low-passed at 30 Hz, four passes forward and back.
  - A mark is 25 ms or more where a pitch stands 13 dB over the median of every pitch at that instant.
  - A mark overlapped by one 6 dB louder within 100 Hz is dropped as that one's leak.
  - A station is a run of neighbouring pitches with five marks or more.
  - Loud noise keys no station.
- **The first version failed:** it set each pitch's floor over the whole recording. That keyed three stations in loud noise and one station from 300 to 900 Hz on every real recording. It was replaced before anything was scored.
- **The map reads no reference.** It finds every station the references name, plus one with no reference: `143906` at 513 Hz.
- **Timing:** the live path's marks start within a few milliseconds of the map's. The median is 0 to 5 ms, and the 90th percentile is under 7 ms on ten of the twelve recordings. So invented is read with 10 ms of tolerance.
- **Wrong:** printed letters the letters alignment counts as a wrong letter or an extra, inside the medium-or-better stretches. A letter that is both wrong and invented counts once, as invented.
- **Invented:** printed letters, in any recording, whose span overlaps no map mark within one bin of their pitch.
- **Score = right − wrong − invented.**
- **Baseline: 206 right, 18 wrong, 2 invented, score 186.** It is in `docs\cw-scoreboard.md`.

**Task 2: real silence prints nothing** (commit `934f137e`).
- **The limit:** every stretch of two seconds or more where no station keys, on the map, joins the hard limits.
- **At HEAD one letter prints in one:** a `■` in `221502` at 21.46 s at 500 Hz, 60 ms after the station's last mark, in the silence from 21.4 to 24.5 s.

**Task 3: every rule measured on the new score** (commit `8159079b`).
- **The thirteen rules removed in work instruction 534** are back from the tag `before-scoreboard`. Each is behind a `CwRules` switch that is off:
  - the 0.2 standing line
  - three lone letters dropped
  - quieter marks
  - the pause
  - a hand's two kinds
  - key-up
  - the five-dit floor
  - the shape's inside-letter tightness
  - the shape's letter-gap tightness
  - the rectangle fit
  - narrowness
  - the neighbour judgement of gaps
  - the neighbour split of marks
- **The crowds narrowing** gained a switch.
- **The scoreboard runs in parallel.** A board takes 14 s, against 44 s before.
- **The carrier is counted over its twenty seeds.** The search's first pass counted five, walked to a set that printed at a sixth seed, and the twenty-seed test caught it. The search was then rerun from HEAD.
- **How the search was stepped:** one change at a time, best first, re-measuring after each. A change was taken only where no hard limit got worse than at HEAD.
  1. Narrowness back in: 188.
  2. Three lone letters dropped back in: 190.
  3. The neighbour judgement of gaps back in: **191**.
  4. Then no change raised the score with the limits held.
- **Neither best set met the bar, so none shipped:**

  | set | rules | score | invented | in silence | carrier | bar |
  |---|---|---|---|---|---|---|
  | **A** | the three above | 191 | 2, as at HEAD | 1, as at HEAD | 1 of 20 | invented does not fall |
  | **B** | A, with gap kinds also out | 193 | 0 | 0 | **2 of 20** | breaks the carrier limit |

  No single change from B brings the carrier back.
- **The defaults read exactly as HEAD:** 206 / 18 / 2 / 186. I checked this after the change.

**Recorded, at the owner's order:** HM-DEC-243, *False characters count against the score*, in `DECISIONS.md` and the `CLAUDE.md` §1 index. Full text:

> **False characters count against the score.** The owner, 2026-10-04, at the radio: *"We are still having way too much false character. With the positive identification of shape in noise and the identification of multiple characters, we should essentially have zero. And we got a lot."* HEAD was tagged `before-false-characters` before any engine change.
>
> **The score is letters right, less wrong, less invented.**
> - **Wrong:** a printed letter the alignment counts as not the reference's, a wrong letter or an extra, inside a stretch of medium confidence or better.
> - **Invented:** a printed letter, in any recording, whose marks overlap no keying on a keying map within one bin of its pitch, with 10 ms of tolerance. A letter that is both counts once, as invented.
>
> **The keying map** (`docs/cw-keying-map.md`) is read offline at every 25 Hz pitch.
> - **What counts as keying:** a mark is a pitch standing 13 dB over the band's median at that instant for 25 ms or more. The radio's AGC moves every pitch together, so it cancels.
> - **The first floor failed:** a floor over the whole recording keyed one station from 300 to 900 Hz on every recording, and three stations in loud noise.
> - **It does not read the references.** It finds a station at 513 Hz in `cw-2026-10-03-143906` that has none.
>
> **Real silence prints nothing; this joins the hard limits.** A silence is two seconds or more with no station keying. At HEAD one letter prints in one: a placeholder in `cw-2026-10-03-221502` at 21.46 s, 60 ms after the station's last mark.
>
> **The baseline is 206 right, 18 wrong, 2 invented: a score of 186.** The junk the owner sees is nearly all misreading of stations that are keying, counted as wrong, not letters printed from nothing.
>
> **Every rule was measured alone on the new score.**
> - **Restored behind switches:** the thirteen removed in work instruction 534 are back from the tag `before-scoreboard`, each behind a switch in `CwRules` that is off.
> - **The search:** one change at a time, taking a change only where no hard limit is worse than at HEAD.
> - **Set A, 191:** narrowness, three lone letters dropped, and the neighbour judgement of gaps, each restored. Invented stays at 2.
> - **Set B, 193:** set A with gap kinds also taken out. Nothing is invented and nothing prints in a silence, but the random carrier prints at 2 of its 20 seeds against 1 at HEAD.
>
> **No set met the bar, so none shipped.** Set A does not lower invented, and set B breaks a hard limit. No rule is restored or removed by default: the final score is the baseline, 186, and the reading is HEAD's. The first pass counted the carrier over five seeds, walked to set B, and the twenty-seed test caught it; the scoreboard now counts all twenty.

## 2. What the owner should expect

- **Rebuild.** Nothing you hear on the air changes with this unit, and Hamlet reads exactly as it did yesterday.
- **The scoreboard now charges for false characters.** Across your twelve recordings, the number is letters right, less wrong, less invented.
- **Invented, before and after: 2 and 2.** On a fair map of where every station keys, Hamlet printed only two letters over no keying at all: an `E` in 14:39:51 and a `■` in 22:15:02. The second is a fragment printed in the 3-second pause after a station stopped.
- **The rest of the junk is wrong, not invented.** The `SES E IE E IEA` you see is Hamlet misreading a station that is really there: 18 wrong letters in the counted stretches, and more in the low-confidence ones.
- **Real letters it took to remove the junk: none, because nothing was removed.**
- **Rules that came back: none.** I brought all thirteen rules that unit 534 took out back into the code, switched off, and measured every rule:
  - **Set A:** narrowness, dropping three lone letters, and judging a gap against its neighbours, together. They take the score from 186 to 191 (2 more letters right, 3 fewer wrong). They leave invented at 2, though, and the order asked for invented to fall.
  - **Set B:** set A with gap kinds also taken out. Nothing is invented, nothing prints in a silence, and the score is 193. But the random carrier then prints on 2 of its 20 seeds instead of 1, which breaks your hard limit.
  - So I shipped neither, and the choice is yours (section 4).
- **What will look red but is not new:**
  - The scoreboard test is red on the carrier at seed 5195, as before. It is now also red on the silence print, the new limit, which fails at HEAD too.
  - The 20-seed carrier test is red on seed 5195, as before.
  - Three `AReplyIsReadFromItsFirstLetter` tests are red, as before.
- **Build:** clean. **App carry-forward:** 278 of 278.
- **Pushed:** everything, to `main`.
- **Untouched and uncommitted, from before this unit:** `PARKED.md`, `RUN_LEDGER.md`, `WORK_INSTRUCTIONS.md`, `.run-unit\denials.txt`, the four `.run-unit\reports` files, and `tests\fixtures\cw\captured\cases-2026-10-0{2,3}.txt`.

## 3. What you should see

**The score per stretch at HEAD.** After this unit it is the same, since no default changed.

| recording | pitch | stretch | confidence | printed | right | wrong | invented |
|---|---|---|---|---|---|---|---|
| `200157` | 662.8 | 0-30 s | verified | `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` | 27 of 27 | 0 | 0 |
| `143906` | 514.2 | 0-30 s | none | `IEE II E NIEEE IET TAEEKEEIEEII IMESE ITT` | 0 of 0 | 0 | 0 |
| `143951` | 499.5 | 14.5-30 s | low | `EWAE IIEWRK I AGN ES` | 12 of 16 | 3 | 1 |
| `144020` | 499.5 | 0-12.5 s | medium | `ES OK ON PA <BT>` | 9 of 9 | 0 | 0 |
| `144020` | 599.9 | 9.5-30 s | medium | `WX IN NETAGIT IEN TEMP` | 17 of 19 | 1 | 0 |
| `144045` | 599.9 | 0-30 s | high | `N E MP 57 57<BT> BTU BOB DE KG8V K` | 22 of 23 | 0 | 0 |
| `221502` | 491.5 | 0-30 s | low | `ED OF ITS OWN HEE BK BK WHATBUGAEI EUI E■ ■` | 29 of 42 | 1 | 1 |
| `221530` | 491.5 | 0-9.5 s | medium | `H CHA MPIMTN` | 6 of 11 | 1 | 0 |
| `221530` | 598.4 | 9-30 s | medium | `IHES MHENI■S AGE12ILEARED CW USINGA V` | 27 of 34 | 3 | 0 |
| `221548` | 597.7 | 0-18.5 s | low | `I M IEE IEE E USINGA V BPLX Z EPS` | 18 of 26 | 5 | 0 |
| `221548` | 498.0 | 18-30 s | medium | `E WRHEE MYSCOET T MASTEN` | 14 of 18 | 3 | 0 |
| `221745` | 501.7 | 0-28 s | medium | `E IAND TMN 4MEMM TONITE . EUROWEE H IRD TOO` | 19 of 29 | 9 | 0 |
| `221805` | 601.3 | 5-30 s | medium | `E 40T U THESE DAYS. TNX FERANOTHERF` | 28 of 33 | 1 | 0 |
| `221828` | 601.3 | 0-30 s | medium | `FER ANOTHER FB QSO ES HOPE U HA VE AGN ED ESBEST EV A MU` | 37 of 41 | 0 | 0 |
| `221851` | 601.3 | 0-30 s | low | `SES E IE E IEA E I GE EI E NAERE I BK` | 4 of 20 | 6 | 0 |

**Totals, medium confidence or better:**

| | right | wrong | invented | score | printed in silence |
|---|---|---|---|---|---|
| before, HEAD | 206 of 244 | 18 | 2 | **186** | 1 |
| after (as shipped) | 206 of 244 | 18 | 2 | **186** | 1 |
| set A, not shipped | 208 | 15 | 2 | **191** | 1 |
| set B, not shipped | 208 | 15 | 0 | **193** | 0 |

**The silences**, two seconds or more with no station keying:

| recording | silences | printed in them at HEAD |
|---|---|---|
| `200157` | 7.3-9.6 s | nothing |
| `143906` | 7.9-13.7, 24.5-26.8, 27.1-30.0 s | nothing |
| `143951` | 0.0-5.7, 6.4-13.8 s | nothing |
| `144020` | 11.3-13.6 s | nothing |
| `221502` | 21.4-24.5 s | `■` at 21.46 s, 500 Hz |
| the other seven | none | — |

**Every rule alone, from HEAD (score 186).** The carrier is counted over its 20 seeds:

| change | right | wrong | invented | score | carrier prints | silence prints |
|---|---|---|---|---|---|---|
| out: first pick waits one word gap | 205 | 14 | 3 | **188** | 2 of 20 | 2 |
| back in: narrowness | 209 | 17 | 4 | **188** | 1 of 20 | 1 |
| back in: three lone letters dropped | 204 | 16 | 1 | **187** | 1 of 20 | 1 |
| back in: key-up | 207 | 17 | 3 | **187** | 1 of 20 | 3 |
| back in: the neighbour judgement of gaps | 207 | 18 | 2 | **187** | 1 of 20 | 1 |
| out: cold-start word line at √21 (first recording splits FER) | 206 | 18 | 2 | **186** | 1 of 20 | 1 |
| out: handover backlog | 206 | 18 | 2 | **186** | 1 of 20 | 1 |
| out: gap kinds | 206 | 18 | 2 | **186** | 2 of 20 | 1 |
| out: word line at the clusters' crossing | 206 | 18 | 2 | **186** | 1 of 20 | 1 |
| back in: the pause | 206 | 18 | 2 | **186** | 1 of 20 | 1 |
| back in: the five-dit floor | 206 | 18 | 2 | **186** | 1 of 20 | 1 |
| back in: the rectangle fit | 206 | 18 | 2 | **186** | 1 of 20 | 1 |
| back in: the neighbour split of marks | 206 | 18 | 2 | **186** | 1 of 20 | 1 |
| out: split overlapping letter and word gaps | 205 | 18 | 2 | **185** | 1 of 20 | 1 |
| back in: the shape's inside-letter tightness | 205 | 18 | 2 | **185** | 1 of 20 | 1 |
| back in: the shape's letter-gap tightness | 205 | 18 | 2 | **185** | 1 of 20 | 1 |
| out: lone letter | 208 | 21 | 4 | **183** | 1 of 20 | 2 |
| out: settle at key-down | 200 | 18 | 2 | **180** | 1 of 20 | 1 |
| out: release under 0.1 | 203 | 21 | 2 | **180** | 1 of 20 | 1 |
| back in: the 0.2 standing line | 197 | 15 | 5 | **177** | 1 of 20 | 3 |
| back in: quieter marks | 199 | 17 | 5 | **177** | 1 of 20 | 4 |
| out: a mark's own shape | 184 | 12 | 0 | **172** | 1 of 20 | 0 |
| out: two kinds held over the last ten marks | 206 | 16 | 21 | **169** | 9 of 20 | 2 |
| back in: a hand's two kinds | 210 | 18 | 24 | **168** | 5 of 20 | 16 |
| out: a silent sender is not a candidate | 175 | 14 | 2 | **159** | 1 of 20 | 1 |
| out: a mark crowds the last only where it overlaps it or is a piece | 187 | 38 | 2 | **147** | 1 of 20 | 1 |
| out: retry over newer marks when speed changes | 156 | 11 | 1 | **144** | 0 of 20 | 1 |
| out: edges | 95 | 3 | 3 | **89** | 1 of 20 | 1 |
| out: the sender's own window (first recording breaks) | 146 | 56 | 2 | **88** | 1 of 20 | 0 |

**The search**, one change at a time, with no hard limit worse than at HEAD:

| step | change | right | wrong | invented | score | carrier | silence |
|---|---|---|---|---|---|---|---|
| 0 | HEAD | 206 | 18 | 2 | 186 | 1 of 20 | 1 |
| 1 | narrowness back in | 209 | 17 | 4 | 188 | 1 of 20 | 1 |
| 2 | three lone letters dropped back in | 207 | 15 | 2 | 190 | 1 of 20 | 1 |
| 3 | the neighbour judgement of gaps back in | 208 | 15 | 2 | **191** | 1 of 20 | 1 |
| — | no further change raises it within the limits | | | | | | |
| (B) | and gap kinds out | 208 | 15 | 0 | 193 | **2 of 20** | 0 |

**The keying map in brief**, per recording, with each station's pitch and when it keys:

- `200157`: 663 Hz, 0.2 to 7.3 s and 9.6 to 29.9 s.
- `143906`: 513 Hz, with no reference.
- `143951`: 522 Hz, 13.8 to 29.3 s.
- `144020`: 498 Hz to 11.3 s, then 598 Hz from 13.6 s.
- `144045`: 598 Hz, plus five marks at 495 Hz in its last half second.
- `221502`: 491 Hz.
- `221530`: 488 Hz to 9.2 s, then 597 Hz.
- `221548`: 596 Hz to 17.9 s, then 496 Hz.
- `221745`: 507 Hz, plus 605 Hz briefly.
- `221805`: 508 Hz to 6.2 s, then 605 Hz.
- `221828` and `221851`: one station at about 606 Hz throughout.

The full map is in `docs\cw-keying-map.md`.

## 4. What's blocking us

- **Your ruling: ship set A, ship set B, or neither.** The full trade-off is in section 2.
  - **Set A**, three restored rules: the score goes 186 → 191 and nothing gets worse, but invented stays at 2.
  - **Set B**, set A with gap kinds out: the score goes to 193, invented to 0, and silence prints to 0, but the random carrier prints at seed 5206 as well as 5195.
  - Each ships as a two-line change in `CwRules` (`Restored`, `TakenOut`).
- **The silence limit is red at HEAD.** One placeholder prints 60 ms into the pause after `221502`'s station stops. Only set B, or rules far too costly (a mark's own shape, the sender's own window), clear it.
- **What the score can't see:**
  - **Spaces.** Taking out the gap crossing or the handover backlog leaves the score unchanged. Neither came out, because nothing shipped. Taking out the crossing would undo HM-DEC-242's fix for `221530`, and it would turn `TheSenderWhoSpacedItsLettersIsNotSplit` red.
  - **Weak stations.** The map does not see a station under 13 dB over the band. A letter read from one counts as invented.
- **The thirteen removed rules are now in the tree again, switched off**, about 800 lines. Keeping them makes the next measurement a one-line change. Deleting them, as unit 534 did, keeps the tree smaller, and the tag `before-false-characters` holds the tree without them either way.
- **The red tests are unchanged from HEAD:**
  - the carrier at seed 5195, in the scoreboard and the 20-seed test;
  - three `AReplyIsReadFromItsFirstLetter` tests;
  - the scoreboard's new silence limit.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
- **Unit 539, 2026-10-04:** ship rule set A, rule set B, or neither (above). Waiting on the owner. Both are switch lists in `CwRules` and neither is on.
