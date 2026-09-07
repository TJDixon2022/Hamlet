# Unit 267, task 1 - the step B trace

**Measured before anything was built.** Seven questions, each answered with a file,
a line and a quotation read off the tree on 2026-09-07, root version `1.12.120`.

Everything below is a reading of the tree as it stands. Where it disagrees with
work instruction 267's own *Verify this instruction against the tree* list, the
disagreement is named in the last section and carried into `output.md` section 4.

---

## 1. Criterion 1 - the CQ button

**The control.** `src/Hamlet.App/Views/MainWindow.axaml:3097`:

```xml
<Button x:Name="DigitalSendCqButton"
        Content="CQ"
        Padding="16,4"
        Command="{Binding SendCallToAnyoneCommand}"
        ToolTip.Tip="{Binding CallToAnyoneText}" />
```

**The command.** `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:8531-8532`:

```csharp
[RelayCommand]
private void SendCallToAnyone() => SendMessage(CallToAnyoneText);
```

It adds nothing. `SendMessage` at `:8169` is the one entry point that arms, and the
CQ button and the right-click menu are the same call - the remark at `:8527` says
so: *"It calls SendMessage and adds nothing: **one send path, not two**"*.

**The text it builds from Settings.** `MainWindowViewModel.cs:8508-8510`:

```csharp
public string CallToAnyoneText
    => Ft8SendOptions.CallToAnyone(
        _settings.Operator.Callsign?.Trim() ?? "", _settings.Operator.GridSquare);
```

- **With a grid set** (`FN00`): `CQ KC3QIS FN00`.
- **With none**: `CQ KC3QIS`. No locator is invented (§0.0). The doc comment at
  `:8505` states it: *"with no grid set it is `CQ KC3QIS`, which is a legal FT8
  message, and **no locator is invented**"*.

**The committed test.**
`tests/Hamlet.App.Tests/ViewModels/OneClickArmsExactlyOneMessageTests.cs:178`,
`TheCqButtonSendsFromSettingsWithNoTypingAndNeverInventsAGrid`, and it asserts both
halves and the route:

```csharp
Assert.Equal("", settings.Operator.GridSquare);
Assert.Equal("CQ KC3QIS", panel.CallToAnyoneText);

settings.Operator.GridSquare = "FN00";

Assert.Equal("CQ KC3QIS FN00", panel.CallToAnyoneText);

// AND IT GOES THROUGH THE SAME COMMAND. One send path, not two.
panel.SendCallToAnyoneCommand.Execute(null);
```

---

## 2. Criterion 2 - where the menu items and the header text are built

**The items.** `MainWindowViewModel.SendMenuFor` at `:8121`, which hands
`Ft8SendOptions.For` what it needs and returns what it says:

```csharp
return record is null
    ? null
    : Ft8SendOptions.For(
        record,
        _settings.Operator.Callsign?.Trim() ?? "",
        _settings.Operator.GridSquare,
        MeasuredReport(row));
```

The options themselves are built in
`src/Hamlet.RadioEngine/Contacts/Ft8SendOptions.cs:99`, walking `InExchangeOrder`
at `:83` - grid, report, roger and report, acknowledge, 73 - and adding one option
per shape at `:121`.

**The flyout.** `src/Hamlet.App/Views/MainWindow.axaml.cs:171`, `SendFlyoutFor`,
raised from `OnDecodedRowContextRequested` at `:125` and shown at the pointer at
`:148` (`flyout.ShowAt(control, showAtPointer: true)`).

**The header text, with its highlight and its repeat count.** `MainWindow.axaml.cs:217`:

```csharp
private static string HeaderFor(Ft8SendOption option)
{
    var said = option.Text + "   " + option.Label;

    if (option.SentBefore > 0)
    {
        said += ", " + Ordinal(option.SentBefore + 1) + " time";
    }

    return option.IsExpected ? said + " - the one that comes next" : said;
}
```

`Ordinal` is at `:230`.

**Which test asserts the header strings, and at which lines.**
`tests/Hamlet.App.Tests/Views/TheMenuIsUnderTheMouseTests.cs:114`,
`EveryStationsPredictedMenuAppearsUnderTheMouse`, holds all five stations' headers
verbatim at **lines 118-160** and compares them to what a real `ContextRequested`
on a real row control produced, at `:180`:

```csharp
Assert.Equal(expected, headers);
```

