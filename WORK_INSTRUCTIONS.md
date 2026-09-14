# Work instruction 350 - step 0, on the record and not on the clock

Step 0 of `PHASE_PLAN.md`. **The screen does not change in this unit.** Step 3 is waiting for Tim.

Where each step stands:

- **Step 3 is waiting for Tim's verdict** on `docs/unit349-what-tim-looks-at.md`.
  - The state reader returned `in progress` on unit 349: *the only exit criterion is Tim saying it
    passed, which has not happened.*
  - No commit after `1456ac5c` carries an answer, and `DECISIONS.md` still tops at HM-DEC-163.
  - **Nothing is authored into step 3** (ruling 46, upheld as ruling 47).
- **Steps 1 and 2 are done** on the state reader's verdicts on units 347 and 348.
- **Step 0 has no `done` anywhere in the record.**
  - `PHASE_OUTCOME.md`'s header reads step 0 `in progress`.
  - Its last step 0 entry is unit 340, `STATE_AFTER: blocked`, and the reload reads that entry as
    winning.
  - There is no unit 341 entry. Work instructions 342 to 349 took step 0 as done on a state-reader
    verdict on unit 341, and `.run-unit\state-verdict.json` has since been overwritten.
  - **Steps 1, 2 and 3 all rest on step 0.** A foundation that exists only in a file that has been
    overwritten is the failure `PHASE_CONTROL.md` §7 names. Even if Tim says *passed* tomorrow, the
    launcher would still read one open step.
- **One of step 0's must-pass numbers depends on the hour the test runs.**
  - `MainWindowViewModel.RankBands` reads `DateTime.Now.Hour` (`MainWindowViewModel.cs:16836`), and
    the green block's *best bet* follows that ranking.
  - On PSK31 at 1400 the top row is 237 px with the best bet drawn and 228 px without. The limit is
    0.262 × 910 = 238.4 (unit 341 item 1, `0f383a3`; unit 339 item 2, `991223a`).
  - The run at `85437c2` read 228, because no best bet was drawn at that hour. **No test has
    measured the worst case on purpose.**

This unit gives step 0 a report of its own, criterion by criterion at this tree, so the state reader
can judge step 0 on it. It also measures criteria 1 and 5 with the best bet pinned both ways, so the
result does not depend on the clock. It has four tasks, 0 to 3. Task 3 is the drop candidate.

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

These four are copied from work instruction 349; unit 349 checked them and they held. Check them
again.

**Also:** `.run-unit\allowed.txt` must permit `dotnet`. For this arbiter it carried `Bash(dotnet:*)`
at line 14.
- If `dotnet test` is refused, stop at task 0, write the report, and say so in section 4, quoting
  the refused command.
- **Do not route around it:** no `tools/tests/run.js`, no assertions written but never run, and no
  edit to `allowed.txt` or `run-unit-tools.txt`. Unit 343 item 1 rejected all three, and those
  rejections stand.

---

## 1. Why this unit exists

**The numbers today:**
- **Step 0:** 6 must-pass criteria. The record holds no `done`: the header reads `in progress`, and
  the last entry reads `blocked` (unit 340).
  - Unit 349 re-ran step 0's classes at `85437c2`: 22 of 22.
  - Criteria 1 and 5 held at 228 px on PSK31 at 1400, **measured at whatever hour the run happened
    to be**.
- **Step 3:** 0 of 1, and it stays 0 of 1 until Tim answers.

**What this arbiter read from the tree at `1456ac5c`. None of it has been run:**
- **`AtFourteenHundredTheLicensedTopRowIsTheMockupsShare`** (`TheTopRowTests.cs:577`) loops over FT8
  and PSK31 at 1920 and 1400.
  - At 1400 it asserts `m.TopRowHeight <= 0.262 * below` (`:652`) and panels `>= below / 2`
    (`:656`).
  - It never sets the best bet, so the ranking at run time decides which case it measures.
