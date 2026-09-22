# Work instruction 391 - the break is measured and named

**Seed under `--seed`.** The first unit of *CW decodes again*, step 0. It builds nothing.
It runs the three CW floor tests at HEAD and writes down every number, finds the newest
commit on `main` where all three were green, names the commit after it that turned one
red, and lists the seams step 1 will have to cross. **Four tasks, drop from the back.**

**Status.** `sh tools/status.sh`, real clock, after every commit and every task, and
immediately before every `dotnet test`. **Write files as UTF-8.**

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite. Only this unit's names and `docs\carry-forward-tests.txt`, run
as its top comment says: two invocations, one build each, a status line immediately
before each. **Never background and poll.** A `dotnet test` with no `--filter` is a
violation, whatever it is for. The engine project has never completed whole and the
watchdog kills at twelve minutes of silence.

**The CW namespace crashes the test host intermittently (HM-OPEN-063)** and
`TheIntegratorBandwidthTable.Write` runs 362 s. **One type per invocation, each with its
own `timeout`.** A run that dies before any assertion is a lost run: record the name it
landed on, re-run it once, and count neither way. A red on an assertion is red and is
never re-run.

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm`
is refused; Python cannot run here; `-m` more than once for a multi-line commit. **A bare
`git worktree`, `git checkout` and `git show` are refused at the prompt** (unit 390's
denials). Multi-step commands go into a script under `.run-unit\unit391-<name>.sh` and
are run with `sh .run-unit/unit391-<name>.sh`, as units 387 to 389 did. `.run-unit\` is
not committed.

## 3. Asks still outstanding

Carried per HM-DEC-139 from unit 390's queue, **verbatim in section 4**: all nine items,
none of them CW, none of them this phase's. Say once that they are carried and not this
unit's to answer.

---

## 4. Why this unit exists

```
PHASE GOAL: CW decodes again.
UNIT GOAL:  The break measured at HEAD in numbers, the commit to go back to
            named, the commit that broke it named, and the seams listed.
ADVANCES:   step 0 criterion 2
DRIFT:      0 - the first unit of the phase
```

**The number this unit is aimed at is unknown, and that is the finding.** The last
measurement of the CW floors in the tree is unit 239's on 2026-09-03:
`docs\unit239-failing-set.txt`, 51 names, six of them cases of
`TheCapturesThatDecodeKeepDecodingTests.EachStillProducesWhatItDid` and two of them
`CwFixtureTests.TheCleanRecordingsDecodeExactly`. No CW test has been run since
2026-09-05. Tim, 2026-09-22: *"The CW currently has taken many steps backwards and no
longer decodes anything."* This unit replaces that sentence with numbers.

**What the record says and this unit checks rather than trusts.** The decoder source
under `src\Hamlet.RadioEngine\Cw` was last changed 2026-08-28 to 08-31 and once on
09-03; the floors in `TheCapturesThatDecodeKeepDecodingTests` were set on 2026-08-25
through that same harness; unit 204 on 2026-08-31 already saw six of them red. So the
commit this unit is looking for is expected between 2026-08-25 and 2026-08-28. **If it
is not there, say so and keep walking back; do not stop at the expectation.**

---

## 5. Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and report
any mismatch. Report the mismatch; do not repair the instruction.

- The three floor tests exist under `tests\Hamlet.RadioEngine.Tests\Cw` by these names:
  `TheCapturesThatDecodeKeepDecodingTests.EachStillProducesWhatItDid`,
  `TheAdjudicatedReadingsKeepReadingTests`, `CwFixtureTests.TheCleanRecordingsDecodeExactly`.
  How many cases each has today.
- `docs\unit239-failing-set.txt` has 51 lines; which are cases of the three above.
- Which CW names the known-reds block of `docs\carry-forward-tests.txt` carries.
- `PROJECT_STATUS.md` reads unit 390 and `Directory.Build.props` reads 1.13.77.
- Every capture the floor table names is on disk under `tests\fixtures\cw\captured`.
- `git log` over `src\Hamlet.RadioEngine\Cw` since 2026-08-24: the commits, their dates.

**Expected reds, so they are not rediscovered:** the six floor cases and the two clean
synthetics in `docs\unit239-failing-set.txt`. Everything else red at HEAD is a finding
to write down, not to chase.

## 6. Rulings in force

**`PHASE_PLAN.md` R47 to R52 and §6**, transcribed there in full with what was rejected;
read them before task 1 and do not re-argue either side. The two this unit stands on:

**R47 - Tim, 2026-09-22:** *"So this phase is get CW working."* *"We've got a bunch of
saved WAV files you can use. So we use those."* Step 0 is the bench: the saved captures
and synthetics through the decoder as it stands, before anything is built. Rejected:
starting from a capture at the radio; starting from git history alone.

**R48 - Tim, 2026-09-22, ruled A:** find the last commit where the floors were green,
bring the decoder back to it, then re-apply the rework's pieces one at a time, each kept
only if the floors stay green. Rejected: repairing the current design forward; measuring
both and halting for a ruling. **This unit finds the commit. It does not restore
anything.**

**`CLAUDE.md` §0.0:** a floor is a count and says nothing about correctness; no sentence
in the report may say the decoder *reads* on the strength of a count. **§0.2:** nothing
that keys is touched. **§12.6:** repair nothing on the way past. **HM-DEC-155**,
**HM-DEC-139**, **HM-DEC-165**, **FACT-004** every result here is an indication,
**FACT-006** this machine has no radio.

**Record this ruling in `DECISIONS.md` at task 0, verbatim, newest first, above
HM-DEC-166:**

```
---
id: HM-DEC-167
date: 2026-09-22
refs: PHASE_PLAN.md, PHASE_STATUS.md, PHASE_OUTCOME.md, PROJECT_CARD.md, docs/phase-hardening-run/, docs/phase-cw/, work instruction 391 task 0, HM-DEC-151
---

