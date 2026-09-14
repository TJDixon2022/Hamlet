# Work instruction 354 - the window sizes Tim can actually open, measured before he looks

Step 3 of `PHASE_PLAN.md`. **Nothing on the screen moves.** The unit measures the main window, and then
the achievements window, at the sizes the product itself opens at and allows. Those are the product's
opening size, its minimum and a small set of common screens. The sheet then tells Tim what those sizes
draw.

Where each step stands:

- **Step 0 is `done`** on the state reader's verdict on unit 351.
- **Steps 1 and 2 are `done`** on the state reader's verdicts on units 347 and 348.
- **Step 3 is `blocked`** on the state reader's verdict on unit 353 (`.run-unit\state-verdict.json`):
  *The only exit criterion is Tim saying the pages passed at his own window size, the report shows he has
  not given that verdict, and no unit can meet it for him since no session can see the screen, so it waits
  on Tim and more work will not help.*
  - No commit up to `0d69123a` carries Tim's verdict.
  - `DECISIONS.md` still tops at HM-DEC-163 (`:7`).

**Why a unit at all, when the phase waits on Tim.** Unit 353 ended clean. Its item 5 and ruling 71 say
nothing is left that ruling 46's exception licenses, and this arbiter agrees: **no more work on the test
window's spot reloads.** But step 3's criterion is Tim's verdict *at his window size*, and the record
has measured two sizes that he may never open:
1. **Everything was measured at 1400 and 1920 px wide, with the main window 1040 px tall**
   (`TheTopRowTests.cs:48`, `:2227`; `TheWorkingPanelsTests.cs:510`, `:778`). The sheet says so
   honestly (`docs\unit349-what-tim-looks-at.md:11`-`12`) and asks Tim for his size.
2. **The product opens the main window at 1100 x 780 and lets it shrink to 900 x 620**
   (`src\Hamlet.App\Views\MainWindow.axaml:12`-`13`). After the first run it reopens at the saved size
   (`src\Hamlet.App\App.axaml.cs:93`-`96`, saved at `:137`-`138`). **The size Tim sees on a fresh launch,
   and the smallest he can drag to, have never been measured.**
3. **R26 says *at no window size do the working panels get less than half the height below the band
   pills*.** Step 0's criteria check that at two sizes.
4. **The achievements window opens at 1040 x 720** (`AchievementsWindow.axaml:9`). Its pages were
   measured by setting the width to 1400 and 1920 with the height left as declared
   (`TheAchievementsPageClicksInTests.cs:630`, `TheCategoryPagesAreTradingCardsTests.cs:533`).

So Tim may give the phase's last verdict on a window nothing measured. A miss found there costs him a
review. This unit finds out, with numbers, and changes nothing. **What to do about a miss belongs to the
next arbiter, not to this unit.**

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

These four are copied from work instruction 353. Unit 353 checked them and they held. Check them again.

**Also:** `.run-unit\allowed.txt` must permit `dotnet`. Unit 353 found `Bash(dotnet:*)` at line 14.
- If `dotnet test` is refused, stop at task 0, write the report, and say so in section 4, quoting the
  refused command.
- **Do not route around it:** no `tools/tests/run.js`, no assertions written but never run, and no edit
  to `allowed.txt` or `run-unit-tools.txt`. Unit 343 item 1 rejected all three, and those rejections
  stand.

---

## 1. Why this unit exists

**The numbers today** (unit 353, `d66a6ace` to `0d69123a`; none re-run by this arbiter):
- **Step 0's filter:** 28 of 28 in each of three runs, and once more after task 3:
  - `TheTopRowTests` 14 of 14, `TheWorkingPanelsTests` 8 of 8, `BindingHealthTests` 1 of 1, `VoiceTests`
    5 of 5;
  - pins 8 of 8 and the 40 m fact 4 of 4 in each run (59 pin lines).
- **The pinned numbers, all at 1040 tall:**
  - 1920: 190 px, panels 503;
  - 1400 FT8: 216 px, panels 477;
  - 1400 PSK31: 228 px, panels 465.
  - The limits: at 1920 within 10% of 190; at 1400 `<= 0.262 * below`; panels `>= below / 2`; rig within
    2 px of the card.
- **The licensed test window draws its fixture's window whatever the hour:** 6 stations, its sparkline
  and no best bet unless a fact pins one. The guard held on every run (`2077432a`).
- **`Unit332TwoWidthsTests` 3 of 3 and `TheGreenZoneTests` 15 of 15.**
- **Carry-forward:** app 111 of 111, except one run at 02:16:36 that read 110 of 111 with the red unnamed
  (unit 353 item 2); engine 86 of 86.
