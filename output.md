```
READ IN THIS ORDER.
```

A. The phase goal - the screen, done right. Step 0 partial after this unit: five of its six
   must-pass met, and the sixth (1400) met by its test with one named miss. Steps 1 to 3 not
   started.
B. Step 0 and its six must-pass - each met or not, with the number.
   1. The top row about 190 px at 1920, and the working panels take the rest: met. The top row
      went from 445 to 190 px, and the working card from 318 to 573 px, reaching the status bar.
   2. The neighborhood card carries the strip, the green block and the world clock, with the band
      as the largest text and one dot on the clock: met. The band is 20 pt, and the clock is
      246 x 134 at the card's right end with 1 marker.
   3. The rig display at the card's height, with drive and the power offer under the S-meter: met.
      The rig panel is 190 px against the card's 190.
   4. The three panels equal height, full to the status bar, and the facts beside the map at 1920:
      met. The panels are 363 px each with one top and bottom. The split is 735/378/734 of 1862.
      The card is 678 px inside, and its facts sit beside the map.
   5. At 1400 the same shape, no callsign clipped, and the facts placed by the stated rule: met by
      its test. The card is 419 px inside against 568 needed, so the facts go under the map, and
      no callsign is clipped. Named miss: on a licensed operator's fixture the top row is 273 px,
      0.300 of the height below the band pills, against the mockup's 0.262.
   6. BindingHealthTests, VoiceTests and the carry-forward list green: met. The layout set is 57
      of 57, app carry-forward 100 of 100, and engine 85 of 85.
   The nice-to-pass (best bet joined to the green block by the check) is kept in the markup, and
   its view-model test is green. It was not measured on the window after the change.
C. The report last. Section 4 raises 3 items on top of the carried queue. Item 1 bears on
   must-pass 5, and item 2 on how must-pass 1 and 5 are read. Item 3 bears on nothing in B.

```
UNIT:       337 - complete at task 4 of 5, tasks 0 to 4 - 2026-09-12 20:00
PHASE GOAL: Make Hamlet's screens look the way Tim drew them - first the main window, then the
            achievements pages - judged at last by Tim at his own window.
UNIT GOAL:  Lay the main window out as the approved mockup: one short top row about where you
            are and what the radio is doing, and the waterfall, decoded list and For You given
            the height, at 1920 and at 1400.
ADVANCED:   yes - step 0 five of six must-pass met, the sixth met by its test with the 1400 top row named as a miss
NUMBER:     top row 445 -> 190 px, working card 318 -> 573 px at 1920; top row 329 -> 273 px,
            working card 434 -> 490 px at 1400 (both at 1040 px tall, licensed fixture)
DRIFT:      0
```

## 1. What Claude did

**Complete: tasks 0 to 4 of 5 are done, and step 0 is recorded `partial`.** Tasks 0 to 3 are
pushed as `107268c`, `a406dc1`, `9b07750` and `d1c2def`. Task 4 is this report; it is committed
with it.

Provenance: Windows 11, `C:\Source\HamLet`, branch `main`. The project gate passed:
`SHACK_FACTS.md` and `CwProbabilisticDecoder.cs` are present, `CoreHMI.sln` and `MURC.sln` are
absent, and the root is `C:\Source\HamLet`. `PHASE_STATUS.md` line 1 names *The screen, done
right*, with four steps.

**Every appearance claim in this report is computed on the headless host, not seen.** Nobody here
can look at a pixel.

### Task 0 - the phase opens

- `PHASE_OUTCOME.md` carries `UNIT 337` under step 0.
- The version is bumped from 1.13.21 to 1.13.22.
- `PROJECT_CARD.md` has `PHASE: The screen, done right`, with `PHASE_SET: 2026-09-12` unchanged.
- `DECISIONS.md` and the `CLAUDE.md` §1 index carry **HM-DEC-163**. It records Tim's ruling of
  2026-09-12: the screen phase is set on the mockup, and the maintenance phase is archived with
  step 2 partial.
- Carry-forward, before anything changed: app 100 of 100, engine 85 of 85.

### Task 1 - the top row

What moved:
- **The how-to mark** moved onto the neighborhood card's header.
- **The card became two columns.** On the left: the strip, the legend, and the green block under
  them. On the right: the world clock.
- **The green block** is now band, frequency and verdict on one line, then the license line, then
  the rule of thumb. The count and the sparkline sit at its right. The band went from 26 to 20 pt
  and is still the largest text.
- **The world clock** left the block. It is 134 px tall at the card's right end, captioned *where
  the sun is · you*.
