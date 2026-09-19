READ IN THIS ORDER.

A. The phase goal - Hamlet works Olivia the way it works PSK31. Steps 0
   and 1 done and closed; step 2 was five of seven met at the start of
   this unit (2.1, 2.2, 2.4, 2.5, 2.6) and is seven of seven met on this
   report's evidence, for the arbiter to mark; steps 3-6 not started, and
   step 3's entry opens only when 2.3 is met.
B. Step 2's two open criteria - 2.3 blind search: candidates found 1,
   variant named 8/250 against 8/250, center 1000.00 Hz against 1000 within
   5 Hz, CER 0.0000 (0.05), audio seconds before the naming 4.096, search
   CPU 0.703 s, noise-only candidates 0 (0); 2.7 drift: CER 0.0000 (0.05),
   offset tracked -0.76 to 42.21 Hz. Met: 2.3, 2.7. The five already met:
   re-measured, with the numbers - the demodulator changed (per-block
   offset tracking, decision V): clean 0.0000 x3, -10 dB 0.0000, -16 dB
   0.9044 (was 0.9203), noise 0 characters x3, CPU within half a second of
   unit 361's.
C. The report last: section 4 raises 7 items on top of the carried queue;
   none stands in the way of 2.3 or 2.7. The search named no wrong
   variant on any file it was run on. Nothing in pj_mfsk.h had to be
   copied; it was not opened this unit. Task 2 left the -16 dB test
   asserting nothing the corrected 2.2 does not ask for: it measures and
   prints the CER, and fails only on no read, the CPU ceiling, or a
   character no accepted block could carry (2.2 with 2.5 and section 3.3).
   One mismatch with the plan's wording: 2.3 says "from the tone spacing
   and symbol rate"; decision P, which the search follows, says spacing
   and occupied band. Item 1 explains why that is the same thing for
   Olivia.

UNIT:       362 - complete at task 4 of 5, task 4 built - 2026-09-19 11:37
PHASE GOAL: Olivia on the air in Hamlet as fully as PSK31 - heard, read, answered and logged - with the variant always taken from the signal (its RSID, or for a carrier that never announced itself, measured off the audio) and never chosen by the operator.
UNIT GOAL:  Find the unannounced 8/250 carrier in the mode author's audio from the audio alone, name its center and variant, hand it to the demodulator for a decode at CER 0.05 or under, find nothing in pure noise; and hold a carrier drifting 20 Hz a minute.
ADVANCED:   yes - 2.3, step 2's last must-pass, is met with its numbers, and 2.7 is met too; step 3's entry now depends only on the arbiter marking step 2.
NUMBER:     step 2 criteria met 5 of 7 -> 7 of 7; the unannounced carrier found blind 0 -> 1
DRIFT:      0

| Criterion | This unit's evidence | State |
| --- | --- | --- |
| 2.1 clean fixtures CER <= 0.01, variant from RSID | **re-measured**: 8/250 0.0000, 16/500 0.0000, 32/1000 0.0000; detector 1000.32 Hz each | met |
| 2.2 -10 dB <= 0.05; -16 dB measured, no ceiling | **re-measured**: -10 dB 0.0000; -16 dB 0.9044 (24 characters from 6 blocks, all right) | met |
| 2.3 no-RSID 8/250 found blind, stated time, CER <= 0.05 | one candidate, 8/250 at 1000.00 Hz, after 4.096 s of audio and 0.703 s CPU; decode after it CER 0.0000; noise 0 candidates | **met** |
| 2.4 noise-only emits zero characters | **re-measured**: 0 / 0 / 0 at 8/250, 16/500, 32/1000 (highest block 3.45, 3.27, 3.15 against 4.0); blind search on it 0 candidates | met |
| 2.5 hashes, under 20 s CPU each | **re-measured**: 9 of 9; demodulator CPU 0.813 to 4.469 s alone; decode after the blind search 5.063 s | met |
| 2.6 timing table measured | **re-measured**: TheTimingTableIsMeasuredByTheDemodulator green after the tracking change, 0.68267 / 0.51200 / 0.40960 s per character, unchanged | met |
| 2.7 a carrier drifting 20 Hz a minute holds | CER 0.0000, 251 of 251; offset tracked -0.76 Hz at 2.9 s to 42.21 Hz at 131.4 s against the ramp's 0.21 to 43.04 | **met** |

## 1. What Claude did

