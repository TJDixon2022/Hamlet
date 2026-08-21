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

Two faults, both found by Tim at the rig, both in the CW terminal. **The first is
a prime-directive violation and comes first.**

### One: Hamlet decodes the operator's own sending and shows it as received

Tim keyed the radio by hand and the terminal filled with fragments of his own
transmission. **Nothing on screen distinguished it from a station.** That is the
confident wrong answer HM-DEC-009 forbids, in a place nobody thought to guard.

**The subtler half is worse.** When CW transmit lands, the app will decode its own
sent text back and present it as received. An operator could read their own
callsign returning and believe somebody answered.

**The fragmentation is a symptom, not the bug.** Break-in is `full`, so the
receiver opens between elements and the sidetone arrives chopped by
transmit-receive switching, decoding as a page of isolated `E` characters. **The
decoder is behaving correctly on input it should never have been given.**

**Ruled by Tim.** Suspend decoding while the radio is transmitting; resume when it
drops. Transmit state is a fact the radio reports over CI-V `1C 00`, Hamlet
already reads it, the diagnostics screen already displays it correctly, and
**nothing consumes it**. Say what is happening rather than going silently blank.
And whatever Hamlet sends must never appear in the received-text stream.

### Two: the advisory boxes appear and disappear, and the screen jumps

Below the terminal sit a stack of panels that come and go independently — the
tone advisory, the keying meter, the case-press prompt, the nothing-coming-through
note, the have-a-look offer, the dimmed-character legend. **Each one appearing or
vanishing reflows everything around it.**

**Tim's words: he hates it.** He is watching that screen for half an hour at a
time while tuning across a band, and the thing he is reading moves under him.

---

## Verify this instruction against the tree

- **Report mismatches; do not repair the instruction silently.**
- **The tree is at 55 failing tests**, fifty of which describe the decoder removed
  two units ago and were not deleted. **That count blinds this unit** — a
  regression here would be invisible in the noise. **Record the exact set before
  you start and the exact set at the end, and name every difference.** Do not
  delete the fifty here; that is its own unit.
- Three of the 55 are `HM-OPEN-055`, rig tests that flake and pass on a rerun.
  **Not this unit.**

---

## Rulings in force

**HM-DEC-009 — Hamlet does not give a confident wrong answer.** Text presented as
received that the operator sent himself is the purest form of it.

**HM-DEC-091 — one source, and it says which.** Transmit state comes from the
radio, never from the audio.

**HM-DEC-093 — no radio on the development machine.** Transmit state cannot be
exercised live here. **Every test must drive the state directly.**

**HM-DEC-098 — an attended automatic transmit cycle reaching an antenna is
unruled and stays unruled.** This unit does not transmit, does not enable
transmitting, and does not touch the interlocks. It observes.

