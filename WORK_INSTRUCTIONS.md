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

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## Why this unit exists

**Two bugs, both found by Tim at the rig. The first is the major one.**

### One: mode-follow is dead

Tim tuned to **14.243 MHz**, which is the SSB portion of 20 metres. **The radio
stayed in CW.** Hamlet used to switch the rig into the mode for the part of the
band it was sitting in, and it no longer does.

The read side is proved sound. His diagnostics screen shows `Mode  CW  CI-V 04
31 seconds ago` — **the radio is reporting its mode correctly and Hamlet has it.**
So the fault is on the write side: either nothing decides to write, or the decision
does not reach the radio.

**This has history and the history is the strongest lead in the unit.** On the 18th
the frequency snap-back bug manifested as `ScheduleModeFollow` firing on every
`FrequencyHz` change, writing unprompted and taking him out of CW for 66 seconds.
That was closed. **Find out what closing it did to the firing condition.** A guard
added then may now be suppressing the legitimate case.

**Do not assume which path he tuned by.** The band-map click and the dial are
different call sites and both must be traced. The dial case is known to reach the
app — the diagnostics screen says the radio tells Hamlet the moment he touches it.

### Two: the preamp is still not on the panel

The previous unit was to put `Preamp`, `Attenuator` and `Overflow` where he is
already looking. **There is no preamp indicator on screen.** The values are in the
diagnostics dialog — `Preamp off`, `Front end overload not overloading`, both read
correctly — and that dialog is a thing he has to go and open.

**Report what the previous unit actually did** before building anything: whether
the panel work was done and did not render, was done somewhere he would not look,
or was not done. Say which.

---

## Verify this instruction against the tree

- **Report mismatches; do not repair the instruction silently.**
- **Record the exact failing-test set before you start and after you finish, and
  name every difference.** The red count from the decoder replacement blinds this
  unit unless the sets are compared exactly.
- `HM-OPEN-055`: rig tests that flake and pass on a rerun. **Not this unit.**

---

## Rulings in force

**HM-DEC-091 — one source, and it says which.**

**HM-DEC-009 — Hamlet does not give a confident wrong answer.** A panel asserting
a setting it has not read is worse than one saying it does not know.

**HM-DEC-093 — no radio on the development machine.** Every test drives rig state
directly.

**Mode-follow may write the mode. It may write nothing else.** Frequency, filter,
power, gain, preamp, attenuator: **not this unit, not as a side effect, not as a
convenience.**

**HM-DEC-098 — attended automatic transmit reaching an antenna is unruled.** This
unit does not transmit and does not touch the interlocks.

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` 13 —
the six fields 13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — Find out why mode-follow stopped

**Report before changing anything.**

1. Where mode-follow lives, what triggers it, and what it writes.
2. **What guards it.** Every condition that can stop it firing, and when each was
   added. **Name the one that is stopping it now.**
3. What the 18th's fix to the snap-back bug changed about that firing condition.
4. Trace **both** tuning paths — the band-map click and the dial — and say for each
   whether it reaches mode-follow at all.
5. Whether the write is attempted and refused, or never attempted.

**If the cause is not in mode-follow, say so and say where it is.** Do not build
around a guess.

---

## Task 2 — Make it follow again

The rig ends up in the mode for the part of the band it is sitting in.

- **Only the mode is written.**
- **It must not fire on a snap-back.** The 18th's bug was a stale read putting the
  old frequency back and mode-follow reacting twice per tune. Whatever guard was
  added for that stays; **the fix is to stop it catching the real case, not to
  remove it.**
- **It must not fight the operator.** If he sets a mode by hand, Hamlet does not
  immediately overwrite it.
- **Say on screen when Hamlet changes the mode**, in the project voice. A rig that
  changes mode with no explanation is its own confident wrong answer.

---

## Task 3 — Put the preamp where he is looking

`Preamp`, `Attenuator` and `Overflow` on the CW terminal panel, beside mode,
filter and the S-meter. **Not in the diagnostics dialog. He has that already and
it is not where he looks while tuning.**

- **Every value is labelled with what it is.** The terminal currently shows
  `off . off`, which says two things are off and nothing about which two. It must
  read `preamp off . att off`, and when the preamp is on it must say **which**
  preamp — `preamp 1` and `preamp 2` are different settings on this radio and
  `on` does not distinguish them. *A reading nobody can interpret is the same
  failure as a reading nobody can find.*
- **A value never read says so.** Not a blank, not a default.
- **`Overflow: overloading` is the loud one** and must be visible without hunting.
- When overflow is asserted, name the control: on the IC-7300 the preamp and
  attenuator share the **P.AMP/ATT** button, each press cycling preamp 1, preamp 2,
  off. **Mention the attenuator only when the preamp is already off.**
- **Do not display `RfGain` as a number.**
- **Do not show a bare value with no name.** *`off . off` is what is on screen
  now and Tim cannot tell which setting is which.* It reads 100% with the knob at noon.
- **The transcript does not move when any of this appears.**

---

## Task 4 — Tests

1. Tuning into a mode's portion of a band writes that mode. **Both tuning paths.**
2. A snap-back does not trigger a write.
3. A mode the operator set by hand is not immediately overwritten.
4. **Nothing but the mode is ever written by mode-follow.**
5. Preamp, attenuator and overflow render on the terminal panel.
6. A setting never read displays as unknown rather than as a value.
7. The overflow advice names the preamp when the preamp is on, and the attenuator
   only when it is off.
8. **Every front-end reading on the panel carries its own name**, and the preamp
   shows which of preamp 1 or preamp 2 is selected rather than `on`.

Then confirm and report that **HM-DEC-120 still holds**.

---

## Task 5 — Record the rulings. **THIS IS THE DROP CANDIDATE.**

**Find the next free `HM-DEC` id. Do not assume one and do not invent one.**
`DECISIONS.md` holds 001-095 then 134 onward, and further ids exist as index rows
in `CLAUDE.md` §1. **Check both.** Report the id and how you established it was
free.

Record that mode-follow writes the mode and nothing else and says so on screen,
and that Hamlet displays receive-path settings and advises on them without writing
them.

**Drop it whole if the session is running long and say so.**

---

## Parked — do not touch, do not raise

- **The fifty dead tests** describing the removed decoder.
- **The inert copy-speed control and the stale panel wording.**
- **Word spacing** on the streaming path.
- **The likelihood gate at 15.0.**
- **HM-OPEN-055, HM-DEC-098, HM-DEC-130, HM-OPEN-033, HM-OPEN-007.**

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: 9.5.1 one branch
and it is `main`, **and every session commits and pushes to it**; no interactive
or destructive git; do not touch coverage thresholds.

Unit-specific:

- **Do not write any setting except the mode.** *Ruled.*
- **Do not remove the snap-back guard to make mode-follow fire.** *That guard
  exists because Hamlet took him out of CW for 66 seconds.*
- **Do not put the front-end readings only in the diagnostics dialog.** *That is
  what the last unit effectively did and it is the bug being fixed.*
- **Do not display `RfGain` as a number.**
- **Do not touch the decoder.**

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings: **What Claude did**, **What Tim should expect**, **What we should
do next**, **What's blocking us** — the last carrying **Asks still outstanding**
per HM-DEC-139.

**Section 1 opens with the named cause of mode-follow not firing** — the specific
condition and when it was added.

**Section 2 states in one sentence what happens when he tunes to 14.243 now, and
in one more the exact text the terminal shows for the front end, verbatim.**

**Report the failing-test set exactly, before and after.**

**Stop and report.**
