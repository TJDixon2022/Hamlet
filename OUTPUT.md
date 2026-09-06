READ IN THIS ORDER

A. THE PHASE GOAL. Everything this project has built reaches the operator's
   screen, and the decoder is taken as far as it will go. **Every step of this
   phase is closed**, since unit 251 shut step 6.
B. THE STEP AND ITS EXIT CRITERIA. None. This unit advances no step and its own
   header says so. The two step 6 criteria that were never met are still open by
   name as HM-OPEN-083 and HM-OPEN-084 and nothing here touches them.
C. WHAT THIS REPORT ADDS, AND WHETHER IT BEARS ON A OR B. It bears on A and not
   on B: hovering a grid now says how far and which way, and hovering a callsign
   says the country when that is certain. Section 4 raises 3 items and **none of
   them asks for a ruling** — one is a numbering collision in the instruction
   itself, one is a measurement reported and deliberately not acted on, and one is
   the standing asks queue.

UNIT:       252 — complete at task 5 of 5 — 2026-09-06 16:16
PHASE GOAL: Everything this project has built reaches the operator's screen, and the decoder is taken as far as it will go. All steps closed.
UNIT GOAL:  Hovering a grid says how far and which way; hovering a callsign says the country when that is certain and nothing at all when it is not.
ADVANCED:   yes — but not a step, and the instruction says so itself. Five operator-facing tasks, all five built, including the named drop candidate.
NUMBER:     79.9 per cent — the share of 1,982 real off-air callsigns the entity table resolves. There was no such figure before; task 5 is the first measurement of it.
DRIFT:      0 consecutive units without advance  (was 0)

---

## 1. What Claude did

**Complete, five of five. Nothing was dropped, including the named drop
candidate**, which was built because the corpus it needs turned out to be on this
machine. Machine `QUIVERFULL`, `C:\Source\HamLet`, branch `main`, all pushed.

### The gate

`SHACK_FACTS.md` present, `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs`
present, `CoreHMI.sln` absent, `MURC.sln` absent. **Hamlet confirmed.**
`SESSION.lock` was checked before the work order was read — it was absent and the
process list showed one session. That check is a habit carried from the collision
of 2026-09-05 and it is still not a rule.

### Shell refusals, recorded verbatim

**None. Not one call was refused**, across nine builds, eleven filtered test runs,
five commits and four network fetches. That is the second unit running with a
clean ledger against the 5 to 28 a unit the tool rule anticipates.

One tool fact that is not a refusal and cost time twice: **this shell collapses a
doubled backslash inside a quoted heredoc**, so a Python string written there
loses its escaping. It produced a broken `csproj` edit and a failed anchor match
before the file-editing tools were used instead.

### Task by task

**1 — the grid arithmetic. Most of it already existed and is reported rather than
rebuilt**, which the instruction asked for. `OperatorLocation` has decoded four
and six character Maidenhead locators to the centre of the square since the spot
cards were built, carries haversine at 6371 km, and computes the initial
great-circle bearing. What was missing was only the units Tim ruled on, so
`GridPath` is a formatter over arithmetic that already had one home.

**2 — the grid tooltip.** The operator's own square reaches the row in exactly one
place, `MainWindowViewModel.PlaceRow`, so `Ft8Vocabulary` stays a static table
with no route to settings and `OperatorProfile.GridSquare` stays the only copy.

**3 — the entity table.** Fetched, cited, generated. See section 3.

**4 — the two tooltips meet the panel.** The `from` field gained the country where
it is certain and is byte for byte unchanged where it is not.

**5 — the resolve rate.** 79.9 per cent of 1,982 real off-air callsigns.

### Decisions made on this session's own authority, reproduced in full

**One: the entity table is the ARRL DXCC list and not ITU Appendix 42, and the
ITU table is cited beside it rather than merged in.** Both were fetched. ITU
Appendix 42 allocates call sign series to *countries*, so it has `I = Italy` and
no row for Sardinia at all — it cannot answer the ruling's own example. DXCC is
what an operator means by *country* and it carries both cases the instruction
requires. **Merging two authorities is how a table acquires a disagreement nobody
can see** (§0), so one is used and the other is named in the file.

