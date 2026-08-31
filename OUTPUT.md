READ IN THIS ORDER — A, then B, then C.

A. PHASE — Hamlet hears FT8 off the radio and displays the decoded text on screen.
Seven steps. Step 1 (the library exists and its tables are proven) is at four of its
six must-pass criteria demonstrated, up from three. Steps 2 (messages round-trip
through 77 bits), 3 (a valid FT8 signal can be produced), 4 (signals are found in
noise), 5 (a found signal becomes a message), 6 (sensitivity meets the published
threshold) and 7 (Hamlet displays decoded FT8) are all not started, and none of them
has been touched by this unit. Steps 2 through 7 remain unreachable until step 1
closes: the plan's own named deviation is that every step depends on the one before
it, so there is no branch of this phase that can be worked in parallel while step 1
is open.
B. STEP 1 — the library exists and its tables are proven. Six must-pass exit
criteria: (1) the project builds under .NET 8 with nullable, warnings as errors and
no third-party runtime dependencies; (2) LICENSE, NOTICE and porting-notes.md present
and correct; (3) the boundary test passing AND shown to fail; (4) tables converted by
a checked-in tool that reads ft8/constants.c, reproducible against a future upstream;
(5) tables verified by LDPC encode against reference parity; (6) whole Hamlet suite —
no new red, inherited failing set unchanged, named and counted. 1, 2 and 3 were
demonstrated by unit 201 and this unit inherits them. THIS UNIT WAS AIMED AT 4 AND AT
NOTHING ELSE. Criterion 4 is met: the six tables were converted by
tests/Ft8Sharp.Tests/TableGen/, the result is checked in at
src/Ft8Sharp/Tables/Ft8Tables.g.cs, and a watched test parses the pinned clone again,
emits into memory and asserts byte-identity — passing, and watched refusing a single
altered element by name. Criterion 5 was not attempted: no encoder was written and no
parity was checked. The four tables it needs — the generator, Nm, Mn and Num_rows —
are now on disk with their geometry proven, so the parity unit is authorable against
real data. Criterion 6 is not a unit's to run: the whole suite takes over 25 minutes,
it is not required until step 1 closes, and running it is what killed unit 200.
C. THIS REPORT — the checked-in tables ARE byte-identical to what the converter
produces from C:\Source\ft8_lib\ft8\constants.c at the pin today: 20043 characters on
each side, line endings normalised and nothing else. Section 4 raises 3 items. None of
them stands in the way of criterion 4, which is met. One of them — an orphaned
testhost holding Hamlet.App.Tests' output — will stand in the way of criterion 6 when
somebody comes to close step 1.

```
UNIT:       202 — complete at task 6 of 6 — 2026-08-31 18:45
PHASE GOAL: Hamlet pulls FT8 out of the air and puts the decoded text on the screen.
UNIT GOAL:  Move the six FT8 protocol tables out of the pinned ft8_lib clone into
            Ft8Sharp by machine, and prove the bytes now in the repository are the
            bytes that machine produces from that clone.
ADVANCED:   yes — criterion 4 is demonstrated, not merely on disk: the regeneration
            test ran, said byte-identical, and was watched refusing a corrupted copy.
NUMBER:     step 1 must-pass criteria demonstrated: 3 -> 4 of 6
DRIFT:      0 consecutive units without advance  (was 0 — unit 201 advanced)
```

## 1. What Claude did

**Complete, at task 6 of 6.** All six tasks were done, including task 6, the named
drop candidate — nothing was dropped. Windows 11, project claimed and confirmed as
Hamlet, branch `main`, six commits, every one pushed and none refused.

The gate was checked against the tree before the instruction was read past its first
screen: `SHACK_FACTS.md` present, `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs`
present, `CoreHMI.sln` absent, `MURC.sln` absent. All four hold.

### Task 1 — the trace

`dotnet build` on both `Ft8Sharp` projects, then the `Ft8Sharp` tests alone: **4
total, 4 passed, 0 failed, 0 skipped.** Unit 201's report says `Total: 4` in its
transcript and "5" in its prose; the count on this machine today is 4, and neither
number is worth chasing further.

`ReferenceCloneProbeTests` printed:

```
clone path              : C:\Source\ft8_lib
reachability            : Reachable (directory enumerated)
ft8\constants.c         : present, 15155 bytes, 392 lines
ft8\constants.h         : present, 3728 bytes, 90 lines
HEAD                    : 9fec6ca39886edbf96f4f5e71edc76da5074e871
pin                     : 9fec6ca39886edbf96f4f5e71edc76da5074e871
HEAD == pin             : True
array definitions found : 9
```

