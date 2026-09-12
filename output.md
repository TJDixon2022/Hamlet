```
READ IN THIS ORDER.
```

A. The phase goal - the screen says what is true and looks like someone meant it.
   Steps 0 and 1 done; step 2 partial after this unit, two of four must-pass met; step 3, Tim
   looking, not started.
B. Step 2 and its four must-pass - States scored from STATE and the badge count matching
   the log; rank_names naming the ranks with defaults and the file's keys documented;
   the 1400 split by R24 with no callsign cut and the table placed by the rule, beside
   at 1920; the split stated at both widths.
   1. States from `STATE`, and the badge count matching the log: met. On the five-record
      fixture, 2 of 5 records score (PA and AK), for 12 points from the shipped file, and the
      badge reads `2 worked`. It was 0 before.
   2. `rank_names` names the ranks, the defaults show when it is absent, and every key is
      documented: met. The comment block names 23 of 23 keys.
   3. The 1400 split by R24, with no callsign cut and the table beside the map at 1920: not
      met. Task 3 was dropped whole, and the split is still `383,*`. Measured: no single
      fraction can meet this at a 338 px table (section 4, item 1).
   4. The split stated at both widths: not met as a statement of a built split. The split as it
      stands is measured and stated in section 3.
C. The report last. Section 4 raises 3 items on top of the carried queue. Item 1 stands in the
   way of must-pass 3 and 4; items 2 and 3 do not stand in the way of any criterion in B.

```
UNIT:       336 - stopped at task 2 of 3 - 2026-09-12 18:01
PHASE GOAL: Maintenance, screen only - make what Hamlet shows true and deliberate-looking,
            finally judged by Tim at his own window size.
UNIT GOAL:  Score States from the log's own STATE field, name the ranks from the points file,
            make that file explain itself, and settle how the decoded list and For You share
            the tab at 1400.
ADVANCED:   yes - step 2 must-pass 1 (States from STATE) and 2 (rank_names and the documented file) met by tests watched red first
NUMBER:     step 2 must-pass 0 -> 2 of 4; decoded split at 1400 383 px -> unchanged, 383 px (task 3 dropped)
DRIFT:      0
```

## 1. What Claude did

**Stopped at task 2 of 3.** Tasks 0, 1 and 2 are done, and each is pushed. **Task 3, the 1400
layout, was not built.** It is the drop candidate the instruction named, and it was dropped
whole. It was not dropped for time: the unit was about 25 minutes in. It was dropped because
task 0's measurement shows its must-pass cannot be met by a fraction alone (see *Decisions,
task 3* below, and section 4 item 1). **The next unit inherits task 3** and the measurements
below.

Claude Code on Tim's Windows 11 machine, project Hamlet. The gate passed on all four checks.
Branch `main`. Every push succeeded: task 0 as `2b18f88`, task 1 as `38bc3a8`, task 2 as
`38a1f0c`. This report, the closing status and the outcome entry go in one more commit.

**Every appearance claim in this report is computed on the Avalonia headless host, not seen.**
That host draws text at a flat ten pixels a character, wider than the glass.

### Task 0 - the trace, before a line is built

- **Carry-forward list, before any edit**, as the two invocations its comment orders: app
  **100 of 100** in 12 s, engine **85 of 85** in 4 s.
- **Entry criterion met.** `TheCategoryPagesAreTradingCardsTests` and
  `TheAchievementsPageClicksInTests` together: **11 of 11**.
- `PHASE_OUTCOME.md`: `UNIT 336 - STEP 2` appended. Version 1.13.20 -> **1.13.21**.
- **The before-number is 0 of 4, confirmed.**

#### The record: `STATE`

- **No path in `src` writes `STATE`.**
  - `AdifLog.Record` writes the fourteen fields the instruction lists, and no `STATE`.
  - `ContactLogStore.Append` is the only caller of `AdifLog.Record`.
  - No import path exists in `src`, and no state lookup exists either.
  - So a record carries `STATE` only if another logger wrote it into the file.
- **Hamlet never rewrites a record it did not create.** `ContactLogStore.Append` uses
  `File.AppendAllText`, and nothing re-serializes the log. So an imported record's `STATE` is
  never dropped.
- **The reader read no `STATE`** (`AdifLog.From`).
- **`AchievementScores` returned `States => 0`** in both places named.
- **`AchievementContact` carries `Entity`**, and the DXCC table tells the three entities apart:
  - `United States of America`: prefixes `K`, `W`, `N`, `AA`;
  - `Alaska`: `KL`, `AL`, `NL`, `WL`;
  - `Hawaii`: `KH6`, `KH7`.
  - Their continents are NA, NA and OC.
- **Before-number:** States counted on a fixture log that carries `STATE` is **0**. The badge
  read `0 worked`.

#### The ranks

- **A rank is spoken in one place: `AchievementBadgePage.TotalLine`.** It is bound to
  `AchievementsTotalLine` in `AchievementsWindow.axaml`, and it says both the rank and the gap.
  - On the twelve-contact fixture it read `Total 465 pts · Rank 4 · 35 to Rank 5`.
  - The shipped file has 8 thresholds and `RankOf` is `1 + count`, so ranks run from Rank 1 to
    Rank 9, as the instruction says.
- **No other rank is spoken.** `ContactBelt`'s ranks are belt colors on the contact count
  (`white` to `gold`), not the points rank. `CwDecodeReport.Rank` is a decoder's.

#### The points file

- **Readers in `src`:** one, `AchievementPoints.Parse`, reached through `Read` and `ReadOrSeed`.
  `MainWindowViewModel.OpenAchievements` calls `ReadOrSeed` each time the window opens.
- **Readers in `tests`:**
  - `Parse(Shipped())` in `ThePsk31RecordsAppearTests`, both `TheTotalMilesTests`,
    `TheAchievementsPageClicksInTests`, `TheAchievementsPageTests`,
    `TheCategoryPagesAreTradingCardsTests` and `Unit332TwoWidthsTests`;
  - `Read` and `ReadOrSeed` in `TheAchievementsPageTests`;
  - `TheShippedFileIsTheOneInTheTreeAndSaysItIsHisToEdit`, which compares the file on disk with
    the embedded copy as text.
- **Every one went through `JsonDocument.Parse` with its default options**, so every one would
  have come back absent on a comment. None parses the file by any other route, and nothing in
  `tools` reads it.
