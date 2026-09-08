PHASE: Hamlet works stations on the air
PHASE_SET: 2026-09-07
STEP: 0 | done | the record is honest about where the phase stands
STEP: A | done | the row knows where the contact stands
STEP: B | done | right-click and it goes
STEP: C | done | the whole chain runs from one click, at the bench
STEP: D | done | the drive level his radio wants - 25 per cent, -12.04 dBFS, ALC -2.0 to -1.5 inside the red zone, on the IC-7300 at 14.074 MHz. The evidence is SHACK_FACTS.md FACT-005, written by unit 271 from the operator's own reading at the radio on 2026-09-07, and no unit may promise to defer that number again.
STEP: E | not started | Tim works a station
STEP: 1 | in progress | (described by the plan)

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

### ALSO RECORDED FOR UNIT 267 - STEP B

A second append for the same unit and the same step, called as UNIT 2.
One unit is one entry, so what this route recorded is folded in here
rather than written as a second entry. Only what differs is listed.

APPROACH: Measure the four criteria of step B against what the previous cut already shipped, then build only the proof missing - an out-of-privileges click driven through the application to a slot boundary, nothing keyed and the Send area saying so
HIT: section 4 wants a ruling: banked - The only ruling asked for is whether the send path should keep refusing when the licence guard is switched off, a wording question the unit answered and left untouched, and every one of step B's four exit criteria is already met and recorded done on runs tonight, so the owner's answer forecloses no work on this step.
WHY: Step B has had no unit spent on it and the loop test found no resembling approach; the previous cut built the menu, the CQ button and the Send area but never joined the licence gate to the operator's click, which is the one criterion of the four with nothing behind it.
DECIDED: Two on my own authority. First, this unit may run seven named committed tests as well as the one it builds, each alone by exact name and foregrounded - because three of step B's four criteria are on-screen, and step A closed last night with a section 4 caveat saying its on-screen half was claimed on committed markup rather than on a run. The rule that bans suites and polling is not widened: no suite, nothing unfiltered, nothing backgrounded, and nothing outside the seven. Second, the goal task is criterion 4's second half rather than the CQ button or the menu, because the gate is the one of the three things the plan says no unit may reason past that is currently proved only on either side of the seam and never across it.
LICENCE: PHASE_PLAN.md step B's four exit criteria; its rule that a bench step's criteria are all satisfiable with no radio and none deferred to Tim; the ruling that nothing is forbidden in the menu and a repeat shows its count; and the third of the three things the arbiter may not reason past - the Settings gate is not bypassable from any send path.
COST: 12.125338
ACCOMPLISHED: Tim can right-click a station he has decoded and send it a message with one click, with the one that conventionally comes next marked and nothing taken away from him, and he can call CQ from his own callsign and grid without typing. He is told in the Send area what is going out and to whom. And on a frequency his licence does not cover, Hamlet says so in the regulator's own words and transmits nothing - proved from his click, not from a test that built its own transmitter.
STATE_WHY: Each of the four must pass criteria is backed by a named test run alone with quoted output, the CQ text built from Settings, the menu under the mouse with its highlight and repeat count and every item enabled, one click yielding one message across two boundaries, and the Send area lines for both a successful send and an out of privileges refusal with nothing keyed, and the single open ruling concerns only the guard off wording, which no criterion depends on.

## UNIT 268 - STEP C