- **Sizes measured: 2 of the 4 the product defines or starts from.** 1400 x 1040 and 1920 x 1040 have
  been measured. The opening 1100 x 780 and the minimum 900 x 620 have not.
- **Step 3:** 0 of 1. It stays 0 of 1 until Tim answers.

```
PHASE GOAL: The screen, done right - the main window as the approved mockup
            (step 0, done on unit 351); every achievements category page as
            trading cards (step 1, done on unit 347); what the last phase left
            (step 2, done on unit 348); then Tim at his window says it passed
            (step 3, 0 of 1, blocked on Tim's verdict).
UNIT GOAL:  Measure what the main window and the achievements window draw at
            the sizes Tim can actually open - the product's opening size, its
            minimum and common screens - against R26's outcomes and step 1's
            no-clip and no-white-card clauses, with no screen change, and put
            the numbers and any miss on the sheet Tim reads.
ADVANCES:   none - no criterion moves. This unit clears what stands between
            Tim and a verdict at his own window size: the record measures only
            1400 and 1920 at 1040 tall, while the product opens at 1100 x 780
            and allows 900 x 620, and R26 promises the panels' half at every
            size.
DRIFT:      0
```

---

## 2. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any mismatch.
**Report the mismatch; do not repair the instruction.** Mismatches go in the report even when the work
succeeds anyway.

Check:
- **Every file, line and item the header and §1 cite.**
- **`HEAD` and `origin/main` read `0d69123a`** for this arbiter, and `output.md` in the tree is unit
  353's.
- **The version is 1.13.40** (`Directory.Build.props:834` for this arbiter).
- **`DECISIONS.md` tops at HM-DEC-163.** If a higher id exists, or any commit after `0d69123a` carries a
  verdict from Tim on step 3 or his window size, **stop at task 0.** Quote it in section 4 and write
  nothing else: a verdict or a size changes what the next unit is.
