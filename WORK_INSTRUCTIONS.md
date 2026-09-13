# Work instruction 343 - step 1, proved on the window

Step 1 of `PHASE_PLAN.md`, **second unit on it**. Unit 342 built what its instruction asked:
- the map across the card;
- the crop asserted;
- the gap clause and the quill sentence;
- the back link;
- the map popup.

**Then it ended without a report.** Its last status, at `9214b2a`, reads *EXECUTING, task 3 of 4 ...
writing the report*, and no `docs(unit342)` commit exists. So the state reader judged step 1 against
the `output.md` in the tree, which was unit 341's step 0 report. It answered `partial`:

> Criteria 2, 5, 6 and 7 have green tests with measured numbers, but criterion 4 is checked only on
> the view model for five kinds and for Continents, criteria 1 and 3 are not checked at 1400 or 1920,
> and the detailed sections of the report describe the step 0 power offer work rather than step 1, so
> nothing there backs the summary.

**This unit builds no new look.** It proves step 1 on the drawn window at Tim's two widths, and it
writes the step 1 report that unit 342 never wrote. **Three tasks, 0 to 2.**

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

**The number: 4 of step 1's 7 must-pass have a named green test on the drawn window. Criteria 2, 5, 6
and 7 do, by the state reader on unit 342. Criteria 1, 3 and 4 do not.** Criterion 7, the telemetry
event, has no width.

**Read from the test source by the arbiter on 2026-09-13.** None of it was run.

| Criterion | Method | Drawn or view model | Widths |
|---|---|---|---|
| 1 color band | `EveryKindsBandCarriesCountScoreLevelAndABar` | drawn | **1040 only** (`Realized(..., 1040)`, line 50) |
| 2 earned card, map, crop | `EveryEarnedCardIsTheContactThatEarnedIt` | drawn | 1400 and 1920 (line 289) |
| 3 next card, callers | `TheNextCardKnowsWhoIsCalling` | **view model**. Drawn only for the Countries quill at 1400 (line 779) and the Countries callers at 1040 (line 802) | **not 1400 or 1920 per kind** |
| 4 eight kinds per R22 | `TheOtherFiveKindsEachDrawTheirOwnCards` | **view model** (`Screen(...)` returns `AchievementsViewModel`, line 1614) | none |
| 4 Continents to seven, each to its countries | `TheAchievementsPageClicksInTests.ContinentsOpensToSevenAndEachToItsCountries` | **view model** | none |
| 5, 6 no clip, no white card | `NoStringClipsAndNoCardIsWhiteAtFourteenHundredAndNineteenTwenty` | drawn | 1400 and 1920, eight kinds plus `continent-EU` and `continent-OC` |
| 7 `achievement_category_opened` | `TheAchievementsPageClicksInTests.OpeningACategoryWritesTheKindAndTheCardCountAndNothingElse` | view model, telemetry | n/a |
| nice-to-pass, map popup | `ACardsMapOpensInItsPopupOnAClickAndAClickOutsideClosesIt` | drawn, real clicks | 1400 and 1920 by its commit |

**`PHASE_PLAN.md` §1: *a step's exit is what is on the screen, asserted by computation*.** A view model
that holds the right words does not show that the page draws them. Step 0 closed the same way: unit
339 moved it from built to proved by asserting on the realized window at both widths.

```
PHASE GOAL: The screen, done right - the main window as the approved mockup
            (step 0, done on the state reader's verdict on unit 341); every
            achievements category page as trading cards as the approved
            Countries picture draws them, all eight kinds (step 1); then what
            the last phase left (step 2); then Tim at his window says it
            passed (step 3).
UNIT GOAL:  Every step 1 must-pass held by a named green test on the drawn
            category pages at 1400 and 1920 - the band (1), the next card and
            its callers (3), all eight kinds with Continents to seven and each
            to its countries (4) - and unit 342's unreported build re-measured,
            so the report in the tree is step 1's own.
ADVANCES:   step 1 - must-pass 1 and 3 in task 1; must-pass 4 in task 2, whose
            1920 half is the drop candidate; must-pass 2, 5, 6, 7 and the
            nice-to-pass re-run with their numbers in task 0.
DRIFT:      0
```

---

## 2. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any mismatch.
**Report the mismatch; do not repair the instruction.** Mismatches go in the report even when the
work succeeds anyway.

Check:

