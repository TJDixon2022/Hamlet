# Work instruction 386 - the list proves itself five times over a tree that has stopped moving, and the false Olivia row gets its correction

**Step 2 of the hardening phase, and the two criteria that are left of it: 2.4 and 2.6.**
Six tasks. **This unit writes no application code at all** - not one file under `src`,
and after task 2 not one file under `tests`, `assets` or `data` either. That is not a
restriction it works around; **it is the measurement**. 2.4 asks whether the list that
proves nothing broke comes back green five times running, and a count of green rounds
means nothing unless the thing being tested held still between them.

**The moment is the point.** 2.4 was deferred by R43, and then deferred again by work
instruction 385 for a reason it stated in terms: *soaking the list before step 9 lands
would invalidate it by its own terms.* Step 9 landed at `e5e4bee0`. **There is no block
of code left in this phase**, so the tree stops here, and this is the unit the last three
instructions have been pointing at.

**One honest caveat, stated at the top rather than buried.** No judging session has read
unit 385's report against step 9. If one later returns `partial` and a unit moves code
again, tonight's soak does not become a lie - but only if you say exactly which commit it
was frozen at. **Task 1 prints that hash and section 3 carries it.** A soak whose frozen
point is not named is worth nothing to the next reader.

**Status.** `sh tools/status.sh`, real clock, after every commit and every task. **The `sh`
is part of the command - read section 2 before you type it.**

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

**HM-DEC-155.** No suite; only `docs\carry-forward-tests.txt` as its own top comment says -
**two invocations, one build each**, status written immediately before each. Never
background and poll. **Tonight that rule and the unit's whole subject are the same thing:**
a round *is* those two command lines, and there is nothing else to run.

**Unit 323 is the warning this file was written from.** It read the list as a list of names
and ran one `dotnet test` per name - forty-one builds - wrote one status line, and the
watchdog killed it at twelve minutes of silence. **It had done nothing wrong except obey the
shape of the file.** You will run those two lines ten to fourteen times tonight. **Write a
status line immediately before every single invocation**, naming the round and the attempt.
Silence is what kills a soak.

**The app invocation is the flaky one and you must expect it.** The headless
`InvalidProgramException: You have caused dispatcher loop` has been seen **sixteen or more
times across six units**, always at about 1 ms inside `HeadlessUnitTestSession`, always
before any assertion, and never twice on the same name. It has never once touched the engine
invocation. **It is not a red. It is a lost invocation**, and section 6's first ruling says
exactly what to do with one. **Do not chase it.** It is nobody's criterion and chasing it is
how this night gets spent on an Avalonia platform fault instead of on 2.4.

**A loop goes in a script file** (`;` is refused in a compound command). `Bash(sh:*)` is
granted, so `sh .run-unit/unit386-round.sh` works. **Write the script with the editor.**

---

## 2. The tool facts

- **The status line is `sh tools/status.sh ...`, with the `sh`.** The granted rule is
  `Bash(sh tools/status.sh:*)` and **a permission rule is a literal prefix match over the
  whole command string**. `tools/status.sh ...` and `./tools/status.sh ...` are both
  refused; unit 384 lost three calls to exactly that.
- **`git status` with no `-C`.** The granted rule is `Bash(git status:*)`. You are already
  standing in the root. `git -C /c/Source/HamLet status ...` is refused.
- **`git rev-parse` and `git diff` are granted** and they are how tonight's central claim is
  proved. Use them; do not assert a frozen tree from memory.
- **Shell output redirection (`>`) is refused to every path.** Write files with the editor.
  **A test run's output is read from the console**, or piped to `grep` in the same command.
- **A compound command with `;`, `&&` or a second operation is refused**, and so is
  `cd X && Y`.
- **Apostrophes in quoted heredocs break; doubled backslashes collapse; `rm` is refused;
  `-m` more than once for a multi-line commit.** Append to a file with the editor, not with
  `cat >> FILE <<'EOF'`.
- **Write this repository's files as UTF-8.** A PowerShell `>` redirect writes UTF-16 and the
  launcher cannot read it. **`PHASE_OUTCOME.md` and the archived Olivia file are BOM+CRLF -
  edit them with the editor and do not normalise them.**
- **`validate-output.bat` is at `tools/arbiter/validate-output.bat`**, and the shape is
  forward slashes, a leading `./`, one command, no `cd` in front and no `cmd /c` around it.
  **Run it before you say the report is written.** If it answers *This command requires
  approval*, that is the permission mode and not the syntax: hand-check the rules against the
  script's own header, say in section 4 that what you did was a hand-check and not a run, and
  move on.

---

## 3. Asks still outstanding

Carried per HM-DEC-139, **verbatim in section 4**.

**The queue stands at forty-seven and you answer none of them.** It is the thirty-nine unit
385 carried plus **unit 385's own eight**. Unit 385's report is committed and readable at the
root (`output.md` at `e5e4bee0`); its section 4 is the list. **Read it; do not reconstruct the
queue from memory.**

Three of them touch tonight's work and each gets one line in your section 4:

- **Unit 385's item 1 - the hold reads the carrier *and* the hand-back.** A finding, already
  acted on in unit 385's own night. **Nothing here reopens it.** It matters to you only
  because the four names it produced are now on the app command line and will run in every
  round.