Two of the quoted lines carry both marks:

```
"K9RST KC3QIS RRR   acknowledge, 2nd time",              // :141 - a repeat count
"W1ABC KC3QIS RRR   acknowledge, 2nd time - the one that comes next",  // :149 - both
```

The repeat count has its own test at `:211`,
`TheRepeatCountBelongsToTheClickAndNotToTheRow`, asserting the move from
`"VK2PQ KC3QIS 73   73"` (`:220`) to `"VK2PQ KC3QIS 73   73, 2nd time"` (`:231`)
after a send.

---

## 3. Criterion 2, the second half - is anything filtered between `Ft8SendOptions.For` and the flyout?

**None.** How it was looked for, and what was found instead:

1. **Inside `Ft8SendOptions.For`** - every `if` in the file was read
   (`Ft8SendOptions.cs`, lines 116, 126, 133, 181, 188, 193). There is no `Where`,
   no `OrderBy`, no `Remove` and no `Skip` anywhere in it. The one `continue`, at
   `:118`, fires where `TextFor` returned **null** - a message that does not exist
   because Settings has no grid or the row has no measured ratio - and the reason
   is then said out loud in `Absent` at `:126-138`. That is absent, not withheld.
2. **In `SendMenuFor`** (`MainWindowViewModel.cs:8121-8137`) - the body is a null
   check, a ledger lookup and the call. Its own remark at `:8111` says
   *"This hands `Ft8SendOptions` what it needs and returns what it says; there is
   no filtering step between the two, on the contact state or on anything else."*
3. **In `SendFlyoutFor`** (`MainWindow.axaml.cs:182-192`) - a `foreach` over
   `menu.Options` with no condition in it, and every item gets
   `Command = vm!.SendMessageCommand`. **No `IsEnabled`, no `IsVisible`, no sort.**
   Grepping `IsEnabled|IsVisible|Where(|OrderBy|Filter` across the whole
   code-behind returns two hits and neither is on a menu item: `:69` and `:79`, the
   window-state property handler.
4. **The two things added after the options** are notes, not options:
   `menu.Absent` at `:194-197` and the licence line at `:199-202`, both through
   `Note` at `:245`, which is `new() { Header = text, IsHitTestVisible = false }` -
   carries no command and cannot be hit.
5. **Asserted, not only read**: `TheMenuIsUnderTheMouseTests.cs:183`,
   `Assert.All(Options(flyout!), item => Assert.True(item.IsEnabled));`

---

## 4. Criterion 3 - what asserts that nothing further transmits without another click

**`OneClickIsOneMessageAcrossTwoBoundaries`,
`tests/Hamlet.App.Tests/ViewModels/TheSendPathReachesARealRadioTests.cs:220`.**

**At what level: through the application.** It builds a real `MainWindowViewModel`,
calls `panel.BuildTheArmedSend(port)` so the `Ft8ArmedSend` is the one the
application's own wiring made rather than one a test handed over, clicks through
`panel.SendMessageCommand`, and drives the boundaries through
`panel.AtSlotBoundaryAsync`. The port is `FakePort`; the sink arrives through the
substituted factory. Nothing is opened.

**And it drives two boundaries** - `:234-235`:

```csharp
var first = await panel.AtSlotBoundaryAsync(slot!.Value);
var second = await panel.AtSlotBoundaryAsync(slot.Value.AddSeconds(15));
...
Assert.Equal(Ft8ArmOutcome.Ran, first.Outcome);
Assert.Equal(Ft8ArmOutcome.NothingArmed, second.Outcome);

// **TWO BOUNDARIES, ONE TRANSMISSION.**
Assert.Equal(1, factory.Sink.TimesCalled);
Assert.Equal(2, port.Written.Count);
```

The engine carries the same property one level down:
`tests/Hamlet.RadioEngine.Tests/Transmit/OneClickSendsExactlyOneMessageTests.cs:52`,
`TwoBoundariesAfterOneClickProduceOneTransmission`.

---

## 5. Criterion 4 - the Send area

**Where the line is built.** `MainWindowViewModel.DigitalSendLine`, written from
five places on the send path: `:8175` (nothing to send), `:8196` (compose refused),
`:8210` (nothing to transmit through), `:8229` (armed), `:8291`/`:8309` (after the
boundary) and `:8566` (the stop).

**While armed** - `SendingLine` at `:8447`:

```csharp
private static string SendingLine(string text, DateTime slotStartUtc)
    => "Sending " + Addressed(text) + "\"" + text + "\" in the slot at "
        + slotStartUtc.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + " UTC.";
```

**After a boundary** - `WentLine` at `:8331`, whose sent branch is `:8357`:

```csharp
return "Sent " + Addressed(text) + "\"" + text + "\" in the slot at "
    + slot + " UTC. " + LevelLine(result.Send.Transmission);
```

**Does it name the addressee: yes.** `Addressed` at `:8452`:

```csharp
private static string Addressed(string text)
{
    var fields = Ft8MessageSplit.Split(text);

    return fields is null || Ft8MessageSplit.IsCallToAnyone(fields.To)
        ? ""
        : "to " + fields.To + ", ";
}
```

So a directed message reads *Sending to W1ABC, "W1ABC KC3QIS -10" in the slot at
...* and a CQ carries no addressee, because there is none.

**Which binding puts it under the waterfall.** `MainWindow.axaml:3128-3133`:

```xml
<TextBlock x:Name="DigitalSendReservedLine"
           Text="{Binding DigitalSendLine}"
           TextWrapping="Wrap"
           VerticalAlignment="Top"
           FontSize="11"
           Foreground="{StaticResource HmTextMutedBrush}" />
```

with two more beside it in the same `StackPanel`: the licence line at `:3137-3142`
(`Text="{Binding DigitalSendLicenceLine}"`, `IsVisible="{Binding
HasDigitalSendLicenceLine}"`) and the unset-grid note at `:3144-3149`
(`Text="{Binding DigitalSendUnset}"`).

---

## 6. Criterion 4's second half - **the question this unit exists for**

### Is there any test in which an out-of-privileges click is driven through the application to a slot boundary?

**None.**

How it was looked for: `RefusedByLicence` was grepped across `tests/` and `src/`.
Every hit in a source file is one of two products - `Ft8TransmitSequence.cs` and
`MainWindowViewModel.cs` - and **every hit in a test file is in
`tests/Hamlet.RadioEngine.Tests/`**:

- `tests/Hamlet.RadioEngine.Tests/Transmit/OneClickSendsExactlyOneMessageTests.cs:201`,
  `OutOfPrivilegesNothingReachesThePortOrTheSink` - the engine's own proof. It
  **constructs the armed send itself** (`armed.Arm(SendAt(Boundary,
  licenseClass: LicenseClass.Technician))`, `:207`) and never touches a panel, a
  setting or a click.
- `tests/Hamlet.RadioEngine.Tests/Transmit/TheLicenceGateIsInsideThePathTests.cs` -
  the same seam, one level lower.

`RefusedByLicence` appears **nowhere in `tests/Hamlet.App.Tests/`.** The three
app-side tests that touch the licence at all read a *display* property and never
send:

- `OneClickArmsExactlyOneMessageTests.cs:226`,
  `TheLicenceLineIsTheGuardsOwnWordsAndTheMenuStillListsEverything` - asserts
  `panel.HasDigitalSendLicenceLine` and `Assert.Equal(5, panel.SendMenuFor(row)!.Options.Count)`.
  It never clicks and never reaches a boundary.
- `OneClickArmsExactlyOneMessageTests.cs:262`, `AClassThatMayTransmitHereGetsNoLicenceLine`
  - the same, inverted.
- `tests/Hamlet.App.Tests/Views/TheMenuIsUnderTheMouseTests.cs:361`,
  `OutOfPrivilegesTheMenuSaysSoAndForbidsNothing` - reads the flyout's note.

**So the instruction is right and task 2 is not smaller than it thought.** The gate
is proved inside `Ft8TransmitSequence.RunAsync` on a sequence a test built, and the
menu is proved to say so on a panel nobody clicked. **Nothing joins them.**

### Is `MainWindowViewModel.cs:8346`'s `RefusedByLicence` branch asserted by anything?

**No.** The branch is:

```csharp
if (run.Outcome == Ft8TransmitOutcome.RefusedByLicence)
{
    return "Hamlet did not send \"" + text + "\": " + run.Reason
        + " (" + run.Citation + ")";
}
```

