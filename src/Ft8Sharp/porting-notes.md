# Porting notes — Ft8Sharp

What this library was ported from, what it deliberately does not touch, and what
it knowingly inherits. Written so that nobody has to reconstruct any of it later
from commit messages.

---

## Provenance

Everything ported into Ft8Sharp comes from **ft8_lib by Kārlis Goba**, pinned at:

```
commit  9fec6ca39886edbf96f4f5e71edc76da5074e871
clone   C:\Source\ft8_lib
```

The clone lives **outside this repository and is never committed**. It is a
reference to read, not a dependency to ship, and Ft8Sharp carries no build-time or
run-time link to it.

The pin is what makes a port reviewable: any table, any constant and any piece of
structure in this library can be checked against exactly one upstream revision,
and a later upstream change cannot silently invalidate a note written here.

**The pin is verified.** Measured 2026-08-31 by
`ReferenceCloneProbeTests.TestProcessCanReachThePinnedReferenceClone`, which reads
the clone's `.git\HEAD`, follows the ref it names, and compares the result against
the commit above in full:

```
HEAD read via  C:\Source\ft8_lib\.git\HEAD -> refs/heads/master
                                           -> C:\Source\ft8_lib\.git\refs\heads\master
HEAD           9fec6ca39886edbf96f4f5e71edc76da5074e871
pin            9fec6ca39886edbf96f4f5e71edc76da5074e871
match          yes
```

The comparison is not a one-off reading recorded here. It is an assertion in the
test suite, so a clone that later moves off the pin turns the suite red rather than
letting a port quietly acquire a second provenance.

`ft8/constants.c` is present at 15155 bytes over 392 lines, and `ft8/constants.h`
at 3728 bytes over 90 lines.

### How the clone is reached, and how it is not

Which door the tables came through is provenance too. Three routes were tried on
2026-08-31 and the results belong here rather than in a report nobody will find:

| Route | Result |
|---|---|
| The agent's own file tools reading `C:\Source\ft8_lib\ft8\constants.c` | **Refused.** *"…is outside C:\Source\HamLet; --restricted confines the file tools to the working directory."* |
| A shell command — `git -C C:\Source\ft8_lib rev-parse HEAD` | **Refused.** *"This command requires approval."* |
| A checked-in test, run by `dotnet test` | **Works.** The clone is read by the test process with the operating system's permissions. |

The first two are checks on the agent's tools; the third is a compiled program
reading a file. **So everything this repository learns about the clone is learned
by something checked in and run by `dotnet test`** — which is the auditable route,
and which is also a constraint on the converter that follows: `dotnet run` is not
available to it, so it must be reachable as a test.

Nothing was copied out of the clone to make any of this work, and no route around
the refusals was attempted.

## The two licences

The Hamlet repository is licensed **GPL-3.0** (see the `LICENSE` at the repository
root). **Ft8Sharp is licensed MIT** (see the `LICENSE` beside this file). MIT nests
inside GPL-3.0 without difficulty: MIT is a permissive licence, GPL-3.0 is free to
incorporate MIT-licensed code, and the combined work as distributed from this
repository remains GPL-3.0.

The point of the MIT grant is extraction. Ft8Sharp is built to be lifted out of
Hamlet and published on its own, and an MIT library is one that others can
actually use.

## The library carries its own version number

**`src/Ft8Sharp/Directory.Build.props` does not import the root one, and that is
deliberate** (HM-DEC-152, ruled by Tim 2026-08-31). Ft8Sharp is at **0.1.0** and
Hamlet is at whatever Hamlet is at.

The root `Directory.Build.props` injects `Version` — and with it `AssemblyVersion`,
`AssemblyInformationalVersion` and `BuildStampUtc` — into every project in the
solution, which is right for one application and wrong for a library built to be
lifted out of it. **The boundary test cannot catch this**: it walks assembly
references, and an injected attribute is not one. Measured by reading
`obj/Debug/net8.0/Ft8Sharp.AssemblyInfo.cs` on 2026-08-31, before and after:

| | Before | After |
|---|---|---|
| `AssemblyVersion` | `1.12.9.0` | `0.1.0.0` |
| `AssemblyFileVersion` | `1.12.9.0` | `0.1.0.0` |
| `AssemblyInformationalVersion` | `1.12.9+53a586e0579f84cb299189aa91d8b772877db33e` | `0.1.0` |
| `AssemblyMetadata("BuildStampUtc")` | present | gone |

That commit hash is Hamlet's. **An extracted Ft8Sharp would have published itself as
a version of a program it has never heard of, stamped with that program's git
history.** Dropping the inheritance was not quite enough on its own — the SDK appends
the source revision to the informational version without any help from the root file
— so `IncludeSourceRevisionInInformationalVersion` is off here too.

**0.1.0 because the library has tables and no decoder.** A 1.x would claim a maturity
it has not got, and the version is the first thing a reader of an extracted package
sees. It bumps when Ft8Sharp gains a capability of its own, not when Hamlet ships.

**0.2.0 on 2026-09-01, because it gained one.** The library now holds the CRC every
FT8 message is protected by and the 77-bit container every message travels in — the
first two pieces of the protocol itself it has ever carried, as against tables that
described one. That is a capability of its own by the rule written a paragraph above,
and it is a minor rather than a patch because it is an addition to what the library
can do rather than a correction to what it already did. Still not a 1.x: nothing here
decodes.

**0.3.0 on 2026-09-01, because it gained another.** The library now turns a message
into the 77 bits that go on the air and turns 77 bits back into the words a person
would read — the standard message, free text and telemetry, with a dispatcher in
front of them. That is the first time anything here has produced or consumed a
*message* rather than the envelope one travels in, so it is a minor by the same rule.
Still not a 1.x: there is no demodulator, so nothing here has yet read anything off a
radio.

This is not drift from HM-DEC-063, which exists so the tree has one answer to *what
version is this app*. Ft8Sharp is not the app: it is a separate work product with its
own licence, its own boundary and an intended life outside this repository, and a
second version number here is the same reasoning that gave it its own `LICENSE`.

## What is not read, ported, or referenced

