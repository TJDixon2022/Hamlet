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

**R54 - Tim, 2026-09-22, night: the loop runs all night.** *"What I want is for CW to work and
this unit to run all night."* Two consequences, both binding on the arbiter:

1. **Unit 389's item 1 is answered: parked.** 9.2 stays as built. Its two shortfalls (one of
   four controls greyed, the hold ending at hand-back rather than carrier drop) belong to the
   hardening phase and are not touched in this one. Answered asks leave the carried list
   (HM-DEC-139), so no unit carries it again and no arbiter stops on it again.
2. **A ruling is wanted only when a criterion of the step being worked cannot be met without
   it.** Every ask carried from before this phase, and every finding a unit raises that
   touches keying, transmit, money or a product fact but blocks no criterion of this plan,
   is parked, not stopped on: the unit writes it to `docs\phase-cw\PARKED.md` with its unit
   number and one line, the arbiter reads that file as the answer "parked to a later phase,"
   and section 4 of a report carries only what blocks a criterion of the step in hand. The
   three stops in §6 stand for the work itself: a unit that would have to change what keys
   or transmits, spend past the budget, or change a fact the product states, in order to
   meet a criterion, stops. A unit that merely notices such a thing parks it.

**R55 - Tim, 2026-09-23, morning: the loop keeps working the decoder before he judges.**
Ruled B. With every loop criterion ticked and 5.1 the only one open, the phase halted on
his verdict while eight reds stayed open under R49 and fourteen rework pieces were never
tried. He chose to extend rather than close: **3.6** takes the eight by number, **4.7**
takes the fourteen as chains, and 5.1 waits until both have a verdict. Rejected: ticking
5.1 now and carrying both into the next phase (the next phase is to be interviewed on a
correctness number, not on this pile); closing by ruling as done-partial. Standing note
from the same ruling: a capture Tim transcribes himself is the next phase's step 0, and
nothing in 3.6 or 4.7 is a substitute for it.

**R56 - Tim, 2026-09-23, 16:00: the tone tracker may be attacked in this phase.** Unit 405
traced one cause under four of 3.6's reds: `CwToneTracker.Switch` moves the mixdown 25 to 85
Hz off a single sender and reports the wrong pitch as measured (700 for 640, 575 for 615).
The same disagreement appears on the air in `cw-2026-09-23-173723`, where the decoder says 600
Hz and the independent sweep says 575 in the same capture. HM-DEC-095 and HM-DEC-127 still
govern that code and are not overruled; what R56 grants is leave to change the switch inside
this phase, with the floors, the adjudicated anchors and the 17:37 key as the guard. Rejected:
parking the four and giving the tracker its own phase - *"A"*, and the phase's sentence is that
CW decodes, which is what the tracker prevents.

**R57 - Tim, 2026-09-23, 16:00: a floor counts named characters, not placeholders.** *"I hate
all the false positive garbage."* The floors count every character emitted, and a placeholder
(`■`) is a character, so unit 405's two working changes were thrown away for lowering rows that
were nothing but placeholders, and every later unit failed the same way. From now: **a floor is
the count of named characters; a row that falls solely because placeholders were suppressed is
not a floor lowered.** All 37 capture rows are re-measured once, in one commit, with the old and
new number printed side by side, and the adjudicated anchors and the 17:37 key are the
independent check that nothing real was lost in the re-measurement. Rejected: a second number
beside the old one (two numbers per row and still a rule needed for which wins); leaving it (the
gate work could never be kept).