- **Drive, its dBFS note and the PSK31 power offer** (with the ALC reference) moved from the send
  area to under the rig display. Every name is kept.
- **The top row's two halves stretch to one height.**
- **`GrayLineMapControl`** now finds the green block by name (`GreenZoneBlock`) for its
  `green_zone_rendered` event, since the map is no longer inside it.

Measured at 1920 x 1040:

| | Before | After |
|---|---|---|
| Top row | 445 px | 190 px |
| Working card | 318 px | 573 px |
| Green block | 1298 x 310 | 1038 x 62 |
| World clock | 492 x 269, in the block | 246 x 134, at the card's right end |
| Rig panel | 136 px | 190 px, the card's height |

At 1400 the top row went from 329 to 288 px, and then to 273 after task 3.

`TheTopRowTests` was watched red on 3 of 4:
- the top row at 445 px;
- the clock inside the green block;
- the drive not in the rig panel.

It is 4 of 4 after the change. **The green-block assertion passed before the change** - the block
was already inside the card with the band largest - so it was never watched red.

Rewritten under R12:
- **`TheGreenZoneTests`.** It asserted the map's width at 1400 (202 to 232 px), the count right of
  the map, and the rule of thumb under the map. It now asserts the clock's 134 px height, the
  count right of the left block, and the rule under the license line.
- **`TheDriveIsSetWhereHeIsLookingTests`.** It asserted the drive inside `DigitalSendReserved`. It
  now asserts the rig panel, and not the send area.

### Task 2 - the working panels

`DigitalPanes` is now one grid of `*,383,*` with a top row.
- **Top row:** the send area over the waterfall, with CQ and Stop in a column of their own. The
  filter bar sits over the two lists.
- **Second row:** the waterfall, the decoded list and For You, one top and one bottom.
- `DigitalDecodedPanes` and the waterfall's own scroller are gone.

**The 383 px is the decoded list's own arithmetic, not a fraction.** It is 24 + 76 + 48 + 35 for
the fixed columns and chrome, plus 200 px for `VP2MAA/P KC3QIS R-09` at 12 pt monospace on the
host. `TheWorkingPanelsTests` measures that line and compares the column: 383 against 383.

Measured at 1920 x 1040, before and after:

| | Before | After |
|---|---|---|
| Waterfall | 926 x 221 | 735 x 363 |
| Decoded text | 378 x 412 | 378 x 363 |
| For You | 543 x 412 | 734 x 363 |
| Card inside | 487, facts under the map | 678, facts beside the map |

**The split at 1920:** 735 / 378 / 734 px of 1862, which is 0.395 / 0.203 / 0.394.

`TheWorkingPanelsTests` was watched red on 2 of 4: the waterfall started 40 px above the lists, and
the facts were under the map at 1920. **The width and no-clip assertions passed before the change**
- the 383 column already existed - so they were never watched red.

Rewritten under R12:
- **`ThePanelsMakeRoomTests`.** Its lookups moved from `DigitalDecodedPanes` to `DigitalPanes`.
  *Half the pair* is now measured from the two panels and their margin.
- **`TheDigitalTabIsTwoColumnsTests`.** It asserted two equal halves with the send area under the
  waterfall. It now asserts three panels in order, one top and one bottom, and the send area above
  the waterfall in its column, still holding CQ.

### Task 3 - 1400

**The rule, the unit's own and overrulable:** the card's facts sit beside the map when the card is
wide enough inside for the map, 10 px, and the table's widest row. Otherwise they go under it. The
decoded column never narrows below its longest line, so the waterfall and For You give up width
first. The existing `WrapPanel` in the card already does this. At 1400 the card is 419 px inside
against 220 + 10 + 338 = 568, so the facts go under.

**The band line became a wrap panel.** At 1400 the verdict *Digital · FT8 · yours to use* sat in a
sliver beside the band and the frequency and broke into 5 lines. It now drops under them as a whole
and takes 2. On the licensed fixture the top row went from 288 to 273 px. Nothing moved at 1920.

`TheWorkingPanelsTests.AtFourteenHundredTheSameShapeHolds` passed on its first run. Tasks 1 and 2
had already delivered what it asserts, so nothing was watched red. At 1400 x 1040 it measured:

| | Run alone | After the rest of the layout set |
|---|---|---|
| Top row | 208 px (0.229) | 229 px (0.252) |
| Working card | 555 px (0.610) | 534 px (0.587) |
| The three panels | 374 px (0.411) | 307 px (0.337) |

Every share is of the 910 px below the band pills. **The spread is run order, not markup**: the
same markup gives both columns. Alone, the rows above the panels are the mode strip 32 px, the
readiness strip 43, the send area 54 and the filter bar 34; the tune strip is not shown. Which row
grows after the set was not measured.