- **The table in §1**, method by method: whether each assertion runs on a realized window or the view
  model, and at which widths. Also find out whether Grids' earned cards are asserted drawn anywhere.
  The arbiter did not find it.
- **Unit 342 ended without a report.** Its commits are `ba5179d`, `6586020`, `40316ea`, `18f5b17` and
  `9214b2a`, and none is `docs(unit342)`. `PROJECT_STATUS.md` at `9214b2a` reads `EXECUTING`,
  `TASK: 3 of 4`, *writing the report*. Its commit messages give:
  - the earned card map 632x231 at 1400 and 892x231 at 1920, which was 564x170 and 796x170
    left-aligned;
  - the gap clause on seven kinds, with the meaning dropped past 60 characters on Grids and Total
    Miles;
  - the quill sentence green on Countries and reached continents, ringed on unreached ones, and none
    on Grids and States;
  - the popup writing no event;
  - *four classes 23 of 23*.

  **These are commit messages, not measurements.** Task 0 measures them again.
- **The launcher's files disagree with themselves.** Report each in one line. Do not edit any of them:
  they are the launcher's.
  - `PHASE_OUTCOME.md` has **no unit 341 entry**, and its header reads step 0 `in progress`.
  - `PHASE_STATUS.md` reads `CURRENT_STEP: 0`.
  - Unit 337's carried-repair entry is filed partly under *STEP 0* and partly under *STEP 1*.

  Step 0 is taken as done on the state reader's verdict on unit 341, quoted in work instruction 342
  §1.
- **`output.md` is deleted in the working tree and not committed.** This unit writes a new one at the
  end. Do not restore the old one.
- **`RULES_AT`.** `PROJECT_STATUS.md` at HEAD reads `HM-DEC-161 (2026-09-11)`, and the highest id in
  `DECISIONS.md` is `HM-DEC-163`. Unit 337's status writes brought the stale line back. Write
  `HM-DEC-163 (2026-09-12)` unless this unit records a decision.
- **The reload says `CLAUDE.md` §1 holds `CPS-DEC-0163`.** Units 339 to 341 found that this is the
  reload misreading the file. It is parked with the id schemes. One line.
- **The tool facts in §7 disagree with unit 337's report.** Unit 337 reports that Python ran,
  `tools/status.sh` was not refused, and `&&` was not refused. Say which held for you.

**Reds expected, older than this unit. Name them and do not chase them:**

- `TheOperatorCanStopItTests`: `TheStopAddedNoNewRouteToATransmission` and
  `AClickWhileTheToneIsPlayingStopsTheSoundAndNotJustTheCarrier`. **Not run.**
- `TheWholeChainRunsFromOneRightClickTests` (2), `TheMenuIsUnderTheMouseTests` (8),
  `ThePsk31RecordsAppearTests.WithNoPsk31ContactNoPsk31CardIsOnTheScreenAndNoneIsDimmed` and
  `TheTotalMilesTests.TheBadgeSaysZeroMilesOnAnEmptyLogAndTheLowestTierIsNext`. **Not run.** They are
  step 2's.

**If a red turns green or a new red appears in what you ran, say which.**

---

## 3. Rulings in force - do not re-argue

**HM-DEC-155**, transcribed: *A unit runs no test suite. It may run only the test it constructs in
that work instruction, filtered by exact name, in the foreground, with a stated timeout, and it never
backgrounds a command and polls for it.*
- Run the carry-forward list as the top comment of `docs\carry-forward-tests.txt` says: two
  invocations, one build each, with status written immediately before each.
- **The classes this instruction names may run filtered by class name, in one filter:**
  `TheCategoryPagesAreTradingCardsTests`, `TheAchievementsPageClicksInTests`, `BindingHealthTests` and
  `VoiceTests`.

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

**`PHASE_PLAN.md` §1, §2 and §6, the lines that bite here:**

- *Screen only. A step's exit is what is on the screen, asserted by computation and described in
  words, then Tim's eyes. Every appearance claim is computed, not seen, and says so.*
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

**The arbiter's rulings 1 to 10** (work instructions 338 to 341, the main window) **stand as built**.
This unit touches none of them.

**The arbiter's rulings 11 to 16** (work instruction 342) **stand as unit 342 built them**. They are
the author's, marked for Tim at step 3, and overrulable:

11. The earned card's map spans the card's inner width, at a height unit 342 chose (231 px by its
    commit). The no-map list border takes the same height. It is the conversation card's
    `Ft8GlobeControl` over the same plot, with no second map and no image asset.
