# Work instruction 351 - step 0's last red: the check fitted at 1400

Step 0 of `PHASE_PLAN.md`. **One case moves on the screen, and the sheet Tim holds moves with it.**

Where each step stands:

- **Step 0 is `partial`** on the state reader's verdict on unit 350 (`.run-unit\state-verdict.json`):
  *Five of the six must-pass criteria are backed by measured numbers at this tree, but criterion 5
  fails at 1400 on PSK31 when the best bet is on the operator's own band, where the top row is 247 px
  against a limit of 238.4 and the panels are 446 px against 455, and this is a layout miss rather
  than a stop.*
  - It is the only red standing between step 0 and `done`.
  - It is also the only place R26's *at no window size do the working panels get less than half the
    height below the band pills* is broken.
- **Steps 1 and 2 are done** on the state reader's verdicts on units 347 and 348.
- **Step 3 is waiting for Tim's verdict** on `docs/unit349-what-tim-looks-at.md`. No commit after
  `1456ac5c` carries an answer, and `DECISIONS.md` still tops at HM-DEC-163 (`:7`). **Nothing is
  authored into step 3.**

Unit 350 held the screen still so the sheet would stay the screen (ruling 47), and it left the red
for the next arbiter to choose between a fix and Tim's verdict. **This arbiter chooses the fix.**
- Tim's verdict cannot move until he reads the sheet. Meanwhile the one known must-pass miss sits
  on the sheet as *what will look wrong, but is known*, item 1.
- A 9 px miss that arrangement may close is not a reason to spend Tim's review on it.
- The sheet is updated in the same unit, so it stays the screen.

The unit has four tasks, 0 to 3. Task 3 is the drop candidate.

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

These four are copied from work instruction 350; unit 350 checked them and they held. Check them
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

**The numbers today** (unit 350, `b49eb3ab`, `437cedd8`; none re-run by this arbiter):
- **Step 0:** 5 of 6 must-pass evidenced at `c88d3974`, and the nice-to-pass holds at 1920 and 1400.
- **Criterion 5, the one red:** `TheTopRowAndThePanelShareHoldWithTheBestBetPinnedBothWays`
  (`TheTopRowTests.cs:688`). On PSK31 at 1400, licensed, with the best bet drawn on 20 m, the band the
  fixture is on:
  - the top row is **247 px of 910 = 0.271**, against 0.262 × 910 = 238.4, **8.6 px over**;
  - the three panels, strip hidden, are **446 px**, against 455, **9 px under half**.
- **The same window with the best bet on another band:** 237 px (green block 109). **With no best
  bet:** 228 px.
- **The green block is 10 px taller when the best bet carries its check** (119 against 109). Unit
  350 did not read why.
- **Step 0's filter reads 23 of 24**, the one red being this fact. Carry-forward is 111 of 111 app
  and 86 of 86 engine.
- **Step 3:** 0 of 1, and it stays 0 of 1 until Tim answers.

**What this arbiter read in the tree at `c88d3974`. None of it has been run:**
- **The green block's two regions are one `Grid`, `GreenZoneRegions`, `ColumnDefinitions="*,Auto"`**
  (`MainWindow.axaml:639`-`640`).
  - The left column, `GreenZoneLeft` (`:658`-`717`), holds the band line, the license line and the
    rule of thumb. The last two are `TextWrapping="Wrap"`.
  - The right column, `GreenZoneRight` (`:720`-`792`), is **`Auto`**. It holds the best bet row
    (`:727`-`746`, *best bet now:* and the `GreenZoneBestBet` button, `Command="{Binding
    TuneToBestBetCommand}"` at `:737`) over `GreenZoneHeardGrid`.
  - So the widest thing in the right column sets how much width the left column's wrapping lines
    get.
- **The check is text in the button's word:** `GreenZone.OnIt = " ✓"` (`GreenZone.cs:51`), appended
  by `BestBetWords` only when the best bet is the band the dial is in (`:296`-`301`).
- **`FitTheHeardCount`** (`MainWindow.axaml.cs:98`-`147`) already decides, from the words' widths,
  whether the sparkline hides. It counts the best bet row's width in the right column (`:114`-`119`).
- **Unit 341 item 1 (`0f383a3`)** measured that where the best bet draws, the block's right column
  widens from 140 to 180 px and the rule of thumb takes a third line.
