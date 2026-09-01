READ IN THIS ORDER — A, then B, then C.

A. PHASE — Hamlet hears FT8 off the radio and displays the decoded text on screen.
Seven steps. Step 1, the library exists and its tables are proven, is **closed**, and its
entry for step 2 was re-verified afresh in task 1 rather than inherited from unit 205's
report: `Ft8Sharp.Tests` measured 38 total / 37 passed / 0 failed / 1 skipped in 291 ms by
path, the library built at 0 warnings and 0 errors, `git diff --name-only 2828ab6..HEAD`
gave 37 paths with none under `src/Hamlet.App/`, `src/Hamlet.RadioEngine/`,
`tests/Hamlet.App.Tests/` or `tests/Hamlet.RadioEngine.Tests/`, and the three channels ran
55 + 13 = 68 tests all green including `DecisionLogOrderTests`. Step 1's last criterion as
`PHASE_PLAN.md` reads today is **attribution from the phase boundary plus the channel
tests, not the whole suite** — no unit runs the whole suite and no step requires it. This
is the **first unit of step 2**, which stands at 3 of 6 must-pass criteria after tonight.
Steps 3, 4, 5, 6 and 7 remain **unreachable until step 2 closes**: every step depends on
the one before it by the plan's own named deviation, and step 3 cannot start without the
77-bit layer this unit begins.
B. STEP 2 — messages round-trip through 77 bits. SIX exit criteria: (1) CRC matches known
values, must-pass — **demonstrated**; (2) standard, free-text, telemetry and
non-standard-callsign messages round-trip across a large generated corpus, must-pass —
**untouched, no packing exists yet**; (3) any random 77-bit pattern either decodes or fails
cleanly and never throws, must-pass — **container half done, criterion stays open**; (4)
contest and DXpedition types round-trip, nice-to-pass, with "an unsupported type must fail
as unsupported and never as a wrong decode" must-pass — **untouched, no packing exists
yet**; (5) Ft8Sharp tests green, must-pass every unit — **demonstrated**; (6) attribution
clean from 2828ab6 and the channel tests green, must-pass every unit — **demonstrated**.
For criterion 1, **all three legs landed**: leg A, provenance, matched both scalars against
the pin by machine; leg B, an independent checker that never calls the library, agreed over
504 messages at each of 20 bit lengths with 0 disagreements; leg C, linearity, held, so 77
basis computations cover all 2^77 messages. **An external known-value vector does exist in
the clone** — one, in `test/test.c` — **and it is stale**: it sits inside a block comment
and disagrees with the constants declared beside it, and a 1458-way search reproduces it
under no reading of them. For criterion 3, the container round-tripped 10 081 messages,
never threw for any of 100 000 arbitrary 12-byte buffers, and refused all 25 207 single-bit
corruptions — but **the criterion stays open until an unpacker exists**, because it is about
a 77-bit pattern *decoding* and nothing in this tree decodes yet. Numbers: `Ft8Sharp` tests
**74 total, 73 passed, 0 failed, 1 skipped**; attribution **44 paths, 0 of them Hamlet**;
channels **AudioSeamTests + PrivilegeTests 55 green, DecisionLogOrderTests + VersionTests +
DecisionEmissionTests + VoiceTests 13 green**, with `VersionTests` re-run green after the
version bump.
C. THIS REPORT — every one of the 10 081 corpus payloads cleared all 83 LDPC parity checks
through `LdpcCheck`, the first time a message-shaped payload has gone through the proven
encoder. Task 5 was **not dropped**: it ran, and stopped on its own stated condition,
because the alphabets are not tables a converter could read — they are enumeration members
with the mapping computed by branching arithmetic, so `Ft8Tables.g.cs` was never opened.
The `Ft8Sharp` project still runs in seconds — 5.96 s including a rebuild, 0.59 s warm.
Section 4 raises 4 items, and **none of them stands in the way of a criterion named in B**.

UNIT:       206 — complete at task 6 of 6 — 2026-09-01 11:05
PHASE GOAL: Hamlet picks FT8 out of the air through the radio it already talks to, and puts
            the decoded text on the screen.
UNIT GOAL:  Build and prove the two things every FT8 message sits on — the CRC-14 that says
            a decode is real rather than a guess, and the 77-bit envelope that carries a
            message into the LDPC encoder — so that when packing arrives next unit, a failed
            round trip is the packer's fault and not the plumbing's.
