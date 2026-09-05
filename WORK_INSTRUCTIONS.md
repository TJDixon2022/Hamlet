# Work instruction 244 - the fixture format, the reader that refuses, and the command Tim runs

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

Unit 243 measured this environment and the finding stands: **two different faults
have been conflated for a week and they behave oppositely.**

| | What it is | How it announces itself | The answer |
|---|---|---|---|
| **A - the allow-list** | a fixed set of permitted command prefixes, matched against the command **as typed** | `This command requires approval` | **there is usually a permitted spelling that works.** Find it before concluding anything |
| **B - the sandbox** | the shell may not create files or directories | `... was blocked. For security, Claude Code may only write to files in the allowed working directories for this session` | **there is no shell spelling that works.** Use `Write`/`Edit` |

`docs/shell-probe-243.md` has all eight probes verbatim. What runs: `dotnet build
<path>`, `dotnet test <path>`, `git` throughout, `ls`, `grep`, `sed`, `head`,
`tail`, `find`, `wc`, `date`. What never runs: `mkdir`, `cp`, `mv`, `git mv`, any
redirect write, and any compound line where one part is refused. `dotnet --version`
is refused and **is not evidence about the toolchain** - unit 242 lost a night to
exactly that inference.

**`tools\arbiter\*.bat` is a closed loop** - every permitted spelling is destroyed
by Git Bash before an interpreter sees it, and every spelling that survives is
refused. Go through `dotnet build tools/arbiter/validate-output.proj` and
`dotnet build tools/arbiter/outcome-append.proj`, which 243 built for this and
which call the scripts unmodified.

**A refused shell call is a signal to reach for the other tool, not to stop.**
Nothing in this unit halts the loop.

---

## Why this unit exists

**This is unit 244. It is the second unit of this phase, and the second aimed at
step 0.**

`PHASE_OUTCOME.md` shows two entries and **that is bookkeeping, not history.** Both
carry the same cost, `13.1837...`, because both were written about the same run -
unit 243's. The one headed `## UNIT 1 - STEP 1` is that run judged against step 1's
exit criteria, which it was never aimed at, and its `not started` verdict is
correct. **No unit has yet attempted step 1. `Ft8Sharp.Deep` does not exist and
`src/` holds `Ft8Sharp`, `Hamlet.App` and `Hamlet.RadioEngine` and nothing else.**
Do not read `units spent: 1` against step 1 as an approach that was tried.

Step 0 stands at **three of six must-pass exits met**, each with a measurement
under it: the ladder runs in the loop through `Ft8LadderHarness.Run`; the as-is
baseline reproduced at 13 of 306 at a delivered -21.001 dB with 0 wrong, not one
decode different from unit 221; and a wrong decode is counted separately from a
missed one everywhere.

**The three that are untouched are all this unit's, and none of them needs a
radio.** They are the capture fixture format, a reader that fails loudly rather
than quietly on a stale or absent capture, and the one-step command Tim runs at the
shack. Only the *fixture itself* waits on Tim, and `PHASE_PLAN.md` marks that one
deferred so no step is held by it.

```
PHASE GOAL:   Hamlet reads FT8 as well as the best decoder there is, and then
              reads it further.
UNIT GOAL:    Step 0 closes. A real capture can be scored against WSJT-X's own
              output without WSJT-X being on this machine, and a fixture that has
              gone stale says so instead of measuring the wrong thing.
ADVANCES:     step 0 - the four capture-fixture exits, three of them must-pass:
              the format, the reader with its loud failure, and the shack command.
              Closing them satisfies step 1's entry criterion, which is the gate
              on steps 2, 3, 4 and 6.
```

**Why this and not `Ft8Sharp.Deep` tonight.** Step 1's entry is *step 0 complete*
and step 0 is `partial`. The remaining exits are the half of the scoreboard that
reads real air, and steps 3 and 5 close on nothing else. A sibling built over an
unfinished scoreboard is a sibling whose gains cannot be shown.

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim below and **report
mismatches in section 1; do not repair them and do not repair this instruction.**

- `tests/Ft8Sharp.Tests/Dsp/Ft8LadderHarness.cs` exists, and `Available()` at
  about `:182` is the seat where a second decoder joins.
