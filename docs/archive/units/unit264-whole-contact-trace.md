# Unit 264 - the whole contact, traced end to end through the application

**PROJECT: Hamlet.** Read from the working tree at `HEAD 8531720`, which is
`d8a92a6` plus this unit's own bookkeeping commit and no product change. **No
product code and no test was written for this document**; every answer below is a
file, a line and a quotation.

**FACT-004.** Nothing here opened a port, keyed anything or played a sound. Every
figure is a reading of source text on **the development machine**, which has never
had a radio attached to it.

---

## 1. Where `_contacts` is built, and with what callsign

`MainWindowViewModel.cs:1129` declares it:

```csharp
private Ft8ContactLedger? _contacts;
```

and `:1132` declares the callsign it was opened for:

```csharp
/// <summary>Whose callsign <see cref="_contacts"/> was opened for.</summary>
private string _contactsFor = "";
```

**It is constructed in exactly one place**, inside `ContactTextFor`, at
`:7901-7906`:

```csharp
if (_contacts is null
    || !string.Equals(_contactsFor, mine, StringComparison.OrdinalIgnoreCase))
{
    _contacts = new Ft8ContactLedger(mine);
    _contactsFor = mine;
}
```

**What `mine` is.** `:7886`:

```csharp
var mine = _settings.Operator.Callsign?.Trim() ?? "";
```

- the operator's own callsign out of Settings, read fresh on every row;
- **when it is empty or absent the method returns before the ledger is ever
  built.** `:7892-7895`:

```csharp
if (mine.Length == 0 || row.SlotStartUtc == default)
{
    return "";
}
```

So with no callsign in Settings, `_contacts` stays null for the whole session,
every row's contact cell is `""` (the early return is `:7892-7895`), **and
`SendMenuFor` returns null** because of
its own `_contacts is null` guard at `:8104`. No callsign in Settings means no
right-click menu at all - which is a fact about the application worth knowing
before Tim sits down, and it is not a defect: `Ft8ContactLedger` is kept *from
somebody's station* and there is no somebody yet.

**A changed callsign opens a new ledger rather than rewriting the old one** - the
`!string.Equals(_contactsFor, mine, ...)` half of the same condition. The comment
at `:7897-7900` gives the reason.

### Is `ContactTextFor`'s instance the same instance `AtSlotBoundaryAsync` writes to?

**Yes.** There is one field and there are only five references to it in the whole
tree (`grep -n "_contacts" MainWindowViewModel.cs`):

| Line | What it does |
|---|---|
| `1129` | declares it |
| `7901-7905` | tests it and constructs it, inside `ContactTextFor` |
| `7908` | `_contacts.RecordHeard(row.Message, row.SlotStartUtc);` |
| `7910` | `var record = _contacts.For(row.Sender);` |
| `8104`, `8109` | `SendMenuFor` reads it |
| `8279` | `_contacts?.RecordSent(text, result.Send!.SlotStartUtc);` |

One field, no copies, no second ledger, no re-assignment anywhere but `:7904`.
**The reader at `:7910` and the writer at `:8279` are the same object**, provided
the callsign has not changed in between - and if it has, `:7904` replaces the
whole ledger and the sends booked against the old one are gone with it. That is
deliberate and documented at `:7897`.

**One consequence that matters for task 2.** `_contacts` is created lazily *by a
heard row*. `AtSlotBoundaryAsync`'s `_contacts?.RecordSent(...)` is
null-conditional, so **an operator who transmits before a single decode has been
placed books nothing at all.** In practice a contact always starts from a decode,
so the CQ row builds the ledger first; but the null-conditional means the failure
mode is silence rather than a throw.

---

## 2. What recomputes a row's contact cell, and when

`:7823`, inside `PlaceRow`:

```csharp
// **AND THE ONE PLACE THE CONTACT STATE REACHES A ROW** (unit 258). Same
// door, same reason: every row goes through here, so there is one place
// that books what was heard and one place that reads back where the
// contact stands.
row = row with { Contact = ContactTextFor(row) };
```

### Every caller of the enclosing method

`PlaceRow` is `private` and `grep -rn "PlaceRow" src/ --include=*.cs` returns, in
code rather than doc comments, exactly **two** call sites:

| Line | Caller | What triggers it |
|---|---|---|
| `7770` | `AddDecodeRow(Ft8Decode decode)` - `PlaceRow(DigitalDecodeRow.From(decode));` | one decode coming out of one slot, and **only if the key is new**: `:7757` `if (!_digitalDecodeKeys.Add(key)) { return false; }` |
| `7855` | `AddDecodeRowForTests(...)` | a test |

