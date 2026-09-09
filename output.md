# Work instruction 299 — the card's header does what it promised

**READ IN THIS ORDER.** One of the four faults was not a fault, and the measurement
that says so is the most useful thing in this report.

A. **The phase goal — FT4 works exactly the way FT8 does.** This unit advanced no
   step of it. It repaired the conversation card's header, which is part of step 4's
   second criterion.

B. **Step 4 and its exit criteria** — pressing FT4 tunes and decodes; the panel, the
   conversation, the ring, the filters, the tooltips, the ledger and the right-click
   menu all working unchanged; one click, one transmission; a whole exchange from one
   right click at the bench. **None was measured tonight.** Step 4 stays `partial`
   exactly where unit 297 left it, and its second criterion now describes a card that
   carries a working `i`, a globe, a short place name and an always-available Log.

C. **The report last, and section 4 raises 3 items.** None blocks the next unit. One
   is a ruling, one is a template defect that has now bitten three units running, and
   one is a limitation named rather than repaired.

```
UNIT:       299 — complete at task 6 of 6, none dropped — 2026-09-09 18:50
PHASE GOAL: FT4 works exactly the way FT8 does.
UNIT GOAL:  The card's header carries the technical detail, a globe showing where
            the station is, a short place name, and a Log option that is always
            available.
ADVANCED:   no — no phase step moved. Step 4 stays partial: nothing tuned, nothing
            transmitted, no exchange run at the bench.
NUMBER:     how many facts the i hover carries, against how many its card holds
            THE INSTRUCTION SAYS IT CARRIED NONE. MEASURED, IT CARRIED 444
            CHARACTERS AND 8 OF THE 12 FACTS ITS CARD HELD.
            The four it dropped were the audio offset, the dial, the time offset
            and their context, and it dropped them whenever the decoded table had
            been cleared. Now 12 of 12, and 875 characters through the window.
DRIFT:      3 consecutive units without advance  (was 2, carried from unit 298)
```

---

## 1. What Claude did

**Complete. Six tasks of six, none dropped, including the named drop candidate.**
Development machine, prompt claimed `PROJECT: Hamlet`, branch `main`, eight commits,
all pushed. Root version 1.12.261 to **1.12.262**, bumped once. **No file under
`src/Ft8Sharp/` was touched**, and nothing here reaches a send path.

The gate held: `SHACK_FACTS.md` present, `CwProbabilisticDecoder.cs` present, no
`CoreHMI.sln`, no `MURC.sln`. This unit was numbered **299** from `PHASE_OUTCOME.md`;
the last entry there was 298.

**No test suite was run.** Twenty-five tests were constructed in this instruction
across five classes and run filtered by exact name, foregrounded, with a 500-second
timeout: **25 of 25 green.** Every `dotnet build` was foregrounded. Nothing was
backgrounded and nothing was polled.

**Task 1 — Log on every card, in every state.** The ruling from use, and it went
first.

**Task 2 — the `i`.** Measured before anything was changed; see section 3.

**Task 3 — the globe**, placed by the coastline's own projection.

**Task 4 — the place name**, measured across all 275 entities before shortening one.

**Task 5 — `assets/world-coastline.md`**, the named drop candidate, not dropped.

**Task 6 — the outcome entry**, filed against step 4.

**Three decisions made for themselves, each reported.**

1. **I measured the `i` before repairing it, and did not repair it, because it was
   not broken.** The instruction's own rule is *report mismatches; do not repair the
   instruction*, and the same rule points the other way here: do not repair code
   against a claim you have not checked.
2. **The band paragraph's fix is a per-station memory written only by a decoded
   row**, so every figure it hands back was measured about **that station** rather
   than read off the dial the radio happens to be standing on now (§0.0).
3. **The short place forms shorten and never narrow.** Task 4's first line offers a
   US state *if callook or the grid can give one*; neither can, so nothing below the
   country is named.

**Two mismatches against the instruction, reported and not repaired.**

1. **"The `i` icon does nothing. It is on the card and it is empty."** It is not.
   See section 3, which quotes the measurement.
2. **`WORK_INSTRUCTIONS.md`'s heading carries no unit number, for the third unit
   running.** Section 4's second item.

## 2. What the owner should expect

**The icons work, the globe shows where they are, and he can log whatever he likes.**

- **Every card carries `Log this contact`**, in all four states. On a finished card
  it is already the action, so it is never drawn twice on one.
- **The `i` carries the reports both ways with the scale they sit on, his grid, the
  distance, the bearing, where his tone sat, what the dial was on, how far into the
  slot he started, the slot times and what closed the exchange** — and it keeps
  carrying the band half after a retune, which it did not before.
