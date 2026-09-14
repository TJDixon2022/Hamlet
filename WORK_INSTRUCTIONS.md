# Work instruction 347 - step 1, a PSK31 caller's distance on the next card

Step 1 of `PHASE_PLAN.md`, **fifth unit on it.**

- **Unit 342** built the pages to the Countries picture and wrote no report.
- **Unit 343** could not run `dotnet` and proved nothing.
- **Unit 345** proved step 1 on the drawn page at 1400 and 1920. The state reader left it `partial` on
  an empty Modes row and two of seven continent pages.
- **Unit 346** filled the empty row and measured all sixteen pages. **The state reader read its report
  and returned `partial`**, in these words:

> *Six of the seven must-pass criteria have named green tests on the drawn page at 1400 and 1920, but
> criterion 3 asks for callers listed with distance and the Modes next card draws the PSK31 caller
> EA3XYZ with no distance, which is a gap to close and not a stop the phase waits for.*

**So one thing stands between step 1 and `done`, and it is named.** A PSK31 caller on a next card is
drawn as his callsign with no distance. **This unit closes it, and makes the row's other words true
while it is there.** It has four tasks, 0 to 3, and task 3 is the drop candidate.

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

The arbiter checked all four against the tree on 2026-09-13, and they held. Check them again.

**Also:** `.run-unit\allowed.txt` must permit `dotnet`. If `dotnet test` is refused, stop at task 0,
write the report, and say so in section 4, quoting the refused command. **Do not route around it**:
no `tools/tests/run.js`, no assertions written unrun, and no edit to `allowed.txt` or
`run-unit-tools.txt`. Unit 343 item 1 rejected all three, and those rejections stand.

---

## 1. Why this unit exists

**The number:** step 1 must-pass held on the drawn page at 1400 and 1920 with nothing qualifying them
is **6 of 7**, as unit 346 reported and the state reader accepted. **Criterion 3 is the seventh.**
This unit's target is **7 of 7, with no qualification the state reader can name.**

**Read from the tree by this arbiter, 2026-09-13, at `0b506ed`. None of it has been run.**
- **The parser already reads the grid.** `Psk31ExchangeParser.Read`
  (`src/Hamlet.RadioEngine/Psk31/Psk31ExchangeParser.cs:157`) returns a `Psk31Exchange` whose `Grid`
  (`:72`) is the one clean Maidenhead word in the message, or null. `IsCertain` (`:212`) is false
  where any token is damaged, the message has no turnover, or two grids disagree.
- **The row already keeps the reading.** `DigitalDecodeRow` carries `Reading` (`:159`) on a text-only
  row. Its `Payload` (`:726`) is `Fields?.Payload ?? ""`, and a text-only row has no `Fields`, so its
  `Payload` is always `""`.
- **The CQ list drops the grid.** `CqSnapshot.From` (`src/Hamlet.App/ViewModels/CqSnapshot.cs:59`)
  sets a call's grid to `Ft8MessageSplit.IsGrid(r.Payload) ? r.Payload : ""`. Its remark (`:29`-`:30`)
  says *a PSK31 row has no fields and so no grid*. **That is the whole gap.** The reading holds the
  grid and the snapshot never looks at it.
- **The distance is already computed from whatever grid the call holds.** On the Modes card,
  `WhoIsThere` (`AchievementCategory.cs:743`) orders PSK31 callers by `MilesBetween` and joins the
  nearest with `MilesTo`. On every other kind, `CallersFrom` (`:988`) joins each caller with `MilesTo`.
  Nothing downstream of the snapshot needs to change for a grid to become a distance.
- **The test's PSK31 caller sends no grid.** `CallingWithPsk31()`
  (`tests/Hamlet.App.Tests/Views/TheCategoryPagesAreTradingCardsTests.cs:2950`) reads `CQ CQ CQ de
  EA3XYZ EA3XYZ K`. So even after the fix, that caller correctly draws no distance. **No fixture has a
  PSK31 caller who sent a grid.**
- **Other kinds read PSK31 callers too.**
  - Hall of Fame's `first_psk31` (`:566`) lists callers whose mode is PSK31.
  - The default next card (`:591`) lists everyone.
  - Countries' `UnworkedCountry` (`:1226`) is keyed on the callsign.
  - The Grids path checks `call.Grid.Length < 4` (`:1033`).
  
  **So a grid on a PSK31 call will reach Grids' and Hall of Fame's next cards as well as Modes'.**
  Unit 346 item 1 names this and stopped there, because its instruction forbade touching another
  kind's next card. Ruling 29 lifts that for this one fact.
