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

# Work instruction 033 — find the station, and stop printing noise

**ISSUED: 2026-08-27. A fresh order, not an amendment.**

**Nine tasks; task 9 is the drop. This is the largest unit this project has
run and every task serves one goal: the operator hears CW, Hamlet decodes it,
and an empty band stays empty.**

## Why this unit exists

**The unit's number: 4 of 4 against 0 of 8.**

Unit 1.11.29 measured every candidate for choosing a pitch against the four
captures the operator can hear and cannot read:

| how the pitch is chosen | right on how many of four |
|---|---|
| cluster separation, dah/dit ratio, level spread, lift over the band floor, quantisation residual, agreement between fitted units, the decoder's window ratio, the per-character span margin | **0 of 4, all eight** |
| **the strongest bin** | **4 of 4** |

On `cw-2026-08-25-012823` the strongest bin reads **`O BET TER ON N`** — English,
from a capture that has never read anything.

**And the second finding is why the screen fills with junk.** Unit 1.11.29 ran
its ranking over the whole bank on `cw-2026-08-20-014854`, a recording holding
nothing, and got **93 characters at a window ratio of 4.47** against a gate of
1.40. That same file is recorded in the gate's own documentation at **0.840** —
correct, and **measured at one pitch**. *Somewhere in six hundred hertz of noise
there is always a pitch that reads.* **A floor calibrated for a single look does
not transfer to a maximum over a bank**, and Tim is watching an empty frequency
fill with `E space E space I` right now.

**Two rulings of Tim's, both given 2026-08-27, are what this unit builds.**

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and
report any mismatch, including where the work succeeded anyway. Every unit since
1.11.17 disproved part of its own order's premise and was right to.

**The engine suite's failing-set diff has not been measured for two units.**
Unit 1.11.29 reports an expectation of 28 of 1852 and says plainly it is an
expectation. **Task 1 measures it before anything else moves.**

**`CwPitchRanking` is in the tree, tested, and called by nothing** — deliberately
disconnected by unit 1.11.29 with its reason in its own documentation. **Task 3
decides its fate; do not delete it before then.**

**The view-test rule is in force** (unit 1.11.27): a view-level test acts through
the control.

**`CLAUDE_CODE.md` is at version 1.6 with twelve sections.** Read its own
section count.