- **A globe sits beside the `i`.** Hovering shows him and the station over a
  coastline, with a line between and a caption naming the place, the grid and the
  distance.
- **`the United States` reads `United States`** beside a callsign and a distance.

**What will look wrong and is not.**

- **The globe is a 16-pixel target and so is the `i`.** Both need a moment's hover.
  If the `i` felt dead before, that is worth trying again deliberately: it was
  carrying 444 characters the whole time.
- **A station that never sent a grid gets no dot**, and the caption says Hamlet does
  not know where he is rather than guessing from the callsign.
- **The line on the map is straight and the caption says that is a simplification.**
  It is not the path the signal took.
- **Fourteen country names still run over sixteen characters** — `Kingdom of
  Eswatini`, `Republic of Turkiye` and so on. They are printed by the test rather
  than padded down, and every one is a name people actually say.
- **The suite was not run and its state is unknown to this unit.** The four inherited
  reds were not chased.

## 3. What you should see

### 1. The `i` hover, quoted whole, on a your-turn card

**First, the measurement the instruction's claim did not survive.**
`Unit299HeaderProbeTests` reads the tooltip off the realized control in the real
window rather than off the property behind it:

```
hint marks realized inside the card: 1
  kind=Detail  bounds=172, 0, 14, 14  text length=444  tip length=469
```

**It was bound, it drew, and it carried 444 characters.** The claim is a mismatch and
is reported rather than repaired.

**What was actually wrong is smaller and real, and the probe found it.** The band
paragraph came off the newest decoded row for that station. The decoded table is
bounded and **clears on a band change**, while the cards are rebuilt from the ledger
and survive — so after a retune the hover quietly lost the frequency and the time
offset and kept the rest. In the window probe the mine side held one row and it was
the operator's own transmission. **444 characters before, 875 after.**

The hover now, whole:

> He hears you at **-9 dB** and you hear him at **-12 dB**. Those are decibels
> against the noise, so a minus number is the ordinary case here: this decoder reads
> down to about **-21**, and anything well above that is a comfortable signal rather
> than a marginal one. He is in grid **EN52**, **540 miles** away, on an initial
> **bearing of 288 degrees** from you. A four-character grid is a box about seventy
> miles across, so the distance is good to about that and no better. His tone sat
> **1240 Hz** up inside the receiver's passband while the dial was on **14.074000
> MHz**. Everybody on the band shares one dial setting and takes a different slice of
> the audio, which is how dozens of stations fit where one voice would go. His
> transmission began **0.2 seconds into the slot**. Both clocks have to agree within
> about a second for this to decode at all, so a small number here is the two of you
> keeping the same time. This ran from **02:11:00 to 02:12:00 UTC**. That is **one
> slot ago**. The last thing he sent you was **RR73**, which is a roger and a goodbye
> in one.

**Twelve facts, each with the context that makes it mean something**, and a test
names all twelve. A second test asserts the opposite half: an exchange with no report
and no grid produces a hover with no decibel figure, no distance and no bearing at
all — and says *he has not put a grid square on the air* rather than leaving a hole.

### 2. The globe's map for a known pair

`FN00` to `IO63`, EI4GNB in Ireland:

```
operator FN00 at 40.5, -79  ->  x 202, y 99
station  IO63 at 53.5, -7   ->  x 346, y 73
frame: left 162, top 30, 224 by 112
```

**The projection is the coastline's own and there is not a second one anywhere.**
`assets/world-coastline.svg` writes it into its own `<desc>`:

```
x = (longitude + 180) * 2
y = (90 - latitude) * 2
```

`TheProjectionIsTheCoastlineOwn` **reads that `<desc>` and the `viewBox` back out of
the file** and asserts the code agrees, so a map redrawn on another projection fails
here rather than silently moving every dot over a coastline that is still correct.
Watched failing: with `X` returning `* 4` instead of `* 2`, three of the eight globe
tests go red.

**The frame fits both points with a margin**, asserted over four pairs:

```
Ireland, across an ocean       left 162, top 30,  224 by 112
Japan, most of the way round   left 162, top 0,   516 by 258
the Caribbean, close to home   left 88,  top 59,  268 by 134
the next state                 left 140, top 57,  164 by 82
```

and it stops shrinking at 120 map units, because below that this outline has nothing
in it to recognise.

The caption:

> EI4GNB is in Ireland, grid IO63. That is 3,180 miles from you. **The line is drawn
> straight on this flat map. A real signal follows a great circle, which curves on a
> picture like this one, so the line says who is where rather than the path the
> signal took.**