12. *Cropped to the two stations* is asserted as a number: both stations lie inside the drawn frame,
    and the frame lies within a stated bound that a whole-globe frame fails.
13. The band line carries the gap to the next level where there is one, and says so where there is
    none. This overruled unit 335's choice.
14. The quill sentence appears only where the decoded list actually draws that mark for such a
    caller. The callers panel's heading keeps the time the list was read and never says *right now*,
    because the window is modal and the list is read once (§0.0).
15. The back control is a plain link, with `BackLabel`'s words and its command unchanged.
16. A card's map opens in a popup on a click, never on a hover, reusing the conversation card's
    mechanism. It writes that popup's event, or none if that popup writes none. A click outside
    closes it.

**The arbiter's rulings for this unit.** They are the author's, marked for Tim, and overrulable:

17. **Criteria 1, 3 and 4 count as met only by assertions on the realized category page, at 1400 and
    1920.** The view-model assertions stay, and every assertion already made is kept (R12). But they
    are not the evidence.
    - *Why:* `PHASE_PLAN.md` §1, and the state reader already refused the view model for criterion 4.
    - *Rejected:* measuring at the window's markup width of 1040. That is not one of Tim's widths, and
      the window's own size is parked.
18. **Unit 342's build stands and is not redone.** Task 0 re-measures it, and this unit's report
    carries its numbers as step 1's evidence.
    - *Why:* its work is committed and its tests reportedly green. What is missing is the proof on the
      page and a report.
    - *Rejected:* re-issuing work instruction 342. That would repeat an approach already tried, which
      is a loop.
19. **Watching red, for an assertion extended to a width where it may already hold.** Show the new
    assertion fail once against a deliberately wrong expectation, set in the test on the test window
    only. For example, a caller line the view model does not hold, or a band read from a kind it is
    not. Give the failure line.
    - **Never break markup or a view to watch a test fail.** Where the new assertion is red against
      the tree as it is, that is the watched red, and it is a finding.
    - *Rejected:* skipping the watch because the page is already right. A test never seen failing may
      be asserting nothing.
20. **Where the view model holds a fact the drawn page does not show, or shows differently, fixing it
    is this step's work.** Fix it in `AchievementsWindow.axaml` or where the string is built, with the
    new assertion watched red first, and name the fix per kind.
    - A fix never changes what a card is earned by, what scores, any points, or the words rulings 11
      to 16 set.
    - A string that will not fit is shortened and named (§6).
    - A miss that cannot be fixed that way ships as `partial` with its number.

**Standing, transcribed:**

- **§0.0**: *Never present a guess as a decode… This binds pictures as hard as sentences.* Every
  appearance claim is computed, not seen, and says so once.
- **§0.2**, *transmit safety, ABSOLUTE*: *One operator action, one transmission.* Nothing here
  transmits. No test presses CQ, Stop or a transmit command.
- **§0.5**: *every panel is collapsible, and a collapsed panel still carries its summary. Collapsing
  hides detail, never information.*
- **§0.6**: every ink clears 4.5:1 against its fill, and color is never the only carrier.
- **R12**: *a session fixes its own tests and never asks the owner to approve it.*
- **R13**: telemetry on every new stage. This unit adds no stage.
- **R14**: *a test exists to prove an exit criterion.* Extend the tests the criteria need and add no
  others, apart from the trace.
- **R19**: American spelling.
- **R23**: States from `STATE`, rank names from the points file. Untouched here.
- **HM-DEC-139**: open asks are carried verbatim until answered.

---

## 4. Status cadence

Write status **before every `dotnet` command, after every commit and after every task**. Use
`tools/status.sh`: it reads the clock, and unit 337 reports that it ran. If it is refused, take a
`date` reading and paste it whole. **Never compose a time.**

The script hard-codes `RULES_AT: HM-DEC-161`. After it writes, set that line back to `HM-DEC-163
(2026-09-12)` (§2).

The watchdog kills a session only when its process tree has used no CPU for ten minutes.

---

## 5. The tasks

### Task 0 - the trace: what step 1 asserts, and what the page draws, before a test is changed

Measure before anything is built. **Say what you find rather than confirming §1.**

