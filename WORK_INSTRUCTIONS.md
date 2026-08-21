# WORK_INSTRUCTIONS.md

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      Hamlet.sln
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwGate.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  src\CoreHMI

These four files are fixed. Do not substitute a different file for any of
them and do not report a check against a file this list does not name.

**`CwGate.cs` is deleted by this unit. The gate above still names it, because
the gate is checked before the work begins. Do not adjust the gate.**

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## Why this unit exists

**Tim ruled the old decoder out on 2026-08-21 — no toggle, no fallback, no
parallel path — and it is still in the tree. It comes out now.**

The unit that replaced the decoder did not finish the removal and said so
honestly. Two units since have parked it. **It has now cost a day.**

Yesterday's sheets read `clockFit dah 15.72 dits`, `decoderWpm not proved,
rolling 50` and `chars 0 emitted` beside text on screen that said
`T E E E E E TTON KT M 5O`. Every one of those numbers came from
`CwSpeedEstimator` and the settled pass — **the removed decoder, which has decoded
nothing since the replacement and whose output nobody can see.** A whole work
order was written from them, diagnosing a clock that was not the clock behind the
words.

**That is the cost of leaving it in: it produces numbers that look like
measurements of the reading and are not.** While it runs it will keep doing so, on
every capture, in the one instrument this project has for scoring.

---

## Verify this instruction against the tree

- **Report mismatches; do not repair the instruction silently.**
- **Record the exact failing-test set before you start and after you finish, and
  name every difference.** 55 were failing at the last report, about fifty of them
  describing the decoder being removed. **This unit should collapse that count.
  Report the exact number and every survivor.**
- **Rulings below are cited by number only. Read each one and apply what it says,
  not what this order says it says.** A preflight found four paraphrased wrongly in
  an earlier order. **If a ruling does not support what this order needs, report
  that and stop.**
- `HM-OPEN-055`: rig tests that flake and pass on a rerun. **Not this unit.**

---

## Rulings in force

- **HM-DEC-120.** Nothing is emitted on audio holding no signal. **Report the
  sensitivity sweep at the end.**
- **HM-DEC-091.** Every number on a sheet must come from the thing that produced
  the text.
- **HM-DEC-146**, `CwGate.ShortestVote` stays at 5. **Moot once `CwGate` is gone;
  say so rather than leaving the ruling dangling.**
- **HM-DEC-150**, the version scheme. Task 6.
- **§12.2**, no radio on the development machine.

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md`
**§13**, which names that file's fields — `STATE`, `PHASE`, `BALL`, `NEXT_PASTE`,
`UPDATED`, `NOTE`. `UPDATED` from the clock; `NOTE` says what is moving inside the
task. Also every ten minutes while a task runs.

---

## Task 1 — Report what still hangs off the old path

**Report before deleting anything.**

The previous unit kept the old decoder running because the element counters, the
watch and the transmit guard hang off it. **Name every one of those dependencies,
what it needs, and where the working decoder can supply it instead.**

For each: is it a fact the probabilistic decoder already has, a fact the tone
tracker or the keying meter has, or a fact that exists only in the old path?

**If anything is genuinely only available from the old code, say so and stop
before deleting it.** That is the one thing that could make this unit larger than
it looks, and it is better found in task 1 than halfway through.

---

## Task 2 — Re-point the survivors

Everything named in task 1 takes its value from the working decoder, the tone
tracker or the keying meter.

**Nothing on any sheet, panel or roster may come from the old path after this
task.** That is the defect this unit exists to remove.

---

## Task 3 — Delete it

`CwGate`, `CwSpeedEstimator`, `CwSettledPass`, `Refine`, the vote window, the
element floors, the old `Emit` sites and everything else that exists only to serve
the removed decode path.

**Keep, as ruled when the decoder was replaced:**

- `CwToneTracker` and the coarse survey. **Finding a station is the one thing that
  works.**
- The keying meter. It is the independent witness and shares no code with the
  decoder on purpose.
- The audio tap, the transmit guard, the capture press, the roster, the sidecar
  and the case measure.

**If any of those turns out to depend on deleted code, task 2 was incomplete —
go back rather than keeping a fragment alive.**

---

## Task 4 — Delete the tests that describe it

**A test that fails only because the old decoder is gone is deleted with it. A
test that asserts something still true is kept and made to pass.**

**Say which you did for each, in a list.** That list is the point of the task —
without it nobody can tell a deletion from a regression.

---

## Task 5 — Prove nothing was lost

- All four recordings holding a station read the same text as before, or better.
- Both recordings holding no keying stay silent, offline and streamed.
- The sensitivity sweep invents nothing at any level.
- **The failing-test set, exactly, with every survivor named and why it survives.**

**If the text got worse anywhere, stop and report.** Removal must not cost
reading.

---

## Task 6 — Bump the version

**Read the current version from `Directory.Build.props`, bump the patch, and
report what it moved from and to.** Do not take a number from this order.

**HM-DEC-150**: the minor is the phase, the patch is the work unit within it. This
is one work unit, so the patch moves by one.

**The previous unit's order carried this task and its report never mentioned the
version.** Say plainly whether the previous bump happened, and if it did not, say
what the version was when you started.

---

## Asks still outstanding

Carried inbound per HM-DEC-139. **Verify against `OPEN_ISSUES.md` and report any
ask that is here and closed, or open and missing.**

- **Whether the sidecar's `text` should include the leading edge.** Tim's.
- **The evenings' captures from the 20th and 21st are not in the tree.**
- **Thirty seconds since the last character, for mode-follow's guard.**
- **Whether `RfGain`'s hundred per cent is a defect or the right answer.**
- **The likelihood gate at 15.0.** Waiting on an evening at the rig.
- **The keying meter's provisional thresholds.**
- **HM-OPEN-052**, **HM-OPEN-053**, **HM-OPEN-054**.
- **HM-DEC-130**, a callsign too long for one keyer send.
- **HM-DEC-098**, whether an attended automatic cycle may reach an antenna.
- **HM-OPEN-033**, the cold-start bin choice.
- **HM-OPEN-007**, open since 2026-08-14.

**The old decoder's removal leaves this queue** with this unit.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: 9.5.1 one branch
and it is `main`, **and every session commits and pushes to it**; no interactive
or destructive git; do not invent a ruling id; do not touch coverage thresholds.

Unit-specific:

- **Do not leave a fragment alive because something depends on it.** *Re-point the
  dependency in task 2. That is what task 2 is for.*
- **Do not weaken a test to make it pass.** *Delete it if it describes the old
  decoder, keep it if it asserts something still true, and say which.*
- **Do not touch the probabilistic decoder's behaviour.** *This unit removes; it
  does not improve.*
- **Do not touch the tone tracker, the survey or the keying meter.**
- **Do not adjust the gate at the top of this file.**

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. **§12.2 names the
four headings** — **What Claude did**, **What Tim should expect**, **What we
should do next**, **What's blocking us** — the last carrying **Asks still
outstanding** per HM-DEC-139. No other headings.

**Section 1 opens with the failing-test count, before and after.**

**Section 2 states in one sentence whether anything he can see changed at all.**
It should not have.

**Stop and report.**
