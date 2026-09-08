# What the log already gives back — work instruction 278, task 1

**Reading only. Nothing was changed to produce any of this, and nothing was written
to any log.** Read from the tree on 2026-09-08 at commit `da49c69`, version
1.12.149 — read rather than assumed, and it is what unit 277 left.

---

## The short answer, and one mismatch to report first

**`AdifLog.Read` never stops and never throws.** It returns every record it finds,
in file order, and cannot be derailed by one bad field. **But it has three silent
behaviours the window has to expose rather than inherit**, and they are the whole of
task 2's *shown as malformed rather than skipped silently*.

**And the count the instruction asks for is zero, because the file is not here.**

## The mismatch: there is no `contacts.adi` on this machine

`%AppData%\Hamlet\` on this machine holds `layouts.json`, `scan-segments.json`,
`settings.json`, `spots.db` and a `telemetry\` folder. **There is no
`contacts.adi`**, and a search of the whole of `%AppData%` finds none.

**This is the expected state and it is not a finding.** `SHACK_FACTS.md` is
explicit: there are two computers, this is the development machine, no radio has
ever been attached to it, and *"an absent capture folder, an empty telemetry file, a
missing sidecar or a capture count of zero on the development machine is the
expected state. It is not a finding, not evidence of a defect."* A contact log is
written by logging a contact, which happens where the radio is.

**So the instruction's opening premise cannot be checked from here.** *He has logged
contacts and cannot see them* may well be true on the shack machine; **this session
has no way to know and does not claim either way.** What follows is read from the
code, and the record count on this machine is **zero, because the file does not
exist**.

**What this costs the unit:** section 3 is asked for *his own log as the window
draws it, the real records on this machine*. The honest answer is the empty state,
and that is what will be shown, beside a synthesised set that exercises the missing
field. Nothing will be invented and called his.

## What `AdifLog.Read` returns, field by field

`AdifContact` carries thirteen properties, **every one of them nullable**, which is
what lets a missing field stay missing:

| Property | ADIF tag | Type |
|---|---|---|
| `Call` | `CALL` | `string?` |
| `StationCallsign` | `STATION_CALLSIGN` | `string?` |
| `StartedUtc` | `QSO_DATE` + `TIME_ON` | `DateTime?` |
| `EndedUtc` | `QSO_DATE` + `TIME_OFF` | `DateTime?` |
| `Band` | `BAND` | `string?` |
| `FrequencyMhz` | `FREQ` | `double?` |
| `Mode` | `MODE` | `string?` |
| `ReportSent` | `RST_SENT` | `string?` |
| `ReportReceived` | `RST_RCVD` | `string?` |
| `GridSquare` | `GRIDSQUARE` | `string?` |
| `MyGridSquare` | `MY_GRIDSQUARE` | `string?` |
| `Comment` | `COMMENT` | `string?` |

**A tag that is not in this list is read and discarded.** The parser puts every
field into a dictionary and `From(fields)` asks for thirteen names; anything a
different logger wrote is skipped without complaint. Worth knowing, and not this
unit's to change.

## Does it stop at the first record it cannot parse? No, and here is what it does instead

**It returns everything and stops only at the end of the text.** There is no
`throw`, no early `return`, and no record is abandoned because a neighbour was bad.
Three behaviours matter to task 2:

1. **A malformed tag is skipped and the record survives without that field.**
   A tag with no colon, or a length that is not an integer, hits
   `if (parts.Length < 2 || !int.TryParse(...)) { i = close + 1; continue; }`. The
   parser steps over the tag and **keeps going inside the same record**, so a
   contact with a broken `BAND` comes back as a contact with no band. **On screen
   that is indistinguishable from a pre-275 record that never had one**, which is
   exactly the confusion task 2 says to avoid.
2. **A record with no `<EOR>` is dropped entirely and silently.** Fields accumulate
   into the dictionary and are only turned into a contact when `EOR` is reached, so
   a file truncated mid-record loses that record with no trace. **That is the
   truncation case**, and it is invisible to any caller of `Read`.
3. **An `<EOR>` with nothing before it produces a contact with thirteen nulls.**
   It is a record as far as the parser is concerned and it will be counted.

**None of these is a defect in `AdifLog`** — it is a reader, and returning what it
could read is right. **They are things the window must say**, and the only one it
can say from `Read`'s current output is the third. **Items 1 and 2 are invisible
downstream**, which is a finding task 2 has to answer.

## What a record made before unit 275 is missing

**`FREQ` and `BAND`.** Unit 275 made a contact record the dial it was heard on;
before that the dialog wrote the frequency the radio happened to be tuned to at the
moment of the right-click, and unit 274's own report named the fix as another unit's
work. `Ft8ContactLogEntry` omits both where the row carries no dial, and **`Record`
omits an absent field entirely** rather than writing an empty one — the rule this
project follows throughout.

So a pre-275 record has `Band = null` and `FrequencyMhz = null`, and **that is
correct rather than damaged**: Hamlet did not observe where the contact happened.
The window shows it as absent.

**Everything else survives**, because the other twelve fields were written from unit
274 onward and the format is forward compatible: a reader takes fields by name.

## Where the *worked* mark reads the log, and when

| | |
|---|---|
| The read | `ContactLogStore.Read()` -> `AdifLog.Read(File.ReadAllText(path))` |
| Its one caller | `MainWindowViewModel.ReadWorkedBefore()` |
| First read | **lazily**, `_workedBefore ??= ReadWorkedBefore()` at `:8501`, on the first decode that needs the mark |
| Re-read | `RefreshWorkedBefore()` at `:9278`, immediately after a contact is logged |
| Test seam | `ReloadContactLogForTests()` |

**It reads the whole file once and keeps a dictionary**, which unit 274 proved by
deleting the file and watching the mark still land.

### The one thing that makes task 3 more than a property

**`ReadWorkedBefore` throws records away, twice over:**

```csharp
foreach (var entry in ContactLogStore.Read())
{
    if (!string.IsNullOrWhiteSpace(entry.Call))
    {
        worked[entry.Call.Trim()] = entry;
    }
}
```

- **A record with no `CALL` is dropped**, and
- **the dictionary is keyed by callsign**, so working the same station on three
  bands leaves **one** entry.

**So `_workedBefore.Count` is the number of distinct callsigns, and the ruling is
that every logged contact counts.** `_workedBefore.Count` would have been the
obvious thing to reach for and it is **the wrong number** — three contacts with one
station would read as one, which is a false claim about his own operating and
exactly the §0.0 exposure this unit's instruction names.

**The fix is one line's worth of restructuring**: `RefreshWorkedBefore` keeps the
list `ContactLogStore.Read()` returns, derives the dictionary from it as now, and
takes the count from the list. **One read, two derivations**, which is what task 3
asks for and avoids the third pass over the file.

---

## What this means for the rest of the unit

- **Task 2 needs one thing `Read` does not give it**: whether a record was
  well-formed. A window that shows a broken `BAND` as *absent* is telling him
  Hamlet never observed the band, when in fact it wrote one and cannot read it
  back. Answering this properly needs the reader to report what it skipped, which
  is a change to `AdifLog` and is the one place this unit touches the engine.
- **Task 3 must not use `_workedBefore.Count`**, for the reason above.
- **Task 5's synthesis is the only way to see the window with rows in it** on this
  machine, and it must go in a redirected `DataFolder` and never near a real log.
- **Nothing here needs a second parser**, and nothing needs `src/Ft8Sharp/`.