- **Hypotheses, not measured.** The *✓* can cost 10 px more in two ways:
  - it widens the word again, wrapping a second left line;
  - it falls back to a font whose line is taller than the 11 px row.

  **The trace settles which.** Do not build on either guess.

```
PHASE GOAL: The screen, done right - the main window as the approved mockup
            (step 0: partial, 5 of 6 must-pass, criterion 5 red at one case);
            every achievements category page as trading cards (step 1, done on
            unit 347); what the last phase left (step 2, done on unit 348); then
            Tim at his window says it passed (step 3, 0 of 1, waiting for Tim).
UNIT GOAL:  Fit the green block at 1400 so that, with the best bet drawn on the
            operator's own band, the top row is at most 0.262 of the height below
            the band pills and the three panels at least half - by arrangement,
            every word, check and command unchanged - with every other pinned case
            still holding, and the sheet Tim holds brought to the new screen.
ADVANCES:   step 0 criterion 5 (the 1400 share, at its worst case) and R26's
            half-the-height floor under criterion 1, which is what stands between
            step 0 and done.
DRIFT:      0
```

---

## 2. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any mismatch.
**Report the mismatch; do not repair the instruction.** Mismatches go in the report even when the
work succeeds anyway.

Check:
- **Every file, line and item §1 cites.**
- **`HEAD` and `origin/main` read `c88d3974`** for this arbiter, and `output.md` in the tree is unit
  350's.
- **The version is 1.13.37** in `Directory.Build.props` (line 809 for this arbiter).
- **`DECISIONS.md` tops at HM-DEC-163.** If a higher id exists, or any commit after `c88d3974`
  carries a verdict from Tim on step 3, **stop at task 0.** Quote it in section 4 and write nothing
  else: a verdict changes what the next unit is.
