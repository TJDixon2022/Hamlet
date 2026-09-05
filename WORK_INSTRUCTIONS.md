# Work instruction 245 - Ft8Sharp.Deep exists, and the scoreboard grows a second column that reads the same

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

Carried forward from unit 244 unchanged, because it is measured and it still
holds. **Two different faults behave oppositely and have been conflated before.**

| | What it is | How it announces itself | The answer |
|---|---|---|---|
| **A - the allow-list** | a fixed set of permitted command prefixes, matched against the command **as typed** | `This command requires approval` | **there is usually a permitted spelling that works.** Find it before concluding anything |
| **B - the sandbox** | the shell may not create files or directories | `... was blocked. For security, Claude Code may only write to files in the allowed working directories for this session` | **there is no shell spelling that works.** Use `Write`/`Edit` |

`docs/shell-probe-243.md` has all eight probes verbatim. What runs: `dotnet build
<path>`, `dotnet test <path>`, `git` throughout, `ls`, `grep`, `sed`, `head`,
`tail`, `find`, `wc`, `date`. What never runs: `mkdir`, `cp`, `mv`, `git mv`, any
redirect write, and any compound line where one part is refused. `dotnet
--version` is refused and **is not evidence about the toolchain.**

**This unit creates four new directories and a dozen new files.** Every one of
them goes in with `Write`. Do not try `mkdir` first and do not report its refusal
as a finding - it is task-shaped work, not a discovery.

**`tools\arbiter\*.bat` is a closed loop.** Go through `dotnet build
tools/arbiter/outcome-append.proj` and `dotnet build
tools/arbiter/validate-output.proj`, which 243 built for this.

**One thing measured tonight, by the arbiter, on that route: an apostrophe in a
field breaks it.** `outcome-read.bat` was handed an approach containing `ladder's`
and PowerShell failed to parse the line; re-run without the apostrophe it worked
first time. **Write the `PHASE_OUTCOME.md` entry fields with no apostrophes.** It
costs nothing and it is the difference between the entry landing and the entry
being silently mangled.

**A refused shell call is a signal to reach for the other tool, not to stop.**
Nothing in this unit halts the loop.

---

## Why this unit exists

**This is unit 245. It is the third unit of this phase, and the first aimed at
step 1.**

Step 0 is closed. Six of six must-pass exits, each with a measurement under it:
the ladder runs in the loop through `Ft8LadderHarness.Run`; the as-is baseline
reproduced at 13 of 306 at a delivered -21.001 dB with 0 wrong; a wrong decode is
counted separately from a missed one everywhere; the capture fixture format is
committed with a worked example; the reader refuses four ways with a test for
each; and the shack command runs and refuses loudly. The seventh exit, one real
fixture, is *deferred by the plan itself*, gates nothing, and is `HM-OPEN-073`.

**`PHASE_OUTCOME.md` carries two verdicts on that run and they disagree** - one
`done`, one `partial`. The `partial` one names a single reason: two of step 0's
exits say the harness scores **`Ft8Sharp.Deep`**, and `Ft8Sharp.Deep` does not
exist. **That is not a defect in step 0. It is step 1, and it is this unit.** The
arbiter has ruled step 1's entry satisfied - see the rulings below - because
holding step 1 behind a criterion that only step 1 can satisfy is a circle, and
`PHASE_PLAN.md` forbids a must-pass criterion no unit can reach.

**Nothing in this tree has ever attempted step 1.** `src/` holds `Ft8Sharp`,
`Hamlet.App` and `Hamlet.RadioEngine` and nothing else. The `## UNIT 1 - STEP 1`
entry in `PHASE_OUTCOME.md` is unit 243's run judged against a step it was never
aimed at; its `APPROACH` field reads `not recorded` and its verdict `not started`
is correct. **Do not read `units spent: 1` against step 1 as an approach that was
tried.**