- **The launcher's files, one line each; edit none:**
  - `PHASE_OUTCOME.md` and `PHASE_STATUS.md` read steps 0, 1 and 2 `done` and step 3 `blocked`.
  - `PHASE_STATUS.md` reads `WORK_INSTRUCTION: 353`.
  - `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md` and files under `.run-unit\` are modified and
    uncommitted by the launcher.
- **The reload says `CLAUDE.md` §1 holds `CPS-DEC-0163`** (`.run-unit\reload.txt:9`, `:34`). Unit 353
  found no `CPS-DEC` in `CLAUDE.md`. It is parked with the id schemes. One line.
- **`tools\arbiter.bak-20260913\` is untracked at the root**, and `SESSION.lock` may be. **Never
  `git add -A` or `git add .`**; stage files by name.
- **The sizes, as this arbiter read them:**
  - `MainWindow.axaml:12`-`13`: `Width="1100" Height="780"`, `MinWidth="900" MinHeight="620"`;
  - `App.axaml.cs:93`-`96`: a saved size is applied when it is over 400 x 300;
  - `AchievementsWindow.axaml:9`: `Width="1040" Height="720"`, and no minimum was found.
  - Say whether anything else sizes, maximizes or clamps either window: `WindowState`, a screen-bounds
    clamp, or a `SizeToContent`.
- **The test windows:**
  - `TheTopRowTests.WindowHeight = 1040` at `:48`;
  - `Realized` builds `new MainWindow { … Width = width, Height = WindowHeight }` at `:2227`;
  - `TheWorkingPanelsTests` does the same at `:510` and `:778`;
  - the achievements tests set the width only (`TheAchievementsPageClicksInTests.cs:630`,
    `TheCategoryPagesAreTradingCardsTests.cs:533`).
- **The tool facts in §7** are unit 353's. Say which held for you.

**Reds expected, older than this unit. This unit runs none of them:**
- `TheMenuIsUnderTheMouseTests` (8);
- `TheAchievementsScreenTests.TheWindowDrawsEverySixRows` (1);
- `TheOperatorCanStopItTests` (2) and `TheWholeChainRunsFromOneRightClickTests` (2), **never run.**

**Expected at the start:**
- **the carry-forward list:** 111 of 111 app and 86 of 86 engine;
- **step 0's filter: 28 of 28**, with pins 8 of 8 and the 40 m fact 4 of 4.

**Expected at the end:**
- **step 0's filter 29 of 29**, counting task 1's trace fact, with every pinned number unmoved;
- **the carry-forward list unchanged**;
- **if a red turns green, a green turns red, or a count moves, say which.**

---

## 3. Rulings in force - do not re-argue

**HM-DEC-155**, transcribed: *A unit runs no test suite. It may run only the test it constructs in that
work instruction, filtered by exact name, in the foreground, with a stated timeout, and it never
backgrounds a command and polls for it.*
- Run the carry-forward list as the top comment of `docs\carry-forward-tests.txt` says: two
  invocations, one build each, with status written immediately before each.
- **Step 0's four classes may run together, filtered by class name**, in one filter:
  `TheTopRowTests`, `TheWorkingPanelsTests`, `BindingHealthTests` and `VoiceTests`.
- **Each trace fact this unit adds is run filtered by its exact name.**

**Step 3, `PHASE_PLAN.md` §4, in full:**

> **Delivers:** Tim opens every page at his window size and says it passed.
> **Entry:** step 2 done.
> **Exit:**
> - Tim says it passed. *must-pass* No script can evaluate this.

**R26, Tim, 2026-09-12 (`PHASE_PLAN.md` §R), in full - every outcome this unit measures:**

> - **The top row is one band, about 190 px tall at 1920**, and the working panels below the tabs take
>   the rest of the window. At no window size do the working panels get less than half the height below
>   the band pills.
> - **The neighborhood card carries three things**: the band strip with the legend and the *you · mode*
>   marker; **the green block** - the band in the largest text, the frequency, mode and *yours to use*,
>   the license line small, the rule-of-thumb line small, and heard-just-now with its count and
>   sparkline; and **the world's clock** at its right end, about 246 px wide, with the operator's dot
>   only.
> - **The rig display is the same height as the neighborhood card**, and carries under the frequency and
>   S-meter the transmit drive and the RF power offer, so no empty column stands under it.
> - **The band pills stay where they are** and are not repeated anywhere.
> - **Below the tabs**: waterfall, decoded text, For You, all the same height, full to the status bar. The
>   decoded list is as wide as its longest line needs and no wider; For You takes the rest, wide enough
>   that the card's facts sit beside its map.
> - **At 1400**: the same shape; where the card's facts cannot sit beside the map they go under it; no
>   callsign is ever clipped in the decoded list. **No mechanism is prescribed; the unit measures and
>   chooses, marks the choice as its own, and reports the numbers at both widths.**

**Step 1's exit, the two clauses task 3 reads:** *No string clips or wraps a word, measured at 1400 and
1920. must-pass* and *No card is a white rectangle. must-pass*

**`PHASE_PLAN.md` §1, §6 and §7, the lines that bite here:**
- *Screen only. Nothing here touches the radio, a decoder, a parser, the transmit chain or the log's
  content. A step's exit is what is on the screen, asserted by computation and described in words, then
  Tim's eyes. Every appearance claim is computed, not seen, and says so.*
- *A must-pass is missed by a little: ship, report, `partial`, move on. Never loosen a test.*
- *A string will not fit: shorten and say which, or widen; never clip.*
- *Anything touches the radio, a decoder, a parser or the transmit chain: `MOVE: stop`.*
- *A package is needed: `MOVE: stop`.*
- *Carried: every open ask from unit 336's queue, verbatim in every unit. Plus: real flags on earned
  country cards - undecided, Tim's; the PSK31 phase's step 6 - Tim's, at the radio; the recording of real
  PSK31 audio; the id-scheme split; the map bitmap's license.*

**The arbiter's rulings 1 to 71 stand as units 337 to 353 built them.** Rulings 1 to 58 are gathered on
the sheet, marked for Tim at step 3, and overrulable. **This unit re-opens, re-words or re-builds none of
them.** These bite here:
- **1.** *The working panels* means the three panels themselves, at least half the height below the
  band pills at 1920 and 1400, with the readiness strip hidden.
- **19. Watching red.** Show a new assertion fail once, against a deliberately wrong expectation set on
  the test window only, and give the failure line. **Never break markup or a view to watch a test fail.**
  This unit adds no assertion, so nothing needs to be watched red.
- **42.** No pictures: no package, no running app, no drawn mockup.
- **43.** The sheet asks Tim for his window size, because the record has none.
- **46.** No further unit is authored into step 3 until Tim answers, unless the sheet is shown wrong.
  **Amended by ruling 72 below.**
- **47**: **no `src` or markup change**, so the sheet Tim holds stays the screen.
- **49 and 56**: `TheTopRowAndThePanelShareHoldWithTheBestBetPinnedBothWays` and the 40 m fact. **Their
  limits are not loosened, and their bodies are not edited.**
- **61 and 67**: the network sources off in the fixtures, and the declared window restored and guarded
  after `Realized`'s settle. **Both stand, and every window this unit builds goes through them.**
- **63**: the sheet changes only where measured wrong. **Amended by ruling 76 below.**
- **U11**, unit 351's arrangement (sheet `:426`), stands as built.

**The arbiter's rulings for this unit.** They are the author's, marked for Tim at step 3, and
overrulable:

72. **Ruling 46 is amended.** A unit may also be authored into step 3 where Tim's verdict would be given
    on a window size nothing measured. The product's opening size and its minimum are such sizes.
    - Ruling 71's reading is upheld: no further unit on the test window's reloads or network sources.
    - *Why:* step 3's criterion is Tim's verdict *at his window size*. The record covers two sizes that
      the product neither opens at nor defaults to. A miss at 1100 x 780 would cost Tim a review, and
      finding it costs one unit.
    - *Overrule with:* "step 3 waits on my verdict; author nothing into it."
73. **The sizes, fixed by this instruction and not chosen by the unit.** For the main window:

    | Size | Why |
    |---|---|
    | 900 x 620 | the product's minimum (`MainWindow.axaml:13`) |
    | 1100 x 780 | the product's opening size (`:12`) |
    | 1280 x 720 | a small laptop screen |
    | 1366 x 728 | a common laptop screen, maximized under a 40 px taskbar |
    | 1536 x 824 | a 1080p screen at 125% scaling, maximized |
    | 1400 x 1040 | the anchor: must reproduce the pinned 216/477 (FT8) and 228/465 (PSK31) |
    | 1920 x 1040 | the anchor: must reproduce the pinned 190/503 |
    | 1920 x 1017 | a 1080p screen at 100%, maximized |
    | 2560 x 1400 | a 1440p screen, maximized |

    For the achievements window (task 3): 900 x 620, its own opening size 1040 x 720 (`AchievementsWindow.axaml:9`),
    1280 x 720, and 1400 x 720 and 1920 x 720 as the anchors.
    - If a size cannot be realized on the headless host, say so and say what it realized instead.
    - Tim's verdict will name his size. **If he names one not listed, the next unit measures it; this unit
      does not guess it.**
74. **Measured through the existing fixtures, with a height added and nothing else changed.**
    - The unit may add an `internal` overload of `TheTopRowTests.Realized` and
      `TheWorkingPanelsTests.Realized` that takes a height. The existing signatures delegate to it with
      `WindowHeight`, as unit 353's hook overload did.
    - The overload keeps ruling 61's sources and ruling 67's restore and guard.
    - **The two anchors must print the pinned numbers exactly.** If they do not, the overload is wrong:
      report that, and ship nothing that depends on it.
    - No pinned fact's body is edited. `WindowHeight` stays 1040.
75. **What is read at each main window size.** Read it on the licensed fixture, on FT8 and PSK31, with
    the best bet pinned absent and pinned drawn on his own band (the worst case unit 350 measured). Print
    one line per window with:
    - the height below the band pills, the top row and its share, and the rig panel against the card;
    - the three panels' heights, their share of the height below the pills, and whether they are equal
      and reach the status bar;
    - whether the green block's band is its largest text, the clock's width and dot count, and the
      heard count;
    - the decoded list's width against its longest line, and whether any callsign is clipped;
    - For You's card: facts beside or under the map, by the rule unit 337 stated;
    - **every drawn `TextBlock` in the top row and the three panels whose text is trimmed, clipped by its
      parent, or wraps a word.** Name each one;
    - **any control drawn outside the window or at zero size that is drawn at 1920.**
    
    Also read the plain (no-license) fixture at 900 x 620 and 1100 x 780 only.
    **The trace asserts nothing and presses nothing.** Against each R26 outcome it says *holds*, *misses
    by n px* or *not measurable here, because…*. Words are computed, not seen.
76. **The sheet gains the other sizes, and nothing else changes.** Ruling 63 is amended for this unit
    only.
    - **`:11`-`12`**: the sentence is brought to what was measured, still asking Tim for his size.
    - **One new table**, placed after the main window's 1400/1920 table in section 2: the sizes as rows,
      and top row, panels (share), the three panels equal, any clipped callsign and any trimmed text as
      columns. It is cited to this unit's commit.
    - **For each miss, one item in the sheet's section 4** (known imperfections), with the size, the R26
      outcome, the numbers and the elements named. It carries no fix proposal and no word that softens it.
    - If task 3 runs: the same table for the achievements window in section 3, and its misses in section 4.
    - **No other line.** Not the verdict form, no U row, and no ruling line.
77. **A miss is reported, never fixed, in this unit.** Ruling 47 holds, because Tim may be reviewing the
    screen now.
    - No markup, view, view model, minimum size or opening size changes.
    - No test is written to go red on a miss. A trace prints; R14 adds a test only for an exit criterion,
      and step 0's criteria name 1400 and 1920.
    - **The next arbiter weighs a fix against Tim's verdict with these numbers.** Section 4 names each
      miss as a finding, and asks only if a miss touches keying, transmit or the radio's safety. For
      example, Stop drawn outside the window or at zero size at the minimum is such a miss: `CLAUDE.md`
      §0.2.
78. **The carry-forward list names its own reds.** The app invocation carries
    `--logger "trx;LogFileName=u354-carry-app.trx"`, and the engine invocation carries its own trx logger
    in the same way.
    - One invocation each, as the file says, with no re-run to name a red. This overrules unit 353
      decision 3 for this unit.
    - If a red shows, name it from the trx, and say whether any class on the list builds a window this
      unit's overload touches.
    - *Logged, not chased:* unit 353 item 3, where `Unit332TwoWidthsTests` builds its own window with the
      sources at their defaults. **A third unit on test-window network isolation would be a loop with
      units 352 and 353.** It is reported only.

**Standing, transcribed:**
- **§0.0**: *Never present a guess as a decode… This binds pictures as hard as sentences.* Every
  appearance claim is computed, not seen, and says so once.
- **§0.2**, *transmit safety, ABSOLUTE*: *One operator action, one transmission.* Nothing here transmits
  or tunes. No test this unit writes presses CQ, Stop, Capture, a band button, the best bet, a send or a
  transmit command.
- **§0.5**: *every panel is collapsible, and a collapsed panel still carries its summary. Collapsing hides
  detail, never information.* At a small size, a panel that loses its summary is a miss to name.
- **§0.6**: every ink clears 4.5:1 against its fill, and color is never the only carrier.
- **R12**: *a session fixes its own tests and never asks the owner to approve it.*
- **R13**: telemetry on every new stage. This unit adds no stage.
- **R14**: *a test exists to prove an exit criterion.* This unit adds only trace facts that assert
  nothing, and one fixture overload.
- **R19**: American spelling.
- **HM-DEC-139**: open asks are carried verbatim until answered.

---

## 4. Status cadence

Write status **before every `dotnet` command, after every commit and after every task**, with
`sh tools/status.sh`. It ran for every write in unit 353. If it is refused, take a `date` reading and
paste it whole. **Never compose a time.**

The script hard-codes `RULES_AT: HM-DEC-161 (2026-09-11)`. **After every write**, set that line back to
`HM-DEC-163 (2026-09-12)`, or to the highest id §2 finds, with the file editor. Never batch two status
writes.

The watchdog kills a session only when its process tree has used no CPU for ten minutes. **Write status
between sections of the report and between sheet edits**, so a quiet stretch of editing is never ten
minutes of nothing.

---

## 5. The tasks

### Task 0 - the gate, the baseline, and whether the sizes can be realized

Measure before anything changes. **Say what you find rather than confirming the header or §1.**

1. **Commit `WORK_INSTRUCTIONS.md`, `PHASE_STATUS.md` and a patch bump (1.13.40 -> 1.13.41, with its
   comment block).**
   - In `PHASE_STATUS.md`, change only the `WORK_INSTRUCTION:` line, to `354 - the window sizes Tim can
     actually open, measured before he looks`. Do not touch its `STEP:` lines, `CURRENT_STEP` or
     `HEARTBEAT`. If the launcher's uncommitted changes to that file are in the way, commit the file
     whole and say so, as unit 353 did.
   - Do not commit `.run-unit\`, `PHASE_OUTCOME.md`, `RUN_LEDGER.md`, `SESSION.lock` or
     `tools\arbiter.bak-20260913\`.
   - The message is `chore(unit354): the window sizes Tim can open - the gate and the baseline`.
   - Run the carry-forward list with ruling 78's loggers, status first, and give both invocations as
     *n of n*. Name any red.
2. **Step 0's filter, once**, status first, with a stated timeout. Give each class as *n of n* and the
   pinned numbers as printed.
3. **Read, and change nothing:**
   - what sizes, clamps or maximizes each window (§2);
   - whether a headless `MainWindow` takes `Height` below 1040 and down to `MinHeight`, and what
     `Bounds` it reports at 900 x 620;
   - what the top row, the tabs and the status bar are made of, so task 1's "trimmed" and "clipped"
     name real elements.
4. **Answer in section 1, before task 1 starts:**
   - Can every size in ruling 73 be realized on the headless host? If not, which, and what did it
     realize?
   - Which elements will task 1 read for clipping and trimming, by name?

**Drop candidate:** none. Task 1 is built on it.

### Task 1 - the main window at every size (rulings 73 to 75)

1. **Add the height overload** to both `Realized`s (ruling 74). Run step 0's filter once, status first:
   it must read 28 of 28 with the pinned numbers unmoved. If not, stop the task and report.
2. **Add `Unit354TraceTheMainWindowAtTheSizesTimCanOpen` to `TheTopRowTests`.** It prints ruling 75's
   lines for every size in ruling 73 and asserts nothing. Run it by exact name, status first, with a
   stated timeout.
3. **Check the anchors.** 1400 x 1040 and 1920 x 1040 must print the pinned numbers. Quote both lines.
4. **Run step 0's filter once more** (29 of 29 expected), status first.
5. **Answer in section 1:** at which sizes does each R26 outcome hold, and by how much does each miss
   miss? Put the smallest size at which every outcome holds on its own line.
6. **Commit and push:** `test(app): task 1 - the main window traced at <n> sizes - <holds at which>;
   misses <size: outcome, numbers | none>; anchors reproduce 190/503, 216/477, 228/465; step 0's filter
   <n of n>`.

**Drop candidate:** the 2560 x 1400 row, if the time runs short.

### Task 2 - the sheet (ruling 76)

1. **Edit only the lines ruling 76 lists**, with numbers from task 1's run and this unit's commit.
2. **Commit and push:** `docs(unit354): the sheet at the sizes Tim can open - <table added at :n>;
   <n> misses in section 4 | no miss>`.

**Drop candidate:** none. Without it Tim reads a sheet that still covers only 1400 and 1920.

### Task 3 - the achievements window at its own sizes (the drop candidate)

1. **Add `Unit354TraceTheAchievementsWindowAtTheSizesTimCanOpen`** to the achievements tests. At each
   size in ruling 73's achievements list, it opens:
   - the opening page;
   - Countries and Modes;
   - one continent page.

   It prints every string that clips or wraps a word and every card that is a white rectangle, by the
   measure the existing no-clip and no-white-card fact uses. It asserts nothing and presses only what
   opens a page, as the existing facts do.
2. **Run it by exact name**, status first. Check that the 1400 and 1920 anchors print what the existing
   facts print.
3. **Add its table and misses to the sheet** (ruling 76), then commit and push under `test(app)` and
   `docs(unit354)`.

**Drop candidate: this task, whole.** If time runs short, skip it, and the report says so.

---

## 6. Parked - do not touch, do not raise

- **Step 3 and the verdict.** They are Tim's (ruling 46, as amended by ruling 72).
- **Everything under `src\`, every word on the screen, all markup, and both windows' opening and minimum
  sizes** (rulings 47 and 77). That includes U11, which stands as built.
- **Fixing a miss this unit finds.** It belongs to the next arbiter (ruling 77).
- **The test window's reloads, restore, guard and network sources**, including `Unit332TwoWidthsTests`'
  window and `BindingHealthTests` (rulings 61, 67, 70 and 78).
- **The best bet ranking, the spot reload, the sources, the heard count and the clock**, in `src\`.
- **The strayed-frequency line on the licensed fixture** (unit 341 item 2).
- **Every red in the record**, and every test except those tasks 0 to 3 name.
- **Steps 1 and 2.** Done. Their classes are not run in this unit; task 3 adds a trace only.
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
- **Change nothing under `src\`** (rulings 47 and 77). A miss that wants the screen changed is reported,
  not answered.
- **Do not change `WindowHeight`, the pinned facts' bodies, or 0.262, one half, the 10% at 1920 or the
  2 px rig tolerance.** Do not write an assertion that goes red on a miss (ruling 77).
- **Do not choose other sizes than ruling 73's, and do not guess Tim's.**
- **Do not press, click or invoke the best bet, a band button, CQ, Stop or any send** (§0.2).
- **Do not edit the sheet beyond ruling 76's lines.**
- **Do not delete a file.** Empty it, comment it, list it (§6).
- **Do not edit `PHASE_OUTCOME.md`, `RUN_LEDGER.md`, the `STEP:` lines, `CURRENT_STEP`,
  `.run-unit\allowed.txt` or anything under `tools\arbiter\`.** They are the launcher's.
- **Never `git add -A` or `git add .`** (§2).
- `CLAUDE.md` §12.6 covers the rest. **No package. Report mismatches. Write American.**
- **The tool facts, as unit 353 found them.** Say which held for you:
  - **ran:**
    - `sh tools/status.sh` joined by `&&` to `date` and `timeout … dotnet test … | grep`;
    - `git add && git commit -m -m && git push && git log | cut`;
    - `grep -n`, `grep -n -A`, `grep -o -e` and `grep -c` on trx files, joined by `&&` with `wc -l`;
    - `sh` on scripts written with the file editor into `testresults\`;
  - **asked for approval, not run:** a line with `pwd -W`; `sed -e` with a grouped expression in a pipe;
    `awk -F:` in a pipe; `tasklist`; a `grep -o` with `\{0,90\}` counts;
  - **refused:** a `grep` with `$(...)` (*Contains command_substitution*); for unit 352, a `for` loop over
    `$f` and `sed -n` on a trx file under `testresults\`;
  - **so:** read trx files with the file reader, and write the report with the file editor.

## 8. Committing and pushing

Commit and push each task on its own, on `main`, staging files by name. The report and the status file go
in their own commit. The report names every commit and whether each push succeeded. **A refused push is
reported as refused, with the reason.**

---

## 9. Reporting

Write `output.md` at the root, then stop. Do not start the next unit. **Every exit writes it**: finished,
blocked, failed or stopped early. **Write the report before task 3 if time is short.**

Canonical headings: `## 1. What Claude did`, `## 2. What the owner should expect`,
`## 3. What you should see`, `## 4. What's blocking us`. Validate it with
`dotnet build tools/arbiter/validate-output.proj -p:Report=output.md`, or with
`tools/arbiter/validate-output.bat output.md`. **Always name the report.** **The `UNIT:` line must fall
inside the first 60 lines.**

