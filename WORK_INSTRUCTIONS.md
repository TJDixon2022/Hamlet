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

# Work instruction 043 — what tonight's captures caught

**ISSUED: 2026-08-29. A fresh order, not an amendment. Follows unit 042.**

**Seven tasks; task 7 is the drop. This is a long unit by instruction.**

**The digital work is parked for this unit. FT8 is a daytime mode and CW is a
night mode; it is night.**

## Why this unit exists

**Five captures taken between 02:05 and 02:10 UTC on 2026-08-29 caught four
distinct faults, all upstream of the decoder, and every one of them is in the
sidecars.**

### Fault 1 — letters from a pitch nobody chose

`cw-2026-08-29-020809`, at 7.0372 MHz:

> `unkeyed YES` — **237 characters reached the screen from a pitch chosen by the
> middle of the bank, which nothing chose, with no keying admitted here.**
> `toneHz 575.0 (NOT MEASURED)`. `reading` 0.2 better than silence against a gate
> of 1. `decoderWpm not proved`, and the speed search **pinned at the top of its
> range**, which is what a speed search does when there is nothing to fit.

`cw-2026-08-29-020938`, at 7.0502 MHz: the same, 300 characters, this time from
**the loudest bin in the band** rather than the bank centre.

**Every instrument said no station. Letters reached the screen anyway.**

**Unit 036 shipped a refusal for the case where a pitch is admitted at the wrong
frequency. This is its sibling and it is not covered: nothing was admitted at
all, and a fallback — bank centre, or loudest bin — supplied a pitch that the
emit path then treated as a station.** The blocks in that text are correct. The
green letters between them are the fault, and on screen they are
indistinguishable from the real letters of `HIGHER IN BAND` three captures
earlier.

### Fault 2 — admission refuses a real station

Also `cw-2026-08-29-020938`. **Measured outside Hamlet, on the audio in that
file: a carrier at 802.7 Hz standing 21.2 dB over the band floor, keyed with 66
key-downs, about 14 WPM.** That is a station by any reading.

Hamlet's own sidecar agrees something is there — *the loudest thing in the band
is at 800 Hz, +19.7 dB over the band floor, keyed 46% of the time* — and then
says **"Nothing has judged it to be a station."**

**Two independent decoders converge on the same words from that audio.** Hamlet
read `S T L O A I S`; an independent decode of the same file read `L?OUIS`.
**That is almost certainly `ST LOUIS`.** A real conversation is being read, badly,
by a decoder that has refused to admit the station it is reading.

**Faults 1 and 2 are the same organ failing in opposite directions**, and they
appear in the same capture.

### Fault 3 — the sweep's 25 Hz grid

The independent keying sweep reported **825 Hz** for a station measured at
**802.7 Hz** — 22 Hz off, because it steps in 25 Hz increments over 400 to 1200
Hz. **The field report of 2026-08-24 named this grid and it is still there.**

### Fault 4 — the dial moved and nothing reset

Across the five captures the operator moved 7.0284 → 7.0372 → 7.0502. **The
transcript carried across all of it**, so text read from the 550 Hz station was
still on screen while the decoder was pointed at a different frequency entirely.

**And the state carried too.** `tonePeak` is documented as *held and decaying* —
so a peak measured on the first station was still decaying into readings taken
where there was no station at all. **The tracker arrived at each new frequency
holding a memory of the last one**, which is part of why the pitch wandered 550 →
575 (bank centre) → 800 (loudest bin), two of those three labelled NOT MEASURED.

**The operator's instruction: when the frequency changes, clear and reset.**

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
Trust the tree over this order everywhere they differ.

**This author has not seen unit 042's report.** State what landed in 042 —
particularly whether a per-neighborhood settings mechanism exists, because **task
7 inherits it if it does and must not build a second one.**

**Record the failing counts for both suites before task 2.** Unit 041 last
reported engine 28 of 1916 byte-identical, app 509 of 509;
`AConfirmedModeWriteFoldsTheDataVariantTooAsync` is a known intermittent.

**Tonight's five captures should be in the tree** — `cw-2026-08-29-020541`,
`-020616`, `-020707`, `-020809`, `-020938`, with sidecars and a
`cases-2026-08-28.txt` roster. **Confirm; if any are missing, say which.**

## Rulings in force

**Transcribed with what was rejected. Do not re-argue either.**

**Tim's rulings:**

> **When the frequency changes, clear and reset.**