That is the whole list. `PlaceRow` is reached **once per new decode**, at the
moment the decode arrives, and from nowhere else in `src/`.

### The question that matters

> After `AtSlotBoundaryAsync` books a send at `:8279`, is there anything that
> makes the row recompute, or does the cell keep the value it was built with?

**Nothing recomputes it. The cell keeps the value it was built with.**

`row` is a `record` and `:7823` uses `with`, so the string is baked into the
instance that goes into `_digitalArrivals` and `DigitalDecodes` at `:7825-7826`.
`AtSlotBoundaryAsync` (`:8250-8285`) touches `_contacts`, `DigitalSendLine` and
nothing else - it does not enumerate `DigitalDecodes`, does not call `PlaceRow`,
and does not raise a change on any row. `grep -n "Contact = " MainWindowViewModel.cs`
returns `:7823` and nothing else.

**So a send changes the ledger and does not change any row already on the table.**
The rows that show the new state are the ones placed *afterwards*, because
`ContactTextFor` reads the ledger fresh each time it runs.

This is the first of the two seams work instruction 264 said had never had a test
through them, and it decides the shape of task 2: **the row that can read
`complete` is the row for the station's last transmission, placed after the
operator's sends have been booked.** A test that clicks twice and then re-reads
the *CQ* row would find the string that row was built with at slot 0 and would be
right to fail.

---

## 3. As of when is it computed?

`:7914`:

```csharp
return record is null
    ? ""
    : Ft8ContactStates.Read(record, row.SlotStartUtc).Text;
```

so `nowUtc` inside `Read` **is the row's own slot boundary, not the current
moment.** The remark at `:7868-7871` says why:

> **THE ROW SHOWS WHERE THE CONTACT STOOD IN ITS OWN SLOT**, which is what a
> table of decodes is: a record of moments. The state is read at the row's own
> slot boundary, so a row never restates itself as the evening goes on and a
> reader can see a contact progressing down the table.

**But the state itself is not filtered by that moment - only the count is.**
`Ft8ContactStates.Read` (`Ft8ContactState.cs:120-169`) uses `nowUtc` in exactly
three places, and all three are the slot *count*:

```csharp
if (IsComplete(record))
{
    var last = Later(heardToUs?.SlotStartUtc, sent?.SlotStartUtc) ?? nowUtc;

    return new Ft8ContactRead(
        record.Callsign,
        Ft8ContactState.Complete,
        Ft8StationRecord.SlotsAgo(last, nowUtc));
}
```

`IsComplete(record)` (`:186`) takes no moment at all. `record.HeardToUs` and
`record.Sent` are the **whole** history, never trimmed. So:

- **which of the four states** is decided from every message in the record,
  whenever it was added;
- **the number of slots printed beside it** is measured from `row.SlotStartUtc`.

**What that means for a row whose last decode was three slots before the
operator's final transmission.** Two separate answers, and they must not be run
together:

1. That row's cell was computed when the row was placed - three slots before the
   send existed - so **it still says whatever it said then**, by section 2. It
   does not move.
