# Work instruction 342 - step 1, the category pages fitted to their mockup

Step 1 of `PHASE_PLAN.md`, **first unit on it**. Step 0 is done on evidence: the state reader read
unit 341's report and answered `done`. Every must-pass has a named green test in FT8 and PSK31 at the
widths it names. **Most of step 1 already exists.** Unit 335 built the trading cards in the last
phase, and `TheCategoryPagesAreTradingCardsTests` went 6 of 6 in unit 340. Units 339 and 340 measured
what is still missing against `assets/category-page-countries.png`:
- the earned card's map is 170 px tall and left-aligned, where the mockup spans the card;
- *cropped to the two stations* is printed, not asserted;
- two of the mockup's strings are absent;
- nothing is behind the nice-to-pass.

**This unit closes those gaps.** It does not rebuild the pages. **Four tasks, 0 to 3.**

---

## 0. The project gate

```
SHACK_FACTS.md                                          must exist
src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs     must exist
CoreHMI.sln                                             must not exist
MURC.sln                                                must not exist
root                                                    C:\Source\HamLet
```

**If any of the four is wrong, stop and say so in `output.md` section 4. Write nothing
else.** The refusal text: *This is not Hamlet. Nothing was changed.*

---

## 1. Why this unit exists

**The number: the earned card's map is a fixed 170 px tall and left-aligned.** Its drawn width was
never measured. In the mockup, the map runs the card's full inner width at about 3 to 1. Unit 340
printed one card's map frame at 175.1 by 95.3 and asserted nothing about the crop.

**What the plan, the tree and the last three reports say, read by the arbiter on 2026-09-12:**

- **Step 0 is done.** The state reader's verdict on unit 341 (`.run-unit\state-verdict.json`):
  > Every must pass criterion has a named passing test with measured numbers in both modes,
  > including a 190 px top row at 1920 and 237 px against 238.4 at 1400 on PSK31 ... though the 1400
  > PSK31 row clears by only 1.4 px and depends on the live best bet.

  That meets step 1's entry clause *step 0 done*. The other entry clause,
  `TheAchievementsPageClicksInTests` green, is checked first, in task 0.
- **`PHASE_PLAN.md` step 1 delivers R22 *as `assets/category-page-countries.png`*.** R22's content was
  judged done in the last phase (unit 335). What it has never been held to is the picture.
- **The card template** (`AchievementsWindow.axaml`, near lines 372 to 588): the map is
  `<ctl:Ft8GlobeControl Opened="True" Height="170" Plot="{Binding Globe}" />`. It sits inside a
  `Border` with `HorizontalAlignment="Left"`. The no-map list border is also `Height="170"`. Cards
  sit in a `UniformGrid Columns="2"`. The window is `Width="1040" Height="720"` in markup.
- **The picture, read by eye and approximate (not measured with a tool):**
  - **Earned card:** a map about 596 x 195 inside a card about 668 wide, spanning the card's inner
    width. Under it, the distance large on the left, with band and mode over the date beside it.
  - **Band line:** *one card per DXCC entity · 11 worked · 80 pts · Bronze · 14 to Silver*, with
    *11 of 25 to Silver* over the bar.
  - **Next card:** the wants line adds *On the CQ list they carry the green quill.* Its panel is
    headed *calling CQ right now, unworked:*.
  - **Back:** a plain link, *‹ All achievements*.
- **Unit 339's gap table** (its section 3, commit `991223a`), in size order:
  1. the map popup (nice-to-pass), which has nothing behind it;
  2. the map at the card's width;
  3. the mockup's strings. *14 to Silver* is dropped where a bar draws, which was unit 335's choice.
     The quill sentence is absent. The heading says the time the list was read, not *right now*,
     which was unit 335's choice because the window is modal.
- **Unit 340's item 5:** *a path map cropped to the two stations* is printed and not asserted.
  `achievement_category_opened` and each continent opening to its countries are asserted in
  `TheAchievementsPageClicksInTests`, not in the trading-card class.

**Every figure above comes from the reports, the markup and the picture. The arbiter ran none of it.**