- `tests/Ft8Sharp.Tests/Dsp/ReferenceRecordings.cs` reads off-air audio from
  `C:\Source\ft8_lib` at run time and **copies nothing into this repository.**
  That ruling is untouched by this unit.
- The CW precedent for a committed capture plus its truth file is
  `tests/fixtures/cw/captured/` - a `.wav` and a sibling `.txt` per capture, with
  `MANIFEST.md` under `unadjudicated/`.
- `tools/score-fixtures/score-fixtures.py` exists and is CW's.
- **No FT8 capture `.wav` is committed anywhere.** `git ls-files "*.wav"` returns
  CW fixtures only. Per `SHACK_FACTS.md` FACT-004 that is **the expected state on
  this machine and is not a finding.**
- `HM-OPEN-067` in `OPEN_ISSUES.md` carries the ladder's figures.
- Root version `1.12.46`. `Ft8Sharp` `0.10.7`.

**Expected to fail, and not this unit's:** `CwAdjudicationTests.ASpeedChangeInRealisticAudio`,
and the 51 inherited CW reds listed in `docs/unit239-failing-set.txt`. Do not chase
either. If `Ft8Sharp.Tests` shows red that is **not** in that set, that is a
finding and it goes in section 1.

---

## Rulings in force for this phase

Transcribed from `PHASE_PLAN.md`. **Not to be re-argued by this unit.**

**The seam is split.** `Ft8Sharp` stays a faithful MIT port of `ft8_lib`,
byte-identical in behaviour, and **nothing in this phase changes a line of it.**
Improvements live in `Ft8Sharp.Deep`. The port's value now is that it cannot
drift: every measurement in this phase is taken against something known-identical
to upstream, so a regression in the sibling is always visible.

**`Ft8Sharp.Deep` is GPL-3.0**, carrying its own `LICENSE` and a `NOTICE`.
`Ft8Sharp` remains MIT. Ruled by Tim, 2026-09-04. **No unit raises this and no
step is held by it.**

**WSJT-X may be run as a measuring instrument, on the shack machine only.** It
decodes the same WAV, its output is compared message by message, and **its source
is not read.** This is the *testing rather than derivation* the spec already
permits.

**There is no WSJT-X on the development machine and no unit may assume one.**
Tim's ruling, 2026-09-04. **A unit that cannot close without a real-air comparison
says so**; it does not substitute `decode_ft8.exe`, which is `ft8_lib` and
therefore the thing being improved on.

**Tim generates the capture fixtures.** Ruled 2026-09-04. He runs one command at
the shack per batch of captures and commits the result.

**A wrong decode is counted separately from a missed one, everywhere, in every
report.** A message returned that was not sent is the one failure this phase
cannot trade against rate (§0.0).

**No algorithm comes from WSJT-X's source or `ft4_ft8_public/`.** Published
description only, cited at the point of use in `porting-notes.md`.

**The steps are a hypothesis, not a contract.** `PHASE_PLAN.md` grants leave to
reorder, replace, retire and add steps and to move a target measured wrong, all
without asking, with the record in `PHASE_OUTCOME.md` as the only constraint.

### Two things the arbiter decided, so this unit does not spend a night on them

1. **Step 0's title is *there is a scoreboard, and the arbiter can read it*.** The
   one-line step list at `PHASE_PLAN.md:53` reads *and it reads WSJT-X*; the step's
   own section heading, `PHASE_STATUS.md` and `PHASE_OUTCOME.md` all read *and the
   arbiter can read it*, and the phase's own ruling forbids assuming WSJT-X here.
   **The step list is the stale line.** Units 242 and 243 both reported it.
   **Report it once more if it is still there and go no further with it** - the
   plan is the owner's file and neither the arbiter nor this unit edits it.