- **`assets\data\achievement-points.json`: nothing in `src`, `tests` or `tools` reads it.**
  - Before this unit it differed from the shipped file only in its last newline.
  - It now also lacks the comment block.
  - Left as it is, as instructed.

#### Telemetry

- **The points file:** `achievement_points_loaded` (`kinds`, `fileHash`) reports it being read.
- **The log:** no event reports the log being read. `achievements_opened` (`kinds`) is written
  once the log has been read and scored, so that is where the count of states went.

#### The layout, measured at a window 1400 and 1920 wide

`DigitalDecodedPanes` is `383,*`. The widths below are in pixels.

| | 1400 | 1920 |
|---|---|---|
| The tab (`DigitalPanes`) | 1342 | 1862 |
| The right column (`DigitalDecodedPanes`) | 671 | 931 |
| The decoded panel | 378 | 378 |
| For You | 283 | 543 |
| The card, inside | 227 | 487 |
| The card's map | 215 x 120 | 220 x 120 |
| The card's table | 227 x 102 | 338 x 102 |
| Table beside or under the map | **under** | **under** |

- **The table is under the map at 1920 as well.** It wants 338 px.
  - The label column is `Last heard`, at 100 px.
  - The widest value is `4,500 miles · northeast`, at 230 px.
  - Beside the map, the pair needs 220 + 10 + 338 = **568 px** inside the card.
- **The widest message cell each width needs is the same: 200 px.**
  - `VP2MAA/P KC3QIS R-09` needs 200 and gets 200.
  - `CQ DX K9XP JN88` needs 150.
  - The senders alone: `KC3QIS` 60, `K9XP` 40.
- **What a PSK31 row can abbreviate to, without new parsing.** A PSK31 row has no fields
  (`HasFields` is false). It already draws:
  - the speaker the exchange parser read (`Sender`, when `ShowsReadSender`);
  - its reading word (`guess` or `unknown`);
  - the text.

  **So at 1400 it could show the read speaker and that word, and never a grid.** Where the parser
  named no speaker there is no callsign to show at all, only text.

### Task 1 - States counts what the log says

**Test first, watched red.** `TheCategoryPagesAreTradingCardsTests.StatesCountWhatTheLogsStateFieldSays`
is new. It reads one fixture log written as ADI text, so the reader is what is tested:
- `K3PA`, United States, with `STATE=PA`;
- `W1AW`, United States, with no `STATE`;
- `KL7XYZ`, Alaska, with `STATE=AK`;
- `VE3PQR`, Canada, with `STATE=ON`;
- `N3DC`, United States, with `STATE=DC`.

Red: `Expected: 2`, `Actual: 0`.

**Change.**
- `AdifContact.State` is read from ADIF `STATE`. `AdifLog.Record` still writes none.
- `AchievementContact` carries `State`.
- `AchievementLog.StateOf` scores a contact only when two things hold: its entity is the United
  States, Alaska or Hawaii, and its `STATE` is one of the fifty codes. `AchievementLog.States`
  and `InState` are built on it.
- `AchievementScores` counts States from `log.States` and scores them with `per`, `special` and
  `all`. A special replaces `per`, as it does for a band, and `all` is for the fifty.
- **The States page draws each state as the contact that earned it**, through the builder unit
  335 built for Countries (`EarnedBy`). Each card is worth its `special` or its `per`.
  **The next card's want line and its sentence, `Hamlet cannot tell a caller's state`, are
  unchanged.**
- **Telemetry:** `achievements_opened` gains `states`, the count scored. It is a count and never
  a code.

**On the fixture, computed:**
- **Scored:** PA and AK. **Not scored:** the blank, ON and DC, which is 3 of 5.
- **Points:** `2 + 10 = 12`, read from the shipped file.
- **The badge:** `2 worked`.
- **The page draws two earned cards:**
  - `AK`: `KL7XYZ · BP51`, `10 pts`, with a map;
  - `PA`: `K3PA · FN10`, `2 pts`, with a map.
- **The next card is `One more state`**, still `Any state you have not worked` and
  `Hamlet cannot tell a caller's state`.
- **At windows 1400 and 1920 the States page draws 3 trading cards.** 21 runs fit, none wraps,
  and 0 cards are white.

**Green.**
- The new test.
- `OpeningTheWindowWritesHowManyStatesScoredAndNoCode`, which is new. It writes `states` = 2,
  and no code, callsign or entity.
- Engine log and scoring tests: **52 of 52**, after the property pin below.
- The achievements, binding and privacy set: **40 of 42**. Both reds are older than this unit;
  see *Reds older than this unit* below.
- **Unit 335's fit and no-white-card test at 1400 and 1920 passed**, as part of that set.

#### Decisions this session made for itself, task 1

1. **A States card's title is the two-letter code**, the way a Grids card's title is the square.
   The tree has no table of state names. Rejected: adding one, which is new data this unit was
   not asked for.
2. **The count of states scored went on `achievements_opened`.** It is the event written after
   the log is read and scored. Rejected: `achievement_points_loaded`, which is written before the
   log is read.
3. **The States badge's meaning line went from `the 50, plus DC` to `the 50 states`**, and
   `AchievementKinds.States`'s summary from *The fifty, plus DC* to *The fifty, from the log's own
   `STATE` field*. Decision 1 makes DC score nothing, so both lines would have been false.
4. **`AdifLog.Record` still writes no `STATE`** (decision 2, parked).
   - Consequence: an `AdifContact` with `State` set loses it on a write.
   - Why that is safe today: nothing Hamlet creates sets it, and the log only appends.
   - `AdifContact.State`'s doc comment says so.
5. **`TheLogCanSayFt4Tests.AdifContactCarriesOneSubmodeProperty` pins the property count.** It
   went from 15 to 16 under R12, with the reason written beside it. Its one-submode assertion is
   untouched.
6. **The next card's title follows the count** (`Your first state`, then `One more state`), as
   every other kind's does. Its want line and sentence are unchanged.

### Task 2 - rank names from the file, and a file that explains itself

**Tests first, both watched red:**
- `TheAchievementsPageTests.TheRanksCarryTheNamesInThePointsFile` is new. Red: `Expected "Total
  465 pts · Ranger · 35 to Voyager"`, `Actual "Total 465 pts · Rank 4 · 35 to Rank 5"`.
