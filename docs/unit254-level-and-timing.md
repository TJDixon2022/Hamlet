# Unit 254 - level, clipping and timing, measured

**Every number below was measured on 2026-09-06 from the arrays
`Hamlet.RadioEngine.Transmit.Ft8Composer` produces**, on the **development
machine**, by
`tests/Hamlet.RadioEngine.Tests/Transmit/WhatTheTransmissionLooksLikeAsAudioTests.cs`.
Six tests, all green, filtered by name and foregrounded.

**Which machine the evidence came from is part of the evidence** (`SHACK_FACTS.md`
FACT-004). Nothing here was measured at a radio, because there is no radio on this
machine and there never has been.

---

## 1. Level and clipping

### The peak

Measured over eight messages at each rate, walking every sample of the slot.

| Rate | Largest peak | Min | Max | Headroom below full scale | Samples outside -1..+1 |
|---|---|---|---|---|---|
| 12000 | **1.000000** | -1.000000 | +1.000000 | **0.000000** | **0** |
| 48000 | **1.000000** | -1.000000 | +1.000000 | **0.000000** | **0** |

The peak was 1.000000 for every one of the eight messages at both rates -
`CQ KC3QIS FN00`, `CQ K1ABC FN42`, `K1ABC W9XYZ -11`, `K1ABC W9XYZ RR73`,
`K1ABC W9XYZ 73`, `CQ DX G4ABC IO91`, `TNX BOB 73 GL`, `CQ PJ4/K1ABC`.

**There is no headroom and that is by construction, not by accident.** The
synthesis is `MathF.Sin(phase)` (`src/Ft8Sharp/Encode/Ft8Waveform.cs:199`) - a sine
of unit amplitude. It reaches full scale and it cannot exceed it, because a sine
cannot. **Nothing in the seam scales it**, so a caller that wants headroom applies
its own gain, and that is a step 3 decision about the codec rather than something
this seam should have guessed at.

### Clipping

**Nothing can exceed full scale, and the sixteen-bit conversion was measured on a
real transmission rather than on contrived input.**

`Ft8Waveform.ToPcm16` on the slot for `CQ KC3QIS FN00` at 12000 Hz:

| | |
|---|---|
| Samples | 180000 |
| Minimum | **-32766** |
| Maximum | **+32767** |
| Scale | 32767, not 32768 |

**The asymmetry is upstream's and is correct.** The conversion is
`(short)(int)(0.5 + (x * 32767.0))` at `Ft8Waveform.cs:280`: a half added, then
truncated toward zero. At +1.0 that is `0.5 + 32767 = 32767.5` truncating to
**32767**; at -1.0 it is `0.5 - 32767 = -32766.5` truncating to **-32766**. So the
bottom of the range is one count short of the top. That is not a defect and it is
not to be "fixed" - the port's own remarks at `Ft8Waveform.cs:249-263` record that
getting this rounding wrong costs one count on roughly half the samples, which is
exactly the size a tolerance would swallow, and the whole byte-identity against
upstream's WAV rests on it.

**Nothing wrapped.** A wrap turns the positive peak into a large negative one; the
two extremes are of opposite sign and comparable size.

### The ends - the ramps

Measured at 12000 Hz on `CQ KC3QIS FN00`.

| | |
|---|---|
| Ramp length | **240 samples = 20.00 ms**, an eighth of a symbol |
| First sample of the signal | **0.000000000** |
| Last sample of the signal | **-0.000000000** |
| Peak within the ramp up | 0.983855 |
| Peak within the ramp down | 0.999957 |
| Peak in an equal window at mid-signal | 0.999995 |

**The transmission does not begin or end on a step.** A raised-cosine envelope
runs over the first and last eighth of a symbol (`Ft8Waveform.cs:203-211`), and it
is exactly zero at its first sample - which is why the first and last samples of
the signal are zero, and why the silence at each end of the slot is one sample
longer than `PaddingSampleCount` says (see section 3).

The two ramp peaks differ because the ramps do not begin at the same phase of the
carrier, not because the envelopes differ; both windows are 240 samples of a
signal whose body peaks at 0.999995 over the same span.

---

## 2. What the radio's input expects

**Sourced, with the unknowns named as unknowns.** `SHACK_FACTS.md` FACT-004 rules
that the IC-7300's USB codec is not present on this machine and that **no
measurement of this machine's audio endpoints says anything about the radio**. So
nothing in this section is measured; it is cited or it is marked unknown.

### What is known, and where it came from