2. **Step 0's fourth exit says the harness "scores `Ft8Sharp.Deep` against it", and
   `Ft8Sharp.Deep` does not exist until step 1.** That is circular as written. The
   reading in force: **the harness scores every decoder `Available()` returns, which
   today is `Ft8Sharp` alone**, and the sibling joins at that same seat with one
   entry when step 1 creates it. **This exit is met by the scoring path existing and
   working through the seat**, not by the sibling existing. Recorded in this unit's
   outcome entry as an arbiter re-scoping under the plan's leave to split criteria.

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, and
`NOTE` saying what is moving inside the task. The same every ten minutes while a
task is running. **Use the file-editing tools if the shell refuses.**

---

## Tasks

Seven tasks. Task 1 is a trace and comes first because this unit must measure what
is there before it designs a format. **Task 6 is the named drop candidate.**

### Task 1 - the trace: what a fixture has to fit into

**Reading only. This task cannot be refused, and nothing is designed until it is
done.** Report each with file and line.

1. **What a decode looks like coming out of Hamlet today.** The type
   `Ft8LadderHarness` and the slot decoder return per decoded message - name every
   field, and say specifically whether **frequency, dt and SNR** are available per
   message or not. `PHASE_PLAN.md` step 0 wants a fixture row of *message,
   frequency, dt and SNR*; if Hamlet cannot produce one of those four today, **say
   which and say it plainly** - that is a finding this unit reports rather than
   works around, and step 5 is the step that owes the SNR.
2. **How `Ft8LadderHarness` compares a returned message with an expected one.**
   The exact normalisation, and where `ReferenceRecordings` does the same job for
   upstream's lists - it documents its own normalisation in the file header.
   **Whatever the fixture reader does must be that same normalisation, called and
   not re-implemented**, for the same reason the harness extends the ladder rather
   than replacing it.
3. **The CW capture fixture precedent**, `tests/fixtures/cw/captured/`: what the
   sidecar `.txt` next to each `.wav` holds, its exact shape, whether anything
   verifies the pairing, and whether any hash of the audio is recorded anywhere.
   Say whether it is a format worth following or worth diverging from, and why.
4. **Whether anything in this tree already hashes a file.** Search for SHA-256 use.
   If there is a helper, name it; if there is none, say so.
5. **`Available()` in `Ft8LadderHarness`** - its exact signature and what a
   `Decoder` entry consists of, since task 4 scores through it.
6. **Whether `tests/Ft8Sharp.Tests` can read a committed WAV at all today** - name
   the WAV reader it would use and where it lives.

**Say what you find, not what this instruction expects.**

### Task 2 - the fixture format, written down and committed

Write the format specification as a document, and commit it. It is read by Tim at
the shack and by a session six units from now, so it is prose with an example in
it, not a schema dump.

The format must carry, at minimum, what `PHASE_PLAN.md` step 0 names:

- **the capture's name**, **its UTC**, and **its SHA-256**;
- **one row per message WSJT-X returned** - message, frequency, dt, SNR;
- **a provenance field naming what produced the rows.**

That last field is not in the plan and it is this unit's addition, for a reason
that matters more than the rest of the format: **it is the difference between a
measurement and a fabrication.** The reader in task 3 refuses to score against any
fixture whose provenance is not a real WSJT-X run.

Decide the file's location and extension yourself and **say why** - the CW
precedent puts the sidecar beside the audio, and following it is defensible; so is
a dedicated `tests/fixtures/ft8/` tree. Either way the capture and its fixture are
committed together.

**Also commit one worked example**, so the reader has something to be tested
against and Tim has something to compare his first real one with. **Its rows may
not claim to be WSJT-X's.** Build it from audio this repository can legitimately
produce - the ladder synthesises a slot at a commanded SNR and knows exactly what
went into it - and mark its provenance as an example. **An example fixture carrying
invented WSJT-X rows would be the single worst artefact this unit could leave
behind**, because every later unit would score against it believing it.

### Task 3 - the reader, and the four ways it must refuse

Write the reader in `Ft8Sharp.Tests`, and **write a test for each refusal.** A
refusal that is not tested is a refusal that will not happen.

It must fail **loudly**, with an exception naming the fixture, the capture and what
was wrong, in each of these cases:

1. **The named capture is absent.**
2. **The capture is present and its SHA-256 does not match.** This is the one the
   plan calls out by name: *a stale fixture silently measures the wrong thing.*
