# Unit 256, task 1 - what this machine can actually play and capture

**Measured 2026-09-06 on the development machine.** Every device figure below is
labelled *development machine* because `SHACK_FACTS.md` FACT-004 rules that there
are two computers, only one has a radio, and **no measurement of this machine's
audio endpoints says anything about the IC-7300's USB codec.** Which machine a
figure came from is part of the figure.

Nothing under `src/` was changed by this task.

---

## How this was measured, and why it was not measured from the shell

**The shell refused device enumeration.** Recorded verbatim:

```
powershell -NoProfile -Command "Get-CimInstance Win32_SoundDevice | Select-Object Name,Status,StatusInfo | Format-List"
-> This command requires approval
```

That is the refusal the work instruction anticipated and it names the alternative:
a throwaway xunit test that enumerates and prints, run filtered by exact name.
That test is
`tests/Hamlet.RadioEngine.Tests/Audio/WhatThisMachineCanPlayAndCaptureTests.cs`.
It asserts nothing about the numbers it prints, because **there is no right
answer to assert** - a machine with no render endpoint is a normal machine under
FACT-004.

Three facts were taken, each by one `[Fact]` run filtered by its fully qualified
name in the foreground:

- `WhatRenderEndpointsThisDevelopmentMachineHas`
- `WhetherLoopbackCaptureStartsOnThisDevelopmentMachine`
- `WhichRenderEndpointsWillOpenOnThisDevelopmentMachine`

---

## 1. Does this machine have an active render endpoint?

**Yes. Four of them, all `DataFlow.Render`, `DeviceState.Active`.** *Development
machine.*

| # | Friendly name | Endpoint id | Default for `Role.Console` |
|---|---|---|---|
| 1 | `S34J55x (3- HD Audio Driver for Display Audio)` | `{0.0.0.00000000}.{18ee4fd1-d3a4-45ec-b01d-217d78612ae0}` | no |
| 2 | `Ball Speaker (USBAudio2.0)` | `{0.0.0.00000000}.{74aaef51-f977-4ca1-9739-98cf6fbef3ed}` | **yes** |
| 3 | `Speakers (USB Audio)` | `{0.0.0.00000000}.{8176ccfa-b55e-4466-8dc5-ff585b04cdaa}` | no |
| 4 | `S34J55x -2 (HD Audio Driver for Display Audio)` | `{0.0.0.00000000}.{838d1db5-3b9b-4fe0-8563-c324f327aba3}` | no |

For completeness, the capture side, which is what `WasapiAudioDevices.List()`
already covers: **two active capture endpoints**, `Microphone (EMEET SmartCam
C960)` and `Microphone (USB Audio)`. *Development machine.*

## 2. What the default render endpoint declares

`Ball Speaker (USBAudio2.0)`, the `Role.Console` default. *Development machine.*

```
48000 Hz, 2 ch, 32-bit, tag Extensible,
subformat 00000003-0000-0010-8000-00aa00389b71, 384000 B/s
```

- **Sample rate 48000 Hz.** Not 12000. The decoder wants 12000.
- **Channel count 2.**
- **Bit depth 32.**
- **Encoding tag `Extensible`**, with the subformat GUID
  `00000003-0000-0010-8000-00aa00389b71` - `KSDATAFORMAT_SUBTYPE_IEEE_FLOAT`.
  **This is exactly the case `WasapiAudioSource.Kind` at
  `src/Hamlet.RadioEngine/Audio/WasapiAudioSource.cs:514` exists to handle**: the
  top-level tag says `Extensible` and the truth is in the subformat. Unit 237
  paid for learning that on the capture side and the render side declares the
  same shape.

**All four endpoints declare the identical format** - 48000 Hz, 2 ch, 32-bit,
`Extensible` over IEEE float, 384000 B/s. *Development machine.*

**This says nothing about the IC-7300's codec.** FACT-004 rules that what format
tag, channel count, sample rate or encoding the radio's USB codec declares is
unknown from this side and may not be inferred. The figures above are the figures
of a Samsung monitor and two USB speakers on Tim's desk.

## 3. Is `WasapiLoopbackCapture` available, and does it start?

**Yes to both.** *Development machine.*

Constructed on the default endpoint, started, given one second, stopped:

```
LOOPBACK FORMAT: 48000 Hz, 2 ch, 32-bit, tag IeeeFloat, 384000 B/s
LOOPBACK BUFFERS : 16
LOOPBACK BYTES   : 0
LOOPBACK PEAK    : 0.000000000
LOOPBACK STARTED AND DELIVERED BUFFERS
```

Read that carefully, because it is the one measurement in this survey that can be
misread:

- **It constructed, it started, and it delivered 16 callbacks in one second.** The
  capture path is alive.
