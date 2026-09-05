PHASE: Everything this project has built reaches the operator's screen, and the decoder is taken as far as it will go
PHASE_SET: 2026-09-05
STEP: 0 | done | Hamlet decodes through Ft8Sharp.Deep
STEP: 1 | not started | the gate set exists, and the slow tests are named
STEP: 2 | not started | the SNR column shows a number
STEP: 3 | not started | ordered statistics, taken as far as it goes
STEP: 4 | not started | strong signals are subtracted and the slot is read again
STEP: 5 | not started | repeated transmissions are combined across slots
STEP: 6 | not started | the closing measurement

---

## What this file is

The phase's memory. `output.md` is overwritten every unit and cannot carry what
was tried; this file survives the unit and records, per unit, the approach taken
and what it hit.

**The header above is a cursor over the entries below, and the entries win.** If
they disagree, the header is wrong and the entries are what happened.

Appended by `tools\arbiter\outcome-append.bat`. Five state words and no others:
`not started`, `in progress`, `partial`, `blocked`, `done`.

The sensitivity phase's entries are archived at
`docs/phase-sensitivity-run/PHASE_OUTCOME.md`. Its closing position: steps 0, 1
and 4 done, step 2 built and measuring 33 of 306 at -21 dB against a waypoint of
40 per cent that was never a fact, steps 3, 5 and 6 not started. Nothing it built
had ever run on a radio, which is why step 0 of this phase exists.

## Entries

## UNIT 249 - STEP 0
APPROACH: wire Ft8Reader to Ft8DeepSlotDecoder with fine sync and ordered statistics on, record the decoder's identity on the per-slot census, and add an off-by-default comparison against the port
ACCOMPLISHED: closed step 0 before this phase was installed, under the previous phase's paused sequence. Task 1 found that Ft8Reader called Decode(Ft8Waterfall), an overload that hands Deep's loop an empty span - fine sync refused 42 of 42 candidates for want of samples, so a Hamlet wired the obvious way would have paid 2.1 times the port's cost for none of the off-grid gain. Through the samples entry point fine sync re-synced 42 and accepted 14. One slot costs 261 ms of a 15,000 ms budget, 1.74 per cent. From the same 55 candidates Deep carries 27 through to text where the port carries 9. Every message returned is re-checked through the port's own gates, 0 wrong. Every sidecar and ft8_slot row now names the decoder and its stages, and an unrecorded decoder says so rather than naming the port. Root 1.12.50 to 1.12.51; Ft8Sharp untouched at 0.10.7.
COST: not recorded
FATE: executed

