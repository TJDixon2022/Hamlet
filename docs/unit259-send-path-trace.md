# Unit 259 - what the send path can actually reach

**Six questions, each with a file and a line number.** This is a trace and not a
survey. Read at `HEAD aec0698` on 2026-09-06, from the tree alone; the
application was not run and no device was opened.

---

## 1. Where does the app know the frequency the licence gate must be asked about?

**It is the band's jump frequency until a radio moves it, and it is live and true
only while a rig is connected.**

- `private long _frequencyHz` - `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:447`,
  an `[ObservableProperty]` whose public face is `FrequencyHz`.
- Seeded at construction from the selected band:
  `_frequencyHz = _selectedBand.Band.JumpHz` -
  `MainWindowViewModel.cs:3098`, where `_selectedBand` is
  `settings.LastBand` or `40 m`.
- Moved by the radio: `OnRigFrequencyChanged` at `:9042` posts
  `ApplyRigFrequency(e.FrequencyHz)` onto the UI thread, and `ApplyRigFrequency`
  at `:9057` follows the dial out of the ham bands rather than clamping it
  (HM-DEC-055, quoted in its own remarks at `:9045`-`:9055`).
- The rig that raises that event only exists after `CreateRig` at `:9473`, which
  is reached from the Connect button.

**So: with no radio connected on this machine - `SHACK_FACTS.md` FACT-004 - the
value is the band's jump frequency and nothing else.** It is a real number and a
legal one, but it is *where the band starts*, not where a dial is. Criterion 6 is
asserted against it, and the assertion is therefore about the gate being asked
the app's own frequency, not about the app's frequency being a measurement of a
radio. **Said out loud here so the report does not claim more.**

The same value already feeds the existing CW guard through
`BuildTransmitContext()` at `:2471`, which passes `FrequencyHz` and
`_settings.RestrictTransmitToPrivileges`. **The send path reads the same two
fields from the same places** rather than opening a second route to them.

---

## 2. Can the app hand `Ft8TransmitSequence` an `ISerialPort` today?

**No. There is no accessor and no seam.**

- `private readonly ISerialPort _port;` -
  `src/Hamlet.RadioEngine/Rig/Ic7300Rig.cs:39`. Assigned in the constructor at
  `:57` and exposed nowhere: no property, no method, no internal.
- The app constructs the port and immediately gives it away:
  `new Ic7300Rig(new SystemSerialPort(selection))` -
  `MainWindowViewModel.cs:9476`, inside `CreateRig`. The `SystemSerialPort`
  reference is not kept.
- `private IRig? _rig` at `:217` holds the rig; `IRig` carries no port.

**The smallest seam, named:** `CreateRig` at `:9473` is a static that returns
`IRig`. Making the view model keep the `SystemSerialPort` it already constructs
is two lines - a field, and an assignment in the one place the port is built -
and adds nothing to `Ic7300Rig`. That is smaller than a property on `Ic7300Rig`,
which would have to be justified in a type whose whole remark set is about the
keying write having one use site.

**What this unit does instead, and why.** Even with that seam, on this machine
`_rig` is always null: no radio has ever been attached (FACT-004), so
`SystemSerialPort` would be constructed against a port name that is not there.
**A seam that can only be exercised by opening a real port measures nothing
here.** So:

- the send command takes its `ISerialPort` and its `ITransmitAudioSink` from a
  **test seam** - the same shape as `UseRigForTests(IRig?)` at `:6863` - and the
  tests drive it with the fakes that already exist;
- with no port and no sink, the command **refuses with words** and the Send area
  says so. It never silently does nothing, which is what work instruction 259
  calls the unacceptable landing.
- **Wiring `CreateRig`'s port through to the command is named as what remains**,
  and it is two lines when there is a radio to try it against.

---

## 3. What does the operator's settings supply?

**Four values, and this trace could not read three of them on this machine.**

| Value | Where the send path reads it | Source default |
|---|---|---|
| Callsign | `_settings.Operator.Callsign` | `"KC3QIS"` - `OperatorProfile.cs:44` |
| Grid square | `_settings.Operator.GridSquare` | `""` - `OperatorProfile.cs:92` |
| Licence class | `MainWindowViewModel.LicenseClass` at `:2508`, reading `_settings.Operator.LicenseClass` | `LicenseClass.Unknown` - `OperatorProfile.cs:152` |
| Guard toggle | `_settings.RestrictTransmitToPrivileges` | `true` - `AppSettings.cs:163` |

**THE LIVE VALUES COULD NOT BE READ AND THIS IS A FINDING, NOT AN ANSWER.**
`AppSettings.SettingsPath` is `%AppData%\Hamlet\settings.json`
(`AppSettings.cs:586`, folder at `:579`). Both tools in this session are confined
to `C:\Source\HamLet`:

- the shell returned *"ls in '/c/Users' was blocked. For security, Claude Code
  may only list files in the allowed working directories for this session:
  'C:\Source\HamLet'"*;
