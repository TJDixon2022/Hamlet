# Work instruction 346 - step 1, the Modes next card and every continent page

Step 1 of `PHASE_PLAN.md`, **fourth unit on it.**

- **Unit 342** built the pages to the Countries picture and wrote no report.
- **Unit 343** could not run `dotnet` and proved nothing.
- **Unit 345** proved step 1 on the drawn page at 1400 and 1920. It reported **7 of 7** and changed no
  markup, because its trace found nothing `NOT DRAWN`.
- **The state reader read unit 345's report and returned `partial`**, in these words:

> *The report shows named green drawn tests at 1400 and 1920 for most criteria, but its own finding 3
> says the Modes next card draws CW with an empty line and no place or callers, which misses R22
> content for criterion 4 and the no one is calling line for criterion 3, and the clip and white card
> test covers only two of the seven continent pages.*

**So two things stand between step 1 and `done`, and both are named.** One is a content gap on the
page: the Modes next card draws a row with nothing on it. The other is a coverage gap: criteria 5 and
6 are asserted on two of the seven continent pages. **This unit closes both, and nothing else is
required of it.** It has four tasks, 0 to 3, and task 3 is the drop candidate.

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

**Also:** `.run-unit\allowed.txt` must permit `dotnet`. If `dotnet test` is refused, stop at task 0,
write the report, and say so in section 4, quoting the refused command. **Do not route around it**:
no `tools/tests/run.js`, no assertions written unrun, and no edit to `allowed.txt` or
`run-unit-tools.txt`. Unit 343 item 1 rejected all three, and those rejections stand.

---

## 1. Why this unit exists

**The number:** unit 345 claimed 7 of 7 step 1 must-pass with a named green test on the drawn page at
1400 and 1920. **The state reader did not accept it.**
- **Criteria 3 and 4 miss on one card.** The Modes next card's CW row draws an empty line, with no
  place and no word on who is there.
- **Criteria 5 and 6 are under-measured.** They were asserted on `continent-EU` and `continent-OC`
  only.

**Read strictly, 3 of 7 hold without qualification: criteria 1, 2 and 7.** This unit's target is
**7 of 7, with no qualification the state reader can name.**