**`ft4_ft8_public/` in the upstream repository is off limits.** It is not read, not
ported, not referenced, and not opened to see what is in it.

It is Fortran of uncertain provenance sitting inside an MIT repository whose
LICENSE names Goba's copyright. Routing the most licence-sensitive artifact in this
project — the constant tables — through a folder whose own licensing cannot be
established is the wrong risk for a library intended for publication. Nothing in
Ft8Sharp may take a route to a table or an algorithm through it, however much
easier that route would be.

The same applies to **WSJT-X**. It is not a source for this library.

## The one legal route for tables

Tables come from **`ft8/constants.c`**, machine-converted by a **converter checked
into this repository**. They are not transcribed by hand, not pasted from a
terminal, and not typed out of a report.

A converter is auditable — anyone can run it against the pinned commit and diff the
result. A transcription is not, and a hand-copied table is exactly the artifact a
future licence question cannot be answered about.

### The converter, and why it is a test

It lives at **`tests/Ft8Sharp.Tests/TableGen/`** and it is reached as a test rather
than as a console tool, for two reasons that are both about where it can run and
where it may ship.

`dotnet run` is not available in the loop this port is built in — only `build`,
`test` and `restore` are — so a console tool would be a converter nobody could
execute, which is no converter at all. And a parser of C source has no business
shipping inside a published decoder assembly: **`Ft8Sharp` gains no parser, no
package reference and no project reference from any of this**, and the boundary test
would refuse it if it did.

| File | Does |
|---|---|
| `TableGen/CSourceParser.cs` | Reads one C array by identifier: nested brace initialisers, block and line comments, hex and decimal literals with integer suffixes, trailing commas |
| `TableGen/ExpressionEvaluator.cs` | Evaluates the arithmetic a dimension macro is written as, so `FTX_LDPC_K_BYTES` can be corroborated rather than shrugged at |
| `TableGen/Ft8TableConverter.cs` | The manifest of six tables, the geometry it derives from them, and the emitter |
| `TableGen/TableComparison.cs` | Says which table differs and at how many positions, and never says by what |
| `Ft8TableGenerationTests.cs` | Runs it: the regeneration proof, and the write gate |
| `Ft8TableGeometryTests.cs` | The assertions below, against the checked-in file |

**To re-run it** — after a pin move, or on another machine:

```
dotnet test tests/Ft8Sharp.Tests -e FT8_TABLEGEN_WRITE=1
```

That is the **only** thing that writes `src/Ft8Sharp/Tables/Ft8Tables.g.cs`, and it
skips itself unless the environment variable is set, because a test that rewrites
the source tree every time somebody runs the suite is a trap. The clone's location
can be moved with `FT8_LIB_PATH`, as the probe already allows.

### What is checked in, and what proves it

**`src/Ft8Sharp/Tables/Ft8Tables.g.cs`** — one `public static class Ft8Tables`, each
table a `ReadOnlySpan<byte>` over a flattened literal, behind named stride constants
(`LdpcM`, `LdpcN`, `LdpcKBytes`, `LdpcNmRowWidth`, `LdpcMnRowWidth`) so a row is
addressable without a magic number. Its header names the pin, the source file, the
tool and the command above, and says **DO NOT EDIT BY HAND**.

**The header carries no generation time and no machine name, deliberately.** A clock
would make the file differ from itself on every run, and that would destroy the one
proof this whole arrangement exists for:

> `Ft8TableGenerationTests.CheckedInTablesAreWhatTheConverterProduces` parses the
> clone, emits into memory, and asserts the result equals the file on disk.

Measured 2026-08-31: **byte-identical, 20 043 characters on each side.** The
comparison **normalises line endings and nothing else** — `.gitattributes` says
nothing about `*.cs`, so whether the working copy holds LF or CRLF is a property of
one machine's `core.autocrlf` rather than of the port, and a test that went red over
that would teach the next reader to distrust it.

It **skips** rather than fails when the clone is absent, and that skip has been
watched by pointing `FT8_LIB_PATH` at a path that does not exist.

It has also been **watched refusing**. One element of `kFTX_LDPC_Nm` was altered in
memory, by machine, in a copy of the generated text — the checked-in file was never
touched — and the comparison reported `kFTX_LDPC_Nm: differs at 1 of 581 positions`,
named that table alone, and printed no value. That is what "reproducible against a
future upstream" buys: when the pin moves, this goes red and names the table that
changed, instead of the port quietly acquiring a second provenance.

### The six that came across, and the three that did not

Converted: `kFT8_Costas_pattern`, `kFT8_Gray_map`, `kFTX_LDPC_generator`,
`kFTX_LDPC_Nm`, `kFTX_LDPC_Mn`, `kFTX_LDPC_Num_rows` — 2197 elements in six tables.

**`kFT4_Costas_pattern`, `kFT4_Gray_map` and `kFT4_XOR_sequence` were deliberately
not converted.** They are in the same file and they are left in it. FT4 is parked for
this phase, and an unused, unproven table inside a library headed for publication is
a liability rather than a head start. The converter emits what its manifest names,
not everything it finds, and adding a table to it is a deliberate act.

### The geometry, proven without a decoder

A conversion can succeed and still be wrong — a table can arrive transposed,
truncated or shifted by one, and every one of those files compiles. Seven assertions
in `Ft8TableGeometryTests` run against the checked-in file, need no reference clone,
and all pass as of 2026-08-31:

- **The element counts and the derived geometry agree.** 996 = 83 × 12,
  581 = 83 × 7 and 522 = 174 × 3, so `FTX_LDPC_M` is 83, `FTX_LDPC_N` is 174 and
  `FTX_LDPC_K_BYTES` is 12 — the published LDPC(174,91) geometry. These are derived
  from the initialisers' own structure and were **corroborated against
  `ft8/constants.h`**, which resolved every declared dimension.
- **`kFT8_Gray_map` is a permutation of the eight tones**, each present exactly once.
- **`kFT8_Costas_pattern` is seven entries**, every one inside the 8-tone alphabet.
- **Every `kFTX_LDPC_Num_rows` entry is between 1 and 7, and they sum to 522** — the
  same number as `Mn`'s element count, because the two count the same incidence
  structure from opposite sides.