- **The launcher's files, one line each; edit none:**
  - `PHASE_OUTCOME.md` and `PHASE_STATUS.md` read step 0 `partial`, steps 1 and 2 `done`, step 3
    `in progress`.
  - `PHASE_STATUS.md` reads `WORK_INSTRUCTION: 350`.
  - `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md` and files under `.run-unit\` are modified
    and uncommitted by the launcher.
- **The reload says `CLAUDE.md` §1 holds `CPS-DEC-0163`.** Units 349 and 350 found no `CPS-DEC` id
  there. It is parked with the id schemes. One line.
- **`tools\arbiter.bak-20260913\` is untracked at the root**, and `SESSION.lock` may be. **Never
  `git add -A` or `git add .`**; stage files by name.
- **The tool facts in §7** are unit 350's. Say which held for you.

**Reds expected, older than this unit. This unit runs none of them:**
- `TheMenuIsUnderTheMouseTests` (8);
- `TheAchievementsScreenTests.TheWindowDrawsEverySixRows` (1);
- `TheOperatorCanStopItTests` (2) and `TheWholeChainRunsFromOneRightClickTests` (2), **never run.**

**Expected at the start:**
- **the carry-forward list:** 111 of 111 app and 86 of 86 engine;
- **step 0's filter: 23 of 24.**
  - `TheTopRowTests` 9 of 10, the red being `TheTopRowAndThePanelShareHoldWithTheBestBetPinnedBothWays`.
  - `TheWorkingPanelsTests` 8 of 8, `BindingHealthTests` 1 of 1 and `VoiceTests` 5 of 5.

**Expected at the end:** step 0's filter all green, whatever count it then has, with the carry-forward
list unchanged. **If a red turns green, a green turns red, or a count moves, say which.**

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
- *A string will not fit: shorten and say which, or widen; never clip.* **This unit narrows that
  to arrangement only (ruling 55).**
- *Anything touches the radio, a decoder, a parser or the transmit chain: `MOVE: stop`.*
- *A package is needed: `MOVE: stop`.*
- *Carried: every open ask from unit 336's queue, verbatim in every unit. Plus: real flags on earned
  country cards - undecided, Tim's; the PSK31 phase's step 6 - Tim's, at the radio; the recording of
  real PSK31 audio; the id-scheme split; the map bitmap's license.*

**The arbiter's rulings 1 to 52 stand as units 337 to 350 built them, except where ruling 53 below
overrules ruling 47.** They are gathered on the sheet, marked for Tim at step 3, and overrulable.
**This unit re-opens, re-words or re-builds none of the others.** These bite here:
- **1.** *The working panels* means the three panels themselves, at least half the height below the
  band pills at 1920 and 1400, with the readiness strip hidden. The number with the strip showing is
  also reported.
- **2.** The rule of thumb is the mockup's sentence, `GreenZone.RuleOfThumb`. Its words do not
  change.
- **19. Watching red.** Show a new assertion fail once, against a deliberately wrong expectation set on
  the test window only, and give the failure line. **Never break markup or a view to watch a test
  fail.** Where the new assertion is red against the tree as it is, that is the watched red.
- **46.** No further unit is authored into step 3 until Tim answers, unless the sheet is shown wrong.
- **49**, as unit 350 built it: `TheTopRowAndThePanelShareHoldWithTheBestBetPinnedBothWays` pins
  `IsBestBet` drawn on 20 m and absent, on FT8 and PSK31, at 1920 and 1400. It asserts the 1920 row
  within 10% of 190, the 1400 row `<= 0.262 * below`, the three panels `>= below / 2`, the rig panel
  within 2 px of the card, and the pin read back. **Its limits are not loosened, and its body is not
  edited to turn it green.**

**The arbiter's rulings for this unit.** They are the author's, marked for Tim at step 3, and
overrulable:

53. **The screen moves for this one case, and the sheet moves with it. This overrules ruling 47 for
    this unit.**
    - Files under `src\Hamlet.App\Views\` and `src\Hamlet.App\ViewModels\` may change, only as ruling
      55 allows.
    - The sheet changes only as task 2 says. When this unit ends, the sheet describes the screen at
      this unit's last commit.
    - Ruling 46 holds: nothing here is step 3's.
    - *Why:* Tim cannot answer a sheet whose first known imperfection is a must-pass miss someone
      could have closed. Criterion 5's red is the only open work in a phase otherwise waiting on him.
54. **The trace chooses the mechanism; the arbiter does not.**
    - Before anything is built, name in pixels what makes the 1400 PSK31 green block 10 px taller
      with the check than with a best bet on another band, and 19 px taller than with none. Give it
      line by line:
      - the right column's width;
      - the left column's width;
      - each left line's height and line count;
      - the best bet row's height, and the height of the word with and without *✓*.
    - Then, **on the test window only**, measure each arrangement the unit considers, the way unit
      340 measured its options. For each, give the 1400 PSK31 top row and panels with the best bet on
      his band, and the 1920 row.
    - Build the one that fits. Name it as the unit's own and overrulable, with the words that
      overrule it.
55. **Arrangement only. What may not change:**
    - **No word changes.** Not *best bet now:*, not the band name, not *✓*, not the license line,
      not the rule of thumb (ruling 2), not *heard just now*, the count or *last minute*.
    - **Nothing hides.** The best bet, its check and the count stay drawn at 1400 whenever they
      draw now. The sparkline's existing rule (U1) may be re-used but not tightened into hiding
      anything else. §0.5: *collapsing hides detail, never information.* §0.6: the check is the carrier
      that is not color.
    - **`GreenZoneBestBet` stays the pill's own press:** its `Command`, `ToolTip.Tip`, `IsVisible`
      binding and `x:Name` unchanged. **Nothing presses it**, in any test or trace (§0.2).
    - **The ranking and the clock do not change:** nothing in `RankBands`, `ApplyBestBet`,
      `BestBetWords`, `GreenZone.For` or any clock source.
    - **The band stays the block's largest text**, and **the 1920 row stays 190 px** in every pinned
      case.
    - **What may change:**
      - where the best bet row and the heard grid stand inside `GreenZoneBlock`;
      - `GreenZoneRegions`' columns;
      - a font size, line height or font family on the check or best bet row only, if the trace
        shows the glyph's line as the cost;
      - `FitTheHeardCount`'s rule, kept measured from the words and not from a window width, as its
        own remark requires.
    - **If no arrangement reaches 238.4 and 455:** ship the best measured one if it is closer than 247
      and 446 and moves no other pinned case past its limit; otherwise ship nothing. Report the
      numbers per arrangement. That is `partial` under §6, and **no word is shortened to close it.**
56. **The pinned fact is the proof, unchanged.**
    - `TheTopRowAndThePanelShareHoldWithTheBestBetPinnedBothWays` is red on the tree. That is its
      watched red, and it already carries the failure line.
    - **Green with its body unedited** is the proof that the fix works.
    - Also assert, in the same fact or one beside it, the case unit 350 printed and did not assert:
      **the best bet drawn on another band** (40 m), with the same limits. Its watched red is by
      ruling 19.
    - **The pin can be overwritten by a spot reload** (unit 350 item 2). If the fact fails with the
      pin message rather than a limit, run the filter once more and report both runs. A pin-message
      red is neither a layout miss nor a layout pass. Do not add a clock seam or change the reload.
57. **The sheet, `docs\unit349-what-tim-looks-at.md`, brought to the new screen - and to nothing
    else:**
    - section 2.2's table: the *Top row*, *Green block* and panels rows (`:93`-`96`), re-measured
      with the best bet on his band, on another band and absent;
    - section 2.2's bullet *When the best bet draws…* (`:106`-`107`) and its source note (`:116`),
      replaced with this unit's numbers and commit;
    - section 2.1's FT8 table: only if the change moved an FT8 number, re-measured;
    - section 3.3: one new row **U11**, this unit's arrangement, in the table's own columns, with its
      overrule words;
    - section 4 items 1 and 2 (`:434`-`438`): item 1 replaced by the result, or removed and the list
      renumbered if the case now holds; item 2's 19 px replaced by the new spread;
    - section 3.3's U2 overrule number (`:414`): only if task 3 runs.
    - **No other line changes.** A measured number that contradicts another line is reported in
      section 4 and not edited.
58. **Section 4 raises no new ask of its own unless the measurements put one there.** Unit 349 item 1
    is carried as Tim's open ask. Everything else this unit finds is a finding, and says so.

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
- **R14**: *a test exists to prove an exit criterion.* This unit adds only task 0's trace and ruling
  56's assertion.
- **R19**: American spelling.
- **HM-DEC-139**: open asks are carried verbatim until answered.

---

## 4. Status cadence

Write status **before every `dotnet` command, after every commit and after every task**, with
`sh tools/status.sh`. It ran for every write in unit 350. If it is refused, take a `date` reading and
paste it whole. **Never compose a time.**

The script hard-codes `RULES_AT: HM-DEC-161 (2026-09-11)`. **After every write**, set that line back
to `HM-DEC-163 (2026-09-12)`, or to the highest id §2 finds, with the file editor.

The watchdog kills a session only when its process tree has used no CPU for ten minutes. **Write
status between sections of the report**, so a quiet stretch of editing is never ten minutes of nothing.

---

## 5. The tasks

### Task 0 - the trace: what the check costs, and what each arrangement gives

Measure before anything is built. **Say what you find rather than confirming §1.**

1. **Commit `WORK_INSTRUCTIONS.md`, `PHASE_STATUS.md` and a patch bump (1.13.37 -> 1.13.38, with its
   comment block).**
   - In `PHASE_STATUS.md`, change only the `WORK_INSTRUCTION:` line, to `351 - step 0's last red: the
     check fitted at 1400`. Do not touch its `STEP:` lines, `CURRENT_STEP` or `HEARTBEAT`.
   - Do not commit `.run-unit\`, `PHASE_OUTCOME.md`, `RUN_LEDGER.md`, `SESSION.lock` or
     `tools\arbiter.bak-20260913\`.
   - The message is `chore(unit351): step 0's last red, the check fitted at 1400 - the trace first`.
   - Run the carry-forward list, status first, and give both invocations as *n of n*.
