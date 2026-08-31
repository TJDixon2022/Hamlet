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

**Verification of the pin is outstanding.** The session that created this file
(work unit 200, 2026-08-31) could not read `C:\Source\ft8_lib` — the path is
outside the session's allowed working directory and every read of it was refused.
The HEAD of the local clone has therefore **not** been compared against the commit
above. That comparison is the first thing the table-converter unit must do, and
nothing may be ported until it has been done.

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

**Not yet measured.** The inventory of `ft8/constants.c` — every table, its C
identifier, its type, its dimensions and its element count, **names and shapes
only, never values** — belongs here and is empty because the session that would
have taken it could not read the clone (see *Provenance* above).

The converter unit fills this section as a by-product of doing its work.

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

## Can ft8_lib be built on this machine?

**Unknown, and here is why.**

- **No C toolchain is on `PATH`.** Measured 2026-08-31: `cc`, `gcc`, `clang`, `cl`,
  `cmake`, `make`, `ninja` and `nmake` are all absent from the session's `PATH`.
- **Whether one is installed off `PATH` could not be determined.** Checking the
  usual Visual Studio locations requires reading outside `C:\Source\HamLet`, and
  those reads were refused.
- **`ft8_lib`'s own build files could not be read** for the same reason, so nothing
  can be said about whether its `Makefile` or `CMakeLists.txt` would work here.

Its build was **not run**, and no artifact of it has been copied into this tree.

This matters to one thing only: the phase's *audio synthesis produces a signal the
reference decoder decodes* check, which needs `ft8_lib` built locally. That check
stays **nice-to-pass** until this question has a real answer.
