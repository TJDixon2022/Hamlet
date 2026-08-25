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
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.

---

# Work instruction 014 — ship the re-read, and stop leaving stations

**Tim operates in a couple of hours. Three tasks; task 3 is the drop. Tasks 1
and 2 are both expected to finish — this unit is sized small on purpose.**

## Why this unit exists

**The unit's number: 167 of 384, already measured, sitting reverted in the
tree.**

Unit 1.11.10 built the re-read-on-settle, measured it — adjudicated characters
158 to **167**, `cw-2026-08-18-003758` giving back **`AA4MP/4 QNIK` whole,
twelve of twelve, for the first time ever**, the ARRL bulletin 22 to 28 — and
reverted it because four character-count floors fell. Every falling floor fell
the same way: **fewer, better characters** — more elements seen, unsure counts
down. The floors predate the success tests and measure a quantity the project
no longer needs them to measure where an adjudicated anchor covers the same
recording.

**Tim has ruled** (2026-08-25, on being shown the numbers): correctness anchors
outrank count floors — a count floor retires wherever an adjudicated anchor
covers the recording — and **`cw-2026-08-25-012748`'s regression (sixteen
elements to four) is accepted and logged** rather than blocking the ship; it is
the drop-candidate task's subject, not a gate.

The second fault is live-operating poison and has a reproducible fixture as of
unit 1.11.10's task 1: on `cw-2026-08-25-012823` **the tracker holds the
correct 500 Hz for the first half of the recording, then leaves a confirmed
station for 450 and stays** — ending 49.8 Hz off a true 499.8, turning the
second half to soup. HM-DEC-127's index row (transcribed by unit 1.11.6) rules
*"a confirmed station is not abandoned for a candidate far below it."* This is
that ruling being violated by something upstream. **Fixing it enforces a ruling
already in force; it does not need a new one.**

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches,
including where the work succeeded anyway.

**Known state after unit 1.11.10: 29 failing of 1789 in the engine, app green
but for the known flaky tests.** Three accepted-cost silence fixtures; two flaky
rig tests; one unreproduced app intermittent. **Twelve success tests and
thirty-six floors; `ARecordingWithNoStationInItSaysNothing(014854)` green and
staying green.**

**The re-read exists complete in a reverted commit of unit 1.11.10's task 2** —
find it in history rather than rebuilding; its two recorded false starts (newest-
hop comparison; firing on first rather than confirmed pitch) are already
measured, do not retry them.

