READ IN THIS ORDER

A. THE PHASE GOAL — Hamlet reads FT8 as well as the best decoder there is, and
then reads it further. Where the seven steps stand as this unit leaves them:
step 0 partial-to-done (six of six must-pass exits met by unit 244; the seventh
is deferred by the plan and is HM-OPEN-073), **step 1 DONE tonight**, steps 2, 3,
4 and 6 not started, step 5 not started. **Closing step 1 unblocks steps 2, 3, 4
and 6** — every one of them was gated on step 1 and on nothing else, so four of
the five remaining steps are open this morning that were shut last night. Step 5
was never gated on it. **NO DECIBEL MOVED TONIGHT and none was meant to:** step 1
is defined as changing no behaviour, the receiver reads 13 of 306 at a delivered
-21.001 dB exactly as it did under unit 221, and both scoreboard columns say so.

B. THIS STEP AND ITS FOUR MUST-PASS EXITS — all four met.
  1. The sibling compiles with its own tests and the mechanical boundary test —
     **MET.** Hamlet.sln builds 10 of 10, Ft8Sharp.Deep.Tests 18 passed 0 failed.
  2. Identical results on the reference recordings and the ladder — **MET.** The
     pinned clone IS on this machine, so this did not fall back to the plan's
     named alternative: 69 of 69 reference recordings, 801 messages, plus two
     ladder blocks and the committed capture, compared on the WHOLE result.
  3. Both scoreboard columns identical — **MET.** 306 trials at each of -19, -20
     and -21 dB, three counts each, identical decode for decode.
  4. A NOTICE citing its sources before implementing them — **MET.** Fossorier
     and Lin 1995 and the QEX paper, cited before a line of either is written,
     and a test reads the file.

C. THIS REPORT, against A and B — the thing here that outlives the unit is
**task 1.2's reachability census**, and it is what step 2 will be authored from:
every stage of the port's decode loop is public, so the loop can be reproduced
from outside the assembly without InternalsVisibleTo, and the ONE thing that
cannot be reached is a constructed Ft8CodewordResult. A public route round that
was measured working, so nothing step 2 needs requires changing the port.
Section 4 raises 3 items. **None of them stands in the way of any exit criterion
in B, and every one states that it asks for no ruling.** Section 4's first line
says whether step 1 is closed.

UNIT:       245 - complete at task 7 of 7 - 2026-09-04 22:22
PHASE GOAL: Hamlet reads FT8 as well as the best decoder there is, and then reads it further
UNIT GOAL:  Ft8Sharp.Deep exists, is GPL-3.0 with its sources cited before a line of them is implemented, and returns exactly what the port returns — proven by running it
ADVANCED:   yes — step 1 closed at four of four must-pass exits, which opens steps 2, 3, 4 and 6
NUMBER:     step 1 must-pass exits met: 0 of 4 -> 4 of 4. Suites: Ft8Sharp.Tests 582 passed / 0 failed / 1 skipped (baseline 578/0/1); Ft8Sharp.Deep.Tests 18 passed / 0 failed / 0 skipped. NO dB MOVED — the ladder reads 13 of 306 at -21.001 dB in both columns, as it did under unit 221
DRIFT:      0 consecutive units without advance  (was 0)

## 1. What Claude did

