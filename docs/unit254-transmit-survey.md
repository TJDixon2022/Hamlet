# Unit 254 - what already exists for making a transmission

**Read at 2026-09-06 against the working tree at `C:\Source\HamLet`, branch `main`.**
Reading only: nothing under `src/` or `tests/` was changed to produce this document.

This is not `docs/unit254-combining-depth.md`, which belongs to the previous phase
and was not opened, read or written by this unit.

---

## 1. Every route from a message to samples that already exists

The chain is complete inside the port. There are four stages and every one of them
is already written, tested, and public.

### 1.1 Packing - text and fields to 77 bits (10 bytes)

| Route | File and line | Takes | Notes |
|---|---|---|---|
| Standard message | `src/Ft8Sharp/Message/Ft8StandardMessage.cs:45` | `(callTo, callDe, extra, Span<byte>)` | Three-argument overload; forwards to the cache overload with a null cache |
| Standard message, with hash cache | `src/Ft8Sharp/Message/Ft8StandardMessage.cs:61` | `(callTo, callDe, extra, Ft8CallsignCache?, Span<byte>)` | This is the route a **compound callsign** takes when it will not fit the 28-bit field; it goes on the wire as a 22-bit hash |
| Non-standard callsign message | `src/Ft8Sharp/Message/Ft8NonstandardMessage.cs:102` | `(callTo, callDe, extra, Ft8CallsignCache?, Span<byte>)` | Carries one call in full in 58 bits and the companion as a **12-bit hash**. `extra` is limited to `RRR`, `RR73`, `73` or nothing (`ReportOf`, constants at `:60-:69`) |
| Free text | `src/Ft8Sharp/Message/Ft8FreeText.cs:60` | `(string, Span<byte>)` | 13 characters from a 42-character alphabet |
| Telemetry | `src/Ft8Sharp/Message/Ft8FreeText.cs:187` | `(ReadOnlySpan<byte>, Span<byte>)` | 9 bytes; **no text form exists** |

All of them return `Ft8PackResult` (`src/Ft8Sharp/Message/Ft8StandardMessage.cs:263`)
and **none of them throws for a correctly sized buffer** - the refusal names the
field. That is the property task 2's refusal requirement is built on.

Unpacking, for reference: `Ft8StandardMessage.TryUnpack` at `:158` and `:175`,
`Ft8NonstandardMessage.TryUnpack` at `:205`, `Ft8FreeText.TryUnpackText` at `:128`.

### 1.2 Symbol assembly - 77 bits to 79 channel symbols

`src/Ft8Sharp/Encode/Ft8SymbolEncoder.cs`:

- `Encode(ReadOnlySpan<byte> message, Span<byte> symbols)` at `:140`
- `Encode(ReadOnlySpan<byte> message)` returning `byte[]` at `:182`
- `SymbolCount = 79` at `:58`, `DataSymbolCount = 58` at `:61`,
  `SyncBlockLength = 7` at `:64`, `SyncBlockCount = 3` at `:67`,
  `SyncBlockOffset = 36` at `:79`, `BitsPerSymbol = 3` at `:82`,
  `ToneCount = 8` at `:88`.
- `SyncBlockStart(int)` at `:96` and `IsSyncSymbol(int)` at `:110` are the Costas
  placement, already public.

CRC and LDPC sit under this: `src/Ft8Sharp/Message/Crc14.cs`,
`src/Ft8Sharp/Message/Ft8Payload.cs`.

### 1.3 Synthesis - 79 symbols to audio

`src/Ft8Sharp/Encode/Ft8Waveform.cs`, `public static class Ft8Waveform` at `:50`.
**Every line number the work instruction gave was checked and every one is
correct.**

