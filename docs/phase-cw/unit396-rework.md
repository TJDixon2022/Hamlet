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

## 3. Exit