1. **Commit `PHASE_STATUS.md` with `WORK_INSTRUCTIONS.md` and a patch bump.** Change only
   `PHASE_STATUS.md`'s `WORK_INSTRUCTION:` line, to `343 - step 1, proved on the window`. Do not touch
   its `STEP:` lines, `CURRENT_STEP` or `HEARTBEAT`, and do not commit `.run-unit\` or `SESSION.lock`.
   The commit message is `chore(unit343): step 1, proved on the window - the trace before a test is
   changed`. Run the carry-forward list, status first, and give both invocations as *n of n*.
2. **Step 1's entry check.** Run `TheAchievementsPageClicksInTests` by class, then
   `TheCategoryPagesAreTradingCardsTests` by class. Give each as *n of n*, with any failure line.
   **If the entry check is red, report it and stop at task 0.**
3. **Add `Unit343TraceStepOneOnTheWindow` to `TheCategoryPagesAreTradingCardsTests`.** It asserts
   nothing and presses nothing that transmits. At 1400 and 1920 it realizes the pages on the fixture
   `NoStringClipsAndNoCardIsWhiteAtFourteenHundredAndNineteenTwenty` uses: `TwelveContacts`,
   `Calling()`, and the best bet. It covers each of the eight kinds, the Continents page's seven
   sub-badges, and each of the seven continent pages. For each, it prints:
   - **the band:** every drawn text, whether a bar is drawn with width, and the view model's
     `BandLine`, `LevelBarLine` and `GapLine` beside them;
   - **every drawn card, top to bottom:** its visible text, whether it has a map, a bar or a list,
     and its map's drawn size;
   - **the next card:** drawn beside the view model's words - the wants line, quill line, callers
     heading, each caller's place and call line, and the no-caller line. **Print any fact the view
     model holds and the page does not draw as `NOT DRAWN`**;
   - **on Continents:** the seven sub-badges drawn. For each, what its page draws: name, back link,
     how many earned cards against the continent's worked entities, and the next card's title;
   - **the tallest page's height** at each width.
4. **Re-run unit 342's numbers.** Run the popup test and the crop assertion by method name, in one
   filter, and print:
   - the Countries map size at both widths;
   - the crop bound and the margin it uses;
   - the kinds with the gap clause;
   - the quill line per kind;
   - the event the popup writes.
5. **Answer from the numbers, in section 1, before task 1 starts:**
   - Does §1's table hold, row by row?
   - Which clause of criteria 1, 3 and 4 is already drawn at both widths?
   - Which facts are `NOT DRAWN`, per kind and width?
   - Do unit 342's commit-message numbers match what you measured?

**Drop candidate:** none. Without it the report has nothing of unit 342's to stand on.

### Task 1 - criteria 1 and 3 on the drawn page at 1400 and 1920

1. **Extend `EveryKindsBandCarriesCountScoreLevelAndABar`** (R12). Keep the 1040 run and its pinned
   strings. **Add the same assertions on the drawn band at 1400 and 1920**, for all eight kinds and
   all seven continent pages:
   - `Standing`, `ScoreLine` and `LevelName` are drawn;
   - where a next level exists: one bar with width, `LevelBarLine` drawn, and `BandLine` drawn ending
     in the gap;
   - where none exists: no bar, and `NoNextLevelLine` drawn.
2. **Extend `TheNextCardKnowsWhoIsCalling`.** At 1400 and 1920, on every kind and every continent
   page, the next card as drawn must carry what the view model holds for it:
   - the wants line, and the quill line where the view model has one and on no other kind;
   - the callers heading with its read time;
   - each caller's place and call line with its distance, or the no-caller line;
   - on Bands, the best bet; on Modes, where each unworked mode lives; on Hall of Fame, the next
     first and its bar.
3. **Watched red** per ruling 19, once for each method, with the failure line. Anything task 0 found
   `NOT DRAWN` goes by ruling 20.
4. **Run the one filter:** `TheCategoryPagesAreTradingCardsTests`, `TheAchievementsPageClicksInTests`,
   `BindingHealthTests` and `VoiceTests`. Give each class as *n of n*. Then run the carry-forward
   list, status first. Commit and push.

**Drop candidate:** none. Criteria 1 and 3 are the two the state reader named unmeasured.

### Task 2 - criterion 4 on the drawn page: all eight kinds, and Continents to seven and each to its countries

1. **Extend `TheOtherFiveKindsEachDrawTheirOwnCards`.** Keep its view-model half. Add a drawn half at
   1400 and 1920 on the same fixtures, asserting per kind on the realized page:
   - **Continents:** seven sub-badges drawn. Each opened one draws its first contact's callsign and
     *n countries worked there*. Each unopened one draws the continent's name and its callers (ZL1ABC
     under Oceania's name).
   - **Total Miles:** the tier bar is drawn with width and its tier line. On the nine-contact log,
     the crossing card is drawn with its date and a map with width.
   - **Bands:** the 20 m earned card is drawn with its first contact's callsign and a map with width.
     The next card draws the best bet first.
   - **Modes:** the FT8 card is drawn with `LA1ZZZ`. The next card draws CW, FT4 and PSK31, with
     PSK31's frequency and band.
   - **Hall of Fame:** each earned first is drawn with its callsign and a map with width. *Over 10,000
     miles* is drawn with its bar.
   - **Countries, States and Grids:** the contact that earned each card is drawn - entity, callsign,
     grid, distance, band, mode, date and points. Countries and States are already covered if task 0
     confirms `EveryEarnedCardIsTheContactThatEarnedIt` and `StatesCountWhatTheLogsStateFieldSays`
     assert that on the drawn page. **Add Grids wherever task 0 found it unasserted**, in
     `EveryEarnedCardIsTheContactThatEarnedIt`.
2. **Extend `ContinentsOpensToSevenAndEachToItsCountries`.** Keep its view-model assertions. On a
   realized window at 1400 and 1920:
   - click each of the seven drawn sub-badges, as `ClickingABadgeReplacesThePageAndTheBackControlReturns`
     clicks;
   - assert the page it opens draws the continent's name, the `‹ Continents` link, and one earned
     card per entity the log worked on that continent, with at most one next card;
   - click the link and assert the seven are drawn again.
3. **Watched red** per ruling 19. Anything `NOT DRAWN` goes by ruling 20.
4. **Run the one filter again**, then the carry-forward list, status first. Commit and push.

**Drop candidate: the 1920 half of this task, whole.** If time runs short, assert steps 1 and 2 at 1400
only, the narrower width, where cards stack and strings are tightest. Drop 1920 from both methods,
not from one. Say so in the report, and report criterion 4 as drawn at 1400 only.

---

## 6. Parked - do not touch, do not raise

- **The main window, all of it.** Step 0 is done:
  - the top row, the one-line power offer and its popup, the green block;
  - the 1400 PSK31 row's 1.4 px and the live best bet;
  - the strayed-frequency fixture, `ThePowerIsOfferedTests`, the line's 4.61:1 ink and where the
    offer's popup lands (unit 341 items 1 to 6).

  Do not run `TheTopRowTests` or `TheWorkingPanelsTests`. If the carry-forward list turns one red,
  report it once and do not chase it.
- **Step 2's items, because they are the next step's:**
  - the States wording, including the States next card's *Hamlet cannot tell a caller's state* (unit
    335 item 1);
  - the Modes test, and with it the PSK31 next-card collision on Hall of Fame and Modes (unit 333
    item 1, unit 336 item 2). Criterion 4 is measured against those cards as the tree draws them;
  - the undeletable files, the points file's comment block, the small `Views` reds and
    `TheTotalMilesTests`.
- **The achievements window's own size** (`1040 x 720`, not sized from the main window; unit 332 item
  3). 1400 and 1920 are reached as the tests already reach them.
- **The PSK31 findings of unit 337's carried repair**, which are outside this phase:
  - carrier 19's 262 characters need audio;
  - `charactersEmitted` counts the shown text;
  - `Psk31Listener.DroppedCharacters` has no consumer.
- **`TheOperatorCanStopItTests`, all of it.** Transmit-side.
- **Carried in `PHASE_PLAN.md` §7 and belonging to Tim or the harness:**
  - the live license lookup and the live heard count;
  - the two id schemes, including `CPS-DEC-0163`;
  - real flags on country cards, PSK31 step 6, real PSK31 audio, the ALC margin and the map bitmap's
    license;
  - the demodulator, the `Why` hovers, and the launcher's `PHASE_OUTCOME.md` header and its missing
    unit 341 entry.
- **Anything touching the radio, a decoder, a parser or the transmit chain. Any package.**

## 7. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.** HM-DEC-155:
  the suite killed sessions.
- **Do not change what a card is earned by, what scores, any points, or the words rulings 11 to 16
  set.** Those belong to the log, to the points file and to step 2. This unit proves the page, and
  fixes only a drawn fact the view model already holds (ruling 20).
- **Do not loosen a threshold or tolerance, and do not break the view to watch a test fail.** A
  loosened test proves nothing. A small miss ships as `partial` with its number (§6).
- **No image assets, no second map control, no new map bitmap.** R22: *vector and the map the app
  already has*.
- **Do not edit `PHASE_OUTCOME.md` or the `STEP:` lines.** They are the launcher's, and a unit writing
  them hides the disagreement the reload exists to show.
- `CLAUDE.md` §12.6 covers the rest. **No package. Report mismatches. Write American.**
- **The tool facts, from units 332 to 342**, with unit 337's contradictions marked. Say which held:
  - apostrophes inside quoted heredocs break, and an apostrophe in an argument breaks the `.bat`
    tools;
  - doubled backslashes collapse;
  - these are refused: `;`, `rm`, `git stash`, `sort` in a pipe, `grep -v` in a pipe, `sed -E`, a
    shell loop variable, a redirect or `tee -a` into `output.md`, and a command substitution;
  - *Python cannot run*, which unit 337 reports it did;
  - a multi-line commit message needs more than one `-m`;
  - use exact-text edits for markup;
  - the validator runs as `dotnet build tools/arbiter/validate-output.proj -p:Report=output.md`;
  - a `public const` cannot be bound, so bind a property built from it.

## 8. Committing and pushing

Commit and push each task on its own, on `main`. The report and the status file go in their own
commit, and that commit also stages the deletion-and-rewrite of `output.md`. The report names every
commit and whether each push succeeded. **A refused push is reported as refused, with the reason.**

---

## 9. Reporting

Write `output.md` at the root, then stop. Do not start the next unit. **Every exit writes it**:
finished, blocked, failed or stopped early. Unit 342 stopped at *writing the report*, and that is why
step 1 was judged on a step 0 report. **Write the report before anything optional.**

Canonical headings: `## 1. What Claude did`, `## 2. What the owner should expect`,
`## 3. What you should see`, `## 4. What's blocking us`. Validate it with
`dotnet build tools/arbiter/validate-output.proj -p:Report=output.md`.

