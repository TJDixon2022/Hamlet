# Work instruction 352 - the sheet Tim holds, checked against the screen unit 351 changed

Step 3 of `PHASE_PLAN.md`, under ruling 46's one exception. **Nothing on the screen moves. The unit
checks that the sheet is the screen, and makes step 0's proof read the same on every run.**

Where each step stands:

- **Step 0 is `done`** on the state reader's verdict on unit 351 (`.run-unit\state-verdict.json`):
  *Every must-pass criterion is backed by measurements or green tests, including 1400 PSK31 at 228 px
  of 238.4 with panels at 465 of 455 in all three best bet states, and the one red left is a pin
  failure at 1920 where the layout still measured 190 and 503, not a missed limit.*
- **Steps 1 and 2 are `done`** on the state reader's verdicts on units 347 and 348.
- **Step 3 is `in progress`.** Its one criterion is Tim's word on `docs\unit349-what-tim-looks-at.md`.
  - No commit up to `cdead300` carries that word.
  - `DECISIONS.md` still tops at HM-DEC-163 (`:7`).

**Why a unit at all, when the phase waits on Tim.** Ruling 46 says no further unit is authored into
step 3 *unless the sheet is shown wrong*. Unit 351 changed the green block's markup (`1faf33a6`) and
left two things open that bear on whether the sheet Tim reads is the screen:
1. **Two test classes that read the green block were not run after the change.** Unit 351 item 2
   names `Unit332TwoWidthsTests` (`GreenZoneRight` at `:65`). This arbiter also found
   `TheGreenZoneTests`, which names `GreenZoneLeft` at `:443`. Neither is in step 0's filter or on the
   carry-forward list.
2. **Step 0's own proof is red on some runs.** `TheTopRowAndThePanelShareHoldWithTheBestBetPinnedBothWays`
   read 25 of 26 twice after the change, each time on a *pin did not hold* line at 1920 (unit 351
   item 1). The sheet's section 5, *Known reds* (`:498`-`509`), does not list it. Only unit 351's
   section 2 mentions it.

This arbiter read, and did not run, one likely cause:
- The test window is a real operator: `TheTopRowTests.Realized` (`:1877`-`1902`) sets callsign
  `KC3QIS` and leaves every source at its default.
- `AppSettings.DefaultSourceEnabled` (`AppSettings.cs:641`-`649`) turns on every source except SOTA and
  the sample feed.
- `BuildSources` (`MainWindowViewModel.cs:7602`-`7638`) builds POTA and, because the callsign is set,
  RBN.
- So the reload that overwrites the pin can be a live network reply landing while `Settle` pumps the
  dispatcher.

**The trace settles this. Do not build on it.**

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

These four are copied from work instruction 351. Unit 351 checked them and they held. Check them
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

**The numbers today** (unit 351, `1faf33a6` to `cdead300`; none re-run by this arbiter):
- **Step 0:** 6 of 6 must-pass and the nice-to-pass, on the state reader's verdict.
- **The 1400 top row:**
  - PSK31 is 228 px of 238.4, with panels at 465 of 455, whether the best bet is on the operator's
    band, on 40 m or absent;
  - FT8 is 216 px in all three.
- **1920:** 190 px, with panels at 503, in every case.
- **Step 0's filter:** 26 tests.
  - The first run after the change read 25 of 26, and the red was unit 351's own trace cast, since
    fixed.
  - The next two runs read 25 of 26, each red a pin message at 1920 PSK31. The pin went to 40 m both
    times: once with the best bet pinned on 20 m, once pinned absent.
- **The pinned fact's pins held:** 8 of 8 in the first run, then 7 of 8 and 7 of 8. The 40 m fact held
  4 of 4 in all three runs.
- **Carry-forward:** 111 of 111 app, 86 of 86 engine.
- **Not run since `1faf33a6`:** `Unit332TwoWidthsTests` (3 facts, `:50`, `:112`, `:151`) and
  `TheGreenZoneTests` (14 facts, `:78` to `:656`).
- **Step 3:** 0 of 1, and it stays 0 of 1 until Tim answers.