ADVANCED:   yes — criterion 1 closed on three independent legs, and the container half of
            criterion 3 built and proven, with criteria 5 and 6 re-demonstrated.
NUMBER:     step 2 must-pass criteria demonstrated: 0 -> 3 of 6
DRIFT:      0 consecutive units without advance  (was 0 — unit 205 advanced)

# 1. What Claude did

**Complete, at task 6 of 6.** No task was dropped. Task 5, the named drop candidate, ran
and stopped on the condition the instruction itself set for it.

This machine, `C:\Source\HamLet`, project gate `PROJECT: Hamlet` verified against the tree —
`SHACK_FACTS.md` and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` both present,
`Hamlet.sln` the only solution, no `CoreHMI.sln` and no `MURC.sln`. Branch `main`, and every
task was committed and pushed to `origin/main` as it finished. Six pushes, all succeeded,
none refused. `HEAD` is now `21a35e8`.

## What was traced, built and measured

**Task 1 — step 2's entry, re-verified.** `HEAD` was `a167390` on `main` as stated.
`Ft8Sharp.Tests` ran 38 / 37 / 0 / 1 in 291 ms. `dotnet build src/Ft8Sharp/Ft8Sharp.csproj`
gave 0 warnings and 0 errors, and the `.csproj` confirms `net8.0`, nullable enabled,
warnings as errors, and zero `PackageReference` and zero `ProjectReference`. `LICENSE`,
`NOTICE` and `porting-notes.md` are all present under `src/Ft8Sharp/`, and the notes record
the pin. `Ft8SharpBoundaryTests` was green in that run; the boundary refusal was **not**
re-witnessed by adding a reference, as unit 201 recorded it at `bb5ba9d`. Attribution gave
37 paths, 0 of them Hamlet. Channels gave 55 and 13, every named class green.

**Task 2 — the sanctioned read.** The clone answered, so the port had a route. A new
inventory test extends `ReferenceCloneProbeTests` rather than duplicating it. Reading the
source *for* the port is gated behind `FT8_CRC_SOURCE_DUMP=1`, mirroring the
`FT8_TABLEGEN_WRITE` idiom the converter already uses, so no ordinary run prints third-party
source. Two defects in the existing test-side C reader were found on the way and fixed —
both are described in section 3.

**Task 3 — the CRC and its three legs.** `src/Ft8Sharp/Message/Crc14.cs`, ported from
`ft8/crc.c`. Leg A asserts both scalars against the pinned header at run time. Leg B is
`tests/Ft8Sharp.Tests/Message/CrcCheck.cs`, which never calls the library and does not run
in the same direction. Leg C is linearity, established from the ported source and then
measured. Every one of the 77 single-bit changes to a message changes its checksum.

**Task 4 — the container.** `src/Ft8Sharp/Message/Ft8Payload.cs`, with all five assertions
answering over one corpus of 10 081 messages.

**Task 5 — the drop candidate, run.** The alphabet inventory settled it: there is nothing
for a converter to read.

**Task 6 — the record.** A *message layer* section in `porting-notes.md`, both versions
bumped, `VersionTests` re-run **after** the bump, and the attribution re-measured.

## Decisions this session made for itself

**One.** The work instruction says leg A reads `ft8/crc.h`. The two scalars are not there —
`crc.h` declares only the three functions, and both macros live in `ft8/constants.h`, which
`crc.c` includes. The provenance test follows the tree, and says so in its own doc comment.
This is a mismatch reported, not repaired: the instruction is left as written.

**Two.** The upstream vector's disagreement was resolved by measurement rather than by
assertion in either direction. The test asserts what is actually known — that the two
independent implementations agree on upstream's own input, and that the stated value is
unreachable under a bounded search of 1458 readings — rather than asserting a match it has
not got or a mismatch a later correction would have to break. Had the search found a hit,
the test says in terms that the port is what needs looking at.

**Three.** `Ft8Payload.Create` **refuses** a message with bits set past its 77th, where
upstream's `ftx_add_crc` silently clears them. This is the same reasoning already ratified
in `LdpcEncoder`, which refuses a payload with its spare bits set. It is a refusal, never a
different answer: no message this refuses would have encoded differently, it would have
encoded as though the bits had never been set.

**Four.** No decision id was minted. Nothing here is a ruling.

# 2. What the owner should expect

`src/Ft8Sharp/` now has a `Message/` folder with two files in it, and
`tests/Ft8Sharp.Tests/` has a `Message/` folder with four. The library is at **0.2.0** and
Hamlet is at **1.12.13**. `porting-notes.md` has a new top-level section, *The message
layer*, which is the longest thing in this delivery and is where the reasoning lives.

**What will look wrong and is not:**

- **`Ft8Sharp.Tests` reports 1 skipped.** That is `RewriteTheCheckedInTablesFile`, by
  design. On a machine without the pinned clone, **nine** would skip rather than the six the
  instruction predicted — six existing plus the three added tonight. A skip is not a failure.
- **A test named `UpstreamsOneStatedVectorIsDisabledAndDoesNotAgreeWithItsOwnConstants`
  passes.** It is green because upstream's disabled comment is stale, which is the finding,
  not because the CRC matched it.
- **`git status --short` still prints 35 lines.** Those are the loop's own uncommitted files
  and they are another author's; they were counted and not committed.
- **`tests/Ft8Sharp.Tests/TempEncoderProbe.cs` is still on disk**, untracked and emptied to a
  comment. A fourth deletion was not attempted.
- **The `Ft8Sharp` build emits no warning about the version change**, because
  `src/Ft8Sharp/Directory.Build.props` deliberately does not inherit the root one. Two
  version numbers in the tree is HM-DEC-152 working, not drift.

# 3. What you should see

## Criterion 1 — the verdict

| Leg | Verdict | The fact that settles it |
|---|---|---|
| **A — provenance** | **landed** | Both scalars **matched the pin**, read out of `ft8/constants.h` at run time by a checked-in test and compared by machine. The test names which constant matched and prints no value. |
| **B — independent checker** | **landed** | **10 080 messages** — 504 at each of 20 bit lengths from 0 to 96, seed **20260901** — with **0 disagreements**. `CrcCheck` never calls `Crc14` and does not run in the same direction. |
| **C — linearity** | **landed** | The shape allowed it: **zero initial remainder, no final XOR**. 2000 seeded XOR pairs, 0 non-linear results. **Every 77-bit message is the XOR of a subset of the 77 weight-one messages, so its checksum is the XOR of that subset of the 77 basis checksums — the 77 basis computations therefore determine the checksum of all 2^77 messages, the whole map rather than a sample of it.** The conclusion was then tested end to end: 20 000 random messages reconstructed from the basis alone, 0 mismatches. |

**Does an external known-value vector exist in the pinned clone? Yes — one — and it is
stale.** It is in `test/test.c`: a ten-byte input, a stated bit count and a comment giving
the expected checksum. **It is inside a block comment**, so no upstream build runs it and
nothing has been keeping it honest, and **it does not agree with the constants declared
beside it**. A bounded search over **1458 readings** — the pinned polynomial and the pinned
polynomial with its leading term restored, every register width from 8 to 16, and every bit
count the vector could carry — **reproduces it under none of them**, which is where a
transposed digit or a mistaken bit order would have shown up. Both independent
implementations agree with each other on that same input.

## The container's five assertions

1. **Round-trip** — **10 081 messages** (the 77 weight-one, four fixed patterns, 10 000
   seeded random, seed 20260901): **0 refused, 0 came back altered**.
2. **Spare bits zero** — the same 10 081 payloads: **0 with any spare bit set**, asserted
   directly on the payload rather than inferred from the encode succeeding.
3. **All 83 LDPC checks** — the same 10 081 payloads encoded with `LdpcEncoder` and checked
   with the existing `LdpcCheck`: **0 payloads with any failure, 0 failed checks in all**.
   **This is the first time in this phase that a message-shaped payload has gone through the
   proven encoder.**
4. **Single-bit corruption refused** — 277 payloads × all 91 bit positions = **25 207
   corruptions, 0 accepted as valid**. Every flip made on a copy; nothing checked in was
   corrupted.
5. **Never throws** — **100 000 random 12-byte buffers**, seed 20260901, of which **96 799
   carried spare bits that make them illegal payloads: 0 exceptions**, 0 validated, 100 000
   refused. **Criterion 3 stays open until an unpacker exists**, because the criterion is
   about a 77-bit pattern *decoding* and nothing in this tree decodes yet.

## Ft8Sharp.Tests, before and after

| | Total | Passed | Failed | Skipped | Wall clock |
|---|---|---|---|---|---|
| Before (task 1) | 38 | 37 | 0 | 1 | 291 ms |
| After (task 6) | **74** | **73** | **0** | **1** | 5.96 s incl. rebuild, 0.59 s warm |

**36 tests added.** By file: 4 in `ReferenceCloneCrcInventoryTests`, 2 in
`UpstreamCrcProvenanceTests`, 23 in `Crc14Tests` (20 of them the bit-length theory), 7 in
`Ft8PayloadTests`. The project still runs in seconds, which is the property that makes this
phase's inner loop work.

## Attribution and the three channels

- **Attribution: 44 paths** changed since `2828ab6`, up from 37. **0 under
  `src/Hamlet.App/`, `src/Hamlet.RadioEngine/`, `tests/Hamlet.App.Tests/` or
  `tests/Hamlet.RadioEngine.Tests/`.** 36 commits since the boundary.
- **No new shared artifact was added**, so no new channel is opened. The one shared artifact
  this unit touched is the root `Directory.Build.props`, which channel 2 already reads.
- **Channel 1 — `AudioSeamTests` + `PrivilegeTests`: 55 tests, 55 green.**
- **Channel 2 — `DecisionLogOrderTests`, `VersionTests`, `DecisionEmissionTests`,
  `VoiceTests`: 13 tests, 13 green.** Each named class green; `DecisionLogOrderTests` is not
  red.
- **`VersionTests` re-run after the bump: 3 tests, 3 green**, at 1.12.13. Unit 205 measured
  channel 2 before its bump and said in its own report that its verdict was therefore
  reasoning rather than measurement. That gap is closed.

## What task 2's inventory found in the clone, as names and shapes only

- **`ft8/crc.h`** — present, 869 bytes, 31 lines. Declares three functions and two includes.
  **No CRC scalar is declared in it.**
- **`ft8/crc.c`** — present, 1884 bytes, 64 lines. One macro, the same three functions, and
  it includes `crc.h` and `constants.h`.
- **`ft8/constants.h`** — where both CRC scalars actually live, as macros. 18 integer macros
  resolve in it once the reader was fixed.
- **The clone's test sources** — a `test/` directory exists, `tests/` does not. **One**
  candidate C source, `test/test.c`, 11 060 bytes, 287 lines, and it does mention CRC. Nine
  functions in it. `crc.c` itself declares no `main()` and contains no `assert()`, so there
  is no self-test there.

## What task 5 did

**It ran, and it stopped on its own stated condition.** The alphabets are **not C string
literals and not braced arrays** — 15 sources scanned in `ft8/`, **0 alphabets as literals,
0 as char arrays**. They are **six enumeration members in `ft8/text.h`**, with lengths of
42, 38, 37, 27, 36 and 10 stated in the comment beside each, and the mapping between a
character and its index is computed by arithmetic and branching in `ft8/text.c`. There is
nothing for the converter to lift, and reading a table out of somebody's comment is worse
provenance than transcribing one. **`Ft8Tables.g.cs` was never opened, so step 1's criteria
4 and 5 are exactly where they were.** What unit 207 inherits is a port of two small
functions rather than an extraction of six tables.

## Two defects found in the test-side C reader, and fixed

- **`CSourceParser.ParseIntegerMacros` was silently dropping macros on CRLF lines.** Its
  regex anchored on a plain end-of-line, which matches before a newline and not before a
  carriage return, and **the pinned `constants.h` has mixed line endings**. Any `#define` on
  the wrong side of that boundary resolved as *unresolved*, which the caller reports as a
  gap in corroboration rather than as a contradiction — the quietest of the three possible
  answers. It surfaced only because the two CRC scalars sit on opposite sides of it and one
  came back without the other. Fixed by matching the terminator explicitly.
