```
READ IN THIS ORDER.
```

A. The phase goal - the screen, done right. Step 0 after this unit: all six must-pass met as
   measured on the test host, and the nice-to-pass met. Every number is computed on a headless
   window 1040 px tall, not seen, and the STEP line is the launcher's to write. Steps 1 to 3
   not started.
B. Step 0 and its exit criteria, each met or not, with the number:
   1. Top row about 190 px at 1920, and the three PANELS take at least half below the band
      pills: met. The top row is 190 px. The panels are 503 px (0.553) on the licensed fixture
      and 505 px (0.555) on the plain one with the readiness strip hidden, and 450 and 452 px
      (0.495, 0.497) with it showing.
   2. The neighborhood card's three things: two things moved. The rule of thumb is the mockup's
      sentence (ruling 2). At 1400 the sparkline hides and *heard just now* stands over the
      count (ruling 3); at 1920 the sparkline shows. The count is on the card at both widths.
      The band strip and the world clock are unchanged.
   3. Rig panel at the card's height, at 1920 AND 1400: met. 190 against 190 px at 1920, and
      219 against 219 px at 1400.
   4. Three panels equal height to the status bar, facts beside the map at 1920: met. They have
      one top and one bottom at the working card's floor, and the split is still 734/378/735.
   5. At 1400 the same shape: met. The panels are 474 px (0.521) licensed and 464 to 494 px
      (0.510 to 0.543) plain, with the readiness strip hidden. The licensed top row is 219 px
      (0.241 against 0.262). No callsign is clipped, and the facts sit under the map by the rule.
   6. BindingHealthTests, VoiceTests, carry-forward: met. The layout set is 90 of 91, and the
      one red is the known `TheStopAddedNoNewRouteToATransmission`. Carry-forward is app 100 of
      100 and engine 85 of 85.
   nice-to-pass: best bet joined to the green block on the window - met, in task 3.
C. The report last. Section 4 raises 6 items on top of the carried queue. None stands in the
   way of a criterion in B. Item 1, a live license lookup in the plain fixture, is why unit
   337's 1400 numbers moved with run order.

```
UNIT:       338 - complete at task 3 of 4 - 2026-09-12 21:00
PHASE GOAL: Make the main window match the mockup Tim approved - a short top row and the
            working panels given the height - then the achievements pages as trading cards,
            what the last phase left, and Tim's own pass at his window.
UNIT GOAL:  Give the waterfall, the decoded list and For You themselves at least half the
            height below the band pills at 1920 and 1400, and bring a licensed operator's
            1400 top row back to the mockup's share.
ADVANCED:   yes - must-pass 1 and 5 now hold on the three panels themselves, must-pass 3 is measured at 1400, and the nice-to-pass is met
NUMBER:     three panels 443 -> 503 px (0.553) at 1920 and 360 -> 474 px (0.521) at 1400;
            licensed top row at 1400 273 -> 219 px (0.241). Licensed fixture, 1040 px tall,
            readiness strip hidden.
DRIFT:      0
```

## 1. What Claude did

**Complete: tasks 0 to 3 of 4 are done and pushed** as `6ab04aa`, `c846bcb`, `136b6b0` and
`d393f6f`. This report is committed after them, with one corrected markup comment.

Provenance: Windows 11, `C:\Source\HamLet`, branch `main`, and the prompt claimed Hamlet. The
gate passed: `SHACK_FACTS.md` and `src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` are
present, `CoreHMI.sln` and `MURC.sln` are absent, and the root is `C:\Source\HamLet`. This is
the development computer, so nothing in this report is evidence about the radio
(`SHACK_FACTS.md`, HM-DEC-093).

**Every appearance claim in this report is computed on the headless host at 1040 px tall, not
seen.** No decision was recorded in `DECISIONS.md`.

### Task 0 - the trace

- **The launcher's files.** `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` were
  committed unchanged in `6ab04aa`, with `WORK_INSTRUCTIONS.md` and the patch bump from 1.13.22
  to 1.13.23. `RULES_AT` is now HM-DEC-163.
- **Carry-forward before any change:** app 100 of 100, engine 85 of 85.
- **The trace** is `TheWorkingPanelsTests.Unit338TraceTheRowsAboveThePanels`. It prints and
  asserts nothing.

**The rows between the working card's top and the panels, before.** Heights are headless px.
The card's own border and padding add 13 px.