The inventory the instruction gives is the inventory on disk: `src/Ft8Sharp/` held
`Ft8Sharp.csproj`, `Ft8SharpAssembly.cs`, `LICENSE`, `NOTICE` and a 203-line
`porting-notes.md` with no `Tables/` folder; `tests/Ft8Sharp.Tests/` held its csproj
and two test files and nothing else; `ToolchainProbe.TEMP.cs` is gone and did not come
back; root `Directory.Build.props` was at 1.12.8 and there is no root
`Directory.Build.targets`; `.gitattributes` forces CRLF for `*.bat` and says nothing
about `*.cs`; both projects are in `Hamlet.sln`; `Ft8Sharp.csproj` declares no
package and no project reference and sets `net8.0`, `Nullable`, `TreatWarningsAsErrors`
itself.

### Task 2 — the converter

`tests/Ft8Sharp.Tests/TableGen/` — four files: `CSourceParser.cs` reads one C array
by identifier, `ExpressionEvaluator.cs` evaluates the arithmetic a dimension macro is
written as, `Ft8TableConverter.cs` holds the six-table manifest and the emitter, and
`RepositoryTree.cs` finds the tree root and reads emitted arrays back. It handles
nested brace initialisers, block and line comments, hex and decimal literals with
integer suffixes, and trailing commas. **Element counts come from the parse; the
manifest's counts are only ever a cross-check.**

**Nine tests on synthetic C watch it refuse**, and they need no clone: a missing
identifier, a value that will not fit `uint8_t`, ragged rows, a literal dimension the
initialiser contradicts, a macro dimension the header contradicts, and — separately —
a macro the header does not resolve, which is *reported* rather than failed, because
a header that will not parse is a gap in corroboration where a header that parses and
disagrees is a contradiction. Every refusal names the identifier, and one of the tests
asserts the refusal does **not** contain the offending value.

`Ft8Sharp` gained no parser, no package reference and no project reference from any
of this.

### Task 3 — emit, check in, prove reproducible

`dotnet test ... -e FT8_TABLEGEN_WRITE=1` wrote
`src/Ft8Sharp/Tables/Ft8Tables.g.cs`: 6 tables, 2197 elements, 20043 characters, 443
lines. The write is gated behind that environment variable, because a generator that
fires on every `dotnet test` would rewrite the tree under anybody running the suite
and the comparison it is meant to be checked by could never fail.

The file is one `public static class Ft8Tables`, each table a `ReadOnlySpan<byte>`
over a flattened literal behind named stride constants. Its header names the pin, the
source file, the tool, the regeneration command and the test that proves it, says
**DO NOT EDIT BY HAND**, and carries no clock and no machine name.

Then the test that makes criterion 4 mean something — see section 3 for its output.

### Task 4 — the geometry

Seven assertions in `Ft8TableGeometryTests`, run against the checked-in file rather
than against the clone, so they need no reference material and never skip. All pass.
Results in section 3.

### Task 5 — the record

`porting-notes.md` gained the converter (where it lives, why it is a test, how to
re-run it, what is checked in), the byte-identity result, the watched skip and the
watched refusal, the three FT4-only tables deliberately not converted, the measured
index base, and the geometry results — **no values anywhere in it**.

`CLAUDE.md` §1 gained **HM-DEC-152** and **HM-DEC-151**, in that order, immediately
below the `|---|` separator, dated 2026-08-31, transcribed in full from the work
instruction rather than paraphrased. `PROJECT_STATUS.md`'s `RULES_AT` advanced to
`HM-DEC-152 (2026-08-31)` and nothing else in that field changed.

**Root `Directory.Build.props` moved 1.12.8 → 1.12.9**, which is what it was actually
at, under HM-DEC-150.

`DecisionLogOrderTests` guards §1's ordering and **could not be run** — building
`Hamlet.App.Tests` fails `MSB3027`, the orphaned `testhost (34836)` holding its
output, which is the known fault this unit was told not to chase. The two rows were
checked by hand against that test's own rule instead: both dated 2026-08-31, 152 above
151, both above HM-DEC-150 of 2026-08-21, neither id reused, and the row shape matches
its regex. See section 4.

### Task 6 — the library's own version

Not dropped. `src/Ft8Sharp/Directory.Build.props`, deliberately not importing the root
one, puts `Ft8Sharp` at **0.1.0**. Before and after in section 3. Dropping the
inheritance was not sufficient on its own and that is worth knowing: the SDK appends
the repository's source revision to `AssemblyInformationalVersion` without any help
from the root file, so `IncludeSourceRevisionInInformationalVersion` is off in that
file too. **A decision this session made for itself**, inside task 6's stated purpose
— the ruling's own words are that an extracted `Ft8Sharp` must not carry "Hamlet's git
commit", and without that property it still would.