- **`Nm` is padded exactly where `Num_rows` says**: real to that length, zero after
  it, no zero inside the real part and no non-zero in the padding.
- **`Nm` and `Mn` are transposes of each other.** All 522 edges agree in both
  directions. This is the single strongest thing that can be asserted about these
  tables without running an encoder, and a wrong bit in either would fail it.

**None of this is LDPC parity.** Encoding a message and checking it against reference
parity is a separate piece of work, and nothing above stands in for it. It was done
on 2026-08-31 and it has its own section below.

### The index base is upstream's, and it was measured

**Both `Nm` and `Mn` are 1-based**, and that was measured from the data rather than
assumed: the entries cover their full ranges — all 83 checks, all 174 variables —
with no gaps, and the smallest is one, not zero. In `Nm`, zero is padding and never
an index, which is what makes a 1-based table the only reading that works.

**They are not renumbered.** A port that reads better and indexes differently is
exactly the failure mode this project's plan names for callsign hashing, and it
applies at least as hard here: every consumer of these tables — the encoder, the
belief-propagation decoder — has to subtract the same one, and it has to be written
down in one place rather than rediscovered by each of them.

### Table inventory

Measured 2026-08-31 from `ft8/constants.c` at the pinned commit, by
`ReferenceCloneProbeTests.ConstantsInventoryIsLegibleAsShapesOnly`. **Names and
shapes only, never values** — the values have one route into this repository and it
is the converter, not a note.

| C identifier | Type | Dimensions | Elements |
|---|---|---|---|
| `kFT8_Costas_pattern` | `const uint8_t` | `[7]` | 7 |
| `kFT4_Costas_pattern` | `const uint8_t` | `[4][4]` | 16 |
| `kFT8_Gray_map` | `const uint8_t` | `[8]` | 8 |
| `kFT4_Gray_map` | `const uint8_t` | `[4]` | 4 |
| `kFT4_XOR_sequence` | `const uint8_t` | `[10]` | 10 |
| `kFTX_LDPC_generator` | `const uint8_t` | `[FTX_LDPC_M][FTX_LDPC_K_BYTES]` | 996 |
| `kFTX_LDPC_Nm` | `const uint8_t` | `[FTX_LDPC_M][7]` | 581 |
| `kFTX_LDPC_Mn` | `const uint8_t` | `[FTX_LDPC_N][3]` | 522 |
| `kFTX_LDPC_Num_rows` | `const uint8_t` | `[FTX_LDPC_M]` | 83 |

Nine array definitions, 2227 elements in total, every one of them `uint8_t`. Three
things a later reader can take from the shapes alone:

- **The dimension macros are consistent with the element counts.** 996 = 83 × 12,
  581 = 83 × 7, 522 = 174 × 3, so `FTX_LDPC_M` is 83, `FTX_LDPC_N` is 174 and
  `FTX_LDPC_K_BYTES` is 12 — the published FT8 LDPC(174,91) geometry. The macros
  themselves live in `constants.h` and the converter reads them from there rather
  than inferring them as this note just did.
- **Five of the nine tables are FT4's or shared with it.** FT4 is not in this phase,
  and the converter emits what it is told to emit rather than everything it finds.
- **The `FTX_` prefix is upstream's**, marking what the two modes share. It is not a
  typo for `FT8_`.

The counts are what makes the converter checkable: a table that arrives in C# with
a different element count than the row above is wrong before anyone runs a decoder.

## The tables answer to parity

Everything above says the tables were *copied faithfully*. This section is what says
they are *right*: that `kFTX_LDPC_generator` and `kFTX_LDPC_Nm` are two descriptions
of one code and not of two. Done 2026-08-31.

### Where the encoder lives, and why there

**`src/Ft8Sharp/Ldpc/LdpcEncoder.cs`** — in the library, not in the tests. Turning a
payload into parity is part of what Ft8Sharp is *for*; the belief-propagation decoder
extends it rather than duplicating it, and putting it in the test project would mean
writing it twice. It adds **no reference of any kind**, and the boundary test stays
green.

**The checker that judges it is in the test project and does not call it**
(`tests/Ft8Sharp.Tests/Ldpc/LdpcCheck.cs`). That separation is the whole value of the
exercise. A checker sharing code with the encoder would be agreeing with itself and
would say nothing about whether the two upstream descriptions match. `LdpcCheck` reads
`LdpcNm`, `LdpcNumRows` and `LdpcMn`, and nothing else.

### Provenance of the encoder

**Ported from `ft8/encode.c`, function `encode174`**, at the pinned commit. It was
read by a test process — the same sanctioned route as everything else here, the
agent's own tools being refused that path — and the port follows what that function
does.

**The generator-matrix multiply and nothing around it.** Upstream's entry point
`ft8_encode` also calls `ftx_add_crc` and maps the codeword onto tones through the
Costas pattern and the Gray map. **None of that is here.** The CRC belongs to message
packing and the tone mapping to the modulator, both later steps, and pulling them
forward would have built protocol nobody had verified yet. `LdpcEncoder` computes
parity and stops.

`UpstreamEncoderProvenanceTests` asserts that `ft8/encode.c` is present at the pin, so
this paragraph is checkable rather than merely written down. It skips when the clone
is absent, like everything else that needs reference material.

### Four things measured, not assumed

Upstream states three of these in comments. A comment is somebody's recollection of a
file they were editing; each of these was established instead by **trying the other
reading and watching the reference parity tables refuse it**, in `Ft8LdpcLayoutTests`.

| Question | Answer | How it was established |
|---|---|---|
| Bit order within a generator byte | **Most significant first** | 0 failing checks over the 91 basis payloads, against **533** for least-significant-first |
| The five spare bits past the 91st | **Zero in every row** | 0 of 83 rows carries a set bit past bit 91 |
| Codeword layout | **Message first, parity appended** | 0 failing checks, against **3730** for parity-first |
| Index base of `Nm` and `Mn` | **Upstream's 1** | `Nm` spans 1..174, `Mn` spans 1..83 |

