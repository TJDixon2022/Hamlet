# Work instruction 348 - step 2, what the last phase left

Step 2 of `PHASE_PLAN.md`, **first unit on it.**

- **Step 0** is taken as done on the state reader's verdict on unit 341, as work instructions 342 to
  347 took it.
- **Step 1 is done.** The state reader read unit 347's report and returned `done`, in these words:

> *All seven must-pass criteria now have named green tests with quoted measurements at 1400 and 1920,
> criterion 3 is closed by the PSK31 row drawing EA3ABC 4,100 mi from a certain grid and the callsign
> alone otherwise, and the nice-to-pass popup test also passes.*

**So the plan moves to step 2, whose entry is *step 1 done*.** Step 2 is R27: five small things unit
336 left open, each with its own must-pass. **This unit takes all five.** It has five tasks, 0 to 4.
Task 4 is the drop candidate, and it carries no criterion.

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

**Also:** `.run-unit\allowed.txt` must permit `dotnet` (it carried `Bash(dotnet:*)` at line 14 for this
arbiter). If `dotnet test` is refused, stop at task 0, write the report, and say so in section 4,
quoting the refused command. **Do not route around it**: no `tools/tests/run.js`, no assertions written
unrun, and no edit to `allowed.txt` or `run-unit-tools.txt`. Unit 343 item 1 rejected all three, and
those rejections stand.

---

## 1. Why this unit exists

**The number:** step 2 must-pass met with a named test or a named list in the report is **0 of 5**. No
unit has run on step 2. This unit's target is **5 of 5**.

**Read from the tree by this arbiter, 2026-09-13, at `066ad04`. None of it has been run.**

- **Criterion 1, *the States badge and its cards say worked; no confirmed anywhere*.**
  - The badge's count is the shape every kind shares: `AchievementBadges.cs:353` builds `n worked`.
  - The States next card says `Any state you have not worked` (`AchievementCategory.cs:964`) and
    `Hamlet cannot tell a caller's state` (`NoStateFromTheAir`, `:929`).
  - R27 also asks that *the count says what it counts*. The count is states read from the log's
    `STATE` field, on United States, Alaska and Hawaii records only (R23). Unit 336 found that
    **Hamlet's own log entries carry no `STATE`**, so none of them counts. The count gives no sign of
    that.
  - **The word *confirmed* is still in achievements code.** `AchievementScreen.cs:543`-`546` holds
    a card detail: *The count says worked rather than confirmed: the DXCC award is counted from
    confirmations...*. `AchievementScreen` is still built at `AchievementsViewModel.cs:352` and
    `MainWindowViewModel.cs:13748`. Whether any window draws that detail today was not read.
  - `ThePsk31RecordsAppearTests.TheWordingSaysWorkedAndNeverConfirmed` (`:152`) asserts that no
    `confirm` is drawn. Its walk is the achievements window at its own size, on one log. It does not
    cover 1400 or 1920.
  - `StatesCountWhatTheLogsStateFieldSays` (`TheCategoryPagesAreTradingCardsTests.cs:1693`) asserts
    that the badge starts `2 `, and that the next card has unit 335's words.
- **Criterion 2, *the Modes test unit 336 named*.** It is
  `ThePsk31RecordsAppearTests.WithNoPsk31ContactNoPsk31CardIsOnTheScreenAndNoneIsDimmed` (`:86`).
  - It walks the page and every kind on a log with no PSK31 contact, and asserts that no drawn run
    contains `PSK`.
  - Unit 336 found it red with `modes draws [PSK31] before any PSK31 contact`.
  - The Modes next card's PSK31 row is R22's *where the unearned mode lives and who is there*, and
    step 1 was proved on it.
  - Hall of Fame's next first, `A PSK31 contact`, is Ruling C's *nearest unearned card* (unit 333
    item 1).
  - **So the test and two of Tim's later rulings assert opposite things about next cards.**
