# What the view can already reach — work instruction 277, task 1

**Reading only. Nothing was changed to produce any of this.** Read from the tree
on 2026-09-07 at commit `515d638`, version 1.12.143 — which is the version the
instruction expects, read rather than assumed.

---

## The short answer

**Three of the four things this unit needs already exist and are reachable.** The
sent message's text is kept, the other station's slot parity is derivable to the
second, and the corrected clock is one call. **What is missing is only the view.**

That changes the shape of tasks 2 and 3 considerably: task 2 does not have to build
storage, and task 3 does not have to build a clock. Both are surfacing work.

---

## 1. Does the text of a sent message survive where the view can read it?

**Yes. In the ledger, with its slot, in full.** Not in telemetry, which cannot hold
it by construction.

**The one call site**, `MainWindowViewModel.cs:8881`, inside `AtSlotBoundaryAsync`:

```csharp
if (run.Sent)
{
    _contacts?.RecordSent(text, result.Send!.SlotStartUtc);
}
```

`Ft8ContactLedger.RecordSent` (`:225`) parses the message, finds the station it was
addressed **to**, and appends to that station's record:

```csharp
public sealed record Ft8LedgerMessage(
    string Message, Ft8MessageFields? Fields, DateTime SlotStartUtc);
```

So a sent message is kept as **its exact text, its parsed fields, and the slot
boundary it went out in** — every field the interleaved rows need. It reaches the
view through `Ft8StationRecord.Sent`, an `IReadOnlyList<Ft8LedgerMessage>`, beside
`Heard` (everything that station sent) and `HeardToUs` (the subset addressed to the
operator).

**Telemetry keeps only the length, and that is deliberate and enforced.**
`TransmitRecord` holds `MessageLength` and no string field at all — *"not one of its
parameters is a string ... so `Ft8Transmission.Text` and `ReadsBackAs` have nowhere
to be put, not by accident, not in a hurry, and not behind a flag"* — and
`ATransmitRecordCannotCarryTheMessage` asserts it **by reflection**. HM-DEC-018.

**Task 2 must not disturb that.** The ledger is in-memory application state, not the
telemetry record, and putting the text on screen from the ledger keeps HM-DEC-018
exactly as it stands. **No sent text may travel into a telemetry bag.**

### Three limits on what the ledger holds, which task 2 has to live with

1. **Only what actually went out is booked.** `RecordSent` is called where
   `run.Sent` is true and nowhere else, deliberately — the remark says a message
   booked at arming *"would put a transmission in the ledger that a licence
   refusal, a cancel or a missed boundary meant never happened."* A stopped or
   refused send is correctly absent. **Rows come from what was transmitted, not
   from what was clicked.**
2. **It is booked against the station the message was addressed to.** A `CQ` has no
   addressee, so `Book(fields.To)` has nothing to key on. **Whether a sent `CQ`
   lands in any station's record needs checking in task 2**, and if it does not,
   the conversation view simply has no `CQ` row to draw, which is honest.
3. **`_contacts` is built lazily by the first row that arrives** (`:7999`) and is
   private, with `ContactRecordForTests` (`:8039`) the read-only seam. It is null
   until something has been heard.

## 2. What does the ledger know about whose slot is whose?

**Enough to derive the parity exactly, with no guessing and no second source.**

Every `Ft8LedgerMessage` carries `SlotStartUtc`, and `Ft8Slots.SlotStart` (`:182`)
truncates the second-of-minute to a multiple of 15. So a slot boundary's second is
always one of `0, 15, 30, 45`, and

```
parity = (SlotStartUtc.Second / 15) % 2
```

is `0` for `:00` and `:30` and `1` for `:15` and `:45`. **A minute holds exactly
four slots, an even number, so the parity is stable across minute and hour
boundaries** — there is no wrap to get wrong.

**The other station's parity is the parity of any message in `Heard`.** The
instruction's own example checks out: a station that sent on `:15` and `:45` has
parity 1, so the operator's slots are parity 0 — `:00` and `:30`.

**And `Heard` is the right list to read, not `HeardToUs`.** A station's own slot
choice is a fact about that station's transmissions whatever they were addressed to,
and reading only the subset addressed to the operator would throw away the evidence
of a station Hamlet has watched calling somebody else. That is the same
sender-versus-pair distinction unit 274's ADIF grid bug turned on.

