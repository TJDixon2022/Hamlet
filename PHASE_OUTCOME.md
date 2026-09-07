PHASE: Hamlet works stations on the air
PHASE_SET: 2026-09-07
STEP: 0 | done | the record is honest about where the phase stands
STEP: A | done | the row knows where the contact stands
STEP: B | done | right-click and it goes
STEP: C | not started | the whole chain runs from one click, at the bench
STEP: D | not started | the drive level his radio wants
STEP: E | not started | Tim works a station
STEP: 1 | not started | (described by the plan)

---

## What this file is

The phase's memory. `output.md` is overwritten every unit and cannot carry what
was tried; this file survives the unit and records, per unit, the approach taken
and what it hit.

**The header above is a cursor over the entries below, and the entries win.**

Appended by `tools\arbiter\outcome-append.bat`. Five state words and no others:
`not started`, `in progress`, `partial`, `blocked`, `done`.

**The previous cut's entries are archived at `docs/phase-send-run/PHASE_OUTCOME.md`
and each unit appears there twice** - once by its real number and once by the
loop's iteration number, which is a bug in `outcome-append.bat` that step 0 fixes.
Read them as one unit each.

What that cut built, and it is not to be rebuilt: the dummy-load rule removed, the
abort proved from six states, the FT8 composer, the sound card route at the rate
the card speaks, the stop button that took 8.4 seconds of audio off the air mid
transmission, the loopback reading 3 of 3 messages back as themselves, and the
transmit level moved from full scale to -12.04 dBFS with a control and a readout.
**What it could not close was the level his own radio wants, which is now step D.**

## Entries

*None yet. This cut has not started.*

## UNIT 266 - STEP 0

STEP: 0
APPROACH: Close the previous cut's steps at the figures that actually closed them, in both archived header lines and nowhere else, then find the duplicate-entry bug at the one place both numbering routes pass through and fix it there rather than in either caller.
HIT: Two things the instruction did not name. The first is that the pairs are identifiable without guessing: a unit does not know what its own run cost, so every entry a unit wrote for itself reads COST unknown and every entry the loop wrote carries the figure it read out of last-run.json, which splits all twenty-six entries into thirteen and thirteen. The second is a second bug in the same script, found by reading before writing: the header updater matched a step with the pattern [0-9]+, and the re-cut's steps are letters, so a call for step A matched nothing and would have appended a SECOND STEP: A line beside the one already in the header - a header listing one step twice, in two states, with no way to tell which is the position.
MOVE: continue
WHY: Neither caller of outcome-append.bat is wrong about its own number and neither can see the other - run-unit.bat:534 passes the work-instruction number and run-phase.bat:373 passes 1, the loop's iteration counter set at :127 and incremented at :171 - so a fix in either one leaves the other free to write a second entry. outcome-entry.py is the single place both routes reach the file, so the number is resolved there, from WORK_INSTRUCTIONS.md's own heading, which is the launcher's one authoritative answer to which unit is running.
DECIDED: Three on my own authority. First, a second append for the same unit and step is folded into the first entry as a ### continuation naming only the fields that differ, rather than being dropped: the loop's entry is the one carrying the run's real cost and the arbiter's judgment, and an entry that silently discarded those would be a worse record than the duplicate it replaces. Second, the resolution is a tie-break and not a takeover - with no instruction to read against, the caller's number stands, OA_UNIT_EXACT turns it off, and where the resolved number differs the entry says UNIT_AS_CALLED on its face - because relabelling somebody's hand-written historical entry would be the same class of fault one direction over. Third, the letter-step regex was fixed as part of this step rather than reported, because appending this very entry would otherwise have corrupted the header it was meant to update.
LICENCE: PHASE_PLAN.md step 0's own exit criteria, which name the figures, name the duplicate as a bug in outcome-append.bat, and require the existing duplicates to be left in place and named. Its named alternatives to stopping licence using the file-editing tools where the shell refuses a call, which it did once.
COST: unknown
ACCOMPLISHED: The record says what happened. The two steps that carried the send phase's real work read done at the figures that closed them - the loopback at 3 of 3 messages, the level at -12.04 dBFS, the device by unit 256 and the rate by unit 262 - and both files say in their own words that the one thing left open in them was the level Tim's own radio wants, which is now step D and is his. Every unit of that phase appears twice in the archive and a reader is now told so in a table, with the evidence for which route wrote which entry, instead of counting thirteen units as twenty-six. And it cannot happen again: the same unit appended twice under both numbering routes now produces one entry, watched failing first.
FATE: executed
STATE_AFTER: done
STATE_WHY: All three must-pass criteria are met. Steps 2 and 3 read done in both archived header lines with the figures and with a section beside each saying what was open and where it went. The duplicate is fixed at outcome-entry.py with the red quoted - expected ONE entry heading, got 2 - and green at 3 of 3 by exact name, with the existing duplicates left in place and named in a table. The archived PHASE_PLAN.md was not touched at all.

