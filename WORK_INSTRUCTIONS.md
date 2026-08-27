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

# Work instruction 034 — the station is refused at the right pitch

**ISSUED: 2026-08-27. A fresh order, not an amendment.**

**Eight tasks; task 8 is the drop. Every task is completable with what is in the
tree** — that is the design constraint, because unit 1.11.30 stopped at task 2
of 9 when its order turned out to target a fault the repository does not hold.

## Why this unit exists

**The unit's number: 0.74 against a floor of 1.40, at the right pitch.**

Unit 1.11.30 measured what the running decoder actually points at:

| capture | station | pointed at | error | emits |
|---|---|---|---|---|
| `cw-2026-08-22-014113` | 607 Hz | 600.0 | **−7 Hz** | **0 characters** |
| `cw-2026-08-26-125941` | 403.5 Hz | 400.0 | **−4 Hz** | **0 characters** |
| `cw-2026-08-22-014308` | 606 Hz | 575.0 | −31 Hz | **0 characters** |
| `cw-2026-08-25-012823` | 500 Hz | 450.0 | −50 Hz | 41 characters of junk |

**Three of the four are pointed at their own station and emit nothing anyway.**
Unit 1.11.29 measured those same three at window ratios of **0.44 to 0.90**
against the emission floor of 1.40. **The station scores worse than the noise
beside it, on the decoder's own measure of reading, while the filter is pointed
at it.**

**So acquisition was never what refused them.** Three units aimed at the survey
while the emission floor was quietly refusing correctly-pointed stations.

**And the operator's junk is not in this repository.** Both recordings holding
nothing emit **zero** characters through the real chain, before and after unit
1.11.30. The 93 characters that motivated the last order came from
`CwPitchRanking` sweeping the bank offline — a component built deliberately
disconnected, which the application never runs. **Task 5 makes the fault
capturable rather than guessing at it.**

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and
report any mismatch, including where the work succeeded anyway. **Three
consecutive units disproved part of their own order's premise and each was right
to.** If task 1 or task 2 contradicts this order's diagnosis, **say so and stop
rather than building on it** — that is what unit 1.11.30 did and it saved a
session.

**A completed engine run with a total is still owed.** Unit 1.11.30 reports the
failing set byte-identical by name at 28 of 1852, from two runs the environment
killed. **Task 1 takes a completed run.**

**`CAPTURE_INVENTORY.md` is in the tree** — 12 adjudicated, 19 unadjudicated,
2 holding nothing, 3 deliberately unclassified.

**`CwPitchRanking` is in the tree and called by nothing**, deliberately. Task 8
decides it.

**`CLAUDE_CODE.md` is at version 1.6 with twelve sections.** **`DECISIONS.md`
has no record for HM-DEC-096–133, 136, 141, 150.**

## Rulings in force

**Tim's ruling, 2026-08-27 — "do it all".** Three targets were put to him and he
took all three: the emission floor, the HM-DEC-127 interaction, and the junk he
is watching. **This unit does all three and adds the two things that unblock
future work.**

**Tim's ruling, same date — the HM-DEC-127 interaction.** *(Drafted from his
"do it all"; flagged for veto in the delivery.)*

> **At acquisition, a keying-confirmed candidate does not outrank the strongest
> bin when the two disagree and the confirmed one is not being read
> successfully.** HM-DEC-127 protects **a station already being read** from
> being abandoned for a candidate far below it. On `cw-2026-08-25-012823`
> keying confirms 450 Hz and emits 41 characters of junk while the station sits
> at 500 — **nothing is being read there, so there is nothing 127 protects.**
>
> **HM-DEC-127 is untouched where it applies:** a pitch that is producing a
> decode is not abandoned.

**HM-DEC-120's property is not traded.** Nothing is emitted on audio holding no
signal. **Both recordings holding nothing emit nothing, checked and stated at
every task that touches the signal path** — and they do today, so any change
that breaks it is this unit's doing and is reverted rather than explained.

**HM-DEC-095 as amended 2026-08-27:** the strongest bin may choose the note at
acquisition; keying structure is a check on the winner.

