# What step E asks of Hamlet

**PROJECT: Hamlet**

Unit 269, task 6. **Named, not built.** Nothing here was performed, no test was
written, no product code was changed and `SHACK_FACTS.md` was not touched. This is
the last bench-side reading before the phase's final step: **what in the
application each of step E's three criteria leans on, with a file and a line, and
whether anything they need is missing at the bench.**

Read off the tree on 2026-09-07, after unit 269's own changes. `SHACK_FACTS.md`
FACT-004: no radio has ever been attached to the machine this was read on, and
nothing here says anything about the IC-7300.

**CORRECTED BY UNIT 270, 2026-09-07.** This page told Tim to expect the stale
cell and to *"read the row that arrives after his last transmission rather than
the one he clicked"*. **That advice is out of date and has been corrected where
it appears** — sections 2.2 and *What is missing at the bench*, and one new row
in criterion 3's table. Unit 270 built the line the corrected advice points at.
**The finding underneath it is unchanged and is not a defect**: a row's `Contact`
cell is still computed once, at that row's own slot, and no row already on the
table is ever rewritten. **Still named and still not built**: nothing on this
page performs any part of step E, and `SHACK_FACTS.md` is still untouched.

---

## Step E's three criteria

> 1. He answers a CQ on 14.074 or 7.074 and completes an exchange.
> 2. The transmitted slots appear in telemetry and the row reads complete.
> 3. What he saw and anything that surprised him, recorded.

**All three are Tim's.** No unit performs them. What follows is only what the
application has to do while he does.

---

## Criterion 1 — he answers a CQ and completes an exchange

Six pieces, in the order one right-click travels through them.

### 1.1 The CQ has to arrive on a row he can right-click

- `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:7787` — `PlaceRow(DigitalDecodeRow.From(decode))`,
  the decoder's own door and the only route a heard signal takes to the table.
- `src/Hamlet.App/Views/MainWindow.axaml:3609` — `x:Name="DigitalDecodedRows"`,
  the table itself, and `:3742` the `Contact` cell on each row. **Both moved down
  by 46 lines when unit 270 added the contact line to the Send area**; they read
  `:3563` and `:3696` when this page was written.

### 1.2 The right-click has to raise the menu under the mouse

- `src/Hamlet.App/Views/MainWindow.axaml.cs:125` —
  `OnDecodedRowContextRequested`, the handler on the realized row control.
- `:136` — `var flyout = SendFlyoutFor(vm, row);`
- `:171` — `internal static MenuFlyout? SendFlyoutFor(MainWindowViewModel vm, DigitalDecodeRow? row)`
- `:190` — `CommandParameter = option.Text`, which is **the string that goes on the
  air**; unit 268 decoded a transmission back off a real card and matched it
  against this, not against a literal a test chose.

### 1.3 The menu has to offer the right messages, with the expected one marked

- `src/Hamlet.RadioEngine/Contacts/Ft8SendOptions.cs:99` — `public static Ft8SendMenu For(...)`,
  which walks all five messages and forbids none.
- `:177` — `private static Ft8SendShape? Expect(Ft8StationRecord record)`, which
  decides only which one is **highlighted**. Unit 266's finding stands: the contact
  state does not decide validity and never has.
- `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:8289` — `SendMenuFor`, the app's
  own door onto it.

### 1.4 The click has to arm exactly one transmission

- `MainWindowViewModel.cs:8337` — `private void SendMessage(string? text)`, the one
  arming site, and `:8729` — `SendCallToAnyone() => SendMessage(CallToAnyoneText)`,
  so the CQ button is the same door and not a second one.
- `MainWindowViewModel.cs:8359` — the single `Ft8Composer.ComposeSignal` call site
  in all of `src/`, composing at the endpoint's own rate and at
  `_settings.TransmitDrivePeak`.

### 1.5 The slot boundary has to fire what was armed, and only that

- `MainWindowViewModel.cs:8415` — `DriveTheArmedSend`, which **can fire what was
  armed and cannot arm anything**, called from `OnSlotTick` at `:7635`, which rides
  the 250 ms `_decodeTimer` (`:3280`, `:4915`).
- `MainWindowViewModel.cs:8453` — `await _armedSend.AtBoundaryAsync(boundaryUtc)`.
- `src/Hamlet.RadioEngine/Transmit/Ft8TransmitSequence.cs:435` — `Permits`, the
  licence gate, refusing anything the operator's class does not reach.

### 1.6 The abort has to be reachable the whole time

- `src/Hamlet.App/Views/MainWindow.axaml:3119` — `x:Name="DigitalStopButton"`, with
  no `IsVisible`, no `CanExecute` and nothing that reads a state.