**Where there is nothing in `Heard`, there is no parity**, and §0.0 says the panel
says so rather than picking one. `Ft8StationRecord.LastHeard` is null in exactly
that case, so the unknown is already representable and does not need inventing.

**`SlotsSinceHeard(nowUtc)` and `SlotsSinceSent(nowUtc)` (`:70`, `:76`) already
exist** and return `int?`, which is what task 4's *how long since it last
transmitted* wants, in the unit the beat is actually measured in.

## 3. Where does the corrected clock come from?

**One source, already used by the send path.**

```csharp
var trueUtc = Ft8Slots.TrueUtc(DateTime.UtcNow, ClockOffset) ?? DateTime.UtcNow;
```

`MainWindowViewModel.cs:8770` in `SendMessage` and `:8825` in the tick both read it
that way. `Ft8Slots.TrueUtc` (`:174`) **returns null when the offset is unknown** and
corrects a reading without ever writing a clock.

**Seconds remaining is arithmetic on what is already there:**

```
Ft8Slots.IntoSlot(trueUtc)              // 0 to 15, seconds since the boundary
Ft8Slots.SlotSeconds - IntoSlot(...)    // what is left
```

`SlotSeconds` is `15` (`:126`) and `TransmissionSeconds` is `12.64` (`:135`) — worth
noting, because the honest deadline for *starting* a transmission is not the end of
the slot.

**The countdown has a tick to ride and does not need a new timer.** `_decodeTimer`
runs at **250 ms** (`:3324`) and already calls `OnSlotTick()` (`:4959`), with the
existing remark: *"Four looks a second is sixty inside every fifteen-second slot."*
That is ample for a one-second-resolution countdown, and adding a second timer would
be a second timing source, which the instruction forbids.

**The `?? DateTime.UtcNow` fallback is a hazard for task 3 and must not be copied.**
It is defensible in the send path, where a slot boundary has to be picked for
something the operator has already clicked. **A turn line has no such obligation**:
where the offset is unknown, §0.0 says the panel says the clock is not measured,
rather than counting down from the PC clock and presenting it as the beat. The
existing `ClockOffset.IsKnown`, `IsConcerning` and `Describe` (`:27`, `:56`, `:80`)
are already there for saying so.

## 4. What is the For you panel bound to today?

**`DigitalMineDecodes`**, an `ObservableCollection<DigitalDecodeRow>` at
`MainWindowViewModel.cs:1122`, bound by `ItemsControl x:Name="DigitalMineRows"` in
`MainWindow.axaml:3966`.

| Piece | Where |
|---|---|
| the collection | `:1122` |
| filled incrementally on arrival | `:1464`, `DigitalMineDecodes.Insert(SideIndexOf(at, IsForHim), row)` |
| rebuilt wholesale on reset | `:1527`–`:1533` |
| count, summary, emptiness | `:1336`, `:1369`, `:1557`–`:1565` |
| what decides a row belongs | `IsForHim`, the same `Ft8MessageSplit.IsAddressedTo` that gates the Log item |

A row is a `DigitalDecodeRow(Utc, Snr, Dt, Hz, Message, ObserverGrid, SlotStartUtc,
Contact, HeardOnHz)` — **already carrying `SlotStartUtc`**, which is what lets a sent
row and a received row be interleaved in time order without a parallel structure.

**So tasks 3 and 4 extend this rather than replacing it.** The collection, the
insert-in-order helper, the summary and the context menu all stay; what changes is
what may go into the collection and what sits above it.

**One thing to watch:** `SideIndexOf(at, IsForHim)` counts a row's position from the
whole decoded table, so it keeps the mine list in step with the master list's order.
**A sent row has no place in the master decoded table** — it is not a decode — so
task 2 has to decide where the ordering comes from for a row that only exists on
this side. That is the one real design question the reading turned up.

---

## What this means for the rest of the unit

- **Task 2 is a view, not a store.** The text, the slot and the parsed fields are
  all in the ledger already. The work is drawing them, keeping `snr` and `dt`
  empty, and answering the ordering question above.
- **Task 3 is arithmetic on two existing calls**, plus an honest unknown in three
  distinct cases: no station heard yet, no clock offset measured, and a stale
  offset.
- **Task 4's *how long since* is `SlotsSinceHeard`**, which already exists.
- **Nothing here needs a line of `src/Ft8Sharp/`**, and nothing needs a new timer,
  a new clock, or a new persistence path.
