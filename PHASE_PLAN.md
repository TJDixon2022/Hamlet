PHASE: CW decodes again
PHASE_SET: 2026-09-22
DESCRIPTION: A restore phase. The CW decoder read on the air on 2026-08-25 and reads nothing now; the floors that recorded what it produced have been red since 2026-08-31 and nobody has run them since. The engine's CW code goes back to the last commit that read, adapted so today's app builds; the floors go green and stay green under a guard every unit runs; the inherited reds are repaired or retired with reasons; the August rework goes back in one piece at a time on numbers. Judged by three named tests and, at the end, by Tim at the radio.
STEP: 0 | The break is measured and named - the three floor tests run at HEAD case by case with their numbers, the newest commit on main where all three were green is named, and the first commit after it that turned one red is named.
STEP: 1 | The decoder reads again - src\Hamlet.RadioEngine\Cw restored to the named commit and adapted only where today's app or tests would not build; the solution builds; the three floor tests green; the app's CW tests green; the carry-forward list green; nothing under src\Hamlet.App changed without a listed reason.
STEP: 2 | CW cannot break silently again - a CW read guard on the engine carry-forward line beside FT8, FT4, PSK31 and Olivia, watched red against a broken decoder before it was trusted, and measured to fit the line's timeout.
STEP: 3 | The inherited reds are gone - every name in docs\unit239-failing-set.txt and the known-reds block is green, repaired, or retired under R49 with its reason in docs\cw-retired-tests.txt; the known-reds block of the carry-forward list names no CW test.
STEP: 4 | The August rework is judged on numbers - each piece of the 2026-08-28 to 08-31 rework re-applied in its own commit and kept only if the three floor tests stay green and a named number moves; a piece that moves nothing goes back out and the report says so.
STEP: 5 | Tim at the radio - CW on 40 m, text on the CW tab that reads as what was sent, and he says it read.

---

# The CW phase - the reasoning under the step list

**Set 2026-09-22 by the web thread on Tim's instruction:** *"Let's try CW one more time."*
*"The CW currently has taken many steps backwards and no longer decodes anything. There
were several stages in the initial CW development where we actually had some decent CW.
Now there's nothing. It can't decode anything. So this phase is get CW working."* The
hardening phase halted at 5.1, Tim's, and is archived at `docs/phase-hardening-run/`; he
closes it at his window when he closes it, and that is not this phase's business.

## §1 What this phase is

Five steps of engine and test work that need no radio and no ruling, and one step of Tim
at the radio. **Nothing here touches what keys or transmits.** The CW transmit path
(`CwTransmitter`, `KeyerCwSender`, `TransmitChain`, `AutoCall`) is in the same folder the
restore touches and is not to be moved by it: a restore that changes a byte of any of
those is `MOVE: stop`.

**What the record says, measured by the web thread on 2026-09-22 from the tree:**

- The CW decoder source under `src\Hamlet.RadioEngine\Cw` was last changed between
  2026-08-28 and 2026-08-31 (`CwPitchRanking`, `CwJointCutter`, `CwElementPitch`,
  `CwUnitEstimator`, `CwStreamSplit`, `CwSpectralPeak`, `CwSwingSurvey`, the posterior)
  and on 2026-09-03 for allocation only (`CwDecoder.cs`, `CwKeyingMeter.cs`,
  `AudioHandoff.cs`). Nothing since. Every phase since 08-31 was FT8, FT4, PSK31, Olivia,
  screen, hardening or maintenance.
- Unit 204 on 2026-08-31 already recorded `clean-12wpm` and `clean-18wpm` not decoding
  exactly, `exchange-easy` not read whole, and six of the 08-25 captures producing less
  than their floors. HM-DEC-151 (2026-08-31) declared them inherited and "not a licence
  to leave the CW reds alone forever." Unit 239 (2026-09-03) listed 51 CW reds by name in
  `docs\unit239-failing-set.txt`. No CW test has been run since 2026-09-05.
- `DECISIONS.md` has no entry between 2026-08-21 and 2026-09-04. The rework that broke it
  left no ruling and no record.