- the file reader returned *"C:\Users\TimDi\AppData\Roaming\Hamlet\settings.json
  is outside C:\Source\HamLet; --restricted confines the file tools to the
  working directory."*

**So what is written above is the source's defaults, and the tree is all this
unit can see.** Two of them matter and are handled rather than assumed:

- **`GridSquare` may be empty.** The option list is built so that an empty grid
  removes the grid-bearing messages and the CQ becomes `CQ KC3QIS`, with the
  reason said out loud in the Send area. **Nothing is invented** (§0.0).
- **`LicenseClass` may be `Unknown`.** `Ft8TransmitSequence`'s own gate treats an
  unknown class as a refusal inside the send path
  (`Ft8TransmitSequence.cs:254`-`:263` and the remarks at `:189`), so the
  default state of this machine is *refuse*, which is the correct direction to
  fail in.

---

## 4. Where would the transmit audio endpoint come from?

**Nowhere today. There is no output device field in settings.**

- `AppSettings.AudioInputDeviceId` - `src/Hamlet.App/Settings/AppSettings.cs:193`
  - a `string?` holding the device's own id, with the remark at `:187`-`:192`
  saying why the id and not the name.
- **There is no `AudioOutputDeviceId` and no other output field** in that file.
- `WasapiTransmitSink(string device, int bufferMilliseconds)` -
  `src/Hamlet.RadioEngine/Audio/WasapiTransmitSink.cs:110`. It takes an endpoint
  id **or its exact friendly name**, `ArgumentException.ThrowIfNullOrWhiteSpace`
  at `:112`, and `Find` at `:119` throws where there is no such endpoint
  - its own summary is *"Opens one named render endpoint, or refuses"* at `:103`.
  **It never plays to the machine default**, which is the behaviour the phase
  wants: playing FT8 into laptop speakers because a name was missing is the
  failure this refusal exists to prevent.

**What it would take to name one:** one nullable string on `AppSettings` beside
`AudioInputDeviceId`, in the same shape and with the same id-not-name reasoning.
Task 4 adds it. **A Settings screen to choose it is out of scope and is named as
what remains** - so the field exists and is empty, and an empty field means the
send refuses with words rather than playing anywhere.

**Nothing here enumerates a device.** `MMDeviceEnumerator` is constructed inside
`WasapiTransmitSink`'s constructor at `:115` and this unit never constructs a
`WasapiTransmitSink`.

---

## 5. The four markup facts

All four confirmed, at the stated lines, in
`src/Hamlet.App/Views/MainWindow.axaml`:

| Claim | Line | Verdict |
|---|---|---|
| `Border x:Name="DigitalSendReserved"`, `MinHeight="72"` | `3074` | correct |
| `TextBlock x:Name="DigitalSendReservedLine"` | `3082` | correct |
| `ItemsControl x:Name="DigitalDecodedRows"`, bound to `DigitalVisibleDecodes` | `3388` | correct |
| the row template's root `Grid ColumnDefinitions="76,48,48,54,*,148"` | `3407` | correct |

The reserved line's text at `3083` reads, in full:

> Send lives here when it is built. Hamlet does not transmit yet, and this space
> is kept clear so the waterfall above it never has to move again.

**That sentence becomes false in task 4 and does not survive.** The comment above
the border at `:3067`-`:3073` says there is deliberately no control in it because
a greyed control claims a feature exists and is unavailable; the feature now
exists.

---

## 6. THE PREDICTED MENUS, WRITTEN DOWN BEFORE THE OPTION LIST EXISTS

**Read at the boundary of slot 13 - `2026-09-06T18:03:15Z` - the moment unit 258
used.** The ledger is built from
`tests/fixtures/ft8/scenes/unit257-band-scene.corpus.txt` the way
`TheRowSaysOneOfFourThingsTests` builds it: a line whose sender is the operator
goes to `RecordSent`, every other line goes to `RecordHeard`.

**Two figures are supplied to the list rather than invented by it**, and they are
stated here so the prediction is checkable:

- **operator grid `FN00`** - what `OperatorProfile.GridSquare` would hold if it
  were set. The empty-grid case is predicted separately below.
- **report `-10` dB** - the signal report to send. **A report is a measurement of
  a received signal, not something a message list may make up** (§0.0), so it
  arrives as a parameter from the decoded row the operator right-clicked, and
  where none is known the report-bearing options are absent for the same reason
  the grid ones are. `-10` is used uniformly here so the predicted texts are
  exact.

**The rule for *expected*, stated before it is built:** the expected next message
is what conventionally answers **the last message heard from that station
addressed to the operator**, and nothing else. Grid answered by a report; a plain
report answered by a roger-and-report; a rogered report answered by an
acknowledgement; an acknowledgement answered by `73`; a station heard only
calling anyone answered by the grid. Where nothing has been heard addressed to
the operator, **none** is expected. That is field-shape arithmetic and it says
nothing about what anybody meant (§12.1).

