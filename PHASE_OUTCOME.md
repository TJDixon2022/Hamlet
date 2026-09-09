PHASE: FT4 works exactly the way FT8 does
PHASE_SET: 2026-09-08
STEP: 0 | done | where FT4 lives, decided by reading
STEP: 1 | partial | FT4 decodes a signal Hamlet made
STEP: 2 | not started | the slot machinery is FT4's
STEP: 3 | not started | the log can say FT4
STEP: 4 | not started | the FT4 button works
STEP: 5 | not started | Tim hears FT4
STEP: 6 | not started | Tim works a station on FT4

---

## What this file is

The phase's memory. `output.md` is overwritten every unit and cannot carry what was
tried; this file survives the unit and records, per unit, the approach taken and what
it hit.

**The header above is a cursor over the entries below, and the entries win.**

Appended by `tools\arbiter\outcome-append.bat`. Five state words and no others:
`not started`, `in progress`, `partial`, `blocked`, `done`.

The send phase's entries are archived at `docs/phase-send-run/PHASE_OUTCOME.md`. What
it built and what is not to be rebuilt: the abort proved from six states, the FT8
composer, the sound card route, the stop that took 8.4 seconds of audio off the air
mid transmission, the loopback reading 3 of 3 messages back as themselves, the
transmit level, the contact ledger and its four row states, the right-click menu, the
ADIF log, the achievements screen, the belt, and the marks. **Tim transmitted on a
live antenna on 2026-09-07 and logged his first contacts.**

## Entries

*None yet. This phase has not started.*

## UNIT 288 - STEP 0

STEP: 0
APPROACH: Read the pinned ft8_lib clone and the port itself rather than deciding from memory, then stood the application up and pressed the FT4 chip through UI Automation.
HIT: There is no file called ft4.c, so the filename test the instruction warns against would have answered wrongly; upstream FT4 lives inside the FT8 files behind a protocol enum. And upstream decode_ft8 -ft4 read 0 messages from upstream gen_ft8 -ft4, which looks like hollow support and is placement: the candidate search runs -10 to 19 blocks, which at 0.048 s is -0.48 to 0.91 s, and the generator centres the transmission at 1.23 s.
MOVE: The port carries FT4. Ten gaps named beyond the decoder and none fixed.
WHY: ft8_lib carries FT4 complete - ft4_encode at ft8/encode.c:127 and four decoder branches at ft8/decode.c:127, 192, 253 and 454 - so the rule Tim ruled on 2026-09-08 decided it and this unit only read. Shifting the identical audio forward 0.99 s returned CQ KC3QIS FN00 at plus 19.5, so the whole upstream chain agrees.
DECIDED: Nothing on the arbiter's authority. The decision was Tim's own rule applied to evidence, and the two questions it raised were handed back rather than settled.
LICENCE: Work instruction 288 tasks 1 to 3, under Tim's ruling of 2026-09-08 that where FT4 lives follows what upstream does.
COST: one session, six tasks, no test suite run, one dotnet build at 9.05 s
ACCOMPLISHED: Where the FT4 decoder goes is answered from the tree with file and line, and what is already shared is counted so nothing is written twice: 28 of 33 port files and 6160 of 7926 lines are protocol-neutral today.
FATE: executed
STATE_AFTER: done
STATE_WHY: All four of step 0's must-pass exits are met: what upstream carries with file and line, what the port already shares, the decision with its reason, and what the 51 fidelity tests would have to become.

## UNIT 289 - STEP 1

