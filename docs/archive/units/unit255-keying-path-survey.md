# Unit 255 - what can key a radio today, and what plays audio

Read out of the tree on 2026-09-06, at the start of work instruction 255, before
a line of `src/` or `tests/` was changed. Every claim carries a file and a line.
Line numbers are as they stood at that reading; match on the symbol.

---

## 1. Every route in the tree that can key a transmitter today

Unit 253 named two - `Ic7300Rig.SendCwAsync` and `CivWrites.TuneNow`. **That
list is incomplete as a list of routes**, because the two it names are the two
*ends*; there are four call sites above them and one of them keys repeatedly on
its own schedule once an operator has started it.

Found by searching for the CI-V send command, for the transceiver-control
command, for `0x1C` and `0x17` written as literals anywhere outside
`CivConstants`, and for PTT by name.

| # | Route | File and line | What reaches the radio |
|---|---|---|---|
| 1 | `Ic7300Rig.SendCwAsync(string, CancellationToken)` | `src/Hamlet.RadioEngine/Rig/Ic7300Rig.cs:409`, frame built at `:420` | CI-V `17` carrying up to 30 ASCII characters. The radio's own keyer sends them. **This is the only place in the tree that builds a keying frame.** |
| 2 | `KeyerCwSender.SendAsync` | `src/Hamlet.RadioEngine/Cw/KeyerCwSender.cs:53`, calling the rig at `:89` | Route 1, once per piece of a split message. |
| 3 | `CwTransmitter.SendAsync` | `src/Hamlet.RadioEngine/Cw/CwTransmitter.cs:126` | Route 2, behind `TransmitGuard.Check` at `:97`. This is the gated CW send path (HM-DEC-059). |
| 4 | `AutoCaller` | `src/Hamlet.RadioEngine/Cw/AutoCall.cs:272`, keying at `:487` | Route 2 in a loop, with `AutoCallStop` interlocks at `:135` and a transmit-status re-read at `:640`. **The one route in the engine that keys more than once per operator action.** Parked by this instruction; named here and not touched. |
| 5 | `Ic7300Rig.SetSettingAsync(CivWrites.AntennaTuner, CivWrites.TuneNow)` | write descriptor `src/Hamlet.RadioEngine/Civ/CivWrites.cs:305`, the keying value `TuneNow = 0x02` at `:311`, the generic write at `Ic7300Rig.cs:255` | CI-V `1C 01 02`, a tuning cycle, which transmits. The descriptor carries `RigWriteTier.Keys` at `CivWrites.cs:308` and its own remarks say "That third value transmits." |

**Route 5 has no caller.** `grep -rn "TuneNow"` across `.cs` and `.axaml`,
excluding `bin/` and `obj/`, returns exactly one line - the constant's own
declaration at `CivWrites.cs:311`. It is a documented, tiered, reachable route
with nothing standing on it.

`TrainingRig.SendCwAsync` at `src/Hamlet.RadioEngine/Rig/TrainingRig.cs:160` has
the same signature as route 1 and touches no hardware; it is not a keying route.

**What does not key:** `BandScanner` says so in its own remarks at
`src/Hamlet.RadioEngine/Scan/BandScanner.cs:156` - it never calls
`IRig.SendCwAsync` and its only rig calls are reads and tuning writes.
`TransmitAbort.Fire` writes `1C 00 00`, which is the *off* value.

---

## 2. What the CI-V surface already has, and what it deliberately lacks

`CivWrites` (`src/Hamlet.RadioEngine/Civ/CivWrites.cs:78`) is a table of
`CivWrite` descriptors, each with its command byte, its manual page, its value
legend and a `RigWriteTier`. `CivWrites.All` at `:348` is the whitelist:
`Ic7300Rig.SetSettingAsync` refuses at `Ic7300Rig.cs:263` anything not in it -
"NOTHING WRITES A BYTE THAT IS NOT IN THE TABLE (§4, HM-DEC-084)". The table
covers mode, the receive chain, filters, CW pitch, USB audio levels, RF power,
keyer speed, break-in and the antenna tuner. **There is no PTT write descriptor
in it and no CW-send descriptor in it** - route 1 does not go through the table
at all, and neither does the abort.

**Confirmed: no constant in the tree turns the transmitter on.**
`src/Hamlet.RadioEngine/Civ/CivConstants.cs` names `CmdSendCwMessage = 0x17`
(`:56`), `CwStopByte = 0xFF` (`:67`), `CmdTransceiverControl = 0x1C` (`:78`),
`SubPtt = 0x00` (`:83`) and `PttOff = 0x00` (`:96`). Its remarks at `:89-95` say
why the on value is absent: "ONLY THE OFF VALUE IS NAMED HERE, AND THAT IS
DELIBERATE ... a constant is the easiest thing in a codebase to reach for by
accident." A grep for `0x1C` and `0x17` as literals outside `CivConstants` finds
them only in `CivReads.cs:177` (read transmit status), `CivWrites.cs:306` (the
tuner) and inside `src/Ft8Sharp/Tables/Ft8Tables.g.cs`, where they are LDPC
table bytes and not commands.