On the bit order: reversing the bits within every generator byte is exactly equivalent
to reading the unreversed table least-significant-bit-first against a
most-significant-first payload, so a reversed in-memory copy is the honest
counterfactual and no second encoder was needed to produce it.

On the spare bits: `LdpcKBytes` is 12, which is 96 bits for a 91-bit payload. Upstream
ANDs across all twelve bytes, so a non-zero spare bit would fold into the parity of any
payload that set the matching bit. They are all zero, which also confirms the row width
is being read right. `LdpcEncoder` goes one step further and **refuses** a payload with
a spare bit set rather than silently absorbing it, because the codeword that came back
would look perfectly well formed.

On the index base: **nothing is renumbered.** The one comes off in exactly one named
place, `LdpcCheck.Variable`, and nowhere else in the tree.

### The proof: 91 encodes cover the whole code space

```
91 payloads x 83 checks = 7553 syndrome bits, all zero
```

**Why 91 encodes are the whole of it and not a sample.** The code is linear over
GF(2). Every one of the 2⁹¹ payloads is a sum of the 91 weight-one payloads, and the
syndrome of a sum is the sum of the syndromes — so if each basis payload encodes to a
codeword with a zero syndrome, every codeword the generator can produce has one. That
is a proof of the whole space, not an agreement count over however many vectors
somebody had patience for.

Three corroborations, in `Ft8LdpcParityTests`, none of them needed for the proof and
each catching a bug the proof cannot see:

- **The all-zero payload encodes to all-zero parity.** Trivial, and it is what refuses
  a checker that returns zero for everything.
- **Every basis payload produces non-zero parity** — the lightest weight seen was 29 of
  83. An all-zero generator column would satisfy every syndrome check and be silently
  wrong: one payload bit carried by the code and protected by none of it.
- **8 fixed patterns and 500 seeded random payloads**, seed **20260831**, all satisfy
  every check. Linearity says these cannot fail if the basis passed; they are here to
  catch an encoder whose indexing depends on the payload, which is not a linear fault.

These tests read the checked-in tables and **never skip**. No clone, no `FT8_LIB_PATH`,
no reference material — what ships is asserted sound on a machine that has never seen
`ft8_lib`.

### Watched refusing

A syndrome check that would pass a corrupted table proves nothing about an
uncorrupted one. Three corruptions, in `Ft8LdpcRefusalTests`, **each on an in-memory
copy — `Ft8Tables.g.cs` was never touched, nothing was hand-edited and nothing was
regenerated**:

| Corruption | The proof's answer |
|---|---|
| One bit flipped in a copy of `LdpcGenerator`, row 40 | 1 of 91 payloads refused, 3 failing checks |
| One element altered in a copy of `LdpcNm`, check 17 | 2 of 91 payloads refused, both at check 17 |
| One bit flipped in a valid codeword, all 174 tried | exactly the checks `Mn` names, every time |

The second matters most: it is what shows the check side is really being consulted
rather than carried along beside a proof that only ever exercises the generator.

The third is a **third and independent corroboration of the `Nm`/`Mn` transpose**,
arrived at from the syndrome side — the failing count comes from `Nm`, the expected
count from `Mn`'s row for that variable, and they agreed on all 174 variables with no
exceptions.

The refusals are produced by `BasisProof`, the same routine that clears the real
tables, so what is quoted is the guard's own words rather than a test's account of what
it would have said. Its message names payload indices, check indices and counts, and
**no value**.

### Why there is no compiled C oracle

`ft8_lib`'s own encoder was **not** built and run as a reference, and this is
deliberate rather than a shortfall.

- **The permission scope is `dotnet build`, `dotnet test` and `dotnet restore`.** Unit
  201 measured that even `dotnet run` is outside it.
- **Nothing is on `PATH`** — no `gcc`, `cc`, `cl`, `make`, `cmake`. MSVC *is* installed
  on this machine (see *Can ft8_lib be built on this machine?* below), so the obstacle
  is the permission scope and `ft8_lib`'s GNU-flavoured `Makefile`, not the absence of
  a compiler.
- **It is not needed, and this is the part worth understanding rather than taking on
  trust.** An oracle would have given agreement on a finite list of vectors. The
  linearity argument gives all 2⁹¹ of them in 91 encodes. The proof above is strictly
  stronger than the one an oracle could have supplied, so building one would have
  bought nothing at the cost of a toolchain decision that is the owner's to make.

## Inherited bugs

**Inheriting Goba's bugs is accepted, deliberately, and recorded here rather than
discovered later.**

A port that tracks its source closely inherits that source's mistakes. The two
kinds are not equally dangerous:

- **A wrong table bit cannot hide.** LDPC parity fails immediately and loudly the
  first time the table is exercised against a reference codeword.
- **An algorithmic weakness can hide.** It reads as a decoder that simply does not
  do very well, and nothing in the build goes red.

**Step 6 of the phase is what would reveal an algorithmic weakness** — the decode
rate measured against the *published* threshold for FT8, rather than against
whatever `ft8_lib` happens to achieve on the same machine. Measuring against
`ft8_lib` would make an inherited weakness invisible by construction, because both
sides would have it.

## Reference recordings

**Reference WAVs are never committed.** They are roughly 21 MB of somebody else's
off-air recordings, and they do not enter a repository headed for publication.

Tests that need them read them from `C:\Source\ft8_lib` and **report skipped when
they are absent**, never failed. A fresh clone on a machine with no reference clone
stays green, and a skipped test says plainly what it could not find.

`[RequiresReferenceCloneFact]` is how that is done here — a `FactAttribute` that
sets `Skip` when the clone is not on the machine. The version of xunit this project
pins has no dynamic skip, and buying a package for one sentence was not worth it.
**Absent is a skip and present-but-unreadable is a failure**, because those are
different findings and `Directory.Exists` answers false to both. Point
`FT8_LIB_PATH` at somewhere that does not exist to watch the skip happen.

## Can ft8_lib be built on this machine?

**Not by its own build, as things stand — but there is a C compiler here.**
Measured 2026-08-31.