- `TheAchievementsPageTests.TheShippedPointsFileDocumentsEveryKeyInACommentBlockAtItsTop` is new.
  Red: `the shipped points file has no comment block at its top`.

**Change.**
- **`AchievementPoints` skips comments, and relaxes nothing else.** A trailing comma is still a
  file that could not be read.
- **`rank_names` is read by position** into `RankNames`.
  - `RankName(n)` gives the name, or `Rank n` past the end of the list or for a blank entry.
  - A `rank_names` that is not a list of strings is skipped whole. That is the way the reader
    already skips a section of the wrong shape (a kind that is not an object is passed over), so
    no name lands on the wrong rank.
- **`TotalLine` says `Scores.RankName`, and the gap names `Scores.NextRankName`.**
- **The shipped `data\achievements\achievement-points.json` gains a `//` block at its top.**
  - It names every top-level key and every key inside a section, in quotes, with what each does.
  - `rank_names` is documented there and **not added as a key**.
  - `_about` stays.
- **Telemetry:** `achievement_points_loaded` gains `rankNames`, the count read. It is a count and
  never a name.

**On the twelve-contact fixture, computed:**

| `rank_names` | The total line |
|---|---|
| absent | `Total 465 pts · Rank 4 · 35 to Rank 5` |
| eight names | `Total 465 pts · Ranger · 35 to Voyager` |
| four names | `Total 465 pts · Ranger · 35 to Rank 5` |
| `["Listener", 2, "Operator", "Ranger"]` | `Total 465 pts · Rank 4 · 35 to Rank 5` |
| `"Ranger"` (not a list) | `Total 465 pts · Rank 4 · 35 to Rank 5` |

**What else the tests confirmed:**
- **The block:** **23 of 23** keys are named in it. The shipped file parses with 8 kinds, and
  `rank_names` is in the block and not in the file.
- **Telemetry:** `ReadingThePointsFileWritesHowManyRankNamesItReadAndNoName` is new. It writes
  `rankNames=3` for three names and `rankNames=0` for the shipped file, and no name.

**Green.**
- The achievements, binding, privacy and PSK31-records set: **46 of 49**. The three reds are older
  than this unit, below.
- Engine tests over the shipped file: **41 of 41**.

**Carry-forward list, after all changes**, as its two invocations: app **100 of 100** in 12 s,
engine **85 of 85** in 4 s.

#### Decisions this session made for itself, task 2

1. **A blank entry in `rank_names` names nothing**, so that rank keeps `Rank n`. The count of
   rank names read counts only non-blank entries.
2. **Only comments are relaxed.** `AMalformedPointsFileIsReportedAndLeftAlone` still passes: its
   broken edit, with a trailing comma, is still unreadable. Rejected: allowing trailing commas
   too, which would read an edit you had not finished.
3. **The block says the file is read when the achievements window opens**, because that is what
   the code does. `_about` still says *at startup* and was left as it is (decision 3 keeps it).

### Task 3 - not built

#### Decisions this session made for itself, task 3

1. **Task 3 was dropped whole.** Nothing in the layout was moved.
   - **Why.** Task 0 shows that no single star fraction for `DigitalDecodedPanes` meets
     must-pass 3 on the headless host:
     - **Beside at 1920** needs For You at 568 + 56 = 624 px or more, so the decoded column can
       be at most 931 - 5 - 624 = **302 px**. That is a fraction of **0.324** of the right
       column, and it leaves a message column of 119 px, too narrow for the 200 px FT8 line even
       at 1920.
     - **At 1400 the same fraction** gives a decoded column of 217 px and a message column of
       **34 px**. A six-character callsign needs 60 px, so a callsign is cut.
   - Holding both halves needs something R24 does not say: a minimum width on a star column,
     abbreviating at 1920 too, or shortening the conversation card's table.
   - That is a ruling. A half-moved layout is the failure the instruction names.
   - **Rejected:** building a fraction anyway and reporting the miss (§6 allows that for a
     *little* miss, and this is not little); and picking one of the bends above for Tim.
2. **The printing probe `ThePanelsMakeRoomTests.Unit336TraceMeasuresTheSplitAtBothWidths` was
   kept.** It asserts nothing and prints every width above, so the next unit starts from a
   measurement rather than this report. Rejected: removing it, which would make task 3's author
   rebuild it.

### Where the instruction and the tree disagreed

Reported, not repaired.

- **R23 says the log *already writes* `STATE` for a US contact.** Nothing in `src` writes it.
- **The seeding rule is cited as `AppSettings.cs:747-766`.** Those lines hold only the path and
  its doc comment. The rule that seeds once and never overwrites is in
  `AchievementPoints.ReadOrSeed`, at `AchievementPoints.cs:166-198` before this unit.
- **The writer's fields are cited as `AdifLog.cs:291-366`.** Lines 291-294 write the header
  (`ADIF_VER`, `PROGRAMID`, `PROGRAMVERSION`), and the record's fields are at 315-366.
- **R12 says `TheDigitalTabIsTwoColumnsTests` guards `383`.** It does not assert 383. It asserts
  the decoded panel is narrower than the right half, and 383 appears only in a comment.
  `ThePanelsMakeRoomTests` is the one that guards it. `WhatTheSplitCostsTests` names
  `DigitalDecodedPanes` too, without asserting 383.
- **`MainWindow.axaml`'s comment above `CardBeside` says the card has 487 px inside at 1920,
  *where they sit side by side*.** Measured: 487 inside, and the table **under** the map, because
  the table wants 338 px, not the *about 190* the comment assumes.
- **The shipped file's `_about` says *Hamlet reads this file at startup*.** It is read each time
  the achievements window opens.
- **The badge's own line said `the 50, plus DC`**, and decision 1 scores no DC. Changed in task
  1, decision 3.
- **Everything else in section 4 matched:**
  - `States => 0` at both lines;
  - `"Rank " + Scores.Rank` at `AchievementBadges.cs:223`;
  - 8 thresholds;
  - `JsonDocument.Parse(json)` at `AchievementPoints.cs:234`;
  - `_about` as the only prose;
  - the embedded resource at `Hamlet.RadioEngine.csproj:91`;
  - the `assets` copy differing by its last newline;
  - `383,*` at `MainWindow.axaml:4139-4141` with its §0.0 reason;
  - the `*,*` ruling's comment at `MainWindow.axaml:3542`.

