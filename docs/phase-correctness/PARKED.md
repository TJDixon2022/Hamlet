# Parked - the correctness phase

Findings and asks that block no criterion of the step in hand (PHASE_PLAN.md R54). Each
says what it is, who raised it, and what would bring it back.

## P1 - the nine other adjudicated recordings are scored and kept out of the baseline total

**Raised by unit 410, 2026-09-23.** `TheAdjudicatedReadingsKeepReadingTests` holds twelve
recordings with adjudicated text, not the three the instruction names. Unit 410 scored all
twelve by the same rule and kept the nine outside the baseline's total: **124 edits over
363 characters against inferred keys**, tabled in `baseline.md`. Step 3's keep rule, 3.2,
judges on "the total edit count over all keyed recordings", and which recordings that is
decides what a change is judged on. **Author's, overrulable: the baseline is the four the
instruction names and the nine are printed beside it**, because a row added to the total
after the baseline was set would move the yardstick. Comes back when a ruling says the
nine join the total, or when step 3 opens.

## P2 - at the bench the 17:37 letters are not all right

**Raised by unit 410, 2026-09-23.** PHASE_PLAN.md §1 and work instruction 410 §4 quote
17:37 as reading `CQ CQ CQ DEW B 6 RE D W B`, every letter right and the spaces wrong. That
is the sidecar's reading, the application's on the day. The bench's replay of the WAV,
which every number in this phase is measured on, reads `CQ CQ CQ DE W T E E T E  E ERE D E
T T TB 7E E I`: 29 edits against an inferred key, 15 of them spaces added and 14 letters, 4
wrong and 10 added, with 14 edits left when spaces cost nothing. The 29 edits PHASE_PLAN.md
records were measured on the bench text, so the number stands; the description of it does
not. Step 3 is written for "letters right, word boundaries wrong", and its 3.1 trace is
where this is settled. Comes back at step 3's first unit.

## P3 - which keyed recordings step 3 is judged on, and on which reading

**Raised by unit 412, 2026-09-24.** 3.2 keeps a change only if "the total edit count over all
keyed recordings falls". There are now three sets: the baseline's four, 33 edits over 46;
P1's nine, 124 over 363; and the ten captures of 2026-09-24 keyed by differencing, 14
stretches, scored two ways - **live**, the sidecar's text as the application read it that
night, 41 over 156 and fixed, and **bench**, each WAV replayed cold, 60 over 156, the only one
a change can move. **Author's, overrulable: the ten are tabled in `baseline.md` beside the
baseline and outside its total, and the bench figure is the one a change is judged on**,
for P1's reason: a row added to the total after it was set moves the yardstick. Four of the
ten follow their predecessor by more than 30 s, so part of what they added predates their
WAV, and 004108's bench row aligns its key to unrelated text. Comes back when step 3 opens,
which is the next unit under R64's preference.

## P4 - the benchmark is fourteen captures, not thirteen

**Raised by unit 412, 2026-09-24.** R63, HM-DEC-171 and PHASE_PLAN.md 2.5 say thirteen; the
range they name, `cw-2026-09-24-003901` through `-004550`, holds fourteen, and the last of
them carries the 625 characters and 11 unsure R63 quotes. Unit 412 banked all fourteen. The
count in the rulings is left as written, since a ruling is never edited. Comes back only if
the owner meant a different thirteen.

## P5 - the two new guards are on no carry-forward line

**Raised by unit 412, 2026-09-24.** `TheNumberCannotBeGamedTests` (13 named floors, 60 s) and
`TheBenchmarkIsKeyedTests` (1 printer, 20 s) are what makes 2.2 and 1.6 hold after this unit,
and `docs/carry-forward-tests.txt` names neither; 2.4 asks only for the three floor tests and
the two lines. Step 3's 3.2 names "no named floor from 2.2 is broken", so the unit that opens
step 3 will run the first by name. Comes back if the owner wants it on the engine line.

## P6 - a repair that joins split letters lowers the named count the floors protect

**Raised by unit 413, 2026-09-24.** The fault step 3 attacks is the decoder splitting where it
should join, and a split does not only add spaces: a gap inside a letter read as a gap
between letters turns one letter into several single-element ones. `WB6RED` on 17:37 reads
`W T E E T E  E ERE D`. Joining those back into letters **lowers the named-character count
while the named-element count holds or rises**, and 3.2 and 2.2 both refuse any fall in a
named count. Measured by unit 413 on two different changes, both out:

- **Candidate 1**, the word gap at seven thirds of the character gap when the word heap
  shows no trough: all keyed 217 to 207 edits over 565 against inferred keys, the ten on
  the bench 60 to 46; named characters fell on five capture rows, 021629 27 to 25, 002016
  44 to 41, 004027 40 to 39, 004133 30 to 28, 004510 38 to 37.
- **G1**, an out-of-order held gap reading refused: all keyed 217 to 193, 17:37 29 to 11
  edits reading `CQ CQ CQ DEW B6 RE D W B 7E E I`; 17:37's named floor 46 to 38 and 004133's
  row 30 to 25 named, **its named elements 87 to 88**.

