PHASE: Hamlet works PSK31 the way it works FT8
PHASE_SET: 2026-09-11
DESCRIPTION: A third digital mode with the same two cards, the same one-click exchange, the same log and the same achievements, on a modem Hamlet builds itself.
STEP: 0 | done | The seam - PSK31 exists as a mode. Family colour, the cited 14.070 watering hole, a tab, a log mode and submode, a telemetry mode field. Pressing it tunes USB-D to 14.070 and shows an empty panel that names itself. Nothing decodes.
STEP: 1 | done | Hear one - a single-channel BPSK demodulator and varicode decoder with AFC and bit-clock recovery, proved against recorded fixtures with a stated character error rate.
STEP: 2 | done | Hear everyone - signals found across the passband, each with its own demodulator, into the same decoded-text list FT8 uses, with frequency, strength and text as it arrives.
STEP: 3 | done | Read the conversation - the parser that turns free text into exchange state, with an explicit unknown. The CQ list is the rows whose text parses as a CQ. Worked-fade, entity resolution and the nudge reuse unchanged.
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

## UNIT 313 - STEP 0

STEP: 0
APPROACH: Read Tim own screenshot directly, sampled the glyph colour out of its pixels, then stood the real window up headless with six cards and read back where the layout put each one.
HIT: THE PANEL HAS NO SCROLL CONTAINER AT ALL AND THE POPUP MAGNIFIES FOUR TIMES. Measured on the real window: the cards control lays out 1748 px of cards inside a panel 264 px tall, five of six cards start below the bottom edge, and the nearest ScrollViewer above the cards is NONE. The card is 301 px wide inside a 331 px panel, so nothing is clipped horizontally - the truncation on the screenshot is the screenshot own crop at 1207 px. The F4DIA popup is 3.98x, the zoom floor unit 310 set permits 4.13x, and a same-state contact hits exactly that. AND THE GREEN CIRCLED GLYPHS ARE UNIT 309 ACHIEVEMENT QUILLS, decode green 3B6D11 sampled exactly off 21 pixels of the screenshot.
MOVE: Cap the magnification rather than floor the frame, and put the cards in the same kind of scroll container the two lists beside them already use.
WHY: A floor on the frame was the wrong instrument: tight enough to stay sharp is a floor of the whole file, which is no zoom at all. And a card that cannot be reached is a contact that cannot be logged.
DECIDED: Nothing yet. Task 1 changed no production file except the version.
LICENCE: Work instruction 313 tasks 1 to 4 under R9 and R10 of 2026-09-11, with unit 310 R1 to R8 unchanged.
COST: one session, no test suite run, sixteen named types run filtered and foregrounded before anything changed
ACCOMPLISHED: The two faults are measured rather than described, and the third thing the instruction asked about is settled: the achievement marks Tim called a total failure are on his list and drawn in the ruled colour.
FATE: executed
STATE_AFTER: done
STATE_WHY: THIS UNIT ADVANCES NO STEP OF THE PSK31 PHASE and is recorded against step 0 because that is the step the phase has reached. It is carried repair of two FT8 and FT4 faults on surfaces PSK31 will inherit. NOTHING WAS MEASURED AT A RADIO.

## UNIT 314 - STEP 1

STEP: 1
APPROACH: Disconnected the FT8 machinery from the PSK31 tab before building anything.
HIT: THE TAB WAS DECODING AND TRANSMITTING FT8 ON 14.070 AND THE OPERATOR FOUND IT BY USING IT. Unit 312 wrote the sentence onto the panel and left the slot tick running: OnSlotTick gates on IsDigitalMode, which is the tab rather than the sub-mode, so the watch went on cutting FT8 slots and the FT8 decoder went on reading them. SendMessage composes through DigitalModeFor, which answers Ft8 for everything that is not FT4, so his answer put FT8 tones on the PSK31 calling frequency. Measured after the gates: thirty ticks under PSK31 give 0 slot looks and 0 rows; the same thirty under FT8 and FT4 give 30 looks each.
MOVE: Two gates at the two doors: the tick returns before the watch is asked, and the one send door refuses in words.
WHY: A tab that transmits the wrong mode into the wrong segment is worse than a tab that does nothing.
DECIDED: One predicate for hearing and a separate one for sending, because the next step gives PSK31 a decoder and must not thereby re-open the transmitter.
LICENCE: Work instruction 314 task 1 under Tim rulings of 2026-09-11 on the tab and on the receipt.
COST: one session, no suite, 24 named types at 146 of 146 green before anything changed
ACCOMPLISHED: The PSK31 tab can no longer decode as another mode or transmit as one, and the CQ receipt offers nothing to log.
FATE: executed
STATE_AFTER: partial
STATE_WHY: STEP 1 IS CARRIED AT partial. Task 1 is a step-0 defect repaired in passing and is not an advance; the demodulator that would close step 1 is tasks 2 to 4. NOTHING WAS MEASURED AT A RADIO.

### ALSO RECORDED FOR UNIT 314 - STEP 1

A second append for the same unit and the same step, called as UNIT 314.
One unit is one entry, so what this route recorded is folded in here
rather than written as a second entry. Only what differs is listed.