**Complete: tasks 0 to 4 of 5 (0-4), all built, none dropped.** Machine: Tim's development box,
`C:\Source\HamLet`, gate held (SHACK_FACTS.md and CwProbabilisticDecoder.cs present, CoreHMI.sln and
MURC.sln absent), branch `main`. Five commits, **every push succeeded**: `49f58a21`, `44bd014f`,
`af2cf4c2`, `d961733a`, `690e41e5`. Status written at every task boundary and before every
`dotnet test`, `EXECUTING` and `code` throughout.

### Task 0 - the unit opens

`PHASE_STATUS.md`: steps 0 and 1 `done`, **step 2 `not started`**, `WORK_INSTRUCTION: 358` (stale,
as the instruction expected). `olivia-8-250-qso-norsid.wav` hashes as its manifest says, 8000 Hz,
172.06 s; **the RSID detector finds nothing in it** over 200-3000 Hz (0 detections, 13.5 s CPU). The
other eight hash, 9 of 9. `UNIT 362 - STEP 2` appended to `PHASE_OUTCOME.md`; version 1.13.48 ->
1.13.49. Carry-forward before any change: **engine 119 of 119 (1 m 16 s), app 166 of 166 (38 s),
first runs.**

### Task 1 - the trace (`Unit362Trace`, asserts nothing)