3. **A row is malformed** - the wrong field count, an unparseable number, an empty
   message.
4. **The provenance is not a real WSJT-X run** and the caller asked to *score*
   against it. Reading an example fixture is fine; scoring a claim against one is
   not.

**Loudly means the test suite goes red and the message says which fixture and
why.** A skip, a warning, a zero-row result or a silently empty list is the
failure mode this exit exists to prevent, and if you find yourself writing one,
that is the wrong branch.

**A fixture whose capture is absent is not the same as no fixtures at all.** Zero
committed fixtures on this machine is FACT-004's expected state and must remain a
clean pass; a fixture that *names* a capture which is not there is a hard failure.
Test both, separately, and say in the report that you did.

### Task 4 - the harness scores a fixture

Extend `Ft8LadderHarness` - **extend, as 243 did, calling rather than copying.**

Given a fixture, it decodes the named capture with **every decoder `Available()`
returns** and reports per decoder:

- **matched** - a message in the fixture that this decoder also returned;
- **missed** - a message in the fixture that it did not;
- **returned wrong** - a message it returned that is not in the fixture.

**Three counts, never two**, and the third keeps its own line with the message
printed, exactly as the ladder does. Note in the code's own comment that on a real
capture the third count is **weaker evidence than on the ladder**: the ladder knows
what it transmitted, whereas a message WSJT-X missed and Hamlet found is a decode
this phase is trying to produce, not necessarily an error. **Say that in the report
too.** Do not let this count be read as the ladder's zero-wrong criterion; they are
different measurements and the report must not merge them.

The comparison uses task 1.2's normalisation. When step 1 lands, this reports two
columns with no further change - check that it would, and say so.

### Task 5 - the command Tim runs at the shack

One command, one capture in, one committed fixture out, **no editing afterwards.**

It computes the SHA-256, runs WSJT-X's decoder over the WAV, parses the rows,
writes the fixture with its provenance set to a real run, and puts it where task 2
decided.

**Be honest about the split, because it decides whether this unit can close.**

- **Reachable here and must work:** the hashing, the row parsing, the fixture
  writing, the loud refusal when the WSJT-X decoder is not found, and the loud
  refusal when it produced nothing. **Unit-test all of these against decode text
  committed as a test input.**
- **Not reachable here:** invoking WSJT-X and getting real rows back. There is no
  WSJT-X on this machine and no unit may assume one. **Write the invocation,
  document exactly how the executable is located, and say in the report that this
  half is unexercised and that Tim's first run is what exercises it.**

**On the row format you parse:** derive it from WSJT-X's *output*, which is
permitted, and **state in the document where your understanding of that format came
from.** If you cannot establish it from anything in this tree or from published
description, **say so and make the parser strict and loud rather than lenient** - a
parser that guesses is how a wrong number reaches a report. Do not read WSJT-X
source. Do not substitute `decode_ft8.exe`.

**It must refuse to write a half-fixture.** A file that exists but is incomplete is
worse than no file, because the reader in task 3 will happily read it.

### Task 6 - the record, and this is the named drop candidate

**If the night runs short, this is what is shed, and the report says it was.**

- Name in `OPEN_ISSUES.md`, each as its own item with an id: the **one real
  fixture** Tim generates (step 0, deferred); **decodes per slot within 10 per cent
  of WSJT-X's across twenty slots** (step 3, deferred); **SNR agreement with WSJT-X
  within 2 dB on real captures** (step 5, deferred). The plan requires each to be
  recorded by name and none of them gates its step.
- Update `HM-OPEN-067` with the reproduction unit 243 measured if it does not
  already carry it.

**Dropping this task costs the phase a record, not a criterion.** Tasks 2 to 5 are
the must-pass exits; this one is bookkeeping over criteria already marked deferred.
Do not drop tasks 2 to 5 in its favour.

### Task 7 - the suite, which unit 243 started and never recorded

**Run `Ft8Sharp.Tests` whole and report the totals** - passed, failed, skipped, and
the wall clock. Unit 243 started this run, ran out of night, and its report
correctly makes no claim about the suite. **`PHASE_PLAN.md` says a unit runs
`Ft8Sharp.Tests` every unit**, and this is the second unit in a row that would
otherwise not have.