**R58 - Tim, 2026-09-23, 16:00: the gate first, then the tracker.** *"both baby"*, in that
order: 3.7 is the emission gate, 3.8 is the tracker. 3.6's four remaining reds - #15, #43, #44,
#45 - get one pass under R57 at the start of 3.7's first unit, since R57 makes 405's greens
keepable, and whatever is still red is then parked in `docs/phase-cw/PARKED.md` with its
numbers; 3.6 closes on that pass either way, and no unit attacks a 3.6 red after it. Unit 405's
**G1** (an out-of-order gap reading is not separated) is picked back up in the same pass: it was
measured at zero cost across every floor and type and died only to the old keep rule.

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
- [x] 1.3 `TheCapturesThatDecodeKeepDecodingTests` and `TheAdjudicatedReadingsKeepReadingTests` are green at HEAD, every case, in one filtered run each, with characters and elements printed beside every floor; `CwFixtureTests.TheCleanRecordingsDecodeExactly` is run and its two cases reported with what they read, red or green (R53: they are step 3's). **Unit 392, task 3: captures 37 of 37 green in 97 s, adjudicated 13 of 13 in 33 s, the two clean synthetics red reading placeholders, every case in `docs/phase-cw/unit392-floors.md`; the two clean synthetics green 2 of 2 from unit 400.**
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
- [x] 2.3 The engine invocation with the guard on it completes inside its timeout, measured, with the wall time before and after; if the whole of the three floor tests does not fit, the guard is the 08-25 captures and the two clean synthetics selected by display name, and the report says which and why. **Unit 393: 150 of 150 in 301 s before, 176 of 176 in 372 s after, against 480; the guard is the 08-25 captures and the adjudicated type, the synthetics off it as known reds under R53; the two clean synthetics joined the engine line by unit 401 once green, 178 of 178 in 374 s.**
- [x] 2.4 The carry-forward list is green on both lines after the change, and nothing is red that was green at entry. **Unit 393, task 4: app 278 of 278 in 159 s and engine 176 of 176 in 373 s with the guard on it, nothing lost; the floors 37 of 37 and 13 of 13, the clean synthetics 0 of 2 as at entry.**

**Depends on:** step 1.

## Step 3 - The inherited reds are gone

**Delivers:** R49's second half. The pile is emptied without a ruling per test.

**Entry:** step 2 done, the three floor tests green at entry.

**Exit:**
- [x] 3.1 Every name in `docs\unit239-failing-set.txt` and every CW name in the known-reds block of `docs\carry-forward-tests.txt` is run at HEAD by type, one type per invocation with its own timeout, and listed in the report as green, red-repaired, red-retired, or red-open with its number. **Unit 394, tasks 1 and 2: all 51 classified in `docs\phase-cw\unit394-reds.md` - 30 green, 0 red-repaired, 0 red-retired, 21 red-open (20 red on an assertion, 1 excluded from compilation and read from source), the known-reds block's one CW name being line 41 of the set. #1 re-included by unit 398, green, 3 of 3 facts, so the set stands at 31 green and 20 red-open. #25, #26, #31 and #32 green by unit 400 on the harness reading the settled transcript and the regenerated clean fixtures under HM-DEC-127, the set at 35 green and 16 red-open; #30 and #33 green by unit 400 on the regenerated prosigns fixture, the set at 37 green and 14 red-open. #18 to #23 green by unit 401 on the settled transcript and a band of 0.005, the set at 43 green and 8 red-open. #24 and #41 green by unit 402 on a follow under half the passband no longer counting as a speed discontinuity, the set at 45 green and 6 red-open. #6 green by unit 406 on a held tracker switch no longer going when the survey it goes on finds the keying back where the tracker listens, and #42 green by unit 406 on its event giving the decoder the radio's report of the operator's own sending under HM-DEC-147, the set at 47 green and 4 red-open.**
- [x] 3.2 Every retirement meets R49: the report quotes the class or method the test names that does not exist under `src\Hamlet.RadioEngine\Cw` at HEAD, and `docs\cw-retired-tests.txt` carries the test's full name, the missing name, the unit, and the date. **Unit 398, task 2: 20 tests retired from 7 of the 22 excluded files - 3 files deleted whole, 4 trimmed - each line in `docs\cw-retired-tests.txt` with its full name, the missing name as grep printed it under `Cw` at HEAD (one, `CwToneTracker.CoarseSpacingHz`, private at HEAD), unit 398 and 2026-09-23; `dotnet build Hamlet.sln -warnaserror` green after the last retirement; 31 facts left excluded and red-open because they assert characters, elements, a tone or a speed; the classification and the grep in `docs\phase-cw\unit398-excluded.md`.**
- [x] 3.3 No test that reads audio and asserts characters, elements, a tone or a speed is retired; each such red is green or is listed red-open with its number and the reason it stays. **Unit 394, task 2: nothing retired; all 21 reds of the set read audio and are listed red-open in `docs\phase-cw\unit394-reds.md` section 2, each with its number and what it asserts - characters 14, share 5, speed 2. Unit 398, task 2: 20 retired from the excluded files, none asserting characters, elements, a tone or a speed from audio; the 31 excluded facts that do are left excluded and listed red-open in `docs\phase-cw\unit398-excluded.md` section 2.**
- [x] 3.4 The known-reds block of `docs\carry-forward-tests.txt` names no CW test, and `docs\unit239-failing-set.txt` carries a closing line naming this phase and the count that went each way. **Unit 401: lines 158 and 159 of the known-reds block replaced by one CW line naming no test and pointing at the set's closing line and `unit394-reds.md`; grep over the file at exit finds no CW test named as a known red; the closing line written - 51 names, 31 green at HEAD without repair, 12 repaired, 0 retired, 8 red-open by number (#6, #15, #24, #41, #42, #43, #44, #45); the step's goal sentence stays partial on the 8 red-open under R49's own clause, red with its number and the step partial, each a repair owed under HM-DEC-151; `CwFixtureTests.TheCleanRecordingsDecodeExactly` on the engine line, 178 of 178 in 374 s of 480.**
- [x] 3.5 The three floor tests are green at the exit of every commit of the step, and the carry-forward list is green on both lines. **Unit 400: captures 37 of 37 and adjudicated 13 of 13 at the exit of every commit of the step since `ee0ea0dc`; the two clean synthetics red at every commit before `7d1ffde6` under R53, which named them step 3's repair, and green 2 of 2 at the exit of every commit from it; app 277 of 278 in each of two runs, each loss a different name to the dispatcher loop before an assertion and green in the other run, and engine 176 of 176 in 374 s at the unit's exit. The tick stands while every later commit of the step keeps all three green and both lines green; a later unit that finds one red un-ticks it and names the commit.**
- [ ] 3.6 Each of the eight reds open at unit 401's closing line - #6, #15, #24, #41, #42, #43, #44 and #45 of `docs\unit239-failing-set.txt` - has a verdict: green by repair of the decoder with no floor lowered and the three floor tests green at that commit's exit, or, after three consecutive units have each attacked it and measured no movement, parked in `docs\phase-cw\PARKED.md` as owed with its number and the three measurements; none retired, and the closing line of the set updated with the final count. **Under R58 this criterion closes on the single pass at the start of 3.7's first unit: any of #15, #43, #44, #45 still red after it is parked with its numbers and no further unit attacks it.**

- [ ] 3.7 The emission gate no longer prints what the decoder does not know: on `cw-2026-09-23-173723`, on the three adjudicated anchors and on all 37 capture fixtures, no character is printed whose own span is below the gate's stated bar, no named character is lost anywhere, the three adjudicated readings are unchanged character for character, and the scored region of the 17:37 key reads no worse than it does at entry; the bar and the before-and-after counts of named and placeholder characters are in the report for every case.
- [ ] 3.8 The decoder and the independent sweep agree about the pitch: on every single-sender case among the 37 captures, the three anchors and the 17:37 capture, the pitch the decoder mixes at and the pitch the 400 to 1200 Hz sweep reports differ by no more than one 25 Hz bin, measured and tabled per case; and each of #15, #43 and #44 is green or carries its post-R56 measurement.
- [ ] 3.9 All 37 capture rows are re-measured once under R57 in a single commit, with the old count, the new named-character count and the placeholder count printed per row; the adjudicated anchors and the 17:37 key are run at that commit and are unchanged or better, and the report states plainly that no real character was lost.
**Depends on:** step 2. Independent of step 4: when one blocks the arbiter works the other.

## Step 4 - The August rework is judged on numbers

**Delivers:** R51.

**Entry:** step 2 done, the three floor tests green at entry, the commit list from 4.1 written before any piece is re-applied.

**Exit:**
- [x] 4.1 Before any piece goes back in, the report lists every commit between the commit step 0 named and 2026-09-03 that touched `src\Hamlet.RadioEngine\Cw`, from `git log`, with the files each touched and what its message claimed. **Met by unit 395: 84 commits from `07f0397a` to 2026-09-03, 39 in the tree by R53's restore and the 45 pieces numbered oldest first, none touching a transmit file, in `docs\phase-cw\unit395-rework.md` section 1.**
- [x] 4.2 Each piece is re-applied in its own commit and judged in the same unit: kept only if the three floor tests stay green and one named number moves - 021410 above its floor of 47 characters or its text nearer `WEEKEND`, `THINKING`, `FLEX` than `ATEEKEND`, `TTHINKING`, `FLENX`; 013637 nearer `ABOVE`, `BREEZE` than `AB OVE`, `BREE Z E`; or any capture above its character floor with no capture below its own. **Ticked by unit 397 at piece 45: 45 of 45 judged across units 395 to 397, each in the unit that took it up. 27 applied in their own commit or a pair commit and measured or refused by the build; 0 kept on a named number - no piece improved one; 14 listed dependent on a chain longer than section 6's pair and never applied - `4786c7e7`, `f2e1db7a`, `386fdb5d`, `4c6e4321`, `0f2089f3`, `62262b94`, `fc1ee77f`, `68a18d66`, `a91d8fe7`, `efcd5242`, `aeea24f2`, `a37cfcff`, `ee2cba8d`, `9c2a7f99`; and 4 never applied for want of anything to apply - `39a42c3f` and `865e66d8` already in the tree as unit 392's seams, `501e8e2d` empty, `3d4694e5` comment only, judged as a pair on piece 5's run. Section 6 licenses a pair once and no more, so the 14 were never run; a reader holding the step partial on them is reading this criterion's letter. Rows in `docs\phase-cw\unit395-rework.md` section 2.**
- [x] 4.3 A piece that moves nothing is taken back out in the next commit, and the report says which and what it measured. **Ticked by unit 396, re-confirmed by unit 397 at 45 of 45: every out piece of units 395 to 397 - the 27 that were applied each reverted in the commit after its piece commit, or after its seam or drop follow-up, checked on the log `ee0ea0dc..HEAD`, and the 18 never applied left nothing to revert; every row in `docs\phase-cw\unit395-rework.md` section 2 carries its measurement or why none ran.**
- [x] 4.4 Every kept piece's numbers are written into the floor table as the new floors, and the floors only rise. **Ticked by unit 397: no piece kept, no floor moved, the floor table byte-identical to the step's entry - `git diff --stat ee0ea0dc HEAD` over both floor test files prints nothing - and no floor lowered.**
- [x] 4.5 Section 3 of the last report of the step leads with one table: piece, number before, number after, kept or out. **Ticked by unit 397: its report's section 3 leads with the 45-row table, rows 1 to 33 from units 395 and 396 and rows 34 to 45 its own.**
- [x] 4.6 The three floor tests and the carry-forward list are green at exit. **Ticked by unit 397's exit round on `3af36501`'s tree, `src` byte-identical to `5688a8a5`: captures 37 of 37 in 92 s with every row identical to entry, adjudicated 13 of 13 in 29 s; app line 278 of 278 in 167 s, engine line 176 of 176 in 374 s of 480; the two clean synthetics red at both ends, 0 of 2 reading placeholders, under R53.**
- [x] 4.7 Each of the fourteen pieces unit 395 listed as dependent on a chain longer than a pair - `4786c7e7`, `f2e1db7a`, `386fdb5d`, `4c6e4321`, `0f2089f3`, `62262b94`, `fc1ee77f`, `68a18d66`, `a91d8fe7`, `efcd5242`, `aeea24f2`, `a37cfcff`, `ee2cba8d`, `9c2a7f99` - is applied inside its chain, each chain in its own commit and judged as one piece under 4.2's rule, kept or taken back out under 4.3, floors raised under 4.4 if kept, and a row per chain added to 4.5's table; a chain that will not build after its seams are adapted is listed with its errors and counts as out. **Ticked by unit 404: the fourteen traced by `git apply --check` into two chains after the shared-piece merge, in `docs\phase-cw\unit404-chains.md` - S, the survey, 4 pieces carrying `4786c7e7` and `f2e1db7a`; D, the decoder, 24 pieces carrying the other twelve. 2 chains judged, 0 kept, 2 out: S built clean and moved nothing, `3c742c4a` reverted in `675d846f`; D built after five unit 392 seam members were met and turned three captures and both clean synthetics red, `bfd97867` reverted in `264b97ce`. 14 of 14 with a verdict, no floor raised. Rows in `docs\phase-cw\unit395-rework.md` section 2 under *Chains under 4.7, judged by unit 404*. Floors green at exit: captures 37 of 37 every row identical to entry, adjudicated 13 of 13, clean synthetics 2 of 2.**

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
- **A stop is for the work, not for a mention.** Under R54 the arbiter stops only when a
  criterion of the step it is working cannot be met without a ruling on one of the three.
  A carried ask, a section-4 finding, a remark in a report or a doc that touches one of the
  three but blocks no criterion is parked in `docs\phase-cw\PARKED.md` and the loop goes
  on. Unit 389's item 1 is the first entry there, answered "parked" by R54.
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
  once, says so, and the pair is judged as one piece under 4.2. **Under 4.7 (R55) a chain of
  any length is one piece**, applied in one commit and judged once.
- **A red under 3.6 that three consecutive units have attacked without movement is parked,
  not chased**, and the criterion closes on that verdict; the loop is never held on a single
  test.
- **A floor would have to be lowered to go green.** Never. Report the number, `partial`.
- **Anything would change what keys or transmits, or a byte of the transmit files in
  §3.** `MOVE: stop`.
- **Suppressing a placeholder is not lowering a floor (R57).** A row that falls only in its
  total while its named-character count holds or rises is not a regression; a row whose named
  count falls is, and the change goes back out.
- **A change to `CwToneTracker` is licensed by R56** and judged by 3.8's per-case table, the
  floors' named counts, the three anchors unchanged, and the 17:37 key's scored region. A
  tracker change that improves the table and costs a single anchor character goes back out.
- **The 17:37 key is inferred, not transcribed.** No report may call it what was sent. Every
  number measured against it is stated as *against an inferred key* (§0.0, FACT-004), and the
  unscored first third of that recording is never keyed.
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
- **2026-09-23, 16:00.** R56 the tracker is open; R57 a floor counts named characters; R58 the
  gate first then the tracker, and 3.6 closes on one pass under R57. New criteria 3.7 the
  emission gate, 3.8 the pitch agreement, 3.9 the re-measurement. Written after seven units
  (401 to 407) tried thirty-four changes and kept none, two of them green but refused by the
  old floor rule, and after `cw-2026-09-23-173723` read `CQ CQ CQ DE WB6RED WB6RED` with the
  letters right and the spacing wrong.
- **2026-09-23, morning.** R55: 3.6 (the eight reds by number, green or parked after three
  tries) and 4.7 (the fourteen chained pieces, each chain one commit, one verdict); §6's pair
  rule extended to chains under 4.7 and a three-tries rule for 3.6. Written after the loop
  halted at stop 1 with every loop criterion ticked.
- **2026-09-22, late.** R54: 9.2 parked; a ruling is wanted only when a criterion of the step in
  hand needs it; carried asks and findings that touch the three stops but block nothing go to
  `docs\phase-cw\PARKED.md`. Written after the loop halted at stop 3 on unit 389's carried ask.
- **2026-09-22, night.** R53 from unit 391's report: step 1 restores to `7e209cb4`; 1.3
  reworded so the two clean synthetics are reported, not required, at step 1; §6 gains the
  `UNIT:` line rule after units 390 and 391 died in `validate-output.bat` rule 1.
