PHASE: FT4 works exactly the way FT8 does
PHASE_SET: 2026-09-08
DESCRIPTION: Where FT4 lives decided by reading upstream, then the decoder, the 7.5 second slot machinery, the ADIF submode, the button, and Tim at the radio
CURRENT_STEP: 1
WORK_INSTRUCTION: 303 - the clock query says what it did
STEP: 0 | done | where FT4 lives, decided by reading
STEP: 1 | partial | FT4 decodes a signal Hamlet made
STEP: 2 | partial | the slot machinery is FT4's
STEP: 3 | done | the log can say FT4
STEP: 4 | partial | the FT4 button works
STEP: 5 | not started | Tim hears FT4
STEP: 6 | not started | Tim works a station on FT4

---

## What this file is

Where the phase stands right now. The panel reads the header; the launcher writes
`HEARTBEAT:` into it.

**There is no `HEARTBEAT:` line above and one must never be written by hand.**

**Nothing of this format's own keys may appear below the `---` rule.** The parser
collects them wherever it finds them and then returns the whole file unreadable,
which takes the phase region off the card entirely. Prose here is free; keys are not.

The send phase is archived at `docs/phase-send-run/`.