It is slow - `Ft8Step6CurveTests` alone is 4 m 23 s - so **start it early and let
it run while you work.** One project at a time and never concurrently. Report red
against the expected set named above.

**Do not run `Hamlet.App.Tests` or `Hamlet.RadioEngine.Tests`.** Nothing in this
unit touches either.

---

## Parked - do not touch, do not raise

- **Building `Ft8Sharp.Deep`**, **OSD**, **subtraction**, **baseband re-sync**,
  **SNR measurement**, **cross-slot combining.** Steps 1 to 6. **Not one line of
  the sibling this unit** - a half-built `Ft8Sharp.Deep` is worse than none,
  because step 1's whole point is that the seam demonstrably changes nothing.
- **`Ft8Sharp.Deep`'s licence.** Ruled GPL-3.0. Do not raise it.
- **The shell permission fault and `allowed.txt`.** Unit 243 asked for one line to
  be added and it is the owner's file. It is **banked, not blocking** - the
  `.proj` route works. Do not repair it, do not re-argue it, do not spend a probe
  on it.
- **`PHASE_PLAN.md:53`'s stale step-0 wording.** Decided above. Report if present;
  go no further.
- **The staged `docs/phase-sensitivity/PROJECT_CARD.md`.** Dead weight, harmless,
  and `install-phase.bat`'s owner's problem.
- **`toolsarbitervalidate-output.bat` at the root.** Unit 228's shim. Harmless.
- **The CW decoder**, the 419 dropped chunks in the 21:58 capture, the 51
  inherited failing cases, the engine project's missing total, the waterfall's
  late first row, `ReusableWindow`, `ProcessDelayForTests`, the tap's owner, unit
  237's Extensible conclusion, work instruction 231's four tree items, the
  101.33 ms pulse above 6 kHz.

---

## What not to do

- **Do not write a fixture row that claims to be WSJT-X's and is not.** §12.1 and
  §0.0. This is the one thing in this unit that would do lasting damage, because a
  fabricated fixture is indistinguishable from a real one to every later unit.
- **Do not assume WSJT-X exists on this machine**, and do not substitute
  `decode_ft8.exe` for it. The phase ruling above.
- **Do not read WSJT-X source or `ft4_ft8_public/`.** The second of the three
  things the arbiter may not reason past.
- **Do not touch `src/Ft8Sharp/`.** The port is the instrument.
- **Do not copy anything out of `C:\Source\ft8_lib` into this repository.**
  `ReferenceRecordings.cs` states that ruling; it is unchanged.
- **Do not treat an absent capture folder as a defect.** `SHACK_FACTS.md` FACT-004.
- **Do not make a failing fixture check a skip or a warning.** Task 3 exists
  because quiet is the failure mode.
- **Do not stop because the shell refused something.** Record it, switch tools,
  continue.
- **Do not rebuild the ladder.** Extend it, per unit 243's finding and the same
  reasoning.
- **Do not report a count of matches without its returned-wrong count.**

---

## Committing and pushing

Commit and push each task before starting the next, on `main`, which is trunk.
Root version `1.12.46` to **`1.12.47`** if anything was committed; if nothing could
be committed, do not bump the version and say why.

Append this unit's entry to `PHASE_OUTCOME.md` through
`dotnet build tools/arbiter/outcome-append.proj`, which reaches
`outcome-append.bat` unmodified. **Use the tool rather than writing the entry by
hand** - it updates the header's step state in the same call, and a hand-written
entry gets the entry and forgets the state. If it refuses, write the entry in
exactly the format the existing entries use **and update the `STEP: 0` header line
yourself**, and say in the report that you did.

Validate `output.md` through `dotnet build tools/arbiter/validate-output.proj`
before you finish, and report the rule count and the exit code.

---

## Reporting

`output.md` at the repository root, overwritten, four sections per
`CLAUDE_CODE.md` §8.

**First, above everything, the ordering block. `validate-output.bat` refuses a
report without it.** Three parts, every line specific to this unit:

