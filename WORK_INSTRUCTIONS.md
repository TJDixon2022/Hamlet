# Work instruction 349 - step 3, what Tim looks at

Step 3 of `PHASE_PLAN.md`, **first unit on it.**

- **Step 0** is taken as done on the state reader's verdict on unit 341, as work instructions 342 to
  348 took it. Unit 348 read its two classes once at `07f48d9`: `TheTopRowTests` 8 of 8,
  `TheWorkingPanelsTests` 8 of 8.
- **Step 1 is done** on the state reader's verdict on unit 347.
- **Step 2 is done.** The state reader read unit 348's report and returned `done`, in these words:

> *All five must-pass criteria are backed by quoted evidence, namely StatesCountWhatTheLogsStateFieldSays
> passing with 2 worked, from STATE and no confirm drawn at 1400 and 1920, the renamed Modes test passing
> with a watched red, the fifteen files in one list in section 2, the eight TheMenuIsUnderTheMouseTests
> reds and TheWindowDrawsEverySixRows named with why, and the two-way points test passing with 24 keys
> documented and 24 in the file.*

**So the plan moves to step 3, whose entry is *step 2 done*.** Step 3 has one exit criterion, *Tim says
it passed*, and the plan says *No script can evaluate this.* **This unit cannot meet it and does not
try.** It prepares everything Tim needs to give a verdict in one sitting at his window. Then the phase
waits for him.

It has four tasks, 0 to 3. Task 3 is the drop candidate.

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

**Also:** `.run-unit\allowed.txt` must permit `dotnet`. It carried `Bash(dotnet:*)` at line 14 for this
arbiter. If `dotnet test` is refused, stop at task 0, write the report, and say so in section 4,
quoting the refused command. **Do not route around it**: no `tools/tests/run.js`, no assertions written
unrun, and no edit to `allowed.txt` or `run-unit-tools.txt`. Unit 343 item 1 rejected all three, and
those rejections stand.

---

## 1. Why this unit exists

**The number:** step 3 must-pass met is **0 of 1**, and it stays 0 of 1 after this unit. It can only be
met by Tim.

**What stands between Tim and that verdict, read from the tree by this arbiter on 2026-09-13 at
`26e5879`. None of it has been run.**

- **No one place says what to look at.** The phase's evidence is spread across twelve reports. Unit
  348's `output.md` alone is 1,957 lines, and most of that is the carried queue. Each report's own
  *what you should see* covers its own unit.
- **Forty decisions were made for Tim, marked *for Tim at step 3, overrulable*, and never gathered.**
  - These are the arbiter's rulings 1 to 40, in work instructions 338 to 348. They are in git history
    (`git log -- WORK_INSTRUCTIONS.md`), and some are in `PHASE_OUTCOME.md`'s `DECIDED` lines.
  - Units made more choices on top of them, each marked as the unit's own. Examples: a string shortened
    under §6, the green block's upgrade row hidden (unit 341 item 3), and the States band losing *the 50
    states* (unit 348 item 4).
  - A verdict of *passed* also accepts all of these. Tim cannot accept what he has not been shown.
- **Several things the record names will look wrong on his screen but are known.** For example:
  - VK2DEF's map spans 633 of 892 px (unit 346 item 4);
  - the PSK31 top row at 1400 holds by 1.4 px while the best bet draws (unit 341 item 1);
  - the power offer's ink clears 4.5:1 by 0.11 (unit 341 item 5);
  - *1 US contact carries no STATE* was measured only at one digit (unit 348 item 3).

  No list of them exists for Tim.
- **No picture can be made here.** The tests run on Avalonia's headless drawing backend, which
  rasterizes nothing. `TheMarksRenderTests.cs:154`-`161` records that `CopyPixels` throws and a captured
  frame is blank. `Unit300SizesTests.WhetherAnythingHereCanActuallyLook` exists to print whether that
  has changed. A raster would need `Avalonia.Headless.Skia`, a package. So the thing Tim reads is
  **words and numbers, computed and not seen**, and his window is the first eyes on it.
- **The numbers the sheet would quote are from reports, not from this tree.**
  - Units 341 to 348 each ran their own classes.
  - No single run at `26e5879` covers step 0's four classes, step 1's two and step 2's two together.
- **Tim's window size is not in the record.** `PHASE_PLAN.md` says *at his window size*. R25 (the
  maintenance plan) says *at the size he uses*. `SHACK_FACTS.md` names none. Every measurement is at
  1400 and 1920 wide, on a host 1040 px tall.

