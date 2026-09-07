READ IN THIS ORDER

A. THE PHASE GOAL. Hamlet works stations on the air. It transmitted on a live
   antenna on 2026-09-07, and since unit 271 every composed message round-trips
   through the decoder.
B. THE STEP AND ITS EXIT CRITERIA. Step E's bench half. It closes no criterion and
   does not claim to; the criteria are yours at the radio.
C. WHAT THIS REPORT ADDS, AND WHETHER IT BEARS ON A OR B. It bears on A: what is
   addressed to you now has its own column and cannot be buried, and the tooltip
   says `He`. **Section 4 has one real ask** — the split does not fit at your
   window size, the left list clips ordinary messages, and choosing what gives is
   yours.

UNIT:       273 — complete at task 5 of 5 — 2026-09-07 18:06
PHASE GOAL: Hamlet works stations on the air.
UNIT GOAL:  Anything addressed to him has its own column and can never be buried, and no tooltip contradicts a rule sitting in its own file.
ADVANCED:   yes — the split is built, the pronoun ruling is discharged and recorded. No step's criterion moved and the instruction says so itself.
NUMBER:     75 px — what the left list has left for the message at 1400 x 1200, against the 180 px an everyday `VP2MAA KC3QIS FN00` needs. There was no such figure before; task 5 is the first measurement of it.
DRIFT:      0 consecutive units without advance  (was 0)

---

## 1. What Claude did

**Complete, five of five.** The named drop candidate was attempted and is the
reason this report has an ask in it. Machine `QUIVERFULL`, `C:\Source\HamLet`,
branch `main`, all pushed.

### The gate

`SHACK_FACTS.md` present, `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs`
present, `CoreHMI.sln` absent, `MURC.sln` absent. **Hamlet confirmed.**
`SESSION.lock` absent, one session in the process list, checked before the order
was read. **This order is a new one and not a reissue** — the first in four that
carries the asks queue inbound, and the pronoun ask unit 271 raised is discharged
by task 3 and leaves the queue.

### Shell refusals, recorded verbatim

**None.** Two heredocs failed on their own content rather than being refused —
this shell will not carry a quoted heredoc containing an apostrophe, and it
collapses a doubled backslash inside one. Both were worked around with the
file-editing tools and a script file. That is a tool fact, not a refusal, and it
is the second unit it has cost time in.

### Task by task

**1, 2 and 4 — the split.** Committed together at `50e9389`, because the empty
line and the shared clear are properties of the split rather than separate
changes. `DigitalDecodes` stays the whole table; `DigitalVisibleDecodes` and a new
`DigitalMineDecodes` are two mirrors of it, filled by **one decision with two
arms** so nothing can land on both.

**3 — the tooltip says `He`.** Committed at `4d147a9`, with the ruling recorded as
**HM-DEC-159**.

**5 — what the split costs.** Committed at `a76b761`. See section 3; it is the
finding of the unit.

### Decisions made on this session's own authority, reproduced in full

**One: the `mine` toggle is retired rather than left inert.** The instruction says
the button leaves the filter row. Removing only the button would have left
`ShowsMine` persisting a setting that changes nothing and `DecodedFilterRule.Wants`
carrying a `mine` arm no caller ever sets true — dead weight that gets re-wired by
accident. So `Wants` is now `Wants(cq, addressee)`, `IsTheOperators` is gone, and
`AppSettings.DecodedShowMine` is kept but marked *written by the migration and read
by nothing*, which is the treatment `DecodedFilter` already had.

**Two: the addressed-to-him rule moved into the engine and is asked once.** Unit
271's contact column already gated on exactly this question. Two copies would
disagree on the screen — a row on the mine side with a blank contact column, or a
state beside a row he cannot find — so it is `Ft8MessageSplit.IsAddressedTo` and
both ask it. **It is the to-field alone**, which is narrower than unit 252's
toggle: that matched either field because you asked there for *his traffic*, and a
contact is two sides.

**Three: half of `TheDecodedListFiltersByCategoryTests` moved rather than being
deleted.** Every test that pressed a `mine` toggle is now asked of the side, in
`TheDecodedAreaSplitsInTwoTests`. What stayed is what is still true.

### One thing this unit got wrong and its own task 5 caught

**The contact column was added to the mine side and not removed from the left
one.** Task 1 says it **moves**. The first commit only added it, so the left row
and its header still carried a 148-pixel column — blank on nearly every row, out
of a list that is now 330 pixels wide. Task 5's measurement is what found it. It
is gone from both grids in `a76b761`.

## 2. What the owner should expect

**What is for you can no longer be buried.**

