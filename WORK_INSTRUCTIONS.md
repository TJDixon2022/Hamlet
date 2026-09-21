# Work instruction 375 - the two headless flakes are measured and settled, and the list runs green five times running

**Step 2 of the hardening phase, first unit aimed at it.** Five tasks. **Nothing in this
unit changes the application**, unless task 1's measurement says a test is red because the
application is wrong - and in the one place that could happen, section 6 says stop rather
than repair. Measure first: task 1 builds nothing.

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

*All four were checked against the tree at authoring time and all four hold.*

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite; only this unit's names and `docs\carry-forward-tests.txt` as its
top comment says - **two invocations, one build each**, status written immediately before
each. Never background and poll.

**This unit runs more invocations than any unit in this phase has** - twenty in task 1, up
to twelve more in task 4. **Every one of them is filtered and foregrounded**, one build
each, and every one is a single `dotnet test` on one project. That is not a suite and it is
not a poll. **A loop goes in a script file** (`;` is refused in a compound command);
`.run-unit\unit372-flake.sh` is the shape, six lines, and it already does exactly this for
one of the two names.

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; `-m` more than once for a multi-line commit. **Write this repository's files as
UTF-8; a PowerShell `>` redirect writes UTF-16 and the launcher cannot read it.**

- **`git worktree add` is refused**, inside the repository root and outside it. No task here
  asks for one.
- **Shell output redirection (`>`) is refused to every path**, including the scratchpad.
  Write files with the editor, not with a heredoc or a redirect. **A test run's output is
  read from the console**, or piped to `grep` in the same command as
  `.run-unit\unit372-flake.sh` does.
- **A compound command with `;` or a second operation is refused.** A loop goes into a script
  file first. **Python runs here** - `python file.py` from the scratchpad worked in units
  361, 362, 371 and 374, contrary to older orders (unit 371's item 6, answered here and off
  the queue).

## 3. Asks still outstanding

Carried per HM-DEC-139, **verbatim in section 4**. Unit 374 raised three, **none of which
wanted a ruling**: the app count of 207 rather than 206 (self-resolving, and now 211), unit
373's carry-forward name missing from the file's own project list (acted on), and
`tools/run-carry-forward.sh` still not matching the list (not repaired).

**Unit 374's item 1 is absorbed into section 5 below** - the expected count is stated there -
and **its item 2 is closed**, the unit having written the name into the list itself. **Its
item 3 stays**, with unit 373's item 2, unit 372's items 4 and 7, unit 371's five and unit
369's four: **thirteen carried, and this unit answers none of them.** Unit 372's item 4 -
the flake measurement - is not an ask but a finding recorded *for this unit*, and section 5
takes it up as evidence.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet holds what it has - the hardening phase, five steps of banked
            screen, record and test work that needs neither the radio nor the owner.
UNIT GOAL:  The two headless flakes stop being rumors. Each is run ten times, its
            cause named, and then either made deterministic on the test's side or
            quarantined off the carry-forward list with the cause stated - and then
            the list runs green five times in a row.
