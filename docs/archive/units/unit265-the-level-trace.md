# Unit 265 - the level trace

**What this is.** The measurement that comes before the build. Work instruction
265 asks seven questions about the level Hamlet transmits at; every answer below
carries a file, a line and a quotation, read from the tree at `HEAD 96d7d2d`
(which is `2a47955` plus this unit's bookkeeping commit - no product code has
been touched at the time of writing).

**Which machine.** Every measured figure in this document came from the
**development machine**. `SHACK_FACTS.md` FACT-004: there are two computers and
only one has a radio on it, and no radio has ever been attached to this one.
**Nothing here says anything whatever about what the IC-7300's USB modulation
input expects.**

---

## Q1 - What peak leaves the composer today?

**The synthesis.** `src/Ft8Sharp/Encode/Ft8Waveform.cs:199`:

```csharp
signal[k] = MathF.Sin(phase);
```

A sine of unit amplitude. There is no amplitude term in the expression and none
is applied afterwards - the only thing that touches the array after this loop is
the raised-cosine ramp at `:204-211`, which multiplies the first and last eighth
of a symbol by an envelope that reaches 1.0 and shapes the edges rather than the
body.

**What the composer hands back.** `Ft8Composer.Build` at
`src/Hamlet.RadioEngine/Transmit/Ft8Composer.cs:305-307`:

```csharp
var audio = wholeSlot
    ? Ft8Waveform.SynthesizeSlot(symbols, sampleRate, baseFrequencyHz)
    : Ft8Waveform.Synthesize(symbols, sampleRate, baseFrequencyHz);
```

The port's array, unaltered, straight into the `Ft8Transmission` record at
`:309-316`.

**The number, not the adjective.** `Ft8Transmission.PeakSample`
(`Ft8Composer.cs:105-121`) is measured over the array every time it is asked
for. Run tonight, filtered by exact name, from
`TheSeamTurnsWordsIntoASlotOfAudioTests.cs:100`:

```
Passed ...TheOperatorsOwnCallBecomesASlotOfAudioAtBothRates(rate: 48000) [328 ms]
 rate            : 48000
 slot samples    : 720000
 slot seconds    : 15.000000
 signal samples  : 606720
 signal seconds  : 12.640000
 peak sample     : 1.000000

Passed ...TheOperatorsOwnCallBecomesASlotOfAudioAtBothRates(rate: 12000) [3 ms]
 rate            : 12000
 peak sample     : 1.000000

Test Run Successful.  Total tests: 2  Passed: 2
```

**`PeakSample` is 1.000000 at both rates. That is 0.0 dBFS**
(`20*log10(1.0) = 0.00 dB`). Development machine, but this figure is arithmetic
on an array and is not a property of any sound card.

---

## Q2 - Is there any gain anywhere between the composer and the endpoint?

Walked, in order, naming **every** place a sample is multiplied by anything.

| Step | File and line | What happens to the samples |
|---|---|---|
| compose | `Ft8Composer.cs:309-316` | the port's array is stored in `Ft8Transmission.Samples`. **No multiplication.** |
| carry | `Ft8TransmitSequence.cs:92` `OperatorSend` | a record holding the `Ft8Transmission`. **It does not touch the array.** |
| arm | `Ft8ArmedSend` | holds one `OperatorSend`. **It does not touch the array.** |
| hand over | `Ft8TransmitSequence.cs:271` `var samples = send.Transmission.Samples;` | **the same array by reference. No copy, no scale.** |
| play | `Ft8TransmitSequence.cs:287-289` `await _sink.PlayAsync(samples, send.Transmission.SampleRate, cancellationToken)` | passed through. **No multiplication.** |
| convert | `WasapiTransmitSink.cs:606-627` | `var sample = Clamp(mono[frame], ref clipped);` then `var word = (short)Math.Round(sample * 32767.0);` |

**There is exactly one multiplication on the whole path and it is not a gain.**
`sample * 32767.0` at `WasapiTransmitSink.cs:617` is the float-to-PCM16
conversion - the unit interval mapped onto a signed 16-bit word - and the
comment at `:615-616` says why the constant is 32767 and not 32768. It changes
the representation and not the level. `Clamp` at `:632` brings a sample inside
the rails and counts it; it can only ever reduce a sample that was already
outside `-1..+1`, and nothing this repository composes is.