2. If something *did* recompute it, the state would be the current one -
   `complete` - but the count would be measured backwards. `SlotsAgo` is
   `Ft8ContactLedger.cs:95`:

   ```csharp
   internal static int SlotsAgo(DateTime thenUtc, DateTime nowUtc)
       => Ft8Slots.BoundariesBetween(thenUtc, nowUtc).Count(at => at > thenUtc);
   ```

   With `last` (the send) *after* `nowUtc` (the row's slot), that counts
   boundaries over an inverted interval. It cannot go negative - it is a count -
   but it is not a meaningful "slots ago" either.

**This is the answer to the second thing the instruction expected to be told it
got wrong**, and the instruction was right to leave it to measurement: the cell is
computed as of the row's own slot, *for the count*, and as of the whole record
*for the state*. What stops the row moving after a send is section 2's finding -
that nothing recomputes it - and not the `row.SlotStartUtc` argument.

---

## 4. Can `complete` be reached at all through the application?

`Ft8ContactState.cs:186-210`:

```csharp
public static bool IsComplete(Ft8StationRecord record)
{
    ArgumentNullException.ThrowIfNull(record);

    // BOTH CALLS: he has addressed the operator and the operator has
    // addressed him, so each has the other's callsign.
    if (record.LastHeardToUs is null || record.Sent.Count == 0)
    {
        return false;
    }

    var his = record.HeardToUs
        .Where(m => m.Fields is not null)
        .Select(m => m.Fields!.Payload)
        .ToList();

    var ours = record.Sent
        .Where(m => m.Fields is not null)
        .Select(m => m.Fields!.Payload)
        .ToList();

    return his.Any(IsGridOrReport) && his.Any(IsAcknowledgement)
        && ours.Any(IsGridOrReport) && ours.Any(IsAcknowledgement);
}
```

**Mismatch with the instruction, reported and not repaired.** Work instruction 264
task 1 question 4 names three requirements - `record.Sent.Count > 0`,
`ours.Any(IsGridOrReport)` and `ours.Any(IsAcknowledgement)`. The tree has **five**:
those three plus `record.LastHeardToUs is not null`, `his.Any(IsGridOrReport)` and
`his.Any(IsAcknowledgement)`. **The tree wins.** Both halves of the exchange are
required, which is what "both calls, both grids or reports, both acknowledgements"
in the remark at `:171-173` says. It changes nothing about the exchange task 2
drives, which satisfies all five.

### `RecordSent` populates `Fields`

`Ft8ContactLedger.cs:224-239`:

```csharp
public void RecordSent(string? message, DateTime slotStartUtc)
{
    var fields = Ft8MessageSplit.Split(message);

    if (fields is null || Ft8MessageSplit.IsCallToAnyone(fields.To))
    {
        return;
    }

    if (IsOperator(fields.To))
    {
        return;
    }

    Book(fields.To).AddSent(
        new Ft8LedgerMessage(message!.Trim(), fields, slotStartUtc));
}
```

**Confirmed: `Fields` is the second constructor argument and it is never null on
this path** - a null `fields` returns before `AddSent` is reached. So the
`.Where(m => m.Fields is not null)` filter in `IsComplete` discards nothing that
`RecordSent` books. **Two things it does discard, and both are correct:** a
message the splitter refuses at all, and the operator's own CQ, whose `To` is a
call to anyone and which books nobody.

### The exact sequence of operator messages that satisfies it

Given a station `W1ABC` that has sent the operator a report and a roger, the
operator's own two messages must between them carry a grid-or-report and an
acknowledgement. **Two clicks is the minimum**, and the shortest ordinary FT8
exchange is exactly it:

| The operator sends | `Fields.Payload` | `IsGridOrReport` | `IsAcknowledgement` |
|---|---|---|---|
| `W1ABC KC3QIS FN00` | `FN00` | yes - `Ft8MessageSplit.IsGrid` | no |
| `W1ABC KC3QIS R-11` | `R-11` | yes - `IsReport` | yes |

`R-11` alone satisfies both `ours` clauses, because one message may do two jobs -
the remark at `Ft8ContactState.cs:181-183` says so explicitly, and it is the
format's arithmetic rather than Hamlet reading intent into anything. So the
strictly minimal operator side is **one** click of `R-11`; the exchange task 2
drives sends two, which is what a real contact looks like.

`IsGridOrReport` tests the courtesies first (`:216-227`), so `RR73` is **not**
counted as a grid despite having a grid's shape. That is why `W1ABC`'s `RR73`
lands as `his` acknowledgement and not as `his` report, and why his `-09` is
needed.

---

## 5. Does the application's own telemetry write a `Transmit` line to disk?

The chain, quoted at each link.

**`App.axaml.cs:38-43`** - the writer the application builds:

```csharp
_telemetry = new JsonlTelemetry(
    SettingsStore.TelemetryFolder,
    version,
    category => _settings.IsTelemetryEnabled(category),
    _settings.TelemetryMaxMegabytes * 1024L * 1024L);
```

**`AppSettings.cs:394-395`** - the predicate:

```csharp
/// <summary>True when the category is on. Unknown categories are on.</summary>
public bool IsTelemetryEnabled(TelemetryCategory category)
    => !TelemetryCategories.TryGetValue(category.ToString(), out var on) || on;
```

**`Ft8TransmitSequence.cs:404-405`** - the write:

```csharp
_telemetry.Write(
    TelemetryCategory.Transmit, TransmitRecord.EventName, record.ToBag(), record.Level);
```

**`AppSettings.cs:600-605`** - the folder:

```csharp
public static string DataFolder { get; internal set; } = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "Hamlet");

/// <summary>%AppData%\Hamlet\telemetry.</summary>
public static string TelemetryFolder => Path.Combine(DataFolder, "telemetry");
```

**`JsonlTelemetry.cs:185-187`** - the file:

```csharp
var path = Path.Combine(_folder,
    DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
    + ".jsonl");
```

**`TransmitRecord.cs:59`** - the event name:

```csharp
public const string EventName = "ft8_transmission";
```

So, named:

| | |
|---|---|
| folder | `%AppData%\Hamlet\telemetry` |
| file | today's date in UTC, `yyyy-MM-dd.jsonl` |
| event name | `ft8_transmission` |
| category | `TelemetryCategory.Transmit` |

**And the writer that reaches the sequence is the application's own.**
`MainWindowViewModel.cs:8048-8049`:

```csharp
_armedSend = new Ft8ArmedSend(
    new Ft8TransmitSequence(port, sink, _sendLicence, _telemetry));
```

`_telemetry` is the constructor parameter at `:3073`, which `App.axaml.cs:48`
fills with the instance quoted above. **There is no second writer on the send
path.**

### Does any existing test read that line back through the application's writer?

**No.** `grep -rn "JsonlTelemetry" tests/` returns seventeen hits. The only one on
the transmit path is
`tests/Hamlet.RadioEngine.Tests/Transmit/WhereTheTransmissionStartsAndWhatTheRecordSaysTests.cs:409`:

```csharp
using (var telemetry = new JsonlTelemetry(folder, "1.12.89", _ => true))
```

**Three arguments, and the predicate is `_ => true`.** That is the *engine's* path:
it is in the engine's test project, it constructs the sequence directly, and it
substitutes a predicate that cannot refuse. It says nothing about whether
`AppSettings.IsTelemetryEnabled(TelemetryCategory.Transmit)` lets the line
through, and nothing about the cap argument the application passes as a fourth
parameter.

The nearest thing on the application's side is
`tests/Hamlet.App.Tests/Telemetry/TheSinkWritesWhenDrivenLikeTheAppTests.cs:81-86`,
which *does* build the writer the way `App.axaml.cs` does:

```csharp
using var telemetry = new JsonlTelemetry(
    _folder,
    "1.12.38",
    category => settings.IsTelemetryEnabled(category),
    CapBytes(settings));

AppEvents.AppStart(telemetry);
```

but it drives `app_start` in category `Diagnostics`. **Nothing anywhere drives a
transmission through the application's writer and reads `ft8_transmission` back
off disk.** That is the second of the two untested seams, and it is task 4.

**What the two lines predict, for the record, so that the measurement can
contradict them.** `TelemetryCategories` is empty on a default `AppSettings`
(asserted at `TheSinkWritesWhenDrivenLikeTheAppTests.cs:66`), so `TryGetValue`
fails and `IsTelemetryEnabled` returns true for `Transmit`. **That is a
prediction from two lines of source and it is exactly what work instruction 264
says to measure instead of reason about.** Task 4 measures it.

---

## 6. The dark tripwire

`tests/Hamlet.RadioEngine.Tests/Transmit/TheUnkeyHappensWhateverGoesWrongTests.cs:404`,
`ExactlyOneFileInTheShippedTreeCallsTheSequence`. **Both of its assertions,
quoted.**

**The first**, `:419-423`:

```csharp
var only = Assert.Single(callers);

Assert.Equal("Ft8ArmedSend.cs", Path.GetFileName(only));
```

over `callers`, which is every `.cs` file under `src/`, excluding `obj`, `bin` and
`Ft8TransmitSequence.cs` itself, whose **code with doc comments stripped**
contains the string `Ft8TransmitSequence`.

**The second**, `:429-441`:

```csharp
var runners = Directory
    .EnumerateFiles(source, "*.cs", SearchOption.AllDirectories)
    ...
    .Where(file => CodeOnly(File.ReadAllText(file))
        .Contains("_sequence.RunAsync", StringComparison.Ordinal))
    .Select(Path.GetFileName)
    .ToList();

_output.WriteLine($"call _sequence.RunAsync: {string.Join(", ", runners)}");

Assert.Equal(["Ft8ArmedSend.cs"], runners);
```

### Which one fails, and why

**The first.** `Assert.Single(callers)` throws, because `callers` has two members.
`grep -rn "Ft8TransmitSequence" src/ --include=*.cs`, with `obj/` and `bin/`
excluded, returns nineteen hits, of which **exactly two are code rather than doc
comment**:

- `src/Hamlet.RadioEngine/Transmit/Ft8ArmedSend.cs:202` -
  `private readonly Ft8TransmitSequence _sequence;` (and `:233`, the constructor
  parameter)
- `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:8049` -
  `new Ft8TransmitSequence(port, sink, _sendLicence, _telemetry));`

The second of those is unit 260's own work: it is the line that gave the send path
a real port and a real sink, and it is what made the application able to transmit
at all. **The test has been red since that commit, fifteen commits ago.** The
other seventeen hits are `<see cref=...>` and `<c>...</c>` inside `<remarks>`, and
`CodeOnly` strips them - which the remark at `:398-402` says is deliberate.

### The safety property, checked by direct grep

The second assertion's property is that `_sequence.RunAsync` has exactly one
caller in `src/`. Measured directly, on the development machine, at `HEAD 8531720`:

```
$ grep -rn "_sequence.RunAsync" --include=*.cs src/ | grep -v "/obj/" | grep -v "/bin/"
src/Hamlet.RadioEngine/Transmit/Ft8ArmedSend.cs:471:            var run = await _sequence.RunAsync(send, source.Token).ConfigureAwait(false);
```

**One line, one file, `Ft8ArmedSend.cs`. The safety property holds.** What is
broken is the guard's *first* assertion, which counts constructions and not
reaches; the thing it guards - one route to a keying frame - is intact. That is
why work instruction 264 names task 5 as the drop candidate, and the substantive
question it leaves is whether the assertion should distinguish **constructing** an
`Ft8TransmitSequence` from **reaching `RunAsync` through one**.

`MainWindow.axaml.cs:168` mentions `Ft8TransmitSequence.RunAsync` in a doc comment
and is not a caller by either measure.

---

## 7. Is there an existing test that drives two sends through `AtSlotBoundaryAsync` for the same station and then reads the row's contact cell?

**None.**

`grep -rn "AtSlotBoundaryAsync" tests/` returns twelve hits across five files. The
nearest is
`tests/Hamlet.App.Tests/ViewModels/TheSendPathReachesARealRadioTests.cs:220-247`,
`OneClickIsOneMessageAcrossTwoBoundaries`:

```csharp
var first = await panel.AtSlotBoundaryAsync(slot!.Value);
var second = await panel.AtSlotBoundaryAsync(slot.Value.AddSeconds(15));
...
Assert.Equal(Ft8ArmOutcome.Ran, first.Outcome);
Assert.Equal(Ft8ArmOutcome.NothingArmed, second.Outcome);
```

**It drives two boundaries against one arming, and its point is that the second
sends nothing.** It never places a row, never reads a contact cell, and never
books a second message.

The others, and what each does instead:

| File | What it drives |
|---|---|
| `TheSendPathReachesARealRadioTests.cs:186` | one boundary, one send, counts sink calls and port frames |
| `TheSendPathComposesAtTheEndpointsRateTests.cs:88,216,254` | one boundary each, asserting the composed rate |
| `TheLoopbackThroughTheApplicationsSendPathTests.cs:157` | one boundary, then decodes the audio back |
| `TheMenuIsUnderTheMouseTests.cs:484` | one boundary per message inside `SendAsync`, driven from a real right-click on a real control tree - **the closest thing in the tree to a whole exchange**, and it asserts the menu's contents, not the row's contact cell |
| `TheOperatorCanStopItTests.cs:140,174,249,327` | one boundary each, stopped part-way |

`TheMenuIsUnderTheMouseTests` is worth naming precisely, because it is the one
that could be mistaken for this: it sends more than one message through
`SendMessageCommand` and `AtSlotBoundaryAsync` in its `Scene` setup
(`:463-466`), so the ledger there does get two sends. **But nothing in that file
reads `DigitalDecodeRow.Contact` at all**, and no row is placed after the sends -
every row is added before them at `:456-459`. So the join this unit exists for is
untested there too.

---

## What the trace decides about task 2

Three findings shape the test, and none of them is a guess:

1. **The row that can read `complete` is a row placed after both sends are
   booked** (section 2). The exchange's slot 4 - `KC3QIS W1ABC RR73` - is that
   row, and it is also the message that gives `his` its acknowledgement. The
   test reads that row's `Contact`.
2. **`IsComplete` ignores time entirely** (section 3), so the exchange's logical
   order is what matters and the fabricated slot times are only a slot count.
3. **The sends book at the slot the clock offered**, not at a slot the test chose:
   `MainWindowViewModel.cs:8174-8175` reads the real clock at the click and
   `:8279` books `result.Send!.SlotStartUtc`. So the test records what
   `ArmedForSlotUtc` said and asserts telemetry against **that**, rather than
   against a number it made up.