- **It delivered zero bytes**, and that is the correct and expected result,
  because **nothing was rendering to that endpoint at the time.** WASAPI loopback
  on an idle endpoint returns packets of length zero; it does not manufacture
  silence. Buffers arriving with `BytesRecorded == 0` is the signature of *the
  capture works and there is nothing to capture*, not of *the capture is broken*.
- **The loopback declares its format as `IeeeFloat` rather than `Extensible`** -
  the same bytes, described by NAudio without the extensible wrapper on this
  path. Worth knowing before writing a conversion that switches on the tag.

**Whether samples arrive when something is actually playing is task 3's to
prove**, and it is the one thing this survey cannot settle on its own, because
nothing in this repository renders yet. That is what task 2 builds.

## 4. Which endpoint would be played to, and is anything audible?

**All four open in shared mode at their own mix format.** Each was initialised and
closed again without a sample being written, so nothing was heard:

```
OPENS: S34J55x (3- HD Audio Driver for Display Audio) - 48000 Hz, 2 ch, 32-bit, buffer 4800 frames
OPENS: Ball Speaker (USBAudio2.0)                     - 48000 Hz, 2 ch, 32-bit, buffer 4800 frames
OPENS: Speakers (USB Audio)                           - 48000 Hz, 2 ch, 32-bit, buffer 4800 frames
OPENS: S34J55x -2 (HD Audio Driver for Display Audio) - 48000 Hz, 2 ch, 32-bit, buffer 4800 frames
```

*Development machine.* The 4800-frame shared-mode buffer is 100 ms at 48000 Hz.

**Is there a render endpoint that is not the speakers? Yes - two.** The two
`S34J55x ... HD Audio Driver for Display Audio` endpoints are a monitor's audio
path over its display cable, not the machine's speakers.

**The endpoint task 3 will choose is `S34J55x -2 (HD Audio Driver for Display
Audio)`**, for three reasons, and it is chosen rather than defaulted to:

1. **It is a display-audio endpoint, not a speaker.** Task 3 plays 12.64 seconds
   of FT8 tones and this may be the middle of the night. `Ball Speaker` and
   `Speakers (USB Audio)` are things that make noise in the room.
2. **It is not the default.** Choosing it exercises task 2's *the caller names the
   device and the sink never silently falls back* requirement, rather than
   letting a bug that falls back to the default look like a pass.
3. **It opens.** Measured above, not assumed.

**The system volume was not touched and will not be.** That is a change to Tim's
machine that outlives the unit, and WASAPI loopback captures the render mix
regardless of it.

## 5. What `WasapiAudioDevices.List()` covers, and what it does not

`src/Hamlet.RadioEngine/Audio/WasapiAudioSource.cs:20` enumerates
**`DataFlow.Capture` only**, at `:29-30`, and returns `Array.Empty<AudioDevice>()`
rather than throwing on a machine with no sound card (`:43-46`). It has no notion
of a render endpoint at all.

**Where a render list belongs: nowhere near `IAudioDevices`.** `IAudioDevices` is
an existing interface with existing callers, and every one of them is asking *what
can Hamlet listen to* - the answer feeds `WasapiAudioSource`, which takes an
`AudioDevice` and opens it for capture. **Putting render endpoints into the same
list would hand a caller a device it cannot open**, and `AudioDevice` carries no
field that would let it tell the two apart. Adding a second method to the
interface is no better: every existing implementation would have to grow one, to
answer a question only the transmit path asks.

**So the render enumeration lives with the render sink and nowhere else**, which
is task 2's decision to make and to record.

## 6. How `Ft8SlotDecoder` is constructed and called, and what comes back

**Correction to the work instruction, reported rather than repaired:** the
instruction says `src/Hamlet.RadioEngine/Transmit/Ft8Composer.cs` *already
constructs and calls* `Ft8SlotDecoder`. **It does not.** A grep for
`new Ft8SlotDecoder` across `src` and `tests` returns no hit in `Ft8Composer.cs`;
what that file calls at `:585` is `Ft8MessageDecoder.Decode(packed, cache)`, the
*message* layer, which unpacks bits back to text and never sees a sample.

**The construction to reuse is in the test the instruction also names**,
`tests/Hamlet.RadioEngine.Tests/Transmit/HamletsOwnDecoderReadsBackWhatHamletComposedTests.cs`:

```csharp
var decoder = new Ft8SlotDecoder();                       // :51
var texts   = decoder.Decode(transmission.Samples).Texts; // :82
```

- **The default constructor.** No geometry, no limits, no arguments.
- **`Decode(ReadOnlySpan<float> samples)`** returns an `Ft8SlotResult`, and the
  member that matters is **`.Texts`**, a collection of decoded message strings.
- **The comparison the existing test makes** is
  `texts.Contains(transmission.ReadsBackAs, StringComparer.Ordinal)` at `:84` -
  ordinal, whole string, no substring and no prefix. **Task 3 uses the same
  comparison**, which is also what *do not loosen the loopback's comparison*
  requires.