- **The decoded area is two lists.** Left: everything, or CQ, a toggle. **Right:
  everything addressed to you, always.**
- **The `mine` button is gone from the filter row**, because it is the right-hand
  column now. There is no control that can hide it, and that is deliberate: a
  control that can hide what is addressed to you is a control that can bury the
  one thing you are waiting for.
- **A message is on exactly one side.** Anything addressed to you is on the right
  and not also on the left.
- **A message you sent is on the left, not the right.** You ruled the side is for
  what is addressed to you, and a sent message is not a decode.
- **The right side says so when it is empty** rather than going blank, and its
  header reads `nothing for you yet`.
- **The contact column moved to the right side**, under the message rather than in
  a column of its own, and it is gone from the left where it was blank on nearly
  every row.
- **One clear empties both sides.**
- **The tooltip says `He`.**

**What will look wrong, and one of them is:**

- **The left list truncates.** At 1400 by 1200 its message column is 75 pixels,
  about ten characters, and `VP2MAA KC3QIS FN00` wants 180. **It clips rather than
  wrapping**, so what you will see is a message cut off mid-callsign. That is
  section 4's ask and it was not papered over.
- **The right side wraps instead of clipping**, so it loses space rather than
  characters. It is still short: 162 pixels against the 210 a compound callsign
  with a report needs.
- **A collapsed `For you` panel still carries its count**, as every panel does.

**Build:** clean, 0 warnings, 0 errors, whole solution, ten times.

**Tests:** filtered and foregrounded, all belonging to this instruction.
`TheDecodedAreaSplitsInTwoTests` and `TheDecodedListFiltersByCategoryTests`
**43 of 43**; with `TheTooltipSaysHeTests`, `TheGridSaysWhereTheStationIsTests`,
`TheGridTooltipSaysHowFarTests` and `TheSenderTooltipNamesTheEntityTests`,
**92 of 92**; `WhatTheSplitCostsTests` **1 of 1**. No suite was run and nothing was
backgrounded.

**Not run, and you should know which:** `TheDecodedColumnsLineUpTests` asserts the
left header's column origins equal every row's, and this unit changed both grids
together from six columns to five. It compiles; it lives in the `Views` namespace
whose stall unit 230 documented, and the standing rule keeps a unit out of a suite.
It is named here rather than left to be discovered.

**Pushed to `main`:** `50e9389`, `4d147a9`, `a76b761`. Version
**1.12.127 → 1.12.130**. `Ft8Sharp` did not move.

## 3. What you should see

**1. A slot with a CQ, a third-party exchange and a message to you.** Measured
this session, callsign `KC3QIS`:

```
  left    CQ VP2MAA FK52
  left    K9TC KJ6IX RRR
  RIGHT   KC3QIS W1ABC -12

left summary : 214135 UTC · 2 shown · newest first
mine summary : 1 for you
```

**Exactly one row on the right and the other two on the left.** The test asserts
none is on both as an **intersection of the two collections**, not by the counts
agreeing — two counts can agree while one row sits in both lists and a third has
gone missing.

Turning `CQ` on takes the third-party row off the left and **leaves the right
untouched**:

```
  left    CQ VP2MAA FK52
  hidden  K9TC KJ6IX RRR
  RIGHT   KC3QIS W1ABC -12

left summary : 214135 UTC · 1 shown · 1 hidden by CQ · newest first
mine summary : 1 for you
```

And the contact column now has something to say where it sits: the mine row reads
`your move, 0 slots` and the left row reads nothing at all.

**2. The mine side's empty line, quoted.**

> Nothing addressed to you yet. Anything a station sends to your callsign lands
> here, and it stays out of the list on the left so it can never be buried.

With no callsign in Settings it says something different, because nothing can be
addressed to a callsign the app has never been told and *nobody has called you*
would be a claim about the band when the truth is a gap in Settings:

> Hamlet does not know your callsign yet, so it cannot tell which messages are for
> you. Put it in Settings, under Operator, and anything addressed to you will
> appear here.

**3. The grid tooltip, reading `He`.**

> IK4LZH is calling anyone. He is in northern Italy, in grid JN54, 4,400 miles
> away on a bearing of 53 degrees.

Nothing else about the wording moved. The country still comes from the callsign and
never from the grid, the compass qualifier is still the committed table, and where
the DXCC table declines there is no country at all:

> VK9XYZ is calling anyone. He is in grid QG44, 9,500 miles away on a bearing of
> 275 degrees.

**The rule came out of `Ft8Vocabulary` rather than gaining an exception**, and one
of task 3's tests reads the source file rather than calling the code — what it
guards is a contradiction between a comment and the code beneath it, which is
invisible to every test that only calls the code.

