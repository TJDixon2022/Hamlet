# Work instruction 243 - what the ladder already is, and what this shell will actually run

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## THE TOOL RULE - read this before task 1

**This session's shell may refuse writes and may refuse `dotnet`.** It happened on
2026-09-04: `mv`, `mkdir`, `git mv` and `echo hi > file.txt` were all refused
inside `C:\Source\HamLet`, against an allow-list naming that exact directory, and
setting the sandbox override on the call did not change the answer.
`RUN_LEDGER.md` shows between 5 and 28 denied calls in every unit for the past
week.

**The file-editing tools were unaffected throughout and wrote normally.**

Standing for this unit and every unit of this phase:

- **A refused shell call is a signal to reach for the other tool, not to stop.**
  Writing a file: use the file-editing tools. Reading a file: read it. Listing a
  directory: list it. Only building and testing genuinely need the shell.
- **Record every refusal verbatim** - the call, and the exact text that came back.
  That record is task 2's deliverable and is worth as much as anything else here.
- **Nothing in this unit halts the loop.** Every task below completes with a dead
  shell. Where a task cannot reach its measurement it reports the refusal and
  moves on. **Reporting a refusal is succeeding at this unit, not failing it.**

---

## Why this unit exists

The phase layer is already installed - Tim ran `install-phase.bat` by hand before
launching, because the previous attempt put that file-moving work inside a unit
and lost a night when the shell refused it. **This unit moves no files.**

**The phase's subject is 1.5 dB.** `HM-OPEN-067`: the 50 per cent crossing near
**-19.5 dB** against a published **-21**, 306 trials a rung, the SNR axis checked
against a second instrument sharing no line of code with the first and agreeing to
0.0098 dB mean. Unit 222 found the loss in no single stage - oracle alignment,
unquantised magnitudes, physics ratios and four times the iteration bound each
landed inside the as-is interval. **At -21 dB the hard decisions carry about 31
bit errors against a code that recovers to zero at 17.** The demodulator is fine;
belief propagation gives up while the answer is still reachable.

**A run of the previous instruction already surveyed part of this and reported the
ladder largely exists.** If its `output.md` is still at the root, read it as a
lead rather than a finding, and check what it says against the tree.

```
PHASE GOAL:   Hamlet reads FT8 as well as the best decoder there is, and then
              reads it further.
UNIT GOAL:    The phase knows exactly what measuring apparatus it already has,
              and exactly what this environment will and will not run.
ADVANCES:     step 0
```

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
Report them; do not repair the instruction.

- `PHASE_PLAN.md`, `PHASE_STATUS.md` and `PHASE_OUTCOME.md` at the root are the
  **new** phase's, and `docs/phase-ft8/` holds the closing phase's. **If the root
  files still name the FT8 phase, `install-phase.bat` was not run** - say so first
  in the report, skip task 5, and carry on with everything else.
- `HM-OPEN-067` in `OPEN_ISSUES.md` carries the ladder's figures.
- Units 218, 221 and 222 built and ran ladders.
- Root version `1.12.45` after work instruction 241. `Ft8Sharp` `0.10.7`.

Expected to fail: `CwAdjudicationTests.ASpeedChangeInRealisticAudio`, and the 51
inherited CW reds in `docs/unit239-failing-set.txt`. Do not chase either.

---

## Rulings in force for this phase

Transcribed from `PHASE_PLAN.md`. **Not to be re-argued.**

**The seam is split.** `Ft8Sharp` stays a faithful MIT port of `ft8_lib`,
byte-identical in behaviour, and **nothing in this phase changes a line of it.**
Improvements live in `Ft8Sharp.Deep`. The port's value now is that it cannot
drift: every measurement is taken against something known-identical to upstream.

**`Ft8Sharp.Deep` is GPL-3.0.** Ruled by Tim, 2026-09-04. **Do not raise it.**

**There is no WSJT-X on the development machine and no unit may assume one.** A
unit that cannot close a real-air criterion marks it deferred, names the fixture
it needs in `OPEN_ISSUES.md`, closes on the ladder and continues. It never
substitutes `decode_ft8.exe`, which is `ft8_lib` and therefore the thing being
improved on.

