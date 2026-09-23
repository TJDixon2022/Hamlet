# Unit 408 - the floors re-measured as named characters (3.9)

Work instruction 408, step 3 criteria 3.9, 3.6 and 3.7 of *CW decodes again*. Every number here is
an indication (FACT-004); this machine has no radio (FACT-006). A count says nothing about
correctness (§0.0), and no sentence here says a change makes CW read.

## 1. The rule and the harness (R57, HM-DEC-168)

`TheCapturesThatDecodeKeepDecodingTests.EachStillProducesWhatItDid` now counts what settles through
`CwDecoder.CharacterSettled`, split in two:

- **named** - a settled character that is not a word gap and not `■`. **Asserted as a floor that
  only rises**, except on the recordings an adjudicated anchor covers, whose count floor retired by
  Tim's ruling of 2026-08-25 and stays retired.
- **named elements** - `max(1, pattern length)` summed over the named characters only. Asserted as
  a floor on every row, anchored or not, as the element floor was before.
- **placeholders** - settled `■`. Recorded in the table's fourth column and printed; asserted on
  nothing.

**The element floor is re-measured over named characters** - the author's, overrulable. Left
counting every element, a change that stops printing a placeholder would lower the element floor
by that placeholder's own elements, which is the same miscount R57 removes, one column over.

## 2. The 37 rows at `c19ecf61`, measured in one run

Old = the floor table's number before this commit (characters, elements). HEAD emitted =
`CwDecodeReport.CharactersEmitted` at the same commit. Named + placeholders = HEAD emitted on every
row. Anchored = count floor retired (`TheAdjudicatedReadingsKeepReadingTests` covers it).

| capture | old characters | old elements | HEAD emitted | **named** | named elements | placeholders | anchored |
|---|---|---|---|---|---|---|---|
| cw-2026-08-17-013347 | 59 | 108 | 59 | **57** | 106 | 2 | yes |
| cw-2026-08-17-013622 | 55 | 84 | 55 | **51** | 80 | 4 | |
| cw-2026-08-17-134712 | 63 | 98 | 63 | **21** | 41 | 42 | yes |
| cw-2026-08-18-003016 | 57 | 149 | 57 | **54** | 146 | 3 | |
| cw-2026-08-18-003126 | 54 | 144 | 54 | **48** | 131 | 6 | |
| cw-2026-08-18-003758 | 63 | 121 | 63 | **44** | 93 | 19 | yes |
| cw-2026-08-18-004507 | 50 | 118 | 50 | **49** | 117 | 1 | yes |
| cw-2026-08-20-014854 | 0 | 0 | 0 | **0** | 0 | 0 | |
| cw-2026-08-20-014935 | 0 | 0 | 0 | **0** | 0 | 0 | |
| cw-2026-08-22-014113 | 0 | 0 | 0 | **0** | 0 | 0 | |
| cw-2026-08-22-014308 | 0 | 0 | 0 | **0** | 0 | 0 | |
| cw-2026-08-22-031838 | 57 | 126 | 57 | **42** | 93 | 15 | yes |
| cw-2026-08-22-031905 | 42 | 118 | 42 | **36** | 108 | 6 | yes |
| cw-2026-08-22-031948 | 34 | 114 | 34 | **31** | 111 | 3 | yes |
| cw-2026-08-22-032012 | 44 | 120 | 44 | **43** | 119 | 1 | yes |
| cw-2026-08-22-032050 | 53 | 123 | 53 | **44** | 105 | 9 | yes |
| cw-2026-08-22-032113 | 55 | 118 | 55 | **47** | 102 | 8 | yes |
| cw-2026-08-22-032129 | 66 | 119 | 66 | **65** | 114 | 1 | yes |
| cw-2026-08-23-001520 | 5 | 45 | 5 | **1** | 1 | 4 | |
| cw-2026-08-23-001831 | 55 | 124 | 55 | **44** | 108 | 11 | |
| cw-2026-08-23-001952 | 75 | 142 | 75 | **56** | 113 | 19 | |
| cw-2026-08-23-002016 | 75 | 136 | 75 | **44** | 84 | 31 | |
| cw-2026-08-24-012403 | 22 | 65 | 22 | **21** | 62 | 1 | yes |
| cw-2026-08-25-011552 | 30 | 89 | 30 | **22** | 74 | 8 | |
| cw-2026-08-25-012748 | 2 | 4 | 4 | **2** | 3 | 2 | |
| cw-2026-08-25-012823 | 41 | 62 | 41 | **26** | 40 | 15 | |
| cw-2026-08-25-012922 | 50 | 112 | 50 | **45** | 106 | 5 | |
| cw-2026-08-25-013010 | 54 | 131 | 54 | **48** | 122 | 6 | |
| cw-2026-08-25-013150 | 58 | 139 | 58 | **51** | 123 | 7 | |
| cw-2026-08-25-013303 | 54 | 146 | 54 | **44** | 127 | 10 | |
| cw-2026-08-25-013402 | 61 | 161 | 61 | **56** | 150 | 5 | |
| cw-2026-08-25-013520 | 60 | 153 | 60 | **55** | 147 | 5 | |
| cw-2026-08-25-013637 | 63 | 164 | 63 | **60** | 157 | 3 | |
| cw-2026-08-25-021410 | 47 | 99 | 47 | **36** | 88 | 11 | |
| cw-2026-08-25-021629 | 47 | 96 | 47 | **27** | 65 | 20 | |
| cw-2026-08-25-021825 | 41 | 74 | 41 | **25** | 49 | 16 | |
| cw-2026-08-26-125941 | 0 | 0 | 0 | **0** | 0 | 0 | |