**HM-DEC-120 — nothing is emitted on audio holding no signal.** Must still hold.

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` 13 —
the six fields 13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — Report what already exists

**Report before changing anything.**

1. Where transmit status arrives from CI-V `1C 00`, what type carries it, how
   often it is polled or whether it is broadcast, and how stale it can be.
2. What the diagnostics screen reads to display it correctly.
3. What the "transmit guard" kept in an earlier unit actually guards, and whether
   it is the same fact or a different one.
4. Every consumer of transmit state today, if any.
5. **How quickly the state is known to change.** Full break-in switches between
   elements — tens of milliseconds. **Say what the poll cadence is and whether it
   can see that at all.** If it cannot, the hysteresis in task 2 is doing more work
   than it looks, and that must be stated rather than assumed.

---

## Task 2 — Suspend decoding while transmitting

- Decoding suspends when transmit is asserted and resumes when it drops.
- **Hysteresis, so full break-in does not flap the decoder on and off between
  elements.** Choose the hold-off, **say what you chose and what evidence you have
  for it.** If the evidence is only the poll cadence from task 1, say that.
- **Suspension must not cost the decoder its state.** The speed hypotheses and the
  noise floor tracking survive a transmit cycle. An operator sending a few
  characters mid-contact must not return to a decoder that has forgotten the
  station it was reading.
- **Nothing decoded during suspension reaches the terminal, the transcript, the
  sidecar or the roster.** Not held and released later — not decoded at all.

---

## Task 3 — Say so on screen

The terminal header states that decoding is suspended and why, in the project
voice, while it is suspended — something along the lines of *you're sending, so
Hamlet is listening to you rather than the band*. The wording is yours.

**A terminal that has stopped without saying why is its own confident wrong
answer**: the operator reads an empty screen as a quiet band.

---

## Task 4 — Sent text and received text are different things

Whatever Hamlet sends must never enter the received-text stream, by any path.
**This holds independently of task 2** — it must be true even if the transmit
state is late, wrong, or missing entirely, because it is a fact about where text
came from rather than about what the radio was doing.

Report how the two are kept apart and what would have to go wrong for them to
merge.

---

## Task 5 — The transmit tests Tim named

All four, driving transmit state directly (HM-DEC-093):

1. Decoding is suspended while transmit is asserted.
2. Decoding resumes when transmit drops.
3. **Full break-in cycling does not cost the decoder its speed estimate or its
   noise floor tracking.**
4. **No text decoded during transmit reaches the terminal.**

Then confirm and report that **HM-DEC-120 still holds** — both recordings holding
no keying stay silent, and the sensitivity sweep still invents nothing.

---

## Task 6 — Record the ruling

**Find the next free `HM-DEC` id. Do not assume one and do not invent one.**
`DECISIONS.md` holds 001-095 then 134 onward, and further ids exist as index rows
in `CLAUDE.md` §1. **Check both.** Report the id you used and how you established
it was free.

The ruling records that Hamlet suspends decoding while the radio is transmitting,
that transmit state comes from the radio and never from the audio, and that sent
text never enters the received stream.

---

## Task 7 — The screen must not move under him

**The governing rule: the transcript never moves.** Nothing appearing or
disappearing below it may reflow it, and nothing appearing or disappearing may
shift its siblings either.

The shape is yours, but the constraint is not. Two approaches that satisfy it:

- **One advisory region of fixed height**, showing the most useful message at any
  moment by priority, its content swapping in place rather than panels stacking
  and unstacking.
- **Every panel always present**, occupying its space whether or not it has
  something to say, so appearing is a change of content rather than a change of
  layout.

**Prefer the first if the messages are genuinely alternatives.** Look at what is
on screen at once in the capture that prompted this: a tone advisory, a keying
meter, a case-press prompt, a nothing-coming-through note, a have-a-look offer and
a dimmed-character legend. **Several of those are saying versions of the same
thing at the same time** — that nothing is being read. That is worth reporting
even if you do not act on it.

- **Do not remove any message.** Each is there for a reason and this unit is about
  where they sit, not whether they are said.
- **The keying meter's own text may not change**, only where it lives. It is the
  independent witness and its wording was ruled.
- **A message that must be seen immediately — transmit suspension from task 3 —
  outranks the rest.**

Report what you chose, what the priority order is, and what a session reading this
later would need to know to add a message without reintroducing the jump.

---

## Task 8 — The panel's stale language. **THIS IS THE DROP CANDIDATE.**

The copy-speed control still sets a seed **the new decoder does not read**. It is
inert. The wording beside it still describes a fitted speed and an operator seed —
machinery that no longer exists.

Either make the control do something the decoder honours, or remove it and the
wording with it. **If that is a judgement between two costs, say so and stop** —
a control that looks live and does nothing is its own confident wrong answer, but
which way to resolve it is Tim's.

**Drop it whole if the session is running long and say so.**

---

## Parked — do not touch, do not raise

- **The fifty dead tests** describing the removed decoder. Their own unit.
- **Word spacing** on the streaming path.
- **The likelihood gate at 15.0.** Waiting on an evening at the rig.
- **HM-OPEN-055**, the flaking rig tests.
- **HM-DEC-130, HM-OPEN-033, HM-OPEN-007.**

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: 9.5.1 one branch
and it is `main`, **and every session commits and pushes to it**; no interactive
or destructive git; do not touch coverage thresholds.

Unit-specific:

- **Do not infer transmit from the audio.** *Not from level, not from the
  sidetone's pitch, not from a change in the noise floor. The radio reports it and
  Hamlet already has it.*
- **Do not transmit, and do not touch the interlocks.** *HM-DEC-098 is unruled.*
- **Do not let suspension reset the decoder.** *A guard that costs the station is
  a worse bug than the one being fixed.*
- **Do not hold decoded text during transmit and release it afterwards.** *It was
  never received. Presenting it late is the same misattribution with a delay.*
- **Do not silence any advisory to stop the jump.** *Task 7 is about layout.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings: **What Claude did**, **What Tim should expect**, **What we should
do next**, **What's blocking us** — the last carrying **Asks still outstanding**
per HM-DEC-139.

**Section 1 opens with task 1's answer to how quickly transmit state is known**,
because the hysteresis rests on it.

**Section 2 states in one sentence what the terminal shows while he is sending,
and in one more whether the screen still moves.**

**Report the failing-test set exactly, before and after, and name every
difference.**

**Stop and report.**