| Row | Height and margin | What it holds | Collapses | Conditional |
|---|---|---|---|---|
| 0 `DigitalModeStrip` | 34 px licensed, 32 plain, at both widths; 10 below | *on this frequency*, the four mode chips, the clock glyph or clock-offset line, the status line | no (Tim, 2026-08-28) | no |
| 1 `DigitalReadinessStrip` | 43 px at both widths; 10 below | the first thing between the operator and a decode; on the host, *nothing is listening yet...* | no | yes, on `HasDigitalReadiness` |
| 2 `DigitalTuneStrip` (the row §5 does not name) | not shown on either fixture; 10 below | what the last tune press did, `DigitalTuneLine`, confirmed or failed | no | yes, on `HasDigitalTuneLine`, after a tune press |
| 3, first row of the panes: `DigitalSendReserved` | 54 px licensed at both widths; 83 plain at 1920, 100 plain at 1400; 6 below | CQ, Stop, what went out, the license guard line (class unknown only), the unset line, the ALC line (PSK31), the level with its mark, where the contact stands | no | the area no; several of its lines yes |
| 3, first row of the panes: `DigitalListControlsBar` | 34 px at both widths; 6 below | the slot clock, `everything`, `CQ`, and with rows the order toggle and `clear` | no | the row controls on `HasDigitalDecodes`, the clock on `ShowsSlotClock` |

From the card's top to the panels' top that is 117 px on the licensed fixture with the readiness
strip hidden, and 170 with it showing, at both widths. The plain fixture at 1920 gives 144 and
197, and 197 is the instruction's figure.

**The panels, before**, as px and as a share of the 910 px below the band pills:

| Fixture | 1920, strip shown | 1920, strip hidden | 1400, strip shown | 1400, strip hidden |
|---|---|---|---|---|
| licensed | 390 (0.429) | 443 (0.487) | 307 (0.337) | 360 (0.396) |
| plain, license class unknown | 363 (0.399) | 416 (0.457) | 307 (0.337) | 360 (0.396) |
| plain, license class read as General | not measured | not measured | 374 (0.411) | not measured |

**The run-order spread, found.** Two rows grow, and they are the same two every time:
- the green block's third line, *Receiving is never restricted. Any license may listen
  anywhere.*, which is 21 px at 1400;
- the send area's license guard sentence, which takes the area from 54 to 100 px at 1400.

Both appear only while the operator's license class is unknown. Together they are 67 px, which
is exactly the gap between unit 337's 374 and 307.

**The plain fixture's class was General on some windows and unknown on others within one run.**
- The only code that writes a class by itself is the callook.info lookup (`LicenseResolver.Apply`,
  from `ResolveProfileAsync`). The view model's constructor starts it whenever there is a
  callsign and no class, and KC3QIS is General there.
- Setting the callsign after the constructor did not stop it: the first window of the next run
  still read General. So a second path exists and was not found, and that fixture change was
  reverted.

**So it is not proved to be state a test leaves behind, and nothing was fixed under R12.** The
criteria were built so their numbers do not depend on it. The licensed fixture sets its class,
read General on every window, and carries the 1400 assertion. The plain fixture passes in both
states. See section 4, item 1.

**The rig panel against the card at 1400**, licensed, before: 273 against 273 px, so they were
equal.

**The best bet.**
- **The pill's badge** is drawn in the band pill's template in `MainWindow.axaml`: a border over
  the card, bound to `IsBestBet`, saying `BestBetLabel`, and taking no clicks.
- **The green block's `GreenZoneBestBet`** reads `GreenZone.BestBet`. That is built from the band
  whose button carries the badge, plus ` ✓` when that band is the dial's.
- **No band is ranked on the test host**, so neither was visible on the realized window at
  task 0. The join holds by construction.

### Task 1 - the three panels take the height

**What moved. The arrangement is the unit's own, marked as such and overrulable.**
- **The send area rides the tab row, right of the tabs.** `DigitalSendReserved` keeps its name,
  CQ, Stop and every line. The box and its padding went, so it is 22 px tall beside a 30 px tab.
  The lines run across in a wrap panel: the three that are always there come first, then the
  license guard, the unset line and the ALC line. It shows only on the Digital tab.
- **The filter and the row controls ride the mode strip**, after the chips, in
  `DigitalHeaderStrip`. Their vertical padding is 2 px, so the strip stays its own height.
- **The slot clock rides the For you header**, at its right.
- **`DigitalPanes` is one row.** `DigitalListControlsBar` is gone.