```
PHASE GOAL:   Hamlet reads FT8 as well as the best decoder there is, and then
              reads it further.
UNIT GOAL:    Ft8Sharp.Deep exists, is GPL-3.0 with its sources cited before a
              line of them is implemented, and returns exactly what the port
              returns - proven by running it, not by reasoning about it.
ADVANCES:     step 1, all four must-pass exits. Closing it opens steps 2, 3, 4
              and 6, every one of which is gated on step 1 and nothing else.
```

**Why the sibling and not a decibel tonight.** Steps 2, 3, 4 and 6 all depend on
step 1 and on nothing else, so this is the single gate standing between the phase
and every remaining step. It is also the cheapest step in the plan: it is meant to
change no behaviour at all.

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and **report mismatches in
section 1; do not repair them and do not repair this instruction.**

- `src/Ft8Sharp/Ft8Sharp.csproj` declares **no** `ProjectReference` and **no**
  `PackageReference`, and its own comment says that is the point.
- `tests/Ft8Sharp.Tests/Ft8SharpBoundaryTests.cs` is the mechanical guard, two
  halves: `DeclaresNoReferences` reads the project file, `NoHamletAssemblyArrives`
  walks the built assembly. Its remarks record it being watched refusing a
  reference on 2026-08-31.
- `tests/Ft8Sharp.Tests/Dsp/Ft8LadderHarness.cs`: `Available()` at about `:183`
  returns one `Decoder`, and the `Decoder` record is at about `:73`.
- `src/Ft8Sharp/Dsp/Ft8SlotDecoder.cs`: `Decode(ReadOnlySpan<float>)` at about
  `:133`, `Decode(Ft8Waterfall)` at about `:139`, `Ft8SlotResult` at about `:270`
  with five counts and a message list.
- `src/Ft8Sharp/LICENSE` is MIT; `src/Ft8Sharp/NOTICE` cites `ft8_lib` and the QEX
  paper. The repository root `LICENSE` is GPL-3.0.
- Root version `1.12.47` in `Directory.Build.props:145`. `Ft8Sharp` `0.10.7` in
  `src/Ft8Sharp/Directory.Build.props`.
- `tests/fixtures/ft8/example/ft8-example-244.wav` and its `.fixture.txt` are
  committed. `tests/fixtures/ft8/captured/` is empty and that is FACT-004's
  expected state.
- The highest issue id in `OPEN_ISSUES.md` is `HM-OPEN-073`.

**Two disagreements the reload measured, reported here so this unit is not
surprised by them and does not chase them:**

1. `PROJECT_STATUS.md` `RULES_AT` reads `HM-DEC-153 (2026-09-04)` while
   `CLAUDE.md` §1 holds `CPS-DEC-0152`. **Report if still present. Do not
   reconcile it** - `CLAUDE.md` is the owner's file.
2. `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` were modified and
   uncommitted at the root when this instruction was authored. **They are the
   loop's own bookkeeping. Commit them with your first task's commit and say you
   did.**

**Expected to fail, and not this unit's:**
`CwAdjudicationTests.ASpeedChangeInRealisticAudio` and the 51 inherited CW reds in
`docs/unit239-failing-set.txt`. **None of those is in `Ft8Sharp.Tests`.** If
`Ft8Sharp.Tests` shows red that is not in that set, that is a finding and it goes
in section 1.

---

## Rulings in force for this phase

Transcribed from `PHASE_PLAN.md`. **Not to be re-argued by this unit.**

**The seam is split.** `Ft8Sharp` stays a faithful MIT port of `ft8_lib`,
byte-identical in behaviour, and **nothing in this phase changes a line of it.**
Improvements live in `Ft8Sharp.Deep`. The port's value now is that it cannot
drift: every measurement in this phase is taken against something known-identical
to upstream, so a regression in the sibling is always visible.

**`Ft8Sharp.Deep` is GPL-3.0**, matching Hamlet's own release licence, carrying
its own `LICENSE` and a `NOTICE` citing the published sources it implements.
`Ft8Sharp` remains MIT and separately publishable. Ruled by Tim, 2026-09-04. **No
unit raises this and no step is held by it.**