1. **The no-RSID file, measured blind from 1 s on.** Averaged spectrum (3.906 Hz bins): floor -57.3
   dB, peak 89.9 dB over it at 1046.88 Hz; **band 20 dB down from the peak 867.19-1132.81 Hz =
   269.53 Hz, middle 1000.00 Hz.** The loudest frequency frame by frame (512-sample windows, 2 ms
   hop) piles into **8 peaks, 890.63 to 1109.38 Hz, mean spacing 31.250 Hz, middle 1000.00 Hz**.
   Symbol period from the spectral change: 30.00 ms (the variant's is 32; hop 2 ms). The band has
   power from the first frame; the decode's first block begins at 0.016 s. **Check, not input:**
   manifest 8/250 at 1000. Two other spacing measures failed and are why the search does not use
   them: the ripple across the averaged spectrum read 35.16 Hz, and the histogram's own
   autocorrelation 62.50 Hz.
2. **The noise-only file, the same measurements.** Floor 2.8 dB, loudest bin 0.86 dB over it (max
   over median 1.220); **no bin 3 dB over the floor, 0 of 717.** The 20 dB band is the whole
   passband and the histogram has 74 scattered peaks (mean gap 37.95 Hz) - nothing periodic. The
   symbol-period measure read 32.00 ms on noise too, so it is no discriminator.
3. **format.json's seven rows.** Tones / bandwidth / spacing / symbol / first tone offset:
   4/250 4, 250, 62.5, 16 ms, -93.75; 4/500 4, 500, 125, 8 ms, -187.5; 8/250 8, 250, 31.25, 32 ms,
   -109.375; 8/500 8, 500, 62.5, 16 ms, -218.75; 16/500 16, 500, 31.25, 32 ms, -234.375; 16/1000 16,
   1000, 62.5, 16 ms, -468.75; 32/1000 32, 1000, 31.25, 32 ms, -484.375. **Every pair is separated
   by (spacing, bandwidth); no pair is left that measurement cannot tell apart.** Neither alone
   does it: 8/250, 16/500 and 32/1000 share the spacing (and so the symbol rate, which is the
   spacing for every row), and 4/250 and 8/250 share the bandwidth. Eleven same-spacing or
   same-bandwidth pairs, each separated by the other measure.
4. **A trial decode at the measured center, 1000.00 Hz**, whole file: 8/250 CER 0.0000, 84 blocks /
   0, sync S/N 89.69, 5.1 s CPU; 16/500 CER 0.9960, **1 block through at 4.10** / 83 rejected;
   32/1000 CER 1.0000, 0 / 84, highest block 3.60. **The number a wrong variant shows on is the
   sync S/N** (89.69 against 4.10 and 0), so that is what task 3 ranks on. The first 4 s already
   give one 8/250 block at 86.95 and nothing at the other two; 4 s of noise tops out at 3.46.
5. **Where drift bites.** `OliviaDemodulator.Decode` step 2 (then lines 241-266) scored every offset
   within half a tone of the center over every frame and kept one for the whole recording. 20 Hz a
   minute is 43.79 Hz over the 131.38 s 16/500 file (1.401 spacings; 21.90 Hz off at each end from
   a middle offset, 0.70 spacing) and 9.66 Hz over the 28.98 s 8/250 CQ (0.309 spacings).

### Task 2 - the -16 dB test (decision U, its own commit `af2cf4c2`)

**The assertion now, in one sentence:** the -16 dB file's burst is heard as 16/500 within 5 Hz of
1000, at least one block is read, the read takes under twenty seconds of CPU, every accepted block
cleared the threshold, and the characters shown are no more than the accepted blocks could carry
(bits per symbol each) - with the CER printed and no ceiling on it. **It fails** if the burst is
missed or misnamed, if no block clears the threshold, if the decode passes 20 s of CPU, or if
characters appear that no accepted block produced. Read at the commit: CER 0.9203, 20 characters
from 5 blocks, 4.0 s CPU. Its name is on the engine line of `docs\carry-forward-tests.txt`, and the
`WHAT UNIT 361 ADDED` paragraph now says it was left off by unit 361 for being red, followed by
`WHAT UNIT 362 CHANGED`.

### Task 3 - the blind variant search (2.3, `d961733a`)

**`OliviaBlindSearch`** (`src\Hamlet.RadioEngine\Olivia\OliviaBlindSearch.cs`), engine only, wired
to nothing. `Search(MonoAudio, lowestHz, highestHz)` returns `OliviaSearch(Candidates, AudioSeconds,
FloorDb, PeakOverFloorDb)`; each `OliviaCandidate` carries the variant, the center, a confidence (the
chosen trial's sync S/N), the audio seconds it needed, the measured middle, spacing, tone count and
occupied band, and every `OliviaTrial` weighed. **It is given no center, no variant and no start
time**, and every resolution it looks with is derived from format.json's rows; no tone count,
spacing, symbol length or bandwidth is a literal.

The shape: audio from the start, a slowest-row block (2.048 s) at a time, two to begin with. Over
what has been taken, a long averaged spectrum; every region standing `GateSigmas` = 8 standard
deviations of the averaged noise over the passband median, at least half the narrowest row's
bandwidth wide, is looked at; its band 20 dB down from its peak is the occupied band; the tones
are the peaks of a loudest-frequency histogram, spacing the median gap between neighbors. Rows
within half an octave of the band or of the spacing are weighed, both-fit first, each by trial
decode at the measured middle; the row with the best sync S/N **on at least `ConfirmBlocks` = 2
accepted blocks** is named, at the middle plus the demodulator's own offset. Otherwise more audio.

**Tests watched failing first** against a stub that found nothing: all three red (no candidate; no
candidate; the noise test on the audio seconds, since the stub consumed none). Then green.

**Decisions this session made for itself, author's and overrulable** (a mechanism and numbers the
arbiter's rules leave to the unit):
- **The spacing is counted off the tone peaks, not the spectrum's ripple** - the trace measured the
  ripple at 35.16 Hz and the histogram autocorrelation at 62.50 Hz on 31.25 Hz tones.
- **The histogram is summed over one transform bin** (five quarter-bin cells) and the spacing is the
  **median** gap. Without them, the first 4.096 s gave 7 tones and 36.46 Hz; with them, 8 and
  31.250.
- **`ConfirmBlocks` = 2.** At one block, the clean 16/500 file was first named on one block at 4.15,
  where the trace had just seen a wrong row (16/500 read over 8/250) put one block through at 4.10,
  and 4/500 read over the 16/500 file put one through at 4.37. One block barely through is what a
  wrong row can do.
- **`GateSigmas` = 8**: at 30 s of noise the gate stands about 2 dB over the median, where the noise's
  loudest bin stood 0.86 dB.
- **`TheOliviaBlindSearchTests` and `TheOliviaDriftTests` run in a non-parallel xunit collection,
  `CpuMeasuredAlone`.** The first carry-forward run with the class on it was **red, 122 of 123**:
  the decode after the search read 23.5 s of *process* CPU because the other classes were running
  beside it (it reads 5.1 alone). Nothing was loosened; the measurement was made to measure the test.

Carry-forward with `TheOliviaBlindSearchTests` on the engine line: engine **122 of 123 (red, above),
then 123 of 123 in 1 m 23 s**; app 166 of 166 (36 s), first run. **Cost**: the class itself is
about 8 s of the engine invocation (three searches, one whole-file decode).

`Unit362Trace.TheSearchOverTheOtherFixtures`, printed and not asserted, runs the same search over the
files that have an RSID burst in front of them (which the search is not told about) - see section 3.

### Task 4 - the carrier that drifts (2.7, `690e41e5`)

**The drifted audio is made in the test and written nowhere** (decision S). The recipe: the clean
`olivia-16-500-qso-rsid.wav`, hash checked; made analytic in one transform over the whole file
(zero-padded to 2^21, negative frequencies zeroed, positive doubled); multiplied by
exp(j 2 pi (r/2) t^2), r = 20/60 Hz per second, t = 0 at the first sample; real part kept. Its tones
rise 43.79 Hz by 131.38 s. The 16/500 file was chosen over the 28.98 s 8/250 CQ because the CQ
drifts only 0.31 spacings, which one offset already holds; the long file is the one that tests it.
Nothing is written under `assets\fixtures\olivia\` and the manifest is untouched.

**Watched failing first** on one offset: CER 0.2032, 235 characters, 60 blocks through, 3 rejected.

**Decision V, built:** `OliviaDemodulator` now tracks the offset per block - each block's frames score
every offset on a grid widened by `TrackTones` = 2 spacings, summed over `TrackSmoothing` = 2 blocks
either side, and the best path through them is found by dynamic programming, starting within half a
tone of the center as the one offset did and moving at most one grid step (3.9 Hz) a block.
Continuity from the start is what keeps it on the carrier: a comb shifted by one spacing looks like
the unshifted one. `OliviaDecoding` gains `OffsetTrack` (offset at each block-segment's middle);
the sync events carry their own block's offset; `FrequencyOffsetHz` is the offset at the first
decoded block. After it: **CER 0.0000, 251 of 251, 63 blocks, 4.1 s CPU**; the tracked offset
ends 0.83 Hz from the ramp (test tolerance an eighth of a spacing, 3.9 Hz).

**The eight rows re-measured** (section 3); no clean CER moved and no noise character appeared, so
the change stays. Carry-forward after: **engine 124 of 124, app 166 of 166 (35 s)**, first runs.
`CivConstants.PttOn` code lines 1 (`Ft8TransmitSequence.cs:513`), `_armedSend.Arm(` lines 2, as at
the start. Nothing on the transmit side and nothing in the app was touched.

### Verified against the tree - mismatches with section 5 of the instruction

- **"The engine line carries `TheOliviaDemodulatorTests` ... four of its five names."** The type has
  **six** test methods; the sixth, `TheTimingTableIsMeasuredByTheDemodulator`, was never on the line.
  Four of six at the start, five of six now. Noted in `docs\carry-forward-tests.txt`.
- Everything else in section 5 held: HEAD `483a5af3`, 1.13.48, `output.md` absent, `PHASE_OUTCOME.md`
  ending on `UNIT 361 - STEP 2` with no FATE, the nine fixtures and their seconds, the five Olivia
  files, the demodulator's signature and constants, the tests' files, carry-forward 119 / 166,
  PttOn 1, Arm( 2, `assets\fixtures\captured\` holding only `README.md`.
- **Tool facts differed from section 2**: a command with `;` ran (task 0's first command).
  `grep -v "^\s*$"` and `sed -n '/a/,/b/p'` in a pipe after `dotnet test` each needed approval;
  plain `grep -E` and `tail` did not.
- The known expected mismatches (RULES_AT, PHASE_STATUS step 2 and 358, 1.5/1.7 unchecked, the
  outcome labels, data file names in R27/R29, the three off-list reds) were not rediscovered; none
  was edited.

## 2. What the owner should expect

- **Hamlet's engine can now find an Olivia station that sent no RSID** and name its variant and
  center from the audio - 8/250 at 1000.00 Hz after 4.1 s of audio - and hears nothing in pure
  noise. **Nothing on screen changes**: it is wired into no panel (decision L), so the Olivia panel
  still says nothing decodes. Wiring it is step 3's.
- **The demodulator now follows a drifting carrier**, up to about two tone spacings from where it
  started, a grid step a block. A clean recording reads exactly as before.
- **What will look wrong but is not:**
  - The -16 dB number moved from 0.9203 to 0.9044. That is the tracking, not a regression; no
    ceiling applies and every character it shows is right.
  - The blind-search and drift test classes run after everything else in the engine invocation,
    not beside it. That is on purpose (`CpuMeasuredAlone`).
  - `TheOliviaDemodulatorTests` CPU figures read about twice as high in the carry-forward run as
    alone (8 s against 4). They share the process; see item 3.
  - `PHASE_STATUS.md` still says step 2 `not started` and unit 358; `PROJECT_STATUS.md` says
    `WORK_INSTRUCTION: 358` because `tools/status.sh` reads it from there.

## 3. What you should see

**No visible change in the application** - this unit is engine and tests only. What it proves, from
the test runs (computed, not seen; nothing here is evidence about the radio, FACT-004):

**The blind-search table** (`TheOliviaBlindSearchTests`, class run in its collection during the
engine carry-forward run after task 3; CPU is process CPU with nothing else running):

| | no-RSID 8/250 | noise-only |
| --- | --- | --- |
| Audio consumed before naming | **4.096 s** | 30.000 s (all of it), nothing named |
| Search CPU | **0.703 s** | 0.219 s |
| Floor / loudest bin over it | -55.7 dB / 90.21 dB | 2.8 dB / 0.89 dB (gate not reached) |
| Occupied band, 20 dB down | 865.23-1134.77 Hz = 269.53 Hz | none |
| Tones counted / spacing | 8 / 31.250 Hz | none |
| Measured middle of the tones | 1000.49 Hz | - |
| Rows admitted, in rank order | 8/250 (both fit), 16/500 (spacing), 4/250 (band), 32/1000 (spacing) | none |
| Trial decode of each: sync S/N, blocks | 8/250 75.72 on 2/0; 16/500 0.00, 0/2 (best 2.60); 4/250 0.00, 0/4 (best 3.98); 32/1000 0.00, 0/2 (best 2.89) | - |
| Chosen | **8/250** | **no candidate** |
| Center against 1000 Hz (5 Hz) | **1000.00 Hz** | - |
| Decode after it, whole file, from 0 s | **CER 0.0000 (0.05)**, 251 of 251, 84 blocks / 0, sync S/N 89.69, 5.063 s CPU (2.5's 20 s) | - |

The decoded text is the manifest text exactly.

**The search's §R13 event, as written** (category `Psk31`, one per region weighed, or one saying
nothing stood out):

```
Psk31 olivia_search {"mode":"olivia","variant":"8/250","found":1,"audioSeconds":4.096,"passbandLowHz":200,"passbandHighHz":3000,"floorDb":-55.71,"peakOverFloorDb":90.21,"gateSigmas":8,"threshold":4,"centerHz":1000,"confidence":75.72,"measuredCenterHz":1000.49,"toneSpacingHz":31.25,"tonesCounted":8,"occupiedLowHz":865.23,"occupiedHighHz":1134.77,"weighed":["8/250","16/500","4/250","32/1000"],"weighedSnr":[75.72,0,0,0],"weighedBlocks":[2,0,0,0]}
```

No text, no callsign: the test asserts no word of the manifest text of three letters or more is in
it and no string value is 12 characters or longer.

**The same search over the other signal fixtures** (`Unit362Trace`, not asserted; each has an RSID
burst in front that the search is not told about; CPU with another class running beside it):

| File (manifest) | Named | Audio | Spacing / tones / band | Other rows weighed |
| --- | --- | --- | --- | --- |
| 16/500 clean (16/500, 1000) | 16/500 at 1000.00, S/N 83.82 | 8.192 s | 31.250 / 16 / 515.63 Hz | 32/1000, 8/250, 8/500 0; **4/500 one block at 4.37** |
| 32/1000 clean (32/1000, 1000) | 32/1000 at 1000.00, S/N 91.69 | 8.192 s | 31.250 / 29 / 1015.63 Hz | 16/500, 16/1000, 8/250 0 |
| 8/250 CQ (8/250, 1000) | 8/250 at 1000.00, S/N 77.14 | 8.192 s | 34.180 / 7 / 269.53 Hz | 4/250, 16/500, 32/1000 0 |
| 16/500 -10 dB | 16/500 at 1000.00, S/N 17.80 on 48 blocks | 102.400 s, 17.8 s CPU | 39.063 / 5 / 144.53 Hz | 8/250, 32/1000 0 |
| 16/500 -16 dB | nothing | 131.378 s, 4.2 s CPU | loudest bin 0.87 dB over the floor, gate not reached | - |

**The phase's three rows separate on audio**, and the four others were weighed where they fit and
never named. That the four other rows would separate on their own audio is shown on paper only
(task 1 item 3); there is no fixture for them.

**The drift table** (`TheOliviaDriftTests`):

| | One offset (before) | Tracked (after) |
| --- | --- | --- |
| CER (0.05) | 0.2032 | **0.0000** |
| Characters / blocks | 235 / 60 through, 3 rejected | 251 of 251 / 63, 0 |
| Detector | 16/500 at 1000.76 Hz, burst at 0.465 s | same |
| Offset reported | 7.06 Hz, one for the file | tracked, below |
| CPU | 4.0 s | 4.1 s |

Tracked offset, seconds: Hz (the ramp's, less the detector's 0.76): 2.9: -0.76 (0.21), 19.3: 7.06
(5.67), 35.7: 10.96 (11.13), 52.0: 14.87 (16.59), 68.4: 22.68 (22.05), 84.8: 26.59 (27.52), 101.2:
34.40 (32.98), 117.6: 38.31 (38.44), **131.4: 42.21 (43.04)**. The recipe is in section 1, task 4.

**Decision V - unit 361's eight rows re-measured** (`TheOliviaDemodulatorTests` run alone after the
tracking change; unit 361's figure in brackets):

| Fixture | CER | Characters | Blocks decoded / rejected | Demodulator CPU |
| --- | --- | --- | --- | --- |
| 8/250 CQ | 0.0000 (0.0000) | 38 of 38 | 13 / 0 | 0.813 s (0.797) |
| 16/500 QSO | 0.0000 (0.0000) | 251 of 251 | 63 / 0 | 4.109 s (4.031) |
| 32/1000 QSO | 0.0000 (0.0000) | 251 of 251 | 51 / 0 | 3.656 s (3.453) |
| 16/500 -10 dB | 0.0000 (0.0000) | 251 of 251 | 63 / 0 | 4.469 s (4.016) |
| 16/500 -16 dB | 0.9044 (0.9203) | 24 of 251, all right | 6 / 57 (5 / 58) | 4.172 s (4.047) |
| noise as 8/250 | 0 characters (0) | 0 | 0 / 14, highest 3.45 | 1.031 s (1.000) |
| noise as 16/500 | 0 characters (0) | 0 | 0 / 14, highest 3.27 | 0.984 s (0.969) |
| noise as 32/1000 | 0 characters (0) | 0 | 0 / 14, highest 3.15 | 1.078 s (1.188) |

## 4. What's blocking us

**Nothing blocks step 2 or step 3's entry.** Seven new items, all findings; none wants a ruling
from the owner. The carried queue follows them.

### Raised by unit 362

**1. 2.3's wording says "tone spacing and symbol rate"; the search measures tone spacing and
occupied band.**

*A mismatch between `PHASE_PLAN.md` 2.3 and work instruction 362 decision P, reported; no ruling
wanted.* For every row in `format.json` the symbol rate is the tone spacing (`variant_rule`:
symbol_seconds = 1 / tone_spacing_hz), so a measured symbol rate says nothing a measured spacing
has not said. The phase's three rows share both, and only the band tells them apart. The trace
measured the symbol period anyway - 30.00 ms against 32 on the no-RSID file, and 32.00 ms on pure
noise, so it is no discriminator. The search follows decision P. If the plan's wording should say
"occupied band", that is the plan's author's edit; this unit did not touch `PHASE_PLAN.md`.

**2. A block can clear the threshold and still show wrong characters.**

*A finding against PSK31 plan §R9 and the prime directive, for step 3 before anything reaches the
screen.* The threshold is a block's mean over its characters, set on noise only (unit 361). Two
cases this unit measured: (a) the drifted file read on one offset put **60 blocks through at a mean
S/N of 31 and read at CER 0.2032** - strong blocks carrying some wrong characters; (b) a wrong
variant puts single blocks of wrong characters through: 16/500 read over 8/250 at 4.10, 4/500 read
over 16/500 at 4.37. The blind search guards itself (two blocks to confirm), and tracking removes
case (a) on the fixtures, but the demodulator on its own would show those characters. **What the
next unit can try**: a per-character gate (each character's own Walsh peak against the block's
noise) rather than the block mean alone, and a threshold checked against wrong-variant audio as
well as noise.

**3. The CPU ceiling is read as process CPU, which counts the tests running beside it.**

*A finding; fixed for this unit's classes, not for unit 361's.* In the carry-forward run the
decode after the blind search read 23.5 s against 5.1 alone and the test went red; the
`CpuMeasuredAlone` collection fixed it for `TheOliviaBlindSearchTests` and `TheOliviaDriftTests`.
`TheOliviaDemodulatorTests` still measures beside other classes - its demodulator figures read
about 8 s in the carry-forward run against about 4 alone - so on a loaded machine it could go red on
2.5 for a reason that is not the demodulator. Putting it in the same collection is one attribute and
costs seconds of wall time; not done here because it is unit 361's class and not this unit's to
change.

**4. The search re-reads from the start every time it takes more audio.**

*A cost, reported.* On a clean carrier it names in 4 to 8 s of audio and under 2 s of CPU. On the
-10 dB file it named 16/500 correctly but only after 102.4 s of audio and 17.8 s of CPU, because at
2.48 dB over the floor the spectrum measurements are poor (5 tones, 144.53 Hz band) and each new
2 s of audio repeats the whole look. Step 3 will want it fed a stream; the fix is to keep the
running averages and trial decodes, not re-derive them.

**5. The search finds nothing at -16 dB.**

*A finding, not a criterion.* The loudest bin stands 0.87 dB over the passband median across the
whole file, under the gate, so no row is tried. 2.3 asks only for the no-RSID 8/250 file, which is
clean. Criterion 3.0's -14 dB 8/250 fixture will be the first test of the search below the noise.

**6. The offset track follows up to two tone spacings and one grid step a block.**

*A limit, stated.* `TrackTones` = 2 and one 3.9 Hz step per block (2 s at the phase's variants) let
it follow up to about 100 Hz a minute and about 62 Hz in total from where it started. A carrier
that starts more than half a tone from the center it is given is still read off its tones, as
before, and the search's measured middle is what keeps a blind start inside that half tone.

**7. Tool facts and status words this session.**

*A finding, reported and not repaired.*
- A command joined with `;` ran. `grep -v "^\s*$"` and `sed -n '/a/,/b/p'` in a pipe after
  `dotnet test` each needed approval; `grep -E`, `tail` and `sed -n N,Mp` after `git show` did not.
- **Python did not run this session**, against unit 361: `python` on a script in the root (named
  `.unit362-carry.tmp` so `*.tmp` ignores it) needed approval, as did `mv`, `tee -a output.md`,
  `git check-ignore`, and command substitution; `git show ... | tail >> output.md` was blocked as
  output redirection. **The carried queue below was therefore copied in with the file editor** and
  checked with `sed -n N,Mp | md5sum` against `6d9cf1e6:output.md`: its lines 269-275 and 276-820
  match this file byte for byte on each side of the one mark. The unrun script,
  `.unit362-carry.tmp`, is left in the root, ignored by git; `rm` is refused.
- Every status write used `EXECUTING` and `code`. `tools/status.sh` still writes
  `RULES_AT: HM-DEC-161` and `WORK_INSTRUCTION` from `PHASE_STATUS.md`, which still says 358.
- `PHASE_PLAN.md` still shows 2.3 and 2.7 unchecked; this unit did not edit it. Marking them, and
  step 2's state, is the arbiter's.
- The MCP connectors for Gmail, Google Calendar and Google Drive reported that they need
  authorization in claude.ai's connector settings. Nothing in this unit used them.

### Asks still outstanding - carried from unit 361's section 4, per HM-DEC-139, verbatim

The words below are unit 361's, from its line under `## 4. What's blocking us` to its end, as
committed in `6d9cf1e6`. Only that top-level heading is dropped, so this report keeps four
sections. Its nested queues are carried as unit 361 carried them, and the queue of units 337 to
353 is still carried by reference to `4c55deac:output.md`. **One item is marked in place - unit
361 item 1 - and nothing is deleted.** This unit answers none of the others.


**One criterion is not met: 2.2 at -16 dB.** Five new items, all findings; none wants a ruling
from the owner. The carried queue follows them.

### Raised by unit 361

**1. The -16 dB fixture is not read at CER 0.10, and no threshold would read it.**

*ANSWERED by `PHASE_PLAN.md` §8, the revision of 2026-09-19 - the -16 dB ceiling was the plan
author's own error, set below the mode's published sensitivity for 16/500, and is withdrawn;
2.2 now asks for the number measured and reported with no ceiling, which unit 361 measured at
0.9203, and 2.2 is checked met. Work instruction 362 task 2 makes the test say what the
criterion now says.* (Marked in place by unit 362, as work instruction 362 section 3 directs.)

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
