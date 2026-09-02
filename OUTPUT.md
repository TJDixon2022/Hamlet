READ IN THIS ORDER — A, then B, then C.

A. PHASE — Hamlet hears FT8 off the radio and displays the decoded text on screen. Seven steps.
Step 1 (the library exists and its tables are proven) and step 2 (messages round-trip through 77
bits) are CLOSED. Step 3 (a valid FT8 signal can be produced) is CLOSED on its four must-pass
criteria, met by unit 212; its NICE-TO-PASS criterion 3 is NOT MET and is now recorded in
OPEN_ISSUES.md by name as HM-OPEN-065, which is the debt task 7 paid — that file held nothing about
FT8 at all, four units after the ruling of 2026-09-01 required it. Step 4 (signals are found in
noise) is THIS UNIT'S and this is its FIRST unit. It ENTERED at 0 of its three subject criteria and
it LEAVES at 0 of its three subject criteria: none was met, none was aimed at, and nothing tonight
searched for anything. What it entered without and leaves with is the thing all three of them
require — there was no FFT, no spectrum and no waterfall anywhere in this tree this morning, and
there is one now, proved against the defining sum. Steps 5 (a found signal becomes a message), 6
(sensitivity meets the published threshold) and 7 (Hamlet displays decoded FT8) are NOT STARTED.
Step 4 was the only step this phase could move, because every step depends on the one before it by
the plan's own named deviation and steps 1 to 3 are done.
B. STEP 4 — signals are found in noise. FIVE exit criteria, ALL FIVE MUST-PASS. (1) A synthesized
signal at a known offset and time is found — NOT MET, and NOT AIMED AT. Tonight built the
representation the finding would happen in: an independent FFT, a real-input path, and the waterfall
spectrogram, with 4424 of 4424 tones recovered from a signal whose frequency and time were HANDED IN
rather than found. NOTHING SEARCHED. There is no Costas correlation, no candidate, no score.
(2) Twenty simultaneous synthesized signals across the passband are found — NOT MET, NOT AIMED AT.
Nothing tonight analysed more than one signal at a time. (3) Candidate ranking is stable across runs
— NOT MET, NOT AIMED AT; there are no candidates and nothing ranks. The foundation it will rest on
was measured: the transform gives bit-identical output on 3840 of 3840 bins on a reused plan and on
a fresh one, and the whole waterfall is byte-identical at 167028 of 167028 on a fresh monitor and on
one reused after a reset. THAT IS NOT THE CRITERION and must not be read as it. (4) Ft8Sharp tests
green — ENTRY 222 total, 221 passed, 0 failed, 1 skipped in 3 s; EXIT 348 total, 347 passed, 0
failed, 1 skipped in 11 s, re-run after both version bumps. 126 tests added. The one skip at entry
and at exit is Ft8TableGenerationTests.RewriteTheCheckedInTablesFile, the table write gate, which is
meant to skip; no new skip was created, because every clone-reading test found the clone.
(5) Attribution clean from 2828ab6 and the channel tests green — 133 paths, NOT ONE under
src/Hamlet.App/, src/Hamlet.RadioEngine/, tests/Hamlet.App.Tests/ or tests/Hamlet.RadioEngine.Tests/;
AudioSeamTests and PrivilegeTests green at 55, DecisionLogOrderTests, VersionTests,
DecisionEmissionTests and VoiceTests green at 13, both re-run after the bumps.
C. THIS REPORT — THE NUMBER: 4424 of 4424 symbols across 56 messages, 100.000 per cent, worst margin
13.5 dB; and across six base frequencies including one EXACTLY HALFWAY between two bins, 2844 of
2844 with a worst margin of 4.5 dB, which is the margin the next unit's correlator actually has.
Over NOISE ALONE the same measurement returns 12.405 per cent against a chance rate of 12.500 for
eight candidates — 0.11 standard deviations from chance — so the first number is a measurement and
not a question with an obvious answer. TONIGHT'S EVIDENCE IS MATHEMATICS, CONSTRUCTION AND
PROVENANCE, and it is explicitly NOT agreement with upstream's own output, because nothing upstream
emits a spectrum and decode_ft8.exe is not on this machine. WHAT IT DOES NOT SHOW: that this
spectrum will find a signal it was not told where to look for. It was told, every time, in frequency
and in time, by construction. Task 6 was NOT DROPPED although the FIRST branch of its condition
licensed it — task 5 ran and its recovery was measured on a clean signal — and it was built anyway
because step 4's own name and the whole of step 6 need it and the generator was already half-built
for task 5's noise-alone check. Section 4 raises 2 items, NEITHER is a ruling request and NEITHER is
in the way of a criterion named in B.

