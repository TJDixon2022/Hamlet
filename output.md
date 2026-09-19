READ IN THIS ORDER.

A. The phase goal - Hamlet works Olivia the way it works PSK31. Steps 0
   and 1 done and closed; step 2 partial (2.1, 2.4, 2.5, 2.6 met; 2.2 met
   at -10 dB and not at -16 dB; 2.3 and 2.7 parked); steps 3-6 not
   started, step 3's entry not open until 2.3 is met.
B. Step 2's criteria - 2.1 clean CER 8/250 0.0000, 16/500 0.0000,
   32/1000 0.0000 (ceiling 0.01, variant from RSID yes); 2.2 -10 dB 0.0000
   (0.05), -16 dB 0.9203 (0.10); 2.3 not attempted (parked); 2.4 noise-only
   characters 0 (0); 2.5 hashes 9 of 9, CPU per file 0.797 / 4.031 / 3.453 /
   4.016 / 4.047 s and noise 1.000 / 0.969 / 1.188 s (ceiling 20); 2.6
   measured yes; 2.7 not attempted (parked). Met: 2.1, 2.4, 2.5, 2.6; 2.2
   half (the -10 dB file).
C. The report last: section 4 raises 5 items on top of the carried queue.
   One stands in the way of a criterion in B: the -16 dB file (item 1),
   which measures -17.19 dB in 2500 Hz and reads at CER 0.33 even with
   every block accepted, so no threshold meets 0.10 without showing wrong
   characters. Nothing in pj_mfsk.h had to be copied (no stop material):
   the format's facts are cited data in data/olivia/format.json and the
   receiver is Hamlet's own. Every fixture decoded; none failed to decode
   at all.

UNIT:       361 - complete at task 6 of 6, task 5 built - 2026-09-18 23:34
PHASE GOAL: Hamlet hears, reads, answers and logs Olivia the way it already does PSK31, with the variant taken from the signal's own RSID announcement and never picked by the operator.
UNIT GOAL:  Build Hamlet's own Olivia demodulator that, handed the variant and center the RSID detector read, turns the mode author's clean 8/250, 16/500 and 32/1000 audio into its text at CER 0.01 or under, the -10 and -16 dB files at 0.05 and 0.10, and pure noise into nothing, each in under 20 s of CPU.
ADVANCED:   yes - step 2 goes from 0 of 7 criteria to 4 met (2.1, 2.4, 2.5, 2.6) and 2.2 half met
NUMBER:     Olivia fixtures decoded at or under their ceiling 0 -> 4 of 5 (three clean, two below the noise); noise-only characters 0
DRIFT:      0

| Criterion | This unit's number | State |
| --- | --- | --- |
| 2.1 clean fixtures, CER <= 0.01, variant and center from RSID | 8/250 0.0000, 16/500 0.0000, 32/1000 0.0000; detector gave 69, 70, 71 at 1000.32 Hz | **met** |
| 2.2 below the noise | -10 dB 0.0000 (ceiling 0.05); -16 dB 0.9203 (ceiling 0.10) | **half**: -10 met, -16 not met |
| 2.3 blind variant search | not attempted | parked |
| 2.4 noise-only, each variant at 1000 Hz, zero characters | 0, 0, 0 (highest block S/N 3.20, 3.48, 3.31 against 4.0) | **met** |
| 2.5 hashes and CPU under 20 s per file | hashes 9 of 9; demodulator CPU 0.797 to 4.047 s | **met** |
| 2.6 timing table measured | 0.68267, 0.51200, 0.40960 s/character, `source: measured`, within 2% (exact) | **met** |
| 2.7 drift fixture | not attempted | parked |

## 1. What Claude did

**Surface and gate.** Claude Code on the development machine, branch `main`, every commit pushed.
The prompt claimed `PROJECT: Hamlet`; the tree confirmed it: `PROJECT_CARD.md` says `PROJECT:
Hamlet`, `Hamlet.sln` exists, `SHACK_FACTS.md` and
`src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` exist, `CoreHMI.sln` and `MURC.sln` do not,
root `C:\Source\HamLet`. **Nothing in this report is evidence about the radio** (FACT-004,
HM-DEC-093). The work order carried the status instruction and six tasks; status was written
with `tools/status.sh` in the allowed words (`EXECUTING`, `code`) after every commit and task and
immediately before every `dotnet test`.

**Commits on `main`, all pushed:**

| Commit | Task |
| --- | --- |
| `ec4b466e` | 0 - the unit opens, 1.13.47 -> 1.13.48 |
| `8b7e6591` | 1 - `Unit361Trace.StepTwosTrace` |
| `b16bb1ae` | 2 - `data/olivia/format.json`, `OliviaFormat`, read through `OliviaData` |
| `23b46c2d` | 3 - `OliviaDemodulator`, `OliviaFixtures`, `TheOliviaDemodulatorTests`, carry-forward list |
| `1a800e4d` | 4 - `Unit361Trace.BelowTheNoise` |
| `69d0a953` | §R12 rewrite of `Unit360Trace`, its own commit |
| `e03d3a8a` | 5 - `data/olivia/timing.json` measured, `OliviaTiming`, `OliviaData.Timing` |

### Task 0 - the unit opens

- `PHASE_STATUS.md` has steps 0 and 1 `done` (uncommitted launcher edit, committed here as told).
- **Step 2's entry, first:** `olivia-16-500-qso-rsid.wav` hashes as the manifest says, and the RSID
  detector reads `OLIVIA_16_500 (70) mode OLIVIA variant "16/500" at 1000.32 Hz, error 0.32 Hz,
  quality 0.902, tones right 15, first tone at 0.464 s`. Then the other eight hash: **9 of 9**.
- `UNIT 361 - STEP 2` appended at the end of `PHASE_OUTCOME.md`; no earlier entry touched.
- Version **1.13.47 -> 1.13.48** in `Directory.Build.props`.
- Carry-forward before any change: **engine 111 of 111, app 166 of 166** (first run, no reruns).

### Task 1 - the trace (`Unit361Trace.StepTwosTrace`, asserts nothing)

**1. The variants, from `pj_mfsk.h`'s formulas at 8000 Hz** (the rate `Psk31Resampler.TargetSampleRate`
hands the RSID detector; decision M):

| Variant | Tones | Spacing | Symbol | Bits/symbol | Symbols/block | Chars/block | Block | s/char | Samples/symbol |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 8/250 | 8 | 31.250 Hz | 32.000 ms | 3 | 64 | 3 | 2.048 s | 0.6827 | 256 |
| 16/500 | 16 | 31.250 Hz | 32.000 ms | 4 | 64 | 4 | 2.048 s | 0.5120 | 256 |
| 32/1000 | 32 | 31.250 Hz | 32.000 ms | 5 | 64 | 5 | 2.048 s | 0.4096 | 256 |