**The hardening phase is archived with 5.1 open, and the CW phase - *CW decodes
again* - is the phase in force from today.** Tim, 2026-09-22.

**What is archived, and at what.** *Hamlet holds what it has*, set 2026-09-20, closes
with every criterion a session can move ticked and one left: **5.1**, Tim's verdict at
his window and at the radio. It is his and is not carried into the new phase as debt.
The run folder is `docs/phase-hardening-run/`.

**What is set.** *CW decodes again*: a restore phase. The CW decoder read on the air on
2026-08-25 and reads nothing now; the floors that recorded what it produced have been red
since 2026-08-31 (HM-DEC-151 named them inherited and "not a licence to leave the CW reds
alone forever") and no CW test has been run since 2026-09-05. Six steps: the break
measured and named; the engine's CW code restored to the last commit that read and
adapted so today's app builds; a CW read guard on the carry-forward list; the inherited
reds repaired or retired with reasons; the August rework re-applied one piece at a time
on numbers; and Tim at the radio. The rulings that shape it are R47 to R52 in
`PHASE_PLAN.md`, in his words.

**Why a ruling and not an edit.** `PROJECT_CARD.md` holds standing facts and is changed
only by ruling (CLAUDE.md 13.3), and `PHASE` and `PHASE_SET` are two of them. This entry
is what licenses those two lines moving from the hardening phase to this one.

**Whose words are whose.** The phase name and description are taken from `PHASE_PLAN.md`
as `install-phase.bat` wrote them; the wording above is work instruction 391 task 0's
recording of his ruling, not a session's own conclusion. Nothing was rejected in the
recording.
```

And one row at the top of the table in `CLAUDE.md` §1, in the table's own form, dated
2026-09-22, headline **The CW phase, *CW decodes again*, is the phase in force; the
hardening phase is archived with 5.1 Tim's**, ref HM-DEC-167.

## 7. Status cadence

As the header says. `NOTE` says what is moving inside the task - *floors: 21 of 36 cases
run, 4 red so far* - never the task name.

---

## 8. The tasks

### Task 0 - the record

Append `## UNIT 391 - STEP 0` to `PHASE_OUTCOME.md` in the shape of the existing entries
(`STEP`, `APPROACH`, `MOVE`, `WHY`, `DECIDED`, `LICENCE`, `COST`, `ACCOMPLISHED`, `ENTRY`),
copying the decision block's fields from the foot of this file. `PHASE_STATUS.md` already
names unit 391. Patch-bump `Directory.Build.props` 1.13.77 to 1.13.78. `PROJECT_CARD.md`
`PHASE: CW decodes again` and `PHASE_SET: 2026-09-22`. `DECISIONS.md` HM-DEC-167 and the
`CLAUDE.md` row, verbatim from section 6. **Entry round:** `docs\carry-forward-tests.txt`
both lines, as its comment says. Commit.

**Drop candidate:** none.

### Task 1 - the three floor tests at HEAD (0.1)

This is the measurement, so it is the trace. One invocation per type, filtered,
foregrounded, `timeout 900`, a status line before each:

- `FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests`
- `FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests`
- `FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly`

Use `--logger "console;verbosity=detailed"` so the `_output.WriteLine` lines with the
measured characters and elements beside each floor reach the console, and keep the whole
output in `.run-unit\unit391-floors-head.txt`. **The report's section 3 leads with one
table**: every case, green or red, measured characters and elements, the floor, the
difference. A lost run is re-run once and recorded as lost.

**Drop candidate:** none. Without this table nothing else in the unit means anything.

### Task 2 - the seams (0.4)

From the tree, with `git ls-files` and `grep` in a script: every file under
`src\Hamlet.App` and `tests\Hamlet.App.Tests` that names a type from
`src\Hamlet.RadioEngine\Cw` (`using Hamlet.RadioEngine.Cw` or a `Cw`-namespace type by
name - `CwDecoder`, `CwDecodeReport`, `CwProbabilisticResult`, `CwCharacter`,
`CwTransmitter`, `CwPhrasebook`, `MorseAlphabet`, and whatever else the grep finds), and
for each file the members it touches. This is the list step 1 adapts against; it goes in
section 3 as a table: file, type, members. **Do not judge whether a member exists at the
older commit yet** - that is task 3's, once the commit is known.

**Drop candidate:** the members column. Keep the file-and-type list.

### Task 3 - the last green commit, and the first red one (0.2, 0.3)

`git log --format="%h %ad %s" --date=short -- src/Hamlet.RadioEngine/Cw tests/Hamlet.RadioEngine.Tests/Cw tests/fixtures/cw`
from HEAD back to 2026-08-24. Then, newest first, for each commit that touched
`src\Hamlet.RadioEngine\Cw`: in a script, `git worktree add --detach C:/Source/HamLet-wt391 <hash>`,
run the three floor tests **as that commit has them** inside the worktree (same filters,
same timeout, a status line before each, output kept under
`.run-unit\unit391-floors-<hash>.txt`), then `git worktree remove --force C:/Source/HamLet-wt391`.
Stop at the first commit where all three are green: **that is 0.2**. Then the commit after
it in the walk (the newest one that was red on the way down) is **0.3**, with the cases it
turned red read out of its output.

Report for 0.2: hash, date, message, the count of commits between it and HEAD touching
`src\Hamlet.RadioEngine\Cw`. Then finish 0.4: for every member task 2 listed, whether it
exists in that commit's `src\Hamlet.RadioEngine\Cw` (`git show <hash>:<path>` in the
script, grep the member).

