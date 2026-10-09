## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks (`SHACK_FACTS.md`, `docs\cw-scoreboard.md`, `docs\carry-forward-tests.txt` present; no `CoreHMI.sln`, no `MURC.sln`) and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 21:41:57 through `tools\arbiter\lock.bat take` and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. Nothing keys, transmits or writes to the radio; no recording added. **Tonight's W1AW capture (`w1aw-2026-10-08-*`) was not in the tree**, so the unit measured on what is. Version 1.13.249 to **1.13.250**. Ruling **HM-DEC-270**, the number the order gave. HM-DEC-269 is not in the record: it was held for unit 565, which this tree never ran. The decision-log test now names 269 as a known gap beside the others, with that reason. Nothing was recorded under §12.1.

**Task 1, the measurement** (`b419bcd7`), shipped as a report. `TheDahThatComesApartTests.EachSpecimenThroughTheSendersWindow` reads each recording through the app's chain with the sender's own window traced. For each specimen it draws the window a hop a character (`.` under 0.4 of the contrast, `#` from 0.4, `+` from 0.6). It gives the printed pieces, the gaps between them, and the sender's figures over the 30 s before: inside-gap p10, centre and p90, letter-gap p10, dah centre, dit and speed.
- **The W1AW table's 20 wrong at HEAD hold none of tonight's signature** (sorted in section 3).
  - Three are kin: a letter's last dah is plainly whole in the sender's window, 25 to 26 hops above 0.6, and the gate never printed it. `LOW` read `LMW`, `TINY` read `TINK`, `ULTRA` read `ULTRE`.
  - The other seventeen are dit groups splitting or losing a dit (`H` as `I`, `S` or `I E`), an inside gap read as a letter gap (`INCREASE`, `KILOMETERS`, `CALIBRATED`), and a piece's cut start.
- **The strong scan catch's nine:**
  - **Six are a dah with a dip in it:** two dit-length pieces, a gap of **14 to 25 ms** between them, under the sender's inside-gap p10 of 23 to 30 ms (centre 38 to 39). The two with the gap come to **140 to 156 ms** against his 166 ms dah.
  - **Two read whole here** (53.63 s, 69.54 s).
  - **One, at 3.54 s,** comes before the sender has settled; the window has nothing yet.
  - **None is a dah with its front lost.**
- **What would tell them apart:** not length, since a dit, an inside gap and a dit come to a dah's length. Only the window's depth and time below 0.4: a dip spends two to four hops there, a real inside gap six to nine.

**Task 2, the join** (`80fa4ffa`): rule 1 built, measured on both boards, **not shipped**. Rule 2 was not tried, because the table shows no front-lost kind.
- **Rule 1**, in the gate's sender as it takes a mark: two dit-length pieces with a gap under the sender's inside-gap bottom, and the three together a dah, are joined into one dah.
  - With the bottom at his inside-gap p10: **240** and **1776**. The first recording read `FER C DAT`, and four letters printed in silences.
  - With the bottom at two spreads under his centre in log length: **289** and **2107**, again four in silences.
  - Either way the twelve-plus-three falls and a hard limit breaks.
- **The synthetic senders**, 18 WPM at 16 dB, three seeds each (`TheSyntheticBrokenDahs`, kept):
  - every fifth dah with a 40 ms dip to 0.3 of the contrast read **46 of 46** with the rule off and on;
  - every fifth dah losing its first 60 ms read **28 to 29 of 46** either way.
- The engine is as at HEAD, and the rule's diff is kept outside the tree.

