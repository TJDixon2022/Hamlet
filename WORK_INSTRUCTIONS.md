# Work instruction 379 - the keyboard modes earn what FT8 earns: measured first, then connected

**Step 8 of the hardening phase, and the whole step: R40.** Five tasks. **Task 1 builds
nothing and repairs nothing**: criterion 8.1 *is* a measurement, and it is written into the
plan first for a reason - most of this machinery already exists, some of it already counts
PSK31 and Olivia, and a unit that rebuilt what the tree already does would be spending the
night on R14's forbidden work. **What task 1 measures decides what tasks 2 and 3 are allowed
to touch.**

**Status.** `tools/status.sh`, real clock, after every commit and every task.

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

*All four were checked against the tree at authoring time and all four hold: `SHACK_FACTS.md`
and `src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` are present, neither `CoreHMI.sln`
nor `MURC.sln` exists at the root, and the root is `C:\Source\HamLet`.*

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite; only this unit's names and `docs\carry-forward-tests.txt` as its
top comment says - **two invocations, one build each**, status written immediately before
each. Never background and poll.

**This unit's own runs are filtered and foregrounded too.** Task 1's measurement is one
`[Fact]` trace in the app test project, run by name. Tasks 2 and 3 run their own named types
and the app and engine types they turn red. That is not a suite and it is not a poll.

**This unit is app work and engine work both, and the app invocation is the flaky one.** Unit
378 lost two of its app invocations to Avalonia's headless `InvalidProgramException: You have
caused dispatcher loop`, at 1 ms in `HeadlessUnitTestSession.EnsureApplication`, on
`ThePowerIsOfferedTests` and on `TheWindowHoldsBelowItsMinimumTests` - two names it had never
landed on before - and both times the single re-run was clean. The engine invocation has not
failed since unit 375. **Expect it, re-run once, record both attempts, do not chase it** - it
is unit 375's item 3.

**A loop goes in a script file** (`;` is refused in a compound command).

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; `-m` more than once for a multi-line commit. **Write this repository's files as
UTF-8; a PowerShell `>` redirect writes UTF-16 and the launcher cannot read it.**

- **Shell output redirection (`>`) is refused to every path.** Write files with the editor.
  **A test run's output is read from the console**, or piped to `grep` in the same command.
- **A compound command with `;`, `&&` or a second operation is refused**, and so is `cd X &&
  Y`. **Python runs here - as a file you wrote with the editor, run by name.**
- **`git worktree add` is refused.** No task here asks for one.
- **Eleven calls were refused in unit 378 and every one of them was shape, not permission**
  (`.run-unit\denials.txt`, read at authoring time). The shapes that were refused:
  `cat >> FILE <<'EOF'` to append an outcome entry; `python - <<'EOF'` to patch a test file;
  `dotnet test ... | sed -n`; `grep ... ; echo ... ; grep ...`; `awk` with a `&&` inside its
  pattern; and **five separate attempts to run `validate-output.bat`**, every one of them
  through `cmd /c` or behind a `cd`. **Append to a file with the editor, not with a heredoc.
  Patch a file with the editor, not with a Python heredoc. Pipe a test run to `grep`, not to
  `sed`.**
- **`validate-output.bat` is runnable and unit 378 could not find the shape.** It is at
  `tools/arbiter/validate-output.bat` and it takes the same shape this instruction's author
  used for `outcome-read.bat` minutes before writing this line: **forward slashes, a leading
  `./`, one command, no `cd` in front of it and no `cmd /c` around it.**

  ```
  ./tools/arbiter/validate-output.bat output.md
  ```

  **Run it before you say the report is written.** If it still refuses, say so in section 4
  with the exact command and the exact refusal - that is a finding about the harness and it
  is worth more than a silent hand-check.

## 3. Asks still outstanding

Carried per HM-DEC-139, **verbatim in section 4**. Unit 378 raised four and **answered none of
the carried nineteen**, which is R31 working as intended. Where this unit stands on each of
378's four:

- **Unit 378's item 1 - `ViewTestsActThroughControlsTests.NoViewTestWritesAPropertyAControlOwns`
  is red, inherited, and reports `TheStopIsAlwaysOnScreenTests.cs:102 writes OperatingMode`.**
  Unit 375 wrote that line; it is not on the carry-forward list and has been red through three
  units. **It is a finding and it is not this unit's** - unit 378 ruled it belongs to whichever
  unit next touches that file, and no task here touches it. **Report it again in section 4.
  Do not repair it.** If some task of yours does end up editing
  `TheStopIsAlwaysOnScreenTests.cs`, then it becomes yours by unit 378's own rule and you
  repair it in that commit and say so.
- **Unit 378's item 2 - five canned line-and-variant pairs refused by the 30-second fallback.**
  **Answered by R43 and closed. Do not add a timing row. Do not raise it again.**
- **Unit 378's item 3 - the canned file is read once per session.** An author's choice, stated
  and overrulable, and **nothing in this unit touches it.** It stays on the queue as item 1.
- **Unit 378's item 4 - `Unit378Trace` now prints the after, not the before.** A note about a
  trace. **Your own trace has the same property and that is correct**: a trace prints and
  asserts nothing about the product, so it goes green when the product moves under it. **The
  before is preserved in your report, in `PHASE_OUTCOME.md` and in task 1's own commit
  message**, which is the only place the next unit can read it. Keep it off the carry-forward
  list (R14).

**The carried queue is twenty-one:** unit 378's items 1, 3 and 4; unit 377's item 4; unit
376's items 3, 4 and 5; unit 375's items 3 and 4; unit 374's item 3; unit 373's item 2; unit
372's items 4 and 7; unit 371's five; unit 369's four. **This unit answers none of them.**

**And the `RULES_AT` id-scheme split, again.** `PROJECT_STATUS.md` reads `HM-DEC-165
(2026-09-19)`; `CLAUDE.md` section 1 holds `CPS-DEC-0165`. The reload names it first among the
disagreements. `tools/status.sh` writes that field as a literal and **you may not edit
`tools\`**. Report it. Do not repair it.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet holds what it has - the hardening phase, screen, record and test
            work banked in the PSK31 and Olivia threads that needs neither the radio
            nor the owner, judged by tests that ran and, at the end, by Tim at his
            window.
UNIT GOAL:  A station Tim works on PSK31 or on Olivia and logs counts for exactly
            what the same station worked on FT8 would count for - the Modes badge,
            the Hall of Fame first for that mode, and every per-contact record the
            log can answer: country, state, grid, continent, miles - with the quill
            on a keyboard-mode row meaning what it means on an FT8 row, and the
            scores and the total moving on the achievements page. Measured first, in
            three columns, and then connected where and only where a column is
            short.
ADVANCES:   step 8, criteria 8.1, 8.2, 8.3 and 8.4 - the whole step, which has zero
            units spent and whose entry unit 378 opened. It also clears step 3's
            entry, which reads step 8 done.
DRIFT:      none.
```