The analysis window (`SymbolLen`) is 512 samples, two symbol periods. **The audio agrees with the
header on every clean fixture**: the symbol period measured from the tone changes is 256 samples
(32.000 ms) on all three.

**2. The spectrum after each burst**, 512-sample raised-cosine windows padded to 2048 (3.906 Hz bins):

| File | Tones found | Lowest | Highest | Midpoint vs center | Mean spacing | Occupied (20 dB) vs bandwidth |
| --- | --- | --- | --- | --- | --- | --- |
| 8/250 | 7 of 8 | 891.87 | 1109.23 | 1000.55 vs 1000.32 | 36.225 | 855.5-1144.5 = 289.1 vs 250 |
| 16/500 | 16 of 16 | 768.06 | 1232.37 | 1000.22 vs 1000.32 | 30.955 | 730.5-1269.5 = 539.1 vs 500 |
| 32/1000 | 30 of 32 | 518.99 | 1483.11 | 1001.05 vs 1000.32 | 33.245 | 480.5-1519.5 = 1039.1 vs 1000 |

The tones the header predicts at a 1000 Hz center run 890.625-1109.375, 765.625-1234.375 and
515.625-1484.375 Hz; the measured lowest and highest sit within 5 Hz of them. The two missing
peaks are the peak-picker merging neighbors whose raised-cosine lobes overlap, not missing tones.

**3. The four format constants**, printed by the trace from the headers at the lines named:
scrambling code `pj_mfsk.h:1076` and `:1235` `0xE257E6D0291574ECLL`; shift 13 `:1185`, `:1335`,
applied at `:1192`, `:1337`; character to Walsh index `:1135`, `:1156`, `:1161-1165`; Walsh bit to
tone bit `:1195-1197`, `:1202`; the inverse Walsh butterfly `pj_fht.h:40-43`; Gray code
`pj_mfsk.h:168` and `pj_gray.h:11-12`; tone position `pj_mfsk.h:172`, `:1722-1723`. The pin is
`SOURCE.md` lines 3-5 (fldigi master branch 2026-09-14).

**4. Where it plugs in.** `RsidDetection` gives `Code`, `Name`, `Mode`, `Variant` (`"16/500"`),
`CenterHz`, `Quality`, `TonesRight`, `StartSeconds` - **the first tone, not the burst's end**
(section 4 item 4); the end is `StartSeconds + Symbols / SymbolRateHz` = +1.393 s, from
`rsid-codes.json`. `OliviaData` reads embedded files once and a malformed one yields a null value
and a sentence. The resampler is `Psk31Resampler` at 8000 Hz (`_rsidResampler` in
`MainWindowViewModel`). The PSK31 receive events use `TelemetryCategory.Psk31`.

**5. The RSID detector's CPU alone:** 8/250 2.172 s (28.98 s of audio), 16/500 9.766 s (131.38),
32/1000 8.391 s (106.80), -10 dB 10.500 s, -16 dB 9.906 s, noise 2.234 s (none detected).

### Task 2 - the format as cited data (decision H)

`data/olivia/format.json`: bits per character 7, symbols per block 64, character mask 127, the
upper half negated, the inverse Walsh butterfly `[[1,-1],[1,1]]`, the scrambling code
`E257E6D0291574EC`, the shift 13, the scrambling rule, the tone-bit rotation 1, negative sets the
bit, the Gray table for symbols 0-31, the analysis window of 2 symbols, the null character 0, and
seven variant rows (4/250, 4/500, 8/250, 8/500, 16/500, 16/1000, 32/1000), **every value with its
`pj_mfsk.h`, `pj_fht.h` or `pj_gray.h` line**. `OliviaFormat.Parse` is strict: a value without a
citation, a row whose spacing, symbol length and first tone disagree with its tone count, or a Gray
table that does not send each symbol on its own tone fails the file. `OliviaData.Format` reads it
at startup; the two-argument `OliviaData.Read` keeps its meaning and takes the embedded format.

Tests in `TheOliviaDataTests`: `TheOliviaFormatIsReadAtStartupWithItsVariantsAndConstants` checks
the code and the shift **against `pj_mfsk.h` itself** and the Gray table against `pj_gray.h`'s rule;
`AMalformedFormatIsReportedInWordsAndNoValue` covers cut off, not hex, a disagreeing row, and
missing. **Watched red first**: the startup test failed (`Format` null) and `BothFilesParse`
failed (the missing-format sentence) before the file was embedded. The malformed test passed on
its first run, because the parser was written with it. 6 of 6 green after.

### Task 3 - hear one (2.1, 2.4, 2.5)

`src\Hamlet.RadioEngine\Olivia\OliviaDemodulator.cs`, Hamlet's own: every eighth of a symbol a
512-sample raised-cosine window padded to 2048; the offset within half a tone that puts the most
power on the tones; each tone's power in units of the noise made a likelihood (the noise is the
median tone power over ln 2, the signal what a symbol carries above it) and turned into a soft
value per bit; the sync is the frame within a symbol and the symbol a block starts on whose blocks
stand furthest out of the noise; at the sync each block is read, de-interleaved, descrambled and
correlated against every Walsh function, **and a block below the threshold shows nothing** (§3.3,
§R9). Idle (null) characters are not text. **No tone count, spacing, symbol length, rate,
scrambling code, shift or mapping is a literal in code**: all from `format.json`, the rate the
caller's. Its constructor takes a named variant and a named center (decision I); it is not wired
into the app (decision L).

**Tests** (`TheOliviaDemodulatorTests`): the three clean files (2.1, 2.5), noise at each variant
(2.4), the events (§R13), and the two below-noise files (task 4's, written into the same class
here). Decision J's CER lives once in `OliviaFixtures.CharacterErrorRate`. **Watched first**: the
demodulator was written before the tests first ran, so red-first held only where the first run was
red - 8/250 at CER 0.0526 (its last block lost, the file ending inside the last window) and -16 dB.
Reading past the end as silence fixed 8/250.

Carry-forward after task 3: **engine 119 of 119, app 166 of 166.** **`TheOliviaDemodulatorTests`
went onto the engine line by type and method, four of its five names**; the fifth,
`TheMinusSixteenDecibelFixtureDecodes`, is red, and `docs\carry-forward-tests.txt` never carries a
known red. The work order asked for the type; this is a departure and it is said here.

### Task 4 - below the noise (2.2)

`Unit361Trace.BelowTheNoise` measured the files rather than taking the manifest's word: **-10.48 dB
and -17.19 dB in 2500 Hz** (noise density from 1600-2400 Hz, signal from 700-1300 Hz less it), Es/N0
per symbol 8.55 dB and 1.84 dB. The -10 dB file decodes at CER 0.0000. The -16 dB file does not
meet 0.10 at any setting tried. **Numbers chosen, each the author's, each on the file named:**