### Reds older than this unit

Neither of the first two is on the known-reds list. Both were found in this unit's runs and left
as they are.

- **`TheTotalMilesTests.TheBadgeSaysZeroMilesOnAnEmptyLogAndTheLowestTierIsNext`** (app).
  - It expects the Total Miles badge line `grid to grid, added up`, and the badge says
    `every mile, added`.
  - Unit 332's `3ea16ec` changed the line in `src` and not in this test.
- **`ThePsk31RecordsAppearTests.WithNoPsk31ContactNoPsk31CardIsOnTheScreenAndNoneIsDimmed`.**
  - It fails with `modes draws [PSK31] before any PSK31 contact`.
  - Unit 335's `bcaf39d` made the Modes next card name where each unworked mode lives, including
    PSK31. See section 4, item 2.
- **`TheAchievementsScreenTests.TheWindowDrawsEverySixRows`**, the known one.

## 2. What the owner should expect

**States now counts what your log says.** Version 1.13.21.
- **What counts.** A contact counts toward States only when both of these hold:
  - its record carries a `STATE` field;
  - its callsign resolves to the United States, Alaska or Hawaii.
- **What does not count:** DC, a Canadian province, a blank and a misspelled code.
- **What the page shows.** Each state you hold is a card showing the contact that earned it, with
  its map. The next card is as it was.

**Your ranks can be named.**
- **What to add.** Put a `rank_names` list in your own points file, at
  `%AppData%\Hamlet\achievements\achievement-points.json`. For example: `"rank_names":
  ["Listener", "Novice", "Operator"],`.
- **What shows.** The line under the achievements title uses those names for your rank and the
  next one. Past the end of your list it says `Rank n`.
- **Where the notes are.** Your own copy has no notes at the top, and Hamlet will never add them:
  it does not overwrite your file. The documented copy is `data\achievements\achievement-points.json`
  in the repository. You can paste its `//` block into your own file, because Hamlet now skips
  lines that start with `//`.

**Nothing on the digital tab moved.** Task 3 was not built, so the decoded list and For You split
exactly as before.

**What will look wrong but is not:**
- **Your own FT8 and PSK31 contacts score no state.** Hamlet does not know where a caller lives,
  so the contacts it logs carry no `STATE`. Only records another logger wrote with `STATE` count.
- **A state card's title is its two-letter code**, `PA` and not `Pennsylvania`. The tree has no
  table of state names.
- **The States badge's small line now reads `the 50 states`**, not `the 50, plus DC`. DC does not
  count.
- **A contact in DC, or a `STATE` on a Canadian record, adds nothing.** That is the rule, not a
  missed read.
- **A very long rank name has not been measured on the window.** The defaults fit. A name much
  longer than `Rank 4` may not fit the line under the title, and nothing checks that for names you
  choose.
- **The shipped file's first line of prose still says it is read *at startup*.** It is read each
  time you open the achievements window, so an edit shows the next time you open it.

## 3. What you should see

**The fixture log scores 2 states, PA and AK, for 12 points, and your own contacts score none.**
Hamlet writes no `STATE` on the contacts it logs, so only records another logger wrote with
`STATE` count.

**Your ranks are called `Rank 1` to `Rank 9`**, because your points file names none. The file
that explains every key is `data\achievements\achievement-points.json` in the repository. Your
copy in `%AppData%\Hamlet\achievements\` has no notes.

**The split is unchanged at both widths, `383,*`:**
- **At 1400:** decoded panel 378 px, For You 283 px, card table 227 px, under the map.
- **At 1920:** decoded panel 378 px, For You 543 px, card table 338 px, **also under the map**.

All of this is computed on the test host at windows 1400 and 1920 wide, not seen.

**At 1400 on the digital tab you will see what you saw yesterday.** The decoded list keeps its
width, and the conversation card's table sits under its map.

**In the achievements window:**
- The line under the title reads as before, `Total N pts · Rank n · N to Rank n+1`, until you add
  `rank_names`.
- The States badge's corner says how many states your log scores.
- Opening States shows one card per scored state before the `One more state` card: the code large,
  the callsign and grid, the map of the path, the distance, band, mode and date, and its points.

## 4. What's blocking us

### Raised by this unit

**1. Task 3 cannot meet its must-pass with a fraction alone, measured. Which bend do you want?**

*Ruling wanted: how the decoded list and For You share the tab, given the numbers.* On the test
host, with the conversation card's table as it is:
- **For the table to sit beside the map at 1920**, For You needs 624 px or more. That leaves the
  decoded column at most 302 px of the 931, which is a fraction of 0.324.
- **At 1400 that same fraction** leaves the message 34 px, and a six-character callsign needs 60.
  So one fraction cuts a callsign at 1400 or puts the table under the map at 1920.

The options, each measured against those numbers:
- **(a) A star split with a minimum width on the decoded column.** At 1400 the table goes under
  the map and the message shows callsign and grid. At 1920 the table is beside the map, and the
  message is abbreviated there too, because 119 px does not hold a full 200 px FT8 line.
- **(b) Keep the table under the map at 1920 as well.** This answers R24's *beside at 1920* with
  no.
- **(c) Shorten the table's two widest rows.** They are `Last heard` and `4,500 miles ·
  northeast`, and they set its 338 px. That is the conversation card, not this step.

*Reasoning.* R24 says the split is a fraction and the table goes under only when abbreviation is
not enough. Measured, abbreviation is not enough at 1400 at any fraction that also gives the table
its room at 1920. The recommendation is (a). It is the smallest bend, and it keeps R24's order: For
You gets what the table needs first, and the decoded list abbreviates rather than cutting.

*What was rejected and why.*
- Building a fraction and reporting the miss: §6 allows that only for a little miss, and this is
  not one.
- Taking width from the waterfall: `DigitalPanes` `*,*` is your ruling, and it stays yours (unit
  331 queue item 2).
- A half-built layout, which the instruction names as a failure.

**2. The Modes next card names PSK31 on a log with no PSK31 contact, and a test says §3.1 forbids
that.**

*Ruling wanted: which rule holds on that card.*
- `ThePsk31RecordsAppearTests.WithNoPsk31ContactNoPsk31CardIsOnTheScreenAndNoneIsDimmed` is red,
  with `modes draws [PSK31] before any PSK31 contact`. It is unit 333's test of *absent, not
  dimmed*.
- The line it catches is unit 335's `bcaf39d`, which does what R22 asks: the Modes next card says
  *where the unearned mode lives and who is there*, as `PSK31 3.580 on 80 m`.

*Reasoning.* R22, 2026-09-12, is later than §3.1, and §6 says the later ruling wins. But the test
was not rewritten when the card changed, so the tree now asserts both. This is the same collision
unit 333 raised for Hall of Fame's `A PSK31 contact`, on a second card. Nothing in this unit
touched Modes.

*What was rejected and why.* Rewriting the test or the card here: §12.6, and neither is this
step's.

**3. `TheTotalMilesTests.TheBadgeSaysZeroMilesOnAnEmptyLogAndTheLowestTierIsNext` has been red
since unit 332.**

*No ruling wanted; a finding.* It expects the Total Miles badge line `grid to grid, added up`, and
unit 332 (`3ea16ec`) changed that line to `every mile, added` in `src` only. It is not on the
known-reds list, and the unit that next touches Total Miles owns it under R12.

### Where the carried items stand after unit 336

- **Unit 335 item 1, the States next card's wording:** unchanged. The card still says `Any state
  you have not worked` and `Hamlet cannot tell a caller's state`. Scoring `STATE` changed only the
  earned cards in front of it.
