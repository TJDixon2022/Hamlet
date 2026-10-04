# Unit 242 task 2 — what units 218, 221, 222, 223 and 227 already left in the tree

**A survey, taken by reading only.** Nothing in this document was executed; the
session that wrote it could not run `dotnet` (see `output.md` section 4). Every
line reference below was read off the file named.

**The short answer: a usable harness exists, and it is nearly everything task 3
asks for.** Task 3 extends it rather than replacing it. What it is missing is a
way to command a single rung and a trial count, and a way for the arbiter to run
it without running the whole fourteen-rung curve.

---

## The ladder, stage by stage, with file and line

| Stage | Where | Notes |
|---|---|---|
| Message generator | `tests/Ft8Sharp.Tests/Encode/EncodeCorpus.cs:71` `Build()`, entries at `:57` | Fixed order, no shuffle, 56 entries across CQ, standard, free text, telemetry and non-standard callsign kinds |
| Population filter | `tests/Ft8Sharp.Tests/Dsp/Ft8Step6Ladder.cs:160` `Population()`, `:175` `CanBeScored` | 51 of 56. The five dropped are the hashed-callsign entries, whose truth-side text is itself empty — measured at -10 dB, not assumed (`Ft8Step6Ladder.cs:144`) |
| Signal synthesiser | `tests/Ft8Sharp.Tests/Dsp/SearchFixture.cs:84` `OneSignal(...)`, `:110` `ManySignals(...)`, `:55` `Place(...)` | Writes the transmission into an otherwise empty slot at a commanded frequency and sample offset |
| Signal power | `SearchFixture.cs:174` `TransmissionPower(...)` | Measured off the samples that will actually be sent, not assumed to be 0.5 |
| Noise source | `tests/Ft8Sharp.Tests/Dsp/GaussianNoise.cs:26` | Box–Muller polar rejection, seeded, repeatable. Its whiteness is asserted by `Ft8NoiseTests`, not assumed |
| Noise delivery | `SearchFixture.cs:152` `AddNoise(clean, noise, sigma, out noisePower)` | Returns the noise power actually realised, which is what the delivered ratio is read from |
| SNR calibration | `tests/Ft8Sharp.Tests/Dsp/SignalToNoise.cs:67` `NoiseAmplitudeFor`, `:80` `DecibelsFor`, `:49` `ReferenceBandwidthHz = 2500` | The convention is written out with its arithmetic in the file header: signal power over noise power in a 2500 Hz reference bandwidth, one-sided density |
| Trial loop | `Ft8Step6Ladder.cs:286` `Walk(messages, frequencyHz, sampleOffset, log)` | Rung → seed → message. Seeds at `:110`, rungs at `:88`, six draws through the collapse and three on the anchors (`:128`) |
| Scoring | `Ft8Step6Ladder.cs:320-324` | `expected = Ft8MessageDecoder.Decode(entry.Message).Text`; returned is an ordinal match in `result.Texts`; **wrong is every returned text that is not the expected one**, kept as strings, not just counted (`:203` `WrongTexts`) |
| Rate and interval | `Ft8Step6Ladder.cs:212` `Rate`, `:214` `Interval`, `:255` `Wilson(successes, trials)` | Wilson score at 95 per cent, z = 1.959963984540054 |
| Row format | `Ft8Step6Ladder.cs:234` `AsRow()`, header at `:247` | Binned by **delivered** ratio, never by requested |
| Driver | `tests/Ft8Sharp.Tests/Dsp/Ft8Step6CurveTests.cs:186` | The whole fourteen-rung curve as one `[Fact]`. ~3519 slot decodes per pass |

### The three counts task 3 asks for already exist

`Ft8Step6Ladder.Row` carries `Trials`, `Returned` and `Wrong` separately
(`:188-192`), and `WrongTexts` (`:203`) keeps the actual returned string so a
non-zero wrong count has evidence under it. **The rule that a wrong decode is
counted separately from a missed one is already in the instrument**, which is
why the phase can adopt it unchanged.

### Determinism

`Ft8Step6Ladder.cs:307` — `new GaussianNoise(Seeds[s] + (int)Math.Round(requested * 10.0))`.
The seed depends only on the rung and the draw index, never on iteration order,
so a fresh process walking the same ladder draws the same noise. The population
is a fixed-order build with a fixed predicate and no shuffle
(`Ft8Step6Ladder.cs:155`). **A result is reproducible exactly, from the rung
alone.**

### The older diagnostic ladder, which is not this one

`tests/Ft8Sharp.Tests/Dsp/SensitivityLadder.cs` is unit 218's, driven by
`Ft8SensitivityLadderTests.cs:58`. Its own header (`:14`) says it claims none of
step 6's criteria. Ten rungs two decibels apart, two seeds, 26 messages. Two
other test classes are calibrated against its rungs, so **it must not be
retargeted** — `Ft8Step6Ladder` was written as a second, wider ladder precisely
to avoid moving it. It does carry one thing `Ft8Step6Ladder` does not:
hard-decision agreement against the true codeword (`SensitivityLadder.cs:290`
`AgreementAt`), which is where the "31 bit errors at -21 dB" figure comes from.