- **`ExpressionEvaluator` had no cast form**, and upstream writes one of the CRC scalars as
  a cast literal. A cast to a fixed-width integer type is now applied rather than ignored, so
  a macro that truncates in C truncates here too.

**`Ft8TableGenerationTests` is green after both changes**, including
`CheckedInTablesAreWhatTheConverterProduces`, in the 74-test run above.

## The two version numbers as they now stand

- **`src/Ft8Sharp/Directory.Build.props`: 0.1.0 → 0.2.0**, under HM-DEC-152. Minor rather
  than patch because the library gained a capability of its own — the checksum and the
  envelope — rather than a correction to what it already did. The reason is written into both
  the props file and the notes so the bump reads as deliberate later.
- **Root `Directory.Build.props`: 1.12.12 → 1.12.13**, under HM-DEC-150, patch per work unit.

## Mismatches between the work instruction and the tree

Reported, not repaired, and none of them stopped the work.

1. **The CRC scalars are not in `ft8/crc.h`.** They are macros in `ft8/constants.h`. The
   instruction's leg A is written against the wrong file; the test follows the tree.
2. **`ft8/pack.c` and `ft8/unpack.c` are not in the pin.** Both are absent. Packing and
   unpacking live in `ft8/message.c`, 37 805 bytes, 1156 lines. Task 5 and unit 207 both
   name files that do not exist.