| Fact | Source |
|---|---|
| One USB cable carries two functions: a virtual COM port for CI-V, and **a USB audio codec for RX and TX audio**. No external interface hardware is needed or used. | `CLAUDE.md:602-604`, from IC-7300 Full Manual, publication `A7292-4EX-6` |
| `1A 05 0059` selects the ACC/USB **output**; `1A 05 0060` sets the ACC/USB AF **output** level | `CLAUDE.md:650`, manual p. 19-4 |
| `1A 05 0061` is the ACC/USB squelch gate | `CLAUDE.md:651`, manual p. 19-5 |
| **The mode matters as much as the level.** Command `06` sets a mode with no way to say whether the data variant is wanted, so a radio told "USB" by `06` lands in **voice USB with the microphone live** rather than in **USB-D routing the computer's audio**. Command `26` is the one that carries the data flag. | `CLAUDE.md:714-718`, HM-DEC-056 |
| Hamlet's receive path works at 12000 Hz and resamples to it | `src/Hamlet.RadioEngine/Audio/Ft8Resample.cs:32` |

**Note what the three cited `1A 05` sub-commands are all about: the ACC/USB
*output*.** They are the receive side. **This repository cites no command and no
manual page for the USB *modulation input* level** - the setting that decides how
loud the computer's audio arrives at the transmitter. That is a genuine gap in
`CLAUDE.md` §4 rather than a thing this unit knows and did not write down.

### What is unknown and must wait for step 3

Stated plainly, as FACT-004 requires, rather than filled with a plausible number:

1. **What sample rate the IC-7300's USB codec accepts on its input.** Unknown from
   this side. The seam produces 12000 and 48000 and the port would produce any of
   the twelve rates measured in section 4; which one the codec wants is a question
   about the shack machine.
2. **What format, bit depth and channel count it declares.** Unknown, and
   explicitly not inferable - `BENCH_CHECK.md` records that the two capture devices
   on *this* machine declare `IeeeFloat 32-bit`, and says in the same paragraph
   that this says nothing about the radio.
3. **What Windows calls the device.** Explicitly unverified in `CLAUDE.md:738-742`;
   HM-OPEN-003. `LooksLikeRadioCodec` matches `"USB Audio CODEC"` to *preselect*
   and never to claim.
4. **What level the radio's USB modulation input expects, and where its ALC sits.**
   No cited figure exists in this repository, as above. **This is the one that
   decides whether a full-scale sine is right or is 20 dB too hot**, and it cannot
   be answered here.
5. **Whether the radio is in USB-D.** A transmission into a rig in voice USB goes
   nowhere useful, per HM-DEC-056. Step 3's problem.

**Consequence for step 3, stated and not acted on:** the seam hands out a
full-scale sine because that is what the synthesis is. **Step 3 owns the gain**,
and it will need a measurement at the radio - not a guess made here - before it
plays anything.

---

## 3. Timing, and where the transmission sits in the slot

**Measured from the arrays**, by finding the first and last non-zero sample rather
than by asking the port, and then held against what the port says.

| | 12000 Hz | 48000 Hz |
|---|---|---|
| Samples a symbol | 1920 | 7680 |
| Signal, samples | 151680 | 606720 |
| **Signal, seconds** | **12.640000** | **12.640000** |
| Slot, samples | 180000 | 720000 |
| **Slot, seconds** | **15.000000** | **15.000000** |
| First non-zero sample at | 14161 | 56641 |
| Last non-zero sample at | 165838 | 663358 |
| Leading silence, samples | 14161 | 56641 |
| **Leading silence, seconds** | **1.180083** | **1.180021** |
| Trailing silence, samples | 14161 | 56641 |
| **Trailing silence, seconds** | **1.180083** | **1.180021** |
| The port's `PaddingSampleCount` | 14160 | 56640 |

**The 12.64 s holds exactly, at both rates.** That is criterion 5's first half and
it is met.

**The silence at each end is one sample longer than the padding** - 14161 against
14160, 56641 against 56640 - because the raised-cosine envelope is exactly zero at
the first sample of each ramp. Measured, then written down; not the other way
round.

### The finding, for step 3

**`PHASE_PLAN.md:213` says *the audio starts on the slot boundary and runs
12.64 s*. The second half is true. The first half is not what the port does.**

`Ft8Waveform.PaddingSampleCount` at `:125-129` computes
`(SlotSeconds * rate - SampleCount(rate)) / 2` and `SynthesizeSlot` at `:220`
copies the signal in at that offset. **The spare 2.36 s is split evenly across
both ends, so the transmission is centred in the slot with about 1.18 s of silence
before it and 1.18 s after.** The port's own docstring at `:118-123` says so, and
it is upstream `gen_ft8.c`'s file layout - it is what the sample-for-sample
comparison against upstream's WAV aligns on.

