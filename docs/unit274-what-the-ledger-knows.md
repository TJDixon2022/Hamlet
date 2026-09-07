# What the ledger already knows — work instruction 274, task 1

**Reading only. No dialog, no file, no ADIF.** The point of asking first is that a
field the ledger cannot supply is a field the dialog must leave empty, and finding
that out after building the dialog is how a log entry acquires a plausible value
Hamlet never observed (§0.0).

Everything below was read from the tree on 2026-09-07 at commit `3232dc2`.

---

## The short answer

**Six of the eight facts come out of the ledger. Two do not and must be handed to
it.** Nothing is missing that would leave a record dishonest; what is missing is
missing because **the ledger is a record of messages and not of a radio**, which is
the right shape for it.

| ADIF needs | Where it comes from | Verdict |
|---|---|---|
| His callsign | `Ft8StationRecord.Callsign` — `Ft8ContactLedger.cs:28` | **in the ledger** |
| His grid | derived from `HeardToUs` where the payload `IsGrid` — `Ft8ContactLedger.cs:52`, `Ft8MessageSplit.cs:177` | **derivable** |
| The report **he** sent | derived from `HeardToUs` where the payload `IsReport` — same two | **derivable** |
| The report **the operator** sent | derived from `Sent` where the payload `IsReport` — `Ft8ContactLedger.cs:55` | **derivable, and see the caveat** |
| Time on | `SlotStartUtc` of the first message either way — `Ft8ContactLedger.cs:9` | **derivable** |
| Time off | `SlotStartUtc` of the last | **derivable** |
| The operator's callsign | `Ft8ContactLedger.OperatorCallsign` — `Ft8ContactLedger.cs:175` | **in the ledger** |
| Frequency | **not in the ledger** | **must be handed in** |
| Band | **not in the ledger**, derivable from the frequency by `HfBands.BandFor` — `HfBands.cs:85` | **must be handed in** |
| Mode | **not in the ledger** | **must be handed in** |
| The operator's grid | **not in the ledger** — `OperatorProfile.GridSquare` | **must be handed in** |

---

## What is in it, with the line

**`Ft8LedgerMessage`** (`src/Hamlet.RadioEngine/Contacts/Ft8ContactLedger.cs:9`)
is the unit the whole log is built from:

```csharp
public sealed record Ft8LedgerMessage(
    string Message, Ft8MessageFields? Fields, DateTime SlotStartUtc);
```

**That record is why most of this works.** It keeps the message, its three fields
already split, and the true UTC of the slot — so a report, a grid and a time are
all readable off it without parsing anything a second time.

**`Ft8StationRecord`** (`:19`) holds three lists per station:

- `Heard` (`:40`) — everything from that station, **whoever it was addressed to**.
- `HeardToUs` (`:52`) — the subset addressed to the operator.
- `Sent` (`:55`) — everything the operator sent to that station.

**`HeardToUs` and not `Heard` is what a log entry reads**, and the distinction is
already argued in the file: a station working three others spends most of its
transmissions on somebody else. A report inside one of those is a report to
somebody else, and putting it in a log entry would be a false record of what
passed between these two.

**`Ft8ContactLedger.OperatorCallsign`** (`:175`) is the operator's own call, taken
as a constructor parameter (`:162`) — the app reads `OperatorProfile.Callsign` and
hands it over, which is §0.1 working: the engine is never told a tab exists.

## The two shapes that do the extracting already exist

`Ft8MessageSplit.IsGrid` (`Ft8MessageSplit.cs:177`) and
`Ft8MessageSplit.IsReport` (`:189`) are what `Ft8ContactStates.IsComplete`
(`Ft8ContactState.cs:256`) already counts completeness with — `IsGridOrReport` at
`:294` and `IsAcknowledgement` at `:308`.

**So the log reads a contact the same way the contact column does**, off the same
two predicates, and a station the column calls *complete* is a station whose log
entry has both reports in it. **A second copy of *what counts as a report* would
be a second answer waiting to disagree** with the state on the screen beside it.

## The caveat on the operator's own report

`RecordSent` has exactly one call site in the tree —
`MainWindowViewModel.cs:8658` — and it is on the send path, booked from what
actually went out rather than from what was composed. **So the operator's sent
report is in the ledger only for contacts made through Hamlet's own send path.**

That is correct rather than a gap, and it matters for the dialog: a contact where
he answered on another program, or where the send failed, **has no sent report and
the field is empty**. It must not be filled from what was offered on the menu — an
offer is not a transmission.

## What is not there, and why that is the right shape

**Frequency, band, mode and the operator's grid are facts about the radio and the
station, not about the messages.** The ledger is mode-neutral and dial-neutral on
purpose: CW inherits it when CW send arrives, and a ledger holding a frequency
would be holding the frequency at the moment it was constructed rather than at the
moment of the contact.

All four are available where the dialog is built:

- **Frequency** — `MainWindowViewModel.FrequencyHz`, read from the radio over
  CI-V.
- **Band** — `HfBands.BandFor(frequencyHz)` (`HfBands.cs:85`), returning a
  `CwBand` whose `Name` (`:13`) is the meter band.
- **Mode** — FT8, and this unit is FT8 only by the instruction's own scope.
- **The operator's grid** — `OperatorProfile.GridSquare`.

**One hazard is worth naming now.** The frequency read at *logging* time is the
frequency the dial is on **when he right-clicks**, which is not necessarily the
frequency the contact happened on. If he has retuned since, an entry built from
the live dial would record a band he did not work the station on. The row already
carries `SlotStartUtc`; **what it does not carry is the dial**, so either the row
gains it or the dialog says which frequency it used and lets him see it before he
saves.

## Where the file goes

`SettingsStore.DataFolder` (`AppSettings.cs:645`) is `%AppData%\Hamlet` and has an
**internal setter so a test can redirect the whole path** — the seam unit 235 added
after nine tests rewrote the operator's own `settings.json`. The ADIF file goes
beside `settings.json` there and uses that same seam, so no test can ever append to
his real log.

## FG-004, read before designing

`FUTURE_GOALS.md:295`:

> **FG-004 — Logging and confirmations.** QSO logging with ADIF export; LoTW /
> QRZ / eQSL integration. Table stakes for a daily-driver app, deliberately after
> the decode work that makes this app different.

**It names ADIF and it specifies nothing**, which is consistent with a goal rather
than a plan. Tim's ruling of 2026-09-07 is what settles the shape: ADIF from the
start rather than a native format with an export promised later. The Settings
dialog's own note — *used for display in the app and, later, for your log
(FG-004)* — is the other half, and it is why the operator's callsign and grid are
already verified facts by the time a log entry needs them.