```
PHASE GOAL: The screen, done right - the main window as the approved mockup
            (step 0, done); every achievements category page as trading cards
            as the approved Countries picture draws them; then what the last
            phase left; then Tim at his window says it passed.
UNIT GOAL:  The earned card's map across the card as the mockup draws it, with
            the crop to the two stations asserted, not printed; the band line
            and the next card carrying the mockup's words where they are true;
            the back control the mockup's link; and, droppable, a card's map
            opening in a popup on click - every step 1 criterion then run by
            name at 1400 and 1920.
ADVANCES:   step 1 - must-pass 2 (path map cropped to the two stations, now
            asserted; the map as the picture draws it) in task 1; must-pass 1
            and 3 (the band line's gap, the next card's words) in task 2;
            must-pass 5 and 6 re-measured on the new card in tasks 1 and 2;
            the nice-to-pass (map popup) in task 3, the drop candidate.
DRIFT:      0
```

---

## 2. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any
mismatch. **Report the mismatch; do not repair the instruction.** Mismatches go in the report even
when the work succeeds anyway.

Check:

- `PHASE_STATUS.md` names *The screen, done right*. **Step 0 may read `blocked` or `done`**, depending
  on whether the launcher has written unit 341's verdict yet. The evidence is the state reader's
  `done`. If the file still says `blocked`, report it in one line and proceed. Do not edit the
  `STEP:` lines, because they are the launcher's.
- The card template reads as §1 says: the globe `Height="170"` in a left-aligned border, and the
  no-map border `Height="170"`. The window is `Width="1040" Height="720"`.
- `TheCategoryPagesAreTradingCardsTests` holds six methods, among them
  `EveryEarnedCardIsTheContactThatEarnedIt`, `TheNextCardKnowsWhoIsCalling`,
  `EveryKindsBandCarriesCountScoreLevelAndABar` and
  `NoStringClipsAndNoCardIsWhiteAtFourteenHundredAndNineteenTwenty`.
- `TheAchievementsPageClicksInTests` holds eight, among them
  `ContinentsOpensToSevenAndEachToItsCountries` and
  `OpeningACategoryWritesTheKindAndTheCardCountAndNothingElse`.
- **The conversation card's map popup:** `Ft8ContactCard.MapIsOpen`, bound near `MainWindow.axaml`
  line 5645. Unit 339 found that `Ft8GlobeControl` takes no pointer input and that the card template
  has no `Popup`.