STEP: 1
APPROACH: Ported upstream FT4 into new files beside the FT8 ones rather than parameterising them, nailed the encoder to gen_ft8 -ft4 symbol for symbol and the waveform to its own WAV sample for sample BEFORE writing the decoder, then round tripped 106 messages through Hamlet's own chain.
HIT: Upstream's own candidate sweep of -10 to 19 blocks cannot reach an FT4 signal centred in its own slot. At a 0.048 s block that sweep ends at 0.912 s and the generator starts the signal at 1.23 s. Measured in task 1, and it is why upstream's decoder reads zero messages out of upstream's generator - unit 288 saw the symptom and this unit has the arithmetic.
MOVE: continue
WHY: Step 1 had all four exit criteria untried and every later step depends on an FT4 decoder existing. Three must-pass criteria were reachable in one unit because 28 of 33 port files were already protocol-neutral and upstream carried working C for every one of the rest.
DECIDED: The FT4 candidate sweep is widened to blocks -10 to 51 where the demo application uses -10 to 19. That bound is not in the ft8 library at all - it is a file-scope judgement in demo/decode_ft8.c about how much work to do, the same class as kMin_score and kMax_candidates, and it was already a constructor parameter in this port. 51 is 156 blocks in a slot less 105 in a transmission, so it covers every placement a well-formed FT4 signal can have including upstream's own centred 25. Nothing about the modulation, the tables, the codeword or the waveform is changed. The 4.48 against 5.04 timing question is NOT settled here - it stays with Tim, and the constant now lives in exactly one file, src/Ft8Sharp/Ft4Timing.cs, so a ruling costs one edit.
LICENCE: PHASE_PLAN.md's named-alternatives table - the tree disagrees with this plan, the tree wins, report the mismatch and continue - together with the arbiter's own moved second exit criterion for step 1.
COST: one session, seven of eight tasks, no test suite run, every test filtered by exact name and foregrounded
ACCOMPLISHED: Hamlet can make an FT4 transmission and read it back as the message it started as, 106 of 106 with zero wrong decodes, and its tones and its samples are proved identical to upstream's own generator rather than only to itself.
FATE: executed
STATE_AFTER: done
STATE_WHY: All three must-pass criteria are met and evidenced: 106 of 106 messages round tripping to themselves including compound callsigns, grids, reports and RR73; the timing measured from the audio - 0.048 s a symbol, 105 symbols, 5.04 s of occupancy, 4 tones 20.8333 Hz apart, a 7.5 s slot - with the 4.48 against 5.04 disagreement named with both numbers; and zero wrong decodes counted separately from zero missed. The nice-to-pass sensitivity ladder is the named drop candidate and is unmet by choice.
APPENDED BY HAND: tools/arbiter/outcome-append.bat could not be run in this session - the shell refused the invocation and this is a non-interactive session, so there was no way to answer for it. The entry above was written with the file-editing tools in the format outcome-entry.py produces, and the header's STEP: 1 line was updated in place by the same means. The arguments the script would have been given are committed at tools/arbiter/unit289-append.bat, so the entry can be reproduced rather than reconstructed. Work instruction 289 task 7 anticipated this and required it to be said.

### ALSO RECORDED FOR UNIT 289 - STEP 1

A second append for the same unit and the same step, called as UNIT 1.
One unit is one entry, so what this route recorded is folded in here
rather than written as a second entry. Only what differs is listed.

APPROACH: Port the upstream FT4 encoder and decoder into the five FT8-specific files of Ft8Sharp, nail the encoder to gen_ft8 -ft4 symbol for symbol over the 51-message corpus first, then round trip a hundred messages through Hamlet own encoder and decoder.
HIT: section 4 wants a ruling: banked - Rulings are wanted on the 4.48 against 5.04 timing, the widened candidate sweep, the PHASE_STATUS ownership conflict and the version scheme, but none forecloses step 1, whose three must-pass criteria are already met and evidenced with the timing disagreement named rather than assumed and the constant isolated to one file so a ruling costs one edit.
WHY: Step 0 closed with all four must-pass exits met and it answered step 1's entry condition - the port carries FT4, upstream carries a working reference for every one of the five files, and unit 288 measured the job at five files plus three generated table entries. Step 1 is untried, the loop test found no resembling approach, and it is the critical path for steps 2, 4, 5 and 6.
DECIDED: I moved step 1's second exit criterion. It asserted '4.48 seconds of transmission' as a must-pass measurement; unit 288 measured upstream at 105 symbols times 0.048 s equals 5.04 s and found 4.48 nowhere in the clone, so as written a faithful port fails a criterion for being faithful. The criterion is now that the timing is measured from the audio and stated with its source, with the 4.48 against 5.04 disagreement named with both numbers. I did NOT decide which figure is right, and I did not license a divergence from upstream - the unit builds on upstream's constant, puts it in one named place so a later ruling is one edit, and the question stays with Tim where unit 288 put it.
LICENCE: PHASE_PLAN.md, 'The steps are a hypothesis, not a contract' - the arbiter may move a target found to have been measured wrong, recording the evidence in PHASE_OUTCOME.md; and its named-alternatives table, 'the tree disagrees with this plan - the tree wins, report the mismatch and continue.'
COST: 33.30902650000002
ACCOMPLISHED: Hamlet can make an FT4 transmission and read it back as the message it started as, over a hundred messages with no wrong decode, and its tones are proved identical to upstream's own generator rather than only to itself.
STATE_AFTER: partial
STATE_WHY: The round trip of 106 messages, the zero wrong decodes across 848 further trials and the sensitivity ladder are all met and measured, but the timing criterion as the step states it is not, since the audio measures 5.04 seconds of transmission rather than the plan's 4.48 and that disagreement is still awaiting Tim's ruling.