- **Unit 385's item 8 / the `RULES_AT` id split, for the eighth unit running.**
  `PROJECT_STATUS.md` reads `HM-DEC-165 (2026-09-19)`; `CLAUDE.md` section 1 holds
  `CPS-DEC-0165`. The reload names it first among the disagreements. `tools\status.sh` writes
  that field as a literal and **you may not edit `tools\`**. Report it. Do not repair it.
- **Unit 383's item 5 and item 4 - the inherited reds.**
  `TheOliviaMoveUpTests.ItIsNotOfferedOnAGuessedYourTurn`,
  `ViewTestsActThroughControlsTests.NoViewTestWritesAPropertyAControlOwns` and
  `TheOliviaSendTests.StopMidPlayAbortsAnOliviaSend`. **None is on the carry-forward list**,
  so none can touch a round. **They are not yours and you do not open their files** - editing
  a test file tonight would break the frozen tree that is the whole point. Report them.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet holds what it has - the hardening phase, screen, record and test
            work banked in the PSK31 and Olivia threads that needs neither the radio
            nor the owner, judged by tests that ran and, at the end, by Tim at his
            window.
UNIT GOAL:  Prove that the list which proves nothing broke can itself be trusted -
            five consecutive green rounds of both invocations over a tree that does
            not move between the first and the last, with the frozen commit named
            and with two counts reported rather than one, so nobody ever again has
            to argue about what a green run meant; and make the archived Olivia
            record tell the truth about the unit of 2026-09-14 that never ran, by
            appending to it and never editing it.
ADVANCES:   step 2 criterion 4, and step 2 criterion 6.
DRIFT:      none.
```

**The count today**, from `PHASE_OUTCOME.md`, `PHASE_PLAN.md` at `e5e4bee0` and the reload of
2026-09-22 08:04.

| Step | State | Units spent | Where it stands |
|---|---|---|---|
| 0 | `partial` | 369, and 371's carried repair | 0.2 to 0.5 certified met at `PHASE_OUTCOME.md:41`. **0.1 is cut down tonight - section 6 ruling 1** |
| 1 | **`done`** | 372, 373, 374 | closed by 374 |
| 2 | `partial` | 375, 377, and 384 for one task | 2.1, 2.2, 2.3 met. **2.4 and 2.6 open - this unit** |
| 3 | **`done`** | 380, 381 | closed by 381, judging session `done` |
| 4 | `partial` | 382, 383 | 4.1 and 4.2 met. 4.3 **cut down** by instruction 384, logged to the owner |
| 5 | `not started` | 0 | Tim's own, and it ends the run |
| 6 | **`done`** | 376 | closed by R42 at the measured floor |
| 7 | `partial` | 378, 383 | 7.1, 7.3, 7.4, 7.5 met. 7.2 met on unit 383's own account, **unjudged** |
| 8 | **`done`** | 379 | all four criteria met, judging session `done` |
| 9 | ungraded | 385 | unit 385's own account says all five met. **No judging session has read it** |

**Step 2's entry, checked:** *step 1 done.* Unit 374 closed step 1 and `PHASE_PLAN.md` at
`e5e4bee0` ticks 1.1 to 1.4. **The entry is open.**

### Why step 2, when the step handed to this arbiter was step 0

**Because 0.1 asks a unit to name something that does not exist, and I measured that rather
than inherit it.** Criterion 0.1 reads *the commit and line between 1.13.30 and 1.13.48 that
dropped the transmit device on load are named in the report.* Against the tree at authoring
time:

| Measurement | Result |
|---|---|
| commits in `681d45c8..ec4b466e` (1.13.30 to 1.13.48) | **119** |
| of those, touching `src/Hamlet.App/Settings/`, `SettingsViewModel.cs`, `SettingsWindow.axaml` or `App.axaml.cs` | **0** |
| of those, touching **any path anywhere in the repository naming `settings`** | **0** |

The second row is unit 369's search. **The third row is wider than unit 369's and it is also
zero** - only a commit *message* mentions `FixtureSettings`, and a message is not a diff. The
loader did not drop the value in that window because nothing in that window touched the
loader. Unit 369 named the line that did drop it and measured it as predating 1.13.30.
**0.1 is a completed negative, and a second unit re-running that search is exactly the loop
shape `ARBITER.md` section 4 describes.** Section 6 ruling 1 cuts it down. **No task here
opens the settings path, and you do not search that window again.**

So step 2 holds **the last two criteria in this phase a unit can reach**. Everything else is
answered, the owner's, or waiting on a judging session: 4.3 is cut down and logged; 5.1 is
Tim at his window; 7.2 and all of step 9 need a judging session and not a unit.

### Why proposing unit 384's approach again is not a loop

**Because unit 384 never executed it, and that was verified against the tree rather than
taken from unit 385's word for it.**