- **There is already a test-only way to set the best bet on a realized window.**
  `TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow` (`:684`) sets `band.IsBestBet` for each
  band and calls `model.NotifyGreenZoneForTests()` (`:697`-`700`). It runs at 1920 only. That test is
  step 0's nice-to-pass.
- **The ranking also runs on each spot refresh:** `RankBands(now)` then `ApplyBestBet(ranking)`
  (`MainWindowViewModel.cs:17037`-`17038`). A refresh during a test could overwrite a pinned best
  bet. Whether one does on the headless host is not known to this arbiter.
- **Criterion 4 says *full to the status bar*.** Unit 349's sheet check (correction 1) found that the
  panels end at y 953, the working card's floor, and that the status bar's top is at y 978. What
  fills those 25 px has not been named in any report this arbiter found. Unit 339 wrote that *full to
  the status bar at 1400 is printed and not asserted*.
- **The three launcher fields disagree**, as the reload says: header `in progress`, last entry
  `blocked`, and no unit 341 entry. Only a step 0 report the state reader can read will change what
  the launcher appends. Neither this arbiter nor the unit may edit those files.

```
PHASE GOAL: The screen, done right - the main window as the approved mockup
            (step 0: 6 must-pass, no done in the record, last entry blocked);
            every achievements category page as trading cards (step 1, done on
            unit 347); what the last phase left (step 2, done on unit 348); then
            Tim at his window says it passed (step 3, 0 of 1, waiting for Tim).
UNIT GOAL:  Give step 0 a report of its own at this tree, criterion by criterion,
            with the 1400 top row and panel share measured with the best bet
            pinned both ways instead of at the hour of the run, and with what
            stands between the panels and the status bar named in pixels.
ADVANCES:   step 0 - criteria 1 and 5 (the 190 px row and the 1400 share, held
            at the best bet's worst case, not by the clock); criterion 4 (full to
            the status bar, measured and named); and step 0's missing record,
            which steps 1 to 3 rest on.
DRIFT:      0
```

---

## 2. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any mismatch.
**Report the mismatch; do not repair the instruction.** Mismatches go in the report even when the
work succeeds anyway.

Check:
- **Every file, line and item §1 cites.**
- **`HEAD` and `origin/main` read `1456ac5c`** for this arbiter, and `output.md` in the tree is unit
  349's.
- **The version is 1.13.36** in `Directory.Build.props` (line 801 for this arbiter).
- **`DECISIONS.md` tops at HM-DEC-163.** If a higher id exists, or any commit after `1456ac5c`
  carries a verdict from Tim on step 3, **stop at task 0.** Quote it in section 4 and write nothing
  else: a verdict changes what the next unit is.