### Decisions this session made for itself

Two, both inside the instruction's scope and both reproduced in full:

1. **The write half of the converter is a separate, environment-gated test**
   (`FT8_TABLEGEN_WRITE=1`) rather than something that runs with the suite. The
   instruction fixed where the converter lives and what it must prove; it did not say
   how the file gets written the first time. A generator that runs unconditionally
   would rewrite checked-in source under anybody who ran `dotnet test` and would make
   the byte-identity assertion incapable of failing.
2. **`IncludeSourceRevisionInInformationalVersion=false` in the library's props**, as
   above.

### The known items, confirmed rather than rediscovered

All ten listed in the instruction were checked and none was repaired: `PHASE_OUTCOME.md`
still has no entries and every step still reads `not started`; `PHASE_STATUS.md` still
reads `WORK_INSTRUCTION: 001` and `STEP: 1 | partial`; `PROJECT_STATUS.md` was stale
from unit 201 and has been overwritten under the status cadence; the `RULES_AT` reload
defect was not touched and nothing under `tools\` was edited; unit 201's `output.md`
has been overwritten by this file; **`ft8sharp-spec.md` is absent from both the root
and `docs/`**, confirmed, and was not written and not treated as a source;
`CLAUDE.md` HM-DEC-004's GPL premise still cites "Phase 3 links ft8_lib (GPL)" where
this phase has it as MIT, and the root `LICENSE` was not touched; the loop's own root
files are still untracked and were not committed; `outcome-read.bat` was not touched;
`ToolchainProbe.TEMP.cs` is gone and stayed gone.

**No mismatch was found between the instruction and the tree.** Every measured fact it
asserted — file lists, project settings, version, `.gitattributes`, the absent
`Tables/` folder, the nine array definitions and their shapes — matched.

## 2. What the owner should expect

**What is now true.** `Ft8Sharp` holds the six FT8 protocol tables, converted by a
tool checked in beside them, and a test that fails the moment those two stop agreeing.
The library builds clean and reports itself as version 0.1.0 with no trace of Hamlet
in its assembly attributes. `CLAUDE.md` carries your two rulings of 2026-08-31 as
HM-DEC-151 and HM-DEC-152.

**What will look wrong and is not:**

- **One `Ft8Sharp` test is always skipped.** `RewriteTheCheckedInTablesFile` is the
  generator. It skips unless `FT8_TABLEGEN_WRITE=1`, and that is the design — it is
  the only thing in the tree that writes `Ft8Tables.g.cs`.
- **Five are skipped on a machine without the clone.** Pointing `FT8_LIB_PATH` at a
  path that does not exist gives 23 total, 18 passed, 5 skipped, 0 failed. A fresh
  clone with no reference material stays green, which is the ruling.
- **`Ft8Tables.g.cs` is 443 lines of hex and is not meant to be read.** It is machine
  output, it says so in its header, and reviewing its values by eye is not how it is
  checked — the regeneration test is.
- **Two version numbers in the tree now**, 1.12.9 for Hamlet and 0.1.0 for the
  library. That is HM-DEC-152 working, not drift.
- **`dotnet build Hamlet.sln` still fails `MSB3027`.** The orphaned `testhost` from
  the session killed earlier today still holds `Hamlet.App.Tests`' output. Nothing in
  this unit touched it and nothing in this unit needs it.

## 3. What you should see

**Are the tables in the repository byte-identical to the tables the converter produces
from `C:\Source\ft8_lib\ft8\constants.c` at the pin? Yes.**

```
Passed Ft8Sharp.Tests.Ft8TableGenerationTests.CheckedInTablesAreWhatTheConverterProduces

C identifier             dimensions     elements
kFT8_Costas_pattern      [7]            7
kFT8_Gray_map            [8]            8
kFTX_LDPC_generator      [83][12]       996
kFTX_LDPC_Nm             [83][7]        581
kFTX_LDPC_Mn             [174][3]       522
kFTX_LDPC_Num_rows       [83]           83

derived geometry        : LdpcM=83 LdpcN=174 LdpcKBytes=12 NmRowWidth=7 MnRowWidth=3
header cross-check      : every declared dimension resolved against ft8/constants.h

