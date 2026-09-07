READ IN THIS ORDER

A. THE PHASE GOAL. Hamlet works stations on the air. It transmitted on a live
   antenna on 2026-09-07 and can write a contact down since unit 274.
B. THE STEP AND ITS EXIT CRITERIA. Step E's bench half. It closes no criterion and
   does not claim to.
C. WHAT THIS REPORT ADDS, AND WHETHER IT BEARS ON A OR B. It bears on A: the left
   list stops clipping callsigns, a log entry records the dial the contact was
   actually heard on, and the decision index is whole again. **Three of the four
   asks the order carried inbound are discharged and leave the queue.** Section 4
   has no ask; two things are stated.

UNIT:       275 — complete at task 5 of 5 — 2026-09-07 19:38
PHASE GOAL: Hamlet works stations on the air.
UNIT GOAL:  The left list shows whole callsigns, a logged contact records the dial it actually happened on, and the decision index is whole again.
ADVANCED:   yes — three inbound asks discharged and two defects found that no task went looking for. No step's criterion moved and the instruction says so itself.
NUMBER:     75 px → 177 px, the left list's message column. Predicted 177 and it is 177.
DRIFT:      0 consecutive units without advance  (was 0)

---

## 1. What Claude did

**Complete, five of five.** The named drop candidate was attempted. Machine
`QUIVERFULL`, `C:\Source\HamLet`, branch `main`, all pushed.

### The gate

`SHACK_FACTS.md` present, `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs`
present, `CoreHMI.sln` absent, `MURC.sln` absent. **Hamlet confirmed.**
`SESSION.lock` absent, one session, tree clean at `0eaf738` with unit 274 fully in
at 1.12.134.

### Shell refusals, recorded verbatim

**None.** The same two tool facts as the last two units and no new ones: this
shell will not carry a quoted heredoc containing an apostrophe, and it collapses a
doubled backslash inside one. Both worked around with script files.

### Task by task

**1 — `dt` and `hz` leave the left list.** 75 → 177 px, exactly the predicted
figure.

**2 — the column test is run.** It answered the instruction's question by
measurement and then found something nobody was looking for.

**3 — a contact remembers its own dial.** And, on the way, a defect unit 274
shipped.

**4 — the decision index is whole again.** All seven rulings read and indexed.

**5 — what the log's frequency was before this.** There is no log. See section 3.

### Two defects found that no task went looking for

**One: the mine list had been crooked since I built it, by twelve pixels.** Task
2's whole point is that the column test had not run for two units. Run, it showed
the mine header putting the message column at x=138 and its rows putting it at
150. **Cause, measured cell by cell**: `from` was the only column on either side
whose content has no bound, so a six-character callsign pushed the row's column
past its header's — the sibling-grid drift unit 241 wrote that entire test class
about, reproduced by me in unit 273 and never seen because the test only ever
examined the left list.

**Two: unit 274 was writing a band ADIF cannot read.** Pushing the application's
own band name through the log for the first time produced `<BAND:4>20 m`.
`HfBands` names bands for the screen and the log took the name straight through;
ADIF's Band enumeration gives `20m`, and a logger has no row for `20 m`.

**Why unit 274's round trip missed it, which is the lesson rather than the bug.**
Every test in that file wrote a hand-made `"20m"` and read back `"20m"` — the
writer and the reader agreeing perfectly about a value **the application never
produces**. That is §12.5 in miniature: a fixture built from the same assumption
as the code proves nothing about the code. What caught it was task 3 sending the
real name through for the first time.

### Decisions made on this session's own authority, reproduced in full

**One: the `from` column left the mine list rather than being widened.** It was
the cause of the misalignment, it duplicated the message beside it — which already
opens with the sender — and widening it to fit a compound callsign would have cost
the message about forty pixels on the side that had least. The sender's entity
tooltip moved to the message cell and the `worked` mark moved under the time,
where the 76 pixels the clock already has carry it for nothing. **The mine message
column went 162 → 224 px** as a consequence.

**Two: the tone test is kept as a skipped record rather than deleted.** `hz` left
the left list on your ruling and was never on the mine side, so there is no `hz`
cell in the application to align and the test has no subject. Deleting it would
destroy the record of a real fault and its fix; leaving it running would report a
property nothing has. It carries what it proved and why it mattered.

**Three: `AdifLog.BandValueFor` removes the space and nothing else.** Every ADIF
band value is a number and a unit run together and every `HfBands` name is the
same two with a space between; it is a format conversion rather than a
translation. A name it cannot recognise returns null and the field is left out —
the rule every other field in this log already follows.