- **A C compiler is installed.** Microsoft's `cl.exe`, *Optimizing Compiler Version
  19.51.36256 for x64*, with `nmake.exe` and `link.exe` beside it, under Visual
  Studio Community 2026 at
  `C:\Program Files\Microsoft Visual Studio\18\Insiders\VC\Tools\MSVC\14.51.36231`.
  It runs — the banner above came from running it.
- **Nothing is on `PATH`.** `cc`, `gcc`, `clang`, `cl`, `cmake`, `make`, `ninja` and
  `nmake` are all absent from `PATH`, which is what unit 200 measured and reported
  as *no toolchain*. That reading was right about `PATH` and wrong about the
  machine: MSVC is there, one `vcvars64.bat` away.
- **`ft8_lib` ships a `Makefile` and no `CMakeLists.txt`** — 1543 bytes, measured by
  the probe as existence and size, contents unread. It expects GNU `make`, and
  there is **no `make` and no `gcc` or `clang` anywhere checked**: not on `PATH`,
  not under `C:\msys64`, `C:\MinGW`, `C:\cygwin64`, `C:\Program Files\LLVM`,
  Chocolatey, or Git for Windows' `mingw64`.

So the honest answer has two halves. **Its shipped build cannot run here**, for want
of `make` and a GNU-flavoured compiler. **Whether its sources compile under MSVC is
untested** — it is portable C, which makes it plausible, and nothing more than
plausible until somebody tries.

Its build was **not run**, and no artifact of it has been copied into this tree.

This matters to one thing only: the phase's *audio synthesis produces a signal the
reference decoder decodes* check, which needs `ft8_lib` built locally. That check
stays **nice-to-pass**. It is no longer blocked on *is there a compiler* — there is
— but on somebody deciding it is worth an hour, and the phase reaches its goal
without it either way.

## The message layer

**Ported 2026-09-01, unit 206.** Two files: `Message/Crc14.cs` and
`Message/Ft8Payload.cs`. Everything below came through the pinned clone read by a
checked-in test, which is the only route there is.

### What was ported, and what was deliberately not

**From `ft8/crc.c`, three functions and no more.** `ftx_compute_crc` became
`Crc14.Compute`. `ftx_add_crc` and `ftx_extract_crc` became `Ft8Payload.Create` and
`Ft8Payload.ExtractCrc` — they are not checksum arithmetic but facts about where the
checksum sits inside a payload, so they live with the container rather than with the
CRC.

**Nothing else in the message layer exists yet.** No packing or unpacking of any
message type, no callsigns, no grids, no reports, no free text, no telemetry, and no
callsign hashing. `Ft8Payload` never looks at what its 77 bits mean. The substrate
was taken on its own so that when a packer arrives, a round trip that fails is
unambiguously the packer's fault rather than the plumbing's.

**One deliberate departure, and it is a refusal rather than a different answer.**
Upstream's `ftx_add_crc` silently clears the three bits between the end of the
message and the start of the checksum. `Ft8Payload.Create` refuses a message that has
them set, for the same reason `LdpcEncoder` refuses a payload with its spare bits
set: a caller who put something there meant something by it, and quietly dropping it
yields a payload that looks perfectly well formed. No message this refuses would have
encoded differently — it would have encoded as though the bits had never been set.

**Three things were measured rather than assumed**, each of which changes the answer
if it is read the other way: that the checksum covers the message *zero-extended past
its own length* rather than the message; that the checksum sits at the top of the
payload most significant bit first, straddling three bytes; and that upstream's
accumulator is not masked inside its loop, so bits above the checksum width
accumulate in it and are shifted out — they can never reach the bit the division
tests, and the final mask discards them, but the port does its arithmetic at the same
width explicitly rather than relying on that.

### Tables go through the converter; scalars are asserted against the pin

**The two CRC scalars are hand-written constants in `Crc14`, with a checked-in test
asserting them against the pinned header at run time.** They are not routed through
the table converter. That distinction is deliberate and was ruled for unit 206.

The rule that tables are machine-converted exists so that the most licence-sensitive
artifacts are auditable and so a wrong value cannot hide. Regenerating
`Tables/Ft8Tables.g.cs` for two scalars would have put step 1's byte-for-byte
regeneration proof — the one load-bearing proof in this tree — at risk for no gain,
while an assertion delivers exactly what the ruling protects: the route stays
auditable, and a transcription that is wrong cannot survive the assertion. **A table
is converted. A scalar is asserted.**

**They are not in `ft8/crc.h`.** That header declares the three functions and nothing
else; both scalars are macros in `ft8/constants.h`, which `crc.c` includes. Measured,
not assumed.

### Three legs, and all three landed

The trap with a CRC is self-agreement: a checksum tested by another copy of itself
proves that the copy was faithful and nothing else, and a "known value" generated by
running the code under test is not a known value. So the port stands on three
arguments, each independent of the other two.

**Leg A — provenance.** A checked-in test reads the pinned header at run time and
asserts the library's two constants equal what the pin declares. It names the
constant it matched and never prints its value.

**Leg B — an independent checker.** `tests/Ft8Sharp.Tests/Message/CrcCheck.cs`
computes the same checksum without calling the library and without sharing its
arithmetic: a 256-entry table built from the polynomial consumes whole bytes forward,
giving the message multiplied by x^14 modulo the generator, and then the overshoot of
the final partial byte is divided back out through the inverse of x. The two do not
even run in the same direction. 504 messages at each of 20 bit lengths, seed
20260901, no disagreements. This is the discipline `LdpcCheck` applies to the encoder
one layer up.

**Leg C — linearity, and it held.** The register starts at zero and there is no final
XOR, only a mask, so the map is linear over GF(2) and `crc(a XOR b) == crc(a) XOR
crc(b)`. That shape was read off the ported function and then measured over 2000
seeded pairs rather than assumed. **Every 77-bit message is the XOR of some subset of
the 77 weight-one messages, so its checksum is the XOR of the corresponding subset of
the 77 basis checksums. The 77 basis computations therefore determine the checksum of
all 2^77 messages — the whole map, not a sample of it.** The argument's own
conclusion is then tested end to end: 20 000 random messages reconstructed from the
basis alone, all matching direct computation. This is the argument that let 91
encodes prove the entire LDPC code space in step 1, one layer down.