ADVANCES:   step 2, criteria 2.3 and 2.4 - both must-pass, both untouched by any unit.
DRIFT:      none.
```

**The count today.** Step 0 `partial` (0.1 met on unit 369's completed negative, ruled in
work instruction 372 section 6; 0.2 to 0.5 met), 2 units spent. **Step 1 `done`**, 6 units
spent, closed by unit 374. **Step 2 open: 2.1 met by unit 374**, 2.2, 2.3 and 2.4 open, and
**no unit has yet been aimed at this step** - unit 374 took 2.1 as its second half. Steps 3,
4 and 5 `not started`, 0 units each.

**Why 2.3 and 2.4 and not 2.2.** Section 9 parks 2.2 and says why in full: finishing it
would put an RSID announcement on the air for four Olivia variants Hamlet refuses to announce
today, and that is one of `PHASE_PLAN.md` section 6's three stops. **It is held for the
owner, and this unit does not touch it.** So **step 2 will not close in this unit**, and the
report must say so plainly rather than rounding up.

**Why these two first anyway.** 2.3 is the criterion every later step's evidence rests on.
Steps 3 and 4 both end in a carry-forward run, and **a flaking name on that list makes every
one of those runs unreadable** - unit 372 saw the Stop name red in three of seven runs, unit
373 in one of four, unit 374 in none of five, and each of those units had to spend a
paragraph saying so. `PHASE_PLAN.md` R35 is blunt about it: *a flaking test on the
carry-forward list is worse than none.*

---

## 5. Verify this instruction against the tree

Every line below was read at authoring time. **Report every mismatch in section 4 and
section 1; repair nothing but this unit's.**

**The two names, and where they live.**

- `tests\Hamlet.App.Tests\Views\TheStopIsAlwaysOnScreenTests.cs` - **five `[AvaloniaFact]`
  names.** The flake is
  **`KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns`**, and the assertion that
  fails is at **line 231**: `Assert.Equal(new[] { KeyOn, CwStop, PttOff }, whileRunning)`,
  where `whileRunning = Frames(scene.Port)` is read **immediately after `Click`** and
  **before `await running`**. The other four names have never been red.
- `tests\Hamlet.App.Tests\Views\TheTestsStayOffTheNetworkTests.cs` - **five `[Fact]` names**:
  `AViewModelATestBuildsHasTheNetworkDenied` (51), `BuildingAViewModelMakesNoHttpClient`
  (75), `ThePlainFixtureTakesGeneralFromTheFixedAnswer` (123),
  `TheLicensedFixtureTakesTheFixedAnswerToo` (148) and
  `The354LayoutReadsTheSameNumbersTwiceRunning` (174). **Which of the five flakes, and how
  often, is not recorded anywhere in the tree** - `PHASE_PLAN.md` R35 names the type from
  unit 365's report and nothing since has measured it. Task 1 measures it. **If it is green
  ten times out of ten, that is the answer**, and section 6's first ruling says what follows.

**What is already measured, and is evidence rather than rumor.**
`docs\unit372-flake-measurement.md`, written by unit 372 for this unit:

- The whole type, seven runs: **5, 5, 4, 5, 5, 4, 4 of 5** - three of seven red. The single
  name alone, seven runs: **two of seven red**.
- It always fails the same way, verbatim in that file: expected `KeyOn, CwStop, PttOff`, got
  those **plus a second `CwStop` and a second `PttOff`** - *the abort pair is on the wire
  twice.*
- The two candidates named there and **not chased**: the click's own `StopNow`, and the
  transmit sequence's unkey as it comes off the token. **Which of the two was not
  determined**, deliberately.
- Its own conclusion: *the read races the sequence's own teardown, so the count the test
  asserts is a count taken at a moment that is not fixed. It is a timing assertion written as
  an equality.*
- **Nothing in that file is evidence about the radio** (FACT-004): a `FakePort` and a
  `FakeSink` on a headless window. No device was opened and nothing was keyed.
- Unit 374 measured the same name **0 of 5 red** in its own session. **Expect that: a flake
  that does not flake at you is the hard case, and section 6's first ruling covers it.**

**The second fault, which is not a name and cannot be quarantined as one.** Unit 374 counted
Avalonia's headless `InvalidProgramException: You've caused dispatcher loop`, **after 1 ms
and before any assertion**, in **2 of 4 full app carry-forward invocations** - two names on
one attempt (`ThePsk31OfferTests.TheOfferIsOneButtonAndItIsTheOneTheEngineNamed`,
`ThePowerIsOfferedTests.TheOfferRendersAtHalfAndNothingMirrorsTheUsbModLevel`), a different
single name on the re-run (`ThePowerIsOfferedTests`), neither on the two exit runs. Unit 373
lost `TheStopIsAlwaysOnScreenTests.AtEachOf354sNineSizes...` to it once. **It moves between
names**, which is what says it is the session and not the test. Section 6's second ruling
says how task 4 counts it.

**The lists.**

- `docs\carry-forward-tests.txt` - **two command lines, one per project**, at the top of the
  file. **Both flaking types are on the app line as whole types**
  (`FullyQualifiedName~TheStopIsAlwaysOnScreenTests`,
  `FullyQualifiedName~TheTestsStayOffTheNetworkTests`), so **taking one name off means
  replacing that type's entry with its surviving names in the type-and-method form**, which
  `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint` on the same line already shows.
  Underneath the lines is a human-readable list by project, and a paragraph per addition.