A grep for `*=`, `gain`, `amplitude` and `scale` across
`src/Hamlet.RadioEngine/Transmit/` and `WasapiTransmitSink.cs` returns **one
hit**, and it is the word "full-scale" inside that PCM16 comment at `:618`.

> **By any route, including editing the settings file by hand, can the operator
> change the level Hamlet transmits at today?**
>
> **No.** There is no amplitude parameter on either compose route, no gain
> anywhere between the composer and the endpoint, and no field on `AppSettings`
> that any of them reads. The only thing he could reach for is Windows' own
> volume for that endpoint, which is not Hamlet and which Hamlet neither sets
> nor reports.

---

## Q3 - What does the sink measure, and who reads it?

`src/Hamlet.RadioEngine/Audio/WasapiTransmitSink.cs:407-408`:

```csharp
PeakWritten = peak;
ClippedSamples = clipped;
```

with the properties declared at `:228` and `:238`. `PeakWritten`'s own remark at
`:224-227` says it is *"measured on the way out, after clamping, so it is what
the device was handed rather than what the caller supplied."* `ClippedSamples`'
remark at `:234-237` says clipping is *"counted, never rounded away"* and that
*"a number other than zero is a finding."*

**Who reads them.** Grep for `PeakWritten` and `ClippedSamples` across `src/`
and `tests/`, excluding the declaring file:

| File | Kind |
|---|---|
| `tests/Hamlet.App.Tests/ViewModels/TheLoopbackThroughTheApplicationsSendPathTests.cs:180,182,230` | test |
| `tests/Hamlet.RadioEngine.Tests/Audio/TheLoopbackProvesTheWholeChainTests.cs:176,329,331,376` | test |
| `tests/Hamlet.RadioEngine.Tests/Audio/TheSinkPlaysToANamedEndpointTests.cs:188,190,207` | test |

**Readers in `src/` outside the declaring file: zero.** The work instruction
measured zero and that is **confirmed, not corrected**. Every reader in the
repository is a test, and the operator is never shown either number.

This is the fourth of the four things the instruction expected to be told it got
wrong, and it lands as predicted, so **task 4 is the smaller shape** - there is
no existing route to build on and the readout has to be created.

---

## Q4 - Does `PeakWritten` describe what leaves the machine?

**It cannot be told from this repository, and this document says so plainly
rather than guessing.**

What the code does say: `PeakWritten` is assigned at `:407` from a running
maximum accumulated inside the render callback at `:608-613`, over the values
that have already been through `Clamp` and are about to be packed into the
16-bit words copied to `destination` at `:629` - that is, **the samples this
process hands to the WASAPI shared-mode render buffer.**

What the code does not say, and cannot: whether Windows applies a per-session
volume, a per-endpoint master volume, or any driver-side attenuation to those
words after this process has copied them. **Nothing in this repository sets,
reads, or reports a render-side volume.** The one volume call anywhere in
`src/` is `WasapiAudioSource.cs:78`,
`device.AudioEndpointVolume.MasterVolumeLevelScalar`, and that is on the
**capture** side, in the receive path, not the transmit sink.

**What this means for task 4's number**, and the report must carry it: the level
Hamlet can honestly show the operator is *the level Hamlet handed to the
endpoint*, not *the level that left the machine*. Anything between that buffer
and the connector - the Windows volume slider for that device, and on the real
station the radio's own USB input gain and its ALC - is outside this
repository's knowledge. FACT-004 already forbids inferring the second half; this
question shows the first half is unknowable from here too.

---

## Q5 - What would a scale break? (task 3's re-run list)

Every test in the tree that asserts a peak, a decode, an SNR or a sample value
on the transmit path. **A test not named here is one that will not be run.**