STEP: C
APPROACH: Join the loopback endpoint to a real right-click in one Avalonia headless test, with the host question - whether a headless window can hold an open WASAPI render endpoint and a loopback capture - built and answered first, before the goal task, with a fallback written in advance. Two transmissions in the class: one clicked and left alone, decoded back to the text on the menu item that was clicked, and one clicked and stopped from the real Stop button part way through, with the card measured going quiet.
HIT: Three things. The first is the good one: THE TWO HOSTS JOIN, on the first run, and the fallback was not taken - a real MainWindow shown in the Avalonia headless session opened a real WasapiTransmitSink on S34J55x display audio at 48000 Hz, held a WasapiLoopbackCapture on the same endpoint, awaited a whole 12.64 s slot and the card made a sound at -12.0 dBFS. Unit 267 named that the most expensive thing step C needs and it cost one build. The second is the trap the trace found before anything was written, and it is not the one unit 267 named: StopNow's abort goes to _rigPort, which BuildTheArmedSend never assigns, so a Stop pressed in the loopback harness would have fired StopNow(null), taken no abort at all, and left criterion 3's carrier half measuring nothing while looking green. The third was found the hard way and cost two watched reds: an open MenuFlyout is a popup with a light-dismiss layer over the window, so the Stop press landed on that layer instead of the button and a real 12.64 s transmission went out of a real card whole. The first diagnosis of that red - the window being too short - was WRONG and is recorded as wrong; resizing did not fix it. It is a harness fidelity fault and not a product defect: in the running application the flyout closes itself when an item is clicked.
MOVE: continue
WHY: Step C had had no unit spent on it, its entry steps A and B were both done, and every piece it needs already existed in the tree with a file and a line. What had never happened was joining them, and the seam nobody had measured is whether the text on the menu item the operator clicked is the text that comes back out of the sound card - a menu that offered the wrong string would have passed every test in the tree.
DECIDED: Two on my own authority, both inside the instruction's own licence. First, the expected string is read off the realized MenuItem's CommandParameter rather than out of vm.SendMenuFor a second time, because reading the view model again would compare the send path against the same source that fed it, while CommandParameter is what the markup's own handler hung on the thing the mouse hits. Second, silence is measured as the peak absolute sample across the window in dBFS against the tree's own AudioLevel.TooQuietDb of -60, with the capture's packet count carried separately as a liveness witness - because a WASAPI loopback is handed audio only while something renders, so "the capture is still running" and "the endpoint rendered nothing" are different facts, and a first version that conflated them was the night's first watched red.
LICENCE: PHASE_PLAN.md step C's four exit criteria, which name the loopback endpoint and the fake transport in the criterion itself; its rule that a bench step's criteria are all satisfiable with no radio and none deferred to Tim; its named alternatives to stopping, including the file-editing tools where the shell refuses; and SHACK_FACTS.md FACT-004, under which the device is a sound card and the port is FakePort. Work instruction 268 task 2's named, bounded exception to the no-suite rule licensed exactly two committed tests, each run alone by exact name.
COST: unknown
ACCOMPLISHED: The message Tim clicks is the message that leaves the machine, and it is proved by decoding the sound off his own card back into text and matching it against the menu item he clicked rather than against a string a test chose for itself. His Stop button takes a real transmission off a real card from the middle of that chain - 4.71 s of a 12.64 s slot went out and the card was quiet 456 ms after the press. Nothing goes out that he did not click, measured as a silent card across a boundary nobody clicked rather than as an untouched counter. That is the last thing about Hamlet's transmit side that can be established without a radio.
FATE: executed
STATE_AFTER: done
STATE_WHY: All four must-pass criteria are met on evidence quotable from runs tonight, and none of them needed the radio, so nothing from step C is deferred to Tim and nothing of it belongs in step D or E. Criterion 1 BY THE RAISED RIGHT-CLICK AND NOT BY THE FALLBACK: a real ContextRequested on the real row control in the real window, the menu item invoked through its own Command and CommandParameter, composed at 48000 Hz, keying and unkeying frames read as hex - FE FE 94 E0 1C 00 01 FD | FE FE 94 E0 1C 00 00 FD - 606720 of 606720 samples played at peak 0.2500 with 0 clipped, and one ft8_transmission line on disk through the application's own enabled-category predicate. Criterion 2, the assertion the step exists for: CLICKED "W1ABC KC3QIS RRR", DECODED "W1ABC KC3QIS RRR", off S34J55x display audio at 48000 Hz, run Sent and OrdinaryUnkey, with the expected string read off the realized menu item rather than from a literal. Criterion 3: his own right-click started it, the real DigitalStopButton was pressed with a real mouse 4.22 s in, the run said Cancelled and came out of transmit by TheAbort, the wire carried FE FE 94 E0 1C 00 01 FD | FE FE 94 E0 17 FF FD | FE FE 94 E0 1C 00 00 FD, the sink counted 206880 of 606720 samples and the card went quiet 456 ms after the press. Criterion 4: a second boundary with nothing armed, the endpoint still open and the capture still running - NothingArmed, run null, wire unchanged at 2 frames, 49 packets over 3.0 s so the capture was alive, 0 samples rendered, peak at or below -90.0 dBFS against a threshold of -60, with the same capture reading -12.0 dBFS while the transmission was on so the instrument is not deaf.
APPENDED_BY: the file-editing tools, not outcome-append.bat. Both invocation forms were refused by this session's shell - verbatim, "This Bash command contains multiple operations. The following part requires approval: tools\arbiter\outcome-append.bat 2>&1" and "This command requires approval" - so work instruction 268 task 5's named alternative was taken and the header's STEP: C line was updated in place by hand, in the format the existing entries use.

### ALSO RECORDED FOR UNIT 268 - STEP C

A second append for the same unit and the same step, called as UNIT 3.
One unit is one entry, so what this route recorded is folded in here
rather than written as a second entry. Only what differs is listed.

