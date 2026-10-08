## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 21:09:50 through `tools\arbiter\lock.bat take` and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. R88 lifted only as the order named, and no recording added. Version 1.13.240 to **1.13.241**. Ruling **HM-DEC-260**, the number the order gave. Nothing was recorded under §12.1. Every test run was one filtered invocation; scripts went in `.run-unit\unit556-*.sh`, not committed. Both scoreboards ran after every task, and neither moved: 237 and 2112.

**Task 1, a reply begins where the other ends** (`1667547f`), **shipped:**
- **The backlog:** when the terminal passes from a sender it printed to another, the new sender's backlog is only what it sent since the printed one last keyed, less one of its word gaps. Older letters are dropped unprinted.
- **Not a reply:** a sender that stood longer than `ReplyOverlapSeconds`, **10 s**, while the printed one kept keying prints no backlog. It is also not picked while the printed one has been silent under 10 s, so a station pausing between sections is picked again when it resumes. The figure is the author's, from what an over is: a reply doubles at most a callsign and a word.
- **The synthetic W1AW** (`AReplyBeginsWhereTheOtherEndsTests`): at HEAD the second sender's past printed in a burst at the pause and W1AW never came back. Now nothing of it prints and W1AW resumes whole (section 3).
- **The QSO replies:** `AReplyIsReadFromItsFirstLetterTests` reads as at HEAD. The owner's 14:40:20 reply prints from its W (`IAN` for `IUN`), and the synthetic QSO's reply reads `TAW` for `W1AW`. Both tests were red before and after, unchanged.

**Task 2, a measured figure that ships stands:** no code. The owner's ruling is HM-DEC-260's first line, and the figures it covers are cleared from the asks below. The four older asks stay.

**Task 3, word spaces** (`93c73549`), traced, nothing shipped. `TheSpacesByCauseTests` finds every missing and added space on the twelve (section 3).
- **The largest group,** 6 of 20, are word gaps as short as the sender's own letter gaps. No gap can tell them from a letter gap, so there is nothing to fix from the sender's gaps.

**Task 4, the broken dahs** (`1d82647e`), traced, nothing shipped:
- **The down line splits them:** 7 of 9 dahs stand in two pieces on this tree. Six dip past the 0.6 down line, at 0.60 to 1.14 of the contrast; the two whole ones dip 0.52 and 0.45.
- **The sender's own gaps** run p1 0.78 and p5 0.91, so half the dips are as deep as its gaps.
- **The down step from its own gaps was built and measured:** 196 on the twelve, the first recording broken, and 783 on w1aw. The step climbs, because a gap shallower than it never ends a mark and so is never counted. It was taken out.
- **Kept:** a key-up trace for tests (`LaneKeyUpTrace`) and `TheDownCrossingAtEachDah`.

**Task 5, the weak fast station** (`21a609d2`), traced, nothing shipped. `TheWeakFastStationTracedTests` follows `143906` through the chain (section 3).
- **What it found:** the station stands on noise blips before it keys, prints them as backlog, and splits QS by lines drawn from them. Once its window opens it reads `DE WB2FU`.
- **Why no fix:** the blips can only be told from its dits with a figure taken from this recording.