**The ordering block comes first.** `validate-output` refuses a report without it.

```
READ IN THIS ORDER.

A. The phase goal - the screen, done right. At <hash>: step 0 done (unit
   351), step 1 done (unit 347), step 2 done (unit 348). Step 3 0 of 1,
   blocked on Tim's verdict on docs/unit349-what-tim-looks-at.md, which
   this unit <did not change | changed at these lines>.
B. Step 3 and its exit criterion: Tim says it passed at his window size -
   not met, and no session can meet it. What this unit measured for it:
   1. sizes realized - <n of 9> main window, <n of 5 | not reached>
      achievements; anchors reproduce the pinned numbers <yes | no: which>
   2. the product's opening size, 1100 x 780 - top row <n> px (<share>),
      panels <n> (<share> of below), three equal <yes | no>; R26 <holds |
      misses: which, by how much>
   3. the product's minimum, 900 x 620 - the same numbers; clipped callsigns
      <none | which>; trimmed text <none | which>; anything off the window
      or at zero size <none | which>
   4. the smallest size where every R26 outcome holds - <size | none listed>
   5. the achievements window at 1040 x 720 and 900 x 620 - clipped or
      wrapped words <none | which>; white cards <none | which> | not reached
   6. carry-forward <n of n>, <n of n>; a red named <none | which>; step 0's
      filter <n of n> with the pinned numbers <unmoved | which moved>
C. The report last. Section 4 raises N items on top of the carried queue.
   Say whether any miss touches Stop, keying or transmit, and whether
   anything is left that a unit can do before Tim answers.
```