- `docs\carry-forward-tests.txt` has a read guard for FT8, FT4, PSK31 and Olivia and none
  for CW, so no unit since could see it break and none could see it fixed.
- The floors that define "working" are already in the tree, with the numbers in their
  own data: `TheCapturesThatDecodeKeepDecodingTests.EachStillProducesWhatItDid` (every
  capture, characters and elements as floors that only rise),
  `TheAdjudicatedReadingsKeepReadingTests` (the three adjudicated anchors: `N4L`,
  `VA3VRR`, `AA4MP/4 QNIK`), and `CwFixtureTests.TheCleanRecordingsDecodeExactly`
  (`clean-12wpm`, `clean-18wpm`). **These three are "the three floor tests" wherever
  this plan says it**, and they are run by name, filtered, foregrounded, with a timeout,
  never as a suite (HM-DEC-155).

**What this phase does not claim.** Not `PHASE_GOAL.md`'s 80% by edit distance - that
needs an adjudicated capture end to end and is a later phase. Not weak signals, crowded
passbands, the scanner, the keying sweep on screen, the `competing` field, or the
instruments the 08-25 analysis called liars. Those are carried in §7.

## §2 What is the same

Every ruling of the PSK31, screen, Olivia and hardening plans stands (their
`PHASE_PLAN.md` files under `docs/phase-*-run/`) and is not restated. The ones this phase
leans on: **R11** nothing at the radio; **R12** a session rewrites its own tests and never
asks the owner; **R14** tests prove criteria and nothing beyond; **R19** American; **R45**
the form of a criterion and of ADVANCES; **§6's later-ruling-wins rule**. `CLAUDE.md`
**§0.0** never present a guess as a decode - a floor is a count and says nothing about
correctness, and no report may say "reads" of a count; **§0.2** transmit safety, absolute;
**§12.5** a fixture built from the same misunderstanding as the code proves nothing.
**HM-DEC-091** a change that reads one recording and quietly costs another is not a fix.
**HM-DEC-103** a fixture retires by ruling, one at a time, with its reason recorded.
**HM-DEC-151** the CW reds are not to be left forever. **HM-DEC-155** no suite.
**HM-DEC-165** nothing red that was green before. **FACT-004** a dev result is an
indication. **FACT-006** the dev machine has no radio.

## §R Rulings, Tim, 2026-09-22

**R47 - the phase, and where it starts.** *"So this phase is get CW working."* *"We've
got a bunch of saved WAV files you can use. So we use those."* Step 0 is the bench: the
saved captures and synthetics through the decoder as it stands, before anything is built.
Rejected: starting from a capture at the radio (one night's evidence, and the bench
splits the fault first); starting from git history alone (a bisect on an intermittent
crash inside `Cw` has lied to this project before, HM-OPEN-063).

**R48 - restore, then re-apply.** Ruled A: *find the last commit where the floors were
green, bring the decoder back to it, then re-apply the rework's pieces one at a time,
each kept only if the floors stay green.* Rejected: repairing the current design forward
(it has no ruling and no record, and "fix it" is not a criterion the arbiter can judge -
that is how three weeks of red happened); measuring both and halting for a ruling (a
guaranteed halt after one unit, the opposite of unattended).

**R49 - what "green" is, and what may be retired.** Ruled C: *the floor tests are the
criterion at every step; the remaining reds are then taken one at a time - a red that
asserts a decode result is repaired, a red that asserts a mechanism no longer in the tree
is retired with its reason recorded.* The rule, worded tightly: **a test may be retired
only if it names a class or method that does not exist under
`src\Hamlet.RadioEngine\Cw` at HEAD, and the report quotes the missing name. A test that
reads audio and asserts characters, elements, a tone or a speed is never retired** - it
is green, or it is red with its number in the report and the step is partial. Rejected:
the operator's floors only (43 reds stay inherited and the next thread finds the pile
again); all 51 by name (chasing a design rather than a decode, the longest phase).

