PHASE: Everything this project has built reaches the operator's screen, and the decoder is taken as far as it will go
PHASE_SET: 2026-09-05
STEP: 0 | done | Hamlet decodes through Ft8Sharp.Deep
STEP: 1 | done | the gate set exists, and the slow tests are named
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


## UNIT 1 - STEP 1

STEP: 1
APPROACH: not recorded
HIT: section 4 wants a ruling: no - The section declares nothing blocking and the one item raised expressly asks for no ruling, while the outstanding asks list is a carried forward register of earlier items, several already parked out of phase by the plan, so no decision is requested for step 1.
MOVE: continue
WHY: not recorded
DECIDED: none
LICENCE: none
COST: 30.093965499999992
ACCOMPLISHED: not recorded
FATE: executed
STATE_AFTER: not started
STATE_WHY: The report describes wiring Hamlet to Ft8Sharp.Deep and shows no per test duration ranking, no committed docs slash gate set file, no measured gate set runtime and no command that runs only the gate set, so none of the step's exit criteria are touched.

## UNIT 1 - STEP 1

STEP: 1
APPROACH: write the breakage record first from PHASE_OUTCOME.md and the archived outcome and RUN_LEDGER.md and the fix commits - then let the gate set be nothing but the tests that guard those breakages - verify every name against the tree by reading and run no test at all
HIT: the two claims the earlier attempt at this instruction left in docs/gate-set.md cannot be supported by the tree. It said the whole command measured 2 m 39 s and that the set had been watched failing against a deliberately broken property. There is no gate-*.trx anywhere in .run-unit/trx - that session was killed by the watchdog at 13:47 with its report never committed - and PROJECT_STATUS.md at 13:35 says in its own words that the wall clock could not yet be measured. Both were removed rather than carried forward. What the same session did leave is real and was used: per-test durations for 27 of the 29 cases in TRX files written between 12:03 and 12:31. Also hit - gate-set.bat was written with LF line endings by a file-editing tool while every other .bat in the folder is CRLF - cmd.exe resumes a running batch file by byte offset and this one has a call and goto loop over four projects. It is now CRLF. Also hit - the instruction names three watchdog kills at 12:02 and 13:09 and 13:47 - the ledger holds kills at 01:32 and 12:02 and 13:09 and the 13:47 line is the halt that followed the 13:09 kill. Three kills is right and one of the three clocks is not.
MOVE: continue
WHY: step 1 was judged not started because no unit had produced a committed gate set - the entry above it judged unit 249's report against step 1's exits and found none of them touched. The step needs no measurement and this instruction says so - each entry is sourced from the record rather than from a stopwatch - so it closes on reading and file edits alone under the ruling that a unit runs no suite.
DECIDED: three. First that docs/breakage-record.md is written as its own file rather than left inside the report - output.md is overwritten every unit and the evidence every future gate-set entry must cite cannot live somewhere that does not survive the night. Second that the earlier attempt's unsupported measurements are removed rather than softened - a wall clock nobody can find a TRX for is not a slower measurement than it should be - it is not a measurement - and the whole-command figure is stated as a three to four minute estimate and marked as one. Third that the closing message of gate-set.bat and the advice at the foot of docs/test-baseline.md were both corrected where they told the next session to run the channels it touched - that is HM-DEC-154's sentence and HM-DEC-155 supersedes it.
LICENCE: PHASE_PLAN.md step 1 and its five must-pass exits - docs/gate-set.md committed with the full name and the property and the breakage and the unit number for every entry - an entry that cannot name a breakage not being in the set with that rule written into the file - a command in tools/arbiter that runs exactly the gate set and nothing else that no unit runs - the standing rules recorded in the same file - and each entry sourced from the record rather than from a stopwatch. Work instruction 250 tasks 1 to 5. Tim's rulings of 2026-09-05 that a unit runs no test suite and may run only the test it constructs and never backgrounds and polls - recorded this unit as HM-DEC-155. CLAUDE.md 13.3 on PROJECT_CARD.md changing only by ruling - already carried by HM-DEC-154.
COST: unknown
ACCOMPLISHED: this project can now say which tests must run before a change is believed - and every one of them names a thing that actually broke. docs/breakage-record.md lists 13 breakages a test caught or would have and 7 that no test could have caught - and the second list is the more useful one. docs/gate-set.md is 27 test methods and 29 cases over four projects covering eight properties - Deep is a whole-result superset of the port - the port's parity and CRC-14 gates are in HAMLET's path and not merely in the sibling's - Ft8Sharp references nothing outside itself so an MIT library never depends on a GPL-3.0 one - the ladder returns nothing that was not sent - a stage may only add - the five-count census reaches all three surfaces - a capture names the decoder that read it - and one slot decodes inside 15 seconds. All 27 names were opened in the tree and every one exists - and where a filter names a class the class was counted method by method. The measured per-test cost is 117.9 s over 27 of the 29 cases against 856 s for Ft8Sharp.Tests alone and an engine project that has never finished. NO TEST WAS RUN THIS UNIT AND THE GATE SET SCRIPT WAS NOT EXECUTED ONCE - it was verified by reading in both directions.
FATE: executed
STATE_AFTER: done
STATE_WHY: all five must-pass exits of step 1 are met by files in the tree. docs/gate-set.md is committed with the full name and the property and the breakage and the unit number for every entry. The rule that an entry naming no breakage is not in the set is written into the file and into HM-DEC-155. tools/arbiter/gate-set.bat runs exactly the gate set and nothing else and was not executed by this unit. The standing rules are in the same file - no suite - only the test the unit constructs - no backgrounding and polling with the three kills of 2026-09-05 named - no test without its breakage - and the inherited known-reds in one table. Every entry is sourced from PHASE_OUTCOME.md and the archived outcome and RUN_LEDGER.md and the fix commits rather than from a stopwatch.