| Test type and method | Where | What it asserts about level |
|---|---|---|
| `WhatTheTransmissionLooksLikeAsAudioTests.NothingTheSeamProducesLeavesFullScale(12000, 48000)` | `:44` | **`Assert.True(worstPeak > 0.99f, ...)` at `:96`.** This is the one assertion in the tree that *requires* full scale. It will go red and its expectation is task 3 item 2's to change with a reason at the site. |
| `WhatTheTransmissionLooksLikeAsAudioTests.TheSixteenBitConversionOfARealTransmissionStaysInRange` | `:112` | PCM16 conversion never wraps |
| `WhatTheTransmissionLooksLikeAsAudioTests.TheTransmissionRampsUpAndDownRatherThanStartingOnAStep` | `:152` | edge envelope, sample values |
| `WhatTheTransmissionLooksLikeAsAudioTests.WhereTheTransmissionSitsInTheSlotIsMeasuredRatherThanAssumed(12000, 48000)` | `:200` | where the signal sits, by sample magnitude |
| `TheSeamTurnsWordsIntoASlotOfAudioTests` (whole type) | - | composes on every route; `:100` prints the peak |
| `TheSinkPlaysToANamedEndpointTests` (whole type) | - | **`Assert.Equal(0, sink.ClippedSamples)` at `:207`** |
| `TheLoopbackProvesTheWholeChainTests` (whole type) | - | engine-side loopback: composes, plays a real endpoint, captures, decodes |
| `TheLoopbackThroughTheApplicationsSendPathTests.AMessageTheApplicationSentLeavesThisMachineAndComesBack` | `:65` | app-side loopback: the decode, and `PeakWritten` in the failure message at `:230` |
| `HamletsOwnDecoderReadsBackWhatHamletComposedTests` (whole type) | - | decodes what the composer produced, over a corpus - **step 2's own proof, and it composes through `Compose`** |
| `TheRoundTripHoldsAtTheEndpointsRateTests` (whole type) | - | decodes composed audio at several rates, `:91` and `:99` |
| `OneClickSendsExactlyOneMessageTests` | `:333` | composes through `ComposeSignal` |

Also composing, and therefore worth naming even though they assert nothing about
level: `TheBandSceneIsWhatHamletsDecoderReadTests:232`,
`TheOperatorsStopFiresFromEveryStateTests:703`, `TheStopStopsTheAudioTooTests:355`,
`TheUnkeyHappensWhateverGoesWrongTests:494`,
`WhereTheTransmissionStartsAndWhatTheRecordSaysTests:65,66,113,161,162,198`,
`TheStopStopsARealEndpointTests:109`.

---

## Q6 - Where must the scale go so no send path misses it?

**Every caller of `Ft8Composer.ComposeSignal` and `Ft8Composer.Compose` in the
whole tree.**

**In `src/` - there is exactly one, and it is a `ComposeSignal`:**

```
src/Hamlet.App/ViewModels/MainWindowViewModel.cs:8184:
    var composed = Ft8Composer.ComposeSignal(wanted, _transmitSampleRate);
```

The only other two mentions in `src/` are the words `Ft8Composer.Compose` inside
a comment at `Ft8TransmitSequence.cs:511` and `:515`, explaining why the padded
slot is refused on the air. **Neither is a call.**

**In `tests/` - twenty-nine call sites**, listed in Q5 above and in the grep
behind it. None of them is a send path.

**Do the right-click send and the CQ button both arrive at `:8184`?** Yes, and
by the shortest possible route: both are the `SendMessageCommand` relay command
whose body begins at `MainWindowViewModel.cs:8169`, and `:8184` is the only
`Ft8Composer` call in that body and in the file. There is **no second compose
site** anywhere in `src/`. The instruction was right about this.

**So the scale goes in `Ft8Composer.Build`** - the private method both public
routes funnel through at `:228` and `:263`. One place, both routes, and the
application's single call site inherits it by default. `Ft8Waveform.cs:199` is
`Ft8Sharp` and may not be touched; the sink may not be used because a gain
applied there would leave `Ft8Transmission.PeakSample` claiming a peak the
array does not have.

---

## Q7 - What decode margin is there to spend?

The budget the default is chosen out of. Run tonight against the **untouched
tree**, filtered by exact name, one 12.64 s transmission into a real render
endpoint on the **development machine**:

```
Passed ...TheLoopbackThroughTheApplicationsSendPathTests
       .AMessageTheApplicationSentLeavesThisMachineAndComesBack [16 s]

 chosen because   : a display-audio endpoint - a monitor's audio path rather
                    than the machine's speakers - chosen from 4 active render
                    endpoints, and it is not the default
 endpoint         : S34J55x (3- HD Audio Driver for Display Audio)
 endpoint declares: 48000 Hz
 message          : CQ KC3QIS FN00
 composed at      : 48000 Hz
 samples composed : 606720
 rate asked       : 48000 Hz
 rate got         : 48000 Hz
 outcome          : Sent
 keyed            : True
 came out of tx   : OrdinaryUnkey
 samples played   : 606720
 play took        : 12.66 s
 peak written     : 1.0000
 rms written      : 0.7064
 clipped samples  : 0
 captured at      : 48000 Hz
 captured samples : 626400
 expected         : CQ KC3QIS FN00
 decoder returned : "CQ KC3QIS FN00"

Test Run Successful.  Total tests: 1  Passed: 1
```