APPROACH: Join the loopback endpoint to a real right-click in one Avalonia headless test - two transmissions inside it, one decoded back to the clicked menu text and one aborted mid-slot with the card measured going quiet
HIT: section 4 wants a ruling: no - All four items are recorded observations, a shell refusal already worked around by the plan's named alternative, a plan versus tree mismatch reported under the rule that the tree wins, a test harness note and the inherited reds left alone, and none of them asks the owner to decide anything or stops work pending his answer.
WHY: Step C has had no unit spent on it, its entry steps A and B are both done, and the loop test found no resembling approach - the three tried so far are bookkeeping on step 0, the state transitions on step A and the licence gate on step B. Every piece step C needs exists in the tree with a file and a line; what has never happened is joining them, and the seam nobody has measured is whether the text on the menu item the operator clicked is the text that comes back out of the sound card.
DECIDED: Three on my own authority. First, the host question - whether a headless Avalonia window can hold an open WASAPI render endpoint and a loopback capture - is task 2 rather than a discovery inside the goal task, with a named fallback written in advance (drive the view's own SendFlyoutFor code path in a plain Fact, and say in the report and the outcome entry that criterion 1's right-click is proved at the flyout builder and not at the raised event), because unit 267 named it the most expensive thing step C needs and a unit that finds it out at hour four has nothing to show. Second, criterion 3's abort is a second method in the same class rather than a second run inside one method, because a stopped transmission does not decode and one method holding two 15-second slots plus a pumped dispatcher makes the timeout the thing under test - criterion 1's 'one test' is satisfied by the chain method
LICENCE: PHASE_PLAN.md step C's four exit criteria, which name the loopback endpoint and the fake transport in the criterion itself; its rule that a bench step's criteria are all satisfiable with no radio and none deferred to Tim; its named alternatives to stopping - an approach that fails is abandoned with its cost recorded and another taken, and the file-editing tools are used where the shell refuses; and SHACK_FACTS.md FACT-004, under which the device is a sound card and the port is FakePort.
COST: 16.319568000000007
ACCOMPLISHED: The message Tim clicks is the message that leaves the machine - proved by decoding the sound off his own card back into text and matching it against the menu item he clicked, rather than against a string a test chose for itself. His Stop button takes a real transmission off a real card from the middle of the chain, not off a fake. Nothing goes out that he did not click, measured as a silent card rather than as an untouched counter. That is the last thing about Hamlet's transmit side that can be established without a radio; everything left is Tim at his own station.
STATE_WHY: All four exit criteria are backed by quoted measurements from runs tonight, the clicked string decoded back as W1ABC KC3QIS RRR, the Stop press at 4.22 s cutting 4.71 s of a 12.64 s slot with the abort's own CI-V frames, and a live capture reading peak under -90 dBFS across an unclicked boundary, with the gesture driven by a real right-click and CI-V on FakePort rather than by the named fallback.

## UNIT 269 - STEP D