- **The launcher's files still disagree with themselves.** One line each; edit none:
  - `PHASE_OUTCOME.md`'s header reads step 0 `in progress`, and its last step 0 entry is unit 340,
    `blocked`. There is no unit 341 entry.
  - `PHASE_STATUS.md` reads `CURRENT_STEP: 0`, step 3 `in progress`, and `WORK_INSTRUCTION: 349`.
  - `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md` and files under `.run-unit\` are modified
    and uncommitted by the launcher.
- **The reload says `CLAUDE.md` §1 holds `CPS-DEC-0163`.** Unit 349 found no `CPS-DEC` id in the
  file. It is parked with the id schemes. One line.
- **`validate-output.bat` reads the `UNIT:` line only in the report's first 60 lines.**
- **`tools\arbiter.bak-20260913\` is untracked at the root.** **Never `git add -A` or `git add .`**;
  stage files by name.
- **The tool facts in §7** are unit 349's. Say which held for you.

**Reds expected, older than this unit. This unit runs none of them:**
- `TheMenuIsUnderTheMouseTests` (8);
- `TheAchievementsScreenTests.TheWindowDrawsEverySixRows` (1);
- `TheOperatorCanStopItTests` (2) and `TheWholeChainRunsFromOneRightClickTests` (2), **never run.**

**Expected green:**
- **the carry-forward list:** 111 of 111 app and 86 of 86 engine, unit 349's numbers after task 3;
- **step 0's filter:** `TheTopRowTests` 8 of 8, `TheWorkingPanelsTests` 8 of 8, `BindingHealthTests`
  1 of 1 and `VoiceTests` 5 of 5, which is 22 of 22.

**If a red turns green, a green turns red, or a count moves, say which.**

---

## 3. Rulings in force - do not re-argue

**HM-DEC-155**, transcribed: *A unit runs no test suite. It may run only the test it constructs in
that work instruction, filtered by exact name, in the foreground, with a stated timeout, and it never
backgrounds a command and polls for it.*
- Run the carry-forward list as the top comment of `docs\carry-forward-tests.txt` says: two
  invocations, one build each, with status written immediately before each.
- **Step 0's four classes may run together, filtered by class name**, in one filter:
  `TheTopRowTests`, `TheWorkingPanelsTests`, `BindingHealthTests` and `VoiceTests`.

**Step 0, `PHASE_PLAN.md` §4, in full.** This is the list the report is judged against:

> **Delivers:** R26.
> **Entry:** the tree is Hamlet's; `PHASE_STATUS.md` names this phase.
> **Exit:**
> - At 1920, the top row (neighborhood card and rig display) is about 190 px tall and the working
>   panels below the tabs take the rest; the numbers reported. *must-pass*
> - The neighborhood card carries the band strip, the green block and the world clock as R26 says;
>   the green block's band is its largest text; the clock carries one dot. *must-pass*
> - The rig display is the neighborhood card's height and carries drive and the power offer under the
>   S-meter. *must-pass*
> - Waterfall, decoded text and For You are equal height, full to the status bar; the card's facts sit
>   beside its map at 1920. *must-pass*
> - At 1400 the same shape holds, no callsign is clipped, and the facts go beside or under by the
>   unit's stated rule; the numbers reported at both widths. *must-pass*
> - `BindingHealthTests`, `VoiceTests` and the carry-forward list green. *must-pass*
> - The band pills' *best bet now* is joined to the green block by the check as before.
>   *nice-to-pass*

**R26, Tim, 2026-09-12 (`PHASE_PLAN.md` §R), in full:**

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

**`PHASE_PLAN.md` §1, §6 and §7, the lines that bite here:**
- *Screen only. Nothing here touches the radio, a decoder, a parser, the transmit chain or the log's
  content. A step's exit is what is on the screen, asserted by computation and described in words,
  then Tim's eyes. Every appearance claim is computed, not seen, and says so.*
- *A must-pass is missed by a little: ship, report, `partial`, move on. Never loosen a test.*
- *Anything touches the radio, a decoder, a parser or the transmit chain: `MOVE: stop`.*
- *A package is needed: `MOVE: stop`.*
- *Carried: every open ask from unit 336's queue, verbatim in every unit. Plus: real flags on earned
  country cards - undecided, Tim's; the PSK31 phase's step 6 - Tim's, at the radio; the recording of
  real PSK31 audio; the id-scheme split; the map bitmap's license.*

**The arbiter's rulings 1 to 46 stand as units 337 to 349 built them.** They are gathered on the
sheet, marked for Tim at step 3, and overrulable. **This unit re-opens, re-words or re-builds none of
them.** Three bite here:
- **1.** *The working panels* means the three panels themselves, at least half the height below the
  band pills at 1920 and 1400, with the readiness strip hidden. The number with the strip showing is
  also reported.
- **19. Watching red.** Show a new assertion fail once, against a deliberately wrong expectation set on
  the test window only, and give the failure line. **Never break markup or a view to watch a test
  fail.** Where the new assertion is red against the tree as it is, that is the watched red.
- **46.** No further unit is authored into step 3 until Tim answers, unless the sheet is shown wrong.

**The arbiter's rulings for this unit.** They are the author's, marked for Tim at step 3, and
overrulable:

47. **Step 3 is held for Tim, and this unit changes nothing Tim is reviewing.**
    - Ruling 46 is upheld. Unit 349's section 4 item 1 is Tim's ask and stays open.
    - **No file under `src\` changes in this unit, and no markup changes.** The sheet describes the
      screen at `85437c2`, and the screen must still be that screen when Tim opens it.
    - The only file under `docs\` that changes is the sheet, and only as task 2 says. Those edits are
      numbers measured here, never new words about the screen.
48. **Step 0 gets a report of its own at this tree.**
    - The report's block B is step 0's seven criteria, each with its test, its numbers at 1400 and
      1920, and *met* or *not met*. The state reader then judges step 0 on this unit, not on a
      verdict that has been overwritten.
    - **Every number comes from a run in this unit.** Where no class prints a number a criterion
      needs, say so and quote the latest report that measured it, with its commit. Do not compose a
      number.
    - Unit 341's report (`0f383a3`) is the step 0 evidence the record lost. Cite it where it
      measured something this unit does not re-measure. Say which items those are.
49. **Criteria 1 and 5 are measured with the best bet pinned both ways, not at the hour of the
    run.**
    - Set the best bet on the test window only, the way `:697`-`700` does, in two states:
      - **drawn**, on the band the fixture is on;
      - **absent**, with every `IsBestBet` false.
    - Do this on FT8 and PSK31, at 1920 and 1400, on the licensed fixture.
    - Assert the limits the tree already has, unchanged, in each state: the 1400 top row
      `<= 0.262 * below`, the three panels `>= below / 2`, and the rig panel within 2 px of the card.
      **0.262 and one half are not loosened.**
    - Put it where it reads best. That can be an extension of
      `AtFourteenHundredTheLicensedTopRowIsTheMockupsShare`, or one new fact beside it in
      `TheTopRowTests`. Name which.
    - **Check that the pin holds.** Read `IsBestBet` and the green block's best bet visibility after
      settling, and print them.
      - If a spot refresh re-ranks and overwrites the pin, the test fails with that message.
      - Do not work around it in `src`: no clock injection and no ranking change. Report it, and the
        next arbiter decides.
    - **Nothing is pressed.** `GreenZoneBestBet` is a button that moves the operator's band. Read it;
      never click it or invoke its command (§0.2).
    - **If the worst case is red at this tree:**
      - that is a finding for block B, with the failure line;
      - it is **not fixed**, because the screen is under Tim's review (ruling 47);
      - a miss by a little is `partial` under §6, and the next arbiter decides between a fix and
        Tim's verdict.
50. **Criterion 4's *full to the status bar* is measured and named, not asserted into a new
    shape.**
    - At 1920 and 1400, on FT8, print from the three panels' bottom edge to the status bar's top edge.
      Give the bottom and top in window coordinates, and every visual between them that has height:
      its name or type, its top, its height, and whether it is a border, padding, margin or content.
    - **The report says, in block B, whether anything a person would call a panel, a strip or empty
      space stands there.** It does not judge whether that meets R26's words; the state reader and
      Tim do.
    - If an assertion is warranted, it asserts the gap as measured, no looser, and names what fills
      it. Otherwise it is printed only. Name which.
51. **Task 3, the drop candidate: the nice-to-pass at 1400.**
    `TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow` runs at 1920 only. Extend it to 1400 in
    the same loop, with its assertions unchanged. Watch it red by ruling 19 if it is green on the tree.
52. **Section 4 raises no new ask of its own unless the measurements put one there.**
    - Unit 349 item 1 is carried as Tim's open ask, marked *STILL OPEN - work instruction 350 authors
      nothing into step 3*.
    - Everything else this unit finds is a finding, and says so.
    - A red at the worst case (ruling 49) is a finding, not an ask. §6 already says what happens to a
      small miss.

**Standing, transcribed:**
- **§0.0**: *Never present a guess as a decode… This binds pictures as hard as sentences.* Every
  appearance claim is computed, not seen, and says so once.
- **§0.2**, *transmit safety, ABSOLUTE*: *One operator action, one transmission.* Nothing here
  transmits or tunes. No test this unit writes presses CQ, Stop, Capture, a band button, the best bet,
  a send or a transmit command.
- **§0.5**: *every panel is collapsible, and a collapsed panel still carries its summary. Collapsing
  hides detail, never information.*
- **§0.6**: every ink clears 4.5:1 against its fill, and color is never the only carrier.
- **R12**: *a session fixes its own tests and never asks the owner to approve it.*
- **R13**: telemetry on every new stage. This unit adds no stage.
- **R14**: *a test exists to prove an exit criterion.* This unit adds only rulings 49 to 51's
  assertions and task 0's trace.
- **R19**: American spelling.
- **HM-DEC-139**: open asks are carried verbatim until answered.

---

## 4. Status cadence

Write status **before every `dotnet` command, after every commit and after every task**, with
`sh tools/status.sh`. It ran for every write in unit 349. If it is refused, take a `date` reading and
paste it whole. **Never compose a time.**

The script hard-codes `RULES_AT: HM-DEC-161 (2026-09-11)`. **After every write**, set that line back
to `HM-DEC-163 (2026-09-12)`, or to the highest id §2 finds, with the file editor.

The watchdog kills a session only when its process tree has used no CPU for ten minutes. **Write
status between sections of the report**, so a quiet stretch of editing is never ten minutes of nothing.

---

## 5. The tasks

### Task 0 - the trace: step 0 at this tree, with the best bet both ways and the floor named

Measure before anything is asserted. **Say what you find rather than confirming §1.**

1. **Commit `WORK_INSTRUCTIONS.md`, `PHASE_STATUS.md` and a patch bump (1.13.36 -> 1.13.37, with its
   comment block).**
   - In `PHASE_STATUS.md`, change only the `WORK_INSTRUCTION:` line, to `350 - step 0, on the record
     and not on the clock`. Do not touch its `STEP:` lines, `CURRENT_STEP` or `HEARTBEAT`.
   - Do not commit `.run-unit\`, `PHASE_OUTCOME.md`, `RUN_LEDGER.md`, `SESSION.lock` or
     `tools\arbiter.bak-20260913\`.
   - The message is `chore(unit350): step 0, on the record and not on the clock - the trace first`.
   - Run the carry-forward list, status first, and give both invocations as *n of n*.
2. **Step 0's filter, once**, status first, with a stated timeout. Give each class as *n of n*, and
   the failure line of any red. **Do not stop for a red; do not fix one.** Print the hour the run
   read, and whether the best bet drew, on each 1400 PSK31 window.
3. **Add `Unit350TraceStepZeroBothWays` to `TheTopRowTests`.** It asserts nothing and presses nothing.
   - **Best bet, both ways (ruling 49):** at 1920 and 1400, FT8 and PSK31, licensed, best bet drawn
     and best bet absent. For each, print:
     - top row px and share;
     - card and rig panel;
     - green block height;
     - the three panels with the strip hidden and showing;
     - whether the pin held after settling.
   - **The floor (ruling 50):** at 1920 and 1400 on FT8, the panels' bottom, the status bar's top, and
     every visual with height between them.
4. **Answer from the numbers, in section 1, before task 1 starts:**
   - The worst 1400 PSK31 top row, and its margin to 0.262 × `below`.
   - The worst panel share at either width, strip hidden.
   - Did the pin hold in every state?
   - What fills the gap between the panels and the status bar, in pixels, at each width?
   - Did step 0's filter read 22 of 22?

**Drop candidate:** none. Every line of block B rests on it.

### Task 1 - the best bet pinned, asserted (ruling 49)

1. **The assertion**, at 1920 and 1400, FT8 and PSK31, best bet drawn and absent, with the limits the
   tree already has. Name where it lives.
2. **The watched red** by ruling 19, on the test window only, with the failure line. Or give the red
   on the tree, and do not fix it (ruling 47).
3. **Run step 0's filter again**, status first. Commit and push:
   `test(app): task 1 - the 1400 top row and panel share held with the best bet pinned both ways -
   <numbers>`.

**Drop candidate:** none. Criterion 5 does not stand on the clock without it.

### Task 2 - the floor named, and the sheet's numbers brought to this tree (rulings 48 and 50)

1. **Criterion 4:** printed or asserted as ruling 50 says. Name which, and why.
2. **The sheet, `docs\unit349-what-tim-looks-at.md`, numbers only:**
   - section 2.2's top-row line and section 4's items 1 and 2 take the numbers task 1 measured,
     citing this unit's commit;
   - section 2.1's *Where the panels end* row adds what task 0 named, in the same words the report
     uses.

   **No other line of the sheet changes.** If a measured number contradicts another line of the
   sheet, say so in section 4. Do not edit that line.
3. **Run the carry-forward list**, status first. Commit and push:
   `docs(unit350): step 0's worst case and floor on the sheet - <numbers>`.

