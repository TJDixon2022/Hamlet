# Unit 270 - the last message trace

**What this is.** Work instruction 270, task 1: the measurement that comes before
anything is built. Six questions, each answered with a file, a line and a
quotation. **An answer of "none" is a finding and is written as one.**

**Read on 2026-09-07 at `HEAD` `731a499`**, root version `Directory.Build.props`
`<Version>1.12.123</Version>` (read, not assumed). Every line number below was
re-read off the tree rather than carried over from the instruction; where the
instruction's number was wrong the correction is stated at the site and repeated
in question 1.

**Nothing here builds, sends, opens a device or edits product code.** It is a
reading of the tree.

---

## 1. What can move a row's `Contact` cell today?

**`PlaceRow` is at `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:7826`**, as
the instruction says:

```csharp
7826:    private DigitalDecodeRow PlaceRow(DigitalDecodeRow row)
```

**The assignment is at `:7840`**, as the instruction says, under its own remark at
`:7836`:

```csharp
7836:        // **AND THE ONE PLACE THE CONTACT STATE REACHES A ROW** (unit 258). Same
7837:        // door, same reason: every row goes through here, so there is one place
7838:        // that books what was heard and one place that reads back where the
7839:        // contact stands.
7840:        row = row with { Contact = ContactTextFor(row) };
```

**It has exactly two callers, and there is no third.** `grep` for `PlaceRow`
across `MainWindowViewModel.cs` returns four hits: the definition at `:7826`, the
doc-comment mention at `:1486`, and two call sites -

```csharp
7787:        PlaceRow(DigitalDecodeRow.From(decode));          // the decoder's door
7872:        var row = PlaceRow(new DigitalDecodeRow(          // AddDecodeRowForTests
```

There is no other call in `src/`, and `DigitalDecodeRow.Contact` is assigned
nowhere else: the only other `with { ... }` on a placed row in this method is
`ObserverGrid` at `:7834`.

**The remark about a row never restating itself, quoted.** The instruction places
it on `PlaceRow`; **it is in fact on `ContactTextFor`'s doc comment at
`:7885-7888`**, which is what `:7840` calls. The words are the ones the
instruction quotes and the design is the same one; only the line is different.

```
7885:    /// <para>**THE ROW SHOWS WHERE THE CONTACT STOOD IN ITS OWN SLOT**, which is
7886:    /// what a table of decodes is: a record of moments. The state is read at the
7887:    /// row's own slot boundary, so a row never restates itself as the evening
7888:    /// goes on and a reader can see a contact progressing down the table.</para>
```

**Plainly: nothing in the tree recomputes the `Contact` cell of a row already on
the table.** `PlaceRow` computes it once, at insertion, from `row.SlotStartUtc` -
the row's own slot. `DigitalDecodes` is only ever `Insert`ed into and `Remove`d
from (`:7798`, the trim); no code path reads a row back out, recomputes
`ContactTextFor` and writes it back. **The only way a `Contact` cell changes is
that a new row is placed.**

---

## 2. What does `RecordSent` reach?

**The one call site in the tree is `MainWindowViewModel.cs:8474`**, inside
`AtSlotBoundaryAsync` (`:8445`), reached only where `run.Sent` is true:

```csharp
8470:        if (run.Sent)
8471:        {
8472:            // **THE ONE CALL SITE OF `RecordSent` IN THE TREE**, which is the line
8473:            // unit 258 left it unreachable for.
8474:            _contacts?.RecordSent(text, result.Send!.SlotStartUtc);
8475:        }
```

`Ft8ContactLedger.RecordSent` at `Ft8ContactLedger.cs:225` **returns `void`** and
writes into a private dictionary. `_contacts` at `MainWindowViewModel.cs:1129` is
a private field with no public accessor; the only window onto it is
`internal Ft8StationRecord? ContactRecordForTests(string)` at `:7951`, which is
`internal` and read-only.

**Every screen-visible thing that changes when a send is booked - the whole
list:**

