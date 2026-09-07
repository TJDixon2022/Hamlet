PHASE: Hamlet works stations on the air
PHASE_SET: 2026-09-06
DESCRIPTION: One click one message - the abort, the waveform, the audio path, the contact state, the right-click menu, and a contact Tim makes
CURRENT_STEP: 1
WORK_INSTRUCTION: 258 - the ledger, on the scene the last unit left
HEARTBEAT: 2026-09-06 22:28:48
STEP: 0 | done | the dummy load is gone from the tree
STEP: 1 | partial | the abort works before anything can key
STEP: 2 | partial | Hamlet's own decoder reads Hamlet's transmission
STEP: 3 | partial | the audio reaches the radio and the radio keys
STEP: 4 | not started | the row knows where the contact stands
STEP: 5 | not started | right-click and it goes
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