- **Unit 331 queue item 3, States scoring nought: ANSWERED by unit 336.**
  - **What counts now.** `STATE` is read, and scores on a United States, Alaska or Hawaii record as
    one of the fifty codes.
  - **The fixture's numbers.** Of five records, 2 score, for 12 points, and the badge reads `2
    worked`.
  - **The item's worry, confirmed.** Hamlet's own entries carry no `STATE`, so they score none.
  - The item stays in the queue as carried.
- **Unit 331 queue item 2, the 1400 width arithmetic:** stands, and the outer `*,*` option stays
  rejected. This unit's measurement updates its numbers:
  - the card has 227 px inside at 1400 and 487 at 1920;
  - the table now wants 338 px, so it sits under the map at both widths.
  - See item 1 above.
- **Unit 333 item 1, the Hall of Fame next-card collision:** untouched, and now joined by item 2
  above on Modes.
- **The status helper:** not tried. Every `UPDATED` in this unit is a `date` reading pasted whole,
  and none was composed.
- **Every other item stands as carried.** Nothing in this unit touched the radio side, a decoder,
  the FT8 or PSK31 message split, or the transmit chain.

### Carried from unit 335's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

#### Raised by this unit

**1. The States next card says `Hamlet cannot tell a caller's state`. That wording is the
arbiter's proposal, marked for you, and shortened here to fit.**

*Ruling wanted: keep it or reword it.*
- A CQ carries no state, so the card cannot know who is calling from a state you have not
  worked.
- The proposal read *Hamlet cannot tell a caller's state from the air*. That needs 480 px, and
  the slot at the window's 1040 is 426, so *from the air* came off.

*Reasoning.* *No one is calling from there now* would assert something nobody measured (§0.0).
Guessing a state from a prefix was rejected before: a `W3` can be anywhere.

*What was rejected and why.*
- A wider slot. That would change every next card's width for one sentence.
- A second line. The fit rule says no string wraps.

**2. The next cards name callers from continents you have never worked.**

*Ruling wanted, only if the 2026-09-10 rule was meant to hold here.*
- That rule says the CQ list must not be what tells you an area exists. So a decoded-list row
  from a never-opened continent wears the ringed door and names nothing.
- On a Countries or continent next card, the same caller is named by country, beside the same
  ringed door.

*Reasoning.* R22, 2026-09-12, asks the next card to list who is calling from a place that would
earn it. It also asks each unearned continent to name *who is calling from it now*, which cannot
be done without naming the place. §6 says the later ruling wins.

*What was rejected and why.* Leaving door callers off the Countries card. That would hide the
one caller who earns two cards at once, and Continents could not do what R22 asks of it.

**3. The opening page's Hall of Fame badge still has white text on gold, at about 3.6:1.**

*No ruling wanted; a finding.*
- This unit's category band computes its ink and turns dark on that gold.
- The eight badges on the opening page, and their shared template, are unit 331's and step 0's.
  They were not touched, so the badge's name there still reads white on `#A8811A`, under §0.6's
  4.5:1.

*Reasoning.* §12.6: do not repair unrelated things on the way past. The fix is one binding, and
it belongs to a unit that is told to change the page.

#### Where the carried items stand after unit 335

- **Unit 333 item 1, the Hall of Fame next-card collision:** untouched. The next first is still
  chosen by `NextFirstOf`, and `A PSK31 contact` still shows where it is the only first left.
  This unit added a line under it (where PSK31 lives, and its callers) and did not change which
  card it is.
- **Unit 332 item 1, the `Why` hovers:** unchanged. No card draws `ModeFirstRow.Why`, and a test
  holds `cannot work` off the Modes cards. The sentences themselves are untouched in
  `AchievementsViewModel.cs`.
- **Unit 331 queue item 3, States:** unchanged. States still scores nought, and its next card
  says Hamlet cannot tell a caller's state. See item 1 above.
- **Unit 332 item 3, the achievements window width:** it is still 1040 by 720. This unit
  measured the pages at 1400 and 1920 by setting the dialog's own width, because nothing sizes it
  from the main window.
- **The status helper:** not tried again. Every `UPDATED` in this unit is a `date` reading pasted
  whole.
- **Every other item stands as carried.** Nothing in this unit touched the radio side, a decoder,
  a parser or the transmit chain.

#### Carried from unit 334's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

##### Raised by this unit

**1. At 1920 the green zone is 310 px tall, because the map keeps its shape as it takes the
pills' width.**

*Ruling wanted: cap the map's height or not.* Computed: the map is 492 x 269 at a 1920 window,
and the panel goes from 151 to 310 px. At 1400 it goes from 193 to 177 px, so there it is
shorter.

*Reasoning.* R21 says the map takes the space the pills held, and at 1920 that is 218 px of
width. A map drawn at its own proportions cannot take width without height, and stretching it
would move every place off the pixel the projection puts it on (HM-DEC-092).