**Rejected already, do not revisit:** a ninth keying statistic; wiring
`CwPitchRanking` as the chooser; building a channel hold for a leak the tree does
not reproduce; widening the empty corpus by reading the decoder's own output,
which is circular.

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is
moving. Same every ten minutes while a task runs. **Eight tasks and the owner is
away.**

## The tasks

### Task 1 — a completed run, and the number

**Run the full engine suite to completion and report the total**, not a partial.
Two units have owed this. If the environment kills it again, **say what killed it
and report the failing set by name**, which is what unit 1.11.30 could give.

### Task 2 — why does a station at its own pitch score below the floor?

**Diagnose before changing anything.** For `cw-2026-08-22-014113` at 600 Hz and
`cw-2026-08-26-125941` at 400 Hz — both pointed within seven hertz and both
silent — report the window ratio **and what it is made of**:

- the ratio over the whole window, as the gate sees it;
- **the ratio over the keyed spans alone**, excluding the silence between
  transmissions;
- **the duty** — what fraction of the window the station is actually sending —
  and the same figures for `cw-2026-08-24-012403`, which scores 13.94 at its
  pitch and reads.

**The hypothesis to test, and it is a hypothesis:** the ratio is an average per
hop over the whole window, so a station sending for a fifth of it is diluted by
the four fifths of silence, while a station sending continuously is not.
**HM-DEC-090 already found and fixed exactly this shape** — the reported SNR and
the located pitch were averages over the silence in a recording, and both became
held peaks. **The emission gate was never given the same treatment.**

**Report whether the hypothesis holds.** If it does not, **say what does, and
stop** — tasks 3 and 4 are built on the answer.

### Task 3 — act on what task 2 found

**Only what task 2's measurement supports.** If dilution is the cause, the
candidate is a ratio taken over the keyed spans rather than pooled over the
window — **the held-peak treatment HM-DEC-090 applied everywhere else.**

**The floor of 1.40 is not moved.** What changes is the quantity it is applied
to, and **if that quantity changes, the floor must be re-derived in the new
units and the derivation reported** — a threshold whose scale moved underneath
it reads as a working gate while gating nothing, which this project has already
done once.

**Acceptance:**

- **the four captures the operator can hear emit something**, and the report says
  what and how much;
- **both recordings holding nothing still emit nought** — absolute;
- **all twelve adjudicated anchors green, character for character**;
- every floor held; chunk invariance intact.

**If no change satisfies all four lines, ship nothing and report the sweep**,
naming which line each candidate breaks.

### Task 4 — the confirmed pitch that reads nothing

Implement the HM-DEC-127 interaction per the ruling. **A keying-confirmed
candidate that is producing no decode does not hold the tracker against the
strongest bin.**

**Acceptance:** `cw-2026-08-25-012823` points at 500 rather than 450; **no
capture where the tracker is currently reading successfully changes its pitch**,
asserted corpus-wide; anchors green.

### Task 5 — make the junk capturable

**The operator is watching an empty frequency fill with characters and nothing in
the tree reproduces it.** This task removes the guesswork.

**When the decoder emits a character while the pitch's provenance is not
`Keying`** — the strongest bin, a bank centre, or unmeasured — **the sidecar
records it**: the provenance, the window ratio, the per-character margin, and
how long the channel has been producing.

**And a capture taken at that moment must carry enough to reproduce it offline**
— the audio already does; what is missing is the tracker's state. **Record the
tracker's state in the sidecar**: what it has confirmed, when, and what it has
lost.

**No panel change.** The next time it happens, one press of "I hear a station"
gives the next unit everything it needs.

### Task 6 — candidates for an empty corpus

Unit 1.11.30 established that the empty list is two recordings and cannot be
widened by reading the decoder's own output, because that is circular.

**Report, for each of the nineteen unadjudicated recordings**, the evidence a
human would use to rule on it: keying swing in decibels, the independent keying
meter's verdict, the strongest bin's lift over the band floor, and what the
decoder reads there. **Rank them by how likely they are to hold nothing.**

**Adjudicate nothing.** This is a list for Tim to rule on, and it turns a
blocked ask into a five-minute decision.

### Task 7 — the corpus, because the signal path moved