---

## What the delivered SNR was verified against, and whether it is still here

**Yes, it is still in the tree.** `tests/Ft8Sharp.Tests/Dsp/Unit222AxisTests.cs`,
the `[Fact]` at `:68`,
`TheDecibelAxisIsCheckedAgainstASecondReadingThatSharesNoCodeWithIt`.

- It reads the ratio a second time by periodogram of a noise-only slot,
  segment length 4096 giving 2.93 Hz bins (`:51`).
- It never calls `SignalToNoise` and never calls `SearchFixture`'s power
  helpers (`:26-29`). The only thing the two readings share is the audio.
- **The second instrument is proved before it is trusted**: the periodogram must
  satisfy Parseval or the test fails (`:123`, asserted at `:266`).
- The agreement bound, 0.2 dB, was fixed before the run (`:57`).

`OPEN_ISSUES.md:280-286` records what it found: over 20 trials at -21 dB and 10
at -10, the two readings differ by a **mean of 0.0098 dB and a largest of
0.0398**. The one-sided noise density in force predicts the measured 2500 Hz
band power to -0.047 dB; a two-sided one would be 2.963 dB out and the sampled
bandwidth taken for the reference one 3.828 dB out. **The axis is sound and the
1.5 dB belongs to the receiver.**

---

## The paired harness, which task 5 should look at before writing a new one

`tests/Ft8Sharp.Tests/Dsp/Unit227Paired.cs` already does side-by-side scoring of
two decoders over the same slots:

- `:71` `SlotOutcome`, `:84` `Side` (with its own Wilson interval at `:88`),
  `:100` `Paired(Both, OursOnly, UpstreamOnly, Neither)`.
- `:138` `WalkRung(requested, log)` — **a single rung on command**, which is the
  closest thing in the tree to what task 3 wants.
- `:247` `ThroughTheFile(...)` writes each slot once as a 12 kHz sixteen-bit
  mono WAV and reads it back, so quantisation is common to both sides.
- `:179` `TargetPeak = 0.999f` and `:209` `GainStage(...)`, with a note at `:193`
  recording that a harness defect once had both decoders return 0 of 306.

Its driver, `Unit227MeasurementTests.cs:46` and `:61`, is gated by
`[RequiresWorkingDecoderFact]` — it needs `decode_ft8.exe` built by
`tools\build-ft8-oracle.bat`. **The shape is reusable for
`Ft8Sharp` against `Ft8Sharp.Deep`; the gate is not, and the phase rules forbid
scoring against `decode_ft8.exe` as an authority anyway** because that is
`ft8_lib`, the thing being improved on.

---

## What task 3 still has to build

1. **A commanded rung and trial count.** `Ft8Step6Ladder.Rungs` and `Seeds` are
   `static readonly` arrays and `Walk` takes neither. A 50-trial smoke check is
   not expressible today; the only entry point runs all fourteen rungs.
2. **An arbiter-runnable entry point.** Everything above is `internal` to
   `Ft8Sharp.Tests` and reachable only through xunit. Either a filtered
   `dotnet test` invocation with the rung passed in the environment, or a small
   console driver.
3. **A second column for `Ft8Sharp.Deep`**, once step 1 exists. `Unit227Paired`
   shows the shape.
4. **The wall-clock time for a 306-trial rung, reported.** The cost harness
   exists — `Ft8Step6CostTests.cs:60-103` prints mean, best and worst per slot
   decode — but the ladder itself does not time a rung.

---

## Two figures in the tree that disagree, reported rather than repaired

**The -20 dB rung.** `PHASE_PLAN.md` and work instruction 242 quote **23.9 per
cent** at -20 dB, which is unit 221's float-array ladder (`OPEN_ISSUES.md:236`).
Unit 227's WAV-mediated run of the same population, seeds, frequency and offset
reads **23.2 per cent, 71 of 306** (`OPEN_ISSUES.md:425`).

**The -21 dB rung.** Units 221, 222 and 223 all read **13 of 306, 4.2 per cent**
at a delivered -21.001 dB (`OPEN_ISSUES.md:223`, `:276`, `:341`). Unit 227,
through the file, reads **14 of 306, 4.6 per cent** (`OPEN_ISSUES.md:424`).

**The difference is the sixteen-bit WAV round trip**, which unit 227 introduced
so that both decoders read identical bytes. It is one decode at -21 and two at
-20, both far inside the Wilson interval. **It matters here only because task 4
must say which of the two paths its 306 trials went down**, and the number it is
asked to reproduce — 13 of 306 — is the float-array one.

---

## The -19 dB rung agrees

81.0 per cent, 248 of 306, on both sides of unit 227's pairing
(`OPEN_ISSUES.md:426`), and 81.0 in unit 221's curve. Nothing to reconcile.