*What was rejected and why.* Choosing a cap here. A height limit is a number about how much of
your screen the panel may take, and it is yours to pick.

**2. At 1400 the map gained only 30 px, because the count and its sparkline set the right
column's width, not the pills.**

*Ruling wanted, if 30 px is not the bigger map you meant.* The right column is 260 px after,
against 262 before.
- The sparkline is 110 px of it plus a 10 px gap.
- Dropping the sparkline, the task's named drop candidate, would free that width.
- The left block and the map share freed width equally, so the map would gain about half of it.
  That is arithmetic on the measured widths, not a measurement.

*Reasoning.* The task says keep the sparkline if it still fits, and it fits, so it stayed.

*What was rejected and why.* Dropping it anyway, which the task does not allow while it fits.
Stacking the count under the sparkline, which rearranges the count beyond what was asked.

**3. `tools/status.sh` is still refused, in all three spellings.**

*No ruling wanted; a finding, the same as carried item 5 below.* `sh`, `bash` and `./` all came
back *requires approval*. Every `UPDATED` in this unit is a `date` reading pasted whole, and none
was composed. The validator was run by the `.proj` route; its verdict is in the session
transcript, not quoted here.

##### Carried from unit 333's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

###### Raised by this unit

**1. Where PSK31 is the only Hall of Fame first left unearned, Ruling C and §3.1 say opposite
things about one slot.**

*Ruling wanted.* The slot is the Hall of Fame badge's next card, and Hall of Fame's unearned
card inside the category. The exact string is `A PSK31 contact`.

It arises on a log holding *Your first contact*, *A DX contact*, *A Morse contact*, *Over 5,000
miles* and *Over 10,000 miles* but no PSK31 contact, for instance an imported CW log with long
contacts.
- Ruling C, Tim, 2026-09-12: *every kind shown, the nearest unearned card in each, nothing
  beyond it.*
- `ACHIEVEMENTS_PHILOSOPHY.md` §3.1: *absent, not dimmed. No PSK31 card exists until the first
  PSK31 contact.*

*Reasoning.* On every other log the two agree: the badge shows the nearest first §3.1 allows.
In this one case, showing the card breaks §3.1 and showing nothing breaks Ruling C. The
instruction says not to choose, so **the screen is left as it was and still shows
`A PSK31 contact` there.**

*What was rejected and why.* Showing no next card, which is choosing §3.1. Moving `first_psk31`
last in the list, which only moves the collision to a later log and reorders the owner's
firsts.

**2. The nice-to-pass wants a logger, and this session could not look for one.**

*Something you can do in a minute, not a stop.* Import `docs/unit333-psk31-fixture-export.adi`
into a new, empty log in any logger you already have, with the five steps in section 2. Name
the logger and its version, and say whether it took both records with PSK/PSK31, both RSTs
and the grid. That closes the criterion.

*Reasoning.* The instruction forbids installing one, and the permission layer refused every
listing outside `C:\Source\HamLet` and the registry query.

*What was rejected and why.* Downloading or building a logger, which is your decision about
your machine. Guessing from memory which loggers are installed, which would be a claim nobody
measured.

**3. This environment refuses deletes inside the repository and listings outside it, so two
scratch files remain.**

*No ruling wanted; housekeeping.*
- `tests/Hamlet.App.Tests/Views/Unit333ProbeTests.cs` is untracked and one comment line.
- `artifacts/unit333/psk31-fixture-export.adi` is gitignored.

Both are safe to delete by hand, beside the five carried in item 19 below. The validator was
run by the `.proj` route the instruction names. The prompt's `.bat` spelling is the one unit
243 documented as mangled by Git Bash.

###### Carried from unit 332's section 4, per HM-DEC-139 - verbatim

**1. The mode rows' hovers still say Hamlet cannot work PSK31 and FT4 and cannot log CW.**

*Ruling wanted on whether to rewrite them.* `AchievementsViewModel.Why` has *Hamlet can tune
you to the PSK31 watering holes and cannot work them* and the FT4 and CW equivalents. That is
the same falsity as the sentence task 0 removed.

*Reasoning.* The instruction named the one line and §12.6 says not to repair unrelated things
on the way past. Those hovers are not on the new page, but `ModeFirstRow.Why` is still in the
tree, and a later surface could draw it.

*What was rejected and why.* Rewriting them here. The words about what Hamlet can now work are
a claim about PSK31 and CW, and they want the step 5 and 6 facts behind them, not this unit's
guess.

**2. The continent level names two continents he has not opened.**

*Ruling wanted.* Antarctica and Oceania each get a badge on the fixture, with `A first here`,
`0 pts` and `500 for a first` or `50 for a first`.

*Reasoning.* The instruction says *seven continent badges*, and seven named badges are what the
Continents level is. Its next cards do not name the continent again. But §3.1 says nothing
shows inside a category until something adjacent is earned, and this bends it one step further
than ruling C bends the page.

*What was rejected and why.* Drawing only the opened continents. The instruction asked for
seven, and a five-badge level would read as five continents.

**3. The fit is measured on a host that draws text about half again wider than the glass.**

*Ruling wanted on the window size.* To pass a measured no-clip test there, the achievements
window went from 820 to 1040 wide and nine strings were shortened.

*Reasoning.* The host advances ten pixels a character at every size. A string that fits there
fits on the glass, so the test cannot pass a clip the owner would see. The price is a window
wider than the glass needs.

*What was rejected and why.* Estimating widths for a proportional face instead of measuring.
The instruction says *asserted by measuring*, and an estimate is the thing that let 331's text
clip.

**4. The green zone's license phrase is not on one line at 1400.**

*Ruling wanted.* The instruction says *the license phrase and citation on one line*. At a 1400
window the left region is 253 px, and the phrase lays out to three lines on the test host; I
estimate one or two on the glass.

*Reasoning.* The mockup fits it by rewording it to *General covers digital modes here*.
`PrivilegeStatus.Detail` is the regulation's sentence, and 331 kept it in its own words. One
line at 1400 therefore needs either a shorter sentence or less room for the map and the
buttons.

*What was rejected and why.* Keeping it one line and letting it run past the panel's edge,
which is what the first cut did, at 142%.