**At full scale, on this machine's own endpoint, the message goes out and comes
back.** `PeakWritten` 1.0000 = **0.0 dBFS**; `RmsWritten` 0.7064 = **-3.02
dBFS**. The RMS is `1/sqrt(2)` to four figures, which is what a constant-envelope
FM-style signal gives: **FT8's crest factor is 3.01 dB and its peak and its
average move together.** That matters for the choice, because scaling the peak
down by *n* dB scales the average down by exactly *n* dB too - there is no
peaky-waveform effect to be surprised by.

**Where the margin actually is.** The path that could cost a decode is the
16-bit quantisation at `WasapiTransmitSink.cs:617`. At peak 1.0 the signal uses
the full 16-bit word. Each 6.02 dB of drive reduction costs one bit:

```
peak 1.0    ->   0.0 dBFS  ->  ~16 bits used  ->  quantisation noise ~ -98 dBFS
peak 0.5    -> -6.02 dBFS  ->  ~15 bits used  ->  quantisation noise ~ -92 dBFS
peak 0.25   -> -12.04 dBFS ->  ~14 bits used  ->  quantisation noise ~ -86 dBFS
peak 0.125  -> -18.06 dBFS ->  ~13 bits used  ->  quantisation noise ~ -80 dBFS
```

FT8 decodes down to about **-21 dB SNR in a 2.5 kHz reference bandwidth**. The
distance between a -12 dBFS signal and an -86 dBFS quantisation floor is about
**74 dB**, which is more than fifty dB of slack against the decoder's own
threshold. **On this path the drive level is not what limits the decode**, and
the budget is therefore not tight: the constraint on the default is what is
sensible to hand a radio, not what the loopback can survive.

**The instruction asked to be told if a conservative default costs decode margin
the loopback cannot afford. On the arithmetic above it does not, and task 2's
round-trip assertion and task 3's single sound run are what turn that from
arithmetic into a measurement.** If either disagrees, the measurement wins and
the level goes up.

**FACT-004, and it is the point of this whole section: every figure above was
measured on the development machine, through a monitor's display-audio endpoint.
No radio has ever been attached to this machine. None of it says anything about
what the IC-7300's USB modulation input expects, and this unit does not claim to
know that number.**

---

## What this trace decided, for task 2 to build

1. **The default is peak `0.25`, which is `20*log10(0.25) = -12.04 dBFS`** - 12 dB
   below full scale, twice the 6 dB the instruction requires as a minimum, and
   with about 74 dB of slack over the 16-bit quantisation floor by Q7's
   arithmetic. It is a **starting point the operator sets against his own
   radio's ALC**, not a figure this repository knows.
2. **It is applied in `Ft8Composer.Build`**, which both `Compose` and
   `ComposeSignal` funnel through, so the application's one call site at
   `MainWindowViewModel.cs:8184` cannot get full scale by forgetting an
   argument.
3. **`WhatTheTransmissionLooksLikeAsAudioTests.NothingTheSeamProducesLeavesFullScale`
   will go red at its `worstPeak > 0.99f` assertion**, correctly, and that
   expectation is changed with the reason written at the site.
4. **`AppSettings.cs:210-213` is stale** - see the note below.

## One thing the instruction expected to be told it got wrong, answered here

**`AppSettings.cs:210-213` is stale, and the tree says so.** The remark reads:

```
/// <para>**A SETTINGS SCREEN FOR IT IS NOT BUILT** (work instruction 259,
/// task 4, which names it as what remains). The field exists so the send path
/// has somewhere to read a name from; until something writes one, the send
/// says so.</para>
```

But `src/Hamlet.App/ViewModels/SettingsViewModel.cs:160` reads it -
`_transmitEndpoint = ChooseEndpoint(TransmitEndpoints, settings.AudioOutputDeviceId);`
- and `:302` writes it - `_settings.AudioOutputDeviceId = value?.Id;`. **The
picker unit 260's outcome entry claims exists, exists.** Unit 260's entry is
right and the remark is wrong. Per the work instruction the remark is only
corrected if task 5 puts the drive control beside that picker, in which case it
is in the way and correcting it is part of that change.