STEP: D
APPROACH: Put the Transmit drive control and the dBFS and clip readout under the waterfall on the Digital tab, out of the modal Settings dialog, and read the level off the sink the application already builds rather than off the setting the operator typed. Trace first, in its own commit, with six questions each answered by a file, a line and a quotation; then the control watched red; then the readout watched red, with a fallback written in advance in case no route to the sink's own figures existed; then measure what the clip count can actually count and correct the page Tim follows at the rig.
HIT: Three things, and the first two went the good way. THE ROUTE TO THE SINK'S OWN FIGURES EXISTS AND THE FALLBACK WAS NOT NEEDED. Unit 265 recorded that there was no route to WasapiTransmitSink.PeakWritten from what Ft8TransmitSequence returns, and that was right - PlayedAudio carries samples and time and no level. But the route does not go through the sequence at all: the application constructs the sink itself at MainWindowViewModel.cs:8026 as a local and drops the reference, so it keeps it and reads the two figures off a two-property read-only interface after the boundary has already returned, with nothing keyed. Ft8TransmitSequence.RunAsync, the key, the sink call, the finally, the abort, the stop, Ft8ArmedSend and ITransmitAudioSink.PlayAsync are all unchanged. THE CLIP COUNT OVER THE COMPOSED ARRAY IS A FLAT ZERO BY CONSTRUCTION, measured rather than reasoned: at 1.0, the highest drive Ft8Composer.DriveIsUsable accepts, a whole slot composed at 48000 Hz through the application's own route is 606720 samples with a largest magnitude of exactly 1.000000 and 0 outside the rails - so a screen that told Tim to watch it as evidence would be telling him something it cannot say, and both the screen line and the page now say what the count is of. THE THIRD IS MINE AND WAS CORRECTED IN PLACE: a git add -A at task 2 tracked .commit-msg.txt and .oa-267.bat, which task 5 names as report-not-repair; both were removed from the index again and left on disk.
MOVE: work around
WHY: No unit can perform step D - its three criteria are Tim at his own radio - but criterion 1 named a control and a readout "under the waterfall" that did not exist there: the drive was behind a ShowDialog modal that covers the waterfall, the decode table and the always-pressable Stop button, and the dBFS and clip count appeared only in a sentence after a send, reporting the level he set rather than the level the card was handed. Unit 268's own section 4 reported the mismatch and was told to report and not repair it. Rather than declare the step unreachable or hand him an evening he cannot run, this unit built the half of criterion 1 a machine with no radio can build and left the half only he can answer.
DECIDED: Four on my own authority. First, the drive control appears on the Digital tab AS WELL AS in Settings rather than moving out of Settings, because both write one AppSettings.TransmitDrivePeak through one Ft8Composer.DriveIsUsable and a control removed from Settings would break unit 265's committed suite for no gain - and the shared arithmetic went into one new file, TransmitDrive.cs, rather than being copied, which left SettingsViewModel's committed strings byte-identical. Second, the readout names which quantity it shows and the two sentences on the screen are deliberately worded apart - composed peak against the peak the endpoint was handed - because the honest failure of this unit is a screen that shows the operator his own setting back and calls it a measurement, and where a sink offers no report the line says there is no measurement rather than falling back. Third, step D is recorded in progress and not partial, because none of its three criteria is met and partial would claim one that is Tim's. Fourth, the two window-level test files each landed their red without one assertion that could only be written against a type or property that did not exist yet - a file that does not compile takes the whole assembly down, including the seven committed tests this unit was permitted to run - and both of those assertions arrived with the green, said so at the site and are green.
LICENCE: PHASE_PLAN.md step D's first exit criterion, which names the Transmit drive control and the dBFS and clip count under the waterfall; the plan's rule that a criterion needing the radio moves to a shack step, which leaves the bench half of criterion 1 as bench work rather than his; its named alternatives to stopping, including the file-editing tools where the shell refuses; unit 268's section 4 item 2, which reported the plan-versus-tree mismatch and was told to report and not repair it; and work instruction 269's named, bounded exception to the no-suite rule, which licensed exactly seven committed tests, each run alone by exact name.
COST: unknown
ACCOMPLISHED: When Tim sits down at the radio he can set Hamlet's drive from the screen he is already looking at, without opening a dialog over the band and over the Stop button, and see the dBFS move as he sets it - before he keys anything, which was not possible this morning. After each slot he reads what the sound card was actually handed, with the screen saying which number that is, so a level he has verified is never confused with a number he typed. And the clip count he was going to be asked to watch has been measured: over the audio Hamlet builds it is zero at any drive the composer allows, so the screen and the page now say so instead of leaving him watching a number that cannot move. What his radio's ALC does at that level, and what the value should be, is still his to answer, and this unit chooses nothing for him.
FATE: executed
STATE_AFTER: in progress
STATE_WHY: NONE OF STEP D'S THREE CRITERIA IS MET AND NONE COULD BE, because all three are Tim's at his own radio: he sets the control and reads the figures, his radio's ALC behaviour at that level is in his words, and the value goes into SHACK_FACTS.md, which this unit did not touch. What moved is the blocker under criterion 1 and only that. Its bench half is built and measured on runs tonight: the control is under the waterfall in DigitalSendReserved with DigitalStopButton asserted still visible, still enabled and still inside the window's bounds beside it; the dBFS is on screen before anything is transmitted and moves with the control (-12.0 dBFS at the default, -8.0 dBFS at 40 %); and after one clicked send through SendMessageCommand to AtSlotBoundaryAsync the readout carries the sink's own figures, proved with a fake reporting 0.5 against a composed 0.25 so a readout showing the setting could not pass. The cost of changing the drive between two slots went from 1 window and 4 controls, with the band and the Stop button hidden, to 0 windows and 1 control. Step D stays in progress until Tim runs the evening.
APPENDED_BY: the file-editing tools, not outcome-append.bat. Both invocation forms were refused by this session's shell - verbatim, "This command requires approval" for `tools\arbiter\outcome-append.bat` and the same words for `cmd //c tools\\arbiter\\outcome-append.bat` - the thirteenth consecutive unit, so work instruction 269 task 5's named alternative was taken and the header's STEP: D line was updated in place by hand, in the format the existing entries use. The instruction asks for fourteen fields; outcome-entry.py's own FIELDS list at :115-118 has twelve, and twelve are written here, plus this line.

### ALSO RECORDED FOR UNIT 269 - STEP D

A second append for the same unit and the same step, called as UNIT 4.
One unit is one entry, so what this route recorded is folded in here
rather than written as a second entry. Only what differs is listed.

APPROACH: put the transmit drive control and the dBFS and clip readout under the waterfall on the Digital tab, out of the modal Settings dialog, and read the level off the sink the app already builds rather than off the setting the operator typed
HIT: section 4 wants a ruling: no - Every one of the five items is a report, a self correction already made, a note for step E or an untouched inherited red, and none asks the owner to decide anything or holds work pending his answer.
WHY: No unit can perform step D - its three criteria are Tim at his own radio - but criterion 1 names a control and a readout 'under the waterfall' that do not exist there: the drive is behind a ShowDialog modal that covers the waterfall, the decode table and the Stop button, and the dBFS and clip count appear only in a sentence after a send, reporting the level he set rather than the level the card was handed. Rather than declare the step unreachable or hand him an evening he cannot run, this unit builds the half of criterion 1 a machine with no radio can build and leaves the half only he can answer.
DECIDED: Three on my own authority. First, the drive control appears on the Digital tab as well as in Settings rather than moving out of Settings, because both write one AppSettings.TransmitDrivePeak through one Ft8Composer.DriveIsUsable and a control removed from Settings would break unit 265's committed suite for no gain. Second, the readout is required to name which quantity it shows - composed peak or the peak the endpoint was handed - with a fallback written in advance, because the honest failure of this unit is a screen that shows the operator his own setting back and calls it a measurement. Third, step D is recorded in progress and not partial, because none of its three criteria is met and partial would claim one that is Tim's.
LICENCE: PHASE_PLAN.md step D's first exit criterion, which names the Transmit drive control and the dBFS and clip count under the waterfall; the plan's rule that a criterion needing the radio moves to a shack step, which leaves the bench half of criterion 1 as bench work rather than his; its named alternatives to stopping, including the file-editing tools where the shell refuses; and unit 268's section 4 item 2, which reported the plan-versus-tree mismatch and was told to report and not repair it.
COST: 24.625143
ACCOMPLISHED: When Tim sits down at the radio he can set Hamlet's drive from the screen he is already looking at, without opening a dialog over the band and over the Stop button, and read what the level actually was after each slot - with the screen saying which number that is, so a level he verified is never confused with a number he typed. What his radio's ALC does at that level, and what the value should be, is still his to answer, and this unit chooses nothing for him.
STATE_AFTER: blocked
STATE_WHY: None of the three must-pass criteria is met, the report quotes no ALC observation, no drive setting made at the rig and confirms SHACK_FACTS.md was never opened for writing, and all three depend on Tim acting at his own radio, an outside change that no further unit effort can supply.

