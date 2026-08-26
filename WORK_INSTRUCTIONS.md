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

# Work instruction 019 — the audible station that reads nothing, and the station left mid-contact

**ISSUED: 2026-08-26. A fresh order, not an amendment. Written before unit
1.11.15's report existed, and aimed at the operator's standing goal rather than
at any session's leftovers: he hears CW, Hamlet must decode it.**

**Four tasks; task 4 is the drop.**

## Why this unit exists

**The unit's number: nineteen decibels of keying, nought characters.**

`cw-2026-08-22-014113` and `cw-2026-08-22-014308` carry a station at 607 Hz
with nineteen decibels of keying, and **three independent chains have failed on
them** — Hamlet, the shack-side analysis chain, and a third chain run from the
WAVs. Unit 1.11.6 measured why, at their exact pitch: the envelope has no two
states to find. On a recording that reads, the upper quartile sits near the 97th
percentile — key up, key down. On these, it sits at a third of it. **One smear
rather than two clusters**, which is the very thing HM-DEC-095 says separates a
station from everything else.

That unit also named the candidate cause and left it unruled since 2026-08-24:
**these recordings run 24 words a minute against `004507`'s 18, and the
integrator spans 33 ms — two thirds of a dit at that speed.** A filter that long
in time rounds the top of every short mark. If that is the cause, it is a
constant, and **every fast sender the operator meets is affected**, not two
files.

**The second half of the unit is the contact that starts well and rots.** On
`cw-2026-08-25-012823` the tracker reaches the correct 500 Hz at three seconds,
holds it for eleven, then **leaves a confirmed-looking station for a rival 4.7
decibels quieter** and stays there, ending 49.8 Hz off and turning the second
half of the recording to soup. Unit 1.11.11 diagnosed it to the line and
stopped, because the fix was not contained: the real station is admitted only
intermittently, confirmation needs **two consecutive** surveys agreeing within
25 Hz, so the alternation 500, 450, 500, 450 never confirms anything; the
tracker rides the correct pitch on the unconfirmed cold-start path with
HM-DEC-127's displacement guard **inert**, and the first thing to confirm is the
rival.

These are items three and four of the operator's own list of what stands between
his ear and his screen. Nothing else in this unit.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches,
including where the work succeeded anyway.

**This unit was written before unit 1.11.15 reported.** That unit was building
the admission valve and the release-on-QSY. **Read its report first**: if the
valve now admits `014113`/`014308` or changes `012823`'s acquisition, task 1
says so and the later tasks are re-aimed at what is left rather than at what
this instruction assumed. **Its measurements outrank this instruction's
premises.**

**Expected state if 1.11.15 landed clean: 28 failing of 1831 in the engine,
byte-identical set for four units; app green; twelve success tests green;
anchors, floors, silence, chunk invariance all as before.** Confirm rather than
assume.

