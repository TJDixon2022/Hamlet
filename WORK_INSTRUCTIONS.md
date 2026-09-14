# Work instruction 353 - the test window draws what its fixture declares, whatever the hour, and the sheet's green block words are cited to a window that draws them

Step 3 of `PHASE_PLAN.md`, under ruling 46's one exception. **Nothing on the screen moves.** The unit
makes step 0's facts that pin nothing measure the window their fixture declares, not the hour's best bet
and a reload's count. It then brings the sheet's green block words back to a test that draws them.

Where each step stands:

- **Step 0 is `done`** on the state reader's verdict on unit 351, unchanged by unit 352.
- **Steps 1 and 2 are `done`** on the state reader's verdicts on units 347 and 348.
- **Step 3 is `blocked`** on the state reader's verdict on unit 352 (`.run-unit\state-verdict.json`):
  *The only exit criterion is Tim saying the pages passed at his window size, no session can see the
  screen or give that verdict, the report shows the sheet ready and the verdict still open, so more unit
  effort cannot move the step until Tim looks.*
  - No commit up to `8fa20cb1` carries Tim's verdict.
  - `DECISIONS.md` still tops at HM-DEC-163 (`:7`).

**Why a unit at all, when the phase waits on Tim.** Ruling 46 says no further unit is authored into
step 3 *unless the sheet is shown wrong*. Unit 352's own item 1 shows it wrong in one place and leaves a
new dependence behind it:
1. **The sheet's §2.1 says *6 stations at 15* (`:55`) and *heard just now stands over 6 stations*
   (`:57`).** Its source note (`:71`-`81`) cites `TheGreenBlockIsInsideTheCardUnderTheStripWithTheBandLargest`
   at `386690a2`. At that commit, that fact draws *0 stations* and *best bet now: 80 m* (unit 352
   §1 task 1, decision 4, item 1). The words on the sheet are cited to a window that no longer draws
   them.
2. **Every step 0 fact that pins nothing now measures the hour of the run.** Before `386690a2` those
   facts raced a POTA reply. Now both spot reloads, `band_changed` and `startup`, land inside
   `Realized`'s settle loop every time. So they overwrite the fixture's `HeardInTheLastMinute = 6` and
   its sparkline (`TheTopRowTests.cs:2081`-`2083`, set before `window.Show()` at `:2090`), and run the
   best bet on the hour's table. Unit 352 item 1: *from 9 am to 5 pm the table puts 20 m first, which is
   the band the fixture is on, so the check would be drawn.* So the sheet's 1920 FT8 block cell (`:47`)
   and the unpinned facts' printed numbers are true at night and not measured by day.

This arbiter read, and did not run, where the fix sits. `Realized` sets the declared state, then
settles six passes (`:2092`-`2096`). The reloads land during those passes. Setting the declared state
again after the passes is a change to the tests' fixture only. **The trace settles whether anything
writes after that. Do not build on this reading.**

The unit has four tasks, 0 to 3. **Task 3 is the drop candidate.**

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

These four are copied from work instruction 352. Unit 352 checked them and they held. Check them
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

**The numbers today** (unit 352, `75e101e2` to `8fa20cb1`; none re-run by this arbiter):
- **Step 0's filter:** 27 of 27 in each of three runs on the committed tree. That is 26 facts plus
  `Unit352TraceTheSpotReloadOnTheTestWindow`: `TheTopRowTests` 13 of 13, `TheWorkingPanelsTests` 8 of
  8, `BindingHealthTests` 1 of 1 and `VoiceTests` 5 of 5. Pins held 8 of 8 and the 40 m fact 4 of 4 in
  each run.
- **The pinned numbers, unchanged by unit 352:**
  - 1920: 190 px, panels 503;
  - 1400 FT8: 216 px, panels 477;
  - 1400 PSK31: 228 px, panels 465.
- **The unpinned numbers that moved at `386690a2`** (unit 352 §1 task 1):
  - the 1920 green block went 55 -> 64 px on FT8 and 67 -> 76 on PSK31;
  - *heard just now* went y 251 -> 260 at 1920 and 268 -> 286 at 1400;
  - the count went y 263 -> 272 and 277 -> 295;
  - the no-license window's three panels at 1920 with the strip showing went 450 -> 441, top y 503 ->
    512;
  - `Unit338TraceTheRowsAboveThePanels`' no-license left column went 480 -> 340 at 1400 and 1000 -> 740
    at 1920;
  - `Unit350TraceStepZeroBothWays`' *before pinning the hour's best bet was visible* went False -> True
    in 12 of 12.
