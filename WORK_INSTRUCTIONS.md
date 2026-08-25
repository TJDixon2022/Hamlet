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

# Work instruction 012 — ready for tonight: adjudicate, align the two paths, fix the witness, shift the band row

**Tim operates tonight and air time for CW is rare. Six tasks; task 6 is the
drop.** Everything here either protects what works or fixes what he will see at
the radio.

## Why this unit exists

**The unit's number: seven captures of known text, adjudicated today.**

Tim ruled this morning — *"I think we can do them all. Why not? … Night's
coming. I wanna be prepared."* — approving the outstanding asks put to him,
including the W1AW adjudication. **The seven W1AW captures are adjudicated per
`W1AW-ARLP034-PROPOSED-TRUTH.md` as it stands in the tree.** That multiplies
the project's answer keys from three callsigns to three callsigns plus seven
lines of known machine-keyed text with numbers and punctuation, and it
unblocks two things by name: the suite's **first regression tests on
successes** (every existing ratchet guards a failure getting less bad — unit
1.11.8's own finding), and the **joint cutter**, whose options table named
this adjudication as its blocking dependency.

The rest of the batch: the keying witness is wrong on 13 of 23 recordings and
its repair is measured and withheld pending a ruling Tim has now given with a
condition; **two drive paths decode the same audio differently** — different
counts on nine captures, and on `032113` a different tracked note entirely,
650 Hz against 500 — which sits under every floor in the suite; and the band
display is cut off at the operator's own window size, which he will be
looking at tonight.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches,
including where the work succeeded anyway.

**Known state after unit 1.11.8: 29 failing of 1661 in the engine; 484 of 484
in the app.** Three engine failures are accepted cost (`clean-12wpm`,
`clean-18wpm`, `prosigns-18wpm` — exact digital silence, HM-OPEN-018); the
flaky rig test flakes. **`ARecordingWithNoStationInItSaysNothing(014854)` is
green and must stay green.**

**The thirteen captures of 2026-08-25 may or may not be in the tree.** Tim was
asked again to copy them to `tests/fixtures/cw/captured/unadjudicated/`. Task 1
checks; every dependent step names its fallback.

**`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141 or 150.**

**`CLAUDE_CODE.md` says four report sections; its version line reads 1.3.**
Read the file's own section count.

## Rulings in force

**Tim's adjudication, 2026-08-25:** the seven W1AW captures carry the text in
`W1AW-ARLP034-PROPOSED-TRUTH.md`. Only the quoted words are adjudicated; `…`
spans are not, and `—` marks a word cut by a capture edge. **The file's status
header changes from PROPOSAL to ADJUDICATED, citing this instruction and the
date. Section 4 asks Tim to enter it in the decision log with an id** — the
session does not mint ids.

**Tim's ruling on the band display, 2026-08-25, quoted:** *"the band's display
is cut off. It needs to be shifted down."* Task 2 implements it. **The
narrowest reading: the band row must be fully visible at the application's own
default and the operator's working window sizes; Tim's stated remedy is moving
the row down.** HM-DEC-141's wavelength-proportioned card widths are meaning,
not size, and are untouched.

**Tim's ruling on the keying witness, with the condition this instruction
attaches on his behalf and flags for veto:** the verdict moves onto the element
median — the repair measured at 17 right of 23 against 10 — **only alongside an
evidence-quantity requirement that keeps the silence property on the live
path.** HM-DEC-120 is the one thing this project has never traded, and "do them
all" is not read as trading it. If no requirement holds both, **the verdict
stays where it is and the measured overlap is the report's finding.**

**HM-DEC-120.** Nothing is emitted on audio holding no signal — and the meter
does not announce Keying on it either. Both empty captures checked and stated
at every task touching the signal path or the meter. `Gate` stays 1.40;
`CharacterMargin` stays 1.

**HM-DEC-126's own reopening condition is met** — it closed HM-OPEN-026 as
"unobtainable … reopens if the file appears" and `003758` has been in the tree
since 2026-08-20. Task 1 acts on the part that needs no new ruling: the
corroborated readings named in existing rulings become success tests.

**Rejected already, do not revisit:** the clock's span-restricted diet and the
fist-quality band (both built, measured, reverted in unit 1.11.8 — the
measurements stand in that report); locking to `CwPitch`; widening the guard;
tuning any threshold to green a test; gap-cluster tuning; treating duty as a
station test (a bulletin runs 47–70 %).

**PROPOSAL, not ruled:** everything else on the panel — the "Hold this pitch"
button, the sweep's visibility, lock-disagreement display. **Untouched.**

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what
is moving. Same every ten minutes while a task runs.

## The tasks

### Task 1 — harden the ground truth

1. **Flip the W1AW truth file to ADJUDICATED** per the ruling above.
2. **Check for the thirteen 2026-08-25 captures.** Present: commit, bank with
   floors, name `013520`, `013303`, `012823` as reference, beat-the-chain, and
   negative control. Absent: section 4 leads with it, fourth consecutive time.
3. **Build the suite's first success tests.** For each adjudicated reading —
   `VA3VRR` (HM-DEC-145), `N4L` (HM-DEC-144), `AA4MP/4 QNIK` (HM-DEC-126), the
   seven W1AW lines — a test asserting the adjudicated text is present in the
   decode, character for character, prosigns and numbers included. These are
   the first tests in the tree that fail when a *repair breaks a success*.
   Punctuation the decoder cannot yet produce is asserted as far as it reads
   today and the shortfall reported, not papered.

Build and run; record the green baseline.

### Task 2 — the band display *(Tim's ruling above)*

Implement the shift. Verify by hit-testing every band card at the default
width, the operator's working width, and one narrower — all seven cards answer
their own centre, and nothing newly occludes the readout. **HM-OPEN-060 closes
if the narrow-window occlusion is gone; say either way.**

### Task 3 — the witness's verdict, under the condition

Move the verdict onto the element median **with an evidence-quantity
requirement**: a live window asserts Keying only when it holds enough
element-length evidence for the median to mean something — the un-attempted
shape unit 1.11.8 named (window-length or run-count floor, to be measured, not
guessed).

- Sweep the requirement against: the 17-of-23 gain (how much survives) and the
  live-path silence on `014854`/`014935` sliced into six-second windows (must
  be **absolute** — zero windows asserting Keying).
- **If no requirement keeps both, the verdict stays on the old figure**, the
  overlap is reported with numbers, and that is the task's answer.

### Task 4 — one decoder, not two

Diagnose why `Process` (hop by hop) and `Listen` (buffered) disagree — nine
captures' counts, and `032113` tracking 650 Hz against 500. Find the divergence
point with file and line; fix it if the fix is contained; **if the fix is
larger than this task, report the mechanism and stop** — a named mechanism
beats a rushed change. Acceptance when fixed: both paths return identical
text and identical tracked pitch on all twenty-three captures, floors intact,
success tests green.

### Task 5 — the joint cutter, now that it has a judge

Option A from unit 1.11.8's table, buildable now the W1AW seven are
adjudicated: fold **character validity** into the path score over a short
window against the fitted clock — a segmentation spelling letters the alphabet
knows outscores one that does not.

**The two named threats are the acceptance tests, not afterthoughts:**

- **Prosigns and callsigns must survive.** `AA4MP/4 QNIK`, `<BT>`, `/`, and
  every digit in the W1AW lines — the validity weight is chosen at the largest
  value that leaves all success tests green, and if that value is nought,
  **ship nothing and report that the term cannot be safely weighted yet.**
- **Cut errors measured before and after** on the adjudicated corpus: the
  bulletin's known spacing is the judge. Improvement is claimed only against
  adjudicated text.
- §0.0's failure mode is named in code: the term biases *segmentation*, never
  substitutes a plausible letter for a measured one — `Lookup` misses still
  print `#`/`■`, never a nearest neighbour.

### Task 6 — floors for the thirteen *(the drop candidate)*

If task 1 found the 2026-08-25 captures: full harness treatment — floors,
reference cases, and the two reverted fixes' target measurements re-run
against them (the clock's 32-on-22.5 and 10-on-17.9, the fist band's `012823`)
so the next attempt at either starts from evidence. **If the captures are
absent, this task is void and says so. Dropped whole either way if time runs
out.**

## Parked — do not touch, do not raise

- **The clock's diet and the fist band** — reverted with measurements; next
  attempt waits for the thirteen.
- **`competing`** — diagnosed (the survey almost never holds two candidates);
  its fix touches the unruled 125 Hz ask.
- **The panel beyond task 2.** **The guard, the margin, the integrator width,
  `001520`'s quadrillions, the reference/port integrator difference,
  `014113`/`014308`'s smear, the six-hertz window disagreement, the
  unmeasured-pitch-costs-`N4L` ruling, HM-OPEN-057, HM-OPEN-059,
  `CHANGELOG.md` at 1.9.0, six survey candidates admitted below the
  neighbouring band.**

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not let any success test written in task 1 go red in tasks 3–5.** They
  exist to make that failure loud.
- **Do not trade the silence property in any form** — emission or meter
  verdict. Absolute, both empty captures, live path included.
- **Do not let the validity term substitute letters.** Segmentation only.
- **Do not touch the panel beyond task 2's ruling.**
- **Do not lower a floor.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 3 leads with the success tests: how many adjudicated readings are now
guarded, and whether every one survived tasks 3–5 untouched.** Section 2 says
plainly what Tim will see at the radio tonight — the band row where he can
reach it, the meter's verdict where it is honest, and what the cutter does to
spacing on a real fist.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Thirteen inbound;
the adjudication and the witness verdict are acted on this unit under Tim's
rulings above; the oldest of the rest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes
   `PHASE` match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150** — and
   section 4 now also asks Tim to enter today's two rulings and the
   adjudication with ids.
5. **The tone tracker** — narrowed by the hold, not closed.
6. **The integrator width** — bears on `014113`/`014308`.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best
   cases.**
9. **Two stations closer than 125 Hz are not named** — and `competing`'s
   diagnosis shows the 125 Hz floor is not what blocks it.
10. **The keying witness** — task 3 acts under the conditioned ruling.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

Still open: **the lock's mixed help with nothing telling the operator which**;
**the "Hold this pitch" button**; **three fixtures at accepted cost**;
**`001520`'s quadrillions**; **the reference/port integrator difference**;
**`CLAUDE_CODE.md`'s version line**; **an unmeasured pitch costs `N4L`**;
**`014113`/`014308`'s second mechanism**; **the six-hertz window
disagreement**; **HM-OPEN-060** (task 2 may close it); **the short-character
bias needs a per-character expectation**; **the two drive paths** (task 4 acts
on it); **HM-DEC-126's reopening** (task 1 acts on the ruling-free part).

**If you finish every task, stop and report. Do not start the next unit.**