**The ordering block comes first.** `validate-output` refuses a report without it.

```
READ IN THIS ORDER.

A. The phase goal - the screen, done right. Step 0 done (state reader on
   unit 341). Step 1 <state as the evidence stands>: <n> of its seven
   must-pass carry a named green test on the drawn page at 1400 AND 1920
   after this unit, was 4 (2, 5, 6, 7); <which do not, and why>;
   nice-to-pass <re-run green | red>. Steps 2 and 3 not started.
B. Step 1 and its exit criteria, each with its test, its result, drawn or
   view model, and the widths realized:
   entry: TheAchievementsPageClicksInTests <n of n>, run first
   1. color band with count, score, level and a bar - <test, result; kinds
      and continent pages at 1400 and 1920; the gap clause on <kinds>>
   2. earned card is the contact, with a path map cropped to the two
      stations - <test, result; map <w x h> at 1400 and 1920 re-measured
      against unit 342's 632x231 and 892x231; the crop bound>
   3. next card names its want and its callers with distance, or no one
      calling - <test, result; per kind at 1400 and 1920; anything NOT
      DRAWN and what ruling 20 did about it>
   4. all eight kinds per R22, Continents to seven and each to its
      countries - <tests, results; drawn at <widths>; whether the 1920
      half was dropped>
   5. no string clips or wraps a word at 1400 and 1920 - <test, result; the
      tallest page's height at each width>
   6. no card is a white rectangle - <test, result; white cards of total>
   7. achievement_category_opened carries the kind and the card count -
      <test, result>
   nice-to-pass: a card's map opens in the popup on click - <test, result;
      the event it writes, or none>
C. The report last. Section 4 raises N items on top of the carried queue;
   say which, if any, stands in the way of a criterion in B - in
   particular any fact the view model holds that the page does not draw.
```