**The count today**, from `PHASE_OUTCOME.md` and the reload of 2026-09-21 14:54.

| Step | State | Units spent | Where it stands |
|---|---|---|---|
| 0 | `partial` | 369, and 371's carried repair | 0.2 to 0.5 met; 0.1 met on a completed negative, ruled author's in instruction 372 |
| 1 | **`done`** | 372, 373, 374 | closed by 374 |
| 2 | `partial` | 375, 377 | 2.1, 2.2, 2.3 met. **2.4 alone holds it open and R43 defers it** |
| 3 | `not started` | 0 | **entry is step 8 done** |
| 4 | `not started` | 0 | entry is step 3 done |
| 5 | `not started` | 0 | Tim's own |
| 6 | **`done`** | 376 | closed by R42 at the measured floor |
| 7 | **`done`** | 378 | all five criteria met, none rounded up |
| 8 | `not started` | **0** | **this unit** |

**Step 8's entry, checked:** *step 7 done.* Unit 378 met 7.1 to 7.5 and the separate judging
session returned `STATE_AFTER: done` with its reasoning. **The entry is open, and it is the
only one that is.** Step 3 waits on this step; step 4 waits on step 3; step 5 is Tim's; 2.4 is
deferred by R43; steps 0, 1, 6 and 7 are done or closed.

**Why step 8 and nothing else.** R39 puts steps 6, 7 and 8 before steps 3 and 4 and makes step
3 depend on step 8. R43 names the order in words - *the arbiter goes to step 7, then step 8,
before anything else.* Step 7 closed last night. **There is one step available and this is
it**, and the loop test finds nothing resembling this approach in any entry of
`PHASE_OUTCOME.md`, because no unit has ever been spent on step 8.

**What this unit is worth, in the owner's terms.** Tim has spent two phases teaching Hamlet to
hear and answer on PSK31 and on Olivia. **Tonight, when he works a man on one of them and
presses Log, some of what that evening earned him arrives on the achievements page and some
of it may not** - nobody has ever measured which. After this unit, a PSK31 or Olivia contact
earns him exactly what the same contact on FT8 would have earned him, the page says so, and
**the report names, mode by mode and record by record, what was already true before the unit
touched anything** - because R40's own words are *measured first, then connected*, and a
number that was already right is worth more as a measurement than as a repair.

---

## 5. Verify this instruction against the tree

Every line below was read at authoring time, at the file and line named. **Report every
mismatch in section 4 and section 1; repair nothing but this unit's.** Line numbers move under
an edit - if one is off by a few, the name is the thing that matters; say so and go on.

### The scoring side already knows about both modes

`src\Hamlet.RadioEngine\Contacts\AchievementScores.cs`:

- Line **200**, `FirstsEarned(log)` - the Hall of Fame keys. Line **208** adds `first_contact`,
  **223** adds `first_psk31` where `log.Modes` holds `PSK31`, and **233** adds `first_olivia`
  where it holds `ContactModes.OliviaName`. **Both keys already exist and both are already
  earned off `log.Modes`.**
- Line **157**, `WorkedIn(kind, log)` - the eight kinds answer off `log.Continents`,
  `log.Entities`, `log.Grids`, `log.Bands`, `log.Modes`, `log.States`, `MilesIn(log)` and
  `FirstsIn(log)`. **Not one of them branches on a mode.**
- Line **326**, `WorkableModes => ContactModes.Logged.Count(m => m.IsContactMode)` - **six**,
  because `ContactModes.Logged` is the six plus Olivia and WSPR is not a contact mode. The
  badge's target is counted from the table and is nowhere typed.

`src\Hamlet.RadioEngine\Contacts\ContactModes.cs`: line **138** `Six` (CW, FT8, FT4, PSK31,
WSPR, Voice), line **172** `Logged` = `Six` plus `Olivia`, line **218** `Olivia(variant)`
composing `MODE=OLIVIA` with `SUBMODE=OLIVIA <variant>`.