- **The reload's disagreements:**
  - **The reload says `CLAUDE.md` §1 holds `CPS-DEC-0163`.** Units 339 to 341 found this is the
    reload misreading the file. The id schemes are parked: report it in one line and do not resolve
    it.
  - `PROJECT_STATUS.md` `RULES_AT: HM-DEC-163` agrees with `DECISIONS.md`. Keep it unless this unit
    records a decision.
  - `PHASE_STATUS.md` and `RUN_LEDGER.md` are uncommitted, and `PHASE_OUTCOME.md` will be too if the
    launcher has appended unit 341. They are the launcher's writes. Commit whichever are modified
    **unchanged** in task 0's commit with `WORK_INSTRUCTIONS.md`, as units 336 to 341 did. Do not
    commit `.run-unit\` or `SESSION.lock`.

**Reds expected, older than this unit. Name them and do not chase them:**

- `TheOperatorCanStopItTests`: `TheStopAddedNoNewRouteToATransmission` and
  `AClickWhileTheToneIsPlayingStopsTheSoundAndNotJustTheCarrier`. **Not run.**
- `TheWholeChainRunsFromOneRightClickTests` (2), `TheMenuIsUnderTheMouseTests` (8),
  `ThePsk31RecordsAppearTests.WithNoPsk31ContactNoPsk31CardIsOnTheScreenAndNoneIsDimmed`,
  `TheTotalMilesTests.TheBadgeSaysZeroMilesOnAnEmptyLogAndTheLowestTierIsNext`. **Not run.** The
  small `Views` reds are step 2's.

**If a red turns green or a new red appears, in what you ran, say which.**

---

## 3. Rulings in force - do not re-argue

**HM-DEC-155**, transcribed: *A unit runs no test suite. It may run only the test it constructs in
that work instruction, filtered by exact name, in the foreground, with a stated timeout, and it never
backgrounds a command and polls for it.* Run the carry-forward list as `docs\carry-forward-tests.txt`'s
top comment says: two invocations, one build each. **The classes this instruction names may each run
filtered by class name:** `TheCategoryPagesAreTradingCardsTests`, `TheAchievementsPageClicksInTests`,
`BindingHealthTests` and `VoiceTests`, in one filter.

**R22, Tim, 2026-09-12 (`docs/phase-maintenance-run/PHASE_PLAN.md`, standing by `PHASE_PLAN.md` §2),
in full:**

> *"So boring."* *"Communicate visually and be appealing. Not white bread boring."* *"I like it, just
> make sure all the other sub pages are as interesting."* The shape is
> `assets/category-page-countries.png`: the category's color band and emblem across the top with its
> count, score, level and a bar to the next level; **each earned card is the contact that earned
> it** - the entity large, the callsign and grid, a map of the path cropped to the two stations as
> the conversation card draws it, the distance in large type, band, mode and date, the points; **the
> next card says what it wants and the one thing Hamlet knows that helps** - who is calling CQ right
> now from a place that would earn it, with distance, from the CQ list. Per kind: **Countries,
> States, Grids** the contact that earned it; **Continents** seven badges, each the first contact that
> opened it, the count of countries worked there since, and the unearned ones naming the continent
> and who is calling from it now; **Total Miles** a tier is a bar filling toward the next line and
> the contact that crossed it; **Bands** the first contact on that band and which band is the best
> bet now for the next; **Modes** the first contact in each mode and where the unearned mode lives
> and who is there; **Hall of Fame** the contact that earned each first, and the nearest first as
> next. **Nothing white, nothing empty.** No image assets; vector and the map the app already has.

**`PHASE_PLAN.md` §2 and §6, the lines that bite here:**

- *The arbiter stops for three things only: keying, transmit or the radio's safety; money past the
  budget; a decision that changes what the product promises the operator. On everything else it
  takes its own recommendation, marks it author's and overrulable, applies it, and continues.*
- *A must-pass is missed by a little: ship, report, `partial`, move on. Never loosen a test.*
- *A string will not fit: shorten and say which, or widen; never clip.*
- *A fact for a card is missing from the log: show what is there; no dash; a map with no grid is no
  map.*
- *Anything touches the radio, a decoder, a parser or the transmit chain: `MOVE: stop`.*
- *A package is needed: `MOVE: stop`.*
- *A file must be deleted: empty it, comment it, list it.*

**The arbiter's rulings from work instructions 338 to 341 stand as built.** Rulings 1 to 6 and 8 to
10 hold the main window, the author's, marked for Tim and overrulable at step 3. This unit touches
none of them. **Unit 341's eight decisions of its own stand too.** Among them are the one-line offer
on the drive note's row and the hidden empty upgrade row.

**The arbiter's rulings for this unit.** They are the author's, marked for Tim and overrulable at step
3.

11. **The earned card's map spans the card's inner width.** Its height is chosen by the unit and
    marked as its own. Take it near the picture's proportion (about 3 to 1) from what task 0 measures
    the control draws at that aspect, and state the number. **The no-map list border takes the same
    height**, so earned cards in a row line up.
    - The map stays the conversation card's `Ft8GlobeControl` over the same plot. **No second map
      and no image asset.**
    - *Why:* the picture draws it that way, and it is the largest visible gap unit 339 measured.
12. **"Cropped to the two stations" is asserted as a number.** Task 0 reads the rule by which the
    popup's path-fitted frame is chosen. Then:
    - on every earned card with a map, both stations lie inside the drawn frame;
    - the frame is no larger than the path's bounding box widened by the margin that rule uses, and
      the test states that margin;
    - if the rule has no fixed margin, the unit states the bound it asserts from the measured cards
      and marks it as its own.

    **A bound chosen so loose that a whole-world frame would pass it is not an assertion.** The test
    must fail on a frame opened to the whole globe. Show that once, on the test window only.
13. **The band line carries the gap to the next level where there is one**, as the picture's
    *· 14 to Silver* does, as well as the words over the bar. Where there is no next level, the line
    says so as it does now. Unit 335's choice to drop the clause is overruled by the picture.
14. **The next card's words:**
    - **The quill sentence goes on only where it is true.** Task 0 reads whether the decoded list's
      rows actually carry the green quill for a caller who would earn that kind's next card.
      - Where they do, the wants line carries the picture's *On the CQ list they carry the green
        quill.*
      - Where a different mark or none is drawn, say what is true in words of the same length, or
        leave the sentence off. Name which, per kind.

      §0.0 binds pictures as hard as sentences, and the card must not promise a mark the list does
      not draw.
    - **The panel's heading keeps the time the list was read.** It does not say *right now*. The
      window is modal and the list is read once when it opens, so *right now* would be a guess
      presented as a reading (§0.0). Unit 335's choice stands. Marked for Tim at step 3.
15. **The back control is the picture's plain link.** It keeps `BackLabel`'s words, whatever they are
    on a sub-page, and it keeps its command. Only the chip style goes.
16. **The nice-to-pass popup:** a card's map opens on click, never on a hover, as the conversation
    card's does. Reuse that mechanism rather than inventing a second one.
    - **Telemetry (R13):**
      - If the conversation card's map popup writes an event, the category card's popup writes that
        same event.
      - If it writes none, opening a map is a view and not a new stage, and no event is added.
      - Say which.
    - It never covers the back control. A click outside it closes it.

**Standing, transcribed:**

- **§0.0**: *Never present a guess as a decode… This binds pictures as hard as sentences.* Every
  appearance claim is computed, not seen, and says so once.
- **§0.2**, *transmit safety, ABSOLUTE*: *One operator action, one transmission.* Nothing here
  transmits. No test presses CQ, Stop or a transmit command.
- **§0.5**: *every panel is collapsible, and a collapsed panel still carries its summary. Collapsing
  hides detail, never information.*
- **§0.6**: every ink clears 4.5:1 against its fill, and color is never the only carrier. The band
  line's new clause is on the band's own computed ink.
- **R12**: *a session fixes its own tests and never asks the owner to approve it.*
- **R13**: telemetry on every new stage. Ruling 16 says when a map popup is one.
- **R14**: *a test exists to prove an exit criterion.* Extend the tests the criteria need and add no
  others.
- **R19**: American spelling.
- **R23**: States from `STATE`, rank names from the points file. Untouched here.

---

## 4. Status cadence

Write status before every `dotnet` command and after every task. `tools/status.sh` has been refused
as *requires approval* in seven units. Try it once. If it is refused, take a `date` reading and paste
it whole. **Never compose a time.** The script hard-codes a stale `RULES_AT`, so do not let it
overwrite `HM-DEC-163`. The watchdog kills a session only when its process tree has used no CPU for
ten minutes.

---

## 5. The tasks

### Task 0 - the trace: the page against its picture, before anything is built

Measure before anything is built. **Say what you find rather than confirming §1.**

1. **Commit the launcher's root files unchanged** (§2) with `WORK_INSTRUCTIONS.md` and a patch bump,
   as `chore(unit342): step 1, the category pages fitted to their mockup - the trace before a card is
   changed`. Set `PHASE_STATUS.md`'s `WORK_INSTRUCTION:` line to `342 - step 1, the category pages
   fitted to their mockup`. Run the carry-forward list, status first.
2. **Step 1's entry check:** run `TheAchievementsPageClicksInTests` by class, then
   `TheCategoryPagesAreTradingCardsTests` by class. Give each as *n of n*, with any failure line.
   **If the entry check is red, report it and stop at task 0.** Step 1's entry is not met.
3. **Add `Unit342TraceTheCountriesPageAgainstItsMockup` to `TheCategoryPagesAreTradingCardsTests`.** It
   asserts nothing and presses nothing. It realizes the Countries page, and one no-map card, at
   1400 and 1920 on the dialog's width as the class already sets it. It prints:
   - **each earned card:** its outer and inner width; the globe's drawn box and x offset inside the
     card; the two stations' pixel positions inside the map; the frame the control opened and the
     path's bounding box, in the control's own units; the distance text's size;
   - **the aspect:** the globe drawn at the card's inner width with heights of 170, the picture's
     proportion and one between, set on the test window only and never in markup. Print each
     card's height and whether the page still fits without clipping;
   - **the crop rule:** where the path-fitted frame is computed (file and method), and the margin
     it uses, read from the source;
   - **the band line** on every kind, and whether the gap clause is on it;
   - **the next card** on every kind: its wants line, its heading, and, read from the decoded list's
     code, which mark a caller who would earn that kind's next card carries there;
   - **the back control:** its words on the top page and on a continent's sub-page;
   - **the conversation card's map popup:** the property, the control, what opens and closes it, and
     whether it writes an event.
4. **Answer from the numbers:**
   - What height does ruling 11 take, and how tall does the tallest page grow at each width?
   - What bound does ruling 12 assert, and does a whole-globe frame fail it?
   - On which kinds is the quill sentence true?
   - Can ruling 16 reuse the conversation card's popup as it stands?

Report the numbers in section 1 before task 1 starts.

**Drop candidate:** none.

### Task 1 - ruling 11 built, ruling 12 asserted: criterion 2 held to the picture

1. **Build ruling 11** in `AchievementsWindow.axaml`: the map across the card's inner width at the
   height task 0 chose, and the no-map border at the same height. **Mark the height as the unit's
   own** in the template's comment, with the numbers.
2. **Extend `EveryEarnedCardIsTheContactThatEarnedIt`** (R12), at 1400 and 1920. On every earned
   card with a map, assert:
   - the map's drawn width equals the card's inner width within 1 px;
   - its height is the stated height;
   - ruling 12's crop: both stations inside the frame, and the frame within the stated bound.

   Keep every assertion it already makes.
3. **Watched red.** Against the tree before the build, the width assertion should fail on the 170 px
   left-aligned map. Give the failure line. **Show the crop assertion failing on a whole-globe frame
   set on the test window only.** Then build. **Do not break the view to watch a test fail.**
4. **Run the one filter:** `TheCategoryPagesAreTradingCardsTests`, `TheAchievementsPageClicksInTests`,
   `BindingHealthTests`, `VoiceTests`. Give each class as *n of n*.
   `NoStringClipsAndNoCardIsWhiteAtFourteenHundredAndNineteenTwenty` must stay green on the taller
   cards. **If it goes red, fit by the height, never by loosening it.** Run the carry-forward list,
   status first.

**Drop candidate:** none. This is the criterion the picture most visibly misses.

### Task 2 - rulings 13, 14 and 15: the picture's words where they are true

1. **Build ruling 13** (the band line's gap clause), **ruling 14** (the quill sentence per kind as
   task 0 found it; the heading unchanged) and **ruling 15** (the back control as a link, its words
   and command unchanged). The strings go in the view model where they are built now. **Name every
   string added or changed, per kind.**
2. **Extend `EveryKindsBandCarriesCountScoreLevelAndABar`** so it asserts the clause on every kind
   with a next level, and its absence where there is none. **Extend `TheNextCardKnowsWhoIsCalling`**
   so it asserts the wants line per kind as ruling 14 decided. Watch both red before the build. The
   back link needs no new test: `ClickingABadgeReplacesThePageAndTheBackControlReturns` holds its
   command. Say whether it stayed green.
3. **Run the one filter again**, then the carry-forward list, status first. The no-clip test must
   hold on the longer band line at 1400. If it does not, shorten per §6 and name the words.

**Drop candidate: ruling 15, the back link.** It is style only, and no criterion names it.

### Task 3 - ruling 16: a card's map opens in its popup (nice-to-pass)

1. **Build ruling 16.** Clicking an earned card's map opens the path in a popup, as the conversation
   card's does, with the mechanism task 0 found reusable. It opens on click, never on a hover. A click
   outside closes it. Telemetry as ruling 16 says.
2. **Extend `EveryEarnedCardIsTheContactThatEarnedIt`**, or add one method if the popup has no
   criterion-bearing home there (R14: this is the nice-to-pass's test, and the report says which).
   Assert:
   - the popup is closed at start;
   - a click on a card's map opens it, holding that card's plot;
   - a click outside closes it;
   - it writes only the event ruling 16 allows.

   Watch it red against the tree, where the map takes no click.
3. **Run the one filter again**, then the carry-forward list, status first.

**Drop candidate: this whole task.** It is the nice-to-pass. If time runs short, drop it and say so.
Criteria 1 to 7 do not depend on it.

---

## 6. Parked - do not touch, do not raise

- **The main window, all of it.** Step 0 is done:
  - the top row, the one-line power offer and its popup, the green block and its empty row;
  - the 1400 PSK31 row's 1.4 px and the live best bet (unit 341 item 1, unit 339 item 2);
  - the strayed-frequency fixture (unit 341 item 2);
  - `ThePowerIsOfferedTests` proving less than its name (unit 341 item 4);
  - the line's 4.61:1 ink (unit 341 item 5), and where the offer's popup lands (unit 341 item 6).

  Do not run `TheTopRowTests` or `TheWorkingPanelsTests`. If the carry-forward list turns one of
  these red, report it and do not chase it.
- **Step 2's items:** the States wording, including the States next card's *Hamlet cannot tell a
  caller's state* (unit 335 item 1, Tim's); the Modes test; the undeletable files; the points file's
  comment block; the small `Views` reds.
- **The achievements window's own size** (`1040 x 720`, not sized from the main window; unit 332
  item 3). 1400 and 1920 are reached as the tests already reach them.
- **`TheOperatorCanStopItTests`, all of it.** Transmit-side.
- **The live license lookup and the live heard count** (unit 338 items 1 and 4).
- **The two id schemes, including the reload's `CPS-DEC-0163` misreading.** Also: real flags on
  country cards; PSK31 step 6; real PSK31 audio; the ALC margin; the map bitmap's license; the status
  script's stale `RULES_AT`; the demodulator; the `Why` hovers. All are Tim's or the harness's,
  carried in `PHASE_PLAN.md` §7.
- **Anything touching the radio, a decoder, a parser or the transmit chain. Any package.**

## 7. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.** (HM-DEC-155.)
- **No image assets, no second map control, no new map bitmap.** The card's map is the conversation
  card's `Ft8GlobeControl` (R22: *vector and the map the app already has*).
- **Do not put the quill sentence on a kind whose callers do not carry the quill.** Do not write
  *right now* over a list read once (§0.0).
- **Do not loosen a threshold or tolerance to make a test pass, and do not break the view to watch one
  fail.** A crop bound a whole-globe frame would pass is a loosened test. A small miss ships as
  `partial` with its number.
- **Do not change what an earned card is earned by, what scores, or any points.** Those are the log's
  and the points file's, and step 2's.
- `CLAUDE.md` §12.6 covers the rest. **No package. Report mismatches; repair nothing. Write
  American.**
- **The tool facts, from units 332 to 341:**
  - apostrophes inside quoted heredocs break, and an apostrophe in an argument breaks the `.bat`
    tools;
  - doubled backslashes collapse;
  - `;`, `rm`, `git stash`, `sort` in a pipe, `git check-ignore`, `sed -E`, a shell loop variable,
    `pwd -W`, `grep -v` in a pipe, a redirect into `testresults\`, a redirect or `tee -a` into
    `output.md`, a `date` flag inside a compound command and a command substitution are refused;
  - Python cannot run;
  - a multi-line commit message needs more than one `-m`;
  - use exact-text edits for markup;
  - the validator runs as `dotnet build tools/arbiter/validate-output.proj -p:Report=output.md`,
    because the `.bat` spelling is mangled by Git Bash;
  - a `public const` cannot be bound; bind a property built from it.

## 8. Committing and pushing

Commit and push each task on its own, on `main`. The report and status file follow in their own
commit. The report names the commits and says whether each push succeeded. **A refused push is
reported as refused, with the reason.**

---

## 9. Reporting

Write `output.md` at the root, then stop. Do not start the next unit. **Every exit writes it**:
finished, blocked, failed or stopped early. Canonical headings: `## 1. What Claude did`,
`## 2. What the owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Validate
it with `dotnet build tools/arbiter/validate-output.proj -p:Report=output.md`.

**The ordering block comes first.** `validate-output` refuses a report without it.

```
READ IN THIS ORDER.

