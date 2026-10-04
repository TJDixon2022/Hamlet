# Unit 260 - the two routes, measured

**Work instruction 260, task 1.** Five questions, every answer with a file and a
line. **This is a trace, not a survey.** Read out of the tree at `HEAD 2f68c40`
on 2026-09-06, before anything in this unit was written.

The two routes are the mouse's - from a right-click on a decoded row to
`SendMenuFor` - and the radio's - from `SendMessage` to a serial port and an
audio endpoint. Neither exists today. This says exactly where each one stops.

---

## 1. Where does a right-click on a decoded row arrive today?

**Nowhere. Nothing handles it.**

The row that would receive it is the `DataTemplate`'s root `Grid` at
`src/Hamlet.App/Views/MainWindow.axaml:3444`:

```
<Grid ColumnDefinitions="76,48,48,54,*,148">
```

inside `<DataTemplate x:DataType="vm:DigitalDecodeRow">` at `:3429`, inside
`<ItemsControl x:Name="DigitalDecodedRows" ItemsSource="{Binding DigitalVisibleDecodes}">`
at `:3425`-`:3427`. That `Grid` fills the row and is what the pointer is over
when the operator right-clicks a decode.

**The grep, over source only** - `bin/` and `obj/` carry Avalonia's own
assemblies and their XML docs and are not the tree:

```
$ grep -rn "ContextFlyout\|ContextRequested\|PointerReleased\|RightButton\|ContextMenu" src/
src/Hamlet.App/Controls/DialTapeControl.cs:342:    protected override void OnPointerReleased(PointerReleasedEventArgs e)
src/Hamlet.App/Controls/DialTapeControl.cs:344:        base.OnPointerReleased(e);
src/Hamlet.App/Controls/NeighborhoodMapControl.cs:541:    protected override void OnPointerReleased(PointerReleasedEventArgs e)
src/Hamlet.App/Controls/NeighborhoodMapControl.cs:543:        base.OnPointerReleased(e);
```

**Four hits, none of them a context request and none of them on a decoded row.**
`DialTapeControl` and `NeighborhoodMapControl` are the dial tape and the band map;
both override `OnPointerReleased` for a drag they own. **`ContextFlyout`,
`ContextRequested`, `ContextMenu` and `RightButton` appear zero times in `src/`.**

*Mismatch with the instruction, reported and not repaired:* the instruction's
table says `grep -rn ContextFlyout src/` returns nothing. Run as written it
returns 40-odd binary and XML-doc matches out of `src/Hamlet.App/bin/` and
`src/Hamlet.App/obj/`, which are build output. **The claim is right about the
tree and the grep needs `--include=*.cs --include=*.axaml` to show it.** The
tree wins and the tree agrees.

`MainWindow.axaml.cs` is **114 lines** and handles `KeyDownEvent` (`:20`),
`PropertyChanged` (`:25`), `Opened` (`:26`) and `Closing` (`:36`). **No pointer
handler of any kind.**

**So: a right-click on a decoded row is delivered to Avalonia, finds no
`ContextFlyout` and no `ContextRequested` handler anywhere up the tree, and is
discarded.** No operator has ever reached `SendMenuFor` with a mouse.

---

## 2. What does the row's `DataContext` carry at the moment of a right-click?

**It carries a `DigitalDecodeRow`, and it is enough.**

`SendMenuFor(DigitalDecodeRow? row)` at
`src/Hamlet.App/ViewModels/MainWindowViewModel.cs:7934` reads exactly two members
of the row:

| Member | Where declared | Read at |
|---|---|---|
| `Sender` | `DigitalDecodeRow.cs:145` - `Fields?.From ?? ""` | `MainWindowViewModel.cs:7941`, `_contacts.For(row.Sender)` |
| `Snr` | `DigitalDecodeRow.cs:86`, the record's own positional parameter | `MainWindowViewModel.cs:7955`, through `MeasuredReport(row)` at `:7953` |

`MeasuredReport` at `:7953`-`:7958` is `int.TryParse(row.Snr, ...)`, returning
null where the cell is `DigitalDecodeRow.NoMeasurement` (`DigitalDecodeRow.cs:97`).
Everything else `SendMenuFor` needs - the ledger, the operator's callsign, the
grid - it takes off the view model itself at `:7936`, `:7947` and `:7948`.
**Nothing is needed from the row that the row does not already hold.**