checked-in file         : C:\Source\HamLet\src\Ft8Sharp\Tables\Ft8Tables.g.cs
characters produced     : 20043
characters checked in   : 20043
byte-identical          : True
```

**The six tables and their parsed element counts** are the table above — 2197 elements
in total, every count taken from what the parser found and then compared against the
manifest, never the other way round. The three FT4-only tables in the same file were
not converted. **No value of any table appears in this report, in `porting-notes.md`,
in any commit message or in any test's output.**

**The dimension macros were corroborated, not inferred.** `FTX_LDPC_M`,
`FTX_LDPC_K_BYTES` and `FTX_LDPC_N` were resolved from `ft8/constants.h` — including
the one that is written as arithmetic rather than as a number — and every one of them
agreed with the shape the initialiser itself has. A disagreement would have been a
failure, not a preference.

**Task 4's geometry, each named, each passing:**

```
Passed ElementCountsAndDerivedGeometryAgree
  996 = 83 x 12, 581 = 83 x 7, 522 = 174 x 3
Passed GrayMapIsAPermutationOfTheEightTones
  Ft8GrayMap: every one of the 8 tones present exactly once.
Passed CostasPatternIsSevenTonesInRange
  Ft8CostasPattern: 7 entries, every one inside the 8-tone alphabet.
Passed NumRowsIsAWidthPerCheckAndSumsToMnsElementCount
  LdpcNumRows: every entry in 1..7, and they sum to 522, which is LdpcMn's
  element count (522).
Passed NmPadsWithZeroExactlyWhereNumRowsSaysItDoes
  LdpcNm: every row is real up to its LdpcNumRows length and zero after it, with
  no zero inside the real part and no non-zero in the padding.
Passed IndexBasesAreUpstreamsAndAreMeasuredRatherThanAssumed
  LdpcMn holds check indices    : 1-based, covering all 83 checks with no gaps.
  LdpcNm holds variable indices : 1-based, covering all 174 variables with no gaps.
Passed NmAndMnAreTransposesOfEachOther
  LdpcNm and LdpcMn agree on all 522 edges in both directions.
```

**The index base was measured, not assumed, and is 1 for both tables.** The
measurement is that the entries cover a contiguous range of exactly the right
cardinality — 83 checks, 174 variables — with no gaps, and that the smallest is one
rather than zero; in `Nm`, zero is padding and never an index, which is what makes
1-based the only reading that works. **They are not renumbered.** Every consumer
subtracts the same one, written down in one place.

**The `Nm`/`Mn` transpose is the strongest of these**, and it is exact in both
directions over all 522 edges. A single wrong bit in either table fails it.

**The comparison was watched refusing.** One element of `kFTX_LDPC_Nm` was altered in
memory, by machine, in a copy of the generated text — the checked-in file was never
touched and nothing was hand-edited:

```
Passed TheComparisonRefusesAFileWithOneAlteredElement
  identical               : False
  reported                : kFTX_LDPC_Nm: differs at 1 of 581 positions.
```

It named the table, counted the positions, said nothing about the other five, and
printed no value.

**The watched skip, with `FT8_LIB_PATH` pointing nowhere:**

```
dotnet test tests/Ft8Sharp.Tests -e FT8_LIB_PATH=C:\Source\ft8_lib_does_not_exist

  Ft8Sharp.Tests.ReferenceCloneProbeTests.TestProcessCanReachThePinnedReferenceClone [SKIP]
  Ft8Sharp.Tests.ReferenceCloneProbeTests.ConstantsInventoryIsLegibleAsShapesOnly [SKIP]
  Ft8Sharp.Tests.Ft8TableGenerationTests.CheckedInTablesAreWhatTheConverterProduces [SKIP]
  Ft8Sharp.Tests.Ft8TableGenerationTests.TheComparisonRefusesAFileWithOneAlteredElement [SKIP]
  Ft8Sharp.Tests.Ft8TableGenerationTests.RewriteTheCheckedInTablesFile [SKIP]

Passed! - Failed: 0, Passed: 18, Skipped: 5, Total: 23
```

The seven geometry tests keep running there, because they read the checked-in file
rather than the clone. That is deliberate: what ships is asserted sound on a machine
that has never seen `ft8_lib`.

**The `Ft8Sharp` suite as it now stands: 23 total, 22 passed, 1 skipped, 0 failed** —
4 at the start of this unit, 23 at the end.

**Task 6, the before and the after**, read out of the generated
`src/Ft8Sharp/obj/Debug/net8.0/Ft8Sharp.AssemblyInfo.cs`:

```
BEFORE
[assembly: AssemblyMetadataAttribute("BuildStampUtc", "2026-08-31 22:41")]
[assembly: AssemblyFileVersionAttribute("1.12.9.0")]
[assembly: AssemblyInformationalVersionAttribute("1.12.9+53a586e0579f84cb299189aa91d8b772877db33e")]
[assembly: AssemblyVersionAttribute("1.12.9.0")]