## UNIT 266 - STEP A

STEP: A
APPROACH: Find what of the previous cut's step 4 survives before building anything, then write only the proof it lacks - one station read at every slot of a whole six-message exchange, walking through all four states in the order they happen.
HIT: The step was very nearly already met. Ft8ContactLedger, Ft8ContactStates with the plan's four words and an IsComplete that never asks for 73, the Contact column bound on the decoded row, and three of the four tests this instruction names were all in the tree and green as they stand. What was missing was not a state but the TRANSITIONS: every state had a case of its own and no case read one station through all four, so a row that reached a state and stuck there would have passed every test in the tree, and the way back OUT of gone quiet was asserted nowhere. The band scene could not carry the walk either - no station in it passes through all four, and gone quiet cannot follow complete by design, so the only place a walk can pass through gone quiet is in the middle of an exchange.
MOVE: continue
WHY: The instruction's own first order was to find what of unit 264's work survives before building anything, and the answer is most of it. Rebuilding a ledger that already holds what passed each way, already says the four words with their slot counts and already refuses to close anybody's contact would have been a night spent producing a second copy of a working thing. What the step actually lacked was one piece of evidence, and that is what was built.
DECIDED: Two on my own authority. First, a second recorded scene was composed rather than the band scene extended: the band scene is at its twelve-slot cap, every other case reads it, and regenerating it would mean running a test this unit did not construct. Second, the compose-and-decode helpers in the new test are a deliberate second copy of the band scene generator's, not a shared refactor, because sharing would mean editing a test this unit may not run to prove the refactor safe - and it is written down in the new file that the two are worth joining by a unit that can run both.
LICENCE: PHASE_PLAN.md step A's four exit criteria and its instruction to check what survives before rebuilding, together with the ruling that a contact is never closed by the app and that 73's absence never withholds complete. Section 12.1 licenses the completeness rule as bookkeeping over the shapes of the payload fields and not as a reading of what any station meant.
COST: unknown
ACCOMPLISHED: Every decoded row on the Digital tab says where its contact stands, in four words with a count of slots beside each, and it is now proved through a whole contact rather than at four separate moments. A station that answers, disappears for ninety seconds and comes back reads your move, then waiting on him, then gone quiet with the count rising slot by slot, then your move again the moment he is heard, then complete on his acknowledgement - one slot BEFORE the 73 arrives, because complete never waited for one. Nothing is closed, hidden or forbidden anywhere in it: a complete contact still shows and a gone-quiet one still shows. The app reports where the contact stands; it does not rule on it.
FATE: executed
STATE_AFTER: done
STATE_WHY: All four must-pass criteria are met and every test named in the instruction is green, each run alone by exact name. Criterion 1 and criterion 3 by the ledger and the completeness rule with the no-73 exchange quoted message by message; criterion 4 by the interleaved station read at all thirteen slots and never once gone quiet, against the recorded multi-slot scene; criterion 2 by the four states with their slot counts, whose on-screen half is the Contact column unit 258 shipped and whose committed app-side test this instruction forbids running, so it is claimed on the tree and not on a run tonight. The new walk was red first with the scene absent and green on its first run, all twelve expectations written before they were read, against a scene composed by Hamlet's own encoder and read back through the decoder Hamlet actually runs: 6 composed, 6 decoded, 0 lost.

## UNIT 266 - STEP 1

UNIT_AS_CALLED: 1
STEP: 1
APPROACH: not recorded
HIT: section 4 wants a ruling: no - The section records a fix already made, a caveat on a claim, a note for a later unit, and untouched inherited failures, and none of these ask the owner to decide anything or stop work pending his answer.
MOVE: continue
WHY: not recorded
DECIDED: none
LICENCE: none
COST: 13.577231000000003
ACCOMPLISHED: not recorded
FATE: executed
STATE_AFTER: not started
STATE_WHY: The live plan carries no step 1, and the report shows work only on steps 0 and A, so nothing has been done toward the step being judged.