```
PHASE GOAL: The screen, done right - the main window as the approved mockup
            (step 0, done on the state reader's verdict on unit 341); every
            achievements category page as trading cards (step 1, done on unit
            347); what the last phase left (step 2, done on unit 348, 5 of 5);
            then Tim at his window says it passed (step 3, not started, 0 of 1).
UNIT GOAL:  Put the whole phase in front of Tim on one sheet he can check at
            his window in one sitting: every page, what it should show at 1400
            and 1920 with the number and the test that measured it at this
            tree, every decision made for him with how to overrule it, and
            every known thing that will look wrong but is not.
ADVANCES:   none - this unit clears the blocker in front of step 3's one
            criterion. Tim cannot say "passed" to forty decisions and a dozen
            known imperfections nobody has gathered, and the numbers he would
            check are scattered over twelve reports and not re-run on this
            tree. Tasks 0 to 2 clear it. Task 3, the drop candidate, closes
            one unmeasured fit the sheet would otherwise carry as arithmetic.
DRIFT:      0
```

---

## 2. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any mismatch.
**Report the mismatch; do not repair the instruction.** Mismatches go in the report even when the
work succeeds anyway.

Check:
- **Every file, line and item §1 cites.**
- **Unit 348's commits** are `02884b1`, `07f48d9`, `1684d51`, `36c3a7b`, `8895891` and `26e5879`, each
  pushed. `origin/main` and `HEAD` read `26e5879` for this arbiter. `output.md` in the tree is unit
  348's.