**A wrong decode is counted separately from a missed one, everywhere.** A message
returned that was not sent is the one failure this phase cannot trade against rate
(§0.0).

**No algorithm comes from WSJT-X's source or `ft4_ft8_public/`.** Published
description only, cited at the point of use.

**The steps are a hypothesis, not a contract.** `PHASE_PLAN.md` grants leave to
reorder, replace, retire and add steps and to move a target measured wrong, all
without asking, with the record as the only constraint.

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, and
`NOTE` saying what is moving inside the task. The same every ten minutes while a
task is running. **Use the file-editing tools if the shell refuses.**

---

## Tasks

### Task 1 - what measuring apparatus already exists

**Reading only. This task cannot be refused.**

Report, with file and line:

1. **The message source** - what text the ladder transmits, and whether it varies
   per trial.
2. **The signal synthesiser** - where a message becomes audio, at what sample
   rate, and how tone, timing and phase are produced.
3. **The noise delivery and its SNR calibration** - how a commanded SNR becomes a
   noise amplitude, what reference bandwidth is assumed, and where that constant
   lives.
4. **The verification instrument.** Unit 222 checked the delivered SNR against a
   second instrument sharing no line of code with the first, agreeing to 0.0098 dB
   mean. **Say whether that instrument is still in the tree**, and where.
5. **The trial loop** - how a rung is run, how a result is scored, and whether a
   wrong decode is distinguished from a missed one today.
6. **What is missing** for the ladder to be run by any unit on demand, at a
   commanded rung and trial count, seeded and reproducible.

**Say what you find, not what this instruction expects.** If the apparatus is
better than described, say so; if it is scattered across three test projects and
unusable as a harness, say that.

### Task 2 - what this environment will actually run

**This task makes every later unit of the phase cheaper, and it cannot fail:
whatever happens is the result.**

Probe the shell in this order, recording for each the exact call and the exact
response, verbatim:

1. `dotnet --version`
2. a directory listing through the shell
3. `mkdir` of a scratch directory under the repository
4. a redirect write: `echo hi > <scratch>\probe.txt`
5. `git status --short`
6. `dotnet build` of `src/Ft8Sharp/Ft8Sharp.csproj`
7. `dotnet test` of `tests/Ft8Sharp.Tests` filtered to one test
8. a `.bat` under `tools\arbiter\`

Then write **`docs/shell-probe-243.md`**: a table of call, verdict, and the
refusal text where refused, and the same list for the file-editing tools. **Write
it with the file-editing tools, so it exists whatever the shell does.**

Also record whether `.run-unit\denials.txt` exists and what it holds for this run,
and whether `.run-unit\allowed.txt` names the spellings that were refused. The
`validate-output.bat` permitted-spellings bug has refused for ten units and this
may be the same fault. **Say whether it looks like the same fault. Do not repair
it.**

### Task 3 - the baseline, attempted

**Goal: reproduce 13 of 306 - 4.2 per cent, 0 wrong - at a delivered -21 dB.**

- If task 2 showed `dotnet` runs: run it, and report the rate with its Wilson
  interval and the wrong-decode count. Also run **-19 dB and -20 dB** against unit
  221's 81.0 and 23.9 per cent, so the shape is checked and not one rung.
- **If it reproduces**, commit it as the baseline.
- **If it does not**, that is the headline finding and **not a stop**: record both
  figures with the delivered SNR each was measured at, adopt this measurement as
  the baseline with its provenance, and note that every target in `PHASE_PLAN.md`
  moves by the same offset. **The relative gain is what this phase measures; the
  absolute figure is a label on the axis.**
- **If `dotnet` is refused**, say so plainly, write down the exact command a next
  session should run, and go to task 4. The measurement is this unit's goal; it is
  not its gate.

### Task 4 - the ladder becomes a harness any unit can run

Only the parts task 2 showed are possible. **Design and write the code even if it
cannot be built** - a file written is progress a next session does not repeat.

- One entry point, taking a rung, a trial count and a seed, deterministic.
- **Three counts, never two**: decoded correctly, missed, **returned wrong**. A
  wrong decode is reported on its own line with the message sent and the message
  returned.
- It takes `Ft8Sharp` today and `Ft8Sharp.Deep` when step 1 creates it, reporting
  them side by side.
- Wall-clock time for a 306-trial rung reported, since every later unit pays it.
- If task 1 found a usable harness, **extend it rather than replacing it** - a
  rebuilt ladder is a different measurement, and task 3's reproduction is what
  decides whether it is the same one.

### Task 5 - the phase's own bookkeeping

**File edits only. No shell needed.**

- `PROJECT_CARD.md` gains the new `PHASE` and `PHASE_SET` from `PHASE_STATUS.md`'s
  header. **This file changes only by ruling** (§13.3), and the ruling is Tim's
  approval of `PHASE_PLAN.md` on 2026-09-04.
- Append that ruling to `DECISIONS.md` in its own format, next id in sequence,
  naming the phase and citing the plan.
- **If `install-phase.bat` was not run, do neither** - say so and leave both files
  alone. A card naming a phase whose plan is not installed is worse than a stale
  card.

### Task 6 - the outcome entry, and the plan's licence to adapt

**This is the named drop candidate.**

- Append this unit's entry to `PHASE_OUTCOME.md` through
  `tools\arbiter\outcome-append.bat`. If the shell refuses it, **append the entry
  with the file-editing tools in exactly the format the existing entries use**,
  and say in the report that the tool was refused and the entry was written by
  hand.
- Then read `PHASE_PLAN.md`'s section *the steps are a hypothesis, not a contract*
  and its table of named alternatives to stopping, and **say in the report that
  you have.** Every later unit depends on that being understood, and a plan
  followed more literally than it was meant is the most likely way this phase
  fails.

---

## Parked - do not touch, do not raise

- **Building `Ft8Sharp.Deep`**, **OSD**, **subtraction**, **baseband re-sync**,
  **SNR measurement**, **cross-slot combining.** Steps 1 to 6. **No unit starts
  one before its scoreboard exists.**
- **The shell permission fault itself.** Record it; do not repair it. It is the
  environment and it is Tim's.
- **`validate-output.bat`'s permitted-spellings bug**, ten units refused.
- **The CW decoder**, the 419 dropped chunks in the 21:58 capture, the 51
  inherited failing cases, the engine project's missing total, the waterfall's
  late first row, `ReusableWindow`, `ProcessDelayForTests`, the tap's owner, unit
  237's Extensible conclusion, work instruction 231's four tree items, the
  101.33 ms pulse above 6 kHz.

---

## What not to do

- **Do not stop because the shell refused something.** Record it, switch tools,
  continue. Nothing in this unit needs the shell to complete.
- **Do not move, archive or install any phase file.** `install-phase.bat` did
  that, or it did not, and either way it is not this unit's.
- **Do not touch `src/Ft8Sharp/`.** The port is the instrument.
- **Do not adjust the ladder to make the baseline reproduce.** If it disagrees,
  that is the finding.
- **Do not report a rate without its wrong-decode count.**
- **Do not assume WSJT-X exists on this machine.**
- **Do not run `Hamlet.App.Tests`** at all in this unit; nothing here touches it.

---

## Committing and pushing

Commit and push each task before starting the next. **If `git` is refused, say so
and carry on** - the files are still written and a next session commits them. Root
version `1.12.45` to `1.12.46` if anything was committed; **if nothing could be
committed, do not bump the version** and say why.

---

## Reporting

`output.md` at the repository root, overwritten, four sections per
`CLAUDE_CODE.md` §8 with the header block above section 1.

**Section 3 leads with two things, in this order:**

1. **What this environment will run**, from task 2 - the table, in one glance.
   Every later unit of this phase is planned against it.
2. **The ladder's rate at -21 dB** with its Wilson interval and wrong-decode
   count, against the 13 of 306 it must reproduce - **or, if `dotnet` was refused,
   the exact command a next session should run to get it.**

**Section 4 names what the next unit should do first**, given what task 2 found.

Write `output.md`, then stop. Do not start the next unit.