- **The known-red block is at line 139**, *known reds, NEVER on the list above*:
  `CwAdjudicationTests.ASpeedChangeInRealisticAudio`, the 51 CW cases in
  `docs\unit239-failing-set.txt`, the `Ft8Sharp.Deep.Tests` whole-type-list tripwire,
  HM-OPEN-088's ten, two `TheAchievementsScreenTests` names,
  `TheFitGuardAsksAboutTheGridTheSendIsOnTests` (engine, not app) and unit 320's item 46.
  **None of those is yours.**
- `docs\carry-forward-dropped.txt` - **this is not a quarantine list and must not be used as
  one.** It says in terms: *NOTHING HERE IS DELETED, RETIRED OR KNOWN RED. Every name below
  is a test that was green when it came off the list.* Section 6's first ruling names the
  file a quarantined name goes in instead.

**The counts to expect.** Unit 374's exit run: **app 211 of 211, engine 146 of 146**, both
invocations, one build each. The app total rose from 206 to 211 across units 373 and 374 by
two additions to the line, not by any test multiplying - unit 374's item 1 explains it and
it is not a finding again. **The app invocation takes about 2 m 20 s; the engine invocation
is the smaller of the two.**

**The version.** `Directory.Build.props` line **956**: `<Version>1.13.61</Version>`.

**Two disagreements the reload measured, and one of them is yours.**

- **Yours, and repair it in task 0.** `PHASE_STATUS.md` and `PHASE_OUTCOME.md` both carry
  `STEP: 2 | not started`, while the same line's own prose in `PHASE_STATUS.md` says *Unit
  374: 2.1 MET* and `PROJECT_STATUS.md` says *Step 2 partial - 2.1 met*.
  `tools\arbiter\outcome-read.bat` therefore prints step 2 as **not started, 0 units spent**,
  which is what the next arbiter will read. **The state token is wrong, the prose is right.**
  Set both to `partial` in task 0 and say in the report that you did and why. This is the
  phase's own record, which your task 0 writes anyway.
- **Not yours.** `PROJECT_STATUS.md` `RULES_AT` reads `HM-DEC-165 (2026-09-19)` while
  `CLAUDE.md` section 1 holds `CPS-DEC-0165` - the id-scheme split, carried in
  `PHASE_PLAN.md` section 7. Report it, do not repair it.
- At authoring time HEAD was **8cfc9208** on `main`, with `PHASE_OUTCOME.md`,
  `PHASE_STATUS.md` and `RUN_LEDGER.md` modified, `SESSION.lock`, `WORK_INSTRUCTIONS.md` and
  `output.md` deleted, and `.run-unit\watched.rc` untracked. **Six uncommitted at the root.**

**What 2.2 is made of, measured so that nobody has to look.** `assets\data\rsid-codes.json`
exists and carries tone sequences for codes **72, 73, 74 and 75** plus the `squares` and
`indices` tables; `data\rsid\rsid-codes.json` has the four codes and **no sequences for
them**; `RsidCodes.FilePath` names the old path; the csproj embeds it at **line 68**;
`RsidCodes.Parse` reads neither table. **Section 9 parks all of it.**

---

## 6. Rulings in force

Transcribed in full. **Do not re-argue any of these.**

### The first is mine: what settling a flake means here, and the one line it must not cross

`PHASE_PLAN.md` section 6 already rules that **a flake that cannot be made deterministic is
quarantined with its cause named, and that is meeting 2.3, not missing it**. This ruling says
how, and where the boundary is.

**1. The repair is on the test's side, and it asserts not less than it does today.** Unit
372's measurement is that line 231 reads the wire at a moment that is not fixed - the click's
frames and the sequence's own teardown race. **Fixing the moment is not loosening**: settle
the sequence before the wire is read, or assert the rule the test is really about - *the
abort reaches the wire while the transmission is still running, and nothing after it is
anything but that same abort pair* - rather than an exact snapshot of a list. **Say in the
report why what you wrote asserts more than what it replaced**, name for name, as unit 373
did for the badge. This is R12 work, in its own commit, and it is not a ruling and not an ask.