2. **Step 0's filter, once**, status first, with a stated timeout. Give each class as *n of n*, and
   the failure line of any red. Expect 23 of 24.
3. **Add `Unit351TraceTheCheckInTheGreenBlock` to `TheTopRowTests`.** It asserts nothing and presses
   nothing.
   - **Setup:** pin the best bet as `Unit350TraceStepZeroBothWays` does, at 1400 and 1920, on FT8 and
     PSK31, licensed, in three states: on his band (20 m, with *✓*), on 40 m, and absent.
   - **For each, print** (ruling 54):
     - top row px and share, and the three panels with the strip hidden;
     - `GreenZoneBlock`'s height;
     - `GreenZoneLeft`'s and `GreenZoneRight`'s widths;
     - each left line's height and its line count (height over the line's own line height);
     - the best bet row's height, the button's desired width and height, and the same word measured
       without *✓*;
     - whether the sparkline is showing;
     - whether the pin held.
   - **Then, on the 1400 PSK31 window with the best bet on his band, set each candidate arrangement on
     the test window only** and print the same numbers, plus the 1920 row with that arrangement.
     Consider at least:
     - the best bet row moved out of the right column;
     - the right column held to the width it has without the best bet, the best bet row wrapping;
     - the check's line held to the row's line height.

     Add any other arrangement the numbers suggest. **Every candidate keeps ruling 55's list.**