**If no commit back to 2026-08-24 is green on all three:** `PHASE_PLAN.md` §6 - take the
newest commit green on `TheCapturesThatDecodeKeepDecodingTests` alone, name it for 0.2
with that qualification stated, and say which of the other two was red there and how.
**If a floor test does not exist at a candidate commit**, it counts as green there and the
report says so - a test that was not yet written could not have been red.

**The worktree is removed before the unit ends, whatever else happens**, and
`git worktree list` in the report proves it.

**Drop candidate:** 0.3, whole. If the walk is running out of clock after 0.2 is named,
name 0.2, say 0.3 was dropped, and stop.

---

## 9. Parked - do not touch, do not raise

- **Every red at HEAD that is not one of the three floor tests.** Step 3's. Write it in
  the table if it was seen; chase nothing.
- **The test host crash inside `Cw`** (HM-OPEN-063). Recorded when it lands; not chased.
- **The restore itself.** Step 1's. This unit changes no file under `src`.
- **The keying sweep, `competing`, the scanner, the 80% goal.** Carried in the plan's §7.
- **Hardening 5.1 and unit 390's nine items.** Tim's. Carried in section 4 verbatim.

## 10. What not to do

- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**
- **Do not change a file under `src`.** This unit measures. A restore begun here is a
  restore nobody can judge against a number.
- **Do not lower a floor, edit a floor test, or add a case to one.** The numbers in the
  table are the evidence; a floor moved to make a case green is a lie in the record.
- **Do not leave the worktree behind.** A second working copy beside the tree is a
  second tree the next unit can edit by mistake.
- **Do not touch what keys.** No file named in `PHASE_PLAN.md` §3 is opened for writing.
- **Report mismatches; repair nothing. Write American. Write files as UTF-8.**

## 11. Committing and pushing

Commit per task. Push at the end and say the push succeeded, or say it was refused and
why.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

```
READ IN THIS ORDER.

A. The phase goal - CW decodes again. Step 0 is this unit's; steps 1 to 5 are
   not started. After this unit step 0 is done, or partial with 0.3 dropped.
B. Step 0's criteria, one line each, met or not: 0.1 the table at HEAD, 0.2 the
   green commit, 0.3 the red commit, 0.4 the seams.
C. The report last. Section 4 raises <n> items - unit 390's nine carried and
   this unit's own, counted - and none of them is in the way of a criterion in B.
```

```
UNIT:       391 - <complete|stopped> at task N of 4, <which dropped> - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   yes | no - <why, on the line>
NUMBER:     floor cases red at HEAD: unknown -> <n> of <m>
DRIFT:      0
```

**Section 3 leads with the answer:** the commit for 0.2 by hash and date, and the table
of every floor case at HEAD with its numbers. Then the seams. **Section 2 tells Tim in
one paragraph what is now known that was not: how many of the floors are red today, and
what date the decoder last kept them all.** No visible change in the application; say so.

---

```
ARBITER-DECISION
STEP: 0
APPROACH: run the three CW floor tests at HEAD case by case with their numbers, then walk main backward in a detached worktree to the newest commit where all three are green and name the commit after it that turned one red
MOVE: continue
WHY: PHASE_PLAN.md step 0 criterion 0.2 asks for the newest commit on main at which all three floor tests are green, and criterion 0.1 for every case at HEAD with its measured characters and elements beside its floor
STATE: not started
DECIDED: the worktree path C:/Source/HamLet-wt391 and the 900 s timeout per type are the author's, overrulable
LICENCE: PHASE_PLAN.md R47, R48, section 6; HM-DEC-155; HM-DEC-139; HM-DEC-165; FACT-004
ACCOMPLISHED: Tim knows what broke and when, in numbers, and step 1 knows which commit to go back to and which seams it will cross
ADVANCES: step 0 criterion 2
END-ARBITER-DECISION
```