`src\Hamlet.RadioEngine\Contacts\AchievementLog.cs`, `Read` at line **296** - one record
becomes one `AchievementContact`: the entity from the callsign, the continent from the entity
(**312**), the mode by `ContactModes.Logged.FirstOrDefault(m => m.Matches(...))` (**320**,
which is where unit 368 fixed Olivia being invisible), the grid from `GRIDSQUARE`, the miles
from his grid and the operator's (**328**), and **the state from `contact.State` and from
nothing else** (**331**).

### And the log side already writes the right mode

`src\Hamlet.App\ViewModels\MainWindowViewModel.cs`:

- Line **15475**, `ContactLogEntryForStation(callsign, heardOnHz)` - **the one place a log
  record is built**. It calls `Ft8ContactLogEntry.For` (**15494**) with the band from the row's
  own dial and the mode from **`Psk31ContactWith(who) ?? _digitalMode.Contact()`** (**15527**).
- Line **15604**, `Psk31ContactWith(station)` - the card says which of the two, and where the
  card cannot the channel does: `ContactModes.Olivia(state.Card.OliviaVariant)` or
  `ContactModes.Named("PSK31")`.
- Lines **15536** and **15551** - the RST and the grid come from the mode's own parser, because
  a PSK31 conversation has no FT8 fields.
- Line **15306**, `LogContactAsync`, appends through `ContactLogStore.Append` (**15388**) and
  then calls `RefreshWorkedBefore` (**15368**). `ReloadContactLogForTests()` at **15234** is
  that same refresh, and it is the handle a headless test has.