| Thing | Where | Changed by the booking? | Does it say where the contact stands? |
|---|---|---|---|
| `DigitalSendLine` | `:8489`, from `WentLine(text, result)` | No - it is set in the same method, from `result`, whether or not the ledger was touched | **No.** It says what went out, into which slot, and how the radio came out of transmit |
| `DigitalTransmitLevelLine` | `:8490`, from `MeasuredLevelLine(_transmitLevelReport)` | No - from the sink's report | **No.** It is a level in dBFS and a clamp count |
| The `Contact` cell of any row on `DigitalDecodes` | `:7840` | **No.** Question 1: computed once, at the row's own slot | n/a |
| The `Contact` cell of the **next** row placed | `:7840` via `:7901` | **Yes** - `ContactTextFor` reads `_contacts.For(row.Sender)` at `:7927` | Yes, but **only if another row arrives** |
| The next right-click menu | `SendMenuFor` at `:8289`, `_contacts.For(row.Sender)` at `:8296` | **Yes** - `Ft8SendOptions.For(record, ...)` sees the sent message | It highlights an expected next message; it is not a statement of state and it is only visible while a flyout is open |

**So: nothing already on the screen changes, and of the two things that do
change, one needs a further decode to arrive and the other needs the operator to
right-click.** If the station has gone quiet after his last transmission, neither
happens. **The answer to "which of them says where the contact now stands" is
none.**

---

## 3. Can the application name the station it just sent to, without a second copy
of the splitter?

**Yes. The route exists, it is public, and the app already uses it. Task 3 does
not take its fallback.**

`src/Hamlet.RadioEngine/Contacts/Ft8MessageSplit.cs`:

```csharp
53:    public static Ft8MessageFields? Split(string? message)
86:    public static bool IsCallToAnyone(string? to)
```

Both are `public static` on a `public static class` in
`Hamlet.RadioEngine.Contacts` - **the same namespace and the same assembly
`MainWindowViewModel` already reaches for `Ft8ContactLedger` and
`Ft8ContactStates`.** The app calls into this type in two places today:

```csharp
src/Hamlet.App/ViewModels/Ft8Vocabulary.cs:57      => Ft8MessageSplit.Split(message);
src/Hamlet.App/ViewModels/DecodedFilter.cs:86      => Hamlet.RadioEngine.Contacts.Ft8MessageSplit.IsCallToAnyone(to);
```

and `Ft8MessageSplit`'s own class remark at `:22-27` says the app's `Split`
"became a one-line forward" and **"Do not write a second copy in either
direction."**

**The record type's field order matters and is easy to get backwards.**
`Ft8MessageSplit.cs:9`:

```csharp
public sealed record Ft8MessageFields(string To, string From, string Payload);
```

**`To` is first.** `Split` builds it positionally at `:73`, so for a
three-field message `parts[0]` is the addressee.

**What `Split` returns for the two messages the question names:**

| Input | `To` | `From` | `Payload` | `IsCallToAnyone(To)` |
|---|---|---|---|---|
| `"W1ABC KC3QIS RRR"` | `W1ABC` | `KC3QIS` | `RRR` | `false` |
| `"CQ KC3QIS FN00"` | `CQ` | `KC3QIS` | `FN00` | `true` |

Read off `Split` itself: neither is four words beginning `CQ`, so both take the
`parts.Length == 3` branch at `:72-74`; `IsCallToAnyone` at `:86-92` returns true
for exactly `"CQ"` or a leading `"CQ "`.

**This is the same question the ledger asks of the same method.**
`Ft8ContactLedger.RecordSent` at `:227-240` does exactly this - `Split`, then
`IsCallToAnyone(fields.To)`, then books `fields.To`. A line that asks the same
two questions of the same two methods is not a second copy of the rule; it is the
same rule read twice. **The answer for task 3 is `fields.To`, and a `CQ` yields
no station because `IsCallToAnyone` says so.**

---

## 4. Where does the state get read from, and at what moment?

**`Ft8ContactStates.Read` is at `Ft8ContactState.cs:117`** and takes the moment:

```csharp
117:    public static Ft8ContactRead Read(Ft8StationRecord record, DateTime nowUtc)
```

**Complete is tested first** (`:127-135`), and the count it returns is measured
from the last message that mattered to that moment:

```csharp
127:        if (IsComplete(record))
129:            var last = Later(heardToUs?.SlotStartUtc, sent?.SlotStartUtc) ?? nowUtc;
131:            return new Ft8ContactRead(
132:                record.Callsign,
133:                Ft8ContactState.Complete,
134:                Ft8StationRecord.SlotsAgo(last, nowUtc));
```

The words and the count are printed by `Ft8ContactRead.Text` at `:48-49`:

```csharp
48:    public string Text => Words + ", " + Slots.ToString(CultureInfo.InvariantCulture)
49:        + (Slots == 1 ? " slot" : " slots");
```

**Which moment a line written after a boundary should pass:
`result.Send!.SlotStartUtc`** - the slot the transmission actually went out in.
Three reasons, all of them off the tree:

1. **It is the moment the ledger was told about.** `:8474` books the send at
   exactly that value, so reading at the same value asks the ledger about the
   state it has just been put into rather than about a moment no message belongs
   to.
2. **It is the same convention the rows use.** `ContactTextFor` at `:7931` reads
   at `row.SlotStartUtc` - the row's own slot, never `DateTime.UtcNow`. A second
   reader that used the wall clock would print a different count from a row
   placed in the same slot, and two readers of one ledger disagreeing about the
   count is the drift this project fixes by construction.
3. **`DateTime.UtcNow` at the `Dispatcher.UIThread.Post` is not a slot boundary
   at all.** `AtSlotBoundaryAsync` awaits the whole transmission before it reaches
   `:8487`, so the wall clock there is some seconds into the slot, and
   `SlotsAgo` would be counting from a moment that is not a slot start.

**What the count means:** how many fifteen-second slots lie between the last
message that carried the state and the moment it was read at - `SlotsAgo(last,
nowUtc)`. Read at the send's own slot, immediately after that send, the count is
**0 slots**, and that is the honest number: the thing that completed the contact
happened in the slot being read at. **The count is what stops the line claiming to
be current when it is not**; a line read at 15:52:45 and still on screen at
16:10 still says which slot it was read at, and the count says how old the
evidence under it was when it was taken.

---

## 5. Which committed test already covers which half of step E's criterion 2?

Criterion 2: *"The transmitted slots appear in telemetry and the row reads
complete."*

**`TheWholeContactWalksThroughTheApplicationTests.AWholeContactWalksThroughAndTheRowReadsComplete`**
(`tests/Hamlet.App.Tests/ViewModels/TheWholeContactWalksThroughTheApplicationTests.cs:109`)
drives five slots through the application - `CQ W1ABC EM12` heard, `Grid`
clicked, a real slot boundary waited for, `KC3QIS W1ABC -09` heard,
`RogerAndReport` clicked, `KC3QIS W1ABC RR73` heard - and asserts four things:
both boundaries ran and reported `Sent`; the ledger holds 2 sent and 3 heard
against `W1ABC`; the **newest** row for `W1ABC` contains `"complete"`; and both
armed slot times appear in the telemetry file on disk under
`data.slotStartUtc`.

**`TheWholeContactWalksThroughTheApplicationTests.OneSendLeavesOneLineOnDiskAndTheLineNamesNobody`**
(`:256`) sends once and asserts that exactly one `ft8_transmission` line is on
disk, that its `slotStartUtc`, `durationSeconds` and `sampleCount` are the ones
that went out, and that neither callsign, neither grid and neither message
appears anywhere in the raw line (HM-DEC-018), with `messageLength` the one thing
it does say.

**So: the telemetry half of criterion 2 is proved twice over, and the "row reads
complete" half is proved for exactly one ending.** The comment in the walk at the
site says which ending that is:

```csharp
        // SLOT 4 - he signs off. **This is the row placed after both sends**, and
        // by the trace's question 2 it is the only kind of row that can move.
        Heard(panel, second.SlotUtc.AddSeconds(15), MeasuredHim, Mine + " " + His + " RR73");
```