**Watched refusing.** A proof that has never rejected anything proves nothing. Every
one of the 77 single-bit changes to a message changes its checksum, across 20
messages; and every one of the 91 single-bit corruptions of a payload is refused by
the container, across 277 payloads. Nothing checked in is corrupted — every flip is
made on a copy.

### Does an external known-value vector exist in the clone? Yes, and it is stale

**Stated either way, because both answers are useful.** There is exactly one place in
the pinned clone that states an expected checksum for a stated input: a ten-byte
vector with a bit count and an expected value, in `test/test.c`. **It is inside a
block comment** — disabled code that no upstream build runs, so nothing has been
keeping it honest — and **it does not agree with the constants declared beside it.**

That disagreement was investigated rather than waved away. A checked-in test does a
bounded search over 1458 readings — the pinned polynomial and the pinned polynomial
with its leading term restored, every register width from 8 to 16, and every bit
count the vector could carry — and **reproduces the stated value under none of them.**
A port that had transposed a digit or mistaken the bit order would have turned up
somewhere in that space. What is recorded, therefore, is that the disabled vector
predates the constants beside it. The test asserts what is actually known: that the
two independent implementations agree on upstream's own input, and that the stated
value is unreachable. It does not assert a match it has not got, and it does not
assert a mismatch that a later correction would have to break.

**The end-to-end settlement arrives in step 3**, where the symbol sequence is
compared bit-identically against `ft8_lib`'s. Until then the CRC rests on the three
legs above, which is a stronger anchor than one vector would have been.

### What `Ft8Payload` does, and what it does not

It takes 77 message bits packed into 10 bytes, checksums them zero-extended, writes
the checksum after them, and produces the 12-byte payload `LdpcEncoder.Encode`
expects — with the five spare bits zero, so the encoder's own refusal is never
tripped. It reads one back, returning the message only when the checksum holds and
the spare bits are zero. **A payload that fails its checksum is never returned as
valid**, and the caller's buffer is left untouched when it fails, so a caller who
ignores the return value gets whatever it already had rather than an unchecked
message dressed as a checked one.

**It never throws for a correctly sized buffer, whatever is in it** — 100 000 random
12-byte buffers, 96 799 of them carrying spare bits that make them illegal payloads,
none producing an exception. A wrong-length buffer is a different thing: that is a
caller mistake rather than a bad signal, and it is refused loudly.

**Every payload the container produced cleared all 83 LDPC parity checks** through
the encoder step 1 proved and the independent checker that proved it, over a corpus
of 10 081 messages — the 77 weight-one, four fixed patterns and 10 000 seeded random.
That is the first time in this port that a message-shaped payload has gone through
the encoder.

**What it does not do is decode.** Nothing in this tree turns 77 bits into text, so
the question *does any random 77-bit pattern either decode or fail cleanly* is only
half answered: the half that says the layer underneath an unpacker will not be the
thing that throws.

### The alphabets are not tables, and unit 207 should know it

The six character alphabets FT8 packs against — the full set, alphanumeric with space
and slash, alphanumeric with space, alphanumeric, letters with space, and numeric —
**are not held as data upstream at all.** They are enumeration members in
`ft8/text.h`, and the mapping between a character and its index is computed by
arithmetic and branching in `ft8/text.c`. There is no string literal and no braced
array for the converter to lift; the only place the characters appear in order is a
trailing comment beside each enumerator, and reading a table out of somebody's
comment is worse provenance than transcribing one, not better. **So the converter is
not the route here, and there is no table for the tables rule to govern.** What comes
next is a port of two small functions.

Also measured, because the next unit will go looking for them: **`ft8/pack.c` and
`ft8/unpack.c` are not in the pin.** Packing and unpacking both live in
`ft8/message.c`.

### Two defects found in the test-side C reader on the way

Neither is upstream's and both are fixed, but they are worth recording because one of
them was silent for five units.

**`CSourceParser.ParseIntegerMacros` was dropping macros on CRLF lines.** Its regex
anchored on a plain end-of-line, which matches before a newline and not before a
carriage return, and **the pinned `constants.h` has mixed line endings** — most of it
one, part of it the other. Any `#define` on the wrong side of that boundary resolved
as *unresolved*, which the caller reports as a gap in corroboration rather than as a
contradiction. It surfaced because the two CRC scalars sit on opposite sides of it
and only one of them came back. Silently unresolved was the worst of the three
possible answers.

**`ExpressionEvaluator` had no cast form.** Upstream writes one of the CRC scalars as
a cast literal. The evaluator now applies a cast to the fixed-width integer types
rather than ignoring it, so a macro that truncates in C truncates here too.

---

## The message layer, part one — unit 207

Unit 206 built the substrate. This is the part that turns text into the 77 bits and the
77 bits back into text.

### What was ported

From **`ft8/message.c`** and **`ft8/text.c`** at the pin:

| Upstream | Here | What it is |
|---|---|---|
| `charn`, `nchar` | `Message/Ft8Text.cs` | The six alphabets, as branching arithmetic |
| `to_upper`, `is_digit`, `is_letter`, `is_space`, `in_range` | `Message/Ft8Text.cs` | The character predicates the callsign shapes test against |
| `int_to_dd`, `dd_to_int` | `Message/Ft8Text.cs` | Fixed-width signed decimals, which the report field is written in |
| `pack28`, `unpack28`, `pack_basecall`, `parse_cq_modifier` | `Message/Ft8CallsignField.cs` | The 28-bit callsign field and the suffix bit beside it |
| `packgrid`, `unpackgrid` | `Message/Ft8GridField.cs` | The 15-bit grid-and-report field and the `R` flag |
| `ftx_message_get_i3`, `ftx_message_get_n3`, `ftx_message_get_type` | `Message/Ft8MessageTypes.cs` | The two type selectors |
| `ftx_message_encode_std`, `ftx_message_decode_std` | `Message/Ft8StandardMessage.cs` | The standard message |
| `ftx_message_encode_free`, `ftx_message_decode_free` | `Message/Ft8FreeText.cs` | Thirteen characters as a base-42 number |
| `ftx_message_encode_telemetry`, `ftx_message_decode_telemetry` and its hex form | `Message/Ft8FreeText.cs` | The raw 71-bit body |
| the dispatch inside `ftx_message_decode` | `Message/Ft8MessageDecoder.cs` | One entry point, and a refusal that says why |

