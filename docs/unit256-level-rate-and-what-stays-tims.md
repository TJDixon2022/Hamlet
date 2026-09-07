# Unit 256, task 4 - the level, the rate, and what stays Tim's

**Measured 2026-09-06.** Every figure in section 1 is a *development machine*
figure. Section 2 says what none of them is.

---

## 1. What the render path measured, on the development machine

The endpoint, chosen by name rather than defaulted to:

| | Measured | Where from |
|---|---|---|
| Endpoint | `S34J55x (3- HD Audio Driver for Display Audio)` | *development machine* |
| Endpoint id | `{0.0.0.00000000}.{18ee4fd1-d3a4-45ec-b01d-217d78612ae0}` | *development machine* |
| Is it the default? | **No.** Chosen deliberately, from 4 active render endpoints | *development machine* |
| Declared mix format | 48000 Hz, 2 ch, 32-bit, tag `Extensible`, subformat `00000003-0000-0010-8000-00aa00389b71` (IEEE float), 384000 B/s | *development machine* |
| Share mode | **shared**, at the endpoint's own mix format | - |
| Buffer granted | 9600 frames = **200 ms** at 48000 Hz | *development machine* |

The rate, asked and got:

| | Value |
|---|---|
| **Rate asked** | **48000 Hz** |
| **Rate got** | **48000 Hz** *(development machine)* |

**They are equal, and that is a design decision rather than a coincidence.**
WASAPI shared mode will resample anything it is handed, silently, so a sink that
asked for 12000 Hz would report *asked 12000, got 12000* while Windows quietly
put a rate conversion nobody chose inside the transmit path. `WasapiTransmitSink`
therefore initialises at the endpoint's declared mix format, publishes that rate,
and **refuses a mismatched rate loudly**:

```
the samples are at 12000 Hz and the endpoint speaks 48000 Hz. Nothing is played
rather than a rate being silently changed on the way out.
```

So the transmission is composed at the endpoint's rate - `Ft8Composer.ComposeSignal(text, 48000)`,
which the port accepts, 606,720 samples and 12.64 s - and nothing resamples on
the way out. The only resampling in the chain is `Ft8Resample.ToFt8Rate` on the
way **back in**, 48000 to 12000, which is an exact factor of four.

The level and the conversion, measured on the way out over three transmissions:

| | Measured | Where from |
|---|---|---|
| Format written | 32-bit IEEE float, 2 channels, the same sample in both | *development machine* |
| **Peak written** | **1.0, which is 0 dBFS** | *development machine* |
| **RMS written** | **0.706407, which is -3.02 dBFS** | *development machine* |
| **Clipped samples** | **0** | *development machine* |
| Peak of the captured audio | **0 dBFS** over the whole capture | *development machine* |
| Captured | 625,920 samples, 13.04 s at 48000 Hz | *development machine* |

**Read the peak and the clip count together, because either alone is
misleading.** The composed FT8 signal is a constant-envelope tone that reaches
1.0 exactly, so it is written **at the rail with no headroom at all** - and
nothing clipped, because nothing exceeded the rail. An RMS of 0.706407 is
1/sqrt(2) to six places, which is what a constant-amplitude sinusoid gives and is
a check that the conversion is doing arithmetic rather than something clever.

**Nothing about that is a defect and nothing about it is a setting.** It is what
`Ft8Waveform` synthesises and what a float-to-float conversion of it comes to.

### A measurement about the meter, not about the audio

`AudioTap.Level` on the finished capture reads **peak -90 dB, floor -26.5 dB,
`NearlySilent` true** - on audio that decoded perfectly. That is correct
behaviour being misread: `AudioTap.Level` is a moving meter over the last 0.2 s
(`AudioTap.cs:84`), and by the time a run finishes the last 0.2 s is the silence
after the transmission. **The tap's own remarks on `PeakOf` at `AudioTap.cs:755`
describe exactly this trap** and say it cost eight decibels of under-reporting
once already. `AudioTap.PeakOf(captured)` over the whole capture is the honest
figure and it is **0 dBFS**. Both are now printed side by side, each labelled.