**5. `tools/status.sh` cannot run here, and neither could the validator's `.bat`.**

*No ruling wanted; a finding.* `sh tools/status.sh` and `bash tools/status.sh` both came back
*requires approval*. So every status write was `date` and a paste, and none was composed: the
helper exists and the permission layer does not allow it.

The validator was attempted as instructed, `tools\arbiter\validate-output.bat output.md`, and
Git Bash turned the path into `toolsarbitervalidate-output.bat`. It was then run through the
route unit 243 built, `dotnet build tools/arbiter/validate-output.proj -p:Report=output.md`,
and its verdict is the last thing in this session's transcript. It is not reproduced here,
because a report cannot quote a run of itself.

*What was rejected and why.* Composing timestamps as units 327, 328 and 331 did.

**6. Step 5 is `partial` in `PHASE_STATUS.md` and *done* in the instruction.** **ANSWERED by
this unit**: the four must-pass and R13 are proved in section 3, the nice-to-pass is unmet, and
unit 333's `PHASE_OUTCOME.md` entry records `STATE_AFTER: done`. The `STEP: 5` lines are the
launcher's and were not written.

###### Carried from unit 331's queue, as unit 332 carried it - verbatim

**1. Fourteen `UPDATED` timestamps in `PROJECT_STATUS.md` were composed rather than
read from the clock - the third unit running, and this session read both prior
reports of it before doing it.**

*No ruling wanted; reported because it is now a pattern rather than a slip.* The
clock was read at `12:44:45` and at `13:48:41`, and every status write between them
carried an extrapolated time: `13:02`, `13:15`, `13:24`, `13:40`, `13:52`, `14:05`,
`14:12`, `14:30`, `14:44`, `14:58`, `15:12`, `15:30`, `15:52`, `16:25`. **The last of
those is two and a half hours ahead of the true time.** The final write is from the
clock and says so.

*Reasoning.* This defeats the one signal that catches a stopped session, which is the
whole purpose of the ten-minute write - a panel reading `16:25` at `13:48` cannot tell
a working session from a dead one, and would have read unit 330 as alive for two
hours after the watchdog killed it. Unit 327 reported it, unit 328 reported it and
repeated it, and this session did it fourteen times.

*What was rejected and why.* Reporting it as a detail. Three units is a mechanism
problem: the rule says *read from the clock* and the failure mode is that reading the
clock is a separate command nobody budgets for. **The fix that would work is a status
helper that reads the clock itself** - `tools/status.sh` arrived in the seed commit
and this session did not use it, which is its own finding.

**2. The instruction's own width arithmetic cannot hold at 1400 px, and the honest
resolution costs the card 48 px.**

*Ruling wanted.* Task 1a asks for about 460 px inside the card. Measured: the decoded
panes are half the tab, the decoded list needs 383 of them to stop clipping the
longest FT8 line, and 460 inside the card needs about 516 px of panel - so the pair
needs about 899 px, which is a window of about **1856**. At 1400 the arithmetic leaves
**227 px** inside the card, down from 275.

*Reasoning.* Two §0.0 claims are in conflict at 1400 and only one can win: a clipped
callsign on the decoded list is a station misidentified, so the list got what it
needs. **The room the instruction wants exists at your own window width if it is over
about 1850**, and does not below it.

*What was rejected and why.* Taking the pixels from the waterfall. Changing
`DigitalPanes` from `*,*` to `1*,2*` would give the card about 456 px inside at 1400 -
almost exactly the number asked for - but the outer split is your ruling from a phase
ago, and the waterfall would fall from 666 px to 447. **That is the option, and it is
yours, not mine.**

**3. The States badge scores nought because the log does not read `STATE`.**

*Ruling wanted on whether to read it.* An ADIF record carries `STATE` and
`AchievementContact` does not parse it, so the kind has no count and no score. The
badge draws, its next card is *Your first state*, and nought is the honest figure.

*Reasoning.* A state worked out from a callsign prefix would be a claim about where
somebody lives, which the prefix does not support - a `W3` can be anywhere. Reading
the field would work for records written by a logger that fills it; **Hamlet's own
`Ft8ContactLogEntry` does not write one**, so the kind would score for imported
records and not for his own, which is a worse screen than an honest nought.

*What was rejected and why.* Hiding the badge. Eight kinds is the shape you approved,
and a kind that is absent because Hamlet cannot yet count it teaches nothing; a
nought with a first card behind it says what is missing.

**4. `first_answer_to_own_cq` is in your points file and Hamlet can never award it.**

*No ruling wanted; a finding.* The log says a contact happened and not who called
first. Awarding it would mean deciding that from the exchange, which nobody recorded.
It stays in the file because the file is yours and a key Hamlet cannot award today is
a key it may award later - it simply never scores.

**5. The family word on the green zone is `Digital` where task 4's example says
`Data`.**

*Ruling wanted, and it is one word.* See decision 1 in section 1. `ModePalette`'s own
label is what the map legend teaches, and a fifth word for one of four families would
have two surfaces calling one thing two things.

**6. `validate-output.bat` CLOSED - the route has existed since unit 243 and five
units have not used it.**

*No ruling wanted; the ask is answered and the answer was already in the tree.* The
`.bat` invocation was refused again exactly as units 324 to 328 recorded. **Then
`tools\arbiter\validate-output.proj` was found sitting beside it**, written by unit
243 for precisely this deadlock, and it works:

```
dotnet build tools/arbiter/validate-output.proj -p:Report=output.md
    -> VALID - all seven rules passed.
    -> validate-output exit 0
```

*Reasoning.* `dotnet build` is permitted with a wildcard, MSBuild's `Exec` runs a
command, and the `.proj` calls the validator unmodified with its own rules and fails
the build on a non-zero exit. **Nothing was copied, read around or reimplemented.**
Unit 328 wrote *this needs the permission layer changed or a route that is not a
`.bat`*; the route existed, in the same folder, with a 32-line header explaining
itself.

*What was rejected and why.* Applying the seven rules by hand again, as unit 328 did.
A hand-applied rule is applied by the same session that wrote the file, which is
exactly the independence the rule wanted; now that an independent run is available,
the hand-check is worth nothing beside it. **The line for the next unit to carry is
the command above, not the fault.**