```
PHASE GOAL: The screen, done right - the main window as the approved mockup
            (step 0, done on unit 351); every achievements category page as
            trading cards (step 1, done on unit 347); what the last phase left
            (step 2, done on unit 348); then Tim at his window says it passed
            (step 3, 0 of 1, waiting on Tim).
UNIT GOAL:  Show that the sheet Tim holds is the screen at this tree - the two
            classes that read the green block run after unit 351's change, and
            step 0's pinned proof reading the same on every run because the test
            window no longer takes a live spot reload - with no screen change, and
            the sheet corrected only where a number moved or a red shows it wrong.
ADVANCES:   none - no criterion moves. This unit clears the two unverified
            points under Tim's step 3 review: a green block reader never run
            after the markup change, and a step 0 proof that goes red on some
            runs and is not on the sheet.
DRIFT:      0
```

---

## 2. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any mismatch.
**Report the mismatch; do not repair the instruction.** Mismatches go in the report even when the
work succeeds anyway.

Check:
- **Every file, line and item the header and §1 cite.**
- **`HEAD` and `origin/main` read `cdead300`** for this arbiter, and `output.md` in the tree is unit
  351's.
- **The version is 1.13.38** in `Directory.Build.props` (line 817 for this arbiter).
- **`DECISIONS.md` tops at HM-DEC-163.** If a higher id exists, or any commit after `cdead300`
  carries a verdict from Tim on step 3, **stop at task 0.** Quote it in section 4 and write nothing
  else: a verdict changes what the next unit is.