**Drop candidate:** the sheet edit (step 2). If time is short, skip it and say so. Section 3 of the
report carries the numbers, and the next arbiter decides whether the sheet needs them before Tim reads
it.

### Task 3 - the nice-to-pass at 1400 (ruling 51, the drop candidate)

1. **Extend `TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow` to 1400**, with its assertions
   unchanged. Give the watched red.
2. **Run step 0's filter again**, status first. Commit and push.

**Drop candidate: this task, whole.** If time runs short, skip it. The nice-to-pass stays at 1920, and
the report says so.

---

## 6. Parked - do not touch, do not raise

- **Step 3 and the verdict.** Tim's (ruling 47).
- **The screen, all of it.** No file under `src\` and no `.axaml` changes, even for a red at the worst
  case.
- **The best bet ranking and the clock.** Nothing in `RankBands`, `ApplyBestBet` or any clock source
  changes; a test that finds the pin overwritten reports it.
- **Every red in the record**, and every test except those rulings 49 to 51 and task 0 name.
- **Steps 1 and 2.** Done. Their classes are not run in this unit.
- **Pictures**: no package, no running app, no drawn mockup (ruling 42).
- **PSK31 decoding, all of it** (ruling 23).
- **Transmit, all of it:** `TheOperatorCanStopItTests`, `TheWholeChainRunsFromOneRightClickTests`, and
  `TheMenuIsUnderTheMouseTests`.
- **The fifteen emptied files.** They are on unit 348's list.
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
- **Do not change a file under `src\`** (ruling 47). **Do not loosen 0.262, one half or the 2 px rig
  tolerance** (§6).
- **Do not press, click or invoke the best bet, a band button, CQ, Stop or any send** (§0.2, ruling
  49).
- **Do not break the view to watch a test fail** (ruling 19).
- **Do not edit the sheet beyond task 2's lines** (ruling 47).
- **Do not delete a file.** Empty it, comment it, list it (§6).
- **Do not edit `PHASE_OUTCOME.md`, `RUN_LEDGER.md`, the `STEP:` lines, `CURRENT_STEP`,
  `.run-unit\allowed.txt` or anything under `tools\arbiter\`.** They are the launcher's.
- **Never `git add -A` or `git add .`** (§2).
- `CLAUDE.md` §12.6 covers the rest. **No package. Report mismatches. Write American.**
- **The tool facts, as unit 349 found them.** Say which held for you:
  - **ran:** `sh tools/status.sh` on every write, joined by `&&` before `dotnet`; `&&` joining
    `git add`, `commit` and `push`; commits with several `-m`; `grep` with several `-e`, `-v`, `-o`,
    `-c`, `-n` and `-A`; `cut`, `paste` and `sed -n` in pipes after `git show`; `ls`; `date`;
  - **asked for approval, not run:** `grep -E` with a `{1,2}` quantifier and `\*` in a pipe after
    `git show`; `sed -e 's/.../'` in a pipe. This arbiter also found `;` joining `git` commands asking
    for approval, and an apostrophe inside `outcome-read.bat --approach` breaking its search;
  - **refused:** none for unit 349. Unit 348 found a `for` loop over a shell variable refused, and
    `sed -i` or a redirect onto `output.md` blocked;
  - **so:** write the report with the file editor.

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

A. The phase goal - the screen, done right. At <hash>: step 0 has no done
   in the record (header in progress, last entry unit 340 blocked, no unit
   341 entry) - this report is its evidence. Step 1 done (unit 347). Step 2
   done (unit 348). Step 3 0 of 1, waiting on Tim's verdict on
   docs/unit349-what-tim-looks-at.md; nothing here moves it.
B. Step 0 and its exit criteria, at this tree, best bet pinned both ways:
   entry: the tree is Hamlet's; PHASE_STATUS.md names this phase - <met?>
   1. 1920 top row about 190, panels take the rest - <met/not>: <px, share>
   2. card carries strip, green block, clock; band largest; one dot -
      <met/not>: <test, numbers>
   3. rig panel the card's height, drive and offer under the S-meter -
      <met/not>: <px at both widths, both modes; or cited from 0f383a3>
   4. three panels equal, full to the status bar; facts beside at 1920 -
      <met/not>: <heights; panels' bottom y, status bar top y, what fills it>
   5. 1400 same shape, no callsign clipped, facts rule - <met/not>: worst
      PSK31 top row <px> of <limit> with the best bet drawn, <px> without;
      worst panel share <n>; pin held <yes/no>
   6. BindingHealthTests, VoiceTests, carry-forward - <n of n each>
   nice: best bet joined to the green block - <1920 and 1400 | 1920 only>
C. The report last. Section 4 raises N items on top of the carried queue.
   Say whether any stands in the way of a criterion in B, and whether any
   red here is a miss by a little (section 6: partial) or more.
```