| Change, on `olivia-16-500-qso-snr-16db.wav` | CER at threshold 4.0 | CER with every block accepted |
| --- | --- | --- |
| task 3's first cut: max-ratio soft bits, 4 frames/symbol | 0.9363 | not measured |
| noncoherent likelihoods (Bessel I0), 4 frames/symbol | 0.9402 | 0.3865 |
| + `Passes = 3` iterative decoding (the codes hand back to the tones) | 0.9124 | 0.3745 |
| + `FramesPerSymbol = 8` | **0.9203** | **0.3267** |

`SyncThreshold = 4.0` was chosen on the noise-only file (highest block S/N 3.48) and kept: at 3.0
the -16 dB file reads 0.43 and noise would clear it, which shows wrong characters. **The clean and
noise results did not move after any tuning**: CER 0.0000 on all three clean files and the -10 dB
file in every run; noise 0 characters in every run, highest block S/N 3.46 after the likelihood change and 3.48 after the last tuning.
The iterative pass is Hamlet's own and not in `pj_mfsk.h`, whose decoder reads each character once.

### Task 5 - the timing table (2.6, decision N)

`data/olivia/timing.json` now reads `source: measured`, with the method stated: the time from the
first decoded block to the last over the characters the blocks before the last one carried, so a
short final block's idle padding is not charged to every character. `OliviaTiming` parses it and
`OliviaData.Timing` hands it out. `TheTimingTableIsMeasuredByTheDemodulator` was **watched red on
the estimated file** (`source` differs), then green. The §R12 rewrite of `Unit360Trace` (it read the
estimate's `fixed_seconds` and `readings`, which went with the estimate) is its own commit.

**Recorded under §12.1: nothing.** No entry was written to `DECISIONS.md`.

**Mismatches with the work order** (section 5 asked for them even where the work succeeded):
- `RsidDetection` does not give the burst's end (task 1 item 4); it gives the first tone's start.
- `docs\carry-forward-tests.txt` gained four method names rather than the type (above).
- Task 4's two tests were written in task 3's file and first ran in task 3.
- The app carry-forward was 166 of 166 at the start, not 165 of 166.
- Everything else in section 5 held as written: HEAD `dcffd02c`, 1.13.47, the nine fixtures and
  their manifest fields, `pj_mfsk.h` 2367 lines with the structure map as named, `Olivia\` holding
  two files and no demodulator, `timing.json` estimated, `assets\fixtures\captured\` holding only
  `README.md`, engine 111 of 111, `PttOn` 1 and `Arm(` 2.

## 2. What the owner should expect

- **Hamlet's engine reads Olivia** from the mode author's audio, at all three variants and at
  -10 dB, handed the variant and center the RSID burst announced. **The app does not use it yet**:
  the Olivia panel still says nothing decodes (decision L). Step 3 wires it.
- **The -16 dB fixture is not read.** Five blocks of 63 clear the threshold and show 20 characters;
  the rest show nothing. What is shown is right: each of the five four-character blocks
  (`C3QI`, `S de`, `ffor`, `KC3Q`, `QTH `) occurs in the manifest's text.
- `TheMinusSixteenDecibelFixtureDecodes` is **red on purpose** and off the carry-forward list; it is
  the criterion, not a flake. The other nine names in the class are green.
- `data/olivia/format.json` and `data/olivia/timing.json` ship embedded. `OliviaData.Problem` stays
  null on the shipped build.
- Build clean. Final carry-forward **engine 119 of 119, app 166 of 166**, first runs, no reruns.
  `PttOn` 1, `Arm(` 2; nothing on the transmit side was touched.
- Version **1.13.48**. Seven commits on `main`, all pushed. Uncommitted at the end:
  `PROJECT_STATUS.md` (the status file), `RUN_LEDGER.md` and `.run-unit\` (the launcher's), and
  this `output.md`, which the session leaves to the launcher.

## 3. What you should see

**The decode table** (from the run of `TheOliviaDemodulatorTests` after the last tuning; CPU is
the demodulator's own process CPU; the detector's is beside it):

| Fixture | Detector gave | CER (ceiling) | Characters | Blocks decoded / rejected | Demod CPU | Detector CPU | Met |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 8/250 CQ | OLIVIA_8_250 (69), 8/250, 1000.32 Hz | 0.0000 (0.01) | 38 of 38 | 13 / 0 | 0.797 s | 2.172 s | yes |
| 16/500 QSO | OLIVIA_16_500 (70), 16/500, 1000.32 Hz | 0.0000 (0.01) | 251 of 251 | 63 / 0 | 4.031 s | 9.844 s | yes |
| 32/1000 QSO | OLIVIA_32_1000 (71), 32/1000, 1000.32 Hz | 0.0000 (0.01) | 251 of 251 | 51 / 0 | 3.453 s | 8.016 s | yes |
| 16/500 -10 dB | OLIVIA_16_500 (70), 16/500, 1000.37 Hz | 0.0000 (0.05) | 251 of 251 | 63 / 0 | 4.016 s | 10.063 s | yes |
| 16/500 -16 dB | OLIVIA_16_500 (70), 16/500, 1000.77 Hz | 0.9203 (0.10) | 20 of 251 | 5 / 58 | 4.047 s | 9.766 s | **no** |
| noise as 8/250 | none (1000 Hz given) | 0 characters (0) | 0 | 0 / 14 | 1.000 s | - | yes |
| noise as 16/500 | none (1000 Hz given) | 0 characters (0) | 0 | 0 / 14 | 0.969 s | - | yes |
| noise as 32/1000 | none (1000 Hz given) | 0 characters (0) | 0 | 0 / 14 | 1.188 s | - | yes |

Block S/N at the sync: clean files lowest 76.95 to 86.03; -10 dB lowest 13.81, median 17.24;
-16 dB lowest 2.49, median 3.17, highest 5.12; noise highest 3.20 / 3.48 / 3.31. Threshold 4.0.

**The one file that is not exact**, decoded beside the manifest:

```
decoded : "C3QIS defforKC3QQTH "
manifest: "CQ CQ CQ de KC3QIS KC3QIS KC3QIS pse K\nKC3QIS de W1AW W1AW K\nW1AW de KC3QIS RST 599 599 Name Tim Tim QTH Trafford PA Grid FN00 FN00 BTU W1AW de KC3QIS K\nKC3QIS de W1AW R R TNX Tim UR 599 599 Name Bob QTH Newington CT Grid FN31 73 73 KC3QIS de W1AW SK\n"
```

**One decision-K event as written** (the clean 16/500 file); neither carries text or a callsign, and
the test asserts no word of the manifest appears in either:

```
Psk31 olivia_sync {"mode":"olivia","variant":"16/500","state":"found","centerHz":1000.32,"frequencyOffsetHz":-0.32,"symbolPhase":4,"blockPhase":14,"snr":80.97,"threshold":4,"atSeconds":2.338}
Psk31 olivia_run {"mode":"olivia","variant":"16/500","centerHz":1000.32,"sampleRate":8000,"samplesPerSymbol":256,"windowSamples":512,"hopSamples":32,"transformSize":2048,"frequencyOffsetHz":-0.32,"symbolPhase":4,"blockPhase":14,"blocksDecoded":63,"blocksRejected":0,"charactersOut":251,"meanSnr":83.6,"threshold":4}
```

**The timing table before and after:**

| Variant | Before (estimated) | Manifest arithmetic | After (measured) | Measured on |
| --- | --- | --- | --- | --- |
| 8/250 | 0.683 | 0.70152 | **0.68267** | CQ file, 2.338 to 26.914 s over 36 characters |
| 16/500 | 0.514 | 0.51417 | **0.51200** | QSO file, 2.338 to 129.314 s over 248 characters |
| 32/1000 | 0.416 | 0.41626 | **0.40960** | QSO file, 2.338 to 104.738 s over 250 characters |

The 8/250 no-RSID file, read with the manifest's variant and center and reported, not used:
0.68267 s per character over 251 characters, CER 0.0000.

**Every figure here is computed, not seen. Nothing here is evidence about the radio** (FACT-004).

## 4. What's blocking us

**One criterion is not met: 2.2 at -16 dB.** Five new items, all findings; none wants a ruling
from the owner. The carried queue follows them.

### Raised by unit 361

**1. The -16 dB fixture is not read at CER 0.10, and no threshold would read it.**

*A finding; 2.2 reported not met, the test not loosened.* Measured, the file carries -17.19 dB in
2500 Hz (the -10 dB file measures -10.48 by the same method), Es/N0 1.84 dB per symbol. With every
block accepted the demodulator reads it at CER 0.3267; at the 4.0 threshold, which noise never
clears, 0.9203. Tried and kept: noncoherent likelihoods, three iterative passes, eight frames per
symbol. **What the next unit can try**: timing and frequency tracked per block rather than once
per file; the sync decided per block from the neighbors' likelihoods; soft combining of the two
frames either side of the chosen one. Whether the fixture's figure is reachable by any Olivia
decoder was not measured here; the fixture is the mode author's and is not in question (§6).

**2. The demodulator runs over a whole recording, not a stream.**

*A finding for step 3.* `Decode(MonoAudio, startSeconds)` finds one offset, one symbol phase and
one block phase for the whole recording. That is what step 2 asks for and what the fixtures need;
step 3's rows per station will need it fed as the RSID path feeds the detector.

**3. The fixture test runs the RSID detector over the whole file first.**

*A cost, reported.* About ten seconds of CPU on the 131 s files before the demodulator's four, so
the four demodulator names add about a minute to the engine carry-forward (1 m 14 s, 119 tests).
The 20 s ceiling is the demodulator's own and is met with room.

**4. `RsidDetection` carries the first tone's start, not the burst's end.**

*A mismatch with the work order's task 1 item 4.* The end is derived from `rsid-codes.json`
(`StartSeconds + Symbols / SymbolRateHz`), which the tests do in one helper.

**5. Tool facts and status words this session.**

*A finding, reported and not repaired.*
- Python scripts written to the scratchpad and run as `python file.py` ran; unit 360 reported
  Python did not run. `python -c` was not tried.
- A quoted heredoc containing apostrophes broke once, as the work order warned. A `sed`
  substitution with backslashes in the pattern (the csproj line) matched nothing and reported
  nothing; it was redone with the editor.
- Every status write used `EXECUTING` and `code`. `tools/status.sh` still writes
  `RULES_AT: HM-DEC-161` and `WORK_INSTRUCTION` from `PHASE_STATUS.md`, which still says 358.
- `PHASE_PLAN.md`'s unchecked 1.5 and 1.7 and the mislabeled outcome entries are unchanged and not
  edited, as told.

### Asks still outstanding - carried from unit 360's section 4, per HM-DEC-139, verbatim

The words below are unit 360's, from its line under `## 4. What's blocking us` to its end, as
committed in `dcffd02c`. Only that top-level heading is dropped, so this report keeps four
sections. Its nested queues are carried as unit 360 carried them, and the queue of units 337 to
353 is still carried by reference to `4c55deac:output.md`. **One item is marked in place - unit
358 item 5 - and nothing is deleted.** This unit answers none of the others.


**Nothing blocks 1.5 or step 2's entry.** Five new items, all findings; none wants a ruling.
The carried queue follows them.

### Raised by unit 360

**1. `longestSeconds` on a typed line's record says 30 when the send was held to 60.**

*A finding, not repaired.* `TransmitRecord.ToBag` writes `OperatorSend.LongestUnslottedSeconds`
for every no-slot send (`TransmitRecord.cs`, the `longestSeconds` line), not the send's own
`Cap`. The typed record in section 3 shows it: `longestSeconds: 30` on a send held to 60. This
is older than this unit (work instruction 357 moved the cap onto the send and did not move
this). Decision F named the fields this unit adds, and this is not one of them. A later unit can
carry `Cap` through `Recorded` the way this one carried the announcement.

**2. The sequence's refusal sentence quotes the whole audio, not the text the cap measured.**

*A finding, not repaired.* `SendableWithNoSlot` says *this is {audio.Seconds} s of PSK31 audio,
and this send may be at most {Cap} s*. Since R32 (a), the number compared to the cap is
`TextSeconds`, so an announced refusal quotes 1.86 s more than was measured. The sentence is
still true about the audio. Changing it touches `Ft8TransmitSequence` beyond decision F's one
change, so it was left.

**3. `WhereTheTransmissionStartsAndWhatTheRecordSaysTests.ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt`
is red, and older than this unit.**

*A finding, not repaired.* It asserts `Assert.Single` over every event a slotted FT8 run
writes. Since `7de5018b` (every stage writes `send_stage`), the run writes four `send_stage`
events and then the record, so it fails on the count before it reaches the record's shape. It
is not on the carry-forward list. This unit's filter picked it up by name, and this unit
changed no stage.

**4. The typed line's card rounds to *60 s of text* for a line that goes.**

*A finding for whoever next touches the card.* 59.968 s prints as *60 s of text* beside *the
most a typed line may be is 60 s*, and it sends. One character more prints *60.1 s*. The card
and the gate agree, as the test proves, but the word does not show the margin.

**5. Tool facts and status words this session.**

*A finding, reported and not repaired.*
- Needed approval and not run: `python -c`, `jq`, `git restore --source`, `git checkout <rev> --
  <file>`, `git stash push`, `awk`, and `tools/status.sh` run directly (not through `sh`) when
  joined by `&&`. `sh tools/status.sh` alone, and joined by `&&` to `git` and `dotnet test`, ran.
  Earlier units report that Python ran; it did not run this session.
- `sed -n` inside a pipe after `git show` ran. `tail -n +N file | md5sum` ran, and was used to
  check that the carried queue below is byte-identical to `db3edfa1:output.md` from its unit 358
  heading to its end.
- **The first five status writes this session read `STATE: WORKING` and `BALL: claude`**, which
  are not allowed words (`CLAUDE.md` §13.1). Every write from the sixth on, during task 1, used
  `EXECUTING` and `code`. `tools/status.sh` still writes `RULES_AT: HM-DEC-161`, and `WORK_INSTRUCTION` is read
  from `PHASE_STATUS.md`, which still says 358.

### Asks still outstanding - carried from unit 359's section 4, per HM-DEC-139, verbatim

The words below are unit 359's, from its line under `## 4. What's blocking us` to its end, as
committed in `db3edfa1`. Only that top-level heading is dropped, so this report keeps four
sections. Its nested queues are carried as unit 359 carried them, and the queue of units 337 to
353 is still carried by reference to `4c55deac:output.md`. **Two items are marked in place and
nothing is deleted.** This unit answers none of the others.

Nothing blocks step 2's entry: the clean 16/500 burst is detected. Five items. **Item 1 wants
Tim, because it changes what a send can be. Item 2 is stop material the unit did not build.**

**1. The report macro to a compound callsign no longer fits its cap with the burst in front.**

*ANSWERED by R32 (a), Tim 2026-09-18 - the burst does not count against the cap; built in work
instruction 360 task 2.*

*Raised for the next arbiter to take to Tim as a §R10 question, as decision A directs.* The four
macros as §R2 writes them fit, so task 4 was built.

**The numbers:** the report to VP2V/W1AW with the default name and place is 28.192 s. With
Hamlet's burst it is **30.050 s, over the 30 s cap by 0.05 s**, so that send is now refused with
its length where it went before. It would be 29.585 s with the tones and no leading silence.
Unit 318 chose thirty so that this report would fit, with 1.8 s to spare. The burst takes that
margin and 0.05 s more.

**Options:**
- *A, as built:* the burst counts inside the cap, and a long report is refused in words. Nothing
  keys; the operator shortens a Settings field.
- *B, Tim raises the macro cap past thirty.* §R10 says *not more than thirty*, so only he can.
- *C, the arbiter's to decide:* drop the five silent symbols in front of a PSK31 send's burst,
  since the transmitter is keyed for 0.46 s of nothing. That fits at 29.585 s, but Hamlet's burst
  would no longer be the file's shape.

**Recommended:** A until Tim says otherwise. The cap is transmit safety, and C leaves 0.4 s of
margin.

**2. The transmission record does not say the send was announced.**

*ANSWERED by R32 (b), Tim 2026-09-18; built in work instruction 360 task 3.*

*Stop material under task 4's fence, so not built.* Criterion 1.5 says *the transmission record
says so*. `ft8_transmission` is built only in `Ft8TransmitSequence.Recorded`, from
`send.Unslotted`'s mode, fit and seconds. Carrying `AnnouncedCode` into it takes one named
argument there and one optional field on `TransmitRecord`: a change to `Ft8TransmitSequence`,
which the instruction forbids. **What was built instead:** `psk31_send_composed` carries
`announced` and `rsidCode`, and `rsid_sent` follows the keying. **To license it, say** *add
`announcedCode` to the no-slot transmission record*. The slotted branch would not change, and
`TheFt8AndFt4SendsAreByteIdenticalTests` pins it.

**3. The fldigi commit is not recorded, and codes 72 to 75 stay undetected.**

*A finding, not a stop.* The session could list nothing outside `C:\Source\HamLet`, so §R5's pin
is still owed and decision C's table could not be added. Step 2 reads `pj_mfsk.h` from the same
clone and will meet the same wall. A launcher that grants read access to `C:\Source\fldigi`, or a
commit hash written into the next instruction, would close it.

**4. Hamlet's own answer is now 1.86 s longer than the turn timing thinks.**

*A finding for step 4's timing work.* `Psk31Macros.AnswerSeconds`, the §R18 stated equivalent of
one FT8 slot, still reads `Psk31Modulator.SecondsFor`, the text alone. Nothing in this unit was
licensed to move turn patience, and it now undercounts Hamlet's own answer by the burst.

**5. One Stop test went red once in a combined run, on an FT8 send.**

*A finding that repeats unit 355 item 6 and unit 357 item 1.*
`TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns` wrote
the abort frames twice in a run of seven types. It passed alone on the first rerun and in the
final 165 run. It drives a slotted FT8 send, which nothing in this unit touches.

### Asks still outstanding - carried from unit 358's section 4, per HM-DEC-139, verbatim

The words below are unit 358's, from its line under `## 4. What's blocking us` to its end, as
committed in `1444962f`. Only that top-level heading is dropped, so this report keeps four
sections. Unit 357's, 356's, 355's and 354's queues are inside it, as unit 358 carried them. The
queue of units 337 to 353 is carried by reference to `4c55deac:output.md`. **This unit answers
none of them.** Unit 358's item 1, the dial 1500 Hz below the center, is logged and not reopened:
step 0 is closed.

Nothing blocks step 1. Five items; **item 1 is the only one that may want a ruling.**

**1. Where the dial goes for "the calling center" - built as center less 1500 Hz, overrulable.**

*Raised because §6 stops the arbiter for "a fact stated about the radio", and this is where the
radio is put.*

**Ruling proposed:** pressing Olivia sets the dial to the row's center less the audio center the
file's own 20 m row implies (1500 Hz), so the cited center sits mid-passband, and the tune line
names both.

**Reasoning:** in USB a dial at 14.073000 puts a signal centered on 14.073000 at 0 Hz of audio,
below the passband, so the operator would hear nothing where Hamlet said Olivia was. The file
states its dial convention and prints one dial, and the derivation is checked against that
printed dial. Step 1's detector listens across the passband and would need the signal inside it.

**Rejected:** *A, dial to the center literally*, as the instruction words it - the spot would be
inaudible and step 6 could not pass at it. *B, a 1500 Hz constant in code* - a number the file
already carries, typed a second time (§0). *C, the fixtures' 1000 Hz audio center* - that is
where the web thread put its test signals, not where the community's dial convention puts them.

To overrule, say *dial the center itself* or name the audio offset.

**2. The Olivia press writes the radio's mode.**

*No ruling wanted; built and marked.* PSK31 and FT8 presses leave the mode to mode-follow. On
three of seven Olivia spots mode-follow has no block to answer from, so the press asks for USB-D
once the frequency is confirmed. A declined mode is said on the tune line and the tune still
counts. It keys nothing.

**3. HM-OPEN-090: `cq_pressed` says `Ft8` under Olivia and under PSK31.**

*No ruling wanted; a finding.* The CQ press record's `detail` is `_digitalMode`, the
decoder enum, which is `Ft8` for every label that is not FT4. The refusal line after it says
`Olivia` correctly. Named in `OPEN_ISSUES.md` and left (§12.6).

**4. Jalocha's headers are pinned to a date, not a commit.**

*No ruling wanted; a mismatch.* `assets/reference/SOURCE.md` says *master branch 2026-09-14*. Step
2 reads `pj_mfsk.h` for structure; a commit hash would let a later reader fetch the same file.
The headers themselves are in `assets/reference/jalocha/`, so nothing is lost today.

**5. `data/olivia/timing.json` was not parsed by a machine.**

*No ruling wanted; a tool limit.* The PowerShell parse needed approval this session could not
give. It was written by hand from the table in section 1; step 2 reads it first.

*ANSWERED by work instruction 360 task 4 - parsed by System.Text.Json, agrees with the
manifest except 8/250 no-RSID per-character rounded up (0.686 against 0.685).* (Marked in place
by unit 361, as work instruction 361 section 3 directs.)

### Asks still outstanding - carried from unit 357's section 4, per HM-DEC-139, verbatim

The words below are unit 357's, from its line under `## 4. What's blocking us` to its end, as
committed in `bd0805cf`, with only that top-level heading dropped so this report keeps four
sections. Unit 356's, 355's and 354's queues are inside it as unit 357 carried them, and the
queue of units 337 to 353 is carried by reference to `4c55deac:output.md` as unit 354 left it.
**This unit answers none of them.** The screen phase's step 3, Tim's verdict at his window,
stays open with that phase archived.

No criterion changes state. Five items.

**1. The app carry-forward list is not stable run to run, and it got worse this unit.**

*No ruling wanted; a finding, and it firms up unit 355 item 6.* Three runs of the same filter
before anything was changed: the first failed `TheTestsStayOffTheNetworkTests.The354LayoutReadsTheSameNumbersTwiceRunning`, the second failed **four different tests**
(`ThePsk31CqGoesOutTests.ASecondPressRefreshesTheReceiptAndSendsAgain` and three in
`TheStopIsAlwaysOnScreenTests`), and the third was 127 of 127. **Every one of them passed when
run alone.** Parallelism is already off — `TestParallelism.cs` disables it assembly-wide — so
this is state or timing leaking between headless-window tests in one sequential run, not two
threads fighting. **It makes a green baseline a thing you have to run three times to believe**,
and it is the reason this report's "before" number names which run it came from.

**2. The box's head names the station only where the parser named one, and on ordinary
conversational text it often does not.**

*No ruling wanted; a finding.* `WholeMessage` puts `Sender` above the text, and `Sender` is
the speaker of the latest **complete** message. Writing task 4's test, three different
plausible transcripts of a real ragchew line produced an empty `Sender`, so the box showed the
time and the words with nobody's name on them. **The box does not go looking for a callsign in
the text itself** (§0.0) — a station Hamlet has not named is not named there either — so what
is missing is upstream, in when the splitter decides a message has finished. Worth a look
before somebody reads a box a week later and cannot tell whose words those were.

**3. `ADVANCED: blocker` has nowhere to go in `PHASE_OUTCOME.md`.**

*No ruling wanted; a mismatch, reported and not repaired.* Task 0 asks for the entry to carry
`ADVANCED: blocker`. `outcome-entry.py`'s `FIELDS` is twelve names and `ADVANCED` is not one
of them, and no entry in the file has ever carried it. It is in this report's header block,
where §12 defines it, and the entry says *clears a blocker* in its prose as every earlier unit
has.

**4. The typed line is not in the conversation the card reads.**

*No ruling wanted; a finding about what the card will say next.* A macro goes through
`RememberWhatWeSent`, which parses it and files it, so the turn indicator moves when Hamlet
answers. **A typed line does not**: it is not a macro, `Psk31ExchangeParser` would read its
frame as an ordinary over, and whether a free sentence should move the turn is a question
nobody has ruled. The card is refreshed after a typed send, so the screen is consistent; what
it is not is *aware* that he just spoke. Left deliberately.

**5. §2's tool facts, checked again.**

*No ruling wanted; a mismatch report.* **Apostrophes in a quoted heredoc broke again**, exactly
as §2 says, on the first attempt at task 2's engine change; that work moved into script files.
**Python ran**, against §2, as it has for four units. `rm` was not needed. `tools/status.sh`
was not refused. **One new tool fact worth writing down**: a `sed` insertion that lands between
an XML doc comment and the member it documents produces `CS1572`/`CS1573` and fails the build,
because warnings are errors here. It happened four times this unit. Anchor on the doc comment's
first line, not the signature.

### Asks still outstanding - carried from unit 356's section 4, per HM-DEC-139, verbatim

The words below are unit 356's, from its line under `## 4. What's blocking us` to its end, as
committed in `9db74913`, with only that top-level heading dropped so this report keeps four
sections. **Its item 2** — `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow`
red — is neither answered nor re-measured here; it is not on the carry-forward list, and this
unit did not run it.


No criterion changes state. Five items.

**1. The cap is not a cure: the card still asks for 623 px at 1100×780 and is scrolling.**

*No ruling wanted; a finding, and the honest limit of task 1.* The send area is on the window
at all nine sizes, which is what was asked and what is measured. What did **not** happen is
the card getting smaller: its own box is unchanged at 623 px (653 on PSK31), it scrolls inside
a 300 px cap, and unit 354's trace still prints 623 because that is the truth about the
control. The three panels reach 0.217 of the height below the pills against R26's half.
**Making the card fit at that width is a wrapping and typography question** — the license line
takes 17 lines and the rule of thumb 17, each breaking words — and §10 kept this unit to its
own repair.

**2. `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow` is red, and it
is not this unit's.**

*No ruling wanted; a finding that firms up unit 355 item 4.* That unit called it red *at some
runs*. **It is red deterministically here**, and it was proved red before anything in this
unit changed, by stashing the layout change and running it alone. It fails on
`Assert.Single()` over the pills wearing the best-bet badge, with the collection empty while
the green block and the best bet both read *20 m*. It is not on the carry-forward list, which
is why the 112-green baseline did not catch it.

**3. A test was holding a sentence R15 forbids in place.**

*No ruling wanted; a finding worth one line.* `ThePowerIsOfferedTests` asserted that a reading
**with no reference behind it** produced a sentence containing *turn the transmit drive*. A
test that requires a judgement Hamlet is not entitled to make will keep that judgement alive
through every session that respects the suite. It has been corrected here, and the pattern is
the one §12.5 warns about one level up: the fixture was not wrong about the code, the test was
wrong about the rule.

**4. The ALC is still never read by the poll, so all four sentences are proved only from
handed-in readings.**

*No ruling wanted; carried and re-stated because this unit rewrote the sentences.* There is no
`CivRead` for `RigField.Alc` anywhere in the tree, so `RigStateMonitor` never fills it, and
`Psk31AlcForTests` is the only way any of this path has ever been exercised. **A command byte
is not invented to close that** (§0, §4). On a radio, the sentences will appear only once that
read exists.

**5. §2's tool facts, checked again.**

*No ruling wanted; a mismatch report, and this time in the instruction's favor.* **Apostrophes
in a quoted heredoc did break**, exactly as §2 says, on the first attempt at the ALC rewrite;
the work was moved into a script file. **Python ran**, against §2, as it has for three units
now. `rm` was not needed and not tested this unit. `tools/status.sh` was not refused.

### Asks still outstanding - carried from unit 355's section 4, per HM-DEC-139, verbatim

The words below are unit 355's, from its line under `## 4. What's blocking us` to its end, as
committed in `9d5322c5`, with only that top-level heading dropped so this report keeps four
sections.

**Three of its items are answered by this unit and are left in place rather than deleted**, so
the drop belongs to the report that records each ruling: **item 1** was ruled by Tim on
2026-09-14 (*"A, the way it has been"*) and is written into `PHASE_PLAN.md` as R28; **item 2**
and **item 3** were built here, in tasks 3 and 1. **Item 4** is re-measured in this report's
item 2 above and is **not** answered.


**One ask, most blocking first: whether Stop should be disabled with nothing keyed (item 1).** Tim's step 3 verdict
stays open. This unit's nine items come first; unit 354's section 4 follows, carried as section 1 decision 13 says.

### Raised by unit 355

**1. Ruling wanted: Stop pressable at every instant, or disabled with nothing keyed.**

*An ask: it touches the abort (`CLAUDE.md` §0.2).* Task 1 said *enabled only while a send is keyed and disabled
otherwise*; your quoted ruling is *Stop lives in the status bar, always*. Built: always pressable.

| Option | For | Against |
|---|---|---|
| A. Always pressable; the word and the edge change (built) | Meets the send plan's step 1 must-pass and its guarding test; a press before the slot un-arms a waiting send; works when Hamlet is wrong about what is keyed | Pressable when there is nothing to stop |
| B. Disabled only when nothing is armed and nothing is keyed | Grey when idle | Disabled at exactly the moment the app's idea of *armed* is wrong; step 1 and `TheOperatorCanStopItTests` would have to be overruled |
| C. Enabled only while keyed (the instruction's words) | Literal | Cannot take a waiting send off before its slot, the fifteen seconds after a wrong click; same overrule as B |

**The industry-standard answer is A**: an emergency stop is never disabled. Rejected B and C for the reasons in the
table. To overrule, say *grey Stop out when nothing is keyed*.

**2. Ruling wanted, low: *no HTTP client is created under test* does not hold.**

*A finding with a choice.* `MainWindowViewModel.BuildSources` (`MainWindowViewModel.cs:7869`, `:7873` at `b5be0db9`)
constructs `PotaActivitySource` and `SotaActivitySource` at construction, each with its own `HttpClient`, whether or
not they are switched on. The layout fixtures switch them off, so neither sends; no callook client is made. Options:
A, a unit that puts the spot sources behind the same kind of seam; B, create their clients on first fetch; C, accept
*no request leaves* as the test. Recommended A, for the reason task 4 exists.

**3. CQ is still below the window at 1100 x 780.**

*A finding.* This session's trace: CQ 54 x 22 at y 800 on FT8, 830 on PSK31, 816 on the plain window; on the window
at 900 x 620 (y 462, 474). The top row and zero-height panels of unit 354 item 2 are unchanged. Only Stop moved.

**4. `TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow` is red at some runs.**

*A finding, not chased.* Red twice this session, with the network denied and with the fixed answer: no band pill
drew the text *best bet now*. Green in the next two runs, and in unit 354's runs at 02:13 to 02:53. The pill's label
is *likely, going on the hour* when nothing was heard (`BandOpportunity.cs:240`), and the test matches the literal.
It depends on the hour and the run's spot history, not on this unit.

**5. `TheOperatorCanStopItTests.TheStopAddedNoNewRouteToATransmission` is red, and older than this unit.**

*A finding, not repaired.* It asserts one `_armedSend.Arm(` line in `src`; there are two,
`MainWindowViewModel.cs:14439` in `SendMessage` and `:14618` in the PSK31 press, since `87485625`. This unit added
none. The guard's expectation predates the PSK31 door.

**6. Three stop tests failed once each under load.**

*A finding.* `TheOperatorCanStopItTests.TheLineSaysWhatHappenedToTheCarrierAndToTheSound` (1 of 3 isolated runs; the
stop landed after the audio ended), `AClickWhileTheToneIsPlayingStopsTheSoundAndNotJustTheCarrier` and
`TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns` each once in a combined run
of 35; each green on every rerun. Timing, not layout.

**7. On the four-signal recording, one carrier is still held 20 s after the audio ends.**

*A finding, engine, parked.* After the file and 20 s of faint noise the search still held the 2200 Hz carrier by
its keep-readable rule; 700, 1100 and 1608 Hz retired. Not touched (§9).

**8. Mismatches with work instruction 355, reported and not repaired.**

- §5: the sheet Tim reads is unit 349's, `docs/unit349-what-tim-looks-at.md`, not unit 350's.
- §5: `TheStopIsOnScreenTests` does not exist. Unit 354's measurement is `TheTopRowTests.Unit354TraceTheMainWindowAtTheSizesTimCanOpen`,
  a trace.
- §5 and task 1: there was one FT8 and PSK31 Stop, `DigitalStopButton`, in the send area in the tab row, not one per
  panel.
- §5: the status bar holds the tip mark and its line, the achievement quill button, the contact badge line and the
  belt ring with its progress; 46 px tall. There is no control named *tray mark* or *count badge*.
- §12 `NUMBER`: *5 of 9* does not match the record. Unit 354 found Stop off the window at 1100 x 780 only, 8 of 9.
- §11 *push at the end*: the owner's prompt says push each task, and each was pushed.
- Tool facts: refused, a `for` loop over `$f` (*simple_expansion*) and `;`; blocked, `tee` to `/tmp` and
  `git show … > output.md`; needed approval and not run, `git worktree add` and `git restore --source`.
  `tools/status.sh` writes `RULES_AT: HM-DEC-161 (2026-09-11)`; this unit's writes kept it until the last, which is
  set to `HM-DEC-163 (2026-09-12)`.

**9. Where unit 354's items stand after this unit.**

- Item 1, Stop below the window at 1100 x 780: *ANSWERED for Stop* by your ruling A and task 1; CQ still below
  (item 3 above).
- Its finding that the plain fixture asks callook.info: *ANSWERED* by task 4.
- Items 2 to 6: unchanged.

### Asks still outstanding - carried from unit 354's section 4, per HM-DEC-139

Unit 354's opening, verbatim, from its line under `## 4. What's blocking us`:

**One ask, most blocking first: Stop is drawn below the window at the size Hamlet opens at (item 1 under
*Raised by unit 354*).** Tim's step 3 verdict stays open.

Unit 353's section 4 comes first, verbatim per HM-DEC-139, from its line under `## 4. What's blocking us` to its
end, as committed in `0d69123a`. It was kept in place with the file editor, and the marks work instruction 354
§9 asks for were added:
- unit 349 item 1, *STILL OPEN*;
- unit 353 item 2, *TAKEN UP by work instruction 354 ruling 78*, with the result;
- unit 353 item 3's `Unit332TwoWidthsTests` bullet, *LOGGED, NOT CHASED*;
- unit 353 item 5, *UPHELD for the reloads*.

This unit's six items follow at the very end, under *Raised by unit 354*. Item 1 is an ask; the rest are
findings.

**The queue unit 354 carried from units 337 to 353** is `4c55deac:output.md`, lines 280 to 2341, unchanged, not
retyped here (section 1 decision 13). Unit 349 item 1, Tim's step 3 verdict, is among it and *STILL OPEN*.

### Raised by unit 354

**1. Ruling wanted: at 1100 x 780, the size Hamlet opens at, Stop is drawn below the window.**

*Mark, unit 355: ANSWERED for Stop by Tim's ruling A of 2026-09-14 and work instruction 355 task 1 (`151a108d`); CQ
still below the window, unit 355 item 3.*

*An ask, under ruling 77: it touches the abort (`CLAUDE.md` §0.2).*
- **Measured** (`00454639`, computed on the host, not seen): the window's bottom edge is at y 780.
  `DigitalStopButton` is 74 x 22 at y 800 on FT8 and 830 on PSK31, and at 798 on the plain window. `DigitalSendCqButton`
  and `ModeTabs` are beside and above it. It is on the window at 900 x 620 (y 462) and at every other size measured.
- **Cause as measured:** the neighborhood card's green block is 218 px wide at that width, its lines stack to a
  623 px top row (653 on PSK31), and the rows under it are pushed off the window. The same geometry gives
  item 2's zero-height panels.
- **Who sees it:** a fresh install, or anyone whose saved size is about this size (`App.axaml.cs:93`-`96`).
- **The question**, for the next arbiter and Tim:

  | Option | For | Against |
  |---|---|---|
  | A. Author a unit that keeps Stop, CQ and the panels on the window at 1100 x 780 and 900 x 620, before Tim's verdict | The abort is reachable at the size Hamlet opens at; Tim reviews a window that meets R26's *at no window size* | A `src` change while Tim may be reviewing, which ruling 47 held off |
  | B. Raise the opening size and minimum to sizes that measure whole | A small change | 1536 x 824 still misses R26 here, and 1400 x 1040 is taller than a maximized 1366 x 768 laptop, so this hides the fault at a size Tim can still drag to |
  | C. Leave it to Tim's verdict at his own size | No work now | Tim may give the verdict on a window whose abort is off-screen at first launch |

  **The industry-standard answer is A.** A stop control that can be laid out off the window at the product's
  own default size is a safety defect, not a styling one. Tim rules.

**2. R26 misses at every listed size under 1040 tall.**

*A finding.* The top row over 0.262 of below the pills and the three panels under half, FT8 [PSK31]:
- **900 x 620:** top row 285 px, 156.6 over [297, 168.6 over]; panels 0, 245 short. The green block's left column is 0 px wide:
  the band, frequency, mode, license and rule-of-thumb lines are not drawn, which §0.5's *collapsing hides detail, never
  information* would call information hidden.
- **1100 x 780:** top row 623, 452.7 over [653, 482.7]; panels 0, 325 short. The plain window: 620, panels 0.
- **1280 x 720:** 270, 115.4 over [291, 136.4]; panels 103, 192 short [82, 213].
- **1366 x 728:** 242, 85.3 over [254, 97.3]; panels 139, 160 short [127, 172].
- **1536 x 824:** 196, 14.2 over [208, 26.2]; panels 281, 66 short [269, 78].
- Rig within 0 px of the card everywhere. Holds at 1400 x 1040, 1920 x 1040, 1920 x 1017 and 2560 x 1400. On the sheet
  as items 29 to 33.

**3. Text is trimmed at the anchors too, which no earlier unit recorded.**

*A finding.* *nothing decoded yet* in the Decoded text header is trimmed to 180 of 190 px at every licensed
size, 1400 and 1920 included. On the plain window *021130 UTC · 2 shown · oldest first* is trimmed to 180 of
350. *not listening yet* in the waterfall header is trimmed at 1366, 1400 and 1536. All are measured on the host's
wide text. On the sheet as item 34.

**4. The achievements window clips 8 runs at 900 x 620 and none at 1040 x 720 or wider.**

*A finding.* The runs are named in section 3 and on the sheet as item 35. No card is white at any size. The window
declares no minimum, so this size is reachable. Its category pages scroll, so cards past the bottom edge are not a
miss.

**5. What the traces do not measure.**

*A finding.*
- The licensed window's callsigns and card: it draws no decoded row or card. They are measured only on the plain
  window at 900 x 620 and 1100 x 780, where both rows sit below a 0 px panel and so read *none clipped*.
- Whether the band pills stay put, and §0.5's collapsed summaries at small sizes.
- Whether an outer box clips text that overruns a non-clipping one. For example, the mode strip's status sentence
  runs past its `StackPanel` at every size, 1920 included.
- The screen itself: every number is the host's, whose text is about half again wider than the glass.

**6. Mismatches with work instruction 354, and the tool facts.**

*A finding, reported and not repaired.*
- **§1 and §2: `TheWorkingPanelsTests.cs:510` builds `EmptyTab`'s window**, not `Realized`'s. `Realized` builds its
  window at `:778`.
- **Ruling 76 places the achievements table in the sheet's section 3.** The sheet's section 3 is *Decided for
  you*, so the table went after 2.4 (section 1, decision 4).
- **§2's launcher files held.** `PHASE_STATUS.md` was committed whole with the launcher's `HEARTBEAT` line.
- **This unit's own citation.** `b902a297`'s message says the table is at `:84`-`102`; it is at `:84`-`100`, and the
  message cannot be amended on a pushed commit. This report cites the lines as they are.
- **The tool facts, against §7.**
  - Ran: `sh tools/status.sh` joined by `&&` to `date` and `timeout … dotnet test … | grep`;
    `git add && git commit -m -m && git push && git log | cut`; `grep -o -e`, `grep -n -o -e`, `grep -rl` and
    `wc -l` on trx and source files.
  - Asked for approval and not run: a `grep -n -o` with a `\{0,160\}` count.
  - Refused: a `for` loop over `$c` (*Contains simple_expansion*); `git show 0d69123a:output.md > testresults\…`
    (*Output redirection … was blocked*), though the path is inside the root.
  - Not tried: `pwd -W`, `sed`, `awk`, `tasklist`, `sh` on a script.
  - Status: the first write at 02:40:20 read `STATE: RUNNING` and `BALL: claude`, neither an allowed word, and every
    later write used `EXECUTING` and `code`. `tools/status.sh` still writes `HM-DEC-161 (2026-09-11)`. Every write
    was set back to `HM-DEC-163 (2026-09-12)` with the file editor, except that the 02:41:28 and 02:42:15 writes
    ran back to back without the edit between them.