- **`transmission.ReadsBackAs`, not `transmission.Text`.** `Ft8Transmission` at
  `src/Hamlet.RadioEngine/Transmit/Ft8Composer.cs:80` carries both: `Text` is what
  the operator asked to send and `ReadsBackAs` is what the bits actually say. For
  a message that is not hash-marked they are the same string; comparing on the
  wrong one is a way to fail a working chain.
- **The decoder reads at 12000 Hz** - `Ft8Composer.DefaultSampleRate` at `:201` is
  `Ft8Waveform.DefaultSampleRate`.

**Which route composes what.** `Ft8Composer.Compose` at `:224` returns the padded
15 s slot and is *the route that goes into a decoder*; `Ft8Composer.ComposeSignal`
at `:259` returns the bare 12.64 s of tones and is *the route that goes on the
air* (its own remarks at `:239-253`). **Task 3 plays `ComposeSignal`'s output**,
because that is what a transmission is, and hands the decoder the captured audio.

## 7. What `AudioTap` needs, and whether it can hold the transmission

`src/Hamlet.RadioEngine/Audio/AudioTap.cs`:

- **`Take(ReadOnlySpan<float> samples, int sampleRate)` at `:192`** wants **mono
  floats and the rate they were taken at**. It is fed, not asked - it opens
  nothing. It allocates its ring the first time it sees a rate and **reallocates
  and empties itself if the rate ever changes** (`:206-213`), so the capture must
  be downmixed to mono and handed over at one consistent rate.
- **`Snapshot()` at `:313`** returns the whole ring as a `MonoAudio?`.
- **`Window(long firstSample, int count)` at `:381`** returns a `MonoAudio?`
  addressed from the first sample ever taken, or null when the tap no longer holds
  all of it.
- **`Tail(TimeSpan wanted)` at `:426`** returns the most recent stretch, oldest
  first, or **null when less than `wanted` is held - short is not padded** (`:437`).
- **`Level`** at `:160` is the `AudioLevel(PeakDb, FloorDb, ...)` record from
  `:17`, with `NearlySilent` at `:42` true at or below -60 dB.

**Can it hold 12.64 s at the endpoint's rate? Yes, comfortably.** `SecondsKept =
30` at `:76`, and the ring is `sampleRate * SecondsKept` floats (`:209`). At
48000 Hz that is 1,440,000 samples, and one transmission at 48000 Hz is
**606,720 samples, 12.64 s** - 42% of the ring. **Checked rather than trusted:**
12.64 s x 48000 = 606,720, and the instruction's own arithmetic (151,680 at
12000 Hz, 606,720 at 48000 Hz, 4,853,760 bytes for stereo 32-bit float) is
correct. The 4800-frame device buffer means roughly 127 `Take` calls per
transmission.

## 8. What is missing, explicitly

- **Nothing in this repository renders audio through the engine.**
  `ITransmitAudioSink` has one implementation and it is
  `tests/.../FakeTransmitAudioSink.cs`. That is task 2.
- **Nothing enumerates render endpoints.** `WasapiAudioDevices` is capture only.
  Task 2 needs its own, and section 5 says why it does not go on `IAudioDevices`.
- **No 48000 Hz -> 12000 Hz path has ever been exercised on captured device
  audio.** `Ft8Resample.ToFt8Rate` at
  `src/Hamlet.RadioEngine/Audio/Ft8Resample.cs:59` exists and
  `TargetSampleRate = 12_000` at `:32`, but every use of it so far has been on
  synthesised or file audio. **48000 / 12000 = 4 exactly**, which is the kindest
  possible ratio and is worth saying out loud.
- **No stereo-to-mono downmix exists on the transmit side.**
  `WasapiAudioSource.Downmix` at `:413` is `internal static` and does exactly this
  job, and it is `internal` to the engine assembly - reachable from engine code,
  and reachable from the test assembly only if `InternalsVisibleTo` is already in
  force. **Task 3 will check that rather than assume it.**
- **Whether loopback delivers non-zero bytes while something is rendering is
  unmeasured**, for the reason in section 3: there was nothing to render. It is
  the first thing task 3 finds out.
- **The IC-7300's expected input level, format, rate and channel count are
  missing and stay missing.** FACT-004. They are not inferable from anything on
  this page.

---

## The go/no-go

Four active render endpoints, all opening in shared mode at 48000 Hz 2-channel
32-bit float; `WasapiLoopbackCapture` constructs, starts, delivers 16 callbacks
in one second and stops cleanly. Nothing measured here is a reason to take the
fallback.

```
LOOPBACK ROUTE: device
```

**Decided by:** `WasapiLoopbackCapture` started on the default endpoint and
delivered 16 buffers in 1.000 s with a clean stop, and all four `DataFlow.Render`
`DeviceState.Active` endpoints initialised in shared mode at 48000 Hz, 2 ch,
32-bit IEEE float with a 4800-frame buffer. *Development machine.*