## UNIT 270 - STEP E

STEP: E
APPROACH: Measure before building. Trace first in its own commit, six questions each answered with a file, a line and a quotation. Then drive the ending nobody had driven - he answers a CQ, the station comes back with a report and a roger in one field, and his own RRR is what completes the exchange - through the application on FakePort and a substituted sink, and print what the ledger says, what the newest row says and what the Send area says, without changing a line of product code. Then, watched red first, a fourth line in the Send area under the waterfall that after a booked send says where that contact stands, computed from the one ledger the rows already read, at the slot the transmission went out in, with no row already on the table restating itself.
HIT: Three things, and the first was the whole night's question. THE HOLE IS REAL AND IT WAS MEASURED RATHER THAN ARGUED. On a run tonight - 16:37:00 KC3QIS W1ABC R-09 heard, 16:37:15 his own W1ABC KC3QIS RRR - the ledger the application itself kept read IsComplete True and "complete, 0 slots", the newest row on the table still read "your move, 0 slots", and neither line in the Send area carried any of the four state words, which the test reads off the enum rather than writing out. Nothing in the tree failed when that happened. THE ADDRESSEE ROUTE EXISTS AND THE FALLBACK WRITTEN IN ADVANCE WAS NOT TAKEN. Ft8MessageSplit.Split at :53 and IsCallToAnyone at :86 are public in the namespace the view model already uses and the app already forwards to both from Ft8Vocabulary.cs:57 and DecodedFilter.cs:86, so the line asks the same two questions of the same two methods that Ft8ContactLedger.RecordSent asks at :227-240, and no second splitter and no invented callsign were needed. THE THIRD IS SMALL AND IS REPORTED, NOT REPAIRED: the instruction places the "a row never restates itself" remark on PlaceRow at :7826; it is in fact on ContactTextFor's doc comment at :7885-7888, and SendMenuFor's read of the ledger at :8296 is a fourth reader the instruction's list does not name.
MOVE: work around
WHY: Step D is blocked on Tim at his own radio and no unit can move it, and all three of step E's criteria are his evening too. Rather than spend a night on a step nobody here can touch, or halt the phase, this unit took the one part of what remains that a machine with no radio can settle: step E's second criterion, whose telemetry half is already proved on disk by the committed walk and whose "the row reads complete" half had a hole. PlaceRow computes a row's Contact cell once, at the row's own slot, and RecordSent writes into a private ledger and touches nothing already on the table, so where the operator's own message is what completes the exchange the newest row on screen was placed before the send and still reads "your move" - and if the station has got what it came for and gone quiet, nothing in Hamlet ever says the contact finished. At the rig that is a man re-sending into a contact that ended, or waiting for a station that has finished with him.
DECIDED: Three on my own authority. First, the present state goes into a line about the present and never into the cells of rows already placed, because ContactTextFor's own contract is that a row shows where the contact stood in its own slot and never restates itself, and a table of moments that edits its own moments is a worse instrument than an incomplete one - the row-rewrite route was named in the instruction, parked, and the measurement that proves the cells do not move is committed as a permanent guard against a future unit "repairing" it. Second, the line is required to say which slot it was read at, because a state with no moment is the same class of fault as unit 269's readout showing the operator his own setting back and calling it a measurement; that is why a later decode leaving the line where it stands is honest rather than stale, and why the sixth should-pass assertion is answered "it does not move, and here is the wording that covers it" instead of adding a second writer on the decode path. Third, task 2's measurement was added to the committed walk file rather than copied into a new one, because Panel(), Heard, ClickAsync and WaitForSlotAsync are already there and a second copy of a harness is a second thing to drift - not one line of any existing method changed - while task 3's four tests are a new file, because they read the line off the realized TextBlock where the operator would read it and one of them asserts where that control sits, and the walk's file builds no window at all.
LICENCE: PHASE_PLAN.md step E's second exit criterion, which names the transmitted slots in telemetry and the row reading complete; the plan's rule that a criterion needing the radio is his, and its mirror, which leaves a bench-provable half as bench work - the same reading that licensed unit 269 to build the bench half of step D's criterion 1; the ruling that a contact is never closed by the app and that nothing is hidden, closed or forbidden; section 12.1, under which a state is bookkeeping over which messages passed and never a reading of meaning; work instruction 270's named, bounded exception to the no-suite rule, which licensed exactly seven committed tests each run alone by exact name; and the plan's named alternatives to stopping, including the file-editing tools where the shell refuses a call.
COST: unknown
ACCOMPLISHED: When Tim answers a CQ and his own RRR is the message that finishes the exchange, Hamlet now tells him it finished. The Send area under the waterfall reads, on a run tonight, "Where the contact with W1ABC stands: complete, 0 slots, read at the 16:35:15 UTC slot. That is what passed between you, counted in slots; it is not advice about what to send next, and nothing is closed or withheld by it." He will not re-send into a contact that ended and he will not sit waiting for a station that has finished with him, and because the line carries the slot it was read at he can never mistake it for a live reading. Nothing was taken away: the table still says where each contact stood in its own slot, the menu after complete offers exactly what it offered before, and no message is suggested to him. What the contact itself is worth, and everything else about the evening, is still his.
FATE: executed
STATE_AFTER: in progress
STATE_WHY: NONE OF STEP E'S THREE EXIT CRITERIA IS MET AND NONE COULD BE, because all three are Tim's at his own radio: he answers a CQ on 14.074 or 7.074 and completes an exchange, the slots that appear in telemetry are his slots, and what he saw and what surprised him are his words. What moved is the blocker under criterion 2 and only that. Its telemetry half was already proved on disk by the committed walk; its "the row reads complete" half was proved for one ending only - the exchange that finishes with a message HEARD - and the ending that finishes with the operator's own message had a hole that nothing in the tree failed on. That hole is measured, committed and closed: a fourth line in DigitalSendReserved, computed from the one ledger the rows already read, asserted on a realized window with DigitalStopButton still visible, still enabled and inside the window's bounds beside it. Criteria 1 and 3 were untouched and SHACK_FACTS.md was not opened. Step D was not touched and stays blocked. Step E stays in progress until Tim runs the evening.
APPENDED_BY: the file-editing tools, not outcome-append.bat. Both invocation forms were refused by this session's shell - verbatim, "This command requires approval" for `tools\arbiter\outcome-append.bat --help` and the same words for `cmd //c "tools\arbiter\outcome-append.bat" --help` - the fourteenth consecutive unit, so work instruction 270 task 4's named alternative was taken and the header's STEP: E line was updated in place by hand, in the format the existing entries use. outcome-entry.py's own FIELDS list at :115-118 holds twelve fields and twelve are written here, plus this line.

