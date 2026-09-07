# What unit 275 measured — 2026-09-07

Two measurements the report quotes, kept here because `output.md` is overwritten
every unit and a figure that only ever lived there is a figure nobody can check
next month.

---

## Task 1 — the left list's message column

Measured through the real window, headless, at the operator's own 1400 by 1200,
by `WhatTheSplitCostsTests`.

| | before | after |
|---|---|---|
| left list width | 330 px | 330 px |
| **left message column** | **75 px** | **177 px** |
| mine list width | 331 px | 331 px |
| **mine message column** | **162 px** | **224 px** |

The left figure is Tim's ruling of 2026-09-07 — `dt` and `hz` off that list,
chosen from five options with the numbers behind each. **The prediction was 177 px
and it is 177 px.** The mine figure is task 2's incidental gain: the `from` column
went, for reasons in that task's own record.

**What fits and what does not**, against the same messages unit 273 measured, at
12-point Consolas:

```
   100 px  IS0/IK2YCW                          fits both
   120 px  CQ W4/YV7AXM                        fits both
   180 px  VP2MAA KC3QIS FN00        left: 3 px over    mine: fits
   210 px  KC3QIS IS0/IK2YCW -12     left: 33 px over   mine: fits
   210 px  W4/YV7AXM KC3QIS R-15     left: 33 px over   mine: fits
   320 px  W4/YV7AXM/QRP W4/YV7AXM/QRP R-15    over on both
```

**The everyday eighteen-character message is three pixels over on the left.** That
is not what the ruling's arithmetic predicted — it predicted ordinary traffic
fitting — and it is reported rather than rounded away. **Three pixels is inside
the uncertainty of a headless font measurement**: the figure is a `FormattedText`
width taken with a fallback typeface in a test host, not a render on his screen.
It is on the boundary, and the honest statement is that it is on the boundary.

Nothing else was shrunk to hide it, which is the instruction's own rule.

**The last row is the widest a standard FT8 message can be** — thirteen characters
a callsign field, twice, plus a report. It does not fit either side and no
arrangement of a 330-pixel column would make it.

---

## Task 5 — what the log's frequency was before this

**There are no records.** `%AppData%\Hamlet\contacts.adi` does not exist on this
machine: the folder holds `settings.json`, `layouts.json`, `scan-segments.json`,
`spots.db` and `telemetry`, and no log.

So **nothing predates task 3**, and that is worth recording rather than passing
over. Unit 274 built the dialog, the writer and the file yesterday; unit 275 fixed
the frequency today; **and the first contact has not been logged yet.** The
defect — an entry recording the dial at logging time rather than at contact time
— never reached a record.

The same is true of task 3's other find. **No entry in his log carries `20 m`
where ADIF wants `20m`**, because there is no entry.

**Nothing was edited and nothing was written.** A log record is a statement the
operator made and it is not a unit's to revise; what was read here is a file
count, and the file is not there.