- **`Unit332TwoWidthsTests` 3 of 3 and `TheGreenZoneTests` 15 of 15**, with identical pixel lines
  before and after `386690a2`.
- **Carry-forward:** 111 of 111 app, 86 of 86 engine.
- **Step 3:** 0 of 1. It stays 0 of 1 until Tim answers.

```
PHASE GOAL: The screen, done right - the main window as the approved mockup
            (step 0, done on unit 351); every achievements category page as
            trading cards (step 1, done on unit 347); what the last phase left
            (step 2, done on unit 348); then Tim at his window says it passed
            (step 3, 0 of 1, blocked on Tim's verdict).
UNIT GOAL:  Make every step 0 fact on the test window measure the window its
            fixture declares - 6 stations, its sparkline, no best bet unless a
            fact pins one - whatever the hour of the run, with no screen change,
            and bring the sheet's green block words and cells back to a test
            that draws them.
ADVANCES:   none - no criterion moves. This unit clears the one place unit 352
            left the sheet shown wrong: its "6 stations" words cited to a fact
            that now draws 0, and step 0's unpinned numbers depending on the
            hour of the run.
DRIFT:      0
```

---

## 2. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any mismatch.
**Report the mismatch; do not repair the instruction.** Mismatches go in the report even when the
work succeeds anyway.

Check:
- **Every file, line and item the header and §1 cite.**
- **`HEAD` and `origin/main` read `8fa20cb1`** for this arbiter, and `output.md` in the tree is unit
  352's.
- **The version is 1.13.39** in `Directory.Build.props` (line 825 for this arbiter).
- **`DECISIONS.md` tops at HM-DEC-163.** If a higher id exists, or any commit after `8fa20cb1`
  carries a verdict from Tim on step 3, **stop at task 0.** Quote it in section 4 and write nothing
  else: a verdict changes what the next unit is.