| Member | Line | What it is |
|---|---|---|
| `DefaultSampleRate = 12000` | `:59` | |
| `DefaultBaseFrequency = 1000.0f` | `:62` | Audio frequency of tone 0 |
| `SymbolPeriodSeconds = 0.160f` | `:66` | |
| `SlotSeconds = 15.0f` | `:69` | |
| `ToneSpacingHz = 1.0f / SymbolPeriodSeconds` | `:72` | 6.25 Hz |
| `SymbolSmoothing = 2.0f` (private) | `:82` | GFSK BT product |
| `GfskConstant = 5.336446f` (private) | `:85` | |
| `PulseSymbolSpan = 3` (private) | `:88` | |
| `RampDivisor = 8` (private) | `:91` | |
| `SamplesPerSymbol(int)` | `:94` | |
| `SampleCount(int)` | `:109` | Signal only, silence excluded |
| `PaddingSampleCount(int)` | `:125` | **Splits the remainder evenly across both ends** - `:128` |
| `SlotSampleCount(int)` | `:132` | |
| `Synthesize(symbols, rate, baseHz)` | `:142` | Signal only, floats in -1..+1 |
| `SynthesizeSlot(...)` | `:220` | Silence, signal, silence |
| `SynthesizeSlotPcm16(...)` | `:240` | |
| `ToPcm16(ReadOnlySpan<float>)` | `:265` | Upstream's clip, scale and round |

Continuous phase is at `:192-:201` - the accumulator is never reset. The raised
cosine ramp is at `:203-:211`, one eighth of a symbol at each end.

**Three refusals are already in the synthesiser and they matter for task 2:**

- `RequireSampleRate` at `:384` - refuses a rate at or below zero.
- `RequireConsistentGeometry` at `:408` - **refuses any rate at which
  `SampleCount(rate)` and `SymbolCount * SamplesPerSymbol(rate)` disagree.** This
  is the constraint that decides what rates the seam may offer.
- `RequireBaseFrequency` at `:427` - refuses a base at or below zero, and refuses
  one where tone 7 would reach Nyquist.

`RequireSymbols` at `:360` refuses anything that is not 79 symbols in the eight-tone
alphabet.

### 1.4 PCM conversion and WAV writing

- PCM16: `Ft8Waveform.ToPcm16` at `:265`, and `SynthesizeSlotPcm16` at `:240`.
- WAV **writing exists only in the test tree**:
  `tests/Ft8Sharp.Tests/Encode/WavFile.cs`, with its own tests in
  `tests/Ft8Sharp.Tests/Encode/WavFileTests.cs`. There is a reader in the product
  at `src/Hamlet.RadioEngine/Audio/WavAudio.cs`, used by the receive/diagnosis
  path.

### 1.5 Everything in `tests/` that already synthesises FT8 audio

Found by searching for `Ft8Waveform` across `tests/`. Twenty-two files, grouped:

**The port's own encode tests** - `tests/Ft8Sharp.Tests/Encode/`:
`Ft8WaveformTests.cs`, `Ft8WaveformComparisonTests.cs`,
`Ft8WaveformSecondOpinion.cs`, `Ft8WaveformSecondOpinionTests.cs`,
`UpstreamSynthesisInventoryTests.cs`.

**The port's DSP tests** - `tests/Ft8Sharp.Tests/Dsp/`: `SearchFixture.cs`
(`:89-:90` and `:203-:204` are the two synthesis sites), `Ft8LadderHarness.cs`,
`Ft8SensitivityLadderTests.cs`, `Ft8ImpairedLadderTests.cs`,
`Ft8LadderCalibrationTests.cs`, `Ft8SlotDecoderGateTests.cs`,
`Ft8CollapseAnatomyTests.cs`, `Ft8MonitorTests.cs`, `Ft8NoiseTests.cs`,
`Ft8Step6CostTests.cs`, `Ft8Step6CurveTests.cs`, `Ft8Step6LossCensusTests.cs`,
`Ft8ToneRecoveryTests.cs`, `ToneRecovery.cs`, `Ft8Unit253MaskingSurveyTests.cs`,
`Ft8WaterfallGeometryTests.cs`, `Unit222AxisTests.cs`, `Unit222TraceTests.cs`,
`Unit227Paired.cs`, `tests/Ft8Sharp.Tests/Fixtures/Ft8ExampleFixture.cs`.