**Is the bound object the same instance the view model holds? Yes.**
`DigitalVisibleDecodes` is `ObservableCollection<DigitalDecodeRow>` at
`MainWindowViewModel.cs:1078`; the `ItemsControl` binds straight to it at
`MainWindow.axaml:3427`; Avalonia sets each container's `DataContext` to the item
itself. The rows are inserted, removed and cleared as objects at `:1406`,
`:1418`, `:1468` and `:1474` - **there is no projection, no wrapper and no copy
between the collection and the template.**

**So the handler can take `(sender as Control)?.DataContext as DigitalDecodeRow`
and pass it straight to `SendMenuFor`.** No lookup by index, no search by text.

**And the count belongs to the click, not to the row.** `SendMenuFor` calls
`_contacts.For(row.Sender)` at `:7941` **at the moment it is called**, and
`Ft8SendOptions.For` reads that record's `Sent` list for `SentBefore`. A menu
built when the row was created would freeze the counts at creation; a menu built
in the handler carries them as of the click. **That is the whole difference task
2 turns on.**

---

## 3. What is the smallest seam that keeps the `SystemSerialPort`?

`CreateRig` at `MainWindowViewModel.cs:9827`-`:9830`, entire:

```csharp
private static IRig CreateRig(string selection)
    => selection == TrainingRadio
        ? new TrainingRig()
        : new Ic7300Rig(new SystemSerialPort(selection));
```

**The `SystemSerialPort` is constructed inside the expression and no reference to
it survives it.** `Ic7300Rig` keeps it in a private field at `Ic7300Rig.cs:39` and
exposes no accessor.

`ConnectToAsync` at `:6499` calls `CreateRig(port)` at `:6501`, connects at
`:6505`, and on success assigns `_rig = rig` at `:6533`. `_rig` is declared at
`:218` and nulled in `TearDownRigAsync`'s `finally` at `:9821`.

**The smallest seam, and it is three edits and no new type:**

1. Change `CreateRig`'s return to carry the port beside the rig - a tuple
   `(IRig Rig, ISerialPort? Port)`, `null` for the training entry. **Two lines
   changed at `:9827`-`:9830`.**
2. Add `private ISerialPort? _rigPort;` beside `_rig` at `:218`, and set it in
   `ConnectToAsync` **in the same statement region that sets `_rig` at `:6533`**.
3. Null it in the same `finally` that nulls `_rig` at `:9821`. **One line.**

**Confirms the instruction and unit 259's trace: the field beside `_rig` is the
smaller seam.** Adding an accessor to `Ic7300Rig` would change a rig class to
serve one caller and would hand the port out to anything holding the rig; a field
in the view model is visible to the send path and to nothing else.

**The training-radio route has no serial port at all.** `TrainingRig` is
constructed with no argument at `:9829` - it is a simulator. So on that route the
seam **sets nothing**: `_rigPort` stays null, `_armedSend` is never built, and a
click lands on the refusal. **The training radio must never become a route to a
keying frame**, and the way it cannot is that it has nothing to key with.

The failure path already disposes the rig without assigning `_rig` at `:6507`;
**the port must be left unassigned on that path too**, which it is, because the
assignment lives beside `_rig`'s at `:6533` and that line is past the early
return at `:6519`.

---

## 4. What would the sink be constructed from, and where?

**From `AppSettings.AudioOutputDeviceId`**, `src/Hamlet.App/Settings/AppSettings.cs:215`:

```csharp
public string? AudioOutputDeviceId { get; set; }
```

**Nothing in `src/` reads it.** Its remarks at `:200`-`:213` say why it exists and
that it has no reader yet.

`WasapiTransmitSink`'s constructor is
`src/Hamlet.RadioEngine/Audio/WasapiTransmitSink.cs:110`:

```csharp
public WasapiTransmitSink(string device, int bufferMilliseconds = DefaultBufferMilliseconds)
```