- **The launcher's files, one line each; edit none:**
  - `PHASE_OUTCOME.md` and `PHASE_STATUS.md` read steps 0, 1 and 2 `done` and step 3 `in progress`.
  - `PHASE_STATUS.md` reads `WORK_INSTRUCTION: 351`.
  - `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md` and files under `.run-unit\` are modified
    and uncommitted by the launcher.
- **The reload says `CLAUDE.md` §1 holds `CPS-DEC-0163`** (`.run-unit\reload.txt:9`, `:34`). Units 349
  to 351 found no `CPS-DEC` id there. It is parked with the id schemes. One line.
- **`tools\arbiter.bak-20260913\` is untracked at the root**, and `SESSION.lock` may be. **Never
  `git add -A` or `git add .`**; stage files by name.
- **The tool facts in §7** are unit 351's. Say which held for you.

**Reds expected, older than this unit. This unit runs none of them:**
- `TheMenuIsUnderTheMouseTests` (8);
- `TheAchievementsScreenTests.TheWindowDrawsEverySixRows` (1);
- `TheOperatorCanStopItTests` (2) and `TheWholeChainRunsFromOneRightClickTests` (2), **never run.**

**Expected at the start:**
- **the carry-forward list:** 111 of 111 app and 86 of 86 engine;
- **step 0's filter: 25 or 26 of 26**, the only possible red being a pin message in
  `TheTopRowAndThePanelShareHoldWithTheBestBetPinnedBothWays`. Its classes:
  - `TheTopRowTests` 11 or 12 of 12;
  - `TheWorkingPanelsTests` 8 of 8, `BindingHealthTests` 1 of 1 and `VoiceTests` 5 of 5;
- **`Unit332TwoWidthsTests` and `TheGreenZoneTests`: unknown.** They have not run since `1faf33a6`,
  and that is what task 0 finds out.

**Expected at the end:** step 0's filter 26 of 26, or more if this unit adds a fact, **on each of three
runs**. The carry-forward list is unchanged. **If a red turns green, a green turns red, or a count
moves, say which.**

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
  filter, at most twice in this unit (ruling 60).

**Step 3, `PHASE_PLAN.md` §4, in full:**

> **Delivers:** Tim opens every page at his window size and says it passed.
> **Entry:** step 2 done.
> **Exit:**
> - Tim says it passed. *must-pass* No script can evaluate this.

**Step 0's exit, `PHASE_PLAN.md` §4, the criteria this unit re-reads and does not re-open:**

> - At 1920, the top row (neighborhood card and rig display) is about 190 px tall and the working
>   panels below the tabs take the rest; the numbers reported. *must-pass*
> - At 1400 the same shape holds, no callsign is clipped, and the facts go beside or under by the
>   unit's stated rule; the numbers reported at both widths. *must-pass*
> - `BindingHealthTests`, `VoiceTests` and the carry-forward list green. *must-pass*

**R26, Tim, 2026-09-12 (`PHASE_PLAN.md` §R), the lines that bite here:**

> - **The top row is one band, about 190 px tall at 1920**, and the working panels below the tabs take
>   the rest of the window. At no window size do the working panels get less than half the height
>   below the band pills.
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

**The arbiter's rulings 1 to 58 stand as units 337 to 351 built them.** They are gathered on the sheet,
marked for Tim at step 3, and overrulable. **This unit re-opens, re-words or re-builds none of them.**
These bite here:
- **1.** *The working panels* means the three panels themselves, at least half the height below the
  band pills at 1920 and 1400, with the readiness strip hidden.
- **19. Watching red.** Show a new assertion fail once, against a deliberately wrong expectation set on
  the test window only, and give the failure line. **Never break markup or a view to watch a test
  fail.**
- **46.** No further unit is authored into step 3 until Tim answers, unless the sheet is shown wrong.
- **47**, back in force after ruling 53's one unit: **no `src` or markup change**, so the sheet Tim
  holds stays the screen.
- **49 and 56**: `TheTopRowAndThePanelShareHoldWithTheBestBetPinnedBothWays` pins `IsBestBet` on the
  test window. It asserts the 1920 row within 10% of 190, the 1400 row `<= 0.262 * below`, the three
  panels `>= below / 2`, the rig panel within 2 px of the card, and the pin read back. **Its limits are
  not loosened, and its body is not edited to turn it green.** A pin-message red is neither a layout
  miss nor a layout pass.
- **U11**, unit 351's arrangement (sheet `:425`), stands as built.

**The arbiter's rulings for this unit.** They are the author's, marked for Tim at step 3, and
overrulable:

59. **Ruling 46's exception is invoked for two points only.** The sheet is unverified where
    `Unit332TwoWidthsTests` and `TheGreenZoneTests` read the green block, and where step 0's proof is
    red on some runs without being on the sheet.
    - This unit authors nothing into Tim's verdict. It moves no word, no markup and no view model, and
      it changes nothing under `src\` (ruling 47).
    - *Why:* Tim is asked to judge a screen against a sheet. A class that reads the changed column and
      was never run, and a red the sheet does not list, are the two places that sheet could be wrong
      without anyone knowing. A phase otherwise waiting on Tim should not leave either one open.
60. **The two unrun classes are run once before anything changes, and once after task 1.**
    - **A green is the answer.** Report *n of n* per class.
    - **A red whose failure line is an old number**, such as the right column's width, the best bet
      row's type or a line count that U11 changed, is a stale expectation. It is corrected under R12 in
      that test only:
      - the old and the new number go in the test's remark and in the report;
      - the new number must be one a run in this unit printed;
      - no limit that stands for R26, ruling 1, 0.262, one half or the 2 px rig tolerance is loosened.
    - **Any other red** is reported with its failure line and left red, and it goes into the sheet's
      section 4 as a known item. Examples: a clipped word, an overlap, a word or band no longer the
      largest text, or a limit the sheet states. This is the sheet shown wrong, and **the screen is
      not changed to answer it** (ruling 47). The next arbiter reads it.
61. **The pin failure is traced before it is fixed, and fixed on the test window only.**
    - **The trace** runs the 1920 PSK31 pinned cases (on 20 m and absent), where unit 351's reds fell,
      five times each, on a window built with `Realized(width, telemetry)`. For each case it prints:
      - every spot reload's trigger and whether it landed after the pin;
      - each source's status as the model reports it;
      - how many spots arrived and from which source;
      - where the best bet ended up;
      - whether the pin held.
    - It reads through public members and the telemetry file only. **Nothing is pressed.**
    - **The fix**, if the trace shows a live source's reply as the cause: in the tests' own fixture,
      `Realized` switches the network sources off with `AppSettings.SetSourceEnabled(<name>, false)`,
      using each source's `SourceName` constant.
      - If the trace shows another cause, fix it on the test window only, by the same standard, and
        name it.
    - **Nothing under `src\` changes.** That means `BuildSources`, `ReloadSpotsAsync`, `ApplyBestBet`,
      `RankBands`, `DefaultSourceEnabled`, `NotifyGreenZoneForTests`, any clock and any source class
      stay as they are.
    - **Not a clock seam, a longer settle loop or a retry.** A retry hides the race rather than removing
      it.
    - The fix is the unit's own and overrulable. Name the words that overrule it.
62. **The proof is three clean runs.**
    - Step 0's filter runs three times after task 1's change. Each run must be all green, with
      `TheTopRowAndThePanelShareHoldWithTheBestBetPinnedBothWays` holding every pin (8 of 8) and the
      40 m fact holding 4 of 4, both bodies unedited.
    - **Every test in `TheTopRowTests` and `TheWorkingPanelsTests` shares `Realized`**, so the report
      names every fact whose measured number moved when the sources were switched off, with old and new.
    - **If a pin still fails in any of the three runs**, report the run and the trigger the trace
      printed. Do not run a fourth time, and ship the fixture change only if the trace showed it removes
      the cause it names. That is a finding, not `partial`: step 0 is `done`, and no criterion is
      re-opened.
63. **The sheet, `docs\unit349-what-tim-looks-at.md`, changes only where this unit measured it
    wrong:**
    - a number in section 2.1 or 2.2 that a run in this unit printed differently, replaced and cited
      to this unit's commit;
    - section 2.1's source note (`:71`) and section 2.2's (`:113`-`117`), only where their test counts
      are wrong at this tree;
    - section 5, *Known reds*: one line for the pin red, saying either that it is gone (the fixture
      commit, three runs) or that it still stands, with its trigger;
    - section 4: one item per ruling 60 red that stays red, in the list's own one-line form, the list
      renumbered;
    - **no other line.** Not the verdict form, not section 3, and no U row, because a test fixture is
      not a screen choice.
64. **Section 4 raises no new ask of its own unless the measurements put one there.** Unit 349 item 1
    is carried as Tim's open ask. Everything else this unit finds is a finding, and says so.
    - **The arbiter's recommendation to the next arbiter**, marked author's: if this unit ends with the
      two classes green, the pins holding and the sheet unchanged or corrected, nothing is left that a
      unit can do for this phase until Tim answers.

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
  fixture change and ruling 60's corrections are edits to existing tests.
- **R19**: American spelling.
- **HM-DEC-139**: open asks are carried verbatim until answered.

---

## 4. Status cadence

Write status **before every `dotnet` command, after every commit and after every task**, with
`sh tools/status.sh`. It ran for every write in unit 351. If it is refused, take a `date` reading and
paste it whole. **Never compose a time.**

The script hard-codes `RULES_AT: HM-DEC-161 (2026-09-11)`. **After every write**, set that line back
to `HM-DEC-163 (2026-09-12)`, or to the highest id §2 finds, with the file editor.

The watchdog kills a session only when its process tree has used no CPU for ten minutes. **Write
status between sections of the report**, so a quiet stretch of editing is never ten minutes of nothing.

---

## 5. The tasks

### Task 0 - the trace: the two unrun readers, and what lands on the pin

Measure before anything changes. **Say what you find rather than confirming the header or §1.**

1. **Commit `WORK_INSTRUCTIONS.md`, `PHASE_STATUS.md` and a patch bump (1.13.38 -> 1.13.39, with its
   comment block).**
   - In `PHASE_STATUS.md`, change only the `WORK_INSTRUCTION:` line, to `352 - the sheet Tim holds,
     checked against the screen unit 351 changed`. Do not touch its `STEP:` lines, `CURRENT_STEP` or
     `HEARTBEAT`.
   - Do not commit `.run-unit\`, `PHASE_OUTCOME.md`, `RUN_LEDGER.md`, `SESSION.lock` or
     `tools\arbiter.bak-20260913\`.
   - The message is `chore(unit352): the sheet checked against the screen unit 351 changed - the trace
     first`.
   - Run the carry-forward list, status first, and give both invocations as *n of n*.
2. **Step 0's filter, once**, status first, with a stated timeout. Give each class as *n of n*, every
   pin line, and the failure line of any red.
3. **`Unit332TwoWidthsTests` and `TheGreenZoneTests`, once, in one filter**, status first. Give each
   class as *n of n* and every failure line whole. Sort each red by ruling 60: stale number or sheet
   shown wrong. **Change nothing yet.**
4. **Add `Unit352TraceTheSpotReloadOnTheTestWindow` to `TheTopRowTests`.** It asserts nothing and
   presses nothing, and it prints what ruling 61 lists for 1920 PSK31, pinned on 20 m and absent, five
   times each. Also print:
   - which sources `BuildSources` built for this window and whether each is enabled;
   - whether any source reported a network status, such as a connection, an error or a count.
5. **Answer from the numbers, in section 1, before task 1 starts:**
   - Are the two classes green? For each red, which kind is it, and why?
   - In how many of the ten traced cases did the pin not hold, and which trigger and source landed each
     time?
   - Is a live source's reply the cause? If not, what is?
   - Which fixture change will task 1 make, and why is it on the test window only?

**Drop candidate:** none. Task 1 is built on it.

### Task 1 - the fixture quieted, the readers re-run, the proof three times (rulings 60 to 62)

1. **Make ruling 61's fixture change** in the tests only. Keep the existing remarks' voice: say what
   changed, why, and the trace's numbers.
2. **Make any ruling 60 stale-number correction** that task 0 found. Watch each red by ruling 19 if the
   corrected assertion is new in shape; otherwise its task 0 failure line is its watched red.
3. **Add a line to the trace** that prints the same ten cases with the change in place.
4. **Run step 0's filter three times**, status first each time. Report every run's counts and pin
   lines, and every number that moved from task 0's run.
5. **Run `Unit332TwoWidthsTests` and `TheGreenZoneTests` again**, then the carry-forward list, status
   first.
6. **Commit and push:** `test(app): task 1 - the test window takes no live spot reload - <what was
   switched off>; step 0's filter <n of n> three times, pins 8 of 8; Unit332TwoWidthsTests <n of n>,
   TheGreenZoneTests <n of n>; <moved numbers or none>`.

**If the trace shows no cause the test window can remove**, commit only the trace and any ruling 60
correction, under `test(app)`, and say so.

**Drop candidate:** none. It is the unit.

### Task 2 - the sheet, only where measured wrong (ruling 63)

1. **Edit only the lines ruling 63 lists**, with numbers from task 1's runs and this unit's commit.
   **If nothing measured differs and the pin red is gone, the only edit is section 5's one line.**
2. **Commit and push:** `docs(unit352): the sheet checked - <what changed, or: section 5 only>`.

**Drop candidate:** none. Without it the sheet may not be the screen (ruling 59).

### Task 3 - what the test window reaches over the network (the drop candidate)

1. **Read, and do not change**, whether `RbnActivitySource` and `PotaActivitySource` open a connection
   or a request when they are constructed, or only when polled while enabled. Give file and line.
2. **From task 0's and task 1's telemetry**, say whether any test window in this unit reached POTA or
   RBN, and under which callsign, before and after the fixture change.
3. **Report it in section 4 as a finding.** Nothing under `src\` changes. If a connection is made while
   a source is switched off, name it for the next arbiter and do not fix it.

**Drop candidate: this task, whole.** If time runs short, skip it, and the report says so.

---

## 6. Parked - do not touch, do not raise

- **Step 3 and the verdict.** Tim's (ruling 46).
- **Everything under `src\`, every word on the screen and all markup** (rulings 47 and 59). That
  includes U11, which stands as built.
- **The best bet ranking, the spot reload, the sources and the clock**, in `src\`: `RankBands`,
  `ApplyBestBet`, `ReloadSpotsAsync`, `BuildSources`, `DefaultSourceEnabled`, every source class and
  every clock source (ruling 61).
- **The strayed-frequency line on the licensed fixture** (unit 341 item 2). The fixture's callsign,
  grid, license and dial stay as they are. Only the sources are switched off.
- **Every red in the record**, and every test except those task 0, task 1 and ruling 60 name.
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
  - the launcher's `PHASE_OUTCOME.md` entries and unit numbering, and `tools/status.sh`'s hard-coded
    `RULES_AT`.
- **Anything touching the radio, a decoder, a parser or the transmit chain. Any package.**

## 7. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.** HM-DEC-155.
- **Never run `TheOperatorCanStopItTests`, `TheWholeChainRunsFromOneRightClickTests` or
  `TheMenuIsUnderTheMouseTests`.**
- **Change nothing under `src\`** (rulings 47, 59 and 61). A red that wants the screen changed is
  reported, not answered.
- **Do not loosen 0.262, one half, the 10% at 1920 or the 2 px rig tolerance. Do not edit either pinned
  fact's body. Do not lengthen `Settle`, add a retry, or add a clock seam** (rulings 49, 56 and 61).
- **Do not press, click or invoke the best bet, a band button, CQ, Stop or any send** (§0.2).
- **Do not break the view to watch a test fail** (ruling 19).
- **Do not edit the sheet beyond ruling 63's lines.**
- **Do not delete a file.** Empty it, comment it, list it (§6).
- **Do not edit `PHASE_OUTCOME.md`, `RUN_LEDGER.md`, the `STEP:` lines, `CURRENT_STEP`,
  `.run-unit\allowed.txt` or anything under `tools\arbiter\`.** They are the launcher's.
- **Never `git add -A` or `git add .`** (§2).
- `CLAUDE.md` §12.6 covers the rest. **No package. Report mismatches. Write American.**
- **The tool facts, as unit 351 found them.** Say which held for you:
  - **ran:**
    - `sh tools/status.sh` joined by `&&` to `date` and `timeout … dotnet test`, then
      `| grep -e … | cut` or `| tail`;
    - `git add && git commit -m -m && git push && git log | cut && sh tools/status.sh && date`;
    - `grep -n -e`, `grep -o -e` on trx files, `sed -n`, `ls`, `hostname`, `git status --short`,
      `git diff --stat`, `git rev-parse HEAD origin/main`;
  - **asked for approval, not run:**
    - `grep -v -e "^\s*$"` in a pipe;
    - a shell variable assignment then `;`-joined greps with `cut`;
    - for unit 350: `cd` before `git`, a `$TEMP` expansion, a `>` redirect joined with `;`,
      `git check-ignore -v`, `grep -E` with a brace quantifier, and `sed -e` substitutions in pipes;
    - for this arbiter: `grep … | grep -v`, and `| head` joined by `;` to `wc -l`;
  - **refused:** none for unit 351;
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
   waiting on Tim's verdict on docs/unit349-what-tim-looks-at.md, which
   this unit checked against the screen and <did not change | changed at
   these lines>.
B. Step 3 and its exit criterion: Tim says it passed - not met, and no
   session can meet it. What this unit checked under ruling 46's exception:
   1. Unit332TwoWidthsTests after unit 351's change - <n of n>; reds
      <none | stale number corrected | sheet shown wrong: which>
   2. TheGreenZoneTests after unit 351's change - <n of n>; the same
   3. step 0's pinned proof, three runs after the fixture change -
      <n of n each>; pins <8 of 8 each | which failed>; the cause the trace
      named: <trigger and source>
   4. step 0's numbers after the change - 1920 <px>, 1400 PSK31 <px>,
      1400 FT8 <px>, panels <px>; <none moved | which moved, old and new>
   5. carry-forward - <n of n>, <n of n>
C. The report last. Section 4 raises N items on top of the carried queue.
   Say whether any shows the sheet wrong, and whether anything is left that
   a unit can do before Tim answers.
```