Re-run everything and report against unit 1.11.30's figures: the four captures'
pitch and decode; **every recording holding nothing at nought characters**; all
twelve anchors character for character; every floor; chunk invariance.

**A capture now emitting where it did not is the unit's whole point. A capture
now emitting where it should not is a failure and is reverted.**

### Task 8 — `CwPitchRanking`'s fate *(the drop candidate)*

It is in the tree, tested, and called by nothing. Unit 1.11.29 disconnected it
deliberately; unit 1.11.30's task 3 was to decide it and was never reached.

**Report whether task 3's check-on-the-winner has a use for it.** If it does,
say what wiring it would take. If it does not, **say so and recommend deletion**
— but do not delete it; that is one line and it is Tim's.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

The joint cutter and its word gaps; the constrained margin; the meter's rebuild;
the integrator width; the whole-file second pass; the short-character bias;
`001520`'s quadrillions and `013347`'s 17.2 million; the reference and port
integrator difference; the channel hold (there is no measured leak to build it
against); the acquisition floor (there is no corpus to measure it against —
task 6 builds the list for one). Also: **the entire screen**; `CHANGELOG.md`;
the seven intermittents; HM-OPEN-057; HM-OPEN-059.

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not build tasks 3 or 4 on a hypothesis task 2 did not confirm.**
- **Do not move the floor's value.** Task 3 may change what it measures, and
  then must re-derive it and report the derivation.
- **Do not trade the silence property.** Both empty recordings emit nought today
  and must at every task.
- **Do not adjudicate a recording as empty.** Task 6 lists; Tim rules.
- **Do not fit anything to the four captures.** The anchors and the empty
  recordings are the judge.
- **Do not touch the screen.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason. **The report is the only exit** —
`CLAUDE_CODE.md` §8 and the prompt both say so, and a session ended one task
short of it two units ago while waiting on a suite.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 3 leads with task 2's answer — what the window ratio is made of on a
station pointed at its own pitch — and then what the four captures emit after
task 3.** **Section 2 says plainly whether a station he can hear now reaches the
screen.**

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Twenty-five inbound.
The oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150** — and
   HM-DEC-090, 095, 120, 125 and 127 are all inside it. **This unit acts on
   index rows alone and amends one.**
5. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
6. **A boxcar's nulls made two of five swept offsets pathological best cases.**
7. **Two stations closer than 125 Hz are not named** — the operator's item five.
8. **The keying meter** — its measurement found a station its verdict denied.
9. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
10. **The joint cutter cannot find word gaps on a compressed fist** (1.11.22).
11. **The constrained margin is bounded and still does not separate** (1.11.22).
12. **Four fixtures are absent and five acceptance lines were unmeasurable**
    (1.11.22).
13. **HM-DEC-086's supersession needs a record** (1.11.25).
14. **The phrasebook's arrival and the absent-widget news are gone** (1.11.25).
15. **The recent-places row has no home** (1.11.26), three options costed.
16. **The owned-property list has no enforcement of staying current** (1.11.27).
17. **A test resolved an ambiguous control by accident** (1.11.27).
18. **Nothing checks that deleting a surface is not deleting a capability**
    (1.11.28).
19. **The scanner and the calling cycle are attached to the rig with no
    control** (1.11.28), and one of them transmits.
20. **Thirteen dead `DataTemplate` blocks nothing can distinguish from live
    ones** (1.11.28).
21. **Whether every constructed view model should be reachable from a binding**
    (1.11.28).
22. **No capture reproduces the junk the operator is watching** (1.11.30) —
    **task 5 makes the next occurrence reproducible.**
23. **The empty-capture corpus cannot honestly be widened** (1.11.30) — **task 6
    builds the list for Tim to rule on.**
24. **`CwPitchRanking` is called by nothing** — task 8.
25. **A completed engine run with a total is owed** (1.11.29, 1.11.30) — task 1.

Still open: **the lock's mixed help**; **three fixtures at accepted cost**; **an
unmeasured pitch costs `N4L`**; **the six-hertz window disagreement**;
**`CHANGELOG.md` at 1.9.0 against 1.11.30**; **the three morning captures of
2026-08-26**.

**If you finish every task, stop and report. Do not start the next unit.**