This fixture has no license class and no heard count, so its green block is shorter than a
licensed operator's. The licensed fixture's 1400 numbers are in section 3.

### Task 4 - stand it up and describe it

The main window was realized by the tests at 1920 and at 1400, and section 3 describes each region
from those measurements, beside the mockup's numbers. **It was not launched on the desktop**:
nothing in this session can see a window, so the description is computed.

### Decisions this unit made for itself, marked as its own and overrulable

1. **The window is measured at 1040 px tall at both widths**: a maximized window on a 1080 px
   screen less a taskbar. The mockup is drawn at 810.
2. **The world clock is 134 px tall at every width**, the mockup's height. Its width follows the
   picture's own proportions (246 px), so no place moves off its pixel.
3. **The top row's two halves stretch to one `Auto` row.** Nothing inside writes a height, so what
   keeps the row short is the card's own content.
4. **The waterfall and For You share what the decoded column leaves, equally (`*,383,*`).** R26
   says For You takes the rest. The mockup's waterfall is wider than its For You, and equal stars
   were chosen as the smallest rule that puts the facts beside the map at 1920.
5. **The send area sits above the waterfall, not under it**, because three panels with one bottom
   leave no room under any of them. CQ and Stop are a column of their own, so no line beside them
   can move them, and both are outside every collapsible panel. They bind the same commands as
   before.
6. **The drive and the power offer show on every tab**, because they are in the top strip, which
   Tim ruled is the same in every mode. The row does not change height when the tab does.
7. **"The working panels" is read as the working card** - the tab's region below the tabs that
   holds the three panels. R26 says *the working panels below the tabs take the rest of the
   window*. The three panels' own share is printed beside it (section 4, item 2).
8. **The facts rule** in task 3 above, and **the band line as a wrap panel**.

### Where the instruction and the tree disagreed

- **`tools/status.sh` could not run.** The instruction says to use it for every status write, and
  `sh tools/status.sh` came back *requires approval*. Every `UPDATED` in this unit is a `date`
  reading pasted whole; none was composed.
- **§11 says commit per task and push at the end; the prompt says commit and push each task.**
  Each task was pushed, per the prompt.
- **`git stash` also came back *requires approval***, so two reds could not be rerun at HEAD
  (section 4, item 3).
- **Every other item §5 names matched the tree.** The green zone as unit 334 left it (map 492 x
  269 at 1920). The world clock control. The rig panel. Drive and the power offer in the digital
  send area. The tabs, the working card and the three panels with the bar above them. The split
  at `383,*`. The card's map row and facts table. The seven named tests. `PROJECT_CARD.md`'s
  `PHASE`, which was the maintenance phase's until task 0 moved it. `TheGreenZoneTests` and
  `TheCardsRightColumnTests` live under `tests/.../ViewModels` though they realize the window;
  that is noted, not moved.

### Reds older than this unit

- **`TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission`.** It expects a single
  `_armedSend.Arm(` line in `src`. There are two, `MainWindowViewModel.cs` lines 14202 and 14381,
  in a file this unit did not touch.
- **`TheWholeChainRunsFromOneRightClickTests`, two tests.** Both fail with *no realized row... mine
  rows: 1; realized row roots: 0*. The diff touches none of `DigitalMineRows`,
  `DigitalDecodedRows`, `ShowsConversation` or the row's context handler. This is the cause unit
  331's item 14 recorded. **Not proved by a run at HEAD**: the stash was refused.

## 2. What the owner should expect

**What is now true, at your window:**
- The top of the window is one short band.
- **The neighborhood card** holds the strip and its legend, a compact green block under them, and
  the world clock at its right end.
- **The rig panel** beside it is exactly as tall. Under the frequency and the S-meter it holds
  *Transmit drive* with its box, the dBFS note and, on PSK31, the power offer.
- **Under the tabs, on the Digital tab:**
  - a short send strip above the waterfall, with CQ and Stop at its left and the lines about what
    went out beside them;
  - the slot clock, the filter and `clear` above the two lists;
  - then the waterfall, the decoded list and For You, the same height, running down to the status
    bar.
- **At 1920 the conversation card's facts sit beside its map.**

**What will look wrong but is not:**
- **The drive and the power offer are on the CW and Voice tabs too**, in the top strip. That is
  decision 6: the header does not change between tabs.
- **The waterfall at 1920 is narrower than before (926 to 735 px) and much taller (221 to 363
  px).** The picture is resampled into its rectangle; nothing about what it measures changed.