- **Criterion 3, *the undeletable files listed once*.** Unit 334's report (`d7178b7`, section 2)
  listed thirteen emptied files. **All thirteen are still in the tree** (this arbiter's `ls`):
  - `commit-msg-326.txt` and `toolsarbitervalidate-output.bat`;
  - `tools\arbiter\unit323-append.bat`, `tools\arbiter\unit323-append.py` and
    `tools\cut-header-action.py`;
  - `tests\Hamlet.App.Tests\Views\Unit333ProbeTests.cs` and
    `tests\Hamlet.App.Tests\ViewModels\ThePsk31TabIsInertTests.cs`;
  - `tests\Ft8Sharp.Tests\Dsp\Unit216Probe.cs`, `Unit217Probe.cs` and `UpstreamSyncSearchProbe.cs`;
  - `tests\Ft8Sharp.Tests\Ldpc\UpstreamLdpcProbe.cs`;
  - `tests\Ft8Sharp.Tests\TempEncoderProbe.cs` and `Unit289SourceProbe.cs`.

  Whether units 335 to 347 emptied any more was not searched.
- **Criterion 4, *the two `Views` reds*.** Unit 331's queue item 14, carried verbatim ever since, says
  *the `Views` reds in `TheMenuIsUnderTheMouseTests` are eight, not two*.
  - **One cause:** the tests fail with `expected both decoded lists in the window, found
    DigitalDecodedRows`. The mine rows are realized only after *show the N messages* is pressed,
    which is deliberate.
  - **The scene drives the send path.** It uses `FakePort` and `FakeSink`, builds `Ft8ArmedSend` over
    `Ft8TransmitSequence` (`:465`), executes `SendMessageCommand` (`:489`) and a menu item's command
    (`:278`), and reads `AudioWentOut`.
  - `TheWholeChainRunsFromOneRightClickTests` (`tests/Hamlet.App.Tests/ViewModels/`) has two more
    reds. Unit 337 traced them to the same cause, and they open a real audio endpoint.
- **Criterion 5, *the points file's comment block names every key in the file and no key it lacks*.**
  - The file is `data/achievements/achievement-points.json`, embedded at
    `Hamlet.RadioEngine.csproj:91`.
  - Its comment block runs to line 50. It names the eleven top-level keys and `per`, `special`,
    `all`, `milestones` and `tiers`.
  - **It also names `rank_names`, which the file lacks.** Line 34 says *Not in this file as shipped*.
  - `TheAchievementsPageTests.TheShippedPointsFileDocumentsEveryKeyInACommentBlockAtItsTop`
    (`:298`) checks one direction only, every key in the file named in the block. **It asserts the
    other direction's opposite:** at `:340`-`:342`, `rank_names` is documented and is not a key.
  - `assets/data/achievement-points.json` is a second copy that nothing reads (unit 336). It lacks the
    comment block.
- **The other small reds R27 names, none of them a criterion:**
  - `TheTotalMilesTests.TheBadgeSaysZeroMilesOnAnEmptyLogAndTheLowestTierIsNext` (`:38`) wants `grid
    to grid, added up`. `src` has drawn `every mile, added` since unit 332's `3ea16ec`.
  - `TheAchievementsScreenTests.TheWindowDrawsEverySixRows` (`:280`) is unit 336's *known one*.
  - `TheOperatorCanStopItTests` has two reds, both transmit-side.
  - The engine carry-forward read 85 of 86 once, in unit 347.

```
PHASE GOAL: The screen, done right - the main window as the approved mockup
            (step 0, done on the state reader's verdict on unit 341); every
            achievements category page as trading cards (step 1, done on the
            state reader's verdict on unit 347); then what the last phase left
            (step 2, not started, 0 of 5); then Tim at his window says it
            passed (step 3).
UNIT GOAL:  Close step 2 on evidence: the States badge and cards say worked,
            and the count says what it counts, with no confirm anywhere the
            achievements window draws at 1400 and 1920; the Modes test agrees
            with the rulings Tim made after it; the thirteen emptied files are
            one list; the Views reds are named with why; and the shipped points
            file documents exactly the keys it carries.
ADVANCES:   step 2 - must-pass 1 in task 1; must-pass 2 and 5 in task 2;
            must-pass 4 in task 3; must-pass 3 by task 0's list, written once in
            the report. Task 4, the drop candidate, advances none.
DRIFT:      0
```

---

## 2. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report any mismatch.
**Report the mismatch; do not repair the instruction.** Mismatches go in the report even when the
work succeeds anyway.

Check:
- **Every file and line §1 cites**, line by line, and the thirteen files: whether each is present,
  tracked, and comments only.
- **Unit 347's commits** are `4be21c0`, `dc47f9f`, `92584a2`, `3dbe927`, `049a30a` and `066ad04`, each
  pushed. `origin/main` read `066ad04` for this arbiter. `output.md` in the tree is unit 347's.
- **The version is 1.13.34** in `Directory.Build.props` (line 785 for this arbiter).
- **`DECISIONS.md` tops at HM-DEC-163.**
- **The launcher's files still disagree with themselves.** One line each; edit none:
  - `PHASE_OUTCOME.md`'s header reads step 0 `in progress`. The reload reads the last step 0 entry
    (unit 340, `blocked`) as winning. There is no unit 341 or 344 entry.
  - `PHASE_STATUS.md` reads `CURRENT_STEP: 0` and step 1 `done`.
  - Units 345, 346 and 347 are recorded as `UNIT 1`, `UNIT 2` and `UNIT 3 - STEP 1`.
  - `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md` and files under `.run-unit\` are modified and
    uncommitted by the launcher.
- **The reload says `CLAUDE.md` §1 holds `CPS-DEC-0163`.** Unit 347 found no `CPS-DEC` id in the file.
  It is parked with the id schemes. One line.
- **`validate-output.bat` reads the `UNIT:` line only in the report's first 60 lines** (unit 347 item
  4). Keep the ordering block and header inside them, and always pass the report (§9).
- **`tools\arbiter.bak-20260913\` is untracked at the root.** **Never `git add -A` or `git add .`**;
  stage files by name.
- **The tool facts in §7** are unit 347's. Say which held for you.

**Reds expected, older than this unit.** Task 0 runs some of them once, by exact name, only to read
their failure lines. **Do not chase any of them outside the task that names it:**
- `ThePsk31RecordsAppearTests.WithNoPsk31ContactNoPsk31CardIsOnTheScreenAndNoneIsDimmed`: task 2.
- `TheMenuIsUnderTheMouseTests` (8): task 3.
- `TheTotalMilesTests.TheBadgeSaysZeroMilesOnAnEmptyLogAndTheLowestTierIsNext` and
  `TheAchievementsScreenTests.TheWindowDrawsEverySixRows`: task 4.
- `TheOperatorCanStopItTests` (2) and `TheWholeChainRunsFromOneRightClickTests` (2): **never run.**
  They are named in the report (ruling 39).

**Expected on the carry-forward list: 111 of 111 app, 86 of 86 engine**, unit 347's numbers. **If a
red turns green or a new red appears in what you ran, say which.** This unit changes no engine source
file. The points file is embedded by the engine project but lives under `data\`, so an engine number
that moves is a finding.

**Expected red, and wanted:** each new assertion in tasks 1 and 2 is red against the tree as it is. By
ruling 19, that red is the watched red. Give its failure line.

---

## 3. Rulings in force - do not re-argue

**HM-DEC-155**, transcribed: *A unit runs no test suite. It may run only the test it constructs in
that work instruction, filtered by exact name, in the foreground, with a stated timeout, and it never
backgrounds a command and polls for it.*
- Run the carry-forward list as the top comment of `docs\carry-forward-tests.txt` says: two
  invocations, one build each, with status written immediately before each.
- **The classes this instruction names may run filtered by class name, one filter per task:**
  `ThePsk31RecordsAppearTests`, `TheAchievementsPageTests`, `TheCategoryPagesAreTradingCardsTests`,
  `TheAchievementsPageClicksInTests`, `BindingHealthTests` and `VoiceTests`.
- **Also by class, once each, where a task says so:**
  - `TheTopRowTests` and `TheWorkingPanelsTests` (task 0, a reading only);
  - `TheMenuIsUnderTheMouseTests` (task 3);
  - `TheTotalMilesTests` and `TheAchievementsScreenTests` (task 4).

**R27, Tim, 2026-09-12 (`PHASE_PLAN.md` §R), in full:**

> **R27 - what the last phase left.** Unit 336's open items: the States wording (*worked*, never
> *confirmed*, and the count says what it counts); the Modes test; the files sessions cannot delete,
> listed once for Tim; the two `Views` reds and any other small red in the record; the points file's
> comment block checked key by key against the file. Small, and one step.

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

**R23**: *States are scored from the ADIF `STATE` field... `Rank 1`...`Rank 8` when absent, so Tim can
name them without a session. The points file's every key is documented in a comment block at its
top.*

**Ruling C, Tim, 2026-09-12**, as unit 333 quoted it: *every kind shown, the nearest unearned card in
each, nothing beyond it.*

**`ACHIEVEMENTS_PHILOSOPHY.md`, the lines that bite here:**
- **§3.1:** *A band's records do not exist until he has made a contact on that band. A mode's records
  do not exist until he has worked that mode... No ghost FT4 records.*
- **§3.4:** *Records unlock on first contact... Named challenges stay visible before they are
  earned... their whole purpose is to be a target, so hiding them defeats them.*
- **§4:** *DXCC and Worked All States are counted by confirmations, not contacts. Hamlet counts
  worked, says worked, and never says confirmed (§0.0).* And: *Nothing shames.*

**`PHASE_PLAN.md` §1, §2 and §6, the lines that bite here:**
- *Screen only. Nothing here touches the radio, a decoder, a parser, the transmit chain or the log's
  content. A step's exit is what is on the screen, asserted by computation and described in words,
  then Tim's eyes. Every appearance claim is computed, not seen, and says so.*
- *The arbiter stops for three things only: keying, transmit or the radio's safety; money past the
  budget; a decision that changes what the product promises the operator. On everything else it
  takes its own recommendation, marks it author's and overrulable, applies it, and continues.*
- *A later ruling of Tim's contradicts a line of this plan. The later ruling wins.*
- *A must-pass is missed by a little: ship, report, `partial`, move on. Never loosen a test.*
- *A string will not fit: shorten and say which, or widen; never clip.*
- *Anything touches the radio, a decoder, a parser or the transmit chain: `MOVE: stop`.*
- *A package is needed: `MOVE: stop`.*
- *A file must be deleted: empty it, comment it, list it.*

**The arbiter's rulings 1 to 10** (the main window) **stand as built.** This unit reads step 0's tests
once and changes nothing there.

**The arbiter's rulings 11 to 32** (work instructions 342 to 347, the category pages) **stand as units
342 to 347 built them.** They are the author's, marked for Tim at step 3, and overrulable. Two of them
bite here:
- **19. Watching red.** Show a new assertion fail once, against a deliberately wrong expectation set
  on the test window only, and give the failure line. **Never break markup or a view to watch a test
  fail.** Where the new assertion is red against the tree as it is, that is the watched red.
- **20.** A fix never changes what a card is earned by, what scores, any points, or the words rulings
  11 to 16 set. A string that will not fit is shortened and named.

**Ruling 22 is closed by this unit.** It kept the PSK31 next-card collision and the States wording as
drawn, for step 2. Rulings 35 and 36 now decide both.

**Ruling 23 stands.** PSK31 decoding findings are logged, not chased.

**The arbiter's rulings for this unit.** They are the author's, marked for Tim at step 3, and
overrulable:

33. **Step 2's entry is met, and step 0 is read once.**
    - Step 1 is `done` on the state reader's verdict on unit 347. Task 0 re-runs its two classes as
      the entry check.
    - Task 0 also runs `TheTopRowTests` and `TheWorkingPanelsTests` once, by class, **as a reading
      only**. The launcher's record of step 0 disagrees with itself (§2), and the phase ends at Tim's
      window. Give each as *n of n* in block A. **Change nothing on the main window.** A red there is
      reported once and not chased.
34. **Criterion 1's reach.**
    - ***The States badge and its cards*** are the States badge on the opening page, and the States
      page's band, earned cards and next card, drawn at 1400 and 1920.
    - ***No confirmed anywhere*** means no case-insensitive `confirm` in any run or hover the
      achievements window draws: the opening page, the eight kinds and the seven continent pages, at
      1400 and 1920. It also means none in any string an achievements view model in
      `src/Hamlet.App/ViewModels/Achievement*.cs` holds for a card or a hover.
    - **The sentence at `AchievementScreen.cs:543`-`546` is reworded without the word, keeping its
      meaning.** An example: *The count says worked: the DXCC award counts contacts confirmed on
      paper or electronically...* is wrong, because it keeps the word. Use something like *The count
      says worked; the DXCC award counts confirmations, on paper or electronic, and Hamlet only knows
      what passed on the air from your own log.* That reword holds whether or not a window draws the
      sentence today. Say which it is.
    - **Out of reach, untouched:** *confirmed* where it is about the radio's read-back, a license
      lookup, telemetry outcomes, or the conversation card's *you both confirmed it* (an exchange,
      not an award). None of them is an achievement, and the read-back and telemetry are the radio
      side.
35. **The States count says what it counts.**
    - Where the States count is spoken - the badge's standing and the category band's count - it
      names **states worked, read from the log's `STATE` field**, in the fewest words that fit at 1400
      and 1920. Say where each phrase landed. The badge and the band say the same number.
    - **Where the log holds United States, Alaska or Hawaii records that carry no `STATE`**, one line
      on the band or the next card says how many, in the sense of *n US contacts carry no state*. It
      is a count Hamlet can read from the log, and it says nothing about where those stations are. It
      is left out where the number is 0. **This line is task 1's drop candidate.**
    - **Unchanged:** what scores, the points, every other kind's `n worked`, and no state worked out
      from a callsign.
    - **Unit 335 item 1 is answered: keep** `Any state you have not worked` and `Hamlet cannot tell a
      caller's state`. Both are true, and the second is the §0.0 reason the card lists no caller.
    - *Why:* R27's own clause. On Tim's log a count of 2 beside hundreds of US contacts reads as a
      fault unless it says what it counted.
    - *Rejected:* a state guessed from a prefix (unit 331 item 3, *a `W3` can be anywhere*). Also
      rejected: writing `STATE` into Hamlet's own log entries, which is the log's content and outside
      this screen-only phase (§1).
36. **The Modes test agrees with Tim's later rulings: with no PSK31 contact, PSK31 is a next card and
    nothing else.**
    - The test at `ThePsk31RecordsAppearTests.cs:86` is corrected under R12. On the no-PSK31 log,
      PSK31 appears on **no earned card, no dimmed card, no record card, no badge and no tab**.
    - It may appear only on **the Modes next card's row** (R22) and **Hall of Fame's next first**
      (Ruling C). Each of those is asserted to be the unearned next card, drawn as next cards are
      drawn.
    - The test may be renamed to say that. Keep its old name in its remark and in the report, so the
      criterion can still find it.
    - `TheFirstPsk31ContactRevealsTheModesRecords`, `TheWordingSaysWorkedAndNeverConfirmed` and the
      reveal event's count of 2 stay as they are.
    - **Nothing on the screen changes.** Only the assertion catches up with what Tim ruled.
    - **Unit 336 item 2 and unit 333 item 1 are answered by this ruling.**
    - *Why this is not a stop:* R22 and Ruling C are Tim's, and later than §3.1. `PHASE_PLAN.md` §6
      lets the later ruling win. §3.4 already keeps a target visible before it is earned, and §3.1
      stays whole for records. The product's promise does not move; a stale test does.
    - *Rejected:* taking PSK31 off both next cards, which reverses R22 and Ruling C. Also rejected:
      naming the test a known red, which leaves the tree asserting two opposite rules. That is the
      fault unit 336 item 2 named.
37. **The shipped points file carries exactly the keys its comment block documents.**
    - The file is `data/achievements/achievement-points.json`.
    - **`rank_names` is added to it as an empty list, `"rank_names": []`.** Its comment changes from
      *Not in this file as shipped* to say it is empty as shipped and where the names go. **Every rank
      still reads `Rank n`**, and that is asserted.
    - Tim's own copy under `%AppData%` is never replaced, so this reaches only a fresh seed. Say so in
      section 2.
    - **The test at `TheAchievementsPageTests.cs:298` checks both directions:**
      - every key in the file is named in the block. Today that is the top level and one level in;
        extend it to every level the file has;
      - every key the block documents is in the file.

      The unit states its rule for telling a documented key from a quoted example value (`"160m"`,
      `"CW"`, `"AK"`, `"NA"`), and prints both lists.
    - `:340`-`:342` (`rank_names` is not a key) is corrected to the new truth and named (R12).
    - *Why:* criterion 5's *no key it lacks*. Dropping `rank_names` from the block would undo R23's
      *so Tim can name them without a session*.
    - *Rejected:* a separate *keys you may add* paragraph. It still names a key the file lacks.
    - **`assets/data/achievement-points.json`** is unread. It is named in the report, and not changed
      or deleted.
38. **Criterion 4: the two `Views` reds are `TheMenuIsUnderTheMouseTests`' reds, and they are named as
    known reds with why. They are not fixed.**
    - Unit 331 item 14 measured them as eight, with one cause.
    - Run the class once, in task 3. Its port and sink are fakes, so nothing reaches a radio. Give *n
      of n* and each failing test's name and failure line.
    - **No edit to the test file.**
    - *Why:* the cause is the test's premise, but the premise sits in a scene that builds the FT8 armed
      send, executes the send command and asserts on audio going out. `PHASE_PLAN.md` §6 puts the
      transmit chain outside this phase, and §0.2 is absolute. Criterion 4 allows naming.
    - The report gives Tim the test-side fix, as a sentence for a later phase: realize the mine rows
      the way the operator does before the menu is read.
    - **Unit 331 queue item 14 is answered by this ruling.**
39. **Any other small red in the record** (R27, not a criterion) **is task 4, the drop candidate.**
    - `TheTotalMilesTests.TheBadgeSaysZeroMilesOnAnEmptyLogAndTheLowestTierIsNext` is corrected to the
      badge line `src` draws (R12). It is achievements wording only.
    - `TheAchievementsScreenTests.TheWindowDrawsEverySixRows` is run by class. It is fixed only if its
      failure is a stale expectation of achievements wording or layout that a ruling changed.
      Otherwise it is named with why.
    - `TheOperatorCanStopItTests` (2) and `TheWholeChainRunsFromOneRightClickTests` (2) are **never
      run.** They are named with why: the transmit side, and a real audio endpoint.
    - The engine's one-time 85 of 86 is named with the failing test if it recurs.
40. **Criterion 3: the undeletable files are one list, once, in section 2.**
    - Give each file's full path, whether it is tracked, and that it is comments only.
    - It covers the thirteen from unit 334, plus any file a session emptied since, found by searching.
      Say how.
    - Under *not on the list*: `tools\arbiter.bak-20260913\` (the launcher's, untracked, not emptied)
      and the gitignored scratch unit 334 named.
    - **In section 4's carried text, unit 331 queue item 19 and unit 333 item 3 are replaced by one line
      each:** `ANSWERED by unit 348 - the one list is in section 2`. Their file names come out, so the
      report lists the files once. HM-DEC-139 carries an ask until it is answered, and this is the
      report that answers it.

**Standing, transcribed:**
- **§0.0**: *Never present a guess as a decode… This binds pictures as hard as sentences.* Every
  appearance claim is computed, not seen, and says so once.
- **§0.2**, *transmit safety, ABSOLUTE*: *One operator action, one transmission.* Nothing here
  transmits or tunes. No test this unit writes presses CQ, Stop, Capture, a band button, a send or a
  transmit command.
- **§0.5**: *every panel is collapsible, and a collapsed panel still carries its summary. Collapsing
  hides detail, never information.*
- **§0.6**: every ink clears 4.5:1 against its fill, and color is never the only carrier.
- **R12**: *a session fixes its own tests and never asks the owner to approve it.* An assertion made
  untrue by a ruling is corrected to the new truth and named, never deleted.
- **R13**: telemetry on every new stage. This unit adds no stage. `achievement_points_loaded`'s
  `fileHash` changes with the file, and that is expected. Say so.
- **R14**: *a test exists to prove an exit criterion.* Extend the tests the criteria need and add no
  others, apart from the trace.
- **R19**: American spelling.
- **HM-DEC-139**: open asks are carried verbatim until answered.

---

## 4. Status cadence

Write status **before every `dotnet` command, after every commit and after every task**, with
`sh tools/status.sh`. It ran for every write in unit 347. If it is refused, take a `date` reading and
paste it whole. **Never compose a time.**

The script hard-codes `RULES_AT: HM-DEC-161`. **After every write** (unit 347 did it only once), set
that line back to `HM-DEC-163 (2026-09-12)`, or to the highest id §2 finds, with the file editor.

The watchdog kills a session only when its process tree has used no CPU for ten minutes.

---

## 5. The tasks

### Task 0 - the trace: what step 2's five things are on the tree today

Measure before anything is built. **Say what you find rather than confirming §1.**

1. **Commit `WORK_INSTRUCTIONS.md`, `PHASE_STATUS.md` and a patch bump (1.13.34 -> 1.13.35, with its
   comment block).**
   - In `PHASE_STATUS.md`, change only the `WORK_INSTRUCTION:` line, to `348 - step 2, what the last
     phase left`. Do not touch its `STEP:` lines, `CURRENT_STEP` or `HEARTBEAT`.
   - Do not commit `.run-unit\`, `PHASE_OUTCOME.md`, `RUN_LEDGER.md`, `SESSION.lock` or
     `tools\arbiter.bak-20260913\`.
   - The message is `chore(unit348): step 2, what the last phase left - the trace before a line is
     changed`.
   - Run the carry-forward list, status first, and give both invocations as *n of n*.
2. **The entry check and the step 0 reading (ruling 33).**
   - Run `TheAchievementsPageClicksInTests` and `TheCategoryPagesAreTradingCardsTests` in one filter.
     Give each as *n of n*. **If either is red, report it and stop at task 0.**
   - Then run `TheTopRowTests` and `TheWorkingPanelsTests` in one filter. Give each as *n of n*, and
     the failure line of any red. Do not stop for a red there.
3. **The reds, read once.** In one filter, run
   `ThePsk31RecordsAppearTests.WithNoPsk31ContactNoPsk31CardIsOnTheScreenAndNoneIsDimmed`,
   `TheTotalMilesTests.TheBadgeSaysZeroMilesOnAnEmptyLogAndTheLowestTierIsNext` and
   `TheAchievementsScreenTests.TheWindowDrawsEverySixRows` by exact name. Give each failure line. Task
   3 runs `TheMenuIsUnderTheMouseTests`.
4. **Add `Unit348TraceWhatTheLastPhaseLeft` to `TheCategoryPagesAreTradingCardsTests`.** It asserts
   nothing and presses nothing that transmits or tunes. It prints:
   - **States:** the badge's standing, and the States page's band count, earned cards and next card,
     as drawn at 1400 and 1920 on `StateContacts()`. Also how many of that log's US, Alaska and Hawaii
     records carry no `STATE`.
   - **Every drawn run and hover containing `confirm`**, case-insensitive: the opening page, the eight
     kinds and the seven continent pages at 1400 and 1920, on the twelve-contact log and on
     `StateContacts()`. Print `NO CONFIRM DRAWN` where there is none.
   - **Every string in `src/Hamlet.App/ViewModels/Achievement*.cs` containing `confirm`**, and
     whether a window draws `AchievementScreen`'s card detail today. Read that from the source, and
     say which member would carry it.
   - **The Modes test's walk on its own no-PSK31 log**: every run naming `PSK`, with the card it sits
     on and whether that card is earned, a next card, a badge or a tab.
   - **The points file:** every key at every level, and every quoted word in the comment block with
     task 2's documented-key rule applied. Print both lists, and each key in one list and not the
     other.
5. **The files.** Search the tree for files sessions emptied (comments only, no code), tracked and
   untracked. Say how you searched. For each of the thirteen and any new one: present, tracked,
   comments only. **This is criterion 3's list.** It goes in section 2 once.
6. **Answer from the numbers, in section 1, before task 1 starts:**
   - What does the States count say today, and how many US records in the fixture carry no `STATE`?
   - Where is `confirm` drawn, and where is it only held?
   - Which PSK31 runs does the Modes test catch, and on which cards?
   - Which keys are in the file and not the block, and in the block and not the file?
   - Were step 0's two classes green?

**Drop candidate:** none. Tasks 1 to 3 are built on its numbers, and criterion 3 is its list.

### Task 1 - States says worked and what it counts, and nothing says confirmed (rulings 34 and 35)

1. **Extend `StatesCountWhatTheLogsStateFieldSays` first**, at 1400 and 1920. Watch it red against the
   tree as it is:
   - the badge's standing and the band's count name states worked from the `STATE` field, and say the
     same number;
   - where `StateContacts()` holds US records with no `STATE`, the drawn line gives that number;
   - the next card keeps `Any state you have not worked` and `NoStateFromTheAir`;
   - no drawn run or hover on the opening page, the eight kinds or the seven continent pages contains
     `confirm`, case-insensitive.

   Keep every existing assertion. `Assert.StartsWith("2 ", badge.Standing)` is corrected only if the
   new words move the number, and it is named if so.
2. **Change the words** in `AchievementBadges` and `AchievementCategory` for States only, and reword
   `AchievementScreen.cs:543`-`546` (ruling 34). Shorten per §6 if they do not fit, and name it.
   **Scoring, points and every other kind's words are unchanged.** Any assertion elsewhere that
   carried the old States words or the old sentence is corrected and named (R12).
3. **Run the one filter:** `TheCategoryPagesAreTradingCardsTests`, `TheAchievementsPageClicksInTests`,
   `TheAchievementsPageTests`, `ThePsk31RecordsAppearTests`, `BindingHealthTests` and `VoiceTests`.
   Give each class as *n of n*. The Modes test stays red until task 2; say so. Then run the
   carry-forward list, status first. Commit and push.

**Drop candidate:** the no-`STATE` count line (ruling 35). If it will not fit at 1400 without clipping
or a shortening that loses its sense, drop it, keep the source clause, and say so.

### Task 2 - the Modes test and the points file (rulings 36 and 37)

1. **The Modes test first.** Correct
   `WithNoPsk31ContactNoPsk31CardIsOnTheScreenAndNoneIsDimmed` to ruling 36:
   - it walks what it walks now;
   - PSK31 is drawn on no earned card, dimmed card, record card, badge or tab;
   - each run naming PSK31 is on the Modes next card's row or Hall of Fame's next first, and each of
     those cards is unearned.

   **Watched red (ruling 19):** the same assertion against a deliberately wrong expectation on the
   test only, for example the Modes next card treated as an earned card. Give the failure line.
   **Change no card, no markup and no view model.**
2. **The points file.**
   - Extend `TheShippedPointsFileDocumentsEveryKeyInACommentBlockAtItsTop` to both directions at every
     level, with the rule printed.
   - Correct `:340`-`:342` to the new truth, and assert that every rank on the shipped file still reads
     `Rank n`.
   - Watch it red on the tree as it is: `rank_names` documented and not in the file.
   - Then add `"rank_names": []` to `data/achievements/achievement-points.json` and change its comment
     (ruling 37). **Change no other key, value or points.**
   - If another key is on one side only, as task 0 found, document it in the block. Never remove a key
     from the file.
3. **Run the one filter again**, then the carry-forward list, status first. Commit and push.

**Drop candidate:** none. Each half is a must-pass.

### Task 3 - the `Views` reds, named (ruling 38)

1. **Run `TheMenuIsUnderTheMouseTests` once, by class**, status first, with a stated timeout. Give *n
   of n*, and each failing test with its failure line.
2. **Read, and do not edit**, the member the failure line comes from. Say whether the eight still share
   unit 331's cause (`DigitalMineRows` realized only behind *show the N messages*), and name any test
   that fails for another reason.
3. **No commit** unless the trace test gained a line. The result goes in section 3's known-reds table
   (§9).

**Drop candidate:** none. It is criterion 4.

### Task 4 - any other small red (ruling 39, the drop candidate)

1. **`TheTotalMilesTests.TheBadgeSaysZeroMilesOnAnEmptyLogAndTheLowestTierIsNext`**: correct its
   expected badge line to what `src` draws (R12). Give the old and new line, and run the class.
2. **`TheAchievementsScreenTests.TheWindowDrawsEverySixRows`**: run the class. Fix it only by ruling
   39's test; otherwise name it with why.
3. **Run the one filter**, then the carry-forward list, status first. Commit and push.

**Drop candidate: this task, whole.** If time runs short, skip it and say which reds stay unnamed. No
criterion rests on it.

---

## 6. Parked - do not touch, do not raise

- **The main window, all of it.** Task 0 reads step 0's two classes once. Nothing is changed.
- **Every achievements card and word except:**
  - the States count's words (ruling 35);
  - `AchievementScreen`'s *confirmed* sentence (ruling 34);
  - the tests rulings 36, 37 and 39 name.

  Rulings 11 to 32 stand as built.
- **VK2DEF's map span** (ruling 28, closed). It is for Tim at step 3.
- **The `Why` hovers** (`ModeFirstRow.Why` stays undrawn), **`first_answer_to_own_cq`** (never awarded,
  and still documented), **the Hall of Fame badge's gold ink** (unit 335 item 3), and **the continent
  level's seven badges** (unit 332 item 2).
- **The log's content**, including writing `STATE` into Hamlet's own entries (ruling 35).
- **PSK31 decoding, all of it** (ruling 23): the parser, the splitter, the demodulator, the ALC margin.
  Also unit 347 item 2, the live grid through the splitter.
- **Transmit, all of it:** `TheOperatorCanStopItTests`, `TheWholeChainRunsFromOneRightClickTests`, and
  any edit to `TheMenuIsUnderTheMouseTests` (ruling 38).
- **The engine carry-forward's 86 against 87**, and the missing version comment blocks at 1.13.29 and
  1.13.31. Findings; not repaired here.
- **Carried in `PHASE_PLAN.md` §7 and belonging to Tim or the harness:**
  - the live license lookup and the live heard count;
  - the two id schemes, including `CPS-DEC-0163`;
  - real flags on country cards, PSK31 step 6, real PSK31 audio and the map bitmap's license;
  - the launcher's `PHASE_OUTCOME.md` header, its missing unit 341 and 344 entries, its unit
    numbering, and `tools/status.sh`'s hard-coded `RULES_AT`.
- **Anything touching the radio, a decoder, a parser or the transmit chain. Any package.**

## 7. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.** HM-DEC-155.
- **Never run `TheOperatorCanStopItTests` or `TheWholeChainRunsFromOneRightClickTests`, and never edit
  `TheMenuIsUnderTheMouseTests`** (rulings 38 and 39).
- **No state from a callsign, and no `STATE` written to a log** (ruling 35).
- **No card, markup or view-model change to make the Modes test pass** (ruling 36). The test moves;
  the screen does not.
- **No key removed from the points file, and no value or points changed** (ruling 37).
- **Do not delete a file.** Empty it, comment it, list it (§6).
- **Do not loosen a threshold or tolerance, and do not break the view to watch a test fail** (ruling
  19, §6).
- **Do not edit `PHASE_OUTCOME.md`, `RUN_LEDGER.md`, the `STEP:` lines, `.run-unit\allowed.txt` or
  anything under `tools\arbiter\`.** They are the launcher's.
- **Never `git add -A` or `git add .`** (§2).
- `CLAUDE.md` §12.6 covers the rest. **No package. Report mismatches. Write American.**
- **The tool facts, as unit 347 found them.** Say which held for you:
  - **ran:** `sh tools/status.sh` (every write); `&&` joining `git add`, `commit` and `push`; commits
    with several `-m`; `grep -E`, `tail` and `cut` in pipes; `date`;
  - **asked for approval, not run:** `grep` with `\s` in its pattern; a `grep -E` pattern with
    `{0,40}` and `-A1`; `;` joining `grep`, a `tail` redirect into a file, `head` and `wc`. This
    arbiter also found `grep -v` with `|` in its pattern asking for approval;
  - **refused:** the file writer on a path outside the repository (`--restricted`);
  - **older and not retested:** a `cd` before `git`; `awk`; `sort` with options; output redirects and
    `sed -i`; `git -C`; an apostrophe in a `.bat` argument; doubled backslashes collapse; use
    exact-text edits for markup.

## 8. Committing and pushing

Commit and push each task on its own, on `main`, staging files by name. The report and the status file
go in their own commit. The report names every commit and whether each push succeeded. **A refused
push is reported as refused, with the reason.**

---

## 9. Reporting

Write `output.md` at the root, then stop. Do not start the next unit. **Every exit writes it**:
finished, blocked, failed or stopped early. **Write the report before task 4 if time is short.**

Canonical headings: `## 1. What Claude did`, `## 2. What the owner should expect`,
`## 3. What you should see`, `## 4. What's blocking us`. Validate it with
`dotnet build tools/arbiter/validate-output.proj -p:Report=output.md`, or with
`tools/arbiter/validate-output.bat output.md`. **Always name the report.** **The `UNIT:` line must
fall inside the first 60 lines.**

**The ordering block comes first.** `validate-output` refuses a report without it.

```
READ IN THIS ORDER.

A. The phase goal - the screen, done right. Step 0 done (state reader on
   unit 341; TheTopRowTests <n of n>, TheWorkingPanelsTests <n of n>, read
   once). Step 1 done (state reader on unit 347). Step 2 <state as the
   evidence stands>: <n> of 5 must-pass met, was 0 of 5. Step 3 not
   started.
B. Step 2 and its exit criteria:
   entry: TheAchievementsPageClicksInTests <n of n>; trading cards <n of n>
   1. States says worked, no confirm - badge [<drawn>], band [<drawn>],
      no-STATE line [<drawn> or DROPPED]; confirm drawn: <none or where>;
      AchievementScreen sentence [<new words>]; <test, result, widths>
   2. the Modes test <old name -> new name>: PSK31 drawn only on
      [<cards>]; <result>; watched red <line>
   3. the undeletable files: <n>, listed once in section 2
   4. the Views reds: TheMenuIsUnderTheMouseTests <n of n>, <n> named
      with why in section 3
   5. the points file: <n> keys in the file, <n> documented, none on one
      side only; rank_names []; <test, result>
C. The report last. Section 4 raises N items on top of the carried queue;
   say which, if any, stands in the way of a criterion in B - in
   particular whether any change reached scoring, a card other than
   States, or the transmit side.
```

**Every line specific to this unit.** If a criterion was not measured, say *not measured*. Do not
fill the shape.

```
UNIT:       348 - <complete|stopped> at task N of 4 - <date time, read from the clock>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   <yes|no> - <which step 2 criteria now hold>
NUMBER:     step 2 must-pass met: 0 of 5 -> <n> of 5
DRIFT:      0
```

**Section 2 carries the one list of undeletable files** (ruling 40): full paths, tracked or not,
comments only. Then *not on the list*. It also says that Tim's own points file under `%AppData%` is
unchanged (ruling 37).

**Section 3 leads with the answer:** does every step 2 must-pass now hold? If not, which one, and why.
Then, in words, at both widths:
- the States badge, band and next card;
- where PSK31 is drawn on the no-PSK31 log.

Then the step 2 table: test, criterion, drawn or source, widths, result, numbers. Then **the known-reds
table**: test, count, failure line, cause, why it is not fixed, and the fix a later phase could make.
List `TheMenuIsUnderTheMouseTests`, `TheWholeChainRunsFromOneRightClickTests`, `TheOperatorCanStopItTests`,
and whatever task 4 left. **Every appearance claim is computed, not seen. Say so once.**

**Section 4:** unit 347's section 4 verbatim per HM-DEC-139, from its line under
`## 4. What's blocking us` to its end, as committed in `066ad04`, including the queues it carries. Keep
it in place with the file editor. In the carried text, mark:
- unit 335 item 1 (the States next card's wording): `ANSWERED by work instruction 348 ruling 35 -
  kept`, with the count's new words;
- unit 336 item 2 and unit 333 item 1 (the PSK31 next-card collision): `ANSWERED by work instruction
  348 ruling 36 and task 2`, with the test's new name;
- unit 336 item 3 (`TheTotalMilesTests`): `TAKEN UP by work instruction 348 task 4`, with the result,
  or `DROPPED by unit 348 - task 4 was the drop candidate`;
- unit 331 queue item 14 (the `Views` reds): `ANSWERED by work instruction 348 ruling 38 - named as
  known reds, section 3`;
- unit 331 queue item 19 and unit 333 item 3 (the files): replaced by `ANSWERED by unit 348 - the one
  list is in section 2` (ruling 40);
- unit 347 item 1 (the engine's 85 of 86): `NOTED by unit 348`, with whether it recurred.

Then anything this unit raises. **A ruling is wanted only where Tim must decide. Everything else is a
finding, and says so.** Rulings 11 to 40 are already marked for Tim at step 3. Do not raise them again
unless the page shows something that changes one.

---

```
ARBITER-DECISION
STEP: 2
APPROACH: open step 2 on unit 336's leftovers - the States count saying worked and its STATE-field source with no confirm anywhere the achievements window draws at 1400 and 1920; the Modes PSK31 test corrected so PSK31 before a PSK31 contact is only a next card (R22, Ruling C); the shipped points file carrying exactly the keys its comment block documents, rank_names added as an empty list; TheMenuIsUnderTheMouseTests' Views reds run once and named as known reds with why, not edited; the thirteen emptied files listed once; other small reds as the drop candidate
MOVE: continue
WHY: The state reader returned step 1 done on unit 347, so step 2's entry is met and the plan moves to it. No unit has run on step 2, and the loop test found nothing like this approach. None of the five items touches keying, money or what the product promises: the PSK31 next cards are Tim's own later rulings, and the Views reds that sit on the send path are named, not edited.
STATE: not started
DECIDED: ruling 33 - step 2 entry met on the unit 347 verdict, step 0's two classes read once and nothing changed; ruling 34 - criterion 1 covers every run and hover the achievements window draws at 1400 and 1920 and the achievements view-model strings, AchievementScreen.cs:543-546 reworded without the word, radio and lookup uses of confirmed out of reach; ruling 35 - the States count names states worked from the STATE field, with a line counting US records carrying no STATE as task 1's drop candidate, unit 335 item 1 answered keep; ruling 36 - the Modes test corrected under R12 so PSK31 with no PSK31 contact is drawn only on the Modes next card row and Hall of Fame's next first, screen unchanged, unit 336 item 2 and unit 333 item 1 answered; ruling 37 - rank_names added to the shipped points file as an empty list and the comment-block test made two-way at every level; ruling 38 - TheMenuIsUnderTheMouseTests' reds named as known reds with why and never edited, because their scene drives the FT8 send path; ruling 39 - other small reds are task 4, the drop candidate, with the stop and whole-chain tests never run; ruling 40 - the files listed once in section 2 and their carried mentions replaced by one ANSWERED line; ruling 22 closed - all the author's, marked for Tim at step 3, overrulable
LICENCE: PHASE_PLAN.md R27, sections 1, 2, 4 (step 2 entry and exit), 6 (the arbiter decides and continues; later ruling wins; shorten and name; never loosen; transmit chain stops; empty, comment, list) and 7; .run-unit/state-verdict.json (step 1 done on unit 347); .run-unit/s4-verdict.json (none); unit 347 output.md sections 1 and 4 at 066ad04; unit 336 output.md at cbe5000 (reds older than the unit; section 4 items 2 and 3); unit 334 output.md at d7178b7 section 2 (the thirteen files); docs/phase-maintenance-run/PHASE_PLAN.md R22, R23 and Ruling C as unit 333 quoted it; ACHIEVEMENTS_PHILOSOPHY.md sections 3.1, 3.4 and 4; AchievementScreen.cs 543-546; AchievementBadges.cs 353; AchievementCategory.cs 929 and 964; ThePsk31RecordsAppearTests.cs 86 and 152; TheAchievementsPageTests.cs 298-342; data/achievements/achievement-points.json lines 1-50 and 34; TheMenuIsUnderTheMouseTests.cs 278, 465, 489, 519; CLAUDE.md 0.0, 0.2, 0.5, 0.6; psk31 R12, R13, R14, R19; HM-DEC-139, HM-DEC-155, HM-DEC-163
ACCOMPLISHED: the achievements pages say worked and never confirmed, and the States count says it counts states from the log's STATE field; the test that contradicted Tim's next-card rulings now agrees with them; the points file Tim edits documents exactly what is in it; the files he has to delete by hand are one list; and every old red is either fixed or named with why - so the screen goes to Tim at step 3 with nothing left over from the last phase
ADVANCES: step 2 - must-pass 1 (States says worked, no confirmed) in task 1; must-pass 2 (the Modes test) and 5 (the points file's comment block) in task 2; must-pass 4 (the Views reds named) in task 3; must-pass 3 (the undeletable files listed once) by task 0's list in section 2; task 4, the drop candidate, advances none
END-ARBITER-DECISION
```
