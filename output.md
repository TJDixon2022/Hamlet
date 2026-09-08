UNIT:       278 — complete at task 6 of 6 — 2026-09-08 10:06
PHASE GOAL: Hamlet works stations on the air. Not decodes them, not shows them —
            the operator makes a contact with his own radio.
UNIT GOAL:  He can see his own log inside Hamlet, and the application notices when
            he passes a milestone.
ADVANCED:   no — it advances step E's bench half and closes no criterion, which the
            instruction said itself. Step E's criteria are Tim at his own radio
            working a station, and this unit adds what happens after a contact
            rather than during one.
NUMBER:     0 -> 0 records on this machine, and that is a mismatch rather than a
            result. **There is no `contacts.adi` here.** By `SHACK_FACTS.md` that is
            the expected state on the development box, so the count the instruction
            asked for is zero because the file does not exist, and the premise *he
            has logged contacts and cannot see them* could not be checked from this
            side. What was measured instead, synthesised: **240 bytes an entry,
            59.58 ms to open the window at 10,000 records.**
DRIFT:      1 consecutive unit without advance (was 0). Reseeded by the author at
            the head of this order, which answers the ask two reports had raised;
            incremented here because `ADVANCED` is no.

## 1. What Claude did

**Complete. Six tasks of six, none dropped** — including task 5, the named drop
candidate, which found two defects nothing else would have. Windows, `PROJECT:
Hamlet` claimed and confirmed against all four gate checks, branch `main`, pushed at
every task. Version `1.12.149` -> `1.12.155`.

**Task 1 — reading only** (`docs/unit278-what-the-log-gives-back.md`, `6d987f0`).
Two findings shaped everything after.

- **`AdifLog.Read` never stops and never throws**, but it had three silent
  behaviours. **A field whose length cannot be read was stepped over** and the
  record came back missing it, indistinguishable from a field Hamlet never observed.
  **A record with no `<EOR>` was dropped entirely with no trace.** An `<EOR>` with
  nothing before it is a contact of thirteen nulls and counts.
- **The obvious count was the wrong number.** `ReadWorkedBefore` drops any record
  with no `CALL` and keys its dictionary by callsign, so `_workedBefore.Count` is
  distinct stations. Three contacts with one station would have read as **one**,
  where the ruling is that every logged contact counts.

**Task 2 — the window** (`af47113`). `Tools > My contacts…`. **It reads and never
writes**: the code-behind has no handler at all, so there is no path from it to the
file. **One parser, two doors** — `AdifLog.ReadRecords` does the scanning and `Read`
is written in terms of it, so no second ADI parser exists; the 31 round-trip tests
are green against the rewritten reader. A missing field says `not recorded`, a word
rather than a dash, because a dash in a column of reports reads as a report.

**Task 3 — the count** (`a070a05`). The number of records, quietly at the right-hand
end of the status bar, absent rather than reading zero before he has logged
anything. **One read, two derivations**, which is what the task asked for. **Mutating
the count to the worked-mark dictionary turns two tests red**, so they catch the
mistake rather than agreeing with whatever was written.

**Task 4 — the badges** (`db38915`). Nine thresholds in one list.
**Watched failing by mutation**: making the earn rule report only the highest crossed
threshold turns the instruction's own nine-to-twenty-six case red, `[10, 25]`
becoming `[25]`.

**Task 5 — measured** (`docs/unit278-what-the-log-costs.md`, `63b330b`). See section
3. It found two defects and neither was findable by reading the code.

**Task 6 — the outcome entry** (`56c1258`), written by
`tools/arbiter/outcome-append.bat`, exit 0. **Second unit running the tool has
worked**, after the fourteen in which it was refused. Step E stays `not started`.

**Decisions this session made for itself**, both reproduced in the outcome entry:
that a damaged record is counted rather than excluded, so the status bar and the log
window cannot disagree; and that the first look at the log seeds the badge level
silently. Nothing touching transmit, and the one §0.0 exposure the instruction named
— the count — is settled by a test that fails if anybody reaches for the dictionary.

