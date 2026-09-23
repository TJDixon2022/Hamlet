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