Totals, summed from the run's own table: old characters 1,592; HEAD emitted 1,594; **named
1,295**; placeholders 299.

**Whether any named character was lost: no.** On every one of the 37 rows the named count is
HEAD's emitted total less its placeholders exactly, and HEAD's emitted total is at or above the old
floor on every row (the captures type was 37 of 37 at entry), so no row's named count is below its
old total minus its placeholders. The re-measurement counts the same characters the decoder
settled and only sorts them. The placeholder column equals the unsure count HEAD printed at entry on
all 37 rows, because this decoder settles only two confidences, `High` and `Unreadable`.

**The anchors at the same commit**: `TheAdjudicatedReadingsKeepReadingTests` 13 of 13, and the
twelve printed readings are identical, line for line, to the entry run's
(`.run-unit/unit408-adj-entry-reads.txt`, `-adj-t1-reads.txt`). No file under `src` changed.

**Captures under the new table**: 37 of 37 in 97 s (`.run-unit/unit408-cap-t1.txt`).

## 3. The 17:37 capture on the bench

`TheSeventeenThirtySevenCaptureTests.EveryCharacterIsPrintedWithItsEvidence`, a printer that
asserts nothing, reads `cw-2026-09-23-173723.wav` hop by hop as the floors read every capture.

**It does not reproduce the sidecar's counts.** The key file quotes the sidecar at 85 characters
emitted and 26 unsure; the bench replay of the WAV settles **46 named and 0 placeholders**:

    T EABNIREDWBZ WB6RED CQ CQ CQ DE W T E E T E  E ERE D E T T TB 7E E I

The sidecar's counts are the application's session counters, which run from when the CW tab began
listening; the WAV is the tap's last 30 s, and a decoder started cold on it reads it differently.
**The sidecar itself, `cw-2026-09-23-173723.txt`, is not in the tree**, so the 85 and 26 cannot be
re-read here. Against an inferred key, the scored region from the first `CQ CQ` reads
`CQ CQ CQ DE W T E E T E  E ERE D E T T TB 7E E I`, **29 edits** from `CQ CQ CQ DE WB6RED WB6RED`.
The bench reads the first `CQ CQ CQ DE` whole and the callsign after it as a run of `E` and `T`.

Every settled character on the bench scores at or above 1.376 per hop, against the gate's 1.0, and
at or above 30.8 in its own span's total log-likelihood ratio. So nothing on the bench's 17:37 is in
the single digits on either scale. Per character in `.run-unit/unit408-s1737-t1.txt`.

## 4. The gate (3.7)