UNIT:       213 — complete at task 7 of 7 — 2026-09-01 21:37
DATE:       2026-09-01
STATE:      COMPLETE
TASKS:      7 of 7
DROPPED:    none — task 6 was the named candidate and was kept; see section 3
PHASE GOAL: Hamlet listens to the radio, finds FT8 transmissions in the audio, and puts the words
            they carry on the screen.
UNIT GOAL:  Build the frequency-domain front end — an FFT of this library's own and the waterfall
            spectrogram — and prove it by recovering the tones of a signal this library synthesized
            at a frequency and a time it chose.
ADVANCED:   no — no subject criterion of step 4 was met and none was aimed at. What advanced is the
            substrate all three of them sit on: there was no frequency-domain representation of
            audio anywhere in this tree and there is one now, proved against the defining sum.
NUMBER:     step 4 subject criteria met: 0 -> 0 of 3. Tone recovery, which is not a criterion:
            none -> 4424 of 4424 symbols over 56 messages
DRIFT:      1 consecutive unit without advance  (was 0 — unit 212 closed step 3's third deliverable)

# 1. What was asked, and what happened

Seven tasks were asked. **All seven ran. Nothing was dropped and nothing was left unreachable.**
Task 6 was the named drop candidate and the first branch of its condition licensed dropping it; it
was built anyway, for the reason given in section 3.

Machine: `C:\Source\HamLet` on Windows 11, branch `main`, project confirmed as Hamlet against all
four preflight checks — `SHACK_FACTS.md` and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs`
both present, `CoreHMI.sln` and `MURC.sln` both absent, and `Hamlet.sln` the only solution at the
root.

**The unit's target was task 5 and it came out whole.** Nothing is deferred.

## What was traced, built and measured

**Task 1 — the ground, re-measured rather than inherited.** `Ft8Sharp` 222 total, 221 passed, 0
failed, 1 skipped in 3 s. The library builds at 0 warnings and 0 errors; `Ft8Sharp.csproj` still
reads `net8.0`, nullable enabled, warnings as errors, **no `PackageReference` and no
`ProjectReference`**. Attribution 118 paths from `2828ab6` with 0 under `src/Hamlet.` or
`tests/Hamlet.`. Channels 55 and 13. `HEAD` `2842dc3`, 8 `.obj` at the root, versions 1.12.19 and
0.6.0, sixteen divergences on record in `porting-notes.md` — the instruction's number, checked
against the file and correct. Step 3's evidence re-run in one line: `Ft8WaveformTests` and
`Ft8WaveformComparisonTests`, 12 green, **0 skipped**, so the clone is present and the one-count WAV
agreement still agrees. Not re-run as a project of its own.

**Task 2 — upstream's receive front end, found rather than assumed.** The files were discovered, not
guessed at: the monitor is `common/monitor.{c,h}`, the waterfall structure and its element type are
in `ft8/decode.h`, and the passband and the two oversampling factors are chosen by
`demo/decode_ft8.c` rather than declared by the library. All of it read through the test process, by
a checked-in test that skips when the clone is absent. **The arbiter's expectation that the transform
size is a power of two is wrong** — see section 3.

**Task 3 — an FFT of this library's own.** `src/Ft8Sharp/Dsp/` is the first receive-side code this
tree has held. `Ft8Fft` is a mixed-radix Cooley–Tukey decimation in time, written from the
decomposition; `Ft8RealFft` is the one-sided real path the monitor actually uses. **Nothing in the
pin's vendored FFT folder was read beyond its licence header**, and that restriction is enforced by
the code rather than promised — the source-dump route refuses the folder by name.

**Task 4 — the waterfall, faithful to what task 2 read**, and the port found the same lesson unit 212
found, on the other side of the radio and larger.

**Task 5 — the target.** Section 3 leads with it.

**Task 6 — noise with an SNR that is defined**, its definition written out with the arithmetic shown,
its delivery measured, and a degradation curve reported as a measurement rather than a target.

**Task 7 — the plan's unpaid debt, the record, both versions and this report.**

## Decisions this session made for itself

**1. The FFT is mixed-radix rather than radix-2, because the length the monitor wants is not a power
of two.** The instruction directs a radix-2 Cooley–Tukey and lists "a length that is not a power of
two" among the refusals to build. Task 2 measured that upstream transforms **3840** points, which is
2^8 × 3 × 5. A radix-2 transform cannot compute it, and a refusal of non-powers of two would refuse
the library's own working size. The general Cooley–Tukey decomposition — of which radix-2 is the
special case, and for a power-of-two length every stage **is** a radix-2 butterfly — is the same
textbook mathematics, so the decision does not touch the licensing reasoning at all. The refusals
built instead are named in section 3.

**2. Two entries were added to the top of `OPEN_ISSUES.md` rather than the bottom.** The instruction
says *append*. That file runs newest-first and all sixty-three entries before these were added at the
top. The reading taken is *add without disturbing anything already there*, which both placements
satisfy, and the file's own convention decided between them. **Not one character of existing content
was touched.**

**3. `PHASE_STATUS.md`'s `WORK_INSTRUCTION:` line was edited.** The work instruction's known item 6
says not to hand-edit that file; the session prompt explicitly assigns that line to me and instructs
me to set it. The prompt is the later and more specific instruction and it resolves the ownership of
exactly that line, so it was followed. **Only that line was changed**, and the file was committed
once, at task 1, exactly as unit 212 committed it.

# 2. What the owner should expect

**`Ft8Sharp` can now turn audio into a spectrum, and that is all it can do with audio.** It cannot
find a signal. It cannot decode one. Given a frequency and a time it is told, it can say which of
eight tones is there, and it is right every time on a clean signal it made itself.

**What will look wrong but is not:**

- **`ADVANCED: no` and `DRIFT: 1`.** No subject criterion of step 4 moved, because none was aimed at.
  The instruction is explicit that a report from this unit reading as though step 4 advanced would be
  worse than one saying the truth. The truth is that the substrate was built and the criteria were
  not touched.
- **The library's version went 0.6.0 → 0.7.0 while `ADVANCED` reads no.** Those measure different
  things. The library gained a capability; the step gained none of its criteria.
- **The tone spacing this library reports is 6.2500001397 Hz, not 6.25 Hz**, and the frequency of a
  bin computed two ways differs by 6.7 × 10⁻⁵ Hz. That is upstream's single-precision symbol period
  showing through, it is one part in 93 110, and it is printed rather than rounded away on purpose.
- **The recovery under noise falls to 37 per cent at −25 dB.** That is **not** step 6's sensitivity
  figure and must not be compared with the published −21 dB. Step 6 measures a *decode* rate through
  demodulation, LDPC and CRC; this is a per-symbol tone recovery with no search and no error
  correction, and error correction is what stands between the two.
- **The `Analyse` guard on sample rates almost never fires.** The symbol period is 4/25 of a second,
  so every rate that is a multiple of 25 passes — which is every audio rate in ordinary use. That is
  precisely why upstream never met the inconsistency it refuses.

# 3. What you should see

## THE TASK 5 MEASUREMENT, FIRST AND IN ONE BLOCK

```
THE CORPUS SWEEP, at 1000.0 Hz and offset 0 samples
  RECOVERED           4424 of 4424 symbols across 56 messages   =  100.000 %
  worst margin        13.5 dB

BASE FREQUENCIES — six, three of them NOT on a bin centre (bins are 3.125 Hz apart)
  1000.0000   474/474   13.5 dB   on a bin centre (320 x 3.125)
   800.0000   474/474   13.5 dB   on a bin centre (256 x 3.125)
  1500.0000   474/474   13.5 dB   on a bin centre (480 x 3.125)
  1001.5625   474/474    4.5 dB   EXACTLY HALFWAY BETWEEN TWO BINS
  1234.0000   474/474   11.0 dB   off centre by 0.75 Hz
  2000.7812   474/474    8.0 dB   a quarter of a bin off centre
  TOTAL       2844 of 2844        WORST MARGIN OVER THE SWEEP  4.5 dB

TIME OFFSETS — five, two NOT a whole number of blocks, one not a whole number of sub-blocks
      0  (0 blocks + 0)      474/474   13.5 dB   residual   0 samples
   1920  (1 block  + 0)      474/474   13.5 dB   residual   0 samples
    960  (0 blocks + 960)    474/474   13.5 dB   residual   0 samples   NOT a whole block
  14160  (7 blocks + 720)    474/474   11.0 dB   residual 240 samples   OFF the sub-block grid
   4805  (2 blocks + 965)    474/474   13.5 dB   residual   5 samples
  TOTAL       2370 of 2370        WORST MARGIN OVER THE SWEEP  11.0 dB

THE MARGIN AS A DISTRIBUTION, over 948 symbols of 12 messages
  worst 13.5   1st pct 14.0   5th pct 14.5   median 16.0   95th pct 101.5   best 113.5
  mean 22.18 dB
  sync symbols (252): worst 14.0 dB, mean 15.87 dB    <- what the next unit's correlator uses
  data symbols (696): worst 13.5 dB, mean 24.46 dB
  every margin is a multiple of 0.5 dB, because the store quantises there

NOISE ALONE — no signal in the slot at all, same frequency, same offset
  symbols asked        1580
  'recovered'           196
  RATE               12.405 %
  CHANCE             12.500 %   (8 candidate tones, so one in eight)
  distance from chance  0.11 standard deviations
  against            100.000 % on the clean signal

DISPLACEMENT IN FREQUENCY — measured, not assumed
  base 1000.00 Hz -> peak at bin 131 sub 0 = 1018.7500 Hz
  base 1500.00 Hz -> peak at bin 211 sub 0 = 1518.7500 Hz
  shift MEASURED 160 cells of 3.125 Hz     shift PREDICTED 160
  and 1018.75 Hz is where the first Costas tone (tone 3) was put, exactly

DISPLACEMENT IN TIME — measured, not assumed
  offset      0 -> peaks at block 33 sub 1 = 5.360 s
  offset   9600 -> peaks at block 38 sub 1 = 6.160 s
  shift MEASURED 5.00 blocks               shift PREDICTED 5.00
```

**Read it this way.** The clean recovery is total and the margin at its worst is 4.5 dB, which is at
the frequency exactly halfway between two bins — the case a single well-chosen frequency would have
hidden, which is why it is in the sweep. **The 4.5 dB is the number the next unit's correlator
actually has to work with**, not the 13.5 dB the headline sweep gives. Noise alone comes back at
chance, so the recovery is discriminating rather than tautological. Both displacements land exactly
where the arithmetic says.

**And what it is not.** **This is not a search.** The base frequency was chosen and handed to the
synthesizer. The slot offset was chosen and used to place the signal. The symbol index is a loop
variable. The block and time sub-offset are *computed* from the geometry, not found. Those words are
in the test file, in `porting-notes.md` and here. **No step 4 subject criterion is met by any of it.**

## Task 3 — the FFT error, measured before the bound was asserted

```
sweep                30 lengths, 1 to 4096, including 1920 and 3840
reference            a naive DFT in the test project computing the defining sum term by term,
                     calling nothing in the library
WORST RELATIVE ERROR 4.575354e-15   at length 4096
absolute there       6.252776e-13
BOUND ASSERTED       1.000000e-13   -- after the measurement, never before
headroom             21.9x
```

**Why the gap is what it is.** Double precision carries about 2.2 × 10⁻¹⁶ per operation. The *naive*
side accumulates N terms into one running sum while the transform accumulates about log₂(N) levels,
so most of what is measured is the reference's own error and it grows with N. The bound is one round
order above the worst measurement. It is not a tolerance that would absorb a real defect: a
transposed index or a sign error moves a bin by order one, not by order 10⁻¹³.

Other measurements, each printed before its own bound: a four-point transform matches the one worked
out **by hand** to 2.2 × 10⁻¹⁶; a chirp at 3840 agrees to 7.3 × 10⁻¹⁵; the real path against the
defining sum at seven lengths and against the complex path over 1921 bins at 1.4 × 10⁻¹⁵; linearity
3.5 × 10⁻¹⁶; Parseval with both energies printed and agreeing to better than 10⁻¹⁴; **an impulse
transforming to a spectrum flat to exactly zero**; DC leaking 9.2 × 10⁻¹⁷ of itself outside bin zero.

**A measurement that surprised, chased down rather than absorbed.** The bin-centre sinusoid test was
written with a bound of 10⁻¹⁴ **before** the measurement, which is the mistake this project has a
rule against, and it measured 1.888 × 10⁻¹⁴ at bin 320 and 1.029 × 10⁻¹³ at bin 1000. Widening the
bound would have been wrong, and the numbers said where to look: the two leakages differed by a
factor of five for two bins whose ratio is about three, and the *larger* bin leaked more. That is how
an **input** behaves, not a transform. At bin 1000 the last angle handed to `Math.Cos` is about 6283
radians, and argument reduction there costs ≈ 1.4 × 10⁻¹² in the sample, which over 3840 samples is a
spectral error of order 10⁻¹⁰ — exactly what was seen. **The leakage was the test's own sinusoid.**
Reducing the angle in exact integers first drops the ratio to **1.587 × 10⁻¹⁶ and 1.699 × 10⁻¹⁶**,
two to three orders, and the bound is now 10⁻¹⁵, set from the new number.

## Task 2 — the shapes, and the strong/weak anchoring split

**The transform.** Block = samples in one symbol at the configured rate. Advance = block ÷ time
oversampling. Transform length = block × frequency oversampling. **Real-input**, output buffer
length/2 + 1 bins. At 12 kHz: block 1920, advance 960, **transform 3840, and 3840 is not a power of
two — it is 2^8 × 3 × 5**, giving 1921 bins.

**The window.** Hann, **written as the square of a sine**, over the whole transform. Hamming,
Blackman and a shorter hand-picked window are all present and all **commented out**. Normalisation is
2 ÷ transform length and **it is folded into the window coefficients, not applied to the output** —
which is the answer to *is there a scale factor on the transform output*, and it is **no**.

**The oversampling.** 2 in time, 2 in frequency. Time offsets come from **shifting the input frame**
by a sub-block; frequency offsets come from **the transform being longer than a symbol**, read out by
striding the bins. **Not zero padding, and not a second finer transform.**

**The storage.** `uint8_t`, one byte. 10·log₁₀(1 × 10⁻¹² + |X|²) — decibels of **power**, floor
**inside** the logarithm. Byte = clamp(0, 255, (int)(2·dB + 240)); half a decibel per count; −120 dB
to +7.5 dB; **clamped, not wrapped**. **Normalised nowhere** — not per block, not per slot; a running
maximum is tracked and never divides anything. Axis order **[block][timeSub][freqSub][bin]**, bin
fastest, stride = timeOsr × freqOsr × bins.

**The anchoring split: 6 strong, 15 weak, 21 shapes read, 0 unread.** Strong (a macro or typedef in a
header): the symbol period, the slot duration, the waterfall element type, the byte-to-decibel macro,
the waterfall structure, and the axis order. Weak (an expression inside a function body, or a value
the application chose): the block size, the advance, the transform length, the normalisation factor,
the window and its length, the block count, the first and last kept bins, the decibel conversion, the
byte scaling, the sub-offset stride, and — **the four weakest of all** — the passband edges and the
two oversampling factors, which are `demo/decode_ft8.c`'s choices and not the library's at all.

**Three things named as unread rather than guessed.** Nothing upstream emits a spectrum, so no number
here was checked against upstream's output. **The exact alignment between a block index and a sample
offset was not settled by reading** — the frame is prefilled with zeros and slides, and the port
reproduces the same prefill and shift so it inherits whatever alignment upstream has, but it is not
asserted as a number. And the float-to-byte truncation order was reproduced rather than verified
against a running monitor. **No constant in the port was guessed.**

## The FFT licence finding

```
folder      fft/  (5 files: kiss_fft.{c,h}, kiss_fftr.{c,h}, _kiss_fft_guts.h)
project     KISS FFT — https://github.com/mborgerding/kissfft
copyright   Copyright (c) 2003-2010, Mark Borgerding. All rights reserved.
licence     SPDX-License-Identifier: BSD-3-Clause
```

**A second copyright holder under a second licence**, against a library carrying one `LICENSE`
(Tim's MIT) and a `NOTICE` crediting Goba. Only the leading comment block was read — the test stops
at the first preprocessor directive — and **no structure, no algorithm and no line of that folder was
consulted**. The decision to write the transform stands on this measurement rather than on an
assumption.

## Task 4 — the waterfall geometry, and the single precision that moves whole integers

```
                      in float (upstream, and this port)   in double ("more accurate")
  block size          12000 * 0.160f -> 1920.0f -> 1920      1919.99995708 -> 1919
  first kept bin        200 * 0.160f ->   32.0f ->   32        31.99999928 ->   31
  last kept bin        3000 * 0.160f ->  480.0f ->  481       479.99998927 ->  480
```

`0.160f` is 0.1599999964237213, and every extent is a truncated product of it. **A block one sample
short misaligns every symbol after the first; a first bin one lower shifts every frequency this
library reports by 6.25 Hz, one whole FT8 tone.** So the more accurate arithmetic is the wrong
arithmetic. This is unit 212's lesson arriving on the receive side, and it is larger here — that unit
found single precision worth 116 counts in the last place, and this one finds it worth whole
integers. Both columns are computed and printed by a checked-in test rather than asserted in a
comment.

Extents at 12 kHz: block 1920, subblock 960, transform 3840, **93 blocks**, bins 32 to 481, **449
bins**, stride **1796**, **167028 magnitudes** in a slot, transform bins **3.125 Hz** apart. A whole
15-second slot analyses in **55 ms**. A slot of 180000 samples gives 93 whole blocks and 1440 samples
left over, and the block capacity computed from the slot duration agrees with the count computed from
the sample count — two routes through the same floats. Silence reads as byte 0 everywhere and −120 dB
exactly, no not-a-number. Every one of the 898 bin centres maps to a frequency and back to itself,
and 100 Hz and 5000 Hz are reported as outside the passband rather than clamped silently into it.

**No constant was guessed**, so there is no guessed constant to flag.

## Every refusal that was watched refusing

| Refusal | Watched at | How far outside |
|---|---|---|
| transform length 0, −1, −3840 | `Ft8Fft` | refused; message names the rule |
| each of 4 transform spans one short | `Ft8Fft` | refused, and **every output value still held its sentinel** |
| real length 0, 1, −2, 3, 1921, 3839 | `Ft8RealFft` | refused; odd and below-two named separately |
| each of 3 real spans wrong | `Ft8RealFft` | refused, sentinel intact, `ParamName` correct |
| sample rates 4410, 11111, 12001, 9999 | geometry | refused — not a whole symbol |
| rates 6000…48000, incl. 8000, 11025, 44100 | geometry | **accepted**, so the guard is not always on |
| time oversampling 7 at a 960-sample block | geometry | refused — 1 sample per block would never be looked at; 5 at the same rate accepted |
| 8 degenerate configurations | geometry | refused (0/negative factors and rates, inverted, empty and negative passbands) |
| 8 out-of-range waterfall reads | `Ft8Waterfall` | refused on every axis, both ends |
| blocks of 0, 1, 1919, 1921, 3840 samples | `Ft8Monitor` | refused, **and the next good block matched a monitor that never saw the refusal** — the sliding frame really was untouched |
| signals of 0, 1, 959, 1919 samples | `Ft8Monitor` | refused; exactly 1920 accepted |
| 5 blocks past the end of the waterfall | `Ft8Monitor` | reported false, nothing stored, matching upstream's early return |

**The one refusal the instruction named that was not built** is "a length that is not a power of
two", because 3840 is the length this library needs. It is replaced by the length and buffer guards
above, and the substitution is a decision reported in section 1.

## Task 6 — not dropped, and which branch licensed dropping it

**The FIRST branch licensed it**: task 5 ran and its recovery was measured on a clean signal, so the
unit's evidence was complete and the fixture is next-unit provisioning. **It was built anyway** —
step 4's own name is *signals are found in noise*, step 6 needs an SNR that is defined rather than
approximate, the generator was already half-built for task 5's noise-alone check, and there was time.

```
DEFINITION, arithmetic shown rather than asserted
  SNR(dB) = 10 log10( signal power / noise power in a 2500 Hz reference bandwidth )
  2500 Hz is the amateur weak-signal convention the published FT8 figures use
  at 12 kHz, real samples occupy 6000 Hz one-sided, so noise in the reference
  is sigma^2 * 2500/6000, and sigma = sqrt( P * 6000 / (2500 * 10^(snr/10)) )
  signal power MEASURED from the samples at 0.499008, not assumed to be 0.5

REQUESTED IS DELIVERED — 8 points, +20 to -30 dB, 3 600 000 samples each
  WORST ERROR 0.0061 dB          tolerance 0.01 dB, set after the measurement

THE GENERATOR, measured before it is trusted — 400 000 samples
  mean 0.000263   deviation 1.001911   KURTOSIS 2.9966  (3 normal, 1.8 uniform)
  68.26 / 95.45 / 99.73 per cent inside 1, 2, 3 sigma
  10000 of 10000 bit-identical on replay from the seed; 0 of 10000 from another

WHITE, measured with this library's own transform, 1919 bins x 200 blocks
  TILT ACROSS THE BAND  -0.0079 dB

THE DEGRADATION — A MEASUREMENT, NOT A TARGET. NOTHING TUNED.
  clean 100.00 %   +20 100.00   +10 100.00   +5 100.00    0 100.00
   -5   100.00 %   -10 100.00   -15  99.21   -20  74.37  -25  37.34
  chance is 12.50 %.  Flat, a knee at -15 dB, then a fall toward chance.
```

**This is not step 6's figure.** Step 6 measures a decode rate through demodulation, LDPC and CRC
against a published threshold near −21 dB. This is a per-symbol tone recovery at a frequency and a
time it was told, with no search and no error correction.

## The divergences added

**Two, numbered 17 and 18 on from the sixteen already on record** — the count was checked against
`porting-notes.md` and the instruction's sixteen is correct.

**17 — the transform computes in double where upstream's computes in single.** There is no
bit-identity to lose: this is a different algorithm from the one upstream vendors, so agreement in the
last place was never available, and the waterfall quantises to half a decibel. **Note what it does
not extend to** — the geometry is single precision deliberately, and so is the value at the point it
becomes a stored byte; only the transform's internal arithmetic is widened. A stored byte could
differ from upstream's by one count where the decibel value sits within ≈ 10⁻⁶ of the truncation
boundary, and nothing tonight could measure that because nothing upstream emits a byte.

**18 — a sample rate the geometry does not divide is refused.** Two shapes: a rate at which a symbol
is not a whole number of samples, and — the dangerous one — a block that does not divide by the time
oversampling factor, where the analysis consumes fewer samples than the caller advances by and the
remainder is **audio silently never looked at**. Upstream truncates and inherits both, because at
12 kHz there is no remainder in either.

## The versions

`src/Ft8Sharp/Directory.Build.props` **0.6.0 → 0.7.0** under HM-DEC-152, with the note saying what
the minor claims — the library can turn audio into a spectrum — and what it does not: nothing
searches, nothing scores, nothing ranks, nothing decodes, and **it can now see and it still cannot
hear.** Root `Directory.Build.props` **1.12.19 → 1.12.20** under HM-DEC-150. **Both re-run after the
bumps:** `Ft8Sharp` 348/347/0/1 in 11 s, channels 55 and 13 with `VersionTests` among them.

**No new shared artifact was added, so no new channel was added.** `Ft8Sharp` still has no
`PackageReference` and no `ProjectReference`; the boundary test is green.

## What was committed, and what was left alone

Seven commits, one per task. **Every file committed is under `src/Ft8Sharp/`,
`tests/Ft8Sharp.Tests/`, `OPEN_ISSUES.md`, the two `Directory.Build.props` files, or the two status
files that are mine.** Nothing else.

**Left alone, deliberately:** the **8 `.obj`** at the repository root, counted at the end and
unchanged; `tools\build-ft8-oracle.bat`, present, untracked, not run and not edited; everything under
`tools\`, including the two modified arbiter files that are the owner's; `PHASE_OUTCOME.md`, stale
and the loop's; the four modified `ANALYSIS-*`/`PROJECT_CARD`/`WORK_INSTRUCTIONS` files;
`tests/Ft8Sharp.Tests/TempEncoderProbe.cs`, on disk, emptied to a comment and still tracked, and an
eleventh attempt to be rid of it was not made; `src/Ft8Sharp/Tables/Ft8Tables.g.cs`, not opened;
`CLAUDE.md` §1. **Nothing from the clone was committed** — not a source file, not a line, not a
licence text, not a value. **No WAV, no binary, no `.obj`.**

## Mismatches against the instruction — reported, not repaired

**1. The transform size is not a power of two, and the instruction assumes it is.** Task 3 says
*sweep every power-of-two size the monitor could want* and lists *a length that is not a power of
two* among the refusals. The monitor's length is **3840 = 2^8 × 3 × 5**. Both statements are wrong
about the tree. The consequence is the decision in section 1; the refusal is not implementable and
was replaced.

**2. `git status --short` printed 27 lines at entry, not the 26 the arbiter measured.** It reads 30
now, which is those 27 plus this session's own uncommitted-then-committed files at the moment of
measuring. Not investigated further.

**3. Known item 6 and the session prompt disagree about `PHASE_STATUS.md`.** Reported as a decision
in section 1 rather than silently resolved.

**Everything else the instruction asserted checked out**: `HEAD` `2842dc3`; 118 attribution paths
with the Hamlet filter returning nothing; 1.12.19 and 0.6.0; 8 `.obj`; `src/Ft8Sharp/` holding
exactly the listed folders and files with `Encode/` holding exactly two; **`tests/Ft8Sharp.Tests/Encode/`
holding exactly twenty-two files**; unit 212's exit figures of 222/221/0/1 and channels 55 and 13;
sixteen divergences on record; `OPEN_ISSUES.md` holding nothing about FT8 — its only two mentions of
the string are a note from 2026-08-18 about how to integrate a decoder, not a criterion.

## Defects of my own, found against myself and corrected

Eight, all in tests and none in the library:

1. A weak-anchoring tally written as 16 where the list holds 15.
2. A regex that could not cross the `(float)` cast in the pin's Hann window.
3. A bin-centre leakage bound written before its measurement — the largest of the eight, and the one
   the instruction's rule exists to catch. Chased to its cause rather than widened.
4. and 5. Two geometry assertions written as though `0.160f` were 0.160.
6. `92 × 0.16` written as 14.88 when it is 14.72.
7. `180000 mod 1920` written as 750 when it is 1440.
8. Four sample rates chosen as "bad" that are all multiples of 25 and therefore perfectly fine —
   which is itself the reason upstream never met that guard, and is now recorded in the test.

## The validator

**It did not run, in any of the five spellings `tools\arbiter\run-unit-tools.txt` lists.** Reported
as a failure to invoke rather than routed around, and it reproduces unit 212's measured diagnosis
exactly — known item 15:

| Spelling | What happened |
|---|---|
| `cmd //c tools\arbiter\validate-output.bat` | `'toolsarbitervalidate-output.bat' is not recognized` — **the backslashes are lost** |
| `cmd.exe //c tools\arbiter\validate-output.bat` | same, backslashes lost |
| `tools\arbiter\validate-output.bat` | `toolsarbitervalidate-output.bat: command not found` — backslashes lost |
| `cmd /c tools\arbiter\validate-output.bat` | prints the Windows banner and stops — **an interactive `cmd`**, the batch never runs |
| `cmd.exe /c tools\arbiter\validate-output.bat` | same, interactive `cmd` |

**The ordering block was therefore checked by hand**, against the four rules the validator enforces:
`A.`, `B.` and `C.` each begin a line with **no indentation**, at lines **3, 17 and 36**; all three
are inside the first 60 lines; all three are above the `UNIT:` line, which is at line **51**; and the
count in C is written as a digit — *"Section 4 raises 2 items"*.

# 4. What needs a decision, or is carried forward

**No ruling is requested. Section 4 raises 2 items and both are carried forward for the next unit,
not questions for the owner.** The reference decoder is not re-raised: it is a standing item with the
owner from units 210, 211 and 212 and it is now recorded in `OPEN_ISSUES.md` as HM-OPEN-065, which is
where the plan says it belongs.

**1. The block-to-sample alignment is inherited, not asserted, and the next unit will need it
exactly.** Upstream's analysis frame is prefilled with zeros and slides by a sub-block, so the
samples behind a block reach back before it; upstream's own resynth comment calls this a
three-sub-block loading offset. This port reproduces the same prefill and the same shift, so it
inherits whatever alignment upstream has — but task 2 could not settle what that alignment *is* as a
number, and nothing tonight asserts one. Tonight's recovery does not depend on it, because the
alignment used is computed from this library's own geometry and is self-consistent. **A correlator
comparing its candidate times against upstream's would depend on it**, and the cheapest way to settle
it is a fixture whose time offset is known and whose recovered block is measured against upstream's
`freq_hz`/`time_sec` expressions rather than against this library's.

**2. The margin the correlator actually has is 4.5 dB, not 13.5 dB.** The headline sweep's 13.5 dB is
at a base frequency sitting exactly on a bin centre. At a frequency exactly halfway between two bins
the worst margin falls to 4.5 dB, and off-centre frequencies are the ordinary case on the air. **The
sync symbols — the ones a Costas correlator sums — have a worst margin of 14.0 dB and a mean of
15.87 dB on a clean on-centre signal**, and that is the budget the next unit is working inside before
any noise is added. Carried forward so the correlator's thresholds are set against a measured number
rather than a hopeful one.