**Every line specific to this unit.** If a criterion was not measured, say *not measured*. Do not
fill the shape.

```
UNIT:       343 - <complete|stopped> at task N of 3 - <date time, read from the clock>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no> - <which of criteria 1, 3 and 4 now hold on the drawn
            page at both widths>
NUMBER:     step 1 must-pass proved on the drawn page at 1400 and 1920:
            4 of 7 -> <n> of 7
DRIFT:      0
```

**Section 1 includes what unit 342 built**, commit by commit, with task 0's re-measured numbers beside
each commit message's claim. It is the step 1 record that was never written.

**Section 3 leads with the answer:** does every step 1 must-pass hold with a named green test on the
drawn page at 1400 and 1920? If not, which criterion, at which width, on which kind, and by how much.
Then, in words, top to bottom, at both widths:
- one Countries earned card and the Countries next card;
- the band line on each of the eight kinds as it is drawn;
- the seven continent badges and one continent's page.

Then the step 1 table: test, criterion, drawn or view model, widths, kinds, result, numbers. **Every
appearance claim is computed, not seen. Say so once.**

**Section 4:** unit 337's section 4 verbatim per HM-DEC-139, from its line under
`## 4. What's blocking us` to its end, as committed in `e4c160f`, including the queue it carries. Unit
342 wrote no section 4, so there is nothing of its own to carry. In the carried text, mark these items:
- unit 340's item 5 (step 1 held weakly): `TAKEN UP by work instructions 342 and 343`, with the crop
  bound and where each clause is now asserted;