**R50 - the restore is the engine folder only.** Ruled B: *restore only
`src\Hamlet.RadioEngine\Cw` and adapt the seams until it builds; the app keeps its
September shape.* Rejected: restoring everything CW-shaped including the app views (the
CW tab loses the screen and maintenance phases' work); bisecting the 08-28 to 08-31
commits and reverting just those (several may each have cost something, and the crash).

**R51 - what earns a piece its place back.** Ruled B: *the three floor tests stay green
and one named number improves - `ATEEKEND` toward `WEEKEND` on 021410, `AB OVE` toward
`ABOVE` on 013637, or a character count on an unadjudicated capture against its sidecar
or its floor. A piece that moves nothing is not re-applied and the report says so.*
Rejected: keep anything that does no harm (dead code with a test bill, and nobody
measured what any of them gave); re-apply nothing this phase (the rework sits unmeasured
for another phase).

**R52 - the loop.** *"The arbiter has a job to keep the phase churning. Set the phase
steps up in such a way that the arbiter has an easier job."* *"The work instruction
comes from Claude Web for the initial work, so immediately it spins up and gets going,
and then the arbiter kicks in at the end and moves forward. The goal is always to move
forward. I want unattended development to be maximized."* Consequence: every criterion
below is a named test or a named number a report can carry; only 5.1 is his.

**R53 - Tim, 2026-09-22, on unit 391's item 1: step 1 restores to `7e209cb4`.** Ruled B.
Unit 391 measured that the two clean synthetics have been red since `8e3ee277` of
2026-08-21, so no commit since then is green on all three floor tests and 0.2's literal
answer is `07f0397a` of 08-21, four days before the evening the floors were set on. He
chose the evening: `7e209cb4` (2026-08-25) is green on `TheCapturesThatDecodeKeepDecodingTests`
36 of 36 and `TheAdjudicatedReadingsKeepReadingTests` 13 of 13, runs the captures type in
97 s where HEAD takes 1995 s, and has the fewest seams. **Step 1's named commit is
`7e209cb4`, not 0.2's answer.** The two clean synthetics start step 1 red; they are
step 3's repairs under R49 and are not retired. Rejected: `07f0397a` (green on all three,
but 32 of the 37 capture floors were never run against it and it has seven app-facing
types missing).

## §3 What is different from the phases before it

This phase moves the decode path, which no phase since 08-31 has. Two consequences the
arbiter must carry into every instruction: **the three floor tests are run before the
first change and after the last change of every unit**, beside the carry-forward list,
and a red after that was green before is a regression named in section 1 and section 4;
and **the CW transmit files are read and never written** - `CwTransmitter.cs`,
`KeyerCwSender.cs`, `TransmitChain.cs`, `AutoCall.cs`, `AutoCallAnswers.cs`,
`CwTransmitGuard.cs`, `TransmissionWatch.cs`, `TransmitReadiness.cs`,
`TransmitPrivileges.cs`, `TransmitNotes.cs`, `ICwSender.cs` - a restore that would change
one of them keeps HEAD's copy and says so.

## §4 The steps

Exit criteria carry ids `N.k`; met is `[x]`; R45 gives the form. A step's exit is its own
assertions, the three floor tests, and `docs/carry-forward-tests.txt` run as its comment
says - never the whole suite.

## Step 0 - The break is measured and named

**Delivers:** R47. The numbers at HEAD and the commit to go back to.

**Entry:** `PHASE_STATUS.md` names this phase; the tree is Hamlet's.

