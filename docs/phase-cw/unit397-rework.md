# Unit 397 - pieces 34 to 45, the list to its end

The piece rows are in `docs\phase-cw\unit395-rework.md` section 2, under `### Judged by unit 397`,
so that step 4's one table lives in one place. This file holds this unit's own numbers: the entry
round, one paragraph per piece, and the exit round.

## 1. Entry, task 0 - the numbers before piece 34

**Decision 20 applied.** `git diff --stat e2b31a40 HEAD -- src tests docs/carry-forward-tests.txt`
at task 0 printed nothing - the command's whole output was empty - so unit 396's exit runs of the
two carry-forward lines, section 3 of its doc, are this unit's entry numbers for the lines: **app
278 of 278 in 155 s; engine 176 of 176 in 374 s of 480.** Neither line was run at entry.

The floors ran in full, HEAD `b0960f47` plus task 0's non-`src` edits, after one `dotnet build
Hamlet.sln -warnaserror` (0 errors, 15 s), each type `--no-build`, one invocation per type:

- `TheCapturesThatDecodeKeepDecodingTests`: **37 of 37 green in 95 s wall**, 1.57 min test time.
  `.run-unit\unit397-floors-1.txt`.
- `TheAdjudicatedReadingsKeepReadingTests`: **13 of 13 green in 29 s**. `.run-unit\unit397-floors-2.txt`.
- `CwFixtureTests.TheCleanRecordingsDecodeExactly`: **0 of 2**, red as R53 expects, in 3 s;
  `clean-12wpm` gave `■ ■ ■ ■ ■  ■ ■ ■ ■■` and `clean-18wpm` `■ ■ ■  ■■■` against `CQ DE W1AW K`.
  `.run-unit\unit397-floors-3.txt`.

Every capture, characters, elements, unsure and tone as the test prints them. **Every row is
identical to unit 396's exit table** (its section 3, the same as its section 1): the compare of
`unit396-floors-exit-1.txt` and `unit397-floors-1.txt` printed no difference in 37 rows.

| Capture | Characters | Elements | Unsure | Tone |
|---|---|---|---|---|
| cw-2026-08-17-013347 | 59 | 108 | 2 | 625 |
| cw-2026-08-17-013622 | 55 | 84 | 4 | 600 |
| cw-2026-08-17-134712 | 63 | 98 | 42 | 500 |
| cw-2026-08-18-004507 | 50 | 118 | 1 | 500 |
| unadjudicated/cw-2026-08-18-003016 | 57 | 149 | 3 | 670 |
| unadjudicated/cw-2026-08-18-003126 | 54 | 144 | 6 | 665 |
| unadjudicated/cw-2026-08-18-003758 | 63 | 121 | 19 | 500 |
| unadjudicated/cw-2026-08-20-014854 | 0 | 0 | 0 | 600 |
| unadjudicated/cw-2026-08-20-014935 | 0 | 0 | 0 | 825 |
| unadjudicated/cw-2026-08-22-014113 | 0 | 0 | 0 | 600 |
| unadjudicated/cw-2026-08-22-014308 | 0 | 0 | 0 | 575 |
| unadjudicated/cw-2026-08-22-031838 | 57 | 126 | 15 | 525 |
| unadjudicated/cw-2026-08-22-031905 | 42 | 118 | 6 | 300 |
| unadjudicated/cw-2026-08-22-031948 | 34 | 114 | 3 | 500 |
| unadjudicated/cw-2026-08-22-032012 | 44 | 120 | 1 | 500 |
| unadjudicated/cw-2026-08-22-032050 | 53 | 123 | 9 | 325 |
| unadjudicated/cw-2026-08-22-032113 | 55 | 118 | 8 | 650 |
| unadjudicated/cw-2026-08-22-032129 | 66 | 119 | 1 | 650 |
| unadjudicated/cw-2026-08-23-001520 | 5 | 45 | 4 | 600 |
| unadjudicated/cw-2026-08-23-001831 | 55 | 124 | 11 | 525 |
| unadjudicated/cw-2026-08-23-001952 | 75 | 142 | 19 | 525 |
| unadjudicated/cw-2026-08-23-002016 | 75 | 136 | 31 | 525 |
| unadjudicated/cw-2026-08-24-012403 | 22 | 65 | 1 | 440 |
| unadjudicated/cw-2026-08-25-011552 | 30 | 89 | 8 | 500 |
| unadjudicated/cw-2026-08-25-012748 | 4 | 16 | 2 | 395 |
| unadjudicated/cw-2026-08-25-012823 | 41 | 62 | 15 | 450 |
| unadjudicated/cw-2026-08-25-012922 | 50 | 112 | 5 | 475 |
| unadjudicated/cw-2026-08-25-013010 | 54 | 131 | 6 | 475 |
| unadjudicated/cw-2026-08-25-013150 | 58 | 139 | 7 | 495 |
| unadjudicated/cw-2026-08-25-013303 | 54 | 146 | 10 | 500 |
| unadjudicated/cw-2026-08-25-013402 | 61 | 161 | 5 | 525 |
| unadjudicated/cw-2026-08-25-013520 | 60 | 153 | 5 | 540 |
| unadjudicated/cw-2026-08-25-013637 | 63 | 164 | 3 | 550 |
| unadjudicated/cw-2026-08-25-021410 | 47 | 99 | 11 | 550 |
| unadjudicated/cw-2026-08-25-021629 | 47 | 96 | 20 | 500 |
| unadjudicated/cw-2026-08-25-021825 | 41 | 74 | 16 | 400 |
| unadjudicated/cw-2026-08-26-125941 | 0 | 0 | 0 | 400 |

**The printer at entry**, `TheReworkNumbersPrinterTests`, 2 of 2 (it asserts nothing), 5 s wall,
`.run-unit\unit397-printer-entry.txt`. Identical to unit 396's section 1:

| Capture | Settled text | Word | Distance | Nearest substring |
|---|---|---|---|---|
| 021410 | `■ ■ ■ M ■ ■ ■ ■ T O MTT T  Y M TT ■ ■ O AO IHI DT ■RIGHR IS ■ FLENT 66OAM` | WEEKEND | 5 | ` FLEN` |
| | | THINKING | 5 | `T ■RIG` |
| | | FLEX | 1 | `FLE` |
| 013637 | `TE MP NEVEN T REV■R G O T AB OV E ■7 5 F ES ■CLEAR S KY LI TE BR EE Z E ALL DAY JUST AWE SO` | ABOVE | 2 | `AB OV` |
| | | BREEZE | 2 | `BR EE` |

**The trees at entry.** The eleven transmit files against `7e209cb4`: nothing. `git diff --stat
5688a8a5 HEAD -- src`: nothing. `git diff --stat 7e209cb4 HEAD -- src/Hamlet.RadioEngine/Cw`: 4
files, 165 insertions, 1 deletion, as the instruction says.

## 2. The pieces, task 1

**Piece 34, `e6b1ece7`**, *a key-down that comes back inside twelve milliseconds never ended*.
The `Cw` patch, eleven transmit files excluded by path, was 124 lines, one file,
`CwUnitEstimator.cs`, 91 insertions; `git apply --check` clean, applied clean, nothing dropped.
Piece commit `9627bc0b`; build 0 errors in 8 s. Captures **23 of 37 in 82 s wall**, 14 red on
their floor assertion: 031948 34 to 31, 012922 50 to 44, 013402 elements 161 to 157, 003758 63
to 57, 032050 53 to 51, 001952 75 to 72, 012823 41 to 34, 032129 66 to 64, 001831 55 to 51,
021410 47 to 45, 031905 42 to 40, 031838 57 to 43, 134712 63 to 49, 021629 47 to 44; rose:
004507 50 to 53, 002016 75 to 77, 032113 elements 118 to 119. Adjudicated **12 of 13 in 27 s**:
`cw-2026-08-22-032012` gave `" AF 117.1. LINKS TO A R T I C L E S OR OT"...`, anchor `R OTHER
WEBSITES MENTI` not found. Synthetics 0 of 2, the same placeholders. Printer:

```
 SETTLED [■ ■ ■ M ■ ■ ■ ■ T O MM T Y M TT ■ O AO IHI DT ■RIGHR IS ■ FLENT 66OAM]
 SETTLED DISTANCE WEEKEND 5 [ FLEN]
 SETTLED DISTANCE THINKING 5 [T ■RIG]
 SETTLED DISTANCE FLEX 1 [FLE]
 SETTLED [TE MP NEVEN T REV■R G OT AB OV E ■7 5 F ES ■CLEAR S KY LI TE BR EE Z E ALL DAY JUST AWE SO]
 SETTLED DISTANCE ABOVE 2 [AB OV]
 SETTLED DISTANCE BREEZE 2 [BR EE]