**Every line specific to this unit.** If something was not measured, say *not measured*. Do not fill
the shape.

```
UNIT:       350 - <complete|stopped> at task N of 3 - <date time, read from the clock>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no> - step 0 criteria <which>; <what is now measured that was not>
NUMBER:     step 0 must-pass evidenced at this tree: <n> of 6; 1400 PSK31 worst-case top row: not measured -> <px>
DRIFT:      0
```

**Section 2 says, first**, that nothing on the screen changed and the sheet Tim holds is still the
screen. Then it says, in one or two sentences, what the top row does at 1400 on PSK31 at the hours
when the best bet shows.

**Section 3 leads with the answer:** does every step 0 criterion hold at this tree, whatever the hour?
Then the table: criterion, test, 1400, 1920, met. Then what fills the space above the status bar. Then
any number that differs from the sheet. **Every appearance claim is computed, not seen. Say so once.**

**Section 4:** unit 349's section 4 verbatim per HM-DEC-139, from its line under
`## 4. What's blocking us` to its end, as committed in `1456ac5c`, including the queues it carries.
Keep it in place with the file editor. In the carried text, mark:
- unit 349 item 1 (Tim's step 3 verdict): `STILL OPEN - Tim's; work instruction 350 authors nothing
  into step 3`;
- unit 341 item 1 and unit 339 item 2 (the best bet moves the 1400 row with the clock): `TAKEN UP by
  work instruction 350 ruling 49`, with the measured worst case.

Then this unit's items, under *Raised by unit 350*. **Each says whether it is a finding or an ask**
(ruling 52).

---

```
ARBITER-DECISION
STEP: 0
APPROACH: put step 0 on the record at this tree criterion by criterion - no screen change - with the 1400 top row and panel share asserted with the best bet pinned both ways on the test window (drawn and absent, FT8 and PSK31, 1920 and 1400) instead of at the hour of the run, and what stands between the three panels and the status bar named in pixels; the sheet's step 0 numbers brought to this tree; the best bet nice-to-pass extended to 1400 as the drop candidate
MOVE: work around
WHY: Step 3 waits on Tim and ruling 46 holds, but step 0, which steps 1 to 3 rest on, has no done in the record (last entry unit 340 blocked, the unit 341 verdict overwritten), and its criterion 5 has held only at whatever hour the test ran (237 of 238.4 px with the best bet, 228 without). This differs from unit 339's proof: it pins the clock-driven input unit 339 item 2 and unit 341 item 1 named, and names criterion 4's floor. The loop test found nothing like it.
STATE: in progress
DECIDED: ruling 47 - step 3 held for Tim (ruling 46 upheld), no src or markup change so the sheet Tim holds stays the screen; ruling 48 - step 0 gets its own report at this tree, criterion by criterion, numbers only from this unit's runs or cited by commit; ruling 49 - criteria 1 and 5 asserted with IsBestBet pinned drawn and absent on the test window (the :697-700 precedent), FT8 and PSK31, 1920 and 1400, with 0.262, one half and 2 px unchanged, the best bet never pressed, an overwritten pin or a red worst case reported and not fixed; ruling 50 - criterion 4's gap from the panels' floor (y 953) to the status bar (y 978) measured and every visual in it named, asserted only as measured; ruling 51 - the nice-to-pass extended to 1400, the drop candidate; ruling 52 - no new ask unless the numbers raise one, unit 349 item 1 carried as Tim's and open - all the author's, marked for Tim at step 3, overrulable
LICENCE: PHASE_PLAN.md sections 1, 4 (step 0 exit in full), 5 (steps 1-3 one pipeline on step 0), 6 (the arbiter decides and continues; a small miss ships partial; never loosen a test; transmit or a package stops) and 7; PHASE_CONTROL.md sections 2 (a step verifies its own ground), 3 (course correction, revisiting a step) and 7 (a step passing on a foundation that failed); ARBITER.md sections 3, 4, 6 and 8; .run-unit/reload.txt disagreements (PHASE_OUTCOME header in progress, last step 0 entry blocked); PHASE_OUTCOME.md unit 340 STATE_WHY and no unit 341 entry; .run-unit/state-verdict.json (step 3 in progress on unit 349); .run-unit/s4-verdict.json (none); unit 349 output.md at 1456ac5c sections 1 (sheet correction 1: y 953 against y 978) and 4 item 1; docs/unit349-what-tim-looks-at.md sections 2.1, 2.2 and 4 items 1-2; unit 341 output.md at 0f383a3 item 1; unit 339 output.md at 991223a item 2; TheTopRowTests.cs 577, 652, 656, 684, 697-700; MainWindowViewModel.cs 16831, 16836, 17037-17038; Directory.Build.props 801; DECISIONS.md (HM-DEC-163 top); .run-unit/allowed.txt 14; CLAUDE.md 0.0, 0.2, 0.5, 0.6; psk31 R12, R13, R14, R19; HM-DEC-139, HM-DEC-155, HM-DEC-163
ACCOMPLISHED: the main window's promise - a short top row and the working panels given more than half the height - is shown to hold at both of Tim's widths whatever time of day the best bet appears, and the space above the status bar is named; step 0 has a report of its own on the tree Tim is reviewing, so the phase's first step stands in the record and not in a file that was overwritten, and nothing on the screen moved under Tim's review
ADVANCES: step 0 - criteria 1 and 5 (the 190 px row and the 1400 share held with the best bet pinned drawn and absent, not by the hour of the run), criterion 4 (full to the status bar, measured and the gap named), and step 0's missing done in the record that steps 1 to 3 rest on; the nice-to-pass at 1400 if task 3 runs
END-ARBITER-DECISION
```