CQ and Stop moved in markup only and bind the commands they always bound. Stop has no
collapsible ancestor at either width.

**Test watched failing first:**
`TheWorkingPanelsTests.TheThreePanelsTakeAtLeastHalfTheHeightBelowTheBandPills`. It was red at
416 of 910 px (0.457) on the plain fixture at 1920.
- **`StopNeverCollapsesAndTheFilterStaysOnAnEmptyOrCollapsedList` passed before the change.** Stop
  and the filter were already outside every collapsible panel, so it was never watched red.
- **The decoded column within 10 px of its need, and no callsign clipped**, are the existing
  `TheDecodedListIsAsWideAsItsLongestLineNeeds`, `NoCallsignIsClipped` and
  `AtFourteenHundredTheSameShapeHolds`. All three ran and are green.

Rewritten under R12:
- **`ThePanelsMakeRoomTests.OneBarAboveBothPanelsCarriesTheThreeControls`** is now
  `TheListControlsSitInTheModeStripAboveBothPanels`. The four controls are in the mode strip,
  above both lists and inside neither.
- **`TheDigitalTabIsTwoColumnsTests.TheThreePanelsAreColumnsWithTheSendAreaAboveTheWaterfall`**
  now asserts the send area is in the tab row above the working card, not in the waterfall's
  column.
- `TheFilterIsAlwaysThereTests`, `ThePanelScrollsTests` and `TheCardsRightColumnTests` passed
  unchanged and were not rewritten.

**After task 1**, with the readiness strip hidden:
- 505 px (0.555) plain at 1920, and 503 px (0.553) licensed at 1920;
- 464 to 485 px (0.510 to 0.533) plain at 1400;
- 420 px (0.462) licensed at 1400, which is under half until task 2.

The layout set was 71 of 72.

### Task 2 - the 1400 top row

- **Ruling 2 alone** took the licensed top row from 273 to 256 px (0.300 to 0.281). That is still
  above 0.262, and the panels were 437 px (0.480).
- **So ruling 3 was applied.** The top row is now 219 px (0.241), the panels 474 px (0.521), and
  the green block went from 518 x 145 to 518 x 91.

**The width rule, the unit's own.** The sparkline hides when the green block's text column, beside
a full-width count column, is narrower than the widest line the column holds on one line.
- **The full-width count column** is *heard just now* or the sparkline, 10 px, and the count or
  *last minute*, or the best-bet line if that is wider.
- **The widest line** is the license phrase, the rule of thumb, or the band, frequency and verdict
  together.
- **When it hides**, *heard just now* stands over the count, and the column is only as wide as its
  words.
- **It is measured from the words, in `MainWindow.FitTheHeardCount`, not from a window width.** The
  host and the glass draw words at different widths.
- **It reads the width the count column would have if it were wide**, so hiding cannot undo itself.
- **On the host** it hides at 1400 and not at 1920.

`GreenZone.RuleOfThumb` is now *20 m and up want daylight along the path; 40 m and down want
dark.* Its remarks name work instruction 338 and the mockup.

**Test watched failing first:** `TheTopRowTests.AtFourteenHundredTheLicensedTopRowIsTheMockupsShare`.
It was red on the old sentence, then on 0.281. It asserts:
- the licensed top row at 1400;
- the licensed panels at 1400;
- rig panel equal to the card within 2 px at both widths;
- the sentence;
- the count at both widths.

Rewritten under R12: **`TheGreenZoneTests.TheMapRegionRendersWithItsRuleOfThumbAndNoClaimOfOpenness`**.
It looked for the words *rule of thumb*; it now asserts the constant and its two clauses. The layout
set is 90 of 91.

**The first build did not engage the rule.** The window's name scope does not find the controls
inside the neighborhood card. `FitTheHeardCount` now looks them up by a walk of the visual tree and
keeps them.

### Task 3 - the best bet, on the window

`TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow` runs at 1920 on the
licensed fixture:
- with the best bet on 20 m, the pill wearing *best bet now* is 20 m, and the green block says
  `20 m ✓`;
- with the best bet on 40 m, the pill is 40 m, and the green block says `40 m` with no check.

It passed on its first run, because task 0 found the join already holds; the test is the task. No
source changed.

### The report

After task 3 the carry-forward list is app 100 of 100 and engine 85 of 85.

One markup comment written in task 1 said the Decoded text header had been *measured* too narrow
for the filter. It was arithmetic from the chip widths the host measured, and the comment now says
so. That fix is in the report's commit.