**`Ft8Sharp.Deep`'s tests**: `Ft8DeepBasebandTests.cs`, `Ft8DeepFineSyncTests.cs`,
`Ft8DeepFineSyncGateTests.cs`, `Ft8DeepOsdCostTests.cs`,
`Ft8DeepRepeatDecoderTests.cs`, `Ft8DeepSlotDecoderTests.cs`,
`Ft8DeepSubtractionTests.cs`, `Ft8Unit254AccumulationTests.cs`.

**Hamlet's own tests already synthesise FT8** - and this is the important one:

- `tests/Hamlet.RadioEngine.Tests/Audio/TheDigitalTabDecodesWhatItKeptTests.cs`.
  At `:182` it calls `Ft8StandardMessage.TryPack("CQ", "K1ABC", "FN42", message)`
  and at `:184-:185`
  `Ft8Waveform.SynthesizeSlot(Ft8SymbolEncoder.Encode(message), rate, PlacedAtHz)`.
  Its `AMessageInTheKeptAudioComesBackAsItself(int rate)` at `:69` is a
  **text-in / text-out round trip through the receive path already living in
  `Hamlet.RadioEngine.Tests`.** It is one message, parameterised over rates,
  aimed at the resampler rather than at transmission.
- Also: `ACapturedFileDiagnosesItselfTests.cs`, `TheSlotSaysHowLoudTheAudioWasTests.cs`,
  `TheSlotWatchTests.cs`.
- `tests/Hamlet.App.Tests/` has five more. **Not run by this unit** - the
  instruction forbids it.

**What this establishes for task 2 and task 3:** `Hamlet.RadioEngine.Tests`
already references `Hamlet.RadioEngine` (`tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj:21`)
which references `Ft8Sharp` (`src/Hamlet.RadioEngine/Hamlet.RadioEngine.csproj:33`)
and `Ft8Sharp.Deep` (`:43`). The whole chain is reachable from where task 3's test
must live, with no project change.

---

## 2. Which of step 2's five exit criteria the tree already satisfies

Criteria taken verbatim from `PHASE_PLAN.md:202-214`.

### Criterion 1 - message to 79 symbols to audio, Costas in place, 6.25 Hz spacing, 0.16 s a symbol, 12.64 s total, continuous phase. *must-pass*

**MET by the tree, before this unit started.** Evidence, all in
`tests/Ft8Sharp.Tests/Encode/Ft8WaveformTests.cs`:

- `TheSampleCountIsWhatTheTimingImpliesAtTwoDifferentRates` at `:46` - the length.
- `PhaseIsContinuousAcrossEverySymbolBoundary` at `:162` - **measures the phase
  step at every symbol boundary directly**, which is the one thing a length check
  and a tone check both miss.
- `EverySymbolOfEveryMessageIsRecoveredBackOutOfTheWaveform` at `:241` - the tones
  come back out of the audio, over the corpus. **Note what this actually proves:
  it recovers symbols by measurement, not by decoding.** It is not a decode.
- Costas placement is `Ft8SymbolEncoder.IsSyncSymbol` at `:110` /
  `SyncBlockStart` at `:96`, and `Ft8SlotDecoderGateTests.cs:325-:340` builds a
  transmission from them independently and compares against
  `Ft8SymbolEncoder.Encode` at `:366`.
- Spacing is `ToneSpacingHz` at `Ft8Waveform.cs:72`, a computed reciprocal rather
  than a literal.

**What is not met in its letter:** *"at the radio's sample rate"*. The radio's
sample rate is not established anywhere in this tree - see section 6 and
`SHACK_FACTS.md`. 12000 is the decoder's rate, not the radio's.

### Criterion 2 - `Ft8Sharp` decodes it back to the message that went in, over at least a hundred messages including compound callsigns, grids, reports and `RR73`. *must-pass*

**NOT MET. This is the gap, and it is the unit's goal task.**

There is no test anywhere in this tree that takes **at least 100 messages** from
text, synthesises them, and asserts the same text comes back out of
`Ft8SlotDecoder`. What exists is close but is not this:

- `tests/Hamlet.RadioEngine.Tests/Audio/TheDigitalTabDecodesWhatItKeptTests.cs:69`
  - **one** message, text in and text out, across sample rates.
- `tests/Ft8Sharp.Tests/Encode/Ft8WaveformTests.cs:241` - the whole 56-entry
  corpus, but recovering **symbols**, not decoding messages.
- `tests/Ft8Sharp.Tests/Dsp/Ft8SlotDecoderGateTests.cs` - synthesises the corpus
  and decodes it, but its four tests all assert **nothing comes back**
  (`ATransmissionCarryingAWrongChecksumReturnsNothing` at `:135`,
  `ATransmissionFarBelowTheDecodableLevelNeverReturnsTheWrongText` at `:224`).
  They are refusal tests. They prove the decoder does not lie; they do not prove
  it reads what we send.
- The ladders (section 3) decode synthesised transmissions in quantity, but they
  score a **population of 51** at a stated SNR and they live in
  `Ft8Sharp.Tests`, not on the Hamlet side, and they are not a text-identity
  assertion per message.

### Criterion 3 - byte-identical to `ft8_lib`'s encoder where one is available to compare against; reuse it rather than writing a second encoder. *must-pass*

**MET by the tree, by reuse, and the arbiter has already ruled that reuse
satisfies it** (`WORK_INSTRUCTIONS.md`, ARBITER-DECISION `DECIDED`).

- `tests/Ft8Sharp.Tests/Encode/Ft8WaveformComparisonTests.cs:69`,
  `EverySampleAgreesWithTheWavUpstreamWritesForTheSameMessage`. **What it
  actually proves:** every sample of our synthesis against every sample of the
  WAV `ft8_lib`'s own generator writes for the same message, over the 51 corpus
  entries that have a text form, with a bound of **2 counts in sixteen bits**
  (`:62`) - and the bound was written after the measurement, not before. The
  alignment is *read* from `PaddingSampleCount`, never cross-correlated for.
  Not bit-identical, deliberately: C single precision and .NET single precision
  round differently at the last place.
- **The condition on it, which must be stated:** it carries
  `[RequiresWorkingOracleFact]`, defined at
  `tests/Ft8Sharp.Tests/Encode/Ft8Oracle.cs:574`, which sets `Skip` when
  `Ft8Oracle.ProbeUsability()` says upstream's generator is not usable. **On a
  machine without the clone and built binary it skips.** It ran on the machine
  where it was written; it is not evidence obtainable on demand here.
- `TheComparisonIsWatchedRefusingEachOfItsFourNamedAlterations` at `:230` is the
  watched-to-fail leg of it.
- `tests/Ft8Sharp.Tests/Encode/Ft8WaveformSecondOpinionTests.cs:64`,
  `AnIndependentSecondSynthesisAgreesWithTheLibrarysOverTheWholeCorpus`. **What
  it actually proves, as against what the name suggests:** it is explicitly
  *"the weaker of two agreeing legs"* (`:14`). It is a second synthesis written a
  different way - phase totalled in double, never wrapped - and its bound is
  **128 counts** (`:61`), two orders of magnitude looser than the upstream
  comparison. Its real finding is stated at `:44-:59`: the divergence is
  *accumulated phase drift* and it is what the port's agreement with upstream
  would have looked like had the port "improved" on upstream by computing in
  double. It runs on a machine with no clone, which is why it is kept, and it is
  the only implementation that takes the smoothing parameter, so it is what makes
  `APortThatRestartsPhaseAtEachSymbolIsCaughtByTheContinuityMeasurement` at `:169`
  possible.

**So the honest reading:** criterion 3 is met on the machine that has the oracle,
by the strong leg, and is backed here by the weak leg plus the symbol-recovery and
phase-continuity tests. **This unit adds nothing to it and changes nothing in it.**

### Criterion 4 - level and clipping stated, with what the radio's input expects. *must-pass*

**PARTLY MET, and the missing half is not obtainable in this repository.**