## 2. What the owner should expect

**He can look at his own contacts without opening Notepad.** `Tools > My contacts…`
lists them newest first with the callsign, the time in UTC, the band, the mode, both
reports, his grid and his notes. **The status bar carries the count**, quietly, at
the right-hand end, and the window says where he stands against the badges and how
far the next one is.

**What will look wrong and is not:**

- **The log will be empty on this machine.** There is no `contacts.adi` here, and
  `SHACK_FACTS.md` says that is expected: this is the development box and a log is
  written where contacts are. **On the shack machine it should list what he has
  logged since unit 274.**
- **Older records say `not recorded` under band.** Records written before unit 275
  carry no band and no frequency, because Hamlet did not observe where the contact
  happened. That is not damage and the row does not mark it as such.
- **A damaged record is listed, marked in amber, rather than left out**, and it
  counts. The status bar and the window will always agree on the total.
- **No badge is announced on first launch**, however many contacts the log already
  holds. That is deliberate and is the fix task 5 found.
- **Inherited reds, untouched and unrun**: `CwAdjudicationTests.ASpeedChangeInRealisticAudio`,
  the 51 CW cases in `docs/unit239-failing-set.txt`, the `Ft8Sharp.Deep.Tests`
  whole-type-list tripwire, and `HM-OPEN-086`.
- **`src/Ft8Sharp/` is untouched** and its version did not move.

## 3. What you should see

**1. His own log as the window draws it.** On this machine there are no records, so
this is what the window actually shows, and it is the empty state rather than a
demonstration dressed up as his:

> **0 contacts logged.**
>
> Nothing here yet. Hamlet writes a line into this log every time you fill in the
> contact dialog after working somebody, and this window is where those lines come
> back. The file lives at `C:\Users\TimDi\AppData\Roaming\Hamlet\contacts.adi`.

**With records in it**, from the tests, including one written before unit 275 and one
the file cut off:

```
3 contacts logged.
One record could not be read whole. It is still listed, marked, rather than left out.

  call     when (UTC)           band  mode  sent  rcvd  my grid  notes
  VP2MAA   not recorded         not recorded ...
      ! the field "TIME_ON" says it is 6 characters and the file ends after 4,
        so it was cut off  this record has no end marker, so the file was cut
        off while it was being written
  W1ABC    2026-09-08 03:15:00  40m   FT8   -11   -08   FN00     second
  K9XP     2026-09-08 02:13:15  20m   FT8   -09   -14   FN00     first one
```

and a pre-275 record, which is sound and simply carries less:

```
  K9XP     2026-09-08 02:13:15  not recorded  FT8   not recorded ...
```

**No fault line under it**, because nothing is damaged: Hamlet did not observe the
band, and saying so is different from saying the file is broken.

**2. The count, and the badge line.**

Status bar, right-hand end: **`3 contacts logged`**

Log window, under the total:

> **You have passed 10 contacts. 15 to go until 25.**

and at the top of the range:

> **You have passed 10,000 contacts. That is every badge there is.**

and at 1,000, from the measurement:

> **You have passed 1,000 contacts. 1,000 to go until 2,000.**

**3. Nine to twenty-six, both badges earned.**

```
9 -> 26 earned: 10, 25
9 -> 26 says:   That is 10 and 25 contacts logged.
```

**Both named, not only the highest.** Mutating `EarnedSince` to take the last one
turns that assertion red with `Expected: [10, 25]` and `Actual: [25]`, which is the
ten silently swallowed.

**What ten thousand costs**, Debug build, so these are the slow figures:

| | 1,000 | 10,000 |
|---|---|---|
| File | 240,272 bytes | 2,400,272 bytes |
| Read and parse | 1.37 ms | **40.09 ms** |
| Build the rows | 0.35 ms | 19.49 ms |
| **Open the window** | 1.72 ms | **59.58 ms** |