### Decisions this unit made for itself, marked as its own and overrulable

1. **Where the rows went** (task 1): the send area to the tab row, the filter and the row controls
   to the mode strip, and the slot clock to the For you header.
2. **The filter is not in the Decoded text header**, where the mockup draws it. On the host that
   header is 376 px inside, and with rows the four controls alone want about 430.
3. **The send lines were reordered** so the three that are always there share one line, and the
   conditional ones follow.
4. **The ruling 3 width rule**, as stated in task 2.
5. **The readiness strip is hidden in the tests by setting its visibility.** The host has no sound
   card, so the strip always has something to say there.
6. **The licensed 1400 panels are asserted in `TheTopRowTests`**, beside the top row that makes
   them reachable. `TheWorkingPanelsTests` asserts the plain fixture at both widths and the
   licensed one at 1920.
7. **The task 0 trace stays** in `TheWorkingPanelsTests`, printed and not asserted, as unit 336's
   did.

### Where the instruction and the tree disagreed

- **`CLAUDE.md` §1 carries no `CPS-DEC-0163`.** No `CPS-DEC` string is in the file, and its
  2026-09-12 row carries HM-DEC-163. This is reported, not resolved, because the id schemes are
  parked.
- **`WORK_INSTRUCTIONS.md` was uncommitted as well**, which §5 does not name. It went into task 0's
  commit.
- **The prompt says to set `PHASE_STATUS.md`'s `WORK_INSTRUCTION:` line, and §5 says to commit that
  file unchanged.** Both were done: it was committed unchanged in `6ab04aa`, then the line was set
  and committed in `c846bcb`.
- **`tools/status.sh` came back *requires approval*.** It was tried once. Every `UPDATED` is a
  `date` reading pasted whole.
- **A `sed` move of the markup blocks was refused**, so the rows were moved with exact-text edits.
- **Every other §5 item matched the tree:**
  - the phase, with step 0 `partial`;
  - `DigitalWorkspace` rows `Auto,Auto,Auto,*`;
  - the mode strip in row 0 with its comment, and the readiness strip in row 1;
  - `DigitalPanes` in row 3 at `*,383,*` over `Auto,*`;
  - Stop in `DigitalSendReserved`, and the two filter names;
  - `RuleOfThumb`'s old sentence;
  - the four test files;
  - `RULES_AT` at HM-DEC-161.

### Reds

- **`TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission`** is red, as §5 says. It is
  the only red in every set that ran.
- **Not run:** `TheWholeChainRunsFromOneRightClickTests`, `TheMenuIsUnderTheMouseTests`,
  `ThePsk31RecordsAppearTests` and `TheTotalMilesTests`. They are not this unit's tests
  (HM-DEC-155), so whether they are still red is not measured.
- No red turned green, and no new red appeared, in what ran.

## 2. What the owner should expect

**What is now true, on the Digital tab:**
- **Right of the CW, Digital and Voice tabs** are CQ, Stop, and then *nothing sent yet · no level
  measured yet · no contact yet* on one line. A long line, such as the license guard, wraps under
  them.
- **The mode strip** holds the chips, then `everything` and `CQ` (and with rows, the order toggle
  and `clear`), then the status at the right.
- **The panels start right under the mode strip** and run to the status bar, with one top and one
  bottom.
- **The slot clock** is at the right of the For you header.
- **The green block's last line** reads *20 m and up want daylight along the path; 40 m and down
  want dark.*
- **On a narrow window** the sparkline goes, and *heard just now* sits over the count.

**What will look wrong but is not:**
- **The send area is outside the working card**, on the tab row. CQ and Stop are the same commands,
  and Stop is never inside anything that folds.
- **The filter is not in the Decoded text header**, where the mockup draws it. See section 1,
  decision 2.
- **There is no sparkline at 1400** on the host. That is ruling 3, and your screen draws words
  narrower, so the sparkline may stay there.
- **The rule of thumb lost *Rule of thumb:* and the gray-edge clause.** That is ruling 2.
- **With the license class unknown at 1400, the tab row is 2 px taller**, because the guard
  sentence takes two lines.

**Build and tests:**
- It builds. The layout set is 90 of 91, with the one known red.
- Carry-forward is app 100 of 100 and engine 85 of 85.
- Four commits are pushed to `main`, and the report commit follows them.
- This unit created no file it could not delete.

## 3. What you should see