- `:112` - `ArgumentException.ThrowIfNullOrWhiteSpace(device)`. **Null or empty
  throws `ArgumentException`.**
- `:119` - `_device = Find(_enumerator, device)` inside a `try` that disposes the
  enumerator and **rethrows**. An id or friendly name that matches no active
  render endpoint throws `InvalidOperationException`.
- `:151` - an endpoint whose mix format this cannot write throws
  `InvalidOperationException` too, with its own words.

**It never plays to the machine default, on purpose** (`AppSettings.cs:200`-`:206`).

**Where it would be constructed: in the view model, at the moment the rig
connects**, beside where `_rigPort` is set - because that is the one moment both
halves are known. **Through a factory**, `Func<string, ITransmitAudioSink>`
defaulting to `name => new WasapiTransmitSink(name)`, so a test can substitute one
and **no test in this unit constructs a real sink** (FACT-004).

**What a wrong or stale name does to an operator who clicks, said plainly.**
`AudioOutputDeviceId` stores a device *id*. Ids survive driver updates; they do
not survive the radio being unplugged from a different USB socket, or the machine
enumerating a device it has never seen. **If the name is stale, the constructor
throws** - at `:119`, `InvalidOperationException`. Constructed inside a click
handler, that exception would reach the operator as an unhandled fault in the UI
thread while he is trying to answer a CQ.

**So it is constructed at connect time, not at click time, and it is caught.**
Where the constructor throws, `_armedSend` stays null, the exception is not
rethrown, and the Send area says the named device was not found. **The click then
refuses with words, which is the behaviour the refusal at `:8009` already has and
which this widens only enough to say which of the two things is missing.**

---

## 5. Every place a transmission can begin, today. THE BASELINE.

```
$ grep -rn "Arm(\|_armedSend\|Ft8TransmitSequence\|Ft8ArmedSend" src/ --include=*.cs --include=*.axaml
src/Hamlet.App/ViewModels/AutoCallViewModel.cs:261:    private void Arm()
src/Hamlet.App/ViewModels/MainWindowViewModel.cs:7896:    private Ft8ArmedSend? _armedSend;
src/Hamlet.App/ViewModels/MainWindowViewModel.cs:7971:    /// <see cref="Ft8ArmedSend.Arm"/> is called from this method and from no
src/Hamlet.App/ViewModels/MainWindowViewModel.cs:7974:    /// <see cref="Ft8ArmedSend"/> holds one field, so two clicks a second apart
src/Hamlet.App/ViewModels/MainWindowViewModel.cs:8004:        if (_armedSend is null)
src/Hamlet.App/ViewModels/MainWindowViewModel.cs:8017:        _armedSend.Arm(new OperatorSend(
src/Hamlet.App/ViewModels/MainWindowViewModel.cs:8045:        if (_armedSend is null || !_armedSend.IsArmed)
src/Hamlet.App/ViewModels/MainWindowViewModel.cs:8075:        if (_armedSend is null)
src/Hamlet.App/ViewModels/MainWindowViewModel.cs:8081:        var result = await _armedSend.AtBoundaryAsync(boundaryUtc).ConfigureAwait(false);
src/Hamlet.App/ViewModels/MainWindowViewModel.cs:8149:    /// inside <see cref="Ft8TransmitSequence.RunAsync"/>, before anything that can
src/Hamlet.App/ViewModels/MainWindowViewModel.cs:8224:    /// uses. It hands over an already-built <see cref="Ft8ArmedSend"/> and so
src/Hamlet.App/ViewModels/MainWindowViewModel.cs:8228:    internal void UseArmedSendForTests(Ft8ArmedSend? armed) => _armedSend = armed;
src/Hamlet.RadioEngine/Audio/WasapiTransmitSink.cs:43:/// <see cref="Ft8TransmitSequence"/>, where coming back out of transmit is
src/Hamlet.RadioEngine/Civ/CivConstants.cs:110:    /// <c>Ft8TransmitSequence</c> writes it, inside a <c>try</c> whose
src/Hamlet.RadioEngine/Transmit/Ft8ArmedSend.cs:49:/// <see cref="Ft8TransmitSequence.RunAsync"/> inside
src/Hamlet.RadioEngine/Transmit/Ft8ArmedSend.cs:61:public sealed class Ft8ArmedSend
src/Hamlet.RadioEngine/Transmit/Ft8ArmedSend.cs:63:    private readonly Ft8TransmitSequence _sequence;
src/Hamlet.RadioEngine/Transmit/Ft8ArmedSend.cs:71:    public Ft8ArmedSend(Ft8TransmitSequence sequence)
src/Hamlet.RadioEngine/Transmit/Ft8Composer.cs:252:    /// and it is stated to <c>Ft8TransmitSequence</c> as a figure, not built into
src/Hamlet.RadioEngine/Transmit/Ft8TransmitSequence.cs:68:/// <see cref="Ft8TransmitSequence"/> has no method that starts a transmission
src/Hamlet.RadioEngine/Transmit/Ft8TransmitSequence.cs:82:/// this path, false is a refusal** - see <see cref="Ft8TransmitSequence"/>.
src/Hamlet.RadioEngine/Transmit/Ft8ArmedSend.cs:93:    public void Arm(OperatorSend send)
src/Hamlet.RadioEngine/Transmit/ITransmitAudioSink.cs:34:/// is <see cref="Ft8TransmitSequence"/>, where the unkey is guaranteed.</para>
```