4. **Answer from the numbers, in section 1, before task 1 starts:**
   - What makes the block 10 px taller with *✓*, and 19 px taller than with no best bet, line by line?
   - Which arrangement brings the 1400 PSK31 row to at most 238.4 and the panels to at least 455 with
     the best bet on his band, and what does it do to the other cases at 1400 and 1920?
   - Which one will task 1 build, and why?
   - Did every pin hold?

**Drop candidate:** none. Task 1 is built on it.

### Task 1 - the arrangement built, and the pinned fact green unchanged (rulings 55 and 56)

1. **Build the chosen arrangement** in the markup and, if needed, `FitTheHeardCount`. Keep the
   existing remarks' voice: say what moved, why, and the numbers, as the block's other comments do.
2. **Run step 0's filter**, status first.
   `TheTopRowAndThePanelShareHoldWithTheBestBetPinnedBothWays` should be green with its body
   unedited, and every other step 0 test green.
3. **Add the 40 m case** to the pinned assertion (ruling 56), with its watched red by ruling 19 on the
   test window only.
4. **Run step 0's filter again**, status first, and the carry-forward list. Commit and push:
   `feat(app): task 1 - the check fitted at 1400 - <arrangement>; 1400 PSK31 with the best bet on his
   band <px> of 238.4, panels <px> of 455; <numbers for the other cases>`.

**If no arrangement fits**, follow ruling 55's last bullet and commit the result under `feat(app)` or
`test(app)`, whichever is true.

**Drop candidate:** none. It is the unit.

### Task 2 - the sheet brought to the new screen (ruling 57)

1. **Edit only the lines ruling 57 lists**, with numbers from task 1's runs and this unit's commit.
2. **U11's row** says, in the table's columns:
   - what moved in the green block;
   - where Tim sees it (green block, 1400);
   - what was rejected, from task 0's other arrangements;
   - the overrule words.
3. **Commit and push:** `docs(unit351): the sheet at the new screen - <numbers>`.

**Drop candidate:** none. Without it the sheet is no longer the screen (ruling 53).

### Task 3 - U2's overrule number re-measured (the drop candidate)

Section 3.3's U2 row says putting the upgrade row back takes the 1400 PSK31 top row to *240 px (0.264)
with the best bet drawn*. That is unit 341's number, from before this unit's arrangement, with the best
bet on another band.
1. **On the test window only**, show the upgrade row and its button again, and print the 1400 PSK31
   top row and panels with the best bet on his band, on 40 m and absent. **Nothing is pressed.**
2. **Replace the number in U2's overrule cell** with what was measured, citing the commit. Commit and
   push.

**Drop candidate: this task, whole.** If time runs short, skip it. U2's cell keeps unit 341's number,
and the report says so.

---

## 6. Parked - do not touch, do not raise

- **Step 3 and the verdict.** Tim's (ruling 46).
- **Every word on the screen**, and everything on the screen outside `GreenZoneBlock` and
  `FitTheHeardCount` (ruling 55).
- **The best bet ranking, the spot reload and the clock.** Nothing in `RankBands`, `ApplyBestBet`,
  `ReloadSpotsAsync` or any clock source changes. A pin-message red is reported (ruling 56).
- **The strayed-frequency line on the licensed fixture** (unit 341 item 2). The fixture is not
  changed to make the case shorter.
- **Every red in the record**, and every test except those task 0, ruling 56 and task 3 name.
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
- **Do not change a word, hide anything, or touch the best bet's command** (ruling 55). **Do not
  loosen 0.262, one half, the 10% at 1920 or the 2 px rig tolerance, and do not edit the pinned
  fact's body to turn it green** (§6, ruling 56).