**`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141, 150, nor Tim's
rulings of 2026-08-25/26.** HM-DEC-095 and 127's index rows are transcribed in
unit 1.11.6's report; **task 3 works directly beside 095's confirmation
constant and cannot read its full record.**

**`CLAUDE_CODE.md` says four report sections; its version line reads 1.3.**

## Rulings in force

**Tim's ruling, 2026-08-26, by adopting this unit (flagged for veto in the
delivery): the integrator's width is settled by measurement against speed.**
It has been a live question since unit 1.11.7 measured 45 Hz against 30 Hz and
handed the trade back. **The measurement now has a purpose it did not have
then** — two recordings with audible keying that no chain can read — so the
sweep is run against *sender speed* rather than against adjacent-station
rejection, and the width that reads the most correct characters across the
anchored corpus ships. **If no width recovers `014113`/`014308` without costing
an anchor, nothing ships and the smear is not ours** — that is the finding, and
it is worth as much as a fix.

**Tim's ruling, same date, same mechanism: a station may be confirmed on
non-consecutive evidence.** Confirmation requires two surveys agreeing within
`ConfirmWithinHz`, **within a short window rather than strictly back to back.**
HM-DEC-095 was written against noise producing one convincing fluke, and two
independent agreements still stand between a candidate and the tracker; what
changes is that a real station admitted intermittently is no longer barred from
ever confirming. **The window's length is measured in task 3, not chosen, and
if no window both confirms `012823`'s station and leaves every other capture's
acquisition untouched, nothing ships and the measurement is the answer.**

**HM-DEC-127 is untouched** — this unit does not change what displaces a
confirmed station; it makes confirmation reachable so the guard can arm at all.

**HM-DEC-120**: silence absolute on all four empty captures, every task.

**Rejected already, do not revisit:** the four dead squelch axes (duty, fist
ratio, `spanLlr`, `marginLlr` as a difference); widening the 2.5–3.8 admission
band (the valve is the sanctioned route); lowering the displacement guard's bar
(measured in 1.11.11 — does not fix `012823`, moves six other captures);
locking to `CwPitch`; headless-only panel verification.

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what
is moving. Same every ten minutes while a task runs.

## The tasks

### Task 1 — read the last report, then measure the two smeared captures

State in one paragraph what unit 1.11.15 changed and whether it moved
`014113`, `014308` or `012823`. **If it did, re-aim the tasks below at what
remains and say so plainly.**

Then, on `014113` and `014308` at their measured pitch, report the envelope's
own shape: upper quartile against the 97th percentile, the same figures unit
1.11.6 used, beside `004507`'s as the reading control. Confirm or refute the
one-smear finding **in the tree today**, after four units of front-end change
since it was taken.

Build and run; record the green baseline.

### Task 2 — the integrator against speed

Sweep the integrator width — at least 60, 45, 30 and 20 Hz, and **wider than 60
if the arithmetic says a 24 WPM dit needs it**; report the span of each in
milliseconds beside the dit length at 18, 24, 30 and 36 words a minute, so the
trade is legible rather than asserted.

Measure at each width: **`014113` and `014308` — characters emitted and the
envelope's two-state shape**; the twelve adjudicated anchors, character for
character; the sensitivity sweep; the empty captures.

**Ship the width that reads the most correct anchored characters, per the
ruling — and if the widest reading of `014113` costs a single anchor character,
ship nothing and report that the smear is not the filter's doing.** A negative
result here retires a question that has been open since 2026-08-24 and is
reported as an achievement, not a failure.

### Task 3 — confirmation that survives a dropout

Implement confirmation within a short window per the ruling. **Measure the
window rather than choosing it**: sweep it, and report for each length
`012823`'s tracked pitch across the whole recording, and **how many other
captures' acquisition changes — which must be none.**

Acceptance: `012823` confirms 500 Hz and holds it end to end; its decode
improves and the improvement is reported; **no other capture's tracked pitch
changes at any point**; every anchor green; every floor held; silence absolute.

**If no window achieves that, ship nothing and report the sweep.** Unit
1.11.11 left a lead worth testing here: a related loosening made
`cw-2026-08-22-031905` hold 500 Hz instead of wandering to 300, and `032113`
hold 500 instead of 650 — both W1AW captures whose documented carrier is 499.8.
**Report what this window does to those two either way.**

### Task 4 — the dimensionless ratio *(the drop candidate)*

If unit 1.11.15 dropped it: log `marginLlr / spanLlr` beside the two already
there and report its first distribution across the corpus, anchored against
everything else. **Measure and report only.** If 1.11.15 already did it, this
task is void and says so. Dropped whole if time runs out.

## Parked — do not touch, do not raise

The admission valve and release-on-QSY (unit 1.11.15's, whatever it did with
them); displacement (HM-DEC-127); fist-quality selection; the meter's rebuild;
the squelch's successor; the whole-file second pass; `001520`'s quadrillions;
the reference/port difference; the six-hertz window disagreement; the
short-character bias; the Avalonia offset; `CHANGELOG.md`; the five
intermittents; HM-OPEN-057; HM-OPEN-059; **the panel, entirely.**

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not ship an integrator width that costs an anchor character.**
- **Do not ship a confirmation window that moves any other capture's
  acquisition.**
- **Do not touch displacement, admission, or the panel.**
- **Do not chase the smear beyond the filter** — if width does not explain it,
  report and stop; the next step is a ruling, not a session's invention.
- **Floors only rise; anchors stay green; silence is absolute; chunk
  invariance holds.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 3 leads with two answers: whether `014113` and `014308` read, and
whether `012823` holds its station end to end.** Section 2 says plainly what
changes at the radio: fast senders that used to produce nothing, and contacts
that used to rot halfway through.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Thirteen inbound.
The oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes
   `PHASE` match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor
   for Tim's rulings of 2026-08-25/26, including the two this unit acts
   under.**
5. **The tone tracker** — admission and release were 1.11.15's; confirmation
   is task 3's; displacement and selection remain.
6. **The integrator width** — task 2 settles it or retires it.
7. **The guard's gap is two to one**, calibrated on two empty captures; the
   operator's own noise session crossed it live on 2026-08-26.
8. **A boxcar's nulls made two of five swept offsets pathological best
   cases.**
9. **Two stations closer than 125 Hz are not named** — the operator's own
   item five.
10. **The keying meter** — its measurement found a station its verdict denied.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

Standing: **the squelch has no axis — four measured dead**; **the three
morning captures of 2026-08-26, asked repeatedly**; **five intermittents**;
**the speed ceiling may be short for a 36–43 WPM station**.

**If you finish every task, stop and report. Do not start the next unit.**
