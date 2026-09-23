# Unit 396 - pieces 10 onward, on the same numbers

The piece rows are in `docs\phase-cw\unit395-rework.md` section 2, under `### Judged by unit 396`,
so that step 4's one table lives in one place. This file holds this unit's own numbers: the entry
round, one paragraph per measured piece, and the exit round.

## 1. Entry, task 0 - the numbers before piece 10

**Decision 12 applied.** `git diff --stat 259eba0a HEAD -- src tests docs/carry-forward-tests.txt`
at task 0 printed nothing, so unit 395's exit runs of the two carry-forward lines, section 3.3 of
its doc, are this unit's entry numbers for the lines: **app 277 of 278 twice, 1 lost to the
headless dispatcher loop each run, different names, no red on an assertion; engine 176 of 176 in
372 s of 480.** Neither line was run at entry.

The floors ran in full, HEAD `259eba0a` plus task 0's non-`src` edits, after one `dotnet build
Hamlet.sln -warnaserror` (0 errors, 14 s), each type `--no-build`, one invocation per type:

- `TheCapturesThatDecodeKeepDecodingTests`: **37 of 37 green in 93 s wall**, 1.55 min test time.
  `.run-unit\unit396-floors-1.txt`.
- `TheAdjudicatedReadingsKeepReadingTests`: **13 of 13 green in 29 s**. `.run-unit\unit396-floors-2.txt`.
- `CwFixtureTests.TheCleanRecordingsDecodeExactly`: **0 of 2**, red as R53 expects, in 3 s;
  `clean-12wpm` gave `■ ■ ■ ■ ■  ■ ■ ■ ■■` and `clean-18wpm` `■ ■ ■  ■■■` against `CQ DE W1AW K`.
  `.run-unit\unit396-floors-3.txt`.

Every capture, characters, elements, unsure and tone as the test prints them. **Every row is
identical to unit 395's exit table** (its section 3.3, the same as its section 3.1): the compare
of `unit395-floors-exit-1.txt` and `unit396-floors-1.txt` printed no difference in 37 rows.

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
`.run-unit\unit396-printer-entry.txt`. Identical to unit 395's section 3.1a:

| Capture | Settled text | Word | Distance | Nearest substring |
|---|---|---|---|---|
| 021410 | `■ ■ ■ M ■ ■ ■ ■ T O MTT T  Y M TT ■ ■ O AO IHI DT ■RIGHR IS ■ FLENT 66OAM` | WEEKEND | 5 | ` FLEN` |
| | | THINKING | 5 | `T ■RIG` |
| | | FLEX | 1 | `FLE` |
| 013637 | `TE MP NEVEN T REV■R G O T AB OV E ■7 5 F ES ■CLEAR S KY LI TE BR EE Z E ALL DAY JUST AWE SO` | ABOVE | 2 | `AB OV` |
| | | BREEZE | 2 | `BR EE` |

**The trees at entry.** The eleven transmit files against `7e209cb4`: nothing. `git diff --stat
5688a8a5 HEAD -- src`: nothing.

## 2. The pieces

The rows are in `unit395-rework.md` section 2 under `### Judged by unit 396`. Below, one paragraph
per piece: the dependency sequence for a dependent piece, the captures wall time and the SETTLED
lines for a measured one.

**Piece 10, `4786c7e7`, task 1.** Patch `.run-unit\unit396-piece-10-4786c7e7.patch`, 121 lines,
`CwToneSurvey.cs` only, 68 insertions and 2 deletions. On the kept state: `git apply --check` rc 1
(`patch failed: src/Hamlet.RadioEngine/Cw/CwToneSurvey.cs:20`), `--3way` conflicted, the file
restored to HEAD. Decision 16, each prior applied to a clean `Cw`, the target checked, `Cw`
restored, never committed (`.run-unit\unit396-deps10.sh`):

| Applied first | Target `--check` |
|---|---|
| piece 5 `9de394da` alone | rc 1 |
| piece 7 `7fb89d5e` alone | rc 1 |
| piece 8 `1bf4372d` alone | prior itself did not apply; rc 1 |
| piece 9 `44cf3fc8` alone | prior itself did not apply; rc 1 |
| 7 then 8 | rc 1 |
| 7 then 9 | **rc 0** |
| 7 then 8 then 9 | rc 0 |

