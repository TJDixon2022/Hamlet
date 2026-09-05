PHASE: Hamlet reads FT8 as well as the best decoder there is, and then reads it further
PHASE_SET: 2026-09-04
STEP: 0 | not started | there is a scoreboard, and the arbiter can read it
STEP: 1 | not started | Ft8Sharp.Deep exists and changes nothing
STEP: 2 | not started | ordered statistics decoding closes the code gap
STEP: 3 | not started | strong signals are subtracted and the slot is read again
STEP: 4 | not started | each candidate is re-synced at baseband
STEP: 5 | not started | Hamlet's own SNR is measured and shown
STEP: 6 | not started | repeated transmissions are combined across slots

---

## What this file is

The phase's memory. `output.md` is overwritten every unit and cannot carry what
was tried; this file survives the unit and records, per unit, the approach taken
and what it hit.

**The header above is a cursor over the entries below, and the entries win.** If
they disagree, the header is wrong and the entries are what happened.

Appended by `tools\arbiter\outcome-append.bat`. Five state words and no others:
`not started`, `in progress`, `partial`, `blocked`, `done`.

The previous phase's entries are archived at `docs/phase-ft8/PHASE_OUTCOME.md`
and are not carried forward. **Its last entry, `## UNIT 1 - STEP 1`, dated
2026-09-04, belongs to no phase**: it was appended by a run of work instruction
242 that halted before the phase transition, against the old plan's step 1. It is
not an FT8-phase unit and should not be read as one. Its closing
position: steps 1 to 5 done, step 6 blocked on `HM-OPEN-067`, step 7 closed by
Tim's eyes at 21:41 UTC on 2026-09-04.

## Entries

*None yet. This phase has not started.*