**2. The line this unit does not cross.** If task 1's measurement says the second `CwStop`
and `PttOff` are written **by the application** - that Hamlet sends the abort pair twice to
the radio - **that is what keys, and `PHASE_PLAN.md` section 6 stops there.** Do not repair
it, do not suppress it, do not make the test tolerate it quietly. **Quarantine the name with
that cause written out in full, report it as a finding wanting the owner's ruling, and say in
section 2 what it would mean at the radio if anything.** A second abort to a radio already
unkeyed is very likely harmless; *very likely* is not this unit's call to make.

**3. Where a quarantined name goes: a new file, `docs\quarantined-tests.txt`.** Not
`carry-forward-dropped.txt`, which states that nothing on it is a known red, and not the
known-red block, which is for inherited reds nobody is chasing. The new file carries, for
each name: the full type and method, **how many runs out of ten were red**, the failure
verbatim, the cause as far as it was determined, whether it is the test's timing or the
environment, and **what would take it off the list again**. **A quarantined name comes off
the carry-forward command line in the same commit**, by rewriting that type's entry into its
surviving names.

**4. And the case where it will not flake at you.** Unit 374 got 0 of 5 on this name.
**Ten runs green is a real answer and you report it as one** - but it is not licence to
leave line 231 as it is, because the measurement of *why* it is unstable is already in the
tree and does not depend on this session's luck. **Fix the moment anyway, under 1 above**,
and report both: ten of ten green, and the assertion made deterministic regardless. For
`TheTestsStayOffTheNetworkTests`, which has no such measurement behind it, **ten of ten green
means it is not quarantined and nothing is changed** - record the counts, say the plan's
premise about it was not reproduced here, and leave it on the list.

*Author's, overrulable.* It is none of `PHASE_PLAN.md` section 6's three stops: no money, no
package, and nothing that changes what the product tells the operator - and where it would
touch what keys, item 2 stops instead of deciding.

### The second is mine: how task 4 counts a run the dispatcher loop kills

Criterion 2.4 is *the carry-forward list runs green five times in a row*. Unit 374 measured
the headless `InvalidProgramException` killing **2 of 4** app invocations after 1 ms, moving
between three different names across three units. **It is not a test failing and it cannot be
quarantined by name.**

**So, for task 4 only:** a run that dies of `You've caused dispatcher loop` **before any
assertion** is a **lost run**. It is recorded with the name it landed on and the attempt
number, re-run, and it **neither counts toward the five nor breaks the streak**. **2.4 is met
by five consecutive completed runs of both invocations, green**, with **every lost run
reported by count and by name** in section 3. **If more than five runs are lost across the
attempt, 2.4 is `partial`, not met** - report the numbers, write the environment fault into
`docs\quarantined-tests.txt` in a section of its own as an environmental cause with no name
attached, and move on. **Never loosen a test, never drop a name, and never re-run a run that
died on an assertion** - that one is red, and the streak restarts.

*Author's, overrulable.* This defines what counts as a run; it does not weaken an assertion.

**That is two rulings, which is R31's limit.** The thirteen carried asks get none, and
section 9 parks the ones that will feel close.

### The phase's standing rulings

