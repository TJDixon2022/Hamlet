# Unit 262, task 1 - where the two rates meet, and what happens there

Every answer below carries a file and a line number, or a measured figure. Read
at `HEAD a81d214`, with the working tree as this unit found it.

**Nothing here opens a serial port and nothing keys.** Question 5 enumerates this
machine's own render endpoints and reads the mix format each declares; no client
is initialised and no sound is made. `SHACK_FACTS.md` FACT-004 stands: **no radio
has ever been attached to this machine**, so nothing measured here is the
IC-7300's USB codec and nothing about that codec may be inferred from it.

---

## 1. What rate does the application compose at today, and where is that decided?

**12000 Hz, and it is decided by a defaulted parameter nobody passes.**

The value passes through these lines, in order, from the click to
`Ft8Transmission.SampleRate`:

| # | File and line | What the line does |
|---|---|---|
| 1 | `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:8110` | `private void SendMessage(string? text)` - the `[RelayCommand]` the click lands on |
| 2 | `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:8120` | `var composed = Ft8Composer.ComposeSignal(wanted);` - **one argument. No rate is passed and none is available to pass.** |
| 3 | `src/Hamlet.RadioEngine/Transmit/Ft8Composer.cs:259-263` | `ComposeSignal(string? text, int sampleRate = DefaultSampleRate, float baseFrequencyHz = DefaultBaseFrequencyHz)` - the omitted argument is filled in here |
| 4 | `src/Hamlet.RadioEngine/Transmit/Ft8Composer.cs:201` | `public const int DefaultSampleRate = Ft8Waveform.DefaultSampleRate;` |
| 5 | `src/Ft8Sharp/Encode/Ft8Waveform.cs:59` | `public const int DefaultSampleRate = 12000;` - **the port's own constant, and it may not be changed** |
| 6 | `src/Hamlet.RadioEngine/Transmit/Ft8Composer.cs:263` | `=> Build(text, sampleRate, baseFrequencyHz, wholeSlot: false);` |
| 7 | `src/Hamlet.RadioEngine/Transmit/Ft8Composer.cs:286` | `if (!RateIsUsable(sampleRate, out var rateExplanation))` - 12000 passes |
| 8 | `src/Hamlet.RadioEngine/Transmit/Ft8Composer.cs:307` | `Ft8Waveform.Synthesize(symbols, sampleRate, baseFrequencyHz)` - the samples are written at 12000 |
| 9 | `src/Hamlet.RadioEngine/Transmit/Ft8Composer.cs:309-316` | `new Ft8Transmission(..., audio, sampleRate, ...)` - the rate is stored beside its samples |
| 10 | `src/Hamlet.RadioEngine/Transmit/Ft8Composer.cs:69` | `Ft8Transmission.SampleRate` - the record parameter it lands in |

**The decision is made at step 3, by absence.** There is no line in the
application that chooses 12000 Hz; there is a line that declines to choose
anything, and 12000 is what the language supplies. That distinction matters for
where the repair goes: nothing has to be un-decided, only decided for the first
time.

The value is then read back out once, at
`src/Hamlet.RadioEngine/Transmit/Ft8TransmitSequence.cs:288`:

```csharp
.PlayAsync(samples, send.Transmission.SampleRate, cancellationToken)
```

## 2. What rate does the real sink demand, and where does it learn it?

**It demands the endpoint's own shared-mode mix rate, and it learns it from the
endpoint at construction.**

Learned at `src/Hamlet.RadioEngine/Audio/WasapiTransmitSink.cs:137-139`:

```csharp
var mix = client.MixFormat;
EndpointSampleRate = mix.SampleRate;
```

Published at `src/Hamlet.RadioEngine/Audio/WasapiTransmitSink.cs:193-194`:

```csharp
/// <summary>The rate the endpoint declared, which is the rate it will get.</summary>
public int EndpointSampleRate { get; }
```

The comparison, verbatim, at
`src/Hamlet.RadioEngine/Audio/WasapiTransmitSink.cs:301`:

```csharp
if (sampleRate != EndpointSampleRate)
```

The exception message, verbatim, at
`src/Hamlet.RadioEngine/Audio/WasapiTransmitSink.cs:306-309`:

```csharp
throw new InvalidOperationException(
    $"the samples are at {sampleRate} Hz and the endpoint speaks "
    + $"{EndpointSampleRate} Hz. Nothing is played rather than a rate being "
    + "silently changed on the way out.");
```

**The refusal is on purpose and is not this unit's to soften.** The comment above
it at `:302-305` says why: shared-mode WASAPI would resample anything handed to
it silently, and a sink that accepted 12000 Hz would report *asked 12000, got
12000* while something nobody chose decided what the samples became.

## 3. Where do the two meet on the live path?

**At `src/Hamlet.RadioEngine/Transmit/Ft8TransmitSequence.cs:287-289`.** That is
the only call to `ITransmitAudioSink.PlayAsync` on the live path, and it passes
`send.Transmission.SampleRate` - the 12000 from question 1 - into the comparison
from question 2.

The throw is caught at `Ft8TransmitSequence.cs:308-316`, which sets the outcome
and composes the reason:

```csharp
catch (Exception ex)
{
    outcome = keyed ? Ft8TransmitOutcome.AudioFailed : Ft8TransmitOutcome.PortFailed;
    reason = keyed
        ? $"the audio path failed while the radio was transmitting: {ex.Message}"
        : $"the radio would not take the keying frame: {ex.Message}";
}
```

That reason reaches the operator through
`MainWindowViewModel.WentLine` at `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:8253-8256`:

```csharp
if (!run.Sent)
{
    return "Hamlet did not send \"" + text + "\": " + run.Reason;
}
```

**So the reserved Send area would read, on an ordinary 48000 Hz endpoint:**

> Hamlet did not send "CQ KC3QIS FN00": the audio path failed while the radio was
> transmitting: the samples are at 12000 Hz and the endpoint speaks 48000 Hz.
> Nothing is played rather than a rate being silently changed on the way out.

It is honest, it names the fault, and it arrives **after the radio has already
been keyed and unkeyed**. That is question 4.

## 4. Where is the radio keyed relative to that throw?

**Keyed first. The ordering, by line number in
`src/Hamlet.RadioEngine/Transmit/Ft8TransmitSequence.cs`:**

| Line | What happens |
|---|---|
| 283 | `await _port.WriteAsync(Frame(CivConstants.PttOn), cancellationToken)` - **PTT on goes down the wire** |
| 285 | `keyed = true;` |
| 287-289 | `await _sink.PlayAsync(samples, send.Transmission.SampleRate, cancellationToken)` - **the sink throws here** |
| 308-314 | the catch: `outcome = AudioFailed`, because `keyed` is true |
| 315-346 | the `finally`: `outcome != Sent`, so the normal unkey at 326 is skipped and `TransmitAbort.Fire` runs at 344 |
| 348-353 | `new TransmitRun(...)` is returned |

**What the wire shows:** one CI-V `PTT on` frame, then - within the time it takes
an already-constructed sink to compare two integers and throw - the abort's
frames. `TransmitAbort.Fire` is the same-thread, no-await abort: CI-V `0x17` with
`0xFF`, PTT off as the fallback. **The transmitter is keyed for microseconds and
no modulation is ever offered to it.** Nothing illegal happens and nothing is
damaged.

**What `TransmitRun` reports** (constructed at `:350-353`):

- `Outcome` = `Ft8TransmitOutcome.AudioFailed`
- `Reason` = *the audio path failed while the radio was transmitting: the samples
  are at 12000 Hz and the endpoint speaks 48000 Hz. Nothing is played rather than
  a rate being silently changed on the way out.*
- `Keyed` = `true`
- `CameOutOfTransmit` (`unkeyedNormally`) = `false`
- `Abort` = a fired `AbortRecord`
- `Played` = `null`
- `SamplesOffered` = 151680, `SecondsOffered` = 12.64