AFTER
[assembly: AssemblyFileVersionAttribute("0.1.0.0")]
[assembly: AssemblyInformationalVersionAttribute("0.1.0")]
[assembly: AssemblyVersionAttribute("0.1.0.0")]
```

No 1.12.9, no Hamlet commit, no `BuildStampUtc`. That commit hash is Hamlet's, and an
extracted `Ft8Sharp` would have carried it. The library builds clean with the
non-inheriting props file and the tests still pass, so nothing was reverted.

**Commits, all on `main`, all pushed, none refused:**

```
663cfca chore(status): unit 202 task 1 — the clone still answers, and the pin still matches
ceec87f feat(ft8sharp): a checked-in converter that reads ft8/constants.c
8602ec9 feat(ft8sharp): the six tables, written by the tool from the pinned clone
11acc15 test(ft8sharp): the four LDPC tables describe one graph, counted from both sides
53a586e docs(docs): record the conversion, and Tim's two rulings as HM-DEC-151 and 152
09717e4 build(ft8sharp): the library stops publishing itself as a version of Hamlet
```

Nothing from `C:\Source\ft8_lib` was committed, no `bin/` or `obj/` output was
committed, and none of the loop's own machinery was committed.

## 4. What's blocking us

**Three items. None of them blocks criterion 4, which is met. One of them will block
criterion 6.** Most-blocking first.

**1. The orphaned `testhost` will stop step 1 from closing, and it is not something a
session can clear.** Building `tests/Hamlet.App.Tests` fails `MSB3027` — `testhost
(34836)`, left by the session killed earlier today, holds `Hamlet.App.dll` and
`Hamlet.RadioEngine.dll` in that project's output folder. This unit did not need it
and did not chase it, as instructed, but it had one real cost here:
`DecisionLogOrderTests`, which is the check on the two `CLAUDE.md` rows this unit
added, **could not be run**, so those rows are verified by hand against the test's own
rule rather than by the test. More importantly, **criterion 6 of step 1 is "whole
Hamlet suite — no new red, inherited failing set unchanged, named and counted", and
that cannot be measured at all while the build of a test project fails.** Killing the
process is outside what a session can do here. **This is a note and a heads-up, not a
ruling request** — the action is a reboot or an end-task before the unit that closes
step 1 runs.

**2. HM-DEC-150's "the minor version is the phase number" has nothing to equal under
the phase layer.** The ruling says the minor version *is* the phase and equals
`PROJECT_STATUS.md`'s `PHASE`, with no second copy of the number. Under the phase
layer `PHASE` is a sentence — "Hamlet hears FT8 off the radio and displays the decoded
text on screen" — so there is no number for the minor to equal, and the version moved
1.12.8 → 1.12.9 on the patch alone. **Noted, not resolved, and no id minted for it**,
as instructed. It is a real inconsistency between two live conventions and it wants a
ruling eventually; it blocks nothing now.

**3. The loop's own root files are untracked, and the phase's memory is empty.**
`ARBITER.md`, `PHASE_CONTROL.md`, `PHASE_PLAN.md`, `PHASE_STATUS.md`,
`PHASE_OUTCOME.md`, `PHASE_UPLIFT.md`, `RUN_LEDGER.md`, `MANIFEST.txt`,
`VERIFY_PASS.md`, `.gitattributes`, `.run-unit/`, `docs/phase-uplift/` and
`tools/arbiter/` are all untracked; another author put them there and this unit did not
commit them. Related and worth stating in the same breath: **`PHASE_OUTCOME.md` still
has no entries and every step still reads `not started`**, although units 200, 201 and
now 202 have run and two of them completed. The phase's memory is empty, so the
arbiter's loop test has nothing to read and has to be judged from `RUN_LEDGER.md` and
the last report instead. **Recommendation, for their author rather than for a session:
commit them, and make the outcome append happen** — three units of work are now
invisible to the one file designed to remember them. `PHASE_OUTCOME.md` was not
hand-edited.

**Nothing else is blocking.** The known defects listed in the instruction — the
`RULES_AT` reload mangling the id, HM-DEC-004's GPL premise, the absent
`ft8sharp-spec.md`, `outcome-read.bat`'s apostrophe — were all confirmed still present
and none was touched. No ruling is needed to author criterion 5's unit: the four
tables it needs are on disk, their geometry is proven, and the pin is verified in the
suite.