**`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141, 150.** HM-DEC-095,
120, 125 and 127 are all inside it. **This unit acts on index rows and on Tim's
rulings below.**

## Rulings in force

**Tim's ruling, 2026-08-27 — HM-DEC-095 is amended.**

> **The strongest bin may choose the note at acquisition. Keying structure is
> demoted from the chooser to a check on the winner.**
>
> Eight statistics have been measured against choosing a pitch by how it is
> keyed and all eight are wrong on the four captures that matter, while the
> strongest bin is right on all four. **HM-DEC-095's evidence was one recording
> where the answer was neither the loudest thing nor the configured pitch, with
> the operator's own transmission in the audio.** That is a reason to exclude
> his own transmission, and to distrust loudness **when a keying statistic
> disagrees** — it is not evidence that loudness is wrong when nothing
> disagrees with it.
>
> **What survives untouched from HM-DEC-095:** the operator's own transmission
> is not evidence about anybody else, and a sender's gaps are classified by
> clustering that sender's own gaps.

**Tim's ruling, same date — acquisition gets its own floor.**

> **The emission floor and the acquisition floor are separate, separately
> measured numbers.** HM-DEC-120's 1.40 is sound for one decode at a tracked
> pitch and says nothing about the best of twenty-five bins. Any scheme that
> searches takes a maximum, and a maximum needs its own calibration.
>
> **HM-DEC-120's property is not traded:** nothing is emitted on audio holding
> no signal. What changes is that a second number now protects it at
> acquisition, measured against **every capture in the tree holding no
> adjudicated station** rather than the two of 2026-08-20 that everything since
> has rested on.

**HM-DEC-127 is untouched.** A confirmed station is not abandoned for a
candidate far below it.

**Rejected already, do not revisit:** a ninth keying statistic; wiring
`CwPitchRanking` as the chooser; re-calibrating the existing floor to permit a
scheme right on none of the four; locking to `CwPitch`; the four dead squelch
axes.

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is
moving. Same every ten minutes while a task runs. **Nine tasks and the owner is
away; the cadence is the only thing telling him this is moving.**

## The tasks

### Task 1 — the baseline, actually measured

**Run the full engine suite to completion and diff the failing set by name.**
Two units have reported an expectation instead. Report the count and the names,
and **if any test outside the known 28 and the seven intermittents is red, stop
and report it** — this unit changes acquisition and must not begin on an unknown
tree.

Also record, for every capture in the tree: which hold an adjudicated station,
which hold an unadjudicated station, and **which hold nothing**. That third list
is task 5's corpus and it has never been written down.

### Task 2 — acquisition chooses the strongest bin

Implement the first ruling. **The strongest bin in the band chooses the pitch at
acquisition.** Keying structure no longer gates the choice.

**What must not change:** displacement (HM-DEC-127); the operator's own
transmission is still excluded; a sender's gaps are still clustered from that
sender's own gaps.

**Acceptance, on the four captures the operator can hear:** the pitch chosen is
**within 25 Hz** of 500, 607, 606 and 403.5 respectively. That is the number
unit 1.11.29 measured the strongest bin achieving; anything worse is a
regression against a measurement already in hand.

### Task 3 — keying structure becomes a check on the winner

The demoted half of the ruling. **After the strongest bin is chosen, keying
structure may refuse it** — the winner is checked, not the field.

Use what is already built: `CwPitchRanking` is tested and disconnected, and
**its scoring is the natural check.** If it serves, wire it here and say so; if
it does not, **say why and leave it disconnected**, and report whether it should
be deleted.

**Acceptance:** the four captures still pass task 2's 25 Hz test — **a check
that refuses a station the operator can hear has failed**, and the report names
which and by how much.

### Task 4 — the channel opens once and stays open

**This is the junk on the screen.** Hamlet decides afresh every window with no
memory: one threshold sampled twice a second on an empty band eventually comes
up heads, and the operator gets an `E`. Working decoders squelch a **channel**,
not a window.

Implement a two-threshold hold:

- a **high** bar to open a channel on a pitch;
- a **lower** bar to keep it open;
- a **timeout of continuous silence** before it closes, rather than an instant
  close.

Both bars and the timeout are **measured in task 5, not chosen here.** Build the
mechanism with provisional numbers and mark them provisional in the code.

**The reference for this shape is external and worth naming**: `cw-dit` opens a
decode channel when a station keys up and closes it after a timeout of silence,
and the operator's own radio squelch works the same way.

### Task 5 — the two floors, measured against every empty capture

**The acquisition floor, per the second ruling.** Measured over **the whole
list task 1 built of captures holding nothing** — not the two of 2026-08-20.

Report, for every such capture: **the best window ratio over the whole bank**,
which is the statistic acquisition actually faces. Unit 1.11.29 measured 4.47
and 2.41 on the two known ones; the rest have never been looked at this way.

Then set:

- **the acquisition floor**, above every empty capture's best-of-bank;
- **task 4's two bars and its timeout**, from the same distributions.

**Report the gap at both ends.** If no acquisition floor sits above every empty
capture and below the four the operator can hear, **say so plainly and ship
nothing from tasks 4 and 5** — that is the finding, and it means the strongest
bin needs the check of task 3 to carry the whole weight.

**The emission floor of 1.40 is not moved.**

### Task 6 — the corpus, because acquisition moved

Re-run everything and report against unit 1.11.29's figures:

- **the four captures**: pitch chosen, and the decode, against floors of 41, 0,
  0 and 0;
- **every capture holding nothing: zero characters.** Absolute, stated per
  capture, and this is the unit's first acceptance line;
- **all twelve adjudicated anchors, character for character**;
- every floor held; chunk invariance intact.

**A capture now pointed at the right pitch that still reads nothing is a
finding, not a failure.** Say so for each, and name what it points at.

### Task 7 — what the operator will see on a dead frequency

**The specific thing Tim is watching right now.** Take the empty captures and
the noise the corpus holds, run them through the finished chain, and report
**how many characters reach the screen from audio holding no station** — before
this unit and after.

**The target is nought.** If it is not nought, report the number and what the
remaining characters were, because he will be looking at exactly this tonight.

### Task 8 — say what it did, in the record

The sidecar records the pitch and whether it was measured. **Add: how the pitch
was chosen** — strongest bin, held channel, or operator assertion — and, where a
channel is open, **how long it has been open**.

This is diagnosis for the next unit, not decoration. **No panel change**; the
capture sheet only.

### Task 9 — the operator's assertion against the new acquisition *(the drop)*

Unit 1.11.21 gave him a way to assert a station. Unit 1.11.29 measured it
winning 4 of 4 where every automatic scheme lost.

**Report what the new acquisition chooses on those four against what his
assertion chooses.** If they now agree, say so — that is the sentence that says
he no longer has to press anything. **If the assertion still wins anywhere, name
where**, because that is what the next unit is aimed at.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

The joint cutter and its word gaps; the constrained margin; the meter's rebuild;
the integrator width; the whole-file second pass; the short-character bias;
`001520`'s quadrillions and `013347`'s 17.2 million; the reference and port
integrator difference; confirmation's consecutive-surveys rule. Also: **the
entire screen** — the scanner and calling cycle having no surface, the dead
templates, the recent-places row, the owned-property list, HM-DEC-086's record;
`CHANGELOG.md`; the seven intermittents; HM-OPEN-057; HM-OPEN-059.

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not move the emission floor of 1.40.** The acquisition floor is a second
  number.
- **Do not trade the silence property.** Every capture holding nothing emits
  nothing, and that is task 6's first line, not its last.
- **Do not choose task 5's numbers by hand.** They are measured from the
  distributions or nothing ships.
- **Do not let a keying check refuse a station the operator can hear.**
- **Do not touch displacement, confirmation, or the screen.**
- **Do not fit anything to the four captures.** They are the motivation; the
  anchors and the empty captures are the judge.

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason. **Nine tasks: if the suite must be
waited on, the report is still written before the session ends** —
`CLAUDE_CODE.md` §8 and the prompt both say there is no exit that leaves it
unwritten, and the last unit ended one task short of that.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 3 leads with two numbers: the pitch chosen on each of the four
captures against 500, 607, 606 and 403.5 — and the characters emitted from
audio holding no station, which should be nought.** **Section 2 says plainly
whether a station he can hear now reaches the decoder without him pressing
anything, and whether a dead frequency stays quiet.**

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Twenty-four inbound.
The oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150** — and
   HM-DEC-095, 120, 125 and 127 are all inside it. **This unit amends one of
   them from an index row alone.**
5. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
6. **A boxcar's nulls made two of five swept offsets pathological best cases.**
7. **Two stations closer than 125 Hz are not named** — the operator's own item
   five, still not attempted.
8. **The keying meter** — its measurement found a station its verdict denied.
9. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
10. **The joint cutter cannot find word gaps on a compressed fist** (1.11.22) —
    the next decode question after this one, still unruled.
11. **The constrained margin is bounded and still does not separate** (1.11.22).
12. **Four fixtures are absent and five acceptance lines were unmeasurable**
    (1.11.22).
13. **HM-DEC-086's supersession needs a record** (1.11.25).
14. **The phrasebook's arrival and the absent-widget news are gone** (1.11.25).
15. **The recent-places row has no home** (1.11.26), three options costed.
16. **The owned-property list has no enforcement of staying current** (1.11.27).
17. **A test resolved an ambiguous control by accident** (1.11.27).
18. **Nothing checks that deleting a surface is not deleting a capability**
    (1.11.28) — measured on three instances.
19. **The scanner and the calling cycle are attached to the rig with no
    control** (1.11.28), and one of them transmits.
20. **Thirteen dead `DataTemplate` blocks nothing can distinguish from live
    ones** (1.11.28).
21. **Whether every constructed view model should be reachable from a binding**
    (1.11.28).
22. **`013347` returns a likelihood ratio of 17.2 million**, with `001520`'s
    quadrillions.
23. **`CwPitchRanking` is called by nothing** — task 3 decides it.
24. **A session ended without writing its report while waiting on a suite**
    (1.11.29), the second time. The prompt's exit rule did not hold when the
    session read itself as still waiting rather than stopping.

Still open: **the lock's mixed help**; **three fixtures at accepted cost**; **an
unmeasured pitch costs `N4L`**; **the six-hertz window disagreement**;
**`CHANGELOG.md` at 1.9.0 against 1.11.29**; **the three morning captures of
2026-08-26**.

**If you finish every task, stop and report. Do not start the next unit.**