- **The version is 1.13.35** in `Directory.Build.props` (line 793 for this arbiter).
- **`DECISIONS.md` tops at HM-DEC-163.**
- **The launcher's files still disagree with themselves.** One line each; edit none:
  - `PHASE_OUTCOME.md`'s header reads step 0 `in progress`. The reload reads the last step 0 entry
    (unit 340, `blocked`) as winning. There is no unit 341 or 344 entry.
  - `PHASE_STATUS.md` reads `CURRENT_STEP: 0`, with steps 1 and 2 `done`.
  - Unit 348 is recorded as `UNIT 4 - STEP 2`.
  - `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md` and files under `.run-unit\` are modified and
    uncommitted by the launcher.
- **The reload says `CLAUDE.md` §1 holds `CPS-DEC-0163`.** Unit 348 found no `CPS-DEC` id in the file.
  It is parked with the id schemes. One line.
- **`validate-output.bat` reads the `UNIT:` line only in the report's first 60 lines.** Keep the
  ordering block and header inside them, and always pass the report (§9).
- **`tools\arbiter.bak-20260913\` is untracked at the root.** **Never `git add -A` or `git add .`**;
  stage files by name.
- **The tool facts in §7** are unit 348's. Say which held for you.

**Reds expected, older than this unit. This unit runs none of them:**
- `TheMenuIsUnderTheMouseTests` (8), named with why by unit 348;
- `TheAchievementsScreenTests.TheWindowDrawsEverySixRows` (1), named with why by unit 348;
- `TheOperatorCanStopItTests` (2) and `TheWholeChainRunsFromOneRightClickTests` (2), **never run.**

They go on the sheet's known-reds lines as unit 348 named them, citing `26e5879`.

**Expected green:**
- **the carry-forward list:** 111 of 111 app, 86 of 86 engine, unit 348's numbers;
- **step 0's classes:** `TheTopRowTests` 8 of 8 and `TheWorkingPanelsTests` 8 of 8;
- **step 1's classes:** `TheCategoryPagesAreTradingCardsTests` 13 of 13, including unit 348's trace,
  and `TheAchievementsPageClicksInTests` 8 of 8;
- **step 2's classes:** `ThePsk31RecordsAppearTests` 4 of 4 and `TheAchievementsPageTests` 10 of 10;
- `BindingHealthTests` 1 of 1 and `VoiceTests` 5 of 5.

**If a red turns green, a green turns red, or a count moves, say which. That finding goes on the sheet,
and it is not chased (ruling 41).** This unit changes no source file, except task 3's one string if it
clips.

---

## 3. Rulings in force - do not re-argue

**HM-DEC-155**, transcribed: *A unit runs no test suite. It may run only the test it constructs in
that work instruction, filtered by exact name, in the foreground, with a stated timeout, and it never
backgrounds a command and polls for it.*
- Run the carry-forward list as the top comment of `docs\carry-forward-tests.txt` says: two
  invocations, one build each, with status written immediately before each.
- **The classes this instruction names may run filtered by class name**, in the filters task 0 gives:
  `TheTopRowTests`, `TheWorkingPanelsTests`, `BindingHealthTests`, `VoiceTests`,
  `TheCategoryPagesAreTradingCardsTests`, `TheAchievementsPageClicksInTests`,
  `ThePsk31RecordsAppearTests` and `TheAchievementsPageTests`.

**Step 3, `PHASE_PLAN.md` §4, in full:**

> **Delivers:** Tim opens every page at his window size and says it passed.
> **Entry:** step 2 done.
> **Exit:** Tim says it passed. *must-pass* No script can evaluate this.

**R25, Tim (`docs/phase-maintenance-run/PHASE_PLAN.md`, standing by `PHASE_PLAN.md` §2):** *Step 3 is
his verdict at his window, at the size he uses.*

**R26, Tim, 2026-09-12 (`PHASE_PLAN.md` §R), in full**, because half the sheet is checked against it:

> **R26 - the main window.** Tim, on unit 334's screen: the green zone had grown to two thirds of the
> window and the working panels were squeezed into the bottom third. The mockup answers it, and its
> outcomes are the rulings:
> - **The top row is one band, about 190 px tall at 1920**, and the working panels below the tabs take
>   the rest of the window. At no window size do the working panels get less than half the height
>   below the band pills.
> - **The neighborhood card carries three things**: the band strip with the legend and the *you · mode*
>   marker; **the green block** - the band in the largest text, the frequency, mode and *yours to use*,
>   the license line small, the rule-of-thumb line small, and heard-just-now with its count and
>   sparkline; and **the world's clock** at its right end, about 246 px wide, with the operator's dot
>   only.
> - **The rig display is the same height as the neighborhood card**, and carries under the frequency
>   and S-meter the transmit drive and the RF power offer, so no empty column stands under it.
> - **The band pills stay where they are** and are not repeated anywhere.
> - **Below the tabs**: waterfall, decoded text, For You, all the same height, full to the status bar.
>   The decoded list is as wide as its longest line needs and no wider; For You takes the rest, wide
>   enough that the card's facts sit beside its map.
> - **At 1400**: the same shape; where the card's facts cannot sit beside the map they go under it; no
>   callsign is ever clipped in the decoded list. **No mechanism is prescribed; the unit measures and
>   chooses, marks the choice as its own, and reports the numbers at both widths.**

**R22** (the category pages as trading cards, `docs/phase-maintenance-run/PHASE_PLAN.md`) and **R27**
(what the last phase left, `PHASE_PLAN.md` §R) are transcribed in full in work instruction 348 §3, at
`02884b1`. Read them there. The sheet checks against both.

**The images the sheet points Tim at:** `assets/main-screen-mockup.png`,
`assets/category-page-countries.png` and `assets/achievements-opening-mockup.png` (`PHASE_PLAN.md`,
the reasoning under the step list).

**`PHASE_PLAN.md` §1, §6 and §7, the lines that bite here:**
- *Screen only. Nothing here touches the radio, a decoder, a parser, the transmit chain or the log's
  content. A step's exit is what is on the screen, asserted by computation and described in words,
  then Tim's eyes. Every appearance claim is computed, not seen, and says so.*
- *The arbiter stops for three things only: keying, transmit or the radio's safety; money past the
  budget; a decision that changes what the product promises the operator. On everything else it
  takes its own recommendation, marks it author's and overrulable, applies it, and continues.*
- *A must-pass is missed by a little: ship, report, `partial`, move on. Never loosen a test.*
- *A string will not fit: shorten and say which, or widen; never clip.*
- *Anything touches the radio, a decoder, a parser or the transmit chain: `MOVE: stop`.*
- *A package is needed: `MOVE: stop`.*
- *Carried: every open ask from unit 336's queue, verbatim in every unit. Plus: real flags on earned
  country cards - undecided, Tim's; the PSK31 phase's step 6 - Tim's, at the radio; the recording of
  real PSK31 audio; the id-scheme split; the map bitmap's license.*

**The arbiter's rulings 1 to 40 stand as units 337 to 348 built them.** They are the author's, marked
for Tim at step 3, and overrulable. **This unit gathers them; it does not re-open, re-word or re-build
any of them.** Two bite here:
- **19. Watching red.** Show a new assertion fail once, against a deliberately wrong expectation set on
  the test window only, and give the failure line. **Never break markup or a view to watch a test
  fail.** Where the new assertion is red against the tree as it is, that is the watched red.
- **20.** A fix never changes what a card is earned by, what scores, any points, or the words rulings
  11 to 16 set. A string that will not fit is shortened and named.

**The arbiter's rulings for this unit.** They are the author's, marked for Tim at step 3, and
overrulable:

41. **Step 3 is entered, and the phase's classes are read once at this tree.**
    - Step 2 is `done` on the state reader's verdict on unit 348. Step 0 is read as done as work
      instructions 342 to 348 read it. The launcher's missing unit 341 entry is the launcher's, and it
      is parked.
    - Task 0 runs step 0's, step 1's and step 2's classes once each at the tree as it opens, so every
      number on the sheet is from **this** tree.
    - **This is a reading, not a re-proof.** A red is reported with its failure line, and it goes on
      the sheet as a red. It is **not fixed**. The one exception is task 3's string (ruling 45).
    - **No step 0, 1 or 2 work is authored here.** If a red appears where §2 expects green, the
      report says so in block B. The next arbiter decides whether the step re-opens.
42. **No pictures. The sheet is words and numbers.**
    - The headless backend rasterizes nothing (§1). A package is a §6 stop.
    - *Rejected:*
      - adding `Avalonia.Headless.Skia` (a package);
      - running `Hamlet.App` to capture a window. It is the whole app, and the radio side is outside
        a unit; the arbiter did not measure what it opens at start, and the rejection holds either
        way on §0.2 and §1;
      - drawing a picture of what the page *should* look like (§0.0 binds pictures as hard as
        sentences).
    - The sheet points Tim at the three approved images in `assets\`, beside the words.
43. **The sheet: one file, `docs\unit349-what-tim-looks-at.md`, in the order Tim uses it.**
    1. **How to look.** Tim's window size is not in the record. The sheet gives both measured widths,
       names the host height (1040 px), and asks him to write his size in his verdict. It says once:
       *every appearance claim here is computed on a test host, not seen; your window is the first
       eyes on it.*
    2. **Page by page.** Each page is one block:
       - the main window on FT8, and on PSK31 with the power offer drawn;
       - the achievements opening page;
       - each of the eight kinds;
       - Continents and one continent page, standing for all seven.

       Each block says how to get there in clicks. It says what R26 or R22 promises there, in the
       ruling's words, and what the tree draws, in words and numbers at 1400 and 1920, each with the
       test that measured it in task 0. Then it gives what to look for with his eyes that no test can
       see: legibility, color, balance, *not white bread boring*.
    3. **Decided for you** (ruling 44).
    4. **What will look wrong but is known.** One line each, citing the report item.
    5. **Known reds**, one line each, from unit 348's table.
    6. **Yours and not this phase's**: `PHASE_PLAN.md` §7's carried list, one line each.
    7. **Your verdict.** A short form: *passed*, or *not passed* with the page, the width and what is
       wrong. Every ruling in section 3 stands unless he names it.

    **Every claim on the sheet has a source**: a test that ran green in task 0 at this tree, with its
    number, or a report's commit and item. **A claim with neither is not on the sheet.** No number is
    composed. The length should let Tim go through it at his window in one sitting. Where a block runs
    long, cut the words, not the pages.
44. **Decided for you: every decision made for Tim, one line each, gathered from git.**
    - **The arbiter's rulings 1 to 40**, from work instructions 338 to 348. Read each with
      `git log --format=%h -- WORK_INSTRUCTIONS.md` and `git show <hash>:WORK_INSTRUCTIONS.md`. Where
      the work instruction a ruling came from is not in history, say so and use `PHASE_OUTCOME.md`'s
      `DECIDED` line.
    - **Each unit's own choice that changed a word or hid something on the screen**, from the
      *decisions made for this unit* and §6 shortenings in reports 337 to 348. Read each with
      `git show <hash>:output.md`, at each unit's `docs(unitNNN)` commit.
    - **Each line gives:** the number or unit, what was decided, where it shows on which page, the
      alternative that was rejected, and the words that overrule it. For example, *say "put the
      upgrade row back"*.
    - **A ruling with no screen effect** (about tests, runs or the record, such as 17 to 19, 33, 38 and
      40) is grouped on one line per work instruction, marked *no screen effect*.
    - **Give the count:** rulings with a screen effect, rulings without one, and unit choices.
45. **Task 3, the drop candidate: the no-`STATE` line at four digits.**
    - Unit 348 item 3 left *1,234 US contacts carry no STATE* as arithmetic, not a measurement. Tim's
      log will draw a number that size.
    - Extend `StatesCountWhatTheLogsStateFieldSays`, or add one fixture inside it, so that a log whose
      no-`STATE` US records number at least 1,000 draws the whole line with its thousands separator at
      1400 and 1920, with no clip and no wrapped word. Measure it the way step 1's clip test measures.
    - **If it is already whole**, the assertion is green on the tree. Watch it red by ruling 19, on the
      test window only.
    - **If it clips**, shorten the words under §6 and ruling 20, keeping *STATE* and the number, and
      name the change on the sheet. Nothing else in `src` changes.
    - *Why:* criterion 5 of step 1 (*no string clips*) should hold on the number Tim will actually see,
      and the sheet should not hand him arithmetic.
46. **Section 4 carries one real ask, and it is Tim's verdict.**
    - Raise one item: *step 3 - Tim's verdict, pass or not, at his window size, with
      `docs\unit349-what-tim-looks-at.md` as the list*.
    - Everything else this unit finds is a finding, and says so.
    - **After this unit, no session can move step 3.** The next arbiter should author nothing further
      into step 3 until Tim answers, unless the sheet is shown wrong.
    - A verdict of *not passed* that names a page is new work for the step it names. It is not a
      re-opened phase, because the phase is not done.

**Standing, transcribed:**
- **§0.0**: *Never present a guess as a decode… This binds pictures as hard as sentences.* Every
  appearance claim is computed, not seen, and says so once.
- **§0.2**, *transmit safety, ABSOLUTE*: *One operator action, one transmission.* Nothing here
  transmits or tunes. No test this unit writes presses CQ, Stop, Capture, a band button, a send or a
  transmit command.
- **§0.5**: *every panel is collapsible, and a collapsed panel still carries its summary. Collapsing
  hides detail, never information.*
- **§0.6**: every ink clears 4.5:1 against its fill, and color is never the only carrier.
- **R12**: *a session fixes its own tests and never asks the owner to approve it.*
- **R13**: telemetry on every new stage. This unit adds no stage.
- **R14**: *a test exists to prove an exit criterion.* Add no test except the trace and task 3's
  assertion.
- **R16**: the two quills. The sheet does not redraw or re-describe them beyond what the pages draw.
- **R19**: American spelling, on the sheet as everywhere.
- **HM-DEC-139**: open asks are carried verbatim until answered.

---

## 4. Status cadence

Write status **before every `dotnet` command, after every commit and after every task**, with
`sh tools/status.sh`. It ran for every write in unit 348. If it is refused, take a `date` reading and
paste it whole. **Never compose a time.**

The script hard-codes `RULES_AT: HM-DEC-161`. **After every write**, set that line back to
`HM-DEC-163 (2026-09-12)`, or to the highest id §2 finds, with the file editor.

The watchdog kills a session only when its process tree has used no CPU for ten minutes. The sheet is
long writing with no process running. **Write status between its sections**, so a quiet stretch of
editing is never ten minutes of nothing.

---

## 5. The tasks

### Task 0 - the trace: the phase at this tree, and the decisions in git

Measure before anything is written. **Say what you find rather than confirming §1.**

1. **Commit `WORK_INSTRUCTIONS.md`, `PHASE_STATUS.md` and a patch bump (1.13.35 -> 1.13.36, with its
   comment block).**
   - In `PHASE_STATUS.md`, change only the `WORK_INSTRUCTION:` line, to `349 - step 3, what Tim looks
     at`. Do not touch its `STEP:` lines, `CURRENT_STEP` or `HEARTBEAT`.
   - Do not commit `.run-unit\`, `PHASE_OUTCOME.md`, `RUN_LEDGER.md`, `SESSION.lock` or
     `tools\arbiter.bak-20260913\`.
   - The message is `chore(unit349): step 3, what Tim looks at - the trace before a line is written`.
   - Run the carry-forward list, status first, and give both invocations as *n of n*.
2. **The phase's classes, once each (ruling 41)**, status before each, with a stated timeout:
   - step 0: `TheTopRowTests`, `TheWorkingPanelsTests`, `BindingHealthTests` and `VoiceTests` in one
     filter;
   - steps 1 and 2: `TheCategoryPagesAreTradingCardsTests`, `TheAchievementsPageClicksInTests`,
     `ThePsk31RecordsAppearTests` and `TheAchievementsPageTests` in one filter.

   Give each class as *n of n*, and the failure line of any red. **Do not stop for a red; do not fix
   one.**
3. **Add `Unit349TraceWhatTimLooksAt` to `TheCategoryPagesAreTradingCardsTests`.** It asserts nothing
   and presses nothing that transmits or tunes. At 1400 and 1920 it prints, per page, in the words and
   numbers the sheet will quote:
   - **The achievements opening page:** each badge's title, standing and next.
   - **Each of the eight kinds** on the twelve-contact log, and States on `StateContacts()`:
     - the band line;
     - each earned card's entity, callsign and grid, distance, band, mode, date and points, and its
       map's size;
     - the next card's lines and callers.
   - **Continents, and each continent page:** the seven badges, and each page's first card and next
     card.
   - **Any drawn run that the page's own clip measure finds clipped or wrapped**, or `NOTHING CLIPPED`.

   For the main window, **print nothing new.** Take the numbers from task 0 step 2's classes'
   own output: top row, neighborhood card, rig panel, green block text sizes, world clock, the three
   panels, and facts beside or under, on FT8 and PSK31. If a class prints no number the sheet needs,
   say which, and quote the latest report that measured it, with its commit.
4. **The decisions, from git (ruling 44).** List, but do not yet write up:
   - each work instruction 337 to 348's commit hash, and the ruling numbers it carries;
   - each report 337 to 348's `docs(unitNNN)` commit, and the unit choices it names that changed a
     word or hid something on the screen.

   Say any ruling number not found. The work instructions for units 341 and 344 may be missing from
   the outcome file but present in git.
5. **The known list, from the reports.** Every item in reports 337 to 348 that says a thing will look
   wrong, holds by a small margin, or is measured only by arithmetic. Give each one's commit and item
   number.
6. **Answer from the numbers, in section 1, before task 1 starts:**
   - Did every class in step 2 read as §2 expects? If not, which moved?
   - How many pages does the sheet cover, and how many drawn runs did the trace read per width?
   - How many rulings have a screen effect, how many have none, and how many unit choices are there?
   - How many known items are there?
   - Was anything clipped at either width?

**Drop candidate:** none. Every line of the sheet rests on it.

### Task 1 - the sheet (rulings 42 to 44)

1. **Write `docs\unit349-what-tim-looks-at.md`** in ruling 43's seven sections, from task 0's output
   only. Status between sections (§4).
2. **Words for Tim**, as `CLAUDE.md` §8 asks of section 2:
   - no test names in the page blocks; the test and number go in a short source note under each block;
   - American spelling;
   - nothing that shames.
3. **Commit and push**: `docs(unit349): what Tim looks at - every page, every decision made for him,
   every known thing, at 1400 and 1920`.

**Drop candidate:** section 6 of the sheet (*yours and not this phase's*). Replace it with a single
pointer to `PHASE_PLAN.md` §7 if time is short.

### Task 2 - the sheet checked against its sources

1. **Read the sheet claim by claim.** For every number and every *draws*, find its source in task 0's
   output or in the cited commit and item.
2. **Correct or remove any claim with no source, or a wrong one.** Give the count of claims checked,
   corrected and removed in section 1. Name each correction.
3. **Check every overrule line** in *decided for you*: does it name a thing Tim can see or say? A line
   that could only be acted on by reading code is rewritten in his terms.
4. **Commit and push if anything changed**: `docs(unit349): the sheet checked - <n> claims, <n>
   corrected, <n> removed`.

**Drop candidate:** none. An unchecked sheet is worse than none, because Tim would pass on it.

### Task 3 - the no-`STATE` line at four digits (ruling 45, the drop candidate)

1. **The assertion first**, at 1400 and 1920, as ruling 45 says. Give the watched red, or the red on
   the tree.
2. **If it clips**, shorten per ruling 45, and name the old and new words.
3. **Run the steps 1 and 2 filter again** (task 0 step 2), then the carry-forward list, status first.
4. **Update the sheet's States block and its known list** with the measured number. Commit and push.

**Drop candidate: this task, whole.** If time runs short, skip it. The sheet's known list keeps unit
348 item 3 as arithmetic, and says so.

---

## 6. Parked - do not touch, do not raise

- **The screen, all of it**, except task 3's one string if it clips. Rulings 1 to 40 stand as built.
  The sheet reports them; it does not change them.
- **Every red in the record.** It goes on the sheet as it stands.
- **Every test except** the trace and task 3's assertion.
- **Pictures**: no package, no running app, no drawn mockup (ruling 42).
- **The log's content**, including `STATE` in Hamlet's own entries.
- **PSK31 decoding, all of it** (ruling 23).
- **Transmit, all of it:** `TheOperatorCanStopItTests`, `TheWholeChainRunsFromOneRightClickTests`, and
  `TheMenuIsUnderTheMouseTests`.
- **The fifteen emptied files.** They are on unit 348's list. The sheet points to it in one line.
- **Carried in `PHASE_PLAN.md` §7 and belonging to Tim or the harness:**
  - the live license lookup and the live heard count;
  - the two id schemes, including `CPS-DEC-0163`;
  - real flags on country cards, PSK31 step 6, real PSK31 audio and the map bitmap's license;
  - the launcher's `PHASE_OUTCOME.md` header, its missing unit 341 and 344 entries, its unit
    numbering, and `tools/status.sh`'s hard-coded `RULES_AT`.
- **Anything touching the radio, a decoder, a parser or the transmit chain. Any package.**

## 7. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.** HM-DEC-155.
- **Never run `TheOperatorCanStopItTests`, `TheWholeChainRunsFromOneRightClickTests` or
  `TheMenuIsUnderTheMouseTests`.**
- **Do not fix a red the trace finds** (ruling 41). Report it, and put it on the sheet.
- **Do not re-word a ruling to make it read better on the sheet.** Quote it or summarize it, and cite
  it.
- **Do not put on the sheet a number or a *draws* that has no source** (ruling 43).
- **Do not run `Hamlet.App`, and do not add a package** (ruling 42).
- **Do not delete a file.** Empty it, comment it, list it (§6).
- **Do not loosen a threshold or tolerance, and do not break the view to watch a test fail** (ruling
  19, §6).
- **Do not edit `PHASE_OUTCOME.md`, `RUN_LEDGER.md`, the `STEP:` lines, `CURRENT_STEP`,
  `.run-unit\allowed.txt` or anything under `tools\arbiter\`.** They are the launcher's.
- **Never `git add -A` or `git add .`** (§2).
- `CLAUDE.md` §12.6 covers the rest. **No package. Report mismatches. Write American.**
- **The tool facts, as unit 348 found them.** Say which held for you:
  - **ran:** `sh tools/status.sh` on every write, joined by `&&` before `dotnet`; `&&` joining
    `git add`, `commit` and `push`; commits with several `-m`; `grep -E`, `cut`, `sed -n` and
    `tail -n +1` in pipes; `git grep --untracked` with several `-e`; `ls`; `date`;
  - **asked for approval, not run:** `awk` in a pipe; `sort -t- -k3 -n` in a chain; `sed -E` in a
    pipe; `xargs grep -L -v` after a `grep -E` pipe; `;` joining `head`, `git ls-files`,
    `git check-ignore` and `git show`; `git grep -L -v` with several `-e`. This arbiter also found `;`
    joining `git rev-parse` and `ls` asking for approval, and `cd` before `git` asking;
  - **refused:** a `for` loop over a shell variable (*Contains simple_expansion*); `sed -i` on
    `output.md` and a redirect onto `output.md`, both *blocked* as outside `C:\Source\HamLet`;
  - **so:** read each `git show <hash>:<file>` as its own command, not in a loop. Write the sheet and
    the report with the file editor.

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
`tools/arbiter/validate-output.bat output.md`. **Always name the report.** **The `UNIT:` line must
fall inside the first 60 lines.**

**The ordering block comes first.** `validate-output` refuses a report without it.

```
READ IN THIS ORDER.