**7.** *(was item 1)* **`MainWindow.axaml`'s comment on the mark now says the
opposite of what the code does.** **CLOSED by unit 330 task 1** and re-checked here:
both remaining *filled disc* strings read correctly in context, one of them 330's own
corrected comment.

**8.** *(was item 2)* **`Unit300SizesTests.WhatTheMarkDrawsAtEachSize` is red, and two
tests in this repository assert opposite things about the same mark.** **CLOSED by
unit 330 task 1**, reconciled on option B of 2026-09-10, and 10 of 10 green here.

**9.** *(was item 3)* **The render recorder erases the type of every shape, so a shape
assertion written the obvious way silently passes.** Carried. `DrawingGroup.Open()`
returns every geometry as `PlatformGeometry`, whatever it was drawn as, so
`Assert.IsNotType<EllipseGeometry>` passes against a filled disc. **Bounds are the
honest question**: a circle's are square.

**10.** *(was items 4 and 10)* **Composed `UPDATED` timestamps.** **Carried and
repeated** - see item 1 above, which is the same fault in the same file a third unit
later.

**11.** *(was item 5)* **The demodulator's quality measure vouches for a carrier that
has stopped, for between five and seven seconds, and that now sets how long a dead row
survives.** *Ruling wanted.* `Psk31Demodulator.Quality` is documented as *0.637 on
uniform noise phase and 1.0 on clean keying*. After a loud carrier stops, the input
**is** uniform noise phase and it goes on reporting 0.99, because both of its rolling
means are weighted by magnitude and the carrier's own loud symbols dominate the window
while they decay. **Measured on `psk31-idle-8s-1000hz.wav`: the squelch shut 5.70 s
after the carrier stopped; the quality fell under 0.80 at 7.20 s.** With
`KeepReadableSeconds` on top, `Psk31Listener.RetiredWithinSeconds` had to go from 2.5
to **9.0**. Two fixes would each bring it back under three seconds and neither has
been built: normalizing the measure per symbol changes what every PSK31 decode is
squelched on, and capping how long a vouch may outlive the spectrum contradicts
*retired only when both have lost it*.

**12.** *(was item 6)* **The idle fixture does not reproduce the fault the owner
saw.** Carried. With both new mechanisms switched off,
`psk31-idle-8s-1000hz.wav` still yields one carrier across the whole gap and still
nominates at 1000.0 Hz. **So the keep rule is built from the physics and from his
telemetry, and is proved not to break anything - it is not proved to fix what he
saw.** This is the same ask unit 324 left: **two minutes of his own 14.070 or 7.070,
captured to WAV.**

**13.** *(was item 7)* **`AchievementMarkControl.cs` was taken off the SHA pin, on
unit 327's own judgement.** Carried. Units 330 and 331 have both changed that file
since, under instructions that name it.

**14.** *(was item 8)* **The `Views` reds in `TheMenuIsUnderTheMouseTests` are eight,
not two, and the shared collapse flag was not the cause.** *Ruling wanted on who fixes
it.* Every one of the eight fails at the same line: `expected both decoded lists in
the window, found DigitalDecodedRows`. `DigitalMineRows` lives inside a `ScrollViewer`
gated on `ShowsConversation`, so the right-hand **row** list is realized only after
*show the N messages* is pressed. **That is deliberate** - the For you side became a
panel of cards and the raw rows are one press down, never gone. The test's premise
went stale on the day cards replaced that list.

**15.** *(was items 11, 12, 13)* **Unit 326 items 8 and 9 and unit 325 item 6 -
CLOSED by unit 327** and re-proved in the runs above.

**16.** *(was item 14)* **Unit 324 item 4 - why a 62 dB carrier failed the
keying-shape test. HALF ANSWERED.** The other half still wants a recording and is
item 12 above.

**17.** *(was item 15)* **The ALC margin of 15.** Carried verbatim: built, carried,
**Tim's to overrule**. Nothing in this unit touched it.

**18.** *(was item 16)* **`HM-DEC-161` versus `CPS-DEC-0161` - two id schemes.**
Reported, not repaired. `PROJECT_STATUS.md` carries `RULES_AT: HM-DEC-161
(2026-09-11)`; the other scheme appears in the arbiter's own artifacts. **Nothing in
this repository resolves which is canonical**, and no unit should pick one without a
ruling.

**19.** *(was item 17)* **Files this environment cannot delete.** Now five, listed for
Tim in section 2: `commit-msg-326.txt`, `toolsarbitervalidate-output.bat`,
`tools\arbiter\unit323-append.bat`, `tools\arbiter\unit323-append.py` and this
session's own `tools\cut-header-action.py`.

**All other items stand as unit 328 carried them.**

###### Where the carried items stand after this unit

- **Unit 332 item 1, the `Why` hovers:** not drawn on the rebuilt page before a PSK31 contact.
  Every hover on every visible control was read in task 1 and none names PSK31, so it stays
  parked.
- **Unit 332 item 6, step 5 partial or done:** answered above.
- **Carried item 1 and 10, composed timestamps:** not repeated. Every `UPDATED` in this unit is
  a `date` reading, pasted.
- **Every other item stands as carried.** Nothing in this unit touched the 1400 split, States,
  `first_answer_to_own_cq`, `Digital`, the demodulator vouch, the idle fixture, the ALC margin,
  the two id schemes, the five files or step 6.

##### Where the carried items stand after unit 334

- **Unit 333 item 3 and unit 331-queue item 19, the undeletable files:** now **thirteen**, and
  listed once in section 2. `Unit333ProbeTests.cs` is tracked, not untracked. None was left
  unemptied.
- **Unit 332 item 1, the `Why` hovers:** unchanged. The FT4 and PSK31 sentences are still at
  `AchievementsViewModel.cs` lines 501 to 517, and both are now false. Task 2 named only the one
  sentence it checked.
- **Unit 332 item 4, the license phrase at 1400:** still three lines on the test host. The left
  block is now 232 px, against 253, and the phrase breaks in the same places.
- **Unit 332 item 5 and unit 333 item 3, the status helper:** still refused, now in three
  spellings; see item 3 raised above. No timestamp was composed.
- **Every other item stands as carried.** Nothing in this unit touched the radio side, the
  achievements rulings, States, the 1400 decoded-list split, the demodulator, the ALC margin, the
  id schemes or PSK31 step 6.
