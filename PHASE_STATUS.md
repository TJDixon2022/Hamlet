PHASE: Hamlet hears FT8 off the radio and displays the decoded text on screen
PHASE_SET: 2026-08-31
DESCRIPTION: Port ft8_lib to a managed FT8 decoder and wire it to Hamlet's audio and display
CURRENT_STEP: 6
WORK_INSTRUCTION: 233 - the session that decoded nothing, read with an instrument that can read it
HEARTBEAT: 2026-09-03 11:48:29
STEP: 1 | done | the library exists and its tables are proven
STEP: 2 | done | messages round-trip through 77 bits
STEP: 3 | done | a valid FT8 signal can be produced
STEP: 4 | done | signals are found in noise
STEP: 5 | done | a found signal becomes a message
STEP: 6 | blocked | sensitivity meets the published threshold
STEP: 7 | blocked | Hamlet displays decoded FT8

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