**This is what decides where the fix belongs.** The keying write at 283 precedes
the sink call at 287. A rate the endpoint cannot take is knowable the moment the
sink is constructed and is not knowable any earlier than that; if the check is
left until the click reaches `PlayAsync`, the radio has already keyed before
anybody finds out. **So the refusal has to happen where the sink is built -
`MainWindowViewModel.BuildTheArmedSend` - and not at the click and not inside the
sequence.** The instruction expected exactly this ordering and the tree agrees
with it.

## 5. What does `WasapiTransmitSink.Endpoints()` publish on this machine?

**Measured, not inferred.** The shell on this machine refused both spellings of a
script that would have called it (recorded in the report's section 3.4), so this
is the instruction's own named fallback: a filtered test that prints the same
table, at
`tests/Hamlet.RadioEngine.Tests/Audio/WhatThisMachinesRenderEndpointsDeclareTests.cs`.

`COUNT: 4 active render endpoints on this machine`

| # | Id | Friendly name | Default | Rate | Ch | Bits | Encoding |
|---|---|---|---|---|---|---|---|
| 1 | `{0.0.0.00000000}.{18ee4fd1-d3a4-45ec-b01d-217d78612ae0}` | S34J55x (3- HD Audio Driver for Display Audio) | no | **48000** | 2 | 32 | Extensible 32-bit subformat `00000003-0000-0010-8000-00aa00389b71` |
| 2 | `{0.0.0.00000000}.{74aaef51-f977-4ca1-9739-98cf6fbef3ed}` | Ball Speaker (USBAudio2.0) | **yes** | **48000** | 2 | 32 | Extensible 32-bit subformat `00000003-0000-0010-8000-00aa00389b71` |
| 3 | `{0.0.0.00000000}.{8176ccfa-b55e-4466-8dc5-ff585b04cdaa}` | Speakers (USB Audio) | no | **48000** | 2 | 32 | Extensible 32-bit subformat `00000003-0000-0010-8000-00aa00389b71` |
| 4 | `{0.0.0.00000000}.{838d1db5-3b9b-4fe0-8563-c324f327aba3}` | S34J55x -2 (HD Audio Driver for Display Audio) | no | **48000** | 2 | 32 | Extensible 32-bit subformat `00000003-0000-0010-8000-00aa00389b71` |

**Every one of the four declares 48000 Hz. Not one declares 12000.** So the
application's send path, as it stands, cannot transmit through any endpoint this
machine has. **0 of 4.**

## 6. Does `Ft8Composer.RateIsUsable` accept every rate those endpoints declare?

**Yes - all four, and it refuses none of them.** Measured by the same test:

| Endpoint | Declared rate | `RateIsUsable` | `BaseFrequencyIsUsable(1000 Hz)` |
|---|---|---|---|
| S34J55x (3- HD Audio Driver for Display Audio) | 48000 | **True** | **True** |
| Ball Speaker (USBAudio2.0) | 48000 | **True** | **True** |
| Speakers (USB Audio) | 48000 | **True** | **True** |
| S34J55x -2 (HD Audio Driver for Display Audio) | 48000 | **True** | **True** |

`Ft8Composer.DefaultBaseFrequencyHz` is 1000 Hz; the top of the eight tones sits
at 1000 + 7 x 6.25 = 1043.75 Hz, far below the 24000 Hz Nyquist limit of a 48000
Hz rate, so the base frequency holds at every one of them.

**The wider rate table, for the machines this repository will never run on**
(same test, `TheRatesOrdinaryHardwareDeclaresAreAllUsable`):

```
   8000 Hz  RateIsUsable=True
  11025 Hz  RateIsUsable=True
  12000 Hz  RateIsUsable=True
  16000 Hz  RateIsUsable=True
  22050 Hz  RateIsUsable=True
  24000 Hz  RateIsUsable=True
  44100 Hz  RateIsUsable=True
  48000 Hz  RateIsUsable=True
  96000 Hz  RateIsUsable=True
 192000 Hz  RateIsUsable=True
```

**A channel symbol is 0.16 s** (`Ft8Waveform.SymbolPeriodSeconds`), so the
port's consistency condition is met by any rate whose product with 0.16 is a whole
number - which every rate consumer audio hardware declares happens to be. **The
refusal path is real but unreached on ordinary hardware**, so it was measured
against constructed rates rather than left unquoted
(`ARateFt8CannotBeBuiltAtIsRefusedInWords`):

```
      0 Hz  RateIsUsable=False
          0 samples per second is not a sample rate. Audio cannot be synthesised at zero or fewer.
   8001 Hz  RateIsUsable=False
          at 8001 samples per second the signal is 101133 samples long measured from the
          transmission's duration and 101120 measured as 79 symbols of 1280. The slot is laid
          out from the first and written from the second, so every sample after the signal
          starts would be at the wrong offset. Use a rate at which a symbol is a whole number
          of samples - 12000 is the one FT8 is decoded at.
  37999 Hz  RateIsUsable=False
          at 37999 samples per second the signal is 480307 samples long measured from the
          transmission's duration and 480320 measured as 79 symbols of 6080. The slot is laid
          out from the first and written from the second, so every sample after the signal
          starts would be at the wrong offset. Use a rate at which a symbol is a whole number
          of samples - 12000 is the one FT8 is decoded at.
```

**That is the engine's wording, and it is written for whoever wrote the call, not
for whoever owns the sound card.** *Use a rate at which a symbol is a whole
number of samples* is not an instruction an operator can act on. Task 3's refusal
has to name the endpoint and tell him to change his sound device, and it will be
built on top of this rather than instead of it.

## 7. Why did no existing test catch this?

**The fake is more permissive than the thing it stands for.**

`tests/Hamlet.RadioEngine.Tests/Transmit/FakeTransmitAudioSink.cs:42-55`:

```csharp
public Task<PlayedAudio> PlayAsync(
    ReadOnlyMemory<float> samples, int sampleRate, CancellationToken cancellationToken)
{
    TimesCalled++;
    SamplesHandedOver = samples.Length;
    RateAskedFor = sampleRate;          // <- line 47

    if (Throws is not null)
    {
        throw Throws;
    }

    return Task.FromResult(new PlayedAudio(PlaysOnly ?? samples.Length, Took));
}
```

**Line 47 is the whole of it.** The fake *records* the rate it was asked for and
then plays anyway. It can be told to throw, to play short, and to be cancelled -
every way a sink can fail except the one that is failing.

The application's tests have a second fake with the same gap and less:
`tests/Hamlet.App.Tests/FakeTransmitParts.cs`'s `FakeSink.PlayAsync` **does not
even read `sampleRate`**. It is the one substituted through
`MainWindowViewModel.TransmitSinkFactory`, so it is the one every test of the
application's own send path runs against.

**The one behaviour neither reproduces: refusing a rate that is not its own.**
`WasapiTransmitSink` has a rate it will accept and throws on anything else;
neither fake has a rate at all. So every test that exercises the send path hands
12000 Hz to a sink that will take any number, and passes - which is why sixteen
units of green tests have sat over a path that cannot transmit on any endpoint
this machine has.

---

## What this trace settles for the rest of the unit

1. **The repair is a decision made for the first time**, not a decision changed.
   Nothing chooses 12000; a defaulted parameter supplies it.
2. **It belongs in `BuildTheArmedSend`**, because line 283 keys before line 287
   throws, and the sink is the only thing that knows the rate.
3. **On this machine it will move the count from 0 of 4 to 4 of 4**, because all
   four endpoints declare 48000 Hz and `RateIsUsable` accepts it.
4. **The refusal branch will be unreachable on this machine's hardware** and must
   still be written and tested, because the operator's machine is not this one.