## UNIT 267 - STEP B

STEP: B
APPROACH: Measure the four criteria of step B against what the previous cut already shipped, then build only the proof missing - an out-of-privileges click driven through the application to a slot boundary, nothing keyed and the Send area saying so - and close the three on-screen criteria on a run tonight instead of on committed markup.
HIT: The instruction's expectation was right and the seam was genuinely empty: RefusedByLicence appeared nowhere in tests/Hamlet.App.Tests at all, and MainWindowViewModel.cs:8346's branch - the sentence an operator reads when the gate refuses his click - was asserted by nothing. What was NOT expected is the guard-off measurement. Switching RestrictTransmitToPrivileges off in Settings does not let the transmission through: Ft8TransmitSequence.Permits:443 accepts one of the gate's three ways of permitting and refuses an overridden permit outright, so the operator gets a different refusal rather than a transmission. Four of the five assertions were green whole on their first run; no product code was needed and none was written.
MOVE: continue
WHY: Step B had had no unit spent on it, and the previous cut built the menu, the CQ button and the Send area without ever joining the licence gate to the operator's click. The gate was proved inside Ft8TransmitSequence.RunAsync on a sequence a test constructed, and the menu was proved to say so on a panel nobody clicked. Nothing drove his own click, with his own licence class and his own guard setting out of Settings, to a slot boundary.
DECIDED: Two on my own authority. First, the guard-off case is recorded as a measurement and left exactly as found, because work instruction 267 said to record it and not to decide it, and because changing it would mean editing the keying path's own gate. Second, the Send area line after a successful send was measured by adding a print and an addressee assertion to the test task 2 built, rather than by running an eighth committed test, because the seven named tests do not carry that string and the no-suite rule permits only those seven and what task 2 builds.
LICENCE: PHASE_PLAN.md step B's four exit criteria; its rule that a bench step's criteria are all satisfiable with no radio and none deferred to Tim; the ruling that nothing is forbidden in the menu and a repeat shows its count; and the third of the three things no unit may reason past - the Settings gate is not bypassable from any send path. Work instruction 267 task 3's named, bounded exception to the no-suite rule licensed the seven committed tests, each run alone by exact name.
COST: unknown
ACCOMPLISHED: Tim can right-click a station he has decoded and send it a message with one click, with the one that conventionally comes next marked and nothing taken away from him, and he can call CQ from his own callsign and grid without typing. The Send area tells him what went out and to whom. And on a frequency his licence does not cover, Hamlet says so in the regulator's own words, cites the paragraph, names the message that did not go, and transmits nothing - proved from his click through the application to a slot boundary, with zero bytes at the port and the sound card never touched, rather than from a test that built its own transmitter.
FATE: executed
STATE_AFTER: done
STATE_WHY: All four must-pass criteria are met on evidence quotable from runs tonight, and none of them needed the radio. Criterion 1 by TheCqButtonSendsFromSettingsWithNoTypingAndNeverInventsAGrid, run alone: CQ KC3QIS FN00 with a grid set and CQ KC3QIS without, and no locator invented. Criterion 2 by four tests run alone, including EveryStationsPredictedMenuAppearsUnderTheMouse matching 5 of 5 stations' header strings on a real ContextRequested with every item enabled, and TheRepeatCountBelongsToTheClickAndNotToTheRow moving "73" to "73, 2nd time". Criterion 3 by OneClickIsOneMessageAcrossTwoBoundaries: first boundary Ran, second NothingArmed, 1 sink call, 2 port frames. Criterion 4 by the new TheLicenceGateHoldsFromTheClickTests, five assertions green: Technician on 14.074 through SendMessageCommand to AtSlotBoundaryAsync gives RefusedByLicence with Sent and Keyed both false, 0 sink calls and 0 bytes at the port; the same click as General sends, 1 sink call and 2 frames, so it is a gate and not a dead path; and the Send area reads - Hamlet did not send "W1ABC KC3QIS -10": Technician privileges do not reach this frequency; it needs General. (97.301(e))
APPENDED_BY: the file-editing tools, not outcome-append.bat. Both invocation forms were refused by this session's shell with the same verbatim message - "This command requires approval" - so work instruction 267 task 4's named alternative was taken and the header's STEP: B line was updated in place by hand, in the format the existing entries use.