- Level and clipping in the **port**: `Ft8WaveformTests.cs:84`,
  `EverySampleIsInRangeAndTheSixteenBitConversionNeverWraps`, and the reasoning
  written out at `Ft8Waveform.cs:249-:263` - the clip is applied before the
  scale, the scale is 32767, and the round is a half added then truncated toward
  zero, which is asymmetric and is upstream's.
- **What the radio's input expects is nowhere in this tree.** `SHACK_FACTS.md`
  rules that the IC-7300's USB codec is not present on this machine. Task 4 will
  state what is known, source it, and say what waits for step 3.
- **Nothing has ever stated the peak the seam produces**, because there is no seam.
  Task 4 pins it.

### Criterion 5 - timing: the audio starts on the slot boundary and runs 12.64 s, measured. *must-pass*

**PARTLY MET, and there is a finding here that task 4 must carry.**

- The 12.64 s is covered by `Ft8WaveformTests.cs:46`.
- **"Starts on the slot boundary" is contradicted by the port's own code.**
  `PaddingSampleCount` at `Ft8Waveform.cs:125-:129` computes
  `(SlotSeconds * rate - SampleCount(rate)) / 2` and `SynthesizeSlot` at `:220`
  copies the signal in at that offset - so the signal is **centred in the slot**,
  with the spare time split evenly across both ends. The docstring at `:119-:123`
  says so in words. That is upstream `gen_ft8.c`'s slot layout and it is what the
  sample comparison aligns on.
  **An FT8 transmission on the air starts shortly after the slot boundary, not in
  the middle of the slot.** This is a step 3 finding, measured in task 4, and
  **the port is not to be changed for it.**

---

## 3. Has anything ever decoded a synthesised transmission back through `Ft8SlotDecoder`?

**Yes, extensively - but never as a text-identity round trip at scale, and never
from the Hamlet side.**

The oracle is `src/Ft8Sharp/Dsp/Ft8SlotDecoder.cs:133`,
`Decode(ReadOnlySpan<float>)`. It builds an `Ft8Monitor` over the samples and is
documented as taking a slot at `Ft8WaterfallGeometry.SampleRate`. Geometry
constants confirmed: slot 15.0 s at `Ft8WaterfallGeometry.cs:52`, rate 12000 at
`:55`, search window **200 Hz** at `:58` to **3000 Hz** at `:61`.

**One decode-scope fact that matters for compound callsigns:** the callsign cache
is created **per slot**, inside `Decode`, at `Ft8SlotDecoder.cs:149`, and is
dropped when the call returns. The comment there is explicit: a callsign heard in
full at candidate 3 can be resolved from its hash at candidate 40, and *nothing
carries over to the next slot*. So a message whose callsign travels as a hash can
only read back if the full call appeared **in the same slot**. This is exactly the
risk the work instruction warned about in advance, and it is now confirmed from the
code rather than assumed.

Where synthesised audio has been decoded:

| Where | What it does | Result recorded |
|---|---|---|
| `tests/Ft8Sharp.Tests/Dsp/Ft8SensitivityLadderTests.cs:58` `TheWholePathIsWalkedDownTheLadderUntilItStopsAnswering` | Synthesise, add calibrated Gaussian noise, decode down a ladder of SNRs | The phase's headline numbers |
| `tests/Ft8Sharp.Tests/Dsp/Ft8LadderHarness.cs:222` and `:494` | The harness both decoders run through; population is **51 messages**, 306 trials is six whole blocks | Cross-slot combining reads **252 of 306 at -21 dB**, zero wrong (carried in `WORK_INSTRUCTIONS.md`'s framing) |
| `tests/Ft8Sharp.Tests/Dsp/Ft8ImpairedLadderTests.cs` | The same under impairment | |
| `tests/Ft8Sharp.Tests/Dsp/Ft8SlotDecoderGateTests.cs:135`, `:224` | Synthesise a deliberately broken or far-too-weak transmission and decode | **Asserts nothing comes back.** Refusal evidence, not identity evidence |
| `tests/Hamlet.RadioEngine.Tests/Audio/TheDigitalTabDecodesWhatItKeptTests.cs:69` | One message, text in through pack/encode/synthesise/resample, text out | The only Hamlet-side text round trip that exists |

**At what SNR:** the ladders run from clean down past -21 dB. The one clean,
noiseless, text-identity round trip is the single-message one in
`Hamlet.RadioEngine.Tests`.

**So the answer to the question as posed:** yes, many times, in noise, scored as a
rate; and **no**, never as *"these hundred messages came back as themselves"*.

---

## 4. What `EncodeCorpus.cs` contains

`tests/Ft8Sharp.Tests/Encode/EncodeCorpus.cs`, `internal static class EncodeCorpus`
at `:49`, `Build()` at `:71`. Counted from the source:

| Kind | Count | Lines |
|---|---|---|
| Standard (`Ft8StandardMessage.TryPack`, no cache) | **37** | `:173-:222` |
| Free text (`Ft8FreeText.TryPackText`) | **8** | `:226-:233` |
| Telemetry (`Ft8FreeText.TryPackTelemetry`) | **4** | `:245-:248` |
| Non-standard (`Ft8NonstandardMessage.TryPack`) | **3** | `:251-:253` |
| Standard with a hashed callsign | **4** | `:257-:260` |
| **Total** | **56** | |

Of the 56, **51 have a text form** (`Text` non-null): the 37 standard, the 8 free
text, the 4 `StandardHashed`, and the 2 non-standard entries whose companion is
spelled out. The 4 telemetry entries and the 1 hashed-companion non-standard entry
have `Text: null` **deliberately** - `:109-:112` and `:157-:165` explain that no
string makes upstream produce those messages, so giving them a text would compare
two different messages and call the difference a defect. **That 51 is the same 51
the population figure and the "51 of 51 fidelity tests" both refer to.**

The coverage is genuinely broad: `CQ` with and without a grid, lettered and
numbered CQs, `QRZ`, signal reports across `-30`..`+30` and their `R` forms,
`RRR`, `RR73`, `73`, empty extras, grid corners `AA00` / `RR99` / `JJ55`, callsign
shapes across seven countries, `/R` and `/P` suffixes, and both hash routes.

**Is it reusable for a round trip?** **Not directly.** Three reasons:

1. It is `internal` to `Ft8Sharp.Tests`. Task 3's test lives in
   `Hamlet.RadioEngine.Tests`, a different assembly, and no `InternalsVisibleTo`
   exists between them. **Copying it into the Hamlet test tree would be a second
   copy that drifts** - contrary to CLAUDE.md §0.
2. It is a corpus of **packed bytes**, built for comparing bits and tones. Task 3
   needs **text in, text out**, which is a different shape: it must go through the
   seam by the words the operator would type.
3. It carries entries with **no text form at all** (telemetry, the hashed
   companion). Those cannot be round-tripped from text by construction.

**What it is good for, and what task 3 will take from it:** its *choice of
messages* is the record of what a band actually carries, and its comments record
two expensive findings - that `"K1ABC RR73 X"` is a standard message and not free
text (`:235-:241`), and that our non-standard hashed form and upstream's standard
hashed form are different wire formats for the same words (`:26-:41`). Task 3's
corpus will be built independently, in the Hamlet test tree, from the categories
the plan names, and will say so.

---

## 5. What Hamlet itself has, as opposed to the port

**Searched `src/` for every use of `Ft8StandardMessage`, `Ft8NonstandardMessage`,
`Ft8SymbolEncoder` and `Ft8FreeText`.** Outside `src/Ft8Sharp/` and
`src/Ft8Sharp.Deep/` there are exactly **two** hits in the whole product:

1. **`src/Hamlet.RadioEngine/Audio/Ft8Reception.cs:631`** - inside
   `private static double?[] Measure(...)` at `:629`. It allocates
   `Ft8SymbolEncoder.SymbolCount` symbols and calls
   `Ft8DeepMessageSymbols.TryEncode` to get the symbol sequence back out of a
   message that **was just received**, so the SNR estimator has something to
   correlate against. The docstring at `:620-:626` says so. **This is the receive
   path re-encoding what it heard. It produces no audio and it is not a
   transmission.**
2. **`src/Hamlet.App/ViewModels/Ft8Vocabulary.cs:42`** - a doc comment naming
   `Ft8StandardMessage.TryUnpack`. `Split` at `:48` splits a decoded message
   string into three fields with string operations. **It reads; it never packs.**

**Therefore: nothing in `Hamlet.RadioEngine` composes a message for transmission,
and nothing in Hamlet produces transmit audio today.** There is no `Transmit`
folder under `src/Hamlet.RadioEngine/` at all - the directories are `Audio`,
`Bands`, `Civ`, `Cw`, `Explore`, `Licensing`, `Rig`, `Scan`, `Solar`, `Telemetry`,
`Training`, `Transport`.

The transmit-adjacent things that do exist, all out of this unit's scope:

- `src/Hamlet.RadioEngine/Civ/TransmitAbort.cs` - unit 253's, proven,
  **with no caller anywhere**, and nothing here will call it.
- `src/Hamlet.RadioEngine/Licensing/TransmitGuard.cs` - the licence gate. Nothing
  in this unit passes through it; the bypass question is banked to step 3.
- `src/Hamlet.RadioEngine/Audio/Ft8Resample.cs:32`, `TargetSampleRate = 12_000` -
  the **receive** path resamples down to the decoder's rate.

---

## 6. What is missing, explicitly

Said as a list rather than left as an absence, because the instruction asks for it.

1. **A seam.** There is no single call anywhere from *what the operator wants to
   say* to *the samples for one slot*. Every existing route assembles the four
   stages by hand at the call site, and each of the twenty-odd test files that
   does so is a separate copy of the sequence. **Task 2 builds it.**
2. **A refusal surface.** `Ft8PackResult` exists and is rich, but nothing turns it
   into an answer a caller can act on without knowing which of the five packing
   routes to have tried. Choosing the route - standard, non-standard, free text -
   is currently the caller's problem and is undone work.
3. **A hundred-message text-identity round trip through `Ft8SlotDecoder`.**
   Criterion 2. **Task 3 builds it.**
4. **A measured peak level for anything Hamlet produces.** The port's clipping is
   pinned; the amplitude the seam hands out is not, because there is no seam.
   **Task 4 pins it.**
5. **Any statement of what the IC-7300's USB input expects.** Not in this tree and
   not measurable on this machine - `SHACK_FACTS.md` rules the codec absent.
   **Task 4 states what is known and names what waits for step 3.**
6. **48000 Hz.** Nothing in the tree has ever synthesised at 48000. From
   `RequireConsistentGeometry` at `Ft8Waveform.cs:408` the rate is admissible in
   principle - a symbol is a whole number of samples - but that must be
   **measured, not asserted**, and task 2 does so.
7. **Where the transmission belongs in the slot.** The port centres it. Real FT8
   does not. **Task 4 measures and reports; step 3 decides.**
8. **Anything that plays audio, keys, or sequences.** Steps 3 and 5, deliberately
   absent, and nothing in this unit adds any of it.

---

## 7. Mismatches between the work instruction and the tree

**None found.** Every file, symbol, line number and constant the instruction cited
was checked against the tree and every one was correct, including all sixteen
members of `Ft8Waveform`, the two `Ft8StandardMessage.TryPack` overloads, the
`Ft8NonstandardMessage.TryPack` at `:102`, `Ft8SlotDecoder.Decode` at `:133`, the
`Ft8WaterfallGeometry` constants, `Ft8Resample.TargetSampleRate`, both project
references, and the root version `1.12.81` at `Directory.Build.props:205`.

The one thing worth naming is not a mismatch but a refinement the instruction left
open: **`PaddingSampleCount` does split the remainder evenly** - `Ft8Waveform.cs:128`
- so the answer to the instruction's own question is that the port centres the
transmission in the slot. Whether that is where it belongs is task 4's to report.