A. The phase goal - the screen, done right. Re-read at <hash> this unit:
   step 0 done (state reader on unit 341; TheTopRowTests <n of n>,
   TheWorkingPanelsTests <n of n>, BindingHealthTests <n of n>, VoiceTests
   <n of n>). Step 1 done (unit 347; trading cards <n of n>, clicks-in
   <n of n>). Step 2 done (unit 348; PSK31 records <n of n>, achievements
   page <n of n>). Step 3 not started: 0 of 1, and only Tim can move it.
B. Step 3 and its exit criterion:
   entry: step 2 done - met on the state reader's verdict on unit 348
   1. Tim says it passed - NOT MET, and not a session's to meet. What
      stands ready for him: docs\unit349-what-tim-looks-at.md, <n> pages
      at 1400 and 1920; <n> rulings with a screen effect, <n> without,
      <n> unit choices, each with its overrule words; <n> known items;
      <n> known reds; <n> claims checked, <n> corrected, <n> removed;
      the four-digit no-STATE line [<measured result> or DROPPED]
   Any class that did not read as work instruction 349 section 2
   expects: <none, or which and its failure line>
C. The report last. Section 4 raises N items on top of the carried queue.
   One is Tim's step 3 verdict. Say whether any other stands in the way of
   step 3's criterion, and whether any red found here re-opens step 0, 1
   or 2.