Grepping `tests/Hamlet.App.Tests/` for `Citation` returns three hits, all in
`Licensing/OutOfBandTests.cs` (`:51`, `:68`) and `Licensing/PrivilegeStatusLineTests.cs`
(`:131`), which assert on `TransmitDecision` and a status line and never on
`DigitalSendLine`. **The branch is uncovered - confirmed, not assumed.**

---

## 7. What task 2 will reuse

From `tests/Hamlet.App.Tests/ViewModels/TheSendPathReachesARealRadioTests.cs`:

| Piece | Where | What it does |
|---|---|---|
| The panel builder | `:320-342`, `private static (MainWindowViewModel, AppSettings, RecordingSinkFactory) Panel()` | Sets callsign `KC3QIS`, grid `FN00`, `LicenseClass.General`; **selects the 20 m band first and then sets `FrequencyHz = 14_074_000`**, because `OnFrequencyHzChanged` clamps to the selected band's map window; installs the recording factory |
| The fake port | `tests/Hamlet.App.Tests/FakeTransmitParts.cs:17`, `FakePort : ISerialPort` | Opens nothing. `Written` (`:22`) is every frame in order; `WasNeverWrittenTo` (`:25`) is the zero-bytes assertion |
| The substituted sink factory | `:352-370`, `RecordingSinkFactory`, installed by `panel.TransmitSinkFactory = factory.Make` at `:339` | `Calls` is every endpoint name it was asked for; `Throws` makes the stale-endpoint case |
| The sink's call count | `FakeTransmitParts.cs:77`, `FakeSink.TimesCalled`; also `WasNeverTouched` at `:112` | How many times something asked it to play. Opens no device and makes no sound |
| `AtSlotBoundaryAsync` | `MainWindowViewModel.cs:8277`, driven as at `:186` and `:234` | Hands the armed send its boundary and returns `Ft8BoundaryResult?` |
| The posted line | `TheApplicationSendsAtTheLevelTheOperatorSetTests.cs:221`, `Dispatcher.UIThread.RunJobs()` | **Required.** `AtSlotBoundaryAsync` posts its sentence at `:8309`, and nothing pumps that queue in a test process |

### What a licence refusal comes back as, at each of the two levels

**This is the trap, and it is written down here so task 2 does not walk into it.**

| Level | Value | Where |
|---|---|---|
| The boundary - `Ft8BoundaryResult.Outcome` | **`Ft8ArmOutcome.Ran`** | `Ft8ArmedSend.cs:471-473` returns `Ran` for anything the sequence ran, including a refusal. It says *the sequence ran*, not *a transmission happened* |
| The run - `TransmitRun.Outcome` | **`Ft8TransmitOutcome.RefusedByLicence`** | `Ft8TransmitSequence.cs:259-263`, before anything that can key |

So the assertion is `result.Outcome == Ft8ArmOutcome.Ran` **and**
`result.Run!.Outcome == Ft8TransmitOutcome.RefusedByLicence`, with
`run.Sent == false` and `run.Keyed == false`. A test written against
`Ft8ArmOutcome` alone would read a refusal as a send.

**And the class to use.** `LicenseClass.Technician` at `14_074_000` in
`TransmitMode.Data` is out of privileges - the engine test's own comment at
`OneClickSendsExactlyOneMessageTests.cs:205` says *"14.074 MHz is FT8's own
watering hole and is outside a Technician's HF phone-and-data privileges."*

---

## Where the instruction and the tree disagree

**Nothing material.** Every line on work instruction 267's verification list was
checked and holds, including the line numbers: `MainWindow.axaml:3097`, `:3129`,
`:3138`, `:3144`; `MainWindowViewModel.cs:8121`, `:8225`, `:8346`, `:8532`;
`MainWindow.axaml.cs:171`; `Ft8TransmitSequence.cs:262`. Two small readings:

1. **`MainWindow.axaml.cs:171` is `SendFlyoutFor` itself**; the handler that raises
   it, `OnDecodedRowContextRequested`, is at `:125`. The instruction names `:171`
   and `SendFlyoutFor` together, which is right.
2. **`MainWindowViewModel.cs:8225` is the `RestrictTransmitToPrivileges` argument**
   and `:8224` is `LicenseClass`; the `OperatorSend` construction opens at `:8221`.
   The instruction's *"`:8225` puts `LicenseClass` and
   `_settings.RestrictTransmitToPrivileges` into the `OperatorSend`"* is one line
   off on the first of the two and correct about the seam.

Root version reads **1.12.120**, as expected.