**Sorted into what each hit is:**

| Line | What it is |
|---|---|
| `AutoCallViewModel.cs:261` | **A declaration, and a different `Arm`.** `private void Arm()` on the parked CW auto-call path. It takes no `OperatorSend` and has nothing to do with FT8. Not a call site of `Ft8ArmedSend.Arm`. |
| `MainWindowViewModel.cs:7896` | Declaration - the field. |
| `MainWindowViewModel.cs:7971`, `:7974`, `:8149`, `:8224` | Documentation comments. Not code. |
| `MainWindowViewModel.cs:8004`, `:8045`, `:8075` | **Null reads.** They test the field; none of them arms. |
| **`MainWindowViewModel.cs:8017`** | **THE ONE ARMING CALL SITE.** `_armedSend.Arm(new OperatorSend(...))`, inside `SendMessage(string?)` at `:7982`. |
| `MainWindowViewModel.cs:8081` | `AtBoundaryAsync` - **fires what is already armed and cannot arm anything.** |
| `MainWindowViewModel.cs:8228` | **The only assignment of `_armedSend` in the tree**, `UseArmedSendForTests`. |
| `Ft8ArmedSend.cs:61`, `:63`, `:71`, `:93` | Declarations - the class, its field, its constructor, and `Arm` itself. |
| `Ft8TransmitSequence.cs:197`, `:216` | Declarations - the class and its constructor. `RunAsync` at `:249` does not match this grep and is called from `Ft8ArmedSend.AtBoundaryAsync` alone. |
| `Ft8TransmitSequence.cs:68`, `:82` | Documentation comments. `:68` is the class's own remark that it *"has no method that starts a transmission"*. |
| everything in `WasapiTransmitSink.cs`, `CivConstants.cs`, `Ft8Composer.cs`, `ITransmitAudioSink.cs` | Documentation comments naming the type. |

**`new Ft8ArmedSend` appears nowhere in `src/`.** `new Ft8TransmitSequence`
appears nowhere in `src/` either - `Ft8TransmitSequence.cs:193` says so in its own
remarks: *"Nothing in this repository calls it."*

**The baseline, in one sentence.** **One arming call site: `MainWindowViewModel.cs:8017`,
inside `SendMessage`. One assignment of `_armedSend`: the test seam at `:8228`.**
After task 3 there must still be one arming call site and it must still be
`SendMessage`; the assignment count may go from one to two, and **the second must
be the connect path and nothing else.**

---

## What this trace could not answer

**Whether headless Avalonia on this machine can raise a `ContextRequested` that
reaches a handler.** Ten test files use `[AvaloniaFact]` and
`tests/Hamlet.App.Tests/Views/BindingHealthTests.cs:9`-`:21` sets up
`HeadlessApp`, so a control tree can be built. Whether the headless platform
delivers a context request through `RaiseEvent` is measured in task 2, not here.

**Nothing was opened to write this.** No serial port, no audio device, no window.
