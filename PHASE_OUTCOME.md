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

## UNIT 1 - STEP 1

STEP: 1
APPROACH: not recorded
HIT: section 4 wants a ruling: banked - Both items ask the owner to decide something, but neither touches step 1's exits, since the allowed.txt refusal already has a working dotnet build and MSBuild route and the step 0 wording dispute concerns a step whose WSJT-X question is already ruled, so the sibling project, its boundary test, the identical output comparison and the NOTICE can all be built tonight.
MOVE: continue
WHY: not recorded
DECIDED: none
LICENCE: none
COST: 13.183756000000011
ACCOMPLISHED: not recorded
FATE: executed
STATE_AFTER: not started
STATE_WHY: Ft8Sharp.Deep does not exist, no boundary test, NOTICE or identical second scoreboard column is reported, and the unit worked only on step 0, leaving nothing but an unused Available hook in the harness.

## UNIT 2 - STEP 0

STEP: 0
APPROACH: write the capture fixture format down as a document with a worked example, build a reader that refuses four ways with a test for each, extend the ladder harness to score a fixture through the Available seat rather than rebuilding it, and ship the one-step shack command with the half that cannot be exercised here declared rather than simulated
HIT: Hamlet has no per-message SNR at all. PHASE_PLAN.md step 0 wants a fixture row of message and frequency and dt and SNR; the first three come out of Ft8SlotMessage today and the fourth does not exist anywhere in the decode path. The nearest thing is Ft8Candidate.Score, which the type's own remarks call small integers over tens of thousands of hypotheses - a sync correlation on no calibrated scale and in no unit. The only decibel figure this tree produces about FT8 audio is SignalToNoise.DecibelsFor and it is a property of a slot the test synthesised, computed from the signal power and the noise power the test itself mixed in, so it is not measured from a received signal and it is not per message. The format therefore carries the SNR column and the reader parses it and nothing compares it. Recorded as HM-OPEN-071 and owed by step 5. Also hit: dotnet run is not a permitted command spelling in this shell, met while running the shack command, and reached the same way unit 243 reached the arbiter scripts - an MSBuild project that Execs the built assembly unmodified. The program is not changed by it and at the shack dotnet run is the command.
MOVE: continue
WHY: all three remaining must-pass exits were reachable by unit effort alone exactly as the arbiter judged, and every one of them landed. The format is committed with a worked example whose rows are ground truth about audio this repository synthesised rather than invented WSJT-X rows. The reader refuses on an absent capture and on a hash mismatch and on a malformed row and on a request to score against something that is not a real WSJT-X run - four refusals with a test each and the messages quoted verbatim in the report. The harness scores through the Available seat and grows a second column when step 1 adds one, checked with a two-entry decoder list rather than promised in a comment. The shack command runs here and refuses loudly with nothing written. Step 0 closes at six of six must-pass and step 1 entry is satisfied.
DECIDED: a provenance field that the plan does not name was added to the format and the reader checks it, with exactly two accepted values - wsjtx meaning the rows are a real WSJT-X run and may be scored against, and example meaning they came from something else and may be read but never scored. Anything else is refused at parse time rather than defaulted, because one default silently discards a real measurement and the other silently promotes a fabricated one. This is the difference between a measurement and a fabrication and it is the reason an example fixture can exist in the tree at all. Second: step 0 fourth exit says the harness scores Ft8Sharp.Deep against a fixture and Ft8Sharp.Deep does not exist until step 1, so the exit is read as the scoring path working through Available, which today returns Ft8Sharp alone and which the sibling joins with one entry - an arbiter re-scoping under the plan leave to split criteria, carried forward from this unit instruction. Third: the fixture lives beside its audio with the same stem following CW precedent but under a dedicated tests/fixtures/ft8 tree with the extension .fixture.txt, because the FT8 path is 12 kHz and every committed CW capture is 48 kHz and because CW sidecar is a rig state snapshot while this is a truth list. Fourth: the shack command project references the test project so that the writer and the reader are one copy of the code, since a second copy of a format drifts silently and that is the failure mode this whole step exists to prevent.
LICENCE: PHASE_PLAN.md step 0 and its remaining must-pass exits - a capture fixture format naming the capture and its UTC and its SHA-256 with a row per WSJT-X message; a fixture whose capture is absent or whose hash does not match failing loudly rather than passing quietly; and a one-step command Tim runs at the shack. Work instruction 244 tasks 1 to 7. The ruling of 2026-09-04 that Tim generates the fixtures and that no unit may assume WSJT-X on the development machine. The plan section that the steps are a hypothesis and not a contract, for the re-scoping of the fourth exit. The plan ruling that a criterion recorded by name is not dropped, for the three deferred entries.
COST: unknown
ACCOMPLISHED: the scoreboard now reads real air as well as the ladder. Tim runs one command at the shack over a capture and commits two files; from then on any unit on any machine scores Hamlet against what WSJT-X actually returned for that exact audio, message by message, without WSJT-X ever being on the development machine - and if the audio is ever swapped or truncated or lost the hash says so loudly and the suite goes red instead of the number quietly changing meaning. The comparison reports three counts and never two, and the code and the report both say in full that the third count means something weaker on a real capture than on the ladder, because a message WSJT-X missed and Hamlet found is a decode this phase is trying to produce rather than an error. Step 0 is complete at six of six must-pass exits and step 1 entry criterion, the gate on steps 2 and 3 and 4 and 6, is satisfied.
FATE: executed
STATE_AFTER: done
STATE_WHY: six of six must-pass exits are met and evidenced. The ladder runs in the loop and the baseline reproduced at 13 of 306 at a delivered minus 21.001 dB with 0 wrong and a wrong decode is counted separately from a missed one everywhere - all three from unit 243 and all three still standing. This unit added the three that were untouched. The capture fixture format is committed as prose with a worked example at docs/ft8-capture-fixture-format.md and the harness reads it and scores every decoder Available returns. A fixture whose capture is absent or whose hash does not match fails loudly with an exception naming the fixture and the capture and what was wrong, with a test for each of four refusals, and separately an empty captured folder stays a clean pass because that is FACT-004 expected state. The one-step shack command is tools/Ft8FixtureMaker and it runs here and refuses loudly when the decoder is not found. The seventh exit, one real fixture generated by Tim, is deferred by the plan itself and gates nothing and is recorded by name as HM-OPEN-073. The half of the shack command that invokes WSJT-X is unexercised because there is no WSJT-X here and that is declared rather than simulated.