**Records:**
- `docs\cw-scoreboard.md`: a 566 row in each table, at HEAD's figures.
- `PHASE_OUTCOME.md`, both copies: `## UNIT 566 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 566.
- `Directory.Build.props`: 1.13.250.
- `CLAUDE.md` §1: a row.
- `DECISIONS.md`: HM-DEC-270.

**Build** `Hamlet.sln -warnaserror`: 0 warnings, 0 errors. **Guards:** 4 of 4. **Decision-log and voice tests:** 7 of 7. **App carry-forward:** 278 of 278.

## 2. What the owner should expect

**The broken dahs are not joined; nothing about reading changed.** Both scoreboards stand: **297** on your twelve plus KM3STU's three (**234** without) and **2111** on W1AW.

**What the join cost:** I built the join the order described and measured it twice.
- **First version:** your recordings fell from 297 to 240, W1AW from 2111 to 1776, and your first recording read `FER C DAT`.
- **Stricter version:** 289 and 2107, still printing letters in silences.

The trouble is that a dit, the gap inside a letter and another dit add up to exactly a dah's length, so a rule that joins by length joins real letters too.

**So tonight's `INFMERMATION`, `FRMEM` and `USINTI` would still read that way.** I couldn't check them directly: tonight's recording isn't in the tree.

**What the recordings in the tree do show:**
- **Different broken dahs on W1AW.** Of its 20 wrong letters, none is your signature. Three are close: a letter's last dah that Hamlet heard perfectly well and never printed. `LOW` came out `LMW`, `TINY` `TINK`, `ULTRA` `ULTRE`.
- **Real dips on the strong scan catch.** Its broken dahs are a dah with a short dip in the middle, under a fifth of a dit, deeper than his own gaps ever are briefly. Length can't tell those from real gaps; how long the signal stays down might.

Rebuild if you like; nothing changes on screen. Pushed to `main`.

**To go further,** add tonight's W1AW capture to the tree (a `w1aw-2026-10-08-*` folder beside the 2026-10-07 pieces) and lift R88 for it, so the fifteen specimens can be drawn and measured.

## 3. What you should see

**The W1AW table's 20 wrong at HEAD, sorted:**

| piece | printed | meant | wrong | this signature? |
|---|---|---|---|---|
| 02 | `AT DOCTOBER` | AND OCTOBER | 1 | no: N lost its dit |
| 02 | `LAY T IE` | THE | 1 | no: H's first dit broken (dit split) |
| 03 | `S WITH` | H | 1 | no: the piece starts inside the H |
| 03 | `ATTAC I` | ATTACH | 1 | no: H split (dit group) |
| 03 | `MICROWAVELENGTI I` | ...GTHS | 2 | no: H and S split (dit groups) |
| 03 | `INTNR TSE` | INCREASE | 4 | no: C's inside gap read as a letter gap, and the letters after |
| 03 | `KILOMETEA S` | KILOMETERS | 1 | no: R's inside gap read as a letter gap |
| 03 | `NETAT` | NEXT | 2 | no: X lost a dit |
| 04 | `I EGHER BANDS ■` | HIGHER BANDS. | 3 | no: dit groups split, the period lost |
| 04 | `LMW` | LOW | 1 | **kin**: O's third dah whole in the window, never printed |
| 04 | `TINK` | TINY | 1 | **kin**: Y's last dah whole, never printed |
| 04 | `ULTRE` | ULTRA | 1 | **kin**: A's dah whole, never printed |
| 04 | `CALIBA ATED` | CALIBRATED | 1 | no: R's inside gap read as a letter gap |
| | | | **20** | **0** the signature, **3** kin |

**The specimens through the sender's own window** (a hop a character; `.` under 0.4, `#` from 0.4, `+` from 0.6; pieces with `.` dit and `-` dah; gaps `i` inside a letter, `L` between letters; his figures over the 30 s before):

