PHASE: Hamlet works stations on the air
PHASE_SET: 2026-09-06
DESCRIPTION: One click one message - the abort, the waveform, the audio path, the contact state, the right-click menu, and a contact Tim makes
CURRENT_STEP: 2
WORK_INSTRUCTION: 265 - the level Tim is asked to set, and the control to set it with
STEP: 0 | done | the dummy load is gone from the tree
STEP: 1 | done | the abort works before anything can key
STEP: 2 | done | Hamlet's own decoder reads Hamlet's transmission
STEP: 3 | done | the audio reaches the radio and the radio keys
STEP: 4 | partial | the row knows where the contact stands
STEP: 5 | partial | right-click and it goes
STEP: 6 | not started | Tim works a station

---

## What this file is

Where the phase stands right now. The panel reads the header; the launcher writes
`HEARTBEAT:` into it.

**There is no `HEARTBEAT:` line above and one must never be written by hand.** An
invented heartbeat makes the card read *the loop is turning* against a loop that
is not.

**Nothing of this format's own keys may appear below the `---` rule.** The parser
collects them wherever it finds them and then returns the whole file unreadable,
which takes the phase region off the card entirely. Prose here is free; keys are
not.

The previous phase's final status is archived at `docs/phase-onair-run/PHASE_STATUS.md`.

## Why steps 2 and 3 read `done` here, closed by work instruction 266

**This cut of the phase is finished and this file is its closing position.** The
live plan is the re-cut at the repository root, whose steps are 0, A, B, C, D and
E. Nothing below moves again.

**Steps 2 and 3 were left `partial` and both are recorded `done` at the figures
that closed them.**

Step 2, Hamlet's own decoder reads Hamlet's transmission. The loopback read **3 of
3 messages back as themselves** through a real render endpoint, and the corpus
round trip reads back at **15 of 15** including every message at 12000, 44100 and
48000 Hz. The signal is 12.6400 s at every rate.

Step 3, the audio reaches the radio and the radio keys. The **device** half of
criterion 1 was met by **unit 256** and the **rate** half by **unit 262** - the
send path composes at the rate the chosen endpoint declares, and all four render
endpoints on the development machine declare 48000 Hz where the old default was
12000. The **level** is **-12.04 dBFS**, taken from 0.00 dBFS full scale by unit
265, with a Transmit drive control and a dBFS readout on the Settings screen.

**What was open in them, and where it went.** One thing, and it is the same thing
in both: **the level Tim's own radio wants**. That is a fact about his IC-7300's
USB input and its ALC, and no measurement taken on a machine with no radio says
anything about it. It is now **step D of the re-cut plan at the repository root** -
*the drive level his radio wants* - which is a shack step and his to close.
Nothing else was left open in either step.

**No entry body below or in `PHASE_OUTCOME.md` was altered.** The entries are the
record of what the units did and they stand as written. Only the step-state words
in the two headers changed, and this section is the line that says why.

Steps 4 and 5 keep the states they were left at, `partial` in both. Their work is
not lost and is not being re-recorded here: it is **inherited by steps A and B of
the re-cut plan**, which is where it is now accounted for.