- **Unit 346 item 2:** the PSK31 row says `no one is calling in it now`. The CQ list is read once when
  the window opens, the Modes card's heading carries no read time, and ruling 14 already kept the read
  time over *right now* on every other kind.
- **Unit 346 item 3:** the FT8, Voice and *list not read* rows are built and have never been drawn by a
  test, so their fit is unmeasured.

```
PHASE GOAL: The screen, done right - the main window as the approved mockup
            (step 0, done on the state reader's verdict on unit 341); every
            achievements category page as trading cards as the approved
            Countries picture draws them, all eight kinds (step 1, partial
            on unit 346, 6 of 7 unqualified); then what the last phase left
            (step 2); then Tim at his window says it passed (step 3).
UNIT GOAL:  A PSK31 caller whose CQ carried a grid is drawn on the next card
            with his distance, from the grid the PSK31 reading already holds
            and only where that reading is certain; a caller who sent none is
            his callsign alone by PHASE_PLAN.md section 6; the PSK31 row's
            no-caller words say what the list read knows; every row the Modes
            next card can draw is measured for fit - so criterion 3 stands at
            1400 and 1920 with nothing left to qualify it.
ADVANCES:   step 1 - must-pass 3 (callers listed with distance) in task 1;
            must-pass 3 (the no-caller line, true to the list) and 5 (the new
            lines fit) in task 2; must-pass 5 over every Modes row in task 3,
            the drop candidate.
DRIFT:      0
```

---

## 2. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any mismatch.
**Report the mismatch; do not repair the instruction.** Mismatches go in the report even when the
work succeeds anyway.

Check:
- **§1's reading of `Psk31ExchangeParser.Read`, `Psk31Exchange.Grid` and `IsCertain`,
  `DigitalDecodeRow.Reading` and `Payload`, `CqSnapshot.From`, `WhoIsThere`, `CallersFrom`, and the
  Grids path at `:1033`**, line by line.
- **Whether a live PSK31 row on the main window's decoded list carries the same `Reading`** that the
  test's row does. Name the member in `MainWindowViewModel.cs` (or wherever the list is fed) that sets
  it. If a live row carries no `Reading`, the fix would draw a distance only in the test. **Report
  that as a mismatch, and do not change how a row is made (ruling 29).**
- **Unit 346's commits** are `7e830e4`, `09da5d7`, `7b5fa8f`, `03a54f7` and `0b506ed`, each pushed.
  `origin/main` read `0b506ed` for this arbiter. `output.md` in the tree is unit 346's.
