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

# Work instruction 032 — pick the pitch that reads

**ISSUED: 2026-08-27. A fresh order, not an amendment.**

**Five tasks; task 5 is the drop. Task 1 decides whether tasks 2 to 4 are built
at all.**

## Why this unit exists

**The unit's number: six families, all of them clustering, all dead.**

The decoder works. Pointed at the right pitch it reads `DE KD0UN KD0UN K` at
84.2 %, fifty-nine characters at 32 words a minute with one unsure, and the ARRL
bulletins. **Every failure of the last two weeks is the survey choosing the wrong
pitch**, and every attempt to fix that has asked the same question: *is a station
keying in this bin?*

Six statistics have now been measured against that question:

| statistic | unit | result |
|---|---|---|
| cluster separation | 1.11.17 | station 1.75, silence 1.72 |
| dah/dit ratio | 1.11.17 | dominant refuser on one capture only |
| bin level spread | 1.11.18 | `N4L` reads at 10.4, silence sits at 12.0 |
| lift over the band floor | 1.11.18 | `N4L` reads at 3.0, silence sits at 35.3 |
| quantisation residual | 1.11.19 | every capture at or worse than random |
| agreement between fitted units | 1.11.21 | **inverted** — silence 0.028, `VA3VRR` 0.400 |

**`N4L` and `VA3VRR` are adjudicated callsigns that Hamlet reads, and an empty
band outscores them.** Six is not six unlucky choices. **Every one of the six is
a measurement of clustering**, and the question has no answer at bin level from
nineteen marks and three seconds.

**This project already named the way out, and its condition is now met.**
HM-DEC-125, 2026-08-18: *"Scoring candidates by their own speed estimator at the
tracked pitch is the direction if a measurement later shows a gap; **it is a
measurement of reading rather than of clustering**, and a fluke at 725 hertz
cannot fool it."* **The measurement showing the gap is the table above.**

**And HM-DEC-095 asks for exactly this.** *"A note is chosen by how it is keyed
and never by how loud it is."* **Ranking bins by how well each one decodes is
that rule taken literally** — judged wholly on keying, and the furthest thing
from loudness in the tree.

**So the question changes from a threshold to a ranking.** Not *is a station
here*, which nothing can answer, but *which of these bins reads best* — which
needs no threshold at all. **The refusal stays exactly where HM-DEC-120 put it**:
the decoder's own margin floor of 14, swept and measured, which already holds
both empty captures silent.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and
report any mismatch, including where the work succeeded anyway. Every unit since
1.11.17 disproved part of its own order's premise and was right to.

**HM-DEC-125 built parallel acquisition candidates once and they failed** — they
named 325, 550 and 725 hertz for a signal at 640, because they took the first
answer and were never subject to the two-agreeing-surveys rule. **Read that
ruling in `CLAUDE.md` before task 2.** What failed was taking a candidate's word
early. What is proposed here is scoring a candidate by what it reads.

**Expected state: 28 failing of 1845 in the engine as the stable set; 509 of 509
in the app.** Seven timing intermittents exist. Do not chase any; diff which
tests moved and never trust a total.

**The view-test rule is in force** (unit 1.11.27): a view-level test acts through
the control.

**`CLAUDE_CODE.md` is at version 1.6.** Read its own section count.

