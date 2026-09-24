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