- **The version is 1.13.33** in `Directory.Build.props` (line 778 for this arbiter).
- **`DECISIONS.md` tops at HM-DEC-163.**
- **The launcher's files still disagree with themselves.** One line each; edit none:
  - `PHASE_OUTCOME.md`'s header reads step 0 `in progress`. The reload reads the last step 0 entry (unit
    340, `blocked`) as winning. There is no unit 341 or 344 entry.
  - `PHASE_STATUS.md` reads `CURRENT_STEP: 0`.
  - Units 343, 345 and 346 are recorded as `UNIT 1 - STEP 1`, `UNIT 1 - STEP 1` and `UNIT 2 - STEP 1`.
  - `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md` and files under `.run-unit\` are modified
    and uncommitted by the launcher.

  Step 0 is taken as done on the state reader's verdict on unit 341, as work instructions 342 to 346
  took it.
- **The reload says `CLAUDE.md` §1 holds `CPS-DEC-0163`.** Unit 346 found no `CPS-DEC` id in the file.
  It is parked with the id schemes. One line.
- **`validate-output.bat` with no argument validates another project's `output.md`.** Always pass the
  report (§9).
- **`tools\arbiter.bak-20260913\` is untracked at the root.** **Never `git add -A` or `git add .`**;
  stage files by name.
- **The tool facts in §7** are unit 346's. Say which held for you.

**Reds expected, older than this unit. Name them and do not chase them:**
- `TheOperatorCanStopItTests`: `TheStopAddedNoNewRouteToATransmission` and
  `AClickWhileTheToneIsPlayingStopsTheSoundAndNotJustTheCarrier`. **Not run.**
- `TheWholeChainRunsFromOneRightClickTests` (2), `TheMenuIsUnderTheMouseTests` (8),
  `ThePsk31RecordsAppearTests.WithNoPsk31ContactNoPsk31CardIsOnTheScreenAndNoneIsDimmed` and
  `TheTotalMilesTests.TheBadgeSaysZeroMilesOnAnEmptyLogAndTheLowestTierIsNext`. **Not run.** They are
  step 2's.

**Expected on the carry-forward list: 111 of 111 app, 86 of 86 engine**, unit 346's numbers. **If a
red turns green or a new red appears in what you ran, say which.** This unit changes no engine file,
so an engine number that moves is a finding.

**Expected red, and wanted:** once task 1's assertions are added, they are red against the tree as it
is, because the snapshot drops the grid. By ruling 19, that red is the watched red. Give its failure
line.

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
- *Screen only. Nothing here touches the radio, a decoder, a parser, the transmit chain or the log's
  content. A step's exit is what is on the screen, asserted by computation and described in words,
  then Tim's eyes. Every appearance claim is computed, not seen, and says so.*
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
    the test window only, and give the failure line. Units 345 and 346 built each watched red into
    its test as a second check, and that pattern stands. **Never break markup or a view to watch a
    test fail.** Where the new assertion is red against the tree as it is, that is the watched red,
    and it is a finding.
20. **Where the view model holds a fact the drawn page does not show, fixing it is this step's
    work.** A fix never changes what a card is earned by, what scores, any points, or the words
    rulings 11 to 16 set. A string that will not fit is shortened and named. A miss that cannot be
    fixed that way ships as `partial` with its number.
21. **Criterion 2 counts only where the earned contact's eight facts are asserted drawn** at 1400 and
    1920 on Countries, States and Grids.

**Ruling 22, as amended by work instruction 346, stands.** Only the PSK31 next-card collision on Hall
of Fame and Modes against `ACHIEVEMENTS_PHILOSOPHY.md` §3.1, and the States next card's wording, stay
for step 2. Both stay as drawn. Neither is a stop.

**Ruling 23 stands.** Unit 344's PSK31 findings are logged, not chased.

**Rulings 24 to 27** (work instruction 346) **stand as unit 346 built them**:
- the Modes next card's rows come from `ModesFor`;
- CW's place is where a band button lands, read through `HfBands.Bands`;
- who is there says only what the CQ list can know, and is never empty;
- criteria 5 and 6 count over the eight kinds, the seven continent pages and the Modes page.

**Ruling 28 is closed, and is not re-attempted.** VK2DEF's 633.18 of 892 at 1920 is
`Ft8GlobePlot.CardFrameFor`'s designed stop (unit 346 item 4). It is logged for Tim at step 3 beside
ruling 11. It is not a criterion.

**The arbiter's rulings for this unit.** They are the author's, marked for Tim at step 3, and
overrulable:

29. **A PSK31 caller's grid on the CQ list is the grid his PSK31 reading already holds, and only where
    that reading is certain.**
    - In `CqSnapshot.From`, a text-only row's call takes `Reading.Grid` where `Reading` is not null,
      `Reading.IsCertain` is true and `Reading.Grid` is a grid by the same test the FT8 path uses.
      Otherwise its grid is `""`, as today. An FT8-shaped row is unchanged.
    - **It reads and does not decode.** No change to `Psk31ExchangeParser`, `Psk31MessageSplitter`,
      the demodulator, `DigitalDecodeRow`'s construction, or how a row reaches the list. **If the
      grid cannot be had without one of those, do not build it.** Report criterion 3 `partial` with
      why. That is §6's parser stop, and the next arbiter's to hand to Tim.
    - **The grid reaches every kind's next card**, not only Modes'. This overrules work instruction
      346's *no other kind's next card* for this one fact. A PSK31 caller who sent a grid on Grids' or
      Hall of Fame's card is the same truth an FT8 caller's grid already is. Say in the report which
      other kinds' drawn next cards changed on the new fixture, and how.
    - *Why:* criterion 3 asks for callers *with distance*. R22 says *with distance, from the CQ list*.
      The list's own row holds the grid, and Hamlet discards it one line before the card.
    - *Rejected:* a distance from the callsign's entity. That is a guess drawn as a measurement
      (§0.0), and unit 346 rejected it too.
    - *Rejected:* taking the grid from an uncertain reading. `PHASE_PLAN.md` §R1 of the PSK31 phase
      says *false is a guess or an unknown, and a reader must show it as one*, and a distance in large
      type does not show a guess.
    - *Rejected:* one more Modes-only special case. The snapshot is where the fact is lost, so the
      snapshot is where it is found.
30. **Criterion 3's *with distance* counts as met where the caller's CQ carried a grid the list now
    holds. A PSK31 caller who sent none, or whose reading is uncertain, is his callsign alone**, by
    `PHASE_PLAN.md` §6 (*show what is there; no dash*) and `NextCaller`'s own remark (*or the callsign
    alone where he sent no grid*). That is the rule FT8 callers already follow.
    - **Both halves are asserted on the drawn Modes page at 1400 and 1920:**
      - a certain PSK31 CQ with a grid draws ` · n mi`, with `n` worked out by the test on its own
        from `GridPath`;
      - a PSK31 CQ with no grid draws the callsign alone;
      - an uncertain PSK31 message with a grid in it draws the callsign alone.
    - **The report states this rule in its block B line 3**, so the state reader reads the rule and
      not only the callsign.
31. **The PSK31 row's no-caller words say what the list read knows, never an unqualified *now*.**
    - The list is read once when the window opens (ruling 14). The words carry the read time, or say
      *on the CQ list*, in the sense of *no PSK31 caller on the list at 21:41 UTC*.
    - The exact words are the unit's, shortened per §6 and named. Keep criterion 3's sense: it says no
      one is calling.
    - `ListNotRead` stays for a window with no list.
    - *Why:* unit 346 item 2. On a list read while Hamlet was not listening in PSK31, *no one is
      calling in it now* claims what nobody heard.
    - *Rejected:* adding a read time to the Modes card's heading *where each one lives*. The CW and
      FT4 rows are not about the list's time, and the heading would say it for them.
32. **Every row the Modes next card can draw is measured for fit at 1400 and 1920.**
    - That covers CW, FT4, FT8 and Voice unworked; PSK31 with a grid caller and more than one caller;
      PSK31 with no caller; and PSK31 with no list.
    - The measure uses a log on the test window that leaves FT8 and Voice unworked. Say how many rows
      the card draws before *and n more*, and measure only the rows it draws.
    - **This is task 3 and the drop candidate.** Criterion 5 is already met on the pages step 1's tests
      realize (ruling 27). This widens coverage to rows no fixture draws, so dropping it does not hold
      step 1 back. Say so if it is dropped.

**Standing, transcribed:**
- **§0.0**: *Never present a guess as a decode… This binds pictures as hard as sentences.* Every
  appearance claim is computed, not seen, and says so once.
- **§0.2**, *transmit safety, ABSOLUTE*: *One operator action, one transmission.* Nothing here
  transmits or tunes. No test presses CQ, Stop, Capture, a band button or a transmit command.
- **§0.5**: *every panel is collapsible, and a collapsed panel still carries its summary. Collapsing
  hides detail, never information.*
- **§0.6**: every ink clears 4.5:1 against its fill, and color is never the only carrier.
- **R12**: *a session fixes its own tests and never asks the owner to approve it.* An existing
  assertion that the new grid makes untrue is corrected to the new truth and named, never deleted.
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
`sh tools/status.sh`. It ran for every write in unit 346. If it is refused, take a `date` reading and
paste it whole. **Never compose a time.**

The script hard-codes `RULES_AT: HM-DEC-161`. After it writes, set that line back to `HM-DEC-163
(2026-09-12)`, or to the highest id §2 finds, with the file editor.

The watchdog kills a session only when its process tree has used no CPU for ten minutes.

---

## 5. The tasks

### Task 0 - the trace: what a PSK31 reading holds and what the next cards draw from it

Measure before anything is built. **Say what you find rather than confirming §1.**

1. **Commit `WORK_INSTRUCTIONS.md`, `PHASE_STATUS.md` and a patch bump (1.13.33 -> 1.13.34, with its
   comment block).**
   - In `PHASE_STATUS.md`, change only the `WORK_INSTRUCTION:` line, to `347 - step 1, a PSK31
     caller's distance on the next card`. Do not touch its `STEP:` lines, `CURRENT_STEP` or
     `HEARTBEAT`.
   - Do not commit `.run-unit\`, `PHASE_OUTCOME.md`, `RUN_LEDGER.md`, `SESSION.lock` or
     `tools\arbiter.bak-20260913\`.
   - The message is `chore(unit347): step 1, a PSK31 caller's distance on the next card - the trace
     before the list is changed`.
   - Run the carry-forward list, status first, and give both invocations as *n of n*.
2. **Step 1's entry check.** Run `TheAchievementsPageClicksInTests` by class, then
   `TheCategoryPagesAreTradingCardsTests` by class. Give each as *n of n*. **If either is red, report
   it and stop at task 0.**
3. **Add `Unit347TraceThePsk31CallersGridAndDistance` to `TheCategoryPagesAreTradingCardsTests`.** It
   asserts nothing and presses nothing that transmits or tunes. It prints:
   - **For each of three PSK31 CQ texts**, what `Psk31ExchangeParser.Read` returns for `Speaker`,
     `Grid` and `IsCertain`, and what `CqSnapshot.From` gives that call for `Grid` and `Mode`:
     - a certain CQ with a grid, for example `CQ CQ CQ de EA3XYZ EA3XYZ JN11 K`;
     - `CallingWithPsk31()`'s own text, with no grid;
     - an uncertain one with a grid, for example the first with its turnover removed.
     
     The texts are the unit's. Say which it used.
   - **The Modes next card on the five-contact log**, drawn beside the view model, row by row, at
     1400 and 1920, on a list with each of those callers.
   - **Every other kind's next card that lists a PSK31 caller**, on the same lists at both widths:
     its caller lines as drawn. Name the kinds where the caller appears at all.
   - **The live route:** which member sets `Reading` on a text-only row the main window's list holds.
     Print `NO LIVE READING` if none does.
4. **Answer from the numbers, in section 1, before task 1 starts:**
   - Does the certain CQ's reading hold the grid, and does the snapshot drop it?
   - Is the uncertain reading's grid null, or present with `IsCertain` false?
   - Which kinds' next cards list a PSK31 caller today, and with what line?
   - Does a live PSK31 row carry the reading the fix would read?

**Drop candidate:** none. Tasks 1 to 3 are built on its numbers.

### Task 1 - the grid onto the CQ list, and the distance on the card (rulings 29 and 30)

1. **Extend the tests first (R12), and watch them red against the tree as it is.**
   - **`TheNextCardKnowsWhoIsCalling`**, at 1400 and 1920, on the five-contact log:
     - with a certain PSK31 CQ that carries a grid, the drawn PSK31 row is `3.580 on 80 m · <call>
       · n mi`, in `CallersFrom`'s join, with `n` from `GridPath` worked out in the test;
     - with `CallingWithPsk31()`, unchanged, the row is the callsign alone;
     - with an uncertain PSK31 message carrying a grid, the row is the callsign alone;
     - with the certain caller and the no-grid caller together, the nearest by miles is named, with
       ` and 1 more`.
   - Add the new caller lists beside `CallingWithPsk31()`. **Do not change `Calling()`**: every other
     test stands on its four rows.
   - **Every other kind's next card that task 0 found listing a PSK31 caller:** where an existing
     assertion becomes untrue because that caller now has a grid, correct it to the new truth and name
     it (R12). Add no new assertion there beyond what criterion 3 needs.
   - Keep every existing assertion. **The certain caller drawn without distance, on the tree as it
     is, is the watched red.** Give its failure line.
2. **Build it in `CqSnapshot.From`**, and correct its remark (`:29`-`:30`) and `WhoIsThere`'s (*a PSK31
   row carries no grid, so today its caller is his callsign alone*) to say what is now true. Change no
   parser, no splitter, no row construction, no card's earning, no score, no points, and no word
   rulings 11 to 16 set.
3. **Run the one filter:** `TheCategoryPagesAreTradingCardsTests`, `TheAchievementsPageClicksInTests`,
   `BindingHealthTests` and `VoiceTests`. Give each class as *n of n*. Then run the carry-forward list,
   status first. Commit and push.

**Drop candidate:** none. It is the named miss on criterion 3.

### Task 2 - the PSK31 row's no-caller words, and the new lines' fit (rulings 31 and 27)

1. **Extend `TheNextCardKnowsWhoIsCalling` first:** at 1400 and 1920, on `Calling()` (no PSK31
   caller), the drawn PSK31 row carries ruling 31's words, and no Modes row contains the bare word
   `now`. The tree as it is (`no one is calling in it now`) is the watched red. Give its failure line.
2. **Change the words** in `AchievementCategory` (`NoOneCallingInIt`, or where task 0 finds them).
   Shorten per §6 if they do not fit, and name it.
3. **Extend `NoStringClipsAndNoCardIsWhiteAtFourteenHundredAndNineteenTwenty`'s Modes realization**
   with the list that carries the certain PSK31 caller and the no-grid caller together, so the longest
   PSK31 line is drawn. If task 0 found a PSK31 caller on another kind's page, realize that page on
   that list too. Keep its `Fits` asserts, the 0.5 px tolerance and `white == 0`.
4. **Where a string clips or a card is white, fix it by §6**: shorten and name it, or widen. Never
   clip and never loosen.
5. **Run the one filter again**, then the carry-forward list, status first. Commit and push.

**Drop candidate:** none. Its words are a §0.0 truth on the row criterion 3 counts, and its fit is
criterion 5 on the line task 1 lengthened.

### Task 3 - every Modes row measured for fit (ruling 32, the drop candidate)

1. **Extend `NoStringClipsAndNoCardIsWhiteAtFourteenHundredAndNineteenTwenty`** with a log on the test
   window that leaves FT8 and Voice unworked, realized at 1400 and 1920:
   - on the list with a PSK31 caller;
   - with no list, so PSK31 says `ListNotRead`.
   
   Print each drawn row, and how many rows the card draws before *and n more*.
2. **Watched red** per ruling 19, built in as a second check: a Modes row held against a deliberately
   narrowed fit on the test window only. Give the failure line.
3. **Fix a clip by §6**, and name it.
4. **Run the one filter**, then the carry-forward list, status first. Commit and push.

**Drop candidate: this task, whole.** If time runs short, skip it and say which rows stay unmeasured.
Ruling 32 makes it no bar to step 1.

---

## 6. Parked - do not touch, do not raise

- **The main window, all of it.** Step 0 is done. Do not run `TheTopRowTests` or
  `TheWorkingPanelsTests`. If the carry-forward list turns one red, report it once and do not chase
  it.
- **VK2DEF's map span** (ruling 28, closed; unit 346 item 4). It is for Tim at step 3.
- **Step 2's items:**
  - the States wording, including the States next card's *Hamlet cannot tell a caller's state*;
  - the PSK31 next-card collision on Hall of Fame and Modes against §3.1 (ruling 22). This unit
    gives a PSK31 caller his grid and does not take PSK31 off either card;
  - the Modes test unit 336 named;
  - the undeletable files, the points file's comment block, the small `Views` reds and
    `TheTotalMilesTests`.
- **Every next card's words other than the PSK31 Modes row's no-caller line.** That includes Bands'
  empty no-caller argument and the CW, FT4, FT8 and Voice rows' words, which stand as unit 346 built
  them.
- **The achievements window's own size** (`1040 x 720`; unit 332 item 3).
- **PSK31 decoding, all of it** (ruling 23): the parser, the splitter, the demodulator, unit 344's five
  items and unit 337's carried repair. **This unit reads a field the parser already fills and changes
  nothing that fills it.**
- **`TheOperatorCanStopItTests`, all of it.** Transmit-side.
- **The engine carry-forward's 86 against 87** (unit 345 item 1), and the missing version comment
  blocks at 1.13.29 and 1.13.31. Findings; not repaired here.
- **Carried in `PHASE_PLAN.md` §7 and belonging to Tim or the harness:**
  - the live license lookup and the live heard count;
  - the two id schemes, including `CPS-DEC-0163`;
  - real flags on country cards, PSK31 step 6, real PSK31 audio, the ALC margin and the map bitmap's
    license;
  - the demodulator and the `Why` hovers (`ModeFirstRow.Why` stays undrawn);
  - the launcher's `PHASE_OUTCOME.md` header, its missing unit 341 and 344 entries, its unit
    numbering, and `tools/status.sh`'s hard-coded `RULES_AT`.
- **Anything touching the radio, a decoder, a parser or the transmit chain. Any package.**

## 7. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.** HM-DEC-155.
- **No grid from an uncertain reading, and no distance from a callsign's entity** (ruling 29, §0.0).
- **No edit under `src/Hamlet.RadioEngine/Psk31/`**, and no change to how a `DigitalDecodeRow` is built
  or reaches the list (ruling 29). If the fix needs one, stop building it and report `partial`.
- **Never say *now* where the list was read once** (ruling 31, ruling 14).
- **Do not change `Calling()`**, what a card is earned by, what scores, any points, or the words
  rulings 11 to 16 set.
- **Do not loosen a threshold or tolerance, and do not break the view to watch a test fail** (ruling
  19, §6).
- **No image assets, no second map control, no new map bitmap** (R22).
- **Do not edit `PHASE_OUTCOME.md`, `RUN_LEDGER.md`, the `STEP:` lines, `.run-unit\allowed.txt` or
  anything under `tools\arbiter\`.** They are the launcher's.
- **Never `git add -A` or `git add .`** (§2).
- `CLAUDE.md` §12.6 covers the rest. **No package. Report mismatches. Write American.**
- **The tool facts, as unit 346 found them.** Say which held for you:
  - **ran:** `sh tools/status.sh` (every write); `&&` joining `git add`, `commit` and `push`; commits
    with several `-m`; `grep`, `tail`, `cut`, `uniq` and `tr` in pipes; `git diff --quiet ... &&
    echo`; `date`;
  - **asked for approval, not run:** a `cd` before `git` in a compound; `grep` with `\s` in its
    pattern; `awk` in a pipe. This arbiter also found `sort` with options, and `git fetch` or `git
    status -sb` in a compound, asking for approval;
  - **older and not retested:** output redirects and `sed -i` blocked; `git -C`; `git show` with
    several hashes; an apostrophe in an argument breaks the `.bat` tools; doubled backslashes
    collapse; use exact-text edits for markup; a `public const` cannot be bound, so bind a property
    built from it.

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
   with nothing qualifying them, was 6 of 7 after the state reader on
   unit 346 (criterion 3: a PSK31 caller drawn without distance); <which
   do not, and why>. Steps 2 and 3 not started.
B. Step 1 and its exit criteria - the one gap the state reader named
   first, then the rest re-run:
   entry: TheAchievementsPageClicksInTests <n of n>, run first
   3. next card names its want and its callers with distance, or no one
      calling - the rule (ruling 30): a distance wherever the caller's
      CQ carried a grid the list holds, the callsign alone where it
      carried none or the reading was uncertain. Modes PSK31 row at 1400
      and 1920: certain CQ with a grid <drawn line>; no grid <drawn
      line>; uncertain with a grid <drawn line>; both callers <drawn
      line>; no PSK31 caller <drawn line, ruling 31's words>.
      Grid source: <the member read, and whether a live row carries it>.
      Other kinds whose next card changed: <kind and line, or none>
   5. no string clips or wraps a word - <pages and lists measured>;
      <test, result>; every Modes row <task 3's rows and result, or
      DROPPED with the rows left unmeasured>
   1, 2, 4, 6, 7 and the nice-to-pass - <test and result each, re-run
      in the one filter>
C. The report last. Section 4 raises N items on top of the carried queue;
   say which, if any, stands in the way of a criterion in B - in
   particular whether taking the grid needed anything beyond
   CqSnapshot.From, and whether any line now claims more than the CQ
   list can know.
```