The count on the main screen: **49.19 ms first ask, 0.000 ms after.**
**No paging was added and the numbers do not ask for one.** Named as unmeasured: the
`ItemsControl` realizes every row rather than virtualizing, so the rendering half is
still open and is not claimed either way.

**Tests.** 22 new across four classes; 22 in the log-dialog, dial, binding-health
and voice classes; 31 ADIF tests in the engine. **All green**, and every one of them
run because this unit changed code they guard.

## 4. What's blocking us

Nothing blocks the next unit. Two things want a ruling, one is a standing question.

---

**The unit's opening premise could not be checked, and the same will be true of the
next log unit written from this side.**

*He has logged contacts and cannot see them* may well be true, and there is no
`contacts.adi` on the development machine to confirm it. Everything in this unit was
built and measured against synthesised logs, which is sound for the code and proves
nothing about what his actual file contains: how many records, whether any predate
unit 275, whether any is damaged.

**What would settle it in one line**: the record count and the file size from the
shack machine, or the file itself if he is willing to have it in the repository —
which §2.1 would need a ruling on, since a log carries callsigns and is his.

*Rejected: inferring from telemetry.* Nothing on this machine records a contact
having been logged, and the telemetry here is the development box's.

---

**A log window that shows a damaged record cannot offer to do anything about it, and
at some point that will be the obvious next question.**

This unit shows damage and stops there, correctly: **a log record is a statement the
operator made and is not a unit's to revise**, and editing is parked. But a record
the file cut off is not a statement he finished making, and the honest options are
to leave it, to let him delete it, or to let him complete it.

*Rejected: doing anything about it here.* Editing is parked by name in this
instruction, and a window that reads is a window with no path to the file at all,
which is a property worth keeping until there is a reason to spend it.

---

## Asks still outstanding

Carried per HM-DEC-139. Nine inbound; one dropped as answered by this unit, eight
carried, two added.

1. **Two issues of one work-instruction number.** Raised by unit 271, and unit 252
   before it. **The author's error: an executed order must never be amended, only
   succeeded.** No unit action.
2. **`PM95` reads *southern Japan***, raised by unit 271. **Not a defect.**
3. **`HM-OPEN-083` and `HM-OPEN-084`**, raised 2026-09-05. By HM-DEC-140 they live
   in `OPEN_ISSUES.md` and not on this queue.
4. **Three pixels.** Unit 275 left `VP2MAA KC3QIS FN00` three pixels over on the
   left list. **No unit action until Tim says.**
5. **`dt` and `hz` were never on the mine list.** **No unit action until Tim says.**
6. **Where an outcome entry goes when the unit it corrects has none.** Decided by
   unit 276 and already in the tree; **the ruling wants Tim's eye.**
7. **`PHASE_OUTCOME.md` is written by a tool no unit is told to run.** Units 273,
   274 and 275 wrote no entry. Units 277 and 278 both wrote one because their
   instructions named the tool. **The general question stands: it is still not a
   task unless an instruction says so.**
8. **`HM-OPEN-086`** — `TheWholeChainRunsFromOneRightClickTests` asks the left list
   for a row addressed to the operator, which `WantsRow` excludes. **Its own unit's
   work.** Unit 277 fixed the same fault in a sibling class, so there are now two
   precedents for the repair.
9. **Where the repeat fold stops**, raised by unit 277, 2026-09-08. Built the
   conservative way and in the tree at `0c359cc`. **The ruling wants Tim's eye.**
10. **What the turn line says during a transmission already going out**, raised by
    unit 277, 2026-09-08. Nothing is in the tree for it.
11. **The real log cannot be seen from the development machine**, raised by this
    unit, 2026-09-08. The record count and file size from the shack machine would
    settle what this unit could only synthesise.

**Dropped as answered:** unit 277's `DRIFT` ask. The author reseeded it at the head
of this order, which is what two reports had asked for, and it is carried forward in
this report's header block.