> **Ship the refusal** (2026-08-27, unit 036): Hamlet stops printing letters from
> a pitch the survey admitted no keying at, and `N4L` becomes blocks. **The
> phantoms are the priority.** `N4L` returns as an anchor when admission can find
> that station honestly.
>
> **Rejected with it and not to be revisited:** the clock-withdrawn refusal,
> measured dead; raising the gate, which fires correctly.

> **Hamlet sets whatever the radio needs for the mode. The operator does not touch
> the radio.** Tuning changes only what would stop him hearing the block, leaves
> everything else alone, and says in plain words what it changed and why. **Once
> per tune-in, then hands off.**

**Standing rulings this unit is bound by:**

- **§0.0 / HM-DEC-009** — never present a guess as a decode. **Fault 1 is this
  rule broken; fault 2 is its cost when the same organ overcorrects.**
- **HM-DEC-120** — nothing is emitted on audio holding no signal, and no letters
  from a pitch nobody judged to be a station. **Tightened only, never loosened.**
- **§0.0.1** — the app's record must distinguish a fault in the signal, the radio,
  or Hamlet. **Tonight it did, in every sidecar. Do not weaken it.**
- **HM-DEC-007** — decoders tested against WAV fixtures.
- **HM-DEC-050 / §0.5** — no rig-control panel.
- **§0.2 / HM-DEC-008** — **no transmit work of any kind.**

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` — `STATE`,
`TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is moving
inside the task. Same every ten minutes while a task runs.

## The tasks

### Task 1 — reproduce all four faults before changing anything

**Run both suites whole and record the numbers first.**

Then reproduce, **on tonight's captures, in tests**, and **report the numbers
before task 2 changes a line** (§0.4):

- **Fault 1:** `-020809` and `-020938` emit letters with nothing admitted. Assert
  the character counts — this order believes 237 and 300 for the session, 65 and
  63 within those files. **Correct the figures from the tree.**
- **Fault 2:** `-020938` holds a keyed carrier at 802.7 Hz, 21 dB over the floor.
  **Assert that admission currently refuses it**, and report **why, with file and
  line** — which test in the admission path it fails, and by how much.
- **Fault 3:** the sweep's reported pitch against the measured one on that file.
- **Fault 4:** state carried across a frequency change — name every field that
  survives a dial move, with file and line.

**Say what you find rather than confirming this list.** If a fault does not
reproduce, that is a finding and the task that depends on it is reported rather
than guessed at.

### Task 2 — no pitch admitted, no letters

**Extend unit 036's refusal to cover the case where nothing was admitted at all.**

- **A pitch that came from the bank centre, or from the loudest bin, or from any
  fallback, is not a station.** Letters must not be emitted from it.
- **Blocks rather than deletions**, as 036 ruled, so no character position is
  lost and only the assertion goes.
- **The sidecar's `unkeyed YES` already detects this exactly.** It is the same
  condition; wire the emit path to it rather than inventing a second test.

**Report the cost per test before declaring the task done**, not after: name every
test that goes red and what it loses. **If the cost is materially larger than unit
036's five tests, stop and report rather than shipping.**

**Acceptance:** `-020809` and `-020938` emit **no letters at all**. Every capture
where a station was genuinely admitted keeps what it reads — the three captures
of the ragchew are the floor and **must not lose more than they read tonight.**

### Task 3 — why admission refused a 21 dB station

**Measure and report. Do not change admission in this task.**

On `-020938`, and across every capture in the corpus:

- What admission requires, and **which requirement the 802.7 Hz carrier failed**,
  with the measured value beside the threshold.
- **How often, across the corpus, a carrier standing more than 15 dB over the band
  floor and keyed between 20% and 70% of the time is refused.** That number is the
  size of fault 2.
- Whether the refusal is the gate, the keying test, the duty test, or the tracker
  never offering the candidate at all.

**This is the measurement the next unit is built from. Change nothing.**

### Task 4 — the sweep's grid

The independent sweep steps 400 to 1200 Hz in 25 Hz increments and reported 825 Hz
for a station at 802.7 Hz.

- **Interpolate between bins**, the way `toneHz` already does when a pitch is
  measured, or narrow the step. **State which and why.**
- **The sweep must stay independent of the decoder** — that independence is its
  whole value, and it is what caught the tracker's 750–775 Hz hold on four
  captures. **Do not couple it to the tracker to fix its resolution.**

**Acceptance:** on `-020938` the sweep reports a pitch within 5 Hz of 802.7, and
the CW captures where the sweep was already right do not move.

### Task 5 — a frequency change clears and resets

**When the dial moves, the decoder starts fresh.**