**Every line specific to this unit.** If a criterion was not measured, say *not measured*. Do not
fill the shape.

```
UNIT:       347 - <complete|stopped> at task N of 3 - <date time, read from the clock>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no> - <whether criterion 3 now holds with nothing the
            state reader named on unit 346 still open>
NUMBER:     step 1 must-pass held on the drawn page at 1400 and 1920 with
            nothing qualifying them: 6 of 7 -> <n> of 7
DRIFT:      0
```

**Section 3 leads with the answer:** does every step 1 must-pass now hold, with a PSK31 caller who
sent a grid drawn with his distance? If not, which criterion, at which width, on which page, and by
how much. Then, in words, top to bottom, at both widths:
- the Modes next card's PSK31 row on each caller list;
- any other kind's next card that now draws a PSK31 caller's distance;
- the Modes rows task 3 measured, if it ran.

Then the step 1 table: test, criterion, drawn or view model, widths, pages, result, numbers. **Every
appearance claim is computed, not seen. Say so once.**

**Section 4:** unit 346's section 4 verbatim per HM-DEC-139, from its line under
`## 4. What's blocking us` to its end, as committed in `0b506ed`, including the queues it carries. Keep
it in place with the file editor if redirects are blocked. In the carried text, mark:
- unit 346 item 1 (the PSK31 caller without distance): `TAKEN UP by work instruction 347 rulings 29
  and 30 and task 1`, with what the row now draws;