### ALSO RECORDED FOR UNIT 270 - STEP E

A second append for the same unit and the same step, called as UNIT 5.
One unit is one entry, so what this route recorded is folded in here
rather than written as a second entry. Only what differs is listed.

APPROACH: after the operator's own last transmission completes a contact, say so in a Send area line read from the one ledger the rows already use, at the moment it was read, without rewriting the contact cell of any row already on the table
HIT: section 4 wants a ruling: no - All four items are reports for the record, a plan versus tree mismatch left unrepaired under the rule that the tree wins, a shell refusal already handled by the plan's named alternative, and a note that one committed test was outside this unit's licence, and none of them asks the owner to decide anything or stops work pending his answer.
WHY: Step D is blocked on Tim at his own radio and no unit can move it, so rather than spend a night on it or halt the phase, this unit takes the one part of what remains that a machine with no radio can settle - step E's second criterion, whose telemetry half is already proved on disk by the committed walk and whose 'the row reads complete' half has a hole: where the operator's own message is what completes the exchange, PlaceRow computed the newest row's cell before the send and RecordSent touches nothing on the table, so if the station then goes quiet nothing on screen ever says the contact finished.
DECIDED: Three on my own authority. First, the present state goes into a line about the present rather than into the cells of rows already placed, because PlaceRow's own contract at :7826 is that a row shows where the contact stood in its own slot and never restates itself, and a table of moments that edits its own moments is a worse instrument than an incomplete one - the row-rewrite route is named in the instruction, parked, and to be reported if it turns out to be the only honest one. Second, step E's criterion 2 is treated as having a bench half and a shack half, on the same reading of the plan that licensed unit 269 to build the bench half of step D's criterion 1, and this unit builds only the bench half and claims no criterion. Third, the line is required to say which slot it was read at, because a state without a moment is the same class of fault as unit 269's readout showing the operator 
LICENCE: PHASE_PLAN.md step E's second exit criterion, which names the transmitted slots in telemetry and the row reading complete; the plan's rule that a criterion needing the radio is his and its mirror, which leaves a bench-provable half as bench work; the ruling that a contact is never closed by the app and that nothing is forbidden, hidden or closed; section 12.1, under which a state is bookkeeping over which messages passed and not a reading of meaning; and the plan's named alternatives to stopping, including the file-editing tools where the shell refuses a call.
COST: 15.337653000000003
ACCOMPLISHED: When Tim answers a CQ and his own RRR is the message that finishes the exchange, Hamlet tells him it finished - naming the station, in the same four words the rows use, with the slot it was read at - instead of leaving the top of his table reading 'your move' until a station that has already moved on happens to transmit again. He will not re-send into a completed contact, and he will not sit waiting for one. What the contact itself is worth, and everything else about the evening, is still his.
STATE_AFTER: blocked
STATE_WHY: All three of step E's exit criteria require Tim to operate his own licensed station, its entry step D is itself blocked on the same outside event, and the unit closed none of them, only clearing a bench blocker under criterion 2, so no further development effort can meet the criteria.