---

## 2. None of it is the radio's number

**Everything in section 1 is a measurement of a Samsung monitor's audio endpoint
on Tim's desktop.** `SHACK_FACTS.md` FACT-004 rules that there are two computers,
only one has a radio, and **what format tag, channel count, sample rate or
encoding the IC-7300's USB codec declares is unknown from this side and may not
be inferred.** The same applies with more force to the level: 0 dBFS into a
monitor is a number about a monitor.

**Step 2's criterion 4 stays deferred, and what Tim must do is unchanged:**

> **Tim sets the drive level at the radio by watching ALC on the first live
> transmission, and writes the figure into `SHACK_FACTS.md` as a fact.**

**And step 3's criterion 1 keeps the same half deferred for the same reason.**
The render path, the named endpoint, the rate asked against the rate got, and the
level and format written are proved. *The right device, rate and level **for the
radio's USB input*** is not, and no unit can reach it from this machine.

**The one figure in section 1 that most looks like an answer and is not is the
peak.** 0 dBFS is what Hamlet writes to a sound card. What the IC-7300's USB
modulation input wants is a different number, on a different machine, read off
the ALC meter, and it is Tim's.

---

## 3. Step 3's six exit criteria

| # | Criterion | State | Evidence |
|---|---|---|---|
| 1 | Audio plays to the radio's USB input at the right device, rate and level | **Met as cut down; radio half deferred to Tim** | This unit: a real `ITransmitAudioSink` opens a *named* endpoint and never falls back, reports rate asked 48000 against rate got 48000, converts float to the endpoint's IEEE float, and measures peak 1.0 / RMS 0.706407 / 0 clipped - all *development machine*. The radio-side half is unreachable under FACT-004 and is left with Tim beside step 2's criterion 4. |
| 2 | Key, transmit, unkey, with the unkey surviving a throwing audio path | **Met** | Unit 255: 6 of 6 failure modes end unkeyed, with the wire quoted. Re-exercised here against the **real** sink - a play cancelled at 400 ms of 2000 ms reported 18,720 of 96,000 samples and came out of transmit through the abort. |
| 3 | **A loopback proves the whole chain - generate, play, capture on the tap, decode, get the message back** | **MET, THIS UNIT** | 3 of 3 messages out of the sound card and back through `Ft8SlotDecoder` as the same text, ordinal, whole message. `LOOPBACK ROUTE: device`. |
| 4 | The transmitted slot recorded to telemetry | **Met** | Unit 255, and re-proved here on the loopback transmissions themselves: each ran through `Ft8TransmitSequence` and wrote an eleven-field `ft8_transmission` record with no callsign and no message text in it. Unit 255's three discipline tests re-run green. |
| 5 | The licence gate in the path, asserted by a test | **Met** | Unit 255: refused at zero bytes on all three permissive branches, without `TransmitGuard.Check` being touched. Untouched by this unit. |
| 6 | Nothing keys without an operator action reaching this code | **Met** | Unit 255, re-greped here: nothing in `src/` constructs `Ft8TransmitSequence` or `WasapiTransmitSink`. Every construction is in a test. |

**Five of six met outright; criterion 1 met as the cut-down it was declared to
be, with its unreachable half named and left with the operator.**

---

## 4. The drop candidate

**It was not dropped.** The named candidate was *the second and third loopback
messages, and the WAV artefact of the captured audio*.

- **The second and third messages were run**, and they carry more than a repeat
  would: they are a report-with-grid and a signal report rather than three CQs,
  so what widened is the message *shape* going through the path.
- **The WAV artefact was dropped**, and not for time. Unit 254 dropped the same
  thing for the same reason and it still holds: a committed binary is a second
  copy of something a deterministic test regenerates, and this one would also be
  a recording of one particular sound card on one particular night.

The three transmissions are the cap exactly. The rate-lie breakage is folded into
the first test - the same capture decoded twice - so catching it cost no fourth
transmission.
