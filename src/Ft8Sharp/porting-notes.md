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