**`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141, 150.** HM-DEC-120,
125 and 127 are all inside that range; **their index rows in `CLAUDE.md` are what
this order quotes and all this unit has.**

## Rulings in force

**Tim's ruling, 2026-08-27, in his words:** *"If any of my rulings are keeping us
from doing something the right way, then I probably ruled in error. I want to get
CW working. I want the next unit to be a massive step forward."*

**Acquisition becomes a ranking rather than a classification.** Candidate pitches
are scored by **what the decoder reads at each of them**, and the best-reading
candidate is taken. **No bin is required to prove it holds a station before it is
decoded.**

**HM-DEC-120 is not traded and is not touched.** Its floor of 14 in the decoder's
own margin units is the refusal, and it does the refusing after the decode rather
than before it. **Both empty captures emit nothing, and that is this unit's first
acceptance line, not its last.**

**HM-DEC-095 is honoured rather than overruled.** A note chosen by what it reads
is a note chosen by how it is keyed. **Loudness may still not choose a note** —
if the shortlist is drawn by energy, that is a shortlist and not a choice, and
the report says how many candidates it carried.

**HM-DEC-127 is untouched.** A confirmed station is not abandoned for a candidate
far below it. **Ranking chooses among candidates at acquisition; it does not
displace a station being read.**

**Rejected already, do not revisit:** the six clustering statistics above;
taking a parallel candidate's first answer (HM-DEC-125's own measured failure);
locking to `CwPitch`; the four dead squelch axes; widening the 2.5–3.8 admission
band by moving its constants.

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is
moving. Same every ten minutes while a task runs. **This is the largest change to
acquisition in the project's history and the cadence is how Tim knows it is
moving rather than stuck.**

## The tasks

### Task 1 — can it be afforded? Measure before building.

**This task decides the unit.** Ranking means decoding at several pitches instead
of one, so the cost is the whole question.

Measure and report:

- **what one decode of a short window costs** at a single pitch — the window
  length the decoder already uses to reach a margin, and the wall time to run it;
- **how many candidate pitches the band would carry** at the survey's existing
  step, and how many a shortlist would carry at, say, four, eight and sixteen;
- **the total per acquisition pass**, against the cadence the survey runs at
  today.

**Then answer in one sentence: how many candidate pitches can be decode-scored
inside the survey's own cadence?**

- **If the answer is four or more**, tasks 2 to 4 proceed at that number.
- **If it is fewer than four, stop and report.** A ranking over two candidates is
  barely a ranking, and the honest answer is that this needs a cheaper scoring
  window or a slower cadence — **which is a ruling, not a session's choice.**

Build and run; record the baseline by diffing which tests fail.

### Task 2 — score candidates by what they read

Shortlist candidate pitches — **by energy is acceptable and is a shortlist, not a
choice** — then **decode a short window at each and score it by the decoder's own
margin**, the same quantity HM-DEC-120's floor is expressed in.

**Take the best-scoring candidate.** Nothing is required to pass a keying test
first.

**Then apply HM-DEC-120's floor to the winner.** Above it, that pitch is
tracked and the decode proceeds. Below it, **nothing is emitted and no pitch is
reported as measured** — the state unit 1.11.22 built for an unmeasured pitch.

**HM-DEC-125's failure is the thing to avoid here**: do not take a candidate's
first answer. The winner is the best over the scoring window, and the report says
whether one pass is enough or whether the existing confirmation applies to the
ranking's output as it does today.

### Task 3 — the corpus, because the instrument moved

Re-run every capture and report against unit 1.11.21's figures:

- **the four stations the operator can hear** — `cw-2026-08-25-012823` at 500,
  `cw-2026-08-22-014113` at 607, `cw-2026-08-22-014308` at 606,
  `cw-2026-08-26-125941` at 403.5 — **the pitch chosen for each, and the
  decode**, against floors of 41, 0, 0 and 0;
- **both silence controls: nothing emitted.** Absolute, and stated;
- **all twelve adjudicated anchors, character for character**;
- every floor held; chunk invariance intact.

**A capture now pointed at the right pitch that still reads nothing is a finding,
not a failure** — it means the fault has moved downstream, and it names where the
next unit goes. Say so for each.

### Task 4 — what it costs when it is wrong

Ranking has no refusal at the bin, so on an empty band it will pick **something**
and hand it to the gate.

**Measure what the gate then does**, on both silence controls and on the noise
capture `cw-2026-08-25-021825`: what pitch was chosen, what margin the winner
scored, and how far below the floor of 14 it sat. **Report the distance.** A floor
that holds by a wide margin and a floor that holds by a hair are different
answers, and Tim needs to know which he has.

### Task 5 — the operator's assertion, measured against it *(the drop candidate)*

Unit 1.11.21 gave the operator a way to assert a station and take the strongest
bin. On `014308` that read thirty-five characters where automatic acquisition read
none.

**Report what ranking chooses on those same four captures against what the
assertion chose.** If ranking matches or beats it, say so. If the assertion still
wins on any capture, **that is the most useful sentence in the report** — it says
the ear is still ahead and names where.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

Displacement (HM-DEC-127); the confirmation rule's consecutive-surveys
requirement; the joint cutter and its word gaps; the constrained margin; the
meter's rebuild; the integrator width; the whole-file second pass; the
short-character bias; `001520`'s quadrillions and `013347`'s 17.2 million; the
reference and port integrator difference. Also: **the entire screen** — the
scanner and calling cycle having no surface, the dead templates, the
recent-places row, the owned-property list, HM-DEC-086's record; `CHANGELOG.md`;
the seven intermittents; HM-OPEN-057; HM-OPEN-059.

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not build tasks 2 to 4 if task 1 says fewer than four candidates fit.**
  Report and stop.
- **Do not move HM-DEC-120's floor.** It is the refusal and it is measured.
- **Do not let loudness choose a note.** A shortlist is not a choice, and the
  report says how many it carried.
- **Do not touch displacement or confirmation.**
- **Do not trade the silence property**, which is task 3's first acceptance line.
- **Do not touch the screen.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 3 leads with the four captures the operator can hear: the pitch chosen
for each and what it read, against zero, zero, zero and forty-one.** **Section 2
says plainly whether a station he can hear now reaches the decoder without him
pressing anything.**

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Twenty-four inbound.
The oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150** — and
   HM-DEC-120, 125 and 127 are all inside it. **This unit acts on index rows
   alone.**
5. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
6. **The guard's gap is two to one**, calibrated on two empty captures.
7. **A boxcar's nulls made two of five swept offsets pathological best cases.**
8. **Two stations closer than 125 Hz are not named** — the operator's own item
   five, still not attempted.
9. **The keying meter** — its measurement found a station its verdict denied.
10. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
11. **The gate opens on everything, including two empty recordings** (1.11.18) —
    **this unit stops depending on it deciding.**
12. **The joint cutter cannot find word gaps on a compressed fist** (1.11.22) —
    the next decode question after this one, still unruled.
13. **The constrained margin is bounded and still does not separate** (1.11.22).
14. **Four fixtures are absent and five acceptance lines were unmeasurable**
    (1.11.22).
15. **HM-DEC-086's supersession needs a record** (1.11.25).
16. **The phrasebook's arrival and the absent-widget news are gone** (1.11.25).
17. **The recent-places row has no home** (1.11.26), three options costed.
18. **The owned-property list has no enforcement of staying current** (1.11.27).
19. **A test resolved an ambiguous control by accident** (1.11.27).
20. **Nothing checks that deleting a surface is not deleting a capability**
    (1.11.28) — measured on three instances.
21. **The scanner and the calling cycle are attached to the rig with no
    control** (1.11.28), and one of them transmits.
22. **Thirteen dead `DataTemplate` blocks nothing can distinguish from live
    ones** (1.11.28).
23. **Whether every constructed view model should be reachable from a binding**
    (1.11.28).
24. **`013347` returns a likelihood ratio of 17.2 million**, with `001520`'s
    quadrillions. Parked, raised once.

Still open: **the lock's mixed help**; **three fixtures at accepted cost**; **an
unmeasured pitch costs `N4L`**; **the six-hertz window disagreement**;
**`CHANGELOG.md` at 1.9.0 against 1.11.28**; **the squelch has no axis**; **the
three morning captures of 2026-08-26**.

**If you finish every task, stop and report. Do not start the next unit.**