The one keying value that *does* exist as a named constant is
`CivWrites.TuneNow = 0x02` at `CivWrites.cs:311`, and it is the sub-value of a
different command family.

---

## 3. What `TransmitAbort.Fire` costs to call

`src/Hamlet.RadioEngine/Civ/TransmitAbort.cs:64`.

- **What it needs:** an `ISerialPort` and two optional address bytes, both
  defaulted (`CivConstants.DefaultRadioAddress`, `DefaultControllerAddress`).
  No rig, no state, no gate, no clock. It is `static` with no fields (`:57`).
- **What it returns:** `AbortRecord(AbortAttempt CwStop, AbortAttempt PttOff)`
  at `:22`, each attempt carrying the `CivFrame` it built, whether the write was
  taken, and the failure message if not. `AnythingReachedTheRadio` at `:25` is
  true if either half got out.
- **The two frames:** `17 FF` and `1C 00 00`, built at `:69-75`, each attempted
  independently at `:81-82`. The second is not a retry of the first.
- **Awkward in a `finally`? No, and it is close to purpose-built for one.** It
  is synchronous - `port.Write(ReadOnlySpan<byte>)` at `:92`, the interface's
  abort-path-only write documented at
  `src/Hamlet.RadioEngine/Transport/ISerialPort.cs:54` - so nothing is awaited,
  which matters because C# permits no `await` in a `finally` under some
  cancellation shapes and permits it awkwardly under all of them. It never
  throws: every exception is caught at `:95` and recorded. So the only real
  cost is that **the caller must do something with the record it returns** -
  the remarks at `:20` require it: "Every caller from step 3 onward logs this."
  A `finally` block cannot return a value, so the record has to be assigned to
  a local declared outside the `try` and carried out in the result.

**Nothing in the repository calls it.** Its own remarks say so at `:53`.
Verified: `grep -rn "TransmitAbort" --include=*.cs src` returns two lines - the
declaration at `TransmitAbort.cs:57` and a doc-comment mention at
`Ft8Composer.cs:164` saying the composer does *not* name it. No call site.

---

## 4. The licence gate's three permitting branches

`src/Hamlet.RadioEngine/Licensing/TransmitGuard.cs:67`,
`Check(LicenseClass, long frequencyHz, TransmitMode, bool guardEnabled)`,
returning `TransmitDecision(bool MayTransmit, string Reason, string Citation,
bool WasOverridden)` declared at `:11`.

**Branch one - the privileges permit it (`:72-75`):**

```csharp
if (verdict.MayTransmit)
{
    return new TransmitDecision(true, "", verdict.Citation, false);
}
```

Returns `MayTransmit: true`, an **empty** `Reason`, the privilege plan's
`Citation`, and `WasOverridden: false`. This is the only branch that says yes
because the licence says yes.

**Branch two - the licence class is unknown (`:77-85`):**

```csharp
if (verdict.Status == PrivilegeStatus.Unknown)
{
    return new TransmitDecision(
        true,
        "Hamlet does not know your license class, so it is not checking this "
        + "transmission. Set your class in Settings and it will.",
        "",
        false);
}
```

Returns `MayTransmit: true`, a Reason that says it is **not checking**, an
**empty Citation**, and `WasOverridden: false`. The method's own remarks at
`:62-66` argue it: "An unknown license class does not block transmitting ... the
operator holds the license and knows what it says."

**Branch three - the operator's guard toggle is off (`:87-90`):**

```csharp
if (!guardEnabled)
{
    return new TransmitDecision(true, verdict.Explanation, verdict.Citation, true);
}
```

Returns `MayTransmit: true` **for a frequency the plan just refused**, carrying
the refusal's own explanation and citation, with `WasOverridden: true`.

The refusal is `:92`: `MayTransmit: false`, the explanation, the citation,
`WasOverridden: false`.