**Exit:**
- [x] 0.1 The three floor tests run at HEAD by name, filtered, foregrounded, with a timeout, one type per invocation, and every case is listed in the report as green or red with its measured characters and elements beside its floor. **Unit 391 ran them at HEAD, 20 of 52 cases red, every case with its numbers in `docs/phase-cw/unit391-floors-head.md`.**
- [x] 0.2 The newest commit on `main` at which all three floor tests are green - each run at that commit as that commit has them - is named by hash and date, with the count of commits between it and HEAD that touch `src\Hamlet.RadioEngine\Cw`. **Unit 391 named `07f0397a` of 2026-08-21, 84 Cw commits before HEAD, in `docs/phase-cw/unit391-walk.md`; R53 then named `7e209cb4` as step 1's commit.**
- [x] 0.3 The first commit after it at which any floor test has a red case is named by hash and date, with the cases it turned red. **Unit 391 named `8e3ee277`, the likelihood decoder, which turned both clean synthetics red, in `docs/phase-cw/unit391-walk.md`.**
- [x] 0.4 The report lists every type under `src\Hamlet.App` and `tests\Hamlet.App.Tests` that references a type in `src\Hamlet.RadioEngine\Cw`, and for each whether the member it uses exists at the named commit, so step 1 knows its seams before it opens a file. **Unit 391 listed the 145 rows in `docs/phase-cw/unit391-seams.md`; unit 392 re-checked every row at `7e209cb4`, the commit R53 named, in `docs/phase-cw/unit392-seams.md`.**

**Depends on:** nothing.

## Step 1 - The decoder reads again

**Delivers:** R48 and R50.

**Entry:** step 0 done or partial with 0.1 and 0.2 answered in unit 391's report; the
named commit is `7e209cb4` by R53.