## UNIT 290 - STEP 2

STEP: 2
APPROACH: Surveyed every fifteen second assumption in the tree BEFORE touching any arithmetic, so criterion 4 is a census rather than a list of this unit own edits, then made the slot grid a value the callers pass instead of a const and threaded it through the cutter, the watch, the sidecar, the turn ring and the two sentences on screen.
HIT: Ft8Slots.SlotStart could not express a 7.5 second boundary at all, and it is arithmetic rather than a constant. It computed (trueUtc.Second / (int)SlotSeconds) * (int)SlotSeconds and built a DateTime from whole seconds, so at 7.5 the cast is 7 - a seven second grid wearing a 7.5 second grid name, right at :00 by coincidence and wrong at the other seven boundaries in the minute, out by three and a half seconds at :52.5. The DateTime constructor has no field for the half either. Rewritten in ticks anchored on the minute, which is the largest unit both lengths divide exactly.
MOVE: continue
WHY: Step 2 entry condition was satisfied by step 1 closing, and step 2 is on the critical path for step 4. The census had to run first because the fourth criterion is closed by naming rather than by fixing, and a unit that starts changing call sites writes a list of its own edits wearing a survey clothes.
DECIDED: Two things on my own authority, both about display rather than measurement. First, the ring shows one decimal where the slot is not a whole number of seconds and whole seconds where it is - rounding up in whole seconds would have opened an FT4 slot at 8, and 8 is a slot length nothing in this application is cutting on. SecondsLeft keeps its int and its ceiling and nothing on screen reads it any more. Second, the sidecar and the two on-screen sentences name the slot LENGTH rather than the mode name, because the length is what was measured and the name is a label - and because the 4.48 against 5.04 question is open, so a sheet reading FT4 would be answering it. I did NOT settle that question, the candidate sweep or the version scheme.
LICENCE: Work instruction 290 tasks 1 to 7, under the arbiter decision block for step 2, together with PHASE_PLAN.md named-alternatives table - the tree disagrees with this plan, the tree wins, report the mismatch and continue.
COST: one session, seven of seven tasks, no test suite run, every test filtered by exact name and foregrounded, five dotnet builds
ACCOMPLISHED: Hamlet can cut, watch, count and count down a 7.5 second slot from corrected UTC; a capture says on its own face which grid it was cut on; a slot the operator keyed still says he keyed it rather than claiming the band was empty; and every place in the tree still assuming fifteen seconds is named with file and line - 47 arithmetic, 72 prose and 15 on screen in the application, plus 4 arithmetic and 12 prose in the port, which is named and left alone.
FATE: executed
STATE_AFTER: done
STATE_WHY: All four must-pass criteria are met and evidenced. One: a minute of audio cuts 4 FT8 slots on the quarter minutes and 8 FT4 slots at :00 :07.5 :15 :22.5 :30 :37.5 :45 :52.5, boundaries from corrected UTC, and the sidecar prints 9 boundaries, corrected to UTC, on 7.50 s slots, 5.04 s transmission. Two: the ring reads 7.5 down to 0.1 and never 8 and never 0, parity is 0 1 0 1 0 1 0 1 across the minute and unchanged an hour and a day later, and running the countdown to zero transmits nothing. Three: 300 of 300 moments across four FT4 slots resolve the transmitted-slot key correctly with 0 misses. Four: the census, with the arithmetic list in full. FT8 is unchanged - 3888 moments tick-identical to the arithmetic this unit replaced.
APPENDED BY HAND: tools/arbiter/outcome-append.bat could not be run in this session. Three invocation forms were tried once each - a relative path with a redirection, cmd //c, and ./tools/arbiter/outcome-append.bat - and all three were refused by the sandbox; this is a non-interactive session, so there was nobody to approve them. This is the same refusal unit 289 measured and work instruction 290 anticipated. The entry above was written with the file-editing tools in the format outcome-entry.py produces, and the arguments the script would have been given are committed at tools/arbiter/unit290-append.bat so the entry can be replayed rather than reconstructed. Unit 289's two entries above were not touched: the file's own rule is that the entries win, and where they disagree the reading belongs in the instruction and in this unit's report, not inside somebody else's record.