**The discrepancy, reported and not resolved** (task 3's instruction, and unit
253's banked owner-class item): §0.2 says the Settings check "is not bypassable
from any send path", and branches two and three are two ways past it. This unit
narrows only its own new send path, by treating both as refusals there. It
changes no line of `Check` and no existing caller - `CwTransmitter.cs:97` keeps
exactly the behaviour it has today. The question of what the *other* callers
should do stays the owner's.

---

## 5. The telemetry surface

`src/Hamlet.RadioEngine/Telemetry/ITelemetry.cs`.

- **Categories** (`:8-27`): `Diagnostics`, `Rig`, `Tuning`, `Explore`, `Decode`,
  `Performance`. **There is no transmit category.** The enum's own comment at
  `:4-7` says "The set is the schema - adding a category is a deliberate act,
  not a string typed at a call site."
- **`TelemetryEvent`** (`:54`): `TimestampUtc`, `SessionId`, `Level`,
  `AppVersion`, `Category`, `Event`, and `IReadOnlyDictionary<string, object?>
  Data`. The `Data` parameter's own documentation at `:52` is the rule: "Never
  callsigns, never decoded message content, never anything identifying a person
  or contact."
- **`ITelemetry.Write`** (`:70`) takes a category, a snake_case event name, an
  optional bag and a level. **`NullTelemetry`** at `:81` records nothing.
  `JsonlTelemetry.cs` is the on-disk implementation.
- **What `RecordDisciplineTests` already enforces**:
  `ADecodeWindowCannotCarryDecodedText` at
  `tests/Hamlet.RadioEngine.Tests/Telemetry/RecordDisciplineTests.cs:105` builds
  a `DecodeWindow`, calls `ToBag()`, and asserts of **every** pair that
  `pair.Value is int or long or double or bool` - "Every value is a number or a
  flag. Nothing here is text at all, so there is nowhere for a character to
  hide." `AWindowThatRejectedEverythingIsAWarning` at `:137` fixes the levels.
- **What HM-DEC-018 permits a transmit event to carry**, ruled already for a
  transmitted CW `CQ`: "the length, the count, the duration, the frequency and
  the mode make the transmission fully diagnosable and identify nobody." So:
  the slot's start in UTC, the frequency, the duration, the sample rate, the
  message *type* and the message *length*. Not the text, not `ReadsBackAs`, not
  a callsign.

---

## 6. The slot clock

`src/Hamlet.RadioEngine/Audio/Ft8Slots.cs:119`.

- `SlotSeconds = 15` (`:126`), `TransmissionSeconds = 12.64` (`:135`).
- `TransmissionFits(double secondsAfterBoundary)` (`:162`) - true where the
  12.64 s fits, with a `1e-6` epsilon for rounding only.
- `TrueUtc(DateTime pcUtc, ClockOffset offset)` (`:174`) - returns **null** when
  the offset is unknown.
- `SlotStart(DateTime trueUtc)` (`:182`) - the quarter-minute boundary at or
  before a moment.
- `IntoSlot(DateTime trueUtc)` (`:195`) - seconds since the boundary.
- `BoundariesBetween` (`:209`).

**Does any of it need a real clock or an SNTP answer to be testable? No.** The
class's own remarks at `:116` say "Pure: an offset and a moment in, a boundary
out. No clock is read here, so every threshold and every edge is testable
without one." `ClockOffset` at `:20` is a value carrying `OffsetSeconds` and
`MeasuredAtUtc`, and `ClockOffset.Unknown` at `:24` is a real state that
`TrueUtc` answers null for. `SntpClock.cs` sits beside it and is the only thing
that goes to the network; nothing in `Ft8Slots` calls it.

That is what lets this unit place a transmission in a slot and measure the
placement without a network, a device or a wall clock.

---

## 7. Audio output in the engine

**There is none.** `src/Hamlet.RadioEngine/Audio/` holds 22 files and every one
of them is capture, cutting, resampling, spectra or slot arithmetic:
`IAudioSource.cs`, `WasapiAudioSource.cs`, `AudioTap.cs`, `BufferedAudioSource.cs`,
`AudioArrival.cs`, `AudioDevice.cs`, `AudioHandoff.cs`, `AudioSpectrumSource.cs`,
`CallbackBudget.cs`, `CaptureHealth.cs`, `DigitalCaptureSheet.cs`,
`Ft8Reception.cs`, `Ft8Resample.cs`, `Ft8SlotCutter.cs`, `Ft8SlotLevel.cs`,
`Ft8SlotWatch.cs`, `Ft8Slots.cs`, `Ft8Sync.cs`, `RealFft.cs`, `ReusableWindow.cs`,
`SntpClock.cs`, `WavAudio.cs`.

`grep -rn "WaveOut\|WasapiOut\|IWavePlayer\|Play("` over `src` returns four
lines and **all four are in the UI project**:
`src/Hamlet.App/Audio/ModeAudioPlayer.cs:23`, `:92`, `:95` and `:115` - a
`WaveOutEvent` at `:92` playing training tones. That is `Hamlet.App`, which this
unit does not touch, and §0.1 forbids the engine referencing it in any case.

**So the audio sink interface this unit declares is written against nothing.**
There is no existing render seam to reuse and no existing playback abstraction
to widen. That was the assumption the instruction asked to be verified, and it
holds.

---

## 8. What is missing

Named explicitly rather than left as an absence:

1. **A render device, and everything under it.** No WASAPI render class, no
   device enumerator, no output device selection, no rate negotiation with an
   output endpoint. The next unit's.
2. **The level the radio's USB modulation input expects.** Nowhere in this
   repository. `SHACK_FACTS.md` FACT-004 rules that no radio has ever been
   attached to this machine, so it cannot be inferred here. Deferred to the
   operator: Tim sets drive at the radio by watching ALC on the first live
   transmission and writes the figure into `SHACK_FACTS.md`.
3. **A loopback.** Nothing captures Hamlet's own output and decodes it.
4. **A PTT write.** Neither a descriptor in `CivWrites` nor an on-value in
   `CivConstants` (this unit adds the constant, and deliberately not the
   descriptor).
5. **A transmit telemetry category.** The enum has none (this unit adds one).
6. **Anything that starts an FT8 transmission.** `Ft8Composer.Compose` at
   `src/Hamlet.RadioEngine/Transmit/Ft8Composer.cs:212` has no caller outside
   its own tests, and after this unit the sequence built on it will have none
   either. The first real caller is step 5's right-click.
7. **A way to make a fake port fail a write.**
   `tests/Hamlet.RadioEngine.Tests/Rig/FakeSerialPort.cs:97` cannot be scripted
   to throw on `Write`, only on `Open` (`:27`) and to hang on `ReadAsync`
   (`:41`). Proving the unkey survives a dying port needs that, and this unit
   adds it as an opt-in flag rather than writing a second fake.

---

## 9. Step 3's six exit criteria

Quoted from `PHASE_PLAN.md:229-242`. **State column is as at the start of this
unit** - what the unit actually reached is in `output.md` section 2.

| # | Criterion | State at entry | Evidence | Who meets it |
|---|---|---|---|---|
| 1 | Audio plays to the radio's USB input at the right device, rate and level. *must-pass* | **not met** | Nothing in the engine plays audio at all (§7 above). The level figure is not in the repository (§8.2). | **The next unit** for the device; the level is deferred to Tim and no unit closes it. |
| 2 | Key, transmit, unkey, with the unkey happening even if the audio path throws. *must-pass* | **not met** | Nothing in the tree keys on purpose for a digital mode. `TransmitAbort` exists (`TransmitAbort.cs:64`) with no caller (`:53`). | **This unit, task 2.** |
| 3 | A loopback proves the whole chain: generate, play, capture on the tap, decode, get the message back. *must-pass* | **not met** | No playback to capture (§7); nothing joins `AudioTap` to a transmission. | **The next unit.** Deliberately not attempted here. |
| 4 | The transmitted slot is recorded to telemetry with what was sent and when. *must-pass* | **not met** | `TelemetryCategory` has no transmit value (`ITelemetry.cs:8-27`). | **This unit, task 4.** |
| 5 | The licence gate is in the path and refuses out-of-privilege frequencies, asserted by a test. *must-pass* | **not met** | `TransmitGuard.Check` exists (`TransmitGuard.cs:67`) and is called by the CW path only (`CwTransmitter.cs:97`); there is no FT8 send path for it to sit in. | **This unit, task 3.** |
| 6 | Nothing keys without an operator action reaching this code. There is no entry point that transmits on a timer. *must-pass* | **not met, and not vacuously true** | `AutoCaller` (`AutoCall.cs:272`) keys repeatedly from one operator start, on the CW path. For the FT8 path there is nothing to assert against yet. | **This unit, task 2**, for the sequence it builds. `AutoCaller` is parked by this instruction and unchanged. |

**Two of the six are not attempted by this unit and are named as the next
unit's: 1 and 3 - the render device and the loopback.** The reason is FACT-004:
no radio has ever been attached to this machine, so a device opened here
measures this machine's endpoint and says nothing about the radio, while the
shape of a keying path is fully provable against fakes tonight.

---

## 10. `RULES_AT` - five minutes, and nothing changed

The reload reported a disagreement: `PROJECT_STATUS.md:8` says
`RULES_AT: HM-DEC-157 (2026-09-06)` while the reload reported `CLAUDE.md` §1's
highest as `CPS-DEC-0152`.

**What `CLAUDE.md` §1 actually holds:** the decision log is a table starting at
`CLAUDE.md:358`, newest first per the instruction at `:350`. Its top row is
`HM-DEC-152`, dated 2026-08-31 (`CLAUDE.md:360`). Every ref in it is spelled
`HM-DEC-nnn`. **The prefix `CPS-DEC-` appears nowhere in this repository** -
that part of the reload's reading is simply wrong about this project.

The number is right: §1's highest *is* 152. `HM-DEC-155`, `156` and `157` are in
`DECISIONS.md` (at `:88`, `:46` and `:7`) and have not been indexed into
`CLAUDE.md` §1's table. So `RULES_AT` is ahead of the index, not ahead of the
record.

**Nothing changed**, per instruction. It is a pointer, not a rule.
