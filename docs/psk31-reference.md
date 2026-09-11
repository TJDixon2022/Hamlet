# The PSK31 reference, pinned

**Work instruction 312 task 3, under `PHASE_PLAN.md` §R5.** This note exists so that
every later claim in this phase can be checked against one fixed thing rather than
against whatever the reference happened to be that evening. It is the same rule the FT8
phase set for `ft8_lib`, written down before a line of it has been read.

## What was cloned, and where

| | |
| --- | --- |
| Project | `fldigi`, the reference PSK31 implementation |
| Origin | `https://github.com/w1hkj/fldigi.git` |
| Clone | `C:\Source\fldigi` — **outside this tree, and never committed to it** |
| Commit | `61b97f4133c488063f3de1795c894d22d5032e8a` |
| Commit date | 2022-06-23 08:19:27 -0500 |
| Commit subject | `Version 4.1.23` |
| Cloned | 2026-09-11 |
| Depth | `--depth 1`, so the clone carries this commit and no history before it |

**Read from the clone rather than recalled.** The commit is `git rev-parse HEAD` in the
clone; the date and subject are `git log -1`. Nothing in this table is from memory.

## The licence

**GNU General Public License, version 3, 29 June 2007**, read from the clone's own
`COPYING` at that commit, whose first lines are:

```
                    GNU GENERAL PUBLIC LICENSE
                       Version 3, 29 June 2007

 Copyright (C) 2007 Free Software Foundation, Inc. <http://fsf.org/>
```

Hamlet is GPL-3.0 (HM-DEC-004), so the licences are compatible. **That is a reason the
reference may be read, and it is not permission to copy it.**

## The rule

**It is read and it is never ported wholesale.** What Hamlet ships is written for
Hamlet. What may be taken from the reference is the shape of the problem: the
raised-cosine shaping, the demodulator structure, and the varicode table, which is a
published standard rather than fldigi's invention.

**Nothing under `src/` in this repository may name this clone or hold a path into it.**
A build that only works on one machine is not a build (§0), and
`ThePsk31ReferenceIsPinnedTests` fails if any source file mentions it.

**Nothing was read from it in unit 312.** Step 0 is the seam and reads no reference at
all; this note is here so that step 1 starts from a pinned commit rather than pinning
one after the fact.

## The varicode table, and where step 1 should cite it from

The PSK31 varicode is **Peter Martinez G3PLX's**, published with the mode itself in
1998, and it is a published standard rather than anybody's source code. Step 1 cites it
rather than transcribing it from memory.

**The two files in the pinned clone that carry it**, named here so the citation is to a
commit rather than to a moving branch:

| File | Holds |
| --- | --- |
| `src/psk/pskvaricode.cxx` | `varicodetab1`, 256 entries, the encoding table, one bit string per byte |
| `src/include/pskvaricode.h` | the two entry points, `psk_varicode_encode` and `psk_varicode_decode` |

That file's own header, read at this commit, records where fldigi got it:

```
// varicode.cxx  --  PSK31 Varicode
//
// Copyright (C) 2006
//		Dave Freese, W1HKJ
//
// This file is part of fldigi.  Adapted from code contained in gmfsk source code
// distribution.
```

**So the chain is Martinez to gmfsk to fldigi**, and the table itself is the standard at
the end of it. **Step 1 must state which of those it read**, and if it transcribes the
table it transcribes it from a source it names and checks, not from this note.

**One thing this note deliberately does not do**: give a URL for the original RadCom
article or for G3PLX's own page. Neither was opened while writing this, and a citation
nobody checked is the thing §4 of `CLAUDE.md` exists to prevent. The pinned files above
are what was actually read.

## A note on the clone itself

The checkout needed one exclusion. `flarq_doxygen/user_src_doc/aux/ARQ2.pdf` cannot be
written on Windows, because `aux` is a reserved device name, and a plain `git clone`
reports `error: invalid path` and leaves the working tree empty. The clone is therefore
sparse: `src`, `COPYING` and `README` are checked out and the doxygen trees are not.

**That costs nothing this phase needs** — the excluded path is generated documentation —
and it is recorded here because the next person to clone it will hit the same error and
should know it is expected rather than a corrupt download.
