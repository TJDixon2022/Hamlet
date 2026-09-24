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
