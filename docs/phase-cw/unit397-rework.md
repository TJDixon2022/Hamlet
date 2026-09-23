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

**Piece 37, `aeea24f2`**, *admit a station by how far its bin swings, not by its average*. The
`Cw` patch was 306 lines: `CwSwingSurvey.cs` new, 148 lines; `CwDecoder.cs` 106 insertions, 1
deletion. `--check` failed at `CwDecoder.cs:384`; `--3way` left three conflict blocks in
`CwDecoder.cs` (lines 285, 670, 747 of the merged file), cleaned at once with
`unit397-clean.sh CwSwingSurvey.cs`, `src` back to `5688a8a5`. What the hunks need: `MaybeSwing`
is called beside `MaybePeak` (piece 29, `efcd5242`) and `MaybeRank` (piece 19, `0f2089f3`); it
reads `PeakWindowSeconds` and `PeakEverySeconds` (piece 29); `_swing is not null` is added to
`Squelched` (piece 30, `95a5e063`); the resets sit beside `_peakToneHz` and `_rankedAtSample`.
`grep` of `CwDecoder.cs` at HEAD for all eight names: 0. Decision 16's sequence on a clean `Cw`,
`unit397-deps37.sh`: target `--check` failed at `CwDecoder.cs:384` after 30 (itself refusing);
after 29 (refusing); after 3; after 3 then 12 as committed; after 3, 12, 19 (19 refusing); after
3, 12, 19, 29, 30 (19, 29, 30 refusing). Three out pieces at least, one of which, 19, is itself
dependent: **out under decision 5**, listed and not applied, no piece commit, no run.

**Piece 38, `a37cfcff`**, *lower the speed ceiling to thirty, and carry every element out*. The
`Cw` patch was 549 lines, four files: `CwElementPitch.cs` new, 266 lines; `CwJointCutter.cs` 18
changed lines; `CwProbabilisticDecoder.cs` 99; `CwUnitEstimator.cs` 34. On the kept state
`CwJointCutter.cs` refused *No such file or directory* (piece 13 created it, `386fdb5d`, out and
itself dependent on 2, 5 and more), `CwProbabilisticDecoder.cs:827` and `CwUnitEstimator.cs:638`
failed, and `--3way` refused the whole patch on the missing file, leaving the tree untouched.
Decision 16's sequence, `unit397-deps38.sh`: after 13 (refusing), 827 and 638 fail; after 34 alone,
638 clears and 827 fails; after 2, 5, 13 (13 refusing), both fail; after 31, 33, 34, 827 fails;
after 2, 5, 13, 31, 33, 34, 827 fails. Two out pieces at least, 13 and 34, and 13's chain: **out
under decision 5**, listed and not applied, no piece commit, no run. `CwElementPitch` did not
come back and the excluded tests that name it stay excluded.

**Piece 39, `a09b36a7`**, *measure whether two people are sending, and withhold the verdict*. The
`Cw` patch was 402 lines, one new file `CwStreamSplit.cs`, 396 lines; checked and applied clean.
Piece commit `2b9c5d10`. Build failed in 2 s, 3 errors, the first
`CwStreamSplit.cs(210,57): error CS0246: The type or namespace name 'CwElement' could not be found
(are you missing a using directive or an assembly reference?)`, the other two at 280 and 281 on
the same type. `CwElement` is `public readonly record struct CwElement(bool IsMark, int StartHop,
int EndHop)`, declared in piece 38's `CwElementPitch.cs` and nowhere at HEAD. Not the seam:
**out under decision 4**, no floor run, reverted in the next commit. Transmit files silent.

**Piece 40, `ee2cba8d`**, *the element streams agree, so the reading is lost after them*. The
`Cw` patch was 78 lines, `CwUnitEstimator.cs`, 52 insertions: an `Elements` overload with an `out
int dropped` count and a `Runs` overload that counts the runs `ShortestRunHops` refuses. `--check`
failed at line 348; `--3way` left one conflict block at 348 to 567, HEAD's `Otsu(db)` against the
piece's `Cut(db)`; cleaned with `unit397-clean.sh`. `Cut(` occurs 0 times in HEAD's file; it is
declared by piece 33, `4935a4f8`, and the new overload calls it. Decision 16's sequence,
`unit397-deps40.sh`: target failed at 348 after 31, after 33 (itself refusing), after 34, after 31
and 34; at 694 after 31 and 33; cleared only after 31, 33, 34. Three out pieces: **out under
decision 5**, listed and not applied, no piece commit, no run.