- **The launcher's files, one line each; edit none:**
  - `PHASE_OUTCOME.md` and `PHASE_STATUS.md` read steps 0, 1 and 2 `done` and step 3 `blocked`.
  - `PHASE_STATUS.md` reads `WORK_INSTRUCTION: 352`.
  - `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md` and files under `.run-unit\` are modified
    and uncommitted by the launcher.
- **The reload says `CLAUDE.md` §1 holds `CPS-DEC-0163`** (`.run-unit\reload.txt:9`, `:34`). Unit 352
  found no `CPS-DEC` match there. It is parked with the id schemes. One line.
- **`tools\arbiter.bak-20260913\` is untracked at the root**, and `SESSION.lock` may be. **Never
  `git add -A` or `git add .`**; stage files by name.
- **Where each fixture sets what it declares**, as this arbiter read it:
  - `TheTopRowTests.Realized(width, telemetry)` at `:2067`-`2099` sets the count and sparkline at
    `:2081`-`2083` and settles at `:2092`-`2096`;
  - `FixtureSettings` at `:2130`-`2144`;
  - `TheWorkingPanelsTests.Realized` at `:718`, with sources off at `:725`-`728`;
  - `TheWorkingPanelsTests.EmptyTab` at `:484`-`486`, sources at their defaults;
  - `BindingHealthTests.cs:108`, `new MainWindowViewModel(new AppSettings(), null)`.
- **Which of step 0's facts call which fixture.** This arbiter did not read `Unit332TwoWidthsTests`'
  window. Say whether it builds its own or calls a `Realized`.
- **The tool facts in §7** are unit 352's. Say which held for you.

**Reds expected, older than this unit. This unit runs none of them:**
- `TheMenuIsUnderTheMouseTests` (8);
- `TheAchievementsScreenTests.TheWindowDrawsEverySixRows` (1);
- `TheOperatorCanStopItTests` (2) and `TheWholeChainRunsFromOneRightClickTests` (2), **never run.**

**Expected at the start:**
- **the carry-forward list:** 111 of 111 app and 86 of 86 engine;
- **step 0's filter: 27 of 27**, with pins 8 of 8 and the 40 m fact 4 of 4;
- **`Unit332TwoWidthsTests` 3 of 3 and `TheGreenZoneTests` 15 of 15.**

**Expected at the end:**
- **step 0's filter all green on each of three runs.** It reads 28 if this unit adds its trace fact.
- **Every printed pixel line is identical across the three runs.**
- **The carry-forward list and the two readers are unchanged.**
- **If a red turns green, a green turns red, or a count moves, say which.**

---

## 3. Rulings in force - do not re-argue

**HM-DEC-155**, transcribed: *A unit runs no test suite. It may run only the test it constructs in
that work instruction, filtered by exact name, in the foreground, with a stated timeout, and it never
backgrounds a command and polls for it.*
- Run the carry-forward list as the top comment of `docs\carry-forward-tests.txt` says: two
  invocations, one build each, with status written immediately before each.
- **Step 0's four classes may run together, filtered by class name**, in one filter:
  `TheTopRowTests`, `TheWorkingPanelsTests`, `BindingHealthTests` and `VoiceTests`.
- **`Unit332TwoWidthsTests` and `TheGreenZoneTests` may run together, filtered by class name**, in one
  filter, once, after task 1's change.

**Step 3, `PHASE_PLAN.md` §4, in full:**

> **Delivers:** Tim opens every page at his window size and says it passed.
> **Entry:** step 2 done.
> **Exit:**
> - Tim says it passed. *must-pass* No script can evaluate this.

**Step 0's exit, `PHASE_PLAN.md` §4, the criteria this unit re-reads and does not re-open:**

> - At 1920, the top row (neighborhood card and rig display) is about 190 px tall and the working
>   panels below the tabs take the rest; the numbers reported. *must-pass*
> - The neighborhood card carries the band strip, the green block and the world clock as R26 says; the
>   green block's band is its largest text; the clock carries one dot. *must-pass*
> - At 1400 the same shape holds, no callsign is clipped, and the facts go beside or under by the
>   unit's stated rule; the numbers reported at both widths. *must-pass*
> - `BindingHealthTests`, `VoiceTests` and the carry-forward list green. *must-pass*

**R26, Tim, 2026-09-12 (`PHASE_PLAN.md` §R), the lines that bite here:**

> - **The neighborhood card carries three things**: the band strip with the legend and the *you · mode*
>   marker; **the green block** - the band in the largest text, the frequency, mode and *yours to use*,
>   the license line small, the rule-of-thumb line small, and heard-just-now with its count and
>   sparkline; and **the world's clock** at its right end, about 246 px wide, with the operator's dot
>   only.
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

**The arbiter's rulings 1 to 64 stand as units 337 to 352 built them.** Rulings 1 to 58 are gathered on
the sheet, marked for Tim at step 3, and overrulable. **This unit re-opens, re-words or re-builds none of
them.** These bite here:
- **1.** *The working panels* means the three panels themselves, at least half the height below the
  band pills at 1920 and 1400, with the readiness strip hidden.
- **19. Watching red.** Show a new assertion fail once, against a deliberately wrong expectation set on
  the test window only, and give the failure line. **Never break markup or a view to watch a test
  fail.**
- **46.** No further unit is authored into step 3 until Tim answers, unless the sheet is shown wrong.
- **47**: **no `src` or markup change**, so the sheet Tim holds stays the screen.
- **49 and 56**: `TheTopRowAndThePanelShareHoldWithTheBestBetPinnedBothWays` pins `IsBestBet` on the
  test window. It asserts:
  - the 1920 row within 10% of 190;
  - the 1400 row `<= 0.262 * below`;
  - the three panels `>= below / 2`;
  - the rig panel within 2 px of the card;
  - the pin read back.
  **Its limits are not loosened, and its body is not edited.** The same holds for the 40 m fact.
- **61**, as built at `386690a2`: POTA, SOTA and RBN are switched off by `SourceName` in
  `TheTopRowTests.FixtureSettings` and `TheWorkingPanelsTests.Realized`. **It stands. This unit does not
  switch them back on, and adds no clock seam, retry or longer settle.**
- **63**: the sheet changes only where measured wrong.
- **U11**, unit 351's arrangement (sheet `:426`), stands as built.

**The arbiter's rulings for this unit.** They are the author's, marked for Tim at step 3, and
overrulable:

65. **Ruling 46's exception is invoked for one point, in two parts.**
    - The sheet's *6 stations* words (`:55`, `:57`) are cited to a fact that draws 0 at `386690a2`.
    - Every step 0 fact that pins nothing measures a window that depends on the hour of the run
      (unit 352 item 1).
    - This unit authors nothing into Tim's verdict. It moves no word, no markup and no view model, and
      it changes nothing under `src\` (ruling 47).
    - *Why:* the sheet tells Tim what the test window draws. If a cited fact draws something else, or
      draws one thing at night and another by day, Tim is judging against a number nobody can
      reproduce.
66. **The trace comes before the fix.** It runs on both licensed fixtures,
    `TheTopRowTests.Realized(width, telemetry)` and `TheWorkingPanelsTests.Realized`, on FT8 and PSK31,
    at 1400 and 1920.
    - **It prints, for each window:**
      - what `HeardInTheLastMinute`, the sparkline's point count and every band's `IsBestBet` hold at
        each of `Realized`'s six settle passes;
      - which reload landed in which pass, by telemetry where the fixture takes a telemetry file, and
        otherwise by the model's public members;
      - what they hold after one more settle pass, the same shape as the six, run by the trace itself
        after `Realized` returns.
    - **It names, by reading `src\` and changing nothing, every writer of those three**, by file and
      line. That includes any timer, clock tick or refresh that can write after the reloads have
      landed.
    - It asserts nothing and presses nothing.
67. **The fix goes in the tests' fixtures only.**
    - **In `TheTopRowTests.Realized`, after the six settle passes:** set the declared count (6), the
      declared sparkline and no best bet again, then run the same six passes once more.
    - **Then a guard in `Realized`:** it fails, with a message that names the property and what it
      held, if the count, the sparkline's point count or any band's `IsBestBet` is not the declared
      one.
    - **In `TheWorkingPanelsTests.Realized`:** clear the best bet the same way, with the same guard.
      It declares no count, so its count is left as the reload gives it, and the trace's printed value
      is reported.
    - **Not allowed:**
      - a clock seam, a retry, a wait-until loop or more than one further set of six passes;
      - any change to `FixtureSettings`' sources, the callsign, grid, license or dial;
      - an edit to either pinned fact's body.
    - **If the trace names a writer that can land after the restore**, such as a timer, name it and
      ship the restore only if the guard held on every run. Otherwise commit the trace only, and say
      so.
    - The fix is the unit's own and overrulable. Name the words that overrule it.
68. **The proof is three clean runs, and the same lines in all three.**
    - **Step 0's filter runs three times after the change.** Each run must be all green, with pins 8 of
      8 and the 40 m fact 4 of 4.
    - **Every printed pixel line is identical across the three runs.** Diff them as unit 352 did.
    - **Every unpinned number is named against unit 352's `u352-t0-step0` run (before `386690a2`) and
      its `u352-t1-final-run1` run (after).** The expected return is the fixture's window: 1920 block 55
      and 67, *heard just now* y 251 and 268, the no-license 1920 panels 450 with the strip showing, and
      `Unit350TraceStepZeroBothWays`' False. **Report what was measured, not what was expected.**
    - **Then run `Unit332TwoWidthsTests` and `TheGreenZoneTests` once, then the carry-forward list.**
    - **The pinned facts' numbers must not move.** If one does, report it and do not ship the restore.
69. **The sheet, `docs\unit349-what-tim-looks-at.md`, changes only where this unit measured it
    wrong:**
    - `:47`, the 1920 FT8 green block cell, cited to this unit's commit. Keep *64 with one drawn* only if
      a run in this unit prints it.
    - `:54`-`57`, the text sizes and the 1400 wrap, re-cited to this unit's commit, or corrected where a
      run printed them differently.
    - `:71`-`73` and `:118`, the source notes, only where their test counts are wrong at this tree.
    - `:20`-`21`, *17 m as the best bet*. Name the fixture that line describes. Correct it only if that
      fixture draws something else.
    - `:438`-`439`, section 4 item 1, only if a number in it moved.
    - Section 5: one line, only if a red stays.
    - **No other line.** Not the verdict form, not section 3, and no U row, because a test fixture is
      not a screen choice.
70. **The other two windows that still reach POTA are the drop candidate (task 3).** They are
    `BindingHealthTests.cs:108` and `TheWorkingPanelsTests.EmptyTab`.
    - **Read first:** can a POTA reply land while either measures, and does anything either prints
      differ across this unit's three runs?
    - **`EmptyTab`'s network sources are switched off by the same list only if its printed numbers
      differed across the three runs.**
    - **`BindingHealthTests` is never changed.** Its window is the product's default settings, and a
      default window binding without a complaint is what it guards. Report it only.
71. **Section 4 raises no new ask of its own unless the measurements put one there.**
    - Unit 349 item 1 is carried as Tim's open ask. Everything else this unit finds is a finding, and
      says so.
    - **The arbiter's recommendation to the next arbiter**, marked author's: if this unit ends with the
      guard holding, three identical runs and the sheet re-cited, no unit remains that ruling 46
      licenses. The next decision block should say that plainly rather than author a unit for the
      loop's sake.
    - **Logged, not chased, for the owner:** `ARBITER.md` §3 and §6 give no move for *waiting on the
      owner's eyes*. So the loop authors a unit into step 3 on each call. Units 349 to 352 cost about
      $10 to $14 each (`RUN_LEDGER.md`).

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
- **R14**: *a test exists to prove an exit criterion.* This unit adds only task 0's trace fact. The
  restore and the guard are edits to existing fixtures.
- **R19**: American spelling.
- **HM-DEC-139**: open asks are carried verbatim until answered.

---

## 4. Status cadence

Write status **before every `dotnet` command, after every commit and after every task**, with
`sh tools/status.sh`. It ran for every write in unit 352. If it is refused, take a `date` reading and
paste it whole. **Never compose a time.**

The script hard-codes `RULES_AT: HM-DEC-161 (2026-09-11)`. **After every write**, set that line back
to `HM-DEC-163 (2026-09-12)`, or to the highest id §2 finds, with the file editor. Unit 352 missed this
where two writes came in one batch; do not batch two status writes.

The watchdog kills a session only when its process tree has used no CPU for ten minutes. **Write
status between sections of the report**, so a quiet stretch of editing is never ten minutes of nothing.

---

## 5. The tasks

### Task 0 - the trace: what the test window holds after its reloads, and who writes it

Measure before anything changes. **Say what you find rather than confirming the header or §1.**

1. **Commit `WORK_INSTRUCTIONS.md`, `PHASE_STATUS.md` and a patch bump (1.13.39 -> 1.13.40, with its
   comment block).**
   - In `PHASE_STATUS.md`, change only the `WORK_INSTRUCTION:` line, to `353 - the test window draws
     what its fixture declares, whatever the hour`. Do not touch its `STEP:` lines, `CURRENT_STEP` or
     `HEARTBEAT`.
   - Do not commit `.run-unit\`, `PHASE_OUTCOME.md`, `RUN_LEDGER.md`, `SESSION.lock` or
     `tools\arbiter.bak-20260913\`.
   - The message is `chore(unit353): the test window draws what its fixture declares - the trace
     first`.
   - Run the carry-forward list, status first, and give both invocations as *n of n*.
2. **Step 0's filter, once**, status first, with a stated timeout. Give each class as *n of n*, the pin
   lines, and every unpinned number §1 lists as this run printed it. **Read the clock with `date`
   before and after**, and say which hour's table the ranking used.
3. **Add `Unit353TraceTheDeclaredWindowAfterTheReloads` to `TheTopRowTests`.** It prints what ruling 66
   lists, for both licensed fixtures, FT8 and PSK31, 1400 and 1920. It asserts nothing and presses
   nothing.
4. **Read `src\` for every writer** of `HeardInTheLastMinute`, `HeardSparkline` and `IsBestBet`. Give
   file and line for each, and say what starts it: a reload, a timer, the clock or a command.
5. **Answer from the numbers, in section 1, before task 1 starts:**
   - Which settle pass does each reload land in, and what does the window hold after the sixth?
   - Can any writer land after `Realized` returns? Which one, and how would a test see it?
   - Which fixture change will task 1 make, and why is it on the test window only?

**Drop candidate:** none. Task 1 is built on it.

### Task 1 - the declared window set again after the reloads, and the proof three times (rulings 67 and 68)

1. **Make ruling 67's change** in the two fixtures only. Keep the existing remarks' voice: say what
   changed, why, and the trace's numbers. Correct the *WHAT THAT MOVES, MEASURED* paragraph
   (`TheTopRowTests.cs:2124`-`2128`) to what this unit measured.
2. **Watch the guard go red once by ruling 19.** Set a deliberately wrong declared value on the test
   window only, give the failure line, then put it back.
3. **Add a line to the trace** that prints the same windows with the change in place.
4. **Run step 0's filter three times**, status first each time. Report every run's counts and pin
   lines, the diff of the printed pixel lines across the three, and every unpinned number against unit
   352's before and after runs.
5. **Run `Unit332TwoWidthsTests` and `TheGreenZoneTests` once, then the carry-forward list**, status
   first.
6. **Commit and push:** `test(app): task 1 - the test window draws what its fixture declares after its
   reloads - <what is set again, in which fixtures>; step 0's filter <n of n> three times, pins 8 of
   8, <n> diff lines; Unit332TwoWidthsTests <n of n>, TheGreenZoneTests <n of n>; <moved numbers,
   old and new>`.

**If the trace shows a writer the restore cannot hold against**, commit only the trace under
`test(app)`, and say so.

**Drop candidate:** none. It is the unit.

### Task 2 - the sheet, only where measured wrong (ruling 69)

1. **Edit only the lines ruling 69 lists**, with numbers from task 1's runs and this unit's commit.
2. **Commit and push:** `docs(unit353): the sheet's green block cited to a window that draws it - <what
   changed>`.

**Drop candidate:** none. Without it the sheet stays shown wrong at `:55` and `:57` (ruling 65).

### Task 3 - the two windows still on POTA (the drop candidate, ruling 70)

1. **Read, and do not change yet**, whether a POTA reply can land while `EmptyTab` or
   `BindingHealthTests`' window measures. Compare what each printed across task 1's three runs.
2. **Switch `EmptyTab`'s network sources off by `TheTopRowTests.NetworkSources` only if its printed
   numbers differed.** If you do, run step 0's filter once more, commit and push under `test(app)`.
3. **`BindingHealthTests` is reported, never changed.**
4. **Report it in section 4 as a finding.**

**Drop candidate: this task, whole.** If time runs short, skip it, and the report says so.

---

## 6. Parked - do not touch, do not raise

- **Step 3 and the verdict.** Tim's (ruling 46).
- **Everything under `src\`, every word on the screen and all markup** (rulings 47 and 65). That
  includes U11, which stands as built.
- **The best bet ranking, the spot reload, the sources, the heard count and the clock**, in `src\`:
  `RankBands`, `ApplyBestBet`, `ReloadSpotsAsync`, `BuildSources`, `DefaultSourceEnabled`, every source
  class and every clock source. Read them for the trace; change none.
- **Ruling 61's source switch** in `FixtureSettings` and `TheWorkingPanelsTests.Realized`, and the
  fixture's callsign, grid, license and dial.
- **The strayed-frequency line on the licensed fixture** (unit 341 item 2).
- **Every red in the record**, and every test except those tasks 0 to 3 name.
- **Steps 1 and 2.** Done. Their classes are not run in this unit.
- **Pictures**: no package, no running app, no drawn mockup (ruling 42).
- **PSK31 decoding, all of it** (ruling 23).
- **Transmit, all of it:** `TheOperatorCanStopItTests`, `TheWholeChainRunsFromOneRightClickTests`, and
  `TheMenuIsUnderTheMouseTests`.
- **The fifteen emptied files.** They are on unit 348's list.
- **Whether an RBN reader already started is stopped when its switch is later turned off** (unit 352
  item 2, *not examined*).
- **Carried in `PHASE_PLAN.md` §7 and belonging to Tim or the harness:**
  - the live license lookup and the live heard count;
  - the two id schemes, including `CPS-DEC-0163`;
  - real flags on country cards, PSK31 step 6, real PSK31 audio and the map bitmap's license;
  - the launcher's `PHASE_OUTCOME.md` entries and unit numbering, and `tools/status.sh`'s hard-coded
    `RULES_AT`.
- **Anything touching the radio, a decoder, a parser or the transmit chain. Any package.**

## 7. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.** HM-DEC-155.
- **Never run `TheOperatorCanStopItTests`, `TheWholeChainRunsFromOneRightClickTests` or
  `TheMenuIsUnderTheMouseTests`.**
- **Change nothing under `src\`** (rulings 47 and 65). A red that wants the screen changed is reported,
  not answered.
- **Do not loosen 0.262, one half, the 10% at 1920 or the 2 px rig tolerance. Do not edit either pinned
  fact's body. Do not add a clock seam, a retry, a wait-until loop or more than one further set of six
  settle passes** (rulings 49, 56, 61 and 67).
- **Do not switch a network source back on**, and do not change `BindingHealthTests` (rulings 61 and
  70).
- **Do not press, click or invoke the best bet, a band button, CQ, Stop or any send** (§0.2).
- **Do not break the view to watch a test fail** (ruling 19).
- **Do not edit the sheet beyond ruling 69's lines.**
- **Do not delete a file.** Empty it, comment it, list it (§6).
- **Do not edit `PHASE_OUTCOME.md`, `RUN_LEDGER.md`, the `STEP:` lines, `CURRENT_STEP`,
  `.run-unit\allowed.txt` or anything under `tools\arbiter\`.** They are the launcher's.
- **Never `git add -A` or `git add .`** (§2).
- `CLAUDE.md` §12.6 covers the rest. **No package. Report mismatches. Write American.**
- **The tool facts, as unit 352 found them.** Say which held for you:
  - **ran:**
    - `sh tools/status.sh` joined by `&&` to `date` and `timeout … dotnet test … | tail`;
    - `git add && git commit -m -m && git push && git log`;
    - `grep -n`, `grep -o -e` on trx files, `ls`, `wc -l`, `git diff --stat`, `git rev-parse`,
      `git log -p`;
    - `sh` on scripts written with the file editor into `testresults\`;
  - **asked for approval, not run:**
    - a `;`-joined line of `ls`, `pwd -W`, `git rev-parse`, `head` and `grep`;
    - a `grep` line with a `;` and `head`;
    - `git log | cut && git show`;
    - for unit 351: `grep -v -e "^\s*$"` in a pipe, and a variable assignment with `;`-joined greps;
  - **refused:**
    - a `for` loop over `$f` (*Contains simple_expansion*);
    - `sed -n` on a trx file under `testresults\` (*may only edit files in the allowed working
      directories*). Read trx files with the file reader;
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

A. The phase goal - the screen, done right. At <hash>: step 0 done (unit
   351), step 1 done (unit 347), step 2 done (unit 348). Step 3 0 of 1,
   blocked on Tim's verdict on docs/unit349-what-tim-looks-at.md, which
   this unit <did not change | changed at these lines>.
B. Step 3 and its exit criterion: Tim says it passed - not met, and no
   session can meet it. What this unit cleared under ruling 46's exception:
   1. what the test window held after its reloads, before the change -
      count <n>, best bet <band or none> at <hour read from the clock>;
      writers after Realized returns: <none | file:line>
   2. after the change - the guard <held on every run | failed: where>;
      count 6, best bet none, on every window of step 0's filter
   3. step 0's filter, three runs - <n of n each>; pins <8 of 8 each>;
      diff lines across the three <n>
   4. the unpinned numbers - <which returned to the fixture's window, old
      and new>; the pinned numbers <unmoved | which moved>
   5. the sheet's "6 stations" (:55, :57) - <cited to a fact that draws it
      at <hash> | corrected to: what>
   6. Unit332TwoWidthsTests <n of n>, TheGreenZoneTests <n of n>;
      carry-forward <n of n>, <n of n>
C. The report last. Section 4 raises N items on top of the carried queue.
   Say whether any shows the sheet wrong, and whether anything is left that
   a unit can do before Tim answers.
```