### What was deliberately not ported

**`save_callsign`, `lookup_callsign`, the 22, 12 and 10-bit hashes and the rolling cache
behind them.** They are the whole of unit 208 and they are the only stateful thing in this
layer. The plan calls hashing *the subtle part*, and its failure mode is the one nothing
else here has: a callsign that resolves to the **wrong text** rather than to no text.
Building it in the same night as the standard message would have given a failed round trip
two possible homes.

Also not ported: the non-standard-callsign message type, which cannot work without the
cache; the contest and DXpedition types, which the dispatcher refuses by name;
`ftx_message_encode`'s text tokeniser, because nothing in this library takes a whole
message as a string yet; and upstream's C string plumbing — trimming, token copying, the
message formatter — which a .NET caller has no use for.

### The seam at the hashed callsign, and why it refuses rather than guesses

The 28-bit callsign field has three sub-ranges. The middle one holds a 22-bit hash of a
callsign, and the text it stands for lives in a cache of calls heard earlier in the
session. **There is no cache here, so that sub-range is refused as unresolved and the whole
message with it.**

Upstream, in the same position with no cache attached, writes a literal placeholder into
the caller's buffer and returns success. **That is the divergence, and it is deliberate.**
`CLAUDE.md` §0.0 / HM-DEC-009 says never present a guess as a decode, and a placeholder
where a callsign should be is a message on the operator's screen that nobody sent. There is
no placeholder here, no partial message, and no numeric field returned as if it were a
call. The packing side of the same seam refuses too: a non-standard callsign would have to
be hashed and stored to be packed, so it is refused rather than written as a value nothing
could read back.

### The type cover, and what *unsupported* means here today

The two selectors give **15 combinations** and every one has a defined behaviour. **Four
are built and round-trip** — the standard message under both of its type codes, free text,
and telemetry. **Eleven are refused as unsupported by name.** No combination throws, and
none returns a decode for a type that is not built.

*Unsupported* here means the library will not tell you what those bits say. It is a correct
answer and it is not a failure: a receiver that guessed at a contest exchange it cannot
parse would be worse than one that says nothing.

### Round-trip proves consistency, not correctness

**A packer and an unpacker that agree with each other are inverses and nothing more.** A
field packed in the wrong order round-trips perfectly and is wholly wrong on the air.
Everything measured in unit 207 — a million callsigns, 200 000 standard messages, 100 000
free-text strings, 100 000 telemetry bodies, the whole of the 15-bit grid field — is
**internal self-consistency over a corpus**, which is what catches an ordinary porting slip
and is not evidence of agreement with upstream.

There were three honest settlements available and only two of them exist:

1. **An external message-level known value in the pin — there is not one that can serve.**
   See below.
2. **Machine-corroborated scalars.** Present, and listed below.
3. **Step 3's bit-identical symbol comparison against the reference implementation.**
   **That is where correctness gets settled**, and a systematically wrong alphabet or a
   swapped field would show up there for certain.

### Does the pin hold a message-level known value? Not a usable one

Asked for the first time in unit 207, by a checked-in inventory over every C source in the
clone.

**The live code holds none.** The clone's one test source drives upstream's own encoder
into upstream's own decoder and compares the text that comes out with the text that went
in. That is the same self-consistency this port measures, not a known value: it would pass
identically for an implementation that packed everything in a different order.

**One message-level vector does exist and it cannot be used.** Three message strings, each
paired with a stated symbol sequence, sit inside the same commented-out block that carries
the stale CRC value unit 206 reported. They are against the **superseded 72-bit packing**,
whose function is not in the pin any more, rather than the 77-bit message layer this port
builds. Disabled code, for a protocol generation this library does not implement.

**So: no.** Correctness is standing on the corroborated scalars and on step 3.

### Which scalars are corroborated against the pin by machine, and which are not

Read out of the pin at run time by
`tests/Ft8Sharp.Tests/Message/UpstreamMessageProvenanceTests.cs`, which reuses the existing
`TableGen` reader unchanged.

| Scalar | Corroborated | How |
|---|---|---|
| The hashed-callsign sub-range size | yes | integer macro in `message.c` |
| The start of the hashed sub-range | yes | integer macro in `message.c` |
| The grid and report boundary | yes | integer macro in `message.c` |
| The six alphabet lengths | yes, weakly | the comment beside each enumerator in `text.h`, not a macro |
| The two type selector widths | yes | the mask and shift shapes in `message.c` |
| The mapping from type code to message type | **no** | it is a `switch`, not a table |
| The token sub-range boundaries inside the field | **no** | they are literals inside a function body |
| The basecall positional alphabet sizes | **no** | they are literals in the multiply chain |

**Three of eight by macro, two more by a weaker mechanical read, three not corroborated at
all.** An honest count is worth more than a claim of full provenance.

### Divergences from upstream, all deliberate, all here

1. **A hashed callsign refuses instead of producing a placeholder.** Above.
2. **The grid value at the boundary is refused.** Both sub-ranges reach it: upstream's
   unpacker takes it as the last grid square and upstream's packer arrives at it from a
   report of thirty-five below zero. The bits are ambiguous and upstream presents one
   reading as certain.
3. **One grid square is refused because a token has taken its name.** Its four characters
   spell a sign-off, and the packer tests for tokens first, so the packer can never produce
   that value while the unpacker can. **Found by sweeping the whole field rather than
   predicted** — which is the argument for exhausting a field instead of sampling it.
4. **A report code whose number will not fit two digits is refused.** Upstream's
   fixed-width formatter emits a character that is not a digit for it; the text it makes is
   not a report and is not anything.
