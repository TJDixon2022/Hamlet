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

# Work instruction 020 — the survey never admits the station

**ISSUED: 2026-08-26. A fresh order, not an amendment.**

**Four tasks; task 4 is the drop.** The operator's standing goal governs this
unit and every part of it: **he hears CW, Hamlet must decode it.**

## Why this unit exists

**The unit's number: zero admissions in thirty seconds.**

Unit 1.11.16 logged `cw-2026-08-25-012823` survey by survey:

```
 3.03s  keyed  none  strongest   500  tracked 500
13.53s  keyed   450  strongest  none  tracked 500
14.03s  keyed  none  strongest   500  tracked 500
14.53s  keyed  none  strongest   500  tracked 450*
```

**500 Hz — the real station — is admitted as keying zero times**, while the
survey names it `Strongest` again and again. On `cw-2026-08-22-014113`, which
carries roughly 16 dB of keying an operator can hear, **no bin is admitted at
all, in any survey, in the whole recording.**

**This is the wall three consecutive units have hit from three directions.**
Unit 1.11.15 reached it through admission on `cw-2026-08-26-125941`; unit
1.11.16 reached it twice more, through the integrator and through confirmation.
Every mechanism built downstream — confirmation windows, the displacement
guard, the hold, filter width — is unreachable when nothing is ever nominated,
which is exactly why each one measured dead when it was built and correctly
shipped nothing.

**Nobody has measured why the survey refuses.** It applies several tests to
each candidate bin and reports one verdict. No instrument in this tree says
*which test refused which bin, and by how much*. Every fix attempted for four
days has been aimed downstream of a decision nobody can see. **Task 1 makes it
visible; the rest of the unit acts only on what it shows.**

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches,
including where the work succeeded anyway.

**A correction this unit must carry, because a shipped verdict may rest on it.**
Unit 1.11.15's order listed `cw-2026-08-22-014113` and `-014308` among "the four
empty captures" used as silence controls. **They are not empty** — unit 1.11.16
measured 16.7 and 15.7 dB of keying swing on them, and the tree's only silence
controls are `cw-2026-08-20-014854` and `-014935`. **If unit 1.11.15 judged its
admission valve unsafe because it admitted candidates on `014113` or `014308`,
that verdict was taken against recordings that hold stations, and task 3
re-measures it.** Report what 1.11.15 actually concluded and on what evidence.

**Expected state after unit 1.11.16: 28 failing of 1841 in the engine,
byte-identical set for five units; 503 of 503 in the app; twelve success tests
green; anchors, floors, silence on `014854`/`014935`, chunk invariance all as
before.** Confirm rather than assume.