- **Do not press, click or invoke the best bet, a band button, CQ, Stop or any send** (§0.2).
- **Do not break the view to watch a test fail** (ruling 19).
- **Do not edit the sheet beyond ruling 57's lines.**
- **Do not delete a file.** Empty it, comment it, list it (§6).
- **Do not edit `PHASE_OUTCOME.md`, `RUN_LEDGER.md`, the `STEP:` lines, `CURRENT_STEP`,
  `.run-unit\allowed.txt` or anything under `tools\arbiter\`.** They are the launcher's.
- **Never `git add -A` or `git add .`** (§2).
- `CLAUDE.md` §12.6 covers the rest. **No package. Report mismatches. Write American.**
- **The tool facts, as unit 350 found them.** Say which held for you:
  - **ran:**
    - `sh tools/status.sh` joined by `&&` to `date` and `timeout … dotnet test`, with `| tail` or
      `| grep -e` after it;
    - `git add && git commit -m -m && git push && date`;
    - `grep -n -e`, `grep -o -e … | cut`, `git diff | grep -c`;
    - `ls`, `cat`, `sed -n`, `git rev-parse HEAD origin/main`;
  - **asked for approval, not run:**
    - `cd` before `git`;
    - `pwd -W && ls … 2>&1 | cat`;
    - a `$TEMP` expansion;
    - a `>` redirect joined with `;`;
    - `git check-ignore -v`;
    - `grep -E` with a brace quantifier;
    - `sed -e` substitutions in pipes;
  - **refused:** none for unit 350. Unit 348 found a `for` loop over a shell variable refused, and
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

A. The phase goal - the screen, done right. At <hash>: step 0 partial on
   unit 350 (criterion 5 red at 1400 PSK31 with the best bet on his band)
   - this report is its evidence. Step 1 done (unit 347). Step 2 done
   (unit 348). Step 3 0 of 1, waiting on Tim's verdict on
   docs/unit349-what-tim-looks-at.md, which this unit updated to the new
   screen and did not otherwise move.
B. Step 0 and its exit criteria, at this tree, best bet pinned three ways:
   entry: the tree is Hamlet's; PHASE_STATUS.md names this phase - <met?>
   1. 1920 top row about 190, panels take the rest - <met/not>: <px, share
      in every pinned case>
   2. card carries strip, green block, clock; band largest; one dot -
      <met/not>: <test, numbers after the arrangement>
   3. rig panel the card's height, drive and offer under the S-meter -
      <met/not>: <px at both widths, both modes>
   4. three panels equal, full to the status bar; facts beside at 1920 -
      <met/not>: <test; this unit ran it after the change>
   5. 1400 same shape, no callsign clipped, facts rule - <met/not>: PSK31
      top row <px> of 238.4 with the best bet on his band (was 247), <px>
      on 40 m, <px> absent; worst panel share <n> (was 446 of 455); FT8
      <px>; pin held <yes/no>
   6. BindingHealthTests, VoiceTests, carry-forward - <n of n each>
   nice: best bet joined to the green block - <1920 and 1400 | which>
C. The report last. Section 4 raises N items on top of the carried queue.
   Say whether any stands in the way of a criterion in B, and, if 5 is
   still red, by how much and what was tried.
```

**Every line specific to this unit.** If something was not measured, say *not measured*. Do not fill
the shape.

```
UNIT:       351 - <complete|stopped> at task N of 3 - <date time, read from the clock>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no> - step 0 criterion 5 <held|closer|unchanged>; <what moved on the screen>
NUMBER:     1400 PSK31 top row, best bet on his band: 247 px -> <px> (limit 238.4); panels 446 -> <px> (floor 455)
DRIFT:      0
```

**Section 2 says, first**, what moved on the screen, in words Tim can find at his window, and that the
sheet was updated to it (with the lines). Then it says whether any word changed or anything hides. The
answer ruling 55 requires is *no*; if it is not, say so plainly.

**Section 3 leads with the answer:** does criterion 5 hold now at every pinned case? Then the table:
case (width, mode, best bet state), top row, panels, green block, met. Then task 0's arrangements
with their numbers, and which was built. **Every appearance claim is computed, not seen. Say so once.**