`src\Hamlet.RadioEngine\Contacts\Ft8ContactLogEntry.cs`, `For` at line **71**, builds the
`AdifContact`: `Call`, `StationCallsign`, `StartedUtc`, `EndedUtc`, `GridSquare`,
`ReportReceived`, `ReportSent`, `Band`, `Mode`, `Submode`, `FrequencyMhz`, `MyGridSquare`,
`Comment`. **`State` is not among them, for any mode.** A search of `src\` at authoring time
found no assignment to `AdifContact.State` anywhere except `AdifLog.cs:579`, which is the
*reader* filling it from somebody else's file. **Measure this yourself in task 1 and report
what you find; section 6 ruling 1 is what it means.**

### The quill reaches a PSK31 row already, and the CQ list may not

- `MarkIfItOpensSomething(row)` at **14634** is called from **two** sites: `PlaceRow`
  (**13655**), the funnel every FT8 row goes through, and **4065**, the PSK31 text-row builder,
  whose own comment says it asks *the same two questions `PlaceRow` asks of an FT8 row, of the
  same methods.* An Olivia row is built by that same builder. **So the quill on a keyboard-mode
  row is very likely already the same object with the same words - 8.3 may be an assertion
  rather than a repair (R14). Measure it.**
- `Nudges()` at **14594** builds one `NudgeSet` from `ContactLogStore.ReadRecords()` and the
  operator's grid (**14607**); `RefreshWorkedBefore` at **14752** drops it and re-asks every row
  on screen the moment the log changes.
- **And here is a thing the tree says that is not true.**
  `src\Hamlet.App\ViewModels\CqSnapshot.cs`, `From` at line **62**, builds the CQ list the
  achievements page is handed (`OpenAchievements`, **7555**, `Calling = CqSnapshot.From(...)`).
  **Line 74 reads `r.IsTextOnly ? "PSK31" : ""`** - and an Olivia row is a text-only row, built
  by the same builder at 4065 with `IsTextOnly: true`. The `CqCall.Mode` doc at
  `CqSnapshot.cs:10` says *`PSK31` where the row is a PSK31 text row*, written by unit 335,
  **before Olivia existed in this tree**. So on the achievements page's next cards, **an Olivia
  station calling CQ is labelled PSK31**. That is a sentence on the screen stating something
  untrue about a station (§0.0), it is inside 8.3's *means the same*, and it is this unit's.
  **Measure it before you move it** and print what you actually find.

### What already asserts this, and what will go red

**Read these before you write anything, and count them at task 0:**

- `tests\Hamlet.App.Tests\Views\TheOliviaRecordsAppearTests.cs` - five names, unit 368's, on
  criterion 5.2 of the Olivia phase: before the first Olivia contact nothing earned names
  Olivia, after it the records appear and the Modes badge counts it, the target is counted from
  the table, and `AnFt8AndPsk31LogScoresExactlyWhatItsOwnRecordsSay`.
- `tests\Hamlet.App.Tests\ViewModels\ThePsk31RecordsAppearTests.cs` - four names, units 326 and
  333, stood up on the drawn window.
- `tests\Hamlet.App.Tests\Views\TheAchievementsPageTests.cs` - ten names, **and 8.4 names this
  type by name.** `TheFixtureLogsScoresMatchTheHandComputedArithmetic` (line **112**) is
  hand-computed arithmetic over a fixture log: **if this unit changes what a contact earns, that
  name moves, and it is an R12 rewrite in its own commit that asserts more than it replaced -
  never a loosened number.**
- `tests\Hamlet.App.Tests\ViewModels\TheBadgesCountContactsTests.cs`,
  `TheCountIsTheRecordsTests.cs`, `TheQuillPopupTests.cs`,
  `tests\Hamlet.App.Tests\Views\TheAchievementsPageClicksInTests.cs`,
  `TheAchievementsScreenTests.cs`, `Unit300QuillLoadsTests.cs`.
- On the carry-forward list already:
  `TheOliviaLogsAsOliviaTests.AnOliviaContactLogsAsOliviaWithTheReportsAndTheGridTheCardShowed`
  - **Olivia's log guard. If that goes red you have changed what a record says, and that is a
  regression, not a repair.**

### The record and the versions

`PHASE_STATUS.md` `CURRENT_STEP` reads **7** and `WORK_INSTRUCTION` reads **378**. **Its
`STEP: 7` line still reads `partial`** although unit 378 met all five criteria and the judging
session returned `done`; `PHASE_OUTCOME.md`'s step 7 header line reads `partial` with the same
text. **Task 0 transcribes step 7 as done in both**, citing unit 378's report and its
`STATE_AFTER: done`. That is transcription of a verdict a separate session already returned -
**it is not a judgment of yours and it does not count as one of this unit's two rulings.**

The reload also names, as a disagreement, that `PHASE_OUTCOME.md`'s header said step 6 done
while an older entry for it said partial. **R42 closed step 6 and unit 377 transcribed it.
Check the two headers agree and report; do not re-argue step 6.**

`Directory.Build.props` line **1040** reads `<Version>1.13.65</Version>`.

---

## 6. Rulings in force

Transcribed in full. **Do not re-argue any of these.**

### The owner's ruling that licenses this unit, in full

**R40 - Tim, 2026-09-21: keyboard modes earn achievements like FT8.** *PSK31 and Olivia are
not connected to the achievement system as FT8 is. The unit measures which of the Modes badge,
the Hall of Fame firsts, the per-contact records and the CQ-list quill each mode reaches
today, then connects what is missing so a PSK31 or Olivia contact earns exactly what an FT8
contact earns.*

**R39 - Tim, 2026-09-21: the UI comes first.** *Steps 6, 7 and 8 are worked before steps 3 and
4; step 3 depends on step 8.*

**R43 - Tim, 2026-09-21: the UI first; the timing question is deferred.** *Step 2 stays partial
where unit 377 left it - 2.4 open, the four new variants capped at the 30-second fallback and
refused above it, which is the safe direction. That is not a stop and not the next unit's work.
The arbiter goes to step 7, then step 8, before anything else.*

**R14 - nothing beyond the criterion.** A criterion the tree already meets is **asserted, not
rebuilt**. This is the ruling that matters most tonight: unit 378 met half of 7.5 by
measurement because the chip was already right, and said so. **Do the same here. A column that
is already at parity is a finding, not a failure, and rewriting it would be the night wasted.**

**R12 - a session rewrites its own tests**, and a rewrite asserts more than it replaced, in its
own commit, after the change it is about. **R13 - telemetry on every stage, and no new event
type where an existing writer will carry it. R11 - nothing at the radio. R19 - American.
R31 - two rulings a unit.**

**HM-DEC-018 section 2.1** - nothing personal in an event. **HM-DEC-165** - nothing red that
was green before. **FACT-004** - nothing in this unit is evidence about the radio. **CLAUDE.md
section 0.0** - a sentence on the screen is a claim, and a refusal says the true reason.

### And the stop is one inch away, as it always is

`PHASE_PLAN.md` section 6: *anything that would change what goes on the air, or what keys -
`MOVE: stop`.* **R40 licenses reading a log record and moving a number on a page. It licenses
nothing on a send path.** This step does not compose, does not arm, does not key and does not
touch a modulator, a demodulator, an RSID burst or a cap. **If you find that reaching a
criterion needs a change at a composer, an `Arm`, a `PttOn`, the RSID path or the Olivia
variant gate - stop, record it, hand it back.** An achievement is a number on a page. It is
never a reason to change what goes out.

### The first is mine: what *exactly as an FT8 contact earns them* means

**Author's, overrulable.**

1. **The bar is parity with FT8, measured side by side - not a full set of five records.**
   8.2's words are *exactly as an FT8 contact earns them*, and the only honest way to check an
   *exactly as* is a comparison. **Task 1 builds one contact three times over** - the same
   callsign, the same grid, the same band, the same dial, the same times - **differing only in
   the mode pair**: `MODE=FT8`; `MODE=PSK` with `SUBMODE=PSK31`; `MODE=OLIVIA`. Everything
   after that is read in three columns.
2. **A cell where all three columns agree is parity, including all three being nothing.**
   Measured at authoring time: `Ft8ContactLogEntry.For` writes no `STATE` for any mode, and
   `AchievementLog.Read` scores a state off `contact.State` alone. **So an FT8 contact Hamlet
   logged earns no state record either**, and a PSK31 or Olivia contact earning none is at
   parity and is **not** a gap in this step. **Prove that in task 1 rather than taking it from
   this paragraph**, and if the measurement disagrees with me, the measurement wins and you say
   so in section 4.
3. **A cell where a keyboard mode reaches less than FT8 from the same facts is the gap, and it
   is this unit's to close.** That is the whole of 8.2.
4. **A cell where reaching more would need Hamlet to invent a fact is left alone and reported**
   (§0.0). A state worked out from a callsign prefix would be a claim about where a man lives;
   `AchievementScores` already says so in its own comment at line 283 and that rule stands. **A
   record Hamlet cannot answer is absent, not guessed.** If closing a gap would require
   inventing anything, it is not a gap - it is a finding, and section 4 is where it goes.
5. **The per-contact records 8.2 names are the kinds the log can answer**: country
   (`AchievementKinds.Countries`), state (`States`), grid (`Grids`), continent (`Continents`)
   and miles (`TotalMiles`), plus the Modes badge (`Modes`) and the Hall of Fame first
   (`HallOfFame`). **All eight kinds go in the table**, `Bands` included, because a mode that
   quietly lost a band record would be found by no other column.

### The second is mine: where a gap is closed, and where nothing may be written

**Author's, overrulable.**

1. **A repair goes at the one place the fact is already derived and nowhere else** - inside
   `AchievementLog.Read`, inside `AchievementScores`, or at the single
   `Ft8ContactLogEntry.For` call site in `ContactLogEntryForStation`. **No second scorer, no
   second log reader, no second `NudgeSet`, and no mode-specific branch anywhere on the
   achievements screen.** The whole reason PSK31 and Olivia already score as well as they do is
   that units 326, 358 and 368 put the mode on the *record* and let one scorer read it; a
   branch that said *if PSK31 then* would undo that.
2. **The CQ list's mode label is named from the row and not from `IsTextOnly`.** Where task 1
   confirms `CqSnapshot.cs:74` labels an Olivia CQ as PSK31, the label comes from the one fact
   the row already carries about which mode it is - the same fact the log record is built from
   at `Psk31ContactWith` and the same one the chip and the send line were taught to read in
   unit 378 - **and an FT8-shaped row goes on saying nothing, because it does not know whether
   it was FT8 or FT4.** Nothing about how a row came to be on the list is touched (unit 335's
   section 10 rule, still in force).
3. **Nothing under `src\Hamlet.RadioEngine\Olivia\`, `\Psk31\`, `\Rsid\` or `\Transmit\` is
   opened in this unit.** If a task appears to need one of them, that is the stop in the
   paragraph above, not a judgment call.
4. **Measure before you move, and say which half was already true.** Every criterion in this
   step is a claim about something the tree may already do. A report that cannot say what the
   before was has not met 8.1, whatever else it met.

---

## 7. Status cadence

`tools/status.sh`, real clock, **after every task and every commit**, and **immediately before
each carry-forward invocation** - task 0's two and task 4's two. **Task 1's measurement is one
task**: status before it and after it, not between runs. Never compose a timestamp.

---

## 8. The tasks

### Task 0 - the record and the entry run

Append `UNIT 379 - STEP 8` to `PHASE_OUTCOME.md` with `ADVANCED: step 8`, **written with the
editor and not with a shell heredoc** (section 2). Patch-bump **1.13.65 -> 1.13.66** in
`Directory.Build.props` with its line in the version log. **Set `CURRENT_STEP: 8` and
`WORK_INSTRUCTION: 379 - ...` in `PHASE_STATUS.md`**, both at 7 and 378 (section 5). **Record
step 7 as done in `PHASE_STATUS.md` and `PHASE_OUTCOME.md`**, citing unit 378's five met
criteria and its `STATE_AFTER: done` - transcription, not judgment, and not one of your two
rulings (section 5).

**Run the carry-forward list, both invocations, before anything changes**, status written
immediately before each, and put the two counts in the outcome entry's `ENTRY:` line. Unit 378
left it at **app 214 of 214, engine 150 of 150**. A red here is not yours; name it and go on.
**If either invocation dies of `InvalidProgramException: You have caused dispatcher loop`
before any assertion, that is unit 375's item 3 - re-run it once and record both attempts.**

**Also run, by name and outside the carry-forward list, the six achievement types section 5
names** - `TheAchievementsPageTests`, `TheAchievementsPageClicksInTests`,
`TheAchievementsScreenTests`, `TheOliviaRecordsAppearTests`, `ThePsk31RecordsAppearTests`,
`TheBadgesCountContactsTests` - **in one filtered invocation, and record the count.** None of
them is on the carry-forward list, so task 0 is the only moment you can learn whether a red you
meet later was yours. Unit 378 found an inherited red that had gone unnoticed for three units
exactly because nobody had done this.

**Drop candidate:** none.

### Task 1 - the trace: what each mode earns today, in three columns

**This task builds nothing, changes no source file, repairs nothing and asserts nothing about
the product.** One `[Fact]` named `Unit379Trace` in `tests\Hamlet.App.Tests\ViewModels\`, run
by name. **This task is criterion 8.1** and the report's section 3 leads with its tables.
Everything in it is composed on the development machine - **computed, not seen** (FACT-004).

Print, in this order:

1. **The one contact, three times over.** Build one `AdifContact` - one callsign with a
   knowable DXCC entity and a continent, one `GRIDSQUARE` far enough from the operator's to put
   real miles on the board, one `BAND`, one `MY_GRIDSQUARE`, start and end times - and then
   three copies differing **only** in the pair: FT8; `PSK`/`PSK31`; `OLIVIA`. For each, print
   what `AchievementLog` reads back out of it: `Entity`, `Continent`, `Band`, `Mode` (**by
   name, and `null` where nothing matched**), `Grid`, `State`, `Miles`.
2. **The eight kinds, three columns.** `AchievementScores.WorkedIn` for every one of
   `AchievementKinds.All` on each of the three logs, and `FirstsEarned` for each, and the score
   and the total each yields with the shipped points file. **Mark every cell where PSK31 or
   Olivia differs from FT8.** That marked set is exactly what task 2 has to close, and **a
   table with no marked cells is a legitimate and complete answer to 8.1** - say so plainly if
   that is what you measure.
3. **What Hamlet's own record carries, which is a different question.** Drive
   `MainWindowViewModel.ContactLogEntryForStation` over (a) a real PSK31 conversation, (b) a
   real Olivia channel at a variant, and (c) an FT8 contact, each on a known dial, and print
   **every field of the `AdifContact` it returns, for all three**. Name every field that is
   filled for one mode and null for another. **The scorer can only read what the record
   carries**, so a gap here and a gap in item 2 are different repairs.
4. **The Modes badge and the Hall of Fame, before.** On each of the three logs:
   `AchievementCategory.For(AchievementKinds.Modes, page)` - every card title verbatim, which
   are `Earned`, the count line and `AchievementBadges.ModesToWork` - and the same for
   `AchievementKinds.HallOfFame`. **Print the card titles verbatim; a title is a sentence Tim
   reads.**
5. **The quill, row by row.** Over a PSK31 fixture and an Olivia fixture, and an FT8 row from
   the same station for comparison: `Nudge`, `NudgeTip`, `NudgeReasonLine`, `NudgeEarnsLine`
   and `NudgePreview.HasPreview` on every row. **Say whether a keyboard-mode row's quill is the
   same kind with the same words as the FT8 row's, and name every difference.** Section 5 says
   both row kinds reach `MarkIfItOpensSomething`; prove it or refute it.
6. **The CQ list the achievements page is handed.** `CqSnapshot.From` over a decoded list
   holding an FT8 CQ, a PSK31 CQ and an Olivia CQ: **every `CqCall` with its `Callsign`,
   `Grid`, `HeardUtc` and `Mode`, verbatim.** Section 5 predicts the Olivia call comes back
   labelled `PSK31`. **Print what you find, whatever it is, and if the prediction is wrong say
   so - that is a mismatch against this instruction and section 4 wants it.**
7. **The page's totals, before.** An `AchievementsViewModel` built the way `OpenAchievements`
   builds it (records, operator grid, points) on each of the three logs: the eight badge scores
   and the total, three columns.

**Drop candidate:** none. **Every later task is spent against this table**, and a unit that
skipped it would be guessing at what it had repaired.

### Task 2 - 8.2: what a logged PSK31 contact and a logged Olivia contact earn

**Close every marked cell from task 1's tables, and nothing else** (R14, section 6 ruling 1).
One commit per concern, each naming the cell it closes.

Then assert 8.2 in one named type of this unit's - **the two contacts driven all the way
through, and not a hand-built log**: a PSK31 conversation and an Olivia channel, each logged
through the path a press of Log actually takes (`ContactLogEntryForStation`, `ContactLogStore`,
`RefreshWorkedBefore`), each then read back through `AchievementLog` and `AchievementScores`,
**asserted against the FT8 contact's own numbers rather than against numbers typed in the
test.** The assertion is *the same as FT8*, not *equal to 7*.

**If nothing was marked, this task is the assertion alone and the commit says so.** That is
R14, and it is the likeliest honest outcome for at least one of the two modes.

**Every red this causes in the six achievement types is handled under R12, in its own commit
after the change, asserting more than it replaced.** `TheAchievementsPageTests.TheFixtureLogs
ScoresMatchTheHandComputedArithmetic` is hand-computed arithmetic: **if it moves, the
arithmetic in it is recomputed and shown in the report - never loosened, never deleted.**

**Drop candidate:** none.

### Task 3 - 8.3 and 8.4: the quill, and the page

**8.3** - the quill on a PSK31 or Olivia row means what it means on an FT8 row, **from the same
nudge**. Assert what task 1 measured true; repair only what it measured short. **Where task 1
confirms `CqSnapshot` labels an Olivia CQ `PSK31`, that is repaired here** at the one site,
from the row's own mode (section 6 ruling 2 item 2), with the FT8-shaped row still claiming no
mode. Assert the label for all three row kinds.

**8.4** - the scores and the total **move** on the achievements page for those two contacts:
the page read before the contact and after it, the same view model built the same way, with the
difference asserted. **`TheAchievementsPageTests` green** - 8.4 names it.

**Drop candidate, and it is the unit's:** **the Olivia half of 8.4's before-and-after on the
page.** If the night runs long, assert the page moving on the PSK31 contact, print the Olivia
figures from task 1's table without a second drive, mark **8.4 partial** and say exactly that
in section 4. **Nothing else in this unit may be dropped** - 8.1's table, 8.2's parity
assertion and 8.3's quill are each the whole of a criterion.

### Task 4 - the exit run, the record and the report

Exit carry-forward, **both invocations, one build each**, status written immediately before
each, name for name against task 0's counts. **Name every difference and say whether it is an
addition of this unit's or a regression** (HM-DEC-165). Re-run the six achievement types from
task 0 in the same filtered shape and compare that count too.

Add this unit's own guard names to `docs\carry-forward-tests.txt` **by type and method with
their paragraphs**, in the same commit as the test each names, and change the human-readable
list underneath to match. Write the outcome entry's `EXIT:`, `MEASURED:` and
`ACCOMPLISHED_FINAL:` lines. Write `output.md` per section 12 and **run
`./tools/arbiter/validate-output.bat output.md`** (section 2). Push.

**Drop candidate:** none.

---

## 9. Parked - do not touch, do not raise

- **2.4, the five consecutive green rounds.** R43 defers it. Not this unit's.
- **The four Olivia variants with no timing row, and the 30-second fallback.** R43 ruled it
  deferred and the fallback the safe direction. **Do not write a timing row.** Unit 378 already
  measured which five canned line-and-variant pairs it refuses; that measurement is in
  `output.md` and `PHASE_OUTCOME.md` and it does not need making again.
- **Step 3's visibility telemetry.** It waits on this step finishing. **Do not start it**, and
  do not add a visibility event to any row in this unit.
- **Step 6's band and step 7's canned list and hover.** Both closed. Do not re-measure, do not
  re-argue, do not tidy.
- **The two inherited reds** - `ViewTestsActThroughControlsTests.NoViewTestWritesAProperty
  AControlOwns` and `TheOliviaMoveUpTests.ItIsNotOfferedOnAGuessedYourTurn`. **Report both.
  Repair neither**, unless a task of yours edits the file the first one names.
- **The `RULES_AT` id-scheme split.** `tools/status.sh` writes it as a literal and `tools\` is
  not yours.
- **The nineteen older carried asks.** Verbatim in section 4, answered by none.

---

## 10. What not to do

- **Do not touch a send path.** Not a composer, not an `Arm`, not a `PttOn`, not a cap, not an
  RSID burst, not the Olivia variant gate. Section 6's stop paragraph says what to do instead.
- **Do not invent a fact to make a number move** (§0.0). No state from a callsign prefix, no
  grid from a guess, no entity from anything but the callsign, no miles without two grids. A
  record Hamlet cannot answer is **absent**.
- **Do not write a second scorer, a second log reader or a mode branch on the achievements
  screen** (section 6 ruling 2 item 1).
- **Do not loosen a test to make a criterion pass.** `PHASE_PLAN.md` section 6: *a must-pass
  missed by a little - ship, report, `partial`, move on. Never loosen a test.*
- **Do not rebuild what the tree already does** (R14). The likeliest shape of a wasted night
  here is a unit that re-implements PSK31 mode counting that units 326, 358 and 368 already
  landed.
- **Do not run the suite** (HM-DEC-155). Two invocations, one build each, plus your own named
  types.
- **Do not repair anything this instruction got wrong about the tree** - report it (section 5).
- **Do not delete a file.** `PHASE_PLAN.md` section 6: empty it, comment it, list it.
- **Do not spend more than two rulings** (R31). Both are spent in section 6.

---

## 11. Committing and pushing

**One commit per task**, with two exceptions: **task 2 takes one commit per marked cell it
closes**, each naming the cell, **and each R12 rewrite is its own commit after the change it is
about.** A carry-forward line edit goes in the same commit as the test it names. Push once, at
the end, after task 4's runs are green or their reds are named.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the owner
should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

**The ordering block first. `validate-output.bat` refuses a report without it.** Fill every
line from what you measured - a line that is the same every unit is furniture.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Step 0 partial, step 1 done, step 2
   partial on 2.4 alone and deferred by R43, step 6 done by R42, step 7 done by unit
   378, steps 3, 4 and 5 not started with step 3 waiting on this one. This is the
   first unit ever spent on step 8.
B. Step 8 - keyboard modes earn achievements. 8.1 <met|partial|not>: <n> of the eight
   kinds measured for PSK31 and for Olivia against FT8 from a fixture log, <n> cells
   differed. 8.2 <met|partial|not>: a logged PSK31 contact earns <what>, a logged
   Olivia contact earns <what>, against the FT8 contact's own numbers. 8.3
   <met|partial|not>: the quill <was already the same|was repaired>, the CQ list's
   Olivia label reads <verbatim>. 8.4 <met|partial|not>: the total moved <before> ->
   <after> on <which> contact, TheAchievementsPageTests <n> of <n>.
C. The report last. Section 4 raises <N> items on top of the carried twenty-one, and
   <none of them is | item <k> is> in the way of a criterion in B. Unit 378's item 2
   came off the queue - R43 had already ruled it - and its items 1, 3 and 4 stay on.
```