**The answer: yes.** With the readiness strip hidden, the three panels themselves take 503 px of
the 910 below the band pills at 1920 (0.553), and 474 px at 1400 (0.521), on the licensed fixture.
**A licensed operator's top row at 1400 is 219 px, which is 0.241 against the mockup's 0.262.**

Every built number is computed from the real main window realized headless at 1040 px tall, and
none was seen. The mockup is drawn at 810 px tall with 710 below its pills, so compare shares
rather than pixels.

### At 1920

| Region | Mockup | Before | After |
|---|---|---|---|
| Top row | 186 px, 0.262 | 190 px, 0.209 | 190 px, 0.209 |
| Rig panel against the card | one height | 190 / 190 | 190 / 190 |
| Tab row | the tabs | the tabs, 30 px | the tabs, 30 px, with CQ, Stop and the send lines 22 px tall at their right |
| Card top to panels top | about 40 px: the mode chips and a status | 117 px: card 13, mode strip 34 + 10, send area 54 + 6 beside the bar 34 + 6 | 57 px: card 13, mode strip 34 + 10 |
| Card top to panels top, readiness strip showing | none drawn | 170 px | 110 px |
| Three panels | 382 px, 0.538 | 443 px, 0.487 (390, 0.429 with the strip) | 503 px, 0.553 (450, 0.495 with the strip) |
| Waterfall / decoded / For You, wide | 612 / 416 / 394 | 734 / 378 / 735 | 734 / 378 / 735 |
| Filter chips | Decoded text header | bar over the lists | mode strip |
| Slot clock | not drawn | bar over the lists | For you header |
| Green block | 72 px, with sparkline | 62 px, with sparkline | 67 px, with sparkline |
| Card's facts | beside the map | beside | beside |

### At 1400

| Region | Mockup | Before | After |
|---|---|---|---|
| Top row, licensed | 0.262 | 273 px, 0.300 | 219 px, 0.241 |
| Green block, licensed | not given | 518 x 145, with sparkline | 518 x 91, no sparkline, *heard just now* over the count |
| Rig panel against the card | one height | 273 / 273 | 219 / 219 |
| Tab row | the tabs | 30 px | 30 px licensed, 32 px with the class unknown |
| Card top to panels top | about 40 px | 117 px (170 with the strip) | 57 px |
| Three panels, licensed | 0.538 | 360 px, 0.396 (307, 0.337 with the strip) | 474 px, 0.521 (with the strip: not measured) |
| Three panels, plain | not given | 360 px, 0.396, or 374 with the strip when the class read as General | 464 to 494 px, 0.510 to 0.543, across this unit's runs |
| Waterfall / decoded / For You, wide | not given | 474 / 378 / 475 | 474 / 378 / 475 |
| Card's facts | not given | under the map, by the rule | under the map, by the rule |
| Callsigns clipped | none | none | none |

## 4. What's blocking us

### Raised by this unit

**1. The plain test fixture asks callook.info for KC3QIS's license class, and the answer changes
what it measures.**

*No ruling wanted; a finding.*
- The view model's constructor looks up a callsign that has no class. General lands inside the
  layout passes on some windows and not others.
- That removes the green block's third line and the send area's guard sentence, 67 px at 1400,
  which is unit 337's run-order spread.
- Setting the callsign after the constructor did not stop it, so a second path exists and was not
  found.
- This unit's criteria hold in both states.
- A seam that keeps tests off the network would change the view model's start-up, which is not
  this step's work.

**2. The filter chips are in the mode strip, not in the Decoded text header where the mockup draws
them.**

*Ruling wanted only if the mockup's placement is what you want.* On the host the header is 376 px
inside. With rows, `everything`, `CQ`, the order toggle and `clear` want about 430 px before the
title, and the summary a shut panel shows would be cut (§R17). The options:
- (a) keep them in the mode strip, which does not collapse;
- (b) put only `everything` and `CQ` in the header, about 188 px, which leaves the title and
  summary about 50 px, and keep the row controls in the strip;
- (c) shorten the chips' words.

*Reasoning.* §R17 says the filter is visible on an empty list and a shut header carries its count
and sentence. Option (a) keeps both at every width, so the recommendation is (a).

*What was rejected and why.* A second row inside the Decoded text panel, above the list. It would
be inside what collapses, and the filter must stay on screen when the panel is shut.

**3. CQ and Stop sit right of the tabs, in a place the mockup draws empty.**

*Ruling wanted only if that is not where you want them.* The mockup draws no send area. Over the
waterfall it was a row all three panels paid for.