**Is that where an FT8 transmission belongs? No.** On the air a transmission
begins shortly after the slot boundary - a fraction of a second, not 1.18 s - and
runs 12.64 s of a 15 s slot, leaving the remainder at the end for decoding and
turnaround. A transmission centred in the slot would be heard by every other
station as **about a second late**, which is well outside the time window a
receiver expects and is exactly the kind of error that would look like a clock
problem.

**What step 3 will have to do about it - and this is a finding, not a task:**

- **Do not change the port.** `Ft8Sharp` is a faithful MIT port and this layout is
  what its byte-identity test rests on.
- **Step 3 should synthesise the *signal*, not the slot** - `Ft8Waveform.Synthesize`
  at `:142` returns the 12.64 s of signal with no padding at all - and decide for
  itself when to start playing it, from the clock, relative to the slot boundary.
  The placement then becomes a scheduling decision at the audio device, which is
  where it belongs, rather than an offset baked into an array.
- `Ft8Composer.Compose` returns the padded slot today because that is the shape
  `Ft8SlotDecoder` reads and this unit's job was the round trip through it. **A
  signal-only entry point is step 3's to add if it wants one**, and adding it now
  would be building for a caller that does not exist.

**Reported only. Nothing in this unit compensates for it and nothing in
`src/Ft8Sharp/` was touched.**

---

## 4. Which sample rates the port will accept

Measured by sweeping `Ft8Composer.RateIsUsable`, which asks the port's own two
length functions rather than deciding anything itself.

| Rate | Samples a symbol | From duration | 79 x symbol | Slot | Usable |
|---|---|---|---|---|---|
| 8000 | 1280 | 101120 | 101120 | 120000 | yes |
| 11025 | 1764 | 139356 | 139356 | 165374 | yes |
| 12000 | 1920 | 151680 | 151680 | 180000 | yes |
| 16000 | 2560 | 202240 | 202240 | 240000 | yes |
| 22050 | 3528 | 278712 | 278712 | 330750 | yes |
| 24000 | 3840 | 303360 | 303360 | 360000 | yes |
| 32000 | 5120 | 404480 | 404480 | 480000 | yes |
| 44100 | 7056 | 557424 | 557424 | 661500 | yes |
| 48000 | 7680 | 606720 | 606720 | 720000 | yes |
| 88200 | 14112 | 1114848 | 1114848 | 1323000 | yes |
| 96000 | 15360 | 1213440 | 1213440 | 1440000 | yes |
| 192000 | 30720 | 2426880 | 2426880 | 2880000 | yes |

**All twelve.** This unit's first guess was that 11025 and 44100 would be refused
for not being round numbers, and the guess was wrong: 0.16 s of either is a whole
number of samples. **What the port refuses is a rate at which it is not** -
`RequireConsistentGeometry` at `Ft8Waveform.cs:408`, because the slot is laid out
from `SampleCount` and the waveform written from `SymbolCount * SamplesPerSymbol`,
and a rate where those disagree puts every sample after the signal starts at the
wrong offset. **12345 and 44101 are two that are refused**, and they are in the
test because they were measured rather than assumed.

The seam turns that refusal into `Ft8ComposeRefusal.SampleRateRefused` with the
reason in words, rather than letting the port's exception out.

---

## 5. What was dropped

The work instruction named a drop candidate: **the 48000 Hz render, the WAV
artefact on disk, and the slot-placement finding, dropped together** if the unit
ran long.

**The unit did not run long, and two of the three were delivered:**

- **The 48000 Hz render: delivered.** Measured throughout above and asserted in
  two tests.
- **The slot-placement finding: delivered.** Section 3.
- **The WAV artefact on disk: NOT delivered, and this is the only thing this unit
  dropped.** The reason is not time. A committed WAV is a ~350 KB binary copy of
  something every test in this file regenerates deterministically in a few
  milliseconds, and a second copy of a truth is what CLAUDE.md §0 says to generate
  rather than store. `tests/Ft8Sharp.Tests/Encode/Ft8WaveformComparisonTests.cs`
  already writes its WAVs to `Path.GetTempPath` and deletes them for the same
  reason. If step 3 wants a file to hand to another program, `WavFile.cs` exists in
  the test tree and the seam's array is what it would be written from.