**Every line specific to this unit.** If something was not measured, say *not measured*. Do not fill
the shape.

```
UNIT:       353 - <complete|stopped> at task N of 3 - <date time, read from the clock>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   no - step 3 waits on Tim; the test window <draws its fixture's window whatever the hour | still depends on: what>; the sheet <re-cited | corrected> at <lines>
NUMBER:     step 0's filter after the change: <n of n>, <n of n>, <n of n>; unpinned facts reading the fixture's count <0 of k> -> <k of k>
DRIFT:      0
```

**Section 2 says, first**, that nothing on the screen moved, and whether the sheet changed (with the
lines). Then it says, in words Tim can read, what the test window now draws whatever time the tests
run.

**Section 3 leads with the answer:** does every step 0 fact on the test window now measure the window
its fixture declares, whatever the hour? Then:
- a table of the trace: fixture, width, mode, what landed in which settle pass, what the window held
  after, before and after the change;
- the writers, file and line;
- step 0's three runs, with every number that moved, old and new.

**Every appearance claim is computed, not seen. Say so once.**

**Section 4:** unit 352's section 4 verbatim per HM-DEC-139, from its line under
`## 4. What's blocking us` to its end, as committed in `8fa20cb1`, including the queues it carries.
Keep it in place with the file editor. In the carried text, mark:
- unit 349 item 1 (Tim's step 3 verdict): `STILL OPEN - Tim's; work instruction 353 authors nothing
  into the verdict`;
- unit 352 item 1 (the unpinned facts measure the hour's best bet and 0 stations): `TAKEN UP by work
  instruction 353 rulings 66 to 69`, with the result;
- unit 352 item 2's last two bullets (`BindingHealthTests` and `EmptyTab` still reach POTA): `TAKEN UP
  by work instruction 353 ruling 70`, with the result, or `NOT REACHED - task 3 dropped`.

Then this unit's items, under *Raised by unit 353*. **Each says whether it is a finding or an ask**
(ruling 71).

---

```
ARBITER-DECISION
STEP: 3
APPROACH: restore the test window declared heard count, sparkline and cleared best bet after Realized's settle, so step 0's unpinned facts measure the fixture's window whatever the hour of the run, guarded in the fixture - no screen change; trace every writer first; the sheet's "6 stations" words and 1920 green block cell re-cited to a fact that draws them; the two windows still reaching POTA (BindingHealthTests, EmptyTab) read as the drop candidate
MOVE: continue
WHY: Steps 0 to 2 are done and step 3 is Tim's word, so under ruling 46 a unit may act only where the sheet is shown wrong - and unit 352's own item 1 shows it: the sheet's "6 stations" is cited to a fact that now draws 0 and the hour's best bet, and every unpinned step 0 number now depends on the hour of the run. The loop test found nothing like this; it follows unit 352's successful change rather than repeating a failed one, and none of it touches keying, money or what the product promises.
STATE: blocked
DECIDED: ruling 65 - ruling 46's exception invoked for one point in two parts (the sheet's 6 stations cited to a fact drawing 0; unpinned facts depending on the hour), ruling 47 in force, nothing under src; ruling 66 - trace both licensed fixtures at 1400 and 1920 on FT8 and PSK31, pass by pass, and name every writer of the heard count, sparkline and IsBestBet by file and line; ruling 67 - after Realized's six settle passes set the declared count, sparkline and no best bet again, six passes more, a guard that names what failed, TheWorkingPanelsTests.Realized clears the best bet only, no clock seam, retry, wait-until loop or source change, a writer after the restore reported and the restore shipped only if the guard held every run; ruling 68 - step 0's filter green three times with pins 8 of 8, identical printed lines, every unpinned number named against unit 352's before and after runs, pinned numbers unmoved; ruling 69 - the sheet changes only at :47, :54-57, :71-73, :118, :20-21, :438-439 and section 5 where measured wrong; ruling 70 - EmptyTab's sources switched off only if its numbers differed across runs, BindingHealthTests never changed, the drop candidate; ruling 71 - no new ask unless measured, unit 349 item 1 carried as Tim's, the recommendation that if this ends clean no unit remains that ruling 46 licenses, and logged for the owner that ARBITER.md has no move for waiting on the owner's eyes so the loop spends a unit per call - all the author's, marked for Tim at step 3, overrulable
LICENCE: PHASE_PLAN.md sections 1, 4 (step 3 exit; step 0 exit criteria 1, 2, 5 and 6), 6 (the arbiter decides and continues; never loosen a test; transmit or a package stops) and 7; R26; ARBITER.md sections 2, 3, 4, 6 and 8; .run-unit/state-verdict.json (step 3 blocked on unit 352); .run-unit/s4-verdict.json (none); PHASE_OUTCOME.md unit 352 (UNIT 8 - STEP 3) STATE_WHY; unit 352 output.md at 8fa20cb1 section 1 task 1 and decisions 3 and 4, section 4 items 1 and 2; ruling 46 (work instruction 349), ruling 47 (work instruction 350), rulings 61 to 64 (work instruction 352); tests/Hamlet.App.Tests/Views/TheTopRowTests.cs 2067-2099, 2081-2083, 2092-2096, 2101-2144; tests/Hamlet.App.Tests/Views/TheWorkingPanelsTests.cs 484-486, 718-737; tests/Hamlet.App.Tests/Views/BindingHealthTests.cs 108; docs/unit349-what-tim-looks-at.md 20-21, 47, 54-57, 71-81, 118, 438-439, 499-514; docs/carry-forward-tests.txt; Directory.Build.props 825; DECISIONS.md 7 (HM-DEC-163); .run-unit/allowed.txt 14; RUN_LEDGER.md (units 349 to 352 cost); CLAUDE.md 0.0, 0.2, 0.5, 0.6; psk31 R12, R13, R14, R19; HM-DEC-139, HM-DEC-155, HM-DEC-163
ACCOMPLISHED: the numbers and words on the sheet Tim reads for the phase's last verdict come from a test window that draws the same thing whatever time the tests run - six stations, its sparkline and no best bet unless a test sets one - so every green block line on the sheet can be reproduced, and nothing on the screen moved under Tim's review
ADVANCES: none - this unit clears the one place unit 352 left the sheet shown wrong under Tim's step 3 review: its "6 stations" words cited to a fact that now draws 0, and step 0's unpinned facts measuring the hour's best bet instead of the fixture's declared window
END-ARBITER-DECISION
```