- unit 346 item 2 (`no one is calling in it now`): `TAKEN UP by work instruction 347 ruling 31 and
  task 2`, with the new words;
- unit 346 item 3 (the FT8, Voice and list-not-read rows): `TAKEN UP by work instruction 347 ruling 32
  and task 3`, with the result, or `DROPPED by unit 347 - task 3 was the drop candidate`;
- unit 346 item 4 (VK2DEF): `NOTED by work instruction 347 - ruling 28 closed, for Tim at step 3`.

Then anything this unit raises. **A ruling is wanted only where Tim must decide. Everything else is a
finding, and says so.** Rulings 11 to 32 are already marked for Tim at step 3. Do not raise them again
unless the page shows something that changes one.

---

```
ARBITER-DECISION
STEP: 1
APPROACH: carry the grid the PSK31 reading already holds onto the CQ list in CqSnapshot.From, only where the reading is certain, so a PSK31 caller who sent a grid is drawn with distance on every next card and one who sent none is his callsign alone by section 6; the PSK31 row's no-caller words tied to the list's read time instead of now; every Modes row measured for fit as the drop candidate; asserted drawn at 1400 and 1920
MOVE: work around
WHY: Unit 346 closed both gaps it was aimed at and the state reader left step 1 partial on one remaining clause - criterion 3's distance, missing for a PSK31 caller because CqSnapshot.From discards the grid the row's PSK31 reading already holds. Reading that field is a different approach from filling the Modes rows or proving coverage, and the loop test found nothing like it.
STATE: partial
DECIDED: ruling 29 - a PSK31 call's grid on the CQ list is Reading.Grid where the reading is certain, read in CqSnapshot.From with no change to the parser, splitter or row construction, and it reaches every kind's next card (overrules work instruction 346's no-other-kind limit for this fact; rejects entity-centroid distance and uncertain grids); ruling 30 - criterion 3's with-distance counts as met where the caller's CQ carried a grid the list holds, the callsign alone otherwise by PHASE_PLAN.md section 6, asserted both ways on the drawn Modes page; ruling 31 - the PSK31 row's no-caller words carry the list's read time or say on the CQ list, never an unqualified now (unit 346 item 2, ruling 14's precedent); ruling 32 - every Modes row measured for fit, the drop candidate and no bar to step 1; ruling 28 closed, VK2DEF's span logged for Tim at step 3 (unit 346 item 4) - all the author's, marked for Tim at step 3, overrulable
LICENCE: PHASE_PLAN.md sections 1, 2, 4 (step 1 exit criterion 3 and 5), 6 (the arbiter decides and continues; show what is there, no dash; never loosen a test; a parser change stops) and 7; .run-unit/state-verdict.json (step 1 partial on unit 346: the PSK31 caller EA3XYZ drawn with no distance); .run-unit/s4-verdict.json (none); unit 346 output.md section 4 items 1 to 4; docs/phase-maintenance-run/PHASE_PLAN.md R22 (with distance, from the CQ list); docs/phase-psk31-run/PHASE_PLAN.md R1 (an uncertain parse is shown as a guess); src/Hamlet.App/ViewModels/CqSnapshot.cs From and its remark at 0b506ed; src/Hamlet.RadioEngine/Psk31/Psk31ExchangeParser.cs Psk31Exchange.Grid and IsCertain; src/Hamlet.App/ViewModels/DigitalDecodeRow.cs Reading and Payload; AchievementCategory.cs WhoIsThere, CallersFrom and NextCaller's remark; TheCategoryPagesAreTradingCardsTests CallingWithPsk31 at 2950; CLAUDE.md 0.0, 0.2, 0.5, 0.6; psk31 R12, R13, R14, R19; HM-DEC-139, HM-DEC-155, HM-DEC-163
ACCOMPLISHED: a station calling CQ in PSK31 who sends his grid shows on the achievements next cards with how far away he is, the same as an FT8 caller does, and a station who sends none shows as his callsign without a made-up number; the PSK31 row no longer says no one is calling now when it only means no one was on the list when the window opened - so every step 1 criterion stands on the drawn page at both of Tim's widths with nothing left for the state reader to name
ADVANCES: step 1 - must-pass 3 (callers listed with distance; the no-caller line true to the list) in tasks 1 and 2; must-pass 5 (the lengthened PSK31 line fits) in task 2 and over every Modes row in task 3, the drop candidate
END-ARBITER-DECISION
```
