PHASE: Hamlet works stations on the air
PHASE_SET: 2026-09-07
DESCRIPTION: Re-cut so its steps can close - the contact state, the right-click menu, the whole chain at the bench, then Tim at the radio
CURRENT_STEP: 0
WORK_INSTRUCTION: 267 - right-click and it goes, and the licence gate holds from the click
HEARTBEAT: 2026-09-07 10:29:46
STEP: 0 | not started | the record is honest about where the phase stands
STEP: A | not started | the row knows where the contact stands
STEP: B | not started | right-click and it goes
STEP: C | not started | the whole chain runs from one click, at the bench
STEP: D | not started | the drive level his radio wants
STEP: E | not started | Tim works a station

---

## What this file is

Where the phase stands right now. The panel reads the header; the launcher writes
`HEARTBEAT:` into it.

**There is no `HEARTBEAT:` line above and one must never be written by hand.**

**Nothing of this format's own keys may appear below the `---` rule.** The parser
collects them wherever it finds them and then returns the whole file unreadable,
which takes the phase region off the card entirely. Prose here is free; keys are
not.

The previous cut of this phase is archived at `docs/phase-send-run/`.