**No algorithm comes from WSJT-X's source or `ft4_ft8_public/`.** Published
description only - Fossorier and Lin 1995 for ordered statistics, and the QEX
paper (Franke K9AN, Somerville G4WJS, Taylor K1JT, "The FT4 and FT8 Communication
Protocols," QEX, July/August 2020) - cited at the point of use.

**There is no WSJT-X on the development machine and no unit may assume one.** A
unit that cannot close without a real-air comparison says so; it does not
substitute `decode_ft8.exe`, which is `ft8_lib` and therefore the thing being
improved on.

**Nothing is claimed without the scoreboard.** No unit in steps 1 to 6 may report
an improvement except as a number on step 0's instrument.

**A wrong decode is counted separately from a missed one, everywhere, in every
report.** (§0.0.)

**The steps are a hypothesis, not a contract.** `PHASE_PLAN.md` grants leave to
reorder, replace, retire and add steps and to split or re-scope criteria, with the
record in `PHASE_OUTCOME.md` as the only constraint.

### Three things the arbiter decided, so this unit does not spend a night on them

1. **Step 1's entry criterion is satisfied.** `PHASE_OUTCOME.md` holds a `done`
   and a `partial` verdict on unit 244's run. The `partial` one's whole reason is
   that two step-0 exits name `Ft8Sharp.Deep`, which step 1 creates. Step 0's six
   must-pass exits are met and evidenced; the sibling's absence is step 1's
   subject, not step 0's shortfall. **Proceed. Do not re-audit step 0** - if this
   unit lands, both readings converge, because from tonight the harness scores
   `Ft8Sharp.Deep` literally rather than through the arbiter's re-scoping.
2. **A `ProjectReference` from `tests/Ft8Sharp.Tests` to `Ft8Sharp.Deep` is not a
   breach of the boundary.** The boundary is a property of the *library*, not of
   its tests, and that project's own csproj already says so about its reference to
   `Ft8Sharp`. The direction that would be a breach is `Ft8Sharp` referencing the
   sibling, and `Ft8SharpBoundaryTests.DeclaresNoReferences` already catches it.
3. **The sibling's `LICENSE` may be the verbatim GPL-3.0 text or a file that names
   GPL-3.0 by SPDX identifier and points at the verbatim text at the repository
   root.** Prefer the verbatim copy. **Say in the report which you did and why.**
   Do not spend a quarter of the night hand-transcribing licence text, and do not
   raise the licence question - it is ruled.

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, and
`NOTE` saying what is moving inside the task. The same every ten minutes while a
task is running. **Use the file-editing tools if the shell refuses.**

---

## Tasks

Seven tasks. **Task 1 is a trace and comes first**, because this unit must measure
what the port's surface actually exposes before it decides where the seam goes.
**Task 7 is the named drop candidate.**

**Start the `Ft8Sharp.Tests` baseline run early** - it is 5 m 12 s and it can run
while task 1 is being written.

### Task 1 - the trace: where the seam can be cut

**Reading only. This task cannot be refused, and nothing is built until it is
done.** Report each with file and line.

1. **The port's decode surface.** The exact signature of `Ft8SlotDecoder`'s
   constructor and both `Decode` overloads, and every member of `Ft8SlotResult`
   and `Ft8SlotMessage`. Which are `public` and which are `internal`.
2. **The reachability census, and this is the finding this unit owes step 2.**
   For each stage of the decode - the monitor and waterfall, the sync search and
   its candidates, soft-symbol extraction, the LDPC and codeword decode, the
   message decode - say whether the type and the method are **`public` on
   `Ft8Sharp`'s surface** or not. Then answer plainly: **could the loop inside
   `Ft8SlotDecoder.Decode(Ft8Waterfall)` be reproduced from outside the assembly,
   using only public members, without `InternalsVisibleTo` and without copying a
   line of it?** If the answer is no, **name exactly what is out of reach**, since
   step 2 has to insert an OSD stage into that loop and every route into it costs
   something different. **Do not build any of it tonight. Measure and say.**
3. **`Available()` and the `Decoder` record** in `Ft8LadderHarness` - the exact
   shape, and what one added entry costs. Task 5 goes through it.
4. **`Ft8SharpBoundaryTests`** - its two halves, and **which of them would catch a
   `ProjectReference` from `Ft8Sharp` to `Ft8Sharp.Deep`.** Say whether the guard
   as written already covers the new direction or needs anything.
5. **Whether `C:\Source\ft8_lib` is on this machine**, how many recordings
   `ReferenceRecordings.All` returns, and what `[RequiresReferenceCloneFact]` does
   when it is absent. **This decides how much of exit 2 can be evidenced tonight
   and the report must state the answer either way.**
6. **How a project joins `Hamlet.sln`** - the exact shape of the two existing
   `Ft8Sharp` entries, the configuration lines each carries, and the nesting
   section.

**Say what you find, not what this instruction expects.**

### Task 2 - the sibling project, its licence and its NOTICE

Create `src/Ft8Sharp.Deep/`:

- **`Ft8Sharp.Deep.csproj`** - `net8.0`, `Nullable` enable,
  `TreatWarningsAsErrors` true, **one `ProjectReference`, to
  `..\Ft8Sharp\Ft8Sharp.csproj`, and nothing else outside the framework.**
- **`Directory.Build.props`** carrying `<Version>0.1.0</Version>`, following the
  shape of `src/Ft8Sharp/Directory.Build.props`.
- **`LICENSE`** - GPL-3.0, per the ruling above.
- **`NOTICE`** - **this is exit 4 and it is not boilerplate.** It must, *before a
  line of any of them is implemented*, cite:
  - **Fossorier and Lin 1995** for ordered statistics decoding (step 2);
  - **the QEX paper** - Franke K9AN, Somerville G4WJS, Taylor K1JT, "The FT4 and
    FT8 Communication Protocols," QEX, July/August 2020 - for the protocol;

  and state: that this library is GPL-3.0; that it depends on `Ft8Sharp`, which is
  MIT and stays MIT; that **no WSJT-X source and no `ft4_ft8_public/` was read**;
  and that everything it implements comes from published description. Follow
  `src/Ft8Sharp/NOTICE`'s tone - it is a good model and it is in this tree.

Then **add both new projects to `Hamlet.sln`.** Try `dotnet sln add` once; if it
is refused, edit `Hamlet.sln` with the file tools following task 1.6's shape, and
verify with `dotnet build Hamlet.sln`. **A mis-nested solution folder is cosmetic;
do not spend the night on it.**

### Task 3 - the pass-through decoder, and the sibling's own tests

**The smallest thing that closes, and no more.**

A public type in `Ft8Sharp.Deep` - name it and say the name in the report - that
holds an `Ft8SlotDecoder` constructed with the same parameters and returns its
`Ft8SlotResult` unchanged.

**Do not design step 2's API tonight.** No OSD hook, no stage interface, no
strategy abstraction, no extension point for something that does not exist. An
abstraction invented before the algorithm it is meant to carry is an abstraction
that will be wrong, and task 1.2 is what step 2 will be authored from instead.

Create `tests/Ft8Sharp.Deep.Tests/` - xunit, referencing `Ft8Sharp.Deep`, matching
`tests/Ft8Sharp.Tests/Ft8Sharp.Tests.csproj`'s package versions. It carries at
least:

- **the sibling's built assembly references `Ft8Sharp`** - so the seam is real and
  not a coincidence;
- **the port's built assembly does not reference `Ft8Sharp.Deep`** - the direction
  that would destroy the port's publishability;
- **`LICENSE` and `NOTICE` exist beside the csproj**, and the `NOTICE` names both
  published sources by title. **A NOTICE nothing checks is a NOTICE that will rot
  the first time someone tidies it.**

### Task 4 - identity, mechanically, over the whole result

**Given the same audio, the sibling returns exactly what the port returns.** Exit
2, and *exactly* is the operative word.

Compare **the whole `Ft8SlotResult`** - all five counts and every message's text,
frequency and dt, in order - **not just `Texts`.** A comparison on text alone
passes while the counts differ, and the counts are what steps 2, 3 and 4 will be
read on.

Over three sets of audio:

1. **The ladder** - at least one whole block of 51 trials at a rung where decodes
   actually happen, and one at -21 dB.
2. **The committed example capture**, `tests/fixtures/ft8/example/ft8-example-244.wav`.
3. **Every reference recording `ReferenceRecordings.All` returns**, if the clone
   is on this machine. If it is not, those tests skip, **the report says so with
   the count found**, and exit 2 closes on 1 and 2. That is the plan's own named
   alternative and it is not a failure.

**Say plainly in the report that a delegating sibling makes this identity
trivially true.** That is the step's point - the plan says *a step that changes no
behaviour is the point* - but the assertion must be **run, not reasoned**, because
what is being proven is that the seam and the harness wiring cost nothing. **Do
not dress a tautology as a discovery**, and do not claim the sibling was
"verified against" the port as though the two had been written independently.

### Task 5 - the scoreboard's second column

One entry in `Available()`, one `ProjectReference` from `Ft8Sharp.Tests` to
`Ft8Sharp.Deep`. Ruled above; it is not a breach.

- **Run a paired ladder rung and print both columns.** At -21 dB, 306 trials,
  seed `DefaultSeed` - the run unit 243 reproduced at 13 of 306, 0 wrong. **Both
  columns must read identically, decode for decode**, and the report quotes the
  table whole. It costs about 20 s a rung by 243's measurement.
- **Run the fixture report** unit 244 built and show it with the real sibling
  where the placeholder `second-seat` was. 244 asserted it would grow a second
  column with no other change; **this is where that claim is either confirmed or
  found wrong**, and if it is found wrong that is a finding, not a repair job.
- **Three counts, never two**, in every table.

### Task 6 - both suites

`PHASE_PLAN.md`: `Ft8Sharp.Tests` **and** `Ft8Sharp.Deep.Tests`, every unit.

- `dotnet test tests/Ft8Sharp.Tests` whole - baseline before your first code
  change and totals after. Unit 244 left it at **578 passed, 0 failed, 1 skipped,
  5 m 12 s**; a different baseline is itself a finding.
- `dotnet test tests/Ft8Sharp.Deep.Tests` whole.
- **One project at a time and never concurrently.** Report passed, failed, skipped
  and wall clock for each, and any red against the expected set.
- **Do not run `Hamlet.App.Tests` or `Hamlet.RadioEngine.Tests`.** Nothing here
  touches either.

### Task 7 - the seam write-up and the record. THIS IS THE DROP CANDIDATE

**If the night runs short, this is what is shed, and the report says it was.**

- `docs/unit245-deep-seam.md` - task 1.2's census written up, so the unit that is
  authored against step 2 does not have to re-measure it.
- An `OPEN_ISSUES.md` entry at the next free id (`HM-OPEN-074` unless something
  took it) naming anything step 2 needs from the port and cannot reach without
  changing it. If task 1.2 found nothing out of reach, **say that instead and open
  no issue** - an empty issue is worse than none.

**Dropping this costs the phase a document, not a criterion.** Tasks 2 to 6 are
the four must-pass exits. **The census finding itself still goes in section 3 of
the report even if this task is dropped** - what is shed is the write-up, not the
measurement.

---

## Parked - do not touch, do not raise

- **OSD, subtraction, baseband re-sync, per-message SNR, cross-slot combining.**
  Steps 2 to 6. Not one line tonight. **A half-built OSD inside a step whose whole
  point is that behaviour does not change is the worst thing this unit could
  leave behind.**
- **`Ft8Sharp.Deep`'s licence.** Ruled GPL-3.0. Do not raise it.
- **`PHASE_PLAN.md:53`'s stale step-0 wording.** Decided by the arbiter twice
  already. Do not report it a fourth time.
- **The `RULES_AT` mismatch** between `PROJECT_STATUS.md` and `CLAUDE.md` §1.
  Report once under "verify against the tree"; go no further.
- **The shell permission fault and `allowed.txt`.** Banked, not blocking. The
  `.proj` route works. Do not probe it.
- **`HM-OPEN-071`'s missing per-message SNR.** Measured by unit 244, owed by step
  5, blocks nothing here.
- **`HM-OPEN-073`, the real capture fixture.** Tim's, deferred, gates nothing.
- **The staged `docs/phase-sensitivity/PROJECT_CARD.md`**, and
  `toolsarbitervalidate-output.bat` at the root. Harmless.
- **The CW decoder**, the 419 dropped chunks, the 51 inherited failing cases, the
  engine project's missing total, the waterfall's late first row,
  `ReusableWindow`, `ProcessDelayForTests`, the tap's owner.

---

## What not to do

- **Do not touch `src/Ft8Sharp/`.** The port is the instrument and the phase
  ruling is that nothing in this phase changes a line of it. **If `Ft8Sharp`'s
  version moves off `0.10.7`, something changed and that is a finding, not a
  bump.**
- **Do not give the sibling any reference except `Ft8Sharp`**, and do not add one
  to `Ft8Sharp` in either direction.
- **Do not read WSJT-X source or `ft4_ft8_public/`.** The second of the three
  things the arbiter may not reason past.
- **Do not copy anything out of `C:\Source\ft8_lib` into this repository.**
  `ReferenceRecordings.cs` states that ruling; it is unchanged.
- **Do not treat an absent reference clone as a defect.** It is a skip and a
  reported count.
- **Do not compare only `Texts`.** Task 4 says why.
- **Do not claim an improvement.** Nothing improves tonight and nothing may be
  reported as though it did. §12.1.
- **Do not report a count of matches without its returned-wrong count.**
- **Do not stop because the shell refused something.** Record it, switch tools,
  continue.

---

## Committing and pushing

Commit and push each task before starting the next, on `main`, which is trunk.
**Commit the three uncommitted root bookkeeping files with your first commit.**
Root version `1.12.47` to **`1.12.48`** if anything was committed; if nothing
could be committed, do not bump and say why. **`Ft8Sharp` stays `0.10.7`.**

Append this unit's entry to `PHASE_OUTCOME.md` through `dotnet build
tools/arbiter/outcome-append.proj`. **Use the tool rather than writing the entry
by hand** - it updates the header's step state in the same call. **No apostrophes
in the field text**, per the tool rule above. If it refuses, write the entry in
exactly the format the existing entries use, **update the `STEP: 1` header line
yourself**, and say in the report that you did.

Validate `output.md` through `dotnet build tools/arbiter/validate-output.proj`
before you finish, and report the rule count and the exit code.

---

## Reporting

`output.md` at the repository root, overwritten, four sections per
`CLAUDE_CODE.md` §8.

**First, above everything, the ordering block. `validate-output.bat` refuses a
report without it.** Three parts, every line specific to this unit:

- **A - THE PHASE GOAL**, and the state of all seven steps as this unit leaves
  them. **Say which steps step 1 unblocks** - 2, 3, 4 and 6 are gated on it and on
  nothing else - and say plainly that **no decibel moved tonight**, because step 1
  is defined as changing no behaviour.
- **B - THIS STEP AND ITS EXIT CRITERIA.** Step 1's four must-pass exits, listed
  one by one with met or not met against each: the sibling compiles with its own
  tests and the mechanical boundary test; identical results on the reference
  recordings and the ladder; both scoreboard columns identical; the NOTICE citing
  its sources before implementing them. **If an exit is not met, say which and
  what is needed - not a summary of effort.** If the reference clone is absent,
  say so under exit 2 with the count found.
- **C - THIS REPORT**, weighed against A and B: what it found that bears on the
  goal and the criteria - **task 1.2's reachability census is the thing here, and
  it is what step 2 will be authored from** - **how many items section 4 raises**,
  and **whether any of them stands in the way of an exit criterion in B.** An item
  that asks for no ruling is logged there as logged.

Then the six-line header: `UNIT`, `PHASE GOAL`, `UNIT GOAL`, `ADVANCED`, `NUMBER`,
`DRIFT`. **`NUMBER` for this unit is the count of step 1's must-pass exits met**,
`0 of 4` going in, whatever it is coming out - plus both suites' totals. **No dB
moves tonight and the header must not imply one did.**

**Section 3 leads with three things, in this order:**

1. **The paired ladder table, whole**, both columns, three counts each, at -21 dB
   over 306 trials - with one sentence saying plainly that identity here is
   trivially true because the sibling delegates, and that the point is that the
   seam and the wiring cost nothing.
2. **The reachability census** from task 1.2 - which stages of the port's decode
   loop are public, and exactly what step 2 would have to get past to insert OSD.
3. **Both suites' totals**, and whether any red is outside the expected set.

**Section 4 says, in one line, whether step 1 is closed**, and if it is not, the
single smallest thing that would close it.

Write `output.md`, then stop. Do not start the next unit.

---

```
ARBITER-DECISION
STEP: 1
APPROACH: create Ft8Sharp.Deep as a pass-through sibling that delegates to Ft8Sharp and join it to the ladder Available seat as a second identical column
MOVE: continue
WHY: step 1 is the gate on steps 2, 3, 4 and 6 and no unit has ever attempted it - the one step-1 entry in PHASE_OUTCOME.md is unit 243's run judged against a step it was never aimed at, with APPROACH not recorded. The loop test returns NOT FOUND and no approach on record resembles this one.
STATE: not started
DECIDED: three. First, that step 1's entry criterion is satisfied despite PHASE_OUTCOME.md carrying both a done and a partial verdict on step 0, because the partial verdict's only stated reason is that two step-0 exits name Ft8Sharp.Deep, which is the thing step 1 creates - holding step 1 behind that is a circle, and PHASE_PLAN.md forbids a must-pass criterion no unit can reach. Second, that a ProjectReference from tests/Ft8Sharp.Tests to Ft8Sharp.Deep is not a breach of the port's boundary, since the boundary is a property of the library and the breaching direction is the reverse one, which the existing mechanical guard already catches. Third, that the sibling's LICENSE may be the verbatim GPL-3.0 text or a file naming it by SPDX identifier and pointing at the verbatim text at the root, so that no part of the night is spent transcribing licence text. Also logged and not chased, per the ruling that the phase goal is the heavy hand: output.md section 4 raised four items and every one of them states it asks for no ruling.
LICENCE: PHASE_PLAN.md step 1 and its four must-pass exits - the sibling compiles with its own tests and a mechanical test that Ft8Sharp references nothing outside itself; identical results on the reference recordings and the ladder; both scoreboard columns identical; and a NOTICE citing the published sources before they are implemented. The phase ruling of 2026-09-04 that the seam is split and that Ft8Sharp.Deep is GPL-3.0 carrying its own LICENSE and NOTICE. The plan section that the steps are a hypothesis and not a contract, and its named alternative that a criterion needing something absent is deferred rather than stopped, for the reference clone.
ACCOMPLISHED: there is now somewhere for every improvement in this phase to live that cannot damage the instrument it is measured against. Ft8Sharp.Deep decodes the same audio as the port and returns the same thing to the decode, so the scoreboard reads two columns that agree today - and from tomorrow every difference between them is attributable to exactly one named change. The port stays MIT and separately publishable, the sibling is GPL-3.0 with Fossorier and Lin and the QEX paper cited before a line of either is written, and the four steps that were gated on this one are open.
ADVANCES: step 1, all four must-pass exits - the sibling compiling with its own tests and the boundary guard, exact identity with the port on the ladder and the committed capture and the reference recordings, both scoreboard columns identical, and the NOTICE citing its sources before implementing them. Closing step 1 opens steps 2, 3, 4 and 6, which are gated on it and on nothing else.
END-ARBITER-DECISION
```