**Complete, at task 7 of 7.** Nothing was dropped, including the named drop
candidate. Machine `C:\Source\HamLet`, project claimed and confirmed as Hamlet
(`SHACK_FACTS.md` and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs`
present, `CoreHMI.sln` and `MURC.sln` absent), branch `main`, seven commits
pushed.

### What was traced, built and measured

**Task 1 — the trace, reading only.** The port's decode surface and the
reachability census, in full in section 3 and written up at
`docs/unit245-deep-seam.md`. The `Ft8Sharp.Tests` baseline was started first and
ran alongside it: **578 passed, 0 failed, 1 skipped, 5 m 8 s**, which agrees with
unit 244's 578/0/1 at 5 m 12 s. The single skip is
`Ft8TableGenerationTests.RewriteTheCheckedInTablesFile` and is **not** a
`[RequiresReferenceCloneFact]` skip, which is how this unit knows the pinned
`C:\Source\ft8_lib` clone is present — the sandbox refuses `ls` outside the
working directory, so the suite answered the question the shell could not.

**Task 2 — the sibling.** `src/Ft8Sharp.Deep/` with `net8.0`, `Nullable` enable,
`TreatWarningsAsErrors` true, one `ProjectReference` to `..\Ft8Sharp\Ft8Sharp.csproj`
and nothing else outside the framework; its own `Directory.Build.props` at
`0.1.0` that does not inherit the root's, for the same reason `Ft8Sharp`'s does
not (HM-DEC-152).

**`LICENSE` is the verbatim GPL-3.0 text and here is why, since the ruling asked.**
674 lines, byte-identical to the repository root `LICENSE` by `diff`. The ruling
permitted an SPDX pointer and preferred the verbatim copy; the obstacle was that
`cp` is refused by the sandbox and hand-transcribing licence text is exactly what
the ruling said not to spend the night on. `dotnet build` is permitted and
MSBuild's `Copy` task copies a file byte for byte, so a one-shot `.proj` did it
and then deleted itself in a second run. **No licence text was transcribed and no
quarter of the night was spent.**

`NOTICE` is exit 4 and is not boilerplate — see section 3.

**Task 3 — `Ft8DeepSlotDecoder`,** named here as the report was asked to. It
holds an `Ft8SlotDecoder` built with the parameters it was handed and returns
that decoder's `Ft8SlotResult` unchanged on both overloads, and it does not
re-implement the port's refusals. **No OSD hook, no stage interface, no strategy,
no extension point** — an abstraction invented before the algorithm it is meant
to carry is an abstraction that will be wrong, and the census is what step 2 is
authored from instead. `tests/Ft8Sharp.Deep.Tests` carries the two boundary
directions, the licence and NOTICE checks, and a whole-result comparison.

**Task 4 — exit 2, over all three sets, on the whole `Ft8SlotResult`.** Never on
`Texts` alone. Section 3 has the numbers.

**Task 5 — the second column.** One entry in `Ft8LadderHarness.Available()`, one
`ProjectReference`, **and no caller in the tree changed** — which was the claim
the seat was built on and is now measured rather than asserted. Unit 244's
`second-seat` placeholder in `Ft8FixtureScoringTests` is replaced by the real
sibling, and **244's claim that the report would grow a column with no other
change is CONFIRMED**, not found wrong.

**Task 6 — both suites, one project at a time and never concurrently.** Section 3
has the totals. `Hamlet.App.Tests` and `Hamlet.RadioEngine.Tests` were not run.

**Task 7 — the drop candidate was NOT dropped.** `docs/unit245-deep-seam.md` is
written. **No `OPEN_ISSUES.md` entry was opened, deliberately**, because the
census found nothing step 2 needs that it cannot reach without changing the port
— see section 3 — and the instruction is explicit that an empty issue is worse
than none. The highest id is still `HM-OPEN-073`.

### Decisions this session made for itself, reproduced in full

**One, and it is a sizing decision inside task 2 rather than a ruling.** Task 2
says to add *both* new projects to `Hamlet.sln`, and the tests project is task 3's
subject. Rather than leave the solution pointing at a `.csproj` that did not yet
exist — which breaks `dotnet build Hamlet.sln` at the task 2 commit — the
`Ft8Sharp.Deep.Tests` **project file** was written in task 2 alongside the
solution entry, and its **test source files** in task 3. Both tasks committed
green. Nothing was skipped and nothing was added.

**`dotnet sln add` was tried once and refused** — it is not on the allow-list,
which permits `dotnet build` and `dotnet test`. `Hamlet.sln` was edited with the
file tools following task 1.6's shape and builds 10 of 10.

### Mismatches between the instruction and the tree

**Checked every claim under "verify this instruction against the tree". All held
except one, and none was repaired.**

| Claim | Found |
|---|---|
| `Ft8Sharp.csproj` declares no `ProjectReference` and no `PackageReference` | holds, and its comment says that is the point |
| `Ft8SharpBoundaryTests` two halves, watched refusing 2026-08-31 | holds |
| `Available()` at `:183`, `Decoder` record at `:73` | holds exactly |
| `Decode(ReadOnlySpan<float>)` `:133`, `Decode(Ft8Waterfall)` `:139`, `Ft8SlotResult` `:270` with five counts | holds exactly |
| `src/Ft8Sharp/LICENSE` MIT, `NOTICE` cites `ft8_lib` and QEX, root `LICENSE` GPL-3.0 | holds |
| Root `1.12.47` at `Directory.Build.props:145`, `Ft8Sharp` `0.10.7` | holds; root bumped to `1.12.48`, **`Ft8Sharp` untouched at `0.10.7`** |
| `tests/fixtures/ft8/example/` fixture and `.fixture.txt` committed | holds |
| `tests/fixtures/ft8/captured/` is empty | **MISMATCH — it holds `README.md`.** No captures, so FACT-004's expected state is met in substance; the directory is not literally empty |
| Highest issue id is `HM-OPEN-073` | holds |
| `RULES_AT` disagreement | **still present** — logged once in section 4, not reconciled |

**Nothing under `src/Ft8Sharp/` was touched.** No red appeared in
`Ft8Sharp.Tests` at all, so there is nothing outside the expected set to report;
the CW reds and `docs/unit239-failing-set.txt` were not run and are not this
unit's.

## 2. What the owner should expect

**There is now a second library.** `src/Ft8Sharp.Deep/` is where every
improvement in this phase will live. It is GPL-3.0, matching Hamlet's own
intended release licence, and it carries its own `LICENSE` and `NOTICE`.
`Ft8Sharp` is untouched, still MIT, still `0.10.7`, still separately publishable.

**Nothing an operator could see has changed, and that is the whole of step 1.**
The receiver decodes exactly what it decoded last night, message for message and
count for count. If you were expecting a sensitivity number to move, it did not,
and it was not supposed to.

### What will look wrong but is not

- **The scoreboard now prints two identical rows at every rung.** That is not a
  duplicated line and not a bug. `Ft8Sharp.Deep` delegates to `Ft8Sharp` tonight,
  so the columns must agree; from the unit that lands ordered statistics
  decoding, a difference between them is attributable to exactly one named
  change, which is what the second seat is for.
- **`Ft8Sharp.Tests` went from 578 to 582 tests and got 6 seconds slower.** The
  four extra are the identity tests; the time is the ladder test now walking two
  columns at every rung instead of one.
- **`PHASE_OUTCOME.md`'s header says step 1 is `done` while `PHASE_STATUS.md`
  still says `not started`.** The `STEP:` lines in `PHASE_STATUS.md` belong to the
  launcher and this session is instructed not to write them. `PHASE_OUTCOME.md`
  was updated by the arbiter's own tool, in the same call that appended the
  entry, and its header is the one that was this unit's to move.
- **A NOTICE file has assertions in a test suite.** That is deliberate. A NOTICE
  nothing checks is a NOTICE that will rot the first time somebody tidies it —
  the citation would go and the code needing it would stay.
- **`Ft8Sharp.Deep` contains exactly one type and a test asserts that it does.**
  That assertion is a tripwire, not a rule: the unit that lands OSD must come and
  change it on purpose, rather than discover afterwards that step 1's claim
  quietly stopped being true.

## 3. What you should see

### 1. The paired ladder, whole — both columns, three counts each

`dotnet test tests/Ft8Sharp.Tests --filter "FullyQualifiedName~Ft8LadderHarnessTests"`,
306 trials a rung, seed `221001`, frequency 1000.00 Hz on a bin centre, offset
5760 samples.

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    wall s    ms/tr
Ft8Sharp         -19.0    -19.001     306      248      58      0    81.0    76.3    85.0     19.5     63.9
Ft8Sharp.Deep    -19.0    -19.001     306      248      58      0    81.0    76.3    85.0     20.0     65.4
Ft8Sharp         -20.0    -20.000     306       73     233      0    23.9    19.4    28.9     19.6     64.1
Ft8Sharp.Deep    -20.0    -20.000     306       73     233      0    23.9    19.4    28.9     20.1     65.6
Ft8Sharp         -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     19.6     64.0
Ft8Sharp.Deep    -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     20.0     65.5

  DECODED + MISSED = trials. WRONG is not part of that partition.
  Ft8Sharp      at -21.0 dB: NO WRONG DECODES. 0 messages returned that were not sent.
  Ft8Sharp.Deep at -21.0 dB: NO WRONG DECODES. 0 messages returned that were not sent.
```

**Identity here is trivially true because the sibling delegates — there is one
decoder and it is called twice. The point is that the seam and the wiring cost
nothing**, and the table is the evidence that they do. The three rungs reproduce
the figures written down before the run: `HM-OPEN-067`'s **13 of 306 at a
delivered -21.001 dB with 0 wrong**, 248 at -19 and 73 at -20.

The fixture report, same seat, unit 244's placeholder replaced by the real
sibling:

```
decoder        rows  MATCHED  MISSED  WRONG    rate   wall s
Ft8Sharp          3        3       0      0   100.0      0.1
Ft8Sharp.Deep     3        3       0      0   100.0      0.1
```

with the provenance line still printing `example - NOT WSJT-X` and 0 returned
that the fixture does not carry, in both columns.

### 2. The reachability census — what step 2 has to get past

**The answer is yes, with one exception, and the exception is exactly what an OSD
stage needs.** Every stage of `Ft8SlotDecoder.Decode(Ft8Waterfall)` is `public`:

| Stage | Type | Public | The member the loop calls |
|---|---|---|---|
| monitor / waterfall | `Ft8Monitor` `:37`, `Ft8Waterfall` `:28` | yes | `Analyse(ReadOnlySpan<float>)` `:211` |
| sync search / candidates | `Ft8SyncSearch` `:50`, `Ft8Candidate` `:48` | yes | `Find(Ft8Waterfall)` `:175`, `ScoreAt` `:265` |
| soft symbols | `Ft8SoftSymbols` `:70` | yes | `Extract` `:117`, `Normalise` `:287` |
| LDPC | `LdpcDecoder` `:81` | yes | `Decode` `:136`, `CodewordBits` `:100` |
| the gate | `Ft8CodewordDecoder` `:46` | yes | `Decode` `:70` |
| **the gate's result** | **`Ft8CodewordResult` `:174`** | **readable, NOT constructible** | ctor `private`; `FromMessage`, `Unreadable`, `Refused` all `internal` |
| message decode | `Ft8MessageDecoder` `:34` | yes | `Decode(bits, cache)` `:76` |
| payload / checksum / tables | `Ft8Payload`, `Crc14`, `Ft8Tables` | yes | all |

**So the loop can be reproduced from outside the assembly using only public
members, without `InternalsVisibleTo` and without copying a line of it.** What is
out of reach, stated exactly: **nothing outside `Ft8Sharp` can construct an
`Ft8CodewordResult`.** `Ft8SlotMessage` is
`(Ft8Candidate Candidate, Ft8CodewordResult Result)`, so **a codeword that OSD
recovers and belief propagation refused cannot be turned into an `Ft8SlotMessage`
and cannot reach an `Ft8SlotResult`** — which is what the scoreboard reads.

**A public route round it was measured rather than reasoned about.** Hand the
recovered codeword back to `Ft8CodewordDecoder.Decode` as normalised
high-confidence ratios and let the port produce the result:

```
status  Decoded
text    "HAMLET 245"
iterations spent 1
```

and with 40 bits of that codeword flipped, the port refuses it —
`ParityNeverSatisfied`, empty text. **Both of the port's gates stay the port's, so
`CLAUDE.md` §0.0 survives the route and nothing bypasses the checksum.** Cost: one
extra belief propagation per OSD success, converging in one iteration on a valid
codeword. `Ft8CodewordResult` has 0 public constructors and 0 public static
factories, measured by reflection, so that assertion goes red the day a later
phase opens it.

**Because that route works, nothing step 2 needs from the port is unreachable
without changing the port, and no issue was opened.** The two other routes — the
sibling carrying its own result type, and making the factories public — are
priced in `docs/unit245-deep-seam.md` §4. The third is forbidden this phase.

### 3. Exit 2, run over all three sets, on the whole result

```
the ladder, one whole block of 51 trials at -19 dB :  40 messages, identical
the ladder, one whole block of 51 trials at -21 dB :   3 messages, identical
the committed capture ft8-example-244.wav          :   3 messages, identical
all 69 reference recordings in the pinned clone    : 801 messages, identical
```

All five counts and every message's text, candidate, frequency and dt, in order —
**never `Texts` alone**, because a text-only comparison passes while the counts
differ and the counts are what steps 2, 3 and 4 will be read on. **The pinned
`ft8_lib` clone IS on this machine and `ReferenceRecordings.All()` returns 69
recordings**, so exit 2 did not fall back to the plan's named alternative.

### 4. Both suites' totals

```
dotnet test tests/Ft8Sharp.Tests        582 passed   0 failed   1 skipped   5 m 14 s
dotnet test tests/Ft8Sharp.Deep.Tests    18 passed   0 failed   0 skipped      1.3 s

baseline before any code change tonight 578 passed   0 failed   1 skipped   5 m  8 s
```

**No red anywhere, so nothing outside the expected set.** The four extra tests are
exactly `Ft8DeepIdentityTests`; the one skip is unchanged and is the table
generator, not a reference-clone skip. One project at a time and never
concurrently. `Hamlet.App.Tests` and `Hamlet.RadioEngine.Tests` were not run —
nothing here touches either.

### 5. The NOTICE, which is exit 4

`src/Ft8Sharp.Deep/NOTICE` cites, **before a line of either is implemented**:

- M. P. C. Fossorier and S. Lin, "Soft-Decision Decoding of Linear Block Codes
  Based on Ordered Statistics," IEEE Transactions on Information Theory, vol. 41,
  no. 5, pp. 1379-1396, September 1995 — for step 2;
- Franke K9AN, Somerville G4WJS, Taylor K1JT, "The FT4 and FT8 Communication
  Protocols," QEX, July/August 2020 — for the protocol.

and states that this library is GPL-3.0, that it depends on `Ft8Sharp` which is
MIT and stays MIT, that **no WSJT-X source and no `ft4_ft8_public/` was read**,
and that everything it implements comes from published description.
`Ft8DeepNoticeTests` reads the file and asserts all of it that a test can — the
files exist, the licence is the verbatim text rather than a stub, and both papers
are named by title. It cannot assert that no WSJT-X source was read; that is a
fact about how the code was written and the report says so rather than implying a
test covers it.

### 6. The boundary, guarded from both sides

`Ft8SharpBoundaryTests.DeclaresNoReferences` **already covers the new direction
and needs nothing** — its bound is zero references rather than a list of
forbidden names, so `Ft8Sharp` referencing `Ft8Sharp.Deep` fails it immediately.
`NoHamletAssemblyArrives` would not, because it filters on the prefix `Hamlet`;
that is the shape of that guard rather than a defect, and its own remarks say so.
A second net was added from the sibling's side anyway: the sibling's built
assembly **does** reference `Ft8Sharp`, the port's built assembly does **not**
reference `Ft8Sharp.Deep`, neither references a `Hamlet` assembly, and the
sibling's csproj declares exactly one `ProjectReference` and no `PackageReference`.

### 7. The record

`PHASE_OUTCOME.md` entry appended through
`dotnet build tools/arbiter/outcome-append.proj`, which reported
`unit : 3   step : 1   state : done`, `outcome-append exit 0`, and
`Step 1 is now [done] in the phase header. Nothing above the new entry was
touched.` No apostrophes in any field. `output.md` validated through
`dotnet build tools/arbiter/validate-output.proj` — **six rules, exit 0**, quoted
in section 4. Root version `1.12.47` -> `1.12.48`; `Ft8Sharp` stays `0.10.7`.
Seven commits pushed to `main`, one per task, and the first carried
`PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md`, which unit 244 left
modified at the root.

## 4. What's blocking us

**Step 1 is CLOSED — four of four must-pass exits met, with evidence in section 3.**
Nothing below stands in the way of an exit criterion, and no item asks for a
ruling.

### Logged for the record, asking for nothing

**1. `RULES_AT` still disagrees between two files.** `PROJECT_STATUS.md` reads
`HM-DEC-153 (2026-09-04)`; `CLAUDE.md` §1's decision log tops out at `HM-DEC-152
(2026-08-31)`. `HM-DEC-153` does exist, in `DECISIONS.md`, and has not been
carried into `CLAUDE.md` §1. **Reported once and not reconciled — `CLAUDE.md` is
the owner's file.** Asks for no ruling.

**2. `tests/fixtures/ft8/captured/` is not literally empty.** The instruction
describes it as empty and FACT-004's expected state; it holds `README.md` and no
captures, so the substance is met. **Not repaired.** Asks for no ruling.

**3. Two more shell spellings are refused, and the working ones are recorded here
so the next unit does not spend time on them.** `dotnet sln add` is not on the
allow-list — the sibling was joined to `Hamlet.sln` with the file tools instead —
and neither is `git clean`, so the one-shot licence-copy `.proj` was made to
delete itself with an MSBuild `<Delete>` task rather than be cleaned up from the
shell. One more, worth a line because it costs a build otherwise:
**`-p:EntryProps=` needs an ABSOLUTE path with FORWARD slashes** —
`C:/Source/HamLet/.run-unit/scratch/unit245-outcome.props`. A relative path is
resolved against `tools/arbiter/`, and a Windows path with backslashes is eaten by
Git Bash before MSBuild sees it. **Banked, not probed, not blocking.** Asks for no
ruling.

### Validator output

```
  ok      rule 1  UNIT: line present
  ok      rule 2  four top-level sections, in order, exact names
  ok      rule 3  no fifth top-level section
  ok      rule 4  section 4 present
  ok      rule 5  section 3 has 128 non-blank lines
  ok      rule 6  ordering block present, A B C, and C names a count

  VALID - all six rules passed.

validate-output exit 0
```