5. **A free-text body larger than thirteen characters of a 42-symbol alphabet is refused.**
   Rather more than half of all 71-bit bodies are such numbers. Upstream shows the low part
   of the number as though it were the message.
6. **The telemetry packer sets the type selectors.** Upstream's sets none — its own comment
   asks whether it should — so a message it produces declares itself free text. The bit the
   secondary selector needs is exactly the one upstream's left shift vacates.
7. **The character lookup refuses a negative index** where C would index off the front of a
   range. No caller here can produce one; what it buys is a total function, which is what
   lets the dispatcher promise never to throw.

### Upstream shapes inherited rather than repaired

Reported because they are on the air, and changing them would change the wire format.

- **The two prefix work-arounds collide.** A callsign spelled the way the Swaziland or
  Guinea work-around spells its own compressed form packs to the same integer as the call
  that work-around is for, and unpacks to that one. Measured at **4971 of a million**
  generated calls.
- **A lettered CQ modifier with a space in it does not round-trip.** The unpacker trims only
  the leading spaces off the four symbols; upstream's parser then stops at the space that is
  left. **56 186 of the 531 441** values in that sub-range.
- **The unpacker will produce an R-prefixed grid square and the packer has no route to
  one.** Upstream's own comment says so.
- **The enumeration declares a contest type its type function never returns.** That
  secondary code falls through to the unknown branch upstream, and it does here.
- **Free text loses leading and trailing spaces.** The encoder pads to the full width with
  spaces and the decoder trims them all off again, so the two are not distinguishable.
  Measured at **4200 of 100 000** generated strings.

---

## The callsign hash and the rolling cache — unit 208

### Where the hash lives, and where the cache does not

`ft8/message.c` declares no function whose name mentions hashing. All three widths are
computed in **one static function**, which packs the callsign against the alphabet the port
calls `AlphanumericSpaceSlash`, multiplies, and takes the top bits of a 64-bit product; the
narrow two are **truncations of the wide one** rather than separate functions, which is why
one stored value answers a lookup at any width.

**The cache is not in the library at all.** `ft8/message.h` declares a two-entry
function-pointer interface — one call to store a callsign under its hash, one to look a
callsign up by it — and `message.c` calls that interface without implementing it. The only
implementations in the pin are in its **demo decoder** and its **test harness**. So the
capacity, the probe stride, the duplicate check and the ageing pass are ported from an
application rather than from a library, and that is weaker provenance than anything else in
this port. It is also lower stakes: a different capacity cannot make this library deaf, and
a different multiplier would.

### Why the hash was held to a different standard of proof

Everything else this port has built is a private agreement between its own packer and its
own unpacker. **A hash is not.** It travels on the air, and it is only useful if it agrees
with what the transmitting station computed. **A hash that is wrong but self-consistent
round-trips perfectly through its own cache, passes every corpus that can be written against
it, and is silently and permanently deaf** — and the symptom is a quiet band rather than a
failing test.

So a round-trip corpus is **not evidence about the hash** and is not reported as any. What
the hash actually stands on is recorded in the section on its three legs below.

### Divergences from upstream, all deliberate, all here

Numbered on from unit 207's seven, which are unchanged.

8. **A cache miss refuses the whole message.** Upstream writes a literal `<...>` into the
   callsign field and returns the message with it in. That is a decode with a station's name
   missing from it, and the operator has no way to know which station. HM-DEC-009: a miss
   writes no text, at the cache, at the field, at the message and at the dispatcher.
9. **A cache collision refuses rather than answering.** **This is the divergence that
   mattered most tonight.** Two distinct callsigns can share a 22, 12 or 10-bit hash; a
   12-bit hash has only four thousand and ninety-six values. Upstream stores both and its
   lookup returns whichever its probe chain reaches first — **a real, plausible, entirely
   wrong callsign, presented with no mark of doubt on it**, which is precisely the one output
   HM-DEC-009 forbids. This lookup finds *every* stored call matching at the requested width
   and returns nothing where there is more than one. Refusing costs a decode upstream would
   have shown; answering costs the operator a logged contact with a station that was never on
   the air, and **a wrong callsign in a log is worse than a gap in one**. Note that a cache
   which knows *more* therefore sometimes produces *less* — that is the correct direction,
   because the extra knowledge is what reveals the answer was never certain. What no cache
   can know is a station it has never heard whose call collides with one it has, and nothing
   here pretends otherwise.
10. **The lookup examines every occupied slot rather than stopping at the first empty one.**
    Upstream's early stop is correct only while nothing has ever been removed, and its own
    ageing pass punches holes in the table. A hole can hide the second half of a colliding
    pair behind it, which turns a refusal back into a confident wrong answer. Scanning the
    whole table costs a walk of at most the capacity and makes the refusal above depend on
    what the cache holds rather than on the order it was filled in.
11. **The cache stores the callsign it was given rather than clipping it to eleven
    characters.** Upstream copies eleven characters into a fixed buffer, so two calls
    agreeing that far collapse into one entry spelled as neither of them and a lookup returns
    that. Storing what was actually heard turns the same case into an ambiguity, which
    refuses. **The hash itself still reads only eleven characters**, because that part is on
    the air.
12. **A full cache answers rather than spinning.** Upstream's insert walks the table looking
    for an empty slot with no bound at all and loops forever once every slot is taken. The
    walk here is bounded by the capacity and the caller is told. A call that could not be
    stored is simply one this cache has not heard, which the lookup already refuses
    correctly.

### Upstream shapes inherited rather than repaired

- **The hash reads eleven characters and stops.** Two callsigns agreeing in their first
  eleven have one hash between them, wherever they are heard. Repairing it would make this
  library disagree with every station transmitting, which is the one failure a hash cannot
  survive.
- **The hash is case-sensitive.** The alphabet it packs against holds upper-case letters and
  no lower-case ones, so a lower-case call has no hash and is refused rather than folded.
- **A space inside a callsign changes its hash.** The padding upstream applies is that same
  space, so a call with one written into it is a call one character longer. Every caller in
  this library trims before it gets here.