**Records:**
- `docs\cw-scoreboard.md`: a row per task in both tables.
- `PHASE_OUTCOME.md`, both copies: `## UNIT 556 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 556.
- `Directory.Build.props`: 1.13.241.
- `CLAUDE.md` §1: a row.
- `DECISIONS.md`: HM-DEC-260.

**Build** `Hamlet.sln -warnaserror`: 0 warnings, 0 errors.

**Both guards:** 4 of 4. **The decision-log and voice tests:** 7 of 7. **App carry-forward:** 278 of 278.

## 2. What the owner should expect

Rebuild and run as usual.

- **The junk at W1AW's section breaks is gone.** When W1AW paused a second or two between sections, Hamlet let it go and handed the screen to whatever else it had been tracking beside it. That other sender then dumped every half-letter it had collected over the last few minutes. Now a sender that has sat beside a station still sending for more than ten seconds isn't treated as someone replying. It doesn't get the screen while the station might come back, and when it does get it, it brings no backlog. W1AW picks up again after its pause.
- **A reply in a QSO still prints from its first letter.** When the other station starts answering just as the first one stops, its opening still comes through, from what it sent since the first station's last word. Your 14:40:20 recording reads exactly as before.
- **The asks list is shorter.** As you ruled, the figures I set from measurement that shipped on the scoreboard stand as mine, and they've come off the list. Four older questions remain.
- **What didn't move:**
  - **Spaces:** most missing word spaces are places where the sender's gap between words is no longer than his gap between letters, so nothing in the timing can find them.
  - **The broken dashes:** half of them dip as deep as real gaps. The fix that joined them broke far more than it saved, so it isn't in.
  - **The weak fast station:** it's lost before it starts. Hamlet builds it from noise blips in the first seconds, and it only reads properly once its own window opens.
- **Neither scoreboard moved:** 237 and 2112.
- Pushed to `main`.

## 3. What you should see

**Both tables' rows:**

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| HEAD | 254 of 288 | 17 | 0 | **237** | 0 | 67 of 87, 3 added |
| 556 tasks 1 to 5 | 254 of 288 | 17 | 0 | **237** | 0 | 67 of 87, 3 added |

| w1aw | score | right | wrong | invented | spaces |
|---|---|---|---|---|---|
| HEAD | **2112** | 2131 of 2151 | 19 | 0 | 438 of 440, 11 added |
| 556 tasks 1 to 5 | **2112** | 2131 of 2151 | 19 | 0 | 438 of 440, 11 added |

Every stretch read identically after each task. The random carrier prints at seed 5195 only, as at HEAD.

**The synthetic W1AW pause.** W1AW at 550 Hz and 25 WPM, a 2 s pause at 50.6 s, and a second sender at 750 Hz, 18 WPM and 10 dB weaker, beside it the whole time:

| | the second sender's letters | of them keyed before the pause | W1AW after the pause |
|---|---|---|---|
| HEAD | 121 | 56, first at 3.7 s: `NGE E KET EEIG EEIETTIG KQ T KET TI S ZT …` | nothing |
| now | 0 | 0 | `QST DE W1AW QST DE W1AW THE SPEEDS … PAGE FIFTY ONE`, whole |

**The spaces by cause**, the twelve's 20 missing and 3 added:

| cause | count | where |
|---|---|---|
| missing: a word gap as short as the sender's letter gaps | 6 | `221530` 598 Hz (5: EN, WHEN, HENI, GE12, SING; gaps 160 to 226 ms against letter clusters 171 to 222), `221745` (.EUR, 137 ms) |
| missing: a letter either side not printed | 5 | `144020` TEMP, `221530` 492 Hz PION, `221828` BEST 73 (3) |
| missing: a word gap under the line, the word cluster shown | 5 | `221530` SAGE 252 under 254; `221548` EEMY 283 under 319; `221805` XFER 372 and THER 424 under 508; `221828` EDES 285 under 300 |
| missing: a word gap under the line, no word cluster yet | 4 | `221745` 305, 380 and 405 under 463; `221805` DAYS 356 under 399 |
| added: a letter gap over the line | 3 | `221530` 6CHA 541 over 277; `221548` SCOU 453 over 314; `221828` EUHA 254 over 243 |

**The broken dahs** on `catch-153810-7033367`, through the detector's own window. In the window, each 5 ms hop is `#` above 0.4 of the contrast, `+` above 0.6, `.` past 0.6:

| dah | deepest dip | the 0.6 line | stood | window |
|---|---|---|---|---|
| 3.54 s | window not yet open | | 2 pieces | |
| 31.62 s | 1.14 | passes | 2 | `.+##############++..##########+++` |
| 32.65 s | 0.85 | passes | 2 | `..#############..+################+` |
| 40.69 s | 0.60 | passes | 2 | `.+#############++.+#############++` |
| 49.17 s | 1.03 | passes | 2 | `.+##############+..############+` |
| 52.14 s | 0.76 | passes | 2 | `.+##############+..+##############+` |
| 53.63 s | 0.52 | short | 1 | `..+############################+` |
| 69.54 s | 0.45 | short | 1 | `.+###############++###############++` |
| 72.11 s | 1.04 | passes | 2 | `+.+##############+..##############+` |

- **The sender's own gaps** on the same window: 379 of them, depth p1 0.78, p5 0.91, median 1.00.
- **The down step from them**, measured and taken out:

| | the twelve | w1aw | dahs in pieces | strong catch | second catch |
|---|---|---|---|---|---|
| without | 237 | 2112 | 7 of 9 | 137 of 152 | 22 of 35 |
| with the step | 196, the first recording `… BEST ZV 73 I TC4ZNPDEAA` | 783 | 1 of 9 | 33 of 152 | 18 of 35 |

**The weak fast station**, `143906`:
- **The plain read:** at 514 Hz, dit 47 ms, dah 154 ms: `T 7ERNEW62FUVEE E E E E E QSY ■<KN>GEWB2FUVEE E E EE`. At 500 and 525 Hz it reads worse.
- **The gate:** a sender at 513 to 515 Hz qualifies at 7 s on 19 marks (shape 0.42 to 0.47). It is silent from 7 to 15 s and so not a candidate. It is printed from 19 s (shape 0.66 to 0.78), released at 24 s at shape 0.
- **Its first 19 marks** are mostly 20 to 35 ms, under the plain dit of 47. They print as backlog: `I E E I I E N I E E`.
- **At 15 to 18 s** its lines, drawn partly from those blips, split QS: `T` 95, `T` 120, `A` 35 135, `E` 35, `E` 45.
- **The window** opens at 18.26 s, at 12.5 dB median contrast (10.7 to 13.8), so the level path runs. From there it reads `Y D E W B 2 F U`, `DE WB2FU`.

## 4. What's blocking us

- **The QSO reply tests stay red as before:** the owner's reply reads `IAN` for `IUN`, and the synthetic reply's first letter `TAW` for `W1AW`.
- **The broken dahs:** three of six dip as deep as the sender's gaps. Depth alone won't join them, and a duration rule measured worse in an earlier unit.
- **The weak fast station:** it stands on noise blips before it keys, and nothing found tells them from its dits without a figure from this recording.
- **Word spaces:** the largest group is invisible in the timing.

### Asks still outstanding

The ruling recorded as HM-DEC-260's first line clears every measured figure that shipped. The four older asks stay:

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