### One tree mismatch, reported and not repaired

**Task 1 says "the mine list keeps both [`dt` and `hz`]". It never had them.**
Unit 273 built that side as `utc`, `from`, `message`. There was nothing to keep,
and after task 2 it is `utc`, `message`.

## 2. What the owner should expect

**Whole callsigns on the left, and a log that records the band you actually
worked.**

- **The left list drops `dt` and `hz`.** It keeps the time, the signal report, the
  `worked` mark and the message, and the message column more than doubles.
- **The mine list keeps everything that was on it and gained room too** — its
  `from` column went, because the message beside it already names the sender, and
  that took its message column from 162 to 224 pixels.
- **Hovering a sender on the mine side still names the country**; that tooltip
  moved onto the message cell rather than going away.
- **A logged contact records the frequency it was heard on**, not the dial you
  happen to be on when you right-click. The dialog's frequency field says *where
  this was heard* instead of *where the dial is now*.
- **A row decoded before this change has no dial**, and its entry carries no
  frequency and no band at all rather than a plausible one. The dialog says the
  dial was not recorded.
- **The band in the file is now `20m`**, which is what ADIF spells it. It was
  `20 m`, which a logger cannot read.
- **`CLAUDE.md`'s decision table carries rulings 153 to 159 again**, including the
  pronoun one.

**What will look wrong and one thing that is:**

- **`VP2MAA KC3QIS FN00` is three pixels over on the left.** The ruling's
  arithmetic predicted that ordinary traffic would fit; the everyday
  eighteen-character message does not, by three pixels. Nothing was shrunk to hide
  it. Three pixels is inside the uncertainty of a headless font measurement, so it
  is on the boundary rather than clearly failing — see section 3.
- **The two lists carry different columns.** That is deliberate and it is why each
  header is now checked against its own rows.
- **The `worked` mark on the mine side sits under the time**, not beside the
  contact line, since the `from` column went.

**Build:** clean, 0 warnings, 0 errors, whole solution, fourteen times.

**Tests:** filtered and foregrounded. `WhatTheSplitCostsTests` **1 of 1**;
`TheDecodedColumnsLineUpTests` **2 passed, 1 skipped**;
`AContactRemembersItsOwnDialTests` **5 of 5**; `TheAdifLogRoundTripsTests` and
`TheLogEntryIsWhatWasHeardTests` **37 of 37**; `DecisionLogOrderTests` **2 of 2**.
Every one is a test this unit wrote or rewrote, which is what the instruction
permits. Nothing was backgrounded and no suite was run.

**Pushed to `main`:** `8c1c806`, `3649175`, `df65f2e`, `00a7e4b`, `cb3fa58`.
Version **1.12.134 → 1.12.139**. `Ft8Sharp` did not move.

## 3. What you should see

**1. The left list's message column, before and after**, measured through the real
window at your own 1400 by 1200:

```
                     before    after
left message column    75 px   177 px
mine message column   162 px   224 px

   100 px  IS0/IK2YCW                          fits both
   120 px  CQ W4/YV7AXM                        fits both
   180 px  VP2MAA KC3QIS FN00        left: 3 px over    mine: fits
   210 px  KC3QIS IS0/IK2YCW -12     left: 33 px over   mine: fits
   210 px  W4/YV7AXM KC3QIS R-15     left: 33 px over   mine: fits
   320 px  W4/YV7AXM/QRP W4/YV7AXM/QRP R-15    over on both
```

**The prediction was 177 and it is 177.** What the prediction also said — ordinary
traffic fits, the rare long one does not — is not quite what came out: **the
everyday eighteen-character message is three pixels over.** It is reported rather
than rounded away, and nothing else was shrunk to hide it.

**Three pixels is inside the uncertainty of the measurement.** The 180 is a
`FormattedText` width taken with a fallback typeface in a headless test host, not
a render on your screen. It is on the boundary; the honest statement is that it is
on the boundary rather than that it fits or that it does not.

The last row is **the widest a standard FT8 message can be** — thirteen characters
a callsign field, twice, plus a report. No arrangement of a 330-pixel column
would hold it.

**2. `TheDecodedColumnsLineUpTests`, and what two units of drift had left in it.**

**Before this unit: nothing.** Measured rather than assumed — I checked out
`0eaf738`'s markup, rebuilt, and ran it: **2 of 2 green.** Unit 273's split and
unit 274's mark column both kept the left header and its rows in step.

**After: the mine side was crooked**, and had been since unit 273 built it:

```
mine header origins : 0, 76, 138
mine row 0 origins  : 0, 76, 150

   header col 1  x=76  w=48   "from"
   row0   col 1  x=76  w=60   "TA3MPK"
```

Fixed, and now:

```
mine header origins : 0, 76
mine row 0 origins  : 0, 76
mine row 1 origins  : 0, 76
```

The class now holds two origins tests — one per side, each header against its own
rows — and one skipped record where the `hz` alignment test used to be.

**3. Two log records.** One for a contact heard on a dial that has since moved:

```
<CALL:6>IK4LZH
<STATION_CALLSIGN:6>KC3QIS
<QSO_DATE:8>20260907
<TIME_ON:6>214130
<TIME_OFF:6>214130
<BAND:3>20m
<MODE:3>FT8
<FREQ:9>14.074000
<RST_RCVD:3>-12
<MY_GRIDSQUARE:6>FN00DJ
<EOR>
```

**`14.074000` and `20m` are where it was heard.** The panel's own live dial reads
7,028,000 Hz in that test, and the entry does not carry it — which is the whole
point of the task. And **`20m` rather than `20 m`**, which is the defect task 3
found on the way.

And one for a row with no recorded dial — anything decoded before this change:

```
<CALL:6>IK4LZH
<STATION_CALLSIGN:6>KC3QIS
<QSO_DATE:8>20260907
<TIME_ON:6>214130
<TIME_OFF:6>214130
<MODE:3>FT8
<RST_RCVD:3>-12
<MY_GRIDSQUARE:6>FN00DJ
<EOR>
```

**`FREQ` and `BAND` are absent, not guessed.** What was heard is still there, so
the absence reads as the missing fact rather than the entry having given up.

**And what the log held before this: nothing.**
`%AppData%\Hamlet\contacts.adi` does not exist on this machine — the folder holds
`settings.json`, `layouts.json`, `scan-segments.json`, `spots.db` and `telemetry`.
**Zero records, so nothing predates task 3.** Unit 274 built the dialog and the
writer yesterday, unit 275 fixed the frequency today, and the first contact has not
been logged yet: **neither defect ever reached a record.** Nothing was edited and
nothing was written; a log record is a statement you made and it is not a unit's to
revise.

## 4. What's blocking us

**Nothing blocks the next unit, and there is no ask.** Two things stated.

**1. Three pixels.**

The left list still comes up three pixels short of the everyday
`VP2MAA KC3QIS FN00`, on a measurement whose own uncertainty is about that size.
**Nothing was shrunk to make it fit**, which is the instruction's rule, and I am
not proposing a change: at this margin the honest move is to look at it on your
own screen, where the real typeface renders, rather than to act on a headless
figure. If a callsign is visibly cut there, the options unit 273 priced are still
priced.

**2. `dt` and `hz` were never on the mine list.**

Task 1 says it keeps both. Unit 273 built that side as `utc`, `from`, `message`,
so there was nothing to keep — and after task 2 it is `utc`, `message`. Reported
rather than repaired, as the instruction directs. **If you want them there**, that
is a different change from the one you ruled, and the mine list now has 224 pixels
of message to spend.

### Asks still outstanding

Carried outbound per HM-DEC-139.

1. **Two issues of one work-instruction number.** Raised by unit 271, and unit 252
   before it. **The author's error: an executed order must never be amended, only
   succeeded.** No unit action; recorded so the phase record's collision is
   explained when either is cited. **Unchanged this unit.**
2. **`PM95` reads *southern Japan***, raised by unit 271. **Not a defect**; the
   table is where to argue with it. **Unchanged this unit.**
3. **`HM-OPEN-083` and `HM-OPEN-084`**, raised 2026-09-05. By HM-DEC-140 they live
   in `OPEN_ISSUES.md` and not on this queue; named once so the next session stops
   rediscovering them. **Unchanged this unit.**

**Dropped this unit, all three discharged:**

- **The message column does not fit.** Raised by unit 273, ruled by you on
  2026-09-07, discharged by task 1. 75 → 177 px, with the three-pixel residue
  above stated rather than hidden.
- **The frequency in a log entry is the dial at logging time.** Raised by unit 274,
  discharged by task 3.
- **`CLAUDE.md` §1 stops indexing at HM-DEC-152.** Raised by unit 273, discharged
  by task 4. All seven rulings read out of `DECISIONS.md` and indexed by what each
  says, and `DecisionLogOrderTests` passes.

**Nothing was added to the queue by this unit.**