**Two: unit 241's habit of never repeating the four characters of a grid is
dropped, and a test replaces it.** That habit was a good instinct against a
sentence growing a country on the end, and it stopped being available the moment
the sentence carried a distance, because the operator has to see which square it
was measured from. What replaces it is a sweep of every grid tooltip in every
state for the place names those very squares would attract — Hungary for `JN86`,
Texas for `EM12`, Japan for `PM95`.

**Three: `/MM` and `/AM` resolve to nothing.** At sea and in the air are not
entities, and naming the one on the licence would be the confident wrong country
in its purest form, when the callsign itself is what says the station is not
there.

### Where my own figures were wrong and the code was right

Worth recording because it happened four times and it is the failure §12.5
describes — had the code been wrong in the same direction, the pair would have
agreed and proved nothing.

- **FN00DJ is at 79.7° west, not 76°.** Three hand-checked distances and two
  bearings in task 1 were wrong because of it.
- **OF88 is Perth, not Johannesburg.** Naming it wrongly is what made 8,060 miles
  look plausible where the answer is 11,321.
- **4,551 miles rounds to 4,600**, not 4,500.
- **`ZZZZZ` is Brazil.** It was chosen as an unresolvable callsign and `ZV-ZZ` is a
  real Brazilian series.

Every expectation is now derived from the square centres `FromGrid` actually
produces, cross-checked against the spherical law of cosines, which shares no
algebra with haversine.

## 2. What the owner should expect

**When he hovers, in his own terms — next door, or China:**

- **Hover a grid in the message column** and it says which square, how far away
  from him it is in miles, and the bearing in degrees true. `JN86` reads
  *4,600 miles away from you, on a bearing of 49 degrees*. That is Hungary and it
  is over the pole side, which is why the bearing is 49 and not 90.
- **It never names a place.** Not a country, not a state, not a city. A
  four-character square is roughly 70 by 100 miles, so *Texas* is fair and
  *Houston* is a lie, and neither is shown.
- **With no grid square in Settings** it still says which square, and that Hamlet
  needs his own before it can measure. It does not guess one and it does not go
  quiet.
- **Hover a sender's callsign** and it says the country where that is certain:
  *Who sent it. HA1BF is a callsign from Hungary.*
- **Where it is not certain it says exactly what it always said** — *Who sent
  it.* — with no *probably*, no *unknown*, no hedge of any kind.
- **About one callsign in five gets no country**, and that is the ruling working
  rather than failing. The commonest are Russian: `RA-RZ` belongs to European
  Russia **and** to Asiatic Russia, and the digit that separates them is not
  something a prefix match can see.
- **`W4/YV7AXM` reads United States of America** and `IS0/IK2YCW` reads
  **Sardinia**, which are his own two log entries and the two that would have gone
  backwards.

**What will look wrong and is not:**

- **A Russian station gets no country.** That is a shared prefix declining on
  purpose, not a lookup failure.
- **Some entity names end oddly** — *Canary Is.*, *Rodrigues I.* Those are the
  ARRL's own words and they are kept, because a name tidied no longer matches the
  source it is cited from.
- **Nothing about the columns, the layout, the filters or the sort changed.**

**Build:** clean, 0 warnings, 0 errors, whole solution, nine times.

**Tests:** only the tests this unit wrote were run, filtered, foregrounded.
Engine side `GridDistanceAndBearingTests`, `TheCallsignCountryIsCertainOrSilentTests`
and `WhatTheEntityTableResolvesTests`: **51 of 51 passed.** App side
`TheGridTooltipSaysHowFarTests` and `TheSenderTooltipNamesTheEntityTests`:
**26 of 26 passed.** No suite was run and nothing was backgrounded.

**Not run, and you should know which:** `TheMessageReadsAsThreePartsTests` asserts
the tooltip wording this unit changed for grids and for senders, and **it is very
likely to have gone red.** It compiles; it was not executed, because it is in
`Hamlet.App.Tests` and the standing rule keeps a unit out of a suite. It is named
here rather than left to be discovered.