```

**Every line specific to this unit.** If something was not measured, say *not measured*. Do not fill
the shape.

```
UNIT:       349 - <complete|stopped> at task N of 3 - <date time, read from the clock>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   no - step 3's criterion is Tim's; <what now stands ready for it>
NUMBER:     step 3 must-pass met: 0 of 1 -> 0 of 1; decisions gathered for Tim: 0 -> <n>
DRIFT:      0
```

**Section 2 says, first**, where the sheet is and that it is the thing to read at his window. It then
says that nothing on it was seen by anyone, and that it asks for his verdict and his window size. Keep
the rest of section 2 short: the sheet carries it.

**Section 3 leads with the answer:** does the phase's evidence stand at this tree? That means every
class as §2 expects, or which did not. Then the counts from block B. Then the sheet's table of contents.
Then any correction task 2 made. **Every appearance claim is computed, not seen. Say so once.**

**Section 4:** unit 348's section 4 verbatim per HM-DEC-139, from its line under
`## 4. What's blocking us` to its end, as committed in `26e5879`, including the queues it carries. Keep
it in place with the file editor. In the carried text, mark:
- unit 348 item 3 (the no-`STATE` line at one digit): `TAKEN UP by work instruction 349 task 3`, with
  the result, or `DROPPED by unit 349 - task 3 was the drop candidate; on the sheet as arithmetic`;