- **Reset:** the tracked pitch, the held-and-decaying `tonePeak`, the speed
  hypothesis, the element and character counters, and anything else task 1 found
  surviving a dial move. **The decoder's state after a frequency change is the
  state it has when it first begins listening.**
- **The transcript is not erased.** The operator may still be reading it. **Mark a
  visible break** carrying the new frequency, so text from the old station stays
  readable and nothing new can be confused with it. A `Clear` button already
  exists and is his to use.
- **A small nudge is not a move.** Fine-tuning a station by a few hundred hertz
  must not reset anything — clearing on every dial click would be unusable.

**Raise, do not decide, in HM-DEC-010's options-table form:**
- **The threshold** at which a dial change counts as a move. Cost at least three
  candidates against tonight's captures, where the moves were 8.8 kHz and 13.0
  kHz and the CW filter is 500 Hz wide.
- **What the break looks like** in the terminal. **The wording is Tim's** (§12.1):
  put the exact proposed text in the report for approval rather than treating it
  as settled.

### Task 6 — regression fixtures from tonight

Tonight's five captures become fixtures with their measured truth recorded:

- `-020541`, `-020616`, `-020707` — **a real ragchew, correctly admitted.**
  Fragments read correctly tonight and not to be lost: `HIGHER IN BAND`,
  `AGE HR 85`, `FB JIM U GO BACK`, and `<BT>` in its right places. **These are the
  floor.**
- `-020809` — **nothing there. The floor is zero letters.**
- `-020938` — **a station at 802.7 Hz that admission refused**, and text that
  independently decodes to something containing `ST LOUIS`. **Its floor is zero
  letters until task 3's finding is acted on**, and it carries a note saying it
  becomes a reading anchor when admission finds that station honestly — the same
  form unit 036 used for `N4L`.

**Record the callsigns as read and as uncertain**: `K8GPH` and `K8MPH` are the
same station read two ways, and `WS3EAA` is probably `W3EAA`. **Do not assert
which is right.**

### Task 7 — CW's receive conditions *(the drop candidate)*

**Only if unit 042 landed a per-neighborhood settings mechanism. If it did not,
skip this task, say so, and do not build a second mechanism.**

All evening the attenuator sat at **20 dB** with the preamp off while the station
faded S4 → S1 → S0, and `CwPitch` read **600 Hz** while the stations measured at
**542 Hz** and **802.7 Hz**.

- **CW's neighborhood rows state what CW needs**, with the reason as text beside
  each value, the same shape 042 used for FT8.
- **The attenuator comes off unless the front end is actually overloading** —
  which Hamlet already reads. Twenty decibels thrown away on a fading signal is
  the same class of defect as a scope span that renders the block seven pixels
  wide.
- **Read first, write only what would get in the way, read back, and say what
  changed and why** — 042's rule, inherited, not reinvented.
- **The operator's hand wins**, and once per tune-in then hands off.

**Raise, do not decide:** whether `CwPitch` should follow the measured tone of an
admitted station. It would centre the filter on the station, **and it changes what
the operator hears**, which is a different kind of write from the others.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

**The whole digital stream** — the FT8 decoder, the slot cutter, the sync search,
the digital waterfall, the digital capture press. **It is night and FT8 is a
daytime mode.**

Also: the joint decoder; the constrained margin; the meter's rebuild; the
integrator width; the whole-file second pass; the scanner and the calling cycle;
`CHANGELOG.md`; the missing `DECISIONS.md` records including HM-DEC-086's
supersession; the phrasebook and the recent-places row; the Twin PBT, which needs
a manual page not on the machine.

**Both halves are required: do not touch them, and do not raise them.**

A parked item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **No transmit. Nothing keys the radio.**
- **Do not change admission in task 3.** Measure only. The next unit is built from
  that measurement.
- **Do not loosen the silence property.** This unit only tightens it.
- **Do not delete an anchor.** Re-express it with its reason, as unit 036 ruled.
- **Do not let the three ragchew captures lose what they read tonight.**
- **Do not couple the independent sweep to the tracker.**
- **Do not erase the transcript on a frequency change.** Mark a break.
- **Do not build a second settings mechanism** if 042 landed one.
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 to `output.md` at the repository root, overwritten
and printed. **Read the file's own section count and follow it.**

**The section that says what the owner should expect leads with this: on a
frequency where nothing is happening the terminal now shows blocks and no letters
at all, and moving the dial starts the decoder fresh rather than carrying the last
station's pitch and text along with it.**

**The section that reports measurements leads with task 3's number** — how often
across the corpus a strong, keyed carrier is refused admission. **That is the size
of what is still wrong.**

**If you finish every task, stop and report. Do not start the next unit.**