**Exit:**
- [x] 1.1 Every file under `src\Hamlet.RadioEngine\Cw` outside the transmit list in §3 is the named commit's, adapted only where today's app or tests would not build; every adaptation is listed in the report with its file and its reason, and the transmit files are byte-identical to HEAD before the step. **Unit 392: the folder is 7e209cb4's plus six hunks in three files and the kept `CwPitchChoice.cs`, each in the table of `docs/phase-cw/unit392-seams.md` section 8; the eleven transmit files printed nothing against 7e209cb4 at task 2 and at task 4.**
- [x] 1.2 `Hamlet.sln` builds with warnings as errors, both Hamlet test projects included. **Unit 392, task 2: 0 warnings, 0 errors, non-incremental, 22 test files excluded from compilation with their missing names and `tools/Hamlet.PitchRank` out of the solution build, `docs/phase-cw/unit392-seams.md` sections 7 and 9.**
- [x] 1.3 `TheCapturesThatDecodeKeepDecodingTests` and `TheAdjudicatedReadingsKeepReadingTests` are green at HEAD, every case, in one filtered run each, with characters and elements printed beside every floor; `CwFixtureTests.TheCleanRecordingsDecodeExactly` is run and its two cases reported with what they read, red or green (R53: they are step 3's). **Unit 392, task 3: captures 37 of 37 green in 97 s, adjudicated 13 of 13 in 33 s, the two clean synthetics red reading placeholders, every case in `docs/phase-cw/unit392-floors.md`.**
- [x] 1.4 The app's CW tests are green by name: `TheSheetSaysWhatEachElementWasSentAtTests`, `ReturningToCwShowsCwTests`, `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint`, `VoiceTests`. **Unit 392, task 3: 13 of 13 green in one invocation, `docs/phase-cw/unit392-floors.md`.**
- [x] 1.5 The carry-forward list is green on both lines, and nothing is red that was green at the step's entry. **Unit 393: engine 150 of 150 in 301 s at task 0, and the app line after `TheOliviaRowsTests` read its telemetry file with `FileShare.ReadWrite` 277 and 275 of 278 in two runs, every loss the dispatcher loop before an assertion and green in the other run, no red on an assertion.**
- [x] 1.6 `git diff` over `src\Hamlet.App` between the step's entry and its exit is empty, or every hunk is listed in the report with the seam it serves. **Unit 392: three changes in `MainWindowViewModel.cs`, each with its seam in `docs/phase-cw/unit392-seams.md` section 10.**

**Depends on:** step 0.

## Step 2 - CW cannot break silently again

**Delivers:** the fifth row of the carry-forward table.

**Entry:** step 1 done, the three floor tests green at entry.

**Exit:**
- [x] 2.1 `docs\carry-forward-tests.txt` carries a CW read guard on the engine line and a CW row in its guard table beside FT8, FT4, PSK31 and Olivia, naming the test and the unit that added it. **Unit 393: line 9 carries `TheAdjudicatedReadingsKeepReadingTests` whole and the thirteen `cw-2026-08-25` cases of `TheCapturesThatDecodeKeepDecodingTests` by display name, and the table has a CW row naming both and unit 393.**
- [x] 2.2 The guard was watched red against a deliberately broken decoder in an uncommitted change and green after the change was taken back, both runs in the report; a guard that has never refused is not a guard. **Unit 393: against a `CwDecoder.Process` that hands nothing on, 20 of 26 red - every case that asserts a decode, the other six retired readings and the shortfall printer - and 26 of 26 green with the file put back, both runs in `docs/phase-cw/unit393-guard.md`.**
- [x] 2.3 The engine invocation with the guard on it completes inside its timeout, measured, with the wall time before and after; if the whole of the three floor tests does not fit, the guard is the 08-25 captures and the two clean synthetics selected by display name, and the report says which and why. **Unit 393: 150 of 150 in 301 s before, 176 of 176 in 372 s after, against 480; the guard is the 08-25 captures and the adjudicated type, the synthetics off it as known reds under R53.**
- [ ] 2.4 The carry-forward list is green on both lines after the change, and nothing is red that was green at entry.

**Depends on:** step 1.

## Step 3 - The inherited reds are gone

**Delivers:** R49's second half. The pile is emptied without a ruling per test.

**Entry:** step 2 done, the three floor tests green at entry.

**Exit:**
- [ ] 3.1 Every name in `docs\unit239-failing-set.txt` and every CW name in the known-reds block of `docs\carry-forward-tests.txt` is run at HEAD by type, one type per invocation with its own timeout, and listed in the report as green, red-repaired, red-retired, or red-open with its number.
- [ ] 3.2 Every retirement meets R49: the report quotes the class or method the test names that does not exist under `src\Hamlet.RadioEngine\Cw` at HEAD, and `docs\cw-retired-tests.txt` carries the test's full name, the missing name, the unit, and the date.
- [ ] 3.3 No test that reads audio and asserts characters, elements, a tone or a speed is retired; each such red is green or is listed red-open with its number and the reason it stays.
- [ ] 3.4 The known-reds block of `docs\carry-forward-tests.txt` names no CW test, and `docs\unit239-failing-set.txt` carries a closing line naming this phase and the count that went each way.
- [ ] 3.5 The three floor tests are green at the exit of every commit of the step, and the carry-forward list is green on both lines.

**Depends on:** step 2. Independent of step 4: when one blocks the arbiter works the other.

## Step 4 - The August rework is judged on numbers

**Delivers:** R51.

**Entry:** step 2 done, the three floor tests green at entry, the commit list from 4.1 written before any piece is re-applied.

**Exit:**
- [ ] 4.1 Before any piece goes back in, the report lists every commit between the commit step 0 named and 2026-09-03 that touched `src\Hamlet.RadioEngine\Cw`, from `git log`, with the files each touched and what its message claimed.
- [ ] 4.2 Each piece is re-applied in its own commit and judged in the same unit: kept only if the three floor tests stay green and one named number moves - 021410 above its floor of 47 characters or its text nearer `WEEKEND`, `THINKING`, `FLEX` than `ATEEKEND`, `TTHINKING`, `FLENX`; 013637 nearer `ABOVE`, `BREEZE` than `AB OVE`, `BREE Z E`; or any capture above its character floor with no capture below its own.
- [ ] 4.3 A piece that moves nothing is taken back out in the next commit, and the report says which and what it measured.
- [ ] 4.4 Every kept piece's numbers are written into the floor table as the new floors, and the floors only rise.
- [ ] 4.5 Section 3 of the last report of the step leads with one table: piece, number before, number after, kept or out.
- [ ] 4.6 The three floor tests and the carry-forward list are green at exit.

**Depends on:** step 2. Independent of step 3: when one blocks the arbiter works the other.

## Step 5 - Tim at the radio

**Delivers:** his verdict.

**Entry:** steps 3 and 4 done or partial with the three floor tests green.

**Exit:**
- [ ] 5.1 Tim, at the radio, on CW on 40 m, sees text on the CW tab that reads as what was sent, and says it read. No script can evaluate this.   *owner's verdict*

**Depends on:** steps 3 and 4.

## §5 Dependencies

Step 0 depends on nothing. 0 to 1 to 2 is one pipeline. Steps 3 and 4 both depend on step
2 and on nothing else, so when one blocks there is somewhere to route: the arbiter works
the other, and comes back. Step 5 waits on both. When step 0, 1 or 2 blocks there is
nowhere to route - work it or halt.

## §6 Branching

- **Three stops only**: keying, transmit or the radio's safety; money past the budget; a
  fact the product states to the operator about a signal, a station or a send. A test's
  shape, a floor's number, a seam, a filter, a timeout: decide, mark author's, continue.
- **The later ruling wins. A done step is closed. Every remaining step Tim's: halt.**
- **A run lost before any assertion** - the test host crash inside `Cw` (HM-OPEN-063) or
  the headless dispatcher loop - counts neither way and is re-run once; a red on an
  assertion is red and is never re-run.
- **No commit since 2026-08-25 is green on all three floor tests**: take the newest commit
  green on `TheCapturesThatDecodeKeepDecodingTests` alone, name it in 0.2 with that
  qualification, and carry the other two into step 3 as repairs. Author's, overrulable.
- **The restore does not build after the seams are adapted**: `MOVE: work around` with
  the build errors listed; if the loop test fires, `MOVE: cut down` at partial with the
  errors in the report. Never restore an app file to make it build (R50).
- **A CW test costs more than 300 s** (`TheIntegratorBandwidthTable.Write` was measured at
  362 s): it never goes on a carry-forward line and is run alone with its own timeout.
- **A piece in step 4 helps only with another**: the arbiter may re-apply two together
  once, says so, and the pair is judged as one piece under 4.2.
- **A floor would have to be lowered to go green.** Never. Report the number, `partial`.
- **Anything would change what keys or transmits, or a byte of the transmit files in
  §3.** `MOVE: stop`.
- **A package is needed.** `MOVE: stop`.
- **The `UNIT:` line of every report carries no parentheses**, and no `&`, `|`, `<`, `>`
  or `^`. `validate-output.bat` rule 1 echoes that line inside a parenthesized block, so a
  `)` in it ends the block early and kills the run before its exit code is recorded; the
  loop then halts at stop 11 with the unit's work done and unjudged. Units 390 and 391 both
  died this way on 2026-09-22. Until the layer is repaired, the instruction says so and the
  unit obeys it. Write *tasks 0 to 3, none dropped* with commas, never in brackets.
- **A file must be deleted.** A retired test file is deleted in its own commit with the
  reason in the message and in `docs\cw-retired-tests.txt`; any other file is emptied,
  commented, listed.

## §7 Carried

`PHASE_GOAL.md`'s 80% by edit distance and the adjudication it needs; the scanner (phases
6 to 8 of `DECODER_AND_SCANNER_BRIEF.md`); the keying sweep on screen, `competing: none
found`, the unclamped scores and duty in the sidecar (the 08-25 analysis, Fix 5); the
keying meter's 8 MB a reading (HM-OPEN-070); the test host crash inside `Cw`
(HM-OPEN-063), chased only as far as the re-run rule above; the CW tab's dead *Have a
look* button (HM-OPEN-087); every open ask of the hardening phase from unit 390's queue,
including hardening 5.1.

## §8 Revision record

- **2026-09-22.** Written from the interview: R47 to R52; six steps; the three floor tests
  named from the tree; the transmit files fenced in §3.
- **2026-09-22, night.** R53 from unit 391's report: step 1 restores to `7e209cb4`; 1.3
  reworded so the two clean synthetics are reported, not required, at step 1; §6 gains the
  `UNIT:` line rule after units 390 and 391 died in `validate-output.bat` rule 1.