The floors were set at readings that counted each stray `E` and `T` as a named character, so
a repair that reads the letter the sender keyed can fail the floor that exists to stop the
decoder going quiet. This is not an ask for the rule to change. 3.2 is Tim's and is applied
as written. It is the measurement a ruling would need if the owner wants the element count,
or edits with the unsure-per-named guard, to carry the cost check for a joining change.
Under 3.4 this is unit one of three without a kept change. Comes back when the arbiter
authors step 3's next unit.

## P7 - whether the synthetic CQ calls ever join the total 3.2 judges

**Raised by unit 414, 2026-09-24.** Unit 414 scored nine synthetic CQ calls against exact keys,
102 edits over 243 characters, and tabled them in `baseline.md` under their own heading,
**outside every total, including the 217 over 565 that 3.2 judges**. The reason is 1.4: no
synthetic case is ever the sole evidence for keeping a change, and a synthetic row inside the
total would let a change that improves only synthetics satisfy 3.2's first test. **Author's,
overrulable: they stay out.** Whether they ever join it, or join a second total that 3.2 reads
beside the first, is the owner's. Comes back when a ruling asks for it, or when a step 3 unit
wants exact-key evidence beside the inferred.

## P8 - the synthetic CQ calls carry no reference score

**Raised by unit 414, 2026-09-24.** HM-DEC-101 and CLAUDE.md 12.5 say a reference
implementation must score well on a generated fixture before it judges Hamlet. The gate that
enforces it, `CwFixtureCommitTests.TheReferenceHasScoredThisFixture`, reads a committed
`reference` line that `tools/score-fixtures/score-fixtures.py` writes by running `cwdecoder.py`,
and it covers the catalogue's fixtures in `tests/fixtures/cw/receiver` only. **Python cannot
run in a loop session**, so the nine cases in `tests/fixtures/cw/synthetic-cq` have no
reference line. They judge nothing yet: no floor, no assertion on a score, outside every
total. Before any of them is used to judge a change, somebody runs the reference over them.
The catalogue's own record says the reference could not read textbook spacing at 18 wpm (a
dit-to-dah ratio measured at 2.45, under its 2.5 floor), so it may refuse some of these.
Comes back before any synthetic case is used as evidence for a change.

## P9 - whether step 3 can close with the spacing only partly repaired

**Raised by unit 416, 2026-09-24.** Two changes are kept: the narrowed relabel and the path's
own word boundary where no character gap was measured. All keyed recordings 217 to 167 edits
over 565 against inferred keys, 17:37 29 to 19 over 25. 3.1, 3.2, 3.3 and 3.5 are ticked; 3.4
only applies after three units with nothing kept. So every criterion that can be ticked is,
but 17:37 still reads `CQ CQ CQ DEWTEETEEERE D ETTTB 7E E I` against `CQ CQ CQ DE WB6RED WB6RED`:
what is left there is letters, not spaces, and one real word gap after `DE` went with the
inserted ones. At unit 416's exit the trace counts, on the ten and 17:37, 128 boundaries:
joined 90, word kept 22, inserted 14, missing 2. Whether step 3 closes done, or another unit attacks the joined gaps or the
letters, is the arbiter's call. Not blocking; the loop goes on.

**Answered by work instruction 417, 2026-09-24 - the arbiter's, overrulable.** Step 3 stays
partial and is not marked done: 3.4 reads *after three consecutive units with no kept change*,
unit 416 kept two, so the count stands at zero and no unit can honestly flip it now, and a
closed step is closed for good. What is left on 17:37 is letters, not spaces, which is P6's
wall and is logged there rather than chased. Under R64 and R65 the loop moves to the screen,
step 6 criterion 6.7, and the spacing stays as unit 416 left it.

## P10 - the `keying` line's caption names a sweep range the meter no longer uses

**Raised by unit 417, 2026-09-24.** `KeyingRecordLine` captions every sidecar's `keying` line
*an independent sweep of 400 to 1200 Hz in 25 Hz steps*, and `CwKeyingMeter` sweeps through
`KeyingEnvelope.Best`, whose range is the tracker's 300 to 900 Hz (`KeyingEnvelope.cs` 130 and
135). The line's three 6.2 clauses hold on the regenerated 17:37 and 014113 sheets, but 6.2's
lead sentence, *every sentence ... is true of that capture*, does not while the caption names a
range nobody swept. Unit 417 was told to leave `keying` as unit 411 left it, so it did not
repair the caption and did not tick 6.2. **Author's, overrulable:** the fix is the caption
reading the range from `KeyingEnvelope`'s own constants, watched failing first, and it closes
6.2 with it. Not blocking; the loop goes on.

**Answered by work instruction 418, 2026-09-24 - the arbiter's, overrulable.** The caption takes
its range and step from `KeyingEnvelope`'s own constants rather than from a second literal, so
it cannot fall out of date again, and it is watched failing first on 17:37. The caption changes
to match the meter; the meter does not change to match the caption (§6). No decision record is
needed: this makes a sentence true under 6.2 and promises the operator nothing new. Unit 418
also checks every other sentence on the sheet against the tree, because 6.2's lead sentence
covers all of them.

## P11 - the noise beside the tone can be the receiver's stopband