## UNIT 271 - STEP 1

UNIT_AS_CALLED: 1
STEP: 1
APPROACH: not recorded
HIT: section 4 wants a ruling: no - Every item is a report for the record, a self declared rule stretch, a caveat on an unverifiable claim or a change deliberately not made, and none asks the owner to decide anything or holds work pending his answer, the one request to him being for his memory of what he pressed rather than a ruling.
MOVE: continue
WHY: not recorded
DECIDED: none
LICENCE: none
COST: 16.066289
ACCOMPLISHED: not recorded
FATE: executed
STATE_AFTER: in progress
STATE_WHY: The report closes none of step E's three criteria, which only the operator at the radio can meet, but it measures and repairs the undecodable transmission that stood in the way of criterion 1, so work is under way and the on-air exchange is still needed.

## UNIT 272 - STEP E

UNIT_AS_CALLED: 2
STEP: E
APPROACH: refuse to key a message that does not read back as the words the operator clicked - close the hashed-field hole in the composer guard, and put the hash flag in the telemetry line
HIT: section 4 wants a ruling: no - All five items are measurements reported for the record, notes for later units, a self declared gap the unit says is not its to re-argue, and a plain caveat on what tonight's run can claim, and none asks the owner to decide anything or holds work pending his answer.
MOVE: work around
WHY: All three of step E's criteria are Tim at his own radio and no unit can meet them, but his one evening on a live antenna produced two slots nobody could decode, and unit 271 repaired the input that caused it without closing the class - Ft8Composer computes CarriesHashedCallsign on every transmission and no line of src/ reads it, so the next compound or portable callsign he answers is keyed unreadably and the telemetry says Standard. Rather than spend a night on a step nobody here can touch, this unit removes the last measured bench obstacle to his answering a CQ.
DECIDED: Three on my own authority. First, a hashed callsign is not the words the operator clicked unless he clicked brackets, so Hamlet refuses to key rather than transmitting something nobody receives - which deliberately means a genuinely nonstandard callsign is refused, and whether to offer a way to send one anyway is logged and parked rather than chased. Second, the refusal lives at the send path where the licence gate already refuses and never in the menu, because nothing is forbidden in the menu; no dialog, no prompt, no compensating control. Third, unit 271 declined to change messageLength because choosing what a diagnostic counts is a decision rather than a repair - I make it: the line carries the hash flag and the length measures the encoded message, inside HM-DEC-018, which admits shape and not words.
LICENCE: PHASE_PLAN.md step E's criteria 1 and 2, and the plan's rule that a criterion needing the radio is his while its bench-provable half is bench work - the same reading that licensed unit 269 on step D and unit 270 on step E criterion 2. Section 0.0 and HM-DEC-092 as unit 271's instruction extended them: a message that cannot be decoded by anybody is a transmission asserting something nobody receives. The plan's named alternatives to stopping, including the file-editing tools where the shell refuses. HM-DEC-018 bounds task 3. FACT-005 licenses task 4.
COST: 14.203481999999997
ACCOMPLISHED: When Tim goes back to the radio, Hamlet will not put a slot on the air that nobody can turn back into his callsign. It happened twice on 2026-09-07 and neither he nor the application knew - the level was right, the timing was right, the log said Sent. Now, if what he clicked cannot be encoded into something Hamlet's own decoder reads back, nothing is keyed and the Send area tells him what he asked for and what the encoder made of it, in the same place it already tells him a frequency is outside his privileges. Nothing is taken out of the menu. And the telemetry line no longer describes a hashed transmission as a healthy sixteen-character standard message, so the record of his next evening is one he can trust.
FATE: executed
STATE_AFTER: not started
STATE_WHY: All three of step E's exit criteria require Tim at his own radio answering a CQ and recording what he saw, and the report states plainly that none of them is claimed or met, the unit having only removed a bench blocker and changed telemetry fields.

## UNIT 276 - STEP E