3. **`git status --short` printed 35 lines, not 34** — 5 modified and 30 untracked. The extra
   entry is `SESSION.lock`, which was not there when the arbiter counted.
4. **The instruction predicts six skips on a machine without the clone.** It is now nine,
   because this unit added three clone-dependent tests.

## The known items, confirmed

Known item 1 holds: `PHASE_STATUS.md` says `STEP: 1 | done` with `CURRENT_STEP: 2`, while
`PHASE_OUTCOME.md`'s header and its last entry both say `partial`. **Neither file was
touched.** `TempEncoderProbe.cs` is still on disk, untracked and emptied to a comment; no
fourth deletion was attempted. The loop's own files were counted and not committed.

## One refusal, and one thing this session got wrong about itself

**Shell output redirection into the tree was refused**, exactly as the instruction predicted
for unit 204 — appending to `porting-notes.md` through a heredoc was blocked and the file
tool was used instead. `python` execution was also refused. **Neither was worked around**;
both are reported as refusals. No process was enumerated or killed, no `dotnet run` was
attempted, and nothing outside the repository root was touched by the agent's own file tools.

**The `UPDATED` stamp written after task 1 was composed rather than read** — 10:46 against a
clock reading 10:41. It was caught at the next write, corrected from the clock, and named in
the status note at the time. Every later stamp was read. This is the failure units 203 and
204 both reported against themselves, and it defeats the one signal that catches a stopped
session, so it is recorded here rather than left in the commit history.