**`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141, 150, nor Tim's
rulings of 2026-08-25 — now five including today's two.** Section 4 carries the
ask again.

**`CLAUDE_CODE.md` says four report sections; version line reads 1.3.** Read
the file's own section count.

## Rulings in force

**Tim's ruling, 2026-08-25 (today): anchors outrank count floors.** Where an
adjudicated anchor covers a recording, its count floor retires — the anchor is
the guard. Count floors stand everywhere no anchor covers. **`012748`'s
regression under the re-read is accepted and recorded in the report, not
traded silently.**

**HM-DEC-127** (index row, per unit 1.11.6's transcription): a confirmed
station is not abandoned for a candidate far below it. **Task 2 enforces it.
The full record is unreadable in this tree — if anything found in code
contradicts the index row's plain sense, stop that task and put the conflict
in section 4 rather than guessing which way the full ruling cuts.**

**HM-DEC-120.** Nothing emitted on audio holding no signal; the meter claims no
Keying window on it — absolute, both empty captures, live path, checked and
stated at every task.

**Chunk-size invariance stands** (unit 1.11.9 task 4): identical text and pitch
at 240/480/960/1920/4800 and through both entry points, asserted after every
task here — the re-read's trigger is a function of hops, not chunk shape.

**Rejected already, do not revisit:** the four re-read trigger variants unit
1.11.10 measured; the validity term at any weight (both halves now measured to
a safe weight of nought); lowering the meter's swing bar; the clock diet; the
fist band; locking to `CwPitch`.

**PROPOSAL, not ruled:** the panel. **Untouched.**

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what
is moving. Same every ten minutes while a task runs.

## The tasks

### Task 1 — ship the re-read under the ruling

1. Re-apply unit 1.11.10's reverted re-read commit.
2. Retire the count floors on recordings an adjudicated anchor covers, citing
   the ruling in each retirement; **floors on unanchored recordings stand.**
3. Record `012748`'s regression in the report and in the floors file as
   accepted under today's ruling, with its numbers.
4. Re-verify the whole measured result: adjudicated characters **167 of 384 or
   better**, `AA4MP/4 QNIK` twelve of twelve, the bulletin at 28, `031948`
   unsure at nought, every success test green, silence absolute, chunk
   invariance across all five sizes and both entry points, empty captures with
   zero re-reads.

If any number lands short of unit 1.11.10's measurement, **stop and report the
difference** — the reverted commit was measured once and the tree has moved.

### Task 2 — a confirmed station is not abandoned

On `cw-2026-08-25-012823`, find what displaces the tracker from a confirmed
500 Hz at mid-recording, with file and line. Unit 1.11.6 transcribed the
displacement question as *never settled* for candidates against a station
already confirmed — the code has some rule there; name it, and fix the case the
index row plainly forbids: **whatever is at 450 on this recording is not better
evidence than the station being read, or the tracker would have been right to
go.** Expected shapes worth checking first: the station's own image or sideband
scored as a fresh candidate; a confirmation counter that decays during the
station's own inter-word silence; the hold (1.11.6 task 4) releasing on a
survey verdict the meter section shows is unreliable.

**Acceptance:** the tracker holds 500 Hz across `012823` end to end; its decode
improves and the improvement is reported (it is the negative-control capture —
improving it genuinely is the point; the harness still watches for trades
against `013520`); **no other capture's tracked pitch changes** — the fix is to
stop unjustified leaving, not to make leaving harder in general, and a capture
where the tracker *rightly* moves between two real stations must still move.
All floors and anchors green; silence absolute.

If the mechanism is found and the fix is not contained, **report the mechanism
with file and line and stop the task** — tonight a named fault beats a rushed
change.

### Task 3 — why the re-read destroys `012748` *(the drop candidate)*

Sixteen elements to four on the one capture the re-read hurts. Diagnose which
replay decision goes wrong — the pitch it re-mixes at, the moment it fires, or
what the second read does to marks the first read had. **Diagnose; fix only if
contained and only if every task-1 number holds.** Dropped whole if time runs
out, and the report says so.

## Parked — do not touch, do not raise

- **The meter** — measured into an overlap; needs a different quantity, not a
  bar; its own future unit.
- **The panel**, the "Hold this pitch" button, re-read indication on screen.
- **`032113`/`032012`/`032050`** — structurally beyond the live re-read;
  whole-file second pass is a different feature and unruled.
- **The integrator width, `014113`/`014308`'s smear, `001520`'s quadrillions,
  the reference/port integrator difference, the six-hertz window disagreement,
  the unmeasured-pitch-costs-`N4L` ruling, the short-character bias, the
  Avalonia geometry offset, `CHANGELOG.md`, HM-OPEN-057, HM-OPEN-059, the
  three flaky tests.**

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not retire a floor on any recording no anchor covers.**
- **Do not retry the four measured re-read triggers.**
- **Do not make the tracker sticky in general** — task 2 removes one
  unjustified move, with the not-changed-elsewhere assertion as proof.
- **Do not retract settled text, break chunk invariance, or trade silence.**
- **Do not touch the panel.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 3 leads with two numbers: adjudicated characters with the re-read
shipped (167 of 384 is the bar), and `012823`'s tracked pitch across the whole
recording.** Section 2 says plainly what Tim will see at the radio tonight:
openings repaired, and stations no longer walked away from mid-contact.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Thirteen inbound.
The oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes
   `PHASE` match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor
   for Tim's five rulings of 2026-08-25, two of which this unit acts under.**
5. **The tone tracker** — task 2 acts on its sharpest known fault.
6. **The integrator width** — bears on `014113`/`014308`.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best
   cases.**
9. **Two stations closer than 125 Hz are not named.**
10. **The keying meter** — calibration measured inside an overlap; needs a new
    quantity, its own unit.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

Still open: **the lock's mixed help**; **the "Hold this pitch" button**;
**three fixtures at accepted cost**; **`001520`'s quadrillions**; **the
reference/port integrator difference**; **`CLAUDE_CODE.md`'s version line**;
**an unmeasured pitch costs `N4L`**; **`014113`/`014308`'s second mechanism**;
**the six-hertz window disagreement**; **the short-character bias**; **the
Avalonia geometry offset**; **`CHANGELOG.md` at 1.9.0**; **three intermittent
tests**; **the whole-file second pass for late-pitch captures** (new — the
form of the lever the live re-read cannot reach).

**If you finish every task, stop and report. Do not start the next unit.**