**`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141, 150, nor Tim's
rulings of 2026-08-25/26.** **This unit works directly on HM-DEC-095's own
admission tests and cannot read its full record.** Its index-row transcription
is in unit 1.11.6's report: *a note is chosen by how it is keyed and never by
how loud it is … what separates them is whether the mark lengths are two
clusters or one smear.* **That principle is not being questioned. What is being
questioned is whether the current tests implement it.**

**`CLAUDE_CODE.md` says four report sections; its version line reads 1.3.**

## Rulings in force

**Tim's ruling, 2026-08-26, by adopting this unit (flagged for veto in the
delivery): the survey's refusals are instrumented before anything is changed,
and any change is made only to a test the instrument shows to be the one
refusing a real station.** No constant is touched on a hunch, and no test is
loosened that the measurement does not implicate.

**HM-DEC-095's principle stands and is not re-litigated.** Loudness may never
choose a note. A test that refuses a real station is a badly implemented
keying test, not permission to rank by energy.

**HM-DEC-120**: silence absolute on `cw-2026-08-20-014854` and `-014935`, every
task. **These two, and only these two, are the silence controls.**

**HM-DEC-127 and confirmation are untouched.** Both are downstream of admission
and both were measured inert by unit 1.11.16.

**Rejected already, do not revisit:** the integrator width as the cause of the
smear (swept 20–120 Hz, zero characters at every width, tracker bypassed —
1.11.16); the confirmation window (swept 2–8 surveys, moved 16–20 captures,
never confirmed 500 — 1.11.16); Q75/P97 as a readable-station test (`N4L`
reads at 0.238, below both unreadable captures — 1.11.16); the four dead
squelch axes; widening the 2.5–3.8 band by moving its constants; locking to
`CwPitch`.

**The integrator ships at 45 Hz, settled by measurement** — and unit 1.11.16's
caveat is carried forward rather than buried: that peak is sharp enough that
45 Hz may be the width this decoder was fitted around rather than the best
width available. **Not this unit's question. Do not touch it.**

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what
is moving. Same every ten minutes while a task runs.

## The tasks

### Task 1 — make the refusal visible

Instrument `CwToneSurvey` so that, for every survey pass, every candidate bin
it considers is recorded with **the value of each admission test and which test
refused it**. Not a verdict — the numbers, per bin, per pass.

Report it for: `cw-2026-08-25-012823` at 500 Hz (never admitted, repeatedly
`Strongest`), `cw-2026-08-22-014113` and `-014308` (nothing ever admitted),
`cw-2026-08-26-125941` at 403.5 Hz (the operator's live miss, dah/dit 3.82),
and — as controls — two anchored captures that admit normally.

**Then answer one question in one sentence: which test refuses each of these
stations, and by how much does it miss?** That sentence is this unit's whole
purpose; everything below depends on it.

Build and run; record the green baseline.

### Task 2 — fix the test the instrument implicates, and only that test

Act on task 1's answer. **The change must be to the specific test named, sized
to what the numbers show, and additive wherever it can be** — anything the
survey admits today is admitted after.

If the numbers show a threshold that a real station misses narrowly, report the
distance and propose the change; **if they show a test that is measuring the
wrong quantity altogether, say so and stop — that is a ruling, not a session's
change.**

**Acceptance:** the stations above are admitted at their measured pitches;
**no capture that admits a candidate today admits a different one**, asserted
corpus-wide by test; all twelve anchors green; every floor held; **silence
absolute on `014854` and `014935` — the two real controls.**

### Task 3 — re-measure unit 1.11.15's valve verdict on correct evidence

Per the correction above. If that unit rejected its admission valve on evidence
from `014113` or `014308` treated as silence, **re-run its measurement with
those two treated as what they are — recordings holding stations — and report
whether the verdict changes.**

If the valve was rejected on other grounds, say so and this task is one
paragraph.

### Task 4 — what the three captures then read *(the drop candidate)*

With whatever task 2 shipped, decode `012823`, `014113`, `014308` and `125941`
end to end and report characters, unsure, pitch and speed against their current
floors of nought, nought, nought and nought.

**A capture that is admitted and still reads nothing is a finding, not a
failure** — it means the fault is downstream and names where to look next.
Dropped whole if time runs out, and the report says so.

## Parked — do not touch, do not raise

The integrator width; confirmation; displacement; the hold and release-on-QSY;
fist-quality selection among admitted candidates; the meter's rebuild; the
squelch's successor; the whole-file second pass; `001520`'s quadrillions; the
reference/port difference; the short-character bias; the Avalonia offset;
`CHANGELOG.md`; the five intermittents; HM-OPEN-057; HM-OPEN-059; **the panel,
entirely.**

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not change any admission test the instrument does not implicate.**
- **Do not rank by loudness.** HM-DEC-095's principle is not the fault.
- **Do not treat `014113` or `014308` as silence controls.**
- **Do not touch anything downstream of admission** — it has been measured
  inert three times.
- **Floors only rise; anchors stay green; silence absolute on the two real
  controls; chunk invariance holds; no panel change.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 3 leads with task 1's sentence: which test refuses a real station,
and by how much.** Section 2 says plainly whether a station the operator can
hear now reaches the decoder at all.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Thirteen inbound.
The oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes
   `PHASE` match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor
   for Tim's rulings of 2026-08-25/26, including the one this unit acts
   under.**
5. **The tone tracker** — admission is this unit's; confirmation, displacement
   and selection are all measured inert until admission works.
6. **The integrator width** — settled at 45 Hz, with the caveat that the peak
   is sharp and may reflect what the decoder was fitted around.
7. **The guard's gap is two to one**, calibrated on two empty captures; the
   operator's noise session crossed it live on 2026-08-26.
8. **A boxcar's nulls made two of five swept offsets pathological best
   cases.**
9. **Two stations closer than 125 Hz are not named** — the operator's own
   item five, and the last of his list not yet attempted.
10. **The keying meter** — its measurement found a station its verdict denied.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

Standing: **the squelch has no axis — four measured dead**; **the three morning
captures of 2026-08-26, asked repeatedly**; **five intermittents**; **the speed
ceiling may be short for a 36–43 WPM station**; **`014113`/`014308` were
mislabelled as silence controls in a shipped order** (task 3).

**If you finish every task, stop and report. Do not start the next unit.**