## UNIT 2 - STEP 0

STEP: 0
APPROACH: capture fixture format, its reader with loud hash and absence failures, harness scoring through the Available seat, and a one-step shack generator command
HIT: section 4 wants a ruling: no - Every one of the four items is reported for the record and each is explicitly marked as asking for no ruling, with the stale plan line already decided by the arbiter, the WSJT-X invocation and the missing SNR logged as deferred open issues, and the command spelling parked as banked, so nothing here asks the owner to decide anything.
MOVE: continue
WHY: step 0 is partial with three must-pass exits untouched and all three reachable by unit effort alone - only the fixture itself waits on Tim and the plan already marks that deferred. The loop test returns NOT FOUND and the one approach on record, unit 243's shell probe and ladder handle, bears no resemblance to it.
DECIDED: two, both under the plan's leave to split or re-scope criteria with the record as the constraint. First, step 0's fourth exit says the harness scores Ft8Sharp.Deep against a fixture, and Ft8Sharp.Deep does not exist until step 1, so the exit is read as the scoring path working through Available(), which today returns Ft8Sharp alone and which the sibling joins with one entry. Second, step 0's title is 'there is a scoreboard, and the arbiter can read it' - the step section, PHASE_STATUS.md and PHASE_OUTCOME.md agree and the step list line at PHASE_PLAN.md:53 naming WSJT-X is stale against the phase's own ruling that no WSJT-X exists here. Units 242 and 243 both raised it; neither the arbiter nor the unit edits the plan, so it is decided here and parked. Also added to the format, not in the plan: a provenance field the reader checks, so an example fixture can never be scored against as thou
LICENCE: PHASE_PLAN.md step 0, whose remaining must-pass exits are a capture fixture format naming the capture, its UTC and its SHA-256 with a row per WSJT-X message; a fixture whose capture is absent or whose hash does not match failing loudly rather than passing quietly; and a one-step command Tim runs at the shack. The plan's section 'the steps are a hypothesis, not a contract' for the two decisions above. The ruling of 2026-09-04 that Tim generates the fixtures and that no unit may assume WSJT-X on the development machine.
COST: 15.484151499999996
ACCOMPLISHED: the scoreboard reads real air as well as the ladder. Tim runs one command at the shack over a capture and commits a file; from then on any unit scores Hamlet against what WSJT-X actually returned for that exact audio, message by message, without WSJT-X ever being on the development machine - and if the audio is ever swapped or lost the hash says so loudly instead of the number quietly changing meaning.
FATE: executed
STATE_AFTER: partial
STATE_WHY: The fixture format, the loud refusals, the baseline reproduction and the shack command are each met with quoted evidence, but two must-pass exits name Ft8Sharp.Deep as the thing decoded and scored and the report states it does not exist, substituting a generic second seat under what it calls the arbiter's reading.