*Reasoning.* Beside the tabs it costs no height, it is outside everything that folds, and it is on
screen whenever the Digital tab is (§0.2).

*What was rejected and why.*
- The For you or waterfall panel: both collapse, and Stop may never be inside something that does.
- The status bar: every tab shares it, and it would grow.

**4. The licensed fixture's heard count read 8 and then 9 stations on two runs at 1920, against
the 6 the fixture sets.**

*No ruling wanted; a finding.* Something live replaces the fixture's count after it is set. The
top-row test asserts only that a count is shown, so nothing failed. The green block at 1920 was 62
px before task 2 and 67 after, and the top row stayed at 190.

**5. The window's name scope does not find the controls inside the neighborhood card.**

*No ruling wanted; a finding for whoever next writes code-behind there.* `FindControl` returned
nothing for `GreenZoneRegions`, and a walk of the visual tree finds it.

**6. Four expected reds were not run.**

*No ruling wanted; a finding.* `TheWholeChainRunsFromOneRightClickTests`,
`TheMenuIsUnderTheMouseTests`, `ThePsk31RecordsAppearTests` and `TheTotalMilesTests` are not this
unit's tests (HM-DEC-155). `TheStopAddedNoNewRouteToATransmission` ran and is still red.

### Where the carried items stand after unit 338

- **Unit 337 items 1 and 2: ANSWERED by the arbiter's ruling in work instruction 338.** Both are
  marked in the carried text below, with what was built.
- **Unit 337 item 3, the reds:** `TheStopAddedNoNewRouteToATransmission` is still red. The two
  `TheWholeChainRunsFromOneRightClickTests` were not run.
- **Unit 332 item 4, the license phrase at 1400:** its wording is unchanged. Its line count at 1400
  was not measured after task 2.
- **The status helper:** still refused.
- **Carried item 18, the id schemes:** untouched. See section 1 on `CPS-DEC-0163`.
- **Every other item stands as carried.** Nothing in this unit touched:
  - the radio side, a decoder, a parser or the transmit logic;
  - States or the achievements pages;
  - the demodulator, the ALC margin or PSK31 step 6.

### Asks still outstanding - carried from unit 337's section 4, per HM-DEC-139, verbatim

Headings under it are moved down one level so they sit inside this one. The words are unchanged,
except that items 1 and 2 are marked answered, as work instruction 338 §3 asks.

#### Raised by this unit

**1. At 1400 a licensed operator's top row is 273 px - 0.300 of the height below the band pills -
against the mockup's 0.262.**

**ANSWERED by the arbiter's ruling in work instruction 338** (rulings 2 and 3). Built: the rule of
thumb is option (a), the mockup's sentence, which alone gave 256 px (0.281); then the sparkline
hides where the text column would wrap and *heard just now* stands over the count, giving 219 px
(0.241). The count stays at both widths.

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

**ANSWERED by the arbiter's ruling in work instruction 338** (ruling 1: the three panels
themselves). Built: the row above the panels is gone - the send area rides the tab row, the filter
and row controls the mode strip, the slot clock the For you header - and the panels are 503 px
(0.553) at 1920 and 474 px (0.521) at 1400 on the licensed fixture, readiness strip hidden.

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

#### Where the carried items stand after unit 337

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

#### Carried from unit 336's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

##### Raised by this unit

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

##### Where the carried items stand after unit 336

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

##### Carried from unit 335's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

###### Raised by this unit

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

###### Where the carried items stand after unit 335

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

###### Carried from unit 334's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

####### Raised by this unit

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

####### Carried from unit 333's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

######## Raised by this unit

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

######## Carried from unit 332's section 4, per HM-DEC-139 - verbatim

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

######## Carried from unit 331's queue, as unit 332 carried it - verbatim

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

######## Where the carried items stand after this unit

- **Unit 332 item 1, the `Why` hovers:** not drawn on the rebuilt page before a PSK31 contact.
  Every hover on every visible control was read in task 1 and none names PSK31, so it stays
  parked.
- **Unit 332 item 6, step 5 partial or done:** answered above.
- **Carried item 1 and 10, composed timestamps:** not repeated. Every `UPDATED` in this unit is
  a `date` reading, pasted.
- **Every other item stands as carried.** Nothing in this unit touched the 1400 split, States,
  `first_answer_to_own_cq`, `Digital`, the demodulator vouch, the idle fixture, the ALC margin,
  the two id schemes, the five files or step 6.

####### Where the carried items stand after unit 334

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