UNIT_AS_CALLED: 276
STEP: E
APPROACH: carry the existing right-click handler to the mine list, extend the test that already opens menus so it looks at both, and correct the record
HIT: a correction with nowhere to go - work instruction 276 task 4 asks for a line appended beneath UNIT 274's entry, and THERE IS NO UNIT 274 ENTRY. This file's last entry before this one is UNIT 272. Units 273, 274 and 275 wrote no outcome entries at all.
MOVE: work around
WHY: the correction is written here, in this unit's own entry, rather than under an entry that does not exist. Inventing a UNIT 274 block to append to would be fabricating a record of a unit this session did not run, which is worse than the gap it would paper over. The claim is named in full below so a reader who goes looking for it finds it.
DECIDED: nothing on this session's authority beyond where to put a correction that had nowhere to go, and two test-shape choices recorded in the commit at c5bb1dc.
LICENCE: work instruction 276 tasks 2, 3 and 4, and Tim's ruling of 2026-09-07 that the mine list gets the right-click menu.
COST: unknown
ACCOMPLISHED: the operator can right-click a message addressed to him and answer it, which he could not do from the day the decoded area was split, and a test now opens the menu on both lists so a fourth unit restructuring that grid cannot repeat it.
FATE: executed
STATE_AFTER: not started
STATE_WHY: Step E's three criteria are Tim at his own radio working a station and no unit can meet them. This unit removed a defect standing between him and them and closes nothing.

### CORRECTION TO UNIT 274, APPENDED BY UNIT 276 ON 2026-09-07

**Unit 274's report asserted a feature that did not exist.** Its section 2 said:

> Right-click a row on the mine side and there is a `Log this contact...` item,
> under a rule below the send options. It is on rows addressed to him and on
> nothing else.

**No part of that was true when it was written.** There was no context menu on the
mine list at all, so there was no Log item on it and no send options either. The
`Log this contact...` item existed in the code, attached to the left list, gated on
the message being addressed to the operator - and unit 273 had already moved every
such row to the other list, so **the item's condition and its list were mutually
exclusive by construction and it had never once been shown.**

**This is recorded here because unit 274's own entry does not exist to carry it.**
The last entry in this file before unit 276's is UNIT 272; units 273, 274 and 275
wrote none. The claim survives in that unit's commit history, and now here.

**What made it possible, and it is not what the correcting instruction assumed.**
Work instruction 276 says nothing could have caught it and that no test opens a
context menu on a decoded row. **A test does, and it did.**
`Views/TheMenuIsUnderTheMouseTests` raises a real `ContextRequested` on a real row
control in a real window and reads the flyout's items, and its row finder asks for
a row addressed to the operator - so it went red the day unit 273 moved those rows
away, and stood at 5 of 6 failing with `no realized row matched. Rows on the table:
8; realized grids with a row DataContext: 8`.

**It stayed red for three units because nothing was allowed to run it.** It is in
the `Views` namespace whose stall unit 230 documented, and HM-DEC-155 rules that a
unit runs only the test it constructs. Units 273, 274 and 275 each reported it or
its neighbours as *not run, and you should know which*, and each was right to.

**The gap is not a missing test. It is a test that was red and unread**, and a
report written from the code that declares a menu item rather than from the menu.

**No blame attaches to unit 274.** Its instruction did not ask for the menu to be
moved, and the one thing that would have contradicted it was a test that unit was
forbidden to run.

## UNIT 277 - STEP E

STEP: E
APPROACH: carry the sent half of the conversation onto the panel from the ledger that already held it, derive the slot parity from the station's own transmissions, scope the panel to one conversation, and reconstruct the evening that prompted it
HIT: three of the four pieces task 1 went looking for already existed and none of them was on screen: the sent text was in Ft8ContactLedger, the parity is arithmetic on a slot boundary, and a 250 ms tick was already running
MOVE: build on
WHY: the work was surfacing rather than building, so the unit went further than a fresh mechanism would have. The reconstruction in task 5 then found a defect that would otherwise have shipped looking exactly like the bug the unit was written to fix
DECIDED: where the repeat fold stops - at anything in between rather than across a whole exchange - because the literal reading destroys the one fact the panel exists to show. Raised for a ruling rather than settled, since it is what the display asserts
LICENCE: work instruction 277 tasks 1 to 6, and Tim's rulings of 2026-09-08 that his own transmissions are interleaved, that one conversation shows with the others above it, and that the panel says whose turn it is
COST: unknown
ACCOMPLISHED: he can see both halves of a contact and whose fifteen seconds it is, which is what he asked for after an exchange failed and he could not see why. The turn line arms nothing and reaching zero does nothing
FATE: executed
STATE_AFTER: not started
STATE_WHY: Step E's criteria are Tim at his own radio working a station and no unit can meet them. This removed what stood between him and them and closes nothing.

### THE RECORD IS MISSING THREE UNITS, AND THEY ARE NOT BACK-FILLED

**Units 273, 274 and 275 wrote no outcome entry at all.** Unit 276 found it while
looking for a unit 274 entry to correct, and wrote its correction into its own entry
instead. So between `## UNIT 272` and `## UNIT 276` this file has nothing, and three
units' findings survive only in their commit messages.

**They are deliberately not reconstructed.** Nobody running now ran them, and an
entry composed from their commits would be a fabricated record of work this session
did not do, which is worse than the gap it would cover.

**The cause is known and is not carelessness.** `PHASE_OUTCOME.md` is written by
`tools/arbiter/outcome-append.bat`, which no work instruction told those units to
run. This unit ran it because task 6 named it. Until the write is a task in the
instruction, or the tool runs at the end of the loop where its absence is visible,
the same gap will open again.