| Evidence | Reading |
|---|---|
| `.run-unit\last-run.json` `terminal_reason` | `api_error` |
| its `api_error_status` | **529** |
| its `is_error`, `num_turns`, `total_cost_usd` | `true`, 72, **$4.7155615** |
| its last four permission denials | still writing the **task 0** status line, *"round 0 starting"* |
| commits from that session | **one** - `9832f972`, task 0 |
| `COST:` on `## UNIT 7 - STEP 2` in `PHASE_OUTCOME.md` | **4.7155615** - the same run |
| its `FATE:` | `executed` |
| its `STATE_WHY:` | *"this unit worked only on steps 4 and 7"* - **which is unit 383's night** |

**The run died at task 0 before a single round ran, and the launcher then graded unit 383's
leftover `output.md` as unit 384's.** `ARBITER.md` section 8: a `never ran` entry is not a
tried approach, the loop test reads `APPROACH` and cannot see the distinction, and the
arbiter can. **The approach is untested, so it is re-proposed.**

**That false row is the same launcher fault criterion 2.6 exists to record about the Olivia
phase, happening a second time.** Unit 385 already appended the correction for it in this
phase's own outcome file. **You do not repeat that work and you do not edit the row.** 2.6
is about the *Olivia* file and only the Olivia file - section 6 ruling 2.

### What this unit is worth, in the owner's terms

Every unit for a month has ended by saying *the carry-forward list came back green*. **Nobody
has ever proved that sentence means anything.** Twice now a name on that list has gone red for
reasons no one could explain, and roughly one app run in three has died to a fault in the test
window rather than in Hamlet. After tonight the record says, with a commit hash beside it,
how many times in a row the whole list came back clean with not one line of Hamlet changing -
**and it says it as two numbers, the strict one and the one that forgives a test window
falling over, so that the difference between them is visible instead of argued about.** And
the archived record of the Olivia phase stops claiming that a unit ran on 2026-09-14 which
never ran, without a single character of the false entry being altered.

---

## 5. Verify this instruction against the tree

Every line below was read at authoring time, at the file and line named. **Report every
mismatch in section 4 and in section 1 of your report; repair nothing but this unit's.** Line
numbers move under an edit - if one is off by a few, the name is the thing that matters; say
so and go on.

### The two command lines, which are the unit's whole instrument

`docs/carry-forward-tests.txt` **line 7** is the app invocation and **line 9** is the engine
invocation. Both begin `timeout 480 dotnet test ...`. **They are used unedited.** At authoring
time the app line carried **59** filter terms including unit 385's four new names -
`TheCarrierHoldsTheButtonsTests`, `TheCardOffersLogAndAnXTests`,
`TheTurnMovesOnEveryHandBackTests`, `TheFourAreOnOliviaCardsTooTests` - and the engine line
carried **25**. **Count both yourself at task 1 and report the numbers**; if they differ from
these, the list moved after authoring and that is a section 4 item, not a reason to stop.

### What a round should come back as

Unit 385's exit run: **engine 150 of 150, app 226 of 226**. Unit 385's entry run: engine 150
of 150; app **225 of 226 on attempt 1** with one dispatcher loop at 1 ms in
`TheStopIsAlwaysOnScreenTests`, and **226 of 226 on attempt 2**. **Those are the numbers a
green round must match or beat.** A round that completes at a lower count has a red in it and
section 6 ruling 1 says what that means.

### The three environmental shapes, which are not reds

`docs/quarantined-tests.txt` is the file where the list's reasons live. It records:

1. **The Avalonia headless dispatcher loop**, `InvalidProgramException "You've caused
   dispatcher loop"` - kills at about **1 ms, before any assertion**, moves between names,
   eight different names across four units by unit 375's count and more since, **never once
   on the engine invocation**. The file calls it *the single biggest obstacle to criterion
   2.4* and says it is nobody's criterion.
2. **The temp-jsonl file lock.**
3. **The 600 ms wall-clock race.**

And one **quarantined name**, off the app command line since unit 375:
`TheTestsStayOffTheNetworkTests.The354LayoutReadsTheSameNumbersTwiceRunning`. The other four
names of that type **stay on the list by name** and are the network guard. **A better failure
message is already in the tree for it, unused, waiting for a reproduction** - it is off the
list, so it cannot fire tonight, and you do not put it back.

One name is recorded there as **not quarantined and not settled**:
`TheStopIsAlwaysOnScreenTests.WithNothingKeyedItSaysStopAndIsStillPressable`, red **1 of 17**
full app invocations at 106 ms - an assertion, not the session fault - and green 0 of 21 on
its own type. **It is on the list.** If it goes red tonight, that is a red under ruling 1, it
is the second occurrence ever, and the message must be captured whole this time because the
file says in terms that the first one was not.

### The false Olivia entry, for 2.6

`docs/phase-olivia-run/PHASE_OUTCOME.md`:

| Where | What is there |
|---|---|
| **line 39** | `## UNIT 2 - STEP 1` |
| its `COST:` | `15.045699500000005` |
| `## UNIT 1 - STEP 0` above it, its `COST:` | **`15.045699500000005` - identical** |
| its `FATE:` / `STATE_AFTER:` | `executed` / `not started` |
| its `STATE_WHY:` | *"The unit delivered only step 0 ... it built no detector, no burst generator ..."* - **which is unit 358's night** |
| **line 59**, in `## UNIT 359 - STEP 1`'s `WHY:` | *"the UNIT 2 - STEP 1 entry above records a run that halted on the session lock and never executed, so the approach is untried"* |