**Pushed to `main`:** `9e73e22`, `e05faa2`, `f519322`, `7fe978f`, `8162bfc`.
Version **1.12.71 → 1.12.76**, a patch a task. `Ft8Sharp` did not move and no file
under `src/Ft8Sharp/` changed.

## 3. What you should see

**1. A grid tooltip, quoted, with the formula behind it.**

> HA1BF is calling anyone from grid JN86, which is 4,600 miles away from you, on
> a bearing of 49 degrees.

Distance is the **haversine formula on a sphere of radius 6371 km**, the IUGG mean
radius, converted at exactly 1.609344 km to the statute mile. Bearing is the
**initial great-circle bearing**, `atan2(sin Δλ · cos φ2, cos φ1 · sin φ2 − sin φ1
· cos φ2 · cos Δλ)`, normalised to 0–360 clockwise from true north. Both were
already in `OperatorLocation` and neither was rewritten. Measured from his own
`FN00DJ`, which decodes to 40.40°N, 79.71°W.

**It is the *initial* bearing and the word is not decoration.** On a great circle
the heading changes along the path; over four thousand miles the far end differs
by tens of degrees, and the one that matters is where the antenna points from
here.

Rounding is to the nearest ten miles under a thousand and the nearest hundred
above, away from zero at the half. The grid decides that: a four-character square
is seventy miles across, so a figure to the mile is precision the input never
carried.

**2. The two compound callsigns, and one that gets nothing at all.**

```
W4/YV7AXM   -> United States of America
IS0/IK2YCW  -> Sardinia
YV7AXM      -> Venezuela          (the home call on its own)
IK2YCW      -> Italy              (the home call on its own)
3D2AB       -> (no tooltip at all)
VK9XYZ      -> (no tooltip at all)
```

The prefix before the slash wins, so the first two name where the operator **is**
rather than who licensed them. The contrast pair underneath is in the test on
purpose: if the slash handling ever regresses, the two pairs disagree and one
fails.

`3D2` is Fiji, Conway Reef **and** Rotuma. `VK9` is five different islands.
**Fourteen prefixes are shared and every one is silent**, and a shared prefix
stops the search rather than falling back to a shorter one — answering *Australia*
for a VK9 station is a guess arrived at by persistence.

**Where the table came from.** The ARRL DXCC List of current entities, January
2026, 340 entities, fetched 2026-09-06 from
`https://www.arrl.org/files/file/DXCC/Current_Deleted.txt`. Its rows are
transcribed verbatim to `tools/dxcc/arrl-dxcc-current.txt` and expanded to
`data/callsigns/dxcc-prefixes.json` by `tools/dxcc/expand-prefixes.py`, so the
derivation is reproducible rather than asserted. **605 prefixes resolve to exactly
one entity.** The expander drops what it does not understand and says so: one
token, `H6-7` for Nicaragua. A first pass produced the prefix `3B67`, which nobody
holds, because a bare digit after a comma must attach to the stem without its
trailing digits.

**3. The resolve rate, over real off-air callsigns.**

```
=== Karlis Goba off-air recordings ===
callsigns seen : 1982  (distinct 432)
resolved       : 1583  (79.9%)
declined       : 399

ten commonest declines, by leading characters:
   RV6    36   e.g. RV6K, RV6AFG, RV6ARS
   R8     33   e.g. R8AU, R8JA
   4U1    30   e.g. 4U1A
   R4     30   e.g. R4OF, R4HM, R4WZ
   R7     22   e.g. R7IW, R7CA, R7EL
   MM0    21   e.g. MM0IMC
   7Z1    20   e.g. 7Z1AL
   2E0    19   e.g. 2E0PKK, 2E0VDS, 2E0LDW
   R3     18   e.g. R3BV, R3FO, R3KCW
   RA3    16   e.g. RA3QUE, RA3TPE
```

**Real stations on the air on 2019-11-11**, with the messages WSJT-X read from
them, which is the difference between measuring the table and measuring our own
synthesis (§12.5). The tree's own FT8 fixture is counted apart and resolves 4 of
4, which is a fact about the United States block being large rather than about
coverage.