**What the gate is in the tree.** `CwEmissionGate.cs` does not exist. The bar is
`CwProbabilisticDecoder.CharacterMargin`, **1.0**, a character's own span log-likelihood ratio
(its marks' evidence against the key never having gone down) **per hop**. Before this unit, a
character below it was printed as `■` (`Marked`); a pattern the alphabet does not know was printed
as `■` whatever it scored.

**The sweep** (`TheGateBarSweepTests`, a printer, `.run-unit/unit408-sweep.txt`), every settled
character on the 37 captures and 17:37 at `c1640919`, after B2:

| bar | named lost | anchor characters lost | margin placeholders under it | unknown-pattern placeholders under it |
|---|---|---|---|---|
| per hop 1.0 (the bar as it stands) | **0** | 0 | **261 of 261** | 0 of 28 |
| per hop 1.047 | 3, over 2 recordings | 0 | 261 | 0 |
| per hop 1.25 | 30, over 12 | 0 | 261 | 0 |
| per hop 1.5 | 53, over 17 | 0 | 261 | 1 |
| per hop 2.0 | 117, over 21 | 4 | 261 | 2 |
| span total 5 | 9, over 5 | 0 | 214 | 0 |
| span total 10 | 37, over 10 | 0 | 234 | 0 |
| span total 20 | 67, over 14 | 0 | 246 | 0 |
| span total 50 | 154, over 23 | 1 | 256 | 0 |

**The bar is 1.0 log-likelihood per hop, its value unchanged; what changed is that below it
nothing is printed.** Every higher value on either scale costs named characters - the weakest named
characters in the tree are lone `E`s at span totals of 3.3 to 8.0, per hop 1.0 to 1.9, on unanchored
captures - and the instruction forbids losing one. So a bar at *the single digits* of the span's
total, as the 17:37 sidecar's numbers suggested, is too high: at 5 it costs 9 named characters, and
it is lowered to the one value that costs none. Nothing but placeholders sits below 1.0 per hop.
The 28 unknown-pattern placeholders stand at span totals of 90 to 29.8 million, well above any bar;
the audio holds keying the alphabet cannot name, and they are still printed as `■`. The bar is the
decoder's own scale, the same on every recording, and was not tuned per case.

**The change**: `CwProbabilisticDecoder.Judged`, formerly `Marked`, leaves out a character whose
span per hop is below the bar instead of replacing it with `#`. It is one line of logic in `src`.

**Watched red first**: `TheSeventeenThirtySevenCaptureTests.NothingBelowTheBarIsPrinted` over
17:37 and the three anchors' recordings - at `c1640919`, red on `013347` (57 printed below the
bar, counting leading-edge revisions), `134712` (47) and `003758` (5); green on 17:37, where the
bench settles nothing below the bar. With the change: 4 of 4, and the 17:37 scored region reads 29
edits against an inferred key, as at entry.

**Per case, named and placeholders** - task 1's re-measurement, before the gate (after B2), after
the gate:

| case | named, t1 / before / after | placeholders, t1 / before / after |
|---|---|---|
| 17:37, `cw-2026-09-23-173723` (bench) | 46 / 46 / 46 | 0 / 0 / 0 |
| cw-2026-08-17-013347 (VA3VRR) | 57 / 57 / 57 | 2 / 2 / 0 |
| cw-2026-08-17-013622 | 51 / 51 / 51 | 4 / 4 / 0 |
| cw-2026-08-17-134712 (N4L, retired) | 21 / 21 / 21 | 42 / 39 / 1 |
| cw-2026-08-18-003016 | 54 / 54 / 54 | 3 / 3 / 0 |
| cw-2026-08-18-003126 | 48 / 48 / 48 | 6 / 6 / 1 |
| cw-2026-08-18-003758 (AA4MP/4 QNIK) | 44 / 44 / 44 | 19 / 19 / 1 |
| cw-2026-08-18-004507 | 49 / 49 / 49 | 1 / 1 / 0 |
| cw-2026-08-20-014854 | 0 / 0 / 0 | 0 / 0 / 0 |
| cw-2026-08-20-014935 | 0 / 0 / 0 | 0 / 0 / 0 |
| cw-2026-08-22-014113 | 0 / 0 / 0 | 0 / 0 / 0 |
| cw-2026-08-22-014308 | 0 / 0 / 0 | 0 / 0 / 0 |
| cw-2026-08-22-031838 | 42 / 42 / **43** | 15 / 15 / 1 |
| cw-2026-08-22-031905 | 36 / 36 / 36 | 6 / 6 / 1 |
| cw-2026-08-22-031948 | 31 / 31 / 31 | 3 / 3 / 0 |
| cw-2026-08-22-032012 | 43 / 43 / 43 | 1 / 1 / 0 |
| cw-2026-08-22-032050 | 44 / 44 / 44 | 9 / 9 / 2 |
| cw-2026-08-22-032113 | 47 / 47 / 47 | 8 / 8 / 2 |
| cw-2026-08-22-032129 | 65 / 65 / 65 | 1 / 1 / 1 |
| cw-2026-08-23-001520 | 1 / 1 / 1 | 4 / 4 / 1 |
| cw-2026-08-23-001831 | 44 / 44 / 44 | 11 / 11 / 1 |
| cw-2026-08-23-001952 | 56 / 56 / **57** | 19 / 18 / 2 |
| cw-2026-08-23-002016 | 44 / 44 / 44 | 31 / 30 / 4 |
| cw-2026-08-24-012403 | 21 / 21 / 21 | 1 / 1 / 0 |
| cw-2026-08-25-011552 | 22 / 22 / 22 | 8 / 6 / 1 |
| cw-2026-08-25-012748 | 2 / 2 / 2 | 2 / 2 / 2 |
| cw-2026-08-25-012823 | 26 / 26 / 26 | 15 / 15 / 0 |
| cw-2026-08-25-012922 | 45 / 45 / 45 | 5 / 5 / 0 |
| cw-2026-08-25-013010 | 48 / 48 / 48 | 6 / 4 / 0 |
| cw-2026-08-25-013150 | 51 / 51 / 51 | 7 / 7 / 2 |
| cw-2026-08-25-013303 | 44 / 44 / 44 | 10 / 9 / 1 |
| cw-2026-08-25-013402 | 56 / 56 / 56 | 5 / 5 / 1 |
| cw-2026-08-25-013520 | 55 / 55 / 55 | 5 / 5 / 0 |
| cw-2026-08-25-013637 | 60 / 60 / 60 | 3 / 3 / 1 |
| cw-2026-08-25-021410 | 36 / 36 / 36 | 11 / 11 / 0 |
| cw-2026-08-25-021629 | 27 / 27 / 27 | 20 / 20 / 2 |
| cw-2026-08-25-021825 | 25 / 25 / 25 | 16 / 16 / 0 |
| cw-2026-08-26-125941 | 0 / 0 / 0 | 0 / 0 / 0 |
| **37 captures** | **1,295 / 1,295 / 1,297** | **299 / 289 / 28** |

**No named character is lost anywhere**: no row's named count or named elements is lower after the
gate, and two rows rise by one. The floor table is left as task 1 set it; the two rises are not
written in as floors - the author's, overrulable.

**The anchors**: `TheAdjudicatedReadingsKeepReadingTests` 13 of 13. With placeholders and spaces
stripped, the twelve printed readings are identical to entry on 11. That includes all three named
anchors - `VA3VRR` on `013347`, `AA4MP/4 QNIK` on `003758`, and the retired `N4L` on `134712` -
and `DE KD0UN KD0UN K`. `031838`, a W1AW line, gains one `T` (`E2TTTTTT` to `E2TTTTTTT`) and loses
nothing (`.run-unit/unit408-adj-gate-named.txt`). The screen text changes where placeholders stood:
`E DEQ 6Q E ■Q DE KD0UN KD0UN K` now reads `E DEQ 6Q E Q DE KD0UN KD0UN K`, and a word that was only
a placeholder leaves two spaces.

**The neighbors, one type per invocation**: `CwFixtureTests` 22 of 23 (clean synthetics 2 of 2,
`fading-18wpm` red as parked), `CwReceiverFixtureTests` 25 of 27, `CwAcquisitionWindowTests` 11 of
12, `CwEmissionGateTests` 8, `CapturedSignalTests` 13, `CwAdjudicationTests` 11,
`CwDisplacementFloorTests` 6, `CwSpeedSilenceTests` 4, `WhyTheGateDidNotFireTests` 2,
`CwTwoStationTests` 5, `EachCharacterAnswersForItselfTests` 6 - each at its count before the gate.
`EachCharacterAnswersForItselfTests.AWeakCharacterIsMarkedRatherThanRemoved` asserted the marking
3.7 replaces and was rewritten under R12 as `AWeakCharacterIsNotPrinted`. Measured in passing, not
attacked (R58): #43 goes from 4 + 7 to 2 + 7 under the gate, still red and parked.