**Every line specific to this unit.** If something was not measured, say *not measured*. Do not fill
the shape.

```
UNIT:       352 - <complete|stopped> at task N of 3 - <date time, read from the clock>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   no - step 3 waits on Tim; <the sheet shown right | the sheet corrected at <lines>>; step 0's proof <steady | still flaky>
NUMBER:     step 0's filter after the change: <n of n>, <n of n>, <n of n>; pin failures <was 1 in each of 2 runs> -> <n>
DRIFT:      0
```

**Section 2 says, first**, that nothing on the screen moved, and whether the sheet changed (with the
lines). Then it names what the test window no longer does, in words Tim can read.

**Section 3 leads with the answer:** is the sheet the screen at this tree? Then:
- a table of the two classes, per fact, with result and failure line;
- a table of the ten traced cases, before and after the change, with trigger, source, pin held;
- step 0's three runs, with every number that moved.

**Every appearance claim is computed, not seen. Say so once.**

**Section 4:** unit 351's section 4 verbatim per HM-DEC-139, from its line under
`## 4. What's blocking us` to its end, as committed in `cdead300`, including the queues it carries.
Keep it in place with the file editor. In the carried text, mark:
- unit 349 item 1 (Tim's step 3 verdict): `STILL OPEN - Tim's; work instruction 352 authors nothing
  into the verdict, and the sheet was checked against the screen at <hash>`;
- unit 350 item 2 and unit 351 item 1 (the pin and the spot reload): `TAKEN UP by work instruction
  352 rulings 61 and 62`, with the result;
- unit 351 item 2 (the unrun readers of the green block): `TAKEN UP by work instruction 352 ruling
  60`, with the counts.

Then this unit's items, under *Raised by unit 352*. **Each says whether it is a finding or an ask**
(ruling 64). Task 3's answer, if it ran, is one of them.

---

```
ARBITER-DECISION
STEP: 3
APPROACH: check the sheet Tim holds against the screen unit 351 changed, with no screen change - run the two unrun classes that read the green block (Unit332TwoWidthsTests, TheGreenZoneTests), trace which spot reload overwrites the pinned best bet, and switch the live spot sources off on the test window only so step 0's pinned proof holds on three runs; the sheet corrected only where a number moved or a red shows it wrong; what the test window reaches over the network read as the drop candidate
MOVE: continue
WHY: Steps 0 to 2 are done and step 3 is Tim's word, so under ruling 46 a unit may only act where the sheet could be wrong - and unit 351 left two such places, a green block reader never run after its markup change and a step 0 proof red on some runs that the sheet's known reds do not list. The loop test found nothing like this approach, and none of it touches keying, money or what the product promises.
STATE: in progress
DECIDED: ruling 59 - ruling 46's exception invoked for two points only, ruling 47 back in force, nothing under src changes; ruling 60 - Unit332TwoWidthsTests and TheGreenZoneTests run before and after, a stale number U11 changed corrected under R12 in that test with no R26 limit loosened, any other red reported and put on the sheet's section 4 without changing the screen; ruling 61 - the pin failure traced over ten 1920 PSK31 cases through telemetry and public members, then fixed in the tests' Realized fixture only by switching the network sources off with SetSourceEnabled, no clock seam, retry or longer settle; ruling 62 - proof is step 0's filter all green three times with pins 8 of 8 and bodies unedited, every moved number named, a pin still failing reported as a finding and step 0 not re-opened; ruling 63 - the sheet changes only where measured wrong, plus one section 5 line on the pin red, no U row; ruling 64 - no new ask unless measured, unit 349 item 1 carried as Tim's, and the recommendation that if this unit ends clean nothing is left that a unit can do before Tim answers - all the author's, marked for Tim at step 3, overrulable
LICENCE: PHASE_PLAN.md sections 1, 4 (step 3 exit; step 0 exit criteria 1, 5 and 6), 6 (the arbiter decides and continues; never loosen a test; transmit or a package stops) and 7; R26; ARBITER.md sections 2, 4, 6 and 8; .run-unit/state-verdict.json (step 0 done on unit 351, the one red a pin failure at 1920); .run-unit/s4-verdict.json (none); PHASE_OUTCOME.md unit 351 (UNIT 7 - STEP 0) STATE_WHY; unit 351 output.md at cdead300 sections 1 and 2 and section 4 items 1 and 2, unit 350 item 2; ruling 46 (work instruction 349) and ruling 47 (work instruction 350); tests/Hamlet.App.Tests/Views/TheTopRowTests.cs 688, 709, 754, 1651-1715, 1771-1778, 1877-1902; tests/Hamlet.App.Tests/Views/Unit332TwoWidthsTests.cs 50, 65, 112, 151; tests/Hamlet.App.Tests/ViewModels/TheGreenZoneTests.cs 443; tests/Hamlet.App.Tests/Views/TheWorkingPanelsTests.cs (shares Realized); src/Hamlet.App/ViewModels/MainWindowViewModel.cs 7602-7638, 16969-16984, 8098, 11066; src/Hamlet.App/Settings/AppSettings.cs 641-663; docs/unit349-what-tim-looks-at.md 71, 93, 106-108, 113-117, 425, 432-496, 498-509; Directory.Build.props 817; DECISIONS.md 7 (HM-DEC-163); .run-unit/allowed.txt 14; CLAUDE.md 0.0, 0.2, 0.5, 0.6; psk31 R12, R13, R14, R19; HM-DEC-139, HM-DEC-155, HM-DEC-163
ACCOMPLISHED: the sheet Tim reads to give the phase's last verdict is shown to be the screen after unit 351's change - every test that reads the green block has run on it - and step 0's proof reads the same on every run because the test window no longer takes a live spot reload in the middle of a measurement; whatever does not match is on the sheet, so nothing Tim is told is untested
ADVANCES: none - this unit clears the two unverified points under Tim's step 3 review: Unit332TwoWidthsTests and TheGreenZoneTests never run after the green block's markup changed, and step 0's pinned proof going red on some runs from a live spot reload without being on the sheet
END-ARBITER-DECISION
```
