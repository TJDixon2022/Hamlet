PHASE: Hamlet reads FT8 as well as the best decoder there is, and then reads it further
PHASE_SET: 2026-09-04
DESCRIPTION: Close the measured 1.5 dB against the published threshold with a sibling library, then combine repeated transmissions to go past it
CURRENT_STEP: 0
WORK_INSTRUCTION: 242 - the scoreboard, and the baseline reproduced rather than inherited
STEP: 0 | not started | there is a scoreboard, and the arbiter can read it
STEP: 1 | not started | Ft8Sharp.Deep exists and changes nothing
STEP: 2 | not started | ordered statistics decoding closes the code gap
STEP: 3 | not started | strong signals are subtracted and the slot is read again
STEP: 4 | not started | each candidate is re-synced at baseband
STEP: 5 | not started | Hamlet's own SNR is measured and shown
STEP: 6 | not started | repeated transmissions are combined across slots

---

## What this file is

Where the phase stands right now. The panel reads the header; the launcher writes
`HEARTBEAT:` into it.

**There is no `HEARTBEAT:` line above and one must never be written by hand.** An
invented heartbeat makes the card read *the loop is turning* against a loop that
is not. Until the launcher writes one, the correct card reading is *stopped*, and
that is the true reading today.

**Nothing of this format's own keys may appear below the `---` rule.** The parser
collects them wherever it finds them and then returns the whole file unreadable,
which takes the phase region off the card entirely. Prose here is free; keys are
not.

The previous phase's final status is archived at `docs/phase-ft8/PHASE_STATUS.md`.