**4. What the split costs.** Measured through the real window, headless, at your
own 1400 by 1200:

```
decoded area : x=700 y=520 w=671 h=615
  left list  : x=705 y=520 w=330 h=615     message column   75 px
  mine list  : x=1040 y=520 w=331 h=615    message column  162 px

   120 px  CQ W4/YV7AXM
   100 px  IS0/IK2YCW
   210 px  KC3QIS IS0/IK2YCW -12
   210 px  W4/YV7AXM KC3QIS R-15
   180 px  VP2MAA KC3QIS FN00
   320 px  W4/YV7AXM/QRP W4/YV7AXM/QRP R-15   (the widest a standard message can be)
```

**It does not fit, and the left side is the worse of the two.** 75 pixels is about
ten characters of 12-point Consolas. The two sides also fail differently: **the
mine row declares `TextWrapping="Wrap"` and grows a line; the left row declares
none and clips.**

## 4. What's blocking us

**One real ask, and two things stated rather than asked.**

**1. The message column does not fit, and what gives is yours to choose.**

> **Ruling wanted: which of these gives, so the decoded lists stop clipping a
> callsign.**

The instruction is explicit that a column is not to be shrunk silently and that the
options come back to you, so nothing was changed to hide it. **A clipped callsign
is a station misidentified**, which is §0.0 on the screen rather than a cosmetic
complaint, and it is live now.

The options, with the numbers behind each:

| What gives | Left message column | Cost |
|---|---|---|
| **Wrap the left list**, as the right already does | 75 px, but nothing lost | Rows grow to two lines on a busy band; no information lost at all |
| **Drop `dt` and `hz` from the left list** | 75 → 177 px | Two measurements leave the everything list; still 33 px short of the longest |
| **Give the decoded area 60 per cent of the tab** instead of 50 | 75 → ~145 px | The waterfall narrows by about 130 px |
| **Split the two lists unevenly**, left 60 / right 40 | 75 → ~140 px | The right list drops to about 260 px and starts wrapping more |
| **A wider window** | ~200 px at 1920 | Nothing in the app changes; it is where you sit |

**Wrapping is the only one that loses nothing**, and it is what the right side
already does, so the two sides would then behave the same way. It is not applied
because rows changing height on a busy band is a real cost and it is a choice about
his screen rather than a defect with one right answer.

**2. `CLAUDE.md` §1 has stopped indexing the decision log, and this unit did not
fix it.**

Stated, not asked. The table's newest row is **HM-DEC-152**; rulings **153 to 158**
are in `DECISIONS.md` and not in the index. HM-DEC-159 was **deliberately not added
either**: `DecisionLogOrderTests` asserts the only missing ids up to the maximum are
105 and 136, so adding 159 would raise the maximum and fail the assertion on six
rulings this unit did not write. Filling the gap is somebody's half-hour and it is
not this unit's (§12.6).

**3. `TheDecodedColumnsLineUpTests` is expected to need a look and was not run.**

It asserts the left header's column origins equal every row's, and both grids
changed from six columns to five together, so it should still hold — but *should*
is not *does*, and it lives in the `Views` namespace the standing rule keeps a unit
out of.

### Asks still outstanding

Carried outbound so the next order can carry it inbound, per HM-DEC-139.

1. **The message column does not fit.** Raised by unit 273, 2026-09-07. Waiting on
   your choice between the five options in section 4 item 1. **The change is not in
   the tree**: nothing was shrunk or wrapped, and the left list clips today.
2. **Two issues of one work-instruction number.** Raised by unit 271, and by unit
   252 before it. Recorded so the phase record's collision is explained when either
   is cited. **No unit action.** Unchanged this unit.
3. **`PM95` reads *southern Japan***, raised by unit 271 as the compass qualifier's
   weakest reading. **Not a defect**; the table is where to argue with it.
   Unchanged this unit.
4. **`HM-OPEN-083` and `HM-OPEN-084`**, raised 2026-09-05, step 6's two unmet exit
   criteria from the closed sensitivity phase. By HM-DEC-140 they live in
   `OPEN_ISSUES.md` and not on this queue; named once so the next session stops
   rediscovering them.
5. **`CLAUDE.md` §1 stops indexing at HM-DEC-152.** Raised by unit 273. Section 4
   item 2 says why 159 was not added either.

**Dropped this unit: the pronoun.** Raised by unit 271, ruled by you on 2026-09-07,
discharged by task 3 and recorded as HM-DEC-159. It does not carry forward.