It needs 7 and 9, two out pieces: *dependent, out*, not applied, no build, no run. `src` clean
after.

**Piece 11, `f2e1db7a`, task 2.** Patch 116 lines, `CwToneSurvey.cs` and `CwToneTracker.cs`, 81
insertions. On the kept state `--check` rc 1 on both files, `--3way` conflicted in both, restored.
Decision 16 (`.run-unit\unit396-deps.sh`): after 5, 7, 8, 9 or 10 alone, rc 1 (8, 9 and 10 do not
apply alone themselves); after 7 then 8, rc 1; **after 7 then 9, rc 0**; after 7, 9, 10, rc 0.
Needs 7 and 9: *dependent, out*. `src` clean after.

**Piece 12, `f27174b5`, task 2, measured.** Patch 172 lines, `CwDecodeReport.cs` and
`CwDecoder.cs`, 119 insertions and 3 deletions. `--check` rc 1; `--3way` merged `CwDecodeReport.cs`
and left one conflict block in `CwDecoder.cs`, theirs being piece 3's `Retuned()` with the piece's
`Unlock()` line added; resolved to ours, keeping unit 392's `Retuned() => Unlock()`, which already
unlocks (decision 3). Piece commit `ad5fa332`. Build: `CS8907` on the seam property
`PitchWasAsserted => false`; removed in `14155613` (decision 4). Build 0 errors in 7 s. Captures
**37 of 37 in 97 s wall**, 1.61 min test time, the compare against entry printed no difference in
37 rows; adjudicated 13 of 13 in 30 s; synthetics 0 of 2 in 3 s, the same placeholders. Printer:

```
 SETTLED [■ ■ ■ M ■ ■ ■ ■ T O MTT T  Y M TT ■ ■ O AO IHI DT ■RIGHR IS ■ FLENT 66OAM]
 SETTLED DISTANCE WEEKEND 5 [ FLEN]
 SETTLED DISTANCE THINKING 5 [T ■RIG]
 SETTLED DISTANCE FLEX 1 [FLE]
 SETTLED [TE MP NEVEN T REV■R G O T AB OV E ■7 5 F ES ■CLEAR S KY LI TE BR EE Z E ALL DAY JUST AWE SO]
 SETTLED DISTANCE ABOVE 2 [AB OV]
 SETTLED DISTANCE BREEZE 2 [BR EE]
```

Transmit files nothing against `7e209cb4`. Nothing moved: out, both commits reverted in the next.

**Piece 13, `386fdb5d`, task 2.** Patch 600 lines: `CwDecoder.cs` 14, `CwJointCutter.cs` 328 new,
`CwProbabilisticDecoder.cs` 120, `CwProbabilisticStream.cs` 16. `--check` rc 1 on three files;
`--3way` merged `CwProbabilisticStream.cs`, conflicted in the other two, and added
`CwJointCutter.cs` to the index. **The first decision 16 pass was contaminated** - the added file
was still on disk, so every target check also failed on it - and was discarded; the file was
removed (`unit396-clean.sh`), `deps.sh` now cleans `Cw` before and after each chain, and the pass
re-run (`unit396-deps13.sh`). The failing files by chain:

| Applied first | Target `--check` fails on |
|---|---|
| nothing | CwDecoder:422, CwProbabilisticDecoder:1262, CwProbabilisticStream:148 |
| piece 1 | the same three |
| piece 2 | CwDecoder:422, CwProbabilisticStream:148 |
| piece 3 | the same three |
| piece 5 | CwDecoder:422, CwProbabilisticDecoder:1262 |
| 2, 3 | CwDecoder:422, CwProbabilisticStream:148 |
| 1, 2, 3 | CwDecoder:422, CwProbabilisticStream:148 |
| 1, 2, 3, 5, 12 | CwDecoder:422 (12 itself rc 1 on that chain) |
| 1, 2, 3, 5 to 12 | CwDecoder:422 (12 itself rc 1) |

Needs 2 and 5 and whatever the `CwDecoder` hunk stands on: *dependent, out*. `src` clean after.

**Piece 14, `8ca6a633`, task 2.** Patch 191 lines, `CwPitchRanking.cs` new, 185 insertions, 59
code lines. Applied clean; piece commit `952fb690`. Build rc 1 in 1 s, two errors, both CS0122 on
`CwToneTracker.CoarseSpacingHz` (lines 174 and 180). Not the seam: out under decision 4, no floor
run. Transmit files nothing against `7e209cb4`. Reverted in the next commit.