```
UNIT:       379 - <complete|stopped> at task N of 5 - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   step 8, criteria <the ids you actually moved>
NUMBER:     of the eight achievement kinds, those a PSK31 contact reaches:
            <before> of 8 -> <after> of 8; and an Olivia contact: <before> -> <after>
DRIFT:      none
```

**Section 3 must lead with task 1's three-column table** - the eight kinds, FT8 beside PSK31
beside Olivia, before and after, **every differing cell marked** - **because that table is what
makes 8.1 a measurement and 8.2 a comparison rather than an adjective.** Then the record table:
every field of the `AdifContact` Hamlet builds for each of the three modes, and which are null
for one and not another. Then the Modes and Hall of Fame cards, titles verbatim, before and
after. Then the quill table, row kind by row kind. Then the CQ list's calls verbatim with their
`Mode` field, before and after. Then the page's eight scores and the total, before and after,
for each contact. Then the carry-forward counts before and after, and the six achievement
types' count before and after.

**Section 2 tells Tim in one paragraph what is different**, in his terms: not that a scorer was
wired, but that **an evening on PSK31 or Olivia now counts for what an evening on FT8 counts
for - the mode on his badge, the first one in the Hall of Fame, the country and the grid and
the miles on the board - and what already counted before tonight, which he never knew.** Say
plainly which half was already true. Every claim computed, not seen (FACT-004), and say in one
line that no port was opened and nothing was keyed.