| specimen | window | pieces printed, ms | gaps, ms | his inside gaps p10 / centre / p90 | letter gap p10 | dah centre | dit | WPM |
|---|---|---|---|---|---|---|---|---|
| W1AW piece-04 130.84 s, `LMW` for `LOW` | `#..#++++++++++......#+++++++++++++++++++++++++......++++++++++.......+++++++++.................###..#+++++++++++++++++++++++++#......#++++++++++++++++++++++++#.....++++++++++++++++++++++++++#.........#..##....###++++++++++#.....#+++++++++++++++++++++++++......#+++++++++++++++++++++++++........` | 46. 123- 44. 41. 116- 124- 46. 123- 123- | 37i 36i 39i 285L 35i 114L 37i 37i | 37 / 39 / 39 | 114 | 122 | 42 | 28 |
| W1AW piece-04 253.96 s, `TINK` for `TINY` | `##.++++++++++++++++++++++++++...................##+++++++++++......++++++++++..................###.++++++++++++++++++++++++++......#+++++++++.....................#++++++++++++++++++++++++++......#+++++++++......#+++++++++++++++++++++++++......#+` | 125- 50. 43. 125- 43. 126- 43. 124- | 111L 37i 115L 38i 113L 37i 36i | 37 / 38 / 40 | 114 | 123 | 43 | 28 |
| W1AW piece-04 256.20 s, `ULTRE` for `ULTRA` | `#...++++++++++......#+++++++++......++++++++++++++++++++++++++.................#++##++++++++++......++++++++++++++++++++++++++......#+++++++++......#+++++++++..................##..++++++++++++++++++++++++++...................###+++++++++#......+++++++++++++++++++++++++#......+++++++++#...........##......###+++++++++#......+` | 43. 43. 123- 47. 123- 43. 43. 125- 43. 120- 40. 41. | 39i 37i 113L 37i 37i 37i 115L 114L 39i 40i 119L | 37 / 38 / 39 | 113 | 123 | 43 | 28 |
| catch-153810 3.54 s | `` (the sender's window not yet open) | 50. 85. 40. | 15i 85i | 20 / 20 / 20 | 260 | 160 | 20 | 60 |
| catch-153810 31.62 s | `++++##...#++++++++++++++##..++++++++++###.....#+++++++++++++` | 167- 66. 49. 61. | 34i 25i 46i | 30 / 39 / 45 | 134 | 166 | 65 | 18 |
| catch-153810 32.65 s | `.........+++++++++++++..#++++++++++++++++#...#++++++++++++++` | 62. 80. 76. | 14i 27i | 30 / 39 / 45 | 134 | 166 | 65 | 18 |
| catch-153810 40.69 s | `.........#+++++++++++++##.#+++++++++++++##....####..........` | 61. 65. | 25i | 27 / 38 / 44 | 128 | 167 | 65 | 18 |
| catch-153810 49.17 s | `..........#++++++++++++++#..++++++++++++#.....#+++++++++++++` | 67. 55. 69. | 20i 40i | 27 / 39 / 45 | 121 | 167 | 65 | 19 |
| catch-153810 52.14 s | `........#++++++++++++++#..#++++++++++++++#..................` | 68. 67. | 21i | 27 / 39 / 45 | 121 | 167 | 65 | 19 |
| catch-153810 53.63 s | `++++##....#++++++++++++++++++++++++++++#....................` | 73. 141- | 34i | 27 / 38 / 45 | 121 | 167 | 65 | 19 |
| catch-153810 69.54 s | `+++###...#+++++++++++++++##+++++++++++++++##................` | 63. 163- | 42i | 23 / 39 / 45 | 125 | 166 | 66 | 18 |
| catch-153810 72.11 s | `.....#+#.#++++++++++++++#..++++++++++++++#....#+++++++++++++` | 69. 67. 67. | 18i 31i | 23 / 38 / 44 | 125 | 166 | 66 | 18 |

On W1AW the missing dah is the third run of `+` after the letter's printed ones (the 26-hop run before `.........#..##`), whole and never printed. On the catch the dip is the `##..` or `..#` between two runs of `+`.

**The synthetic senders** (18 WPM, 16 dB, 600 Hz; `CQ CQ DE W1AW INFORMATION FROM QST USING VARIOUS MODES K`, 46 letters):

| sender | seed 5661 | seed 5662 | seed 5663 | rule 1 off | rule 1 on |
|---|---|---|---|---|---|
| every fifth dah with a 40 ms dip to 0.3 | 46 | 46 | 46 | 46, 46, 46 | 46, 46, 46 |
| every fifth dah missing its first 60 ms | 29 | 28 | 29 | 29, 28, 29 | 29, 28, 29 |

The front-lost sender reads `CGT CGT DE W AO A AT I N F MT R M A T I O N ...`: each lost front leaves a 126 ms gap read as a letter gap.

**Both scoreboards' rows:**

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| HEAD | 318 of 353 | 20 | 1 | **297** | 0 | 83 of 110, 3 added |
| rule 1, bottom at his p10 (not shipped) | 289 of 353 | 47 | 2 | 240 | 4 | 84 of 110, 4 added |
| rule 1, bottom at two spreads (not shipped) | 314 of 353 | 23 | 2 | 289 | 4 | 83 of 110, 4 added |
| 566 task 2 (engine as HEAD) | 318 of 353 | 20 | 1 | **297** (234 without) | 0 | 83 of 110, 3 added |

| w1aw | score | right | wrong | invented | spaces |
|---|---|---|---|---|---|
| HEAD | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |
| rule 1, bottom at his p10 | 1776 | 1960 of 2151 | 184 | 0 | 437 of 440, 10 added |
| rule 1, bottom at two spreads | 2107 | 2129 of 2151 | 22 | 0 | 438 of 440, 11 added |
| 566 task 2 (engine as HEAD) | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |

## 4. What's blocking us

- **Tonight's W1AW capture was not in the tree,** so tonight's fifteen specimens were not drawn. They need the folder placed and R88 lifted for it.
- **W1AW's whole dahs that are never printed** (`LMW`, `TINK`, `ULTRE`) are a different fault from the dip: the window holds them whole. Where they are lost, at the detector's call or the gate, is not traced.
- **The dip is told from a real gap only by depth and time below 0.4 on the window,** and a depth step cost the board when it was tried before.
- **`TheRsidDetectorTests.TheDetectorKeepsUpWithRealTime`** is red when the engine line runs whole, and the two reply tests (`TAW`, `IAN`) are red at HEAD.

### Asks still outstanding

- **Unit 562, 2026-10-08:** what next for the junk before a weak call, given the 0.4 start costs the start of every real call. Waiting on the owner. No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
