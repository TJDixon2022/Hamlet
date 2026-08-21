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

**Hamlet knew why it could not read the band and did not say.**

`cw-2026-08-21-183848`, 20 metres in daylight, S9, and nothing readable. The
sidecar carried the answer: `Overflow: overloading`, `Preamp: preamp 1`,
`RfGain: 100%`. The receiver's front end was being overdriven. Measured on the
recording: 16 to 17 dB of envelope swing at **every** pitch from 450 to 700 Hz,
with 300 to 900 spurious sub-20 ms runs at all of them. A real station gives 22 to
24 dB at one pitch and noise elsewhere. Overload compresses the whole passband
together, so there is no tone standing above anything.

Tim could hear dits and dahs — the ear takes pitch and rhythm out of a compressed
mess. **The decoder measures amplitude, and amplitude is what overload destroys.**
Both Hamlet and an independent decoder read nothing from that file, correctly.

**The app was reading `Overflow` and `Preamp` from the radio every capture and
showing him neither.** He found it in a text file afterwards. That is the gap this
unit closes.

**The point of this application is to find CW on the air that the operator cannot
yet read and hold conversations with it.** A receiver setting standing between him
and a contact, which the app already knows about and does not mention, is squarely
in the way of that.

### Ruled by Tim

**Read only.** Hamlet displays these settings and says what they mean. **It does
not write them.**

*Rejected: Hamlet turning the preamp off itself.* Receive-only settings radiate
nothing, so a write would be safe in that narrow sense — but it is still the app
changing his radio underneath him, and mode-follow writing unprompted cost an
evening and a ruling. **A later unit may offer a button he presses. Not this one.**

---

## Verify this instruction against the tree

- **Report mismatches; do not repair the instruction silently.**
- **Record the exact failing-test set before you start and after you finish, and
  name every difference.** The tree carries a large red count from the decoder
  replacement; that count blinds this unit unless the set is compared exactly.
- `HM-OPEN-055`: rig tests that flake and pass on a rerun. **Not this unit.**

---

## Rulings in force

**HM-DEC-091 — one source, and it says which.** Every one of these facts comes
from the radio. Where a read is stale or has never been answered, the display says
so rather than showing a value that looks current.

**HM-DEC-009 — Hamlet does not give a confident wrong answer.** A panel asserting
the preamp is off when the read failed is worse than a panel saying it does not
know.

**HM-DEC-093 — no radio on the development machine.** Every test drives rig state
directly.

**HM-DEC-120 — nothing is emitted on audio holding no signal.** Untouched by this
unit; confirm it still holds.

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` 13 —
the six fields 13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — Report what is already read

**Report before changing anything.**

For `Overflow`, `Preamp`, `Attenuator` and `RfGain`: the CI-V command each comes
from, how often it is read, how stale each can be, and what happens to the value
when a read has never been answered.

**Two of these are known to be untrustworthy and must be reported honestly:**

- **`RfGain` reads 100% with the knob at noon.** Tim observed this directly. If
  the read is wrong, **`RfGain` must not go on the panel as a number** — say so
  and leave it off rather than displaying a figure he has already seen contradict
  his own radio.
- **`Overflow` has been seen reading `overloading` exactly once.** Nothing
  establishes how it behaves normally. **Say what is known about it and what is
  not.**

---

## Task 2 — Put the front end on the panel

`Preamp`, `Attenuator` and `Overflow` where he is already looking while tuning —
alongside mode, filter and the S-meter, not in a diagnostics screen he would have
to go and find.

- **A value that has never been read says so.** Not a blank, not a default.
- **`Overflow: overloading` is the loud one.** It is a condition that stops the
  band being readable and it should be visible without hunting.
- **The transcript does not move when it appears.** The layout rule from the
  previous unit governs: nothing appearing or disappearing may reflow the terminal
  or its siblings.

---

## Task 3 — Say what to do about it, in terms of a knob

When overflow is asserted, Hamlet says so in the project voice and names the
control, not the concept. **"Your receiver is overloading" is a diagnosis. "Your
receiver is overloading — try the preamp off" is help.**

- Name the front-panel control. On the IC-7300 the preamp and attenuator share the
  **P.AMP/ATT** button; each press cycles preamp 1, preamp 2, off.
- **Mention the attenuator only if the preamp is already off.** Advice for a knob
  already in the right position is noise.
- **Do not advise on `RfGain` unless task 1 finds its read is trustworthy.**
- **Say it once and let it stand while the condition holds.** Do not repeat, blink
  or re-announce.

**Wording is yours.** The requirement is that an operator who has never thought
about front-end overload knows which button to press.

---

## Task 4 — Tests

1. Overflow asserted puts the message on screen; overflow clearing removes it.
2. The preamp advice appears when the preamp is on, and the attenuator advice only
   when the preamp is off.
3. A setting never read displays as unknown rather than as a value.
4. **Nothing in this unit writes to the radio.** Assert it — no CI-V write is
   issued by any path this unit adds.

Then confirm and report that **HM-DEC-120 still holds**.

---

## Task 5 — Record the ruling. **THIS IS THE DROP CANDIDATE.**

**Find the next free `HM-DEC` id. Do not assume one and do not invent one.**
`DECISIONS.md` holds 001-095 then 134 onward, and further ids exist as index rows
in `CLAUDE.md` §1. **Check both.** Report the id and how you established it was
free.

The ruling records that Hamlet displays receive-path settings and advises on them,
and does not write them; and that where the app can name the control standing
between the operator and a readable signal, it does.

**Drop it whole if the session is running long and say so.**

---

## Parked — do not touch, do not raise

- **The mode-follow regression** — the app no longer switching to USB in the voice
  portion of a band. Real, reported, and its own unit.
- **The fifty dead tests** describing the removed decoder.
- **The inert copy-speed control and the stale panel wording.**
- **Word spacing** on the streaming path.
- **The likelihood gate at 15.0.** Waiting on an evening at the rig.
- **HM-OPEN-055, HM-DEC-098, HM-DEC-130, HM-OPEN-033, HM-OPEN-007.**

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: 9.5.1 one branch
and it is `main`, **and every session commits and pushes to it**; no interactive
or destructive git; do not touch coverage thresholds.

Unit-specific:

- **Do not write any setting to the radio.** *Ruled. Not the preamp, not the
  attenuator, not RF gain, not as a fallback, not behind a flag.*
- **Do not display `RfGain` as a number unless task 1 finds the read is sound.**
  *He has seen it report 100% with the knob at noon.*
- **Do not infer overload from the audio.** *The radio reports it. HM-DEC-091.*
- **Do not move the transcript.** *The layout rule stands.*
- **Do not touch the decoder.** *It read that recording correctly and so did an
  independent one.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings: **What Claude did**, **What Tim should expect**, **What we should
do next**, **What's blocking us** — the last carrying **Asks still outstanding**
per HM-DEC-139.

**Section 1 opens with task 1's answer on `Overflow` and `RfGain`** — what is
trustworthy and what is not.

**Section 2 states in one sentence what he will see next time the front end
overloads.**

**Report the failing-test set exactly, before and after.**

**Stop and report.**