**Piece 41, `2828ab69`**, *report work instruction 056 - the streams agree, the reading is lost
after*. The `Cw` patch was 46 lines, `CwStreamSplit.cs`, 17 insertions and 3 deletions: the
trusted resolution made a `const` and a parameter of `Divide`. It refused on the kept state, *No
such file or directory* - the file is piece 39's, one out piece - so it was tried once as a pair
with 39 under decision 5 (`unit397-pair41.sh`: 39 applied rc 0, 41 applied rc 0, the file 410
lines). Pair commit `548d7007`. Build failed in 1 s, 3 errors, the first `CwStreamSplit.cs(223,23):
error CS0246: The type or namespace name 'CwElement' could not be found`, at 294 and 295 the same
- piece 39's error, since both pieces name piece 38's `CwElement` and piece 41 also
`CwElementPitch.ResolutionHz`. Not the seam: **out under decision 4**, no floor run, reverted in
the next commit. Transmit files silent.

**Piece 42, `43efc525`**, *the tap is fed from the callback, the decoder from a queue*. The `Cw`
patch was 235 lines, `CwDecoder.cs`, 181 insertions and 4 deletions: an `AudioHandoff` queue and
one `cw-decode` worker thread built in `Listen`, `StopWorker`, `DrainHandoff`, a private
`Process(chunk, takeIntoTap)`, a `Flush` that waits for the queue to drain, and an `OnSamples` that
feeds the tap and queues the chunk. Not taken under R50 and decision 24: `Audio\AudioHandoff.cs`,
`Audio\WasapiAudioSource.cs`, one test; the `Audio` types are at HEAD already. `--check` failed at
line 728; `--3way` merged every hunk but one, `OnSamples` (conflict lines 784 to 1365 of the
merged file), where the piece's side carried about 580 lines of bodies from out pieces - the
held-audio re-read and the ranking - as context. Resolved under decision 3 by
`unit397-p42-resolve.sh`: HEAD's surroundings kept, the one-line `OnSamples` replaced with the
piece's own, and the piece's `DecodeQueueDepth` taken; its `DecodeQueueDroppedChunks` and
`DecodeQueueDroppedSamples` dropped as already in the tree, because unit 392's seam declares both
at lines 397 and 400 (at nought, unit 392's standing decision) and the seam is kept. Staged diff
175 insertions, 4 deletions; line endings LF as HEAD. Piece commit `ec9dec4a`; build 0 errors in
7 s. Captures **37 of 37 in 94 s**, every row identical; adjudicated 13 of 13 in 29 s; printer
identical to entry. **Synthetics 0 of 2, both `Actual: ""`**, where the entry state gives the
placeholders `■ ■ ■ ■ ■  ■ ■ ■ ■■` and `■ ■ ■  ■■■`, and in 0.67 s against 2.7 s: the synthetics
attach a source, which now routes the decode through the worker, and nothing came back. No named
number improved: **out**, reverted in the next commit. Transmit files silent.

**Piece 43, `865e66d8`**, *no CW decode in Digital mode*. The `Cw` patch was 48 lines,
`CwDecoder.cs`, 22 insertions and 1 deletion, in three hunks: the `DigitalMode` property with its
doc comment, `if (DecodingSuspended || DigitalMode)`, and one blank line. Not taken under R50 and
decision 24: `MainWindowViewModel.cs`, one test. `--check` failed at line 649; `--3way` merged the
condition and conflicted only on the property's doc comment - HEAD's reads *865e66d8's GATE,
CARRIED ACROSS THE RESTORE (work instruction 392, a seam for today's application)*. At HEAD,
after `unit397-clean.sh`, `public bool DigitalMode { get; set; }` is line 361 and `if
(DecodingSuspended || DigitalMode)` line 501: both code hunks already in the tree as unit 392's
seam, dropped under decision 3, as unit 395 did for piece 4. What is left is a doc comment and a
blank line: **out under decision 17**, no piece commit, no run.

**Piece 44, `9c2a7f99`**, *a reader on a timer stops allocating the audio it reads*. The `Cw`
patch was 99 lines: `CwDecoder.cs` 30 changed lines in five hunks, four `Audio.ReusableWindow`
fields and four `Tap.Window` calls replaced by `_xWindow.From(Tap, ...)`; `CwKeyingMeter.cs` 12,
a `ReusableWindow` field and `tap.Tail` replaced by `_window.Tail(tap, ...)`. Not taken under R50
and decision 24: `Audio\Ft8SlotWatch.cs`, `Audio\ReusableWindow.cs` (at HEAD already),
`MainWindowViewModel.cs`, one test. `--check` failed at `CwDecoder.cs:1147`; `--3way` merged
`CwKeyingMeter.cs` clean and conflicted in `CwDecoder.cs` (lines 643 to 1246 of the merged file).
Cleaned; `unit397-check44.sh` at HEAD: `Tap.Window`, `ReadHeldAudioAgain`, `MaybeSwing`,
`MaybePeak`, `MaybeRank`, `_reReadAt` - 0 lines in `CwDecoder.cs`. The four call sites are in the
re-read (piece 1, `2068f868`), the swing survey (piece 37, `aeea24f2`), the peak (piece 29,
`efcd5242`) and the ranking (piece 19, `0f2089f3`), all out. Four out pieces: **out under
decision 5**, listed and not applied, no piece commit, no run. The `CwKeyingMeter` half merged
clean on its own; a piece is its whole `Cw` diff under decision 3 and decision 5 lists a
dependent piece whole, so it was not split off and measured - parked as `397 item 2`.

**Piece 45, `1a84188e`**, *the callback budget is set, not inherited*, the last piece. The `Cw`
patch was 22 lines, `CwKeyingMeter.cs`, 11 insertions: `public int WindowSizings =>
_window.Sizings`. Not taken under R50 and decision 24: `Telemetry\AppEvents`,
`MainWindowViewModel.cs`, `Audio\AudioArrival`, `Audio\CallbackBudget`,
`Audio\DigitalCaptureSheet`, `Audio\WasapiAudioSource`, three tests. Checked and applied clean.
Piece commit `25bc3bcf`. Build failed in 2 s, 1 error: `CwKeyingMeter.cs(192,33): error CS0103:
The name '_window' does not exist in the current context`. `_window` is the `ReusableWindow` field
piece 44 adds to the meter; piece 44 is out and dependent on four out pieces, so a pair with it
cannot be applied. Not the seam: **out under decision 4**, no floor run, reverted in the next
commit. Transmit files silent. **45 of 45 judged.**

## 3. The exit round, task 2

HEAD `3af36501`, every piece 34 to 45 out: `git diff --stat 5688a8a5 HEAD -- src` prints nothing.

**The carry-forward lines**, as `docs\carry-forward-tests.txt` prints them at lines 7 and 9, one
build each, a status line immediately before each:

| Line | Entry, unit 396's exit under decision 20 | Exit |
|---|---|---|
| App | 278 of 278 in 155 s | **278 of 278 in 167 s**, nothing lost |
| Engine | 176 of 176 in 374 s of 480 | **176 of 176 in 374 s of 480** |

No regression. `.run-unit\unit397-carry-exit-{app,eng}.txt`.

**The floors**, one invocation per type, `--no-build` after the engine line's build:

- `TheCapturesThatDecodeKeepDecodingTests`: **37 of 37 green in 92 s**; the compare of
  `unit397-floors-1.txt` and `unit397-floors-exit-1.txt` printed no difference in 37 rows, so the
  kept state's table is section 1's, every row.
- `TheAdjudicatedReadingsKeepReadingTests`: **13 of 13 green in 29 s**.
- `CwFixtureTests.TheCleanRecordingsDecodeExactly`: **0 of 2** in 4 s, red as at entry under R53,
  `■ ■ ■ ■ ■  ■ ■ ■ ■■` and `■ ■ ■  ■■■`.

**The printer at exit**, 2 of 2 in 6 s, identical to entry: 021410 WEEKEND 5, THINKING 5, FLEX 1;
013637 ABOVE 2, BREEZE 2; both settled texts as section 1.

**The trees at exit.** The eleven transmit files against `7e209cb4`: nothing. `src` against
`5688a8a5`: nothing - no piece kept. `git diff --stat ee0ea0dc HEAD` over the two floor test files:
nothing; the floor table is the step's entry table. `git worktree list`: the root and the three
preflight trees. **Decision 14's log check**, `ee0ea0dc..HEAD` by `unit397-log.sh`: every one of the
27 piece or pair commits of units 395 to 397 that touched `src` is followed by its revert, `step 4
piece n out`, before the next piece starts; the seam and drop follow-ups of pieces 2, 3 and 12 sit
between their piece and its revert.