- unit 348 item 4 (the States band loses *the 50 states*): `ON THE SHEET for Tim - work instruction 349
  ruling 44`;
- unit 346 item 4 (VK2DEF's span): `ON THE SHEET for Tim - work instruction 349 ruling 43`;
- unit 341 items 1, 3 and 5 (the 1400 PSK31 margin, the hidden upgrade row, the offer's ink): `ON THE
  SHEET for Tim - work instruction 349 ruling 43`.

Then this unit's items, under *Raised by unit 349*:
- **The first is the ask** (ruling 46): *Ruling wanted, Tim's, step 3: pass or not, at your window
  size, reading `docs\unit349-what-tim-looks-at.md`.* Give the reasoning in two lines: every step before
  it is done on evidence, and no session can see the screen.
- **Everything else is a finding, and says so.** Rulings 1 to 46 are already on the sheet for Tim. Do
  not raise them again.

---

```
ARBITER-DECISION
STEP: 3
APPROACH: prepare step 3 for Tim without changing the screen - re-run step 0, 1 and 2's classes once at this tree and trace every achievements page's drawn words and numbers at 1400 and 1920; write one review sheet, docs/unit349-what-tim-looks-at.md, page by page with what R26 and R22 promise and what the tree draws, every arbiter ruling 1 to 40 and every unit choice that changed a word or hid something gathered from git with its overrule words, the known imperfections and known reds; check the sheet claim by claim against its sources; the four-digit no-STATE line measured at 1400 and 1920 as the drop candidate
MOVE: continue
WHY: The state reader returned step 2 done on unit 348, so step 3's entry is met, and its one criterion is Tim's word, which no session can supply and no picture can be made for without a package. What a unit can do is clear what stands between Tim and that word: forty decisions made for him, a dozen known imperfections and the phase's numbers spread over twelve reports. Nothing here is a stop, and the loop test found nothing like it.
STATE: not started
DECIDED: ruling 41 - step 3 entered on the unit 348 verdict, step 0 read as done as work instructions 342 to 348 read it, the phase's eight classes run once at this tree as a reading and never fixed; ruling 42 - no pictures, the headless backend rasterizes nothing, and Avalonia.Headless.Skia (a package), running Hamlet.App and a drawn mockup are all rejected; ruling 43 - one sheet at docs/unit349-what-tim-looks-at.md in seven sections, every claim sourced to a test run this unit or a report's commit and item, and Tim asked for his window size since the record has none; ruling 44 - every arbiter ruling 1 to 40 and every unit choice that changed a word or hid something, gathered from git, one line each with where it shows, the rejected alternative and the words that overrule it; ruling 45 - the no-STATE line at four digits asserted drawn at 1400 and 1920, shortened and named only if it clips, the drop candidate; ruling 46 - section 4 carries one real ask, Tim's step 3 verdict, and no further unit is authored into step 3 until he answers unless the sheet is shown wrong - all the author's, marked for Tim at step 3, overrulable
LICENCE: PHASE_PLAN.md sections 1 (a step's exit is computed, described in words, then Tim's eyes), 2, 4 (step 3 entry and exit; No script can evaluate this), 6 (the arbiter decides and continues; a package stops; shorten and name; never loosen) and 7; docs/phase-maintenance-run/PHASE_PLAN.md R22 and R25; PHASE_PLAN.md R26 and R27; .run-unit/state-verdict.json (step 2 done on unit 348); .run-unit/s4-verdict.json (none); unit 348 output.md at 26e5879 sections 1, 3 and 4 items 3 and 4; unit 346 item 4 and unit 341 items 1, 3 and 5 as carried there; TheMarksRenderTests.cs 154-161 and Unit300SizesTests.WhetherAnythingHereCanActuallyLook (no raster on the headless backend); work instructions 338 to 348 in git (rulings 1 to 40); ARBITER.md sections 6 and 8; CLAUDE.md 0.0, 0.2, 0.5, 0.6, 8; psk31 R12, R13, R14, R16, R19; HM-DEC-139, HM-DEC-155, HM-DEC-163
ACCOMPLISHED: Tim has one sheet to read at his own window that tells him, page by page, what the main window and every achievements page should show and what this tree measures there at both widths, every choice made for him and how to undo it, and every known thing that will look off - with every number checked against its source - so he can give step 3's verdict in one sitting, and the phase's evidence is re-read on the tree he will be looking at
ADVANCES: none - this unit clears the blocker in front of step 3's one criterion (Tim says it passed): forty overrulable decisions never gathered, known imperfections never listed and the phase's numbers scattered over twelve reports and not re-run at this tree; task 3, the drop candidate, turns step 1 criterion 5's four-digit States line from arithmetic into a measurement
END-ARBITER-DECISION
```