- **At 1400 the facts sit under the map on the card.** That is the rule, and it is what keeps the
  decoded list from cutting a callsign.
- **The world clock is half the size unit 334 made it** (492 x 269 to 246 x 134). That is the
  mockup's size.
- **The `?` for the neighborhood map's how-to is on the card's header** now, beside the title.
- **At narrow widths the verdict (*Digital · FT8 · yours to use*) sits on its own line** under
  the band and the frequency.
- **The headless host draws text about half again wider than the glass** (unit 332's
  measurement). So at 1400 the green block probably wraps less on your screen than the numbers
  below say. That is an inference, not a measurement.

This unit created no file it could not delete.

## 3. What you should see

**The answer: the top row went from 445 to 190 px at 1920, and the working card from 318 to 573
px.** The three working panels are 363 px each, with one top and one bottom at the working card's
floor. At 1400 the same shape holds, with the top row at 273 px.

Every built number here is computed from the real main window realized headless at 1040 px tall.
None was seen. The mockup's numbers are the instruction's, drawn at 810 px tall. They are a
picture, not a specification, so compare shapes and proportions rather than pixels.

### At 1920

| Region | Mockup | Built |
|---|---|---|
| Header and band pills | to about y 100 | pills end at y 130 |
| Top row | y 110 to 296, 186 px | y 140 to 330, **190 px** |
| Neighborhood card | x 14 to 1000 | 1328 x 190 at x 16 |
| Green block | 28-720 x 210-282, 72 px tall | 1038 x 62 at 31,238 |
| Band in the green block | 16 pt bold, the largest text | 20 pt bold, the largest text in the card |
| World clock | 246 x 134 at the card's right end, one dot | 246 x 134 at x 1083, 1 marker, *where the sun is · you* under it |
| Rig display | x 1012 to 1488, the card's height | 546 x 190 at x 1358, the card's height |
| Drive and power | a line under the S-meter | the drive box at 1521,274, under the display; the power offer under it on PSK31 |
| Tabs and working card | tabs y 310; card y 336 to 770, 434 px | card y 393 to 966, **573 px** |
| Waterfall | x 28 to 640, y 376 to 758 | x 29, 735 wide, y 590 to 953, 363 tall |
| Decoded text | x 652 to 1068 | x 774, 378 wide, same top and bottom |
| For You | x 1080 to 1474 | x 1157, 734 wide, same top and bottom |
| Card's facts | beside the map | beside it: card 678 px inside, table at x 232 |
| Status bar | unchanged | y 978, 46 px |

As shares of the height below the band pills, the mockup's top row is 186 of 710 (0.262). Built,
it is 190 of 910 (0.209), and the working card 573 of 910 (0.630).

### At 1400

| Region | Built |
|---|---|
| Top row | y 140 to 413, **273 px** (licensed fixture); 0.300 of the 910 below the pills |
| Neighborhood card | 808 x 273 |
| Green block | 518 x 145. The verdict takes 2 lines, the license line 4, the rule of thumb 6. |
| World clock | 246 x 134, 1 marker |
| Rig panel | 546 wide, stretched to the card's height. The two were measured equal at 288 before the task 3 change and are not re-measured at 273. |
| Working card | 1368 x 490 at y 476 |
| Waterfall / decoded / For You | 474 / 378 / 475 px wide, one top and bottom. They are 307 to 374 px tall depending on run order (section 1, task 3). |
| Card's facts | under the map: 419 px inside against 568 needed |
| Decoded message column | 200 px. `VP2MAA/P KC3QIS R-09` needs 200, and `CQ DX K9XP JN88` needs 150. No callsign is clipped. |

## 4. What's blocking us

### Raised by this unit

**1. At 1400 a licensed operator's top row is 273 px - 0.300 of the height below the band pills -
against the mockup's 0.262.**

*Ruling wanted, or the arbiter's own recommendation taken: how to shorten the green block at 1400.*
The block wraps because its text column is about 210 px wide there. The clock takes 246 px and the
count with its sparkline about 270. On the host the license line takes 4 lines and the rule of
thumb 6. The options, each measured against those numbers:
- **(a) Shorten the rule of thumb to the mockup's own words**: *Rule of thumb: 20 m and up want
  daylight along the path; 40 m and down want dark.* That drops *the gray edge is where both
  happen*.
- **(b) Take the sparkline off**, task 1's named drop candidate, keeping the count. That widens the
  text column by about 120 px.
- **(c) Accept it.** The host draws text about half again wider than the glass, so on your screen
  the block is probably shorter. That is an inference.