**What the two do not cover, in one sentence:** neither drives an exchange in
which **the operator's own last transmission** is the message that satisfies
`IsComplete`, so neither ever asks what Hamlet shows when nothing is heard
afterwards - which is the ending where the newest row on the table was placed
*before* the send and still reads *your move*.

---

## 6. Where in the Send area does a fourth line go?

**`src/Hamlet.App/Views/MainWindow.axaml`.** The area is
`<Border Grid.Row="1" x:Name="DigitalSendReserved"` at **`:3081`**, directly
beneath `DigitalWaterfallPanel` (`:3025`) in the same `Grid`. Its one child is a
vertical `StackPanel` at `:3089`, whose children in order are:

| Order | Line | Control |
|---|---|---|
| 1 | `:3095` | horizontal `StackPanel` holding `DigitalSendCqButton` (`:3097`) and **`DigitalStopButton` (`:3119`)** |
| 2 | `:3128` | `DigitalSendReservedLine`, `{Binding DigitalSendLine}` |
| 3 | `:3135` | `DigitalSendLicenceText` |
| 4 | `:3142` | `DigitalSendUnsetText` |
| 5 | `:3192` | the `Grid` holding `DigitalTransmitDriveBox` (`:3200`) |
| 6 | `:3226` | `DigitalTransmitDriveNote` |
| 7 | `:3257-3261` | `DigitalTransmitLevelText`, `{Binding DigitalTransmitLevelLine}` |

The `StackPanel` closes at `:3262` and the `Border` at `:3263`.

**The fourth line goes immediately after `DigitalTransmitLevelText` and before the
`</StackPanel>` at `:3262`** - i.e. as the **last** child of the vertical stack,
**below** the level readout and **below** the drive control.

**What that sits above or below so `DigitalStopButton` does not move: it sits
below everything, and the Stop button is the first child.** In a vertical
`StackPanel` a child's position depends only on the children **before** it, so
appending at the end cannot move item 1. This is the rule unit 269 already wrote
into the markup at `:3169-3175`:

```
                                         **IT IS BELOW THE STOP BUTTON AND NOT
                                         ABOVE IT.** Anything inserted before
                                         the button row would push the one
                                         control that must never be unreachable
                                         further down the area, and that is the
                                         first of the things no unit may reason
                                         past.
```

The backing field follows the shape of `_digitalSendLine`
(`MainWindowViewModel.cs:8149`) and `_digitalTransmitLevelLine` (`:8243`): an
`[ObservableProperty]` on the view model with the whole sentence formatted there,
so it can be asserted without opening a window, and markup that binds and does no
arithmetic.

---

## Mismatches between the instruction and the tree

**Reported, not repaired**, per the instruction.

1. **The "never restates itself" remark is not on `PlaceRow`.** The instruction
   says `MainWindowViewModel.cs:7826` - `PlaceRow`, "**Its own remarks** are the
   design this unit does not overturn". The remark quoted is on **`ContactTextFor`
   at `:7885-7888`**, whose call is the assignment at `:7840` inside `PlaceRow`.
   The words and the design are exactly as quoted; only the owning member is
   different.
2. **`SendMenuFor` is at `:8289`, not where the instruction leaves it unstated.**
   The instruction lists three `_contacts` sites (`:1129`, `:7901`/`:7927`,
   `:8474`) and does not mention `SendMenuFor`'s `_contacts.For(row.Sender)` at
   **`:8296`**. It is a read and it changes nothing, but it is a fourth reader of
   the ledger and question 2's table has to name it.

Every other line number the instruction gives - `:7826`, `:7840`, `:7787`,
`:7872`, `:7901`, `:8445`, `:8474`, `:8489-8490`, `:1129`, `:8149`, `:8243`,
`Ft8ContactLedger.cs:183`/`:196`/`:225`, `Ft8MessageSplit.cs:53`/`:86`,
`Ft8ContactState.cs:48`/`:117`/`:186`, `MainWindow.axaml:3081`/`:3119`/`:3128`/
`:3200`/`:3226`/`:3257`, `TheDriveIsSetWhereHeIsLookingTests.cs:81`/`:109-115`,
`Directory.Build.props:205` reading `1.12.123` - **was read off the tree tonight
and is correct.**