**R35 - the record's small lies.** Codes 72-75 have no tone sequence in `rsid-codes.json`;
**two app tests flake in headless runs** (`TheStopIsAlwaysOnScreenTests`,
`TheTestsStayOffTheNetworkTests`, unit 365's report): **a flaking test on the carry-forward
list is worse than none.**

**R11 - nothing at the radio.** No unit asks the operator to set a level, read a meter, or
know what ALC is.

**R12 - Tim, 2026-09-11: a session fixes its own tests and never asks the owner to approve
it.** A test a session wrote that later blocks the unit told to do the work is **the
session's to rewrite in its own commit** so it guards the rule and not the accident - and
that is not a ruling, not an ask, and not a stop. **The arbiter never puts the wording of a
test to the owner.**

**R13 - telemetry is a must-pass on every remaining step.** **This unit adds no stage and
therefore no event.** If you find yourself adding one, you have left the criterion.

**R14 - Tim, 2026-09-11: eyes on the prize.** *"We don't focus too much on pointless testing.
We remember what the phase goal is."* A unit writes the tests its criteria need and no
others; no pins, no guards for doors it is not building, no tests of tests.

**R19 - American spelling** in every operator-facing string and every instruction.

**R31 - this phase runs unattended.** Criteria by id. A done step is closed. The owner's step
ends the run. **Two rulings per unit at most.**

**Section 0.0** a sentence on the screen is a claim; a refusal says the true reason.
**Section 0.2** one click, one transmission - **nothing in this unit sends anything to a
device**; every port in it is a `FakePort`. **HM-DEC-155**, **HM-DEC-139** (the carried queue
verbatim), **HM-DEC-165** (no name green before a unit is red after it), **FACT-004**
(nothing here is evidence about the radio), **HM-DEC-018 section 2.1** (nothing personal in
an event).

## 7. Status cadence

`tools/status.sh`, real clock, **after every task and every commit**, and **immediately
before each carry-forward invocation** - task 0's two, and task 4's. **Task 1's twenty runs
are one task**: write status before the task's first invocation and after its last, not
between every run. Never compose a timestamp.

---

## 8. The tasks

### Task 0 - the record, and the step-2 token

Append `UNIT 375 - STEP 2` to `PHASE_OUTCOME.md` with `ADVANCED: step 2`. Patch-bump
**1.13.61 -> 1.13.62** in `Directory.Build.props` with its line in the version log.

**Set step 2's state token to `partial` in both `PHASE_STATUS.md` and `PHASE_OUTCOME.md`**
(section 5: the token says `not started` while its own prose says 2.1 is met, and
`outcome-read.bat` prints the token). One line in the report saying you did.

**Run the carry-forward list, both invocations, before anything changes**, and write the two
counts in the outcome entry's `ENTRY:` line. Unit 374 left it at **app 211 of 211, engine 146
of 146**. A red here is not yours; name it and go on. **If either invocation dies of the
dispatcher loop, that is the fault section 6's second ruling is about - re-run it once, and
record both attempts in the `ENTRY:` line.**

**Drop candidate:** none.

### Task 1 - the trace: twenty runs, and what each red actually is

**This task builds nothing, changes no source file and repairs nothing.** It is the whole
purchase of the unit: 2.3 says the two types are *run ten times each*, and this is that.

**1. Ten runs of `TheStopIsAlwaysOnScreenTests`, whole type, filtered, one build each.**
`.run-unit\unit372-flake.sh` is the shape - copy it into the scratchpad, change the filter to
the whole type and the count to ten. Record **per run**: the count (`n of 5`), and for every
red, **which name** and **the failure verbatim**.

**2. Ten runs of `TheTestsStayOffTheNetworkTests`, the same way.** Nothing in the tree has
ever measured this one. Record the same three things per run.

**3. For every red in either set, say which of two things it is**, because section 6's ruling
forks on it:
   - **an assertion failure** - the test ran and disagreed with the application; or
   - **`InvalidProgramException: You've caused dispatcher loop`** after about 1 ms - the
     session died and no assertion ran.

**4. For the Stop name only, and only if it is red at least once: whose second abort pair is
it?** Print the frames the click produced and the frames present after `await running`
completes, from the same run. **The question is whether the second `CwStop`/`PttOff` exists
before the sequence's teardown or only after it.** If it exists only after teardown, the test
is reading too early and it is the test's. If the click alone produces two pairs, **that is
the application and section 6 item 2 stops you** - print it, do not chase it.
**If it is green ten of ten, say so and go to task 2 anyway** (section 6 item 4).

**What this task must answer in the report, by id.** For **2.3**: how many of ten runs each
type was red; which name; the failure verbatim; the assertion-or-session split; and, for the
Stop name, whose second abort pair it is or that it could not be determined this session
because it never went red.

**Drop candidate:** none, and if the unit runs long everything else goes before this does. A
measured flake handed to the next unit beats an unmeasured repair - which is exactly what
unit 372 did for you, and why task 1 can start from numbers instead of a rumor.

### Task 2 - criterion 2.3, the Stop name

Act on task 1's answer, **per section 6's first ruling**, in a commit of its own:

- **The test's timing is the cause** (the second pair appears only once the sequence has torn
  down): **rewrite the assertion under R12** so the moment it reads is fixed, and say in the
  report, name for name, **what it asserts now that it did not assert before**. It stays on
  the carry-forward list. **Then run the whole type ten more times** and put both tens in the
  report - before and after. **Ten of ten green is what meets the criterion here.**
