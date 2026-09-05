PHASE: Everything this project has built reaches the operator's screen, and the decoder is taken as far as it will go
PHASE_SET: 2026-09-05
DESCRIPTION: Wire Ft8Sharp.Deep into Hamlet, show a real SNR, then take ordered statistics, subtraction and cross-slot combining as far as they go
CURRENT_STEP: 4
WORK_INSTRUCTION: 253 - strong signals are subtracted and the slot is read again
HEARTBEAT: 2026-09-05 16:17:56
STEP: 0 | done | Hamlet decodes through Ft8Sharp.Deep
STEP: 1 | done | the gate set exists, and the slow tests are named
STEP: 2 | done | the SNR column shows a number
STEP: 3 | done | ordered statistics, taken as far as it goes
STEP: 4 | not started | strong signals are subtracted and the slot is read again
STEP: 5 | not started | repeated transmissions are combined across slots
STEP: 6 | not started | the closing measurement

---

## What this file is

Where the phase stands right now. The panel reads the header; the launcher writes
`HEARTBEAT:` into it.

**There is no `HEARTBEAT:` line above and one must never be written by hand.** An
invented heartbeat makes the card read *the loop is turning* against a loop that
is not. Until the launcher writes one, the correct card reading is *stopped*.

**Nothing of this format's own keys may appear below the `---` rule.** The parser
collects them wherever it finds them and then returns the whole file unreadable,
which takes the phase region off the card entirely. Prose here is free; keys are
not.

The previous phase's final status is archived at
`docs/phase-sensitivity-run/PHASE_STATUS.md`.