**Anything missing at the bench for criterion 1? No.** Every piece above is joined
and was measured end to end by unit 268 on a real sound card: the clicked string
`"W1ABC KC3QIS RRR"` decoded back as `"W1ABC KC3QIS RRR"`, and the Stop button took
a real transmission off a real card 4.22 s in.

---

## Criterion 2 — the transmitted slots appear in telemetry and the row reads complete

Two halves, and **the second half has a real caveat.**

### 2.1 The telemetry line — nothing missing

- `src/Hamlet.RadioEngine/Telemetry/TransmitRecord.cs:59` —
  `public const string EventName = "ft8_transmission"`.
- `src/Hamlet.RadioEngine/Transmit/Ft8TransmitSequence.cs:391` — the record is
  built, and `:405` writes it under `TelemetryCategory.Transmit`.
- `src/Hamlet.App/Settings/AppSettings.cs:432-433` — the app's own predicate:

  ```csharp
    public bool IsTelemetryEnabled(TelemetryCategory category)
        => !TelemetryCategories.TryGetValue(category.ToString(), out var on) || on;
  ```

  **A category nobody has switched off is on**, so a fresh settings file writes
  transmit telemetry without him doing anything.
- The file lands in `%AppData%\Hamlet\telemetry\<yyyy-MM-dd>.jsonl`.

Unit 264 proved this off disk through the application's own predicate, with two
sends in two distinct slots, and checked HM-DEC-018 by hand: the line names
nobody — callsigns, grids and both messages all absent, `messageLength` the only
thing said about the text.

**Nothing missing.** He will have to open the file to see it; there is no telemetry
viewer in the application and step E does not ask for one.

### 2.2 The row that reads complete — **and the line under the waterfall that now says so too**

- `src/Hamlet.RadioEngine/Contacts/Ft8ContactState.cs:117` —
  `public static Ft8ContactRead Read(Ft8StationRecord record, DateTime nowUtc)`,
  and `:186` — `IsComplete`, which does **not** require `73`.
- `src/Hamlet.RadioEngine/Contacts/Ft8ContactLedger.cs:196` — `RecordHeard`, and
  `:225` — `RecordSent`.
- `MainWindowViewModel.cs:8474` — the **one** call site of `RecordSent` in the
  tree, inside `AtSlotBoundaryAsync`, and only where `run.Sent`.
- `MainWindowViewModel.cs:7925` — `_contacts.RecordHeard(row.Message, row.SlotStartUtc)`,
  inside `ContactTextFor`.
- `MainWindowViewModel.cs:7840` — `row = row with { Contact = ContactTextFor(row) };`,
  inside `PlaceRow`.

**The caveat, and it is unit 264's finding restated for a man at a radio rather
than for a test:** `PlaceRow` has exactly two callers — the decoder's door at
`:7787` and `AddDecodeRowForTests` at `:7872` — so **a row's `Contact` cell is
computed once, when the row is placed, and nothing recomputes it afterwards.**
`RecordSent` at `:8474` books the send into the ledger and does not touch any row
already on the table.

What that means on his screen: **the row he right-clicked does not change when his
transmission goes out.** The state moves in the ledger, and the next row from that
station — the next slot he is heard in, fifteen seconds later — reads the new
state. In an exchange that is running, this is invisible: every slot brings a new
row. **In an exchange that has just ended it is not** — after his final message,
with the other station gone quiet, the newest row on the table for that callsign
may be one placed before the send, so the cell he is looking at can read *your
move* or *waiting on him* while the ledger already says complete.

**Unit 270 built the answer, and it is not a display refresh.** Rewriting the
cells of rows already on the table was the obvious repair and it was rejected:
a table of moments that edits its own moments is a worse instrument than one
that is merely incomplete, and `ContactTextFor`'s own contract
(`MainWindowViewModel.cs:7885-7888`) is that a row shows where the contact stood
**in its own slot** and never restates itself. **So the present state went into a
line about the present.**

- `src/Hamlet.App/Views/MainWindow.axaml` — `x:Name="DigitalContactStandsText"`,
  the **last** line in the Send area under the waterfall, below the drive control
  and the measured-level readout. It is the last child of that stack, so nothing
  before `DigitalStopButton` moved and the Stop button is asserted still visible,
  still enabled and inside the window's bounds beside it.
- `MainWindowViewModel.cs` — `DigitalContactStandsLine` and `ContactStandsLine`,
  set in the same `Dispatcher.UIThread.Post` after a boundary has returned that
  unit 269's readout is set in.

**What step E should do about it, corrected.** After his own last transmission,
**he reads the line at the bottom of the Send area under the waterfall.** On a
run at the bench on 2026-09-07 it read, whole:

> Where the contact with W1ABC stands: complete, 0 slots, read at the 16:35:15
> UTC slot. That is what passed between you, counted in slots; it is not advice
> about what to send next, and nothing is closed or withheld by it.

**What it says when the other station has gone quiet.** It stands exactly where
it is. The line is written once, in the slot the transmission went out in, and a
later decode does not move it — which is why it always names the slot it was read
at. If the station never comes back, that sentence is the last word and it is
still true: it says what passed between them and when it was counted. **If the
station does come back, the row that decode places carries the state in its own
slot** — at the bench, `KC3QIS W1ABC 73` heard one slot later placed a row
reading `complete, 0 slots`.

**What it never does.** It does not tell him what to send next, it highlights,
arms and queues nothing, and nothing transmits because a contact reached a state.
Nothing is closed, hidden, greyed or filtered because a contact is complete: the
menu after `complete` offers exactly the five messages it offered before. A CQ
books nobody, so after the CQ button the line names no station and invents no
contact.

**And the row is still a record of its own slot, deliberately.** The cell he
right-clicked will still read what was true when that row arrived. That is not a
bug to be reported on the evening; it is what lets him watch a contact progress
down the table.

---

## Criterion 3 — what he saw, and anything that surprised him, recorded

**This one leans on nothing in the application**, and that is the correct answer
rather than a gap. It is a fact about an evening, written by the operator, and
`SHACK_FACTS.md`'s own rule is that a fact in it outranks any inference a session
draws from its own reading.

What the application can give him to write down, with a file and a line for each:

| What | Where it is on the screen | Where it comes from |
|---|---|---|
| The message he sent and its slot | `MainWindow.axaml:3128`, `DigitalSendReservedLine` | `MainWindowViewModel.cs:8513` (`WentLine`) |
| The level Hamlet composed at | the same line | `LevelLine`, `MainWindowViewModel.cs:8578` |
| The level the sound card was handed, and what it clamped | `MainWindow.axaml:3257`, `DigitalTransmitLevelText` | `MeasuredLevelLine`, `MainWindowViewModel.cs:8248` |
| What the drive is set to, in percent and dBFS | `MainWindow.axaml:3200` and `:3226` | `TransmitDrive.NoteFor` |
| Where the contact stood in each slot | the `Contact` cell, `MainWindow.axaml:3742` | `Ft8ContactState.Read`, at the row's own slot |
| Where the contact stands after his own last transmission | `DigitalContactStandsText`, the last line in the Send area | `ContactStandsLine`, `MainWindowViewModel.cs`, read at the slot that went out |
| Which slots went out | `%AppData%\Hamlet\telemetry\<date>.jsonl` | `TransmitRecord`, `EventName` `ft8_transmission` |

**Anything missing at the bench? One thing, and it is deliberate:** there is no
place in the application to write any of it down. Hamlet has no log, and logging
is out of this phase by ruling. **Do not build one for step E** — the record goes
into `SHACK_FACTS.md`, by his hand, and that is criterion 3 of step D as well.

---

## What is missing at the bench, all together

**Nothing, as of unit 270.**

The one item this section listed — *a row's `Contact` cell is computed once and
never recomputed*, so the row he clicked does not update when his own
transmission books into the ledger — **is still true and is still deliberate**
(`MainWindowViewModel.cs:7826`, `:7840`). What was missing was not a refresh but
**anywhere at all on the screen that said where the contact stood after his own
last message**, and that is now the last line in the Send area under the
waterfall. Section 2.2 has the wording and what it does when the station has gone
quiet.

**And two things that are correctly absent rather than missing:** no telemetry
viewer, and no log. Both are out of this phase.

## Which of step E's three criteria is still entirely his

**All three.** Unit 270 closed none of them and none could be closed at a bench.

| Criterion | Whose | Did anything move under it? |
|---|---|---|
| 1 — he answers a CQ on 14.074 or 7.074 and completes an exchange | **entirely his** | No. Untouched. |
| 2 — the transmitted slots appear in telemetry and the row reads complete | **entirely his** — his slots, his contact | The criterion did not move. Its bench blocker did: the telemetry half was already proved on disk by unit 264's walk, and the ending where his own last message completes the exchange now has somewhere on the screen that says so. |
| 3 — what he saw and anything that surprised him, recorded | **entirely his** | No. Untouched, and `SHACK_FACTS.md` was not opened. |

---

## What this page does not do

- **It performs no part of step E.** Working a station is Tim's.
- **It builds nothing, adds no test, and changes no product code.**
- **It chooses no frequency, no power and no drive level.**
- **It does not touch `SHACK_FACTS.md`.**
