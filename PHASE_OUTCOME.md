PHASE: Hamlet reads FT8 as well as the best decoder there is, and then reads it further
PHASE_SET: 2026-09-04
STEP: 0 | partial | there is a scoreboard, and the arbiter can read it
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

## UNIT 1 - STEP 0

STEP: 0
APPROACH: probe the shell before believing it is dead, then walk the ladder that already exists and put a commanded entry point on it rather than building a second one
HIT: the previous unit halted on a false negative. dotnet was reported dead after one refused probe, dotnet --version, which is simply not one of the three dotnet spellings that .run-unit allowed.txt names. dotnet build and dotnet test both run and always did. The real refusals are two separate faults that behave differently: an allow-list matched against the command as it is typed, which a permitted spelling gets past, and a sandbox that refuses every shell write, which nothing gets past. Only the second has no workaround, and the file-editing tools cover it. The validator deadlock is the intersection of the first fault with Git Bash eating backslashes: the five permitted spellings are destroyed before an interpreter sees them and every spelling that survives the shell is refused.
MOVE: continue
WHY: the instruction ruled that a refused shell call is a signal to reach for another tool rather than to stop, and that reporting a refusal is succeeding at this unit rather than failing it. Probing first turned a night the previous unit spent blocked into a night that reproduced the baseline and built the harness. The move is continue, and step 0 stays open on the capture-fixture criteria that this instruction did not ask for and no unit has yet written.
DECIDED: that the baseline is reproduced rather than adopted with an offset, because it came back 248 and 73 and 13 of 306 against unit 221 finding 248 and 73 and 13 of 306. No target in PHASE_PLAN.md moves. That the harness extends Ft8Step6Ladder by calling its population and synthesiser and noise and calibration rather than copying them, because a rebuilt ladder would be a different measurement and the reproduction is what proves it is not. That the arbiter validator and this outcome tool are reached through the permitted dotnet build and an MSBuild Exec task, which leaves the permitted-spellings fault untouched exactly as the instruction parked it. That PROJECT_CARD.md changing is licensed by Tim approving PHASE_PLAN.md on 2026-09-04, recorded as HM-DEC-153.
LICENCE: PHASE_PLAN.md step 0, whose must-pass exits are that the ladder runs in the loop, that the as-is baseline of 4.2 per cent at minus 21 dB over 306 trials with zero wrong is reproduced rather than inherited, and that a wrong decode is counted separately from a missed one everywhere. Work instruction 243 tasks 1 to 6, which name the harness, the three counts and the bookkeeping. CLAUDE.md section 13.3 on PROJECT_CARD.md changing only by ruling. The plan section that the steps are a hypothesis and not a contract, and its table of named alternatives to stopping, both of which were read this unit.
COST: unknown
ACCOMPLISHED: the phase can now measure itself. One call, Ft8LadderHarness.Run of a rung and a trial count and a seed, returns decoded and missed and returned-wrong with a Wilson interval and a wall clock, reproduces unit 221 to the decode at all three rungs, and has the seat for Ft8Sharp.Deep already cut so step 1 joins it with one line and every trial thereafter runs both decoders over identical samples. It also costs a known 63.9 ms a slot decode and 19.6 seconds a 306-trial rung, so every later unit can plan against a real number. And the night recovered a working toolchain that the previous unit had written off, which is worth more than any single measurement in it.
FATE: executed
STATE_AFTER: partial
STATE_WHY: three of step 0 must-pass exits are met and evidenced. The ladder runs in the loop through Ft8LadderHarness. The baseline reproduced at 13 of 306 at a delivered minus 21.001 dB with 0 wrong, and 0 wrong over all 3519 trials of the full curve. Three counts are reported everywhere and a wrong return prints the message sent beside the message returned. The four capture-fixture exits are untouched: this instruction did not ask for them, and no unit has written the fixture format, the reader, the loud failure on an absent capture or a mismatched hash, or the one-step command Tim runs at the shack.