- **The application writes the pair twice**: **stop.** Change nothing in the abort path.
  Quarantine the name into `docs\quarantined-tests.txt` with the cause written out in full,
  take it off the app command line, and write it in section 4 as a finding wanting the
  owner's ruling.
- **It never went red in ten runs**: fix the moment anyway, report ten of ten, and say that
  the determinism does not rest on this session's luck.

**Never loosen it to make it pass.** If the only assertion that survives is weaker than
today's, quarantine instead and say so.

**Drop candidate:** none. This is the criterion the unit exists for.

### Task 3 - criterion 2.3, the network type, and the lists

- **Act on task 1's second ten**, the same three ways, in its own commit.
- **`docs\quarantined-tests.txt`**, if either name needed it: created here, in the shape
  section 6 names - full type and method, runs red out of ten, the failure verbatim, the
  cause, test-timing or environment, and what would take it off. **If nothing was
  quarantined, do not create the file**; say in the report that neither name needed it.
- **`docs\carry-forward-tests.txt`**: any quarantined name comes off the command line **in
  the same commit**, by rewriting that whole-type entry into its surviving names in the
  type-and-method form. **Add nothing that is knowingly red.** Whatever you change, **change
  the human-readable list underneath to match** - unit 374's item 2 was exactly that
  mismatch, twice in two units.
- **`TheWindowGivesUpHeightInOneOrderTests` and `TheWindowHoldsBelowItsMinimumTests` are not
  yours** and neither is anything else step 1 left behind. Step 1 is done and closed.

**Drop candidate:** none.

### Task 4 - criterion 2.4: five in a row, then the record and the report

- **Run the carry-forward list, both invocations, after the last change.** Compare name by
  name against task 0. **A red after that was green before is a regression** and sections 1
  and 4 both name it as one (HM-DEC-165).
- **Then run both invocations five times in a row**, per section 6's second ruling. Record
  **every run's two counts**, in order, and **every lost run by attempt number and by the
  name it landed on**. **Five consecutive completed runs green meets 2.4.** A red on an
  assertion restarts the streak and is named. **More than five lost runs makes 2.4 `partial`**
  with the numbers and the environment fault written into the quarantine file.
- **`PHASE_STATUS.md` and `PHASE_OUTCOME.md`:** step 2 criterion by criterion by id (R31).
  **2.1 met (unit 374), 2.3 and 2.4 as your measurements support, 2.2 untouched and held for
  the owner** - so **step 2 stays `partial`, not `done`, even if 2.3 and 2.4 both pass.**
  **Do not round up, and do not tick 2.2 on any reading.**
- Write `output.md` per section 12.

**Drop candidate: the five-run streak, and only it.** If the unit is running long, drop the
streak - not the final carry-forward run, not the record, not the report - report 2.4 `not
met` with however many runs you got, and say the next unit starts from a settled list.
**2.4 has to be run again after 2.2 lands in any case**, which is why it is the cheapest
thing in this unit to lose.

---

## 9. Parked - do not touch, do not raise

- **Criterion 2.2 - the RSID codes, the whole of it.** `assets\data\rsid-codes.json`,
  `data\rsid\rsid-codes.json`, `RsidCodes`, `RsidBurst`, `RsidDetector`, the csproj's
  embedded resource, and every test of them. **Do not swap the file, do not extend the
  parser, do not add a round-trip test.** *Here is why, measured, so that it is not
  re-litigated at three in the morning:* `OliviaModulator.Compose` at **lines 149-155**
  gates the announcement burst on `RsidBurst.TonesFor(...) is not null` and, where it is
  non-null, **composes that burst into the audio that is transmitted**. The four codes 72-75
  have no sequences today, so Hamlet refuses to announce those variants; giving them
  sequences makes it announce them. **That is a change to what goes on the air**, which
  `PHASE_PLAN.md` section 6 makes the owner's, and **the four new sequences have not been
  checked against the mode author's fixtures the way the existing four were**
  (`TheRsidBurstTests`). **It is raised to the owner in the arbiter's decision block and in
  your section 4's first line; it is not yours to resolve and not yours to work around.**