*Reasoning.* §6 says a string that will not fit is shortened and named, or widened, never clipped.
The recommendation is (a): the mockup is the ruling, and it already draws the shorter sentence.

*What was rejected and why.*
- Hiding the rule of thumb behind a mark: the mockup shows it on the card.
- Shrinking the world clock at narrow widths: its size is the mockup's.
- Rewording the license line: it is `PrivilegeStatus.Detail`, the regulation's own sentence in
  the engine's words.

**2. "The working panels" is read as the working card. Read as the three panels themselves, they
are under half the height below the pills at both widths.**

*Ruling wanted only if the panels themselves were meant.*
- **At 1920** the working card is 573 of 910 px (0.630). The three panels are 363 (0.399).
- **At 1400** the card is 534 to 555 (0.587 to 0.610). The panels are 307 to 374 (0.337 to 0.411).
- **What stands between them**, measured alone at 1400: the mode strip 32 px, the readiness strip
  43, the send area 54 and the filter bar 34. The readiness strip shows only while there is
  something standing between you and a decode; on the test host there is no radio.

*Reasoning.* R26 says *the working panels below the tabs take the rest of the window*, and the
region below the tabs is the working card, so that is what the test asserts. Both numbers are
printed on every run.

*What was rejected and why.* Folding the send strip or the filter bar into a panel header to win
back height: collapsing that panel would then hide Stop (§0.2), or the filter (§R17).

**3. Three reds off the carry-forward list are older than this unit.**

*No ruling wanted; a finding.*
- **`TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission`** expects one
  `_armedSend.Arm(` line in `src`. There are two, at `MainWindowViewModel.cs` 14202 and 14381, in
  a file this unit did not touch.
- **`TheWholeChainRunsFromOneRightClickTests`, two tests,** find *mine rows: 1; realized row roots:
  0*. That is the `DigitalMineRows`-behind-*show the messages* cause of unit 331's item 14. This
  unit's diff touches none of that path. **Not proved by a run at HEAD**, because `git stash` was
  refused. Both tests also open a real audio endpoint.

### Where the carried items stand after unit 337

- **Unit 336 item 1, how the decoded list and For You share the tab: ANSWERED by R26 and this
  unit.**
  - The tab is `*,383,*`.
  - At 1920 the facts sit beside the map, with the card 678 px inside.
  - At 1400 they go under it, with the card 419 px inside.
  - No callsign is cut at either width.
  - The outer `*,*` was superseded by the mockup, Tim's own, not by this unit.
- **Unit 331 queue item 2, the 1400 width arithmetic:** superseded the same way. Its numbers are
  replaced by section 3's.
- **Unit 334 items 1 and 2, the map's height at 1920 and its 30 px at 1400:** superseded by R26.
  The world clock is 246 x 134 at the card's right end at both widths.
- **Unit 332 item 4, the license phrase at 1400:** still wraps, now 4 lines on the host at 1400 and
  1 at 1920. See item 1 above.
- **The status helper:** still refused. Every `UPDATED` is a `date` reading pasted whole.
- **Every other item stands as carried.** Nothing in this unit touched:
  - the radio side, a decoder or a parser;
  - the transmit logic (CQ, Stop, the drive and the power offer moved in markup only, on the same
    commands);
  - States, rank names, the Modes card, the achievements pages;
  - the demodulator, the ALC margin, the id schemes, or PSK31 step 6.

### Carried from unit 336's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

#### Raised by this unit

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

#### Where the carried items stand after unit 336

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

#### Carried from unit 335's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

##### Raised by this unit

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

##### Where the carried items stand after unit 335

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

##### Carried from unit 334's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

###### Raised by this unit

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

###### Carried from unit 333's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

####### Raised by this unit

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

####### Carried from unit 332's section 4, per HM-DEC-139 - verbatim

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

####### Carried from unit 331's queue, as unit 332 carried it - verbatim

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

####### Where the carried items stand after this unit

- **Unit 332 item 1, the `Why` hovers:** not drawn on the rebuilt page before a PSK31 contact.
  Every hover on every visible control was read in task 1 and none names PSK31, so it stays
  parked.
- **Unit 332 item 6, step 5 partial or done:** answered above.
- **Carried item 1 and 10, composed timestamps:** not repeated. Every `UPDATED` in this unit is
  a `date` reading, pasted.
- **Every other item stands as carried.** Nothing in this unit touched the 1400 split, States,
  `first_answer_to_own_cq`, `Digital`, the demodulator vouch, the idle fixture, the ALC margin,
  the two id schemes, the five files or step 6.

###### Where the carried items stand after unit 334

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