```

Transmit files silent against `7e209cb4`. Floors red and no named number improved: **out**,
reverted in the next commit. The outputs are `.run-unit\unit397-p34-{1,2,3,printer}.txt`.

**Piece 35, `dfb357ef`**, *the four configurations, and a carrier count that does not work*. The
`Cw` patch was 23 lines, `CwSpectralPeak.cs`, 12 insertions, a public `AverageSpectrum` wrapping
the private `Average`. It refused on the kept state: *No such file or directory* - the file is
piece 28's. Decision 16's sequence on a clean `Cw`: after 28 alone, `patch failed:
CwSpectralPeak.cs:177`; after 28 then 32, check rc 0. The failure after 28 alone is piece 32's
context lines around the insertion point, not a name the piece uses, so as unit 396 did for piece
33 on piece 31's context, piece 28 was applied and piece 35 merged under `--3way` on it: applied
cleanly, 255 lines, 243 of piece 28 and 12 of piece 35, no conflict marker. One pair under decision
5, commit `2669b5f9`. Build 0 errors in 6 s. Captures **37 of 37 in 94 s**, every row identical to
entry; adjudicated 13 of 13 in 29 s; synthetics 0 of 2, the same placeholders; printer identical
to entry, WEEKEND 5, THINKING 5, FLEX 1, ABOVE 2, BREEZE 2. Transmit files silent. Nothing moved:
**out**, reverted in the next commit.

**Piece 36, `efc33267`**, *the sheet stops lying about arithmetic, and tonight's captures land*.
The `Cw` patch was 40 lines, `CwCounterTrail.cs`, 24 insertions and 1 deletion - the deletion and
its insertion are line 1 gaining a byte-order mark - checked and applied clean. Not taken under
R50 and decision 24: `MainWindowViewModel.cs`, one test, 18 fixture files. Piece commit
`c08f766d`; build 0 errors in 6 s. Captures **37 of 37 in 94 s**, every row identical; adjudicated
13 of 13 in 29 s; synthetics 0 of 2, the same placeholders; printer identical to entry. Transmit
files silent. The change makes `CwCounterTrail` return no delta when a counter went backwards
inside the window, a sheet fact outside the decode. Nothing moved: **out**, reverted in the next
commit.

## 3. The exit round, task 2