**Raised by unit 417, 2026-09-24.** Both the held figure and the new per-recording `tonePeak`
take the noise beside the tone as the median of the 25 Hz grid from 300 to 900 Hz, leaving out
125 Hz either side of the tone. Measured over the whole of `cw-2026-08-20-014854` and
`-014935`, every grid pitch below 550 Hz sits 25 to 44 dB under the passband, so at a tone near
the passband's edge the figure compares the tone with the filter's stopband: 50.2 and 52.7 dB
there at a pitch nobody measured, on recordings holding keying at no pitch. **The sheet does
not print the figure where the pitch was not measured**, which is where this was seen; on 17:37
and 013347, both with the tone mid-passband, the neighbours sit inside it. The alternative
measured beside it, the tone's own quietest fifth, gave 93.3 dB on 013347, whose tone bin falls
to -51.8 dB between elements under a steady -22 dBFS broadband, so it is no better as it
stands. Whether the noise should be taken inside the passband only is the owner's; it would
touch the held figure too, which HM-DEC-091 protects. Not blocking.

## P12 - two clock readings on the capture sheet run a second behind the press

**Raised by unit 418, 2026-09-24.** Since unit 417 the press measures `tonePeak` over the whole
file on a pool thread before the sheet is composed (`MainWindowViewModel.cs` 11677), and that
takes 871 ms on 17:37 and 885 ms on 013347. Two lines read the clock when the sheet is composed
rather than at the press: `captured` (11729), which can now be a second later than the file's
own stamp taken at 11669, and `broadcast` (12981), which asks whether the radio volunteered
anything in the thirty seconds ending then rather than when the recording ended. Neither states
anything about a signal, so they are outside 6.2 and parked rather than fixed, per work
instruction 418's drop rule. The fix would be one timestamp taken at the press and handed to
both. Not blocking.

## P13 - two of the three Morse blocks on each band state no receiver conditions

**Raised by unit 419, 2026-09-24.** `ReceiverConditions.ForBlock` looks the block's short name
up in `mode-receiver-conditions.json`, which states conditions for `CW`, `FT8` and `FT4`. The
map's Morse family carries three short names: `CW` (9 blocks), `CW DX` (4, the bottom 25 kHz of
80, 40, 20 and 15 m) and `QRP` (7). **The last two state nothing, so tuning into them writes
nothing to the radio.** 7.030 MHz, one of work instruction 419's two CW frequencies, is the
first hertz of the 40 m QRP watering hole (7.030000 to 7.039999 after `Separate`), and the
field table at task 1 prints *the app writes nothing here*: a radio left at preamp 1 on 40 m
stays at preamp 1, against the CW row's own text. The CW row driven directly at 7.030 writes
the text's value. **Stating the CW row for `CW DX` and `QRP` would be two `sameAs` lines**, but
it makes the app write to the radio in 11 blocks where it writes nothing today, and PHASE_PLAN.md
section 6 licenses 7.5 to write less and to carry a band rule, not to write in more places. So
it is the owner's: do the `CW DX` and `QRP` blocks count as entering CW mode? Not blocking under
R65; 7.5 is met where the block states the CW row.

*Answered by R70, 2026-09-24: the CW DX and QRP blocks are CW mode; work instruction 420.*

## P14 - the attenuator's 20 dB write goes out as a plain byte

**Raised by unit 419, 2026-09-24.** `Ic7300Rig.BuildSettingData` sends a value as two BCD bytes
only where the write's range note contains `0000`; the attenuator's reads `00=off, 20=20 dB`, so
20 goes out as the plain byte `0x14`. The read side decodes the same field as BCD
(`CivDecode.DecodeAttenuator`, `CivValues.Level(0x00, payload[0])`), where `0x20` is 20 dB and
`0x14` would be 14. The CW row asks for 20 dB whenever the front end reads overloading, so on a
tune-in into an overloading band the byte sent is not the value asked for; the scripted radio in
task 1's table refuses it and the setup files it `NotConfirmed`. What an IC-7300 does with `11 14`
has not been measured here (FACT-006). A fix changes a byte sent to the radio, which is outside
what 7.5 licenses, so it is parked. Not blocking.

## P15 - the receive advice compares percent reads against raw thresholds

**Raised by unit 419, 2026-09-24.** `ReceiveAdvice.Gain` tests the RF gain read, a percent since
`CivDecode.DecodePercent`, against `OpenGainAbove` = 240 on the raw scale, so a gain at 100 percent
is never open enough and the row says *It is at about 39 percent*, computing 100/255. `UsbLevel`
does the same with `LowUsbLevel` = 77 against a percent, so the radio's shipped 50 percent is
called low. Unit 419 fixed the same shape in `ReceiverSetup` and `Ic7300Rig.SetSettingAsync` and
added `CivDecode.OnReadScale` and `PercentOfLevel`; after its item 2 the advice leaves the RF gain
alone after a CW tune-in, since the CW row owns it, but it still speaks this way anywhere else.
**The fixtures that certify the advice carry the same misunderstanding** (CLAUDE.md 12.5):
`RigWriteTests` builds states with `RfGain` 255 labelled `100%`, 107 labelled `42%` and
`AccUsbAfLevel` 128 labelled `50%`, raw numbers the real read never produces. A fix is the
threshold through `PercentOfLevel` and those fixtures rebuilt on the read's scale, which is
another unit's diff. Not blocking.

## P16 - a source-sweep test's bound is already exceeded at entry