- **`PHASE_PLAN.md`'s criterion checkboxes** (unit 372's item 7). Do not tick them; do not
  raise it again. The record this phase maintains is `PHASE_STATUS.md` and
  `PHASE_OUTCOME.md`.
- **`tools\run-carry-forward.sh`** (unit 374's item 3, unit 373's item 4). It still does not
  match the list. **Run the two command lines from `docs\carry-forward-tests.txt` itself**,
  as units 373 and 374 did, and leave the script alone.
- **`ApplyBestBet`'s stale `BestBetLabel`** (unit 373's item 2), **`LearnedAlcReference.Ago()`**
  (unit 369's item 3), **the archived Olivia phase** at `docs\phase-olivia-run\` - a phase is
  never reopened - and **the `RULES_AT` id-scheme split.** Carry them; do not repair them.
- **Steps 3, 4 and 5.** The visibility events, the radio sheet, Tim's verdict. Not this unit.
- **Anything about what keys. Any package.** A package is `MOVE: stop`.

## 10. What not to do

- **No unfiltered `dotnet test`** (HM-DEC-155). Every invocation filtered, foregrounded, one
  build. **Never background and poll. Never compose a timestamp.**
- **Do not loosen a test to make a criterion pass** (`PHASE_PLAN.md` section 6). A flake is
  made deterministic or quarantined with its cause; it is never made to pass by asking less.
- **Do not touch the abort path, `StopNow`, the transmit sequence's unkey, or anything that
  writes to a port.** Section 6 item 2 is the ruling and it says stop.
- **Do not put a quarantined name in `carry-forward-dropped.txt`.** That file says nothing on
  it is a known red.
- **Do not add an event, and do not add a test for a door you are not building** (R13, R14).
- **Do not delete a file.** Empty it, comment it, list it (`PHASE_PLAN.md` section 6).
- **Do not chase a red that is on the known-red block at line 139.** None of those is yours.
- **Report mismatches; repair nothing but this unit's. Write American. Write files as UTF-8.**

## 11. Committing and pushing

**One commit per task**, on `main`, message naming the unit and the task. **An R12 rewrite
goes in its own commit**, separate from any list change it causes. **A quarantine and the
command-line edit that follows it go in one commit**, so the two never disagree in the
history. Push once, at the end, after task 4's runs are green or their reds are named.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

**The ordering block first. `validate-output.bat` refuses a report without it.** Fill every
line from what you measured - a line that is the same every unit is furniture.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Step 0 partial, step 1 done and
   closed by unit 374, step 2 open with 2.1 banked, steps 3, 4, 5 not started
   and all three waiting on step 2.
B. Step 2 - the record says what was true. 2.3 <met|partial|not>: the Stop name
   was red <n> of ten and the network type <n> of ten, and each was <made
   deterministic|quarantined|left alone> - say which and why. 2.4 <met|partial
   |not>: <n> consecutive green runs of both invocations, <n> runs lost to the
   dispatcher loop. 2.1 carried met. 2.2 NOT ATTEMPTED - parked for the owner.
C. The report last. Section 4 raises <N> items on top of the carried thirteen,
   and <none of them is | item <k> is> in the way of 2.3 or 2.4 - and step 2
   does not close in this unit, because 2.2 is the owner's.
```

```
UNIT:       375 - <complete|stopped> at task N of 5 - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   step 2, criteria <the ids you actually moved>
NUMBER:     the Stop name, red runs out of ten: 3 of 7 for unit 372 -> <yours>
DRIFT:      none
```

**Section 3 must lead with task 1's twenty-run table** - two types, ten runs each, the count
per run, the name of every red and whether it was an assertion or the dispatcher loop -
**because that table is the criterion**, and it comes before any prose about a repair. Then
the second ten for whichever name task 2 rewrote, beside the first. Then the wire frames from
task 1 item 4 if the name ever went red. Then task 4's five runs, in order, with the lost runs
counted. Then the carry-forward counts before and after.

**Section 2 tells Tim in one paragraph what is different**, in his terms: not that a test was
fixed, but that **the list which proves nothing broke can now be believed** - and, if section
6 item 2 was reached, **that Hamlet may be sending its abort twice and that question is his.**
Every appearance claim is computed, not seen (FACT-004).

**Section 4:** your own items first, most-blocking first, each saying plainly whether it wants
a ruling or is a finding - a note is not a ruling request. **Your first item is 2.2**: say
that it is parked, that finishing it would announce four Olivia variants on the air that
Hamlet refuses to announce today, that the arbiter held it for the owner, and that **step 2
cannot close until he rules.** Then the carried queue verbatim per HM-DEC-139: **unit 374's
item 3, unit 373's item 2, unit 372's items 4 and 7, unit 371's five and unit 369's four -
thirteen**, with one line saying that unit 374's item 1 was absorbed into this instruction's
section 5 and its item 2 was closed by unit 374 itself.

---

```
ARBITER-DECISION
STEP: 2
APPROACH: run the two named headless flakes ten times each, repair them in the test under R12 or quarantine them into a named non-carry-forward list with the cause stated, and run the carry-forward list green five times in a row
MOVE: continue
WHY: step 2 has had no unit aimed at it, and 2.3 is the criterion every later step's evidence rests on - steps 3 and 4 both end in a carry-forward run, and the Stop name was red in 3 of 7 runs for unit 372, 1 of 4 for unit 373 and 0 of 5 for unit 374, so every run of that list has had to be argued about for three units.
STATE: partial
DECIDED: author's, overrulable, three - two given to the unit in work instruction 375 section 6, and one scoping decision the unit is told not to touch. (1) Settling a flake means fixing the moment the test reads, not asking less: the repair is on the test's side under R12 and must assert more than it replaced, ten green runs do not excuse leaving line 231's unfixed moment alone, a quarantined name goes in a new docs\quarantined-tests.txt and comes off the carry-forward command line in the same commit rather than into carry-forward-dropped.txt which states nothing on it is a known red - and if the measurement shows the second abort pair is written by the application rather than by the test reading too early, that is what keys and the unit stops, quarantines with the cause named, and hands it to the owner. (2) For criterion 2.4 only, a run killed by Avalonia's headless InvalidProgramException 'You've caused dispatcher loop' before any assertion is a lost run: recorded with the name it landed on, re-run, counting neither toward the five nor against them, because unit 374 measured it killing 2 of 4 app invocations and moving between three names across three units, so it cannot be quarantined as a name; five consecutive completed green runs meet 2.4, more than five lost runs make it partial, and a red on an assertion restarts the streak. (3) THE SCOPING DECISION, AND IT IS THE OWNER'S TO OVERRULE FIRST: criterion 2.2 is not attempted and is parked. OliviaModulator.Compose lines 149-155 gate the announcement burst on RsidBurst.TonesFor being non-null and compose it into transmitted audio, so giving codes 72-75 their tone sequences makes Hamlet announce four Olivia variants on the air that it refuses to announce today, and those four sequences have not been checked against the mode author's fixtures the way the existing four were. That is a change to what goes on the air - PHASE_PLAN.md section 6's stop - so no unit takes it without Tim's word, and step 2 cannot close until he rules. The phase was not halted for it because 2.3 and 2.4 are a full night of work that touches nothing outside the fence, and halting would have spent the night Tim asked to use.
LICENCE: PHASE_PLAN.md R35, R31 and section 6 - a test's shape and a mechanism are the arbiter's and never a stop, and a flake quarantined with its cause named IS meeting 2.3; PSK31 plan R11, R12, R13, R14, R19; CLAUDE.md 0.0, 0.2; HM-DEC-018 section 2.1, HM-DEC-139, HM-DEC-155, HM-DEC-165; FACT-004
ACCOMPLISHED: The list Hamlet runs to prove nothing broke can be believed. Two tests that were red some nights and green others are each run ten times, and each is either made to check the same thing every time or set aside with the reason written down - so when a later unit says the list was green, that sentence means something. And if the reason one of them wavers turns out to be that Hamlet sends its stop command to the radio twice, that is written down and handed to Tim rather than quietly smoothed over.
ADVANCES: step 2, criteria 2.3 and 2.4 - both must-pass, both untouched by any unit, and 2.3 is what makes every later step's carry-forward evidence readable. Step 2 does not close here: 2.2 is held for the owner, for the reason in DECIDED (3).
END-ARBITER-DECISION
```