**Read from the tree by this arbiter, 2026-09-13, at `218d6e0`. None of it has been run.**
- **`AchievementCategory.ModesFor`** (`src/Hamlet.App/ViewModels/AchievementCategory.cs:662`) builds
  one `NextCaller` per unworked mode. The row's line is `Joined(LivesAt(mode, bet.Band),
  CallingIn(calling, mode))`.
  - **`LivesAt`** (`:695`) reads only `DigitalCallingFrequencies`, so CW gets `""`.
  - **`CallingIn`** (`:712`) counts rows whose `Mode` equals the mode. The CQ list carries no Morse,
    so CW gets `""` there too.
  - `Joined` of two empty strings is empty. **That is the empty line unit 345 reported.**
  - The card passes `""` as its no-caller line, so no row, and not the card, says that no one is
    there.
- **The method's own remark** (`:655`-`:659`) says *Morse and Voice have no digital row, so their line
  is the mode alone*, and *an FT8 or FT4 row does not say which of the two it is*. So an FT4 count
  from the list is always 0 or unknowable.
- **Hamlet already cites a CW place per band.** `HfBands.Landing` (`src/Hamlet.RadioEngine/Bands/HfBands.cs:190`)
  is where a band button lands, per HM-DEC-110. It is the first CW block named `LandingBlock` inside
  the CW segment, or the bottom of that segment, *derived from the same citation as the segment*. It
  is `private`. Whether a public read of it exists was not checked.
- **The Hall of Fame card already says the Morse truth.** Its `first_cw_qso` next card (`:571`) uses
  `NoMorseOnTheList` (*the CQ list carries no Morse*), and its comment gives the §0.0 reason.
- **`NoStringClipsAndNoCardIsWhiteAtFourteenHundredAndNineteenTwenty`**
  (`tests/Hamlet.App.Tests/Views/TheCategoryPagesAreTradingCardsTests.cs:1441`) loops over
  `AchievementKinds.All` plus `continent-EU` and `continent-OC`, on `TwelveContacts()` only.
  - The twelve contacts have worked all five modes, so **the Modes next card is never realized by the
    clip test at all.**
- **`Calling()`** (`:2585`) is four FT8-shaped rows. **No PSK31 caller is on it**, so the *who is
  there* branch of a Modes row has never been drawn.
- **Unit 345 item 2:** VK2DEF's map is 633.18 x 231 in an 892 px card at 1920, on Grids and Bands.
  Ruling 11's span is asserted on Countries only.

```
PHASE GOAL: The screen, done right - the main window as the approved mockup
            (step 0, done on the state reader's verdict on unit 341); every
            achievements category page as trading cards as the approved
            Countries picture draws them, all eight kinds (step 1, partial
            on unit 345); then what the last phase left (step 2); then Tim
            at his window says it passed (step 3).
UNIT GOAL:  The two gaps the state reader named on unit 345 closed on the
            drawn page at 1400 and 1920 - every row of the Modes next card
            says where that mode lives and who is there, truly and never
            empty; and no string clips and no card is white on all seven
            continent pages and on the Modes page that draws a next card -
            so step 1's seven must-pass stand with nothing left to qualify.
ADVANCES:   step 1 - must-pass 3 (the no-caller line) and 4 (R22's Modes
            content) in task 1; must-pass 5 and 6 over every page in task 2;
            ruling 11's span on every kind in task 3, the drop candidate.
DRIFT:      0
```

---

## 2. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any mismatch.
**Report the mismatch; do not repair the instruction.** Mismatches go in the report even when the
work succeeds anyway.

Check:
- **§1's reading of `ModesFor`, `LivesAt`, `CallingIn`, `NextCard` and `HfBands.Landing`**, line by
  line. Also check whether any public member already exposes a band's CW landing or CW segment to
  `Hamlet.App`. The band buttons land somewhere, so something reads it.
- **What `Mode` a `DigitalDecodeRow` built by `Heard(...)` carries**, and whether any row the list
  holds can say `FT4`.
- **The clip test's page list** (§1), and that `StatesCountWhatTheLogsStateFieldSays` still covers
  States on its own log at both widths for fit and white cards. Unit 345 said so.
- **Unit 345's commits** are `de709fd`, `c5bb6a9`, `ff09a11`, `04b8abf` and `218d6e0`, each pushed.
  `output.md` in the tree is unit 345's.
- **The version is 1.13.32** in `Directory.Build.props`.
- **The launcher's files still disagree with themselves.** One line each; edit none:
  - `PHASE_OUTCOME.md`'s header reads step 0 `in progress` and has no unit 341 entry. The reload
    reads the last step 0 entry (unit 340, `blocked`) as winning.
  - `PHASE_STATUS.md` reads `CURRENT_STEP: 0`.
  - Units 343 and 345 are both recorded as `UNIT 1 - STEP 1`, and unit 344 has no entry.

  Step 0 is taken as done on the state reader's verdict on unit 341, as work instructions 342 to 345
  took it.
- **`RULES_AT`.** `PROJECT_STATUS.md` reads `HM-DEC-163 (2026-09-12)`, set by hand by unit 345.
  `tools/status.sh` still writes `HM-DEC-161`. Confirm the highest `HM-DEC` id in `DECISIONS.md`.
- **The reload says `CLAUDE.md` §1 holds `CPS-DEC-0163`.** Unit 345 found no `CPS-DEC` id in the
  file. It is parked with the id schemes. One line.
- **`validate-output.bat` with no argument validates another project's `output.md`.** Always pass the
  report (§9).
- **`tools\arbiter.bak-20260913\` is untracked at the root.** **Never `git add -A` or `git add .`**;
  stage files by name.
- **The tool facts in §7** are unit 345's. Say which held for you.

**Reds expected, older than this unit. Name them and do not chase them:**
- `TheOperatorCanStopItTests`: `TheStopAddedNoNewRouteToATransmission` and
  `AClickWhileTheToneIsPlayingStopsTheSoundAndNotJustTheCarrier`. **Not run.**
- `TheWholeChainRunsFromOneRightClickTests` (2), `TheMenuIsUnderTheMouseTests` (8),
  `ThePsk31RecordsAppearTests.WithNoPsk31ContactNoPsk31CardIsOnTheScreenAndNoneIsDimmed` and
  `TheTotalMilesTests.TheBadgeSaysZeroMilesOnAnEmptyLogAndTheLowestTierIsNext`. **Not run.** They are
  step 2's.

**Expected on the carry-forward list: 111 of 111 app, 86 of 86 engine**, unit 345's numbers. Unit
344's 87 is unexplained and parked (unit 345 item 1). **If a red turns green or a new red appears in
what you ran, say which.**

**Expected red, and wanted:** once task 1's assertions are added, they are red against the tree as it
is, because the CW row is empty. By ruling 19, that red is the watched red. Give its failure line.

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

**The arbiter's rulings 1 to 10** (work instructions 338 to 341, the main window) **stand as built.**
This unit touches none of them.

**The arbiter's rulings 11 to 16** (work instruction 342) **stand as unit 342 built them.** They are
the author's, marked for Tim at step 3, and overrulable:

11. The earned card's map spans the card's inner width, at a height unit 342 chose (231 px). The
    no-map list border takes the same height. It is the conversation card's `Ft8GlobeControl` over
    the same plot, with no second map and no image asset.
12. *Cropped to the two stations* is asserted as a number: both stations lie inside the drawn frame,
    and the frame lies within a stated bound that a whole-globe frame fails.
13. The band line carries the gap to the next level where there is one, and says so where there is
    none.
14. The quill sentence appears only where the decoded list actually draws that mark for such a
    caller. The callers panel's heading keeps the time the list was read and never says *right now*,
    because the window is modal and the list is read once (§0.0).
15. The back control is a plain link, with `BackLabel`'s words and its command unchanged.
16. A card's map opens in a popup on a click, never on a hover, reusing the conversation card's
    mechanism. It writes that popup's event, or none if that popup writes none. A click outside
    closes it.

**The arbiter's rulings 17 to 21** (work instructions 343 and 345) **stand as unit 345 built them.**
They are the author's, marked for Tim, and overrulable:

17. **Criteria 1, 3 and 4 count as met only by assertions on the realized category page, at 1400 and
    1920.** The view-model assertions stay, but they are not the evidence.
18. **Unit 342's build stands and is not redone.**
19. **Watching red.** Show a new assertion fail once, against a deliberately wrong expectation set on
    the test window only, and give the failure line. Unit 345 built each watched red into its test
    as a second check, and that pattern stands. **Never break markup or a view to watch a test
    fail.** Where the new assertion is red against the tree as it is, that is the watched red, and it
    is a finding.
20. **Where the view model holds a fact the drawn page does not show, fixing it is this step's
    work.** A fix never changes what a card is earned by, what scores, any points, or the words
    rulings 11 to 16 set. A string that will not fit is shortened and named. A miss that cannot be
    fixed that way ships as `partial` with its number.
21. **Criterion 2 counts only where the earned contact's eight facts are asserted drawn** at 1400 and
    1920 on Countries, States and Grids. Unit 345 did this in `EarnedCardMiss`.

**Ruling 22, amended by this instruction.** It is the author's and overrulable.
- **What stays for step 2:** the PSK31 next-card collision on Hall of Fame and Modes against §3.1
  (unit 333 item 1, unit 336 item 2), and the States next card's wording (unit 335 item 1, R27). Both
  stay as drawn. R22 is later than §3.1, and `PHASE_PLAN.md` §6 lets the later ruling win, so neither
  is a stop.
- **What does not:** the **empty row** on the Modes next card. It is not the PSK31 collision. Ruling
  24 takes it into step 1.

**Ruling 23 stands.** Unit 344's PSK31 findings are logged, not chased.

**The arbiter's rulings for this unit.** They are the author's, marked for Tim at step 3, and
overrulable:

24. **The Modes next card's empty row is step 1's work, under criteria 3 and 4.** This overrules unit
    345 item 3's reading that it belonged to step 2.
    - *Why:* the state reader named it as the miss on both criteria. R22 asks the Modes next card for
      *where the unearned mode lives and who is there*, and ends *nothing empty*. Ruling 20 did not
      reach it, because the view model holds the empty string. So the fix goes where the line is
      built, in `ModesFor`.
    - *Rejected:* leaving it for step 2. Step 2's exit names *the Modes test unit 336 named*, not this
      card. Step 1 cannot reach `done` while the state reader counts an empty row against it, so
      deferring it closes nothing.
25. **Where a mode lives comes only from a table Hamlet already cites. No frequency is typed into a
    string.**
    - Digital modes keep `DigitalCallingFrequencies`, unchanged.
    - **CW takes the CW place Hamlet already derives for a band button (HM-DEC-110)**, on the green
      zone's best-bet band where that band has one, and the lowest band that does otherwise. That is
      the same rule `LivesAt` uses. It is written in `LivesAt`'s form: `7.030 on 40 m`.
    - If `Hamlet.App` has no public read of it, **one read-only accessor may be added beside
      `HfBands.Landing`** that returns what `Landing` already returns. It must change no band, no
      landing and no behavior, and the engine carry-forward run is the proof.
    - **If the trace finds the CW place cannot be read without changing how a band button lands, or
      anything that tunes, do not build it.** Report the row's place as `partial` with why.
    - *Why:* §0.0. A place the operator is sent to must be one Hamlet can cite, and this one is
      already what a press on a band button uses.
    - *Rejected:* `ModeGuide`'s `LivesAt40mHz`. It is editorial field-guide copy for 40 m only
      (HM-DEC-016), not the cited band plan.
    - *Rejected:* typing `7.030` into the card. That would be a number from nowhere.
26. **Who is there, on each row, says only what the CQ list can know (§0.0), and the row is never
    empty.**
    - **Where the list's rows say that mode and someone is calling:** the nearest such caller's
      callsign with distance, as `CallersFrom` joins them, and the count where there is more than
      one. That is criterion 3's *lists them with distance*.
    - **Where the list's rows say that mode and no one is calling:** that no one is calling in it now.
      Keep the sense of `NoOneCalling` and fit the words to the row.
    - **CW:** `NoMorseOnTheList`'s words, as the Hall of Fame card already says them.
    - **A mode the list cannot tell apart** (FT4 from FT8, by the method's own remark, if the trace
      confirms it): say that the list cannot tell it apart. Never say *no one*.
    - The exact words are the unit's, shortened per §6 and named in the report.
    - *Rejected:* one no-caller line for the whole card. The rows are modes, and the answer differs
      by mode.
    - *Rejected:* a "take me there" press on a row. It would tune the radio, and §6 makes that a stop.
27. **Criteria 5 and 6 count only over every category page step 1's drawn tests realize, at 1400 and
    1920.**
    - On `TwelveContacts()`: the eight kinds and **all seven continent pages**.
    - On the five-contact log: **the Modes page**, which is where its next card is drawn, with a PSK31
      caller on the list so the longest row is drawn.
    - States on its own log stays in `StatesCountWhatTheLogsStateFieldSays`. Name it; do not duplicate
      it.
    - *Why:* the state reader named two of seven, and the Modes next card this unit changes has never
      been in the clip test.
28. **Ruling 11's span holds on every earned card with a map, on every kind, at both widths.**
    VK2DEF's 633 in 892 at 1920 is fixed in how the card sizes its map, never by loosening the span
    assertion. **This is task 3 and the drop candidate.** The span is ruling 11's, not a criterion's,
    so dropping it does not hold step 1 back. Say so if it is dropped.

**Standing, transcribed:**
- **§0.0**: *Never present a guess as a decode… This binds pictures as hard as sentences.* Every
  appearance claim is computed, not seen, and says so once.
- **§0.2**, *transmit safety, ABSOLUTE*: *One operator action, one transmission.* Nothing here
  transmits or tunes. No test presses CQ, Stop, Capture, a band button or a transmit command.
- **§0.5**: *every panel is collapsible, and a collapsed panel still carries its summary. Collapsing
  hides detail, never information.*
- **§0.6**: every ink clears 4.5:1 against its fill, and color is never the only carrier.
- **R12**: *a session fixes its own tests and never asks the owner to approve it.*
- **R13**: telemetry on every new stage. This unit adds no stage and no card, so
  `achievement_category_opened`'s card count is unchanged.
- **R14**: *a test exists to prove an exit criterion.* Extend the tests the criteria need and add no
  others, apart from the trace.
- **R19**: American spelling.
- **R23**: States from `STATE`, rank names from the points file. Untouched here.
- **HM-DEC-139**: open asks are carried verbatim until answered.

---

## 4. Status cadence

Write status **before every `dotnet` command, after every commit and after every task**, with
`sh tools/status.sh`. It ran for every write in unit 345. If it is refused, take a `date` reading and
paste it whole. **Never compose a time.**

The script hard-codes `RULES_AT: HM-DEC-161`. After it writes, set that line back to `HM-DEC-163
(2026-09-12)`, or to the highest id §2 finds, with the file editor. Unit 345 found `sed -i` blocked.

The watchdog kills a session only when its process tree has used no CPU for ten minutes.

---

## 5. The tasks

### Task 0 - the trace: the Modes next card, the CW place, and every page's fit, before anything changes

Measure before anything is built. **Say what you find rather than confirming §1.**

1. **Commit `WORK_INSTRUCTIONS.md`, `PHASE_STATUS.md` and a patch bump (1.13.32 -> 1.13.33, with its
   comment block).**
   - In `PHASE_STATUS.md`, change only the `WORK_INSTRUCTION:` line, to `346 - step 1, the Modes next
     card and every continent page`. Do not touch its `STEP:` lines, `CURRENT_STEP` or `HEARTBEAT`.
   - Do not commit `.run-unit\`, `SESSION.lock` or `tools\arbiter.bak-20260913\`.
   - The message is `chore(unit346): step 1, the Modes next card and every continent page - the trace
     before a card is changed`.
   - Run the carry-forward list, status first, and give both invocations as *n of n*.
2. **Step 1's entry check.** Run `TheAchievementsPageClicksInTests` by class, then
   `TheCategoryPagesAreTradingCardsTests` by class. Give each as *n of n*. **If either is red, report
   it and stop at task 0.**
3. **Add `Unit346TraceTheModesNextCardAndEveryPage` to `TheCategoryPagesAreTradingCardsTests`.** It
   asserts nothing and presses nothing that transmits or tunes. At 1400 and 1920 it prints:
   - **the Modes next card on the five-contact log**, drawn beside the view model, row by row. Print
     each row's place and line, and each empty one as `EMPTY`. Do it twice: once with `Calling()`, and
     once with `Calling()` plus one PSK31 caller row on the test window, built the way the list builds
     a PSK31 row. Print what `Mode` each row carries, and whether any row can say `FT4`;
   - **the CW place candidates, per HF band:** what the band button lands on (HM-DEC-110) and the CW
     segment's bounds, read through whatever public route exists, with the route named. If none
     exists, print `NO PUBLIC ROUTE` and name the private member;
   - **every page ruling 27 names**, with its runs that fit, its card count, its white cards and its
     height. That is the eight kinds and seven continent pages on `TwelveContacts()`, and Modes on the
     five-contact log;
   - **every earned card with a map, on every kind**, with its map's drawn width against the card's
     inner width.
4. **Answer from the numbers, in section 1, before task 1 starts:**
   - Which Modes rows are `EMPTY`, at which width, on which caller list?
   - Where can the CW place be read from, and what does it give on the fixture's best-bet band?
   - Can the list say FT4?
   - Does any page ruling 27 names clip a string or draw a white card today?
   - Which maps fall short of their card's inner width, by how much?

**Drop candidate:** none. Tasks 1 to 3 are built on its numbers.

### Task 1 - the Modes next card: where each unworked mode lives and who is there, never empty

1. **Extend the tests first (R12), and watch them red against the tree as it is.**
   - **`TheNextCardKnowsWhoIsCalling`**: at 1400 and 1920, on the five-contact log, with `Calling()`
     and with the added PSK31 caller, assert that every drawn Modes row has a place and a non-empty
     line. The line must say ruling 25's place where one is cited, and ruling 26's *who is there* for
     that mode. The PSK31 row with the caller must draw the caller's callsign with a ` · n mi`
     distance.
   - **`TheOtherFiveKindsEachDrawTheirOwnCards`**, Modes half: assert that CW draws its cited place,
     in `LivesAt`'s form, and the no-Morse words. FT4 and PSK31 keep their `3.575 on 80 m` and
     `3.580 on 80 m` if the trace shows the bet band unchanged.
   - Keep every existing assertion. The CW row as the tree draws it is the watched red. Give its
     failure line.
2. **Build it in `AchievementCategory.ModesFor` and `LivesAt`**, with the one accessor ruling 25 allows
   if the trace found no public route. Change no card's earning, no score, no points, no other kind's
   next card and no word rulings 11 to 16 set.
3. **Run the one filter:** `TheCategoryPagesAreTradingCardsTests`, `TheAchievementsPageClicksInTests`,
   `BindingHealthTests` and `VoiceTests`. Give each class as *n of n*. Then run the carry-forward list,
   status first. If an engine file changed, the engine run is the proof that nothing moved. Commit and
   push.

**Drop candidate:** none. It is the named miss on criteria 3 and 4.

### Task 2 - criteria 5 and 6 on every page

1. **Extend `NoStringClipsAndNoCardIsWhiteAtFourteenHundredAndNineteenTwenty`** (ruling 27):
   - on `TwelveContacts()`, all seven continent pages at both widths, replacing the two;
   - on the five-contact log with the PSK31 caller, the Modes page at both widths.

   Keep its existing assertions and thresholds, and print each page's line as it does now.
2. **Watched red** per ruling 19, built into the test as a second check: a page held against a
   deliberately narrowed fit on the test window only, or a card with its map, bar and list hidden on
   the test window only. Give the failure line.
3. **Where a string clips or a card is white, fix it by §6**: shorten and name it, or widen. Never
   clip and never loosen.
4. **Run the one filter again**, then the carry-forward list, status first. Commit and push.

**Drop candidate:** none. It is the named miss on criteria 5 and 6.

### Task 3 - ruling 11's span on every kind (the drop candidate)

1. **Extend `EveryEarnedCardIsTheContactThatEarnedIt`**: at 1400 and 1920, every earned card with a
   map, on every kind that draws one, has its map's drawn width equal to the card's inner width, by
   the tolerance the Countries assertion already uses.
   - VK2DEF on Grids and Bands at 1920 is the watched red, or task 0's measured shortfall if it
     differs.
2. **Fix it where the card sizes its map**, the way unit 342 sized the Countries map. Change no crop
   bound, no height and no word.
3. **Run the one filter**, then the carry-forward list, status first. Commit and push.

**Drop candidate: this task, whole.** If time runs short, skip it and report VK2DEF's number as
unit 345 gave it. Ruling 28 makes it no bar to step 1.

---

## 6. Parked - do not touch, do not raise

- **The main window, all of it.** Step 0 is done. That covers the top row, the power offer and its
  popup, the green block, the 1400 PSK31 row and the live best bet, unit 341 items 1 to 6, and unit
  344's **Capture 2 minutes** press. Do not run `TheTopRowTests` or `TheWorkingPanelsTests`. If the
  carry-forward list turns one red, report it once and do not chase it.
- **Step 2's items:**
  - the States wording, including the States next card's *Hamlet cannot tell a caller's state*;
  - the PSK31 next-card collision on Hall of Fame and Modes against §3.1 (ruling 22). Task 1 fills
    the empty row and does not take PSK31 off the Modes card;
  - the Modes test unit 336 named;
  - the undeletable files, the points file's comment block, the small `Views` reds and
    `TheTotalMilesTests`.
- **Every other kind's next card.** That includes Bands' empty no-caller argument. R22 asks Bands for
  the best bet, and the state reader did not name it.
- **The achievements window's own size** (`1040 x 720`; unit 332 item 3).
- **PSK31, all of it** (ruling 23): unit 344's five items and unit 337's carried repair.
- **`TheOperatorCanStopItTests`, all of it.** Transmit-side.
- **The engine carry-forward's 86 against 87** (unit 345 item 1), and the missing version comment
  blocks at 1.13.29 and 1.13.31 (unit 343 item 4, unit 345 item 4). Findings; not repaired here.
- **Carried in `PHASE_PLAN.md` §7 and belonging to Tim or the harness:**
  - the live license lookup and the live heard count;
  - the two id schemes, including `CPS-DEC-0163`;
  - real flags on country cards, PSK31 step 6, real PSK31 audio, the ALC margin and the map bitmap's
    license;
  - the demodulator and the `Why` hovers (`ModeFirstRow.Why` stays undrawn);
  - the launcher's `PHASE_OUTCOME.md` header, its missing unit 341 entry, its unit numbering, and
    `tools/status.sh`'s hard-coded `RULES_AT`.
- **Anything touching the radio, a decoder, a parser or the transmit chain. Any package.**

## 7. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.** HM-DEC-155.
- **No frequency typed into a string** (ruling 25). No "take me there", no tune, and no press on a
  Modes row. **No change to how a band button lands.** If ruling 25's accessor would need one, stop
  building it and report `partial`.
- **Never say *no one is calling* where the list could not show one** (ruling 26, §0.0).
- **Do not change what a card is earned by, what scores, any points, or the words rulings 11 to 16
  set.**
- **Do not loosen a threshold or tolerance, and do not break the view to watch a test fail** (ruling
  19, §6).
- **No image assets, no second map control, no new map bitmap** (R22).
- **Do not edit `PHASE_OUTCOME.md`, the `STEP:` lines, `.run-unit\allowed.txt` or anything under
  `tools\arbiter\`.** They are the launcher's.
- **Never `git add -A` or `git add .`** (§2).
- `CLAUDE.md` §12.6 covers the rest. **No package. Report mismatches. Write American.**
- **The tool facts, as unit 345 found them.** Say which held for you:
  - **ran:** `sh tools/status.sh`; `&&` joining `git add`, `commit` and `push`; `;` joining two
    `grep`s; `grep`, `cut`, `uniq`, `tail`, `head` and `grep -v` in pipes; commits with several `-m`;
  - **asked for approval, not run:** a compound of `status.sh`, `sed -i` and `dotnet test`; `git -C`;
    `git show` with several hashes (`git log --no-walk` ran instead); `sort` with options in a pipe;
  - **blocked:** an output redirect, to `/tmp` and into the repository; `sed -i` on `output.md`;
    `grep` on a file outside the repository;
  - **older and not retested:** an apostrophe in an argument breaks the `.bat` tools; doubled
    backslashes collapse; use exact-text edits for markup; a `public const` cannot be bound, so bind
    a property built from it.

## 8. Committing and pushing

Commit and push each task on its own, on `main`, staging files by name. The report and the status file
go in their own commit. The report names every commit and whether each push succeeded. **A refused
push is reported as refused, with the reason.**

---

## 9. Reporting

Write `output.md` at the root, then stop. Do not start the next unit. **Every exit writes it**:
finished, blocked, failed or stopped early. **Write the report before task 3 if time is short.**

Canonical headings: `## 1. What Claude did`, `## 2. What the owner should expect`,
`## 3. What you should see`, `## 4. What's blocking us`. Validate it with
`dotnet build tools/arbiter/validate-output.proj -p:Report=output.md`, or with
`tools/arbiter/validate-output.bat output.md`. **Always name the report.**

**The ordering block comes first.** `validate-output` refuses a report without it.

```
READ IN THIS ORDER.

A. The phase goal - the screen, done right. Step 0 done (state reader on
   unit 341). Step 1 <state as the evidence stands>: <n> of its seven
   must-pass carry a named green test on the drawn page at 1400 AND 1920
   with nothing qualifying them, was 3 unqualified after the state reader
   on unit 345 (unit 345 claimed 7); <which do not, and why>. Steps 2 and
   3 not started.
B. Step 1 and its exit criteria - the two gaps the state reader named
   first, then the rest re-run:
   entry: TheAchievementsPageClicksInTests <n of n>, run first
   3. next card names its want and its callers with distance, or no one
      calling - Modes rows at 1400 and 1920: <each row's drawn line, with
      Calling() and with the PSK31 caller>; EMPTY rows <n -> n>
   4. all eight kinds per R22 - Modes: where each unworked mode lives
      <CW's place and its cited source>, and who is there <per mode>
   5. no string clips or wraps a word - <pages measured: 8 kinds, 7
      continent pages, Modes on five contacts>; <test, result; tallest
      page at each width>
   6. no card is a white rectangle - <same test; white cards of total per
      width>
   1, 2, 7 and the nice-to-pass - <test and result each, re-run in the one
      filter>
   ruling 11's span on every kind - <task 3's result, VK2DEF at 1920, or
      DROPPED>
C. The report last. Section 4 raises N items on top of the carried queue;
   say which, if any, stands in the way of a criterion in B - in
   particular whether the CW place needed an engine accessor, and whether
   any row's words claim more than the CQ list can know.
```

**Every line specific to this unit.** If a criterion was not measured, say *not measured*. Do not
fill the shape.

```
UNIT:       346 - <complete|stopped> at task N of 3 - <date time, read from the clock>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no> - <whether criteria 3, 4, 5 and 6 now hold with
            nothing the state reader named on unit 345 still open>
NUMBER:     step 1 must-pass held on the drawn page at 1400 and 1920 with
            nothing qualifying them: 3 of 7 -> <n> of 7
DRIFT:      0
```

**Section 3 leads with the answer:** does every step 1 must-pass now hold, with the Modes next card
drawing no empty row and every continent page measured? If not, which criterion, at which width, on
which page, and by how much. Then, in words, top to bottom, at both widths:
- the Modes next card on the five-contact log, row by row, with and without the PSK31 caller;
- the seven continent pages' fit lines;
- the Grids VK2DEF card's map, if task 3 ran.

Then the step 1 table: test, criterion, drawn or view model, widths, pages, result, numbers. **Every
appearance claim is computed, not seen. Say so once.**

**Section 4:** unit 345's section 4 verbatim per HM-DEC-139, from its line under
`## 4. What's blocking us` to its end, as committed in `218d6e0`, including the queues it carries. Keep
it in place with the file editor if redirects are blocked. In the carried text, mark:
- unit 345 item 3 (Modes' next card draws CW with no place): `TAKEN UP by work instruction 346 rulings
  24 to 26 and task 1`, with what the CW row now draws;
- unit 345 item 2 (VK2DEF's map 633 x 231 at 1920): `TAKEN UP by work instruction 346 ruling 28 and
  task 3`, with the new number, or `DROPPED by unit 346 - task 3 was the drop candidate`.

Then anything this unit raises. **A ruling is wanted only where Tim must decide. Everything else is a
finding, and says so.** Rulings 11 to 28 are already marked for Tim at step 3. Do not raise them again
unless the page shows something that changes one.

---

```
ARBITER-DECISION
STEP: 1
APPROACH: close the two gaps the state reader named on unit 345 - fill the empty CW row on the Modes next card with the cited CW place Hamlet already derives for band buttons and a who-is-there line per mode that claims only what the CQ list can know, never empty; extend the clip and white card test from two to all seven continent pages and the Modes page with its next card; the map span on every kind as the drop candidate; measured at 1400 and 1920
MOVE: work around
WHY: Unit 345 proved the existing pages on the window and changed nothing, and the state reader still returned partial on two named gaps - an empty Modes row (criteria 3 and 4) and continent coverage of two of seven (criteria 5 and 6). Filling a content gap and widening coverage is a different approach from proving what is already drawn; the loop test found nothing like it.
STATE: partial
DECIDED: ruling 22 amended - only the PSK31 collision and the States wording stay for step 2; ruling 24 - the empty Modes row is step 1 work under criteria 3 and 4 (overrules unit 345 item 3); ruling 25 - where a mode lives comes only from a cited table, CW from the HM-DEC-110 band-button landing on the bet band or the lowest that has one, via at most one read-only accessor, never a typed frequency and never a change to landing or tuning; ruling 26 - who is there per row says only what the CQ list can know (nearest caller with distance, no one in that mode, the list carries no Morse, or the list cannot tell FT4 from FT8), never empty; ruling 27 - criteria 5 and 6 count over all seven continent pages and the Modes page with its next card; ruling 28 - ruling 11 span on every kind, VK2DEF fixed, the drop candidate and no bar to step 1 - all the author's, marked for Tim at step 3, overrulable
LICENCE: PHASE_PLAN.md sections 1, 2, 4 (step 1 exit criteria 3 to 6), 6 (the arbiter decides and continues; later ruling wins; shorten a string and name it; never loosen a test; anything that tunes or transmits stops) and 7; .run-unit/state-verdict.json (step 1 partial on unit 345: the Modes CW row, two of seven continent pages); unit 345 output.md section 4 items 2 and 3; .run-unit/s4-verdict.json (none); docs/phase-maintenance-run/PHASE_PLAN.md R22 (Modes: where the unearned mode lives and who is there; nothing empty); src/Hamlet.App/ViewModels/AchievementCategory.cs ModesFor, LivesAt, CallingIn, NextCard, first_cw_qso and NoMorseOnTheList at 218d6e0; src/Hamlet.RadioEngine/Bands/HfBands.cs Landing (HM-DEC-110); TheCategoryPagesAreTradingCardsTests lines 1441 and 2585; CLAUDE.md 0.0, 0.2, 0.5, 0.6; psk31 R12, R13, R14, R19; HM-DEC-139, HM-DEC-155, HM-DEC-163
ACCOMPLISHED: every row of the Modes next card tells the operator where that mode lives on the air and who is there, in words Hamlet can stand behind, and every achievements category page - all seven continents and the Modes page included - is measured at both of Tim's widths with no clipped word and no blank card, so step 1 stands on evidence with nothing left for the state reader to name
ADVANCES: step 1 - must-pass 3 (the no-caller line on the Modes next card) and 4 (R22 Modes content) in task 1; must-pass 5 (no clip) and 6 (no white card) over all seven continent pages and the Modes page in task 2; ruling 11 map span on every kind in task 3, the drop candidate
END-ARBITER-DECISION
```