**Every line specific to this unit.** If something was not measured, say *not measured*. Do not fill the
shape.

```
UNIT:       354 - <complete|stopped> at task N of 3 - <date time, read from the clock>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   no - step 3 waits on Tim; R26 <holds at every size measured | misses at: sizes>; the sheet <gained the sizes at lines | unchanged: why>
NUMBER:     main window sizes measured <n of 9>, R26 holding at <n of 9>; achievements sizes <n of 5 | not reached>
DRIFT:      0
```

**Section 2 says, first**, that nothing on the screen moved, and which sheet lines changed. Then it says,
in words Tim can read, what the main window looks like when it first opens and at its smallest, and
which sizes, if any, fall short of the mockup's promise.

**Section 3 leads with the answer:** at the size Hamlet opens at, and at the smallest size it allows,
do the working panels keep half the height, with nothing clipped? Then:
- the table of every size: top row, panels and share, three equal, rig against card, facts beside or
  under, clipped callsigns, trimmed text;
- the anchors against the pinned numbers;
- each miss with the elements named.

**Every appearance claim is computed, not seen. Say so once.**

**Section 4:** unit 353's section 4 verbatim per HM-DEC-139, from its line under
`## 4. What's blocking us` to its end, as committed in `0d69123a`, including the queues it carries. Keep
it in place with the file editor. In the carried text, mark:
- unit 349 item 1 (Tim's step 3 verdict): `STILL OPEN - Tim's; work instruction 354 authors nothing into
  the verdict and measures the sizes he can open`;
- unit 353 item 2 (the unnamed carry-forward red): `TAKEN UP by work instruction 354 ruling 78`, with the
  result;
- unit 353 item 3's `Unit332TwoWidthsTests` bullet: `LOGGED, NOT CHASED - work instruction 354 ruling 78`;
- unit 353 item 5 (the recommendation): `UPHELD for the reloads by work instruction 354 ruling 72; ruling
  46 amended for window sizes`.

Then this unit's items, under *Raised by unit 354*. **Each says whether it is a finding or an ask.** A miss
is a finding. It is an ask only where ruling 77 says so.

---

```
ARBITER-DECISION
STEP: 3
APPROACH: measure the main window and the achievements window at the sizes Tim can actually open - the product's opening size 1100 x 780, its minimum 900 x 620 and common laptop and desktop screens - against R26's outcomes and step 1's no-clip and no-white-card clauses, through a height overload of the existing Realized fixtures with the 1400 and 1920 anchors reproducing the pinned numbers, no screen change and every miss put on Tim's sheet unfixed; the achievements window as the drop candidate; the carry-forward list run with trx loggers so a red names itself
MOVE: work around
WHY: Step 3 is blocked on Tim's verdict at his window size, and unit 353 left nothing on the test window's reloads (ruling 71, upheld). But every measurement in the phase is at 1400 or 1920 wide and 1040 tall, while Hamlet opens at 1100 x 780 and allows 900 x 620, so Tim may judge a window nothing measured. Measuring the sizes he can open is a different approach from re-checking the sheet against the test window, the loop test found nothing like it, and waiting on the owner's eyes is not one of the three stops.
STATE: blocked
DECIDED: ruling 72 - ruling 46 amended so a unit may be authored into step 3 where Tim's verdict would fall on an unmeasured window size, ruling 71's no-more-reload-work upheld; ruling 73 - the sizes fixed by the instruction (main window 900x620, 1100x780, 1280x720, 1366x728, 1536x824, 1400x1040 and 1920x1040 as anchors, 1920x1017, 2560x1400; achievements 900x620, 1040x720, 1280x720, 1400x720, 1920x720), Tim's own size measured only once he names it; ruling 74 - an internal height overload of both Realized fixtures keeping ruling 61's sources and ruling 67's guard, the anchors reproducing 190/503, 216/477 and 228/465 or nothing ships; ruling 75 - R26's outcomes read per size on FT8 and PSK31 with the best bet pinned absent and on his band, trimmed or clipped text and off-window controls named, asserting nothing; ruling 76 - ruling 63 amended for this unit: the sheet's :11-12, one sizes table and one section 4 item per miss, nothing else; ruling 77 - a miss is reported and never fixed here (ruling 47 holds), no red-going test, the next arbiter weighs a fix, and only a miss touching Stop, keying or transmit is an ask; ruling 78 - the carry-forward list run once with trx loggers so a red names itself (overrules unit 353 decision 3), and Unit332TwoWidthsTests' network reach logged, not chased, as a third reload unit would loop with 352 and 353 - all the author's, marked for Tim at step 3, overrulable
LICENCE: PHASE_PLAN.md R26 (at no window size less than half), sections 1 (computed, described, then Tim's eyes), 4 (step 3 exit at his window size; step 0 and step 1 exit clauses), 6 (the arbiter decides and continues; never loosen a test; never clip; transmit or a package stops) and 7; ARBITER.md sections 2, 3, 4, 6 and 8; .run-unit/state-verdict.json (step 3 blocked on unit 353); .run-unit/s4-verdict.json (none); PHASE_OUTCOME.md unit 353 (UNIT 9 - STEP 3) STATE_WHY; unit 353 output.md at 0d69123a section 1 and section 4 items 2, 3 and 5; rulings 43 and 46 (work instruction 349), 47 (350), 61 (352), 67, 70 and 71 (353); src/Hamlet.App/Views/MainWindow.axaml 12-13; src/Hamlet.App/App.axaml.cs 93-96, 137-138; src/Hamlet.App/Views/AchievementsWindow.axaml 9; tests/Hamlet.App.Tests/Views/TheTopRowTests.cs 48, 2227; tests/Hamlet.App.Tests/Views/TheWorkingPanelsTests.cs 510, 778; tests/Hamlet.App.Tests/Views/TheAchievementsPageClicksInTests.cs 630; tests/Hamlet.App.Tests/Views/TheCategoryPagesAreTradingCardsTests.cs 533; docs/unit349-what-tim-looks-at.md 11-18; docs/carry-forward-tests.txt; Directory.Build.props 834; DECISIONS.md 7 (HM-DEC-163); RUN_LEDGER.md (units 349 to 353, about $8 to $14 each); CLAUDE.md 0.0, 0.2, 0.5, 0.6; psk31 R12, R13, R14, R19; HM-DEC-139, HM-DEC-155, HM-DEC-163
ACCOMPLISHED: when Tim opens Hamlet to give the phase's last verdict - at the size it first opens, at the smallest he can drag it to, or on a common laptop or desktop screen - the sheet already says what the main window and the achievements pages draw there against the mockup's promises, with every shortfall named in pixels, so his verdict is not given on a window nobody measured, and nothing on the screen moved under his review
ADVANCES: none - this unit clears what stands between Tim and a verdict at his own window size: every measurement in the phase is at 1400 or 1920 wide and 1040 tall, while Hamlet opens at 1100 x 780 and allows 900 x 620, and R26 promises the panels' half at every size
END-ARBITER-DECISION
```