**The Russian calls are the largest group and they are not a simple gap.** The
ARRL rows are `UA-UI1-7,RA-RZ = European Russia` and `UA-UI8-0,RA-RZ = Asiatic
Russia`, and **`RA-RZ` appears in both**. Transcribed in full they would still
decline, under the shared-prefix rule, which is this instruction's own *a prefix
whose entity depends on a number returns nothing*. The digit is what separates
them and a prefix match cannot see it.

**Nothing was extended on the strength of any of this**, which the instruction
required. The data file gained a `notTranscribed` note naming exactly which source
rows were left out — every row whose prefix column carries a slash, because a
slash means something different in each of them, and the two Russia rows.

## 4. What's blocking us

**Nothing blocks the next unit.** Three items, none of which asks for a ruling.

**1. This work order is numbered 252 and so was the last one.**

Reported, not repaired, per the instruction's own rule about tree mismatches.

The previous order was *252 - the filter shows what he asked for and hides the
rest*, completed yesterday. This one is *252 - who they are and how far away*.
Both name themselves 252 and they are different units. This one's tree-check
section also says *work instruction 251 delivered the two-column layout, the CQ
and mine toggles, and the `dt` fix* — the layout and the `dt` fix were 251, the
toggles were the first 252. **The version told the truth where the numbering did
not**: `Directory.Build.props` read 1.12.71, which is where the first 252 left it,
and that is what the instruction said to trust.

Nothing is broken by it. It matters only because the phase record indexes units by
number, and two units sharing one will collide in `PHASE_OUTCOME.md` the next time
either is cited.

**2. Task 1 was already built, and the instruction did not know.**

Also reported rather than repaired. `OperatorLocation.FromGrid`,
`OperatorLocation.DistanceKm` and `OperatorLocation.BearingDegrees` have existed
since the spot cards (HM-DEC-038) and do exactly what task 1 specifies, including
the centre-of-square rule and both locator lengths. The instruction says *reading
and building only*, and the reading is what found them.

**What was genuinely built is `GridPath`**, which is units and rounding over
arithmetic that already had one home. A second haversine would have been a second
answer waiting to disagree, silently, about a number nobody can check by eye.

**3. `TheMessageReadsAsThreePartsTests` is expected to be red and was not run.**

Named here rather than left to be found. It asserts the exact tooltip wording for
grid payloads and for the sender field, and this unit changed both on your ruling.
It compiles. Under the standing rule a unit runs only the test it writes, so it was
not executed, and updating a test whose expectations this unit deliberately
superseded is work for whoever runs the suite at the end of the phase.

### Asks still outstanding

Carried verbatim until you rule, per HM-DEC-139.

**This session cannot honestly reconstruct the queue, and that is now the third
report in a row saying so.** HM-DEC-139 requires the work order to carry the
outstanding asks inbound, and **work instruction 252 carries no
`Asks still outstanding` heading**, which by §9.6 makes the order defective and
obliges the session to rebuild the queue from `OPEN_ISSUES.md` and the recent
reports. `OUTPUT.md` is overwritten every unit, so the reports that would carry
those asks are gone; rebuilding from `OPEN_ISSUES.md` alone would produce the long
list HM-DEC-140 expressly says does not belong here.

What can be stated without a rebuild:

- **The missing heading is itself the first ask.** It is a defect in the order
  under §9.6 and HM-DEC-137, reported here as those clauses require. It has now
  been missing from three consecutive orders, which is the shape HM-DEC-137 was
  written about — a rule nothing carries into the session is the same as no rule.
- **`HM-OPEN-083` and `HM-OPEN-084`**, raised 2026-09-05, are step 6's two unmet
  exit criteria. They are recorded issues with an id, an owner and a date, so by
  HM-DEC-140 they belong in `OPEN_ISSUES.md` and not on this queue. Neither was
  touched.
- **This unit added nothing to the queue.** Its three section 4 items are a
  numbering collision, a tree mismatch and a named red test; none is a question
  handed back for a ruling.