**Two costs to fifteen significant figures cannot be two runs.** That is the evidence 2.6
names as *identical cost*; the second half is *no commit*, and **you measure that yourself
with `git log` over 2026-09-14 rather than copying it from here.**

### The record and the versions

`Directory.Build.props` line 1184 reads `<Version>1.13.72</Version>`. `PHASE_STATUS.md` names
this phase and reads `CURRENT_STEP: 9`, `WORK_INSTRUCTION: 385`. `PHASE_PLAN.md` at
`e5e4bee0` ticks 0.2 to 0.5, all of step 1, 2.1 to 2.3, all of steps 3, 6 and 8, 4.1, 4.2,
7.1, 7.3, 7.4, 7.5. **2.4, 2.6, 0.1, 4.3, 7.2, 5.1 and 9.1 to 9.5 are unticked.**

### What will be red before you start

`TheOliviaMoveUpTests.ItIsNotOfferedOnAGuessedYourTurn`,
`ViewTestsActThroughControlsTests.NoViewTestWritesAPropertyAControlOwns` and
`TheOliviaSendTests.StopMidPlayAbortsAnOliviaSend` - **none of them on the carry-forward
list**, so none can enter a round. Plus the inherited known-red block at
`docs/carry-forward-tests.txt` line 152, which is likewise never on the list.

---

## 6. Rulings in force

**Transcribed in full. Do not re-argue any of them.** The first is inherited and is the most
important thing in this document; the last two are mine, author's and overrulable under R31.

### Inherited, and it is why this unit can be honest: what a round is, from work instruction 384 section 6 ruling 1

**This was written before any round ran, and that is its whole virtue - a definition invented
after a round breaks is not a definition.** It is transcribed verbatim from
`PHASE_OUTCOME.md:470`. **It is not reopened, not tuned, and not improved tonight.**

> What a round is and what 2.4 counts, written before any round ran because a definition
> invented after a round breaks is not a definition: a round is both command lines exactly as
> `docs/carry-forward-tests.txt` prints them at lines 7 and 9, unedited, one build each,
> foregrounded, with a status line immediately before each; a round is green only when both
> invocations complete and every name passes; five in a row means five consecutive rounds with
> no file under `src`, `tests`, `assets` or `data` and no line of the list changing between the
> first counted round and the last, proved by a printed `git diff` against task 1's commit; an
> invocation lost to one of the three recorded environmental shapes - the ~1 ms dispatcher
> loop, the temp-jsonl file lock, the 600 ms wall-clock race - is a lost invocation rather than
> a red one, re-run once with both attempts recorded, while a name that ran and disagreed is
> always a red; the report carries TWO counts and neither may stand alone, the strict count of
> rounds green on the first attempt with no re-run and the counted streak under the re-run
> rule, said in the same sentence every time with the difference between them named as the
> night's finding; a name that fails an assertion resets the streak to zero and is worked under
> 2.3's own mechanism - ten isolated runs, then deterministic with the cause named under R12 or
> quarantined into `docs/quarantined-tests.txt` with the cause stated and taken off the command
> line in the same commit - after which the streak restarts and, if the night is gone, the
> number reached is reported and 2.4 stays partial under `PHASE_PLAN.md` section 6's
> ship-report-partial-move-on; and nothing is added to the list tonight.

**Four things in it that decide tonight, called out so they are not skimmed:**

1. **Two counts, always, in the same sentence.** The strict count and the counted streak.
   **A report that gives one of them has not met 2.4** however good the number is.
2. **A lost invocation is re-run once.** Once. Not until it works.
3. **A red resets the streak to zero** and pulls you into 2.3's mechanism, which is task 4.
4. **Nothing is added to the list tonight.** The list is the instrument; you do not adjust
   the instrument during the measurement.

**One amendment of mine, and it only tightens:** where ruling 1 says *proved by a printed
`git diff` against task 1's commit*, tonight it is **task 2's** commit, because task 2 lands
2.6 and task 1 changes no file at all. **The frozen point is task 2's commit and its hash is
printed in the report.**

### The plan's own, which 2.4 is the second half of