APPROACH: Built the decoder against the shipped fixtures after disconnecting the machinery that was pretending to be one.
HIT: IT READS EVERY QSO FIXTURE AT CER 0.0000, WHICH IS WHAT THE OFFLINE REFERENCE SCORED. Clean, +10 dB, +3 dB, -3 dB, a carrier drifting 20 Hz in a minute, and a second station 500 Hz away: 255 of 255 characters each, none of the other station text, every one in 0.02 seconds. The noise-only fixture emits nothing where the unsquelched reference emitted 112 garbage characters. ONE REAL BUG, FOUND BY MEASURING RATHER THAN REASONING: the timing profile decayed faster than a single bit, so the clock chased whichever symbol had just gone past. It cost CER 0.1976 at +10 dB where clean was 0.0237. The cadence is fixed and nudged one sample at a time now.
MOVE: One channel, one offset, fed from the same audio tap the FT8 decoder uses, with characters on the panel as they complete.
WHY: PSK31 has no slots, so there is nothing to wait for the end of, and a panel that filled only when the audio ran out would show nothing at all on a live band.
DECIDED: The squelch measures shape rather than loudness, because loudness cannot tell a station from a burst of noise at the same level. Its number is 0.90 on a scale whose two ends are 0.637 for uniform noise phase and 1.0 for clean keying.
LICENCE: Work instruction 314 tasks 1 to 5, PHASE_PLAN.md sections 1, 3, R1, R3 and R5, and Tim rulings of 2026-09-11.
COST: one session, no test suite run, 24 named types at 146 of 146 green before anything changed and 170 green after
ACCOMPLISHED: Pressing PSK31 tunes to the cited watering hole, listens at one spot and shows text as it arrives, and can neither decode as another mode nor transmit as one.
STATE_AFTER: done
STATE_WHY: STEP 1 IS done. Every must-pass is met: a recorded fixture decodes to its known text with the error rate stated, the varicode table is cited, the reference commit is recorded with the clone outside the tree, the squelch rule is stated and the noise-only fixture produces nothing. The nice-to-pass is met too - the drifting carrier holds lock. NOTHING WAS MEASURED AT A RADIO and every fixture is synthetic, so all of it is an indication rather than a finding.

## UNIT 315 - STEP 2

UNIT_AS_CALLED: 1
STEP: 2
APPROACH: find every PSK31 carrier across the passband by measurement and run one demodulator per carrier into the FT8 decoded-text list
HIT: section 4 wants a ruling: banked - Several rulings are asked for, on the 1500 Hz reference text, the search floor, the ruling id, the squelch delay and the hover wording, but at most they touch how firmly one must-pass is shown, and the other step 2 criteria and later work do not wait on them.
MOVE: continue
WHY: Steps 0 and 1 are done and proved; step 2 is the next in the pipeline and its ground is the single-channel demodulator unit 314 built. Nothing blocks it.
DECIDED: the search rule and the retire rule are the unit's to choose and state, within the plan's requirement that both be measurements with a stated number; and the seed unit records the phase-setting ruling in DECISIONS.md and recovers the FT4 run files, because the phase was installed without install-phase.bat
LICENCE: PHASE_PLAN.md step 2 entry and exit criteria, A6 branching, HM-DEC-155; install-phase.bat's stated seed-unit duties; CLAUDE.md A12.1 for recording a ruling the owner gave
COST: 24.949754000000002
ACCOMPLISHED: pressing PSK31 shows every station on the band as its own row of text, not one spot
FATE: executed
STATE_AFTER: done
STATE_WHY: All six must pass criteria are backed by measurements, with two and four signal rows at CER 0.0000, zero rows on noise, a stated 2.5 s retire rule measured at 1.75 to 2.50 s, a described search with no offset list and a real time ratio of 0.004, and the weakest point is that the 1500 Hz reference was derived rather than recorded.

## UNIT 316 - STEP 3

UNIT_AS_CALLED: 2
STEP: 3
APPROACH: parse each PSK31 message into speaker, addressee, kind, turnover and certainty against the transcript corpus, split the growing row at turnover words, and feed the parse to the unchanged FT8 row readers
HIT: section 4 wants a ruling: banked - Rulings are wanted on the replaced assertion, the guess and unknown wording, roger recognition, the split rule edges and the callsign copies, but they only confirm choices the unit has already made or affect step 4 planning, so step 3 is not blocked and its remaining work can go on without an answer.
MOVE: continue
WHY: Steps 0 to 2 are done, step 2 by the separate reading of unit 315's report, and step 3 is next in the plan's one pipeline with nothing blocking it; the loop test finds no approach tried on step 3. Unit 315's eight section 4 items are logged as asks 17 to 24, not chased - none bears on a step 3 criterion.
DECIDED: three on the arbiter's authority. The corpus lines are joined into one stream to make A3.4's 'a message is what arrived between two turnovers' measurable. The corpus's self-contradiction on 05-garbled (expected 0.2, two of five lines uncertain) is reported under both readings and not resolved. Task 5, end to end from the four-signal audio, is the drop candidate because it moves no must-pass.
LICENCE: PHASE_PLAN.md step 3 entry and exit criteria, A1, A3.4, A3.5, AR1, AR3, AR9 and A6 branching; ARBITER.md A6, which makes the tasks, the tiering and the drop candidate the arbiter's
COST: 19.498643
ACCOMPLISHED: with PSK31 pressed, the CQ filter shows the stations calling CQ, a station calling Tim lands on his side, and every row says who is speaking and whether Hamlet is sure - with the same fade, country and quill an FT8 row gets, and nothing that transmits
FATE: executed
STATE_AFTER: done
STATE_WHY: The report backs every criterion with a measurement, 32 of 32 corpus lines matched with none wrongly certain, garbled uncertainty at 0.40 against 0.20 expected, own callsign recognised on 8 lines, the CQ filter taking 12 of 12 CQ rows and 0 of 28 others, an empty diff on the reused code with the same Costa Rica quill, IsGuess plus a visible word, no name or QTH members, and 03 no report ending on 73.