**Piece 15, `4c6e4321`, task 2.** Patch 241 lines over four files; `CwPitchChoice.cs` "already
exists in working directory", dropped under decision 3, and the check re-made on the other three,
182 lines (`unit396-piece-15-4c6e4321-nochoice.patch`). On the kept state all three fail. After
piece 3 only `CwDecodeReport:65` and `CwDecoder:286` fail; after 1, 2, 3, 5, 7, 9, 10, 11 the same
two. Piece 12's raw patch does not apply after 3, so piece 12 *as committed* (`git diff 56a90616
14155613 -- Cw`, the resolution and seam included) was used as the prior: after it alone,
`CwDecodeReport:65` and `CwToneTracker:773` fail; after 3 and 12 in either order, only
`CwDecodeReport:65`, whose context is unit 392's `PitchChoice` seam. Needs 3 and 12: *dependent,
out*. `src` clean after (`unit396-deps15.sh`, `unit396-deps15b.sh`).

**Piece 16, `501e8e2d`, task 2.** Its `Cw` diff deletes `CwPitchRanking.cs` only; the file is
not in the tree (piece 14 out). `--check` and `--3way` refused, nothing changed. Empty.

**Piece 17, `f9c11989`, task 2, measured.** Patch 212 lines, `CwProbabilisticDecoder.cs`, 201
insertions; applied clean, piece commit `0bec4dd6`. Build 0 errors in 6 s. Captures **37 of 37 in
94 s wall**, 1.55 min, no row differs from entry; adjudicated 13 of 13 in 29 s; synthetics 0 of 2
in 4 s, `■ ■ ■ ■ ■  ■ ■ ■ ■■` and `■ ■ ■  ■■■` as at entry. Printer: the seven SETTLED lines
identical to section 1 - WEEKEND 5, THINKING 5, FLEX 1, ABOVE 2, BREEZE 2. Transmit files
nothing. Out, reverted in the next commit.

**Piece 18, `b48d1158`, task 2.** Patch 308 lines, `CwPitchRanking.cs` new, 302 insertions;
applied clean, piece commit `5dd24810`. Build rc 1 in 1 s, CS0122 on `CwToneTracker.CoarseSpacingHz`
at lines 225 and 232. Out under decision 4, no floor run; transmit files nothing; reverted next.

**Piece 19, `0f2089f3`, task 2.** Patch 318 lines over three files. `--3way` merged
`CwPitchChoice.cs` to a file with no diff against HEAD - already in the tree, dropped - and
conflicted in the other two. Checked without it (288 lines, `unit396-deps19.sh`): `CwDecodeReport:52`
and `CwDecoder:289` fail on the kept state and after 3; 12 as committed; 18; 3 and 12; 3, 12, 15;
3, 12, 15, 18 (15 itself rc 1 on the chain). *Dependent, out*. `src` clean after.

**Piece 20, `ac1d56da`, task 2, measured.** Patch 1129 lines, `CwReferenceDecoder.cs` new, 1123
insertions; applied clean, piece commit `304ec791`. Build 0 errors in 6 s. Captures **37 of 37 in
93 s wall**, 1.55 min, no row differs; adjudicated 13 of 13 in 29 s; synthetics 0 of 2 in 3 s.
Printer: the seven SETTLED lines identical to section 1. Transmit files nothing. Out, reverted next.

**Piece 21, `62262b94`, task 2.** Patch 42 lines, `CwDecoder.cs`, 31 insertions, all inside
`Retuned()`. `--check` rc 1 at `CwDecoder.cs:382`; `--3way` conflicted, theirs being piece 3's
`Retuned` body. After 1; 3; 12 as committed; 1 and 3; 3 and 12; 1, 3, 12: rc 1 every time
(`unit396-deps21.sh`). `_reReadAt` and `_lastMeasuredForReRead` occur 0 times in HEAD's
`CwDecoder.cs`. *Dependent, out*. `src` clean after.

(The first pass of the script printed `basename`'s exit code for each prior; corrected and re-run
before this was written. The table for piece 10 came from unit 395's `dep.sh`, which was right.)

## 3. Exit