**R35 - the record's small lies.** *Two app tests flake in headless runs
(`TheStopIsAlwaysOnScreenTests`, `TheTestsStayOffTheNetworkTests`, unit 365's report): **a
flaking test on the carry-forward list is worse than none.***

**`PHASE_PLAN.md` section 6, the branching rules that bear on tonight:** *A must-pass missed
by a little: ship, report, `partial`, move on. **Never loosen a test.*** *A flake cannot be
made deterministic: quarantine it with its cause named; **that is meeting 2.3, not missing
it**.* *A file must be deleted: empty it, comment it, list it.* *Anything would change what
goes on the air, or what keys: `MOVE: stop`.*

**R31 - unattended.** Criteria by id, a done step closed, the owner's step ends the run, **two
rulings a unit**.

### The first is mine: criterion 0.1 is unachievable as written, and step 0 is cut down at `partial`

**Author's and overrulable. The measurement is in section 4 and it is mine, not inherited.**

0.1 presupposes a fact that does not exist: across the 119 commits of the 1.13.30-to-1.13.48
window, **zero** touch any settings path, and **zero** touch any path in the repository whose
name contains `settings`. Unit 369 searched it and reported the negative; a separate judging
session returned `partial` because the report *"names none"*; work instruction 372 ruled it met
on the negative; and the same judging session said in terms that whether a negative satisfies
0.1 *"is a plan wording matter the unit was to rule on itself, not a stop."*

**So it is ruled, and the ruling is this.** Step 0 is **cut down at `partial`**, exactly as
step 4 was cut down on 4.3. **0.1 stays unticked in `PHASE_PLAN.md` and is never ticked by a
unit of this phase**, because a tick would imply a commit was named and no commit exists to
name; **the record shows the negative plainly instead of hiding it behind a mark.** **No unit
in this phase searches that window again** - that search is complete, and repeating it is the
loop. The remedy, if the owner wants one, is to reword 0.1 so that a completed negative
satisfies it, **and rewording the plan is his.** It is logged to him, not chased.

**What you do about it tonight: one sentence in task 0's appended entry, and nothing else.**
No file under `src/Hamlet.App/Settings/` is opened, no `git log` is run over that window, and
no task waits on it.

### The second is mine: what 2.6 is, what it is not, and when it lands

**Author's and overrulable.**

**(a) 2.6 is the *Olivia* file and only the Olivia file.** The criterion names
`docs/phase-olivia-run/PHASE_OUTCOME.md` and its `UNIT 2 - STEP 1` of 2026-09-14. **The second
occurrence of the same fault - unit 384's `## UNIT 7 - STEP 2` row in this phase's own outcome
file - is already recorded by unit 385 and is not re-recorded, not edited and not widened
into 2.6.** One sentence in your section 4 noting that the fault has now happened twice and is
logged to the owner is the whole of what tonight owes it.

**(b) The evidence is measured, not copied.** 2.6 asks for *the evidence (identical cost, no
commit)*. Read both `COST:` lines out of the Olivia file yourself and quote them. Establish
*no commit* with `git log` over 2026-09-14 and **quote what you ran and what came back** -
including, if it is so, that the commits of that date belong to unit 358 and unit 359 and that
none belongs to a unit between them. **If your measurement disagrees with section 5's table,
your measurement wins and it is the night's finding.**

**(c) Nothing is edited. Ever.** *The false row is not edited, per the append-only rule* is
the criterion's own last clause. **You append a new entry at the end of the Olivia file**,
naming what it corrects, carrying the evidence, and saying which unit and which date wrote it.
The characters of `## UNIT 2 - STEP 1` are not touched - not its `FATE`, not its `STATE_AFTER`,
not a typo in it if you find one. **The file is BOM+CRLF; keep it that way.**

**(d) It lands at task 2, before the first counted round.** 2.6 changes a documentation file.
Ruling 1's frozen-tree clause names `src`, `tests`, `assets` and `data`, so a docs change
between rounds would be permitted by the letter - **and it will not happen anyway, because
landing 2.6 first makes the tree frozen in every sense and removes the argument entirely.**
After task 2's commit, **the only files this unit may write are `PHASE_STATUS.md`,
`PHASE_OUTCOME.md`, `output.md`, `docs/quarantined-tests.txt` (task 4 only) and its own
scratch under `.run-unit/`.**

---

## 7. Status cadence

`sh tools/status.sh`, real clock, **after every commit, after every task, and immediately
before every single test invocation** - which tonight means before each of the ten to fourteen
invocations you will run. Name the round and the attempt in the message: *round 3, app,
attempt 1*. **The watchdog kills at twelve minutes of silence and a round is not instant.**

---

## 8. The tasks

**Six tasks. The trace is task 1 and it builds nothing. The named drop candidate is in task 1
and it is item 4.**

### Task 0 - the record and the entry round

Version **1.13.72 -> 1.13.73**. `PHASE_STATUS.md` to **step 2 and unit 386**, and its
`STEP: 0` line gains one sentence carrying section 6 ruling 1 - that 0.1 is cut down as a
completed negative, unticked, and logged to the owner. **`PHASE_PLAN.md` ticks nothing at this
task**; no judging session has certified anything since unit 385's task 0 did its tick-up.

Append the `## UNIT 386 - STEP 2` entry to `PHASE_OUTCOME.md` in the house shape, carrying
this instruction's `STEP`, `APPROACH`, `MOVE`, `WHY`, `DECIDED`, `LICENCE`, `COST`,
`ACCOMPLISHED` and an `ENTRY:` line with the entry round's real numbers.

**Then run the entry round - round 0.** Both command lines, unedited, status before each.
**Round 0 is not a counted round**; it is the before, and it tells you whether the tree you
are about to freeze is green at all. **If round 0 has a red in it, that red is inherited, it
is not this unit's, and it is worked under ruling 1's red path at task 4 before any round is
counted.**

**Commit.**

### Task 1 - the trace: what a round costs, and what the tree is frozen at

**One task, no file under `src`, `tests`, `assets` or `data` touched, nothing built.** This is
the measurement that decides how the night is spent.

1. **The instrument.** Count the filter terms on `docs/carry-forward-tests.txt` line 7 and
   line 9 and report both numbers against section 5's 59 and 25. Confirm the two lines begin
   `timeout 480 dotnet test` and that you will use them unedited.
2. **What a round costs, in wall time.** From round 0: the app invocation's seconds, the
   engine invocation's seconds, and the two added together. **Multiply by five and say in one
   line whether five counted rounds plus a re-run allowance fit in the night.** If they do
   not, say so now - not at round 4.
3. **The frozen point.** `git status --short` and `git rev-parse HEAD`. **Report both.** After
   task 2 commits, `git rev-parse HEAD` again: **that hash is the frozen commit and it is the
   single most important number in tonight's report.**
4. **The prior, and this is the drop candidate.** Out of `PHASE_OUTCOME.md`, tally how many
   full app invocations across units 375 to 385 were lost to the dispatcher loop and how many
   were red on an assertion. It gives you an expected cost for the soak and it tells the
   reader whether tonight was lucky or normal. **No criterion depends on it: if the night is
   tight, drop item 4 and say you dropped it.**

**Commit** (the trace's record only - no source file moves).

### Task 2 - 2.6, and the tree freezes here

The appended entry on `docs/phase-olivia-run/PHASE_OUTCOME.md`, per section 6 ruling 2.
Measure the evidence, quote it, append, **edit nothing**.

**Commit. This commit's hash is the frozen point.** Print it and carry it into the report.

### Task 3 - the soak: five counted rounds

Rounds 1 to 5 under ruling 1, **and no file is written during the soak at all** - not a
scratch note, not the outcome file. Keep the numbers in the session and commit them once at
task 5. The only things that run are the two command lines and `sh tools/status.sh`.

For **every** invocation record: the round, `app` or `engine`, the attempt number, the
completed count in the form *n of m*, the wall seconds, and - where an attempt was lost - the
name it landed on and the millisecond figure that proves it was the session fault and not an
assertion.

**A loop in a script file** (`sh .run-unit/unit386-round.sh`), not a compound command. **But
the status line must still be written before each invocation**, so if a script makes that
impossible, run the lines by hand; the status line wins.

**At the end of round 5, print `git diff` against task 2's commit and print that it is
empty.** That printed diff is the proof of the frozen tree and ruling 1 requires it.

**If a red appears, stop counting and go to task 4.** If five counted rounds are reached,
skip task 4 and say so.

### Task 4 - only if a name went red on an assertion

**2.3's own mechanism and nothing else.** Ten isolated runs of that type, whole type,
filtered, one build each. Then **either** deterministic with the cause named under R12
**or** quarantined into `docs/quarantined-tests.txt` with the cause stated and taken off the
command line **in the same commit**. Then the streak restarts from zero.

**Capture the failure message whole.** If the name is
`WithNothingKeyedItSaysStopAndIsStillPressable`, the quarantine file says in terms that the
first occurrence's message was never captured - **do not let that happen twice.**

**If the night is gone, report the number reached and 2.4 stays `partial`** under
`PHASE_PLAN.md` section 6. **That is a permitted outcome and not a failure.**

**Commit** (one per repair or quarantine).

### Task 5 - the exit run, the record and the report

The exit round of both invocations, its numbers against task 0's. Then `PHASE_PLAN.md`: **tick
2.6 if it landed, and tick 2.4 only if five counted rounds were reached**, with both counts
written beside it in the plan's own text - per work instruction 384 section 6 ruling 2's last
clause, *2.4 is ticked only by tonight's own rounds with both counts written beside it*.
**Tick nothing else.** Then `PHASE_STATUS.md`'s `STEP: 2` line rewritten to what tonight
measured, then `output.md`, then `validate-output.bat`, then push.

---

## 9. Parked - do not touch, do not raise

- **`docs/RADIO_SHEET.md`, `TheRadioSheetQuotesTheScreenTests` and every operator-facing
  string the sheet quotes.** 4.3 is cut down and logged to the owner.
- **Everything of step 9.** Unit 385's four new names run in every round and that is their
  only part tonight. You do not read its code, judge its criteria, tick them or re-prove them.
- **7.2.** It needs a judging session, not a unit.
- **`src/Hamlet.App/Settings/` and the 1.13.30-to-1.13.48 commit window.** Section 6 ruling 1.
- **The `## UNIT 7 - STEP 2` row and the `## UNIT 2 - STEP 1` row.** Both are false; **both
  stay exactly as they are.** Append-only.
- **`tools\`.** Including the `RULES_AT` split.
- **The dispatcher loop's cause.** Nobody's criterion. Record every occurrence; chase none.

---

## 10. What not to do

- **Do not change one file under `src` - not one, for any reason.** If you believe a criterion
  needs one, that is a section 4 item and the answer is no tonight.
- **Do not edit `docs/carry-forward-tests.txt`** except in task 4's quarantine commit, where
  taking a name off the command line is required to be in the same commit. **Nothing is added
  to the list tonight** (ruling 1).
- **Do not loosen a test, widen a filter, or re-run past the one allowance** to turn a number
  green. `PHASE_PLAN.md` section 6: never loosen a test.
- **Do not report one count.** Two, in the same sentence, every time (ruling 1).
- **Do not call a red environmental because you would like it to be.** The discriminator is
  measured and it is in the tree: **about 1 ms and before any assertion** is the session
  fault; **anything that ran and disagreed is a red.** 405 ms and 106 ms are the two recorded
  assertion failures; both are reds.
- **Do not edit a false row to fix it.** Append.
- **Do not tick a criterion in `PHASE_PLAN.md` that tonight did not earn**, and do not tick
  0.1 at all (ruling 1).
- **Do not run a suite.** HM-DEC-155, and tonight it is also the measurement.
- **Do not chase the dispatcher loop**, and do not spend a task on the quarantined
  `The354Layout...` name - it is off the command line and cannot fire.

---

## 11. Committing and pushing

**One commit per task**, with these exceptions: **task 4 takes one commit per repair or
quarantine**, and a carry-forward line edit goes in the same commit as the name it is about.
**Task 3 commits nothing** - the soak must leave the tree untouched, and its numbers are
committed at task 5. **Push once, at the end.**

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

**The ordering block first. `validate-output.bat` refuses a report without it.** Fill every
line from what you measured - a line that is the same every unit is furniture.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Steps 1, 3, 6 and 8 done; step 0
   partial with 0.1 cut down tonight as a completed negative; step 4 partial with
   4.3 cut down and logged to the owner; step 7 partial on an unjudged 7.2; step 9
   worked by unit 385 and ungraded; step 5 Tim's own. Step 2 is the last step a
   unit can move and this unit is the third spent on it.
B. Step 2 - the record says what was true. 2.1, 2.2 and 2.3 carried met and
   untouched. 2.6 <met|not>: the Olivia entry appended at <where>, evidence
   <the two cost figures> and <what git log returned>, the false row unedited.
   2.4 <met|partial>: <strict count> of 5 rounds green on the first attempt with
   no re-run, and <counted streak> of 5 under ruling 1's re-run rule, over a tree
   frozen at <commit hash> with git diff printed empty; <n> invocations lost to
   the dispatcher loop and <n> red on an assertion.
C. The report last. Section 4 raises <N> items on top of the carried forty-seven,
   and <none of them is | item <k> is> in the way of a criterion in B. Say in one
   line what the difference between the two counts in B means - that is the
   night's finding and ruling 1 asks for it by name.
```

```
UNIT:       386 - <complete|stopped> at task N of 6 - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   step 2, criteria <the ids you actually moved>
NUMBER:     consecutive green rounds of the carry-forward list over a tree that
            did not move: <strict> of 5 strict, <counted> of 5 counted; and false
            rows in the archived Olivia record now carrying a correction: 0 of 1
            -> <n> of 1
DRIFT:      none
```

**Section 3 must lead with the invocation table** - **every** invocation of the night, one
row each: round, app or engine, attempt, *n of m*, wall seconds, and the name and millisecond
figure where an attempt was lost. **That table is the criterion.** A prose summary of it is
not. Then, immediately under it, **the frozen commit hash and the printed empty `git diff`**.
Then the two counts in one sentence. Then task 1's cost arithmetic against what the night
actually took. Then 2.6's evidence quoted - both `COST:` lines and what `git log` returned.
Then the entry and exit round counts against each other.

**Section 2 tells Tim in one paragraph what is different**, in his terms: not that a soak was
run, but that **the list of tests which proves nothing broke has now itself been proved -
five times end to end with not one line of Hamlet changing in between, with the exact version
it was frozen at written down - and that the record says how many of those came back clean
first time and how many needed a second go because the test window fell over, two numbers
rather than one, so nobody has to argue about what green meant.** And that the archived
record of the Olivia run no longer claims a unit ran which never ran. **If 2.4 fell short, say
the number reached in his words and say plainly that it fell short.** Every claim computed,
not seen (FACT-004), and one line saying no port was opened, nothing was enumerated and
nothing was keyed.

**Section 4:** your own items first, most-blocking first, each saying plainly whether it wants
a ruling or is a finding - **a note is not a ruling request.**

- **If the two counts differ, that is your first item and it is a finding**, with the number
  of lost invocations and what they landed on.
- **If a name went red on an assertion, that is your first item** - say which, what the
  message was verbatim, and what task 4 did about it.
- **If you found that reaching 2.4 needs a change under `src`, that wants a ruling** and the
  answer tonight was no; say what you found and did not do.
- **One line on the launcher fault having now happened twice** - the Olivia phase's
  `UNIT 2 - STEP 1` and this phase's `## UNIT 7 - STEP 2` - **logged to the owner, both rows
  unedited.**
- **One line on 0.1**, saying it was cut down by this instruction's section 6 ruling 1, that
  no search was re-run, and that the rewording is the owner's.

Then the carried queue verbatim per HM-DEC-139: **the thirty-nine unit 383 carried plus unit
385's own eight - forty-seven** - read out of unit 385's committed `output.md`, with one line
on each of the three items of section 3 that tonight's work touched.

---

```
ARBITER-DECISION
STEP: 2
APPROACH: soak the carry-forward list five consecutive green rounds on a tree frozen after the phase's last code change, naming the frozen commit and printing the empty diff, and append 2.6's correction to the archived Olivia record in the same unit
MOVE: continue
WHY: step 2 holds the last two criteria in this phase a unit can reach - 4.3 is cut down and logged, 5.1 is Tim at his window, 7.2 and the whole of step 9 need a judging session rather than a unit, and step 0's 0.1 is a completed negative I re-measured myself and cut down tonight; and 2.4 counts only over a tree that does not move, which is true for the first time now that step 9 landed at e5e4bee0 and no block of code is left in the phase, exactly as work instructions 384 and 385 both said it would be.
STATE: partial
DECIDED: author's and overrulable, two rulings and one choice of step. (1) Criterion 0.1 is unachievable as written and step 0 is CUT DOWN at partial, on a measurement of my own and not on unit 369's alone: 119 commits lie between 681d45c8 (1.13.30) and ec4b466e (1.13.48), ZERO touch any of the four settings paths, and ZERO touch any path anywhere in the repository whose name contains settings - only a commit message mentions FixtureSettings, and a message is not a diff - so the criterion presupposes a commit that does not exist; 0.1 stays UNTICKED and is never ticked by a unit of this phase, because a tick would imply a commit was named, and the record shows the negative plainly instead; no unit in this phase searches that window again, since that search is complete and repeating it is the loop ARBITER.md section 4 describes; the remedy is to reword 0.1 so a completed negative satisfies it, which is a change to the plan and therefore the owner's, and it is logged to him rather than chased. (2) What 2.6 is and is not: it is the archived Olivia file and only that file, so the SECOND occurrence of the same launcher fault - unit 384's ## UNIT 7 - STEP 2 row, already recorded by unit 385 - is neither re-recorded nor folded into 2.6 nor edited; the evidence is measured rather than copied from the instruction, both COST figures read out of the Olivia file and the no-commit half established with git log over 2026-09-14 and quoted, with the unit's own measurement winning if it disagrees with section 5; nothing is edited, ever, and the correction is a new entry appended at the end of a BOM+CRLF file; and it lands at task 2, BEFORE the first counted round, so the tree is frozen in every sense and no argument about a docs file changing mid-soak is possible. Also decided and not a ruling: work instruction 384 section 6 ruling 1 - what a round is and what 2.4 counts - is transcribed VERBATIM and is not reopened, tuned or improved tonight, because its whole virtue is that it was written before any round ran; my only amendment tightens it, naming task 2's commit rather than task 1's as the frozen point, since task 1 changes no file at all. (0) The choice of step, which PHASE_PLAN.md section 6 makes the arbiter's: step 2 rather than step 0, which was the step this arbiter was handed - the reasoning is ruling 1 and it is set out with its numbers in section 4 of the instruction.
LICENCE: PHASE_PLAN.md R35, which 2.4 is the second half of, and R43, whose ordering has arrived and whose deferral has expired by its own terms; PHASE_PLAN.md section 6 - a must-pass missed by a little ships partial, a flake quarantined with its cause named is MEETING 2.3 rather than missing it, a test is never loosened, and a file that must go is emptied and listed rather than deleted; work instruction 384 section 6 ruling 1, transcribed in full as the definition of a round; ARBITER.md section 3's cut it down, applied to 0.1; ARBITER.md section 6, which makes the choice of step the arbiter's and keeps a change to the plan's own wording with the owner; ARBITER.md section 8's FATE table, which is what licenses re-proposing unit 384's approach - .run-unit/last-run.json reads terminal_reason api_error, api_error_status 529, 72 turns and $4.7155615 matching the COST on ## UNIT 7 - STEP 2, the session's only commit is 9832f972 task 0, and its STATE_WHY describes a unit that worked only on steps 4 and 7, which is unit 383's night, so the run NEVER RAN THE APPROACH and re-proposing it is not a loop; ARBITER.md section 4, whose reading returned NOT FOUND on tonight's approach line and whose judgment call on unit 384's resemblance is made above; HM-DEC-139, HM-DEC-155, HM-DEC-165; PSK31 plan R11, R12, R13, R14, R19, R31; CLAUDE.md 0.0, 0.2; FACT-004
ACCOMPLISHED: Every unit for a month has ended by saying the carry-forward list came back green, and nobody has ever proved that sentence means anything. After tonight the record says how many times in a row the whole list came back clean with not one line of Hamlet changing between the runs, with the exact commit it was frozen at written beside it and an empty diff printed to prove it - and it says it as two numbers rather than one, the count that came back clean first time and the count that forgives a test window falling over, so the difference between them is visible instead of argued about. Where a name failed for real it was measured ten times on its own and either fixed or written down with its cause. And the archived record of the Olivia run stops claiming that a unit ran on 2026-09-14 which never ran, with the evidence beside it and without a single character of the false entry being altered.
ADVANCES: step 2 criterion 4, and step 2 criterion 6 - the last two criteria in this phase that a unit can reach. It advances no other step: 0.1 is cut down rather than moved, 4.3 is cut down and logged, 7.2 and step 9 wait on a judging session, and 5.1 is Tim's.
END-ARBITER-DECISION
```