A. The phase goal - the screen, done right. Step 0 done (state reader on
   unit 341). Step 1 <state as the evidence stands>: <how many of its seven
   must-pass carry a green named test at 1400 AND 1920 after this unit,
   and which do not>; nice-to-pass <built | dropped>. Steps 2 and 3 not
   started.
B. Step 1 and its exit criteria, each with the test that proves it, its
   result and the widths it realized:
   entry: TheAchievementsPageClicksInTests <n of n>, run first
   1. color band with count, score, level and a bar - <test, result; the
      gap clause on <n> kinds, absent on <kinds with no next level>>
   2. earned card is the contact, with a path map CROPPED TO THE TWO
      STATIONS - <test, result; map <w x h> against card inner <w> at 1400
      and 1920, was 170 tall left-aligned; the crop bound asserted, and
      whether a whole-globe frame failed it>
   3. next card names its want and its callers with distance, or no one
      calling - <test, result; the quill sentence on <kinds>, off or
      reworded on <kinds>, and why>
   4. all eight kinds per R22, Continents to seven and each to its
      countries - <tests, results>
   5. no string clips or wraps a word at 1400 and 1920 - <test, result; the
      tallest page's height at each width>
   6. no card is a white rectangle - <test, result; white cards of total>
   7. achievement_category_opened carries the kind and the card count -
      <test, result>
   nice-to-pass: a card's map opens in the popup on click - <test, result,
      or dropped; the event it writes, or none, and why>