**Section 4:** your own items first, most-blocking first, each saying plainly whether it wants
a ruling or is a finding - a note is not a ruling request.

**If any record stayed empty because Hamlet would have had to invent the fact, that is a
finding and not a ruling request** - section 6 ruling 1 item 4 already settles it. Name the
record, the mode and what Hamlet would have had to guess.

**If you found that reaching a criterion needs a change on a send path, that is your first item
and it wants a ruling** - it is `PHASE_PLAN.md` section 6's first stop, and nothing in this
unit licenses it.

Then the carried queue verbatim per HM-DEC-139: **unit 378's items 1, 3 and 4; unit 377's item
4; unit 376's items 3, 4 and 5; unit 375's items 3 and 4; unit 374's item 3; unit 373's item 2;
unit 372's items 4 and 7; unit 371's five; unit 369's four - twenty-one**, with one line saying
that unit 378's item 2 came off it because R43 had already ruled it.

---

```
ARBITER-DECISION
STEP: 8
APPROACH: measure which achievements a PSK31 and an Olivia contact reach today from a fixture log, then connect the Modes badge, the Hall of Fame firsts, the per-contact records and the quill so a keyboard-mode contact earns exactly what an FT8 contact earns
MOVE: continue
WHY: step 8 is the only step whose entry is open - unit 378 closed step 7 on all five criteria and the judging session returned done, step 6 is closed by R42, 2.4 is deferred by R43, and steps 3, 4 and 5 wait behind this one - and R39 and R43 name the order in words: step 7, then step 8, before anything else. Step 8 has zero units spent, the loop test finds nothing resembling this approach in any entry of PHASE_OUTCOME.md because no unit has ever been permitted to attempt it, and the tree measured at authoring time says a large part of the step may already be met, which is why the unit is aimed at the measurement first and the repair second.
STATE: not started
DECIDED: author's, overrulable, two, both transcribed in work instruction 379 section 6. (1) What exactly as an FT8 contact earns them means: the bar is parity measured side by side and not a full set of five records - one contact built three times over, differing only in the mode pair, read in three columns; a cell where all three agree is parity including all three being nothing, and measured at authoring time Ft8ContactLogEntry.For writes no STATE for any mode, so a keyboard-mode contact earning no state record is at parity rather than short; a cell where a keyboard mode reaches less than FT8 from the same facts is the gap and is this unit's to close; a cell where reaching more would need Hamlet to invent a fact is left alone and reported, because a state worked out from a callsign prefix is a claim about where a man lives; and the per-contact records 8.2 names are the log-answerable kinds - countries, states, grids, continents, total miles - with modes and hall of fame beside them and bands in the table too. (2) Where a gap is closed and where nothing may be written: a repair goes at the one place the fact is already derived - AchievementLog.Read, AchievementScores, or the single Ft8ContactLogEntry.For call site in ContactLogEntryForStation - and never a second scorer, a second log reader, a second NudgeSet or a mode branch on the achievements screen; the CQ list's mode label, which CqSnapshot.cs line 74 writes as PSK31 for any text-only row and therefore labels an Olivia CQ PSK31 on the achievements page, is named from the row's own mode with an FT8-shaped row still claiming nothing; nothing under src\Hamlet.RadioEngine\Olivia, \Psk31, \Rsid or \Transmit is opened in this unit, and a criterion that appears to need one of them is the stop and not a judgment call; and every criterion is measured before it is moved, because a report that cannot say what the before was has not met 8.1 whatever else it met.
LICENCE: PHASE_PLAN.md R40 - the owner's ruling of 2026-09-21 that a PSK31 or Olivia contact earns exactly what an FT8 contact earns, measured first and then connected, which is this unit's whole subject; R39 and R43 which order step 8 next and defer 2.4 and the timing rows; R42 closing step 6 and unit 378 closing step 7, which together opened this entry; R31 and section 6 - a mechanism, a path, a label, a table's shape and the reading of a criterion are the arbiter's and never a stop; PSK31 plan R11, R12, R13, R14, R19; CLAUDE.md 0.0, 0.2, 0.5; HM-DEC-018 section 2.1, HM-DEC-139, HM-DEC-155, HM-DEC-165; FACT-004
ACCOMPLISHED: When Tim works a man on PSK31 or on Olivia and logs him, that evening counts for exactly what the same evening on FT8 would have counted for - the mode goes on the Modes badge, the first contact in that mode lands in the Hall of Fame, and the country, the grid, the continent and the miles move the numbers an FT8 contact moves. The quill on a keyboard-mode row means what it means on an FT8 row, and the achievements page stops telling him a station calling CQ on Olivia is calling on PSK31. Nothing is invented to make a number move: a record Hamlet cannot answer stays absent, for these two modes exactly as for FT8. And he is told, record by record, how much of this was already true before tonight.
ADVANCES: step 8, criteria 8.1, 8.2, 8.3 and 8.4 - the whole step, which has zero units spent. It also clears step 3's entry, which reads step 8 done, and through step 3 it clears step 4's; it does not advance 2.4, which R43 defers.
END-ARBITER-DECISION
```