- **A - THE PHASE GOAL**, and **the state of all seven steps** as this unit leaves
  them. Say which steps are gated on step 0 and which on step 1.
- **B - THIS STEP AND ITS EXIT CRITERIA.** Step 0's seven exits, six must-pass,
  **listed one by one with met or not met against each**, distinguishing the three
  unit 243 met from the ones this unit was aimed at. If step 0 does not close, the
  block says which exit is open and what is needed - not a summary of effort.
- **C - THIS REPORT**, weighed against A and B: what it found that bears on the
  goal and the criteria, **how many items section 4 raises**, and **whether any of
  them stands in the way of an exit criterion in B.** An item that asks for no
  ruling is logged there as logged.

Then the six-line header: `UNIT`, `PHASE GOAL`, `UNIT GOAL`, `ADVANCED`, `NUMBER`,
`DRIFT`. **`NUMBER` for this unit is the count of step 0's must-pass exits met**,
`3 of 6` going in, whatever it is coming out - plus the suite totals from task 7.
No dB moves tonight and the header must not imply one did.

**Section 3 leads with three things, in this order:**

1. **The four refusals, demonstrated.** For each of task 3's four cases, the
   failure message a session would actually see, verbatim. This is the exit that
   says a stale fixture fails loudly, and prose claiming it does is not evidence -
   the message is.
2. **The fixture format in one glance** - the worked example, whole, with its
   provenance field visible, and one sentence on where it lives and why there.
3. **The suite totals**, and whether any red is outside the expected set.

**Section 4 says, in one line, whether step 0 is closed**, and if it is not, the
single smallest thing that would close it.

Write `output.md`, then stop. Do not start the next unit.

---

```
ARBITER-DECISION
STEP: 0
APPROACH: capture fixture format, its reader with loud hash and absence failures, harness scoring through the Available seat, and a one-step shack generator command
MOVE: continue
WHY: step 0 is partial with three must-pass exits untouched and all three reachable by unit effort alone - only the fixture itself waits on Tim and the plan already marks that deferred. The loop test returns NOT FOUND and the one approach on record, unit 243's shell probe and ladder handle, bears no resemblance to it.
STATE: partial
DECIDED: two, both under the plan's leave to split or re-scope criteria with the record as the constraint. First, step 0's fourth exit says the harness scores Ft8Sharp.Deep against a fixture, and Ft8Sharp.Deep does not exist until step 1, so the exit is read as the scoring path working through Available(), which today returns Ft8Sharp alone and which the sibling joins with one entry. Second, step 0's title is "there is a scoreboard, and the arbiter can read it" - the step section, PHASE_STATUS.md and PHASE_OUTCOME.md agree and the step list line at PHASE_PLAN.md:53 naming WSJT-X is stale against the phase's own ruling that no WSJT-X exists here. Units 242 and 243 both raised it; neither the arbiter nor the unit edits the plan, so it is decided here and parked. Also added to the format, not in the plan: a provenance field the reader checks, so an example fixture can never be scored against as though it were a real WSJT-X run.
LICENCE: PHASE_PLAN.md step 0, whose remaining must-pass exits are a capture fixture format naming the capture, its UTC and its SHA-256 with a row per WSJT-X message; a fixture whose capture is absent or whose hash does not match failing loudly rather than passing quietly; and a one-step command Tim runs at the shack. The plan's section "the steps are a hypothesis, not a contract" for the two decisions above. The ruling of 2026-09-04 that Tim generates the fixtures and that no unit may assume WSJT-X on the development machine.
ACCOMPLISHED: the scoreboard reads real air as well as the ladder. Tim runs one command at the shack over a capture and commits a file; from then on any unit scores Hamlet against what WSJT-X actually returned for that exact audio, message by message, without WSJT-X ever being on the development machine - and if the audio is ever swapped or lost the hash says so loudly instead of the number quietly changing meaning.
ADVANCES: step 0, exits four, five and seven - the capture fixture format, the loud failure on an absent capture or a mismatched hash, and the one-step shack command. Closing them completes step 0 and satisfies step 1's entry criterion, which is the gate on steps 2, 3, 4 and 6.
END-ARBITER-DECISION
```