# 4. What's blocking us

**Nothing is blocking.** None of the four items below stands in the way of a criterion named
in section B, and none is a ruling request. The pinned clone was reachable all night, so the
one thing that would have been a ruling request did not arise.

**Item 1 — a note, for the record rather than for a decision.** The only external CRC
known-value vector in the pinned clone is disabled code and does not agree with the
constants beside it. Criterion 1 therefore rests on the three legs rather than on an
external vector, which is what the work instruction anticipated for the case where no vector
exists at all. **The end-to-end settlement still arrives in step 3**, where the symbol
sequence is compared bit-identically against `ft8_lib`'s, and that is where a systematically
wrong CRC would show up if this reading is somehow wrong. No action is asked for.

**Item 2 — an observation for whoever writes unit 207.** The instruction's file names for
the packing layer are wrong about the tree: `pack.c` and `unpack.c` are not in the pin, and
the alphabets are not tables. Unit 207 is a port of `ft8/message.c` and the two character
functions in `ft8/text.c`, not an extraction. Recorded in `porting-notes.md` so it does not
have to be rediscovered.

**Item 3 — a defect already acted on.** The test-side C reader was silently dropping every
`#define` on a CRLF line, for five units, in a file with mixed line endings. It is fixed and
`Ft8TableGenerationTests` is green. It is raised because *silently unresolved* was the
failure mode, and the same shape of fault could exist elsewhere in `tools/` — which this
session may not edit and did not.

**Item 4 — the three reported-but-unfixed loop faults, carried forward unchanged.**
`PHASE_OUTCOME.md`'s `partial` against `PHASE_STATUS.md`'s `done`; the `UNIT 1` mislabelling
in `PHASE_OUTCOME.md`; and the reload's `CPS-DEC-0152` mangling of `HM-DEC-152`. All three
were confirmed still present and none was touched, `tools/` not being this session's.