**Raised by unit 419, 2026-09-24.** `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten`
requires the text of `MainWindowViewModel.FollowTheMapAsync`, up to
`EstablishReceiveConditionsAsync`, to be under 6,000 characters, so the sweep cannot pass by
reading the rest of the file. It fails on that bound: the region is 137 lines, about 6,115 bytes,
the same at `4bd85b35` (this unit's entry), at `cb526e01` (unit 418's exit) and at unit 419's
exit, and none of unit 419's hunks in the file fall inside it. The type is on no carry-forward
line, so no entry round ran it; it was red before this unit and is red after, for the same
reason. Its forbidden-write checks, the ones that matter, are not reached while the bound
fails. Raising the bound or trimming the region's comments is a test-shape decision left for the
next unit under 12.6. Not blocking.

## P17 - 3.6 cannot remove a stray letter without lowering a floor

**Raised by work instruction 420's arbiter, 2026-09-24, measured while authoring.** At unit
419's exit 49 of the 51 capture rows read exactly their named floor and 2 read one above it
(43 against 42, 57 against 56); the 13 keyed floors of 2.2 were set at what each recording
read and no letter has moved since, because unit 416's two kept changes moved only spaces.
3.6 asks that stray single-element characters be removed under 3.2's four tests, and test 2
(no named floor broken) and test 4 (no capture row's named count falls) count those very
strays as named characters. Unit 412 measured it: every E and T left out took 12 edits off and
broke 13 of 13 floors. So 3.6 is reachable only by a change that turns a wrong single-element
character into the right one without removing any, and no trace yet says such a change exists.

Ruling proposed, the owner's: a named character the inferred key aligns as added, inside the
scored region, may leave a floor, with every one printed per recording before and after, and
no named character the key aligns as right or wrong may leave. Reasoning: the floor exists so
the decoder cannot score well by going quiet (R59), and a character the key says was never sent
is not reading. Rejected by the arbiter: ruling it here, because R63 and 2.2 say a floor is
never lowered, and an arbiter's ruling may not overrule an earlier one. Not blocking: the loop
works 7.7 and step 6 meanwhile (R64, R65).

*Answered by R71, 2026-09-24: a floor counts characters at or above a stated span bar;
criterion 3.7; work instruction 421.*

## P18 - the conditions file's CW unknowns entry is stale

**Raised by unit 420, logged by work instruction 421.** The conditions file's `unknowns` entry
for `CW` says nothing about CW has been measured, while the file states nine CW conditions for
20 blocks. Raised by unit 420. `TheBlockStatesWhatTheModeNeedsTests.ABlockWithNothingToSayProducesNoClaim`
asserts an unknown whose topic contains `CW`. Not this phase's step 3; not blocking.

## P19 - 3.6's added strays stand above the bar under floors at their count

**Raised by unit 421, logged by work instruction 422's arbiter, 2026-09-24.** Unit
421's trace put the eight added single-element letters on the keyed recordings at raw
`SpanLogLikelihoodRatio` 33.4 to 159.2, above the lowest right letter at 30.8, so R71's
bar of 13.0 reaches none of them. Each sits on a recording whose floor equals its
above-bar count: 17:37 46 of 46 with six of them, `031838` 40 of 40, and `032050` 44 of
44. Removing any one breaks 2.2's floor and 3.2's fourth test, whatever the edit count
does. Joining a stray to a neighbor lowers the count the same way. So 3.6, read as unit
421 read it (an added letter comes off a scored stretch), cannot be met by any change the
floors allow.

**Ruling proposed, the owner's:** a named character that the inferred key aligns as
added, inside a scored stretch, may leave a floor. Every one is printed per recording
before and after, and no character the key aligns as right or wrong may leave. The floor
exists so the decoder cannot score well by going quiet (R59), and a letter the key says
was never sent is not reading.

**Rejected by the arbiter:**
- ruling it itself, because R63, 2.2 and R71 are the owner's and an arbiter may not
  overrule them
- ticking 3.6 on unit 421's two edits, which came off wrong letters and not added ones

Not blocking: the loop works 6.4 meanwhile (R64, R65).

*Answered by R73, HM-DEC-178, work instruction 425.*

## P20 - a ceiling test looks for a countdown that is not in the tree

**Raised by unit 422, 2026-09-24.** `Unit302CeilingHoldsStillTests.TheLiveReadoutsAreStillOnScreen`
fails with *the slot countdown is no longer on the Digital tab*: it looks for an element named
`TurnRingCountText`, and nothing under `src` carries that name, at entry `bdd0070b` or after
unit 422. `HowMuchTheApplicationSaysTests` still lists the name with a budget of 3. Unit 422
changed hover text only and did not run this type at entry, so it cannot say which unit took the
name away. Whether the countdown should be back on the Digital tab, or the test retired with the
readout, is a screen question for step 6 or the owner. Not this unit's (§12.6); not blocking.

## P21 - outside his privileges the top row still grows taller

**Raised by unit 423, 2026-09-24.** Since unit 423 the card, the sun map and the rig face keep their
left edges and widths when Tim tunes outside his privileges (6.3), both ways and at every size the
top-row tests use. **The row's height still follows the card's words**, as it did before: outside
his privileges the green block adds the reassurance sentence and the upgrade button, and the
verdict is longer. Measured headless by `WhereThePanelsStandOutsideHisPrivilegesTests`, General,
14.050 to 14.010 MHz on the CW tab: the card and the rig face are 2 px taller at 1920 x 1040, 22 at
1536 x 824, 42 at 1280 x 720, 1366 x 728 and 1400 x 1040, and 90 at 1200 x 900. Where the row
stands under the top row's 300 px cap, the working panels below it move down by that much. At 1100 x
780, the size Hamlet opens at, the card is 345 px tall inside his privileges and 523 outside, both
over the cap; what the capped row draws there was not measured by unit 423. Holding the height as well would mean
the card scrolls its added lines out of sight inside the row, which §0.5 allows but which hides
the sentence that removes the fear until he scrolls. Whether it should is the owner's call. Not
blocking 6.3, which names columns.

## P22 - the preamp's overload case is read at the tune-in only

**Raised by unit 424, 2026-09-24.** Since HM-DEC-177 the CW row turns the preamp off when the
front end reads overloading (`IC-7300_ENG_FM_12b` page 4-3), and `ReceiverSetup` reads the
`Overflow` flag (`CivReads.Overflow`, `15 07`, which the capture sheet prints as `Overflow`)
**once, at the tune-in**, exactly as it reads it for the attenuator. A band that starts
overloading after he has tuned in is not followed: the preamp stays where the tune-in left it,
and inside a CW block the overload sentence says *the preamp and the attenuator are set by this
mode when you tune in, so Hamlet is not asking you to change them here*, while the manual would
have the preamp off. Following the flag live would write to the radio outside a tune-in, which
R67 and HM-DEC-056 rule against (once per tune-in, his hand wins), so it wants a ruling rather
than a unit's choice. Not blocking.

**Taken up by HM-DEC-179, work instruction 426, 2026-09-24.** The arbiter's reading of R74 and
criterion 7.8, overrulable by Tim: the preamp alone may be written outside a tune-in, on the
`Overflow` flag only, while the block owns it, never while transmitting, and never after his
hand has moved it.

## P23 - 6 m has no block on the map, and its top edge is not in the tree

**Raised by unit 424, 2026-09-24.** The CW row states preamp 2 from 50.000 to 54.000 MHz
(HM-DEC-177), and `ReceiverSetup` writes 2 when driven there directly, but `HfBands.Names` is 80
to 10 m and `data/bands/us-neighborhoods.json` has no 6 m rows, so tuning to 50.100 in the app
finds no block and writes nothing. 160 m and 12 m are the same (1.810 and 24.900 find no block).
The 54 MHz top edge is 47 CFR 97.301's and no file in the tree carries it:
`data/privileges/us-part97-privileges.json` has no 6 m row. Adding bands to the map is the scope
decision `HfBands` names. Not blocking.

## P24 - the attenuator's sentence gives the quiet band's reason when it writes 20 dB

**Raised by unit 424, 2026-09-24.** Since unit 424 the setup says a conditional row as the value
the radio read back rather than as the rule. For the attenuator that will read *I set the
attenuator to 20 dB because twenty decibels thrown away on a signal that had none to spare*
once a 20 dB write lands, which gives the off case's reason for the on case. It cannot be heard
today, because the 20 dB write is refused (P14). The row's `says` is the attenuator's, which is
another receive condition and not this unit's; its page in `IC-7300_ENG_FM_12b` was not checked,
because the manual is not in the tree. Not blocking.

## P25 - the decision log's index has no HM-DEC-166 row

**Found by unit 424, 2026-09-24.** `DecisionLogOrderTests.EveryRulingAppearsOnceAndTheGapsAreTheKnownOnes`
is red at entry `e4085d43` and after: *Expected [105, 136], Actual [105, 136, 166]*.
`DECISIONS.md` holds HM-DEC-166 and the `CLAUDE.md` §1 table has no row for it. Unit 424 did not
repair it (report, repair nothing). Also found: work instruction 424 named its ruling HM-DEC-176,
which unit 421's floor-bar ruling already holds, so it is recorded as HM-DEC-177. Not blocking.

## P26 - a ceiling test's added paragraph comes back 64 characters longer at some times

**Found by unit 424, 2026-09-24.** `HowMuchTheApplicationSaysTests.AddingASentenceToACappedSurfaceTurnsItRed`
failed twice, its output files written at 18:30:19 and 18:31:13 Eastern, with *Expected 1779,
Actual 1843* (1379 before, 400 added), and passed at 18:31:47 on the task 2 tree and at 18:32:34
on the same source that had failed (times are the output files' own, read from the disk). Something on the Digital tab adds 64 characters between the two measurements at some
wall-clock times. Not traced by unit 424. Not blocking.

## P27 - once he has moved on, the overload sentence asks for the attenuator the CW tune-in left off

**Found by unit 426, 2026-09-24.** With the preamp now turned off on a live overload
(HM-DEC-179), the overload sentence outside a block that owns the front end reads *the preamp is
already off, so the next thing to try is the attenuator. Hold P.AMP/ATT for a moment to bring it
in*, 12 times in unit 426's trace (six frequencies, two moved-on views, the overload point). It
is true, and it is on a field no tune-in owns there; before unit 426 the same view asked him to
turn off the preamp instead. The attenuator is a condition the CW row states (20 dB while
overloading) and is set at the tune-in only; HM-DEC-179 forbids following it, and its 20 dB write
is P14. Whether a voice may ask for a field the last CW tune-in set once he has moved into a block
that does not state it is the owner's reading of 7.8's third clause. Not blocking.

## P28 - Receive Help's USB level has the RF gain's scale mismatch

**Found by unit 426, 2026-09-24.** `ReceiveAdvice.UsbLevel` compares the `AccUsbAfLevel` reading,
which `CivDecode.DecodePercent` gives as a percent, to `LowUsbLevel = 77` on the 0 to 255 write
scale, the same mismatch unit 426 repaired for the RF gain (R65, HM-DEC-172). A level at 50
percent, where the radio ships, would read as low and be asked to turn up. It is not one of the
nine fields the CW row states and was not changed. Not blocking.

## P29 - Unit297CardSentenceTests has two reds that predate unit 427

**Found by unit 427, 2026-09-24.** `TheDetailKeepsWhatTheFaceGaveUp` expects *bearing* in the
card's hover and `NoGridMeansNoDistance` expects *has not put a grid square on the air*; neither
string is in the card any longer (the second left the source in b12b6425, 2026-09-10; the
bearing left under Tim's no-bearing ruling). Both are red on the committed tree with unit 427's
work stashed, so they are not this unit's. The type is on no carry-forward line. Whether the two
expectations are retired or the words come back is a later unit's. Not blocking.

## P30 - the grid-to-place table is drawn from border coordinates, not from a boundary file

**Found by unit 427, 2026-09-24.** HM-DEC-180 needs to know which entity a grid square is in, and
nothing in the tree could say. `data/callsigns/grid-places.json` holds twelve boxes - California,
the lower 48, Alaska, Hawaii - drawn by the unit's author from known border coordinates, each
with its reason, and a square is placed only where the whole square lies inside one box. It is
not read from a cited boundary file, it covers only those four places, and a contradiction
anywhere else (a Canadian call from a US grid, a US call from Puerto Rico) is not caught: the
prefix stands there as before. Whether to cite a boundary source and widen the table is the
owner's. Not blocking.

## P31 - item 1: on a row with no callsign, *make a card anyway* is a note and makes no card

**Found by unit 427, 2026-09-24** (`TheFourteenOnScreenTests.Item01TheMenu`). Expected, the
owner's list item 1: *Capture and make a card anyway always present, and make a card anyway
makes a card, not a note.* There: on a row with a callsign it is a command and one press makes a
card (0 -> 1); on a row with none (`e5 ttt tu ee`) it reads *Make a card anyway - Hamlet read no
callsign on this row, so a card would have nobody on it. Capture the audio and the card follows
when a call comes through.*, carries no command, and a press makes no card (1 -> 1).
`RowMenuAlwaysFor` says this is deliberate under the 2026-09-06 rule and §0.0. A card for nobody
is a feature and a ruling, not a word. Not blocking.

## P32 - item 13: a station's first over, still arriving, opens no card

**Found by unit 427, 2026-09-24** (`Item13HisCardWhileHeTalks`). Expected, item 13: *KC3QIS de
VE3YX* opens his card mid-over marked *he is sending to you*, buttons held until his hand-back;
the right-click names him; any keyboard mode. There, on PSK31 and Olivia alike, with the text
`KC3QIS de VE3YX GM OM TNX FER THE CALL NAME HERE IS` and no hand-back yet: no card opens, the
row names no station, and the right-click offers only Capture and the no-callsign note. A station
whose earlier over completed does get *he is still sending* on his card while a new over arrives
(item 8), which is what exists. Unit 390's commits (406d1efe to 74fd016a) show no task that built
the parse of a growing row. A feature, not a word. Not blocking.

## P33 - item 8: the hold also lets go on carrier drop, and its sentence promises a send that does not happen

**Found by unit 427, 2026-09-24** (`Item08TheLiveCarrier`). The owner's list says the hold
releases on K or BTU, not on carrier drop, by design. There: it releases on K or BTU **and** on
carrier drop (`HisCarrierIsLive` answers false for an ended row, and
`TheCarrierHoldsTheButtonsTests.WhenHisCarrierDropsTheSamePressGoesOut` asserts it). The refusal
reads *Hamlet did not send that: he is still sending. ... It goes out the moment his carrier
drops.* - but nothing goes out when his carrier drops; the operator presses again. That sentence
is on the send path (§0.2) and the release is parked by the instruction; both are the owner's.
Not blocking.

## P34 - a PSK31 CQ that carries a grid is read as no station at all

**Found by unit 427, 2026-09-24** (`Item02TheRowHover`). `CQ CQ CQ de VE3YX VE3YX FN03 pse K`
gives a row whose hover has no station, no entity and no kind - only the offset, the strength
and the time - where `CQ CQ CQ de W1AW W1AW pse K` gives all of them, and an over carrying
`QTH FN10` gives the grid and the distance. The parser is the engine's and was not opened. It
bears on the phase goal - Hamlet reads a CQ call correctly - for the keyboard modes. Not blocking.

## P35 - bank the 2026-09-25 traffic net and re-run the trace on it

**Raised by unit 428, 2026-09-24, section 4 item 1; parked by unit 429 task 0.** Verbatim:

**1. Bank the 2026-09-25 traffic net and re-run the trace on it.** Proposed ruling: the 7.052 captures the instruction describes are copied into `tests/fixtures/cw/captured/unadjudicated` with a key file for at least the stretches the instruction quotes (`OPERATION`, `ALL LOGS WILL BE UPLOADED`). A unit then adds them to the trace's recordings and re-runs `WhatTheNeighborsSayTests` unchanged. Reasoning: HM-DEC-181 was drawn from that recording, and the keyed corpus holds only 17 added letters, 6 of them in one weak passage on 17:37. A strong, single-signal recording with hundreds of litter characters is the case the ruling describes and the one the tree lacks. Rejected: building the rule anyway on the instruction's quoted figures, because the trace on the tree's recordings says it costs right letters, and section 10 forbids building what the trace does not support.

Not blocking.

## P36 - whether 3.6 is reachable on the span at all

**Raised by unit 428, 2026-09-24, section 4 item 2; parked by unit 429 task 0.** Verbatim:

**2. Whether 3.6 is reachable on the span at all.** Proposed ruling: with raw span, per-hop span (units 421 and 425), span over neighbors, and span per mark over neighbors all traced and none separating the 8 added single elements from the 62 right ones, the next 3.6 unit traces something other than a span figure, or 3.6 waits for item 1's recording. Reasoning: four figures measured on one corpus all overlap. Rejected: lowering a floor to admit a relative bar, which R71 and R73 forbid outside key-aligned added letters.

Not blocking.

## P37 - the held-gap litter on 004535

**Raised by unit 429, 2026-09-25, section 4 item 3; parked by unit 430 task 0.** Verbatim:

**3. The held-gap litter on 004535.**
- **Proposed ruling:** parked as a second, separate cause. It is a trace for a later step 3 or step 7 unit, not 7.4's.
- **Reasoning:** held gaps of 12/345/306 ms, with a word gap under the character gap, arrive with the run of `E`, and the cold decode of the same audio holds 48/238/443. One reading.
- **Rejected:** folding it into 7.4, which would put two changes against one criterion.

Not blocking.

## P38 - the sidecar's counters

**Raised by unit 429, 2026-09-25, section 4 item 4; parked by unit 430 task 0.** Verbatim:

**4. The sidecar's counters.** For the record.
- **Proposed ruling:** a later unit checks what `inThis`, `characters` and `elements` count and words them to match.
- **Reasoning:** 003901 says 92 characters and 223 elements "since the transcript was cleared", beside a text of 27 named characters. The figures fit a count since the decoder started, 114 s earlier.
- **Rejected:** repairing it here; section 10 forbids it and it touches nothing 7.3 or 7.4 depends on.

Not blocking.

## P39 - what the second 7.4 unit builds

**Raised by unit 430, 2026-09-25, section 4 item 1; parked by unit 431 task 0.** Verbatim:

**1. What the second 7.4 unit builds.**
- **Proposed ruling:** the next 7.4 unit builds the variant's intent correctly. The mixdown follows at once any move that a survey read has confirmed since the mix's own pitch was last set, not only one confirmed on the read just before the move, and still waits after a `Switch` made on the read that confirmed it. It is judged on the same four tests, with `032113` watched first.
- **Reasoning:** both attempts cured the opening and lowered the keyed total. Both failed only on `032113`. The measured reason for the variant's failure is a hold that spanned two reads, which that rule covers. This unit could not build it: the instruction allows no third attempt.
- **Rejected:**
  - Moving the tracker's choice, because the trace shows HM-DEC-095 governing the move.
  - Excusing `032113` as outside the scored stretch, because R73 excuses only key-aligned added characters inside one.

Not blocking.

## P40 - AHeldPitchDoesNotOutliveItsEvidenceTests red at entry, 1 of 4

**Raised by unit 430, 2026-09-25, section 4 item 2; parked by unit 431 task 0.** Verbatim:

**2. `AHeldPitchDoesNotOutliveItsEvidenceTests` is red at entry, 1 of 4.**
- **Proposed ruling:** open an issue and have a later unit find when `CwDecoder.Retuned()` stopped releasing on a QSY. The unit that finds it does not repair it without a ruling.
- **Reasoning:**
  - `Retuned()` is now only `Unlock()`.
  - The three red tests expect a move of the dial to release, in their words, the pitch measured before it, the held peak, and the speed.
  - The type is not on the carry-forward list, so no unit's round would have caught it.
  - This is the 2026-08-26 fault the file describes, a decoder still pointed at a pitch measured on another frequency.
- **Rejected:** repairing it here, which section 10 forbids.

Not blocking.

## P41 - the named pitch test

**Raised by unit 430, 2026-09-25, section 4 item 3; parked by unit 431 task 0.** Verbatim:

**3. The instruction's named test.** For the record.
- **Proposed ruling:** future instructions name `CwAdjudicationTests.ASignalOffTheExpectedPitchIsFoundInRealisticAudio`.
- **Reasoning:** `ASignalAtTheWrongPitchIsStillFound` exists in the tree only in comments. Two pitch-named files, `EveryElementCarriesItsOwnPitchTests` and `ThePeakFindsThePitchTheTrackerMissedTests`, are `Compile Remove`d, so a "run every pitch type" line runs 0 tests on them.
- **Rejected:** nothing.

Not blocking.

## P42 - the opening's figure

**Raised by unit 430, 2026-09-25, section 4 item 4; parked by unit 431 task 0.** Verbatim:

**4. The opening's figure.** For the record.
- **Proposed ruling:** 7.4's figure is the stream opening's text set beside the cold group `EANQNID`, and not its named count alone.
- **Reasoning:** a repair that removes litter lowers the named count, 31 to 21 here.
- **Rejected:** scoring the opening against a key, which no recording in the tree carries (R72 rules out inventing one).

Not blocking.

## P43 - step 3 closes partial

**Written by unit 431, 2026-09-25, task 3, under criterion 3.4.** Three consecutive step 3 units
kept no change, so the trace and the measurements are written here and step 3 closes partial rather
than holding the loop. 3.6 stays unmet.

**The three units.**
- **Unit 425.** Traced every single-element named character on the keyed recordings and the 29
  unkeyed rows against measures other than span: the window Gate score, the gaps either side in
  units, key-down length, standing alone between word gaps, pitch off the sender's, and energy over
  its neighbors, one at a time and in pairs. No measure and no pair separated the 8 added
  single-element letters from the right ones without taking right or unkeyed letters with them.
  The trace stopped it: no change was built.
- **Unit 428.** Traced each emitted character's span against the median span of its neighbors, and
  span per mark over neighbors. Neither separated the 8 added single elements from the 62 right
  ones (P36). The trace stopped it: no change was built, because the rule would cost right letters
  (P35).
- **Unit 431.** Re-applied unit 413's G1 (`687aab1a`): `CwUnitEstimator.MeasureGaps` returns the
  textbook gaps when the clipped character gap stands at or past the word gap. Built in `856d226e`
  and taken back out in `4701969d`. It failed 3.2's second test on 17:37 and its fourth on
  `004133`, and R73, read for a join, excused neither (below).

**Unit 431's task 1 count.** 5 of the 8 single-element added letters were read under a held reading
whose character gap stood at or past its word gap. All five are on 17:37:
- `T` 22.540 s and `E` 22.720 s, under character 552 ms and word 295 ms;
- `E` 26.250 s, `T` 26.515 s and `T` 26.820 s, under character 828 ms and word 250 ms.

The other three: `E` at 17:37 21.170 s was read under a held reading in order, and `T` on `031838`
and `T` on `032050` were read under textbook gaps.

**Unit 431's task 2, 3.2's four tests as numbers.**

| Test | Entry | Under G1 | Result |
|---|---|---|---|
| 1. All keyed edits fall | 165 over 565 | 154 over 565 | pass |
| 2. No named floor from 2.2 broken | 13 of 13 | 12 of 13; 17:37 46 to 38 | fail |
| 3. Adjudicated readings unchanged or onto their own text | 13 of 13 | 13 of 13; `032012` `ARTICLESOR` to `ARTICLES OR`, its adjudicated text (R66) | pass |
| 4. No capture row's above-bar count falls, except under R73 | 51 of 51 | 50 of 51; `004133` 28 to 25 | fail |

Why R73 did not reach either fall:
- **`004133`.** Seven characters left, and every one was outside every scored stretch: `T` 7.040 s,
  `E` 7.260 s, `T` 8.265 s, `E` 8.485 s, `E` 10.855 s, `K` 14.510 s, which became a placeholder,
  and `A` 19.195 s, which read `U`. R73 reaches only inside a scored stretch.
- **17:37.** The whole fall of 8 is inside its scored region. But it is larger than the fall in
  key-aligned added characters there, 8 to 1, which is 7. The characters that left also include
  the key-aligned right `E` at 22.990 s, which became part of the right `B` settled at the same
  moment. The characters that left were:
  - added: `T` 22.540, `E` 22.720, `E` 26.250, `T` 26.515, `T` 26.820 s;
  - wrong: `T` 23.460, `E` 23.740, `E` 24.020, `E` 24.395, `T` 27.350 s;
  - right: `E` 22.990 s.

  Under G1, 17:37 read `CQ CQ CQ DEWB6 RE D W B 7E E I`, 10 edits over 25, where entry reads
  `CQ CQ CQ DEWTEETEEERE D ETTTB 7E E I`, 19 over 25. The key is `CQ CQ CQ DE WB6RED WB6RED`.
  Added letters on the keyed recordings went from 17 to 10, and the single-element ones from 8 to 2.

The narrower variant was not built. It was allowed only if the first attempt failed on a single
row or test, and G1 failed on two rows and two tests.

**The numbers step 3 leaves standing.**
- 165 edits over 565 characters on all keyed recordings, against inferred keys.
- 17:37: 19 edits over its 25-character scored region.
- 17 added letters on the keyed recordings, 8 of them single-element.

**What would reopen it.** G1 is the only change on record that turns 17:37's split `W T E E T E`
back into `WB6`. It is held off by `004133`'s unscored stretch and by one right `E` on 17:37.
Either a key covering `004133` from 7.0 to 11.0 s (P35's recording is the other route), or an
owner's reading of R73 that counts a join by the letters it leaves rather than the characters it
removes, would let it be judged again. Neither is this unit's to make.

Not blocking.