**The repeat count is an exact-text match** against what the ledger holds in
`Sent` for that station.

**Nothing is ever absent on account of the contact state.** All five stations get
all five messages. Absence below is only ever the empty-grid case.

### G4XYZ - ledger reads *waiting on him, 2 slots*

Last heard to us: `KC3QIS G4XYZ IO91` (slot 2, a grid). Sent:
`G4XYZ KC3QIS -14` (slot 11).

| Text | Label | Expected | Sent before |
|---|---|---|---|
| `G4XYZ KC3QIS FN00` | grid | | 0 |
| `G4XYZ KC3QIS -10` | report | **yes** | 0 |
| `G4XYZ KC3QIS R-10` | roger and report | | 0 |
| `G4XYZ KC3QIS RRR` | acknowledge | | 0 |
| `G4XYZ KC3QIS 73` | 73 | | 0 |

### VK2PQ - ledger reads *gone quiet, 11 slots*

Last heard to us: `KC3QIS VK2PQ QF56` (slot 2, a grid). Sent: nothing.

| Text | Label | Expected | Sent before |
|---|---|---|---|
| `VK2PQ KC3QIS FN00` | grid | | 0 |
| `VK2PQ KC3QIS -10` | report | **yes** | 0 |
| `VK2PQ KC3QIS R-10` | roger and report | | 0 |
| `VK2PQ KC3QIS RRR` | acknowledge | | 0 |
| `VK2PQ KC3QIS 73` | 73 | | 0 |

**A gone-quiet station offers everything.** Ruled 2026-09-06.

### K9RST - ledger reads *complete, 7 slots*

Last heard to us: `KC3QIS K9RST 73` (slot 6, an acknowledgement). Sent:
`K9RST KC3QIS -13` (slot 3), `K9RST KC3QIS RRR` (slot 5).

| Text | Label | Expected | Sent before |
|---|---|---|---|
| `K9RST KC3QIS FN00` | grid | | 0 |
| `K9RST KC3QIS -10` | report | | 0 |
| `K9RST KC3QIS R-10` | roger and report | | 0 |
| `K9RST KC3QIS RRR` | acknowledge | | **1** - *acknowledge, 2nd time* |
| `K9RST KC3QIS 73` | 73 | **yes** | 0 |

**THIS IS THE COMPLETE STATION AND IT LOSES NOTHING.** All five messages, `73`
among them, and the grid a second time for an operator whose first was lost. **A
contact is never closed by the app.**

### W1ABC - ledger reads *complete, 4 slots*

Last heard to us: `KC3QIS W1ABC R-15` (slot 8, a rogered report). Sent:
`W1ABC KC3QIS -12` (slot 7), `W1ABC KC3QIS RRR` (slot 9).

| Text | Label | Expected | Sent before |
|---|---|---|---|
| `W1ABC KC3QIS FN00` | grid | | 0 |
| `W1ABC KC3QIS -10` | report | | 0 |
| `W1ABC KC3QIS R-10` | roger and report | | 0 |
| `W1ABC KC3QIS RRR` | acknowledge | **yes** | **1** - *acknowledge, 2nd time* |
| `W1ABC KC3QIS 73` | 73 | | 0 |

**The expected one is also a repeat, and both are shown.** That is the ruling
working: FT8 loses transmissions constantly and sending `RRR` again is correct.

### N5TT - ledger reads *gone quiet, 5 slots*

Last heard to us: `KC3QIS N5TT EM10` (slot 8, a grid). Sent: nothing.

| Text | Label | Expected | Sent before |
|---|---|---|---|
| `N5TT KC3QIS FN00` | grid | | 0 |
| `N5TT KC3QIS -10` | report | **yes** | 0 |
| `N5TT KC3QIS R-10` | roger and report | | 0 |
| `N5TT KC3QIS RRR` | acknowledge | | 0 |
| `N5TT KC3QIS 73` | 73 | | 0 |

### The CQ, which has no station record

- with a grid: `CQ KC3QIS FN00`
- **with `GridSquare` empty: `CQ KC3QIS`** - a legal FT8 message - and the Send
  area says the grid is unset. **`FN00` is Tim's own square and is never a
  fallback** (§0.0).

### With `GridSquare` empty, for every station

The grid row is **absent** from all five tables above and the other four remain,
with the same expected and the same counts. **Absent, not greyed** - there is no
message to send, rather than a message withheld.

### The two messages the splitter refuses

`TNX BOB 73 GL` (slot 4) and `ABCDEFGHIJKLM` (slot 10) are not booked by the
ledger at all (`Ft8ContactLedger.cs:200`), so no station record exists for them
and **the option list is never asked about them**. It must not crash on a record
whose `Fields` are null, and that is asserted.

---

## What this trace could not answer

**One thing, and it is question 3's live values.** The settings file is outside
the working directory and both tools refused it, quoted above. The trace reports
the source defaults and the send path is built so that the two values that could
be missing - the grid and the licence class - **fail toward saying so and not
toward inventing one.**