That last sentence appears **wherever a line is drawn and nowhere else**, and a test
asserts both halves.

### 3. The globe on a station with no grid

> Hamlet does not know where K9XP is. He has not put a grid square on the air, and a
> callsign only names a country, which is not a point on a map.

**No dot, no line, no distance.** The mark is still there, because *he never sent
one* is a fact about the contact worth a hover rather than a gap in the screen.

### 4. A partial contact's log dialog, and the record it saves

He answered, the station came back with a grid, and it stopped there. Before tonight
that card offered no Log at all. It builds:

```
CALL          : K9XP
GRIDSQUARE    : EN52
RST_SENT      : (not recorded)
RST_RCVD      : (not recorded)
BAND          : 20m
```

and writes:

```
<CALL:4>K9XP<STATION_CALLSIGN:6>KC3QIS<QSO_DATE:8>20260908<TIME_ON:6>021100
<TIME_OFF:6>021115<BAND:3>20m<MODE:3>FT8<FREQ:9>14.074000
<GRIDSQUARE:4>EN52<MY_GRIDSQUARE:4>FN00<EOR>
```

**Neither report field reaches the file**, asserted against the ADIF text itself, so
a half exchange never looks like a whole one. And a card whose rows have aged off
still logs, with `FREQ` and `BAND` **absent entirely** rather than taken from wherever
the radio is standing now.

**And the X keeps its guard**: clearing a finished, unlogged card still says what is
being dropped.

## 4. What's blocking us

Nothing blocks the next unit. Three items.

---

**The `i` was never empty, so something else made it feel dead — and the likeliest
thing is that it is fourteen pixels wide.**

Measured: bound, drawn at `14 by 14`, carrying 444 characters and a 469-character
tooltip with a 150 ms show delay. **The claim in the instruction is a mismatch**, and
the real defect the probe found — the band paragraph vanishing after a retune — is
fixed and would not have made the mark look dead, only thinner.

So what is left is the target. `HintMarkControl` draws a **ring with no fill**: the
glyph and the outline are ink and the middle is empty, and it is a bare `Control`
rather than a `Border`. **The globe this unit added is deliberately a `Border` with
`Background="Transparent"`**, which is a solid 16-pixel hit target, and it is worth
comparing the two by hand before changing anything.

What was rejected. **Widening the mark on this unit's own authority**, which changes
every hint mark on every screen in the application on a guess about one hover.
**Doing nothing**, because he reported it and the report should say what was found
rather than closing the question.

What would settle it: he hovers the globe and the `i` side by side on the same card
and says whether one is easier to hit than the other. If it is, the mark becomes a
`Border` like the globe, everywhere, in one change.

---

**A work instruction's heading should carry its unit number. This is the third unit
running that has had to correct the entry by hand.**

`outcome-append.bat` resolves the unit number from `WORK_INSTRUCTIONS.md`'s heading
rather than from the argument it is given. That is unit 266's repair and it is right:
two callers were passing different numbers and the heading is the tie-break.

**The last three orders have carried no number there** — `# Work instruction - the
conversation becomes cards`, `# Work instruction - the achievements screen grows as
he operates`, `# Work instruction - the card's header does what it promised` — and
instead tell the session to take the next number from `PHASE_OUTCOME.md`, which the
script cannot read. It falls back to the previous unit's number every time, and units
297, 298 and 299 have each corrected the heading afterwards and reported it.

What was rejected. **Changing the script to prefer its argument**, which undoes unit
266's repair and reintroduces the double-entry it fixed. **Repairing the
instruction**, which its own rule forbids.

What would settle it, and it is one line: the order's heading reads `# Work
instruction 300 — ...`, **or** the numbering paragraph says *write that number into
the heading before task 1*.

---

**The card still cannot say a US state, and the instruction asked for one.**

Task 4's first line is *for a US station, the state if callook or the grid can give
one*. **Neither can.** `DxccPrefixes` names a DXCC entity, which for all fifty states
is *United States of America*; a US call area is historical rather than a residence,
so `W6` in Ohio is ordinary; and a four-character grid square is a box about seventy
miles across that straddles state lines.

So the face reads `United States` — the country said shorter — and never `Arizona`.
**Units 297, 298 and 299 have now each hit the same wall from a different direction**:
the card's place line, the achievements screen's Worked All States family, and this.

What would settle it. **The parked callook instruction returns a licensee's address**,
which is cited data with a state in it. Until then nothing below the country is
honest, and it is named here so the next author does not plan around a state that
does not exist yet.