C. The report last. Section 4 raises N items on top of the carried queue;
   say which, if any, stands in the way of a criterion in B - in
   particular whether the taller card kept criterion 5 at 1400, and
   whether any kind's quill sentence was withheld as untrue.
```

**Every line specific to this unit.** If a criterion was not measured, say *not measured*. Do not
fill the shape.

```
UNIT:       342 - <complete|stopped> at task N of 4 - <date time, read from the clock>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no> - <whether the card's map now spans the card with the
            crop asserted, whether the band line and next card carry the
            picture's words where true, and whether the popup was built>
NUMBER:     earned card map, Countries page: 1400 <was w x 170> -> <w x h>;
            1920 <was w x 170> -> <w x h>
DRIFT:      0
```

**Section 3 leads with the answer:** does every step 1 exit criterion hold with a named green test at
1400 and 1920? If not, which one, at which width, and by how much? Then describe one earned card and
the Countries next card in words, top to bottom, with the numbers before and after at both widths.
Then the band line on each of the eight kinds, as it reads now. Then the step 1 table: test,
criterion, widths, kinds, result, numbers. **Every appearance claim is computed, not seen. Say so
once.**

**Section 4:** unit 341's section 4 verbatim, per HM-DEC-139, including the queue it carries. In the
carried text, mark these two items:
- unit 340's item 5 (step 1 weakly held): `TAKEN UP by work instruction 342 tasks 0 and 1`, with the
  crop bound asserted;
- unit 339's item 5 (the nice-to-pass has nothing behind it): `TAKEN UP by work instruction 342 task
  3`, with what was built or that it was dropped.

Then anything this unit raises. **A ruling is wanted only where Tim must decide; everything else is a
finding and says so.** Ruling 14's kept read time and ruling 8's extra click are already marked for
Tim at step 3. Do not raise them again unless the build found something that changes them.

---

```
ARBITER-DECISION
STEP: 1
APPROACH: fit the built category pages to assets/category-page-countries.png - the earned card map across the card inner width with the crop to the two stations asserted as a bound a whole-globe frame fails; the band line gap clause and the next card quill sentence where true; the back control as a link; a card map opening in a click popup as the drop candidate; measured at 1400 and 1920
MOVE: continue
WHY: The state reader found step 0 done on unit 341's evidence, so the plan moves to step 1, whose content unit 335 built in the last phase but which was never held to the approved picture; units 339 and 340 measured the gaps, and the loop test found nothing like this approach (unit 335 built the pages, this fits them to the picture).
STATE: not started
DECIDED: rulings 11 to 16 - the card map spans the card inner width at a height the unit chooses near the picture's 3 to 1, with the no-map border matched; the crop asserted as a stated bound that fails on a whole-globe frame; the band line carries the gap to the next level (overrules unit 335's choice); the quill sentence only on kinds whose callers carry the quill; the read time kept over "right now" (keeps unit 335's choice, section 0.0); the back control a plain link with its words and command unchanged; the map popup on click reusing the conversation card's mechanism, with that popup's event or none - all the author's, marked for Tim at step 3, overrulable
LICENCE: PHASE_PLAN.md sections 2, 4 (step 1 delivers R22 as the picture; entry step 0 done) and 6; docs/phase-maintenance-run/PHASE_PLAN.md R22; assets/category-page-countries.png; .run-unit/state-verdict.json (step 0 done on unit 341); unit 339 output.md section 3 (the Countries page against its mockup, three largest gaps) and section 4 item 5; unit 340 output.md section 4 item 5; CLAUDE.md 0.0, 0.5, 0.6; psk31 R12, R13, R14, R19; HM-DEC-155
ACCOMPLISHED: each achievements category page looks like the Countries picture Tim approved - the map of the contact's path across the card and cropped to the two stations, the level's gap on the band, the next card's words where they are true - with every step 1 criterion proven by a named test at both of Tim's widths, and a card's map one click from its popup if the night allows
ADVANCES: step 1 - must-pass 2 (map across the card, crop to the two stations asserted) in task 1; must-pass 1 and 3 (the band's gap clause, the next card's words) in task 2; must-pass 5 and 6 re-measured on the changed cards in tasks 1 and 2; the nice-to-pass map popup in task 3, the drop candidate
END-ARBITER-DECISION
```
