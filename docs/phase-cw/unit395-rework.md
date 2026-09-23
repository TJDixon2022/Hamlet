# Unit 395 - the rework goes back one piece at a time, on numbers

Step 4 of *CW decodes again*. Section 1 is criterion 4.1, the list. Section 2 is one row per
piece judged, in 4.5's shape. Section 3 holds the numbers: the numbers before piece 1 at entry,
the printer's texts and distances, and the exit round. Every number here is an indication
(FACT-004); *green* means the assertion held and nothing more (`CLAUDE.md` §0.0).

## 1. The list

Written at task 1.

## 2. The pieces judged

Written at task 2.

## 3. The numbers

### 3.1 The numbers before piece 1, at entry

Unit 395 task 0, HEAD `ee0ea0dc` plus task 0's non-`src` edits, `src` as unit 394 left it.
`TheCapturesThatDecodeKeepDecodingTests`, one invocation, `--no-build` after the engine line's
build, 37 of 37 green in 92 s wall, 1.53 min test time; output `.run-unit\unit395-floors-1.txt`.
Characters, elements and unsure as the test prints them from `decoder.Report`; tone in Hz.

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

Every row equals its floor in the table; the restored decoder stands exactly on the floors, not
above them. `TheAdjudicatedReadingsKeepReadingTests` 13 of 13 green in 29 s wall;
`CwFixtureTests.TheCleanRecordingsDecodeExactly` 0 of 2, `clean-12wpm` and `clean-18wpm` red as
R53 expects, in 3 s. Outputs `.run-unit\unit395-floors-2.txt` and `-3.txt`.

### 3.2 The carry-forward lines at entry

- **App**, line 7 as printed: 276 of 278 in 171 s, 2 lost to the headless dispatcher loop
  (`ThePsk31ConversationCardTests.NoSlotClockUnderPsk31AndFt8AndFt4StillShowIt`,
  `TheRstIsYoursToCorrectTests.OnTheWindowTheTwoReportsAreBoxesWithTheirMarks`); re-run once,
  276 of 278 in 169 s, 2 lost the same way (`BindingHealthTests.TheMainWindowBindsWithoutOneComplaint`,
  `ThePsk31OfferTests.TheOfferIsOneButtonAndItIsTheOneTheEngineNamed`). Each lost name absent from
  the other run's failures, so green there; no red on an assertion.
- **Engine**, line 9 as printed: 176 of 176 in 375 s of 480.