**Section 4:** unit 350's section 4 verbatim per HM-DEC-139, from its line under
`## 4. What's blocking us` to its end, as committed in `c88d3974`, including the queues it carries.
Keep it in place with the file editor. In the carried text, mark:
- unit 349 item 1 (Tim's step 3 verdict): `STILL OPEN - Tim's; work instruction 351 authors nothing
  into step 3, and the sheet was updated to the new screen at <hash>`;
- unit 350 item 1 (criterion 5 red): `TAKEN UP by work instruction 351 rulings 53 to 56`, with the
  measured result;
- unit 350 item 3 (three sheet lines with the older number): `TAKEN UP by work instruction 351 ruling
  57`, naming which lines changed;
- unit 350 item 2 (the pin and the spot reload): `STANDS`, with this unit's pin readings.

Then this unit's items, under *Raised by unit 351*. **Each says whether it is a finding or an ask**
(ruling 58).

---

```
ARBITER-DECISION
STEP: 0
APPROACH: fit the 1400 green block by arrangement so the best bet with its check on the operator's own band no longer widens the Auto right column or grows the block - every word, the check, the best bet's command and the ranking unchanged, nothing hidden - traced line by line and chosen from arrangements measured on the test window, proven by the unedited pinned fact going green plus the 40 m case, the sheet brought to the new screen; U2's overrule number re-measured as the drop candidate
MOVE: work around
WHY: Step 0 is partial on one case - 1400 PSK31 with the best bet on his band, 247 of 238.4 px and panels 446 of 455 - and unit 350 held the screen still and left a fix or Tim's verdict to this arbiter; a fix is the only work open in the phase and Tim should not spend his review on a 9 px miss. Unit 338 shortened a string and unit 341 hid an empty row; this changes no word and hides nothing, and the loop test found nothing like it.
STATE: partial
DECIDED: ruling 53 - the screen moves for this one case and the sheet with it, overruling ruling 47 for this unit, ruling 46 upheld; ruling 54 - the trace names what the check costs line by line and measures candidate arrangements on the test window before one is built, the choice the unit's; ruling 55 - arrangement only: no word, check, command, tooltip, ranking or clock change, nothing hidden, the band the largest text and 1920 at 190, and a miss beyond arrangement ships partial without shortening; ruling 56 - TheTopRowAndThePanelShareHoldWithTheBestBetPinnedBothWays green with its body unedited is the proof, the 40 m case asserted beside it, a pin-message red run once more and reported; ruling 57 - the sheet's section 2.2 rows, bullet and note, a new U11 row and section 4 items 1 and 2 brought to the new screen, nothing else; ruling 58 - no new ask unless the numbers raise one, unit 349 item 1 carried as Tim's - all the author's, marked for Tim at step 3, overrulable
LICENCE: PHASE_PLAN.md R26 (at no window size less than half; at 1400 the unit measures and chooses), sections 1, 4 (step 0 exit criteria 1 and 5), 6 (the arbiter decides and continues; a small miss ships partial; shorten or widen, never clip; never loosen a test; transmit or a package stops) and 7; ARBITER.md sections 2, 3, 4, 6 and 8; .run-unit/state-verdict.json (step 0 partial on unit 350, criterion 5 at 1400 PSK31 with the best bet on his band); .run-unit/s4-verdict.json (none); PHASE_OUTCOME.md unit 350 (UNIT 6 - STEP 0) STATE_WHY, unit 338 and unit 341 approaches; unit 350 output.md at c88d3974 sections 1 and 3 and section 4 items 1 to 3 (b49eb3ab, 437cedd8); unit 341 output.md at 0f383a3 item 1 (the right column 140 to 180 px); src/Hamlet.App/Views/MainWindow.axaml 604, 639-640, 658-717, 720-792, 727-746, 737; src/Hamlet.App/Views/MainWindow.axaml.cs 98-147; src/Hamlet.App/ViewModels/GreenZone.cs 51, 296-301; tests/Hamlet.App.Tests/Views/TheTopRowTests.cs 577, 688, 757, 1195; docs/unit349-what-tim-looks-at.md 93-96, 106-107, 116, 414, 434-438; Directory.Build.props 809; DECISIONS.md 7 (HM-DEC-163); .run-unit/allowed.txt 14; CLAUDE.md 0.0, 0.2, 0.5, 0.6; psk31 R12, R13, R14, R19; HM-DEC-139, HM-DEC-155, HM-DEC-163
ACCOMPLISHED: on PSK31 at Tim's narrower width, when the best bet is the band he is already on, the short top row stays the mockup's share and the waterfall, decoded text and For You keep at least half the window, with every word and the check still on the screen - so the last measured miss in step 0 is closed or narrowed with numbers, and the sheet Tim is reading describes the screen as it now is
ADVANCES: step 0 - criterion 5 (the 1400 top row share at its worst case, the best bet drawn on the operator's own band) and R26's half-the-height floor under criterion 1, the only red standing between step 0 and done
END-ARBITER-DECISION
```
