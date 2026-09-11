PHASE: Hamlet works PSK31 the way it works FT8
PHASE_SET: 2026-09-11
DESCRIPTION: A third digital mode with the same two cards, the same one-click exchange, the same log and the same achievements, on a modem Hamlet builds itself.
STEP: 0 | done | The seam - PSK31 exists as a mode. Family colour, the cited 14.070 watering hole, a tab, a log mode and submode, a telemetry mode field. Pressing it tunes USB-D to 14.070 and shows an empty panel that names itself. Nothing decodes.
STEP: 1 | not started | Hear one - a single-channel BPSK demodulator and varicode decoder with AFC and bit-clock recovery, proved against recorded fixtures with a stated character error rate.
STEP: 2 | not started | Hear everyone - signals found across the passband, each with its own demodulator, into the same decoded-text list FT8 uses, with frequency, strength and text as it arrives.
STEP: 3 | not started | Read the conversation - the parser that turns free text into exchange state, with an explicit unknown. The CQ list is the rows whose text parses as a CQ. Worked-fade, entity resolution and the nudge reuse unchanged.
STEP: 4 | not started | Say it - the modulator and the macro exchange through the proved transmit chain. One click, one transmission. Receipt and conversation cards on the same panel; the slot clock replaced by whose turn it is. Proved by loopback at the bench.
STEP: 5 | not started | Log and achievements - RST in the log, ADIF PSK with submode PSK31, and the PSK31 records revealed by the first contact and absent until then.
STEP: 6 | not started | Tim at the radio - tune 14.070, see text, work a station, log it. Only he can close it.

## UNIT 312 - STEP 0

STEP: 0
APPROACH: Read the PSK31 phase plan in full, then traced how FT4 was added as a mode and checked every claim the instruction makes against the tree before changing a line.
HIT: MUCH OF THE SEAM IS ALREADY THERE AND ONE PIECE OF IT IS DISHONEST. PSK31 is already one of DigitalModeChip.Labels, already carries the cited 14.070 row in us-neighborhoods.json, already writes USB-D through ModeFollowPlan line 219, and is already in ContactModes.Six as MODE=PSK with SUBMODE=PSK31 from unit 287. What is missing is that DigitalModeFor maps PSK31 onto DigitalMode.Ft8, so pressing PSK31 tunes to 14.070 and then runs FT8 slots and the FT8 decoder on it, while the panel says slots here run 15 seconds on a mode that has no slots.
MOVE: Name the gap rather than filling it silently, and make the panel say what is true for this mode.
WHY: A tab that tunes correctly and then decodes as another mode is worse than one that does nothing, because the screen looks busy and every word on it is about a mode the operator did not press.
DECIDED: Nothing yet. Task 1 changes no production file except the version.
LICENCE: Work instruction 312 tasks 1 to 4, PHASE_PLAN.md sections 1 to R7 and HM-DEC-155.
COST: one session, no test suite run, the carry-forward list of 19 named types run filtered and foregrounded at 106 of 106 green before anything changed
ACCOMPLISHED: The phase record is open and the starting position is measured rather than assumed.
FATE: executed
STATE_AFTER: partial
STATE_WHY: STEP 0 IS CARRIED AT partial UNTIL THE SEAM IS BUILT. Task 1 only measured. Its must-pass criteria - the panel naming the mode, the tune, the log mode and the binding test - are tasks 2 and onward.

### ALSO RECORDED FOR UNIT 312 - STEP 0

A second append for the same unit and the same step, called as UNIT 312.
One unit is one entry, so what this route recorded is folded in here
rather than written as a second entry. Only what differs is listed.

APPROACH: Measured the seam before building it, then built only the part that was missing and made the panel say what is true.
HIT: STEP 0 WAS MOSTLY ALREADY THERE AND THE MISSING PIECE WAS AN UNTRUTHFUL PANEL. PSK31 was on the mode strip, had the cited 14.070 row, wrote USB-D through ModeFollowPlan and was in ContactModes.Six as MODE=PSK with SUBMODE=PSK31. DigitalModeFor mapped it onto DigitalMode.Ft8 - the right default for a slot grid, used as an answer to can this be read - so pressing PSK31 tuned correctly and then ran FT8 slots and the FT8 decoder on a PSK31 frequency under a line saying slots here run 15 seconds. AND NO RIBBON ON THE MAP LIT FOR ANY MODE, contrary to the instruction, so task 4 built the mechanism rather than copying it.
MOVE: Name what has a decoder, say plainly that this one does not, carry the label into the record, and pick out the block he asked for.
DECIDED: Two on the author authority, both reported. The strip line answers for the pressed label rather than the mapped decoder, because the sentence is about the label. And the engine enum keeps its two members: a third for a mode with no decoder would assert a capability the application does not have, which is what that type own remarks already say.
LICENCE: Work instruction 312 tasks 1 to 4, PHASE_PLAN.md sections 1 to R7, HM-DEC-032, HM-DEC-054, HM-DEC-092 and HM-DEC-155.
COST: one session, no test suite run, every run filtered by exact name and foregrounded with a 480 s timeout
ACCOMPLISHED: Pressing PSK31 takes the radio to the cited watering hole in USB-D, the panel says the mode by name and says Hamlet cannot read it yet, the log already spells it the way ADIF does, the record can tell a PSK31 evening from an FT8 one, and the map picks out the block he asked for. Nothing decodes, nothing transmits, and the reference is pinned at one commit with nothing read from it.
STATE_AFTER: done
STATE_WHY: STEP 0 IS done. Every must-pass met: the press tunes to the cited 14.070 in USB-D, the panel names the mode, the log offers it, BindingHealthTests is green, and the family is the map own answer with this unit adding ink and never a fill. The nice-to-pass is met too. NOTHING WAS MEASURED AT A RADIO - FACT-006, this machine has none - so every appearance claim is computed.