- unit 339's item 5 (the nice-to-pass has nothing behind it): `ANSWERED by unit 342 task 3 (9214b2a)`,
  re-run in unit 343 task 0;
- unit 337's (carried repair) item 3 (the carried queue named unit 336): `NOTED by work instruction
  343` - this report carries unit 337's.

Then anything this unit raises. **A ruling is wanted only where Tim must decide. Everything else is a
finding, and says so.** Rulings 11 to 20 are already marked for Tim at step 3. Do not raise them
again unless the page shows something that changes one.

---

```
ARBITER-DECISION
STEP: 1
APPROACH: prove step 1 on the drawn category pages at 1400 and 1920 instead of the view model - the band (criterion 1), the next card and its callers per kind (criterion 3), all eight kinds with Continents to seven and each to its countries by click (criterion 4) - fixing only a fact the view model holds and the page does not draw, and re-measuring and reporting unit 342's unreported build
MOVE: work around
WHY: Unit 342 built its rulings and ended at "writing the report", so the state reader judged step 1 against unit 341's step 0 report and found criteria 1, 3 and 4 unproved on the screen (view model or 1040 only, per the test source). Proving on the drawn page is a different approach from fitting the pages to the picture. The loop test found nothing like it for step 1, and it is the method that closed step 0 (unit 339), not a repeat of it.
STATE: partial
DECIDED: rulings 17 to 20 - criteria 1, 3 and 4 count only on the realized page at 1400 and 1920, not at 1040 and not on the view model; unit 342's build stands and is re-measured, not redone; a watched red for an already-true assertion is a wrong expectation on the test window only, never broken markup; a fact the view model holds and the page does not draw is fixed as step 1 work, never changing scoring, earning or rulings 11-16's words; plus the Hall of Fame and Modes PSK31 next-card collision left to step 2 as drawn, and unit 337's PSK31 findings logged, not chased - all the author's, marked for Tim at step 3, overrulable
LICENCE: PHASE_PLAN.md sections 1 (a step's exit is what is on the screen, asserted by computation), 2, 4 (step 1 exit criteria), 6 (the arbiter decides and continues; ship partial on a small miss; never loosen a test) and 7; .run-unit/state-verdict.json (step 1 partial on unit 342: criteria 1, 3, 4); PHASE_OUTCOME.md unit 342 STATE_WHY; PROJECT_STATUS.md at 9214b2a (EXECUTING, writing the report) and git log (no docs(unit342) commit); tests TheCategoryPagesAreTradingCardsTests lines 50, 289, 779, 802, 1029, 1614 and TheAchievementsPageClicksInTests line 281; docs/phase-maintenance-run/PHASE_PLAN.md R22; unit 337 output.md section 4 (e4c160f); CLAUDE.md 0.0, 0.2, 0.5, 0.6; psk31 R12, R13, R14, R19; HM-DEC-139, HM-DEC-155
ACCOMPLISHED: every achievements category page is shown, on the page as drawn at both of Tim's widths, to carry its band, its earned contacts and a next card naming who is calling, for all eight kinds and every continent - and the step has a report of its own saying what unit 342 built and what it measures
ADVANCES: step 1 - must-pass 1 (color band) and 3 (next card and callers) in task 1; must-pass 4 (all eight kinds, Continents to seven and each to its countries) in task 2, whose 1920 half is the drop candidate; must-pass 2, 5, 6, 7 and the nice-to-pass re-measured and reported in task 0
END-ARBITER-DECISION
```
